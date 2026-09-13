using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Compiled player-map anchors used by $82:9028, independent of editable marker artwork.</summary>
public static class FileSelectMapLoadAnchors
{
    /// <summary>$80:C4C5 LoadStations_Crateria plus each $8F room's map origin, projected by $82:9040/$82:9098.</summary>
    private static readonly Entry[] crateria = [new(0, 216, 40), new(1, 144, 56), new(8, 416, 88),
        new(9, 272, 64), new(10, 184, 144), new(11, 48, 72), new(12, 136, 80)];
    /// <summary>$80:C5CF LoadStations_Brinstar and native room origins; slots retain unused gaps.</summary>
    private static readonly Entry[] brinstar = [new(0, 120, 40), new(1, 64, 48), new(2, 40, 96),
        new(3, 392, 152), new(4, 304, 72), new(8, 72, 24), new(9, 208, 88), new(10, 296, 56), new(11, 328, 152)];
    /// <summary>$80:C6D9 LoadStations_Norfair and native room origins, including lower-Norfair elevators.</summary>
    private static readonly Entry[] norfair = [new(0, 96, 96), new(1, 168, 32), new(2, 88, 48),
        new(3, 128, 72), new(4, 160, 88), new(5, 288, 104), new(8, 80, 24), new(9, 168, 88), new(10, 168, 112)];
    /// <summary>$80:C81B LoadStations_WreckedShip and its save-room map origin.</summary>
    private static readonly Entry[] wreckedShip = [new(0, 136, 120)];
    /// <summary>$80:C917 LoadStations_Maridia and native room origins.</summary>
    private static readonly Entry[] maridia = [new(0, 96, 160), new(1, 280, 40), new(2, 152, 96), new(3, 328, 56), new(8, 272, 24)];
    /// <summary>$80:CA2F LoadStations_Tourian and native room origins.</summary>
    private static readonly Entry[] tourian = [new(0, 128, 144), new(1, 168, 104), new(8, 160, 96)];

    /// <summary>Resolves the saved station's initial visibility anchor without reading room definitions or selecting a room state.</summary>
    public static FileSelectMapAnchor Get(AreaId area, int stationIndex)
    {
        _ = MapSaveMarkerDefinitions.Id(area, stationIndex);
        ReadOnlySpan<Entry> entries = area switch
        {
            AreaId.Crateria => crateria, AreaId.Brinstar => brinstar, AreaId.Norfair => norfair,
            AreaId.WreckedShip => wreckedShip, AreaId.Maridia => maridia, AreaId.Tourian => tourian,
            _ => throw new ArgumentOutOfRangeException(nameof(area))
        };
        foreach (var entry in entries)
            if (entry.Station == stationIndex) return new(entry.X, entry.Y);
        throw new InvalidOperationException($"Missing compiled map anchor for {area} station {stationIndex}.");
    }
    private readonly record struct Entry(int Station, ushort X, ushort Y);
}

/// <summary>Player-map position supplied to native initial-scroll clipping, not a marker drawing origin.</summary>
public readonly record struct FileSelectMapAnchor(ushort X, ushort Y);

/// <summary>Compiled $81:AAA0 FileSelectMapArea_IndexTable order used by label composition.</summary>
public static class FileSelectMapAreaOrder
{
    private static readonly AreaId[] areas = [AreaId.Crateria, AreaId.WreckedShip, AreaId.Tourian,
        AreaId.Brinstar, AreaId.Maridia, AreaId.Norfair];
    public static AreaId Get(int displayIndex) => (uint)displayIndex < areas.Length
        ? areas[displayIndex] : throw new ArgumentOutOfRangeException(nameof(displayIndex));
}
