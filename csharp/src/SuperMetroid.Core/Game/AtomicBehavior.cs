namespace SuperMetroid.Core.Game;

/// <summary>The Atomic species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class AtomicBehavior : IEnemySpecies
{

    internal readonly AtomicEnemyState?[] _atomicStates =
        new AtomicEnemyState?[RoomEnemySystem.MaximumEnemyCount];
    internal readonly ushort[] _atomicSpeedFractions = new ushort[RoomEnemySystem.MaximumEnemyCount];
    internal readonly ushort[] _atomicSpeedWholes = new ushort[RoomEnemySystem.MaximumEnemyCount];
    internal readonly ushort[] _atomicNegativeSpeedFractions = new ushort[RoomEnemySystem.MaximumEnemyCount];
    internal readonly ushort[] _atomicNegativeSpeedWholes = new ushort[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Atomic</c> at <c>$A8:E388</c>.</summary>
    internal void InitializeAtomic(RoomEnemySlot slot)
    {
        var state = new AtomicEnemyState(
            slot,
            _atomicSpeedFractions,
            _atomicSpeedWholes,
            _atomicNegativeSpeedFractions,
            _atomicNegativeSpeedWholes);
        _atomicStates[slot.SlotIndex] = state;

        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);

        slot.CurrentInstruction = AtomicMovementDefinitions.InitialInstructionList(slot.Parameter1);

        // Parameter two is an index into the shared eight-byte linear-speed record:
        // positive whole/fraction followed by its ROM-generated negative whole/fraction.
        // Keeping all four words exactly avoids the one-subpixel errors caused by rebuilding
        // the negative pair from a C# integer after the fact.
        ushort speedTableOffset = unchecked((ushort)(slot.Parameter2 * 8));
        (short positiveWhole, ushort positiveFraction) =
            EnemyMath.ReadLinearEnemySpeed(speedTableOffset);
        (short negativeWhole, ushort negativeFraction) =
            EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));
        state.SpeedWhole = unchecked((ushort)positiveWhole);
        state.SpeedFraction = positiveFraction;
        state.NegativeSpeedWhole = unchecked((ushort)negativeWhole);
        state.NegativeSpeedFraction = negativeFraction;
    }

    /// <summary>
    /// Ports <c>MainAI_Atomic</c> at <c>$A8:E3C3</c>, including all four indirect targets.
    /// Atomic deliberately ignores terrain and chooses both signs again every frame.
    /// </summary>
    private static void RunAtomicMain(
        RoomEnemySlot slot,
        AtomicEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Atomic movement requires the active Samus actor.");

        // The cartridge treats equality as the positive direction: down for Y and right for
        // X. Compare the wrapped 16-bit subtraction as signed, matching BMI rather than a host
        // integer comparison that would disagree across the $0000/$FFFF coordinate boundary.
        state.VerticalMovement = unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)) < 0
            ? AtomicVerticalMovement.MoveUp
            : AtomicVerticalMovement.MoveDown;
        state.HorizontalMovement = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) < 0
            ? AtomicHorizontalMovement.MoveLeft
            : AtomicHorizontalMovement.MoveRight;

        switch (state.VerticalMovement)
        {
            case AtomicVerticalMovement.MoveUp:
                slot.Position = slot.Position.WithY(AddAtomicVelocity(
                    slot.Position.Y,
                    state.NegativeSpeedWhole,
                    state.NegativeSpeedFraction));
                break;

            case AtomicVerticalMovement.MoveDown:
                slot.Position = slot.Position.WithY(AddAtomicVelocity(
                    slot.Position.Y,
                    state.SpeedWhole,
                    state.SpeedFraction));
                break;

            default:
                throw new InvalidDataException(
                    $"Atomic vertical function $A8:{(ushort)state.VerticalMovement:X4} is not translated.");
        }

        switch (state.HorizontalMovement)
        {
            case AtomicHorizontalMovement.MoveLeft:
                slot.Position = slot.Position.WithX(AddAtomicVelocity(
                    slot.Position.X,
                    state.NegativeSpeedWhole,
                    state.NegativeSpeedFraction));
                break;

            case AtomicHorizontalMovement.MoveRight:
                slot.Position = slot.Position.WithX(AddAtomicVelocity(
                    slot.Position.X,
                    state.SpeedWhole,
                    state.SpeedFraction));
                break;

            default:
                throw new InvalidDataException(
                    $"Atomic horizontal function $A8:{(ushort)state.HorizontalMovement:X4} is not translated.");
        }
    }

    /// <summary>
    /// Reproduces each movement target's unusual ordering: add the signed whole word first,
    /// then add the fractional word and increment the already-updated whole on carry.
    /// </summary>
    private static SnesFixedPosition AddAtomicVelocity(
        SnesFixedPosition position,
        ushort wholeVelocity,
        ushort fractionalVelocity)
    {
        ushort whole = unchecked((ushort)(position.Whole + wholeVelocity));
        uint fractionalSum = (uint)position.Fraction + fractionalVelocity;
        if (fractionalSum > ushort.MaxValue)
            whole = unchecked((ushort)(whole + 1));
        return new(whole, unchecked((ushort)fractionalSum));
    }

    private AtomicEnemyState RequireAtomicState(RoomEnemySlot slot) =>
        _atomicStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Atomic state.");

    internal void RegisterAtomicInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Atomic, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Atomic, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeAtomic(slot);
        });
    }

    internal void RegisterAtomicMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Atomic, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Atomic, (slot, frame, nmiFrameCounter8) =>
        {
            RunAtomicMain(slot, RequireAtomicState(slot), frame.Samus);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_atomicStates);
        Array.Clear(_atomicSpeedFractions);
        Array.Clear(_atomicSpeedWholes);
        Array.Clear(_atomicNegativeSpeedFractions);
        Array.Clear(_atomicNegativeSpeedWholes);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
