using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// A fade of the whole gameplay display toward one colour (#1512): the death fade to black, which keeps Samus lit, or the
/// time-up whiteout. It takes one step of the shared gradual colour change per call and shows as one fade of the display,
/// which every picture, the HUD's among them, and every shaded sprite draw shows after its own shading.
/// </summary>
internal sealed class GameplayFadeOut : IGameplayShadingSource
{
    /// <summary>The running fade's steps, or null while none runs.</summary>
    private GradualFadeSteps? steps;

    /// <summary>The colour the running fade ends in.</summary>
    private RgbColor toward;

    /// <summary>Whether the running fade keeps Samus's draws lit.</summary>
    private bool sparesSamus;

    /// <summary>Whether a fade runs or has completed and still shows.</summary>
    internal bool IsShown => steps is not null;

    /// <summary>Starts fading the display toward <paramref name="colour"/> over <paramref name="denominator"/> steps.</summary>
    /// <param name="colour">The colour the fade ends in.</param>
    /// <param name="denominator">The fade's length.</param>
    /// <param name="counter">The shared step counter the fade reads and advances.</param>
    /// <param name="keepsSamusLit">Whether Samus's draws keep their own colours, as the death fade keeps her.</param>
    internal void Begin(RgbColor colour, int denominator, GradualColorChangeCounter counter, bool keepsSamusLit)
    {
        steps = new GradualFadeSteps(denominator, counter);
        toward = colour;
        sparesSamus = keepsSamusLit;
    }

    /// <summary>Takes one step of the running fade.</summary>
    /// <returns>Whether the fade completed on this call; the display keeps showing its end until <see cref="End"/>.</returns>
    /// <exception cref="InvalidOperationException">No fade runs.</exception>
    internal bool Step() => (steps ?? throw new InvalidOperationException("A gameplay fade steps once it has begun.")).Step();

    /// <summary>Ends the fade: the display shows its own colours.</summary>
    internal void End() => steps = null;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (steps is { } running)
            frame.Fade(new GameplayDisplayFade(running.Toward(toward), SparesHud: false, SparesSamus: sparesSamus));
    }
}
