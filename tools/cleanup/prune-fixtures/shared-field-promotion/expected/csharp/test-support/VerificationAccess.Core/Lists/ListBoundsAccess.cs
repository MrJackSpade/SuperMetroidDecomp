namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Verification access to <see cref="ListBounds"/> members production does not use.</summary>
internal static class ListBoundsAccess
{
    /// <summary>Exposes <see cref="ListBounds"/> state that only verification reads.</summary>
    extension(ListBounds self)
    {
        /// <summary>The list's first entry, which only checks read.</summary>
        internal int Start => self.first;
    }
}
