namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Cartridge definitions shared by the Norfair palette animator and Samus-in-heat owner.
/// </summary>
public static class PaletteFxHeatData
{
    /// <summary>Fractional 16.16 damage added by $8D:E379 on every unprotected frame.</summary>
    public const int SubdamagePerFrame = 16384;

    /// <summary>
    /// Energy must be greater than $0046 before $8D:E379 queues another environmental
    /// damage sound; this prevents the sound from obscuring low-health warnings.
    /// </summary>
    public const int DamageSoundEnergyThreshold = 70;

    /// <summary>The heat-damage sound's request priority.</summary>
    public static readonly SoundRequestPriority DamageSoundPriority = SoundRequestPriority.Normal;
}
