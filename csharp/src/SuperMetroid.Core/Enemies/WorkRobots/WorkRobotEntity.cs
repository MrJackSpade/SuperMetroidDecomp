namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One WorkRobot occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The WorkRobot species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WorkRobotEntity(WorkRobotBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyShotAdmission, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeWorkRobot;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => WorkRobotPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        WorkRobotPart.Robot => species.RunWorkRobotMainAi,
        // The unpowered robot's main does nothing.
        WorkRobotPart.RobotNoPower => EnemyNoOpAi.Main,
        _ => throw new InvalidOperationException($"WorkRobot entity bound to {Slot.DefinitionId}."),
    };

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
