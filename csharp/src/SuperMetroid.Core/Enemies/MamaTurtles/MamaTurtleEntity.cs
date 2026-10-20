namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One MamaTurtle occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MamaTurtle species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MamaTurtleEntity(MamaTurtleBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MamaTurtlePartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        MamaTurtlePart.MamaTurtle => MamaTurtleBehavior.InitializeMamaTurtle,
        MamaTurtlePart.BabyTurtle => MamaTurtleBehavior.InitializeBabyTurtle,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => MamaTurtlePartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        MamaTurtlePart.MamaTurtle => species.RunMamaTurtleMainAi,
        MamaTurtlePart.BabyTurtle => species.RunBabyTurtleMainAi,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => MamaTurtlePartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        MamaTurtlePart.MamaTurtle => species.AdvanceMamaTurtleTypedAnimation,
        MamaTurtlePart.BabyTurtle => species.AdvanceBabyTurtleTypedAnimation,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <summary>The baby turtle's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle && callback == EnemyInteractionCallback.BabyTurtleShot;
}
