namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Norfair Rio's flame follower (#2164), which shares the Rio's definition: its placement names it the follower of the
/// parent its room lists right before it, whose freeze, visibility, flames and position it copies.
/// </summary>
/// <param name="species">The Norfair Rio species owner.</param>
/// <param name="place">The place the follower occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class NorfairRioFollowerEntity(NorfairRioBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeNorfairRioFollower(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunNorfairRioFollowerMainAi(Place);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => NorfairRioBehavior.AdvanceNorfairRioFollowerTypedAnimation(Place);
}
