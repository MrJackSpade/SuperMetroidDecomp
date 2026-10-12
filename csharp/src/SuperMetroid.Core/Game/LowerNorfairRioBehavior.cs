using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Lower Norfair rio species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class LowerNorfairRioBehavior : IEnemySpecies
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Stores slot table.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Initializes a new lower norfair rio behavior instance.</summary>
    internal LowerNorfairRioBehavior(EnemyRoomBinding room, EnemyTerrain terrain, EnemySlotTable slotTable)
    {
        _room = room;
        _terrain = terrain;
        _slotTable = slotTable;
    }

    /// <summary>Stores lower norfair rio horizontal trigger distance.</summary>
    private const ushort LowerNorfairRioHorizontalTriggerDistance = 0x0070;
    /// <summary>Stores lower norfair rio gravity.</summary>
    private const ushort LowerNorfairRioGravity = 32;
    /// <summary>Stores lower norfair rio return sound.</summary>
    private const ushort LowerNorfairRioReturnSound = 0x0064;

    /// <summary>Stores lower norfair rio states.</summary>
    internal readonly LowerNorfairRioEnemyState?[] _lowerNorfairRioStates =
        new LowerNorfairRioEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Most recent library-two return sound request produced during this frame.</summary>
    public ushort? LastLowerNorfairRioSoundEffect { get; private set; }

    /// <summary>Resets lower norfair rio room state.</summary>
    internal void ResetLowerNorfairRioRoomState()
    {
        Array.Clear(_lowerNorfairRioStates);
        LastLowerNorfairRioSoundEffect = null;
    }

    /// <summary>Ports <c>LowerNorfairRio_Init</c> at $A2:C6F3.</summary>
    internal void InitializeLowerNorfairRio(RoomEnemySlot slot)
    {
        var state = new LowerNorfairRioEnemyState(slot)
        {
            AnimationSignal = false,
            FollowerVisible = false,
        };
        _lowerNorfairRioStates[slot.SlotIndex] = state;

        if (state.IsFollower)
        {
            ValidateLowerNorfairRioParent(slot);
            state.InstalledInstructionList = LowerNorfairRioProgram.Flames;
            slot.CurrentInstruction = ListStart(LowerNorfairRioProgram.Flames);
            state.Function = LowerNorfairRioEnemyFunction.FollowParent;
            return;
        }

        state.InstalledInstructionList = LowerNorfairRioProgram.Idle;
        slot.CurrentInstruction = ListStart(LowerNorfairRioProgram.Idle);
        state.Function = LowerNorfairRioEnemyFunction.WaitForAttackOpportunity;
    }

    /// <summary>
    /// Ports <c>LowerNorfairRio_Main</c> and functions one through six at $A2:C724-$C8A2.
    /// The shared RNG advances before every parent and follower dispatch, exactly as retail.
    /// </summary>
    private void RunLowerNorfairRioMain(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        ushort random = _room.Random.Generate();

        switch (state.Function)
        {
            case LowerNorfairRioEnemyFunction.FollowParent:
                FollowLowerNorfairRioParent(slot);
                return;

            case LowerNorfairRioEnemyFunction.WaitForAttackOpportunity:
                bool canAttack = samus is not null && (random & 0x0101) != 0 &&
                    EnemyMath.IsWithinStrictModularDistance(
                        samus.Position.X.Whole,
                        slot.Position.X.Whole,
                        LowerNorfairRioHorizontalTriggerDistance);
                if (!canAttack)
                {
                    // Unlike upper Norfair Rio, this variant clears the handshake and calls
                    // its list installer on every rejected frame. The helper itself prevents
                    // an unchanged $C61A list from restarting.
                    state.AnimationSignal = false;
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioProgram.Idle);
                    return;
                }

                state.Velocity = new(
                    new SnesSignedEightEight(RioLaunchDefinitions.LowerNorfairXVelocity),
                    new SnesSignedEightEight(RioLaunchDefinitions.LowerNorfairYVelocity));
                if (unchecked((short)(samus!.Position.X.Whole - slot.Position.X.Whole)) < 0)
                    NegateLowerNorfairRioXVelocity(state);
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioProgram.PrepareToSwoop);
                state.Function = LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation;
                return;

            case LowerNorfairRioEnemyFunction.WaitForTakeoffAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioProgram.Descending);
                state.Function = LowerNorfairRioEnemyFunction.Dive;
                return;

            case LowerNorfairRioEnemyFunction.Dive:
                RequireLowerNorfairRioLevel(level);
                MoveLowerNorfairRioHorizontally(level!, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level!,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    BeginLowerNorfairRioReturn(slot, state);
                    return;
                }

                ApplyLowerNorfairRioGravity(state);
                if (state.Velocity.Y.SignedRawValue < 0)
                    BeginLowerNorfairRioReturn(slot, state);
                return;

            case LowerNorfairRioEnemyFunction.ReturnToPerch:
                RequireLowerNorfairRioLevel(level);
                MoveLowerNorfairRioHorizontally(level!, slot, state);
                if (_terrain.MoveEnemyVertically(
                        level!,
                        slot,
                        EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.Y.RawValue)))
                {
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioProgram.Cooldown);
                    state.Function = LowerNorfairRioEnemyFunction.WaitForLandingAnimation;
                    return;
                }

                ApplyLowerNorfairRioGravity(state);
                if (state.AnimationSignal)
                {
                    state.AnimationSignal = false;
                    InstallLowerNorfairRioInstructionList(
                        slot,
                        state,
                        LowerNorfairRioProgram.AscendingPart2);
                }
                return;

            case LowerNorfairRioEnemyFunction.WaitForLandingAnimation:
                if (!state.AnimationSignal)
                    return;
                state.AnimationSignal = false;
                InstallLowerNorfairRioInstructionList(
                    slot,
                    state,
                    LowerNorfairRioProgram.Cooldown);
                state.Function = LowerNorfairRioEnemyFunction.WaitForAttackOpportunity;
                return;

            default:
                throw new InvalidDataException(
                    $"Lower Norfair Rio function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports all three private no-operand instructions at $A2:C6D2-$C6F2.</summary>
    private bool TryProcessLowerNorfairRioInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        // The shared interpreter hands every remaining negative word here; only
        // Holtz's private opcodes belong to this handler.
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Holtz ||
            !Enum.IsDefined((LowerNorfairRioInstruction)opcode))
            return false;

        LowerNorfairRioEnemyState state = RequireLowerNorfairRioState(slot);
        switch ((LowerNorfairRioInstruction)opcode)
        {
            case LowerNorfairRioInstruction.SetAnimationFinishedFlag:
                state.AnimationSignal = true;
                break;
            case LowerNorfairRioInstruction.HideFlames:
                state.FollowerVisible = false;
                break;
            case LowerNorfairRioInstruction.ShowFlames:
                state.FollowerVisible = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined Holtz instruction ${opcode:X4}.");
        }
        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    /// <summary>Follow lower norfair rio parent.</summary>
    private void FollowLowerNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingLowerNorfairRioSlot(follower);
        if (parent.Health == 0)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Deleted);
            return;
        }
        ValidateLowerNorfairRioParent(follower);

        LowerNorfairRioEnemyState parentState = RequireLowerNorfairRioState(parent);
        follower.FrozenTimer = parent.FrozenTimer;
        if (parent.FrozenTimer != 0 || !parentState.FollowerVisible)
        {
            follower.Properties = follower.Properties.With(EnemyProperties.Invisible);
            return;
        }

        // gEnemySpawnData(child)[31].field_4 again aliases the preceding parent's family
        // extra word $02. Here it is Boolean rather than a signed offset. The drawing-queue
        // X/Y aliases supply the live parent origin and this variant adds a literal 12 px.
        follower.Properties = follower.Properties.Without(EnemyProperties.Invisible);
        follower.Position = follower.Position.WithWhole(new(parent.Position.X.Whole, unchecked((ushort)(parent.Position.Y.Whole + 12))));
    }

    /// <summary>Begins lower norfair rio return.</summary>
    private void BeginLowerNorfairRioReturn(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(0xffff));
        InstallLowerNorfairRioInstructionList(
            slot,
            state,
            LowerNorfairRioProgram.AscendingPart1);
        state.Function = LowerNorfairRioEnemyFunction.ReturnToPerch;
        LastLowerNorfairRioSoundEffect = LowerNorfairRioReturnSound;
    }

    /// <summary>Moves lower norfair rio horizontally.</summary>
    private void MoveLowerNorfairRioHorizontally(
        RoomLevelData level,
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                EnemyMath.ToEightBitVelocityDisplacement(state.Velocity.X.RawValue)))
        {
            NegateLowerNorfairRioXVelocity(state);
        }
    }

    /// <summary>Negate lower norfair rio xvelocity.</summary>
    private static void NegateLowerNorfairRioXVelocity(LowerNorfairRioEnemyState state) =>
        state.Velocity = state.Velocity.ReversedX();

    /// <summary>Applies lower norfair rio gravity.</summary>
    private static void ApplyLowerNorfairRioGravity(LowerNorfairRioEnemyState state) =>
        state.Velocity = state.Velocity.WithY(new SnesSignedEightEight(
            unchecked((ushort)(state.Velocity.Y.RawValue - LowerNorfairRioGravity))));

    /// <summary>The bank-$A2 address at which <paramref name="program"/> begins, loaded into the enemy's instruction cursor.</summary>
    private static ushort ListStart(LowerNorfairRioProgram program) => (ushort)program;

    /// <summary>Installs lower norfair rio instruction list.</summary>
    private static void InstallLowerNorfairRioInstructionList(
        RoomEnemySlot slot,
        LowerNorfairRioEnemyState state,
        LowerNorfairRioProgram instructionList)
    {
        if (state.InstalledInstructionList == instructionList)
            return;
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = ListStart(instructionList);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Gets preceding lower norfair rio slot.</summary>
    private RoomEnemySlot GetPrecedingLowerNorfairRioSlot(RoomEnemySlot follower)
    {
        if (follower.SlotIndex == 0)
        {
            throw new InvalidDataException(
                "A Lower Norfair Rio follower cannot occupy enemy slot zero.");
        }
        return _slotTable._slots[follower.SlotIndex - 1];
    }

    /// <summary>Validates lower norfair rio parent.</summary>
    private void ValidateLowerNorfairRioParent(RoomEnemySlot follower)
    {
        RoomEnemySlot parent = GetPrecedingLowerNorfairRioSlot(follower);
        if (parent.EnemyDefinitionPointer != EnemyDefinitionId.Holtz ||
            (parent.Parameter1 & 0x8000) != 0)
        {
            throw new InvalidDataException(
                $"Lower Norfair Rio follower in slot {follower.SlotIndex} must immediately " +
                "follow a parent record of the same definition.");
        }
    }

    /// <summary>Requires lower norfair rio level.</summary>
    private static void RequireLowerNorfairRioLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Lower Norfair Rio movement requires room level data.");
    }

    /// <summary>Requires lower norfair rio state.</summary>
    private LowerNorfairRioEnemyState RequireLowerNorfairRioState(RoomEnemySlot slot) =>
        _lowerNorfairRioStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Lower Norfair Rio state.");

    /// <summary>Registers lower norfair rio initialization ai.</summary>
    internal void RegisterLowerNorfairRioInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Holtz, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Holtz, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeLowerNorfairRio(slot);
        });
    }

    /// <summary>Registers lower norfair rio main ai.</summary>
    internal void RegisterLowerNorfairRioMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Holtz, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Holtz, (slot, frame, nmiFrameCounter8) =>
        {
            RunLowerNorfairRioMain(
                slot,
                RequireLowerNorfairRioState(slot),
                frame.Samus,
                frame.Level);
        });
    }

    /// <summary>Registers lower norfair rio owned instructions.</summary>
    internal void RegisterLowerNorfairRioOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessLowerNorfairRioInstruction(slot, word, ref cursor);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetLowerNorfairRioRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastLowerNorfairRioSoundEffect = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastLowerNorfairRioSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
