using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Turns PLM draw lists into level changes and BG tilemap updates, as room-level families draw (#1395).</summary>
internal interface IPlmDrawing
{
    /// <summary>Draws <paramref name="draw"/> with its block origin at <paramref name="blockIndex"/>, with its family's installed visuals and its compiled collision.</summary>
    void Draw(RoomLevelData level, int blockIndex, PlmDraw draw);
}
