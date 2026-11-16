namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One WorkRobot occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The WorkRobot species owner.</param>
/// <param name="place">The place the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WorkRobotEntity(WorkRobotBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyShotAdmission, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeWorkRobot(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (WorkRobotPartDefinitions.Parts[Place.DefinitionId])
        {
            case WorkRobotPart.Powered:
                species.RunWorkRobotMainAi(Place, frame);
                break;
            // The unpowered robot's main does nothing.
            case WorkRobotPart.Unpowered:
                break;
            default:
                throw new InvalidOperationException($"WorkRobot entity bound to {Place.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceWorkRobotTypedAnimation(Place, context);

    /// <summary>The powered robot returns before common shot AI until Phantoon is defeated.</summary>
    public bool AdmitsShots() =>
        Place.DefinitionId != EnemyDefinitionId.WorkRobot || species.IsAreaBossDefeated;

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

    /// <summary>The Work Robot's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the radius path or a hitbox selects; none where the definition leaves shots to the species.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) => reaction is null && LeavesShotToSpecies;
}
