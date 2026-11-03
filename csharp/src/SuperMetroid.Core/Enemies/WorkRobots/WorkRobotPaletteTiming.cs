using static SuperMetroid.Core.Enemies.WorkRobots.WorkRobotPaletteTimingDefinitions;

namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>How long the Work Robot's glow shows each record.</summary>
internal static class WorkRobotPaletteTiming
{
    /// <summary>The updates the glow shows record <paramref name="record"/> for.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="record"/> is not one of the six records.</exception>
    public static int DurationOf(int record) => Durations[Ensure.BetweenInclusive(0, RecordCount - 1, record)];
}
