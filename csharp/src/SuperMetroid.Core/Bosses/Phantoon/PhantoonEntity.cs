using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One Phantoon occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class PhantoonEntity(PhantoonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IBossBodyFrames, IEnemyContactHooks, IEnemyPrivateShotReaction
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => PhantoonPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        PhantoonPart.Body => species.InitializePhantoonBody,
        PhantoonPart.Eye or PhantoonPart.Tentacles or PhantoonPart.Mouth => species.InitializePhantoonPart,
        _ => throw new InvalidOperationException($"Phantoon entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => PhantoonPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        PhantoonPart.Body => species.RunPhantoonBodyMainAi,
        // The eye's main is an empty step; the body positions it and it plays its own animation.
        PhantoonPart.Eye => EnemyNoOpAi.Main,
        // The drawing parts' main is an empty step; these drawing parts play their own animations.
        PhantoonPart.Tentacles or PhantoonPart.Mouth => EnemyNoOpAi.Main,
        _ => throw new InvalidOperationException($"Phantoon entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => (enemy, _) => species.AdvancePhantoonPartAnimation(enemy);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => PhantoonCombat.FindPhantoonHitboxReaction(Slot, probe, out reaction);

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        PhantoonBodyLayerFrameDefinitions.Frames.Contains(selected) ? _bossArtwork.Body(BossBodyOwner.Phantoon).Frame(selected) : null;

    /// <summary>
    /// Phantoon's body resolves its projectile hits before the ordinary passes: a retained Plasma hit must publish its hurt
    /// clock before this frame ticks it.
    /// </summary>
    public void ResolveBeforeOrdinaryHits(RoomEnemyFrame frame)
    {
        if (Slot.DefinitionId == EnemyDefinitionId.PhantoonBody &&
            frame.SamusProjectiles is not null && frame.SharedProjectiles is not null)
            _ = species.Combat.ResolvePhantoonProjectileHits(frame.SamusProjectiles, frame.SharedProjectiles);
    }

    /// <inheritdoc/>
    public bool OwnsSamusTouch => false;

    /// <inheritdoc/>
    public void ResolveAfterSamusContact(SamusState samus)
    {
    }

    /// <summary>Only Samus's contact meets the body's frames' hitboxes; its weapon collision is the encounter's own resolver.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId == EnemyDefinitionId.PhantoonBody ? EnemyCapabilities.SamusFrameHitboxes : EnemyCapabilities.None;

    /// <summary>The body's shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId == EnemyDefinitionId.PhantoonBody && reaction == EnemyReaction.PhantoonShot;
}
