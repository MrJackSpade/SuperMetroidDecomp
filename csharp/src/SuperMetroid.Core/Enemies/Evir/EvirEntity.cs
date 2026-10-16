namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One Evir occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EvirEntity(EvirBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Evir => species.RunEvirInitializationAi,
        EnemyDefinitionId.EvirProjectile => species.RunEvirProjectileInitializationAi,
        _ => throw new InvalidOperationException($"Evir entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Evir => species.RunEvirMainAi,
        EnemyDefinitionId.EvirProjectile => species.RunEvirProjectileMainAi,
        _ => throw new InvalidOperationException($"Evir entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceEvirTypedAnimation;
}
