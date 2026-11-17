namespace SuperMetroid.Core.Fx;

/// <summary>The rain's randomly chosen sideways drift (#1415).</summary>
public static class AtmosphereDrift
{
    /// <summary>
    /// The wind a rainy room's load chooses from the shared random number: the one of the rain's winds
    /// (<see cref="AtmosphereDriftDefinitions.RainWinds"/>) its choice reads; the rest of the number is ignored.
    /// </summary>
    public static RainWind RainWindChosenBy(int randomNumber) =>
        AtmosphereDriftDefinitions.RainWinds[AtmosphereDriftDefinitions.RainWindChoice.Of(randomNumber)];

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
