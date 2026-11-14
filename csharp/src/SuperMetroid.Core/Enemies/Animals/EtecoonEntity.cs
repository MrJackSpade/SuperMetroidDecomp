namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>One Etecoon occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Etecoon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class EtecoonEntity(EtecoonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => EtecoonBehavior.InitializeEtecoon(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunEtecoonMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => EtecoonBehavior.AdvanceEtecoonTypedAnimation;
}
