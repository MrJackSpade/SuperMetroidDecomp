namespace SuperMetroid.Core.Rooms;

/// <summary>One Mother Brain glass frame's visible blocks in cartridge run order.</summary>
/// <param name="Id">Compiled identity of an initial, damaged, shifted, shattering, or cleared glass layout.</param>
/// <param name="Blocks">Metatile/flip words flattened in native run order and word order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmMotherBrainGlassVisualEntry(string Id, ushort[] Blocks);
