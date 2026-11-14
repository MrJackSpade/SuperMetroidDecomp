using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One Draygon occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class DraygonEntity(DraygonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IBossBodyFrames, IEnemyPrivateShotReaction
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (DraygonPartDefinitions.Parts[Slot.DefinitionId])
        {
            case DraygonPart.Body:
                species.Parts.InitializeDraygonBody(Slot);
                break;
            case DraygonPart.Eye or DraygonPart.Tail or DraygonPart.Arms:
                species.Parts.InitializeDraygonPart(Slot);
                break;
            default:
                throw new InvalidOperationException($"Draygon entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (DraygonPartDefinitions.Parts[Slot.DefinitionId])
        {
            case DraygonPart.Body:
                species.RunDraygonBodyMainAi(Slot, frame);
                break;
            case DraygonPart.Eye:
                species.RunDraygonEyeMainAi(Slot, frame);
                break;
            case DraygonPart.Tail or DraygonPart.Arms:
                species.RunDraygonTailMainAi(Slot, frame);
                break;
            default:
                throw new InvalidOperationException($"Draygon entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context) => species.AdvanceDraygonTypedAnimation(Slot, context);

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return DraygonCombat.FindDraygonHitboxReaction(Slot, probe, out reaction);
    }

    /// <summary>Whether the occupant is Draygon's body, the one part with reactions of its own.</summary>
    private bool IsBody => Slot.DefinitionId == EnemyDefinitionId.DraygonBody;

    /// <summary>The body's touch reaction, which its definition leaves to it; the other parts name shared ones.</summary>
    public EnemyReaction? SpeciesTouch => IsBody ? DraygonReactions.Touch : null;

    /// <summary>The body's shot reaction, which its definition leaves to it; the other parts name shared ones.</summary>
    public EnemyReaction? SpeciesShot => IsBody ? DraygonReactions.Shot : null;

    /// <summary>
    /// The body's hurt behaviour: it flashes body, eye and tail together, while a connected grapple's electrocution may
    /// drain another 256 health every eighth frame. The other parts take the shared hurt their definitions name.
    /// </summary>
    /// <param name="frame">The enemy frame.</param>
    public override bool RunHurt(RoomEnemyFrame frame)
    {
        if (!IsBody)
            return base.RunHurt(frame);
        species.Combat.ApplyDraygonHurt(Slot, species.Combat.RequireCompleteDraygonState(Slot), frame.Samus);
        return true;
    }

    /// <summary>Whether the body's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => (IsBody && LeavesTouchToSpecies) || base.ReactsToTouch;

    /// <summary>Runs the body's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (IsBody && LeavesTouchToSpecies)
            species.Combat.ReactToDraygonTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The body's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction =>
        IsBody && LeavesShotToSpecies ? species.Combat.DraygonShotReaction : base.ShotReaction;

    /// <summary>
    /// The body's dud shot for a bomb whose hitbox selects it, in place of common bomb damage once the ignoring check has
    /// passed.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReplaceCommonBombDamage(EnemyBombContext context)
    {
        if (!IsBody || context.SelectedShot != EnemyReaction.DudShot)
            return base.TryReplaceCommonBombDamage(context);
        species.Combat.CreateBombDudShot(context);
        return true;
    }

    /// <summary>The body's own stages around common bomb damage, for a bomb that selects its shot reaction.</summary>
    /// <param name="context">The bomb contact.</param>
    public override EnemyBombReaction? CommonBombDamageReaction(EnemyBombContext context) =>
        IsBody && context.SelectedShot == DraygonReactions.Shot
            ? species.Combat.DraygonBombReaction
            : base.CommonBombDamageReaction(context);

    /// <summary>The body's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        IsBody && LeavesPowerBombToSpecies ? species.Combat.DraygonPowerBombReaction : base.PowerBombReaction;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        DraygonBodyLayerFrameDefinitions.Frames.Contains(selected) ? _bossArtwork.Body(BossBodyOwner.Draygon).Frame(selected) : null;

    /// <summary>Only the body's contact and shots meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId == EnemyDefinitionId.DraygonBody ? EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes : EnemyCapabilities.None;

    /// <summary>The body's shot reaction and its dud shot, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        IsBody && (reaction == DraygonReactions.Shot || reaction == EnemyReaction.DudShot);
}
