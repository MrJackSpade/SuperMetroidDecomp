using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The state of a vertical shutter: its behaviour, speeds, endpoints, rest lengths, trigger and rider carry, each a
/// value of its own.
/// </summary>
public sealed class VerticalShutterEnemyState
{
    /// <summary>Starts the state in <paramref name="action"/>.</summary>
    internal VerticalShutterEnemyState(VerticalShutterAction action) => Action = action;

    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShutterSegment> Playback { get; set; }

    /// <summary>
    /// The current behaviour, shared by shootable/destroyable shutters and the Kamer vertical platform.
    /// </summary>
    public VerticalShutterAction Action { get; internal set; }

    /// <summary>
    /// Wrapping AI-update countdown; initial waiting activates at zero, while endpoint resting resumes once
    /// it passes zero.
    /// </summary>
    public WrappingCountdown ActionTimer { get; internal set; }
    /// <summary>Downward signed 1/65536-pixel velocity in pixels per update, one of the common linear speeds.</summary>
    public SignedSixteenSixteen DownVelocity { get; internal set; }

    /// <summary>
    /// Upward signed 1/65536-pixel velocity in pixels per update; its fraction is added together with the negative whole part,
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
    /// <summary>How long the shutter rests at its upper end, or that it stays there until a reaction starts it again.</summary>
    public ShutterRest MovedUpRest { get; internal set; }
    /// <summary>How long the shutter rests at its lower end, or that it stays there until a reaction starts it again.</summary>
    public ShutterRest MovedDownRest { get; internal set; }
    /// <summary>Whether the pre-movement riding test found Samus supported by this shutter; enables whole-pixel extra-Y carry for the current movement.</summary>
    public bool MovingSamus { get; internal set; }
    /// <summary>One-shot reaction latch, initially clear and set by a one-shot shutter's first reaction so later reactions cannot start it again.</summary>
    public bool ShotActivated { get; internal set; }
    /// <summary>
    /// The shutter's whole-pixel row saved before its current move to derive the rider's wrapped vertical displacement
    /// (#2178): a vertical shutter only moves vertically.
    /// </summary>
    public int PreviousY { get; internal set; }
    /// <summary>Upper whole-pixel travel row derived from the spawn row and travel distance; upward motion stops upon reaching or passing it.</summary>
    public int UpperEndpointY { get; internal set; }
    /// <summary>Lower whole-pixel travel row derived from the spawn row and travel distance; downward motion stops upon reaching or passing it.</summary>
    public int LowerEndpointY { get; internal set; }
    /// <summary>The end the next reaction heads for, initially the primary heading; it turns after each reaction.</summary>
    public ShutterHeading ReactionDirection { get; internal set; }
}
