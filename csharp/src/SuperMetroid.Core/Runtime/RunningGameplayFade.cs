using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// A fade of the whole gameplay display that has begun and not yet ended (#1512, #2554): its steps of the shared gradual
/// colour change, the colour it ends in and what it leaves in its own colours. The step that runs it holds it and steps it.
/// </summary>
/// <param name="steps">The fade's steps of the shared gradual colour change.</param>
/// <param name="toward">The colour the fade ends in.</param>
/// <param name="spared">What the fade leaves in its own colours.</param>
internal sealed class RunningGameplayFade(GradualFadeSteps steps, RgbColor toward, GameplayFadeSpares spared)
{
    /// <summary>Takes one step of the fade.</summary>
    /// <returns>Whether the fade completed on this update; the display keeps showing its end until it ends.</returns>
    internal bool Step() => steps.Step();

    /// <summary>How the display shows the fade now.</summary>
    internal GameplayDisplayFade Shown => new(steps.Toward(toward), spared);
}
