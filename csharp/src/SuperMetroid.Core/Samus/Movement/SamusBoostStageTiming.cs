namespace SuperMetroid.Core.Samus.Movement;

/// <summary>The Speed Booster stage a running cadence is and how the boost restarts on it (#2324).</summary>
/// <param name="Stage">The boost stage, zero through <c>FullStage</c>.</param>
/// <param name="CountdownAfterAdvance">The cadence countdown, 0 through 2, the boost restarts with after advancing onto the stage.</param>
internal sealed record SamusBoostStageTiming(int Stage, int CountdownAfterAdvance);
