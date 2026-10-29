using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Development-tool instance members of <see cref="EnemySpritemapParts"/>.</summary>
internal static class EnemySpritemapPartsToolingExtensions
{
    /// <summary>Adapts enemy composition parts to tooling sequence and empty-frame checks.</summary>
    extension(EnemySpritemapParts self)
    {
        /// <summary>Exposes the native part count to development-time sequence and bounds checks.</summary>
        public int Length => self.Count;
        /// <summary>Identifies a frame with no drawable parts before tooling indexes its part list.</summary>
        public bool IsEmpty => self.Count == 0;
    }
}
