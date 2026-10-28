using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Enemies.Artwork;

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
    internal static readonly RgbColor DoorTransparentMetadata = RgbColor.FromWord(0x2003);
    /// <summary>$A7:CA63-CA67: common gold door/machine accent endpoints and selected middle red.</summary>
    internal const int GoldMiddleRed = 23, GoldDarkRed = 12, GoldLightGreen = 22, GoldDarkGreen = 5;
    /// <summary>$A7:CA69-CA6F, repeated atCA89-CA8F: warm common-room machinery red/green intensities.</summary>
    internal const int MetalLightRed = 27, MetalLightGreen = 21, MetalMiddleRed = 19, MetalMiddleGreen = 13,
        MetalDeepRed = 14, MetalDeepGreen = 9, MetalDarkRed = 6, MetalDarkGreen = 4;
    /// <summary>$A7:CA69-CA6F: the two lighter steel facets add two blue levels to green; deeper facets add one.</summary>
    internal const int MetalLightBlueTint = 2, MetalDarkBlueTint = 1;
    /// <summary>$A7:CA71/CA83/CAFB: common green indicator/door highlight paint.</summary>
    internal static readonly RgbColor GreenHighlight = RgbColor.FromWord(0x0bb1);
    /// <summary>$A7:CA85/CAF9: common green door/indicator middle paint.</summary>
    internal static readonly RgbColor GreenMiddle = RgbColor.FromWord(0x1ea9);
    /// <summary>$A7:CA87/CB05: green door shadow; green is twice red and blue is absent.</summary>
    internal const int GreenDarkRed = 5;
    /// <summary>$A7:CA73/CAA5: shared magenta door/common indicator paint.</summary>
    internal static readonly RgbColor MagentaMiddle = RgbColor.FromWord(0x48fb);
    /// <summary>$A7:CAA3/CAA7: magenta door glint and dark red-blue outline.</summary>
    internal const int MagentaLight = 28, MagentaLightGreen = 21, MagentaDarkRed = 22, MagentaDarkBlue = 6;
    /// <summary>$A7:CA7B: additional copied common-room blue accent paint; not assigned a visible role by the source metatile packet.</summary>
    internal static readonly RgbColor CommonBlueAccent = RgbColor.FromWord(0x44e5);
    /// <summary>$A7:CA91/CAA9-CABB/CAC9-CADB/CB23-CB3B: common neutral placeholder/material ink.</summary>
    internal const int CommonNeutral = 24;
    /// <summary>$A7:CA93-CA95: independent common-room neutral shadow and outline intensities.</summary>
    internal const int CommonShadow = 7, CommonOutline = 4;
    /// <summary>$A7:CAC3-CAC7: blue door light/middle/dark red and green, shared bright blue and darker blue.</summary>
    internal const int DoorBlueLightRed = 18, DoorBlueLightGreen = 21, DoorBlueMiddleRed = 7,
        DoorBlueMiddleGreen = 14, DoorBlueDarkRed = 3, DoorBlueDarkGreen = 8,
        DoorBlueBright = 28, DoorBlueDark = 19;
    /// <summary>$A7:CAE3-CAF7/CB09-CB17: powered wall/conduit shade step and common bright intensity.</summary>
    internal const int ShipLight = 20, ShadeStep = 6;
    /// <summary>$A7:CAE3-CAE7: green material raises green three levels over the warm wall red intensity.</summary>
    internal const int GreenMaterialTint = 3;
    /// <summary>$A7:CAE9-CAED/CB09-CB0F: warm wall material blue separation and shadow floor.</summary>
    internal const int WarmBlueSeparation = 5, WarmBlueFloor = 1;
    /// <summary>$A7:CAF1-CAF7/CB11-CB17: cool conduit material adds seven blue levels.</summary>
    internal const int CoolBlueTint = 7;
    /// <summary>$A7:CAEF/CAF7: foreground outline intensity and independent warm-outline blue.</summary>
    internal const int ForegroundOutline = 4, ForegroundOutlineBlue = 5;
    /// <summary>$A7:CB09/CB11: background warm/cool lighting reductions from the foreground highlight.</summary>
    internal const int BackgroundWarmReduction = 2, BackgroundCoolReduction = 3;
    /// <summary>$A7:CB17: selected darkest background conduit red/green intensity, distinct from the warm material's blue floor.</summary>
    internal const int BackgroundCoolFloor = 1;
    /// <summary>$A7:CB03: background green highlight subtracts this common shade from the green door middle ink.</summary>
    internal const int BackgroundGreenReduction = 3;
    /// <summary>$A7:CB07/CB19/CB1B: dark green, red indicator and gold indicator selected intensities.</summary>
    internal const int BackgroundGreenDark = 6, RedIndicator = 21, GoldIndicatorRed = 27, GoldIndicatorGreen = 18;
}
