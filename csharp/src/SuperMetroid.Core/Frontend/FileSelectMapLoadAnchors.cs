using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>Compiled player-map anchors used by $82:9028, independent of editable marker artwork.</summary>
public static class FileSelectMapLoadAnchors
{
    /// <summary>Projects a valid saved station from compiled placement and room metadata.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against native $80:C437 load placement and
    /// $82:9040/$82:9098 map projection. First wrap camera plus player offset to a
    /// sixteen-bit world coordinate (including the horizontal128 bias), then take
    /// its high byte. Add the room map coordinate and multiply by eight; Y has one
    /// additional map row. The thirty-four supported station identities retain the
    /// existing marker-validity guard before narrowing the station index to a byte.
    /// Only fixed compiled room metadata is needed, never room-state selection,
    /// cartridge access, editable marker art, or a generated coordinate cache.
    /// </remarks>
    public static FileSelectMapAnchor Get(AreaId area, int stationIndex)
    {
        _ = MapSaveMarkerDefinitions.Id(area, stationIndex);
        LoadStationEntry station = LoadStationDefinitions.Get(area, (byte)stationIndex);
        RoomHeaderDefinition room = RoomHeaderDefinitions.Get(station.RoomPointer);
        return new((ushort)((room.MapX + (station.SamusX >> 8)) << 3),
            (ushort)((room.MapY + (station.SamusY >> 8) + 1) << 3));
    }
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
