namespace SuperMetroid.Desktop.Lists;

/// <summary>Cartridge values and helpers of <see cref="ListPings"/> that only verification reads.</summary>
internal static partial class ListPingsConstants
{
    /// <summary>Counts the lists of <paramref name="layout"/> twice over and signals it, which only checks do.</summary>
    internal static int Recount(ListLayout layout)
    {
        Recounted();
        return Twice(layout.Count);
    }
    /// <summary>Signals that the lists were counted again, which nothing implements.</summary>
    static partial void Recounted();
    /// <summary>Twice <paramref name="value"/>.</summary>
    internal static partial int Twice(int value);
    /// <summary>Twice <paramref name="value"/>.</summary>
    internal static partial int Twice(int value) => value * 2;
}
