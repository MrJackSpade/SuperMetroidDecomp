using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>Typed state for the cartridge's complete, though retail-unused, horizontal shutter.</summary>
public sealed class HorizontalShutterEnemyState
{
    /// <summary>Backs native variable A-F access for one horizontal shutter slot.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates the typed view over a room-owned horizontal shutter slot.</summary>
    internal HorizontalShutterEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ShutterProgram> Program { get; set; }

    /// <summary>Enemy variable A's bank-$A2 dispatcher pointer for the retail-unused horizontal shutter, retained across wait, movement, and endpoint-rest updates.</summary>
    public HorizontalShutterFunction Function
    {
        get => (HorizontalShutterFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Variable B: wrapping AI-update countdown; initial wait tests for zero after decrement, whereas endpoint rests test for signed underflow.</summary>
    public NativeCountdown FunctionTimer { get => NativeCountdown.FromRaw(_slot.VariableB); internal set => _slot.VariableB = value.RawValue; }

    /// <summary>
    /// Rightward signed 16.16 velocity in pixels per update, also supplied to an overlapping Samus's extra-X
    /// displacement. Its fraction and whole halves stay in variables C and D, which other enemies' aliased
    /// enemy-RAM reads can observe.
    /// </summary>
    public SnesSignedSixteenSixteen RightVelocity
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableD), _slot.VariableC);
        internal set
        {
            _slot.VariableC = value.Fraction;
            _slot.VariableD = value.WholeWord;
        }
    }

    /// <summary>
    /// Leftward signed 16.16 velocity in pixels per update, also supplied to an overlapping Samus's extra-X
    /// displacement. Its fraction and whole halves stay in variables E and F for the same aliased reads.
    /// </summary>
    public SnesSignedSixteenSixteen LeftVelocity
    {
        get => SnesSignedSixteenSixteen.FromParts(unchecked((short)_slot.VariableF), _slot.VariableE);
        internal set
        {
            _slot.VariableE = value.Fraction;
            _slot.VariableF = value.WholeWord;
        }
    }

    /// <summary>Spawn speed selector, 0 through 255: the common linear-speed record holding the rightward and leftward velocities.</summary>
    public int SpeedTableIndex { get; internal set; }
    /// <summary>Spawn direction selector, 0 through 255: zero starts leftward; any other value starts rightward.</summary>
    public int PrimaryDirection { get; internal set; }
    /// <summary>Left endpoint's rest length in sixteen-update units, 0 through 255; 255 selects an inert endpoint.</summary>
    public int MovedLeftRestParameter { get; internal set; }
    /// <summary>Right endpoint's rest length in sixteen-update units, 0 through 255; 255 selects an inert endpoint.</summary>
    public int MovedRightRestParameter { get; internal set; }
    /// <summary>Population trigger mode, validated to 0 through 4: timer, horizontal proximity, immediate, one-shot reaction, or repeatable reaction activation.</summary>
    public int TriggerMode { get; internal set; }
    /// <summary>Whole-pixel X separation of the two endpoints, 0 through 255; the native storage calls this field YDistance.</summary>
    public int TravelDistance { get; internal set; }
    /// <summary>Parameter 2, 0 through 65535: initial AI-update wait or strict absolute horizontal pixel-distance threshold, according to trigger mode.</summary>
    public int HorizontalProximityOrWaitTime { get; internal set; }
    /// <summary>Twice the trigger mode, 0 through 8: the even offset selecting one of the five initial functions.</summary>
    public int InitialFunctionTableOffset { get; internal set; }
    /// <summary>Left endpoint's rest length in AI updates, sixteen per parameter unit (0 through 4080); 4080 installs the inert function.</summary>
    public int MovedLeftRestTime { get; internal set; }
    /// <summary>Right endpoint's rest length in AI updates, sixteen per parameter unit (0 through 4080); 4080 installs the inert function.</summary>
    public int MovedRightRestTime { get; internal set; }
    /// <summary>Initially clear mode-3 activation latch; the first eligible reaction sets it permanently for this enemy lifetime.</summary>
    public bool ShotActivated { get; internal set; }
    /// <summary>Left whole-pixel travel endpoint derived from spawn position and travel distance; leftward motion stops upon reaching or passing its X.</summary>
    public Position16<RoomPixels> LeftEndpoint { get; internal set; }
    /// <summary>Right whole-pixel travel endpoint derived from spawn position and travel distance; rightward motion stops upon reaching or passing its X.</summary>
    public Position16<RoomPixels> RightEndpoint { get; internal set; }
    /// <summary>Whether Samus overlaps the shutter on its leading side before movement, enabling fixed-point extra-X displacement and opposing-input ejection.</summary>
    public bool MovingSamus { get; internal set; }
    /// <summary>
    /// Alternating reaction direction, 0 through 255: initialized as the primary direction with bit zero inverted and
    /// toggled before each reaction, which moves left for zero and right otherwise. A primary direction above one
    /// therefore never reacts leftward.
    /// </summary>
    public int ReactionDirection { get; internal set; }
}
