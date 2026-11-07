using SuperMetroid.Core.Assets;
using Presentation = SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork.MotherBrainHealthPalettePresentation;

namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// Recognizes Mother Brain's supplied health bands as red tints of a state-zero basis, and the basis as the stock paint or
/// shade ramps of its own endpoint inks, keeping supplied colours only where a relationship does not reproduce them.
/// </summary>
internal static class MotherBrainHealthPaletteRecognition
{
    /// <summary>Compiles body and rear-leg health-band tints, retaining independently supplied colors when they differ from shading.</summary>
    internal static Presentation Create(RgbColor[][] body, RgbColor[][] backLegs)
    {
        Presentation.TintPalette bodyTint = Tint(body, MotherBrainPalettePart.Body, front: null);
        return new(bodyTint, Tint(backLegs, MotherBrainPalettePart.BackLeg, bodyTint));
    }

    /// <summary>Compares all four supplied health bands with the calculated tint before deciding whether to retain them.</summary>
    private static Presentation.TintPalette Tint(RgbColor[][] healthBands, MotherBrainPalettePart part, Presentation.TintPalette? front)
    {
        Presentation.BasePalette basis = Base(healthBands[0], part, front?.basis);
        var calculated = new Presentation.TintPalette(basis, part, supplied: null);
        for (int state = 0; state < healthBands.Length; state++)
            for (int color = 0; color < healthBands[state].Length; color++)
                if (calculated.Color(state, color) != healthBands[state][color])
                    return new(basis, part, healthBands);
        return calculated;
    }

    /// <summary>
    /// Shares a stock basis only when every supplied color and its dependent front palette agree; otherwise selects
    /// independent endpoint inks and retains every supplied color if shade relationships do not reproduce them.
    /// </summary>
    private static Presentation.BasePalette Base(RgbColor[] colors, MotherBrainPalettePart part, Presentation.BasePalette? front)
    {
        bool rear = part == MotherBrainPalettePart.BackLeg;
        Presentation.BasePalette stock = rear ? Presentation.BasePalette.StockRear : Presentation.BasePalette.StockBody;
        bool matches = !rear || ReferenceEquals(front, Presentation.BasePalette.StockBody);
        for (int color = 0; matches && color < colors.Length; color++)
            matches = colors[color] == stock.Color(color);
        if (matches)
            return stock;
        var anchors = new Presentation.BasePaletteAnchors(
            rear ? RgbColor.Black : colors[0], rear ? RgbColor.Black : colors[1], rear ? RgbColor.Black : colors[2],
            front is null ? colors[3] : RgbColor.Black, front is null ? colors[4] : RgbColor.Black,
            rear ? RgbColor.Black : colors[8]);
        var calculated = new Presentation.BasePalette(part, front, anchors, supplied: null);
        for (int color = 0; color < colors.Length; color++)
            if (calculated.Color(color) != colors[color])
                return new(part, front, anchors, colors);
        return calculated;
    }
}
