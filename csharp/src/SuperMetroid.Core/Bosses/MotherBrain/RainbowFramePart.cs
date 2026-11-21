namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// A part of one of Samus's rainbow colour frames (#1951): what the Baby Metroid's slowing of her rainbow has accumulated
/// toward the next time her rainbow's glow is held longer. Each update adds <see cref="PartsPerUpdate"/> of the <see cref="PartsPerFrame"/> parts a frame is counted in; a sum that completes a
/// whole frame holds the glow longer and keeps the remainder.
/// </summary>
public readonly record struct RainbowFramePart
{
    /// <summary>The parts one whole rainbow frame is counted in.</summary>
    private const int PartsPerFrame = 256;

    /// <summary>The parts one update of the slowing adds.</summary>
    private const int PartsPerUpdate = 3;

    /// <summary>Holds <paramref name="parts"/> of a frame's parts, from none up to one short of a whole frame.</summary>
    internal RainbowFramePart(int parts) => Parts = Ensure.BetweenInclusive(0, PartsPerFrame - 1, parts);

    /// <summary>The frame's parts accumulated, from none up to one short of a whole frame, which the type carries into frames.</summary>
    private int Parts { get; }

    /// <summary>No part of a frame.</summary>
    public static RainbowFramePart None => default;

    /// <summary>
    /// Adds one update's parts, returning the part left over and whether the addition completed a whole frame.
    /// </summary>
    public SamusRainbowFractionStep AdvancedByOneUpdate()
    {
        int sum = Parts + PartsPerUpdate;
        return sum >= PartsPerFrame
            ? new SamusRainbowFractionStep(new RainbowFramePart(sum - PartsPerFrame), true)
            : new SamusRainbowFractionStep(new RainbowFramePart(sum), false);
    }
}
