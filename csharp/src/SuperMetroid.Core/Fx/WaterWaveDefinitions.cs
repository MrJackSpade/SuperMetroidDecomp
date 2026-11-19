using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>How a water surface's waves and drift move (#1415).</summary>
public static class WaterWaveDefinitions
{
    /// <summary>Frames between the effect-overlay wave's steps.</summary>
    public const int OverlayWavePhaseDuration = 10;

    /// <summary>Frames between the optional background wave's steps.</summary>
    public const int BackgroundWavePhaseDuration = 6;

    /// <summary>The surface's sideways drift a frame when its water drifts: a quarter pixel.</summary>
    public static SignedEightEight SurfaceDriftSpeed { get; } = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>
    /// One period of the wave on the water's surface as its first phase shows it, one sideways displacement in pixels per
    /// line below the surface (#1468, #2054, #2367): two identical eight-line triangular half-periods, rising then
    /// falling, each with its crest two lines down. Each later phase draws it one line further down.
    /// </summary>
    public static ImmutableArray<int> SurfaceWaveform { get; } = [0, 0, 1, 1, 0, 0, -1, -1, 0, 0, 1, 1, 0, 0, -1, -1];

    /// <summary>
    /// One period of the water's optional background wave as its first phase shows it, one sideways background
    /// displacement in pixels per line below the surface (#1468, #2054, #2367): the surface wave's two half-periods
    /// started at their crest. Each later phase draws it one line further up.
    /// </summary>
    public static ImmutableArray<int> BackgroundWaveform { get; } = [1, 1, 0, 0, -1, -1, 0, 0, 1, 1, 0, 0, -1, -1, 0, 0];
}
