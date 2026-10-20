namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable tile appearances for one compiled PLM draw layout.</summary>
/// <param name="Id">Compiled identity of the draw layout.</param>
/// <param name="Blocks">Metatile/flip words flattened in native run order, then word order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmVisualEntry(string Id, ushort[] Blocks);
