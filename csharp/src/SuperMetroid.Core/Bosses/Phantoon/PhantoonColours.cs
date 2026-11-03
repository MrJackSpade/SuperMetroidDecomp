using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>
/// Phantoon's colours (#1512): the settled colouring his body shows on layer 2, the fade running from it, and the Wrecked
/// Ship's power coming on over his room once he dies. His body and the room show the import's shade of each step; a fade
/// starts from the colouring last settled.
/// </summary>
internal sealed class PhantoonColours : IGameplayShadingSource
{
    /// <summary>The colouring his body last settled in, or null while it shows his room's own colours.</summary>
    private PhantoonBodyLook? settled;

    /// <summary>The running fade's start, target, length and last step value, or null while none runs.</summary>
    private (PhantoonBodyLook From, PhantoonBodyLook To, int Denominator, int Numerator)? fade;

    /// <summary>The last step value of the power coming on, or null before it starts.</summary>
    private int? powerOn;

    /// <summary>Whether the power has finished coming on.</summary>
    private bool powered;

    /// <summary>Shows <paramref name="look"/> on his body at once, ending any fade.</summary>
    internal void Show(PhantoonBodyLook look)
    {
        settled = look;
        fade = null;
    }

    /// <summary>
    /// Takes the step of value <paramref name="numerator"/> of his body's fade toward <paramref name="to"/> over
    /// <paramref name="denominator"/> steps, starting from the colouring last settled when no such fade runs.
    /// </summary>
    /// <exception cref="InvalidOperationException">His body has settled in no colouring to fade from.</exception>
    internal void Fade(PhantoonBodyLook to, int denominator, int numerator)
    {
        PhantoonBodyLook from = fade is { } running && running.To == to && running.Denominator == denominator ? running.From
            : settled ?? throw new InvalidOperationException("Phantoon's body fades once it has shown a colouring.");
        fade = (from, to, denominator, numerator);
    }

    /// <summary>Completes his body's running fade: it settles in the colouring it faded to.</summary>
    internal void CompleteFade()
    {
        if (fade is { } running)
            settled = running.To;
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
        if (fade is { } running)
            frame.Shade(GameplayPictures.BossBody, PhantoonShades.Fading(running.From, running.To, running.Denominator, running.Numerator));
        else if (settled is { } look)
            frame.Shade(GameplayPictures.BossBody, PhantoonShades.Body(look));
        int? step = powered ? PhantoonShades.PowerOnDenominator + 1 : powerOn;
        if (step is { } shown)
        {
            foreach (GameplayPictures pictures in PoweredPictures)
                frame.Shade(pictures, PhantoonShades.PowerOn(shown));
        }
    }

    /// <summary>The pictures the power coming on recolours: the room's, its HUD's and its layer-3 effect's.</summary>
    private static readonly GameplayPictures[] PoweredPictures = [GameplayPictures.Room, GameplayPictures.Hud, GameplayPictures.Layer3];
}
