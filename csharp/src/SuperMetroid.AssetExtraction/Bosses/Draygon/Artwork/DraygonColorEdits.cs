using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Areas.Ceres.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;

/// <summary>
/// Compares Draygon's supplied opening, material, white-flash and health colours with the calculated paint and keeps
/// only the colours that differ, the selection the game resolves its colours from.
/// </summary>
internal static class DraygonColorEdits
{
    /// <summary>Selects the deviations of every supplied Draygon colour from its calculated paint.</summary>
    internal static DraygonColorSelection Select(RgbColor[] intro, RgbColor[] background, RgbColor[] sprite,
        RgbColor[] whiteFlash, RgbColor[][] healthBands) =>
        new(Intro(intro), Material(background), Material(sprite), WhiteFlashEdits(whiteFlash), Health(healthBands));

    /// <summary>Splits the 25 copied opening colours into primary, clear-target, and door-surface aliases.</summary>
    private static DraygonIntroPaint Intro(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 25);
        return new(Material(colors[..16]), CeresDoorPaintRecognition.Surface(colors[17..]),
            colors[16] == DraygonMaterialPaint.ClearTarget ? null : colors[16]);
    }

    /// <summary>Keeps the entries of a sixteen-colour material palette that differ from the calculated stock paint.</summary>
    private static DraygonMaterialPaint Material(ReadOnlySpan<RgbColor> colors)
    {
        Ensure.LengthEqual(colors, 16);
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < colors.Length; color++)
            if (colors[color] != DraygonMaterialPaint.StockColor(color)) edits.Add(new ColourCell(0, color), colors[color]);
        return new DraygonMaterialPaint(edits);
    }

    /// <summary>Keeps the supplied flash colours that differ from stock white.</summary>
    private static Dictionary<ColourCell, RgbColor> WhiteFlashEdits(RgbColor[] whiteFlash)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int color = 0; color < whiteFlash.Length; color++)
            if (whiteFlash[color] != DraygonPalette.StockWhiteFlash(color)) edits.Add(new ColourCell(0, color), whiteFlash[color]);
        return edits;
    }

    /// <summary>Keeps the belly colours that differ from the four-shade, eight-band paint calculation.</summary>
    private static DraygonHealthPaint Health(RgbColor[][] healthBands)
    {
        var edits = new Dictionary<ColourCell, RgbColor>();
        for (int band = 0; band < DraygonHealthPaintDefinitions.Bands; band++)
            for (int shade = 0; shade < DraygonHealthPaintDefinitions.Shades; shade++)
                if (healthBands[band][shade] != DraygonHealthPaint.Calculate(band, shade))
                    edits.Add(new ColourCell(band, shade), healthBands[band][shade]);
        return new DraygonHealthPaint(edits);
    }
}
