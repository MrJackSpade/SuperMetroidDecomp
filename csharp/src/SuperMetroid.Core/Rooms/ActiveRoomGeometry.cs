using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms;

/// <summary>Room-definition fields shared by camera tracking and minimap publication.</summary>
/// <param name="Area">The room's area.</param>
/// <param name="MapPosition">The room's top-left area-map tile.</param>
/// <param name="FallingCameraOffset">How many pixels above Samus the camera aims its top while she moves any way but up, leading the view below her as she falls (#2482); zero or more.</param>
/// <param name="RisingCameraOffset">How many pixels above Samus the camera aims its top while she moves up, leading the view above her as she rises (#2482); zero or more.</param>
internal readonly partial record struct ActiveRoomGeometry(
    AreaId Area,
    Position16<MapTiles> MapPosition,
    int FallingCameraOffset,
    int RisingCameraOffset);
