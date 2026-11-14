using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One Crocomire occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class CrocomireEntity(CrocomireBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyNormalBombAdmission, IBossBodyFrames, IEnemyPrivateShotReaction
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (CrocomirePartDefinitions.Parts[Slot.DefinitionId])
        {
            case CrocomirePart.Crocomire:
                species.Fight.InitializeCrocomire(Slot);
                break;
            case CrocomirePart.Tongue:
                species.Fight.InitializeCrocomireTongue(Slot);
                break;
            default:
                throw new InvalidOperationException($"Crocomire entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (CrocomirePartDefinitions.Parts[Slot.DefinitionId])
        {
            case CrocomirePart.Crocomire:
                species.RunCrocomireMainAi(Slot, frame);
                break;
            // The tongue's main AI is an empty step; the tongue follows the body through its animation and extended map.
            case CrocomirePart.Tongue:
                break;
            default:
                throw new InvalidOperationException($"Crocomire entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (CrocomirePartDefinitions.Parts[Slot.DefinitionId])
        {
            case CrocomirePart.Crocomire:
                species.AdvanceCrocomireTypedAnimation(Slot, context);
                break;
            case CrocomirePart.Tongue:
                species.Fight.AdvanceCrocomireTongueAnimation(Slot);
                break;
            default:
                throw new InvalidOperationException($"Crocomire entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <summary>None: the body's definition names the ignoring touch and the tongue's the ordinary one.</summary>
    public EnemyReaction? SpeciesTouch => null;

    /// <summary>None: the body's definition names no shot reaction, and every live hitbox names its own.</summary>
    public EnemyReaction? SpeciesShot => null;

    /// <summary>Whether the occupant is Crocomire's body rather than his tongue.</summary>
    private bool IsBody => Slot.DefinitionId == EnemyDefinitionId.Crocomire;

    /// <summary>
    /// The touch the body's ignoring definition and the tongue's ordinary one lead to: Crocomire's component contact, which
    /// his frames' hitboxes decide. Neither falls to the shared touch reactions.
    /// </summary>
    private bool OwnsTouch =>
        IsBody ? Slot.Definition.Touch == EnemyReaction.Ignore : Slot.Definition.Touch == EnemyReaction.NormalEnemyTouch;

    /// <summary>Whether Crocomire's component contact or a shared touch applies.</summary>
    public override bool ReactsToTouch => OwnsTouch || base.ReactsToTouch;

    /// <summary>Runs Crocomire's component contact, or the shared touch.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (OwnsTouch)
            species.Combat.ReactToCrocomireTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>
    /// The shot reaction of the body, whose definition names none, and of the tongue, whose definition names the ordinary
    /// one: both route their hitboxes' reactions; anything else takes the shared one.
    /// </summary>
    public override EnemyShotReaction? ShotReaction =>
        (IsBody ? Slot.Definition.Shot is null : Slot.Definition.Shot == EnemyReaction.NormalEnemyShot)
            ? species.Combat.CrocomireShotReaction
            : base.ShotReaction;

    /// <summary>
    /// Runs the body's own reaction to a bomb its frames' hitboxes meet with one of his own reactions, before the ignoring
    /// check: the body's definition names no shot reaction, so normal bombs reach the hit counter, dust or mouth reaction
    /// the hitbox selects.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (!IsBody || Slot.Definition.Shot is not null || !context.UsesFrameHitboxes ||
            context.SelectedShot == EnemyReaction.NormalEnemyShot)
            return base.TryReactToBomb(context);
        species.Combat.ReactToCrocomireBomb(context);
        return true;
    }

    /// <summary>The body's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        IsBody && LeavesPowerBombToSpecies ? species.Combat.CrocomirePowerBombReaction : base.PowerBombReaction;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return CrocomireCombat.FindCrocomireBodyHitboxReaction(Slot, probe, out reaction) ??
            CrocomireCombat.FindCrocomireTongueHitboxReaction(Slot, out reaction);
    }

    /// <summary>The Crocomire body takes normal bombs on its frames' hitboxes although its species definition names no shot reaction.</summary>
    public bool AdmitsNormalBombs(bool collidesByFrameHitboxes) =>
        Slot.DefinitionId == EnemyDefinitionId.Crocomire && Slot.Definition.Shot is null && collidesByFrameHitboxes;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        Slot.DefinitionId == EnemyDefinitionId.Crocomire && CrocomireBodyLayerFrameDefinitions.Frames.Contains(selected)
            ? _bossArtwork.Body(BossBodyOwner.Crocomire).Frame(selected) : null;

    /// <summary>The body's and tongue's contact and shots both meet their own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Crocomire's own hitbox shot reactions.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction == CrocomireReactions.HitCountShot || reaction == CrocomireReactions.DustHitboxShot ||
        reaction == CrocomireReactions.MouthShot || reaction == CrocomireReactions.AlternateDustHitboxShot;
}
