namespace SuperMetroid.Core.Fx;

/// <summary>How far and how fast a tidal liquid surface rises and falls (#1415).</summary>
public static class LiquidTideDefinitions
{
    /// <summary>
    /// How far a small tide moves the surface at the wave's crest, in pixels: 8, so the surface swings up to 8 pixels
    /// either side of its base line.
    /// </summary>
    public const int SmallTidePeakPixels = 8;

    /// <summary>
    /// How far a large tide moves the surface at the wave's crest, in pixels: 32, so the surface swings up to 32 pixels
    /// either side of its base line.
    /// </summary>
    public const int LargeTidePeakPixels = 32;

    /// <summary>How far a small tide turns each update while its surface is at or below its base line: a step and an eighth.</summary>
    public static readonly RotationAngle SmallTidePositivePhaseDelta = RotationAngle.FromEighthsOfStep(9);

    /// <summary>How far a small tide turns each update while its surface is above its base line: 3/4 of a step.</summary>
    public static readonly RotationAngle SmallTideNegativePhaseDelta = RotationAngle.FromEighthsOfStep(6);

    /// <summary>How far a large tide turns each update while its surface is at or below its base line: 7/8 of a step.</summary>
    public static readonly RotationAngle LargeTidePositivePhaseDelta = RotationAngle.FromEighthsOfStep(7);

    /// <summary>How far a large tide turns each update while its surface is above its base line: half a step.</summary>
    public static readonly RotationAngle LargeTideNegativePhaseDelta = RotationAngle.FromEighthsOfStep(4);
}
