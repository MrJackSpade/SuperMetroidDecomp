using SuperMetroid.Core.Audio;
namespace SuperMetroid.Core.Game;

/// <summary>The Norfair lava-jumping enemy species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class NorfairLavaJumpingEnemyBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemySlotTable _slotTable;

    internal NorfairLavaJumpingEnemyBehavior(EnemyRoomBinding room, EnemySlotTable slotTable)
    {
        _room = room;
        _slotTable = slotTable;
    }

    private const ushort NorfairLavaJumpingEnemyGravity = 56;
    private const ushort NorfairLavaJumpingEnemyAnimationSwitchVelocity = 0xfc00;
    private const ushort NorfairLavaJumpingEnemyJumpSound = 0x000d;

    internal readonly NorfairLavaJumpingEnemyState?[] _norfairLavaJumpingEnemyStates =
        new NorfairLavaJumpingEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Most recent library-two jump sound request emitted during this frame.</summary>
    public ushort? LastNorfairLavaJumpingEnemySoundEffect { get; private set; }

    internal void ResetNorfairLavaJumpingEnemyRoomState()
    {
        Array.Clear(_norfairLavaJumpingEnemyStates);
        LastNorfairLavaJumpingEnemySoundEffect = null;
    }

    /// <summary>Ports <c>NorfairLavajumpingEnemy_Init</c> at $A2:BE99.</summary>
    internal void InitializeNorfairLavaJumpingEnemy(RoomEnemySlot slot)
    {
        var state = new NorfairLavaJumpingEnemyState(slot)
        {
            AnimationFinished = false,
            InstalledInstructionList = 0,
        };
        _norfairLavaJumpingEnemyStates[slot.SlotIndex] = state;

        if (state.IsFollower)
        {
            // The retail population always places this cosmetic second half immediately
            // after its moving parent. Its main AI relies on WRAM aliasing across those two
            // physical 64-byte records, so reject reordered/custom populations explicitly.
            ValidateNorfairLavaJumpingEnemyParent(slot);
            slot.CurrentInstruction = NorfairLavaJumperInstructionProgramDefinitions.Follower;
            state.Function = NorfairLavaJumpingEnemyFunction.FollowParent;
            return;
        }

        // The jumping parent returns to exactly these population coordinates after every
        // arc. Subpositions are deliberately not saved or cleared by the retail routine.
        state.SpawnPosition = slot.Position.Whole;
        slot.CurrentInstruction = NorfairLavaJumperInstructionProgramDefinitions.Hidden;
        state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
    }

    /// <summary>
    /// Ports <c>NorfairLavajumpingEnemy_Main</c> and functions one through five at
    /// $A2:BED2-$C011. Main advances the shared enemy RNG once for every parent and follower
    /// frame; even the follower discards the word, which still affects later room actors.
    /// </summary>
    internal void RunNorfairLavaJumpingEnemyMain(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state)
    {
        ushort random = _room.Random.Generate();

        switch (state.Function)
        {
            case NorfairLavaJumpingEnemyFunction.FollowParent:
                FollowNorfairLavaJumpingEnemyParent(slot);
                return;

            case NorfairLavaJumpingEnemyFunction.BeginJump:
                state.YVelocity = NorfairLavaJumpDefinitions.InitialVerticalVelocity(random);
                state.Function = NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch;
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                LastNorfairLavaJumpingEnemySoundEffect = NorfairLavaJumpingEnemyJumpSound;
                return;

            case NorfairLavaJumpingEnemyFunction.RiseBeforeAnimationSwitch:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);
                state.YVelocity = unchecked((ushort)(state.YVelocity + NorfairLavaJumpingEnemyGravity));

                // The 65816 CMP is unsigned. Starting jump velocities are near $F800; the
                // test becomes true while still negative at $FC00, before the arc apex.
                if (state.YVelocity >= NorfairLavaJumpingEnemyAnimationSwitchVelocity)
                {
                    InstallNorfairLavaJumpingEnemyInstructionList(
                        slot,
                        state,
                        NorfairLavaJumperInstructionProgramDefinitions.Jump);
                    state.Function = NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal;
                }
                return;

            case NorfairLavaJumpingEnemyFunction.MoveUntilAnimationSignal:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);
                state.YVelocity = unchecked((ushort)(state.YVelocity + NorfairLavaJumpingEnemyGravity));
                if (state.AnimationFinished)
                {
                    state.AnimationFinished = false;
                    state.Function = NorfairLavaJumpingEnemyFunction.FallBackIntoLava;
                }
                return;

            case NorfairLavaJumpingEnemyFunction.FallBackIntoLava:
                MoveNorfairLavaJumpingEnemyVertically(slot, state);

                // The low screen-row nibble is the retail lava-entry sentinel. Once the
                // actor reaches row $xF0 it snaps to its exact population origin, restores
                // the hidden list, and sleeps off-screen until the next jump begins.
                if ((slot.Position.Y.Whole & 0x00f0) == 0x00f0)
                {
                    slot.Position = slot.Position.WithWhole(state.SpawnPosition);
                    InstallNorfairLavaJumpingEnemyInstructionList(
                        slot,
                        state,
                        NorfairLavaJumperInstructionProgramDefinitions.Hidden);
                    state.Function = NorfairLavaJumpingEnemyFunction.BeginJump;
                    slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                }
                else
                {
                    state.YVelocity = unchecked((ushort)(state.YVelocity + NorfairLavaJumpingEnemyGravity));
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Norfair lava-jumping enemy function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the private no-operand animation handshake at $A2:BE8E.</summary>
    private bool TryProcessNorfairLavaJumpingEnemyInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Squeept ||
            opcode != NorfairLavaJumperInstructionProgramDefinitions.AnimationFinishedCallback)
        {
            return false;
        }

        RequireNorfairLavaJumpingEnemyState(slot).AnimationFinished = true;
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    private void FollowNorfairLavaJumpingEnemyParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairLavaJumpingEnemySlot(follower);

        // These four reads look nonsensical in the disassembly because the routine indexes
        // enemy_drawing_queue with offsets +100/+109/+2/+93. For a follower one slot after
        // its parent, those WRAM addresses alias the preceding EnemyData record exactly:
        // health, frozen timer, variable C (Y velocity), and Y position respectively.
        if (parent.Health == 0)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Deleted);
            return;
        }

        // Pair identity is checked at population initialization, not on every AI frame.
        // The native flame routine reads the preceding physical record even after its
        // header has been cleared: script deletion does not zero health or movement words.
        // Requiring a live definition here rejects valid retained-WRAM reads (#322).

        follower.FrozenTimer = parent.FrozenTimer;
        if (parent.FrozenTimer != 0 || (parent.VariableC & 0x8000) == 0)
        {
            // The second half appears only while the parent is travelling upward. At the
            // apex and during descent, the parent animation alone owns the visible shape.
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.Position = follower.Position.WithWholeY(parent.Position.Y.Whole);
    }

    private RoomEnemySlot GetPrecedingNorfairLavaJumpingEnemySlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
        {
            throw new InvalidDataException(
                "A Norfair lava-jumping follower cannot occupy enemy slot zero.");
        }

        return _slotTable._slots[follower.SlotIndex - 1];
    }

    private void ValidateNorfairLavaJumpingEnemyParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingNorfairLavaJumpingEnemySlot(follower);
        if (parent.EnemyDefinitionPointer != EnemyDefinitionId.Squeept ||
            (parent.Parameter1 & 0x8000) != 0)
        {
            throw new InvalidDataException(
                $"Norfair lava-jumping follower in slot {follower.SlotIndex} must immediately follow " +
                "a parent record of the same definition. " +
                $"Preceding slot: definition=${(int)parent.EnemyDefinitionPointer:X4}, " +
                $"health=${parent.Health:X4}, parameter=${parent.Parameter1:X4}, properties={parent.Properties}.");
        }
    }

    private static void MoveNorfairLavaJumpingEnemyVertically(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state)
    {
        slot.Position = slot.Position.AdvanceY(new SnesSignedEightEight(state.YVelocity).ToSixteenSixteenDelta());
    }

    /// <summary>Ports function six at $A2:C012 without restarting an unchanged list.</summary>
    private static void InstallNorfairLavaJumpingEnemyInstructionList(
        RoomEnemySlot slot,
        NorfairLavaJumpingEnemyState state,
        ushort instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;

        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private NorfairLavaJumpingEnemyState RequireNorfairLavaJumpingEnemyState(RoomEnemySlot slot) =>
        _norfairLavaJumpingEnemyStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Norfair lava-jumping state.");

    internal void RegisterNorfairLavaJumpingEnemyInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Squeept, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Squeept, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeNorfairLavaJumpingEnemy(slot);
        });
    }

    internal void RegisterNorfairLavaJumpingEnemyMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Squeept, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Squeept, (slot, frame, nmiFrameCounter8) =>
        {
            RunNorfairLavaJumpingEnemyMain(
                slot,
                RequireNorfairLavaJumpingEnemyState(slot));
        });
    }

    internal void RegisterNorfairLavaJumpingEnemyOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessNorfairLavaJumpingEnemyInstruction(slot, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetNorfairLavaJumpingEnemyRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastNorfairLavaJumpingEnemySoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastNorfairLavaJumpingEnemySoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
