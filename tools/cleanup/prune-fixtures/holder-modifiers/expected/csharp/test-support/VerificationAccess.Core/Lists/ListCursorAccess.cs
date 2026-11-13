namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Verification access to <see cref="ListCursor"/> members production does not use.</summary>
internal static class ListCursorAccess
{
    /// <summary>Exposes <see cref="ListCursor"/> state that only verification reads.</summary>
    extension(ListCursor self)
    {
        /// <summary>Moves back to the first entry, which only checks do.</summary>
        internal void Rewind() => self.Index = 0;
    }
}
