using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>The state of Oum, with its ten family-extra values.</summary>
public sealed class MaridiaLargeSnailEnemyState
{
    /// <summary>Where the animation resumes.</summary>
    internal AnimationProgramCursor<MaridiaLargeSnailProgram> Program { get; set; }

    /// <summary>
    /// Extra word zero: the requested animation, 0 through 7, an index into the eight-entry selector table.
    /// Its lowest bit selects the left-facing program, and the idle, roll and attack transitions set, clear and toggle
    /// its upper two bits, which never carries it outside the table.
    /// </summary>
    public int RequestedAnimationScriptIndex { get; internal set; }

    /// <summary>The last requested animation <c>SetOumInstList</c> installed, 0 through 7.</summary>
    public int InstalledAnimationScriptIndex { get; internal set; }

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
    /// The bounce arc's quadratic-speed record in 256ths, stepped by 384 per update. Falling caps it at 16384; an
    /// impact removes 4096 and may leave it below zero, which ends the bounces, and rising stores zero once it would go
    /// below zero. It therefore stays within -3712 through 16384.
    /// </summary>
    public int YSpeedTableIndex { get; internal set; }

    /// <summary>Attack cooldown, initialized to 128 and expiring when it steps below zero.</summary>
    public int AttackCooldown { get; internal set; }

    /// <summary>Whether the snail moves left rather than right.</summary>
    public bool MovingLeft { get; internal set; }

    /// <summary>Remaining landing impacts before Oum is settled.</summary>
    public int RemainingBounces { get; internal set; }

    /// <summary>Falling/rising half of the quadratic bounce.</summary>
    public MaridiaLargeSnailBounceFunction? BounceFunction { get; internal set; }
}
