using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of the settled colourings Phantoon's body shows on the background (#1512): black as his fight starts, a health band's
/// colours, the white hurt flash, or his faded-out colours. His fades run from one to another.
/// </summary>
public readonly record struct PhantoonBodyLook
{
    /// <summary>
    /// Each colouring's name in its installed shades' names (#2472), written out so renaming a colouring's identity never
    /// changes a name on disk, as Torizo's are (#2409). A health colouring is named by its band instead.
    /// </summary>
    private static readonly FrozenDictionary<PhantoonBodyLookKind, string> Keys =
        new Dictionary<PhantoonBodyLookKind, string>
        {
            [PhantoonBodyLookKind.Black] = "black",
            [PhantoonBodyLookKind.White] = "white",
            [PhantoonBodyLookKind.FadedOut] = "fadedout",
        }.ToFrozenDictionary();

    /// <summary>The colouring of <paramref name="kind"/>, showing <paramref name="band"/>'s colours for a health colouring.</summary>
    private PhantoonBodyLook(PhantoonBodyLookKind kind, PhantoonHealthBand? band)
    {
        Kind = kind;
        Band = band;
    }

    /// <summary>Which colouring.</summary>
    public PhantoonBodyLookKind Kind { get; }

    /// <summary>The health band whose colours a health colouring shows (#2259); null for the others.</summary>
    public PhantoonHealthBand? Band { get; }

    /// <summary>His body black, as his fight starts.</summary>
    public static PhantoonBodyLook Black => new(PhantoonBodyLookKind.Black, null);

    /// <summary>His body white, as he flashes when hurt.</summary>
    public static PhantoonBodyLook White => new(PhantoonBodyLookKind.White, null);

    /// <summary>His body faded out, as he vanishes.</summary>
    public static PhantoonBodyLook FadedOut => new(PhantoonBodyLookKind.FadedOut, null);

    /// <summary>His body in <paramref name="band"/>'s colours.</summary>
    public static PhantoonBodyLook Health(PhantoonHealthBand band) => new(PhantoonBodyLookKind.Health, Ensure.NotNull(band));

    /// <summary>The colouring's name in its shades' names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is a health colouring that names no band.</exception>
    [InstalledKey]
    public string Key => Band is { } band ? $"health-{band.Key}" : Ensure.Listed(Keys, Kind);
}
