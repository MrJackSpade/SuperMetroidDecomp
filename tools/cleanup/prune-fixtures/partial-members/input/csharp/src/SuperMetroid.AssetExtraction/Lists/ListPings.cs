namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>Counts of list layouts and the signals that follow them.</summary>
internal static partial class ListPings
{
    /// <summary>The number of lists production lays out.</summary>
    internal static int Count(ListLayout layout) => layout.Count;

    /// <summary>Counts the lists of <paramref name="layout"/> twice over and signals it, which only checks do.</summary>
    internal static int Recount(ListLayout layout)
    {
        Recounted();
        return Twice(layout.Count);
    }

    /// <summary>Signals that the lists were counted again, which nothing implements.</summary>
    static partial void Recounted();

    /// <summary>Twice <paramref name="value"/>.</summary>
    private static partial int Twice(int value);

    /// <summary>Twice <paramref name="value"/>.</summary>
    private static partial int Twice(int value) => value * 2;
}

/// <summary>A meter over a list.</summary>
internal sealed partial class ListMeter
{
    /// <summary>The meter's level.</summary>
    internal int Level => 3;

    /// <summary>Twice the level, which only checks read.</summary>
    internal partial int Doubled();

    internal partial int Doubled() => Level * 2;

    /// <summary>Three times the level, which only checks read.</summary>
    internal partial int Tripled { get; }

    internal partial int Tripled { get => Level * 3; }
}
