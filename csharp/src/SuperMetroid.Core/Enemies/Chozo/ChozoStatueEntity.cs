namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One ChozoStatue occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The ChozoStatue species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChozoStatueEntity(ChozoStatueBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.NoobTubeCrack => species.RunNoobTubeCrackInitializationAi,
        EnemyDefinitionId.Chozo => species.RunChozoInitializationAi,
        _ => throw new InvalidOperationException($"ChozoStatue entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Chozo => species.RunChozoStatueMainAi,
        EnemyDefinitionId.NoobTubeCrack => EnemyCommonParticipation.FindMain(Slot),
        _ => throw new InvalidOperationException($"ChozoStatue entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Chozo => species.AdvanceChozoStatueTypedAnimation,
        EnemyDefinitionId.NoobTubeCrack => null,
        _ => throw new InvalidOperationException($"ChozoStatue entity bound to {Slot.EnemyDefinitionPointer}."),
    };
}
