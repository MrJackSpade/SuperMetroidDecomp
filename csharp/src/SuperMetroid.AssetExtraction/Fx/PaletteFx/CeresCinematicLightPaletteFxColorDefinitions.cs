using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// The records of the cutscene gunship engine's and the Ceres navigation lights' effects and the colours each writes,
/// for the import.
/// </summary>
/// <remarks>
/// The two navigation-light effects start at their own records but share one loop of records.
/// </remarks>
public static class CeresCinematicLightPaletteFxColorDefinitions
{
    /// <summary>Each gunship-engine record writes one live BGR555 color.</summary>
    private const int GunshipEngineColorsPerFrame = 1;
    /// <summary>Each Ceres navigation-light record writes two live BGR555 colors.</summary>
    private const int NavigationLightsColorsPerFrame = 2;

    /// <summary>The gunship engine's records and the colour each writes.</summary>
    public static PaletteFxColorFrames GunshipEngineFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.CutsceneGunshipEngineFlicker, GunshipEngineColorsPerFrame);

    /// <summary>The navigation lights' records and the colours each writes.</summary>
    public static PaletteFxColorFrames NavigationLightsFrames { get; } = PaletteFxColorFrames.Of(ColourEffectId.CutsceneCeresNavigationLightsSprite, NavigationLightsColorsPerFrame);
}
