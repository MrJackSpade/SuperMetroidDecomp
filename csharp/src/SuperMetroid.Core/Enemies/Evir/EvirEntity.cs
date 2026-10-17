namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One Evir occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EvirEntity(EvirBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
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

    /// <summary>The Evir body's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    /// <param name="callbackPointer">The selected callback pointer.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback, ushort callbackPointer) =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.Evir && callback == EnemyInteractionCallback.EvirShot;
}
