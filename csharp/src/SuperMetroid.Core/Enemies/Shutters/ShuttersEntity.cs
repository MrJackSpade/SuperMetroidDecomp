namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>One Shutters occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShuttersEntity(ShuttersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.ShutterGrowing => ShuttersBehavior.RunShutterGrowingInitializationAi,
        EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable or EnemyDefinitionId.Kamer => ShuttersBehavior.RunShutterShootableInitializationAi,
        EnemyDefinitionId.ShutterHorizShootable => ShuttersBehavior.RunShutterHorizShootableInitializationAi,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.ShutterGrowing => species.RunShutterGrowingMainAi,
        EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable or EnemyDefinitionId.Kamer => species.RunShutterShootableMainAi,
        EnemyDefinitionId.ShutterHorizShootable => ShuttersBehavior.RunShutterHorizShootableMainAi,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.ShutterGrowing => ShuttersBehavior.AdvanceShutterGrowingTypedAnimation,
        EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.ShutterDestroyable or EnemyDefinitionId.Kamer => ShuttersBehavior.AdvanceShutterShootableTypedAnimation,
        EnemyDefinitionId.ShutterHorizShootable => ShuttersBehavior.AdvanceShutterHorizShootableTypedAnimation,
        _ => throw new InvalidOperationException($"Shutters entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <summary>The shootable and destroyable vertical shutters' shot callbacks, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    /// <param name="callbackPointer">The selected callback pointer.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback, ushort callbackPointer) =>
        Slot.EnemyDefinitionPointer is EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer && callback == EnemyInteractionCallback.ShootableVerticalShutterShot ||
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.ShutterDestroyable && callback == EnemyInteractionCallback.DestroyableVerticalShutterShot;
}
