using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// A fade of the whole gameplay display toward one colour (#1512): the death fade to black, which keeps Samus lit, or the
/// time-up whiteout. It takes one step of the shared gradual colour change each update and shows as one fade of the display,
/// which every picture, the HUD's among them, and every shaded sprite draw shows after its own shading.
/// </summary>
internal sealed class GameplayFadeOut : IGameplayShadingSource
{
    /// <summary>The runtime's shared fade step count, which every fade this owner runs reads and advances.</summary>
    private readonly SharedFadeStepCount counter;

    /// <summary>The fade the display shows: none, or the running one (#2554).</summary>
    private FadeState state = new FadeState.NotFading();

    /// <summary>Creates the display's fade over the runtime's shared fade step count.</summary>
    /// <param name="counter">The runtime's shared fade step count, which its door fades and its enemies' fades share.</param>
    internal GameplayFadeOut(SharedFadeStepCount counter) => this.counter = Ensure.NotNull(counter);

    /// <summary>Whether a fade has begun and not yet ended: the display shows it.</summary>
    internal bool IsUnderWay => state is FadeState.Fading;

    /// <summary>
    /// Starts fading the display toward <paramref name="colour"/> in <paramref name="parts"/> parts, from the step the
    /// shared count holds.
    /// </summary>
    /// <param name="colour">The colour the fade ends in.</param>
    /// <param name="parts">The parts the fade's colours move in, one for each of its steps after its first (<see cref="GradualFadeSteps"/>).</param>
    /// <param name="spares">What the fade leaves in its own colours: Samus for the death fade, nothing for the whiteout.</param>
    internal void Begin(RgbColor colour, int parts, GameplayFadeSpares spares)
    {
        state = new FadeState.Fading(new GradualFadeSteps(parts, counter), colour, spares);
    }

    /// <summary>Takes one step of the running fade.</summary>
    /// <returns>Whether the fade completed on this update; the display keeps showing its end until <see cref="End"/>.</returns>
    /// <exception cref="InvalidOperationException">No fade runs.</exception>
    internal bool Step() => state switch
    {
        FadeState.Fading running => running.Steps.Step(),
        FadeState.NotFading => throw new InvalidOperationException("A gameplay fade steps once it has begun."),
        _ => throw new InvalidOperationException($"Undefined gameplay fade {state}."),
    };

    /// <summary>Ends the fade: the display shows its own colours.</summary>
    internal void End() => state = new FadeState.NotFading();

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (state is FadeState.Fading running)
            frame.Fade(new GameplayDisplayFade(running.Steps.Toward(running.Toward), running.Spared));
    }

    /// <summary>The fade the display shows (#2554): none, or one running toward its colour.</summary>
    private abstract record FadeState
    {
        /// <summary>Only the states below are choices.</summary>
        private FadeState()
        {
        }

        /// <summary>No fade runs: the display shows its own colours.</summary>
        internal sealed record NotFading : FadeState;

        /// <summary>A fade runs.</summary>
        /// <param name="Steps">Its steps.</param>
        /// <param name="Toward">The colour it ends in.</param>
        /// <param name="Spared">What it leaves in its own colours.</param>
        internal sealed record Fading(GradualFadeSteps Steps, RgbColor Toward, GameplayFadeSpares Spared) : FadeState;
    }
}
