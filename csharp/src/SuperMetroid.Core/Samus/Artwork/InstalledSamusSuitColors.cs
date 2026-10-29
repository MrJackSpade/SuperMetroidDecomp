using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Three editable, full-body normal suit palettes; suit selection remains gameplay code.</summary>
/// <remarks>Issue #1165 retains the22 distinct opaque ink colors and two
/// transparent-slot payloads from $9B:9400/9520/9800 under the nonsense exception.
/// $91:DD5B copies those words to fixed OBJ palette slots. Sprite tile pixels
/// choose categorical ink indices, not a time, distance, light level or shade
/// magnitude from which RGB can be calculated. For example, the yellow Power
/// inks at slots1/2/10/11/12 have intensities8/29/21/11/18: the order identifies
/// painted pixel classes, not samples of a brightness ramp. Their common hue
/// does not determine those chosen intensities. An index fit would encode the
/// painting. Color-zero payloads are also selected source data; OBJ rendering
/// skips zero-index pixels before reading their color. Shared suit words are
/// removed independently, and generated animation/tint rows are reviewed in
/// their own owners. This exception covers these base inks, not all palettes.</remarks>
public sealed class InstalledSamusSuitColors
{
    /// <summary>Complete authored Power suit palette, including its transparent-slot payload.</summary>
    private readonly RgbColor[] power;
    /// <summary>Only Varia slots differing from the Power palette's shared inks.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> varia;
    /// <summary>Only Gravity slots differing from the Power palette's shared inks.</summary>
    internal readonly Dictionary<ColourCell, RgbColor> gravity;

    /// <summary>Stores a Power palette and only differing colors for the other suits.</summary>
    /// <remarks>Original $9B:9400/9520/9800 share twelve of sixteen slots
    /// per secondary suit. Native suit selection is categorical; named owners
    /// replace the palette roster. Comparing supplied values preserves arbitrary
    /// independent player edits. This removes duplicated words; the remaining
    /// base-ink disposition is documented on the catalog.</remarks>
    internal InstalledSamusSuitColors(RgbColor[] power, RgbColor[] varia, RgbColor[] gravity)
    {
        this.power = power;
        this.varia = Differences(varia, power);
        this.gravity = Differences(gravity, power);
    }

    /// <summary>Finds independently selected suit inks while omitting slots that reuse the Power palette word.</summary>
    private static Dictionary<ColourCell, RgbColor> Differences(RgbColor[] supplied, RgbColor[] power)
    {
        var differences = new Dictionary<ColourCell, RgbColor>();
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != power[index]) differences.Add(new ColourCell(0, index), supplied[index]);
        return differences;
    }

    /// <summary>Resolves one of a suit's normal colours.</summary>
    /// <param name="suit">The suit whose colours are read.</param>
    /// <param name="colorIndex">The suit colour, 0 through 15, including the transparent one.</param>
    /// <returns>The suit's colour.</returns>
    public RgbColor Resolve(SamusSuit suit, int colorIndex)
    {
        Dictionary<ColourCell, RgbColor>? differences = suit switch
        {
            SamusSuit.Power => null,
            SamusSuit.Varia => varia,
            SamusSuit.Gravity => gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(suit)),
        };
        if ((uint)colorIndex >= SamusSuitColorFormat.ColorsPerSuit)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return differences is not null && differences.TryGetValue(new ColourCell(0, colorIndex), out RgbColor color)
            ? color : power[colorIndex];
    }

    /// <summary>
    /// Shows <paramref name="shown"/>'s normal colours in <paramref name="suit"/>, Samus's suit colours or the intro's
    /// copy of them; no equipment or phase state changes.
    /// </summary>
    public void Apply(ColourRun suit, SamusSuit shown)
    {
        for (int index = 0; index < SamusSuitColorFormat.ColorsPerSuit; index++)
            suit.Write(index,
                Resolve(shown, index));
    }
}
