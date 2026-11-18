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
/// <param name="species">The Tourian entrance statue species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="statueRoom">The statue room, whose statue the scene's decorations follow down (#2382).</param>
internal sealed class TourianEntranceStatueEntity(TourianEntranceStatueBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common,
    ITourianStatueRoom statueRoom)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeTourianEntranceStatue(Place, statueRoom);

    /// <inheritdoc/>
    /// <remarks>A placement has no behaviour of its own before it leaves.</remarks>
    public void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    /// <remarks>The placement leaves the room: its first animation step is its last.</remarks>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Place.Presence = Place.Presence.With(EnemyPresence.Deleted);
}
