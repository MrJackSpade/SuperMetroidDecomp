using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.RoomObjects;

/// <summary>
/// One horizontal strip of a PLM block draw: where the strip starts and its cells. A draw's strips are a list whose end
/// is its last strip (#1546).
/// </summary>
/// <param name="Start">Where the strip's first cell lies, in blocks from the draw's origin; the first strip starts at the origin.</param>
/// <param name="Cells">How each cell the strip writes collides, left to right; what it shows is the frame's installed picture.</param>
internal readonly record struct RoomObjectBlockStrip(Offset16<RoomBlocks> Start, RoomCollisionType[] Cells);
