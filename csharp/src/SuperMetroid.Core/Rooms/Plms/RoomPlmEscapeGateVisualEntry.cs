namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable visual block references for one Mother Brain escape-gate frame.</summary>
/// <param name="Id">Compiled open, half-closed, or closed escape-gate draw identity.</param>
/// <param name="Blocks">Four metatile/flip words ordered down the native vertical run; changed arrays are copied by the catalog.</param>
public sealed record RoomPlmEscapeGateVisualEntry(string Id, ushort[] Blocks);
