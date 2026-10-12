using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Runtime;

/// <summary>Final native door-scroll camera and 16.16 Samus coordinates.</summary>
internal readonly record struct DoorTransitionPlacement(
    Position16<RoomPixels> CameraPosition,
    FixedPosition<RoomPixels> SamusPosition);
