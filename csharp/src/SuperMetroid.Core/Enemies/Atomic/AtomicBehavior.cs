using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Atomic;

/// <summary>The Atomic species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class AtomicBehavior : IEnemySpecies, IEnemySpawnFactory<AtomicSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Atomic];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, AtomicSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new AtomicEntity(occupant, place, common, settings));

    /// <summary>Initializes the Atomic.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static AtomicEnemyState InitializeAtomic(AtomicSpawnSettings settings, RoomEnemyPlace place)
    {
        var state = new AtomicEnemyState();

        place.AnimationClock.Restart();
        state.Playback = new(AtomicAnimations.StartingWith(settings.Spin), AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>
    /// Moves the Atomic one step toward Samus on each axis at its spawn settings' speed. It ignores terrain and chooses both
    /// directions again every update.
    /// </summary>
    internal static void MoveAtomicTowardSamus(
        AtomicSpawnSettings atomicSpawnSettings, RoomEnemyPlace place,
        SamusState samus)
    {
        // The Atomic moves towards Samus on each axis; being level with her counts as her being below or to the right, so
        // it moves down or right.
        SignedSixteenSixteen speed = atomicSpawnSettings.Speed;
        SignedSixteenSixteen vertical = speed.Toward(samus.Position.Y.Whole < place.Position.Y.Whole ? VerticalDirection.Up : VerticalDirection.Down);
        place.Position = place.Position.WithY(place.Position.Y.MovedBy(vertical));

        SignedSixteenSixteen horizontal = speed.Toward(samus.Position.X.Whole < place.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right);
        place.Position = place.Position.WithX(place.Position.X.MovedBy(horizontal));
    }

    /// <summary>Advances Atomic's typed animation.</summary>
    internal static void AdvanceAtomicTypedAnimation(RoomEnemyPlace place, AtomicEnemyState atomicEnemyState) =>
        EnemyAnimationPlayback.AdvanceSilently(place, atomicEnemyState, AtomicAnimations.Segments,
            static (AtomicEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback);
}
