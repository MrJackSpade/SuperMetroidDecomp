using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Kzan;

/// <summary>
/// Debugger-facing projection of every private Kzan-top word. The first six fields alias
/// ordinary enemy variables A-F; the final four live in the native bank-$7E extension arrays.
/// The bottom half has no private state: it follows the immediately preceding top slot.
/// </summary>
public sealed class KzanEnemyState
{
    /// <summary>Top-half enemy slot containing the native A-F state words.</summary>
    private readonly RoomEnemySlot _slot;
    /// <summary>The slot's Kzan extension words, kept for the room across occupants.</summary>
    private readonly KzanExtensionWords _words;

    /// <summary>Projects a Kzan top slot and its bank-$7E extension arrays into typed state.</summary>
    internal KzanEnemyState(
        RoomEnemySlot slot, KzanExtensionWords words)
    {
        _slot = slot;
        _words = words;
    }

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<KzanProgram> Program { get; set; }

    /// <summary>Current bank-$A6 AI entry, backed by enemy variable A at native <c>$0FA8,x</c>; this view reads the owning top slot directly.</summary>
    public KzanEnemyFunction Function
    {
        get => (KzanEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Top-position wait in actor updates, backed by variable B at <c>$0FAA,x</c>; decremented with 16-bit wrapping, so an initial zero does not expire immediately.</summary>
    public NativeCountdown FallWaitTimer
    {
        get => NativeCountdown.FromRaw(_slot.VariableB);
        internal set => _slot.VariableB = value.RawValue;
    }

    /// <summary>The top's original position, where upward motion stops before restarting the wait; only its Y is meaningful, and it stays in variable C (<c>$0FAC,x</c>) for other enemies' aliased enemy-RAM reads. Positions compare with it by the wrapped spatial difference.</summary>
    public Position16<RoomPixels> RisingTarget
    {
        get => new(0, _slot.VariableC);
        internal set => _slot.VariableC = value.Y;
    }

    /// <summary>The lowest position of the fall, the spawn Y plus population parameter 2's high-byte fall distance with room-coordinate wraparound; only its Y is meaningful, and it stays in variable D (<c>$0FAE,x</c>) for other enemies' aliased enemy-RAM reads. Positions compare with it by the wrapped spatial difference.</summary>
    public Position16<RoomPixels> FallingTarget
    {
        get => new(0, _slot.VariableD);
        internal set => _slot.VariableD = value.Y;
    }

    /// <summary>
    /// The initial downward speed sample selected by population parameter 1's low byte, in pixels per update; this
    /// initialization snapshot is not the accelerating current speed. Its fraction and whole halves stay in variables E
    /// and F (<c>$0FB0,x</c> and <c>$0FB2,x</c>) for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public SnesSignedSixteenSixteen InitialFallingSpeed
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableF), _slot.VariableE);
        internal set
        {
            _slot.VariableE = value.Fraction;
            _slot.VariableF = value.WholeWord;
        }
    }

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
    /// Byte offset into <c>CommonEnemySpeeds_LinearlyIncreasing</c>. Kzan advances by eight
    /// bytes per falling frame because each logical speed owns positive and negative pairs.
    /// Backed by <c>$7E:7804,x</c>, initialized from population parameter 1's low byte times
    /// eight (0 through 2040), and capped at 512 after each falling update.
    /// </summary>
    public int FallingYSpeedTableIndex
    {
        get => _words.FallingYSpeedTableIndex;
        internal set => _words.FallingYSpeedTableIndex = value;
    }

    /// <summary>Post-landing wait in actor updates, backed by <c>$7E:7806,x</c>; loaded with 64 on landing and decremented to zero before upward motion begins.</summary>
    public NativeCountdown RiseWaitTimer
    {
        get => NativeCountdown.FromRaw(_words.RiseWaitTimer);
        internal set => _words.RiseWaitTimer = value.RawValue;
    }
}
