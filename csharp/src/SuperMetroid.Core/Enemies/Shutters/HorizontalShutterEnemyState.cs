using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>The state of the cartridge's complete, though retail-unused, horizontal shutter. The values the cartridge keeps in the enemy's common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).</summary>
public sealed class HorizontalShutterEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ShutterProgram> Program { get; set; }

    /// <summary>
    /// The bank-$A2 dispatcher behaviour of the retail-unused horizontal shutter, retained across wait, movement, and
    /// endpoint-rest updates; none once a kill has emptied the record.
    /// </summary>
    public HorizontalShutterFunction? Function { get; internal set; }

    /// <summary>
    /// Wrapping AI-update countdown; initial wait tests for zero after decrement, whereas endpoint rests test for
    /// signed underflow.
    /// </summary>
    public NativeCountdown FunctionTimer { get; internal set; }

    /// <summary>
    /// Rightward signed 16.16 velocity in pixels per update, also supplied to an overlapping Samus's extra-X
    /// displacement.
    /// </summary>
    public SnesSignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>
    /// Leftward signed 16.16 velocity in pixels per update, also supplied to an overlapping Samus's extra-X
    /// displacement.
    /// </summary>
    public SnesSignedSixteenSixteen LeftVelocity { get; internal set; }

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

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        FunctionTimer = NativeCountdown.Start(0);
        RightVelocity = SnesSignedSixteenSixteen.Zero;
        LeftVelocity = SnesSignedSixteenSixteen.Zero;
    }
}
