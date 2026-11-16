namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// A work robot, powered (<see cref="PoweredWorkRobotEntity"/>) or unpowered (<see cref="UnpoweredWorkRobotEntity"/>):
/// both set up and animate as work robots do and run their own touch, shot and bomb reactions where their definitions
/// leave them to them.
/// </summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract class WorkRobotEntity(WorkRobotBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The work robot species owner.</summary>
    protected WorkRobotBehavior Species => species;

    /// <inheritdoc/>
    public void Initialize() => species.InitializeWorkRobot(Place);

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceWorkRobotTypedAnimation(Place, context);

    /// <summary>Whether the Work Robot's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => LeavesTouchToSpecies || base.ReactsToTouch;

    /// <summary>Runs the Work Robot's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (LeavesTouchToSpecies)
            WorkRobotBehavior.ReactToWorkRobotTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Work Robot's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => LeavesShotToSpecies ? species.WorkRobotShotReaction : base.ShotReaction;

    /// <summary>The Work Robot's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot is null ? OwnBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The robot's own stages around a bomb's common damage: the powered robot's rejection, or the unpowered robot's recoil alone.</summary>
    protected abstract EnemyBombReaction OwnBombReaction { get; }

    /// <summary>The Work Robot's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
