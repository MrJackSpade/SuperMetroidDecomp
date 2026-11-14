namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The cadence of the Work Robot's glow. The colours each step shows are the import's shades (#1512); this catalog
/// owns how many steps the glow has and how long each shows.
/// </summary>
internal static class WorkRobotGlowDefinitions
{
    /// <summary>Number of steps in the glow's cycle; after the last it starts again at the first.</summary>
    public const int StepCount = 6;

    /// <summary>The updates each glow step shows for: a three-step cadence of 64 updates then 16 and 16.</summary>
    /// <remarks>
    /// Independently reviewed for #1165 against the Japanese and North American v1.0 release: the six steps' timers repeat a
    /// three-step cadence, 64 ticks at i%3==0 and 16 otherwise. After the sixth step the cycle restarts at the first;
    /// there is never a seventh duration.
    /// </remarks>
    internal static readonly int[] Durations = [64, 16, 16, 64, 16, 16];
}
