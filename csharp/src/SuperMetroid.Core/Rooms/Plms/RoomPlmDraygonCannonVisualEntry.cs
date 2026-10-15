namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One Draygon cannon frame's visible blocks in cartridge run order.</summary>
/// <param name="Id">Compiled identity of a reachable Draygon cannon draw frame.</param>
/// <param name="Blocks">Metatile/flip words flattened in native run order, then word order within each run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmDraygonCannonVisualEntry(string Id, ushort[] Blocks);
