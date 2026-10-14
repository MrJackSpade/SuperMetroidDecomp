namespace SuperMetroid.Core.Rooms;

/// <summary>Visual-only blocks for a grey-door cap or shared door-clear frame.</summary>
/// <param name="Id">Compiled clear or grey-door frame identity, including orientation.</param>
/// <param name="Blocks">Four metatile/flip words in native run order; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmGreyDoorVisualEntry(string Id, ushort[] Blocks);
