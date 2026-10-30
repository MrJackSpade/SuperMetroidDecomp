namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>
/// Which of the seven Ceres floor animation frames the elevator platform and the Baby's capsule cycle through (#1467).
/// Each frame is a picture drawn over the turning floor (<see cref="CeresFloorAnimation"/>); the import pictures it.
/// </summary>
public static class CeresFloorAnimationDefinitions
{
    /// <summary>The three capsule frames, indexed by reflected phase 0, 1, 2.</summary>
    internal static readonly CeresFloorAnimationFrame[] BabyFrames =
        [CeresFloorAnimationFrame.BabyFrame0, CeresFloorAnimationFrame.BabyFrame1, CeresFloorAnimationFrame.BabyFrame2];

    /// <summary>The number of capsule phases; the capsule frames reflect across them, 0, 1, 2, 1.</summary>
    internal const int BabyPhaseCount = 4;

    /// <summary>The elevator platform's light and dark strips, indexed by platform frame.</summary>
    internal static readonly CeresFloorAnimationFrame[] PlatformFrames =
        [CeresFloorAnimationFrame.ElevatorLight, CeresFloorAnimationFrame.ElevatorDark];
}
