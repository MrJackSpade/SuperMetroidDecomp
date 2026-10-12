using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// The seven powered Wrecked Ship BG material palettes at $A7:CA61-CB40.
/// Native $DC71-DC8A uniformly interpolates CGRAM toward these selected paints.
/// CRE door/machinery illustrations and area wall/conduit/indicator illustrations
/// select the corresponding ink identities; this palette contains no timing or
/// physical trajectory. Remaining named hues and composition policies are that
/// specific illustration content. Replacing them would invent different paint.
/// All repeats, shared channels, ramps, lighting offsets and fills calculate.
/// </summary>
internal static class WreckedShipPowerPaintDefinitions
{
    /// <summary>$A7:CA81/CAA1: exact copied ink-zero metadata of green/magenta door rows; BG ink zero is transparent.</summary>
    private static readonly Bgr555 DoorTransparentMetadata = Bgr555.FromWord(0x2003);
    /// <summary>$A7:CA63-CA67: common gold door/machine accent endpoints and selected middle red.</summary>
    private const int GoldMiddleRed = 23, GoldDarkRed = 12, GoldLightGreen = 22, GoldDarkGreen = 5;
    /// <summary>$A7:CA69-CA6F, repeated atCA89-CA8F: warm common-room machinery red/green intensities.</summary>
    private const int MetalLightRed = 27, MetalLightGreen = 21, MetalMiddleRed = 19, MetalMiddleGreen = 13,
        MetalDeepRed = 14, MetalDeepGreen = 9, MetalDarkRed = 6, MetalDarkGreen = 4;
    /// <summary>$A7:CA69-CA6F: the two lighter steel facets add two blue levels to green; deeper facets add one.</summary>
    private const int MetalLightBlueTint = 2, MetalDarkBlueTint = 1;
    /// <summary>$A7:CA71/CA83/CAFB: common green indicator/door highlight paint.</summary>
    private static readonly Bgr555 GreenHighlight = Bgr555.FromWord(0x0bb1);
    /// <summary>$A7:CA85/CAF9: common green door/indicator middle paint.</summary>
    private static readonly Bgr555 GreenMiddle = Bgr555.FromWord(0x1ea9);
    /// <summary>$A7:CA87/CB05: green door shadow; green is twice red and blue is absent.</summary>
    private const int GreenDarkRed = 5;
    /// <summary>$A7:CA73/CAA5: shared magenta door/common indicator paint.</summary>
    private static readonly Bgr555 MagentaMiddle = Bgr555.FromWord(0x48fb);
    /// <summary>$A7:CAA3/CAA7: magenta door glint and dark red-blue outline.</summary>
    private const int MagentaLight = 28, MagentaLightGreen = 21, MagentaDarkRed = 22, MagentaDarkBlue = 6;
    /// <summary>$A7:CA7B: additional copied common-room blue accent paint; not assigned a visible role by the source metatile packet.</summary>
    private static readonly Bgr555 CommonBlueAccent = Bgr555.FromWord(0x44e5);
    /// <summary>$A7:CA91/CAA9-CABB/CAC9-CADB/CB23-CB3B: common neutral placeholder/material ink.</summary>
    private const int CommonNeutral = 24;
    /// <summary>$A7:CA93-CA95: independent common-room neutral shadow and outline intensities.</summary>
    private const int CommonShadow = 7, CommonOutline = 4;
    /// <summary>$A7:CAC3-CAC7: blue door light/middle/dark red and green, shared bright blue and darker blue.</summary>
    private const int DoorBlueLightRed = 18, DoorBlueLightGreen = 21, DoorBlueMiddleRed = 7,
        DoorBlueMiddleGreen = 14, DoorBlueDarkRed = 3, DoorBlueDarkGreen = 8,
        DoorBlueBright = 28, DoorBlueDark = 19;
    /// <summary>$A7:CAE3-CAF7/CB09-CB17: powered wall/conduit shade step and common bright intensity.</summary>
    private const int ShipLight = 20, ShadeStep = 6;
    /// <summary>$A7:CAE3-CAE7: green material raises green three levels over the warm wall red intensity.</summary>
    private const int GreenMaterialTint = 3;
    /// <summary>$A7:CAE9-CAED/CB09-CB0F: warm wall material blue separation and shadow floor.</summary>
    private const int WarmBlueSeparation = 5, WarmBlueFloor = 1;
    /// <summary>$A7:CAF1-CAF7/CB11-CB17: cool conduit material adds seven blue levels.</summary>
    private const int CoolBlueTint = 7;
    /// <summary>$A7:CAEF/CAF7: foreground outline intensity and independent warm-outline blue.</summary>
    private const int ForegroundOutline = 4, ForegroundOutlineBlue = 5;
    /// <summary>$A7:CB09/CB11: background warm/cool lighting reductions from the foreground highlight.</summary>
    private const int BackgroundWarmReduction = 2, BackgroundCoolReduction = 3;
    /// <summary>$A7:CB17: selected darkest background conduit red/green intensity, distinct from the warm material's blue floor.</summary>
    private const int BackgroundCoolFloor = 1;
    /// <summary>$A7:CB03: background green highlight subtracts this common shade from the green door middle ink.</summary>
    private const int BackgroundGreenReduction = 3;
    /// <summary>$A7:CB07/CB19/CB1B: dark green, red indicator and gold indicator selected intensities.</summary>
    private const int BackgroundGreenDark = 6, RedIndicator = 21, GoldIndicatorRed = 27, GoldIndicatorGreen = 18;

    internal static Bgr555 Color(int index)
    {
        if ((uint)index >= 112) throw new ArgumentOutOfRangeException(nameof(index));
        int row = index / 16, ink = index % 16;
        if (ink == 0) return row is 1 or 2 ? DoorTransparentMetadata : Bgr555.Black;
        if (ink == 15) return Bgr555.Black;
        return row switch
        {
            0 => Common(ink),
            1 => GreenDoor(ink),
            2 => MagentaDoor(ink),
            3 => BlueDoor(ink),
            4 => Foreground(ink),
            5 => Background(ink),
            _ => ink == 14 ? White : Neutral(CommonNeutral),
        };
    }
    private static Bgr555 Common(int ink) => ink switch
    {
        >= 1 and <= 3 => Gold(ink - 1),
        >= 4 and <= 7 => CommonMetal(ink - 4),
        8 => GreenHighlight, 9 => MagentaMiddle, 13 => CommonBlueAccent,
        10 or 12 or 14 => White, 11 => Bgr555.Black,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    private static Bgr555 GreenDoor(int ink) => ink switch
    {
        1 => GreenHighlight, 2 => GreenMiddle, 3 => GreenDark,
        >= 4 and <= 7 => CommonMetal(ink - 4),
        8 => Neutral(CommonNeutral), 9 => Neutral(CommonShadow), 10 => Neutral(CommonOutline),
        11 => Bgr555.Black, 12 => White, 13 => Gold(0), 14 => Rgb(31, 0, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    private static Bgr555 MagentaDoor(int ink) => ink switch
    {
        1 => Rgb(MagentaLight, MagentaLightGreen, MagentaLight),
        2 => MagentaMiddle, 3 => Rgb(MagentaDarkRed, 0, MagentaDarkBlue),
        14 => White, _ => Neutral(CommonNeutral),
    };
    private static Bgr555 BlueDoor(int ink) => ink switch
    {
        1 => Rgb(DoorBlueLightRed, DoorBlueLightGreen, DoorBlueBright),
        2 => Rgb(DoorBlueMiddleRed, DoorBlueMiddleGreen, DoorBlueBright),
        3 => Rgb(DoorBlueDarkRed, DoorBlueDarkGreen, DoorBlueDark),
        14 => White, _ => Neutral(CommonNeutral),
    };
    private static Bgr555 Foreground(int ink) => ink switch
    {
        >= 1 and <= 3 => Warm(ShipLight - ShadeStep * (ink - 1), GreenMaterialTint),
        >= 4 and <= 6 => Warm(ShipLight - ShadeStep * (ink - 4)),
        7 => Rgb(ForegroundOutline, ForegroundOutline, ForegroundOutlineBlue),
        >= 8 and <= 11 => Cool(Math.Max(ForegroundOutline, ShipLight - ShadeStep * (ink - 8))),
        12 => GreenMiddle, 13 => GreenHighlight, 14 => White,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    private static Bgr555 Background(int ink) => ink switch
    {
        1 => Subtract(GreenMiddle, BackgroundGreenReduction), 2 => GreenDark,
        3 => Rgb(0, BackgroundGreenDark, 0),
        >= 4 and <= 7 => Warm(Math.Max(ForegroundOutline, ShipLight - BackgroundWarmReduction - ShadeStep * (ink - 4))),
        >= 8 and <= 11 => Cool(Math.Max(BackgroundCoolFloor, ShipLight - BackgroundCoolReduction - ShadeStep * (ink - 8))),
        12 => Rgb(RedIndicator, 0, 0), 13 => Rgb(GoldIndicatorRed, GoldIndicatorGreen, 0), 14 => White,
        _ => throw new ArgumentOutOfRangeException(nameof(ink)),
    };
    private static Bgr555 Gold(int shade)
    {
        int red = shade switch { 0 => 31, 1 => GoldMiddleRed, _ => GoldDarkRed };
        int green = GoldDarkGreen + (red - GoldDarkRed) * (GoldLightGreen - GoldDarkGreen) / (31 - GoldDarkRed);
        return Rgb(red, green, 0);
    }
    private static Bgr555 CommonMetal(int shade)
    {
        (int red, int green) = shade switch
        {
            0 => (MetalLightRed, MetalLightGreen), 1 => (MetalMiddleRed, MetalMiddleGreen),
            2 => (MetalDeepRed, MetalDeepGreen), _ => (MetalDarkRed, MetalDarkGreen),
        };
        return Rgb(red, green, green + (shade < 2 ? MetalLightBlueTint : MetalDarkBlueTint));
    }
    private static Bgr555 Warm(int intensity, int greenTint = 0) =>
        Rgb(intensity, intensity + greenTint, Math.Max(WarmBlueFloor, intensity - WarmBlueSeparation));
    private static Bgr555 Cool(int intensity) => Rgb(intensity, intensity, intensity + CoolBlueTint);
    private static Bgr555 Subtract(Bgr555 color, int amount) =>
        Rgb(Math.Max(0, (color.Red) - amount), Math.Max(0, (color.Green) - amount), Math.Max(0, (color.Blue) - amount));
    private static Bgr555 GreenDark => Rgb(GreenDarkRed, GreenDarkRed * 2, 0);
    private static Bgr555 Neutral(int intensity) => Rgb(intensity, intensity, intensity);
    private static Bgr555 White => Neutral(31);
    private static Bgr555 Rgb(int red, int green, int blue) => new Bgr555(red, green, blue);
}
