namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>An unpowered work robot (#2146, #2179): it holds the pose its placement names and takes no action of its own.</summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="settings">What the enemy's population entry tells its species.</param>
internal sealed partial class UnpoweredWorkRobotEntity(WorkRobotBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, WorkRobotSpawnSettings settings)
    : WorkRobotEntity(species, occupant, place, common)
{
    /// <summary>What the enemy's population entry tells its species, which its factory checked as it built the enemy.</summary>
    internal WorkRobotSpawnSettings Settings { get; } = settings;

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly UnpoweredWorkRobotEnemyState state = species.InitializeUnpoweredWorkRobot(settings, place);

    /// <summary>Takes no action of its own.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => WorkRobotBehavior.AdvanceUnpoweredWorkRobotTypedAnimation(Place, state);

    /// <summary>The robot takes a bomb's damage, never rejecting it, and does nothing more (#2164).</summary>
    protected override EnemyBombReaction OwnBombReaction => Species.UnpoweredWorkRobotBombReaction;

    /// <summary>The robot's own shot reaction, which runs nothing after the common shot reaction.</summary>
    protected override EnemyShotReaction OwnShotReaction => Species.UnpoweredWorkRobotShotReaction;
}
