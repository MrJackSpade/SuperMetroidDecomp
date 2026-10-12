using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Runtime;

/// <summary>Room-header fields shared by camera tracking and minimap publication.</summary>
internal readonly record struct ActiveRoomGeometry(
    AreaId AreaIndex,
    Position16<MapTiles> MapPosition,
    byte UpScroller,
    byte DownScroller);
