using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Torizo.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// A Torizo's colours (#1512): the colouring his body and belly last showed whole and the fade running from it toward the
/// colours his program names, each step on the shared gradual colour change. His draws show the import's shade of each.
/// His initialization clears his draw's palette bits, so his sheets are coloured in the rows his parts name and need no
/// selection shade; a hurt flash, which clears the same bits, changes nothing he shows.
/// </summary>
/// <remarks>
/// The game moves each colour one step from the colour shown, so a fade starts from what is shown when it takes its
/// first step. A hurt colouring written in the middle of a fade restarts it from that colouring at the counter value it
/// stands at, which the fade keeps as the first value it stepped from there; the import pre-colours every such start and
/// step. A program names a new target only while the colours rest on a whole colouring (his awakening and his death each
/// name theirs before their steps, and the awakening's last call takes only the counter's zero step, which moves nothing),
/// so a target named while a fade has moved the colours is not a state the game reaches and is refused.
/// Once his fight starts, his belly's glow writes each of its records over the belly's glowing colours, and those colours
/// show the last record written until his own colouring is written again; a fade steps them from the record shown, so the
/// glow keeps its own run of steps over its colours.
/// </remarks>
internal sealed class TorizoColours
{
    /// <summary>The colouring last shown whole: loaded, written by a hurt, or reached by a completed fade.</summary>
    private TorizoLook settled = new(TorizoLookKind.BombStatue);

    /// <summary>The colouring the program's fade runs toward, or null before it names one.</summary>
    private TorizoLook? target;

    /// <summary>
    /// The counter values of the first and last steps the running fade took from <see cref="settled"/>, or null while
    /// none runs.
    /// </summary>
    private TorizoFadeSteps? fade;

    /// <summary>
    /// The belly glow's record written over the belly's glowing colours after his own colouring, or null while they show
    /// his colouring.
    /// </summary>
    private PaletteFxFrame? belly;

    /// <summary>The counter value of the first fade step taken since <see cref="belly"/> was written, or null for none.</summary>
    private int? bellyFirst;

    /// <summary>The shading his draws show now.</summary>
    internal PictureShading Shading
    {
        get
        {
            PictureShading shading = PictureShading.Shaded(TorizoShades.Shared);
            shading = Moved is { } steps && target is { } to
                ? shading.With(TorizoShades.Fading(settled, to, steps.First, steps.Last))
                : shading.With(TorizoShades.Body(settled));
            if (belly is not { } record)
                return shading;
            // The zero step moves nothing, so the glow's colours move from their first step past zero.
            return fade is { } running && running.Last > 0 && bellyFirst is { } first && target is { } goal
                ? shading.With(TorizoShades.BellyFading(record, goal, Math.Max(first, 1), running.Last))
                : shading.With(PaletteFxShades.Of(PaletteFxColours.TorizoBelly, record));
        }
    }

    /// <summary>
    /// The running fade's steps that move colours, or null when none has: the counter's zero step changes nothing, so a
    /// fade's first moving step is its first step past zero.
    /// </summary>
    private TorizoFadeSteps? Moved =>
        fade is { } running && running.Last > 0 ? new TorizoFadeSteps(Math.Max(running.First, 1), running.Last) : null;

    /// <summary>
    /// Shows <paramref name="look"/> at once over his body and belly, the glowing colours among them; a running fade
    /// continues from it on its next step.
    /// </summary>
    internal void Show(TorizoLook look)
    {
        settled = look;
        fade = null;
        belly = null;
        bellyFirst = null;
    }

    /// <summary>Shows the belly glow's <paramref name="record"/> over the belly's glowing colours; a running fade continues from it.</summary>
    internal void ShowBelly(PaletteFxFrame record)
    {
        belly = record;
        bellyFirst = null;
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
            bellyFirst = null;
        }
        target = look;
    }

    /// <summary>
    /// Takes one step of the fade on <paramref name="counter"/>: values zero through the fade's length plus one move the
    /// colours from those shown, and the call after completes the fade and resets the counter, leaving the colours shown.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// No fade target is named, or the counter skipped a value since the fade's last step.
    /// </exception>
    internal void Step(GradualColorChangeCounter counter)
    {
        Ensure.NotNull(counter);
        TorizoLook to = target ?? throw new InvalidOperationException("A Torizo's fade steps once its program names its colours.");
        int value = counter.Numerator;
        if (value > TorizoPaletteDefinitions.FadeDenominator + 1)
        {
            counter.Reset();
            // The last step reached the target's colours exactly; with no step since a hurt colouring, that colouring stays,
            // and likewise the glow's record.
            if (fade is not null)
                settled = to;
            if (bellyFirst is not null)
                belly = null;
            fade = null;
            bellyFirst = null;
            return;
        }
        if (fade is { } running && value != running.Last + 1)
            throw new InvalidOperationException($"A Torizo's fade stepped at counter value {running.Last}, then at {value}.");
        fade = new TorizoFadeSteps(fade?.First ?? value, value);
        if (belly is not null)
            bellyFirst ??= value;
        counter.Advance();
    }
}
