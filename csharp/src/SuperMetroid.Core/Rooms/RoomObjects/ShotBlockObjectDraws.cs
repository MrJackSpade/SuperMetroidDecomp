using SuperMetroid.Core.Rooms.LevelData;
using static SuperMetroid.Core.Rooms.RoomObjects.RoomObjectShotBlockDrawDefinitions;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The breakup and restoration draws of the ordinary shot blocks (#1862, #1994, #2034): the block shape each draws and
/// the installed picture each shows.
/// </summary>
internal static class ShotBlockObjectDraws
{
    /// <summary>The shot blocks' breakup frames, pictured by the installed shot-block frames.</summary>
    internal static RoomObjectDrawFamily<BreakupDraw> BreakupFamily { get; } =
        RoomObjectDrawFamily<BreakupDraw>.Pictured<InstalledRoomObjectShotBlockVisuals>("shot-block",
            frame => frame.Blocks, (visuals, draw) => visuals.Image(draw));

    /// <summary>The shot blocks' restoration frames, pictured by the installed shot-block frames.</summary>
    internal static RoomObjectDrawFamily<RestorationDraw> RestorationFamily { get; } =
        RoomObjectDrawFamily<RestorationDraw>.Pictured<InstalledRoomObjectShotBlockVisuals>("shot-block",
            frame => frame.Blocks, (visuals, draw) => visuals.Image(draw));

    /// <summary>A shot block of <paramref name="shape"/> showing <paramref name="stage"/> of its breakup.</summary>
    internal static RoomObjectFrameDraw<BreakupDraw> Breakup(Shape shape, BlockBreakupStage stage) =>
        BreakupFamily.Of(new BreakupDraw(shape, stage));

    /// <summary>The draw that restores a broken shot block of <paramref name="shape"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="shape"/> is a single block, which has no restoration draw.</exception>
    internal static RoomObjectFrameDraw<RestorationDraw> Restoration(Shape shape) =>
        RestorationFamily.Of(new RestorationDraw(shape != Shape.Single ? shape : throw new ArgumentOutOfRangeException(nameof(shape))));

    /// <summary>The blocks a shot block of each arrangement covers besides its first: the one right of it, below it, or both and the one diagonally between.</summary>
    private static readonly Dictionary<Shape, Offset16<RoomBlocks>[]> Children = new()
    {
        [Shape.Single] = [],
        [Shape.Horizontal] = [new(1, 0)],
        [Shape.Vertical] = [new(0, 1)],
        [Shape.Square] = [new(1, 0), new(0, 1), new(1, 1)],
    };

    /// <summary>Every block a breakup draw of each arrangement opens: all of them air.</summary>
    private static readonly Dictionary<Shape, RoomBlockShape> BrokenUp = Children.ToDictionary(pair => pair.Key,
        pair => new RoomBlockShape([new(Offset16<RoomBlocks>.Zero, RoomCollisionType.Air), .. pair.Value.Select(child => new RoomBlockShapeCell(child, RoomCollisionType.Air))]));

    /// <summary>The blocks a restoration of each arrangement rebuilds: a shootable parent with its other blocks linked to it.</summary>
    private static readonly Dictionary<Shape, RoomBlockShape> Restored = Children.Where(pair => pair.Key != Shape.Single)
        .ToDictionary(pair => pair.Key, pair => RoomBlockShapes.Linked(RoomCollisionType.ShootableBlock, pair.Value));

    /// <summary>
    /// A shot block of one shape at one stage of its breakup. Every breakup draw opens its blocks to air; what it shows is its
    /// installed picture (#1557).
    /// </summary>
    /// <param name="Layout">The blocks' arrangement.</param>
    /// <param name="Stage">What the breaking blocks show.</param>
    internal readonly record struct BreakupDraw(Shape Layout, BlockBreakupStage Stage)
    {
        /// <summary>Whether the draw is a breakup's last stage, which leaves the broken block open.</summary>
        internal bool BreaksOpen => Stage == BlockBreakupStage.Gone;

        /// <summary>The blocks the draw opens: the whole arrangement, all air.</summary>
        internal RoomBlockShape Blocks => BrokenUp[Layout];
    }

    /// <summary>
    /// A broken shot block of one shape growing back: its children linked to its shootable parent. What it shows is its
    /// installed picture (#1557).
    /// </summary>
    /// <param name="Layout">The blocks' arrangement, never a single block.</param>
    internal readonly record struct RestorationDraw(Shape Layout)
    {
        /// <summary>The blocks the draw rebuilds around its shootable parent.</summary>
        internal RoomBlockShape Blocks => Restored[Layout];
    }
}
