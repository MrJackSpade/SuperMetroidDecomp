using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo's colours (#1512): the colouring his body and belly last settled in and the fade running from it toward the
/// colours his program names, each step on the shared gradual colour change. His draws show the import's shade of each.
/// His initialization clears his draw's palette bits, so his sheets are coloured in the rows his parts name and need no
/// selection shade; a hurt flash, which clears the same bits, changes nothing he shows.
/// </summary>
internal sealed class TorizoColours
{
    /// <summary>The colouring last settled.</summary>
    private TorizoLook settled = new(TorizoLookKind.BombStatue);

    /// <summary>The colouring the program's fade runs toward, or null before it names one.</summary>
    private TorizoLook? target;

    /// <summary>The running fade's start and the counter value of its last step, or null while none runs.</summary>
    private (TorizoLook From, int Value)? fade;

    /// <summary>The shading his draws show now.</summary>
    internal PictureShading Shading
    {
        get
        {
            PictureShading shading = PictureShading.Shaded(TorizoShades.Shared);
            return fade is { } running && target is { } to
                ? shading.With(TorizoShades.Fading(running.From, to, running.Value))
                : shading.With(TorizoShades.Body(settled));
        }
    }

    /// <summary>Shows <paramref name="look"/> at once, ending any fade.</summary>
    internal void Show(TorizoLook look)
    {
        settled = look;
        fade = null;
    }

    /// <summary>Names the colouring the fade runs toward; the fade starts from the colouring last settled.</summary>
    internal void FadeToward(TorizoLook look) => target = look;

    /// <summary>
    /// Takes one step of the fade on <paramref name="counter"/>: values zero through the fade's length plus one move the
    /// colours, and the call after completes the fade, settling it in its target, and resets the counter.
    /// </summary>
    /// <exception cref="InvalidOperationException">No fade target is named.</exception>
    internal void Step(GradualColorChangeCounter counter)
    {
        Ensure.NotNull(counter);
        TorizoLook to = target ?? throw new InvalidOperationException("A Torizo's fade steps once its program names its colours.");
        int value = counter.Numerator;
        if (value > TorizoPaletteDefinitions.FadeDenominator + 1)
        {
            counter.Reset();
            settled = to;
            fade = null;
            return;
        }
        fade = (fade?.From ?? settled, value);
        counter.Advance();
    }
}
