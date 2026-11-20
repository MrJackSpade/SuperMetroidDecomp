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
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class CrocomireBodyEntity(CrocomireBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CrocomireEntity(species, occupant, place, common), IEnemyNormalBombAdmission, IBossBodyFrames
{
    /// <inheritdoc/>
    public override void Initialize() => Species.Fight.InitializeCrocomire(Occupant, Place);

    /// <summary>The encounter the body set up.</summary>
    internal override CrocomireEnemyState Encounter => Place.RequireSpeciesState<CrocomireEnemyState>();

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunCrocomireMainAi(Occupant, frame);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.AdvanceCrocomireTypedAnimation(Place, context);

    /// <inheritdoc/>
    public override bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) =>
        CrocomireCombat.FindCrocomireBodyHitboxReaction(Place, probe, out reaction);

    /// <summary>Crocomire's components own his touch: Samus meets his body through their contact.</summary>
    protected override bool OwnsTouch => true;

    /// <summary>Crocomire's hitboxes own his shot reaction: a shot meets the reaction the hitbox it strikes selects.</summary>
    protected override bool OwnsShot => true;

    /// <summary>
    /// Runs the body's own reaction to a bomb its frames' hitboxes meet with one of his own reactions, before the ignoring
    /// check: his hitboxes own his shot reaction, so normal bombs reach the hit counter, dust or mouth reaction the hitbox
    /// selects.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (!context.UsesFrameHitboxes || context.SelectedShot == EnemyReaction.OrdinaryShot)
            return base.TryReactToBomb(context);
        Species.Combat.ReactToCrocomireBomb(context, Encounter);
        return true;
    }

    /// <summary>The body's own Power Bomb reaction where its definition leaves the blast to it, or the shared one.</summary>
    public override EnemyPowerBombReaction PowerBombReaction =>
        LeavesPowerBombToSpecies ? OwnPowerBombReaction : base.PowerBombReaction;

    /// <summary>The body's own Power Bomb reaction, which acts on the encounter the body set up.</summary>
    private EnemyPowerBombReaction OwnPowerBombReaction => field ??= CrocomireCombat.CrocomirePowerBombReactionOf(this);

    /// <summary>The body takes normal bombs on its frames' hitboxes, which own his shot reaction.</summary>
    public bool AdmitsNormalBombs(bool collidesByFrameHitboxes) => collidesByFrameHitboxes;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        CrocomireBodyLayerFrameDefinitions.Frames.Contains(selected) ? bossArtwork.Body(BossBodyOwner.Crocomire).Frame(selected) : null;
}
