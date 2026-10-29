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
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyNormalBombAdmission, IBossBodyFrames, IEnemyPrivateShotCallback
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
        // $A4:F6BB is a literal RTL; the tongue follows the body through its instruction list and extended map.
        CrocomirePart.Tongue => EnemyNoOpAi.Main,
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => CrocomirePartDefinitions.Parts[Slot.DefinitionId] switch
    {
        CrocomirePart.Crocomire => species.AdvanceCrocomireTypedAnimation,
        CrocomirePart.Tongue => (enemy, _) => species.Fight.AdvanceCrocomireTongueAnimation(enemy),
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.DefinitionId}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        return CrocomireCombat.FindCrocomireBodyHitboxCallback(Slot, probe, out callback) ??
            CrocomireCombat.FindCrocomireTongueHitboxCallback(Slot, probe, out callback);
    }

    /// <summary>The Crocomire body takes normal bombs on its extended frames although its header names no shot callback.</summary>
    public bool AdmitsNormalBombs(bool usesExtendedHitboxes) =>
        Slot.DefinitionId == EnemyDefinitionId.Crocomire && Slot.Definition.ShotBody == EnemyCallbackBody.None && usesExtendedHitboxes;

    /// <inheritdoc/>
    public BossBodyFrame? FindBodyFrame(EnemyFrameId selected) =>
        Slot.DefinitionId == EnemyDefinitionId.Crocomire && CrocomireBodyLayerFrameDefinitions.Frames.Contains(selected)
            ? _bossArtwork.Body(BossBodyOwner.Crocomire).Frame(selected) : null;

    /// <summary>The body's and tongue's contact and shots both enter the extended-spritemap walker over their own hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes;

    /// <summary>Crocomire's hitbox shot callbacks, including the records that point at the body header's return.</summary>
    /// <param name="callback">The named callback the header or hitbox selects, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        callback is EnemyInteractionCallback.CrocomireNoOpHitboxShot or EnemyInteractionCallback.CrocomireDustHitboxShot or
            EnemyInteractionCallback.CrocomireMouthShot or EnemyInteractionCallback.CrocomireAlternateDustHitboxShot or
            EnemyInteractionCallback.CrocomireHeaderTouch;
}
