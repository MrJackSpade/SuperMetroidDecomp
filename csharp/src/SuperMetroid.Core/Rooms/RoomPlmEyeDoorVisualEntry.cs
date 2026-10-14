namespace SuperMetroid.Core.Rooms;

/// <summary>Editable visual blocks for one eye-door draw frame.</summary>
/// <param name="Id">Compiled editable identity for an eye, middle, bottom, or shared clearing frame.</param>
/// <param name="Blocks">Metatile/flip words in native single-run order, with the component's exact block count; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmEyeDoorVisualEntry(string Id, ushort[] Blocks);
