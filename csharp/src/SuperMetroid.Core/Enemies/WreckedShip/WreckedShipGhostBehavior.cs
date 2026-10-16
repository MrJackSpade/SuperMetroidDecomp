using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.WreckedShip.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.WreckedShip;

/// <summary>The Wrecked Ship ghost species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class WreckedShipGhostBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Coven];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WreckedShipGhostEntity(this, slot, common);

    /// <summary>Owns the live OBJ palette row whitened and restored during each apparition.</summary>
    private readonly EnemyPresentation _presentation;

    /// <summary>Binds the ghost's palette updates to the room's shared presentation buffer.</summary>
    internal WreckedShipGhostBehavior(EnemyPresentation presentation)
    {
        _presentation = presentation;
    }

    /// <summary>$A8:9B17 combines the 120-frame appearance delay with an additional 160-frame startup wait.</summary>
    private const ushort WreckedShipGhostInitialDelay = 280;
    /// <summary>Frames the ghost remains visible after its appearance cycle enters bobbing.</summary>
    private const ushort WreckedShipGhostVisibleDuration = 120;
    /// <summary>Samus tracking must remain within the positional margin this long before spawning.</summary>
    private const ushort WreckedShipGhostStablePositionDuration = 64;
    /// <summary>Samus's facing must remain stable for this many tracking calls.</summary>
    private const ushort WreckedShipGhostStableDirectionDuration = 16;
    /// <summary>Initial whole-pixel vertical speed of the visible bobbing cycle.</summary>
    private const ushort WreckedShipGhostInitialVerticalVelocityWhole = 1;
    /// <summary>Fractional vertical acceleration applied to the bobbing ghost.</summary>
    private const int WreckedShipGhostVerticalAccelerationFraction = 6144;

    /// <summary>Ports <c>WreckedShipGhost_Init</c> at <c>$A8:9AEE</c>.</summary>
    internal void InitializeWreckedShipGhost(RoomEnemySlot slot)
    {
        // Every named retail ghost population places this actor in physical slot zero.
        // Its tracking routine contains several absolute-Samus reads indexed by the native
        // enemy offset; a nonzero slot would deliberately alias unrelated WRAM. Enforce the
        // authored layout rather than silently pretending that retail bug has generic rules.
        if (slot.SlotIndex != 0)
        {
            throw new InvalidDataException(
                "Wrecked Ship ghost requires retail physical slot zero for indexed Samus reads.");
        }

        // Native ORs property bits $2000, $0400, and $0100. In this host those mean
        // instruction processing, exclusion from Samus interaction, and invisibility.
        slot.Properties = slot.Properties.With(
            EnemyProperties.ProcessInstructions |
            EnemyProperties.IgnoreSamusCollision |
            EnemyProperties.Invisible);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        var state = new WreckedShipGhostEnemyState(slot)
        {
            Program = AnimationProgramCursor<WreckedShipGhostProgram>.Start(WreckedShipGhostProgram.Floating),
            Function = WreckedShipGhostAiFunction.InitialInvisibleDelay,
            PhaseTimer = NativeCountdown.Start(WreckedShipGhostInitialDelay),
        };

        // Retail clears the actor palette's target-buffer entries here. Its room fade then
        // settles the independently stored current palette to that black target long before
        // the 280-frame appearance delay expires. SnesCgram is this runtime's already-settled
        // visible buffer, so clear it alongside the retained target buffer; otherwise the
        // first white ramp would begin from the graphics-set palette and overflow BGR fields.
        state.MutableTargetPalette.Clear();
        WreckedShipGhostPalette(slot).Fill(Bgr555.Black);
        (slot.Entity as WreckedShipGhostEntity ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} is not bound to a WreckedShipGhost.")).State = state;
    }

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
                    $"Wrecked Ship ghost function $A8:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>
    /// Ports $A8:9D13. A zero timer also advances immediately, matching the native ORA/DEC
    /// branch rather than wrapping zero to $FFFF.
    /// </summary>
    private static void RunWreckedShipGhostInitialDelay(WreckedShipGhostEnemyState state)
    {
        state.PhaseTimer = state.PhaseTimer.DecrementToZero();
        if (!state.PhaseTimer.IsZero)
            return;

        state.PhaseTimer = NativeCountdown.Start(1);
        state.FlickerTableOffset = 2;
        state.Function = WreckedShipGhostAiFunction.TrackingSamusForSpawn;
    }

    /// <summary>The one-pixel margin of the nearly-still box on each axis.</summary>
    private static readonly Offset16<RoomPixels> WreckedShipGhostStillMargin = new(1, 1);

    /// <summary>
    /// Ports $A8:9D36. The one-pixel upper bound is exclusive in retail; this asymmetric
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
                state.SpawnColumnOffset = 4;
                state.SpawnRowOffset = 12;
                SpawnWreckedShipGhost(slot, state, samusPosition);
                return;
            }
        }
        else
        {
            state.StablePositionTimer = NativeCountdown.Start(WreckedShipGhostStablePositionDuration);
            Offset16<RoomPixels> motion = samusPosition - state.PreviousSamusPosition;
            ushort horizontalClass = motion.X switch
            {
                < 0 => 0,
                0 => 4,
                _ => 8,
            };

            if (horizontalClass != state.SpawnColumnOffset)
            {
                state.SpawnColumnOffset = horizontalClass;
                state.StableDirectionTimer = NativeCountdown.Start(WreckedShipGhostStableDirectionDuration);
            }
            else
            {
                ushort verticalClass = motion.Y switch
                {
                    < 0 => 0,
                    0 => 12,
                    _ => 24,
                };

                if (verticalClass != state.SpawnRowOffset)
                {
                    state.SpawnRowOffset = verticalClass;
                    state.StableDirectionTimer = NativeCountdown.Start(WreckedShipGhostStableDirectionDuration);
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

        // The native tracker publishes a fresh asymmetric [position-1, position+1) box at
        // the end of every non-spawning frame.
        state.PreviousSamusPosition = samusPosition;
        state.SamusLowerBound = samusPosition - WreckedShipGhostStillMargin;
        state.SamusUpperBound = samusPosition + WreckedShipGhostStillMargin;
    }

    /// <summary>Installs the direction-selected relative position and begins $A8:9B42.</summary>
    internal static void SpawnWreckedShipGhost(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        Position16<RoomPixels> samusPosition)
    {
        int byteOffset = state.SpawnColumnOffset + state.SpawnRowOffset;
        if ((byteOffset & 3) != 0 || (uint)(byteOffset / 4) >= WreckedShipGhostAppearanceDefinitions.SpawnCount)
        {
            throw new InvalidDataException(
                $"Wrecked Ship ghost movement classes ${state.SpawnColumnOffset:X4}/" +
                $"${state.SpawnRowOffset:X4} do not select a retail spawn offset.");
        }

        (short xOffset, short yOffset) = WreckedShipGhostAppearanceDefinitions.SpawnOffset(byteOffset / 4);
        slot.Position = slot.Position.WithWhole(samusPosition + new Offset16<RoomPixels>(xOffset, yOffset));
        state.Function = WreckedShipGhostAiFunction.BrighteningAndFlickering;
        state.StablePositionTimer = NativeCountdown.Start(WreckedShipGhostStablePositionDuration);
        state.StableDirectionTimer = NativeCountdown.Start(WreckedShipGhostStableDirectionDuration);
    }

    /// <summary>Ports the white-flash ramp at $A8:9B42 and its flicker helper.</summary>
    internal void RunWreckedShipGhostBrightening(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        AdvanceWreckedShipGhostFlicker(slot, state);
        CgramPalette palette = WreckedShipGhostPalette(slot);
        int colorsChanged = 0;
        for (int color = 0; color < palette.Count; color++)
        {
            Bgr555 current = palette[color];
            if (current.Red >= 31)
                continue;

            // Native adds $0421 as a word, raising red, green, and blue together. It gates
            // only on red and does not saturate the others: a full green or blue carries
            // into the next field exactly as the word sum does, and CGRAM keeps 15 bits.
            palette.Write(color,
                Bgr555.FromCgramPortWord(unchecked((ushort)(current.ToWord() + WreckedShipGhostAppearanceDefinitions.ColorStepEachChannel))));
            colorsChanged++;
        }

        if (colorsChanged != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.FadingToGhostPalette;
        for (int color = 0; color < state.MutableTargetPalette.Length; color++)
            state.MutableTargetPalette[color] = WreckedShipGhostAppearanceDefinitions.PaletteColor(color);
    }

    /// <summary>Ports $A8:9BAD, including the simultaneous flicker and per-component fade.</summary>
    private void RunWreckedShipGhostPaletteFade(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state,
        SamusState samus)
    {
        int componentsChanged = StepWreckedShipGhostPaletteTowardTarget(slot, state);
        AdvanceWreckedShipGhostFlicker(slot, state);
        if (!state.PhaseTimer.IsZero || componentsChanged != 0)
            return;

        // Clearing $0500 makes the fully materialized ghost visible and eligible for the
        // ordinary touch/shot collision handlers authored by its enemy definition.
        slot.Properties = slot.Properties.Without(
            EnemyProperties.Invisible | EnemyProperties.IgnoreSamusCollision);
        state.Function = WreckedShipGhostAiFunction.BobbingWhileVisible;
        state.BobbingOriginY = slot.Position.Y.Whole;
        state.PhaseTimer = NativeCountdown.Start(WreckedShipGhostVisibleDuration);
        state.FlickerTableOffset = 2;
        state.VerticalVelocityFraction = 0;
        state.VerticalVelocityWhole = WreckedShipGhostInitialVerticalVelocityWhole;
        state.SpawnColumnOffset = 4;
        state.SpawnRowOffset = 12;
        state.PreviousSamusPosition = samus.Position.Whole;
        state.SamusLowerBound = samus.Position.Whole;
        state.SamusUpperBound = samus.Position.Whole;
        state.StablePositionTimer = NativeCountdown.Start(WreckedShipGhostStablePositionDuration);
        state.StableDirectionTimer = NativeCountdown.Start(WreckedShipGhostStableDirectionDuration);
    }

    /// <summary>Ports the signed 16.16 oscillation and 120-frame visible lifetime at $A8:9C8A.</summary>
    private static void RunWreckedShipGhostBobbing(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        uint position = slot.Position.Y.RawValue;
        uint velocity = new SnesFixedPosition(state.VerticalVelocityWhole, state.VerticalVelocityFraction).RawValue;
        position = unchecked(position + velocity);
        slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)position)));

        int acceleration = unchecked((short)(slot.Position.Y.Whole - state.BobbingOriginY)) < 0
            ? WreckedShipGhostVerticalAccelerationFraction
            : -WreckedShipGhostVerticalAccelerationFraction;
        velocity = unchecked((uint)(velocity + acceleration));
        state.VerticalVelocityWhole = unchecked((ushort)(SnesSixteenSixteen.WholeOf(velocity)));
        state.VerticalVelocityFraction = unchecked((ushort)velocity);

        state.PhaseTimer = state.PhaseTimer.Decrement().Remaining;
        if (!state.PhaseTimer.IsZero)
            return;

        state.Function = WreckedShipGhostAiFunction.WaitingForWhiteFade;
        slot.Properties = slot.Properties.With(EnemyProperties.IgnoreSamusCollision);
        state.MutableTargetPalette.Fill(Bgr555.White);
    }

    /// <summary>Ports $A8:9C69: finish whitening, hide, and restart the 120-frame delay.</summary>
    private void RunWreckedShipGhostWhiteFade(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        if (StepWreckedShipGhostPaletteTowardTarget(slot, state) != 0)
            return;

        state.Function = WreckedShipGhostAiFunction.InitialInvisibleDelay;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.PhaseTimer = NativeCountdown.Start(WreckedShipGhostVisibleDuration);
    }

    /// <summary>
    /// Ports $A8:9C31. The table alternates hidden and visible intervals whose lengths
    /// converge; its $FFFF terminator leaves the ghost permanently visible for this cycle.
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
                    $"Wrecked Ship ghost flicker offset ${state.FlickerTableOffset:X4} exceeds $A8:9ACC.");
            }

            short duration = WreckedShipGhostAppearanceDefinitions.FlickerDuration(tableIndex);
            if (duration < 0)
            {
                state.PhaseTimer = NativeCountdown.Start(0);
                state.FlickerTableOffset = 0;
                return;
            }

            state.PhaseTimer = NativeCountdown.FromRaw(unchecked((ushort)duration));
            ushort oldOffset = state.FlickerTableOffset;
            state.FlickerTableOffset = unchecked((ushort)(oldOffset + 2));
            if ((oldOffset & 2) != 0)
                return;
        }

        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
    }

    /// <summary>
    /// Ports $A8:9E88. Every BGR555 component moves exactly one hardware level toward its
    /// target per frame; the return value counts component changes, not changed colors.
    /// </summary>
    internal int StepWreckedShipGhostPaletteTowardTarget(
        RoomEnemySlot slot,
        WreckedShipGhostEnemyState state)
    {
        CgramPalette palette = WreckedShipGhostPalette(slot);
        int componentsChanged = 0;
        ReadOnlySpan<Bgr555> target = state.TargetPalette.Span;
        for (int color = 0; color < palette.Count; color++)
        {
            Bgr555 current = palette[color];
            Bgr555 next = current;
            next = StepWreckedShipGhostPaletteComponent(next, target[color], ColorChannel.Red, ref componentsChanged);
            next = StepWreckedShipGhostPaletteComponent(next, target[color], ColorChannel.Green, ref componentsChanged);
            next = StepWreckedShipGhostPaletteComponent(next, target[color], ColorChannel.Blue, ref componentsChanged);
            if (next != current)
                palette.Write(color, next);
        }
        return componentsChanged;
    }

    /// <summary>Moves one five-bit color component by a single level and counts an actual change.</summary>
    private static Bgr555 StepWreckedShipGhostPaletteComponent(
        Bgr555 current,
        Bgr555 target,
        ColorChannel channel,
        ref int componentsChanged)
    {
        int currentComponent = current[channel];
        int targetComponent = target[channel];
        if (currentComponent == targetComponent)
            return current;

        currentComponent += currentComponent > targetComponent ? -1 : 1;
        componentsChanged++;
        return current.With(channel, currentComponent);
    }

    /// <summary>Selects the ghost's OBJ palette row from its native palette index.</summary>
    private CgramPalette WreckedShipGhostPalette(RoomEnemySlot slot) =>
        _presentation.CgramRows.ObjectRow((slot.PaletteIndex >> 9) & 7);

    /// <summary>Rejects a ghost main callback before its slot has initialized apparition state.</summary>
    internal static WreckedShipGhostEnemyState RequireWreckedShipGhostState(RoomEnemySlot slot) =>
        (slot.Entity as WreckedShipGhostEntity)?.State ?? throw new InvalidOperationException(
            $"Wrecked Ship ghost slot {slot.SlotIndex} has no initialized native state.");

    /// <summary>Advances WreckedShipGhost's typed animation.</summary>
    internal static void AdvanceWreckedShipGhostTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireWreckedShipGhostState(slot);
        AnimationProgramCursor<WreckedShipGhostProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, WreckedShipGhostAnimations.Programs, ref cursor, WreckedShipGhostFrameDefinitions.Spritemap);
        state.Program = cursor;
    }

    /// <summary>Ports WreckedShipGhost's initialization AI.</summary>
    internal void RunWreckedShipGhostInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeWreckedShipGhost(slot);
    }

    /// <summary>Ports WreckedShipGhost's main AI.</summary>
    internal void RunWreckedShipGhostMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte _)
    {
        RunWreckedShipGhostMain(slot, RequireWreckedShipGhostState(slot), frame.Samus);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
