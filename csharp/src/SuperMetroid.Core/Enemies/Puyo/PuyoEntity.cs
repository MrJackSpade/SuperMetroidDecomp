namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>One Puyo occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Puyo species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PuyoEntity(PuyoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => PuyoBehavior.InitializePuyo(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPuyoMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PuyoBehavior.AdvancePuyoTypedAnimation(Slot, context);
}
