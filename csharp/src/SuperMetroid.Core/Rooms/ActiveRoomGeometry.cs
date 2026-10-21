using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Room-header fields shared by camera tracking and minimap publication.</summary>
/// <param name="AreaIndex">The room's area.</param>
/// <param name="MapPosition">The room's top-left area-map tile.</param>
/// <param name="UpScroller">The room's up-scroller distance in pixels, 0 through 255.</param>
/// <param name="DownScroller">The room's down-scroller distance in pixels, 0 through 255.</param>
internal readonly record struct ActiveRoomGeometry(
    AreaId AreaIndex,
    Position16<MapTiles> MapPosition,
    int UpScroller,
    int DownScroller);
