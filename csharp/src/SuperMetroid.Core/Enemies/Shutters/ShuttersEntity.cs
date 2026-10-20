namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>One Shutters occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShuttersEntity(ShuttersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => ShuttersPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        ShuttersPart.ShutterGrowing => ShuttersBehavior.RunShutterGrowingInitializationAi,
        ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer => ShuttersBehavior.RunShutterShootableInitializationAi,
        ShuttersPart.ShutterHorizShootable => ShuttersBehavior.RunShutterHorizShootableInitializationAi,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => ShuttersPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        ShuttersPart.ShutterGrowing => species.RunShutterGrowingMainAi,
        ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer => species.RunShutterShootableMainAi,
        ShuttersPart.ShutterHorizShootable => ShuttersBehavior.RunShutterHorizShootableMainAi,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => ShuttersPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        ShuttersPart.ShutterGrowing => ShuttersBehavior.AdvanceShutterGrowingTypedAnimation,
        ShuttersPart.ShutterShootable or ShuttersPart.ShutterDestroyable or ShuttersPart.Kamer => ShuttersBehavior.AdvanceShutterShootableTypedAnimation,
        ShuttersPart.ShutterHorizShootable => ShuttersBehavior.AdvanceShutterHorizShootableTypedAnimation,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <summary>The shootable and destroyable vertical shutters' shot callbacks, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.EnemyDefinitionPointer is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer && callback == EnemyInteractionCallback.ShootableVerticalShutterShot ||
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterDestroyable && callback == EnemyInteractionCallback.DestroyableVerticalShutterShot;
}
