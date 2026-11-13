namespace SuperMetroid.Core.Fx;

/// <summary>How a water surface's waves and drift move (#1415).</summary>
public static class WaterWaveDefinitions
{
    /// <summary>Frames between the effect-overlay wave's steps.</summary>
    public const int OverlayWavePhaseDuration = 10;

    /// <summary>Frames between the optional background wave's steps.</summary>
    public const int BackgroundWavePhaseDuration = 6;

    /// <summary>The surface's horizontal drift a frame when its water drifts: a quarter pixel, in 1/256 pixel.</summary>
    public const int HorizontalSubscrollVelocity = 64;

    /// <summary>The steps in one cycle of the water wave.</summary>
    public const int WaveDisplacementCount = 16;
}
