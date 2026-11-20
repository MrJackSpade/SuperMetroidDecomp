namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// A powered work robot (#2146, #2179): the powered robot's definition placed once Phantoon is defeated and the ship has
/// power. It walks, fires its laser and falls where nothing holds it up, and a shot or bomb that reaches it recoils it.
/// </summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PoweredWorkRobotEntity(WorkRobotBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : WorkRobotEntity(species, occupant, place, common)
{
    /// <summary>The enemy's own bomb reaction, built over its state (#2554).</summary>
    private EnemyBombReaction OwnPoweredWorkRobotBombReaction => field ??= WorkRobotBehavior.PoweredWorkRobotBombReactionOf(State);

    /// <summary>The enemy's own shot reaction, built over its state (#2554).</summary>
    private EnemyShotReaction OwnWorkRobotShotReaction => field ??= WorkRobotBehavior.WorkRobotShotReactionOf(State);

    /// <summary>The enemy's state, which its initialization builds as the enemy is built (#2554).</summary>
    internal WorkRobotEnemyState State { get; } = species.InitializePoweredWorkRobot(place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunWorkRobotMainAi(Place, State, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceWorkRobotTypedAnimation(Place, State, context);

    /// <summary>The robot takes a bomb's damage and recoils (#2164).</summary>
    protected override EnemyBombReaction OwnBombReaction => OwnPoweredWorkRobotBombReaction;

    /// <summary>The robot's own shot reaction, which recoils it after the common shot reaction.</summary>
    protected override EnemyShotReaction OwnShotReaction => OwnWorkRobotShotReaction;
}
