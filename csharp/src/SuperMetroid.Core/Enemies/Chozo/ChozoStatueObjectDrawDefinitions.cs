using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The draw layouts used by the translated Chozo statue room objects: run geometry, signed offsets and how each cell collides.
/// The crumble plug shares the shot-block breakup frames. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class ChozoStatueObjectDrawDefinitions
{
    /// <summary>
    /// Each draw's horizontal runs. Lower Norfair's cleared hand is one air cell. The two Wrecked Ship slope-access
    /// states share five runs: a fourteen-cell ledge, a nine-cell fill, a two-cell
    /// transition and two single-cell stair runs, each run after the first five cells right and each run lower, the
    /// final run level. Clearing leaves slope endpoints on the ledge, the fill and the final stair; blocking makes the
    /// access solid, with spikes across the ledge except its final cell.
    /// </summary>
    internal static readonly Dictionary<ChozoStatueDraw, RoomObjectBlockStrip[]> Strips = new()
    {
        [ChozoStatueDraw.LowerNorfairClearedHand] =
        [
            new(new(0, 0), [RoomCollisionType.Air]),
        ],
        [ChozoStatueDraw.ClearSlopeAccess] =
        [
            new(new(0, 0),
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Slope,
            ]),
            new(new(0, 5),
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Air,
                RoomCollisionType.Slope,
            ]),
            new(new(5, 6),
            [
                RoomCollisionType.Air,
                RoomCollisionType.Air,
            ]),
            new(new(5, 7),
            [
                RoomCollisionType.Air,
            ]),
            new(new(5, 8),
            [
                RoomCollisionType.Slope,
            ]),
        ],
        [ChozoStatueDraw.BlockSlopeAccess] =
        [
            new(new(0, 0),
            [
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SpikeBlock,
                RoomCollisionType.SolidBlock,
            ]),
            new(new(0, 5),
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ]),
            new(new(5, 6),
            [
                RoomCollisionType.SolidBlock,
                RoomCollisionType.SolidBlock,
            ]),
            new(new(5, 7),
            [
                RoomCollisionType.SolidBlock,
            ]),
            new(new(5, 8),
            [
                RoomCollisionType.SolidBlock,
            ]),
        ],
    };
}
