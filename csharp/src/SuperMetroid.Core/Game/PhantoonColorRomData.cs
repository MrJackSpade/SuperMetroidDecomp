namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A7 Phantoon visual palette targets and CGRAM geometry.</summary>
public static class PhantoonColorRomData
{
    /// <summary>Eight sixteen-color health palettes at $A7:CB41, selected by $A7:DC0F.</summary>
    public const int HealthBandsSource = 0xa7cb41;
    /// <summary>Number of health-dependent body palette bands stored at <see cref="HealthBandsSource"/>.</summary>
    public const int HealthBandCount = 8;
    /// <summary>Number of consecutive colors in each Phantoon health palette band.</summary>
    public const int HealthBandColorCount = 16;
    /// <summary>First CGRAM color index of Phantoon's sixteen-color body palette.</summary>
    public const int BodyDestination = 112;

    /// <summary>Sixteen-color materialization fade-out target at $A7:CA41.</summary>
    public const int FadeOutSource = 0xa7ca41;
    /// <summary>Number of colors in the materialization fade-out target palette.</summary>
    public const int FadeOutCount = 16;

    /// <summary>
    /// Wrecked Ship power-on target at $A7:CA61, interpolated by $A7:DC5A over
    /// CGRAM colors 0..111 after Phantoon's death.
    /// </summary>
    public const int PowerOnSource = 0xa7ca61;
    /// <summary>Number of CGRAM colors interpolated from the backdrop through color 111.</summary>
    public const int PowerOnCount = 112;
}
