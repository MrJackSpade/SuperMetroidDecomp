namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>An unpowered work robot (#2146, #2179): it holds the pose its placement names and takes no action of its own.</summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class UnpoweredWorkRobotEntity(WorkRobotBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : WorkRobotEntity(species, place, common)
{
    /// <inheritdoc/>
    public override void Initialize() => Species.InitializeUnpoweredWorkRobot(Place);

    /// <summary>Takes no action of its own.</summary>
    /// <param name="frame">The enemy frame.</param>
    public override void RunMainAi(IEnemyAiFrame frame)
    {
    }

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => WorkRobotBehavior.AdvanceUnpoweredWorkRobotTypedAnimation(Place);

    /// <summary>The robot takes a bomb's damage, never rejecting it, and does nothing more (#2164).</summary>
    protected override EnemyBombReaction OwnBombReaction => Species.UnpoweredWorkRobotBombReaction;

    /// <summary>The robot's own shot reaction, which runs nothing after the common shot reaction.</summary>
    protected override EnemyShotReaction OwnShotReaction => Species.UnpoweredWorkRobotShotReaction;
}
