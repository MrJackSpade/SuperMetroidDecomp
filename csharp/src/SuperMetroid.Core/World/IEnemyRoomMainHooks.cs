using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World;

/// <summary>The enemy state and hooks the room main behaviours and scrolling read or run.</summary>
internal interface IEnemyRoomMainHooks
{
    /// <summary>The room's Crocomire once his body has set up; none in a room without him.</summary>
    CrocomireEnemyState? Crocomire { get; }

    /// <summary>The lowest Y the room's camera may take, as the room's bosses hold it.</summary>
    RoomCameraFloor CameraFloor { get; }

    /// <summary>How far ahead of Samus the room's camera aims, as the room's bosses requested it.</summary>
    CameraDistanceMode CameraDistance { get; }
}
