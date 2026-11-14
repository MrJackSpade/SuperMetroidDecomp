namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>One Skultera occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Skultera species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SkulteraEntity(SkulteraBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => SkulteraBehavior.InitializeSkultera(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSkulteraMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => SkulteraBehavior.AdvanceSkulteraTypedAnimation(Slot, context);
}
