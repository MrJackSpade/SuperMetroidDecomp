namespace SuperMetroid.Core.Assets;

/// <summary>Every <see cref="CrocomireSkeletonVisualDefinitions"/> frame in native order, for the development audits.</summary>
internal static class CrocomireSkeletonVisualDefinitionsFramesTooling
{
    extension(CrocomireSkeletonVisualDefinitions)
    {
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, CrocomireSkeletonVisualDefinitions.FrameCount).Select(CrocomireSkeletonVisualDefinitions.Frame)];
    }
}
