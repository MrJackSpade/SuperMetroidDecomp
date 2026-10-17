using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Owtch;

/// <summary>
/// Typed view of Owtch's six ordinary enemy words and three extended WRAM words. Keeping
/// the native split velocities visible makes debugger watches directly comparable with
/// <c>$0FA8-$0FB2</c> and the extended <c>$7E:7800-$7804</c> record.
/// </summary>
public sealed class OwtchEnemyState
{
    /// <summary>Ordinary enemy record supplying Owtch's common velocity, state, and burial words.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Attaches the typed Owtch view to its physical enemy slot.</summary>
    internal OwtchEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<OwtchProgram> Program { get; set; }

    /// <summary>The rightward speed's fraction in 65536ths of a pixel, 0 through 65535 (native <c>$0FA8,x</c>).</summary>
    private int _rightVelocityFraction;

    /// <summary>
    /// The signed 16.16 rightward patrol speed in pixels per update, taken from the shared linear-speed table (native
    /// <c>$0FA8,x</c> fraction and <c>$0FAA,x</c> whole). Its whole pixels are kept in variable A for other enemies'
    /// aliased enemy-RAM reads.
    /// </summary>
    public SnesSignedSixteenSixteen RightVelocity
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableA), checked((ushort)_rightVelocityFraction));
        internal set
        {
            _slot.VariableA = value.WholeWord;
            _rightVelocityFraction = value.Fraction;
        }
    }

    /// <summary>
    /// The signed 16.16 leftward patrol speed in pixels per update, the negative half of the same linear-speed record
    /// (native <c>$0FAC,x</c> fraction and <c>$0FAE,x</c> whole). Its fraction and whole pixels are kept in variables B
    /// and C for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public SnesSignedSixteenSixteen LeftVelocity
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableC), _slot.VariableB);
        internal set
        {
            _slot.VariableB = value.Fraction;
            _slot.VariableC = value.WholeWord;
        }
    }

    /// <summary>Native <c>$0FB0,x</c> state/table index.</summary>
    public OwtchBehaviorState Behavior
    {
        get => (OwtchBehaviorState)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>
    /// Native <c>$0FB2,x</c>: how many pixels the Owtch has sunk below its walking height, zero through sixteen while
    /// it sinks; set directly by initialization and the sinking steps. Rising counts it down through
    /// <see cref="RiseOnePixel"/>, which can wrap it. It stays in variable E for other enemies' aliased enemy-RAM reads.
    /// </summary>
    public int BurialDepth
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = checked((ushort)value);
    }

    /// <summary>
    /// Counts one rising pixel off the burial depth and reports whether it reached zero (<c>DEC</c>, <c>BNE</c>). An
    /// Owtch that starts rising unburied wraps to 65535, as the cartridge's decrement does, and keeps rising that many
    /// more updates.
    /// </summary>
    internal bool RiseOnePixel()
    {
        NativeWordCounterStep step = NativeWordCounter.Decrement(_slot.VariableE);
        _slot.VariableE = step.Value;
        return step.IsZero;
    }

    /// <summary>Native extended word <c>$7E:7800,x</c>.</summary>
    public NativeCountdown UndergroundTimer { get; internal set; }

    /// <summary>
    /// Native extended word <c>$7E:7802,x</c>: the left end of the patrol, the spawn X less the travel distance with
    /// room-coordinate wraparound; only its X is meaningful. Positions compare with it by the wrapped spatial difference.
    /// </summary>
    public Position16<RoomPixels> MinimumPosition { get; internal set; }

    /// <summary>
    /// Native extended word <c>$7E:7804,x</c>: the right end of the patrol, the spawn X plus the travel distance with
    /// room-coordinate wraparound; only its X is meaningful. Positions compare with it by the wrapped spatial difference.
    /// </summary>
    public Position16<RoomPixels> MaximumPosition { get; internal set; }
}
