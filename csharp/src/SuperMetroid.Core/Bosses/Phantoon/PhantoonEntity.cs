using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>One Phantoon occupant: its own AI handlers, with the shared reactions.</summary>
/// <param name="species">The Phantoon species owner.</param>
/// <param name="slot">The slot the enemy occupies.</param>
/// <param name="common">The shared participation.</param>
internal sealed class PhantoonEntity(PhantoonBehavior species, RoomEnemySlot slot, EnemyCommonParticipation common)
    : CommonParticipationEnemyEntity(slot, common), IEnemyInitialization, IEnemyMainAi, IEnemyTypedAnimation, IEnemyExtendedHitboxes, IEnemyBg2Frames, IEnemyContactHooks, IEnemyPrivateShotCallback
{
    /// <inheritdoc/>
    public EnemyInitializationAiHandler? FindInitialization() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.PhantoonBody => species.RunPhantoonBodyInitializationAi,
        EnemyDefinitionId.PhantoonEye or EnemyDefinitionId.PhantoonTentacles or EnemyDefinitionId.PhantoonMouth => species.RunPhantoonEyeInitializationAi,
        _ => throw new InvalidOperationException($"Phantoon entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public EnemyMainAiHandler? FindMain() => Slot.EnemyDefinitionPointer switch
    {
        EnemyDefinitionId.PhantoonBody => species.RunPhantoonBodyMainAi,
        EnemyDefinitionId.PhantoonEye => PhantoonBehavior.RunPhantoonEyeMainAi,
        EnemyDefinitionId.PhantoonTentacles or EnemyDefinitionId.PhantoonMouth => PhantoonBehavior.RunPhantoonTentaclesMainAi,
        _ => throw new InvalidOperationException($"Phantoon entity bound to {Slot.EnemyDefinitionPointer}."),
    };

    /// <inheritdoc/>
    public Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation() => species.AdvancePhantoonTypedAnimation;

    /// <inheritdoc/>
    public bool? FindHitboxCallback(EnemyHitboxProbe probe, out ushort callback) => PhantoonBehavior.FindPhantoonHitboxCallback(Slot, probe, out callback);

    /// <inheritdoc/>
    public EnemyBg2FrameLookup? FindBg2Frame(ushort selected, EnemyRoomBinding room)
    {
        if (Slot.Definition.Bank != PhantoonBg2FrameDefinitions.Bank || !PhantoonBg2FrameDefinitions.IsFrame(selected))
            return null;
        ReadOnlyMemory<EnemyBg2TilemapWrite> writes = default;
        bool found = room.InstalledArtwork.PhantoonBg2Frames?.TryGet(selected, out writes) == true;
        return new("Phantoon", found, writes);
    }

    /// <summary>
    /// Phantoon's body resolves its projectile hits before the ordinary passes: a retained Plasma hit must publish its hurt
    /// clock before this frame ticks it.
    /// </summary>
    public void ResolveBeforeOrdinaryHits(RoomEnemyFrame frame)
    {
        if (Slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody &&
            frame.SamusProjectiles is not null && frame.SharedProjectiles is not null)
            _ = species.ResolvePhantoonProjectileHits(frame.SamusProjectiles, frame.SharedProjectiles);
    }

    /// <inheritdoc/>
    public bool OwnsSamusTouch => false;

    /// <inheritdoc/>
    public void ResolveAfterSamusContact(SamusState samus)
    {
    }

    /// <summary>Only the body's Samus contact enters the extended-spritemap walker; its weapon collision is the encounter's own resolver.</summary>
    public override EnemyCapabilities Capabilities =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody ? EnemyCapabilities.ExtendedSamusHitboxes : EnemyCapabilities.None;

    /// <summary>The body's shot callback, which normal bombs dispatch like the common one.</summary>
    /// <param name="callback">The named callback the selected pointer decodes to, or null.</param>
    public bool OwnsPrivateShotCallback(EnemyInteractionCallback? callback) =>
        Slot.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody && callback == EnemyInteractionCallback.PhantoonShot;
}
