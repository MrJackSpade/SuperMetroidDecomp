namespace SuperMetroid.Core.Rooms;

/// <summary>Editable metatile references for one station animation frame.</summary>
/// <param name="Id">Case-sensitive compiled frame identity for map, energy, missile, save-pod, or left/right station-access artwork.</param>
/// <param name="Runs">Mutable input arrays in native draw-run/cell order, with the compiled run lengths; words contain only ten-bit 16-by-16 metatile indices and two parent flip bits, not collision data or run offsets.</param>
public sealed record RoomPlmStationVisualEntry(string Id, ushort[][] Runs);
