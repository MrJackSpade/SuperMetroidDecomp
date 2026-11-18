using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Shutters;

/// <summary>
/// The state of a vertical shutter: its behaviour, endpoints, timers, reaction and rider carry, each a value of its own.
/// It reads its speed, first heading, trigger, travel, proximity, wait and rests from its spawn settings where it uses
/// them (#2450).
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
    /// wraps and waits the countdown's whole range.
    /// </summary>
    public WrappingCountdown ActionTimer { get; internal set; }

    /// <summary>
    /// The AI updates left in an endpoint rest, the one the shutter moves on included: the rest's updates and the update
    /// it runs out on (<see cref="EnemyWaitDefinitions.RunOutUpdate"/>, #2281).
    /// </summary>
    public int RestTimer { get; internal set; }
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
    /// <summary>The way the next reaction moves the shutter, initially its first heading; it turns after each reaction.</summary>
    public VerticalDirection ReactionDirection { get; internal set; }
}
