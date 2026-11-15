namespace SuperMetroid.Core.Fx;

/// <summary>How a water surface's waves and drift move (#1415).</summary>
public static class WaterWaveDefinitions
{
    /// <summary>Frames between the effect-overlay wave's steps.</summary>
    public const int OverlayWavePhaseDuration = 10;

    /// <summary>Frames between the optional background wave's steps.</summary>
    public const int BackgroundWavePhaseDuration = 6;

    /// <summary>The surface's sideways drift a frame when its water drifts: a quarter pixel.</summary>
    public static SignedEightEight SurfaceDriftSpeed { get; } = SignedEightEight.FromParts(0, 64);

    /// <summary>The steps in one cycle of the water wave.</summary>
    public const int WaveDisplacementCount = 16;

    /// <summary>
    /// The step of the cycle each water wave shows first, from the room's first update: the step after the cycle's start,
    /// which the wave returns to only once it has shown every other.
    /// </summary>
    public const int FirstPhase = 1;
}
