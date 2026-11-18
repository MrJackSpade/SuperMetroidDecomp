namespace SuperMetroid.Core.Enemies.WorkRobots;

/// <summary>
/// The holds of the Work Robot's glow (#2380). The colours each step shows are the import's shades (#1512); the order of the
/// steps is <see cref="WorkRobotGlowAnimations"/>.
/// </summary>
internal static class WorkRobotGlowDefinitions
{
    /// <summary>The updates the glow rests at either end of its run.</summary>
    internal const int EndHold = 64;

    /// <summary>The updates the glow shows each step between the ends of its run.</summary>
    internal const int PassingHold = 16;
}
