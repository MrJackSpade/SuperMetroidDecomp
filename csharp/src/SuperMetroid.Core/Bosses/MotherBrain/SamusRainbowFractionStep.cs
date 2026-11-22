namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>The part of a rainbow colour frame left after one update's addition, and whether the addition completed a frame.</summary>
/// <param name="Fraction">The part of a frame left over.</param>
/// <param name="FrameCompleted">Whether the addition completed a whole frame.</param>
[IndependentFlag("whether the fraction step completed a rainbow frame, a fact of the accumulated fraction")]
public readonly partial record struct SamusRainbowFractionStep(RainbowFramePart Fraction, bool FrameCompleted);
