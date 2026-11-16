using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Abilities.PowerBombExplosionDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The afterglow steps of a Crystal Flash's explosion.</summary>
public static class PowerBombExplosionAfterglow
{
    /// <summary>
    /// The updates Crystal Flash's afterglow fades for before the blast ends, one fade step on each:
    /// <see cref="PowerBombExplosionDefinitions.CrystalFlashShortestAfterglowUpdates"/>, and one more for each glow band
    /// (<see cref="PowerBombRadius.GlowColorBand"/>) the explosion had reached when it stopped. The shown colours never
    /// change how long it lasts.
    /// </summary>
    /// <param name="consumedRadius">The radius the explosion had reached when it stopped, below Crystal Flash's limit.</param>
    /// <exception cref="ArgumentOutOfRangeException">The radius is not below Crystal Flash's limit.</exception>
    public static int CrystalFlashAfterglowSteps(PowerBombRadius consumedRadius)
    {
        if (!(consumedRadius < CrystalFlashRadiusLimit))
            throw new ArgumentOutOfRangeException(nameof(consumedRadius),
                "Crystal Flash consumes a radius below its compiled expansion limit.");
        return CrystalFlashShortestAfterglowUpdates + consumedRadius.GlowColorBand;
    }
}
