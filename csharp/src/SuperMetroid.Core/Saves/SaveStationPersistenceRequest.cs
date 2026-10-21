using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Saves;

/// <summary>Confirmed save-point identity handed to the selected-slot save owner.</summary>
/// <param name="AreaIndex">The area whose load-station table holds the station.</param>
/// <param name="StationIndex">The station's zero-based load-station index within <paramref name="AreaIndex"/>.</param>
public readonly record struct SaveStationPersistenceRequest(
    AreaId AreaIndex,
    int StationIndex);
