namespace SuperMetroid.Desktop.Lists;

/// <summary>Verification access to <see cref="ListSpan"/> members production does not use.</summary>
internal static class ListSpanAccess
{
    /// <summary>Exposes <see cref="ListSpan"/> state that only verification reads.</summary>
    extension(ListSpan self)
    {
        /// <summary>The entry just past the list, which only checks read.</summary>
        internal int End() => self.Start + self.Length;
        /// <summary>The list's last entry, which only checks read.</summary>
        internal int Last => self.Start + self.Length - 1;
    }
}
