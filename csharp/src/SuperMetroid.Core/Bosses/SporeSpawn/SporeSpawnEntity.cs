using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One SporeSpawn occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The SporeSpawn species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SporeSpawnEntity(SporeSpawnBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => species.RunSporeSpawnInitializationAi;

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => species.RunSporeSpawnMainAi;

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceSporeSpawnTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
        return SporeSpawnBehavior.FindSporeSpawnHitboxCallback(Slot, probe, out callback);
    }

    /// <summary>Spore Spawn's dud shot ($A5:8046 CreateDudShot) after the no-op check, ahead of the shared ones.</summary>
    public override Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) =>
        context.SelectedShotAi == EnemyInteractionCallback.DudShot ? EnemyDudShot.Bomb : base.FindBombSpecialAfterNoOp(context);

    /// <summary>Spore Spawn's contact and shots both enter the extended-spritemap walker over its own hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes;

    /// <summary>Spore Spawn's shot callback and its dud shot, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    /// <param name="callbackPointer">The selected callback pointer.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback, ushort callbackPointer) =>
        callback is EnemyInteractionCallback.SporeSpawnShot or EnemyInteractionCallback.DudShot;
}
