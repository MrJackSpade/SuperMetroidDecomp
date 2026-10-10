namespace SuperMetroid.Core.Assets;

/// <summary>Every <see cref="CrocomireSkeletonVisualDefinitions"/> frame in native order, for the development audits.</summary>
internal static class CrocomireSkeletonVisualDefinitionsFramesTooling
{
    /// <summary>Adds the development-audit frame view to the compiled Crocomire skeleton visual catalog.</summary>
    extension(CrocomireSkeletonVisualDefinitions)
    {
        /// <summary>Gets every compiled skeleton frame in the native frame order used by Crocomire's visual program.</summary>
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, CrocomireSkeletonVisualDefinitions.FrameCount).Select(CrocomireSkeletonVisualDefinitions.Frame)];
    }
}
