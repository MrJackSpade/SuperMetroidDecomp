using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Atomic.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>The Atomic species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class AtomicBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Atomic];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new AtomicEntity(slot, common);


    /// <summary>Ports <c>InitAI_Atomic</c> at <c>$A8:E388</c>.</summary>
    internal static void InitializeAtomic(RoomEnemySlot slot)
    {
        var state = new AtomicEnemyState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        state.Program = AnimationProgramCursor<AtomicProgram>.Start(AtomicMovementDefinitions.InitialProgram(checked((ushort)slot.Spawn.Population.Parameter1)));

        // Parameter two is an index into the shared eight-byte linear-speed record:
        // positive whole/fraction followed by its ROM-generated negative whole/fraction.
        // Keeping all four words exactly avoids the one-subpixel errors caused by rebuilding
        // the negative pair from a C# integer after the fact.
        int speedTableOffset = unchecked((ushort)(slot.Spawn.Population.Parameter2 * 8));
        SignedSixteenSixteen positive =
            EnemyMath.ReadLinearEnemySpeed(speedTableOffset);
        SignedSixteenSixteen negative =
            EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));
        state.Speed = positive;
        state.NegativeSpeed = negative;
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
                slot.Position = slot.Position.WithY(AddAtomicVelocity(slot.Position.Y, state.NegativeSpeed));
                break;

            case AtomicVerticalMovement.MoveDown:
                slot.Position = slot.Position.WithY(AddAtomicVelocity(slot.Position.Y, state.Speed));
                break;

            default:
                throw new InvalidDataException(
                    $"Atomic vertical movement {state.VerticalMovement.ToString() ?? "none"} is not translated.");
        }

        switch (state.HorizontalMovement)
        {
            case AtomicHorizontalMovement.MoveLeft:
                slot.Position = slot.Position.WithX(AddAtomicVelocity(slot.Position.X, state.NegativeSpeed));
                break;

            case AtomicHorizontalMovement.MoveRight:
                slot.Position = slot.Position.WithX(AddAtomicVelocity(slot.Position.X, state.Speed));
                break;

            default:
                throw new InvalidDataException(
                    $"Atomic horizontal movement {state.HorizontalMovement.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Reproduces each movement target's unusual ordering: add the signed whole word first,
    /// then add the fractional word and increment the already-updated whole on carry.
    /// </summary>
    private static FixedCoordinate AddAtomicVelocity(
        FixedCoordinate position,
        SignedSixteenSixteen velocity)
    {
        int whole = unchecked((ushort)(position.Whole + velocity.WholeWord));
        uint fractionalSum = (uint)position.Fraction + velocity.Fraction;
        if (fractionalSum > ushort.MaxValue)
            whole = unchecked((ushort)(whole + 1));
        return new(checked((ushort)whole), unchecked((ushort)fractionalSum));
    }

    /// <summary>Advances Atomic's typed animation.</summary>
    internal static void AdvanceAtomicTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, AtomicAnimations.Programs, AtomicFrameDefinitions.ShownFrames.Of,
            static (AtomicEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Atomic's main AI.</summary>
    internal static void RunAtomicMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunAtomicMain(slot, slot.RequireSpeciesState<AtomicEnemyState>(), frame.Samus);
    }
}
