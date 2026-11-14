namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One WorkRobot occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The WorkRobot species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WorkRobotEntity(WorkRobotBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyShotAdmission, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeWorkRobot(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (WorkRobotPartDefinitions.Parts[Slot.DefinitionId])
        {
            case WorkRobotPart.Robot:
                species.RunWorkRobotMainAi(Slot, frame);
                break;
            // The unpowered robot's main does nothing.
            case WorkRobotPart.RobotNoPower:
                break;
            default:
                throw new InvalidOperationException($"WorkRobot entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceWorkRobotTypedAnimation(Slot, context);

    /// <summary>The powered robot returns before common shot AI until Phantoon is defeated.</summary>
    public bool AdmitsShots() =>
        Slot.DefinitionId != EnemyDefinitionId.Robot || species.IsAreaBossDefeated;

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
        context.SelectedShot is null ? species.WorkRobotBombReaction : base.CommonBombDamageReaction(context);

    /// <summary>The Work Robot's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
