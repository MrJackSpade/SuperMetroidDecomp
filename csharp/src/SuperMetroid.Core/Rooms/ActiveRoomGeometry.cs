namespace SuperMetroid.Core.Rooms;

/// <summary>Room-header fields shared by camera tracking and minimap publication.</summary>
internal readonly record struct ActiveRoomGeometry(
    AreaId AreaIndex,
    Position16<MapTiles> MapPosition,
    byte UpScroller,
    byte DownScroller);
