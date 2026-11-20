using SuperMetroid.Core.Animation;
namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>
/// The state of one of Shaktool's seven linked segments: its own orbit, turn speed, motion and animation. The motion and
/// target the whole chain shares are its chain's (<see cref="ShaktoolChain"/>, #2452).
/// </summary>
public sealed class ShaktoolSegmentState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShaktoolSegment> Playback { get; set; }
    /// <summary>The angle around the immediately preceding segment; its whole step selects the orbit displacement.</summary>
    public RotationAngle OrbitAngle { get; internal set; }
    /// <summary>
    /// The turn the segment orbits by each update, up to four whole steps: an authored curling rate, or a sixty-fourth of
    /// the whole steps left to its chain's target. Its chain's motion gives its direction.
    /// </summary>
    public RotationAngle AngularVelocity { get; internal set; }
    /// <summary>
    /// The compass direction the centre segment's head aims: selected from the midpoint of its neighbours' angles, it
    /// chooses its head's picture.
    /// </summary>
    public CompassDirection Orientation { get; internal set; }
    /// <summary>Whether the chain runs reversed, so the centre segment's midpoint flips a half turn; an endpoint swap toggles it.</summary>
    public bool OrientationReversed { get; internal set; }
    /// <summary>
    /// The final saw's curl progress: the turns it has orbited by since the chain last reached its target, short of a
    /// full turn. Reaching 240 whole steps starts the chain straightening, which stops the accumulation one update's turn
    /// past it at most.
    /// </summary>
    public RotationAngle CurlProgress { get; internal set; }
    /// <summary>
    /// How the segment moves itself each update, its definition's from initialization; a collision response stops every
    /// segment (<see cref="ShaktoolSegmentMotion.Idle"/>) until its reset restores each segment's own motion. A segment's
    /// state emptied by a kill does not move itself.
    /// </summary>
    public ShaktoolSegmentMotion Motion { get; internal set; } = ShaktoolSegmentMotion.Idle;

    /// <summary>Resets the orientation, curl progress and reversal, as the chain does when it starts over.</summary>
    internal void ResetOrientation()
    {
        Orientation = CompassDirection.Up;
        OrientationReversed = false;
        CurlProgress = RotationAngle.Zero;
    }
}
