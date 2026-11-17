namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// A powered robot's definition placed while the ship has no power, before Phantoon is defeated (#2179): it holds the
/// unpowered pose its placement names and falls under gravity, rejects a bomb's damage, and returns before common shot
/// AI. No room places it: the rooms place the powered definition only once Phantoon is defeated.
/// </summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PowerlessWorkRobotEntity(WorkRobotBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : WorkRobotEntity(species, place, common), IEnemyShotAdmission
{
    /// <inheritdoc/>
    public override void Initialize() => Species.InitializeUnpoweredWorkRobot(Place);

    /// <summary>Falls under gravity while nothing holds the robot up.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunPowerlessWorkRobotMainAi(Place, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => WorkRobotBehavior.AdvanceUnpoweredWorkRobotTypedAnimation(Place);

    /// <summary>The robot rejects a bomb's damage that selects its own shot reaction, and takes no recoil.</summary>
    protected override EnemyBombReaction OwnBombReaction => Species.PowerlessWorkRobotBombReaction;

    /// <summary>The robot's own shot reaction, which runs nothing after the common shot AI.</summary>
    protected override EnemyShotReaction OwnShotReaction => Species.UnpoweredWorkRobotShotReaction;

    /// <summary>Shots return before common shot AI: the ship has no power.</summary>
    public bool AdmitsShots() => false;
}
