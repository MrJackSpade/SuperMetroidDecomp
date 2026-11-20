namespace SuperMetroid.Core.Enemies.Rios;

/// <summary>
/// A Holtz parent (#2164): it waits on its perch and dives at Samus, its animation's cues showing and hiding
/// the flames of the follower its room lists right after it (<see cref="HoltzFollowerEntity"/>).
/// </summary>
/// <param name="species">The Holtz species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class HoltzEntity(HoltzBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyAi, IEnemyTypedAnimation
{
    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal HoltzEnemyState State { get; } = HoltzBehavior.InitializeHoltz(place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunHoltzMainAi(Place, State, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => HoltzBehavior.AdvanceHoltzTypedAnimation(Place, context);
}
