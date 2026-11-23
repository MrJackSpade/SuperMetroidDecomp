namespace SuperMetroid.Core.Fx;

/// <summary>How far and how fast one tide moves a liquid surface (#2557).</summary>
/// <param name="PeakPixels">How far the tide moves the surface at the wave's crest, in pixels, either side of its base line.</param>
/// <param name="AtOrBelowPhaseDelta">How far the tide turns each update while its surface is at or below its base line.</param>
/// <param name="AbovePhaseDelta">How far the tide turns each update while its surface is above its base line.</param>
public sealed partial record LiquidTideMotion(int PeakPixels, RotationAngle AtOrBelowPhaseDelta, RotationAngle AbovePhaseDelta);
