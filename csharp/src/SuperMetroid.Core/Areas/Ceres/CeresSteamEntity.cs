using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One CeresSteam occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The CeresSteam species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresSteamEntity(CeresSteamBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunCeresSteamInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => CeresSteamBehavior.RunCeresSteamMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceCeresSteamTypedAnimation;
}
