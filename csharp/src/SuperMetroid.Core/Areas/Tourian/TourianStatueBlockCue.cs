using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Areas.Tourian;

/// <summary>
/// One cue of a Tourian boss statue's picture animation (#2571): the effect it carries out on its statue, or the condition
/// under which its statue starts another animation. Each cue is its own behaviour, so the statue sequence runs the cue it
/// meets rather than looking up what a kind does; the values a cue acts on come from the statue's definition.
/// </summary>
internal sealed partial class TourianStatueBlockCue
{
    /// <summary>What the cue does to its statue, returning the animation the statue starts, or null to run on.</summary>
    private readonly Func<ITourianStatueCueTarget, AnimationContinuation<TourianStatueBlockSegment>?> _run;

    /// <summary>The cue's name in diagnostics.</summary>
    private readonly string _name;

    /// <summary>A cue named <paramref name="name"/> that carries out <paramref name="run"/>.</summary>
    private TourianStatueBlockCue(string name, Func<ITourianStatueCueTarget, AnimationContinuation<TourianStatueBlockSegment>?> run)
    {
        _name = name;
        _run = run;
    }

    /// <summary>A cue that acts on its statue and lets the animation run on.</summary>
    private static TourianStatueBlockCue Effect(string name, Action<ITourianStatueCueTarget> effect) =>
        new(name, target =>
        {
            effect(target);
            return null;
        });

    /// <summary>Makes the statue join the statues that are animating.</summary>
    internal static TourianStatueBlockCue MarkAnimating { get; } = Effect(nameof(MarkAnimating), target => target.MarkAnimating());

    /// <summary>The statue may already be grey: when its grey event exists, it starts its already-grey finish (#2189).</summary>
    internal static TourianStatueBlockCue MayAlreadyBeGrey { get; } = new(nameof(MayAlreadyBeGrey), target => target.MayAlreadyBeGrey());

    /// <summary>The statue may be released: when its area boss is dead, it starts its release (#2189).</summary>
    internal static TourianStatueBlockCue MayBeReleased { get; } = new(nameof(MayBeReleased), target => target.MayBeReleased());

    /// <summary>Makes the statue stop animating.</summary>
    internal static TourianStatueBlockCue ClearAnimating { get; } = Effect(nameof(ClearAnimating), target => target.ClearAnimating());

    /// <summary>The statue may wait its turn: while another release runs, it starts its waiting oscillation.</summary>
    internal static TourianStatueBlockCue MayWaitItsTurn { get; } = new(nameof(MayWaitItsTurn), target => target.MayWaitItsTurn());

    /// <summary>Claims the release for this statue.</summary>
    internal static TourianStatueBlockCue ClaimRelease { get; } = Effect(nameof(ClaimRelease), target => target.ClaimRelease());

    /// <summary>Shows the statue's eye colours blacked out.</summary>
    internal static TourianStatueBlockCue BlackOutEyes { get; } = Effect(nameof(BlackOutEyes), target => target.BlackOutEyes());

    /// <summary>Spawns the glow of the statue's eye.</summary>
    internal static TourianStatueBlockCue SpawnEyeGlow { get; } = Effect(nameof(SpawnEyeGlow), target => target.SpawnEyeGlow());

    /// <summary>Spawns the soul the statue releases.</summary>
    internal static TourianStatueBlockCue SpawnSoul { get; } = Effect(nameof(SpawnSoul), target => target.SpawnSoul());

    /// <summary>Starts the colour effect that greys the statue.</summary>
    internal static TourianStatueBlockCue SpawnGreyColourEffect { get; } = Effect(nameof(SpawnGreyColourEffect), target => target.StartGreyOut());

    /// <summary>Sets the statue's grey event.</summary>
    internal static TourianStatueBlockCue SetGreyEvent { get; } = Effect(nameof(SetGreyEvent), target => target.SetGreyEvent());

    /// <summary>Finishes the statue's release.</summary>
    internal static TourianStatueBlockCue FinishRelease { get; } = Effect(nameof(FinishRelease), target => target.FinishRelease());

    /// <summary>Shows the statue grey.</summary>
    internal static TourianStatueBlockCue TurnGrey { get; } = Effect(nameof(TurnGrey), target => target.TurnGrey());

    /// <summary>Carries the cue out on <paramref name="target"/>, returning the animation its statue starts, or null to run on.</summary>
    internal AnimationContinuation<TourianStatueBlockSegment>? Run(ITourianStatueCueTarget target) => _run(Ensure.NotNull(target));

    /// <summary>Names the cue.</summary>
    public override string ToString() => _name;
}
