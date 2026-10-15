namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable tile appearance for one Mother Brain fake-death draw.</summary>
/// <param name="Id">Compiled identity of a wall, escape-door, background-row, or tube-clearing layout, including the two retained unused background rows.</param>
/// <param name="Blocks">Metatile/flip words flattened in native run order and word order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmMotherBrainFakeDeathVisualEntry(
    string Id, ushort[] Blocks);
