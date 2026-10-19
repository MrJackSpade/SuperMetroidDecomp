namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>One Gunship occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Gunship species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class GunshipEntity(GunshipBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.ShipTop => species.RunShipTopInitializationAi,
        EnemyDefinitionId.ShipBottomEntrance => species.RunShipBottomEntranceInitializationAi,
        _ => throw new InvalidOperationException($"Gunship entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.ShipTop => species.RunGunshipMainAi,
        EnemyDefinitionId.ShipBottomEntrance => EnemyCommonParticipation.FindMain(Slot),
        _ => throw new InvalidOperationException($"Gunship entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceGunshipTypedAnimation;
}
