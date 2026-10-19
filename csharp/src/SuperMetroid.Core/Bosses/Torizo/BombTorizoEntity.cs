using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One BombTorizo occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The BombTorizo species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class BombTorizoEntity(BombTorizoBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunBombTorizoInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.BombTorizo => species.RunBombTorizoMainAi,
        EnemyDefinitionId.GoldenTorizo => species.RunGoldenTorizoMainAi,
        _ => throw new InvalidOperationException($"BombTorizo entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceBombTorizoTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        return BombTorizoBehavior.FindTorizoHitboxCallback(Slot, probe, out callback);
    }

    /// <summary>Both Torizos' contact and shots enter the extended-spritemap walker over their own hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes;

    /// <summary>The Torizo shot callbacks, which normal bombs dispatch like the common one; only Golden Torizo owns its own.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback is EnemyInteractionCallback.BombTorizoShot or EnemyInteractionCallback.TorizoStandUpSitDownShot ||
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo && callback == EnemyInteractionCallback.GoldenTorizoShot;
}
