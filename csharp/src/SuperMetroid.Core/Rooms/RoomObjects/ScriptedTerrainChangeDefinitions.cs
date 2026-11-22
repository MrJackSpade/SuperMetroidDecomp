namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The terrain changes of the scripted room objects (#1554): a block an enemy breaks, the
/// critters' wall breaking open, Crateria's main-street escape passage and the Maridia elevatube's start.
/// </summary>
internal static partial class ScriptedTerrainChangeDefinitions
{
    /// <summary>How long each of the first three images of a block's breakup shows.</summary>
    internal const int BreakupFrameDuration = 4;

    /// <summary>The updates the Maridia elevatube waits before its sound.</summary>
    internal const int ElevatubeDelay = 16;

    /// <summary>The blocks the escape passage moves right to clear its second pair.</summary>
    private static readonly Offset16<RoomBlocks> SecondPassagePair = new(4, 0);

    /// <summary>
    /// A block an enemy breaks: the crumble sound, at low priority, with the first of a single block's four breakup
    /// images; the last image, the cleared block, shows for one update.
    /// </summary>
    internal static TerrainChange EnemyBreakableBlockBreak { get; } = SingleBlockBreakup(EnemySound.BlockCrumble, SoundRequestPriority.Low);

    /// <summary>
    /// The critters' wall breaking open where a shot hit it: the crumble sound with the first of its four images; the
    /// last, the cleared block, shows for one update before <see cref="CritterWallBreakthroughObject"/> marks the
    /// critters escaped.
    /// </summary>
    internal static TerrainChange CritterWallBreakthrough { get; } = TerrainChange.Breakup(
        EscapeAnimalWalls.Family.Of(EscapeAnimalDraw.Cracking), EscapeAnimalWalls.Family.Of(EscapeAnimalDraw.Breaking),
        EscapeAnimalWalls.Family.Of(EscapeAnimalDraw.Crumbling), EscapeAnimalWalls.Family.Of(EscapeAnimalDraw.Blank),
        BreakupFrameDuration, EnemySound.BlockCrumble, SoundRequestPriority.Normal);

    /// <summary>
    /// A single block's breakup over its four breakup images, <paramref name="sound"/>, when there is one, requested at
    /// <paramref name="soundPriority"/> with the first.
    /// </summary>
    internal static TerrainChange SingleBlockBreakup(SoundEffectId? sound, SoundRequestPriority soundPriority) => TerrainChange.Breakup(
        ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Rock), ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Pieces),
        ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Dust), ShotBlockObjectDraws.Breakup(BlockArrangement.Single, BlockBreakupStage.Gone),
        BreakupFrameDuration, sound, soundPriority);

    /// <summary>Crateria's main-street escape passage: its first pair of blocks clears, then, an update later, the pair beside it (<see cref="SecondPassagePair"/>).</summary>
    internal static TerrainChange CrateriaEscapePassage { get; } = new(1,
    [
        new(CrateriaMainstreetEscapePassageObjectDefinitions.ClearPairDraw, 1),
        new(CrateriaMainstreetEscapePassageObjectDefinitions.ClearPairDraw, 1, SecondPassagePair),
    ]);

    /// <summary>
    /// The Maridia elevatube's start: its block draw, then the wait before <see cref="MaridiaElevatubeObject"/> plays
    /// the elevatube sound as it ends.
    /// </summary>
    internal static TerrainChange MaridiaElevatube { get; } = new(1,
    [
        new(MaridiaElevatubeObjectDefinitions.BlockDraw, ElevatubeDelay),
    ]);
}
