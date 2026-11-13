using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>The state of the game's complete, though retail-unused, horizontal shutter.</summary>
public sealed class HorizontalShutterEnemyState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShutterSegment> Playback { get; set; }

    /// <summary>
    /// The dispatcher behaviour of the retail-unused horizontal shutter, retained across wait, movement, and
    /// endpoint-rest updates.
    /// </summary>
    public HorizontalShutterAction? Action { get; internal set; }

    /// <summary>
    /// Wrapping AI-update countdown; initial wait tests for zero after decrement, whereas endpoint rests test for
    /// signed underflow.
    /// </summary>
    public WrappingCountdown ActionTimer { get; internal set; }

    /// <summary>
    /// Rightward signed 16.16 velocity in pixels per update, also supplied to an overlapping Samus's extra-X
    /// displacement.
    /// </summary>
    public SignedSixteenSixteen RightVelocity { get; internal set; }

    /// <summary>
    /// Leftward signed 16.16 velocity in pixels per update, also supplied to an overlapping Samus's extra-X
    /// displacement.
    /// </summary>
    public SignedSixteenSixteen LeftVelocity { get; internal set; }

    /// <summary>The end the shutter first heads for, as its population entry names it.</summary>
    public ShutterHeading PrimaryDirection { get; internal set; }
    /// <summary>Population trigger mode: timer, horizontal proximity, immediate, one-shot reaction, or repeatable reaction activation.</summary>
    public ShutterTriggerMode TriggerMode { get; internal set; }
    /// <summary>Whole-pixel X separation of the two endpoints.</summary>
    public int TravelDistance { get; internal set; }
    /// <summary>The strict horizontal pixel distance Samus must come within to start a proximity-triggered shutter.</summary>
    public int ProximityDistance { get; internal set; }
    /// <summary>The AI updates a timer-triggered shutter waits before it moves.</summary>
    public int WaitTime { get; internal set; }
    /// <summary>Left endpoint's rest length in AI updates, a multiple of sixteen; 4080 sets the inert action.</summary>
    public int MovedLeftRestTime { get; internal set; }
    /// <summary>Right endpoint's rest length in AI updates, a multiple of sixteen; 4080 sets the inert action.</summary>
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
    /// The end the shutter last reacted toward: it starts opposite the primary heading and turns before each
    /// reaction, so the first reaction heads the primary way.
    /// </summary>
    public ShutterHeading ReactionDirection { get; internal set; }
}
