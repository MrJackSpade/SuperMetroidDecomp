using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>One Crocomire occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Crocomire species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class CrocomireEntity(CrocomireBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyNormalBombAdmission, IEnemyBg2Frames
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Crocomire => species.RunCrocomireInitializationAi,
        EnemyDefinitionId.CrocomireTongue => species.RunCrocomireTongueInitializationAi,
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Crocomire => species.RunCrocomireMainAi,
        EnemyDefinitionId.CrocomireTongue => CrocomireBehavior.RunCrocomireTongueMainAi,
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.Crocomire => species.AdvanceCrocomireTypedAnimation,
        EnemyDefinitionId.CrocomireTongue => species.AdvanceCrocomireTongueTypedAnimation,
        _ => throw new InvalidOperationException($"Crocomire entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback)
    {
        callback = 0;
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
        return new();
    }
}
