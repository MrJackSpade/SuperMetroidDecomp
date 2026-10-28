namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>Verification access to <see cref="SnesSpritemapYByte"/> members production does not use.</summary>
internal static class SnesSpritemapYByteAccess
{
    /// <summary>Exposes <see cref="SnesSpritemapYByte"/> state that only verification reads.</summary>
    extension(SnesSpritemapYByte self)
    {
        /// <summary>Whether the displacement points above the origin.</summary>
        public bool IsNegative => self.SignedOffset < 0;
    }
}
