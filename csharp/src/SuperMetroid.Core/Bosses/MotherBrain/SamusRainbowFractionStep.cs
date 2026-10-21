namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The rainbow palette fraction after one step, and whether the step completed a frame.</summary>
/// <param name="Fraction">The fraction.</param>
/// <param name="FrameCompleted">The frame completed.</param>
[IndependentFlag("whether the fraction step completed a rainbow frame, a fact of the accumulated fraction")]
public readonly record struct SamusRainbowFractionStep(int Fraction, bool FrameCompleted);
