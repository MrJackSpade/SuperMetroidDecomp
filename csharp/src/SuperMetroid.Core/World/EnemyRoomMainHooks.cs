using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World;

/// <summary>The room main hooks over Crocomire and the camera distance and floor the room's bosses request.</summary>
/// <param name="encounters">The encounters of the room's bosses, Crocomire's among them once his body sets up (#2194).</param>
/// <param name="cameraDistance">The room camera's distance mode.</param>
/// <param name="cameraFloor">The room camera's floor.</param>
internal sealed class EnemyRoomMainHooks(RoomBossEncounters encounters, RoomCameraDistance cameraDistance, RoomCameraFloor cameraFloor) : IEnemyRoomMainHooks
{
    /// <inheritdoc/>
    public void WhenCrocomireJoins(Action<CrocomireEnemyState> joins) => encounters.WhenCrocomireJoins(joins);

    /// <inheritdoc/>
    public RoomCameraFloor CameraFloor => cameraFloor;

    /// <inheritdoc/>
    public CameraDistanceMode CameraDistance => cameraDistance.Mode;
}
