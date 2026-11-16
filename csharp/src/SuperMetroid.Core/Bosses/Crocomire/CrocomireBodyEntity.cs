using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// Crocomire's body (#2146): his fight, his frames' hitboxes, the normal bombs they take and the frames he shows on his
/// boss body layer.
/// </summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class CrocomireBodyEntity(CrocomireBehavior species, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CrocomireEntity(species, place, common), IEnemyNormalBombAdmission, IBossBodyFrames
{
    /// <inheritdoc/>
    public override void Initialize() => Species.Fight.InitializeCrocomire(Place);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunCrocomireMainAi(Place, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceCrocomireTypedAnimation(Place, context);

    /// <inheritdoc/>
    public override bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        CrocomireCombat.FindCrocomireBodyHitboxReaction(Place, probe, out reaction);

    /// <summary>The body's ignoring touch leads to Crocomire's component contact.</summary>
    protected override bool OwnsTouch => Place.Definition.Touch == EnemyReaction.Ignore;

    /// <summary>The body's definition names no shot reaction, which leads to his hitboxes' reactions.</summary>
    protected override bool OwnsShot => Place.Definition.Shot is null;

    /// <summary>
    /// Runs the body's own reaction to a bomb its frames' hitboxes meet with one of his own reactions, before the ignoring
    /// check: the body's definition names no shot reaction, so normal bombs reach the hit counter, dust or mouth reaction
    /// the hitbox selects.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (Place.Definition.Shot is not null || !context.UsesFrameHitboxes ||
            context.SelectedShot == EnemyReaction.NormalEnemyShot)
            return base.TryReactToBomb(context);
        Species.Combat.ReactToCrocomireBomb(context);
        return true;
    }

    /// <summary>The body's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        LeavesPowerBombToSpecies ? Species.Combat.CrocomirePowerBombReaction : base.PowerBombReaction;

    /// <summary>The body takes normal bombs on its frames' hitboxes although its species definition names no shot reaction.</summary>
    public bool AdmitsNormalBombs(bool collidesByFrameHitboxes) => Place.Definition.Shot is null && collidesByFrameHitboxes;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        CrocomireBodyLayerFrameDefinitions.Frames.Contains(selected) ? bossArtwork.Body(BossBodyOwner.Crocomire).Frame(selected) : null;
}
