using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>
/// The seven powered Wrecked Ship BG material palettes.
/// Powering the ship uniformly interpolates the palette toward these selected paints.
/// CRE door/machinery illustrations and area wall/conduit/indicator illustrations
/// select the corresponding ink identities; this palette contains no timing or
/// physical trajectory. Remaining named hues and composition policies are that
/// specific illustration content. Replacing them would invent different paint.
/// All repeats, shared channels, ramps, lighting offsets and fills calculate.
/// </summary>
internal static class WreckedShipPowerPaintDefinitions
{
    /// <summary>Exact copied ink-zero metadata of the green and magenta door sets; background ink zero is transparent.</summary>
    internal static readonly RgbColor DoorTransparentMetadata = new(3, 0, 8);
    /// <summary>Common gold door/machine accent endpoints and selected middle red.</summary>
    internal const int GoldMiddleRed = 23, GoldDarkRed = 12, GoldLightGreen = 22, GoldDarkGreen = 5;
    /// <summary>Repeated atCA89-CA8F: warm common-room machinery red/green intensities.</summary>
    internal const int MetalLightRed = 27, MetalLightGreen = 21, MetalMiddleRed = 19, MetalMiddleGreen = 13,
        MetalDeepRed = 14, MetalDeepGreen = 9, MetalDarkRed = 6, MetalDarkGreen = 4;
    /// <summary>The two lighter steel facets add two blue levels to green; deeper facets add one.</summary>
    internal const int MetalLightBlueTint = 2, MetalDarkBlueTint = 1;
    /// <summary>Common green indicator/door highlight paint.</summary>
    internal static readonly RgbColor GreenHighlight = new(17, 29, 2);
    /// <summary>Common green door/indicator middle paint.</summary>
    internal static readonly RgbColor GreenMiddle = new(9, 21, 7);
    /// <summary>Green door shadow; green is twice red and blue is absent.</summary>
    internal const int GreenDarkRed = 5;
    /// <summary>Shared magenta door/common indicator paint.</summary>
    internal static readonly RgbColor MagentaMiddle = new(27, 7, 18);
    /// <summary>Magenta door glint and dark red-blue outline.</summary>
    internal const int MagentaLight = 28, MagentaLightGreen = 21, MagentaDarkRed = 22, MagentaDarkBlue = 6;
    /// <summary>Additional copied common-room blue accent paint; not assigned a visible role by the source metatile packet.</summary>
    internal static readonly RgbColor CommonBlueAccent = new(5, 7, 17);
    /// <summary>Common neutral placeholder/material ink.</summary>
    internal const int CommonNeutral = 24;
    /// <summary>Independent common-room neutral shadow and outline intensities.</summary>
    internal const int CommonShadow = 7, CommonOutline = 4;
    /// <summary>Blue door light/middle/dark red and green, shared bright blue and darker blue.</summary>
    internal const int DoorBlueLightRed = 18, DoorBlueLightGreen = 21, DoorBlueMiddleRed = 7,
        DoorBlueMiddleGreen = 14, DoorBlueDarkRed = 3, DoorBlueDarkGreen = 8,
        DoorBlueBright = 28, DoorBlueDark = 19;
    /// <summary>Powered wall/conduit shade step and common bright intensity.</summary>
    internal const int ShipLight = 20, ShadeStep = 6;
    /// <summary>Green material raises green three levels over the warm wall red intensity.</summary>
    internal const int GreenMaterialTint = 3;
    /// <summary>Warm wall material blue separation and shadow floor.</summary>
    internal const int WarmBlueSeparation = 5, WarmBlueFloor = 1;
    /// <summary>Cool conduit material adds seven blue levels.</summary>
    internal const int CoolBlueTint = 7;
    /// <summary>Foreground outline intensity and independent warm-outline blue.</summary>
    internal const int ForegroundOutline = 4, ForegroundOutlineBlue = 5;
    /// <summary>Background warm/cool lighting reductions from the foreground highlight.</summary>
    internal const int BackgroundWarmReduction = 2, BackgroundCoolReduction = 3;
    /// <summary>Selected darkest background conduit red/green intensity, distinct from the warm material's blue floor.</summary>
    internal const int BackgroundCoolFloor = 1;
    /// <summary>Background green highlight subtracts this common shade from the green door middle ink.</summary>
    internal const int BackgroundGreenReduction = 3;
    /// <summary>Dark green, red indicator and gold indicator selected intensities.</summary>
    internal const int BackgroundGreenDark = 6, RedIndicator = 21, GoldIndicatorRed = 27, GoldIndicatorGreen = 18;
}
