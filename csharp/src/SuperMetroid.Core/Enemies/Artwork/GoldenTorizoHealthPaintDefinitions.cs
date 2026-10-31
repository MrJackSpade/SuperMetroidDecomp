using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Exact $84:8032-8231 Golden Torizo health paint composition. Reviewed categorical endpoint
/// channels and material lighting specify this painted transition; health thresholds, mechanics, timing and pixels are excluded.</summary>
internal static class GoldenTorizoHealthPaintDefinitions
{
    /// <summary>$84:8034-8042 / $AA:8709-8717: eight low-health armor red levels.</summary>
    internal const int LowHighlight = 26, LowEdge = 18, LowRecess = 7, LowOutline = 3,
        LowPlateLight = 21, LowPlateDark = 8;
    /// <summary>$84:803C-8042: selected five-level armor shade step, clipped at the dark facet.</summary>
    internal const int LowPlateShadeStep = 5;
    /// <summary>$84:8034-8042: low armor green and blue deficits; highlight shares green, middle blue selects13.</summary>
    internal const int LowGreenDrop = 5, LowBlueDrop = 2, LowMiddleBlue = 13;
    /// <summary>$84:8044-8048: chosen purple core paints.</summary>
    internal static readonly RgbColor LowCoreLight = new(31, 27, 27), LowCoreMiddle = new(24, 15, 20),
        LowCoreDark = new(14, 8, 16);
    /// <summary>$84:804A-804E: warm eye red shade step and two selected green levels; dark green halves middle.</summary>
    internal const int LowEyeRedStep = 5, LowEyeGreenLight = 24, LowEyeGreenMiddle = 14;
    /// <summary>$84:8050: neutral low-health contour intensity.</summary>
    internal const int LowNeutral = 3;
    /// <summary>$84:8114-8122 / $AA:8789-8797: golden armor channel choices.</summary>
    internal const int GoldHighlightRed = 30, GoldHighlightBlue = 18, GoldRecessRed = 8,
        GoldPlateLightRed = 26, GoldPlateLightBlue = 5, GoldPlateMiddleRed = 22, GoldPlateDarkRed = 13,
        GoldLightGreenDrop = 1, GoldShadeGreenDrop = 3;
    /// <summary>$84:8116: independent golden edge paint.</summary>
    internal static readonly RgbColor GoldEdge = new(25, 21, 1);
    /// <summary>$84:8124-812E: clipped cyan core/eye shade policies, not a historical generator claim.</summary>
    internal const int CyanGreenHeadroom = 2, CoreGreenStep = 8, CoreBlueLight = 28,
        CoreBlueStep = 9, EyeGreenStep = 12, EyeBlueShade = 22;
    /// <summary>$84:8130: golden contour's red/green; blue is zero.</summary>
    internal const int GoldContourRed = 3, GoldContourGreen = 2;
    /// <summary>$84:8134-8150/8214-8230: rear material lighting reductions and the selected low rear dark-facet blue channel.</summary>
    internal const int RearShade = 5, RearCoreShade = 10, LowRearPlateBlue = 3;
    /// <summary>$84:8032/8112/8132/8212: copied transparent word1000, except final front band0000.</summary>
    internal static readonly RgbColor TransparentPayload = new(0, 0, 4);
    /// <summary>$84:8005-8016: eight native health-selected palette bands; interpolation spans seven intervals.</summary>
    internal const int Intervals = 7;
}
