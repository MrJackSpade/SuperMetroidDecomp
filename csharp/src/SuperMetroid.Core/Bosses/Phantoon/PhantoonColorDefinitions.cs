namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's health bands, each showing its body in its own colours.</summary>
public static class PhantoonColorDefinitions
{
    /// <summary>The health-dependent body colours, one per health band.</summary>
    public const int HealthBandCount = 8;

    /// <summary>The health each band spans, from his last point of health upward; the last band holds the rest (#1940).</summary>
    public const int HealthPerBand = 312;

    /// <summary>The steps his body's fades in and out take, every one but the fade-in at a flame rain (#2251).</summary>
    public const int BodyFadeSteps = 12;

    /// <summary>The steps his fade-in as he turns solid during a flame rain takes.</summary>
    public const int FlameRainFadeInSteps = 1;

    /// <summary>The steps the Wrecked Ship's power takes to come on over his room once he dies.</summary>
    public const int PowerOnSteps = 12;
}
