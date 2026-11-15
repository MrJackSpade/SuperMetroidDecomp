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
/// The condition the effect checks at the start of each turn from the update after it shows its first frame, or null for
/// an effect that watches nothing.
/// </param>
/// <param name="Animation">The animation the effect plays; the heat shimmer's start chooses its suit's instead.</param>
internal sealed record ColourEffectDefinition(
    ColourEffectStart Start,
    ColourEffectColours Colours,
    ColourEffectCondition? Condition,
    Animation<ColourEffectId> Animation);
