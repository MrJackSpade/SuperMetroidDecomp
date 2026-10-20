using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>One Kraid occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Kraid species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class KraidEntity(KraidBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => KraidPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        KraidPart.Kraid => species.Rise.InitializeKraidBody,
        KraidPart.Arm => species.Limbs.InitializeKraidArm,
        KraidPart.LintTop => species.Projectiles.RunKraidLintTopInitializationAi,
        KraidPart.LintMiddle => species.Projectiles.RunKraidLintMiddleInitializationAi,
        KraidPart.LintBottom => species.Projectiles.RunKraidLintBottomInitializationAi,
        KraidPart.Foot => species.Limbs.InitializeKraidFoot,
        KraidPart.Nail => species.Projectiles.RunKraidNailInitializationAi,
        KraidPart.NailBad => species.Projectiles.RunKraidNailBadInitializationAi,
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => KraidPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        KraidPart.Kraid => species.RunKraidMainAi,
        KraidPart.Arm => species.RunKraidArmMainAi,
        KraidPart.LintTop or KraidPart.LintMiddle or KraidPart.LintBottom => species.RunKraidLintTopMainAi,
        KraidPart.Foot => species.RunKraidFootMainAi,
        KraidPart.Nail or KraidPart.NailBad => species.RunKraidNailMainAi,
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => KraidPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        KraidPart.Arm or KraidPart.LintTop or KraidPart.LintMiddle or KraidPart.LintBottom or KraidPart.Foot or KraidPart.Nail or KraidPart.NailBad => (enemy, _) => species.Limbs.AdvanceKraidPartAnimation(enemy),
        KraidPart.Kraid => null,
        _ => throw new InvalidOperationException($"Kraid entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        return KraidCombat.FindKraidFootHitboxCallback(Slot, probe, out callback) ??
            KraidCombat.FindKraidArmHitboxCallback(Slot, probe, out callback);
    }

    /// <summary>The arm and foot take both extended-spritemap walkers; Kraid's body takes only the projectile walker; the lint and nails take neither.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.EnemyDefinitionPointer is EnemyDefinitionId.KraidArm or EnemyDefinitionId.KraidFoot
            ? EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes
            : Slot.EnemyDefinitionPointer == EnemyDefinitionId.Kraid ? EnemyCapabilities.ExtendedProjectileHitboxes : EnemyCapabilities.None;

    /// <summary>The arm's shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm && callback == EnemyInteractionCallback.KraidArmShot;
}
