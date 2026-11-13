namespace SuperMetroid.Core.Fx;

/// <summary>The rain's randomly chosen sideways drift (#1415).</summary>
public static class AtmosphereDrift
{
    /// <summary>
    /// The wind a rainy room's load chooses from the shared random number: the number divided by 4, modulo 4, chooses
    /// one of the four winds, and the rest of the number is ignored.
    /// </summary>
    /// <remarks>The four original velocities and every random input are independently verified against NTSC J/U v1.0
    /// and pinned reference sources (362be646929cf8e483f692b73a6561cfc2dc1d0d).</remarks>
    public static RainWind RainWindChosenBy(int randomNumber) => AtmosphereDriftDefinitions.RainWindChoice.Of(randomNumber) switch
    {
        0 => RainWind.StrongLeft,
        1 => RainWind.StrongRight,
        2 => RainWind.GentleLeft,
        _ => RainWind.GentleRight,
    };

    /// <summary>The rain's horizontal velocity under <paramref name="wind"/>, in 1/256 pixel a frame.</summary>
    public static int RainHorizontalVelocity(RainWind wind) => wind switch
    {
        RainWind.StrongLeft => -EightEight.FromWhole(6),
        RainWind.StrongRight => EightEight.FromWhole(6),
        RainWind.GentleLeft => -EightEight.FromWhole(4),
        RainWind.GentleRight => EightEight.FromWhole(4),
        _ => throw new ArgumentOutOfRangeException(nameof(wind), wind, "Undefined rain wind."),
    };
}
