namespace SuperMetroid.Core.Assets;

/// <summary>Every <see cref="CrocomireSkeletonVisualDefinitions"/> frame in native order, for the development audits.</summary>
internal static class CrocomireSkeletonVisualDefinitionsFramesTooling
{
    /// <summary>Exposes Crocomire skeleton frames in native order for tooling audits.</summary>
    extension(CrocomireSkeletonVisualDefinitions)
    {
        /// <summary>Gets frames.</summary>
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, CrocomireSkeletonVisualDefinitions.FrameCount).Select(CrocomireSkeletonVisualDefinitions.Frame)];
    }
}
