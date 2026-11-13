namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The identities of list layouts and of their lists, which the import pipeline compares.</summary>
internal static class ListIdentity
{
    /// <summary>The identity of a <see cref="ListLayout"/>.</summary>
    extension(ListLayout layout)
    {
        /// <summary>A layout's identity: its list count mixed with its spread weight.</summary>
        internal int Identity => Mix(layout.Count, Spread(layout.Weight()));

        /// <summary>The sum of every list's entry.</summary>
        private int Weight()
        {
            int weight = 0;
            for (int list = 0; list < layout.Count; list++)
                weight += layout.Entry(list);
            return weight;
        }
    }

    /// <summary>The identity of a <see cref="ListEntry"/>, which only checks compare.</summary>
    extension(ListEntry entry)
    {
        /// <summary>An entry's identity: its index mixed with its seed, and its layout's spread weight.</summary>
        public int Identity => Mix(entry.Index, entry.Seed()) + ListIdentity.Spread(entry.Layout.Weight());

        /// <summary>The list's entry, the seed of its identity.</summary>
        int Seed() => entry.Layout.Entry(entry.Index);
    }

    /// <summary>Mixes two words into an identity.</summary>
    private static int Mix(int high, int low) => high * 31 + low;

    /// <summary>Spreads a layout's weight over an identity.</summary>
    private static int Spread(int weight) => weight << 4;
}
