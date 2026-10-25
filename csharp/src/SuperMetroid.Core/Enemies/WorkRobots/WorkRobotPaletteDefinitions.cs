namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>The shape of the Work Robot's colour cycle; its cadence is <see cref="WorkRobotPaletteTimingDefinitions"/>.</summary>
public static class WorkRobotPaletteDefinitions
{
    /// <summary>The colours each cycle record replaces.</summary>
    public const int ColorCount = 4;

    /// <summary>Each record replaces the colours from index nine of the robot's colour set, nine through twelve.</summary>
    public const int FirstAnimatedColor = 9;
}
