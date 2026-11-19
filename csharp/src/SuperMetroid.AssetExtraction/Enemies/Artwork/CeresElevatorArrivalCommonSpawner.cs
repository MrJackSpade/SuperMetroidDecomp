using SuperMetroid.Core.Areas.Ceres;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The Ceres elevator arrival as a common spawner (#2425): its pad and platform are actors of their own, not enemy
/// projectiles, so the frame generator finds no projectile frame of theirs, and the shared sheet of the sets whose rooms
/// run the arrival holds the pictures of their poses under the arrival's own names.
/// </summary>
internal static class CeresElevatorArrivalCommonSpawner
{
    /// <summary>What the arrival puts on the shared sheet: no projectile or effect actor, the pictures of every pose.</summary>
    internal static CommonSpawnedObjects Spawned { get; } =
        new(CommonSpawner.CeresElevatorArrivalState, new SpawnedObjects([], []), Ensure.EveryMemberOnce(CeresElevatorArrivalPoseKeys.ListedOrder));
}
