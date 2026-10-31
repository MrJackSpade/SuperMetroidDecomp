using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The state of a Kzan top. The bottom half has no private state: it follows the immediately preceding top slot. The
/// values the cartridge keeps in the enemy's common record are emptied with it when the enemy is killed
/// (<see cref="ClearWithRecord"/>). The rest are the Kzan's own too: setup writes the wait reload and the speed row,
/// each fall or rise writes the previous position before carrying Samus by it, and landing writes the rise wait before
/// it counts down, so nothing a previous Kzan in the slot left reaches a new one.
/// </summary>
public sealed class KzanEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<KzanProgram> Program { get; set; }

    /// <summary>The current AI behaviour; none once a kill has emptied the record.</summary>
    public KzanEnemyFunction? Function { get; internal set; }

    /// <summary>
    /// Top-position wait in actor updates; decremented with 16-bit wrapping, so an initial zero does not expire
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
    /// The lowest position of the fall, the spawn Y plus population parameter 2's high-byte fall distance with
    /// room-coordinate wraparound; only its Y is meaningful. Positions compare with it by the wrapped spatial
    /// difference.
    /// </summary>
    public Position16<RoomPixels> FallingTarget
    {
        get;
        internal set => field = new(0, value.Y);
    }

    /// <summary>
    /// The initial downward speed sample selected by population parameter 1's low byte, in pixels per update; this
    /// initialization snapshot is not the accelerating current speed.
    /// </summary>
    public SignedSixteenSixteen InitialFallingSpeed { get; internal set; }

    /// <summary>Reload value for the top wait in actor updates, 0 through 255, from the spawn's fall wait.</summary>
    public int FallWaitTimerResetValue { get; internal set; }

    /// <summary>The top's position saved before a falling or rising update; only its Y is read, and the accepted whole-pixel movement since it determines rider displacement.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }

    /// <summary>
    /// The row of the shared linear speed table the fall moves at. Initialized from population parameter 1's low byte
    /// (0 through 255), it advances one row per falling update and is capped at row 64.
    /// </summary>
    public int FallingSpeedRow { get; internal set; }

    /// <summary>Post-landing wait in actor updates; loaded with 64 on landing and decremented to zero before upward motion begins.</summary>
    public WrappingCountdown RiseWaitTimer { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        FallWaitTimer = WrappingCountdown.Start(0);
        RisingTarget = Position16<RoomPixels>.Zero;
        FallingTarget = Position16<RoomPixels>.Zero;
        InitialFallingSpeed = SignedSixteenSixteen.Zero;
    }
}
