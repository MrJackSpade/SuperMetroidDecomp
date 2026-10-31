namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Cartridge definitions shared by the Norfair palette animator and Samus-in-heat owner.
/// </summary>
public static class PaletteFxHeatData
{
    /// <summary>Fractional 16.16 damage added by $8D:E379 on every unprotected frame.</summary>
    public const ushort SubdamagePerFrame = 16384;

    /// <summary>
    /// Energy must be greater than $0046 before $8D:E379 queues another environmental
    /// damage sound; this prevents the sound from obscuring low-health warnings.
    /// </summary>
    public const ushort DamageSoundEnergyThreshold = 70;

    /// <summary>$8D:E379 calls <c>QueueSfx3_Max6</c>.</summary>
    public static readonly SoundRequestPriority DamageSoundMaximumQueued = SoundRequestPriority.Normal;
}
