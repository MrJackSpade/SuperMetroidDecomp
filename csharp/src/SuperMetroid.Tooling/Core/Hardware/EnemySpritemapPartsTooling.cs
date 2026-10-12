using System.Collections;

namespace SuperMetroid.Core.Hardware;

/// <summary>Development-tool instance members of <see cref="EnemySpritemapParts"/>.</summary>
internal static class EnemySpritemapPartsToolingExtensions
{
    /// <summary>Provides collection-style operations for enemy spritemap parts.</summary>
    extension(EnemySpritemapParts self)
    {
        /// <summary>Returns the number of spritemap parts.</summary>
        public int Length => self.Count;
        /// <summary>Reports whether the spritemap has no parts.</summary>
        public bool IsEmpty => self.Count == 0;
    }
}
