using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// The block shape of each of the Bomb Torizo statue hand's two frames (#1608), around the hand's block. What each frame
/// shows is its installed picture (#1557).
/// </summary>
internal static class BombTorizoHandObjectDrawDefinitions
{
    /// <summary>
    /// The intact hand: eight solid blocks, the hand's block and the one right of it, the block left of it, the pair
    /// above, and the three below from the left.
    /// </summary>
    private static readonly RoomBlockShape IntactHand = new(
    [
        new(new(0, -1), RoomCollisionType.SolidBlock), new(new(1, -1), RoomCollisionType.SolidBlock),
        new(new(-1, 0), RoomCollisionType.SolidBlock), new(new(0, 0), RoomCollisionType.SolidBlock), new(new(1, 0), RoomCollisionType.SolidBlock),
        new(new(-1, 1), RoomCollisionType.SolidBlock), new(new(0, 1), RoomCollisionType.SolidBlock), new(new(1, 1), RoomCollisionType.SolidBlock),
    ]);

    /// <summary>
    /// The cleared hand: the four-by-four square of open blocks from two left of and two above the hand's block, which
    /// covers every block of the intact hand.
    /// </summary>
    private static readonly RoomBlockShape ClearedHand = new(
    [
        new(new(-2, -2), RoomCollisionType.Air), new(new(-1, -2), RoomCollisionType.Air), new(new(0, -2), RoomCollisionType.Air), new(new(1, -2), RoomCollisionType.Air),
        new(new(-2, -1), RoomCollisionType.Air), new(new(-1, -1), RoomCollisionType.Air), new(new(0, -1), RoomCollisionType.Air), new(new(1, -1), RoomCollisionType.Air),
        new(new(-2, 0), RoomCollisionType.Air), new(new(-1, 0), RoomCollisionType.Air), new(new(0, 0), RoomCollisionType.Air), new(new(1, 0), RoomCollisionType.Air),
        new(new(-2, 1), RoomCollisionType.Air), new(new(-1, 1), RoomCollisionType.Air), new(new(0, 1), RoomCollisionType.Air), new(new(1, 1), RoomCollisionType.Air),
    ]);

    /// <summary>The block shape each of the hand's two frames draws.</summary>
    internal static readonly Dictionary<BombTorizoHandDraw, RoomBlockShape> Shapes = new()
    {
        [BombTorizoHandDraw.Intact] = IntactHand,
        [BombTorizoHandDraw.Cleared] = ClearedHand,
    };
}
