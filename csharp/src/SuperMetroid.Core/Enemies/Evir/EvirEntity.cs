namespace SuperMetroid.Core.Enemies.Evir;

/// <summary>One Evir occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Evir species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EvirEntity(EvirBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemySurroundingsInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemySurroundingsInitializationAiHandler? FindSurroundingsInitialization() => EvirPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        EvirPart.Evir => species.RunEvirInitializationAi,
        EvirPart.Projectile => (projectile, _) => species.InitializeEvirProjectile(projectile),
        _ => throw new InvalidOperationException($"Evir entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => EvirPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        EvirPart.Evir => species.RunEvirMainAi,
        EvirPart.Projectile => species.RunEvirProjectileMainAi,
        _ => throw new InvalidOperationException($"Evir entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceEvirTypedAnimation;

    /// <summary>The Evir body's own shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId == EnemyDefinitionId.Evir && reaction == EnemyReaction.EvirShot;
}
