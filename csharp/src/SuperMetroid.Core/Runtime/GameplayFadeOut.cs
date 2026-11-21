using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// A fade of the whole gameplay display toward one colour (#1512): the death fade to black, which keeps Samus lit, or the
/// time-up whiteout. It takes one step of the shared gradual colour change each update and shows as one fade of the display,
/// which every picture, the HUD's among them, and every shaded sprite draw shows after its own shading. Beginning a fade
/// hands back the running fade (#2554), which the step that runs it holds and steps; the display shows it until it ends.
/// </summary>
internal sealed class GameplayFadeOut : IGameplayShadingSource
{
    /// <summary>The runtime's shared fade step count, which every fade this owner runs reads and advances.</summary>
    private readonly SharedFadeStepCount counter;

    /// <summary>Creates the display's fade over the runtime's shared fade step count.</summary>
    /// <param name="counter">The runtime's shared fade step count, which its door fades and its enemies' fades share.</param>
    internal GameplayFadeOut(SharedFadeStepCount counter) => this.counter = Ensure.NotNull(counter);

    /// <summary>The fade the display shows, from its beginning until it ends; none while the display shows its own colours.</summary>
    internal RunningGameplayFade? Running { get; private set; }

    /// <summary>
    /// Starts fading the display toward <paramref name="colour"/> in <paramref name="parts"/> parts, from the step the
    /// shared count holds.
    /// </summary>
    /// <param name="colour">The colour the fade ends in.</param>
    /// <param name="parts">The parts the fade's colours move in, one for each of its steps after its first (<see cref="GradualFadeSteps"/>).</param>
    /// <param name="spares">What the fade leaves in its own colours: Samus for the death fade, nothing for the whiteout.</param>
    /// <returns>The running fade, which the step that runs it steps.</returns>
    internal RunningGameplayFade Begin(RgbColor colour, int parts, GameplayFadeSpares spares) =>
        Running = new RunningGameplayFade(new GradualFadeSteps(parts, counter), colour, spares);

    /// <summary>Ends the fade: the display shows its own colours.</summary>
    internal void End() => Running = null;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (Running is { } fade)
            frame.Fade(fade.Shown);
    }
}
