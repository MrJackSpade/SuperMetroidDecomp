namespace SuperMetroid.Core.Enemies.Sbug;

/// <summary>One Sbug occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Sbug species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SbugEntity(SbugBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => SbugBehavior.InitializeSbug(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSbugMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => SbugBehavior.AdvanceSbugTypedAnimation;
}
