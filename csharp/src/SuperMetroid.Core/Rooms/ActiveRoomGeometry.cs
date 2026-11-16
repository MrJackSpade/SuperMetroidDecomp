using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Room-definition fields shared by camera tracking and minimap publication.</summary>
/// <param name="Area">The room's area.</param>
/// <param name="MapPosition">The room's top-left area-map tile.</param>
/// <param name="UpScroller">The camera's vertical target offset while Samus moves down, in pixels, zero or more.</param>
/// <param name="DownScroller">The camera's vertical target offset while Samus moves up, in pixels, zero or more.</param>
internal readonly record struct ActiveRoomGeometry(
    AreaId Area,
    Position16<MapTiles> MapPosition,
    int UpScroller,
    int DownScroller);
