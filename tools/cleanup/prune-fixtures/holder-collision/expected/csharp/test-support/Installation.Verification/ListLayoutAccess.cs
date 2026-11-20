namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Verification access to <see cref="ListLayout"/> reads production does not make.</summary>
internal static class ListLayoutAccess
{
    /// <summary>The list positions checks compare with.</summary>
    extension(ListLayout self)
    {
        /// <summary>The position just after list <paramref name="list"/>'s entry.</summary>
        internal int Second(int list) => self.Position(list) + 1;
        /// <summary>The word just before list <paramref name="list"/>.</summary>
        internal int Prefix(int list) => self._prefixes[list];
    }
}
