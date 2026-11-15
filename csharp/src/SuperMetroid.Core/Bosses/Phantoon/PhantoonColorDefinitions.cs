namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Phantoon's health bands, each showing its body in its own colours.</summary>
public static class PhantoonColorDefinitions
{
    /// <summary>The health-dependent body colours, one per health band.</summary>
    public const int HealthBandCount = 8;

    /// <summary>The health each band spans, from his last point of health upward; the last band holds the rest (#1940).</summary>
    public const int HealthPerBand = 312;
}
