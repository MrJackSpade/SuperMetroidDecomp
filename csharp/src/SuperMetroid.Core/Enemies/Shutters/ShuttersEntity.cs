namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>One Shutters occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShuttersEntity(ShuttersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => ShuttersPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        ShuttersPart.ShutterGrowing => ShuttersBehavior.InitializeGrowingShutter,
        ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer => ShuttersBehavior.InitializeVerticalShutter,
        ShuttersPart.ShutterHorizShootable => ShuttersBehavior.InitializeHorizontalShutter,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => ShuttersPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        ShuttersPart.ShutterGrowing => species.RunShutterGrowingMainAi,
        ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer => species.RunShutterShootableMainAi,
        ShuttersPart.ShutterHorizShootable => species.RunShutterHorizShootableMainAi,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => ShuttersPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        ShuttersPart.ShutterGrowing => ShuttersBehavior.AdvanceShutterGrowingTypedAnimation,
        ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer => ShuttersBehavior.AdvanceShutterShootableTypedAnimation,
        ShuttersPart.ShutterHorizShootable => ShuttersBehavior.AdvanceShutterHorizShootableTypedAnimation,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.DefinitionId}."),
    };

    /// <summary>The shootable and destroyable vertical shutters' shot reactions, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer && reaction == EnemyReaction.ShootableVerticalShutterShot ||
        Slot.DefinitionId == EnemyDefinitionId.ShutterDestroyable && reaction == EnemyReaction.DestroyableVerticalShutterShot;
}
