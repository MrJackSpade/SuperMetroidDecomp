using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One Kraid occupant: its own AI handlers, with the shared reactions.</summary>
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

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => KraidPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        KraidPart.Arm or KraidPart.LintTop or KraidPart.LintMiddle or KraidPart.LintBottom or KraidPart.Foot or KraidPart.Nail or KraidPart.NailBad => (enemy, _) => species.Limbs.AdvanceKraidPartAnimation(enemy),
        KraidPart.Kraid => null,
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.DefinitionId}."),
    };

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
        Slot.DefinitionId == EnemyDefinitionId.KraidArm && reaction == EnemyReaction.KraidArmShot;
}
