using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Fx;

/// <summary>How one colour effect starts and the animation it starts (#1554).</summary>
/// <param name="Start">How the effect starts.</param>
/// <param name="InitialAnimation">The animation the effect starts.</param>
internal readonly record struct ColourEffectDefinition(
    ColourEffectStart Start,
    ColourEffectAnimation InitialAnimation);
