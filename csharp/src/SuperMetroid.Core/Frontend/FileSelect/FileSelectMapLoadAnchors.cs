using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Compiled player-map anchors used when the map opens, independent of editable marker artwork.</summary>
public static class FileSelectMapLoadAnchors
{
    /// <summary>Projects a valid saved station from compiled placement and room metadata.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the game's load placement and
    /// map projection. Take the whole room screens of Samus's placement at the station, add the room's map
    /// coordinate and multiply by eight; Y has one additional map row. A map square and a room's screens are small
    /// counts, so the result is an ordinary map pixel and nothing wraps. The thirty-four supported station identities retain the
    /// existing marker-validity guard.
    /// Only fixed compiled room metadata is needed, never room-state selection,
    /// game data access, editable marker art, or a generated coordinate cache.
    /// </remarks>
    public static Position16<MapPixels> Get(LoadStation station)
    {
        _ = MapSaveMarkers.Id(station);
        LoadStationPlacement placement = station.Placement;
        RoomDefinition room = placement.Room;
        Position16<RoomPixels> samus = placement.SamusPosition;
        return new((room.MapPosition.X + RoomGridConversions.PixelToScreen(samus.X)) * 8,
            (room.MapPosition.Y + RoomGridConversions.PixelToScreen(samus.Y) + 1) * 8);
    }
}
