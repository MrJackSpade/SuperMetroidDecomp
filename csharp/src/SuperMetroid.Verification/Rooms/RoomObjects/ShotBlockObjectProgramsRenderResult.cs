using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>The physical, immediate and streamed of one shot block roomObject programs render result.</summary>
/// <param name="Physical">The physical.</param>
/// <param name="Immediate">The immediate.</param>
/// <param name="Streamed">The streamed.</param>
internal readonly record struct ShotBlockObjectProgramsRenderResult(RoomTerrainBlock Physical, RoomBlockAppearance Immediate, RoomBlockAppearance Streamed);
