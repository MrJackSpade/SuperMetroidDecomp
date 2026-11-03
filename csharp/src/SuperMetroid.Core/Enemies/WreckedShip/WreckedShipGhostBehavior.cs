using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship ghost species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WreckedShipGhostBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Coven];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WreckedShipGhostEntity(this, slot, common);

    /// <summary>Initialization combines the 120-frame appearance delay with an additional 160-frame startup wait.</summary>
    private const int WreckedShipGhostInitialDelay = 280;
    /// <summary>Frames the ghost remains visible after its appearance cycle enters bobbing.</summary>
    private const int WreckedShipGhostVisibleDuration = 120;
    /// <summary>Samus tracking must remain within the positional margin this long before spawning.</summary>
    private const int WreckedShipGhostStablePositionDuration = 64;
    /// <summary>Samus's facing must remain stable for this many tracking calls.</summary>
    private const int WreckedShipGhostStableDirectionDuration = 16;
    /// <summary>Initial whole-pixel vertical speed of the visible bobbing cycle.</summary>
    private static readonly SignedSixteenSixteen WreckedShipGhostInitialVerticalVelocity = SignedSixteenSixteen.FromParts(1, 0);
    /// <summary>Fractional vertical acceleration applied to the bobbing ghost.</summary>
    private const int WreckedShipGhostVerticalAccelerationFraction = 6144;
    /// <summary>The movement class, horizontal or vertical, of Samus holding still on that axis: the middle of three.</summary>
    private const int WreckedShipGhostStationaryClass = 1;
    /// <summary>The three movement classes on each axis: moving toward smaller coordinates, still, or toward larger ones.</summary>
    private const int WreckedShipGhostMovementClassCount = 3;

    /// <summary>Initializes the Wrecked Ship ghost.</summary>
    internal static void InitializeWreckedShipGhost(RoomEnemySlot slot)
    {
        // Every named retail ghost population places this actor in physical slot zero.
        // Its tracking routine contains several absolute-Samus reads indexed by the
        // enemy's slot; a nonzero slot would deliberately alias unrelated memory. Enforce the
        // authored layout rather than silently pretending that retail bug has generic rules.
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                "Wrecked Ship ghost requires retail physical slot zero for indexed Samus reads.");
        }

        // The initializer sets the animation-script, intangibility and invisibility properties. In this host those mean
        // instruction processing, exclusion from Samus interaction, and invisibility.
        slot.Properties = slot.Properties.With(
            EnemyProperties.RunsAnimationScript |
            EnemyProperties.IgnoreSamusCollision |
            EnemyProperties.Invisible);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        var state = new WreckedShipGhostEnemyState
        {
            Program = AnimationProgramCursor<WreckedShipGhostProgram>.Start(WreckedShipGhostProgram.Floating),
            Function = WreckedShipGhostAiFunction.InitialInvisibleDelay,
            PhaseTimer = WrappingCountdown.Start(WreckedShipGhostInitialDelay),
        };

        // Retail clears the actor palette's target-buffer entries here. Its room fade then
        // settles the independently stored current palette to that black target long before
        // the 280-frame appearance delay expires. The ghost's fade is that already-settled
        // value, so darken both; otherwise the first white ramp would begin from the
        // graphics-set palette and carry between channels.
        state.Fade.Darken();
        ShowFade(slot, state);
        slot.SetSpeciesState(state, state.ClearWithRecord);
    }

    /// <summary>Shows the ghost in the shade its fading colours are in (#1512).</summary>
    private static void ShowFade(RoomEnemySlot slot, WreckedShipGhostEnemyState state) =>
        slot.Shading = PictureShading.Shaded(state.Fade.Shade);

    /// <summary>Ports the indirect dispatch in <c>WreckedShipGhost_Main</c>.</summary>
    private void RunWreckedShipGhostMain(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        SamusState? samus)
    {
        SamusState activeSamus = samus ?? throw new InvalidOperationException(
            "Wrecked Ship ghost AI requires the active Samus actor.");

        switch (state.Function)
        {
            case WreckedShipGhostAiFunction.BrighteningAndFlickering:
                RunWreckedShipGhostBrightening(slot, state);
                break;
            case WreckedShipGhostAiFunction.FadingToGhostPalette:
                RunWreckedShipGhostPaletteFade(slot, state, activeSamus);
                break;
            case WreckedShipGhostAiFunction.WaitingForWhiteFade:
                RunWreckedShipGhostWhiteFade(slot, state);
                break;
            case WreckedShipGhostAiFunction.BobbingWhileVisible:
                RunWreckedShipGhostBobbing(slot, state);
                break;
            case WreckedShipGhostAiFunction.InitialInvisibleDelay:
                RunWreckedShipGhostInitialDelay(state);
                break;
            case WreckedShipGhostAiFunction.TrackingSamusForSpawn:
                RunWreckedShipGhostTracking(slot, state, activeSamus);
                break;
            default:
                throw new InvalidDataException(
                    $"Wrecked Ship ghost function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Counts down the invisible delay. A zero timer also advances immediately, matching the game's
    /// branch rather than wrapping zero to -1.
    /// </summary>
    private static void RunWreckedShipGhostInitialDelay(WreckedShipGhostEnemyState state)
    {
        state.PhaseTimer = state.PhaseTimer.DecrementToZero();
        if (!state.PhaseTimer.IsZero)
            return;

        state.PhaseTimer = WrappingCountdown.Start(1);
        state.FlickerTableOffset = 2;
        state.Function = WreckedShipGhostAiFunction.TrackingSamusForSpawn;
    }

    /// <summary>The one-pixel margin of the nearly-still box on each axis.</summary>
    private static readonly Offset16<RoomPixels> WreckedShipGhostStillMargin = new(1, 1);

    /// <summary>
    /// Tracks Samus for the spawn. The one-pixel upper bound is exclusive in retail; this asymmetric
    /// comparison is deliberately not replaced by an absolute-distance convenience test.
    /// </summary>
    private static void RunWreckedShipGhostTracking(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        Position16<RoomPixels> samusPosition = samus.Position.Whole;
        Offset16<RoomPixels> fromLower = samusPosition - state.SamusLowerBound;
        Offset16<RoomPixels> fromUpper = samusPosition - state.SamusUpperBound;

        bool withinPreviousBox =
            fromLower.X >= 0 &&
            fromUpper.X < 0 &&
            fromLower.Y >= 0 &&
            fromUpper.Y < 0;

        if (withinPreviousBox)
        {
            state.StablePositionTimer = state.StablePositionTimer.Decrement().Remaining;
            if (state.StablePositionTimer.IsZero)
            {
                // Remaining inside the tiny box for 64 frames means "stationary" on both
                // axes and therefore selects the central spawn offset.
                state.SpawnColumn = WreckedShipGhostStationaryClass;
                state.SpawnRow = WreckedShipGhostStationaryClass;
                SpawnWreckedShipGhost(slot, state, samusPosition);
                return;
            }
        }
        else
        {
            state.StablePositionTimer = WrappingCountdown.Start(WreckedShipGhostStablePositionDuration);
            Offset16<RoomPixels> motion = samusPosition - state.PreviousSamusPosition;
            int horizontalClass = motion.X switch
            {
                < 0 => 0,
                0 => WreckedShipGhostStationaryClass,
                _ => 2,
            };

            if (horizontalClass != state.SpawnColumn)
            {
                state.SpawnColumn = horizontalClass;
                state.StableDirectionTimer = WrappingCountdown.Start(WreckedShipGhostStableDirectionDuration);
            }
            else
            {
                int verticalClass = motion.Y switch
                {
                    < 0 => 0,
                    0 => WreckedShipGhostStationaryClass,
                    _ => 2,
                };

                if (verticalClass != state.SpawnRow)
                {
                    state.SpawnRow = verticalClass;
                    state.StableDirectionTimer = WrappingCountdown.Start(WreckedShipGhostStableDirectionDuration);
                }
                else
                {
                    state.StableDirectionTimer = state.StableDirectionTimer.Decrement().Remaining;
                    if (state.StableDirectionTimer.IsZero)
                    {
                        SpawnWreckedShipGhost(slot, state, samusPosition);
                        return;
                    }
                }
            }
        }

        // The tracker publishes a fresh asymmetric [position-1, position+1) box at
        // the end of every non-spawning frame.
        state.PreviousSamusPosition = samusPosition;
        state.SamusLowerBound = samusPosition - WreckedShipGhostStillMargin;
        state.SamusUpperBound = samusPosition + WreckedShipGhostStillMargin;
    }

    /// <summary>Installs the direction-selected relative position and begins brightening.</summary>
    internal static void SpawnWreckedShipGhost(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        Position16<RoomPixels> samusPosition)
    {
        if ((uint)state.SpawnColumn >= WreckedShipGhostMovementClassCount ||
            (uint)state.SpawnRow >= WreckedShipGhostMovementClassCount)
        {
            throw new InvalidDataException(
                $"Wrecked Ship ghost movement classes {state.SpawnColumn}/{state.SpawnRow} " +
                "do not select a retail spawn offset.");
        }

        slot.Position = slot.Position.WithWhole(samusPosition + WreckedShipGhostAppearance.SpawnOffset(
            state.SpawnRow * WreckedShipGhostMovementClassCount + state.SpawnColumn));
        state.Function = WreckedShipGhostAiFunction.BrighteningAndFlickering;
        state.StablePositionTimer = WrappingCountdown.Start(WreckedShipGhostStablePositionDuration);
        state.StableDirectionTimer = WrappingCountdown.Start(WreckedShipGhostStableDirectionDuration);
    }

    /// <summary>Runs the white-flash ramp and its flicker helper.</summary>
    internal static void RunWreckedShipGhostBrightening(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        AdvanceWreckedShipGhostFlicker(slot, state);

        // The brightening gates only on red and does not saturate the others: a full green or blue carries into the
        // next channel, as the fade value reproduces.
        int colorsChanged = state.Fade.Brighten();
        ShowFade(slot, state);
        if (colorsChanged != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.FadingToGhostPalette;
        state.Fade.BeginMaterializing();
    }

    /// <summary>Fades to the ghost palette, including the simultaneous flicker and per-component fade.</summary>
    private static void RunWreckedShipGhostPaletteFade(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        int componentsChanged = StepWreckedShipGhostPaletteTowardTarget(slot, state);
        AdvanceWreckedShipGhostFlicker(slot, state);
        if (!state.PhaseTimer.IsZero || componentsChanged != 0)
            return;

        // Clearing intangibility and invisibility makes the fully materialized ghost visible and eligible for the
        // ordinary touch/shot collision handlers authored by its enemy definition.
        slot.Properties = slot.Properties.Without(
            EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
        state.Function = WreckedShipGhostAiFunction.BobbingWhileVisible;
        state.BobbingOrigin = new(0, slot.Position.Y.Whole);
        state.PhaseTimer = WrappingCountdown.Start(WreckedShipGhostVisibleDuration);
        state.FlickerTableOffset = 2;
        state.VerticalVelocity = WreckedShipGhostInitialVerticalVelocity;
        state.SpawnColumn = WreckedShipGhostStationaryClass;
        state.SpawnRow = WreckedShipGhostStationaryClass;
        state.PreviousSamusPosition = samus.Position.Whole;
        state.SamusLowerBound = samus.Position.Whole;
        state.SamusUpperBound = samus.Position.Whole;
        state.StablePositionTimer = WrappingCountdown.Start(WreckedShipGhostStablePositionDuration);
        state.StableDirectionTimer = WrappingCountdown.Start(WreckedShipGhostStableDirectionDuration);
    }

    /// <summary>Runs the signed 16.16 oscillation and 120-frame visible lifetime.</summary>
    private static void RunWreckedShipGhostBobbing(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.VerticalVelocity);

        int acceleration = (slot.Position.Whole - state.BobbingOrigin).Y < 0
            ? WreckedShipGhostVerticalAccelerationFraction
            : -WreckedShipGhostVerticalAccelerationFraction;
        state.VerticalVelocity += SignedSixteenSixteen.FromRaw(acceleration);

        state.PhaseTimer = state.PhaseTimer.Decrement().Remaining;
        if (!state.PhaseTimer.IsZero)
            return;

        state.Function = WreckedShipGhostAiFunction.WaitingForWhiteFade;
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.Fade.BeginVanishing();
    }

    /// <summary>Finishes whitening, hides, and restarts the 120-frame delay.</summary>
    private static void RunWreckedShipGhostWhiteFade(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        if (StepWreckedShipGhostPaletteTowardTarget(slot, state) != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.InitialInvisibleDelay;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.PhaseTimer = WrappingCountdown.Start(WreckedShipGhostVisibleDuration);
    }

    /// <summary>
    /// Advances the flicker. The table alternates hidden and visible intervals whose lengths
    /// converge; its -1 terminator leaves the ghost permanently visible for this cycle.
    /// </summary>
    internal static void AdvanceWreckedShipGhostFlicker(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        if (!state.PhaseTimer.IsZero)
        {
            state.PhaseTimer = state.PhaseTimer.Decrement().Remaining;
            if (!state.PhaseTimer.IsZero)
                return;

            int tableIndex = state.FlickerTableOffset / 2;
            if ((uint)tableIndex >= WreckedShipGhostAppearanceDefinitions.FlickerCount)
            {
                throw new InvalidDataException(
                    $"Wrecked Ship ghost flicker offset {state.FlickerTableOffset} is outside the flicker table.");
            }

            int duration = WreckedShipGhostAppearance.FlickerDuration(tableIndex);
            if (duration < 0)
            {
                state.PhaseTimer = WrappingCountdown.Start(0);
                state.FlickerTableOffset = 0;
                return;
            }

            state.PhaseTimer = WrappingCountdown.Start(duration);
            int oldOffset = state.FlickerTableOffset;
            state.FlickerTableOffset = oldOffset + 2;
            if ((oldOffset & 2) != 0)
                return;
        }

        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
    }

    /// <summary>
    /// Steps the ghost's fade: every colour channel moves exactly one level toward its target per frame; the return value
    /// counts channel changes, not changed colours.
    /// </summary>
    internal static int StepWreckedShipGhostPaletteTowardTarget(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        int componentsChanged = state.Fade.StepTowardTarget();
        ShowFade(slot, state);
        return componentsChanged;
    }

    /// <summary>Advances WreckedShipGhost's typed animation.</summary>
    internal static void AdvanceWreckedShipGhostTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, WreckedShipGhostAnimations.Programs, WreckedShipGhostFrameDefinitions.ShownFrames.Of,
            static (WreckedShipGhostEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports WreckedShipGhost's main AI.</summary>
    internal void RunWreckedShipGhostMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWreckedShipGhostMain(slot, slot.RequireSpeciesState<WreckedShipGhostEnemyState>(), frame.Samus);
    }
}
