namespace SuperMetroid.Core.Assets;

/// <summary>Every <see cref="KraidFootVisualDefinitions"/> frame in native order, for the development audits.</summary>
internal static class KraidFootVisualDefinitionsFramesTooling
{
    /// <summary>Provides tooling helpers for Kraid foot visual definitions.</summary>
    extension(KraidFootVisualDefinitions)
    {
        /// <summary>Gets frames.</summary>
        internal static EnemyExtendedFrameDefinition[] Frames =>
            [.. Enumerable.Range(0, KraidFootVisualDefinitions.FrameCount).Select(KraidFootVisualDefinitions.Frame)];
    }
}
