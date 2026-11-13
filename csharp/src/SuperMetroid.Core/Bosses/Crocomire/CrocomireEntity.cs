using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One Crocomire occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class CrocomireEntity(CrocomireBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyFrameHitboxes, IEnemyNormalBombAdmission, IBossBodyFrames, IEnemyPrivateShotReaction
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => CrocomirePartDefinitions.Parts[Slot.DefinitionId] switch
    {
        CrocomirePart.Crocomire => species.Fight.InitializeCrocomire,
        CrocomirePart.Tongue => species.Fight.InitializeCrocomireTongue,
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => CrocomirePartDefinitions.Parts[Slot.DefinitionId] switch
    {
        CrocomirePart.Crocomire => species.RunCrocomireMainAi,
        // The tongue's main AI is an empty step; the tongue follows the body through its animation script and extended map.
        CrocomirePart.Tongue => EnemyNoOpAi.Main,
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyAnimationCueContext>? FindTypedAnimation() => CrocomirePartDefinitions.Parts[Slot.DefinitionId] switch
    {
        CrocomirePart.Crocomire => species.AdvanceCrocomireTypedAnimation,
        CrocomirePart.Tongue => (enemy, _) => species.Fight.AdvanceCrocomireTongueAnimation(enemy),
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxReaction(EnemyHitboxProbe probe, out EnemyReaction? reaction)
    {
        reaction = null;
        return CrocomireCombat.FindCrocomireBodyHitboxReaction(Slot, probe, out reaction) ??
            CrocomireCombat.FindCrocomireTongueHitboxReaction(Slot, out reaction);
    }

    /// <summary>The Crocomire body takes normal bombs on its frames' hitboxes although its species definition names no shot reaction.</summary>
    public bool AdmitsNormalBombs(bool collidesByFrameHitboxes) =>
        Slot.DefinitionId == EnemyDefinitionId.Crocomire && Slot.Definition.Shot is null && collidesByFrameHitboxes;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        Slot.DefinitionId == EnemyDefinitionId.Crocomire && CrocomireBodyLayerFrameDefinitions.Frames.Contains(selected)
            ? _bossArtwork.Body(BossBodyOwner.Crocomire).Frame(selected) : null;

    /// <summary>The body's and tongue's contact and shots both meet their own frames' hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.SamusFrameHitboxes | EnemyCapabilities.ProjectileFrameHitboxes;

    /// <summary>Crocomire's own hitbox shot reactions.</summary>
    /// <param name="reaction">The reaction the species definition or the selected hitbox names, or null.</param>
    public bool OwnsPrivateShotReaction(EnemyReaction? reaction) =>
        reaction is EnemyReaction.CrocomireHitCountShot or EnemyReaction.CrocomireDustHitboxShot or
            EnemyReaction.CrocomireMouthShot or EnemyReaction.CrocomireAlternateDustHitboxShot;
}
