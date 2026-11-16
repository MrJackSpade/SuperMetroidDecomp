using SuperMetroid.Core.Animation;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// Ridley's body in flight as both encounters fly it (#2228): which way he faces, how he moves within his arena, his wing
/// beat and swoop, and the body animation he plays with its claws, roar and aimed fireball.
/// </summary>
public sealed class RidleyFlightState
{
    /// <summary>Makes his body facing <paramref name="facing"/>, kept within <paramref name="minimum"/> and <paramref name="maximum"/>.</summary>
    /// <param name="facing">The way he faces as he is placed.</param>
    /// <param name="minimum">The movement clamp's minimum whole-pixel corner.</param>
    /// <param name="maximum">The movement clamp's maximum whole-pixel corner.</param>
    /// <param name="wingBeatStep">The step of his wing beat the wings show first.</param>
    internal RidleyFlightState(RidleyFacing facing, Position16<RoomPixels> minimum, Position16<RoomPixels> maximum, int wingBeatStep)
    {
        FacingDirection = facing;
        MovementMinimum = minimum;
        MovementMaximum = maximum;
        WingBeatStep = wingBeatStep;
    }

    /// <summary>Gets which way Ridley faces: left, toward the screen in his intermediate turning pose, or right.</summary>
    public RidleyFacing FacingDirection { get; internal set; }
    /// <summary>Gets the movement clamp's minimum whole-pixel corner, retaining negative limits wrapped into the coordinate range.</summary>
    public Position16<RoomPixels> MovementMinimum { get; internal set; }
    /// <summary>Gets the movement clamp's maximum whole-pixel corner.</summary>
    public Position16<RoomPixels> MovementMaximum { get; internal set; }
    /// <summary>Whether Ridley's movement hit a room boundary, consumed by the swoop.</summary>
    public bool HitRoomBoundary { get; internal set; }
    /// <summary>Gets Ridley's signed 1/256-pixel velocity, each axis held unchanged.</summary>
    public EightEightVector<RoomPixels> Velocity { get; internal set; }
    /// <summary>Gets the step of his wing beat the wings show, advancing when the wing timer passes zero and wrapping at the beat's end.</summary>
    public int WingBeatStep { get; internal set; }
    /// <summary>Gets the wing animation's countdown, reloaded to 32 once it runs below zero.</summary>
    public int WingAnimationTimer { get; internal set; }
    /// <summary>Gets the decrement (zero through 48) selected from velocity magnitude for the wing countdown; nonnegative vertical motion halves it.</summary>
    public int WingAnimationTimerDelta { get; internal set; }
    /// <summary>Gets the swoop's heading, which wraps around the full turn; its whole step selects the sine sample.</summary>
    public RotationAngle SwoopAngleAccumulator { get; internal set; }
    /// <summary>
    /// Gets the swoop's speed along its heading, at most five pixels an update, moved an eighth of a pixel each update
    /// toward each phase's target speed before it resolves into axis velocities.
    /// </summary>
    public SignedEightEight SwoopSpeed { get; internal set; }
    /// <summary>Ridley's playing body animation and where it stands (#641, #1600); before the first start, the initial animation.</summary>
    internal AnimationPlayback<RidleyBodySegment> BodyPlayback { get; set; } =
        new(RidleyAnimations.StartingWith(RidleyBodySegment.InitialFacingLeft), AnimationPlace.StartOfAnimation);
    /// <summary>Gets the claw height Ridley's body animation last selected; he starts standing.</summary>
    public RidleyClawHeight ClawHeight { get; internal set; } = RidleyClawHeight.Standing;
    /// <summary>Gets whether Ridley's animation has enabled the roar presentation.</summary>
    public bool Roaring { get; internal set; }
    /// <summary>Gets the signed 1/256-pixel velocity his body animation's aim computed from the clamped fireball aim angle.</summary>
    public EightEightVector<RoomPixels> FireballVelocity { get; internal set; }
}
