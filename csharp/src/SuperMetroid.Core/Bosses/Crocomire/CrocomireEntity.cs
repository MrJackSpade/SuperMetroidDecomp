using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One Crocomire occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CrocomireEntity(CrocomireBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyNormalBombAdmission, IEnemyBg2Frames, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => CrocomirePartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        CrocomirePart.Crocomire => species.InitializeCrocomire,
        CrocomirePart.Tongue => species.InitializeCrocomireTongue,
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
        CrocomirePart.Tongue => (enemy, _) => species.AdvanceCrocomireTongueAnimation(enemy),
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        return CrocomireBehavior.FindCrocomireBodyHitboxCallback(Slot, probe, out callback) ??
            CrocomireBehavior.FindCrocomireTongueHitboxCallback(Slot, probe, out callback);
    }

    /// <summary>The Crocomire body takes normal bombs on its extended frames although its header names no shot callback.</summary>
    public bool AdmitsNormalBombs(bool usesExtendedHitboxes) =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.Crocomire && Slot.Definition.ShotAiPointer == 0 && usesExtendedHitboxes;

    /// <inheritdoc/>
    public EnemyBg2FrameLookup? FindBg2Frame(ushort selected, EnemyRoomBinding room)
    {
        if (Slot.EnemyDefinitionPointer != EnemyDefinitionId.Crocomire ||
            Slot.Definition.Bank != CrocomireBodyVisualDefinitions.Bank || !CrocomireBodyVisualDefinitions.HasBg2(selected))
            return null;
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes = default;
        bool found = room.InstalledArtwork.CrocomireBg2Frames?.TryGet(selected, out writes) == true;
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
