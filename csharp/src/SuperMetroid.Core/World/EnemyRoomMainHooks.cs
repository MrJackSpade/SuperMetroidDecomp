using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.World;

/// <summary>The room main hooks over Crocomire and the camera distance and floor the room's bosses request.</summary>
/// <param name="crocomire">Crocomire.</param>
/// <param name="cameraDistance">The room camera's distance mode.</param>
/// <param name="cameraFloor">The room camera's floor.</param>
internal sealed class EnemyRoomMainHooks(CrocomireBehavior crocomire, RoomCameraDistance cameraDistance, RoomCameraFloor cameraFloor) : IEnemyRoomMainHooks
{
    /// <inheritdoc/>
    public CrocomireEnemyState? Crocomire => crocomire.Fight.Encounter;

    /// <inheritdoc/>
    public CrocomireDeathState? CrocomireDeath => crocomire.Fight.Encounter?.Death;

    /// <inheritdoc/>
    public RoomCameraFloor CameraFloor => cameraFloor;

    /// <inheritdoc/>
    public CameraDistanceMode CameraDistance => cameraDistance.Mode;
}
