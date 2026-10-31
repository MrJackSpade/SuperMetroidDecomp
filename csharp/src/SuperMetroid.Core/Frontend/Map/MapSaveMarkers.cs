using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Progression;
using static SuperMetroid.Core.Frontend.Map.MapSaveMarkerDefinitions;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Decides which save stations of a Zebes area have map markers (<see cref="MapSaveMarkerDefinitions"/>), names them, and
/// reports whether any marked station has been used.
/// </summary>
public static class MapSaveMarkers
{
    /// <summary>Enumerates usable save/elevator identities in native slot order.</summary>
    /// <remarks>
    /// Independently reviewed for #1165: each of the six original sixteen-slot marker
    /// views at $82:C80B has a usable coordinate exactly when the corresponding compiled
    /// load-station record has a nonzero room pointer. Unused slots have FFFE coordinates
    /// and a zero-room placement. Enumerate that existing semantic definition rather
    /// than store a second list of station identities. Validate the area immediately,
    /// before returning the lazy sequence, preserving the previous span API's rejection.
    /// </remarks>
    /// <param name="area">One of the six Zebes area identities; Ceres has no save-map marker list.</param>
    /// <returns>A lazy ascending sequence of station slots whose compiled load placement has a nonzero room pointer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area is outside the six supported identities; rejection occurs before enumeration.</exception>
    public static IEnumerable<int> Indices(AreaId area)
    {
        ValidateArea(area);
        return Enumerate();

        IEnumerable<int> Enumerate()
        {
            for (int index = 0; index < SlotsPerArea; index++)
                if (LoadStations.Get(area, (byte)index).Room is not null)
                    yield return index;
        }
    }

    /// <summary>Whether a bounded station identity has an active placement and map marker.</summary>
    /// <param name="area">One of the six Zebes area identities.</param>
    /// <param name="index">Native zero-based station slot, 0..15, not the ordinal within the usable-slot sequence.</param>
    /// <returns>True for a slot with a compiled load room, independently of the player's used-station bits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area or station slot is outside its supported range.</exception>
    public static bool IsUsable(AreaId area, int index)
    {
        ValidateArea(area);
        if ((uint)index >= SlotsPerArea) throw new ArgumentOutOfRangeException(nameof(index));
        return LoadStations.Get(area, (byte)index).Room is not null;
    }

    /// <summary>Rejects Ceres and undefined areas because only the six Zebes areas own marker tables.</summary>
    private static void ValidateArea(AreaId area)
    {
        if (!AreaIds.IsZebes(area))
            throw new ArgumentOutOfRangeException(nameof(area), "Only Zebes areas have save-map markers.");
    }
    /// <summary>Formats the stable editable marker key for a usable native save/elevator station, without selecting its drawing coordinates or load target.</summary>
    /// <param name="area">One of the six Zebes area identities.</param>
    /// <param name="index">Native zero-based station slot, 0..15.</param>
    /// <returns>The key formatted as the area name followed by <c>.Save.</c> and the station slot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area or station slot is outside its supported range.</exception>
    /// <exception cref="InvalidDataException">The bounded station slot has no usable placement.</exception>
    public static string Id(AreaId area, int index)
    {
        if (!IsUsable(area, index)) throw new InvalidDataException($"Area {area} has no map coordinate for station {index}.");
        return $"{area}.Save.{index}";
    }
    /// <summary>$81:A97E DrawAreaSelectMapLabels: at least one used, non-unused save-coordinate slot.</summary>
    /// <param name="area">One of the six Zebes area identities.</param>
    /// <param name="usedStations">The area's used save stations and elevators, by station slot.</param>
    /// <returns>True when a used station has a usable marker; used unused slots do not reveal an area label.</returns>
    public static bool HasUsedMarker(AreaId area, NumberedFlags usedStations)
    {
        Ensure.NotNull(usedStations);
        foreach (int index in Indices(area))
            if (usedStations.Contains(index)) return true;
        return false;
    }
    /// <summary>Enumerates the complete stable save/elevator marker identity set for editable layout validation, independent of player discovery.</summary>
    /// <returns>Lazy marker keys in native area-ID order and ascending usable station-slot order.</returns>
    public static IEnumerable<string> AllIds()
    {
        foreach (AreaId area in AreaIds.Zebes)
            foreach (int index in Indices(area)) yield return Id(area, index);
    }
}
