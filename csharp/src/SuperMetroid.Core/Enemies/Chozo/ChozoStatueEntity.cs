namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One Chozo statue: its own AI, with the shared reactions.</summary>
/// <param name="species">The Chozo statue species owner.</param>
/// <param name="slot">The slot the statue occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChozoStatueEntity(ChozoStatueBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeChozoStatue(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunChozoStatueMainAi(Slot, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceChozoStatueTypedAnimation(Slot, context);
}
