using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The room main hooks over Crocomire, Spore Spawn and the Zebetites.</summary>
/// <param name="crocomire">Crocomire.</param>
/// <param name="sporeSpawn">Spore Spawn.</param>
/// <param name="zebetites">The Zebetites.</param>
internal sealed class EnemyRoomMainHooks(CrocomireBehavior crocomire, SporeSpawnBehavior sporeSpawn, ZebetiteBehavior zebetites) : IEnemyRoomMainHooks
{
    /// <inheritdoc/>
    public CrocomireEnemyState? Crocomire => crocomire.Access.Crocomire;

    /// <inheritdoc/>
    public CrocomireDeathState? CrocomireDeath => crocomire.Access.CrocomireDeath;

    /// <inheritdoc/>
    public void RunSporeSpawnScrollingFinishedHook(ScrollBoundaryCamera camera) => sporeSpawn.RunScrollingFinishedHook(camera);

    /// <inheritdoc/>
    public CameraDistanceMode ZebetiteCameraDistanceIndex => zebetites.CameraDistanceIndex;
}
