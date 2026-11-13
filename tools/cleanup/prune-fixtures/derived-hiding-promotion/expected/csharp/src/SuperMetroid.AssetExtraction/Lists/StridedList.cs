namespace SuperMetroid.AssetExtraction.Lists;

/// <summary>A list whose items lie a stride apart.</summary>
internal class StridedList
{
    /// <summary>The entries from one of the list's items to the next.</summary>
    private readonly int stride;

    /// <summary>A list whose items lie <paramref name="stride"/> entries apart.</summary>
    /// <param name="stride">The entries from one of the list's items to the next.</param>
    internal StridedList(int stride)
    {
        this.stride = stride;
    }

    /// <summary>The entry the list's second item is at, which production reads.</summary>
    internal int Second => stride;

    /// <summary>The entry the list's third item is at, which only checks read.</summary>
    internal int Third => stride * 2;
}

/// <summary>A list whose items lie four entries apart.</summary>
internal sealed class WideList : StridedList
{
    /// <summary>A list whose items lie four entries apart.</summary>
    internal WideList()
        : base(4)
    {
    }

    /// <summary>The stride under the name the wide list's catalog gives it, which production reads.</summary>
    internal int stride => Second;
}
