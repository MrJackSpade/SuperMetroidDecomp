namespace SuperMetroid.Core.Areas.Ceres;

/// <summary>A Ceres floor animation frame and one of its tile runs.</summary>
/// <param name="Frame">The frame.</param>
/// <param name="Row">The run, in the frame's drawing order.</param>
internal readonly record struct CeresFloorAnimationRow(CeresFloorAnimationFrame Frame, int Row);
