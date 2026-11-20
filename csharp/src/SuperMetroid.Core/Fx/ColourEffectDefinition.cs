using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One colour effect (#1554, #1982): how it starts, the colours it shows its frames in, the condition it watches and the
/// frames it plays (<see cref="ColourEffectAnimations"/>). An effect in a frontend screen's colours starts plainly and
/// watches nothing, checked once when it is built (#2553): a screen has no Samus, equipment, Torizo or area to start by or
/// watch.
/// </summary>
internal sealed record ColourEffectDefinition
{
    /// <summary>Keeps the definition.</summary>
    /// <param name="start">How the effect starts.</param>
    /// <param name="colours">The colours the effect shows every frame in.</param>
    /// <param name="condition">
    /// The condition the effect checks at the start of each turn from the update after it shows its first frame:
    /// <see cref="ColourEffectCondition.WatchesNothing"/> for an effect that watches nothing.
    /// </param>
    /// <param name="frames">
    /// The frames the effect plays: an animation of its own, or, for the heat shimmer, whose start chooses its suit's pulse,
    /// that pulse's (<see cref="ColourEffectFrames.SuitHeatPulse"/>).
    /// </param>
    /// <exception cref="ArgumentException">
    /// The effect shows a frontend screen's colours but starts other than plainly or watches a condition.
    /// </exception>
    internal ColourEffectDefinition(ColourEffectStart start, ColourEffectColours colours, ColourEffectCondition condition, ColourEffectFrames frames)
    {
        Start = start;
        Colours = colours;
        Condition = Ensure.NotNull(condition);
        Frames = Ensure.NotNull(frames);
        if (ColourEffectScreenColourDefinitions.FrontendScreenGroups.Contains(colours)
            && (start != ColourEffectStart.Plain || condition != ColourEffectCondition.WatchesNothing.Instance))
            throw new ArgumentException($"An effect in the screen colours {colours} starts by {start} and watches {condition}; a screen's effect starts plainly and watches nothing.", nameof(condition));
    }

    /// <summary>How the effect starts.</summary>
    internal ColourEffectStart Start { get; }

    /// <summary>The colours the effect shows every frame in.</summary>
    internal ColourEffectColours Colours { get; }

    /// <summary>The condition the effect checks at the start of each turn from the update after it shows its first frame.</summary>
    internal ColourEffectCondition Condition { get; }

    /// <summary>The frames the effect plays.</summary>
    internal ColourEffectFrames Frames { get; }
}
