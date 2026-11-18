namespace SuperMetroid.Core.Fx;

/// <summary>The rain's sideways drift under its randomly chosen wind (#1415, <see cref="AtmosphereDriftDefinitions.RainWindDraw"/>).</summary>
public static class AtmosphereDrift
{
    /// <summary>The rain's sideways velocity a frame under <paramref name="wind"/>: negative blows it left.</summary>
    public static SignedEightEight RainHorizontalVelocity(RainWind wind) => wind switch
    {
        RainWind.StrongLeft => SignedEightEight.FromPixels(-6),
        RainWind.StrongRight => SignedEightEight.FromPixels(6),
        RainWind.GentleLeft => SignedEightEight.FromPixels(-4),
        RainWind.GentleRight => SignedEightEight.FromPixels(4),
        _ => throw new ArgumentOutOfRangeException(nameof(wind), wind, "Undefined rain wind."),
    };
}
