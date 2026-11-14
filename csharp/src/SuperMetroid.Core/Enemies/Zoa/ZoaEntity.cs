namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>One Zoa occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ZoaEntity(RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => ZoaBehavior.InitializeZoa(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => ZoaBehavior.RunZoaMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => (enemy, _) => ZoaBehavior.AdvanceZoaAnimation(enemy);
}
