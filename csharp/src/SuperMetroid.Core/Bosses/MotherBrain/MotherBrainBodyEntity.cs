using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// Mother Brain's body (#2146): her fight, the shots her definition leaves to her, the dud her own shot reaction makes of
/// a bomb, and the pictures she shows on her boss body layer.
/// </summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="occupant">The occupant the entity is created for, which carries its identity.</param>
/// <param name="place">The place the body occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
/// <param name="zebesWarning">The room's background warning, which her death types Zebes's warning on.</param>
internal sealed class MotherBrainBodyEntity(MotherBrainBehavior species, RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common,
    BossArtworkBinding bossArtwork, RoomWarning zebesWarning)
    : MotherBrainEntity(species, occupant, place, common), IBossBodyFrames, IEnemyPrivateShotReaction
{
    /// <summary>The encounter the body begins as it is set up, which her head joins (#2218, #2554).</summary>
    internal override MotherBrainEnemyState Encounter { get; } = species.BeginEncounter(occupant, zebesWarning);

    /// <summary>The body's hitboxes: her brain's rectangle until its first animation shows a body frame, then its frame's.</summary>
    public override EnemyHitboxContact FindHitboxReaction(EnemyHitboxProbe probe) =>
        MotherBrainCombat.FindMotherBrainBodyHitboxReaction(Place, probe);

    /// <inheritdoc/>
    public override void RunMainAi(IEnemyAiFrame frame) => Species.RunMotherBrainBodyMain(Place, Encounter, frame.Samus, frame.SamusBombs);

    /// <inheritdoc/>
    public override void AdvanceTypedAnimation(EnemyAnimationCueContext context) => Species.Body.AdvanceMotherBrainBodyAnimation(Encounter, context.Samus);

    /// <summary>The body's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction ShotReaction => Occupant.Definition.Shot == MotherBrainReactions.BodyShot ? (field ??= Species.Combat.BodyShotReactionOf(this)) : base.ShotReaction;

    /// <summary>
    /// Makes a dud of a bomb that selects the body's shot reaction, before the ignoring check: the bomb's explosion is
    /// already marked as having struck, and the body's reaction does no health, phase or multipart work of its
    /// own.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context) =>
        context.SelectedShot == MotherBrainReactions.BodyShot || base.TryReactToBomb(context);


    /// <inheritdoc/>
    public BossBodyParts BodyParts => Encounter.BodyParts;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        MotherBrainBodyVisualDefinitions.AnimationFrames.Contains(selected) ? bossArtwork.Body(BossBodyOwner.MotherBrain).Frame(selected) : null;

    /// <summary>Shots meet the body's frames' hitboxes; its contact uses the definition's radius.</summary>
    public override EnemyCapabilities Capabilities => EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>The body's own shot reaction, which a normal bomb triggers as it does the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction reaction) => reaction == MotherBrainReactions.BodyShot;
}
