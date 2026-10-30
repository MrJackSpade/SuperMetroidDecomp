using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable block appearances for one compiled PLM draw layout, run by run.</summary>
/// <param name="Id">Compiled identity of the draw layout.</param>
/// <param name="Runs">The appearances of each run in order; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmVisualRunEntry(string Id, RoomBlockAppearance[][] Runs);
