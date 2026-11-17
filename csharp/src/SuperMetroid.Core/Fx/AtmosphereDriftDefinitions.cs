using System.Collections.Immutable;

namespace SuperMetroid.Core.Fx;

/// <summary>How fast rain, spores and fog drift across the screen each frame (#1415).</summary>
public static class AtmosphereDriftDefinitions
{
    /// <summary>Rain falls six pixels a frame.</summary>
    public static SignedEightEight RainFallSpeed { get; } = SignedEightEight.FromPixels(6);

    /// <summary>Spores rise a quarter pixel a frame.</summary>
    public static SignedEightEight SporesRiseSpeed { get; } = SignedEightEight.FromParts(0, 64);

    /// <summary>Fog rises a quarter pixel a frame.</summary>
    public static SignedEightEight FogRiseSpeed { get; } = SignedEightEight.FromParts(0, 64);

    /// <summary>Fog drifts right five sixteenths of a pixel a frame.</summary>
    public static SignedEightEight FogDriftSpeed { get; } = SignedEightEight.FromParts(0, 80);

    /// <summary>The winds a rainy room's load chooses among, in the order its random choice (<see cref="RainWindChoice"/>) counts them.</summary>
    internal static ImmutableArray<RainWind> RainWinds { get; } =
        [RainWind.StrongLeft, RainWind.StrongRight, RainWind.GentleLeft, RainWind.GentleRight];

    /// <summary>The random selection a rainy room's load reads to choose the rain's wind: one of the four <see cref="RainWinds"/>.</summary>
    internal static readonly RandomSampleField RainWindChoice = new(2, 2);
}
