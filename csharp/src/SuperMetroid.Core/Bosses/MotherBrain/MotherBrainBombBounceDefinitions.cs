namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// $86:C550-C563, the Mother Brain bomb's per-bounce Y acceleration selected by its bounce stage.
/// Stage zero uses the ordinary fall gravity of 7; each later bounce's gravity grows by sixteen
/// times the bounce number, capped at 64 per bounce (16, 32, 64, 112, 176, 240, 304, 368); the
/// stage after the eighth bounce is the zero expiry sentinel.
/// </summary>
internal static class MotherBrainBombBounceDefinitions
{
    /// <summary>Stages zero through nine, including the expiry sentinel.</summary>
    internal const int StageCount = 10;
    /// <summary>The ordinary fall gravity, also used before the first bounce.</summary>
    internal const ushort FallAcceleration = 0x0007;

    /// <summary>Base gravity increment accumulated after each bounce.</summary>
    private const int GrowthUnit = 16;
    /// <summary>Caps each later bounce's additional increment at four growth units.</summary>
    private const int MaximumGrowthSteps = 4;

    /// <summary>Returns native acceleration for stages zero through eight, then zero for expiry at stage nine.</summary>
    internal static ushort YAcceleration(int stage)
    {
        if ((uint)stage >= StageCount) throw new ArgumentOutOfRangeException(nameof(stage));
        if (stage == 0) return FallAcceleration;
        if (stage == StageCount - 1) return 0;
        int acceleration = GrowthUnit;
        for (int bounce = 1; bounce < stage; bounce++)
            acceleration += GrowthUnit * Math.Min(bounce, MaximumGrowthSteps);
        return (ushort)acceleration;
    }
}
