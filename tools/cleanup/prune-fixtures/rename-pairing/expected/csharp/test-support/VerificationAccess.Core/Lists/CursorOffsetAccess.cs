namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Verification access to <see cref="CursorOffset"/> members production does not use.</summary>
internal static class CursorOffsetAccess
{
    /// <summary>Exposes <see cref="CursorOffset"/> state that only verification reads.</summary>
    extension(CursorOffset self)
    {
        /// <summary>The columns alone, which only checks read.</summary>
        internal int Columns => self._xOffset;
    }
}
