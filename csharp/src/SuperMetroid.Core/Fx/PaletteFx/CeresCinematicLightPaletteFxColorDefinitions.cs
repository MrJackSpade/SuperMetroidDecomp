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
    /// <summary>The gunship-engine loop contains two timed records.</summary>
    public const int GunshipEngineFrameCount = 2;
    /// <summary>Each gunship-engine record writes one live BGR555 color.</summary>
    public const int GunshipEngineColorsPerFrame = 1;
    /// <summary>Bytes from one gunship-engine duration through its terminal wait.</summary>
    public const int GunshipEngineFrameByteCount = 6;

    /// <summary>The shared Ceres navigation-light loop contains fourteen timed records.</summary>
    public const int NavigationLightsFrameCount = 14;
    /// <summary>Each Ceres navigation-light record writes two live BGR555 colors.</summary>
    public const int NavigationLightsColorsPerFrame = 2;
    /// <summary>Bytes from one navigation-light duration through its terminal wait.</summary>
    public const int NavigationLightsFrameByteCount = 8;

    /// <summary>The gunship engine's records and the colour each writes.</summary>
    public static PaletteFxColorRows GunshipEngineRows { get; } = new(26, GunshipEngineFrameCount, GunshipEngineColorsPerFrame);

    /// <summary>The navigation lights' records and the colours each writes.</summary>
    public static PaletteFxColorRows NavigationLightsRows { get; } = new(28, NavigationLightsFrameCount, NavigationLightsColorsPerFrame);
}
