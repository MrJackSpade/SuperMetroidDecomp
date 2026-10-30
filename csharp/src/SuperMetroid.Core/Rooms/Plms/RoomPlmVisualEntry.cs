using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable block appearances for one compiled PLM draw layout.</summary>
/// <param name="Id">Compiled identity of the draw layout.</param>
/// <param name="Blocks">Appearances flattened in run order, then block order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmVisualEntry(string Id, RoomBlockAppearance[] Blocks);
