using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World;

/// <summary>The enemy state and hooks the room main behaviours and scrolling read or run.</summary>
internal interface IEnemyRoomMainHooks
{
    /// <summary>Has <paramref name="joins"/> run with the room's Crocomire as his body sets up (#2554).</summary>
    void WhenCrocomireJoins(Action<CrocomireEnemyState> joins);

    /// <summary>The lowest Y the room's camera may take, as the room's bosses hold it.</summary>
    RoomCameraFloor CameraFloor { get; }

    /// <summary>How far ahead of Samus the room's camera aims, as the room's bosses requested it.</summary>
    CameraDistanceMode CameraDistance { get; }
}
