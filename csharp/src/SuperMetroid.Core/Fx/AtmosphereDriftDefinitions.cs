namespace SuperMetroid.Core.Fx;

/// <summary>How fast rain, spores and fog drift across the screen each frame (#1415).</summary>
public static class AtmosphereDriftDefinitions
{
    /// <summary>Rain falls six pixels a frame.</summary>
    public static SignedEightEight RainFallSpeed { get; } = SignedEightEight.FromPixels(6);

    /// <summary>Spores rise a quarter pixel a frame.</summary>
    public static SignedEightEight SporesRiseSpeed { get; } = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>Fog rises a quarter pixel a frame.</summary>
    public static SignedEightEight FogRiseSpeed { get; } = SignedEightEight.FromPixelFraction(1, 4);

    /// <summary>Fog drifts right five sixteenths of a pixel a frame.</summary>
    public static SignedEightEight FogDriftSpeed { get; } = SignedEightEight.FromPixelFraction(5, 16);

    /// <summary>
    /// The wind a rainy room's load chooses from the shared random number (#2510): four rolls of the number take a strong
    /// wind left, a strong wind right, a gentle wind left and a gentle wind right in turn; the rest of the number is ignored.
    /// </summary>
    internal static WeightedRandomChoice<RainWind> RainWindDraw { get; } = WeightedRandomChoice<RainWind>.InTurn(
        new RandomSampleField(2, 2), RainWind.StrongLeft, RainWind.StrongRight, RainWind.GentleLeft, RainWind.GentleRight);


    /// <summary>How fast a strong wind blows the rain sideways, either way: six pixels a frame.</summary>
    public static SignedEightEight StrongWindSpeed { get; } = SignedEightEight.FromPixels(6);

    /// <summary>How fast a gentle wind blows the rain sideways, either way: four pixels a frame.</summary>
    public static SignedEightEight GentleWindSpeed { get; } = SignedEightEight.FromPixels(4);
}
