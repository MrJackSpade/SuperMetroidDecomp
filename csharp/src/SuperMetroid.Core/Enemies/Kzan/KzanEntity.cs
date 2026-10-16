namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>One Kzan occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Kzan species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KzanEntity(KzanBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <summary>The occupant's state, set by its initializer.</summary>
    internal KzanEnemyState? State { get; set; }

    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.KzanTop => species.RunKzanTopInitializationAi,
        EnemyDefinitionId.KzanBottom => species.RunKzanBottomInitializationAi,
        _ => throw new InvalidOperationException($"Kzan entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.KzanTop => species.RunKzanTopMainAi,
        EnemyDefinitionId.KzanBottom => species.RunKzanBottomMainAi,
        _ => throw new InvalidOperationException($"Kzan entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.KzanTop => KzanBehavior.AdvanceKzanTypedAnimation,
        EnemyDefinitionId.KzanBottom => Common.FindTypedAnimation(Slot),
        _ => throw new InvalidOperationException($"Kzan entity bound to {Slot.EnemyDefinitionPointer}."),
    };
}
