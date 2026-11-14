namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The shot blocks' arrangements and breakup length (#1557, #1862). What each frame draws is its block shape
/// (<see cref="ShotBlockObjectDraws.Draw.Blocks"/>); what a cell shows is the frame's installed picture.
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

    /// <summary>Breakup frames per shape.</summary>
    internal const int BreakupFrameCount = 4;
}
