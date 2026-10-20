namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable mechanics for the cutscene gunship-engine and shared Ceres navigation-light
/// palette programs.
/// </summary>
/// <remarks>
/// The two navigation-light definitions retain their distinct native entry points but
/// converge on the cartridge's single shared fourteen-record loop at <c>$8D:C892</c>.
/// Their BGR555 colors remain live presentation data; this catalog owns palette placement,
/// timing, waits, and loop control.
/// </remarks>
public static class CeresCinematicLightPaletteFxColorDefinitions
{
    /// <summary>Each gunship-engine record writes one live BGR555 color.</summary>
    public const int GunshipEngineColorsPerFrame = 1;
    /// <summary>Each Ceres navigation-light record writes two live BGR555 colors.</summary>
    public const int NavigationLightsColorsPerFrame = 2;

    /// <summary>The gunship engine's records and the colour each writes.</summary>
    public static PaletteFxColorRows GunshipEngineRows { get; } = PaletteFxColorRows.Of(PaletteFxProgram.CutsceneGunshipEngineFlicker1, GunshipEngineColorsPerFrame);

    /// <summary>The navigation lights' records and the colours each writes.</summary>
    public static PaletteFxColorRows NavigationLightsRows { get; } = PaletteFxColorRows.Of(PaletteFxProgram.CommonCeresNavigationLights, NavigationLightsColorsPerFrame);
}
