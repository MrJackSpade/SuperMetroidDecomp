using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>The Atomic species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class AtomicBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Atomic], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new AtomicEntity(place, common));

    /// <summary>Initializes the Atomic.</summary>
    internal static void InitializeAtomic(RoomEnemyPlace place)
    {
        var state = new AtomicEnemyState();
        place.SetSpeciesState(state);

        place.AnimationClock.Restart();

        AtomicSpawnSettings settings = place.SpawnSettings<AtomicSpawnSettings>();
        state.Playback = new(AtomicAnimations.StartingWith(settings.Spin), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Moves the Atomic one step toward Samus on each axis at its spawn settings' speed. It ignores terrain and chooses both
    /// directions again every update.
    /// </summary>
    internal static void MoveAtomicTowardSamus(
        RoomEnemyPlace place,
        SamusState samus)
    {
        // The Atomic moves towards Samus on each axis; being level with her counts as her being below or to the right, so
        // it moves down or right.
        SignedSixteenSixteen speed = place.SpawnSettings<AtomicSpawnSettings>().Speed;
        SignedSixteenSixteen vertical = speed.Toward(samus.Position.Y.Whole < place.Position.Y.Whole ? VerticalDirection.Up : VerticalDirection.Down);
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(vertical));

        SignedSixteenSixteen horizontal = speed.Toward(samus.Position.X.Whole < place.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right);
        place.Position = place.Position.WithX(place.Position.X.MovedBy(horizontal));
    }

    /// <summary>Advances Atomic's typed animation.</summary>
    internal static void AdvanceAtomicTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.AdvanceSilently(place, AtomicAnimations.Segments,
            static (AtomicEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);
}
