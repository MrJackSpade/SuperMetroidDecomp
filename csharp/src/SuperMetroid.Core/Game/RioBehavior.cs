using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class RioBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemyTerrain _terrain;

    internal RioBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    private const ushort RioTriggerDistance = 0x00a0;
    private const ushort RioDiveSound = 0x0065;
    private const ushort RioGravityStep = 24;

    // These are live ROM words, not friendly host tuning constants. The retail routine
    // reads them at $A2:BBBB/$A2:BBBF immediately before a dive begins.

    internal readonly RioEnemyState?[] _rioStates = new RioEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Most recent library-two sound request produced by Rio during this frame.</summary>
    public ushort? LastRioSoundEffect { get; private set; }

    internal void ResetRioRoomState()
    {
        Array.Clear(_rioStates);
        LastRioSoundEffect = null;
    }

    /// <summary>Ports <c>Rio_Init</c> at $A2:BBCD.</summary>
    internal void InitializeRio(RoomEnemySlot slot)
    {
        var state = new RioEnemyState(slot)
        {
            AnimationFinished = false,
            InstalledInstructionList = 0,
            Function = RioEnemyFunction.WaitingForSamus,
        };
        _rioStates[slot.SlotIndex] = state;

        // Rio_Init writes the list directly rather than calling Rio_6. Variable F must
        // therefore remain zero until the first behavioral transition installs a new list.
        slot.CurrentInstruction = RioInstructionProgramDefinitions.Idle;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Ports <c>Rio_Main</c> and Rio_1..Rio_5 at $A2:BBE3-$BD53.</summary>
    private void RunRioMain(
        RoomEnemySlot slot,
        RioEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        Position16<RoomPixels> camera)
    {
        // Rio_Main advances the global enemy RNG even though this particular family never
        // consumes the returned word. Omitting that apparently-useless call changes every
        // later random enemy in the room, so it is part of Rio's observable behavior.
        _room.Random.Generate();

        switch (state.Function)
        {
            case RioEnemyFunction.WaitingForSamus:
                if (samus is null || !EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        RioTriggerDistance))
                {
                    return;
                }

                state.Velocity = new EightEightVector<RoomPixels>(
                    new SnesSignedEightEight(RioLaunchDefinitions.RioXVelocity),
                    new SnesSignedEightEight(RioLaunchDefinitions.RioYVelocity));
                if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0)
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                InstallRioInstructionList(
                    slot,
                    state,
                    RioInstructionProgramDefinitions.SwoopingPart1);
                state.Function = RioEnemyFunction.Diving;

                // CheckIfEnemyIsOnScreen returns zero for an on-screen origin. The native
                // branch consequently queues sound $65 only for the visible attack start.
                if (!EnemyProcessingWindow.EnemyWithNormalSpritesIsOffScreen(slot, camera))
                    LastRioSoundEffect = RioDiveSound;
                return;

            case RioEnemyFunction.WaitingForLandingAnimation:
                if (!state.AnimationFinished)
                    return;
                state.AnimationFinished = false;
                InstallRioInstructionList(
                    slot,
                    state,
                    RioInstructionProgramDefinitions.PostSwoopIdle);
                state.Function = RioEnemyFunction.WaitingForSamus;
                return;

            case RioEnemyFunction.Diving:
                RequireRioLevel(level);
                if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level!,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X.RawValue)))
                {
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }
                if (_terrain.MoveEnemyVertically(
                        level!,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    ReverseRioVerticalVelocityAndBounce(state);
                    return;
                }

                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - RioGravityStep))));
                if (state.Velocity.Y.SignedRawValue < 0)
                {
                    state.SavedHorizontalVelocity = state.Velocity.X.RawValue;
                    state.Velocity = new EightEightVector<RoomPixels>(0, 0);
                    state.Function = RioEnemyFunction.HoveringTowardSamus;
                }
                else if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    InstallRioInstructionList(
                        slot,
                        state,
                        RioInstructionProgramDefinitions.SwoopingPart2);
                }
                return;

            case RioEnemyFunction.BouncingBackToPerch:
                RequireRioLevel(level);
                if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                        level!,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X.RawValue)))
                {
                    state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.X.SignedRawValue)));
                }
                if (_terrain.MoveEnemyVertically(
                        level!,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    InstallRioInstructionList(
                        slot,
                        state,
                        RioInstructionProgramDefinitions.SwoopCooldown);
                    state.Function = RioEnemyFunction.WaitingForLandingAnimation;
                }
                else
                {
                    state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)(state.Velocity.Y.RawValue - RioGravityStep))));
                }
                return;

            case RioEnemyFunction.HoveringTowardSamus:
                if (samus is null)
                    throw new InvalidOperationException("Rio homing requires the active Samus actor.");
                RequireRioLevel(level);

                // Once Rio reaches Samus's Y coordinate from above, it restores the saved
                // horizontal dive speed and starts the upward/bounce phase at -1/256 px.
                if (unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)) >= 0)
                {
                    state.Velocity = new EightEightVector<RoomPixels>(
                        new SnesSignedEightEight(state.SavedHorizontalVelocity),
                        new SnesSignedEightEight(0xffff));
                    state.Function = RioEnemyFunction.BouncingBackToPerch;
                    return;
                }

                byte angle = EnemyMath.CalculateCartridgeAngle(
                    new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
                state.Velocity = state.Velocity.WithX(new SnesSignedEightEight(ReadRioSignedSineCosineSample(
                    unchecked((byte)(angle + 0x40)))));
                state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(ReadRioSignedSineCosineSample(angle)));
                _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                    level!,
                    slot,
                    EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X.RawValue));
                _terrain.MoveEnemyVertically(
                    level!,
                    slot,
                    EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue));
                return;

            default:
                throw new InvalidDataException(
                    $"Rio function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the one private instruction at $A2:BBC3.</summary>
    private bool TryProcessRioInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Rio ||
            opcode != RioInstructionCodes.SetAnimationFinished)
            return false;

        RequireRioState(slot).AnimationFinished = true;
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    /// <summary>Ports Rio_6: install a list only when its address actually changes.</summary>
    private static void InstallRioInstructionList(
        RoomEnemySlot slot,
        RioEnemyState state,
        ushort instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private static void ReverseRioVerticalVelocityAndBounce(RioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(unchecked((ushort)-state.Velocity.Y.SignedRawValue)));
        state.Function = RioEnemyFunction.BouncingBackToPerch;
    }

    /// <summary>
    /// Reads one sign-extended entry from kSinCosTable8bit_Sext at $A0:B443. Rio consumes
    /// the raw table sample as an 8.8 velocity; unlike Rinka, it performs no speed multiply.
    /// </summary>
    internal static ushort ReadRioSignedSineCosineSample(byte angle) =>
        unchecked((ushort)EnemyTrigonometryTables.SignedSine(angle));

    private static void RequireRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Rio movement requires the current room level data.");
    }

    private RioEnemyState RequireRioState(RoomEnemySlot slot) =>
        _rioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Rio state.");

    internal void RegisterRioInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Rio, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rio, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeRio(slot);
        });
    }

    internal void RegisterRioMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Rio, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Rio, (slot, frame, nmiFrameCounter8) =>
        {
            RunRioMain(
                slot,
                RequireRioState(slot),
                frame.Samus,
                frame.Level,
                frame.Camera);
        });
    }

    internal void RegisterRioOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessRioInstruction(slot, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetRioRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastRioSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastRioSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
