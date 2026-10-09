using System.Collections;

namespace SuperMetroid.Core.Hardware;

/// <summary>Development-tool instance members of <see cref="EnemySpritemapParts"/>.</summary>
internal static class EnemySpritemapPartsToolingExtensions
{
    /// <summary>Exposes collection-style inspection of an enemy spritemap's parts for development tools.</summary>
    extension(EnemySpritemapParts self)
    {
        /// <summary>Gets the number of parts in the spritemap.</summary>
        public int Length => self.Count;

        /// <summary>Gets whether the spritemap contains no parts.</summary>
        public bool IsEmpty => self.Count == 0;
    }
}
