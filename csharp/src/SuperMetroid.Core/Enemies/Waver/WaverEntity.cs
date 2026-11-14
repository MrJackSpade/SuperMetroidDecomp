namespace SuperMetroid.Core.Enemies.Waver;

/// <summary>One Waver occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Waver species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class WaverEntity(WaverBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => WaverBehavior.InitializeWaver(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunWaverMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => WaverBehavior.AdvanceWaverTypedAnimation(Slot, context);
}
