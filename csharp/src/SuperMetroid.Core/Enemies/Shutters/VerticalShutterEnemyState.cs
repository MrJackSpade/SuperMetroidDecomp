using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The state of a vertical shutter. Its extended values correspond one-for-one with the native $7800/$8800 arrays;
/// keeping them explicit avoids another opaque bag of numbered scratch fields. The values the cartridge keeps in the
/// enemy's common record are emptied with it when the enemy is killed (<see cref="ClearWithRecord"/>).
/// </summary>
public sealed class VerticalShutterEnemyState
{
    /// <summary>Where the typed animation (#641) resumes, in place of the native instruction pointer.</summary>
    internal AnimationProgramCursor<ShutterProgram> Program { get; set; }

    /// <summary>
    /// The bank-$A2 dispatcher behaviour, shared by shootable/destroyable shutters and the Kamer vertical platform;
    /// none once a kill has emptied the record.
    /// </summary>
    public VerticalShutterFunction? Function { get; internal set; }

    /// <summary>
    /// Wrapping AI-update countdown; initial waiting activates at zero, while endpoint resting resumes on signed
    /// underflow.
    /// </summary>
    public NativeCountdown FunctionTimer { get; internal set; }
    /// <summary>Downward signed 16.16 velocity in pixels per update, from the common linear-speed table.</summary>
    public SnesSignedSixteenSixteen DownVelocity { get; internal set; }

    /// <summary>
    /// Upward signed 16.16 velocity in pixels per update; its fraction is added together with the negative whole part,
    /// not negated independently.
    /// </summary>
    public SnesSignedSixteenSixteen UpVelocity { get; internal set; }

    /// <summary>Spawn speed selector, 0 through 255: the common linear-speed record containing the downward and upward velocities.</summary>
    public int SpeedTableIndex { get; internal set; }
    /// <summary>Spawn direction selector, 0 through 255: zero starts upward; any other value starts downward.</summary>
    public int PrimaryDirection { get; internal set; }
    /// <summary>Upward endpoint's rest length in sixteen-update units, 0 through 255; 255 selects the inert endpoint.</summary>
    public int MovedUpRestParameter { get; internal set; }
    /// <summary>Downward endpoint's rest length in sixteen-update units, 0 through 255; 255 selects the inert endpoint.</summary>
    public int MovedDownRestParameter { get; internal set; }
    /// <summary>Population trigger mode, validated to 0 through 4: timer, proximity, immediate, one-shot reaction, or repeatable reaction activation.</summary>
    public int TriggerMode { get; internal set; }
    /// <summary>Whole-pixel separation of the two Y endpoints, 0 through 255.</summary>
    public int TravelDistance { get; internal set; }
    /// <summary>Parameter 2, 0 through 65535: initial AI-update wait in timer mode, or strict absolute-X pixel-distance threshold in proximity mode.</summary>
    public int HorizontalProximityOrWaitTime { get; internal set; }
    /// <summary>Twice the trigger mode, 0 through 8: the even offset selecting one of the five initial functions.</summary>
    public int InitialFunctionTableOffset { get; internal set; }
    /// <summary>Upward endpoint's rest length in AI updates, sixteen per parameter unit (0 through 4080); 4080 selects the inert function instead of counting down.</summary>
    public int MovedUpRestTime { get; internal set; }
    /// <summary>Downward endpoint's rest length in AI updates, sixteen per parameter unit (0 through 4080); 4080 selects the inert function instead of counting down.</summary>
    public int MovedDownRestTime { get; internal set; }
    /// <summary>Whether the pre-movement riding test found Samus supported by this shutter; enables whole-pixel extra-Y carry for the current movement.</summary>
    public bool MovingSamus { get; internal set; }
    /// <summary>One-shot reaction latch, initially clear and set by the first mode-3 reaction so later reactions cannot activate movement again.</summary>
    public bool ShotActivated { get; internal set; }
    /// <summary>The shutter's whole-pixel position saved before its current move to derive the rider's wrapped vertical displacement.</summary>
    public Position16<RoomPixels> PreviousPosition { get; internal set; }
    /// <summary>Upper whole-pixel travel endpoint derived from spawn position and travel distance; upward motion stops upon reaching or passing its Y.</summary>
    public Position16<RoomPixels> UpperEndpoint { get; internal set; }
    /// <summary>Lower whole-pixel travel endpoint derived from spawn position and travel distance; downward motion stops upon reaching or passing its Y.</summary>
    public Position16<RoomPixels> LowerEndpoint { get; internal set; }
    /// <summary>Next reaction's direction selector, 0 through 255, initially the primary direction: zero moves up, any other value moves down, then bit zero is toggled.</summary>
    public int ReactionDirection { get; internal set; }

    /// <summary>Empties the values the cartridge keeps in the enemy's common record.</summary>
    internal void ClearWithRecord()
    {
        Function = null;
        FunctionTimer = NativeCountdown.Start(0);
        DownVelocity = SnesSignedSixteenSixteen.Zero;
        UpVelocity = SnesSignedSixteenSixteen.Zero;
    }
}
