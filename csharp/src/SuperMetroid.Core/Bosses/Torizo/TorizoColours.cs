using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo's colours (#1512, #1597, #2236): the colouring his body and belly last showed whole, the fade running from it
/// toward the colours his animation names, and his belly's glow. His draws show the import's shade of each settled
/// colouring, over the orb, flame and statue-shading colours both Torizos show, and a running fade as a typed fade between
/// two of them, its progress the shared fade counter's. He shows no common hurt flash: his own white colouring is his
/// flash, which covers his belly's glow while it shows (#1544).
/// </summary>
/// <remarks>
/// A fade starts from what is shown when it takes its first step. A hurt colouring written in the middle of a fade
/// restarts it from that colouring at the counter value it stands at. An animation names a new target only while the
/// colours rest on a whole colouring (his awakening and his death each name theirs before their steps, and the awakening's
/// last call takes only the counter's zero step, which moves nothing), so a target named while a fade has moved the colours
/// is not a state the game reaches and is refused. Once his fight starts, his belly glows: its glowing colours show the
/// glow's current step over every colouring but his white flash, and a fade moves them from the step shown, so the glow
/// keeps its own part of the fade from the step at which it last changed. The colours never reach gameplay; only when a
/// fade completes does, through the counter (<see cref="Step"/>).
/// </remarks>
internal sealed class TorizoColours
{
    /// <summary>The colouring last shown whole: loaded, written by a hurt, or reached by a completed fade.</summary>
    private TorizoLook settled = new(TorizoLookKind.BombStatue);

    /// <summary>The colouring the animation's fade runs toward, or null before it names one.</summary>
    private TorizoLook? target;

    /// <summary>How far the running fade has come from <see cref="settled"/>, or null while none runs.</summary>
    private TorizoColourFadeProgress? fade;

    /// <summary>The step his belly's glow shows, or null before his fight starts it.</summary>
    private ColourEffectFrame? glow;

    /// <summary>
    /// The counter value of the first step the running fade took over the glow's current step, or null for none: the glow
    /// moving to its next step starts the fade's part over it afresh.
    /// </summary>
    private int? glowFadeFirst;

    /// <summary>The shading his draws show now.</summary>
    internal PictureShading Shading
    {
        get
        {
            PictureShading shading = PictureShading.Shaded(TorizoShades.Shared);
            TorizoLook? fadingToward = fade is { Moved: > 0 } ? target : null;
            shading = fadingToward is { } to && fade is { } running
                ? shading.With(new PictureShadeFade(
                    PictureShading.Shaded(TorizoShades.Body(settled)), PictureShading.Shaded(TorizoShades.Body(to)), running.Moved, running.Length))
                : shading.With(TorizoShades.Body(settled));
            if (glow is not { } step || settled.Kind == TorizoLookKind.White)
                return shading;
            PictureShade glowing = ColourEffectShades.Of(ColourEffectColours.TorizoBelly, step);
            if (fadingToward is not { } goal || fade is not { } body)
                return shading.With(glowing);
            // Over a moving fade the glow's colours show as their own fade, unmoved until the fade steps over this glow step.
            TorizoColourFadeProgress? overGlow = glowFadeFirst is { } first ? body.Since(first) : null;
            return shading.With(new PictureShadeFade(PictureShading.Shaded(glowing), PictureShading.Shaded(TorizoShades.GlowingBelly(goal)),
                overGlow?.Moved ?? 0, overGlow?.Length ?? body.Length));
        }
    }

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
        bool moved = fade is { Moved: > 0 };
        if (target is { } running && running != look && moved)
            throw new InvalidOperationException($"A Torizo's animation names {look.Key} while its fade toward {running.Key} has moved the colours.");
        if (!moved)
        {
            fade = null;
            glowFadeFirst = null;
        }
        target = look;
    }

    /// <summary>
    /// Takes one step of the fade on <paramref name="counter"/>: values zero through
    /// <see cref="TorizoShadeDefinitions.FadeLastStep"/> move the colours from those shown, the last reaching the colouring
    /// faded toward, and the call after completes the fade and resets the counter, leaving his body in that colouring and
    /// his belly's glow showing its step over it.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No fade target is named, or the counter fell since the fade's last step.
    /// </exception>
    internal void Step(SharedFadeStepCount counter)
    {
        Ensure.NotNull(counter);
        TorizoLook to = target ?? throw new InvalidOperationException("A Torizo's fade steps once its animation names its colours.");
        int value = counter.Step;
        if (value > TorizoShadeDefinitions.FadeLastStep)
        {
            counter.Reset();
            // With no step since a hurt colouring, that colouring stays.
            if (fade is not null)
                settled = to;
            fade = null;
            glowFadeFirst = null;
            return;
        }
        fade = fade is { } running ? running.Then(value) : TorizoColourFadeProgress.At(value);
        if (glow is not null)
            glowFadeFirst ??= value;
        counter.Advance();
    }
}
