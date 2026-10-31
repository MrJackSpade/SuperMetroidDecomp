using static SuperMetroid.Core.Samus.Abilities.PowerBombExplosionDefinitions;

namespace SuperMetroid.Core.Samus.Abilities;

/// <summary>The afterglow steps of a Crystal Flash's explosion.</summary>
public static class PowerBombExplosionAfterglow
{
    /// <summary>
    /// Compiled $88:A35D wake criterion for a stock Crystal Flash: the maximum
    /// native component is 14+row for the four reachable $88:8D85 rows. Keeping
    /// this operand independent of editable RGB preserves the native fade count,
    /// signed-timer cadence and cleanup frame without storing presentation colors.
    /// </summary>
    public static int CrystalFlashAfterglowSteps(int consumedRadius)
    {
        if ((uint)consumedRadius >= CrystalFlashRadiusLimit)
            throw new ArgumentOutOfRangeException(nameof(consumedRadius),
                "Crystal Flash consumes a radius below its compiled expansion limit.");
        return WordWrap.LowByte(CrystalFlashInitialFadeSteps + (consumedRadius >> CrystalFlashColorRadiusShift));
    }
}
