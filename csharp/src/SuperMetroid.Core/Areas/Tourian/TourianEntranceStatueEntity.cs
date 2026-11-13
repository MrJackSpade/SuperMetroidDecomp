using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>One TourianEntranceStatue occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The TourianEntranceStatue species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class TourianEntranceStatueEntity(TourianEntranceStatueBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeTourianEntranceStatue;

    /// <inheritdoc/>
    /// <remarks>The statues have no main behaviour; they play their authored animations.</remarks>
    public EnemyMainAiHandler? FindMain() => EnemyNoOpAi.Main;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => TourianEntranceStatueBehavior.AdvanceTourianEntranceStatueTypedAnimation;
}
