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
    /// <remarks>$AA:D7C7 is the one-byte RTL before the initializer; the statues animate through their ROM lists.</remarks>
    public EnemyMainAiHandler? FindMain() => EnemyNoOpAi.Main;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceTourianEntranceStatueTypedAnimation;
}
