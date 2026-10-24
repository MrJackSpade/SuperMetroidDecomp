namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The fixed timing of the <see cref="SoundAcceptanceClock{TSound}"/>: how many sound libraries it serves, how many of
/// the sound program's samples one update holds, and how often the clock steps and checks the requests.
/// </summary>
/// <remarks>Evidence: the sound program's 500-Hz timer and its request read (<c>$1621</c>).</remarks>
[CompatibilityService("the fixed timing of the clock on which the sound libraries accept requests")]
public static class SoundClockTiming
{
    /// <summary>The sound libraries the clock accepts requests for, addressed by zero-based index.</summary>
    public const int LibraryCount = 3;

    /// <summary>The sound clock's samples in one 60-Hz update: 32,040 / 60.</summary>
    public const int SamplesPerUpdate = 534;

    /// <summary>Samples between clock steps (the sound program's 500-Hz timer).</summary>
    public const int SamplesPerStep = 64;

    /// <summary>The most steps one update can contain: one pending at its start plus one per whole step period.</summary>
    public const int MaximumStepsPerUpdate = SamplesPerUpdate / SamplesPerStep + 1;

    /// <summary>Added to the request-check timer on every step; its overflow, every eighth step, checks the requests.</summary>
    public const int CheckTimerIncrement = TickAccumulator.Period / 8;
}
