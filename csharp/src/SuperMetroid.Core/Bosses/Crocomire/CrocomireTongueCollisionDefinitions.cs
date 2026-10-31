namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>
/// All nine extended frames selected by Crocomire's independently scheduled
/// tongue actor. The four fight frames refer to one empty hitbox
/// list; the five melting frames refer to another. Their sprite
/// art is deliberately absent so presentation can be edited separately.
/// </summary>
internal static class CrocomireTongueCollisionDefinitions
{
    /// <summary>
    /// Fight frames share a single physical component; melting frame identities select their pose-specific offsets
    /// and the common empty hitbox list. Sprite/artwork identity remains separate from this physical geometry.
    /// </summary>
    internal static readonly Dictionary<CrocomireTongueFrame, CrocomireTongueCollisionComponent> Components = new()
    {
        [CrocomireTongueFrame.Fight0] = new(CrocomireTongueHitboxList.Fight),
        [CrocomireTongueFrame.Fight1] = new(CrocomireTongueHitboxList.Fight),
        [CrocomireTongueFrame.Fight2] = new(CrocomireTongueHitboxList.Fight),
        [CrocomireTongueFrame.Fight3] = new(CrocomireTongueHitboxList.Fight),
        [CrocomireTongueFrame.MeltingPose0] = new(CrocomireTongueHitboxList.Melting),
        [CrocomireTongueFrame.MeltingPose1] = new(CrocomireTongueHitboxList.Melting),
        [CrocomireTongueFrame.MeltingPose2] = new(CrocomireTongueHitboxList.Melting),
        [CrocomireTongueFrame.MeltingPose3] = new(CrocomireTongueHitboxList.Melting),
        [CrocomireTongueFrame.MeltingPose4] = new(CrocomireTongueHitboxList.Melting),
    };

    /// <summary>The number of collision boxes in each tongue hitbox list: both are empty.</summary>
    internal static readonly Dictionary<CrocomireTongueHitboxList, int> HitboxCounts = new()
    {
        [CrocomireTongueHitboxList.Fight] = 0,
        [CrocomireTongueHitboxList.Melting] = 0,
    };
}
