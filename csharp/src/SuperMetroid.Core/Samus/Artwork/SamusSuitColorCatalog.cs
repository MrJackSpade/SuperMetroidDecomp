using SuperMetroid.Core.Assets;

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
public sealed class SamusSuitColorCatalog
{
    /// <summary>Complete authored Power suit palette, including its transparent-slot payload.</summary>
    private readonly Bgr555[] power;
    /// <summary>Only Varia slots differing from the Power palette's shared inks.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> varia;
    /// <summary>Only Gravity slots differing from the Power palette's shared inks.</summary>
    internal readonly Dictionary<ColourCell, Bgr555> gravity;

    /// <summary>Stores a Power palette and only differing colors for the other suits.</summary>
    /// <remarks>Original $9B:9400/9520/9800 share twelve of sixteen slots
    /// per secondary suit. Native suit selection is categorical; named owners
    /// replace the palette roster. Comparing supplied values preserves arbitrary
    /// independent player edits. This removes duplicated words; the remaining
    /// base-ink disposition is documented on the catalog.</remarks>
    internal SamusSuitColorCatalog(Bgr555[] power, Bgr555[] varia, Bgr555[] gravity)
    {
        this.power = power;
        this.varia = Differences(varia, power);
        this.gravity = Differences(gravity, power);
    }

    /// <summary>Finds independently selected suit inks while omitting slots that reuse the Power palette word.</summary>
    private static Dictionary<ColourCell, Bgr555> Differences(Bgr555[] supplied, Bgr555[] power)
    {
        var differences = new Dictionary<ColourCell, Bgr555>();
        for (int index = 0; index < supplied.Length; index++)
            if (supplied[index] != power[index]) differences.Add(new ColourCell(0, index), supplied[index]);
        return differences;
    }

    /// <summary>Resolves one packed RGB555 color from the selected normal suit palette.</summary>
    /// <param name="suitTableOffset">Native categorical table offset: 0 for Power, 2 for Varia, or 4 for Gravity.</param>
    /// <param name="colorIndex">OBJ palette slot from 0 through 15, including the transparent slot.</param>
    /// <returns>The selected packed SNES RGB555 word.</returns>
    public Bgr555 Resolve(ushort suitTableOffset, int colorIndex)
    {
        Dictionary<ColourCell, Bgr555>? differences = suitTableOffset switch
        {
            0 => null,
            2 => varia,
            4 => gravity,
            _ => throw new ArgumentOutOfRangeException(nameof(suitTableOffset)),
        };
        if ((uint)colorIndex >= SamusSuitColorFormat.ColorsPerSuit)
            throw new ArgumentOutOfRangeException(nameof(colorIndex));
        return differences is not null && differences.TryGetValue(new ColourCell(0, colorIndex), out Bgr555 color)
            ? color : power[colorIndex];
    }

    /// <summary>Writes only Samus's sixteen OBJ colors; no equipment or phase state changes.</summary>
    public void Apply(ColourRun suit, ushort suitTableOffset)
    {
        for (int index = 0; index < SamusSuitColorFormat.ColorsPerSuit; index++)
            suit.Write(index,
                Resolve(suitTableOffset, index));
    }
}
