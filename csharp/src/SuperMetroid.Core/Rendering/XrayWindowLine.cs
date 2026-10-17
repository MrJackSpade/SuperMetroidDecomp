namespace SuperMetroid.Core.Rendering;

/// <summary>Inclusive window endpoints in screen columns; inverted endpoints hide the reveal on that line.</summary>
/// <param name="Left">Inclusive left screen column, 0 through <see cref="MaximumColumn"/>.</param>
/// <param name="Right">Inclusive right screen column, 0 through <see cref="MaximumColumn"/>.</param>
public readonly record struct XrayWindowLine(int Left, int Right)
{
    /// <summary>The rightmost screen column a window edge names.</summary>
    public const int MaximumColumn = SnesPpuLayout.ScreenWidthPixels - 1;

    /// <summary>Inclusive left screen column.</summary>
    public int Left
    {
        get => left;
        init => left = Ensure.BetweenInclusive(0, MaximumColumn, value);
    }

    /// <summary>Validated store of <see cref="Left"/>.</summary>
    private readonly int left = Ensure.BetweenInclusive(0, MaximumColumn, Left);

    /// <summary>Inclusive right screen column.</summary>
    public int Right
    {
        get => right;
        init => right = Ensure.BetweenInclusive(0, MaximumColumn, value);
    }

    /// <summary>Validated store of <see cref="Right"/>.</summary>
    private readonly int right = Ensure.BetweenInclusive(0, MaximumColumn, Right);
}
