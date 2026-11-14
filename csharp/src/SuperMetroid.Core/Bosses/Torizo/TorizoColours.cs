using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo's colours (#1512, #1597): the colouring his body and belly last showed whole, the fade running from it toward
/// the colours his animation names, each step on the shared gradual colour change, and his belly's glow. His draws show the
/// import's shade of each, over the orb, flame and statue-shading colours both Torizos show. He shows no common hurt
/// flash: his own white colouring is his flash, which covers his belly's glow while it shows (#1544).
/// </summary>
/// <remarks>
/// The game moves each colour one step from the colour shown, so a fade starts from what is shown when it takes its
/// first step. A hurt colouring written in the middle of a fade restarts it from that colouring at the counter value it
/// stands at, which the fade keeps as the first value it stepped from there; the import pre-colours every such start and
/// step. An animation names a new target only while the colours rest on a whole colouring (his awakening and his death each
/// name theirs before their steps, and the awakening's last call takes only the counter's zero step, which moves nothing),
/// so a target named while a fade has moved the colours is not a state the game reaches and is refused.
/// Once his fight starts, his belly glows: its glowing colours show the glow's current step over every colouring but his
/// white flash, and a fade steps them from the step shown, so the glow keeps its own run of steps over its colours.
/// </remarks>
internal sealed class TorizoColours
{
    /// <summary>The colouring last shown whole: loaded, written by a hurt, or reached by a completed fade.</summary>
    private TorizoLook settled = new(TorizoLookKind.BombStatue);

    /// <summary>The colouring the animation's fade runs toward, or null before it names one.</summary>
    private TorizoLook? target;

    /// <summary>
    /// The counter values of the first and last steps the running fade took from <see cref="settled"/>, or null while
    /// none runs.
    /// </summary>
    private InclusiveRange? fade;

    /// <summary>The step his belly's glow shows, or null before his fight starts it.</summary>
    private ColourEffectFrame? glow;

    /// <summary>
    /// The counter value of the first step the running fade took over the glow's current step, or null for none: the glow
    /// moving to its next step starts the fade's run over it afresh.
    /// </summary>
    private int? glowFadeFirst;

    /// <summary>The shading his draws show now.</summary>
    internal PictureShading Shading
    {
        get
        {
            PictureShading shading = PictureShading.Shaded(TorizoShades.Shared);
            shading = Moved is { } steps && target is { } to
                ? shading.With(TorizoShades.Fading(settled, to, steps.First, steps.Last))
                : shading.With(TorizoShades.Body(settled));
            if (glow is not { } step || settled.Kind == TorizoLookKind.White)
                return shading;
            // The zero step moves nothing, so the glow's colours move from their first step past zero.
            return fade is { } running && running.Last > 0 && glowFadeFirst is { } first && target is { } goal
                ? shading.With(TorizoShades.BellyFading(step, goal, Math.Max(first, 1), running.Last))
                : shading.With(ColourEffectShades.Of(ColourEffectColours.TorizoBelly, step));
        }
    }

    /// <summary>
    /// The running fade's steps that move colours, or null when none has: the counter's zero step changes nothing, so a
    /// fade's first moving step is its first step past zero.
    /// </summary>
    private InclusiveRange? Moved =>
        fade is { } running && running.Last > 0 ? new InclusiveRange(Math.Max(running.First, 1), running.Last) : null;

    /// <summary>
    /// Shows <paramref name="look"/> at once over his body and belly; a running fade continues from it on its next step,
    /// and his belly's glow keeps showing its step over it unless it is his white flash.
    /// </summary>
    internal void Show(TorizoLook look)
    {
        settled = look;
        fade = null;
        glowFadeFirst = null;
    }

    /// <summary>Shows step <paramref name="step"/> of his belly's glow over its glowing colours; a running fade continues from it.</summary>
    internal void ShowBellyGlow(ColourEffectFrame step)
    {
        glow = step;
        glowFadeFirst = null;
    }

    /// <summary>Names the colouring the fade runs toward; the fade starts from the colours shown.</summary>
    /// <exception cref="InvalidOperationException">A fade toward another colouring has already moved the colours.</exception>
    internal void FadeToward(TorizoLook look)
    {
        if (target is { } running && running != look && Moved is not null)
            throw new InvalidOperationException($"A Torizo's program names {look.Key} while its fade toward {running.Key} has moved the colours.");
        if (Moved is null)
        {
            fade = null;
            glowFadeFirst = null;
        }
        target = look;
    }

    /// <summary>
    /// Takes one step of the fade on <paramref name="counter"/>: values zero through the fade's length plus one move the
    /// colours from those shown, and the call after completes the fade and resets the counter, leaving his body in the
    /// colouring it reached and his belly's glow showing its step over it.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No fade target is named, or the counter skipped a value since the fade's last step.
    /// </exception>
    internal void Step(SharedFadeStepCount counter)
    {
        Ensure.NotNull(counter);
        TorizoLook to = target ?? throw new InvalidOperationException("A Torizo's fade steps once its program names its colours.");
        int value = counter.Step;
        if (value > TorizoPaletteDefinitions.FadeDenominator + 1)
        {
            counter.Reset();
            // The last step reached the target's colours exactly; with no step since a hurt colouring, that colouring stays.
            if (fade is not null)
                settled = to;
            fade = null;
            glowFadeFirst = null;
            return;
        }
        if (fade is { } running && value != running.Last + 1)
            throw new InvalidOperationException($"A Torizo's fade stepped at counter value {running.Last}, then at {value}.");
        fade = new InclusiveRange(fade?.First ?? value, value);
        if (glow is not null)
            glowFadeFirst ??= value;
        counter.Advance();
    }
}
