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
        get;
        init => field = Ensure.BetweenInclusive(0, XrayWindowLine.MaximumColumn, value);
    } = Ensure.BetweenInclusive(0, XrayWindowLine.MaximumColumn, Left);

    /// <summary>Inclusive right screen column.</summary>
    public int Right
    {
        get;
        init => field = Ensure.BetweenInclusive(0, XrayWindowLine.MaximumColumn, value);
    } = Ensure.BetweenInclusive(0, XrayWindowLine.MaximumColumn, Right);

    /// <summary>Expanded red addition, 0 through 255.</summary>
    public int Red
    {
        get;
        init => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    } = Ensure.BetweenInclusive(0, byte.MaxValue, Red);

    /// <summary>Expanded green addition, 0 through 255.</summary>
    public int Green
    {
        get;
        init => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    } = Ensure.BetweenInclusive(0, byte.MaxValue, Green);

    /// <summary>Expanded blue addition, 0 through 255.</summary>
    public int Blue
    {
        get;
        init => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    } = Ensure.BetweenInclusive(0, byte.MaxValue, Blue);

    /// <summary>Inverted endpoints represent an empty window, including hidden HUD lines.</summary>
    public static ColorAddWindow Empty => new(1, 0, 0, 0, 0);
}
