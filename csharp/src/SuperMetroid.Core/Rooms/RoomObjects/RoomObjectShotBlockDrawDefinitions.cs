namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The shot blocks' arrangements (#1557, #1862). What each draw writes is its block shape
/// (<see cref="ShotBlockObjectDraws.BreakupDraw.Blocks"/>, <see cref="ShotBlockObjectDraws.RestorationDraw.Blocks"/>);
/// what a cell shows is the draw's installed picture, at each <see cref="BlockBreakupStage"/> of the breakup.
/// </summary>
internal static class RoomObjectShotBlockDrawDefinitions
{
    /// <summary>Physical block arrangement affected by a shot-block draw.</summary>
    internal enum Shape
    {
        /// <summary>One breakable block.</summary>
        Single,
        /// <summary>Two adjacent blocks in a row.</summary>
        Horizontal,
        /// <summary>Two stacked blocks in a column.</summary>
        Vertical,
        /// <summary>Four blocks arranged in two rows.</summary>
        Square,
    }
}
