using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>One SporeSpawn occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The SporeSpawn species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class SporeSpawnEntity(SporeSpawnBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize() => species.InitializeSporeSpawn(Slot);

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame) => species.RunSporeSpawnMainAi(Slot, frame);

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => (enemy, _) => species.AdvanceSporeSpawnAnimation(enemy);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return SporeSpawnBehavior.FindSporeSpawnHitboxReaction(Slot, probe, out reaction);
    }

    /// <summary>Spore Spawn's dud shot after the no-op check, ahead of the shared ones.</summary>
    public override Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) =>
        context.SelectedShotAi == EnemyReaction.DudShot ? species.CreateBombDudShot : base.FindBombSpecialAfterNoOp(context);

    /// <summary>Spore Spawn's contact and shots both meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Spore Spawn's shot reaction and its dud shot, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction is EnemyReaction.SporeSpawnShot or EnemyReaction.DudShot;
}
