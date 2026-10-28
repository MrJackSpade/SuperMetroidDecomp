using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>Exact $8C:E3E9-E5E8 intro paint composition. Reviewed categorical material/endpoints and policies
/// specify this exact painting; all duplicated slots and described shade relationships calculate. Pixel, timing and gameplay scope is excluded.</summary>
internal static class IntroCinematicPaintDefinitions
{
    /// <summary>$8C:E3ED and flat rows4/5/6/10/11: selected repeated neutral fill intensity.</summary>
    internal const int Fill = 24;
    /// <summary>$8C:E40F/E415: selected text gray.</summary>
    internal const int TextGray = 10;
    /// <summary>$8C:E435-E43A: main sepia dark level, two final outline steps and blue deficit.</summary>
    internal const int SepiaDark = 8, SepiaOutlineStep = 2, SepiaBlueDeficit = 2;
    /// <summary>$8C:E42B-E434: six main sepia levels including both endpoints.</summary>
    internal const int SepiaIntervals = 5;
    /// <summary>$8C:E4D5/E4DD/E58D: selected warmer material red/green offsets.</summary>
    internal const int WarmTint = 1, StrongWarmTint = 2;
    /// <summary>$8C:E4D1/E4D9/E50B/E5DB: plate highlight is one RGB5 level below the shared5739 paint.</summary>
    internal const int PlateShade = 1;
    /// <summary>$8C:E4CB-E4D8/E5CF-E5D8: selected neutral and near-neutral material levels.</summary>
    internal const int SceneHighlight = 29, SceneDark = 7, SceneDarkBlue = 6,
        Contour = 4, MiddleNeutral = 9, LowNeutral = 6, DeepContour = 2, PlateNeutral = 8, PlateDarkNeutral = 5;
    /// <summary>$8C:E3EF/E3F7/E3FF/E407: four ceiling gamma2 shadows from maximum green to this endpoint.</summary>
    internal const int FontShadowDark = 11;
    /// <summary>$8C:E3F1/E3F9/E401: zero-ink tint endpoints; green gamma2 and blue hue scaling derive the middle.</summary>
    internal const int FontZeroGreenDark = 12, FontZeroBlue = 14;
    /// <summary>$8C:E4EB-E508: repeating four-level neutral cycle, with one-level brightness steps.</summary>
    internal const int WhiteCycleLength = 4, WhiteCycleStep = 1;
    /// <summary>$8C:E51B: selected dark red object accent.</summary>
    internal const int ObjectAccentRed = 4, ObjectAccentBlue = 1;
    /// <summary>$8C:E5C1/E5C5: crossfade blue endpoint and green accent channels.</summary>
    internal const int CrossfadeBlueDark = 4, CrossfadeAccentGreen = 14, CrossfadeAccentBlue = 5;
    /// <summary>$8C:E5BB-E5C2: equal decrement derived from full blue and the selected dark endpoint over three intervals.</summary>
    internal const int CrossfadeBlueStep = (31 - CrossfadeBlueDark) / 3;
    /// <summary>$8C:E45B/E465/E467: helmet red endpoints and hue; middle red follows gamma2.</summary>
    internal const int HelmetRedLight = 24, HelmetRedDark = 5, HelmetGreen = 5, HelmetBlue = 5;
    /// <summary>$8C:E457/E459/E453: cyan cheek light and fixed channel shadow decrement.</summary>
    internal const int CheekRed = 13, CheekGreen = 20, CheekBlue = 21, CheekShade = 6;
    /// <summary>$8C:E45F/E463/E45D: central visor green gamma2 endpoints and linear blue endpoints.</summary>
    internal const int VisorGreenLight = 10, VisorGreenDark = 2, VisorBlueLight = 12, VisorBlueDark = 6;
    /// <summary>$8C:E451: bright visor's green/blue, with zero red.</summary>
    internal const int VisorGlintGreen = 28, VisorGlintBlue = 22;
    /// <summary>$8C:E44B: blue-only outline intensity.</summary>
    internal const int PortraitOutlineBlue = 5;

    /// <summary>$8C:E4E9 and every object-row zero: copied transparent compatibility word.</summary>
    internal static readonly RgbColor ObjectZero = RgbColor.FromWord(0x3800);
    /// <summary>$8C:E44D: shoulder/edge paint.</summary>
    internal static readonly RgbColor Shoulder = RgbColor.FromWord(0x15aa);
    /// <summary>$8C:E455: dark cyan contour.</summary>
    internal static readonly RgbColor PortraitContour = RgbColor.FromWord(0x14a2);
    /// <summary>$8C:E461: deep cool silhouette paint.</summary>
    internal static readonly RgbColor PortraitDeep = RgbColor.FromWord(0x1c42);
}
