using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Waver.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>The Waver species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class WaverBehavior : IEnemySpecies
{
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
    private const int WaverHorizontalSpeedFixed = 0x00018000;
    /// <summary>Four-pixel amplitude used by Waver's negative-sine vertical motion.</summary>
    private const int WaverVerticalRadius = 4;

    /// <summary>Per-enemy motion and animation state indexed by the physical room slot.</summary>
    internal readonly WaverEnemyState?[] _waverStates =
        new WaverEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Waver</c> at <c>$A3:86ED</c>.</summary>
    internal void InitializeWaver(RoomEnemySlot slot)
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
        _waverStates[slot.SlotIndex] = state;

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
                state.Angle = unchecked((ushort)(unchecked((byte)(state.Angle + 0x80))));
            }
            else
            {
                state.Angle = unchecked((ushort)(state.Angle + 2));
            }
        }

        // Once per half-wave, angle low seven bits equal $38. Waver temporarily swaps to
        // the four-map spin list without changing the facing bit.
        if ((state.Angle & 0x7f) == 0x38)
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
    private WaverEnemyState RequireWaverState(RoomEnemySlot slot) =>
        _waverStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Waver state.");

    /// <summary>Registers the Waver-specific slot initializer under its bank-$A3 AI pointer.</summary>
    internal void RegisterWaverInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Waver, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Waver, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeWaver(slot);
        });
    }

    /// <summary>Registers Waver's collision-driven horizontal and sine-wave movement dispatcher.</summary>
    internal void RegisterWaverMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Waver, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Waver, (slot, frame, nmiFrameCounter8) =>
        {
            RunWaverMain(slot, RequireWaverState(slot), frame.Level);
        });
    }

    /// <summary>Runs Waver's animations as typed programs (#641); the spin's completion cue goes back to main AI.</summary>
    internal void RegisterWaverTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Waver, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireWaverState(slot);
            AnimationProgramCursor<WaverProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, WaverAnimations.Programs, ref cursor, WaverFrameDefinitions.Spritemap,
                    (cue, _) => ApplyWaverCue(state, cue));
            state.Program = cursor;
        });

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_waverStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
