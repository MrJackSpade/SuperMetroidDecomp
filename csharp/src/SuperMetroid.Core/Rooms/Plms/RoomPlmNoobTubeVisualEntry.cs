namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One N00b tube frame's visible blocks in cartridge run order.</summary>
/// <param name="Id">Compiled identity of an intact, damaged, opened, cleared, or broken-tube layout.</param>
/// <param name="Blocks">Metatile/flip words flattened in native run order and word order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmNoobTubeVisualEntry(string Id, ushort[] Blocks);
