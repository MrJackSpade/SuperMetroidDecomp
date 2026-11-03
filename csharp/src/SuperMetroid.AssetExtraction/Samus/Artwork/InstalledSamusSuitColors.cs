using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Three editable, full-body normal suit palettes; suit selection remains gameplay code.</summary>
/// <remarks>Issue #1165 retains the22 distinct opaque ink colors and two
/// transparent-slot payloads of the three suit palettes under the nonsense exception.
/// The suit palette load copies those words to fixed sprite palette slots. Sprite tile pixels
/// choose categorical ink indices, not a time, distance, light level or shade
/// magnitude from which RGB can be calculated. For example, the yellow Power
/// inks at slots1/2/10/11/12 have intensities8/29/21/11/18: the order identifies
/// painted pixel classes, not samples of a brightness ramp. Their common hue
/// does not determine those chosen intensities. An index fit would encode the
/// painting. Color-zero payloads are also selected source data; OBJ rendering
/// skips zero-index pixels before reading their color. Shared suit words are
/// removed independently, and generated animation/tint steps are reviewed in
/// their own owners. This exception covers these base inks, not all palettes.</remarks>
public sealed class InstalledSamusSuitColors
{
    /// <summary>Complete authored Power suit palette, including its transparent-slot payload.</summary>
    private readonly RgbColor[] power;
    /// <summary>Only Varia slots differing from the Power palette's shared inks.</summary>
    internal readonly Dictionary<TableColourPlace, RgbColor> varia;
    /// <summary>Only Gravity slots differing from the Power palette's shared inks.</summary>
    internal readonly Dictionary<TableColourPlace, RgbColor> gravity;

    /// <summary>Stores a Power palette and only differing colors for the other suits.</summary>
    /// <remarks>The original suit palettes share twelve of sixteen slots
    /// per secondary suit. Suit selection is categorical; named owners
    /// replace the palette roster. Comparing supplied values preserves arbitrary
    /// independent player edits. This removes duplicated words; the remaining
    /// base-ink disposition is documented on the catalog.</remarks>
    /// <param name="power">The complete Power palette.</param>
    /// <param name="varia">The Varia slots the import found differing from the Power palette.</param>
    /// <param name="gravity">The Gravity slots the import found differing from the Power palette.</param>
    internal InstalledSamusSuitColors(RgbColor[] power, Dictionary<TableColourPlace, RgbColor> varia,
        Dictionary<TableColourPlace, RgbColor> gravity)
    {
        this.power = power;
        this.varia = varia;
        this.gravity = gravity;
    }

    /// <summary>Resolves one of a suit's normal colours.</summary>
    /// <param name="suit">The suit whose colours are read.</param>
    /// <param name="colorIndex">The suit colour, 0 through 15, including the transparent one.</param>
    /// <returns>The suit's colour.</returns>
    public RgbColor Resolve(SamusSuit suit, int colorIndex)
    {
        Dictionary<TableColourPlace, RgbColor>? differences = suit switch
        {
            SamusSuit.Power => null,
            SamusSuit.Varia => varia,
            SamusSuit.Gravity => gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(suit)),
        };
        if ((uint)colorIndex >= SamusSuitColorFormat.ColorsPerSuit)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return differences is not null && differences.TryGetValue(new TableColourPlace(0, colorIndex), out RgbColor color)
            ? color : power[colorIndex];
    }
}
