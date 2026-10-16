namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>One Shutters occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Shutters species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ShuttersEntity(ShuttersBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's GrowingShutterEnemyState, set by its initializer.</summary>
    internal GrowingShutterEnemyState? GrowingShutterState { get; set; }

    /// <summary>The occupant's VerticalShutterEnemyState, set by its initializer.</summary>
    internal VerticalShutterEnemyState? VerticalShutterState { get; set; }

    /// <summary>The occupant's HorizontalShutterEnemyState, set by its initializer.</summary>
    internal HorizontalShutterEnemyState? HorizontalShutterState { get; set; }

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
}
