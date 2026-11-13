using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The state of a Kzan top. The bottom half has no private state: it follows the immediately preceding top slot. The
/// top's values are its own: setup writes the wait reload and the speed row, each fall or rise writes the previous
/// position before carrying Samus by it, and landing writes the rise wait before it counts down, so nothing a previous
/// Kzan in the slot left reaches a new one.
/// </summary>
public sealed class KzanEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<KzanSegment> Playback { get; set; }

    /// <summary>The current AI behaviour.</summary>
    public KzanAction? Action { get; internal set; }

    /// <summary>
    /// Top-position wait in actor updates; decremented with a wrap below zero to 65535, so an initial zero does not expire
    /// immediately.
    /// </summary>
    public WrappingCountdown FallWaitTimer { get; internal set; }

    /// <summary>
    /// The top's original position, where upward motion stops before restarting the wait; only its Y is meaningful.
    /// Positions compare with it by the wrapped spatial difference.
    /// </summary>
    public Position16<RoomPixels> RisingTarget
    {
        get;
        internal set => field = new(0, value.Y);
    }

    /// <summary>
    /// The lowest position of the fall, the spawn Y plus the spawn settings' fall distance with
    /// room-coordinate wraparound; only its Y is meaningful. Positions compare with it by the wrapped spatial
    /// difference.
    /// </summary>
    public Position16<RoomPixels> FallingTarget
    {
        get;
        internal set => field = new(0, value.Y);
    }

    /// <summary>Reload value for the top wait in actor updates, 0 through 255, from the spawn's fall wait.</summary>
    public int FallWaitTimerResetValue { get; internal set; }

    /// <summary>The top's position saved before a falling or rising update; only its Y is read, and the accepted whole-pixel movement since it determines rider displacement.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// The speed, in pixels per update, the fall moves at. It starts at the spawn settings' fall speed, grows by one
    /// sixteenth of a pixel per falling update and is capped at four pixels per update.
    /// </summary>
    public SignedSixteenSixteen FallingSpeed { get; internal set; }

    /// <summary>Post-landing wait in actor updates; loaded with 64 on landing and decremented to zero before upward motion begins.</summary>
    public int RiseWaitTimer { get; internal set; }
}
