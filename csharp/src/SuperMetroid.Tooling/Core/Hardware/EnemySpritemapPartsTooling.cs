using System.Collections;

namespace SuperMetroid.Core.Hardware;

/// <summary>Development-tool instance members of <see cref="EnemySpritemapParts"/>.</summary>
internal static class EnemySpritemapPartsToolingExtensions
{
    extension(EnemySpritemapParts self)
    {
        public int Length => self.Count;
        public bool IsEmpty => self.Count == 0;
    }
}
