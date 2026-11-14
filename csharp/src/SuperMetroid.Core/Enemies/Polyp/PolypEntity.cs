namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>One Polyp occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Polyp species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PolypEntity(PolypBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializePolyp(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunPolypMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => PolypBehavior.AdvancePolypTypedAnimation(Slot, context);
}
