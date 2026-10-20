using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Crocomire.Artwork;

/// <summary>Every <see cref="CrocomireSkeletonVisualDefinitions"/> frame in native order, for the development audits.</summary>
internal static class CrocomireSkeletonVisualDefinitionsFramesTooling
{
    /// <summary>Projects the closed skeleton frame catalog into the sequence used by development audits.</summary>
    extension(CrocomireSkeletonVisualDefinitions)
    {
        /// <summary>Materializes every defined frame in native index order without changing its artwork.</summary>
        internal static EnemySpritemapDefinition[] Frames =>
            [.. Enumerable.Range(0, CrocomireSkeletonVisualDefinitions.FrameCount).Select(CrocomireSkeletonVisualDefinitions.Frame)];
    }
}
