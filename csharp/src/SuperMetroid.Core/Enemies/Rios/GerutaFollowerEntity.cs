namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Geruta's flame follower (#2164), which shares the Rio's definition: its placement names it the follower of the
/// parent its room lists right before it, whose freeze, visibility, flames and position it copies.
/// </summary>
/// <param name="species">The Geruta species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="parent">The parent the room lists right before the follower, which offered it its creation (#2554).</param>
internal sealed partial class GerutaFollowerEntity(GerutaBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    GerutaEntity parent)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly GerutaFollowerState state = species.InitializeGerutaFollower(place, parent);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunGerutaFollowerMainAi(Place, state);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GerutaBehavior.AdvanceGerutaFollowerTypedAnimation(Place, state);
}
