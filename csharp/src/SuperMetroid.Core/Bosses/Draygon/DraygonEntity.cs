using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>One Draygon occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Draygon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
/// <param name="bossArtwork">The session's installed boss artwork.</param>
internal sealed class DraygonEntity(DraygonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common, BossArtworkBinding bossArtwork)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyBg2Frames, IEnemyPrivateShotCallback
{
    /// <summary>The session's installed boss artwork.</summary>
    private readonly BossArtworkBinding _bossArtwork = bossArtwork;
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => DraygonPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        DraygonPart.Body => species.Parts.InitializeDraygonBody,
        DraygonPart.Eye or DraygonPart.Tail or DraygonPart.Arms => species.Parts.InitializeDraygonPart,
        _ => throw new InvalidOperationException($"Draygon entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => DraygonPartDefinitions.Parts[Slot.EnemyDefinitionPointer] switch
    {
        DraygonPart.Body => species.RunDraygonBodyMainAi,
        DraygonPart.Eye => species.RunDraygonEyeMainAi,
        DraygonPart.Tail or DraygonPart.Arms => species.RunDraygonTailMainAi,
        _ => throw new InvalidOperationException($"Draygon entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvanceDraygonTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out EnemyInteractionCallback? callback)
    {
        callback = null;
        return DraygonCombat.FindDraygonHitboxCallback(Slot, probe, out callback);
    }

    /// <summary>The Draygon body's dud shot ($A5:8046 CreateDudShot) after the no-op check, ahead of the shared ones.</summary>
    public override Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody && context.SelectedShotAi == EnemyInteractionCallback.DudShot
            ? EnemyDudShot.Bomb
            : base.FindBombSpecialAfterNoOp(context);

    /// <inheritdoc/>
    public EnemyBg2FrameLookup? FindBg2Frame(ushort selected, EnemyRoomBinding room)
    {
        if (Slot.Definition.Bank != DraygonBg2FrameDefinitions.Bank || !DraygonBg2FrameDefinitions.IsFrame(selected))
            return null;
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes = default;
        bool found = _bossArtwork.Installed.DraygonBg2Frames?.TryGet(selected, out writes) == true;
        return new("Draygon", found, writes);
    }

    /// <summary>Only the body's contact and shots enter the extended-spritemap walker over its own hitboxes.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody ? EnemyCapabilities.ExtendedSamusHitboxes | EnemyCapabilities.ExtendedProjectileHitboxes : EnemyCapabilities.None;

    /// <summary>The body's shot callback and its dud shot, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody && callback is EnemyInteractionCallback.DraygonShot or EnemyInteractionCallback.DudShot;
}
