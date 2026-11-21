namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Geruta's flame follower (#2164), which shares the Rio's definition: its placement names it the follower of the
/// parent its room lists right before it, whose freeze, visibility, flames and position it copies.
/// </summary>
/// <param name="species">The Geruta species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed partial class GerutaFollowerEntity(GerutaBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeGerutaFollower(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunGerutaFollowerMainAi(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => GerutaBehavior.AdvanceGerutaFollowerTypedAnimation(Place);
}
