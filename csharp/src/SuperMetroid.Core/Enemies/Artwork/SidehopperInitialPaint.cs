using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.Artwork.SidehopperInitialPaintDefinitions;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Paints the standalone Sidehopper palette from its reviewed inks and derived midtones.</summary>
internal static class SidehopperInitialPaint
{
    /// <summary>Combines reviewed ink endpoints with derived midtones for the standalone Sidehopper palette.</summary>
    internal static RgbColor Color(int color) => color switch
    {
        0 => TransparentSlot,
        1 or 9 => RgbColor.White,
        2 => BodyHighlight,
        3 => BodySurface,
        4 => SidehopperInitialPalette.BodyShadow(BodySurface),
        5 => HeadGlint,
        6 => DetailLight,
        7 => SidehopperInitialPalette.DetailMidpoint(DetailLight, DetailDark),
        8 => DetailDark,
        10 => ClawLight,
        11 or 14 => SidehopperInitialPalette.ClawMidpoint(ClawLight, ClawDark),
        12 or 15 => ClawDark,
        13 => HeadYellow,
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };
}
