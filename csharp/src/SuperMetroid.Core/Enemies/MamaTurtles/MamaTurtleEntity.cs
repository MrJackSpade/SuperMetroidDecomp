namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One MamaTurtle occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MamaTurtle species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MamaTurtleEntity(MamaTurtleBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.MamaTurtle => MamaTurtleBehavior.RunMamaTurtleInitializationAi,
        EnemyDefinitionId.BabyTurtle => MamaTurtleBehavior.RunBabyTurtleInitializationAi,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.MamaTurtle => species.RunMamaTurtleMainAi,
        EnemyDefinitionId.BabyTurtle => species.RunBabyTurtleMainAi,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.MamaTurtle => species.AdvanceMamaTurtleTypedAnimation,
        EnemyDefinitionId.BabyTurtle => species.AdvanceBabyTurtleTypedAnimation,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.EnemyDefinitionPointer}."),
    };
}
