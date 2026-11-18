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
    /// The timer-triggered shutter's first wait, a wrapping AI-update countdown that activates at zero: a wait of zero
    /// wraps and waits the countdown's whole range. The wrap is load-bearing (#2454): the Pink Brinstar hopper room's two
    /// timer-triggered shootable shutters are placed with no wait, no speed and no travel, so each waits the 65536 updates
    /// of the full wrap and then turns at its ends without moving, queuing the shutter sound on every turn while it is on
    /// screen.
    /// </summary>
    public WrappingCountdown ActionTimer { get; internal set; }

    /// <summary>
    /// The AI updates left in an endpoint rest, the one the shutter moves on included: the rest's updates and the update
    /// it runs out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>, #2281).
    /// </summary>
    public int RestTimer { get; internal set; }
    /// <summary>The shutter's speed, 1/65536 pixels per update, one of the common linear speeds (#2278).</summary>
    public SignedSixteenSixteen Speed { get; internal set; }

    /// <summary>The shutter's velocity moving toward <paramref name="direction"/>: its speed, reversed upward, fraction included.</summary>
    public SignedSixteenSixteen VelocityToward(VerticalDirection direction) => direction == VerticalDirection.Up ? -Speed : Speed;

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
    /// The shutter's whole-pixel row saved before its current move, from which the rider moves by the shutter's own move
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
