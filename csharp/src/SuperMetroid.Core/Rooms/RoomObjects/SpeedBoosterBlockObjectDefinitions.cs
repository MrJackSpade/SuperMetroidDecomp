using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The five retail Speed Booster collision blocks and their authored crumble (#1554). A block plays its break sound and
/// crumbles through four images; a respawning block stays gone, grows back through them and restores its saved block.
/// </summary>
internal static class SpeedBoosterBlockObjectDefinitions
{
    /// <summary>The break sound.</summary>
    internal static readonly SoundEffectId BreakSoundId = EnemySound.ContactBlockBreak;

    /// <summary>The break sound's request priority.</summary>
    internal static readonly SoundRequestPriority BreakSoundPriority = SoundRequestPriority.WhenIdle;

    /// <summary>How long each crumble image shows on Brinstar's slow blocks.</summary>
    internal const int SlowCrumbleDuration = 2;

    /// <summary>How long each crumble image shows on the other blocks.</summary>
    internal const int CrumbleDuration = 1;

    /// <summary>How long a respawning block stays gone.</summary>
    internal const int GoneDuration = 48;

    /// <summary>How long each image shows as a respawning block grows back.</summary>
    internal const int RegrowDuration = 4;

    /// <summary>A slow block crumbling and growing back.</summary>
    internal static BlockReactionChange SlowRespawningCrumble { get; } = new(1,
    [
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(0), SlowCrumbleDuration, BreakSoundId) { SoundPriority = BreakSoundPriority },
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(1), SlowCrumbleDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(2), SlowCrumbleDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(3), GoneDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(2), RegrowDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(1), RegrowDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(0), RegrowDuration),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A block crumbling and growing back.</summary>
    internal static BlockReactionChange RespawningCrumble { get; } = new(1,
    [
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(0), CrumbleDuration, BreakSoundId) { SoundPriority = BreakSoundPriority },
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(1), CrumbleDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(2), CrumbleDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(3), GoneDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(2), RegrowDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(1), RegrowDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(0), RegrowDuration),
        BlockReactionFrame.RestoredBlock,
    ]);

    /// <summary>A slow block crumbling for good.</summary>
    internal static BlockReactionChange SlowPermanentCrumble { get; } = new(1,
    [
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(0), SlowCrumbleDuration, BreakSoundId) { SoundPriority = BreakSoundPriority },
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(1), SlowCrumbleDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(2), SlowCrumbleDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(3), 1),
    ]);

    /// <summary>A block crumbling for good.</summary>
    internal static BlockReactionChange PermanentCrumble { get; } = new(1,
    [
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(0), CrumbleDuration, BreakSoundId) { SoundPriority = BreakSoundPriority },
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(1), CrumbleDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(2), CrumbleDuration),
        new(SpeedBoosterBlockObjectDraws.CrumbleDrawList(3), 1),
    ]);

    /// <summary>A bombed Speed Booster block's reveal: its block image for one update.</summary>
    internal static BlockReactionChange BombReveal { get; } = new(1,
    [
        new(SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw, 1),
    ]);

    /// <summary>Brinstar's respawning Speed Booster block with the slower crumble.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition BrinstarSlowRespawning = new(
        RoomObjectKind.SpeedBlockBrinstarSlowRespawning,
        SlowRespawningCrumble);

    /// <summary>Brinstar's permanent Speed Booster block with the slower crumble.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition BrinstarSlowPermanent = new(
        RoomObjectKind.SpeedBlockBrinstarSlowPermanent,
        SlowPermanentCrumble);

    /// <summary>The area-independent standard respawning block.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition Respawning = new(
        RoomObjectKind.SpeedBlockRespawning,
        RespawningCrumble);

    /// <summary>Brinstar's Dachora-room standard respawning block.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition DachoraRespawning = new(
        RoomObjectKind.SpeedBlockDachoraRespawning,
        RespawningCrumble);

    /// <summary>The permanent block, by the area-independent behaviour or Brinstar's own.</summary>
    public static readonly SpeedBoosterBlockObjectDefinition Permanent = new(
        RoomObjectKind.SpeedBlockPermanent,
        PermanentCrumble);

    /// <summary>The Speed Booster blocks the common special-block reactions select, in every area.</summary>
    public static readonly IReadOnlyDictionary<BlockReaction, SpeedBoosterBlockObjectDefinition> CommonBlocks =
        new Dictionary<BlockReaction, SpeedBoosterBlockObjectDefinition>
        {
            [BlockReaction.Common(14)] = Respawning,
            [BlockReaction.Common(15)] = Permanent,
        };

    /// <summary>The Speed Booster blocks Brinstar's own special-block reactions select.</summary>
    public static readonly IReadOnlyDictionary<BlockReaction, SpeedBoosterBlockObjectDefinition> BrinstarBlocks =
        new Dictionary<BlockReaction, SpeedBoosterBlockObjectDefinition>
        {
            [BlockReaction.AreaSpecific(2)] = BrinstarSlowRespawning,
            [BlockReaction.AreaSpecific(3)] = BrinstarSlowPermanent,
            [BlockReaction.AreaSpecific(4)] = DachoraRespawning,
            [BlockReaction.AreaSpecific(5)] = Permanent,
        };
}
