using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Verification access to <see cref="EnemySpritemapCatalog"/> members production does not use.</summary>
internal static class EnemySpritemapCatalogAccess
{
    /// <summary>Inspects the installed frame dictionary by native bank and pointer.</summary>
    extension(EnemySpritemapCatalog self)
    {
        /// <summary>Returns a known installed frame; callers must reject missing artwork.</summary>
        internal bool TryGet(byte bank, ushort pointer, out EnemySpritemapParts parts)
        {
            if (self.frames.TryGetValue(((int)new SnesAddress(bank, pointer)), out EnemySpritemapParts? found))
            {
                parts = found;
                return true;
            }
            parts = EnemySpritemapParts.Empty;
            return false;
        }
    }
}
