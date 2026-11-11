using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One cell of a <see cref="RoomBlockShape"/>: where it lies from the room object's origin block and how it collides.
/// What it shows is the frame's installed picture at the same place (#1557).
/// </summary>
/// <param name="Cell">Where the cell lies, in blocks from the room object's origin block.</param>
/// <param name="Collision">How the cell collides once the shape is drawn.</param>
internal readonly record struct RoomBlockShapeCell(Offset16<RoomBlocks> Cell, RoomCollisionType Collision);
