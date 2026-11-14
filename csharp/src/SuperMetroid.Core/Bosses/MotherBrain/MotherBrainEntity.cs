using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One MotherBrain occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The MotherBrain species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class MotherBrainEntity(MotherBrainBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IBossBodyFrames, IEnemyPrivateShotReaction
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (MotherBrainPartDefinitions.Parts[Slot.DefinitionId])
        {
            case MotherBrainPart.Body:
                species.Body.InitializeMotherBrainBody(Slot);
                break;
            case MotherBrainPart.Head:
                species.Head.InitializeMotherBrainHead(Slot);
                break;
            case MotherBrainPart.Tubes:
                MotherBrainFakeDeath.InitializeMotherBrainFallingTube(Slot);
                break;
            default:
                throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (MotherBrainPartDefinitions.Parts[Slot.DefinitionId])
        {
            case MotherBrainPart.Body:
                species.RunMotherBrainBodyMainAi(Slot, frame);
                break;
            case MotherBrainPart.Head:
                species.RunMotherBrainHeadMainAi(Slot, frame);
                break;
            case MotherBrainPart.Tubes:
                species.FakeDeath.RunMotherBrainFallingTubeMain(Slot);
                break;
            default:
                throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (MotherBrainPartDefinitions.Parts[Slot.DefinitionId])
        {
            case MotherBrainPart.Tubes:
                MotherBrainBody.AdvanceMotherBrainTubeAnimation(Slot);
                break;
            case MotherBrainPart.Body or MotherBrainPart.Head:
                species.AdvanceMotherBrainBodyTypedAnimation(Slot, context);
                break;
            default:
                throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <summary>The part the occupant is.</summary>
    private MotherBrainPart Part => MotherBrainPartDefinitions.Parts[Slot.DefinitionId];

    /// <summary>The head's touch reaction, which its definition leaves to it; the body and tubes name shared ones.</summary>
    public EnemyReaction? SpeciesTouch => Part == MotherBrainPart.Head ? MotherBrainReactions.HeadTouch : null;

    /// <summary>The head's or the body's shot reaction, which their definitions leave to them; the tubes name a shared one.</summary>
    public EnemyReaction? SpeciesShot => Part switch
    {
        MotherBrainPart.Head => MotherBrainReactions.HeadShot,
        MotherBrainPart.Body => MotherBrainReactions.BodyShot,
        MotherBrainPart.Tubes => null,
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}."),
    };

    /// <summary>Whether the head's own touch, where its definition leaves Samus's touch to it, or a shared one applies.</summary>
    public override bool ReactsToTouch => (Part == MotherBrainPart.Head && LeavesTouchToSpecies) || base.ReactsToTouch;

    /// <summary>Runs the head's own touch where its definition leaves Samus's touch to it, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (Part == MotherBrainPart.Head && LeavesTouchToSpecies)
            MotherBrainCombat.ReactToMotherBrainHeadTouch(context);
        else
            base.ReactToTouch(context);
    }

    /// <summary>The head's or the body's own shot reaction where its definition leaves shots to it, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction => !LeavesShotToSpecies ? base.ShotReaction : Part switch
    {
        MotherBrainPart.Body => species.Combat.BodyShotReaction,
        MotherBrainPart.Head => species.Combat.HeadShotReaction,
        MotherBrainPart.Tubes => base.ShotReaction,
        _ => throw new InvalidOperationException($"MotherBrain entity bound to {Slot.DefinitionId}."),
    };

    /// <summary>
    /// Runs the body's or the head's own reaction to a bomb that selects its shot reaction, before the ignoring check. The
    /// body's makes a dud shot: the frame-hitbox bomb walker has already marked the physical explosion, and the body's
    /// reaction does no health, phase or multipart work of its own.
    /// </summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (Part == MotherBrainPart.Body && context.SelectedShot == MotherBrainReactions.BodyShot)
            return true;
        if (Part == MotherBrainPart.Head && context.SelectedShot == MotherBrainReactions.HeadShot)
        {
            species.Combat.ReactToMotherBrainHeadBomb(context);
            return true;
        }
        return base.TryReactToBomb(context);
    }

    /// <summary>
    /// The shared Power Bomb reaction; the head and body leave the blast to the species, which has no reaction to it, and
    /// fail loudly if a blast reaches them.
    /// </summary>
    /// <exception cref="InvalidDataException">The occupant is the head or the body.</exception>
    public override EnemyPowerBombReaction? PowerBombReaction =>
        (Part is MotherBrainPart.Head or MotherBrainPart.Body) && LeavesPowerBombToSpecies
            ? throw new InvalidDataException($"Enemy {Slot.DefinitionId} has no response to its own Power Bomb reaction.")
            : base.PowerBombReaction;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction) => MotherBrainCombat.FindMotherBrainHitboxReaction(Slot, probe, out reaction);

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        Slot.DefinitionId == EnemyDefinitionId.MotherBrainBody && MotherBrainBodyVisualDefinitions.BodyLayerFrames.Contains(selected)
            ? _bossArtwork.Body(BossBodyOwner.MotherBrain).Frame(selected) : null;

    /// <summary>Shots meet the body's and head's frames' hitboxes; their contact and the tubes' use the definition's radius.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId is EnemyDefinitionId.MotherBrainBody or EnemyDefinitionId.MotherBrainHead ? EnemyCapabilities.ProjectileFrameHitboxes : EnemyCapabilities.None;

    /// <summary>The body's and head's own shot reactions, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species' definition leaves to it or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        (Part == MotherBrainPart.Body && reaction == MotherBrainReactions.BodyShot) ||
        (Part == MotherBrainPart.Head && reaction == MotherBrainReactions.HeadShot);
}
