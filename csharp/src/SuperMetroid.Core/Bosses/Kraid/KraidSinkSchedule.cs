namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>The platform crumbles of Kraid's sinking body.</summary>
internal static class KraidSinkSchedule
{
    /// <summary>
    /// The platform crumble Kraid's body sinking to <paramref name="y"/> starts, or null when none does (#1889): six of the
    /// heights he sinks through crumble a platform block (<see cref="KraidArenaDefinitions.SinkCrumbles"/>), and the rest
    /// crumble nothing.
    /// </summary>
    public static KraidSinkStep? StepAt(int y) =>
        KraidArenaDefinitions.SinkCrumbles.TryGetValue(y, out KraidSinkStep step) ? step : null;
}
