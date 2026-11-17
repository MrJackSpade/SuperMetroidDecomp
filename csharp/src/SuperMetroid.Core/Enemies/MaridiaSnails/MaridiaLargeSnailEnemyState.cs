using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The state of Oum.</summary>
public sealed class MaridiaLargeSnailEnemyState
{
    /// <summary>Starts the snail in <paramref name="action"/>, its bounce in <paramref name="bouncePhase"/>.</summary>
    internal MaridiaLargeSnailEnemyState(MaridiaLargeSnailAction action, MaridiaLargeSnailBouncePhase bouncePhase)
    {
        Action = action;
        BouncePhase = bouncePhase;
    }

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MaridiaLargeSnailSegment> Playback { get; set; }

    /// <summary>
    /// The way Oum faces: toward Samus while idle, the way it moves when it attacks, and as it faced then while it rolls
    /// (#2145, #2338). It spawns facing right.
    /// </summary>
    public HorizontalDirection Facing { get; internal set; }

    /// <summary>
    /// Whether Oum's roll runs backwards: a roll from idle or after an attack runs forwards, and each wall it meets
    /// reverses it (#2145).
    /// </summary>
    public bool RollsBackwards { get; internal set; }

    /// <summary>
    /// Whether Oum still shows the left-facing idle it spawns with while it counts as facing right (#2145): until it first
    /// starts an animation, asking for the right-facing idle keeps that left-facing one, and asking for the left-facing
    /// idle starts it afresh.
    /// </summary>
    internal bool ShowsSpawnIdle { get; set; }

    /// <summary>Set by the attack animation's cue when it ends.</summary>
    public bool AttackAnimationFinished { get; internal set; }

    /// <summary>
    /// Set only during the rolling frames where Oum may turn into its next
    /// attack; it says nothing of Oum's hitboxes.
    /// </summary>
    public bool AttackAllowsRotation { get; internal set; }

    /// <summary>Samus is within the strict 24-by-32 proximity window.</summary>
    public bool SamusInPushingRange { get; internal set; }

    /// <summary>The side of Oum Samus is on while she is within its pushing range (#2338); right while she is not.</summary>
    public HorizontalDirection SamusSide { get; internal set; }

    /// <summary>Whole-pixel position captured before this frame's AI.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// Movement suppression produced after AI and consumed on the next frame.
    /// This one-frame pipeline is intentional game ordering, not input latency invented
    /// on top of it.
    /// </summary>
    public bool StopMovementNextFrame { get; internal set; }

    /// <summary>The outer action.</summary>
    public MaridiaLargeSnailAction Action { get; internal set; }

    /// <summary>
    /// The bounce arc's place along the quadratic speed curve, stepped by a mark and a half per update. Falling caps it
    /// at mark 64; an impact removes 16 marks and may leave it below the start, which ends the bounces, and rising
    /// stores the start once it would go below it. It therefore stays within 3712 256ths of a mark below the start
    /// through mark 64.
    /// </summary>
    public QuadraticSpeedCurvePlace YSpeedCurvePlace { get; internal set; }

    /// <summary>Attack cooldown, initialized to 128 and expiring when it steps below zero.</summary>
    public int AttackCooldown { get; internal set; }

    /// <summary>
    /// The way Oum rolls (#2338): toward Samus when it attacks, turned by each wall it meets, and leftward once Samus is
    /// to its left while it idles.
    /// </summary>
    public HorizontalDirection Heading { get; internal set; }

    /// <summary>Remaining landing impacts before Oum is settled.</summary>
    public int RemainingBounces { get; internal set; }

    /// <summary>Falling/rising half of the quadratic bounce.</summary>
    public MaridiaLargeSnailBouncePhase BouncePhase { get; internal set; }
}
