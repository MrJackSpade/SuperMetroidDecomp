using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One colour effect (#1554, #1982): how it starts, the colours it shows its frames in, the condition it watches and the
/// animation it plays (<see cref="ColourEffectAnimations"/>).
/// </summary>
/// <param name="Start">How the effect starts.</param>
/// <param name="Colours">The colours the effect shows every frame in.</param>
/// <param name="Condition">
/// The condition the effect checks at the start of each turn from the update after it shows its first frame:
/// <see cref="ColourEffectCondition.WatchesNothing"/> for an effect that watches nothing.
/// </param>
/// <param name="Frames">
/// The frames the effect plays: an animation of its own, or, for the heat shimmer, whose start chooses its suit's pulse,
/// that pulse's (<see cref="ColourEffectFrames.SuitHeatPulse"/>).
/// </param>
internal sealed record ColourEffectDefinition(
    ColourEffectStart Start,
    ColourEffectColours Colours,
    ColourEffectCondition Condition,
    ColourEffectFrames Frames)
{
    /// <summary>The animation the effect plays in its own frames.</summary>
    /// <exception cref="InvalidOperationException">The effect plays its suit's heat pulse, with no animation of its own.</exception>
    internal Animation<ColourEffectSegment> OwnAnimation => Frames is ColourEffectFrames.OwnAnimation own
        ? own.Animation
        : throw new InvalidOperationException("The heat shimmer plays its suit's heat pulse, with no animation of its own.");
}
