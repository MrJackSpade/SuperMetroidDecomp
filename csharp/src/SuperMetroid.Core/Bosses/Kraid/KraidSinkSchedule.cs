namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>Compiled mechanical steps in Kraid's sinking schedule.</summary>
internal static class KraidSinkSchedule
{
    /// <summary>
    /// The platform crumble Kraid's body sinking to <paramref name="y"/> starts, or null when none does (#1889): six of the
    /// rows eight pixels apart that he sinks through crumble a platform block, and the rest crumble nothing.
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
            _ => null,
        };
    }
}
