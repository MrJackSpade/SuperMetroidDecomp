using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// One of the settled colourings Phantoon's body shows on the background (#1512): black as his fight starts, a health band's
/// colours, the white hurt flash, or his faded-out colours. His fades run from one to another.
/// </summary>
public readonly partial record struct PhantoonBodyLook
{
    /// <summary>
    /// The colouring of <paramref name="kind"/> named <paramref name="key"/>, showing <paramref name="band"/>'s colours for a
    /// health colouring.
    /// </summary>
    private PhantoonBodyLook(PhantoonBodyLookKind kind, PhantoonHealthBand? band, string key)
    {
        Kind = kind;
        Band = band;
        Key = key;
    }

    /// <summary>Which colouring.</summary>
    public PhantoonBodyLookKind Kind { get; }

    /// <summary>The health band whose colours a health colouring shows (#2259); null for the others.</summary>
    public PhantoonHealthBand? Band { get; }

    /// <summary>His body black, as his fight starts.</summary>
    public static PhantoonBodyLook Black => new(PhantoonBodyLookKind.Black, null, "black");

    /// <summary>His body white, as he flashes when hurt.</summary>
    public static PhantoonBodyLook White => new(PhantoonBodyLookKind.White, null, "white");

    /// <summary>His body faded out, as he vanishes.</summary>
    public static PhantoonBodyLook FadedOut => new(PhantoonBodyLookKind.FadedOut, null, "fadedout");

    /// <summary>His body in <paramref name="band"/>'s colours.</summary>
    public static PhantoonBodyLook Health(PhantoonHealthBand band) => new(PhantoonBodyLookKind.Health, Ensure.NotNull(band), $"health-{band.Key}");

    /// <summary>
    /// The colouring's name in its installed shades' names (#2472), written out where each colouring is made so renaming a
    /// colouring's identity never changes a name on disk, as Torizo's are (#2409); a health colouring is named by its band.
    /// </summary>
    [InstalledKey]
    public string Key { get; }
}
