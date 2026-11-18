using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>The Atomic species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class AtomicBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Atomic];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new AtomicEntity(place, common);


    /// <summary>Initializes the Atomic.</summary>
    internal static void InitializeAtomic(RoomEnemyPlace place)
    {
        var state = new AtomicEnemyState();
        place.SetSpeciesState(state);

        place.AnimationClock.Restart();

        AtomicSpawnSettings settings = place.SpawnSettings<AtomicSpawnSettings>();
        state.Playback = new(AtomicAnimations.StartingWith(settings.Spin), AnimationPlace.StartOfAnimation);

        state.Speed = settings.Speed;
    }

    /// <summary>
    /// Moves the Atomic one step toward Samus on each axis. It ignores terrain and chooses both directions again every
    /// update.
    /// </summary>
    private static void RunAtomicMain(
        RoomEnemyPlace place,
        AtomicEnemyState state,
        SamusState samus)
    {
        // The Atomic moves towards Samus on each axis; being level with her counts as her being below or to the right, so
        // it moves down or right.
        SignedSixteenSixteen vertical = samus.Position.Y.Whole < place.Position.Y.Whole
            ? -state.Speed
            : state.Speed;
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(vertical));

        SignedSixteenSixteen horizontal = samus.Position.X.Whole < place.Position.X.Whole
            ? -state.Speed
            : state.Speed;
        place.Position = place.Position.WithX(place.Position.X.MovedBy(horizontal));
    }

    /// <summary>Advances Atomic's typed animation.</summary>
    internal static void AdvanceAtomicTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, AtomicAnimations.Segments,
            static (AtomicEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            static (_, cue) => throw new InvalidOperationException($"A cue-free Atomic animation raised {cue}."));

    /// <summary>Runs Atomic's main AI.</summary>
    internal static void RunAtomicMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunAtomicMain(place, place.RequireSpeciesState<AtomicEnemyState>(), frame.Samus);
    }
}
