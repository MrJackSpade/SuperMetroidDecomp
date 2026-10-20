namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable tile appearances for one compiled PLM draw layout, run by run.</summary>
/// <param name="Id">Compiled identity of the draw layout.</param>
/// <param name="Runs">Metatile/flip words of each native run in order; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmVisualRunEntry(string Id, ushort[][] Runs);
