using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>The Waver species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class WaverBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Waver];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new WaverEntity(this, slot, common);

    /// <summary>Room collision service used to reflect horizontal motion and the sine wave on impact.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Connects Waver AI to room terrain collision.</summary>
    internal WaverBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out Waver's one animation cue.</summary>
    private static WaverProgram? ApplyWaverCue(WaverEnemyState state, WaverCue cue)
    {
        switch (cue.Kind)
        {
            case WaverCueKind.FinishSpin:
                // The four-frame spin hands its completion back to main AI rather than
                // going on to the steady animation itself.
                state.SpinFinished = true;
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(WaverCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Native horizontal speed of one and a half pixels per update in signed 16.16 form.</summary>
    private const int WaverHorizontalSpeedFixed = 98304;
    /// <summary>Four-pixel amplitude used by Waver's negative-sine vertical motion.</summary>
    private const int WaverVerticalRadius = 4;

    /// <summary>Ports <c>InitAI_Waver</c> at <c>$A3:86ED</c>.</summary>
    internal static void InitializeWaver(RoomEnemySlot slot)
    {
        bool facingRight = (slot.Parameter1 & 1) != 0;
        int signedSpeed = facingRight
            ? WaverHorizontalSpeedFixed
            : -WaverHorizontalSpeedFixed;
        var state = new WaverEnemyState(slot)
        {
            XSubvelocity = unchecked((ushort)signedSpeed),
            XVelocity = unchecked((short)(SnesSixteenSixteen.WholeOf(signedSpeed))),
            CurrentInstructionListIndex = WaverAnimationSelector.None,
            Angle = 0,
            SpinFinished = false,
            RequestedInstructionListIndex =
                (WaverAnimationSelector)(slot.Parameter1 & 1),
        };
        (slot.Entity as WaverEntity ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} is not bound to a Waver.")).State = state;

        // The native initializer first installs the left steady animation and then invokes the
        // change detector. Parameter bit zero therefore changes only right-facing spawns;
        // left-facing spawns intentionally observe current==requested and keep this one.
        state.Program = AnimationProgramCursor<WaverProgram>.Start(WaverProgram.SteadyFacingLeft);
        SetWaverProgram(slot, state);
    }

    /// <summary>Ports <c>MainAI_Waver</c> at <c>$A3:874C</c>.</summary>
    private void RunWaverMain(
        RoomEnemySlot slot,
        WaverEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Waver AI requires active room collision data.");

        int horizontalDisplacement =
            (SnesSixteenSixteen.FromWhole(state.XVelocity)) | state.XSubvelocity;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            // `$A3:875F-$8781` negates the overlapping velocity bytes and writes them back
            // as two words. For every initialized Waver this is exactly a signed 16.16
            // negation; expressing that value directly preserves the native ±1.5 px/frame.
            horizontalDisplacement = unchecked(-horizontalDisplacement);
            state.XSubvelocity = unchecked((ushort)horizontalDisplacement);
            state.XVelocity = unchecked((short)(SnesSixteenSixteen.WholeOf(horizontalDisplacement)));
            state.RequestedInstructionListIndex =
                (state.RequestedInstructionListIndex ^ WaverAnimationSelector.FacingRight) &
                WaverAnimationSelector.FacingRight;
            SetWaverProgram(slot, state);
        }
        else
        {
            int verticalPixels = EnemyMath.ReadEightBitNegativeSineProduct(
                state.Angle,
                WaverVerticalRadius);
            if (_terrain.MoveEnemyVertically(level, slot, SnesSixteenSixteen.FromWhole(verticalPixels)))
            {
                // Vertical collision reflects the sine phase by half a turn. The explicit
                // byte mask matters when a debugger has written high garbage into var3.
                state.Angle = unchecked((ushort)(unchecked((byte)(state.Angle + EnemyByteAngleDefinitions.HalfTurn))));
            }
            else
            {
                state.Angle = unchecked((ushort)(state.Angle + 2));
            }
        }

        // Once per half-wave, angle low seven bits equal $38. Waver temporarily swaps to
        // the four-map spin list without changing the facing bit.
        if ((state.Angle & EnemyByteAngleDefinitions.HalfWaveIndexMask) == 56)
        {
            state.RequestedInstructionListIndex |= WaverAnimationSelector.Spinning;
            SetWaverProgram(slot, state);
        }

        // The list's $86:E3 command sets this flag after 32 animation frames. Main AI owns
        // the return to steady art, matching the command/AI seam rather than timing it here.
        if (state.SpinFinished)
        {
            state.SpinFinished = false;
            state.RequestedInstructionListIndex &= WaverAnimationSelector.FacingRight;
            SetWaverProgram(slot, state);
        }
    }

    /// <summary>Starts a changed facing or spin animation and restarts its instruction and loop timers.</summary>
    internal static void SetWaverProgram(RoomEnemySlot slot, WaverEnemyState state)
    {
        WaverAnimationSelector requested = state.RequestedInstructionListIndex;
        if (requested == state.CurrentInstructionListIndex)
            return;

        state.CurrentInstructionListIndex = requested;
        state.Program = AnimationProgramCursor<WaverProgram>.Start(WaverAnimationDefinitions.Program(requested));
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Requires initialization state for a live Waver before processing its AI or spin opcode.</summary>
    internal static WaverEnemyState RequireWaverState(RoomEnemySlot slot) =>
        (slot.Entity as WaverEntity)?.State ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Waver state.");

    /// <summary>Ports Waver's initialization AI.</summary>
    internal static void RunWaverInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeWaver(slot);
    }

    /// <summary>Ports Waver's main AI.</summary>
    internal void RunWaverMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte _)
    {
        RunWaverMain(slot, RequireWaverState(slot), frame.Level);
    }

    /// <summary>Advances Waver's typed animation.</summary>
    internal static void AdvanceWaverTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireWaverState(slot);
        AnimationProgramCursor<WaverProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, WaverAnimations.Programs, ref cursor, WaverFrameDefinitions.Spritemap,
                (cue, _) => ApplyWaverCue(state, cue));
        state.Program = cursor;
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
