using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One draw a room object makes on the room's level data: a frame of its family (#1402, #2034). A draw is drawn by its own
/// family (<see cref="RoomObjectDrawFamily{TFrame}"/>), so a timed table that holds frames of several families draws each
/// through the family it belongs to. A family's visual catalog keys its installed pictures by the draw. Where the game
/// stored each draw is known only to the import pipeline.
/// </summary>
internal abstract record RoomObjectDraw
{
    /// <summary>Draws the frame with its block origin at <paramref name="origin"/>, through the family it belongs to.</summary>
    /// <param name="level">The room's level data.</param>
    /// <param name="origin">The block the frame's shape starts at.</param>
    /// <param name="drawing">The room's installed room-object pictures.</param>
    internal abstract void Draw(RoomLevelData level, Position16<RoomBlocks> origin, IRoomObjectDrawing drawing);
}
