using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork.DraygonMaterialPaintDefinitions;
namespace SuperMetroid.AssetExtraction.Bosses.Draygon.Artwork;

/// <summary>
/// Shared primary Draygon paint and the sprite palette.
/// Highlights, outlines and clipped green-carapace ramps are selected material
/// content; the four gold belly shades use their existing health-paint owner.
/// Replacing the selected highlight/outline/eye levels or these precise material
/// groupings would invent different categorical paint. Copied clear/green targets
/// are bounded source metadata, not values inferred from transparency or absence.
/// Each installed occurrence preserves independently supplied channel values.
/// </summary>
internal sealed class DraygonMaterialPaint
{
    /// <summary>Selected palette entries whose installed channels differ from calculated stock material paint.</summary>
    internal readonly Dictionary<PaintColour, RgbColor> edits;

    /// <summary>Retains only independently edited entries from the sixteen-color Draygon material palette.</summary>
    /// <param name="edits">The entries the import found differing from the calculated stock material paint.</param>
    internal DraygonMaterialPaint(Dictionary<PaintColour, RgbColor> edits) => this.edits = edits;

    /// <summary>Uses an installed paint edit when present and otherwise calculates the selected stock material shade.</summary>
    internal RgbColor Color(int index)
    {
        if ((uint)index >= 16) throw new ArgumentOutOfRangeException(nameof(index));
        return edits.TryGetValue(new PaintColour(0, index), out RgbColor edited) ? edited : StockColor(index);
    }

    /// <summary>Copied clear-slot target with its independent blue channel.</summary>
    internal static RgbColor ClearTarget => Pack(0, 0, ClearBlue);

    /// <summary>Combines carapace ramps, eye paint, and the separately owned healthy belly shades.</summary>
    internal static RgbColor StockColor(int color)
    {
        if (color is >= 5 and <= 8)
        {
            int shade = color - 5;
            int red = Math.Max(MiddleRedFloor, MiddlePeak - ChannelBias * shade);
            return Pack(red, red + ChannelBias, Math.Min(MiddleBlueCeiling, MiddlePeak >> shade));
        }
        if (color is >= 9 and <= 12) return DraygonHealthPaint.HealthyShade(color - 9);
        return color switch
        {
            0 => ClearTarget,
            1 => Pack(HighlightRed, HighlightGreen, HighlightBlue),
            2 => Pack(SecondaryRed, SecondaryRed + ChannelBias, SecondaryBlue),
            3 => Pack(ContourRed, ContourRed + ChannelBias, 0),
            4 => Pack(0, OutlineGreen, 0),
            13 => Pack(MagentaBlue + ChannelBias, 0, MagentaBlue),
            14 => Pack(RedTargetRed, RedTargetGreen, RedTargetBlue),
            15 => Pack(ChannelBias, ChannelBias * 2, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(color)),
        };
    }

    /// <summary>Forms the five-bit RGB channels calculated for one Draygon material entry.</summary>
    private static RgbColor Pack(int red, int green, int blue) => new FadeLevelColour(red, green, blue).ToColour();
}
