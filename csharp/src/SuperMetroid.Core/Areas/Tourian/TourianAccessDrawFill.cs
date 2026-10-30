using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>A run of rows, four blocks wide, every block drawn with one terrain block.</summary>
/// <param name="Rows">The rows.</param>
/// <param name="Block">The terrain every block of every row becomes.</param>
internal readonly record struct TourianAccessDrawFill(int Rows, RoomTerrainBlock Block);
