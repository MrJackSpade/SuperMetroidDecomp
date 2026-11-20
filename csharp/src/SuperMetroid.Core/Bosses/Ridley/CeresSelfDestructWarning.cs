using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// The warning Ceres's self-destruct shows and types once the getaway ends (#2228): its phase, its phase's wait, its
/// typewriter, its alarm's step and what it has written over the arena's foreground.
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
    /// <summary>Gets the installed warning typewriter state, created after the initial delay; null before then.</summary>
    public EscapeTypewriterState? Typewriter { get; internal set; }
    /// <summary>
    /// The warning's cycling alarm (#2316, #2409), which shows nothing before its first step; each step, every fourth
    /// update, plays it on to its next colouring, the first after the last.
    /// </summary>
    internal AnimationPlayer<RidleyFadeSegment, RidleyFadeImage, NoAnimationCue> Alarm { get; } = RidleyFadeAnimations.AlarmCycling();
    /// <summary>
    /// What the warning shows over the arena's foreground (#1555): its EMERGENCY title, the Japanese subtitle and the glyphs
    /// its typewriter has typed, in the order shown, each at its foreground cell; nothing before it shows any.
    /// </summary>
    public RoomWarningText Overlay { get; private set; } = RoomWarningText.Empty;

    /// <summary>Shows <paramref name="picture"/> as the next part of <see cref="Overlay"/>.</summary>
    internal void Show(WrittenPicture picture) => Overlay = Overlay.With(picture);
}
