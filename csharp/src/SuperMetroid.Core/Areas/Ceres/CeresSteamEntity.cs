using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>One CeresSteam occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The CeresSteam species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CeresSteamEntity(CeresSteamBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.InitializeCeresSteam;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => CeresSteamBehavior.RunCeresSteamMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => (enemy, _) => species.AdvanceCeresSteamAnimation(enemy);

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback) => CeresSteamBehavior.FindHitboxCallback(Slot, probe, out callback);

    /// <summary>The steam's contact and shots both enter the extended-spritemap walker over its own hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes;
}
