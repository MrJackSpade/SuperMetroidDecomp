using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The five Speed Booster collision blocks and their authored crumble (#1554). A block plays its break sound and
/// crumbles through four images; a respawning block stays gone, grows back through them and restores its saved block.
/// </summary>
internal static partial class SpeedBoosterBlockObjectDefinitions
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

    /// <summary>A block's crumble, the break's sound with its rock: its rock, pieces and dust, each for <paramref name="duration"/>.</summary>
    private static BlockReactionFrame[] Crumbling(int duration) =>
    [
        new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Rock), duration, BreakSoundId) { SoundPriority = BreakSoundPriority },
        new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Pieces), duration),
        new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Dust), duration),
    ];

    /// <summary>A respawning block growing back: its dust, pieces and rock, each for <see cref="RegrowDuration"/>, then the block restored.</summary>
    private static BlockReactionFrame[] Regrowth =>
    [
        new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Dust), RegrowDuration),
        new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Pieces), RegrowDuration),
        new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Rock), RegrowDuration),
        BlockReactionFrame.RestoredBlock,
    ];

    /// <summary>A slow block crumbling and growing back.</summary>
    internal static BlockReactionChange SlowRespawningCrumble { get; } = new(1, [.. Crumbling(SlowCrumbleDuration), new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Gone), GoneDuration), .. Regrowth]);

    /// <summary>A block crumbling and growing back.</summary>
    internal static BlockReactionChange RespawningCrumble { get; } = new(1, [.. Crumbling(CrumbleDuration), new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Gone), GoneDuration), .. Regrowth]);

    /// <summary>A slow block crumbling for good.</summary>
    internal static BlockReactionChange SlowPermanentCrumble { get; } = new(1, [.. Crumbling(SlowCrumbleDuration), new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Gone), 1)]);

    /// <summary>A block crumbling for good.</summary>
    internal static BlockReactionChange PermanentCrumble { get; } = new(1, [.. Crumbling(CrumbleDuration), new(SpeedBoosterBlockObjectDraws.Crumble(BlockBreakupStage.Gone), 1)]);

    /// <summary>A bombed Speed Booster block's reveal: its block image for one update.</summary>
    internal static LastingBlockReactionChange BombReveal { get; } = new(1,
    [
        new(SpeedBoosterBlockObjectDrawDefinitions.BombRevealDraw, 1),
    ]);

    /// <summary>
    /// The crumble of each Speed Booster block the common special-block reactions select, in every area: the respawning
    /// block and the permanent one.
    /// </summary>
    public static readonly IReadOnlyDictionary<SpecialBlockReaction, BlockReactionChange> CommonBlocks =
        new Dictionary<SpecialBlockReaction, BlockReactionChange>
        {
            [SpecialBlockReaction.RespawningSpeedBlock] = RespawningCrumble,
            [SpecialBlockReaction.PermanentSpeedBlock] = PermanentCrumble,
        };

    /// <summary>
    /// The crumble of each Speed Booster block Brinstar's own special-block reactions select: the slow respawning and
    /// slow permanent blocks, the Dachora room's respawning block (which crumbles as the common one does) and the permanent
    /// block.
    /// </summary>
    public static readonly IReadOnlyDictionary<SpecialBlockReaction, BlockReactionChange> BrinstarBlocks =
        new Dictionary<SpecialBlockReaction, BlockReactionChange>
        {
            [SpecialBlockReaction.SlowRespawningSpeedBlock] = SlowRespawningCrumble,
            [SpecialBlockReaction.SlowPermanentSpeedBlock] = SlowPermanentCrumble,
            [SpecialBlockReaction.BrinstarRespawningSpeedBlock] = RespawningCrumble,
            [SpecialBlockReaction.BrinstarPermanentSpeedBlock] = PermanentCrumble,
        };
}
