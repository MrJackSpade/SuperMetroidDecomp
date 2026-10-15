namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>Editable metatile references for one elevator-platform animation frame.</summary>
/// <param name="Id">Compiled elevator-platform draw-frame identity.</param>
/// <param name="Runs">Metatile/flip words grouped and ordered by native draw run; changed selections are deeply copied by the catalog.</param>
public sealed record RoomPlmElevatorPlatformVisualEntry(string Id, ushort[][] Runs);
