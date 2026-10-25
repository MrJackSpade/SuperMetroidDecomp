using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// The state of a Kzan top; its last four values live in the native bank-$7E extension arrays. The bottom half has no
/// private state: it follows the immediately preceding top slot. The values the cartridge keeps in the enemy's common
/// record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class KzanEnemyState
{
    /// <summary>The slot's Kzan extension words, kept for the room across occupants.</summary>
    private readonly KzanExtensionWords _words;

    /// <summary>Projects a Kzan top slot and its bank-$7E extension arrays into typed state.</summary>
    internal KzanEnemyState(KzanExtensionWords words)
    {
        _words = words;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<KzanProgram> Program { get; set; }

    /// <summary>Current bank-$A6 AI entry; none once a kill has emptied the record.</summary>
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
    public SnesSignedSixteenSixteen InitialFallingSpeed { get; internal set; }

    /// <summary>Reload value for the top wait in actor updates, 0 through 255, copied from population parameter 2's low byte into the owning slot's native <c>$7E:7800,x</c> extension word.</summary>
    public int FallWaitTimerResetValue
    {
        get => _words.FallWaitTimerResetValue;
        internal set => _words.FallWaitTimerResetValue = value;
    }

    /// <summary>The top's position saved before a falling or rising update, backed by <c>$7E:7802,x</c>; only its Y is read, and the accepted whole-pixel movement since it determines rider displacement.</summary>
    public Position16<RoomPixels> PreviousPosition
    {
        get => _words.PreviousPosition;
        internal set => _words.PreviousPosition = value;
    }

    /// <summary>
    /// Byte offset into <c>CommonEnemySpeeds_LinearlyIncreasing</c>. Kzan advances by eight bytes per falling frame
    /// because each logical speed owns positive and negative pairs. Backed by <c>$7E:7804,x</c>, initialized from
    /// population parameter 1's low byte times eight (0 through 2040), and capped at 512 after each falling update.
    /// </summary>
    public int FallingYSpeedTableIndex
    {
        get => _words.FallingYSpeedTableIndex;
        internal set => _words.FallingYSpeedTableIndex = value;
    }

    /// <summary>Post-landing wait in actor updates, backed by <c>$7E:7806,x</c>; loaded with 64 on landing and decremented to zero before upward motion begins.</summary>
    public WrappingCountdown RiseWaitTimer
    {
        get => _words.RiseWaitTimer;
        internal set => _words.RiseWaitTimer = value;
    }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        FallWaitTimer = WrappingCountdown.Start(0);
        RisingTarget = Position16<RoomPixels>.Zero;
        FallingTarget = Position16<RoomPixels>.Zero;
        InitialFallingSpeed = SnesSignedSixteenSixteen.Zero;
    }
}
