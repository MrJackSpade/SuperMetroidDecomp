
namespace SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Verification access to <see cref="EnemySpritemapFrameList"/> members production does not use.</summary>
internal static class EnemySpritemapFrameListAccess
{
    /// <summary>Exposes <see cref="EnemySpritemapFrameList"/> state that only verification reads.</summary>
    extension(EnemySpritemapFrameList self)
    {
        /// <summary>Array-compatible length for internal catalog consumers.</summary>
        internal int Length => self.Count;
    }
}
