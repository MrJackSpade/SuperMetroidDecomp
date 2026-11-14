using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>The room main hooks over Crocomire, Spore Spawn and the camera distance the room's bosses request.</summary>
/// <param name="crocomire">Crocomire.</param>
/// <param name="sporeSpawn">Spore Spawn.</param>
/// <param name="cameraDistance">The room camera's distance mode.</param>
internal sealed class EnemyRoomMainHooks(CrocomireBehavior crocomire, SporeSpawnBehavior sporeSpawn, RoomCameraDistance cameraDistance) : IEnemyRoomMainHooks
{
    /// <inheritdoc/>
    public CrocomireEnemyState? Crocomire => crocomire.Access.Crocomire;

    /// <inheritdoc/>
    public CrocomireDeathState? CrocomireDeath => crocomire.Access.CrocomireDeath;

    /// <inheritdoc/>
    public void RunSporeSpawnScrollingFinishedHook(ScrollBoundaryCamera camera) => sporeSpawn.RunScrollingFinishedHook(camera);

    /// <inheritdoc/>
    public CameraDistanceMode CameraDistance => cameraDistance.Mode;
}
