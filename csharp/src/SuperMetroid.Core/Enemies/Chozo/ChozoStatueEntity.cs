namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>One Chozo statue: its own AI, with the shared reactions.</summary>
/// <param name="species">The Chozo statue species owner.</param>
/// <param name="place">The place the statue occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class ChozoStatueEntity(ChozoStatueBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(place, common), IEnemyInitialization, IEnemyAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeChozoStatue(Place);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunChozoStatueMainAi(Place, frame);

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceChozoStatueTypedAnimation(Place, context);
}
