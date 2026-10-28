using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>
/// What an extended enemy frame may hold (<see cref="EnemyExtendedFrameDefinitions"/>): the Phantoon and Draygon poses
/// drawn only on BG2, and the component bound that Crocomire's fragmenting skeleton raises.
/// </summary>
internal static class EnemyExtendedFrameBounds
{
    /// <summary>These frames contain BG2 streams only; stock legitimately has no OAM components.</summary>
    internal static bool IsBg2Only(EnemySpritemapDefinition frame) =>
        PhantoonBg2FrameDefinitions.FrameIds.Contains(frame.Frame) || DraygonBg2FrameDefinitions.FrameIds.Contains(frame.Frame);

    /// <summary>Skeleton fragmentation needs thirteen components; all other sprite families retain their existing bound.</summary>
    internal static int MaximumSpriteComponents(EnemySpritemapDefinition frame) =>
        CrocomireSkeletonVisualDefinitions.IsFrame(frame.Frame)
            ? CrocomireSkeletonVisualDefinitions.MaximumComponents : EnemyExtendedFrameDefinitions.MaximumComponents;
}
