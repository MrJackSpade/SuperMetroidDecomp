namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Verification access to <see cref="StridedList"/> members production does not use.</summary>
internal static class StridedListAccess
{
    /// <summary>Exposes <see cref="StridedList"/> state that only verification reads.</summary>
    extension(StridedList self)
    {
        /// <summary>The entry the list's third item is at, which only checks read.</summary>
        internal int Third => self.stride * 2;
    }
}
