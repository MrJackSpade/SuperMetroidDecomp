namespace SuperMetroid.Core.Game;

/// <summary>Bank-$A7 Phantoon visual palette targets and CGRAM geometry.</summary>
public static class PhantoonColorRomData
{
    /// <summary>Eight sixteen-color health palettes at $A7:CB41, selected by $A7:DC0F.</summary>
    public const int HealthBandsSource = 0xa7cb41;
    public const int HealthBandCount = 8;
    public const int HealthBandColorCount = 16;
    public const int BodyDestination = 112;

    /// <summary>Sixteen-color materialization fade-out target at $A7:CA41.</summary>
    public const int FadeOutSource = 0xa7ca41;
    public const int FadeOutCount = 16;

    /// <summary>
    /// Wrecked Ship power-on target at $A7:CA61, interpolated by $A7:DC5A over
    /// CGRAM colors 0..111 after Phantoon's death.
    /// </summary>
    public const int PowerOnSource = 0xa7ca61;
    public const int PowerOnCount = 112;
    public const int PowerOnDestination = 0;
}
