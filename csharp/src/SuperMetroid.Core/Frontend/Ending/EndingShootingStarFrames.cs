using static SuperMetroid.Core.Frontend.Ending.EndingShootingStarDefinitions;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Names and draws the ending shooting star's sheet frames (<see cref="EndingShootingStarDefinitions"/>).
/// </summary>
internal static class EndingShootingStarFrames
{
    /// <summary>The sheet frame showing a visible animation frame (#1392).</summary>
    internal static string SheetFrameName(int frame) => frame is > 0 and < FrameCount
        ? $"ShootingStar.{Math.Min(frame, FullStarFrame)}"
        : throw new ArgumentOutOfRangeException(nameof(frame));
}
