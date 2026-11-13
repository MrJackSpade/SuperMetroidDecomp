namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled mechanical steps in Kraid's sinking schedule.</summary>
internal static class KraidSinkSchedule
{
    /// <summary>
    /// Kraid's sinking schedule: twenty-eight rows spaced eight pixels
    /// apart, terminated by an end marker. Six rows crumble platforms; the remaining rows
    /// run the distinct empty step. Where each row draws is presentation data.
    /// </summary>
    public static KraidSinkStep? StepAt(int y)
    {
        if (y < 304 || y > 520 || !KraidCadenceDefinitions.SinkRowEveryEighthPixel.IsDue(y))
            return null;
        return y switch
        {
            304 => KraidSinkStep.CrumbleLeftPlatformLeft,
            328 => KraidSinkStep.CrumbleRightPlatformMiddle,
            352 => KraidSinkStep.CrumbleRightPlatformLeft,
            384 => KraidSinkStep.CrumbleLeftPlatformRight,
            408 => KraidSinkStep.CrumbleLeftPlatformMiddle,
            432 => KraidSinkStep.CrumbleRightPlatformRight,
            _ => KraidSinkStep.NoOperation,
        };
    }
}
