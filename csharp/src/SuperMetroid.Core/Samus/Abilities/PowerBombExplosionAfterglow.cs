using SuperMetroid.Core.Samus.Projectiles;
using static SuperMetroid.Core.Samus.Abilities.PowerBombExplosionDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The afterglow steps of a Crystal Flash's explosion.</summary>
public static class PowerBombExplosionAfterglow
{
    /// <summary>
    /// Compiled afterglow wake criterion for a stock Crystal Flash: the maximum
    /// component is 14+step for the four reachable explosion colour steps. Keeping
    /// this criterion independent of editable RGB preserves the fade count,
    /// signed-timer cadence and cleanup frame without storing presentation colors.
    /// </summary>
    public static int CrystalFlashAfterglowSteps(PowerBombRadius consumedRadius)
    {
        if (!(consumedRadius < CrystalFlashRadiusLimit))
            throw new ArgumentOutOfRangeException(nameof(consumedRadius),
                "Crystal Flash consumes a radius below its compiled expansion limit.");
        return CrystalFlashInitialFadeSteps + consumedRadius.WholePixels / CrystalFlashColorBandPixels;
    }
}
