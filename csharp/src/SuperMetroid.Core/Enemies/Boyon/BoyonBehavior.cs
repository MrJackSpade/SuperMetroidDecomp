using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Boyon.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Boyon;

/// <summary>The Boyon species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class BoyonBehavior : IEnemySpecies
{
    /// <summary>Carries out one cue of Boyon's animations.</summary>
    private BoyonProgram? ApplyBoyonCue(RoomEnemySlot slot, BoyonEnemyState state, BoyonCue cue)
    {
        switch (cue.Kind)
        {
            case BoyonCueKind.Return:
                // `$A2:88C5` is an explicit RTL instruction with no effect; keeping it documents the idle-list seam.
                break;
            case BoyonCueKind.StartBounce:
                StartBoyonBounce(state);
                break;
            case BoyonCueKind.ProcessOffScreen:
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                break;
            case BoyonCueKind.StopProcessingOffScreen:
                slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BoyonCueKind)} {cue.Kind}.");
        }
        return null;
    }


    /// <summary>Per-slot bounce state cleared whenever a new room loads.</summary>
    internal readonly BoyonEnemyState?[] _boyonStates =
        new BoyonEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Last library-two bounce sound requested during the current enemy frame.</summary>
    public ushort? LastBoyonSoundEffect { get; private set; }

    /// <summary>Ports <c>InitAI_Boyon</c> at <c>$A2:871C</c>.</summary>
    internal void InitializeBoyon(RoomEnemySlot slot)
    {
        int multiplierIndex = SnesLittleEndian.LowByte(slot.Parameter1);
        int jumpHeightIndex = SnesLittleEndian.HighByte(slot.Parameter1);
        if (multiplierIndex >= 8 || jumpHeightIndex >= 9)
        {
            throw new InvalidDataException(
                $"Boyon parameter one ${slot.Parameter1:X4} indexes beyond its " +
                "eight multipliers or nine jump heights.");
        }

        // Initialization writes the ordinary common-bank empty map before selecting the
        // idle list. RoomEnemySystem's load tail intentionally restores the same transient
        // empty map; the first processed instruction frame installs real bank-$A2 art.
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;

        var state = new BoyonEnemyState(slot)
        {
            BounceMovement = BoyonBounceMovement.Rising,
            SpeedMultiplier = BoyonSpeedDefinitions.InitialMultiplier(multiplierIndex),
            JumpHeight = BoyonSpeedDefinitions.InitialHeight(jumpHeightIndex),
            DistanceAccumulator = 0,
            SpeedTableIndex = 0,
            Bouncing = false,
            InitialBounceSpeedTableIndex = 0,
            BounceDisabled = false,
            SpeedMultiplierMirror = 0,
            IdleDisabled = false,
            BounceSpeedCalculated = false,
        };
        _boyonStates[slot.SlotIndex] = state;
        SetBoyonProgram(slot, state, BoyonProgram.Idle);
    }

    /// <summary>Ports <c>MainAI_Boyon</c> at <c>$A2:879C</c>.</summary>
    internal static void RunBoyonMain(RoomEnemySlot slot, BoyonEnemyState state, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Boyon proximity AI requires the active Samus actor.");

        // The cartridge spends Boyon's first active AI frame deriving the curve index and
        // returns without moving or testing proximity. Preserving that frame is important:
        // animation and main AI otherwise become one frame early immediately after a load.
        if (!state.BounceSpeedCalculated)
        {
            CalculateInitialBoyonBounceSpeed(state);
            state.BounceSpeedCalculated = true;
            return;
        }

        state.SpeedMultiplierMirror = state.SpeedMultiplier;
        bool samusIsClose = IsBoyonSamusWithinHorizontalRange(slot, state, samus);
        if (samusIsClose)
        {
            // Proximity continuously permits another bounce. The separate Bouncing latch
            // prevents resetting the animation list every frame during the current arc.
            state.BounceDisabled = false;
            state.IdleDisabled = false;
            if (!state.Bouncing)
            {
                state.Bouncing = true;
                SetBoyonProgram(slot, state, BoyonProgram.Bouncing);
            }
        }
        else if (state.BounceDisabled)
        {
            // Boyon may only return to idle after the falling half has reached its baseline.
            // Leaving the trigger range in midair therefore finishes the current arc.
            if (!state.IdleDisabled)
            {
                state.IdleDisabled = true;
                SetBoyonProgram(slot, state, BoyonProgram.Idle);
            }
            return;
        }

        // The native routine restores variable A from its mirror before indirect dispatch.
        // This looks redundant for retail data but is observable and prevents an animation
        // command or debugger edit during this frame from changing the current displacement.
        state.SpeedMultiplier = state.SpeedMultiplierMirror;
        switch (state.BounceMovement)
        {
            case BoyonBounceMovement.Falling:
                MoveBoyonFalling(slot, state);
                break;
            case BoyonBounceMovement.Rising:
                MoveBoyonRising(slot, state);
                break;
            default:
                throw new InvalidDataException(
                    $"Boyon movement-table index {(ushort)state.BounceMovement} is not translated.");
        }
    }

    /// <summary>Ports the one-time loop at <c>$A2:8755</c>.</summary>
    private static void CalculateInitialBoyonBounceSpeed(BoyonEnemyState state)
    {
        // Every iteration multiplies two low bytes through hardware registers $4202/$4203,
        // adds the 16-bit product with wraparound, then compares via the 65C816 negative
        // flag. The curve saturates at $FF once its 23 stored bytes are exhausted.
        for (int guard = 0; guard < ushort.MaxValue; guard++)
        {
            state.Speed = MultiplyBoyonCurveEntry(state);
            state.DistanceAccumulator = unchecked((ushort)(
                state.DistanceAccumulator + state.Speed));
            state.SpeedTableIndex = unchecked((ushort)(state.SpeedTableIndex + 1));

            if (!EnemyMath.IsNegative16(state.DistanceAccumulator - state.JumpHeight))
            {
                state.InitialBounceSpeedTableIndex = state.SpeedTableIndex;
                state.BounceDisabled = true;
                state.IdleDisabled = true;
                return;
            }
        }

        throw new InvalidDataException(
            "Boyon initial bounce-speed calculation did not reach its ROM jump height.");
    }

    /// <summary>Ports falling movement at <c>$A2:8801</c>.</summary>
    private static void MoveBoyonFalling(RoomEnemySlot slot, BoyonEnemyState state)
    {
        state.SpeedTableIndex = unchecked((ushort)(state.SpeedTableIndex + 1));
        state.Speed = MultiplyBoyonCurveEntry(state);

        // XBA/AND #$00FF discards the product's fractional byte. Boyon has no Y subposition
        // integration here: each frame moves by exactly the product's high byte.
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + (SnesEightEight.WholeOf(state.Speed)))));
        if (EnemyMath.IsNegative16(state.SpeedTableIndex - state.InitialBounceSpeedTableIndex))
            return;

        state.BounceMovement = BoyonBounceMovement.Rising;
        state.BounceDisabled = true;
        state.Bouncing = false;
    }

    /// <summary>Ports rising movement at <c>$A2:8850</c>.</summary>
    private static void MoveBoyonRising(RoomEnemySlot slot, BoyonEnemyState state)
    {
        state.Speed = MultiplyBoyonCurveEntry(state);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole - (SnesEightEight.WholeOf(state.Speed)))));
        state.SpeedTableIndex = NativeWordCounter.Decrement(state.SpeedTableIndex).Value;
        if ((state.SpeedTableIndex & 0x8000) != 0)
            state.BounceMovement = BoyonBounceMovement.Falling;
    }

    /// <summary>
    /// Recreates the native 8×8 unsigned multiply for the current curve index. Values after
    /// the stored table use the explicit saturated byte rather than reading adjacent ROM.
    /// </summary>
    internal static ushort MultiplyBoyonCurveEntry(BoyonEnemyState state) =>
        BoyonSpeedDefinitions.Multiply(state.SpeedTableIndex, state.SpeedMultiplier);

    /// <summary>Ports <c>IsSamusWithinAPixelColumnsOfEnemy</c> for Boyon's init1 radius.</summary>
    private static bool IsBoyonSamusWithinHorizontalRange(
        RoomEnemySlot slot,
        BoyonEnemyState state,
        SamusState samus)
    {
        _ = state; // Retains a symmetric family-helper signature for debugger inspection.
        int distance = Math.Abs((int)unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)));
        return EnemyMath.IsNegative16(distance - slot.Parameter2);
    }

    /// <summary>Starts the idle or bounce animation and restarts its instruction and auxiliary timers.</summary>
    private static void SetBoyonProgram(RoomEnemySlot slot, BoyonEnemyState state, BoyonProgram program)
    {
        state.Program = AnimationProgramCursor<BoyonProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Returns the initialized state paired with this physical enemy slot.</summary>
    private BoyonEnemyState RequireBoyonState(RoomEnemySlot slot) =>
        _boyonStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Boyon state.");

    /// <summary>Instruction <c>$A2:88C6</c>: permit the arc and request sound $0E.</summary>
    private void StartBoyonBounce(BoyonEnemyState state)
    {
        state.BounceDisabled = false;
        LastBoyonSoundEffect = EnemySoundEffectDefinitions.BoyonBounceSound;
    }

    /// <summary>Registers Boyon's guarded native initialization routine.</summary>
    internal void RegisterBoyonInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Boyon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Boyon, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeBoyon(slot);
        });
    }

    /// <summary>Registers Boyon's guarded per-frame bounce routine.</summary>
    internal void RegisterBoyonMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Boyon, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Boyon, (slot, frame, nmiFrameCounter8) =>
        {
            RunBoyonMain(slot, RequireBoyonState(slot), frame.Samus);
        });
    }

    /// <summary>Runs Boyon's idle and bouncing loops as typed programs (#641).</summary>
    internal void RegisterBoyonTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Boyon, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireBoyonState(slot);
            AnimationProgramCursor<BoyonProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, BoyonAnimations.Programs, ref cursor, BoyonFrameDefinitions.Spritemap,
                    (cue, _) => ApplyBoyonCue(slot, state, cue));
            state.Program = cursor;
        });


    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastBoyonSoundEffect = null;
        Array.Clear(_boyonStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastBoyonSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastBoyonSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
