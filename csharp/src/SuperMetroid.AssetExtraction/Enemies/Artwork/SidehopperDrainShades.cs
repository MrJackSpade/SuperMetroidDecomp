using SuperMetroid.Core.Assets;
using static SuperMetroid.AssetExtraction.Enemies.Artwork.SidehopperDrainShadeDefinitions;

namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Places the Sidehopper drain image's intermediate shades between their body and detail endpoints.</summary>
internal static class SidehopperDrainShades
{
    /// <summary>Places an upper intermediate red seven steps below its light endpoint.</summary>
    private static int LightShadeRed(RgbColor light) => light.FadeLevels.Red - SelectedLightRedDrop;
    /// <summary>Places a lower intermediate red four steps above its dark endpoint.</summary>
    private static int DarkShadeRed(RgbColor dark) => dark.FadeLevels.Red + SelectedDarkRedLift;

    /// <summary>Selects the eight body/detail endpoints and intermediate shades in palette order.</summary>
    internal static RgbColor Resolve(int color, RgbColor bodyLight, RgbColor detailLight, RgbColor detailDark, RgbColor bodyDark) => color switch
    {
        1 => bodyLight,
        2 => Shade(bodyLight, bodyDark, LightShadeRed(bodyLight)),
        3 => Shade(bodyLight, bodyDark, DarkShadeRed(bodyDark)),
        4 => bodyDark,
        5 => detailLight,
        6 => Shade(detailLight, detailDark, LightShadeRed(detailLight)),
        7 => Shade(detailLight, detailDark, DarkShadeRed(detailDark)),
        8 => detailDark,
        _ => throw new ArgumentOutOfRangeException(nameof(color)),
    };

    /// <summary>Interpolates each RGB5 channel between its endpoints at the chosen red-channel fraction.</summary>
    private static RgbColor Shade(RgbColor light, RgbColor dark, int red)
    {
        int lightRed = light.FadeLevels.Red, darkRed = dark.FadeLevels.Red;
        return light.FadeLevels.Zip(dark.FadeLevels, (_, a, b) => ((a * (red - darkRed) + b * (lightRed - red)) / (lightRed - darkRed))).ToColour();
    }
}
