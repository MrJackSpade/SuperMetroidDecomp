namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only blocks for one colored-door orientation and frame.</summary>
/// <param name="Id">Compiled yellow, green, or red door-cap frame identity, including its orientation.</param>
/// <param name="Blocks">Four metatile/flip words in native run order; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmColoredDoorVisualEntry(string Id, ushort[] Blocks);
