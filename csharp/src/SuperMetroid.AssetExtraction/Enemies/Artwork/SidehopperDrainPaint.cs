using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Enemies.Artwork.SidehopperDrainPaintDefinitions;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Paints the seven shifted Sidehopper drain images: the alive images fading toward their endpoints, then the corpse.</summary>
internal static class SidehopperDrainPaint
{
    /// <summary>Interpolates one of six alive images toward its selected endpoint, then uses corpse paint for frame six.</summary>
    internal static RgbColor Color(int frame, int color)
    {
        if ((uint)frame > CorpseFrame || (uint)color >= 15) throw new ArgumentOutOfRangeException(nameof(frame));
        if (frame == CorpseFrame) return SidehopperCorpsePaint.Color(color);
        RgbColor start = SidehopperInitialPaint.Color(color), end = Endpoint(color);
        return start.FadeLevels.Zip(end.FadeLevels, (_, a, b) => ((a * (DrainIntervals - frame) + b * frame + DrainIntervals / 2) / DrainIntervals)).ToColour();
    }

    /// <summary>Selects the unchanged clear slot, shifted drain shades, or mapped corpse colors for a channel endpoint.</summary>
    private static RgbColor Endpoint(int color) => color switch
    {
        0 => SidehopperInitialPaint.Color(0),
        >= 1 and <= 8 => SidehopperDrainShades.Resolve(color, BodyLight, DetailLight, DetailDark, SidehopperCorpsePaint.Color(15)),
        >= 9 and <= 12 => SidehopperCorpsePaint.Color(color - 8),
        13 or 14 => SidehopperCorpsePaint.Color(color - 10),
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };
}
