using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Saves;

/// <summary>Confirmed native save-point identity handed to the selected-slot SRAM owner.</summary>
public readonly record struct SaveStationPersistenceRequest(
    AreaId AreaIndex,
    ushort StationIndex);
