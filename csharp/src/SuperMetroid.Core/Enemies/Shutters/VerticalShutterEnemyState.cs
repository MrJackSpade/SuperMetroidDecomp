using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The state of a vertical shutter: its behaviour, speeds, endpoints, rest lengths, trigger and rider carry, each a
/// value of its own.
/// </summary>
public sealed class VerticalShutterEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShutterSegment> Playback { get; set; }

    /// <summary>
    /// The dispatcher behaviour, shared by shootable/destroyable shutters and the Kamer vertical platform.
    /// </summary>
    public VerticalShutterAction? Action { get; internal set; }

    /// <summary>
    /// Wrapping AI-update countdown; initial waiting activates at zero, while endpoint resting resumes on signed
    /// underflow.
    /// </summary>
    public WrappingCountdown ActionTimer { get; internal set; }
    /// <summary>Downward signed 16.16 velocity in pixels per update, from the common linear-speed table.</summary>
    public SignedSixteenSixteen DownVelocity { get; internal set; }

    /// <summary>
    /// Upward signed 16.16 velocity in pixels per update; its fraction is added together with the negative whole part,
    /// not negated independently.
    /// </summary>
    public SignedSixteenSixteen UpVelocity { get; internal set; }

    /// <summary>The end the shutter first heads for, as its population entry names it.</summary>
    public ShutterHeading PrimaryDirection { get; internal set; }
    /// <summary>Population trigger mode: timer, proximity, immediate, one-shot reaction, or repeatable reaction activation.</summary>
    public ShutterTriggerMode TriggerMode { get; internal set; }
    /// <summary>Whole-pixel separation of the two Y endpoints.</summary>
    public int TravelDistance { get; internal set; }
    /// <summary>The strict horizontal pixel distance Samus must come within to start a proximity-triggered shutter.</summary>
    public int ProximityDistance { get; internal set; }
    /// <summary>The AI updates a timer-triggered shutter waits before it moves.</summary>
    public int WaitTime { get; internal set; }
    /// <summary>Upward endpoint's rest length in AI updates, a multiple of sixteen; 4080 selects the inert function instead of counting down.</summary>
    public int MovedUpRestTime { get; internal set; }
    /// <summary>Downward endpoint's rest length in AI updates, a multiple of sixteen; 4080 selects the inert function instead of counting down.</summary>
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
    /// <summary>The end the next reaction heads for, initially the primary heading; it turns after each reaction.</summary>
    public ShutterHeading ReactionDirection { get; internal set; }
}
