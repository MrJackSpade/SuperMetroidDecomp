namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>One replaceable Samus Eater block appearance in native run order.</summary>
/// <param name="Id">Compiled floor/ceiling idle or chewing-pose identity.</param>
/// <param name="Blocks">Eight metatile/flip words flattened by native run then word order; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmSamusEaterVisualEntry(string Id, ushort[] Blocks);
