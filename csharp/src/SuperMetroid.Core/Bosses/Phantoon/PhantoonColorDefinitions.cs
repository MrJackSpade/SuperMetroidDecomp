namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>The shapes of Phantoon's colour targets: its health palettes, its materialization fade and the ship's power-on.</summary>
public static class PhantoonColorDefinitions
{
    /// <summary>The health-dependent body palettes, one per health band.</summary>
    public const int HealthBandCount = 8;

    /// <summary>The colours in each health palette.</summary>
    public const int HealthBandColorCount = 16;

    /// <summary>The colours of the materialization fade-out target.</summary>
    public const int FadeOutCount = 16;

    /// <summary>The colours the Wrecked Ship's power-on fades after Phantoon's death: the backdrop through colour 111.</summary>
    public const int PowerOnCount = 112;
}
