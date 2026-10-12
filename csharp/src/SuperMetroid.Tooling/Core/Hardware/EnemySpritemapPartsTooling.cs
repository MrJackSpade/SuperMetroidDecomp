using System.Collections;

namespace SuperMetroid.Core.Hardware;

/// <summary>Development-tool instance members of <see cref="EnemySpritemapParts"/>.</summary>
internal static class EnemySpritemapPartsToolingExtensions
{
    extension(EnemySpritemapParts self)
    {
        /// <summary>Exposes the native part count to development-time sequence and bounds checks.</summary>
        public int Length => self.Count;
        /// <summary>Identifies a frame with no drawable OAM parts before tooling indexes its part list.</summary>
        public bool IsEmpty => self.Count == 0;
    }
}
