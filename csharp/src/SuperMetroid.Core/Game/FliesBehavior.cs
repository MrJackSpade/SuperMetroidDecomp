namespace SuperMetroid.Core.Game;

/// <summary>The Flies species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class FliesBehavior : IEnemySpecies
{
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Initializes a new flies behavior instance.</summary>
    internal FliesBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Defines fly attack horizontal range.</summary>
    private const int FlyAttackHorizontalRange = 0x70;
    /// <summary>Stores fly states.</summary>
    internal readonly FlyEnemyState?[] _flyStates = new FlyEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>initialize fly.</summary>
    internal void InitializeFly(RoomEnemySlot slot)
    {
        var state = new FlyEnemyState(slot)
        {
            Angle = 0,
            Function = FlyEnemyFunction.ClockwiseCircle,
        };
        _flyStates[slot.SlotIndex] = state;
        slot.CurrentInstruction = FlyInstructionProgramDefinitions.Flight;
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
    }

    /// <summary>run fly main.</summary>
    private void RunFlyMain(RoomEnemySlot slot, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Fly AI requires the active Samus actor.");

        // $A2:B11F advances the global RNG even though this family does not consume the
        // returned word. The side effect matters when several random enemies share a room.
        _room.Random.Generate();
        FlyEnemyState state = RequireFlyState(slot);
        switch (state.Function)
        {
            case FlyEnemyFunction.ClockwiseCircle:
                RunFlyCircle(slot, state, samus, clockwise: true);
                return;
            case FlyEnemyFunction.AntiClockwiseCircle:
                RunFlyCircle(slot, state, samus, clockwise: false);
                return;
            case FlyEnemyFunction.AttackSamus:
                RunFlyAttack(slot, state);
                return;
            case FlyEnemyFunction.Retreat:
                MoveFlyAccordingToVelocities(slot, state);
                state.RetreatTimer = unchecked((ushort)(state.RetreatTimer - 1));
                if ((short)state.RetreatTimer < 0)
                {
                    state.RetreatTimer = 24;
                    state.Function = FlyEnemyFunction.ClockwiseCircle;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Fly function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>run fly circle.</summary>
    private static void RunFlyCircle(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus,
        bool clockwise)
    {
        if (state.RetreatTimer != 0)
        {
            state.RetreatTimer = unchecked((ushort)(state.RetreatTimer - 1));
        }
        else if (Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) <
            FlyAttackHorizontalRange)
        {
            SetFlyToAttackSamus(slot, state, samus);
            return;
        }

        MoveFlyAccordingToAngle(slot, state);
        state.Angle = unchecked((ushort)((state.Angle + (clockwise ? 0x20 : -0x20)) & 0x01ff));
        if (state.Angle == 0)
        {
            state.Function = clockwise
                ? FlyEnemyFunction.AntiClockwiseCircle
                : FlyEnemyFunction.ClockwiseCircle;
        }
    }

    /// <summary>set fly to attack samus.</summary>
    private static void SetFlyToAttackSamus(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus)
    {
        byte angle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
        state.Velocity = new(
            unchecked((short)(ReadSignedSineCosine(angle + 64) * 2)),
            unchecked((short)(ReadSignedSineCosine(angle) * 4)));
        state.TargetYPosition = samus.Position.Y.Whole;
        state.Function = FlyEnemyFunction.AttackSamus;
    }

    /// <summary>run fly attack.</summary>
    private static void RunFlyAttack(RoomEnemySlot slot, FlyEnemyState state)
    {
        MoveFlyAccordingToVelocities(slot, state);
        state.RetreatTimer = unchecked((ushort)(state.RetreatTimer + 1));

        bool crossedTarget = state.Velocity.Y.SignedRawValue < 0
            ? slot.Position.Y.Whole < state.TargetYPosition
            : slot.Position.Y.Whole >= state.TargetYPosition;
        if (!crossedTarget)
            return;

        state.Velocity = state.Velocity.ReversedY();
        state.Function = FlyEnemyFunction.Retreat;
    }

    /// <summary>move fly according to angle.</summary>
    private static void MoveFlyAccordingToAngle(RoomEnemySlot slot, FlyEnemyState state)
    {
        int tableIndex = state.Angle >> 1;
        slot.Position = slot.Position.AdvanceX(new SnesSignedEightEight(unchecked((ushort)ReadSignedSineCosine(tableIndex + 64))).ToSixteenSixteenDelta());
        slot.Position = slot.Position.AdvanceY(new SnesSignedEightEight(unchecked((ushort)ReadSignedSineCosine(tableIndex))).ToSixteenSixteenDelta());
    }

    /// <summary>move fly according to velocities.</summary>
    private static void MoveFlyAccordingToVelocities(RoomEnemySlot slot, FlyEnemyState state)
    {
        slot.Position = slot.Position.Advance(state.Velocity.ToSixteenSixteen());
    }

    /// <summary>read signed sine cosine.</summary>
    internal static short ReadSignedSineCosine(int index)
    {
        // The backing table begins with negative cosine at index zero, sine at index 64,
        // and continues far enough for every byte angle plus the cosine phase offset.
        return EnemyTrigonometryTables.SignedNegativeCosineWord(index);
    }

    /// <summary>require fly state.</summary>
    private FlyEnemyState RequireFlyState(RoomEnemySlot slot) =>
        _flyStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized fly state.");

    /// <summary>register flies initialization ai.</summary>
    internal void RegisterFliesInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Mellow_Mella_Menu, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeFly(slot);
        });
    }

    /// <summary>register flies main ai.</summary>
    internal void RegisterFliesMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Mellow_Mella_Menu, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu, (slot, frame, nmiFrameCounter8) =>
        {
            RunFlyMain(slot, frame.Samus);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_flyStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
