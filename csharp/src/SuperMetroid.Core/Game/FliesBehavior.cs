namespace SuperMetroid.Core.Game;

/// <summary>The Flies species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class FliesBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;

    internal FliesBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }

    private const int FlyAttackHorizontalRange = 0x70;
    internal readonly FlyEnemyState?[] _flyStates = new FlyEnemyState?[RoomEnemySystem.MaximumEnemyCount];

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
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

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
                state.RetreatTimer = state.RetreatTimer.Decrement().Remaining;
                if (state.RetreatTimer.IsNegative)
                {
                    state.RetreatTimer = NativeCountdown.Start(24);
                    state.Function = FlyEnemyFunction.ClockwiseCircle;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Fly function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void RunFlyCircle(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus,
        bool clockwise)
    {
        if (!state.RetreatTimer.IsZero)
        {
            state.RetreatTimer = state.RetreatTimer.Decrement().Remaining;
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

    private static void RunFlyAttack(RoomEnemySlot slot, FlyEnemyState state)
    {
        MoveFlyAccordingToVelocities(slot, state);
        state.RetreatTimer = NativeCountdown.FromRaw(unchecked((ushort)(state.RetreatTimer.RawValue + 1)));

        bool crossedTarget = state.Velocity.Y.SignedRawValue < 0
            ? slot.Position.Y.Whole < state.TargetYPosition
            : slot.Position.Y.Whole >= state.TargetYPosition;
        if (!crossedTarget)
            return;

        state.Velocity = state.Velocity.ReversedY();
        state.Function = FlyEnemyFunction.Retreat;
    }

    private static void MoveFlyAccordingToAngle(RoomEnemySlot slot, FlyEnemyState state)
    {
        int tableIndex = state.Angle >> 1;
        slot.Position = slot.Position.AdvanceX(new SnesSignedEightEight(unchecked((ushort)ReadSignedSineCosine(tableIndex + 64))).ToSixteenSixteenDelta());
        slot.Position = slot.Position.AdvanceY(new SnesSignedEightEight(unchecked((ushort)ReadSignedSineCosine(tableIndex))).ToSixteenSixteenDelta());
    }

    private static void MoveFlyAccordingToVelocities(RoomEnemySlot slot, FlyEnemyState state)
    {
        slot.Position = slot.Position.Advance(state.Velocity.ToSixteenSixteen());
    }

    internal static short ReadSignedSineCosine(int index)
    {
        // The backing table begins with negative cosine at index zero, sine at index 64,
        // and continues far enough for every byte angle plus the cosine phase offset.
        return EnemyTrigonometryTables.SignedNegativeCosineWord(index);
    }

    private FlyEnemyState RequireFlyState(RoomEnemySlot slot) =>
        _flyStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized fly state.");

    internal void RegisterFliesInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Mellow_Mella_Menu, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeFly(slot);
        });
    }

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
