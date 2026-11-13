namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The shapes of list layouts. Checks lay lists out in a shape of their own.</summary>
internal static class ListShapes
{
    /// <summary>The total production lays a layout out with.</summary>
    extension(ListLayout layout)
    {
        /// <summary>The sum of every list's entry.</summary>
        internal int Total
        {
            get
            {
                int total = 0;
                for (int list = 0; list < layout.Count; list++)
                    total += layout.Entry(list);
                return total;
            }
        }
    }
}
