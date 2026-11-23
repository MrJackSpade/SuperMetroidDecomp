namespace SuperMetroid.Core.Fx.ColourEffects;

/// <summary>
/// Definitions shared by Norfair's heat glow and Samus's heat shimmer.
/// </summary>
public static partial class ColourEffectHeatData
{
    /// <summary>The damage the heat deals unprotected Samus every update: a quarter of one energy, no whole energy.</summary>
    public static readonly EnergyAmount DamagePerUpdate = EnergyAmount.FromEnergyFraction(1, 4);

    /// <summary>
    /// Energy must be greater than this before the heat condition queues another environmental
    /// damage sound; this prevents the sound from obscuring low-health warnings.
    /// </summary>
    public const int DamageSoundEnergyThreshold = 70;

    /// <summary>
    /// The updates on which unprotected Samus in a heated room queues the heat-damage sound, counted on the frame counter
    /// at the period's first place, while her energy is above <see cref="DamageSoundEnergyThreshold"/>.
    /// </summary>
    public static readonly UpdateCadence HeatDamageSound = new(8, 0);

    /// <summary>The heat-damage sound's request priority.</summary>
    public static readonly SoundRequestPriority DamageSoundPriority = SoundRequestPriority.Normal;
}
