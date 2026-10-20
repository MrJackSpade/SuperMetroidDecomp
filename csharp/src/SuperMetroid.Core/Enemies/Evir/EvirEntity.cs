namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One Evir occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EvirEntity(EvirBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemySurroundingsInitializationAiHandler? FindSurroundingsInitialization() => EvirPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        EvirPart.Evir => species.RunEvirInitializationAi,
        EvirPart.Projectile => (projectile, _) => species.InitializeEvirProjectile(projectile),
        _ => throw new InvalidOperationException($"Evir entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => EvirPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        EvirPart.Evir => species.RunEvirMainAi,
        EvirPart.Projectile => species.RunEvirProjectileMainAi,
        _ => throw new InvalidOperationException($"Evir entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceEvirTypedAnimation;

    /// <summary>The Evir body's own shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.Evir && callback == EnemyInteractionCallback.EvirShot;
}
