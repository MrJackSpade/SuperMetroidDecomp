namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>One DeadTourian occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The DeadTourian species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class DeadTourianEntity(DeadTourianBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunDeadTourianInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunDeadTourianMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceDeadTourianTypedAnimation;

    /// <summary>The corpse's rot trigger, when its header names it as the shot callback; it is selected by pointer because it is not a named callback.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    /// <param name="callbackPointer">The selected callback pointer.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback, ushort callbackPointer) =>
        DeadTourianBehavior.HasDeadTourianCorpseShotCallback(Slot) && callbackPointer == Slot.Definition.ShotAiPointer;
}
