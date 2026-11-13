using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The state of Oum, with its ten family-extra values.</summary>
public sealed class MaridiaLargeSnailEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<MaridiaLargeSnailProgram> Playback { get; set; }

    /// <summary>
    /// The animation the snail's behaviour asks for: idle facing Samus, a forward roll from idle, an attack facing Samus,
    /// a forward roll again once the attack ends, and the opposite roll each time a roll meets a wall.
    /// </summary>
    public MaridiaLargeSnailAnimation RequestedAnimation { get; internal set; }

    /// <summary>
    /// The requested animation the snail last started. Asking for it again leaves the playing animation running, and the
    /// snail starts out holding the right-facing idle here while it shows the left-facing one.
    /// </summary>
    public MaridiaLargeSnailAnimation InstalledAnimation { get; internal set; }

    /// <summary>Extra word two: set by the attack animation's cue when it ends.</summary>
    public bool AttackAnimationFinished { get; internal set; }

    /// <summary>
    /// Set only during the rolling frames where Oum may turn into its next
    /// attack. The name is "attack allowing rotation flag"; it is not a hitbox flag.
    /// </summary>
    public bool AttackAllowsRotation { get; internal set; }

    /// <summary>Samus is within the strict 24-by-32 proximity window.</summary>
    public bool SamusInPushingRange { get; internal set; }

    /// <summary>One when Samus is to the left, zero when to the right.</summary>
    public bool SamusIsLeft { get; internal set; }

    /// <summary>Whole-pixel position captured before this frame's AI.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// Movement suppression produced after AI and consumed on the next frame.
    /// This one-frame pipeline is intentional game ordering, not input latency invented
    /// by the host translation.
    /// </summary>
    public bool StopMovementNextFrame { get; internal set; }

    /// <summary>Outer behavior function.</summary>
    public MaridiaLargeSnailEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The bounce arc's place along the quadratic speed table, stepped by a record and a half per update. Falling caps it
    /// at record 64; an impact removes 16 records and may leave it below the start, which ends the bounces, and rising
    /// stores the start once it would go below it. It therefore stays within 3712 256ths of a record below the start
    /// through record 64.
    /// </summary>
    public EnemyQuadraticSpeedIndex YSpeedTableIndex { get; internal set; }

    /// <summary>Attack cooldown, initialized to 128 and expiring when it steps below zero.</summary>
    public int AttackCooldown { get; internal set; }

    /// <summary>Whether the snail moves left rather than right.</summary>
    public bool MovingLeft { get; internal set; }

    /// <summary>Remaining landing impacts before Oum is settled.</summary>
    public int RemainingBounces { get; internal set; }

    /// <summary>Falling/rising half of the quadratic bounce.</summary>
    public MaridiaLargeSnailBounceFunction? BounceFunction { get; internal set; }
}
