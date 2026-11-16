namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// A powered work robot (#2146): it walks and fires its laser, and returns before common shot AI until Phantoon is
/// defeated.
/// </summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PoweredWorkRobotEntity(WorkRobotBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : WorkRobotEntity(species, place, common), IEnemyShotAdmission
{
    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunWorkRobotMainAi(Place, frame);

    /// <summary>The powered robot returns before common shot AI until Phantoon is defeated.</summary>
    public bool AdmitsShots() => Species.IsAreaBossDefeated;
}
