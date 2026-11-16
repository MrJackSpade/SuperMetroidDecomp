namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>An unpowered work robot (#2146): it holds its pose and takes no action of its own.</summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class UnpoweredWorkRobotEntity(WorkRobotBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : WorkRobotEntity(species, place, common)
{
    /// <summary>Takes no action of its own.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }
}
