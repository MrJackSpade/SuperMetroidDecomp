namespace SuperMetroid.Core.Assets;

/// <summary>Every <see cref="KraidFootVisualDefinitions"/> frame in native order, for the development audits.</summary>
internal static class KraidFootVisualDefinitionsFramesTooling
{
    /// <summary>Tooling-only accessors for enumerating the foot's selected native frame roots.</summary>
    extension(KraidFootVisualDefinitions)
    {
        /// <summary>Builds the selected frame definitions in index order, with the initial foot frame last.</summary>
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, KraidFootVisualDefinitions.FrameCount).Select(KraidFootVisualDefinitions.Frame)];
    }
}
