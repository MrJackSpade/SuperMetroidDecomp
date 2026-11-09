using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>Turns room-object draw lists into level changes and background picture updates, as room-level families draw (#1395).</summary>
internal interface IRoomObjectDrawing
{
    /// <summary>Draws <paramref name="draw"/> with its block origin at <paramref name="blockIndex"/>, with its family's installed visuals and its compiled collision.</summary>
    void Draw(RoomLevelData level, int blockIndex, RoomObjectDraw draw);
}
