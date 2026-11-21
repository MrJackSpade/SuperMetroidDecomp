using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// One colour effect (#1554, #1982): how it starts, the colours it shows its frames in, the condition it watches and the
/// frames it plays (<see cref="ColourEffectAnimations"/>). An effect in a frontend screen's colours starts plainly and
/// watches nothing: a screen has no Samus, equipment, Torizo or area to start by or watch.
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
    ColourEffectFrames Frames);
