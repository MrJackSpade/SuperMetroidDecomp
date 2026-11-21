using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Escape;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The warning Ceres's self-destruct shows and types once the getaway ends (#2228): its phase, its phase's wait, its
/// typewriter and its alarm's step. What it writes goes to the room's foreground warning (#2554), which Ceres Ridley holds.
/// </summary>
public sealed class CeresSelfDestructWarning
{
    /// <summary>Which step of the warning runs next; the getaway's end starts it.</summary>
    internal CeresSelfDestructPhase Phase { get; set; } = CeresSelfDestructPhase.Start;
    /// <summary>
    /// The wait of the phase that has one (#2368, #2469): the waits for the escape timer and warning and for the EMERGENCY
    /// title, the title's hold, then, for Japanese text, the subtitle's wait. The typewriter keeps its own schedule.
    /// </summary>
    public BossCountdown PhaseWait { get; } = new();
    /// <summary>
    /// The typewriter the title's hold started, which the phases that type run: none before then, when nothing types.
    /// </summary>
    public EscapeTypewriterState? Typewriter { get; internal set; }
    /// <summary>
    /// The warning's cycling alarm (#2316, #2409), which shows nothing before its first step; each step, at the alarm's
    /// cadence, plays it on to its next colouring, the first after the last.
    /// </summary>
    internal AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> Alarm { get; } = RidleyFadeAnimations.AlarmCycling();
}
