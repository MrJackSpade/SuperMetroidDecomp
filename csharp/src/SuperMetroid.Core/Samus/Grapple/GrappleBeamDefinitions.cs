using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>The Grapple Beam's definitions: its beam shade, sounds, swing physics, firing and rope drawing (#1411).</summary>
/// <remarks>
/// Grapple runtime state deliberately remains elsewhere. This type names the beam shade, sounds, physics coefficients,
/// firing definitions and rope drawing measures that define the subsystem's data contract.
/// </remarks>
public static class GrappleBeamDefinitions
{
    /// <summary>The beam shade grapple firing shows.</summary>
    public static class BeamShades
    {
        /// <summary>Firing selects the Ice beam's colours before entering the firing phase.</summary>
        public const SamusBeamCombination FiringSelection = SamusBeamCombination.Ice;
    }

    /// <summary>The exact sound requests the grapple beam makes.</summary>
    public static class Sounds
    {
        /// <summary>Stop the old beam before pose-change refiring.</summary>
        public static readonly SoundRequest RestartStop = new(SamusSound.GrappleEnd, SoundRequestPriority.Normal);
        /// <summary>Firing the beam: start the extending beam's sound, only when no other sound is waiting.</summary>
        public static readonly SoundRequest Fire = new(SamusSound.GrappleStart, SoundRequestPriority.WhenIdle);
        /// <summary>A beam that latches on: the attached beam's sound.</summary>
        public static readonly SoundRequest Attach = new(SamusSound.Grappling, SoundRequestPriority.Normal);
        /// <summary>Every grapple teardown: stop the active grapple sound, at essential priority.</summary>
        public static readonly SoundRequest Stop = new(SamusSound.GrappleEnd, SoundRequestPriority.Essential);
    }
    /// <summary>Angular-physics coefficients embedded in behavior.</summary>
    public static class Physics
    {
        /// <summary>A fired beam that connects: the rope retracts by itself eight pixels per frame after connecting clears its change of length.</summary>
        public const int InitialConnectionRetraction = -8;
        /// <summary>How far the rope grows each update while the beam is fired.</summary>
        public const int FiringExtension = 12;
        /// <summary>The rope length at which a fired beam that has met nothing is cancelled.</summary>
        public const int FiringCancelLength = 128;
        /// <summary>
        /// The longest the rope becomes: firing extends a rope shorter than <see cref="FiringCancelLength"/> by
        /// <see cref="FiringExtension"/> and then cancels it, and connected motion keeps it shorter still.
        /// </summary>
        public const int LongestRopeLength = FiringCancelLength - 1 + FiringExtension;
        /// <summary>Per-update downward angular acceleration: 24/256 of a step.</summary>
        public static readonly AngularSpeed GravityMagnitude = AngularSpeed.FromStepFraction(3, 32);
        /// <summary>Per-update angular acceleration contributed by directional input: 12/256 of a step.</summary>
        public static readonly AngularSpeed DirectionInputMagnitude = AngularSpeed.FromStepFraction(3, 64);
        /// <summary>Correction applied when velocity opposes the current swing direction: 5/256 of a step.</summary>
        public static readonly AngularSpeed VelocityCorrectionMagnitude = AngularSpeed.FromStepFraction(5, 256);
        /// <summary>The fastest the swing turns either way: four and a half steps an update.</summary>
        public static readonly AngularSpeed MaximumAngularVelocity = AngularSpeed.FromStepFraction(9, 2);
        /// <summary>Angular impulse applied by Jump during a swing: three steps an update.</summary>
        public static readonly AngularSpeed JumpImpulseMagnitude = AngularSpeed.FromSteps(3);
        /// <summary>How much of the Jump impulse each sweep that meets nothing takes away, toward zero: 6/256 of a step.</summary>
        public static readonly AngularSpeed JumpImpulseDecay = AngularSpeed.FromStepFraction(3, 128);
        /// <summary>The angular velocity a motionless pendulum hanging straight down is kicked to by a held direction: a step an update.</summary>
        public static readonly AngularSpeed MotionlessPumpKick = AngularSpeed.FromSteps(1);
        /// <summary>A pendulum hanging straight down slower than this, a step an update, stops.</summary>
        public static readonly AngularSpeed RestingAngularVelocity = AngularSpeed.FromSteps(1);
        /// <summary>
        /// The parts of its angular velocity, out of <see cref="SubmergedSwingParts"/>, the swing turns through each update
        /// under water; in air it turns through all of it.
        /// </summary>
        public const int SubmergedSwingShare = 5;
        /// <summary>The parts a submerged swing's angular velocity divides into: eighths.</summary>
        public const int SubmergedSwingParts = 8;
        /// <summary>The updates after a bounce off terrain in which Jump adds its impulse.</summary>
        public const int CollisionBounceFrames = 16;
        /// <summary>
        /// The share of gravity, of a held direction's push and of Jump's impulse a submerged swing takes: a half (#2280).
        /// </summary>
        public const int SubmergedShare = 2;
        /// <summary>The share of gravity and of the velocity correction the swing takes above the anchor's level: a quarter.</summary>
        public const int UpperQuadrantShare = 4;
        /// <summary>
        /// The angle steps a motionless release's horizontal speed angle leads the rope by: a quarter turn.
        /// </summary>
        public const int ReleaseLeadSteps = RotationAngle.StepsPerTurn / 4;
        /// <summary>The angle steps the release's lead loses per whole angle step of swing speed.</summary>
        public const int ReleaseLeadStepsPerSwingStep = 3;
    }

    /// <summary>The swinging rope's length and the points along Samus's body its sweeps test.</summary>
    public static class Swing
    {
        /// <summary>
        /// How far into its block a block anchor sits on an axis whose rope component is not negative: the rope leaves the
        /// block's middle (#2280).
        /// </summary>
        public const int AnchorBiasTowardPositive = 8;
        /// <summary>How far into its block a block anchor sits on an axis whose rope component is negative: one pixel short of its middle.</summary>
        public const int AnchorBiasTowardNegative = AnchorBiasTowardPositive - 1;
        /// <summary>How far an Up or Down press shortens or lengthens the rope each update.</summary>
        public const int RopeLengthStep = 2;
        /// <summary>The shortest a swing rope is drawn in to; at this length a strike near the hand stops the swing.</summary>
        public const int ShortestRope = 8;
        /// <summary>The longest Down lets out a swing rope.</summary>
        public const int LongestLetOutRope = 63;
        /// <summary>Down cuts a rope this long or longer to this length instead of letting it out.</summary>
        public const int RopeLengthCap = 64;
        /// <summary>The distance beyond the rope's end of the body point a shortening rope tests: the hand end of Samus.</summary>
        public const int ShorteningProbeOffset = 8;
        /// <summary>The distance beyond the rope's end of the body point a lengthening rope tests: the feet end of Samus.</summary>
        public const int LengtheningProbeOffset = 56;
        /// <summary>The distance between the body points a swing sweep tests, the first this far beyond the hand.</summary>
        public const int BodyPointSpacing = 8;
        /// <summary>The body points a swing sweep tests along Samus, from the hand toward the feet.</summary>
        public const int BodyPointCount = 6;
        /// <summary>The body points nearest the hand whose strike, on the shortest rope, stops the swing instead of bouncing it.</summary>
        public const int StoppingPointsNearHand = 2;
    }

    /// <summary>What an enemy that hurts Samus through the grapple beam does to her.</summary>
    public static class Contact
    {
        /// <summary>The invulnerability, in updates, a grapple-beam hurt gives her.</summary>
        public const int HurtInvincibilityFrames = 96;
        /// <summary>The knockback, in updates, a grapple-beam hurt gives her.</summary>
        public const int HurtKnockbackFrames = 5;
    }

    /// <summary>Holding onto a wall with the beam, and the moments after letting go.</summary>
    public static class WallGrab
    {
        /// <summary>The updates after a wall grab ends in which Jump still wall-jumps.</summary>
        public const int WallJumpWindowFrames = 30;
        /// <summary>How far the grapple wall-jump probe looks for a wall toward the side Samus faces, in pixels.</summary>
        public const int WallJumpProbePixels = 16;
    }

    /// <summary>How a connection takes up a long rope and how a released beam drops Samus.</summary>
    public static class Connection
    {
        /// <summary>A connected rope this long or longer is shortened by <see cref="LongRopeShortening"/>.</summary>
        public const int LongRope = 64;
        /// <summary>How much a connection shortens a long rope.</summary>
        public const int LongRopeShortening = 24;
        /// <summary>The smallest vertical radius of a body that drops standing; a smaller body drops crouching.</summary>
        public const int StandingDropRadius = 17;
        /// <summary>
        /// The farthest, in pixels on each axis, the previous position may lag behind Samus after a connection snaps her
        /// to an exact rope angle.
        /// </summary>
        public const int MostSnapPreviousLag = 12;
    }

    /// <summary>How the beam's block checks read the room.</summary>
    public static class Blocks
    {
        /// <summary>
        /// The extension links a grapple block check follows from the block it starts on; a longer chain is broken room
        /// data, which the check reports.
        /// </summary>
        public const int LongestExtensionChain = 16;
    }

    /// <summary>Firing the beam: the pose-change auto-fire wait and the art origin of the poses Draygon holds Samus in.</summary>
    public static class Firing
    {
        /// <summary>Firing initializes the grapple's pose-change auto-fire timer to ten; the pose-change check decrements before checking the pose.</summary>
        public const int PoseChangeAutoFireFrames = 10;
        /// <summary>The art origin firing uses, six pixels, for the moving poses Draygon holds Samus in.</summary>
        public const int DraygonMovingArtOrigin = 6;
    }

    /// <summary>How the rope is drawn: a chain of rope pictures from the muzzle toward the anchor, animated as one.</summary>
    public static class Rendering
    {
        /// <summary>The distance in pixels between the centres of consecutive rope pictures along the rope.</summary>
        public const int SegmentSpacing = 8;
        /// <summary>Rope draws each of the rope's pictures lasts before the whole rope steps on to the next.</summary>
        public const int SegmentAnimationDelay = 5;
        /// <summary>
        /// Rope draws a fresh beam's first pictures last: one more than the rest, since the beam's first rope draw only
        /// takes up the pattern it fired with.
        /// </summary>
        public const int FirstSegmentAnimationDelay = SegmentAnimationDelay + 1;
    }

}
