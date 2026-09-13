using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Native save-marker index validity, distinct from editable drawing coordinates.</summary>
public static class MapSaveMarkerDefinitions
{
    /// <summary>$82:C80B lists retain sixteen station-index slots per area, including unused entries.</summary>
    public const int SlotsPerArea = 16;
    /// <summary>$82:C853: Crateria's usable save/elevator marker indices.</summary>
    private static readonly int[] crateria = [0, 1, 8, 9, 10, 11, 12];
    /// <summary>$82:C8BD: Brinstar's usable save/elevator marker indices.</summary>
    private static readonly int[] brinstar = [0, 1, 2, 3, 4, 8, 9, 10, 11];
    /// <summary>$82:C923: Norfair's usable save/elevator marker indices.</summary>
    private static readonly int[] norfair = [0, 1, 2, 3, 4, 5, 8, 9, 10];
    /// <summary>$82:C991: Wrecked Ship's sole usable save marker.</summary>
    private static readonly int[] wreckedShip = [0];
    /// <summary>$82:C9F3: Maridia's usable save/elevator marker indices.</summary>
    private static readonly int[] maridia = [0, 1, 2, 3, 8];
    /// <summary>$82:CA51: Tourian's usable save/elevator marker indices.</summary>
    private static readonly int[] tourian = [0, 1, 8];

    public static ReadOnlySpan<int> Indices(AreaId area) => area switch
    {
        AreaId.Crateria => crateria, AreaId.Brinstar => brinstar, AreaId.Norfair => norfair,
        AreaId.WreckedShip => wreckedShip, AreaId.Maridia => maridia, AreaId.Tourian => tourian,
        _ => throw new ArgumentOutOfRangeException(nameof(area), "Only Zebes areas have save-map markers.")
    };
    public static string Id(AreaId area, int index)
    {
        var indices = Indices(area);
        if ((uint)index >= SlotsPerArea) throw new ArgumentOutOfRangeException(nameof(index));
        if (!indices.Contains(index)) throw new InvalidDataException($"Area {area} has no map coordinate for station {index}.");
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
            foreach (int index in Indices((AreaId)area).ToArray()) yield return Id((AreaId)area, index);
    }
}
