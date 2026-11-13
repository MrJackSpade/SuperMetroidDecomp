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
    /// The Wrecked Ship slope access opened: a fourteen-block ledge from the object's block, open but for the slope at its
    /// right end; five rows below, a nine-block fill, open but for the slope at its right end; and below the fill's sixth
    /// block a two-block step, one open block and a slope.
    /// </summary>
    private static readonly RoomBlockShape SlopeAccessOpened = new(
    [
        new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.Air), new(new(2, 0), RoomCollisionType.Air),
        new(new(3, 0), RoomCollisionType.Air), new(new(4, 0), RoomCollisionType.Air), new(new(5, 0), RoomCollisionType.Air),
        new(new(6, 0), RoomCollisionType.Air), new(new(7, 0), RoomCollisionType.Air), new(new(8, 0), RoomCollisionType.Air),
        new(new(9, 0), RoomCollisionType.Air), new(new(10, 0), RoomCollisionType.Air), new(new(11, 0), RoomCollisionType.Air),
        new(new(12, 0), RoomCollisionType.Air), new(new(13, 0), RoomCollisionType.Slope),
        new(new(0, 5), RoomCollisionType.Air), new(new(1, 5), RoomCollisionType.Air), new(new(2, 5), RoomCollisionType.Air),
        new(new(3, 5), RoomCollisionType.Air), new(new(4, 5), RoomCollisionType.Air), new(new(5, 5), RoomCollisionType.Air),
        new(new(6, 5), RoomCollisionType.Air), new(new(7, 5), RoomCollisionType.Air), new(new(8, 5), RoomCollisionType.Slope),
        new(new(5, 6), RoomCollisionType.Air), new(new(6, 6), RoomCollisionType.Air),
        new(new(5, 7), RoomCollisionType.Air),
        new(new(5, 8), RoomCollisionType.Slope),
    ]);

    /// <summary>
    /// The Wrecked Ship slope access closed, over the same blocks as <see cref="SlopeAccessOpened"/>: spikes across the
    /// ledge but for its solid right end, and every other block solid.
    /// </summary>
    private static readonly RoomBlockShape SlopeAccessClosed = new(
    [
        new(new(0, 0), RoomCollisionType.SpikeBlock), new(new(1, 0), RoomCollisionType.SpikeBlock), new(new(2, 0), RoomCollisionType.SpikeBlock),
        new(new(3, 0), RoomCollisionType.SpikeBlock), new(new(4, 0), RoomCollisionType.SpikeBlock), new(new(5, 0), RoomCollisionType.SpikeBlock),
        new(new(6, 0), RoomCollisionType.SpikeBlock), new(new(7, 0), RoomCollisionType.SpikeBlock), new(new(8, 0), RoomCollisionType.SpikeBlock),
        new(new(9, 0), RoomCollisionType.SpikeBlock), new(new(10, 0), RoomCollisionType.SpikeBlock), new(new(11, 0), RoomCollisionType.SpikeBlock),
        new(new(12, 0), RoomCollisionType.SpikeBlock), new(new(13, 0), RoomCollisionType.SolidBlock),
        new(new(0, 5), RoomCollisionType.SolidBlock), new(new(1, 5), RoomCollisionType.SolidBlock), new(new(2, 5), RoomCollisionType.SolidBlock),
        new(new(3, 5), RoomCollisionType.SolidBlock), new(new(4, 5), RoomCollisionType.SolidBlock), new(new(5, 5), RoomCollisionType.SolidBlock),
        new(new(6, 5), RoomCollisionType.SolidBlock), new(new(7, 5), RoomCollisionType.SolidBlock), new(new(8, 5), RoomCollisionType.SolidBlock),
        new(new(5, 6), RoomCollisionType.SolidBlock), new(new(6, 6), RoomCollisionType.SolidBlock),
        new(new(5, 7), RoomCollisionType.SolidBlock),
        new(new(5, 8), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>The block shape each Chozo statue room-object frame draws.</summary>
    internal static readonly Dictionary<ChozoStatueDraw, RoomBlockShape> Shapes = new()
    {
        [ChozoStatueDraw.LowerNorfairClearedHand] = LowerNorfairClearedHand,
        [ChozoStatueDraw.ClearSlopeAccess] = SlopeAccessOpened,
        [ChozoStatueDraw.BlockSlopeAccess] = SlopeAccessClosed,
    };
}
