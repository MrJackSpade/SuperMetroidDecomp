using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native save-marker index validity, distinct from editable drawing coordinates.</summary>
public static class MapSaveMarkerDefinitions
{
    /// <summary>$82:C80B lists retain sixteen station-index slots per area, including unused entries.</summary>
    public const int SlotsPerArea = 16;
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
                if (LoadStationDefinitions.Get(area, (byte)index).RoomPointer != 0)
                    yield return index;
        }
    }

    /// <summary>Whether a bounded station identity has an active placement and map marker.</summary>
    /// <param name="area">One of the six Zebes area identities.</param>
    /// <param name="index">Native zero-based station slot, 0..15, not the ordinal within the usable-slot sequence.</param>
    /// <returns>True for a nonzero compiled load-room pointer, independently of the player's used-station bits.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area or station slot is outside its supported range.</exception>
    public static bool IsUsable(AreaId area, int index)
    {
        ValidateArea(area);
        if ((uint)index >= SlotsPerArea) throw new ArgumentOutOfRangeException(nameof(index));
        return LoadStationDefinitions.Get(area, (byte)index).RoomPointer != 0;
    }

    /// <summary>Ensures the area has a native save-map marker list.</summary>
    /// <param name="area">Area identity to validate against the six Zebes marker lists.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area has no supported save-map marker list.</exception>
    private static void ValidateArea(AreaId area)
    {
        if ((uint)area >= FileSelectMapRomData.AreaCount)
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
    /// <param name="usedMask">Sixteen native used-station bits, with bit positions matching station slots.</param>
    /// <returns>True when at least one set bit names a usable marker; bits for unused slots do not reveal an area label.</returns>
    public static bool HasUsedMarker(AreaId area, ushort usedMask)
    {
        foreach (int index in Indices(area))
            if ((usedMask & (1 << index)) != 0) return true;
        return false;
    }
    /// <summary>Enumerates the complete stable save/elevator marker identity set for editable layout validation, independent of player discovery.</summary>
    /// <returns>Lazy marker keys in native area-ID order and ascending usable station-slot order.</returns>
    public static IEnumerable<string> AllIds()
    {
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
            foreach (int index in Indices((AreaId)area)) yield return Id((AreaId)area, index);
    }
}
