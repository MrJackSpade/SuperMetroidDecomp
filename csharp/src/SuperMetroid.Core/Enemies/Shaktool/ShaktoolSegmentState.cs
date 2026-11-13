using SuperMetroid.Core.Programs;
namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The state of one of Shaktool's seven linked segments.</summary>
public sealed class ShaktoolSegmentState
{
    /// <summary>The animation the enemy plays and where it stands in it (#1600).</summary>
    internal AnimationPlayback<ShaktoolProgram> Playback { get; set; }
    /// <summary>The chain's target heading, also advanced during straightening and used by attack movement cues.</summary>
    public RotationAngle TargetAngle { get; internal set; }
    /// <summary>The angle around the immediately preceding segment; its table index selects the orbit displacement.</summary>
    public RotationAngle OrbitAngle { get; internal set; }
    /// <summary>
    /// The turn the segment orbits by each update, up to four whole steps: an authored curling rate, a sixty-fourth of
    /// the whole steps left to a target, or the straightening step. The motion flags give its direction.
    /// </summary>
    public RotationAngle AngularVelocity { get; internal set; }
    /// <summary>
    /// The centre segment's heading bucket: one of eight eighth-turns in table units (0, 32, ... 224), selected
    /// from the midpoint of its neighbours' angles and choosing its head animation.
    /// </summary>
    public int OrientationBucket { get; internal set; }
    /// <summary>Whether the chain runs reversed, so the centre segment's midpoint flips a half turn; an endpoint swap toggles it.</summary>
    public bool OrientationReversed { get; internal set; }
    /// <summary>
    /// The final saw's curl progress: the turns it has orbited by since the chain last reached its target, short of a
    /// full turn. Reaching 240 whole steps starts the chain straightening, which stops the accumulation one update's turn
    /// past it at most.
    /// </summary>
    public RotationAngle CurlProgress { get; internal set; }
    /// <summary>
    /// The segment motion run by Shaktool's main AI; collision-response animations temporarily replace it with the idle
    /// attack motion.
    /// </summary>
    public ShaktoolFrameBehavior? FrameBehavior { get; internal set; }
    /// <summary>The chain's reversal, straightening and orbit-direction flags, written to every segment together.</summary>
    internal ShaktoolMotionFlags MotionFlags { get; set; }

    /// <summary>Empties the orientation, curl progress and reversal together, as the game clears them together.</summary>
    internal void ResetOrientation()
    {
        OrientationBucket = 0;
        OrientationReversed = false;
        CurlProgress = RotationAngle.Zero;
    }
}
