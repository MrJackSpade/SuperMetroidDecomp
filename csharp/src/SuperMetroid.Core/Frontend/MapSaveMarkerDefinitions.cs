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
    public static bool IsUsable(AreaId area, int index)
    {
        ValidateArea(area);
        if ((uint)index >= SlotsPerArea) throw new ArgumentOutOfRangeException(nameof(index));
        return LoadStationDefinitions.Get(area, (byte)index).RoomPointer != 0;
    }

    private static void ValidateArea(AreaId area)
    {
        if ((uint)area >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area), "Only Zebes areas have save-map markers.");
    }
    public static string Id(AreaId area, int index)
    {
        if (!IsUsable(area, index)) throw new InvalidDataException($"Area {area} has no map coordinate for station {index}.");
        return $"{area}.Save.{index}";
    }
    /// <summary>$81:A97E DrawAreaSelectMapLabels: at least one used, non-unused save-coordinate slot.</summary>
    public static bool HasUsedMarker(AreaId area, ushort usedMask)
    {
        foreach (int index in Indices(area))
            if ((usedMask & (1 << index)) != 0) return true;
        return false;
    }
    public static IEnumerable<string> AllIds()
    {
        for (int area = 0; area < FileSelectMapRomData.AreaCount; area++)
            foreach (int index in Indices((AreaId)area)) yield return Id((AreaId)area, index);
    }
}
