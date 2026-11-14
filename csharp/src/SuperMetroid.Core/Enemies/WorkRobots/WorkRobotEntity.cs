namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One WorkRobot occupant: its own AI handlers, with the shared reactions.</summary>
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
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceWorkRobotTypedAnimation;

    /// <summary>The powered robot returns before common shot AI until Phantoon is defeated.</summary>
    public bool AdmitsShots() =>
        Slot.DefinitionId != EnemyDefinitionId.Robot || species.IsAreaBossDefeated;

    /// <summary>The powered and unpowered robots' shot reactions, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction is EnemyReaction.WorkRobotShot or EnemyReaction.WorkRobotNoPowerShot;
}
