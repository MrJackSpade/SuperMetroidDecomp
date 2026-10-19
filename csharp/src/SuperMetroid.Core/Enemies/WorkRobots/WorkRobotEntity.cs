namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>One WorkRobot occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The WorkRobot species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WorkRobotEntity(WorkRobotBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyShotAdmission, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunWorkRobotInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Robot => species.RunWorkRobotMainAi,
        EnemyDefinitionId.RobotNoPower => WorkRobotBehavior.RunUnpoweredWorkRobotMainAi,
        _ => throw new InvalidOperationException($"WorkRobot entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceWorkRobotTypedAnimation;

    /// <summary>The powered robot ($E8FF) returns before common shot AI until Phantoon's area-boss bit is set.</summary>
    public bool AdmitsShots() =>
        Slot.EnemyDefinitionPointer != EnemyDefinitionId.Robot || species.IsAreaBossDefeated;

    /// <summary>The powered and unpowered robots' shot callbacks, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback is EnemyInteractionCallback.WorkRobotShot or EnemyInteractionCallback.WorkRobotNoPowerShot;
}
