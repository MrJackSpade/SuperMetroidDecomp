using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's colours (#1512): the colouring his body last showed whole on the background, the fade running from it, and the
/// Wrecked Ship's power coming on over his room once he dies. His body and the room show the import's shade of each step.
/// </summary>
/// <remarks>
/// The game moves each of his colours one step from the colour shown, so a fade starts from what is shown when it
/// takes its first step. His hurt flash writes white and his health colours in the middle of the fade that follows the
/// hit, which restarts that fade from the colouring written at the step value it stands at; the fade keeps that value as
/// the first it stepped from there, and the import pre-colours every such start and step. Each fade his patterns start
/// runs to completion before another starts (his first fireball rain's fade-out has at least sixty frames before the
/// rain, which it needs under thirty for), and his health, which names a fade-in's target, changes only while he is
/// tangible after his fades, so a fade naming another target while his colours are mid-fade is not a state the
/// game reaches and is refused.
/// </remarks>
internal sealed class PhantoonColours : IGameplayShadingSource
{
    /// <summary>The colouring his body last showed whole, or null while it shows his room's own colours.</summary>
    private PhantoonBodyLook? settled;

    /// <summary>
    /// The running fade's target and length, and the values of the first and last steps it took from
    /// <see cref="settled"/>, or null while none runs.
    /// </summary>
    private PhantoonBodyFade? fade;

    /// <summary>The last step value of the power coming on, or null before it starts.</summary>
    private int? powerOn;

    /// <summary>Whether the power has finished coming on.</summary>
    private bool powered;

    /// <summary>Shows <paramref name="look"/> on his body at once; a running fade continues from it on its next step.</summary>
    internal void Show(PhantoonBodyLook look)
    {
        settled = look;
        fade = null;
    }

    /// <summary>
    /// Takes the step of value <paramref name="numerator"/> of his body's fade toward <paramref name="to"/> over
    /// <paramref name="denominator"/> steps, from the colours shown.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// His body has shown no colouring to fade from, a fade toward another target has moved his colours, or the step's
    /// value does not follow the fade's last.
    /// </exception>
    internal void Fade(PhantoonBodyLook to, int denominator, int numerator)
    {
        if (settled is null)
            throw new InvalidOperationException("Phantoon's body fades once it has shown a colouring.");
        if (fade is { } running && running.Last > 0)
        {
            if (running.To != to || running.Denominator != denominator)
                throw new InvalidOperationException($"Phantoon's fade toward {to.Key} starts while his fade toward {running.To.Key} has moved his colours.");
            if (numerator != running.Last + 1)
                throw new InvalidOperationException($"Phantoon's fade stepped at value {running.Last}, then at {numerator}.");
            fade = running with { Last = numerator };
            return;
        }
        fade = new PhantoonBodyFade(to, denominator, fade?.First ?? numerator, numerator);
    }

    /// <summary>
    /// Completes his body's running fade: its last step reached the colouring it faded to, which it settles in; with no
    /// step since a colouring was written, that colouring stays.
    /// </summary>
    /// <exception cref="InvalidOperationException">The fade completes before its last step reached its target.</exception>
    internal void CompleteFade()
    {
        if (fade is { } running)
        {
            if (running.Last != running.Denominator + 1)
                throw new InvalidOperationException($"Phantoon's fade toward {running.To.Key} completes at step {running.Last} of {running.Denominator + 1}.");
            settled = running.To;
        }
        fade = null;
    }

    /// <summary>Takes the step of value <paramref name="numerator"/> of the Wrecked Ship's power coming on over his room.</summary>
    internal void PowerOn(int numerator) => powerOn = numerator;

    /// <summary>Completes the power coming on: the room shows its powered colours.</summary>
    internal void CompletePowerOn() => powered = true;

    /// <inheritdoc />
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        // The zero step moves nothing, so a fade's first moving step is its first step past zero.
        if (fade is { } running && running.Last > 0 && settled is { } from)
            frame.Shade(GameplayPictures.BossBody, PhantoonShades.Fading(from, running.To, running.Denominator, Math.Max(running.First, 1), running.Last));
        else if (settled is { } look)
            frame.Shade(GameplayPictures.BossBody, PhantoonShades.Body(look));
        int? step = powered ? PhantoonShades.PowerOnDenominator + 1 : powerOn;
        if (step is { } shown)
        {
            foreach (GameplayPictures pictures in PoweredPictures)
                frame.Shade(pictures, PhantoonShades.PowerOn(shown));
        }
    }

    /// <summary>
    /// The pictures the power coming on recolours: the room's and its effect overlay's. The HUD keeps its own colours,
    /// which the power coming on does not change (#1544).
    /// </summary>
    private static readonly GameplayPictures[] PoweredPictures = [GameplayPictures.Room, GameplayPictures.FxOverlay];
}
