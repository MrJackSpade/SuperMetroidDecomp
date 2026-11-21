using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One of the Tourian entrance statue scene's three placements (#2014): the first spawns the scene's decorations as it
/// initializes, and each leaves the room at its first animation step, on the first update the enemy pass processes it.
/// </summary>
/// <remarks>
/// Until it leaves, a placement is an invisible enemy at its place that Samus's grapple beam, exploding bombs and contact
/// can meet. Entering the room from below, the two placements that spawn nothing stay until the camera first reaches
/// them, so they are kept although they show and do nothing else.
/// </remarks>
/// <param name="species">The Tourian entrance statue species owner, which holds the statue room.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class TourianEntranceStatueEntity(TourianEntranceStatueBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyTypedAnimation, IEnemyDeparture
{
    /// <summary>The place the placement holds until it leaves, which shows the scene's look.</summary>
    internal RoomEnemyPlace OccupiedPlace => Place;

    /// <inheritdoc/>
    public void Initialize() => species.InitializeTourianEntranceStatue(this);

    /// <inheritdoc/>
    /// <remarks>The scene shows no later look on a place its placement has left (#2356).</remarks>
    public void Leave() => species.Leave(this);

    /// <inheritdoc/>
    /// <remarks>The placement leaves the room: its first animation step is its last.</remarks>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Place.Presence = Place.Presence.With(EnemyPresence.Deleted);
}
