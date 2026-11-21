using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo's colours (#1512, #1597, #2236): the colouring his body and belly last showed whole, the fade running from it
/// toward the colours his animation names, and his belly's glow. His draws show the import's shade of each settled
/// colouring, over the orb, flame and statue-shading colours both Torizos show, and a running fade as a typed fade between
/// two of them, its progress the fade's own steps (#2302). He shows no common hurt flash: his own white colouring is his
/// flash, which covers his belly's glow while it shows (#1544).
/// </summary>
/// <remarks>
/// A fade starts from what is shown when it takes its first step, which shows it unmoved; each later step moves one of its
/// <see cref="TorizoShadeDefinitions.FadeLastStep"/> parts. A hurt colouring written in the middle of a fade restarts it
/// from that colouring. An animation names a new target only while the colours rest on a whole colouring (his awakening
/// and his death each name theirs before their steps, and the awakening's last update takes only a fade's first step, which
/// moves nothing), so a target named while a fade has moved the colours is not a state the game reaches.
/// Once his fight starts, his belly glows: its glowing colours show the glow's current step over every colouring but his
/// white flash, and a fade moves them from the step shown, so the glow keeps its own part of the fade from the step at
/// which it last changed. The colours never reach gameplay; only when a fade completes does, through the shared counter
/// (<see cref="Step"/>).
/// </remarks>
internal sealed class TorizoColours
{
    /// <summary>The colouring last shown whole: loaded, written by a hurt, or reached by a completed fade.</summary>
    private TorizoLook settled = new(TorizoLookKind.BombStatue);

    /// <summary>
    /// The colouring the animation's fade runs toward: the loaded colouring until his animation names one, so a fade before
    /// then runs toward the colours shown.
    /// </summary>
    private TorizoLook target = new(TorizoLookKind.BombStatue);

    /// <summary>How far the running fade has come from <see cref="settled"/>, or null while none runs.</summary>
    private StepCountedFadeProgress? fade;

    /// <summary>The step his belly's glow shows, or null before his fight starts it.</summary>
    private ColourEffectFrame? glow;

    /// <summary>
    /// The parts the running fade had moved when it first stepped over the glow's current step, or null for none: the glow
    /// moving to its next step starts the fade's part over it afresh.
    /// </summary>
    private int? glowFadeFrom;

    /// <summary>The shading his draws show now.</summary>
    internal PictureShading Shading
    {
        get
        {
            PictureShading shading = PictureShading.Shaded(TorizoShades.Shared);
            TorizoLook? fadingToward = fade is { Moved: > 0 } ? target : null;
            shading = fadingToward is { } to && fade is { } running
                ? shading.With(new PictureShadeFade(
                    PictureShading.Shaded(TorizoShades.Body(settled)), PictureShading.Shaded(TorizoShades.Body(to)), new FadeLevel(running.Moved, running.Length)))
                : shading.With(TorizoShades.Body(settled));
            if (glow is not { } step || settled.Kind == TorizoLookKind.White)
                return shading;
            PictureShade glowing = ColourEffectShades.Of(ColourEffectColours.TorizoBelly, step);
            if (fadingToward is not { } goal || fade is not { } body)
                return shading.With(glowing);
            // Over a moving fade the glow's colours show as their own fade, unmoved until the fade steps over this glow step.
            StepCountedFadeProgress? overGlow = glowFadeFrom is { } from ? body.Since(from) : null;
            return shading.With(new PictureShadeFade(PictureShading.Shaded(glowing), PictureShading.Shaded(TorizoShades.GlowingBelly(goal)),
                new FadeLevel(overGlow?.Moved ?? 0, overGlow?.Length ?? body.Length)));
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
        glowFadeFrom = null;
    }

    /// <summary>Shows step <paramref name="step"/> of his belly's glow over its glowing colours; a running fade continues from it.</summary>
    internal void ShowBellyGlow(ColourEffectFrame step)
    {
        glow = step;
        glowFadeFrom = null;
    }

    /// <summary>Names the colouring the fade runs toward; the fade starts from the colours shown.</summary>
    internal void FadeToward(TorizoLook look)
    {
        if (fade is not { Moved: > 0 })
        {
            fade = null;
            glowFadeFrom = null;
        }
        target = look;
    }

    /// <summary>
    /// Takes one step of the fade on <paramref name="counter"/>: while the counter stands at zero through
    /// <see cref="TorizoShadeDefinitions.FadeLastStep"/> the fade takes its next step, and once the counter has passed them
    /// the step completes the fade and resets the counter, leaving his body in the colouring faded toward and his belly's
    /// glow showing its step over it. The counter decides only when the fade completes; how far the colours have come is
    /// the fade's own steps (#2302).
    /// </summary>
    internal void Step(SharedFadeStepCount counter)
    {
        Ensure.NotNull(counter);
        TorizoLook to = target;
        if (counter.Step > TorizoShadeDefinitions.FadeLastStep)
        {
            counter.Reset();
            // With no step since a hurt colouring, that colouring stays.
            if (fade is not null)
                settled = to;
            fade = null;
            glowFadeFrom = null;
            return;
        }
        int movedBefore = fade?.Moved ?? 0;
        fade = fade is { } running ? running.Then() : StepCountedFadeProgress.Started(TorizoShadeDefinitions.FadeLastStep);
        if (glow is not null)
            glowFadeFrom ??= movedBefore;
        counter.Advance();
    }
}
