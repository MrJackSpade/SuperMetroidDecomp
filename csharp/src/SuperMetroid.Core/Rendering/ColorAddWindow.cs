namespace SuperMetroid.Core.Rendering;

/// <summary>The band of columns, both edges included, and the eight-bit colour added on one screen line.</summary>
/// <param name="Left">Inclusive left screen column, 0 through <see cref="XrayWindowLine.MaximumColumn"/>.</param>
/// <param name="Right">Inclusive right screen column, 0 through <see cref="XrayWindowLine.MaximumColumn"/>.</param>
/// <param name="Red">The red added, 0 through 255.</param>
/// <param name="Green">The green added, 0 through 255.</param>
/// <param name="Blue">The blue added, 0 through 255.</param>
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

    /// <summary>The red added, 0 through 255.</summary>
    public int Red
    {
        get;
        init => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    } = Ensure.BetweenInclusive(0, byte.MaxValue, Red);

    /// <summary>The green added, 0 through 255.</summary>
    public int Green
    {
        get;
        init => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    } = Ensure.BetweenInclusive(0, byte.MaxValue, Green);

    /// <summary>The blue added, 0 through 255.</summary>
    public int Blue
    {
        get;
        init => field = Ensure.BetweenInclusive(0, byte.MaxValue, value);
    } = Ensure.BetweenInclusive(0, byte.MaxValue, Blue);

    /// <summary>A left edge past the right covers no columns, as on the HUD's lines.</summary>
    public static ColorAddWindow Empty => new(1, 0, 0, 0, 0);
}
