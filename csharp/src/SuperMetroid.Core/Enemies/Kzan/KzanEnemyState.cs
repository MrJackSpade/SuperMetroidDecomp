using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The state of a Kzan top. The bottom half has no state of its own: it follows the top just before it in the room's
/// enemies. The top reads its fall's starting speed and its wait from its spawn settings where it uses them (#2439); each
/// fall or rise carries Samus by the move it makes that update (#2473), and landing sets the rise wait before it counts
/// down.
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
    /// Top-position wait in actor updates, a <see cref="WrappingCountdown"/> counted down past zero, so an initial zero
    /// does not expire immediately. The wrap is load-bearing (#2340): the Wrecked Ship main shaft's four Kzans, placed before its power
    /// returns, wait zero, so each waits the 65536 updates of the full wrap before it first falls and between falls.
    /// </summary>
    public WrappingCountdown FallWaitTimer { get; internal set; }

    /// <summary>
    /// The top's original room Y, where its rise stops before the wait restarts.
    /// </summary>
    public int RisingTargetY { get; internal set; }

    /// <summary>
    /// The lowest room Y of the fall: the spawn Y plus the spawn settings' fall distance, inside the room.
    /// </summary>
    public int FallingTargetY { get; internal set; }

    /// <summary>
    /// The speed, in pixels per update, the fall moves at. It starts at the spawn settings' fall speed, grows by one
    /// sixteenth of a pixel per falling update and is capped at four pixels per update.
    /// </summary>
    public SignedSixteenSixteen FallingSpeed { get; internal set; }

    /// <summary>Post-landing wait in actor updates; loaded with 64 on landing and decremented to zero before upward motion begins.</summary>
    public int RiseWaitTimer { get; internal set; }
}
