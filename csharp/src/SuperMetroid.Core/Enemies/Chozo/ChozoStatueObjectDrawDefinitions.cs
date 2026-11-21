using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>
/// The block shape each Chozo statue room-object frame draws (#1622), around the object's block. The crumble plug shares
/// the shot-block breakup frames. What each frame shows is its installed picture (#1557).
/// </summary>
internal static class ChozoStatueObjectDrawDefinitions
{
    /// <summary>Lower Norfair's cleared hand: the hand's block, open.</summary>
    private static readonly RoomBlockShape LowerNorfairClearedHand = new([new(new(0, 0), RoomCollisionType.Air)]);

    /// <summary>
    /// The Wrecked Ship slope access's blocks, each run along its row with how it collides opened and closed: the ledge's
    /// thirteen blocks and its right end, the shelf below and its right end, and the shaft down from the shelf.
    /// </summary>
    private static readonly ChozoSlopeAccessRun[] SlopeAccessRuns =
    [
        new(new(0, 0), 13, RoomCollisionType.Air, RoomCollisionType.SpikeBlock), new(new(13, 0), 1, RoomCollisionType.Slope, RoomCollisionType.SolidBlock),
        new(new(0, 5), 8, RoomCollisionType.Air, RoomCollisionType.SolidBlock), new(new(8, 5), 1, RoomCollisionType.Slope, RoomCollisionType.SolidBlock),
        new(new(5, 6), 2, RoomCollisionType.Air, RoomCollisionType.SolidBlock), new(new(5, 7), 1, RoomCollisionType.Air, RoomCollisionType.SolidBlock),
        new(new(5, 8), 1, RoomCollisionType.Slope, RoomCollisionType.SolidBlock),
    ];

    /// <summary>
    /// The Wrecked Ship slope access opened: a fourteen-block ledge from the object's block, open but for the slope at its
    /// right end; five rows below, a nine-block fill, open but for the slope at its right end; and below the fill's sixth
    /// block a two-block step, one open block and a slope.
    /// </summary>
    private static readonly RoomBlockShape SlopeAccessOpened = SlopeAccess(run => run.Opened);

    /// <summary>
    /// The Wrecked Ship slope access closed, over the same blocks as <see cref="SlopeAccessOpened"/>: spikes across the
    /// ledge but for its solid right end, and every other block solid.
    /// </summary>
    private static readonly RoomBlockShape SlopeAccessClosed = SlopeAccess(run => run.Closed);

    /// <summary>The block shape each Chozo statue room-object frame draws.</summary>
    internal static readonly Dictionary<ChozoStatueDraw, RoomBlockShape> Shapes = new()
    {
        [ChozoStatueDraw.LowerNorfairClearedHand] = LowerNorfairClearedHand,
        [ChozoStatueDraw.ClearSlopeAccess] = SlopeAccessOpened,
        [ChozoStatueDraw.BlockSlopeAccess] = SlopeAccessClosed,
    };

    /// <summary>The slope access's blocks, each run colliding as <paramref name="collision"/> picks from its two states.</summary>
    private static RoomBlockShape SlopeAccess(Func<ChozoSlopeAccessRun, RoomCollisionType> collision) =>
        RoomBlockShapes.Joined([.. SlopeAccessRuns.Select(run => RoomBlockShapes.Moved(RoomBlockShapes.Row(run.Length, collision(run)), run.From))]);
}
