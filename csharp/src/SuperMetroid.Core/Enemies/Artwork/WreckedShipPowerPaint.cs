using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Enemies.Artwork.WreckedShipPowerPaintDefinitions;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Paints the powered Wrecked Ship colours of the seven background sets the power-on fades from the catalog's selected hues: repeats, shared channels, ramps, lighting offsets and fills calculate.</summary>
internal static class WreckedShipPowerPaint
{
    /// <summary>Gets the powered Wrecked Ship colour <paramref name="ink"/> of background set <paramref name="set"/>.</summary>
    internal static RgbColor Color(BackgroundColourSet set, int ink)
    {
        if ((uint)ink >= ColourSets.SetColours) throw new ArgumentOutOfRangeException(nameof(ink));
        if (ink == 0) return set is BackgroundColourSet.HudItemsAndEffects or BackgroundColourSet.TilesFirst
            ? DoorTransparentMetadata
            : RgbColor.Black;
        if (ink == ColourSets.SetColours - 1) return RgbColor.Black;
        return set switch
        {
            BackgroundColourSet.Hud => Common(ink),
            BackgroundColourSet.HudItemsAndEffects => GreenDoor(ink),
            BackgroundColourSet.TilesFirst => MagentaDoor(ink),
            BackgroundColourSet.TilesSecond => BlueDoor(ink),
            BackgroundColourSet.TilesThird => Foreground(ink),
            BackgroundColourSet.TilesFourth => Background(ink),
            BackgroundColourSet.TilesFifth => ink == 14 ? White : Neutral(CommonNeutral),
            _ => throw new ArgumentOutOfRangeException(nameof(set), set, "The Wrecked Ship's power-on does not paint this set."),
        };
    }
    /// <summary>Builds shared gold, machinery, indicator, and neutral inks for the common row.</summary>
    private static RgbColor Common(int ink) => ink switch
    {
        >= 1 and <= 3 => Gold(ink - 1),
        >= 4 and <= 7 => CommonMetal(ink - 4),
        8 => GreenHighlight, 9 => MagentaMiddle, 13 => CommonBlueAccent,
        10 or 12 or 14 => White, 11 => RgbColor.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    /// <summary>Builds the green door's highlights, metal facets, neutral shadows, and accents.</summary>
    private static RgbColor GreenDoor(int ink) => ink switch
    {
        1 => GreenHighlight, 2 => GreenMiddle, 3 => GreenDark,
        >= 4 and <= 7 => CommonMetal(ink - 4),
        8 => Neutral(CommonNeutral), 9 => Neutral(CommonShadow), 10 => Neutral(CommonOutline),
        11 => RgbColor.Black, 12 => White, 13 => Gold(0), 14 => Rgb(31, 0, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    /// <summary>Builds the magenta door's three selected shades and neutral fill.</summary>
    private static RgbColor MagentaDoor(int ink) => ink switch
    {
        1 => Rgb(MagentaLight, MagentaLightGreen, MagentaLight),
        2 => MagentaMiddle, 3 => Rgb(MagentaDarkRed, 0, MagentaDarkBlue),
        14 => White, _ => Neutral(CommonNeutral),
    };
    /// <summary>Builds the blue door's bright, middle, and dark painted facets.</summary>
    private static RgbColor BlueDoor(int ink) => ink switch
    {
        1 => Rgb(DoorBlueLightRed, DoorBlueLightGreen, DoorBlueBright),
        2 => Rgb(DoorBlueMiddleRed, DoorBlueMiddleGreen, DoorBlueBright),
        3 => Rgb(DoorBlueDarkRed, DoorBlueDarkGreen, DoorBlueDark),
        14 => White, _ => Neutral(CommonNeutral),
    };
    /// <summary>Builds foreground wall, conduit, outline, and green-indicator inks.</summary>
    private static RgbColor Foreground(int ink) => ink switch
    {
        >= 1 and <= 3 => Warm(ShipLight - ShadeStep * (ink - 1), GreenMaterialTint),
        >= 4 and <= 6 => Warm(ShipLight - ShadeStep * (ink - 4)),
        7 => Rgb(ForegroundOutline, ForegroundOutline, ForegroundOutlineBlue),
        >= 8 and <= 11 => Cool(Math.Max(ForegroundOutline, ShipLight - ShadeStep * (ink - 8))),
        12 => GreenMiddle, 13 => GreenHighlight, 14 => White,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    /// <summary>Builds darker background materials and the red and gold indicators.</summary>
    private static RgbColor Background(int ink) => ink switch
    {
        1 => Subtract(GreenMiddle, BackgroundGreenReduction), 2 => GreenDark,
        3 => Rgb(0, BackgroundGreenDark, 0),
        >= 4 and <= 7 => Warm(Math.Max(ForegroundOutline, ShipLight - BackgroundWarmReduction - ShadeStep * (ink - 4))),
        >= 8 and <= 11 => Cool(Math.Max(BackgroundCoolFloor, ShipLight - BackgroundCoolReduction - ShadeStep * (ink - 8))),
        12 => Rgb(RedIndicator, 0, 0), 13 => Rgb(GoldIndicatorRed, GoldIndicatorGreen, 0), 14 => White,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    /// <summary>Interpolates a gold accent shade between the selected light and dark endpoints.</summary>
    private static RgbColor Gold(int shade)
    {
        int red = shade switch { 0 => 31, 1 => GoldMiddleRed, _ => GoldDarkRed };
        int green = GoldDarkGreen + (red - GoldDarkRed) * (GoldLightGreen - GoldDarkGreen) / (31 - GoldDarkRed);
        return Rgb(red, green, 0);
    }
    /// <summary>Combines a machinery facet's red and green levels with its blue steel tint.</summary>
    private static RgbColor CommonMetal(int shade)
    {
        (int red, int green) = shade switch
        {
            0 => new RedGreenLevels(MetalLightRed, MetalLightGreen), 1 => new RedGreenLevels(MetalMiddleRed, MetalMiddleGreen),
            2 => new RedGreenLevels(MetalDeepRed, MetalDeepGreen), _ => new RedGreenLevels(MetalDarkRed, MetalDarkGreen),
        };
        return Rgb(red, green, green + (shade < 2 ? MetalLightBlueTint : MetalDarkBlueTint));
    }
    /// <summary>Builds warm wall paint with a clipped blue deficit and optional green tint.</summary>
    private static RgbColor Warm(int intensity, int greenTint = 0) =>
        Rgb(intensity, intensity + greenTint, Math.Max(WarmBlueFloor, intensity - WarmBlueSeparation));
    /// <summary>Builds conduit paint by adding the selected blue tint to a neutral level.</summary>
    private static RgbColor Cool(int intensity) => Rgb(intensity, intensity, intensity + CoolBlueTint);
    /// <summary>Darkens each RGB5 channel without passing below zero.</summary>
    private static RgbColor Subtract(RgbColor color, int amount) =>
        Rgb(Math.Max(0, (color.Red) - amount), Math.Max(0, (color.Green) - amount), Math.Max(0, (color.Blue) - amount));
    /// <summary>Gets the green door and background's dark green shade.</summary>
    private static RgbColor GreenDark => Rgb(GreenDarkRed, GreenDarkRed * 2, 0);
    /// <summary>Builds an equal-channel neutral material ink.</summary>
    private static RgbColor Neutral(int intensity) => Rgb(intensity, intensity, intensity);
    /// <summary>Gets the shared maximum-intensity white accent.</summary>
    private static RgbColor White => Neutral(31);
    /// <summary>Packs selected five-bit red, green, and blue levels into a BG palette word.</summary>
    private static RgbColor Rgb(int red, int green, int blue) => new RgbColor(red, green, blue);
}
