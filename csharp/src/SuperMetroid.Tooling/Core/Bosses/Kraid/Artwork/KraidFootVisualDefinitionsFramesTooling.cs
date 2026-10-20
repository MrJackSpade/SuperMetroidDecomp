using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Bosses.Kraid.Artwork;

/// <summary>Every <see cref="KraidFootVisualDefinitions"/> frame in native order, for the development audits.</summary>
internal static class KraidFootVisualDefinitionsFramesTooling
{
    /// <summary>Adds the audit-only frame enumeration to the Kraid foot visual catalog.</summary>
    extension(KraidFootVisualDefinitions)
    {
        /// <summary>Materializes every catalog frame in native index order for development audits.</summary>
        internal static EnemySpritemapDefinition[] Frames =>
            [.. Enumerable.Range(0, KraidFootVisualDefinitions.FrameCount).Select(KraidFootVisualDefinitions.Frame)];
    }
}
