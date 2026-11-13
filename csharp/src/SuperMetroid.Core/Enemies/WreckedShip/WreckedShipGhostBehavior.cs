using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship ghost species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class WreckedShipGhostBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Coven];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WreckedShipGhostEntity(slot, common);

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
    private static readonly SignedSixteenSixteen WreckedShipGhostVerticalAcceleration = SignedSixteenSixteen.FromParts(0, 6144);
    /// <summary>The movement class, horizontal or vertical, of Samus holding still on that axis: the middle of three.</summary>
    private const int WreckedShipGhostStationaryClass = 1;
    /// <summary>The three movement classes on each axis: moving toward smaller coordinates, still, or toward larger ones.</summary>
    private const int WreckedShipGhostMovementClassCount = 3;

    /// <summary>Initializes the Wrecked Ship ghost.</summary>
    internal static void InitializeWreckedShipGhost(RoomEnemySlot slot)
    {
        // Every named retail ghost population places this actor in physical slot zero.
        // Its tracking reads Samus's position through the enemy's slot; a nonzero slot would read
        // unrelated state. Enforce the authored layout rather than silently pretending that retail
        // bug has generic rules.
        if (!slot.IsListedFirst())
        {
            throw new InvalidDataException(
                "Wrecked Ship ghost requires retail physical slot zero for indexed Samus reads.");
        }

        // The initializer sets the animation-script, intangibility and invisibility properties. In this host those mean
        // animation processing, exclusion from Samus interaction, and invisibility.
        slot.Processing = slot.Processing.With(EnemyProcessing.AnimationScript);
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        var state = new WreckedShipGhostEnemyState
        {
            Playback = new(WreckedShipGhostAnimations.StartingWith(WreckedShipGhostProgram.Floating), AnimationPlace.StartOfAnimation),
            Function = WreckedShipGhostAiFunction.InitialInvisibleDelay,
            PhaseTimer = WreckedShipGhostInitialDelay,
        };

        // The ghost starts dark: its colours have settled to black long before the 280-update appearance delay
        // expires, so the first white ramp begins from black rather than from the ghost's own colours.
        state.Fade = WreckedShipGhostFade.Dark;
        ShowFade(slot, state);
        slot.SetSpeciesState(state);
    }

    /// <summary>Shows the ghost in the shade its fading colours are in (#1512).</summary>
    private static void ShowFade(RoomEnemySlot slot, WreckedShipGhostEnemyState state) =>
        slot.Shading = PictureShading.Shaded(WreckedShipGhostFadeShade.Of(state.Fade));

    /// <summary>Runs the Wrecked Ship ghost's function dispatch.</summary>
    private static void RunWreckedShipGhostMain(
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
            case WreckedShipGhostAiFunction.FadingToGhostColours:
                RunWreckedShipGhostColourFade(slot, state, activeSamus);
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
        state.PhaseTimer = Math.Max(0, state.PhaseTimer - 1);
        if (state.PhaseTimer != 0)
            return;

        state.PhaseTimer = 1;
        state.NextFlickerStep = 0;
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
            if (--state.StablePositionTimer == 0)
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
            state.StablePositionTimer = WreckedShipGhostStablePositionDuration;
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
                state.StableDirectionTimer = WreckedShipGhostStableDirectionDuration;
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
                    state.StableDirectionTimer = WreckedShipGhostStableDirectionDuration;
                }
                else
                {
                    if (--state.StableDirectionTimer == 0)
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
        state.StablePositionTimer = WreckedShipGhostStablePositionDuration;
        state.StableDirectionTimer = WreckedShipGhostStableDirectionDuration;
    }

    /// <summary>Runs the white-flash ramp and its flicker helper.</summary>
    internal static void RunWreckedShipGhostBrightening(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        AdvanceWreckedShipGhostFlicker(slot, state);

        // The brightening gates only on red and does not saturate the others: a full green or blue carries into the
        // next channel, as the fade value reproduces.
        WreckedShipGhostFadeStep step = state.Fade.Brightened();
        state.Fade = step.Next;
        int colorsChanged = step.Changes;
        ShowFade(slot, state);
        if (colorsChanged != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.FadingToGhostColours;
        state.Fade = state.Fade.Materializing(WreckedShipGhostAppearance.Colours);
    }

    /// <summary>Fades to the ghost's own colours, with the simultaneous flicker and per-channel fade.</summary>
    private static void RunWreckedShipGhostColourFade(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        int componentsChanged = StepWreckedShipGhostFadeTowardTarget(slot, state);
        AdvanceWreckedShipGhostFlicker(slot, state);
        if (state.PhaseTimer != 0 || componentsChanged != 0)
            return;

        // Clearing intangibility and invisibility makes the fully materialized ghost visible and eligible for the
        // ordinary touch/shot collision handlers authored by its enemy definition.
        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
        slot.Contact = slot.Contact.Without(EnemyContact.IgnoresSamus);
        state.Function = WreckedShipGhostAiFunction.BobbingWhileVisible;
        state.BobbingOrigin = new(0, slot.Position.Y.Whole);
        state.PhaseTimer = WreckedShipGhostVisibleDuration;
        state.NextFlickerStep = 0;
        state.VerticalVelocity = WreckedShipGhostInitialVerticalVelocity;
        state.SpawnColumn = WreckedShipGhostStationaryClass;
        state.SpawnRow = WreckedShipGhostStationaryClass;
        state.PreviousSamusPosition = samus.Position.Whole;
        state.SamusLowerBound = samus.Position.Whole;
        state.SamusUpperBound = samus.Position.Whole;
        state.StablePositionTimer = WreckedShipGhostStablePositionDuration;
        state.StableDirectionTimer = WreckedShipGhostStableDirectionDuration;
    }

    /// <summary>Runs the signed 16.16 oscillation and 120-frame visible lifetime.</summary>
    private static void RunWreckedShipGhostBobbing(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(state.VerticalVelocity);

        SignedSixteenSixteen acceleration = (slot.Position.Whole - state.BobbingOrigin).Y < 0
            ? WreckedShipGhostVerticalAcceleration
            : -WreckedShipGhostVerticalAcceleration;
        state.VerticalVelocity += acceleration;

        if (--state.PhaseTimer != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.WaitingForWhiteFade;
        slot.Contact = slot.Contact.With(EnemyContact.IgnoresSamus);
        state.Fade = state.Fade.Vanishing();
    }

    /// <summary>Finishes whitening, hides, and restarts the 120-frame delay.</summary>
    private static void RunWreckedShipGhostWhiteFade(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        if (StepWreckedShipGhostFadeTowardTarget(slot, state) != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.InitialInvisibleDelay;
        slot.Presence = slot.Presence.With(EnemyPresence.Hidden);
        state.PhaseTimer = WreckedShipGhostVisibleDuration;
    }

    /// <summary>
    /// Advances the flicker: when the current step's updates run out, the next step starts and may show the ghost; past the
    /// last step the flicker ends and every later call keeps the ghost visible for this cycle.
    /// </summary>
    internal static void AdvanceWreckedShipGhostFlicker(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        if (state.PhaseTimer != 0)
        {
            if (--state.PhaseTimer != 0)
                return;

            IReadOnlyList<WreckedShipGhostFlickerStep> steps = WreckedShipGhostAppearanceDefinitions.FlickerSteps;
            if (state.NextFlickerStep == steps.Count)
                return;

            WreckedShipGhostFlickerStep step = steps[state.NextFlickerStep];
            state.PhaseTimer = step.Duration;
            state.NextFlickerStep++;
            if (step.Visibility == WreckedShipGhostFlickerVisibility.Unchanged)
                return;
        }

        slot.Presence = slot.Presence.Without(EnemyPresence.Hidden);
    }

    /// <summary>
    /// Steps the ghost's fade: every colour channel moves exactly one level toward its target per frame; the return value
    /// counts channel changes, not changed colours.
    /// </summary>
    internal static int StepWreckedShipGhostFadeTowardTarget(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        WreckedShipGhostFadeStep step = state.Fade.SteppedTowardTarget();
        state.Fade = step.Next;
        int componentsChanged = step.Changes;
        ShowFade(slot, state);
        return componentsChanged;
    }

    /// <summary>Advances the Wrecked Ship ghost's typed animation.</summary>
    internal static void AdvanceWreckedShipGhostTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, WreckedShipGhostAnimations.Segments,
            static (WreckedShipGhostEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"The Wrecked Ship ghost's cue-free animation raised {cue}."));

    /// <summary>Runs the Wrecked Ship ghost's main AI.</summary>
    internal static void RunWreckedShipGhostMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunWreckedShipGhostMain(slot, slot.RequireSpeciesState<WreckedShipGhostEnemyState>(), frame.Samus);
    }
}
