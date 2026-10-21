using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One Crocomire occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class CrocomireEntity(CrocomireBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyNormalBombAdmission, IEnemyBg2Frames, IEnemyPrivateShotCallback
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => CrocomirePartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        CrocomirePart.Crocomire => species.Fight.InitializeCrocomire,
        CrocomirePart.Tongue => species.Fight.InitializeCrocomireTongue,
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => CrocomirePartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        CrocomirePart.Crocomire => species.RunCrocomireMainAi,
        // $A4:F6BB is a literal RTL; the tongue follows the body through its instruction list and extended map.
        CrocomirePart.Tongue => EnemyNoOpAi.Main,
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => CrocomirePartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        CrocomirePart.Crocomire => species.AdvanceCrocomireTypedAnimation,
        CrocomirePart.Tongue => (enemy, _) => species.Fight.AdvanceCrocomireTongueAnimation(enemy),
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.EnemyDefinitionPointer}."),
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
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire && Slot.Definition.ShotAiPointer == 0 && usesExtendedHitboxes;

    /// <inheritdoc/>
    public EnemyBg2FrameLookup? FindBg2Frame(EnemyFrameId selected, EnemyRoomBinding room)
    {
        if (Slot.EnemyDefinitionPointer != EnemyDefinitionId.Crocomire || !CrocomireBg2FrameDefinitions.IsFrame(selected))
            return null;
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes = default;
        bool found = _bossArtwork.Installed.CrocomireBg2Frames?.TryGet(selected, out writes) == true;
        return new("Crocomire body", found, writes);
    }

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
