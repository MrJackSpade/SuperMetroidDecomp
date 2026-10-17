namespace SuperMetroid.Core.Rendering;

/// <summary>Inclusive horizontal endpoints and expanded-byte RGB addition for one physical scanline.</summary>
/// <param name="Left">Inclusive left screen column, 0 through <see cref="XrayWindowLine.MaximumColumn"/>.</param>
/// <param name="Right">Inclusive right screen column, 0 through <see cref="XrayWindowLine.MaximumColumn"/>.</param>
/// <param name="Red">Expanded red addition, 0 through 255.</param>
/// <param name="Green">Expanded green addition, 0 through 255.</param>
/// <param name="Blue">Expanded blue addition, 0 through 255.</param>
public readonly record struct ColorAddWindow(int Left, int Right, int Red, int Green, int Blue)
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

    /// <summary>Expanded red addition, 0 through 255.</summary>
    public int Red
    {
        get => red;
        init => red = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>Validated store of <see cref="Red"/>.</summary>
    private readonly int red = Ensure.BetweenInclusive(0, byte.MaxValue, Red);

    /// <summary>Expanded green addition, 0 through 255.</summary>
    public int Green
    {
        get => green;
        init => green = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>Validated store of <see cref="Green"/>.</summary>
    private readonly int green = Ensure.BetweenInclusive(0, byte.MaxValue, Green);

    /// <summary>Expanded blue addition, 0 through 255.</summary>
    public int Blue
    {
        get => blue;
        init => blue = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    }

    /// <summary>Validated store of <see cref="Blue"/>.</summary>
    private readonly int blue = Ensure.BetweenInclusive(0, byte.MaxValue, Blue);

    /// <summary>Inverted endpoints represent an empty window, including hidden HUD lines.</summary>
    public static ColorAddWindow Empty => new(1, 0, 0, 0, 0);
}
