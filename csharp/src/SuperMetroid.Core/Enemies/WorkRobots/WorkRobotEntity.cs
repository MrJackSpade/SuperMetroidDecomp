namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// A work robot, powered (<see cref="PoweredWorkRobotEntity"/>) or unpowered (<see cref="UnpoweredWorkRobotEntity"/>),
/// as its definition names it (#2179, #2455). Each runs its own touch, shot and bomb reactions where its definition leaves
/// them to it.
/// </summary>
/// <param name="species">The work robot species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the robot occupies.</param>
/// <param name="common">The shared participation.</param>
internal abstract partial class WorkRobotEntity(WorkRobotBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(occupant, place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <summary>The work robot species owner.</summary>
    protected WorkRobotBehavior Species => species;

    /// <inheritdoc/>
    public abstract void Initialize();

    /// <inheritdoc/>
    public abstract void RunMainAi(IEnemyAiFrame frame);

    /// <inheritdoc/>
    public abstract void AdvanceTypedAnimation(EnemyAnimationCueContext context);

    /// <summary>Whether the Work Robot's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => Occupant.Definition.Touch == WorkRobotReactions.Touch || base.ReactsToTouch;

    /// <summary>Runs the Work Robot's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Occupant.Definition.Touch == WorkRobotReactions.Touch)
            WorkRobotBehavior.ReactToWorkRobotTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The Work Robot's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == WorkRobotReactions.Shot ? OwnShotReaction : base.ShotReaction;

    /// <summary>The Work Robot's own stages around common bomb damage for a bomb that selects its own shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction CommonBombDamageReaction(EnemyBombContext context) =>
        context.SelectedShot == WorkRobotReactions.Shot ? OwnBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The robot's own stages around a bomb's common damage, as its power decides them.</summary>
    protected abstract EnemyBombReaction OwnBombReaction { get; }

    /// <summary>The robot's own shot reaction, as its power decides it.</summary>
    protected abstract EnemyShotReaction OwnShotReaction { get; }

    /// <summary>The Work Robot's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects: the species' own where its definition names it.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == WorkRobotReactions.Shot;
}
