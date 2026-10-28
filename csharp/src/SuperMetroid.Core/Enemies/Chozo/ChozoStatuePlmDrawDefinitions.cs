using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Physical bank-$84 draw layouts used by the translated Chozo statue PLMs.
/// The crumble plug shares the already-compiled shot-block breakup frames.
/// Complete level words, run geometry and signed offsets are cartridge mechanics;
/// installed artwork may replace only their visual block references.
/// </summary>
internal static class ChozoStatuePlmDrawDefinitions
{
    /// <summary>
    /// Each draw's horizontal runs. Lower Norfair's cleared hand is one air cell with the blank tile. The two Wrecked Ship
    /// slope-access states share five runs and identical art: a fourteen-cell ledge, a nine-cell fill, a two-cell
    /// transition and two single-cell stair runs, each run after the first five cells right and each run lower, the
    /// final run level. Clearing leaves slope endpoints on the ledge, the fill and the final stair; blocking makes the
    /// access solid, with spikes across the ledge except its final cell.
    /// </summary>
    internal static readonly Dictionary<ChozoStatueDraw, ChozoStatueStrip[]> Strips = new()
    {
        [ChozoStatueDraw.LowerNorfairClearedHand] =
        [
            new(0, 0, [RoomLevelWord.Create(0xff, LevelBlockFlipFlags.None, RoomCollisionType.Air)]),
        ],
        [ChozoStatueDraw.ClearSlopeAccess] =
        [
            new(0, 5,
            [
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
            ]),
            new(5, 6,
            [
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x19b, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x129, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x129, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x129, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
            ]),
            new(5, 7,
            [
                RoomLevelWord.Create(0x1bb, LevelBlockFlipFlags.None, RoomCollisionType.Air),
                RoomLevelWord.Create(0x129, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ]),
            new(5, 8,
            [
                RoomLevelWord.Create(0x1bb, LevelBlockFlipFlags.None, RoomCollisionType.Air),
            ]),
            new(0, 0,
            [
                RoomLevelWord.Create(0x1bb, LevelBlockFlipFlags.None, RoomCollisionType.Slope),
            ]),
        ],
        [ChozoStatueDraw.BlockSlopeAccess] =
        [
            new(0, 5,
            [
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SpikeBlock),
                RoomLevelWord.Create(0x12b, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ]),
            new(5, 6,
            [
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x111, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x19b, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x129, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x129, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x129, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ]),
            new(5, 7,
            [
                RoomLevelWord.Create(0x1bb, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
                RoomLevelWord.Create(0x129, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ]),
            new(5, 8,
            [
                RoomLevelWord.Create(0x1bb, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ]),
            new(0, 0,
            [
                RoomLevelWord.Create(0x1bb, LevelBlockFlipFlags.None, RoomCollisionType.SolidBlock),
            ]),
        ],
    };

    /// <summary>The cleared hand's and the blocked and clear slope layouts' published visual identities.</summary>
    internal static readonly Dictionary<ChozoStatueDraw, string> VisualIds = new()
    {
        [ChozoStatueDraw.LowerNorfairClearedHand] = "lower-norfair-cleared-hand",
        [ChozoStatueDraw.ClearSlopeAccess] = "wrecked-ship-clear-slope-access",
        [ChozoStatueDraw.BlockSlopeAccess] = "wrecked-ship-block-slope-access",
    };
}
