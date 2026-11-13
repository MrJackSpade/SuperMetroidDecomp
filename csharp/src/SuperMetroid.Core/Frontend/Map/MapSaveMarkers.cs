using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Decides which stations of a Zebes area have saved-game map markers, names them, and reports whether any marked station
/// has been used. Every station Samus uses in a Zebes area has a marker; Ceres has none.
/// </summary>
public static class MapSaveMarkers
{
    /// <summary>The stations of <paramref name="area"/> that have markers, in the order the load stations are declared.</summary>
    /// <param name="area">One of the six Zebes areas; Ceres has no save-map marker list.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not one of the six Zebes areas; rejection occurs before enumeration.</exception>
    public static IEnumerable<LoadStation> Stations(AreaId area)
    {
        ValidateArea(area);
        return LoadStationDefinitions.Usable.Where(station => station.Area == area);
    }

    /// <summary>Whether <paramref name="station"/> has a map marker.</summary>
    public static bool IsMarked(LoadStation station) =>
        AreaIds.IsZebes(Ensure.NotNull(station).Area) && LoadStationDefinitions.Usable.Contains(station);

    /// <summary>Rejects Ceres and undefined areas because only the six Zebes areas own marker lists.</summary>
    private static void ValidateArea(AreaId area)
    {
        if (!AreaIds.IsZebes(area))
            throw new ArgumentOutOfRangeException(nameof(area), area, "Only Zebes areas have save-map markers.");
    }

    /// <summary>The stable editable marker key of a marked station, without selecting its drawing coordinates or load target.</summary>
    /// <param name="station">A station with a map marker.</param>
    /// <returns>The station's name.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The station has no map marker.</exception>
    public static string Id(LoadStation station) => IsMarked(station)
        ? station.Name
        : throw new ArgumentOutOfRangeException(nameof(station), station, "The station has no save-map marker.");

    /// <summary>Whether the area-select map labels the area: at least one of its marked stations has been used.</summary>
    /// <param name="area">One of the six Zebes areas.</param>
    /// <param name="usedStations">The save stations and elevators Samus has used.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area is not one of the six Zebes areas.</exception>
    public static bool HasUsedMarker(AreaId area, ProgressSet<LoadStation> usedStations)
    {
        Ensure.NotNull(usedStations);
        return Stations(area).Any(usedStations.Contains);
    }
}
