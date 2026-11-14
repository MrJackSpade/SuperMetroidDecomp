using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One Draygon occupant: its own AI handlers, with the shared reactions.</summary>
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
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => species.AdvanceDraygonTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return DraygonCombat.FindDraygonHitboxReaction(Slot, probe, out reaction);
    }

    /// <summary>The Draygon body's dud shot after the no-op check, ahead of the shared ones.</summary>
    public override Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) =>
        Slot.DefinitionId == EnemyDefinitionId.DraygonBody && context.SelectedShotAi == EnemyReaction.DudShot
            ? species.Combat.CreateBombDudShot
            : base.FindBombSpecialAfterNoOp(context);

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        DraygonBodyLayerFrameDefinitions.Frames.Contains(selected) ? _bossArtwork.Body(BossBodyOwner.Draygon).Frame(selected) : null;

    /// <summary>Only the body's contact and shots meet its own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId == EnemyDefinitionId.DraygonBody ? EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes : EnemyCapabilities.None;

    /// <summary>The body's shot reaction and its dud shot, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId == EnemyDefinitionId.DraygonBody && reaction is EnemyReaction.DraygonShot or EnemyReaction.DudShot;
}
