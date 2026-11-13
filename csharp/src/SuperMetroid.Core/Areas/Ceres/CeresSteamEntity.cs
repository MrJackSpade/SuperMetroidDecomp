using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One Ceres steam occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Ceres steam species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresSteamEntity(CeresSteamBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeCeresSteam;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => CeresSteamBehavior.RunCeresSteamMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => (enemy, _) => CeresSteamBehavior.AdvanceCeresSteamAnimation(enemy);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => CeresSteamBehavior.FindHitboxReaction(Slot, probe, out reaction);

    /// <summary>The steam's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;
}
