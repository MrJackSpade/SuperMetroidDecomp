namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// A powered work robot (#2146, #2179): the powered robot's definition placed once Phantoon is defeated and the ship has
/// power. It walks, fires its laser and falls where nothing holds it up, and a shot or bomb that reaches it recoils it.
/// </summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="state">What the enemy's setup built (#2554).</param>
internal sealed partial class PoweredWorkRobotEntity(WorkRobotBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place,
    EnemyCommonParticipation common, WorkRobotEnemyState state)
    : WorkRobotEntity(species, occupant, place, common), IEnemyDeparture
{
    /// <summary>
    /// Builds the enemy, running its setup as the game builds it (#2554). A restore of the game's state builds it through the
    /// primary constructor from what it holds, so the setup never runs again.
    /// </summary>
    /// <param name="species">The work robot species owner.</param>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
    /// <param name="place">The place the robot occupies.</param>
    /// <param name="common">The shared participation.</param>
    internal PoweredWorkRobotEntity(WorkRobotBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
        : this(species, occupant, place, common, species.InitializePoweredWorkRobot(place))
    {
    }

    /// <summary>The enemy's state, which its setup builds as the enemy is built (#2554).</summary>
    private readonly WorkRobotEnemyState state = state;

    /// <summary>Recoils the robot after a shot's common damage.</summary>
    /// <param name="context">The shot's hit.</param>
    internal void Recoil(EnemyShotHitContext context) =>
        WorkRobotBehavior.ResolveWorkRobotShotAfterCommon(Ensure.NotNull(context).Enemy, state, context.Samus);

    /// <summary>Recoils the robot after a bomb's common damage.</summary>
    /// <param name="context">The bomb's hit.</param>
    internal void Recoil(EnemyBombHitContext context) =>
        WorkRobotBehavior.ResolveWorkRobotShotAfterCommon(Ensure.NotNull(context).Enemy, state, context.Samus);

    /// <summary>The robot leaves its place, so the glow no longer shows there (#2559).</summary>
    public void Leave() => Species.RemovePoweredRobot(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunWorkRobotMainAi(Place, state, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceWorkRobotTypedAnimation(Place, state, context);

    /// <summary>The robot takes a bomb's damage and recoils (#2164).</summary>
    protected override EnemyBombReaction OwnBombReaction => field ??= WorkRobotBehavior.PoweredWorkRobotBombReaction(Recoil);

    /// <summary>The robot's own shot reaction, which recoils it after the common shot reaction.</summary>
    protected override EnemyShotReaction OwnShotReaction => field ??= WorkRobotBehavior.WorkRobotShotReaction(Recoil);
}
