namespace SuperMetroid.Desktop.Lists;

/// <summary>A kind of list and the width it is laid out at.</summary>
internal class ListKind
{
    /// <summary>The width production lays a list of this kind out at.</summary>
    internal int Width { get; } = 8;
}

/// <summary>A kind of list whose entries are wider.</summary>
internal sealed class WideListKind : ListKind
{
}

/// <summary>Where a list starts and how many entries it holds.</summary>
internal struct ListSpan
{
    /// <summary>A list of <paramref name="length"/> entries from <paramref name="start"/>.</summary>
    /// <param name="start">The list's first entry.</param>
    /// <param name="length">The number of entries.</param>
    internal ListSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }

    /// <summary>The list's first entry.</summary>
    internal int Start { get; }

    /// <summary>The number of entries.</summary>
    internal int Length { get; }
}

/// <summary>A place in a list that production advances.</summary>
internal sealed class ListCursor
{
    /// <summary>The entry the cursor stands on.</summary>
    internal int Index { get; private set; }

    /// <summary>Moves to the next entry.</summary>
    internal void Advance() => Index++;

    /// <summary>Moves back to the first entry, which only checks do.</summary>
    internal void Rewind() => Index = 0;
}

/// <summary>Entry arithmetic.</summary>
internal static partial class ListArithmetic
{
    /// <summary>Twice <paramref name="value"/>, as production lays entries out.</summary>
    internal static int Double(int value) => Twice(value);

    /// <summary>Twice <paramref name="value"/>.</summary>
    private static partial int Twice(int value);

    /// <summary>Twice <paramref name="value"/>.</summary>
    private static partial int Twice(int value) => value * 2;
}
