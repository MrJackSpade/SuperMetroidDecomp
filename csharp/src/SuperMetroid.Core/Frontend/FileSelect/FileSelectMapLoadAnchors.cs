using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The player-map anchors used when the map opens, independent of editable marker artwork.</summary>
public static class FileSelectMapLoadAnchors
{
    /// <summary>Projects a valid saved station from its placement and the room definitions.</summary>
    /// <remarks>
    /// Take the whole room screens of Samus's placement at the station, add the room's map
    /// cell and scale by a map square's pixels. A map square and a room's screens are small
    /// counts, so the result is an ordinary map pixel and nothing wraps. The thirty-four supported station identities retain the
    /// existing marker-validity guard.
    /// Only fixed room definitions are needed, never room-state selection,
    /// game data access, editable marker art, or a generated coordinate cache.
    /// </remarks>
    public static Position16<MapPixels> Get(LoadStation station)
    {
        if (!MapSaveMarkers.IsMarked(station))
            throw new ArgumentOutOfRangeException(nameof(station), station, "The station has no save-map marker.");
        LoadStationPlacement placement = station.Placement;
        RoomDefinition room = placement.Door.DestinationRoom;
        Position16<RoomPixels> samus = placement.SamusPosition;
        return new((room.MapPosition.X + RoomGridConversions.PixelToScreen(samus.X)) * FileSelectMapDefinitions.MapSquarePixels,
            (room.MapPosition.Y + RoomGridConversions.PixelToScreen(samus.Y)) * FileSelectMapDefinitions.MapSquarePixels);
    }
}
