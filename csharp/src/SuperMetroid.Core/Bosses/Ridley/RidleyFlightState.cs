using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Bosses.Ridley.Artwork;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's body in flight as both encounters fly it (#2228): which way he faces, how he moves within his arena, his wing
/// beat and swoop, and the body animation he plays with its claws, roar and aimed fireball.
/// </summary>
public sealed class RidleyFlightState
{
    /// <summary>Makes his body facing <paramref name="facing"/>, kept within <paramref name="minimum"/> and <paramref name="maximum"/>.</summary>
    /// <param name="facing">The way he faces as he is placed.</param>
    /// <param name="minimum">The top-left corner of the box his movement keeps him in, where it lies from the room's origin.</param>
    /// <param name="maximum">The bottom-right corner of the box his movement keeps him in, where it lies from the room's origin.</param>
    /// <param name="firstWingPose">The pose of his wing beat the wings show first.</param>
    internal RidleyFlightState(RidleyFacing facing, Offset16<RoomPixels> minimum, Offset16<RoomPixels> maximum, RidleyWingPose firstWingPose)
    {
        FacingDirection = facing;
        MovementMinimum = minimum;
        MovementMaximum = maximum;
        Wings = new RidleyWingBeat(firstWingPose);
        BodyPlayback = new(RidleyAnimations.StartingWith(RidleyAnimations.Initial.For(facing)), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Gets which way Ridley faces: left, toward the screen in his intermediate turning pose, or right.</summary>
    public RidleyFacing FacingDirection { get; internal set; }
    /// <summary>
    /// Gets the top-left corner of the box his movement keeps him in, where it lies from the room's origin: Ceres Ridley's
    /// box reaches above his chamber, at negative heights.
    /// </summary>
    public Offset16<RoomPixels> MovementMinimum { get; internal set; }
    /// <summary>Gets the bottom-right corner of the box his movement keeps him in, where it lies from the room's origin.</summary>
    public Offset16<RoomPixels> MovementMaximum { get; internal set; }
    /// <summary>Whether Ridley's movement hit a room boundary, consumed by the swoop.</summary>
    public bool HitRoomBoundary { get; internal set; }
    /// <summary>Gets Ridley's signed 1/256-pixel velocity, each axis held unchanged.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>His wing beat: the pose his wings show and the countdown to the next, paced by his flight speed (#2329).</summary>
    public RidleyWingBeat Wings { get; }
    /// <summary>Gets the swoop's heading, which wraps around the full turn; its whole step selects the sine sample.</summary>
    public RotationAngle SwoopAngleAccumulator { get; internal set; }
    /// <summary>
    /// Gets the swoop's speed along its heading, at most five pixels an update, moved an eighth of a pixel each update
    /// toward each phase's target speed before it resolves into axis velocities.
    /// </summary>
    public SignedEightEight SwoopSpeed { get; internal set; }
    /// <summary>
    /// Ridley's playing body animation and where it stands (#641, #1600): from his setup, his standing pose for the way he
    /// faces as he is placed (#2335).
    /// </summary>
    internal AnimationPlayback<RidleyBodySegment> BodyPlayback { get; set; }
    /// <summary>Gets the claw height Ridley's body animation last selected; he starts standing.</summary>
    public RidleyClawHeight ClawHeight { get; internal set; } = RidleyClawHeight.Standing;
    /// <summary>Gets whether Ridley's animation has enabled the roar presentation.</summary>
    public bool Roaring { get; internal set; }
    /// <summary>Gets the signed 1/256-pixel velocity his body animation's aim computed from the clamped fireball aim angle.</summary>
    public EightEightVector<RoomPixels> FireballVelocity { get; internal set; }
}
