using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>The breakup and restoration draws of the ordinary shot blocks and the block shape each draws (#1862).</summary>
internal static class ShotBlockObjectDraws
{
    /// <summary>Breakup frame <paramref name="frame"/>, 0 through 3, of a shot block of <paramref name="shape"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="frame"/> is outside 0 through 3.</exception>
    internal static RoomObjectFrameDraw<Draw> Breakup(Shape shape, int frame) =>
        RoomObjectDraw.Of(new Draw(shape, (uint)frame < BreakupFrameCount ? frame : throw new ArgumentOutOfRangeException(nameof(frame)), Restore: false));

    /// <summary>The draw that restores a broken shot block of <paramref name="shape"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is a single block, which has no restoration draw.</exception>
    internal static RoomObjectFrameDraw<Draw> Restoration(Shape shape) =>
        RoomObjectDraw.Of(new Draw(shape != Shape.Single ? shape : throw new ArgumentOutOfRangeException(nameof(shape)), 0, Restore: true));

    /// <summary>The blocks a shot block of each arrangement covers besides its first: the one right of it, below it, or both and the one diagonally between.</summary>
    private static readonly Dictionary<Shape, Offset16<RoomBlocks>[]> Children = new()
    {
        [Shape.Single] = [],
        [Shape.Horizontal] = [new(1, 0)],
        [Shape.Vertical] = [new(0, 1)],
        [Shape.Square] = [new(1, 0), new(0, 1), new(1, 1)],
    };

    /// <summary>Every block a breakup frame of each arrangement opens: all of them air.</summary>
    private static readonly Dictionary<Shape, RoomBlockShape> BrokenUp = Children.ToDictionary(pair => pair.Key,
        pair => new RoomBlockShape([new(Offset16<RoomBlocks>.Zero, RoomCollisionType.Air), .. pair.Value.Select(child => new RoomBlockShapeCell(child, RoomCollisionType.Air))]));

    /// <summary>The blocks a restoration of each arrangement rebuilds: a shootable parent with its other blocks linked to it.</summary>
    private static readonly Dictionary<Shape, RoomBlockShape> Restored = Children.Where(pair => pair.Key != Shape.Single)
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Linked(RoomCollisionType.ShootableBlock, pair.Value));

    /// <summary>
    /// A shot-block draw: four breakup stages per shape, or parent/child restoration. Breakup frames are air;
    /// restoration links its children to a shootable parent. What each frame shows is its installed picture (#1557).
    /// </summary>
    [IndependentFlag("whether a shot-block draw is the restoring variant, a fact of the decoded draw list")]
    internal readonly record struct Draw(Shape Layout, int Frame, bool Restore)
    {
        /// <summary>Whether the draw is a breakup's last stage, which leaves the broken block open.</summary>
        internal bool BreaksOpen => !Restore && Frame == BreakupFrameCount - 1;

        /// <summary>The blocks the draw changes and how each collides: the arrangement opened, or rebuilt around its shootable parent.</summary>
        internal RoomBlockShape Blocks => Restore ? Restored[Layout] : BrokenUp[Layout];
    }
}
