namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Lower Norfair Rio's flame follower (#2164), which shares the Rio's definition: its placement names it the follower
/// of the parent its room lists right before it, whose freeze, visibility and position it copies.
/// </summary>
/// <param name="species">The Lower Norfair Rio species owner.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class LowerNorfairRioFollowerEntity(LowerNorfairRioBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeLowerNorfairRioFollower(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunLowerNorfairRioFollowerMainAi(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => LowerNorfairRioBehavior.AdvanceLowerNorfairRioFollowerTypedAnimation(Place);
}
