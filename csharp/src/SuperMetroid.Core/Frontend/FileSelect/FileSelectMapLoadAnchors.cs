using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

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
    public static Position16<MapPixels> Get(AreaId area, int stationIndex)
    {
        _ = MapSaveMarkers.Id(area, stationIndex);
        LoadStationEntry station = LoadStations.Get(area, (byte)stationIndex);
        RoomHeaderDefinition room = station.Room ??
            throw new InvalidOperationException($"{area} station {stationIndex} has no room.");
        Position16<RoomPixels> samus = station.SamusPosition;
        return new((ushort)((room.MapPosition.X + RoomGridConversions.PixelToScreen(samus.X)) << 3),
            (ushort)((room.MapPosition.Y + RoomGridConversions.PixelToScreen(samus.Y) + 1) << 3));
    }
}
