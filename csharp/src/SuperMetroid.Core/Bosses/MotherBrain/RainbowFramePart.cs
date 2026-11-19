namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// A part of one of Samus's rainbow colour frames (#1951): what the Baby Metroid's slowing of her rainbow has accumulated
/// toward the next time her rainbow's glow is held longer. Each update adds three of the 256 parts a frame is counted in; a sum that completes a
/// whole frame holds the glow longer and keeps the remainder.
/// </summary>
public readonly record struct RainbowFramePart
{
    /// <summary>The parts one whole rainbow frame is counted in: 256ths of a frame.</summary>
    private const int PartsPerFrame = 256;

    /// <summary>The parts one update of the slowing adds: three.</summary>
    private const int PartsPerUpdate = 3;

    /// <summary>Holds <paramref name="twoHundredFiftySixths"/> 256ths of a frame, 0 through 255.</summary>
    internal RainbowFramePart(int twoHundredFiftySixths) =>
        TwoHundredFiftySixthsOfFrame = Ensure.BetweenInclusive(0, PartsPerFrame - 1, twoHundredFiftySixths);

    /// <summary>The part of a frame, in 256ths of a frame, 0 through 255.</summary>
    [IntegerQuantity("256ths of a rainbow colour frame accumulated toward the next held glow")]
    public int TwoHundredFiftySixthsOfFrame { get; }

    /// <summary>No part of a frame.</summary>
    public static RainbowFramePart None => default;

    /// <summary>
    /// Adds one update's three parts, returning the part left over and whether the addition completed a whole frame.
    /// </summary>
    public SamusRainbowFractionStep AdvancedByOneUpdate()
    {
        int sum = TwoHundredFiftySixthsOfFrame + PartsPerUpdate;
        return sum >= PartsPerFrame
            ? new SamusRainbowFractionStep(new RainbowFramePart(sum - PartsPerFrame), true)
            : new SamusRainbowFractionStep(new RainbowFramePart(sum), false);
    }
}
