using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// What one common spawner, code outside the enemy behaviours, spawns onto the shared sheet of the sets whose rooms run it
/// (#1513).
/// </summary>
/// <param name="Spawner">
/// The spawner, named for the Core type whose code spawns, or for a type and member where a type's members serve unrelated
/// rooms. <see cref="CommonSpawnerPresenceDefinitions"/> names the rooms each one runs in.
/// </param>
/// <param name="Objects">The projectile frames and effect actors it spawns.</param>
/// <param name="ElevatorArrivalPoses">
/// The Ceres elevator arrival's poses whose pictures it shows (#2425), the arrival's pad and platform being actors of
/// their own rather than projectiles; none for any other spawner.
/// </param>
internal sealed partial record CommonSpawnedObjects(CommonSpawner Spawner, SpawnedObjects Objects, IReadOnlyList<CeresElevatorArrivalPose>? ElevatorArrivalPoses = null)
{
    /// <summary>The Ceres elevator arrival's poses whose pictures the spawner shows, none for any spawner but the arrival.</summary>
    internal IReadOnlyList<CeresElevatorArrivalPose> ArrivalPoses => ElevatorArrivalPoses ?? [];
}
