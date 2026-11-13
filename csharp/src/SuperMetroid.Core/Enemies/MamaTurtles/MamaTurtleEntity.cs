namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>One MamaTurtle occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MamaTurtle species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class MamaTurtleEntity(MamaTurtleBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => MamaTurtlePartDefinitions.Parts[Slot.DefinitionId] switch
    {
        MamaTurtlePart.MamaTurtle => species.InitializeMamaTurtle,
        MamaTurtlePart.BabyTurtle => MamaTurtleBehavior.InitializeBabyTurtle,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => MamaTurtlePartDefinitions.Parts[Slot.DefinitionId] switch
    {
        MamaTurtlePart.MamaTurtle => species.RunMamaTurtleMainAi,
        MamaTurtlePart.BabyTurtle => species.RunBabyTurtleMainAi,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => MamaTurtlePartDefinitions.Parts[Slot.DefinitionId] switch
    {
        MamaTurtlePart.MamaTurtle => species.AdvanceMamaTurtleTypedAnimation,
        MamaTurtlePart.BabyTurtle => species.AdvanceBabyTurtleTypedAnimation,
        _ => throw new InvalidOperationException($"MamaTurtle entity bound to {Slot.DefinitionId}."),
    };

    /// <summary>The baby turtle's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId == EnemyDefinitionId.BabyTurtle && reaction == EnemyReaction.BabyTurtleShot;
}
