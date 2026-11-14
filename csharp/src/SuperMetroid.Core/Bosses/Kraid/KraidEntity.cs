using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One Kraid occupant: its own AI, with the shared reactions.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KraidEntity(KraidBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyPrivateShotReaction
{
    /// <inheritdoc/>
    public void Initialize()
    {
        switch (KraidPartDefinitions.Parts[Slot.DefinitionId])
        {
            case KraidPart.Kraid:
                species.Rise.InitializeKraidBody(Slot);
                break;
            case KraidPart.Arm:
                species.Limbs.InitializeKraidArm(Slot);
                break;
            case KraidPart.LintTop:
                species.Projectiles.RunKraidLintTopInitializationAi(Slot);
                break;
            case KraidPart.LintMiddle:
                species.Projectiles.RunKraidLintMiddleInitializationAi(Slot);
                break;
            case KraidPart.LintBottom:
                species.Projectiles.RunKraidLintBottomInitializationAi(Slot);
                break;
            case KraidPart.Foot:
                species.Limbs.InitializeKraidFoot(Slot);
                break;
            case KraidPart.Nail:
                species.Projectiles.RunKraidNailInitializationAi(Slot);
                break;
            case KraidPart.NailBad:
                species.Projectiles.RunKraidNailBadInitializationAi(Slot);
                break;
            default:
                throw new InvalidOperationException($"Kraid entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <inheritdoc/>
    public void RunMainAi(IEnemyAiFrame frame)
    {
        switch (KraidPartDefinitions.Parts[Slot.DefinitionId])
        {
            case KraidPart.Kraid:
                species.RunKraidMainAi(Slot, frame);
                break;
            case KraidPart.Arm:
                species.RunKraidArmMainAi(Slot, frame);
                break;
            case KraidPart.LintTop or KraidPart.LintMiddle or KraidPart.LintBottom:
                species.RunKraidLintTopMainAi(Slot, frame);
                break;
            case KraidPart.Foot:
                species.RunKraidFootMainAi(Slot, frame);
                break;
            case KraidPart.Nail or KraidPart.NailBad:
                species.RunKraidNailMainAi(Slot, frame);
                break;
            default:
                throw new InvalidOperationException($"Kraid entity bound to {Slot.DefinitionId}.");
        }
    }

    /// <summary>The part the occupant is.</summary>
    private KraidPart Part => KraidPartDefinitions.Parts[Slot.DefinitionId];

    /// <summary>Every part but Kraid's body animates by its typed animation.</summary>
    public bool HasTypedAnimation => Part != KraidPart.Kraid;

    /// <inheritdoc/>
    public void AdvanceTypedAnimation(EnemyAnimationCueContext context)
    {
        switch (Part)
        {
            case KraidPart.Arm or KraidPart.LintTop or KraidPart.LintMiddle or KraidPart.LintBottom or KraidPart.Foot or KraidPart.Nail or KraidPart.NailBad:
                species.Limbs.AdvanceKraidPartAnimation(Slot);
                break;
            case KraidPart.Kraid:
            default:
                throw new InvalidOperationException($"Kraid entity bound to {Slot.DefinitionId} has no typed animation.");
        }
    }

    /// <summary>The arm's touch reaction, which its definition leaves to it; no other part's hitboxes name one.</summary>
    public EnemyReaction? SpeciesTouch => Part == KraidPart.Arm ? KraidReactions.ArmTouch : null;

    /// <summary>None: the arm's, foot's and body's definitions name the ignoring shot reaction.</summary>
    public EnemyReaction? SpeciesShot => null;

    /// <summary>
    /// Whether the occupant's definition leaves Samus's touch to its part's own reaction: the arm's push, or a nail's
    /// touch. Kraid's body leaves its touch to the species too, but has no reaction to it.
    /// </summary>
    private bool OwnsTouch => LeavesTouchToSpecies && (Part is KraidPart.Arm or KraidPart.Nail or KraidPart.NailBad);

    /// <summary>Whether the part's own touch or a shared one applies.</summary>
    public override bool ReactsToTouch => OwnsTouch || base.ReactsToTouch;

    /// <summary>Runs the nails' or the arm's own touch, or the shared one.</summary>
    /// <param name="context">The touch.</param>
    public override void ReactToTouch(EnemyTouchContext context)
    {
        if (!OwnsTouch)
            base.ReactToTouch(context);
        else if (Part == KraidPart.Arm)
            species.Combat.ReactToArmTouch(context);
        else
            species.Projectiles.ReactToKraidNailTouch(context);
    }

    /// <summary>Whether the occupant is Kraid's arm or foot, which run their own shot and bomb reactions.</summary>
    private bool IsArmOrFoot => Part is KraidPart.Arm or KraidPart.Foot;

    /// <summary>The arm's and foot's shot reaction behind their ignoring definitions, or the shared one.</summary>
    public override EnemyShotReaction? ShotReaction =>
        IsArmOrFoot && Slot.Definition.Shot == EnemyReaction.Ignore ? species.Combat.ArmAndFootShotReaction : base.ShotReaction;

    /// <summary>Runs the arm's and foot's own reaction to every bomb that overlaps them, before the ignoring check.</summary>
    /// <param name="context">The bomb contact.</param>
    public override bool TryReactToBomb(EnemyBombContext context)
    {
        if (!IsArmOrFoot)
            return base.TryReactToBomb(context);
        species.Combat.ReactToArmOrFootBomb(context);
        return true;
    }

    /// <summary>
    /// The shared Power Bomb reaction; Kraid's body leaves the blast to the species, which has no reaction to it, and fails
    /// loudly if a blast reaches him.
    /// </summary>
    /// <exception cref="InvalidDataException">The occupant is Kraid's body.</exception>
    public override EnemyPowerBombReaction? PowerBombReaction => Part == KraidPart.Kraid && LeavesPowerBombToSpecies
        ? throw new InvalidDataException($"Enemy {Slot.DefinitionId} has no response to its own Power Bomb reaction.")
        : base.PowerBombReaction;

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return KraidCombat.FindKraidFootHitboxReaction(Slot, probe, out reaction) ??
            KraidCombat.FindKraidArmHitboxReaction(Slot, probe, out reaction);
    }

    /// <summary>The arm's and foot's contact and shots meet their frames' hitboxes; only shots meet Kraid's body's; the lint and nails meet neither.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.DefinitionId is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot
            ? EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes
            : Slot.DefinitionId == EnemyDefinitionId.Kraid ? EnemyCapabilities.ProjectileFrameHitboxes : EnemyCapabilities.None;

    /// <summary>The arm's shot reaction, which normal bombs dispatch like the common one.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId == EnemyDefinitionId.KraidArm && reaction == KraidReactions.ArmShot;
}
