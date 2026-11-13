using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>The Atomic species owner: its per-room state, behaviour and the interactions it registers.</summary>
internal sealed class AtomicBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Atomic];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new AtomicEntity(slot, common);


    /// <summary>Initializes the Atomic.</summary>
    internal static void InitializeAtomic(RoomEnemySlot slot)
    {
        var state = new AtomicEnemyState();
        slot.SetSpeciesState(state);

        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);

        AtomicSpawnSettings settings = slot.SpawnSettings<AtomicSpawnSettings>();
        state.Playback = new(AtomicAnimations.StartingWith(settings.Spin), AnimationPlace.StartOfAnimation);

        // The atomic moves at its speed toward increasing coordinates and at the same speed back.
        state.Speed = settings.Speed;
        state.NegativeSpeed = SignedSixteenSixteen.Zero - settings.Speed;
    }

    /// <summary>
    /// Runs the Atomic's main AI, including all four indirect targets.
    /// Atomic deliberately ignores terrain and chooses both signs again every frame.
    /// </summary>
    private static void RunAtomicMain(
        RoomEnemySlot slot,
        AtomicEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Atomic movement requires the active Samus actor.");

        // The game treats equality as the positive direction: down for Y and right for
        // X. Compare the wrapped coordinate difference by its sign, as the game does, rather than with a plain
        // integer comparison that would disagree across the room coordinates' wrap.
        state.VerticalMovement = CoordinateAxis.Difference(samus.Position.Y.Whole, slot.Position.Y.Whole) < 0
            ? AtomicVerticalMovement.MoveUp
            : AtomicVerticalMovement.MoveDown;
        state.HorizontalMovement = CoordinateAxis.Difference(samus.Position.X.Whole, slot.Position.X.Whole) < 0
            ? AtomicHorizontalMovement.MoveLeft
            : AtomicHorizontalMovement.MoveRight;

        switch (state.VerticalMovement)
        {
            case AtomicVerticalMovement.MoveUp:
                slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(state.NegativeSpeed));
                break;

            case AtomicVerticalMovement.MoveDown:
                slot.Position = slot.Position.WithY(slot.Position.Y.MovedBy(state.Speed));
                break;

            default:
                throw new InvalidDataException(
                    $"Atomic vertical movement {state.VerticalMovement.ToString() ?? "none"} is not translated.");
        }

        switch (state.HorizontalMovement)
        {
            case AtomicHorizontalMovement.MoveLeft:
                slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(state.NegativeSpeed));
                break;

            case AtomicHorizontalMovement.MoveRight:
                slot.Position = slot.Position.WithX(slot.Position.X.MovedBy(state.Speed));
                break;

            default:
                throw new InvalidDataException(
                    $"Atomic horizontal movement {state.HorizontalMovement.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Advances Atomic's typed animation.</summary>
    internal static void AdvanceAtomicTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, AtomicAnimations.Segments,
            static (AtomicEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Atomic animation raised {cue}."));

    /// <summary>Runs Atomic's main AI.</summary>
    internal static void RunAtomicMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunAtomicMain(slot, slot.RequireSpeciesState<AtomicEnemyState>(), frame.Samus);
    }
}
