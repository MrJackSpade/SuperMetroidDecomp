namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// The authored timing of a change a room object makes to the terrain at its block (#1554): how many updates pass
/// before its first frame draws, and its frames in order, each staying for its own updates. The change ends on the
/// update after its last frame's updates run out; a change with no frames ends on its first update.
/// </summary>
/// <param name="FirstFrameDelay">The updates from the object's spawn to its first frame, at least one.</param>
/// <param name="Frames">The frames in the order they draw.</param>
internal sealed record TerrainChange(int FirstFrameDelay, IReadOnlyList<TerrainChangeFrame> Frames)
{
    /// <summary>A change that draws <paramref name="draw"/> on the object's first update and ends on its second.</summary>
    internal static TerrainChange DrawnOnce(RoomObjectDraw draw) => new(1, [new TerrainChangeFrame(draw, 1)]);

    /// <summary>
    /// A change whose whole effect its spawn made: the object draws nothing and leaves the room on its first update.
    /// </summary>
    internal static TerrainChange MadeAtSpawn { get; } = new(1, []);
}
