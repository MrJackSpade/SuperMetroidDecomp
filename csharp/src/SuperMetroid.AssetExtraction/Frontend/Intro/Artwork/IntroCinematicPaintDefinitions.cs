using SuperMetroid.Core.Assets;
namespace SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;

/// <summary>Exact intro paint composition. Reviewed categorical material/endpoints and policies
/// specify this exact painting; all duplicated slots and described shade relationships calculate. Pixel, timing and gameplay scope is excluded.</summary>
internal static class IntroCinematicPaintDefinitions
{
    /// <summary>The background's base colour and flat rows4/5/6/10/11: selected repeated neutral fill intensity.</summary>
    internal const int Fill = 24;
    /// <summary>Selected text gray.</summary>
    internal const int TextGray = 10;
    /// <summary>Main sepia dark level, two final outline steps and blue deficit.</summary>
    internal const int SepiaDark = 8, SepiaOutlineStep = 2, SepiaBlueDeficit = 2;
    /// <summary>Six main sepia levels including both endpoints.</summary>
    internal const int SepiaIntervals = 5;
    /// <summary>Selected warmer material red/green offsets.</summary>
    internal const int WarmTint = 1, StrongWarmTint = 2;
    /// <summary>Plate highlight is one RGB5 level below the shared5739 paint.</summary>
    internal const int PlateShade = 1;
    /// <summary>Selected neutral and near-neutral material levels.</summary>
    internal const int SceneHighlight = 29, SceneDark = 7, SceneDarkBlue = 6,
        Contour = 4, MiddleNeutral = 9, LowNeutral = 6, DeepContour = 2, PlateNeutral = 8, PlateDarkNeutral = 5;
    /// <summary>Four ceiling gamma2 shadows from maximum green to this endpoint.</summary>
    internal const int FontShadowDark = 11;
    /// <summary>Zero-ink tint endpoints; green gamma2 and blue hue scaling derive the middle.</summary>
    internal const int FontZeroGreenDark = 12, FontZeroBlue = 14;
    /// <summary>Repeating four-level neutral cycle, with one-level brightness steps.</summary>
    internal const int WhiteCycleLength = 4, WhiteCycleStep = 1;
    /// <summary>Selected dark red object accent.</summary>
    internal const int ObjectAccentRed = 4, ObjectAccentBlue = 1;
    /// <summary>Crossfade blue endpoint and green accent channels.</summary>
    internal const int CrossfadeBlueDark = 4, CrossfadeAccentGreen = 14, CrossfadeAccentBlue = 5;
    /// <summary>Equal decrement derived from full blue and the selected dark endpoint over three intervals.</summary>
    internal const int CrossfadeBlueStep = (31 - CrossfadeBlueDark) / 3;
    /// <summary>Helmet red endpoints and hue; middle red follows gamma2.</summary>
    internal const int HelmetRedLight = 24, HelmetRedDark = 5, HelmetGreen = 5, HelmetBlue = 5;
    /// <summary>Cyan cheek light and fixed channel shadow decrement.</summary>
    internal const int CheekRed = 13, CheekGreen = 20, CheekBlue = 21, CheekShade = 6;
    /// <summary>Central visor green gamma2 endpoints and linear blue endpoints.</summary>
    internal const int VisorGreenLight = 10, VisorGreenDark = 2, VisorBlueLight = 12, VisorBlueDark = 6;
    /// <summary>Bright visor's green/blue, with zero red.</summary>
    internal const int VisorGlintGreen = 28, VisorGlintBlue = 22;
    /// <summary>Blue-only outline intensity.</summary>
    internal const int PortraitOutlineBlue = 5;

    /// <summary>The transparent colour of every sprite colour set: copied transparent compatibility word.</summary>
    internal static readonly RgbColor ObjectZero = new(0, 0, 14);
    /// <summary>Shoulder/edge paint.</summary>
    internal static readonly RgbColor Shoulder = new(10, 13, 5);
    /// <summary>Dark cyan contour.</summary>
    internal static readonly RgbColor PortraitContour = new(2, 5, 5);
    /// <summary>Deep cool silhouette paint.</summary>
    internal static readonly RgbColor PortraitDeep = new(2, 2, 7);
}
