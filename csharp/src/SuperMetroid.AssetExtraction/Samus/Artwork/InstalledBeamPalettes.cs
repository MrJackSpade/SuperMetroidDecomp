using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Immutable ordinary beam colors; selection and charge/Hyper animation remain engine-owned.</summary>
public sealed class InstalledBeamPalettes
{
    /// <summary>Only selected RGB5 words that differ from calculated stock beam paint.</summary>
    internal readonly Dictionary<TableColourPlace, RgbColor> palettes;
    /// <summary>Stores independently edited colors while deriving unchanged words from stock paint.</summary>
    /// <param name="palettes">The selected words the import found differing from the stock beam paint.</param>
    internal InstalledBeamPalettes(Dictionary<TableColourPlace, RgbColor> palettes) => this.palettes = palettes;

    /// <summary>
    /// Colour <paramref name="color"/> of retail beam combination <paramref name="selection"/>, which the import also
    /// pre-colours frozen enemies in (#1512).
    /// </summary>
    public RgbColor ColourOf(SamusBeamCombination selection, int color)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        return Color(selection, Ensure.BetweenInclusive(0, BeamPaletteDefinitions.ColorCount - 1, color));
    }

    /// <summary>Gets an edited beam color when supplied, otherwise its calculated stock value.</summary>
    private RgbColor Color(SamusBeamCombination selection, int color) =>
        palettes.TryGetValue(new TableColourPlace(selection.TableIndex, color), out RgbColor supplied)
            ? supplied : BeamPaint.Color(selection, color);

    /// <summary>The sixteen colours a retail beam combination's beams show (#1512); the import pre-colours Samus's sheets in them.</summary>
    /// <param name="selection">A retail beam combination.</param>
    /// <exception cref="ArgumentOutOfRangeException">The combination is not a retail one.</exception>
    public RgbColor[] Colours(SamusBeamCombination selection)
    {
        if (!selection.IsRetail) throw new ArgumentOutOfRangeException(nameof(selection), selection, "Beam palettes cover retail combinations only.");
        var colours = new RgbColor[BeamPaletteDefinitions.ColorCount];
        for (int i = 0; i < colours.Length; i++)
            colours[i] = Color(selection, i);
        return colours;
    }
}
