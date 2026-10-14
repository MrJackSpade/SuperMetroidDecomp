namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only blocks for a blue-door cap or shared door-clear frame.</summary>
/// <param name="Id">Compiled editable frame identity, shared by any physical aliases of that frame.</param>
/// <param name="Blocks">Four visual metatile words in the native draw run's order; catalog construction copies changed words and rejects collision bits.</param>
public sealed record RoomPlmBlueDoorVisualEntry(string Id, ushort[] Blocks);
