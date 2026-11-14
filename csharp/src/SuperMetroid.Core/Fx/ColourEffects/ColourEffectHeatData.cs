namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// Definitions shared by the Norfair palette animator and Samus-in-heat owner.
/// </summary>
public static class ColourEffectHeatData
{
    /// <summary>Fractional damage, in 1/65536ths of a unit, the heat condition adds on every unprotected frame.</summary>
    public const int SubdamagePerFrame = 16384;

    /// <summary>
    /// Energy must be greater than 70 before the heat condition queues another environmental
    /// damage sound; this prevents the sound from obscuring low-health warnings.
    /// </summary>
    public const int DamageSoundEnergyThreshold = 70;

    /// <summary>
    /// The updates on which unprotected Samus in a heated room queues the heat-damage sound: every eighth update of the
    /// frame counter, on its first place, while her energy is above <see cref="DamageSoundEnergyThreshold"/>.
    /// </summary>
    public static readonly UpdateCadence DamageSoundEveryEighthUpdate = new(8, 0);

    /// <summary>The heat-damage sound's request priority.</summary>
    public static readonly SoundRequestPriority DamageSoundPriority = SoundRequestPriority.Normal;
}
