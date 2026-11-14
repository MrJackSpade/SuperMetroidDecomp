using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One Phantoon occupant: its own AI, with the shared reactions.</summary>
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
    public void Initialize()
    {
        switch (PhantoonPartDefinitions.Parts[Slot.DefinitionId])
        {
            case PhantoonPart.Body:
                species.InitializePhantoonBody(Slot);
                break;
            case PhantoonPart.Eye or PhantoonPart.Tentacles or PhantoonPart.Mouth:
                species.InitializePhantoonPart(Slot);
                break;
            default:
                throw new InvalidOperationException($"Phantoon entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (PhantoonPartDefinitions.Parts[Slot.DefinitionId])
        {
            case PhantoonPart.Body:
                species.RunPhantoonBodyMainAi(Slot, frame);
                break;
            // The eye's main is an empty step; the body positions it and it plays its own animation.
            case PhantoonPart.Eye:
                break;
            // The drawing parts' main is an empty step; these drawing parts play their own animations.
            case PhantoonPart.Tentacles or PhantoonPart.Mouth:
                break;
            default:
                throw new InvalidOperationException($"Phantoon entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvancePhantoonPartAnimation(Slot);

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

    /// <summary>Whether the occupant is Phantoon's body, the one part with reactions of its own.</summary>
    private bool IsBody => Slot.DefinitionId == EnemyDefinitionId.PhantoonBody;

    /// <summary>The body's touch reaction, which its definition leaves to it; the other parts name shared ones.</summary>
    public EnemyReaction? SpeciesTouch => IsBody ? PhantoonReactions.Touch : null;

    /// <summary>The body's shot reaction, which its definition leaves to it; the other parts name shared ones.</summary>
    public EnemyReaction? SpeciesShot => IsBody ? PhantoonReactions.Shot : null;

    /// <summary>
    /// The body's hurt behaviour after a vulnerable-eye hit, which owns every Phantoon hurt frame: it alternates the
    /// body's colours between white and the new health band; body movement and main AI resume only after the common
    /// flash timing clears the hurt state. The other parts take the shared hurt their definitions name.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        if (!IsBody)
            return base.RunHurt(frame);
        species.ApplyPhantoonHurt(Slot, species.RequireCompletePhantoonState(Slot));
        return true;
    }

    /// <summary>Whether the body's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => (IsBody && LeavesTouchToSpecies) || base.ReactsToTouch;

    /// <summary>Runs the body's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (IsBody && LeavesTouchToSpecies)
            species.Combat.ReactToPhantoonBodyTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The body's own stages around common bomb damage, for a bomb that selects its shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        IsBody && context.SelectedShot == PhantoonReactions.Shot
            ? species.Combat.PhantoonBombReaction
            : base.CommonBombDamageReaction(context);

    /// <summary>The body's shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        IsBody && reaction == PhantoonReactions.Shot;
}
