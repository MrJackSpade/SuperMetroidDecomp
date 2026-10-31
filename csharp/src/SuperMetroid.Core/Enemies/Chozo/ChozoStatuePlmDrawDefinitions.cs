using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// Physical bank-$84 draw layouts used by the translated Chozo statue PLMs.
/// The crumble plug shares the already-compiled shot-block breakup frames.
/// Complete terrain blocks, run geometry and signed offsets are cartridge mechanics;
/// installed artwork may replace only their block appearances.
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
    internal static readonly Dictionary<ChozoStatueDraw, PlmBlockStrip[]> Strips = new()
    {
        [ChozoStatueDraw.LowerNorfairClearedHand] =
        [
            new(new(0, 0), [RoomTerrainBlock.Of(255, BlockMirroring.None, RoomCollisionType.Air)]),
        ],
        [ChozoStatueDraw.ClearSlopeAccess] =
        [
            new(new(0, 5),
            [
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.Slope),
            ]),
            new(new(5, 6),
            [
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(411, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(297, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(297, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(297, BlockMirroring.None, RoomCollisionType.Slope),
            ]),
            new(new(5, 7),
            [
                RoomTerrainBlock.Of(443, BlockMirroring.None, RoomCollisionType.Air),
                RoomTerrainBlock.Of(297, BlockMirroring.None, RoomCollisionType.Air),
            ]),
            new(new(5, 8),
            [
                RoomTerrainBlock.Of(443, BlockMirroring.None, RoomCollisionType.Air),
            ]),
            new(new(0, 0),
            [
                RoomTerrainBlock.Of(443, BlockMirroring.None, RoomCollisionType.Slope),
            ]),
        ],
        [ChozoStatueDraw.BlockSlopeAccess] =
        [
            new(new(0, 5),
            [
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SpikeBlock),
                RoomTerrainBlock.Of(299, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ]),
            new(new(5, 6),
            [
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(273, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(411, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(297, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(297, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(297, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ]),
            new(new(5, 7),
            [
                RoomTerrainBlock.Of(443, BlockMirroring.None, RoomCollisionType.SolidBlock),
                RoomTerrainBlock.Of(297, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ]),
            new(new(5, 8),
            [
                RoomTerrainBlock.Of(443, BlockMirroring.None, RoomCollisionType.SolidBlock),
            ]),
            new(new(0, 0),
            [
                RoomTerrainBlock.Of(443, BlockMirroring.None, RoomCollisionType.SolidBlock),
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
