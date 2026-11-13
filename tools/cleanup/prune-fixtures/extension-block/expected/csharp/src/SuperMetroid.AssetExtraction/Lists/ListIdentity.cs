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
        internal int Weight()
        {
            int weight = 0;
            for (int list = 0; list < layout.Count; list++)
                weight += layout.Entry(list);
            return weight;
        }
    }

    /// <summary>Mixes two words into an identity.</summary>
    internal static int Mix(int high, int low) => high * 31 + low;

    /// <summary>Spreads a layout's weight over an identity.</summary>
    internal static int Spread(int weight) => weight << 4;
}
