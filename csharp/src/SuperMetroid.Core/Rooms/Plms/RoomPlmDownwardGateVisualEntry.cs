namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable visual block references for one downward-gate frame or shot trigger.</summary>
/// <param name="Id">Compiled identity of a gate draw frame or colored shot-trigger layout.</param>
/// <param name="Runs">Metatile/flip words grouped by native draw run and ordered within each run; changed selections are deeply copied by the catalog.</param>
public sealed record RoomPlmDownwardGateVisualEntry(string Id, ushort[][] Runs);
