namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>The shapes of list layouts. Checks lay lists out in <see cref="Shape{T}">a shape of their own</see>.</summary>
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

    /// <summary>The weight of a run of layouts, which only checks compare.</summary>
    extension(ListLayout[] layouts)
    {
        /// <summary>The first layout's <see cref="extension(ListLayout).Total"/> per layout, as <see cref="extension(ListLayout[]).Spread"/> spreads it.</summary>
        internal int Weight => layouts.Spread / layouts.Length;

        /// <summary>The first layout's total spread over the run.</summary>
        internal int Spread => layouts[0].Total << 2;
    }

    /// <summary>The first of an array's items, which only checks read.</summary>
    extension<T>(T[] items)
    {
        /// <summary>The first item.</summary>
        internal T Head => items[0];
    }

    /// <summary>A shape the checks lay a list out in.</summary>
    /// <typeparam name="T">The kind of entry.</typeparam>
    internal sealed class Shape<T>
    {
        /// <summary>The shape's size: the number of entries of <see cref="Kind"/>.</summary>
        internal int Size { get; } = 2;

        /// <summary>The kind of entry, which <see cref="Size"/> counts.</summary>
        internal string Kind { get; } = typeof(T).Name;
    }

    /// <summary>The seed of a layout of <paramref name="lists"/> lists, which only checks compute.</summary>
    internal static int Seed(int lists) => lists * 7;
}
