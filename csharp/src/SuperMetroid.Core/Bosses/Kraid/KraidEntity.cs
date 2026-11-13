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
    public EnemyInitializationAiHandler? FindInitialization() => KraidPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        KraidPart.Kraid => species.Rise.InitializeKraidBody,
        KraidPart.Arm => species.Limbs.InitializeKraidArm,
        KraidPart.LintTop => species.Projectiles.RunKraidLintTopInitializationAi,
        KraidPart.LintMiddle => species.Projectiles.RunKraidLintMiddleInitializationAi,
        KraidPart.LintBottom => species.Projectiles.RunKraidLintBottomInitializationAi,
        KraidPart.Foot => species.Limbs.InitializeKraidFoot,
        KraidPart.Nail => species.Projectiles.RunKraidNailInitializationAi,
        KraidPart.NailBad => species.Projectiles.RunKraidNailBadInitializationAi,
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => KraidPartDefinitions.Parts[Slot.DefinitionId] switch
    {
        KraidPart.Kraid => species.RunKraidMainAi,
        KraidPart.Arm => species.RunKraidArmMainAi,
        KraidPart.LintTop or KraidPart.LintMiddle or KraidPart.LintBottom => species.RunKraidLintTopMainAi,
        KraidPart.Foot => species.RunKraidFootMainAi,
        KraidPart.Nail or KraidPart.NailBad => species.RunKraidNailMainAi,
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.DefinitionId}."),
    };

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
    /// <param name="reaction">The reaction the header or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        Slot.DefinitionId == EnemyDefinitionId.KraidArm && reaction == EnemyReaction.KraidArmShot;
}
