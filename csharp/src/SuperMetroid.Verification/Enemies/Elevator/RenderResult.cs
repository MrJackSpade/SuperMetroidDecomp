using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Enemies.Elevator;

/// <summary>The terrain block and the displayed appearance at one drawn elevator-platform cell.</summary>
/// <param name="Physical">The cell's terrain block, which owns its collision.</param>
/// <param name="Rendered">The appearance the room displays there.</param>
internal readonly record struct RenderResult(RoomTerrainBlock Physical, RoomBlockAppearance Rendered);
