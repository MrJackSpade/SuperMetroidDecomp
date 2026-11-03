namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of the settled colourings Phantoon's body shows on layer 2 (#1512): black as his fight starts, a health band's
/// colours, the white hurt flash, or his faded-out colours. His fades run from one to another.
/// </summary>
/// <param name="Kind">Which colouring.</param>
/// <param name="Band">The health band, zero through seven, for a health colouring; zero otherwise.</param>
public readonly record struct PhantoonBodyLook(PhantoonBodyLookKind Kind, int Band = 0)
{
    /// <summary>His body black, as his fight starts.</summary>
    public static PhantoonBodyLook Black => new(PhantoonBodyLookKind.Black);

    /// <summary>His body white, as he flashes when hurt.</summary>
    public static PhantoonBodyLook White => new(PhantoonBodyLookKind.White);

    /// <summary>His body faded out, as he vanishes.</summary>
    public static PhantoonBodyLook FadedOut => new(PhantoonBodyLookKind.FadedOut);

    /// <summary>His body in health band <paramref name="band"/>'s colours.</summary>
    public static PhantoonBodyLook Health(int band) => new(PhantoonBodyLookKind.Health, Ensure.BetweenInclusive(0, PhantoonColorDefinitions.HealthBandCount - 1, band));

    /// <summary>The colouring's name in its shades' names.</summary>
    public string Key => Kind == PhantoonBodyLookKind.Health ? $"health-{Band}" : Kind.ToString().ToLowerInvariant();
}
