using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Artwork;
namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The state of one of Shaktool's seven linked segments.</summary>
public sealed class ShaktoolSegmentState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShaktoolSegment> Playback { get; set; }

    /// <summary>
    /// The picture the centre segment's head shows while its chain orients it (#2284): the one its orientation chose on
    /// its last update, shown for good in place of <see cref="Playback"/>. Null while the segment plays its animation: its
    /// first pose, its head bob, and every other segment's.
    /// </summary>
    internal EnemyFrameId? AimedPicture { get; set; }
    /// <summary>The chain's target heading, also advanced during straightening and used by attack movement cues.</summary>
    public RotationAngle TargetAngle { get; internal set; }
    /// <summary>The angle around the immediately preceding segment; its whole step selects the orbit displacement.</summary>
    public RotationAngle OrbitAngle { get; internal set; }
    /// <summary>
    /// The turn the segment orbits by each update, up to four whole steps: an authored curling rate, a sixty-fourth of
    /// the whole steps left to a target, or the straightening step. The motion flags give its direction.
    /// </summary>
    public RotationAngle AngularVelocity { get; internal set; }
    /// <summary>
    /// The compass direction the centre segment's head aims: selected from the midpoint of its neighbours' angles, it
    /// chooses its head's picture and the direction its attack cues move the segments along.
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
    /// The segment motion run by Shaktool's main AI, its definition's from initialization; collision-response animations
    /// temporarily replace it with the idle attack motion. A segment's state emptied by a kill does not move itself.
    /// </summary>
    public ShaktoolSegmentMotion Motion { get; internal set; } = ShaktoolSegmentMotion.Idle;
    /// <summary>The chain's reversal, straightening and orbit-direction flags, written to every segment together.</summary>
    internal ShaktoolMotionFlags MotionFlags { get; set; }

    /// <summary>Empties the orientation, curl progress and reversal together, as the game clears them together.</summary>
    internal void ResetOrientation()
    {
        Orientation = CompassDirection.Up;
        OrientationReversed = false;
        CurlProgress = RotationAngle.Zero;
    }
}
