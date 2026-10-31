using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>
/// The state of Oum, with its ten family-extra values. The values the cartridge keeps in the enemy's common record are
/// emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class MaridiaLargeSnailEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<MaridiaLargeSnailProgram> Program { get; set; }

    /// <summary>
    /// Extra word $00: the requested animation, 0 through 7, an index into the eight-entry selector table at $CB77.
    /// Its lowest bit selects the left-facing program, and the idle, roll and attack transitions set, clear and toggle
    /// its upper two bits, which never carries it outside the table.
    /// </summary>
    public int RequestedInstructionListIndex { get; internal set; }

    /// <summary>Extra word $01: the last requested animation <c>SetOumInstList</c> installed, 0 through 7.</summary>
    public int InstalledInstructionListIndex { get; internal set; }

    /// <summary>Extra word $02: set by instruction $CCB3 when the attack animation ends.</summary>
    public bool AttackAnimationFinished { get; internal set; }

    /// <summary>
    /// Extra word $03: set only during the rolling frames where Oum may turn into its next
    /// attack. The native name is "attack allowing rotation flag"; it is not a hitbox flag.
    /// </summary>
    public bool AttackAllowsRotation { get; internal set; }

    /// <summary>Extra word $04: Samus is within the strict 24-by-32 proximity window.</summary>
    public bool SamusInPushingRange { get; internal set; }

    /// <summary>Extra word $05: one when Samus is to the left, zero when to the right.</summary>
    public bool SamusIsLeft { get; internal set; }

    /// <summary>Extra words $06/$08: whole-pixel position captured before this frame's AI.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// Extra word $0B: movement suppression produced after AI and consumed on the next frame.
    /// This one-frame pipeline is intentional cartridge ordering, not input latency invented
    /// by the host translation.
    /// </summary>
    public bool StopMovementNextFrame { get; internal set; }

    /// <summary>Outer behavior function; none once a kill has emptied the record.</summary>
    public MaridiaLargeSnailEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// The bounce arc's quadratic-speed record in 256ths, stepped by 384 per update. Falling caps it at 16384; an
    /// impact removes 4096 and may leave it below zero, which ends the bounces, and rising stores zero once it would go
    /// below zero. It therefore stays within -3712 through 16384.
    /// </summary>
    public int YSpeedTableIndex
    {
        get;
        internal set => field = Ensure.BetweenInclusive(short.MinValue, short.MaxValue, value);
    }

    /// <summary>Signed-underflow attack cooldown, initialized to 128.</summary>
    public WrappingCountdown AttackCooldown { get; internal set; }

    /// <summary>Whether the snail moves left rather than right.</summary>
    public bool MovingLeft { get; internal set; }

    /// <summary>Remaining landing impacts before Oum is settled.</summary>
    public WrappingCountdown RemainingBounces { get; internal set; }

    /// <summary>Falling/rising half of the quadratic bounce; none once a kill has emptied the record.</summary>
    public MaridiaLargeSnailBounceFunction? BounceFunction { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        YSpeedTableIndex = 0;
        AttackCooldown = WrappingCountdown.Start(0);
        MovingLeft = false;
        RemainingBounces = WrappingCountdown.Start(0);
        BounceFunction = null;
    }
}
