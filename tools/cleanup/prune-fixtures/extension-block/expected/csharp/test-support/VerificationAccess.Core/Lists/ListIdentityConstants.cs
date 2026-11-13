namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Cartridge values and helpers of <see cref="ListIdentity"/> that only verification reads.</summary>
internal static class ListIdentityConstants
{
    /// <summary>The identity of a <see cref="ListEntry"/>, which only checks compare.</summary>
    extension(ListEntry entry)
    {
        /// <summary>An entry's identity: its index mixed with its seed, and its layout's spread weight.</summary>
        public int Identity => ListIdentity.Mix(entry.Index, entry.Seed()) + ListIdentity.Spread(entry.Layout.Weight());

        /// <summary>The list's entry, the seed of its identity.</summary>
        internal int Seed() => entry.Layout.Entry(entry.Index);
    }
}
