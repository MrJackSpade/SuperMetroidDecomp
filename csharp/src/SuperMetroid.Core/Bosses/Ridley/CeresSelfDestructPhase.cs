using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Where the Ceres station's self-destruct warning has reached after Ridley's getaway: the steps the warning runs
/// through in order, one or more updates each, before the station's countdown starts. Each phase runs itself and enters the
/// next (#2571); one that enters the next in the same update has the warning run that phase too.
/// </summary>
internal sealed partial class CeresSelfDestructPhase
{
    /// <summary>The phase's name, for diagnostics.</summary>
    private readonly string _name;

    /// <summary>
    /// Runs the phase one update on the turn's escape and pace, returning whether it entered the next phase to run in the
    /// same update.
    /// </summary>
    private readonly Func<CeresSelfDestructTurn, bool> _run;

    /// <summary>A phase named <paramref name="name"/> that runs <paramref name="run"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="run">Runs the phase one update, returning whether the next phase runs in the same update.</param>
    private CeresSelfDestructPhase(string name, Func<CeresSelfDestructTurn, bool> run)
    {
        _name = name;
        _run = run;
    }

    /// <summary>The getaway has just ended: the wait for the escape timer and warning starts, its first update this one.</summary>
    internal static CeresSelfDestructPhase Start { get; } = new(nameof(Start), RunStart);

    /// <summary>The escape timer and warning wait to show; the wait's last update shows them and starts the wait for the title.</summary>
    internal static CeresSelfDestructPhase WaitingForWarning { get; } = new(nameof(WaitingForWarning), RunWaitingForWarning);

    /// <summary>The EMERGENCY title waits to show, its wait's first update the warning's showing; the last draws it and starts the music.</summary>
    internal static CeresSelfDestructPhase WaitingForTitle { get; } = new(nameof(WaitingForTitle), RunWaitingForTitle);

    /// <summary>The EMERGENCY title holds, the alarm colours cycling, until its hold runs out.</summary>
    internal static CeresSelfDestructPhase HoldingTitle { get; } = new(nameof(HoldingTitle), RunHoldingTitle);

    /// <summary>
    /// Japanese text only: <paramref name="typewriter"/> runs while the subtitle's wait counts down, then the subtitle is
    /// drawn.
    /// </summary>
    /// <param name="typewriter">The typewriter the title's hold started.</param>
    internal static CeresSelfDestructPhase JapaneseSubtitle(EscapeTypewriterState typewriter) =>
        new(nameof(JapaneseSubtitle), new Typing(typewriter).RunJapaneseSubtitle);

    /// <summary><paramref name="typewriter"/> types the warning, one step per update, until its message ends.</summary>
    /// <param name="typewriter">The typewriter the title's hold started.</param>
    internal static CeresSelfDestructPhase TypingWarning(EscapeTypewriterState typewriter) =>
        new(nameof(TypingWarning), new Typing(typewriter).RunTypingWarning);

    /// <summary>The warning is complete: Ridley stops, the station's countdown starts and the area boss counts as beaten.</summary>
    internal static CeresSelfDestructPhase Finishing { get; } = new(nameof(Finishing), RunFinishing);

    /// <summary>Runs the phase one update.</summary>
    /// <param name="escape">The station's escape, whose warning the phase runs.</param>
    /// <param name="ridleyFrameCounter">Ridley's body's update count, which paces the alarm.</param>
    /// <param name="warning">The room's foreground warning, which the warning's phases write.</param>
    /// <returns>Whether the phase entered the next phase, which runs in this same update.</returns>
    internal bool Run(CeresEscape escape, int ridleyFrameCounter, RoomWarning warning) => _run(new(escape, ridleyFrameCounter, warning));

    /// <summary>Runs <see cref="Start"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    /// <returns>Whether the phase entered the next phase, which runs in this same update.</returns>
    private static bool RunStart(CeresSelfDestructTurn turn) => turn.Escape.RunStart();

    /// <summary>Runs <see cref="WaitingForWarning"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    /// <returns>Whether the phase entered the next phase, which runs in this same update.</returns>
    private static bool RunWaitingForWarning(CeresSelfDestructTurn turn) => turn.Escape.RunWaitingForWarning();

    /// <summary>Runs <see cref="WaitingForTitle"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    /// <returns>Whether the phase entered the next phase, which runs in this same update.</returns>
    private static bool RunWaitingForTitle(CeresSelfDestructTurn turn) => turn.Escape.RunWaitingForTitle(turn.Warning);

    /// <summary>Runs <see cref="HoldingTitle"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    /// <returns>Whether the phase entered the next phase, which runs in this same update.</returns>
    private static bool RunHoldingTitle(CeresSelfDestructTurn turn) => turn.Escape.RunHoldingTitle(turn.RidleyFrameCounter);

    /// <summary>Runs <see cref="Finishing"/> for one update.</summary>
    /// <param name="turn">The update's turn.</param>
    /// <returns>Whether the phase entered the next phase, which runs in this same update.</returns>
    private static bool RunFinishing(CeresSelfDestructTurn turn) => turn.Escape.RunFinishing(turn.RidleyFrameCounter);

    /// <summary>The phases the typewriter the title's hold started runs (#2554), each one update at a time.</summary>
    /// <param name="typewriter">The typewriter the title's hold started.</param>
    private sealed partial class Typing(EscapeTypewriterState typewriter)
    {
        /// <summary>Runs <see cref="JapaneseSubtitle"/> for one update.</summary>
        /// <param name="turn">The update's turn.</param>
        /// <returns>Whether the phase entered the next phase, which runs in this same update.</returns>
        internal bool RunJapaneseSubtitle(CeresSelfDestructTurn turn) => turn.Escape.RunJapaneseSubtitle(turn.RidleyFrameCounter, typewriter, turn.Warning);

        /// <summary>Runs <see cref="TypingWarning"/> for one update.</summary>
        /// <param name="turn">The update's turn.</param>
        /// <returns>Whether the phase entered the next phase, which runs in this same update.</returns>
        internal bool RunTypingWarning(CeresSelfDestructTurn turn) => turn.Escape.RunTypingWarning(turn.RidleyFrameCounter, typewriter, turn.Warning);
    }

    /// <summary>Names the phase.</summary>
    public override string ToString() => _name;
}
