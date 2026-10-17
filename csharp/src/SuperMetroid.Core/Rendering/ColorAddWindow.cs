namespace SuperMetroid.Core.Rendering;

/// <summary>Inclusive horizontal endpoints and expanded-byte RGB addition for one physical scanline.</summary>
/// <param name="Left">Inclusive left screen column, 0 through <see cref="XrayWindowLine.MaximumColumn"/>.</param>
/// <param name="Right">Inclusive right screen column, 0 through <see cref="XrayWindowLine.MaximumColumn"/>.</param>
/// <param name="Red">Expanded red addition.</param>
/// <param name="Green">Expanded green addition.</param>
/// <param name="Blue">Expanded blue addition.</param>
public readonly record struct ColorAddWindow(int Left, int Right, byte Red, byte Green, byte Blue)
{
    /// <summary>Inclusive left screen column.</summary>
    public int Left
    {
        get => left;
        init => left = Ensure.BetweenInclusive(0, XrayWindowLine.MaximumColumn, value);
    }

    /// <summary>Validated store of <see cref="Left"/>.</summary>
    private readonly int left = Ensure.BetweenInclusive(0, XrayWindowLine.MaximumColumn, Left);

    /// <summary>Inclusive right screen column.</summary>
    public int Right
    {
        get => right;
        init => right = Ensure.BetweenInclusive(0, XrayWindowLine.MaximumColumn, value);
    }

    /// <summary>Validated store of <see cref="Right"/>.</summary>
    private readonly int right = Ensure.BetweenInclusive(0, XrayWindowLine.MaximumColumn, Right);

    /// <summary>Inverted endpoints represent an empty window, including hidden HUD lines.</summary>
    public static ColorAddWindow Empty => new(1, 0, 0, 0, 0);
}
