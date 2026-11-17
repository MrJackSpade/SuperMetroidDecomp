using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The state of a Kzan top. The bottom half has no private state: it follows the top just before it in the room's
/// enemies. The top's values are its own: setup writes the wait reload and the speed, each fall or rise writes the
/// previous position before carrying Samus by it, and landing writes the rise wait before it counts down, so nothing a
/// previous Kzan left reaches a new one.
/// </summary>
public sealed class KzanEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal KzanEnemyState(KzanAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<KzanSegment> Playback { get; set; }

    /// <summary>The current AI behaviour.</summary>
    public KzanAction Action { get; internal set; }

    /// <summary>
    /// Top-position wait in actor updates; decremented with a wrap below zero to 65535, so an initial zero does not expire
    /// immediately. The wrap is load-bearing (#2340): the Wrecked Ship main shaft's four Kzans, placed before its power
    /// returns, wait zero, so each waits the 65536 updates of the full wrap before it first falls and between falls.
    /// </summary>
    public WrappingCountdown FallWaitTimer { get; internal set; }

    /// <summary>
    /// The top's original room Y, where upward motion stops before restarting the wait. Positions compare with it by the
    /// wrapped difference.
    /// </summary>
    public int RisingTargetY { get; internal set; }

    /// <summary>
    /// The lowest room Y of the fall, the spawn Y plus the spawn settings' fall distance with room-coordinate
    /// wraparound. Positions compare with it by the wrapped difference.
    /// </summary>
    public int FallingTargetY { get; internal set; }

    /// <summary>Reload value for the top wait in actor updates, from the spawn's fall wait.</summary>
    public int FallWaitTimerResetValue { get; internal set; }

    /// <summary>The top's room Y saved before a falling or rising update; the accepted whole-pixel movement since it determines rider displacement.</summary>
    public int PreviousY { get; internal set; }

    /// <summary>
    /// The speed, in pixels per update, the fall moves at. It starts at the spawn settings' fall speed, grows by one
    /// sixteenth of a pixel per falling update and is capped at four pixels per update.
    /// </summary>
    public SignedSixteenSixteen FallingSpeed { get; internal set; }

    /// <summary>Post-landing wait in actor updates; loaded with 64 on landing and decremented to zero before upward motion begins.</summary>
    public int RiseWaitTimer { get; internal set; }
}
