namespace SuperMetroid.Core.Samus.Poses;

/// <summary>
/// The pose families gameplay tests, each listed by name (#1517); <see cref="SamusPoseMetadata"/> answers whether a pose
/// belongs to one.
/// </summary>
internal static partial class SamusPoseFamilyDefinitions
{
    /// <summary>
    /// The X-ray scope's two crouching poses, which have the <see cref="Movement.SamusMovementType.Crouching"/> movement
    /// type outside the crouching family.
    /// </summary>
    internal static readonly SamusPoseId[] XrayCrouchingPoses =
    [
        SamusPoseId.XrayingCrouchingRightPose,
        SamusPoseId.XrayingCrouchingLeftPose,
    ];

    /// <summary>The two morphing transitions, facing right and left, which start Samus's morph.</summary>
    internal static readonly SamusPoseId[] MorphingTransitionPoses =
    [
        SamusPoseId.MorphingTransitionRightPose,
        SamusPoseId.MorphingTransitionLeftPose,
    ];

    /// <summary>The six shinespark flights: horizontal, vertical and diagonal, facing right and left.</summary>
    internal static readonly SamusPoseId[] ShinesparkPoses =
    [
        SamusPoseId.ShinesparkHorizontalRightPose,
        SamusPoseId.ShinesparkHorizontalLeftPose,
        SamusPoseId.ShinesparkVerticalRightPose,
        SamusPoseId.ShinesparkVerticalLeftPose,
        SamusPoseId.ShinesparkDiagonalRightPose,
        SamusPoseId.ShinesparkDiagonalLeftPose,
    ];

    /// <summary>The generic spin jump facing right and the spins it becomes when Screw Attack or Space Jump is equipped.</summary>
    internal static readonly SamusEquippedSpinPoses SpinPosesFacingRight =
        new(Generic: SamusPoseId.SpinJumpRightPose, ScrewAttack: SamusPoseId.ScrewAttackRightPose, SpaceJump: SamusPoseId.SpaceJumpRightPose);

    /// <summary>The generic spin jump facing left and the spins it becomes when Screw Attack or Space Jump is equipped.</summary>
    internal static readonly SamusEquippedSpinPoses SpinPosesFacingLeft =
        new(Generic: SamusPoseId.SpinJumpLeftPose, ScrewAttack: SamusPoseId.ScrewAttackLeftPose, SpaceJump: SamusPoseId.SpaceJumpLeftPose);

    /// <summary>
    /// The poses an elevator arms under: the power-suit front view and the standing poses facing right or left, normal or
    /// aimed up, diagonally up or diagonally down. The Varia and Gravity front view does not arm it.
    /// </summary>
    internal static readonly SamusPoseId[] ElevatorArmingPoses =
    [
        SamusPoseId.ForwardFacingPowerSuitPose,
        SamusPoseId.FacingRightNormalPose,
        SamusPoseId.FacingLeftNormalPose,
        SamusPoseId.StandingAimUpRightPose,
        SamusPoseId.StandingAimUpLeftPose,
        SamusPoseId.StandingAimDiagonalUpRightPose,
        SamusPoseId.StandingAimDiagonalUpLeftPose,
        SamusPoseId.StandingAimDiagonalDownRightPose,
        SamusPoseId.StandingAimDiagonalDownLeftPose,
    ];

    /// <summary>
    /// Whether each pose may change posture while firing, for the poses with their own answer; every other pose is
    /// <see cref="SamusPostureTransitionAdmission.AnsweredBeyondTransitions"/>.
    /// </summary>
    internal static readonly Dictionary<SamusPoseId, SamusPostureTransitionAdmission> PostureTransitionAdmissions = new()
    {
        [SamusPoseId.CrouchingTransitionRightPose] = SamusPostureTransitionAdmission.CrouchOrStandAdmits,
        [SamusPoseId.CrouchingTransitionLeftPose] = SamusPostureTransitionAdmission.CrouchOrStandAdmits,
        [SamusPoseId.MorphingTransitionRightPose] = SamusPostureTransitionAdmission.MorphKeepsCharge,
        [SamusPoseId.MorphingTransitionLeftPose] = SamusPostureTransitionAdmission.MorphKeepsCharge,
        [SamusPoseId.StandingTransitionRightPose] = SamusPostureTransitionAdmission.CrouchOrStandAdmits,
        [SamusPoseId.StandingTransitionLeftPose] = SamusPostureTransitionAdmission.CrouchOrStandAdmits,
        [SamusPoseId.UnmorphingTransitionRightPose] = SamusPostureTransitionAdmission.MorphKeepsCharge,
        [SamusPoseId.UnmorphingTransitionLeftPose] = SamusPostureTransitionAdmission.MorphKeepsCharge,
        [SamusPoseId.DraygonHeldStandingUpLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonHeldBallLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimUpRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimUpLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimDiagonalUpRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimDiagonalUpLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimDiagonalDownRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.LandingAimDiagonalDownLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.FiringLandingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.FiringLandingLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DrainedCrouchingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DrainedCrouchingLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DrainedStandingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DrainedStandingLeftPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedNeutralRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedAimUpRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedFiringRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedAimDownRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.DraygonGrabbedMovingRightPose] = SamusPostureTransitionAdmission.NeverAdmits,
        [SamusPoseId.CrouchingTransitionAimUpRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimUpLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimUpRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimUpLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimDiagonalUpRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimDiagonalUpLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimDiagonalDownRightPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
        [SamusPoseId.StandingTransitionAimDiagonalDownLeftPose] = SamusPostureTransitionAdmission.AlwaysAdmits,
    };

    /// <summary>Each damage boost, with the family of jumps it turns into: the left-named boost's left jumps, the right-named's right.</summary>
    internal static readonly IReadOnlyDictionary<SamusPoseId, SamusPoseFamily> DamageBoostJumps = new Dictionary<SamusPoseId, SamusPoseFamily>
    {
        [SamusPoseId.DamageBoostLeftPose] = SamusPoseFamily.DamageBoostJumpLeft,
        [SamusPoseId.DamageBoostRightPose] = SamusPoseFamily.DamageBoostJumpRight,
    };

    /// <summary>The poses each family holds (<see cref="SamusPoseFamily"/>).</summary>
    internal static readonly IReadOnlyDictionary<SamusPoseFamily, SamusPoseId[]> Members = new Dictionary<SamusPoseFamily, SamusPoseId[]>
    {
        [SamusPoseFamily.RightFacingStanding] =
        [
            SamusPoseId.FacingRightNormalPose,
            SamusPoseId.StandingAimUpRightPose,
            SamusPoseId.StandingAimDiagonalUpRightPose,
            SamusPoseId.StandingAimDiagonalDownRightPose,
        ],
        [SamusPoseFamily.ForwardFacing] =
        [
            SamusPoseId.ForwardFacingPowerSuitPose,
            SamusPoseId.ForwardFacingSuitedPose,
        ],
        [SamusPoseFamily.GunExtended] =
        [
            SamusPoseId.MovingRightGunExtendedPose,
            SamusPoseId.MovingLeftGunExtendedPose,
            SamusPoseId.NormalJumpGunExtendedRightPose,
            SamusPoseId.NormalJumpGunExtendedLeftPose,
            SamusPoseId.FallingGunExtendedRightPose,
            SamusPoseId.FallingGunExtendedLeftPose,
        ],
        [SamusPoseFamily.MoonwalkingFacingRight] =
        [
            SamusPoseId.MoonwalkFacingRightPose,
            SamusPoseId.MoonwalkAimUpRightPose,
            SamusPoseId.MoonwalkAimDownRightPose,
        ],
        [SamusPoseFamily.RightFacingDraygonGrabbed] =
        [
            SamusPoseId.DraygonGrabbedNeutralRightPose,
            SamusPoseId.DraygonGrabbedAimUpRightPose,
            SamusPoseId.DraygonGrabbedFiringRightPose,
            SamusPoseId.DraygonGrabbedAimDownRightPose,
            SamusPoseId.DraygonGrabbedMovingRightPose,
        ],
        [SamusPoseFamily.MoonwalkTurnToSpinJump] =
        [
            SamusPoseId.MoonwalkTurnToSpinJumpLeftPose,
            SamusPoseId.MoonwalkTurnToSpinJumpRightPose,
            SamusPoseId.MoonwalkTurnToSpinJumpAimUpLeftPose,
            SamusPoseId.MoonwalkTurnToSpinJumpAimUpRightPose,
            SamusPoseId.MoonwalkTurnToSpinJumpAimDownLeftPose,
            SamusPoseId.MoonwalkTurnToSpinJumpAimDownRightPose,
        ],
        [SamusPoseFamily.MoonwalkTurnToSpinJumpRight] =
        [
            SamusPoseId.MoonwalkTurnToSpinJumpRightPose,
            SamusPoseId.MoonwalkTurnToSpinJumpAimUpRightPose,
            SamusPoseId.MoonwalkTurnToSpinJumpAimDownRightPose,
        ],
        [SamusPoseFamily.GroundedAim] =
        [
            SamusPoseId.StandingAimUpRightPose,
            SamusPoseId.StandingAimUpLeftPose,
            SamusPoseId.StandingAimDiagonalUpRightPose,
            SamusPoseId.StandingAimDiagonalUpLeftPose,
            SamusPoseId.StandingAimDiagonalDownRightPose,
            SamusPoseId.StandingAimDiagonalDownLeftPose,
            SamusPoseId.RunningAimDiagonalUpRightPose,
            SamusPoseId.RunningAimDiagonalUpLeftPose,
            SamusPoseId.RunningAimDiagonalDownRightPose,
            SamusPoseId.RunningAimDiagonalDownLeftPose,
            SamusPoseId.CrouchingAimUpRightPose,
            SamusPoseId.CrouchingAimUpLeftPose,
            SamusPoseId.CrouchingAimDiagonalUpRightPose,
            SamusPoseId.CrouchingAimDiagonalUpLeftPose,
            SamusPoseId.CrouchingAimDiagonalDownRightPose,
            SamusPoseId.CrouchingAimDiagonalDownLeftPose,
            SamusPoseId.LandingAimUpRightPose,
            SamusPoseId.LandingAimUpLeftPose,
            SamusPoseId.LandingAimDiagonalUpRightPose,
            SamusPoseId.LandingAimDiagonalUpLeftPose,
            SamusPoseId.LandingAimDiagonalDownRightPose,
            SamusPoseId.LandingAimDiagonalDownLeftPose,
            SamusPoseId.RanIntoWallAimUpRightPose,
            SamusPoseId.RanIntoWallAimUpLeftPose,
            SamusPoseId.RanIntoWallAimDownRightPose,
            SamusPoseId.RanIntoWallAimDownLeftPose,
        ],
        [SamusPoseFamily.RightFacingAimedLanding] =
        [
            SamusPoseId.LandingAimUpRightPose,
            SamusPoseId.LandingAimDiagonalUpRightPose,
            SamusPoseId.LandingAimDiagonalDownRightPose,
        ],
        [SamusPoseFamily.RightFacingLanding] =
        [
            SamusPoseId.NormalLandingRightPose,
            SamusPoseId.SpinLandingRightPose,
            SamusPoseId.FiringLandingRightPose,
        ],
        [SamusPoseFamily.RightFacingCrouching] =
        [
            SamusPoseId.CrouchingRightPose,
            SamusPoseId.CrouchingAimUpRightPose,
            SamusPoseId.CrouchingAimDiagonalUpRightPose,
            SamusPoseId.CrouchingAimDiagonalDownRightPose,
        ],
        [SamusPoseFamily.RightToLeftGroundTurn] =
        [
            SamusPoseId.TurningRightToLeftPose,
            SamusPoseId.TurningRightToLeftAimUpPose,
            SamusPoseId.TurningRightToLeftAimDiagonalUpPose,
            SamusPoseId.TurningRightToLeftAimDiagonalDownPose,
            SamusPoseId.TurningRightToLeftCrouchingPose,
            SamusPoseId.TurningRightToLeftCrouchingAimUpPose,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose,
        ],
        [SamusPoseFamily.AimedCrouchingTurn] =
        [
            SamusPoseId.TurningRightToLeftCrouchingAimUpPose,
            SamusPoseId.TurningLeftToRightCrouchingAimUpPose,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose,
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose,
            SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose,
            SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose,
        ],
        [SamusPoseFamily.JumpingTurn] =
        [
            SamusPoseId.TurningRightToLeftJumpPose,
            SamusPoseId.TurningLeftToRightJumpPose,
            SamusPoseId.TurningRightToLeftJumpAimUpPose,
            SamusPoseId.TurningLeftToRightJumpAimUpPose,
            SamusPoseId.TurningRightToLeftJumpAimDownPose,
            SamusPoseId.TurningLeftToRightJumpAimDownPose,
            SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose,
            SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose,
        ],
        [SamusPoseFamily.FallingTurn] =
        [
            SamusPoseId.TurningRightToLeftFallingPose,
            SamusPoseId.TurningLeftToRightFallingPose,
            SamusPoseId.TurningRightToLeftFallingAimUpPose,
            SamusPoseId.TurningLeftToRightFallingAimUpPose,
            SamusPoseId.TurningRightToLeftFallingAimDownPose,
            SamusPoseId.TurningLeftToRightFallingAimDownPose,
            SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose,
            SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose,
        ],
        [SamusPoseFamily.RightToLeftAerialTurn] =
        [
            SamusPoseId.TurningRightToLeftJumpPose,
            SamusPoseId.TurningRightToLeftJumpAimUpPose,
            SamusPoseId.TurningRightToLeftJumpAimDownPose,
            SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose,
            SamusPoseId.TurningRightToLeftFallingPose,
            SamusPoseId.TurningRightToLeftFallingAimUpPose,
            SamusPoseId.TurningRightToLeftFallingAimDownPose,
            SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose,
        ],
        [SamusPoseFamily.Drained] =
        [
            SamusPoseId.DrainedCrouchingRightPose,
            SamusPoseId.DrainedCrouchingLeftPose,
            SamusPoseId.DrainedStandingRightPose,
            SamusPoseId.DrainedStandingLeftPose,
        ],
        [SamusPoseFamily.Knockback] =
        [
            SamusPoseId.KnockbackRightPose,
            SamusPoseId.KnockbackLeftPose,
        ],
        [SamusPoseFamily.JumpTransition] =
        [
            SamusPoseId.NeutralJumpTransitionRightPose,
            SamusPoseId.NeutralJumpTransitionLeftPose,
            SamusPoseId.NormalJumpTransitionAimUpRightPose,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose,
        ],
        [SamusPoseFamily.StoredShineShinesparkStart] =
        [
            SamusPoseId.NeutralJumpRightPose,
            SamusPoseId.NeutralJumpLeftPose,
            SamusPoseId.NormalJumpAimUpRightPose,
            SamusPoseId.NormalJumpAimUpLeftPose,
            SamusPoseId.NormalJumpAimDiagonalUpRightPose,
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose,
        ],
        [SamusPoseFamily.ScrewAttack] =
        [
            SamusPoseId.ScrewAttackRightPose,
            SamusPoseId.ScrewAttackLeftPose,
        ],
        [SamusPoseFamily.SpaceJump] =
        [
            SamusPoseId.SpaceJumpRightPose,
            SamusPoseId.SpaceJumpLeftPose,
        ],
        [SamusPoseFamily.CrouchingTransition] =
        [
            SamusPoseId.CrouchingTransitionRightPose,
            SamusPoseId.CrouchingTransitionLeftPose,
            SamusPoseId.CrouchingTransitionAimUpRightPose,
            SamusPoseId.CrouchingTransitionAimUpLeftPose,
            SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose,
            SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose,
            SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose,
            SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose,
        ],
        [SamusPoseFamily.StandingTransition] =
        [
            SamusPoseId.StandingTransitionRightPose,
            SamusPoseId.StandingTransitionLeftPose,
            SamusPoseId.StandingTransitionAimUpRightPose,
            SamusPoseId.StandingTransitionAimUpLeftPose,
            SamusPoseId.StandingTransitionAimDiagonalUpRightPose,
            SamusPoseId.StandingTransitionAimDiagonalUpLeftPose,
            SamusPoseId.StandingTransitionAimDiagonalDownRightPose,
            SamusPoseId.StandingTransitionAimDiagonalDownLeftPose,
        ],
        [SamusPoseFamily.GroundedMorphBall] =
        [
            SamusPoseId.MorphBallGroundRightPose,
            SamusPoseId.MorphBallGroundLeftPose,
            SamusPoseId.MorphBallMovingRightPose,
            SamusPoseId.MorphBallMovingLeftPose,
        ],
        [SamusPoseFamily.AirborneMorphBall] =
        [
            SamusPoseId.MorphBallFallingRightPose,
            SamusPoseId.MorphBallFallingLeftPose,
        ],
        [SamusPoseFamily.GroundedSpringBall] =
        [
            SamusPoseId.SpringBallGroundRightPose,
            SamusPoseId.SpringBallGroundLeftPose,
            SamusPoseId.SpringBallMovingRightPose,
            SamusPoseId.SpringBallMovingLeftPose,
        ],
        [SamusPoseFamily.AirborneSpringBall] =
        [
            SamusPoseId.SpringBallFallingRightPose,
            SamusPoseId.SpringBallFallingLeftPose,
            SamusPoseId.SpringBallJumpRightPose,
            SamusPoseId.SpringBallJumpLeftPose,
        ],
        [SamusPoseFamily.MorphTransition] =
        [
            SamusPoseId.MorphingTransitionRightPose,
            SamusPoseId.MorphingTransitionLeftPose,
            SamusPoseId.UnmorphingTransitionRightPose,
            SamusPoseId.UnmorphingTransitionLeftPose,
        ],
        [SamusPoseFamily.AimedAerial] =
        [
            SamusPoseId.NormalJumpAimUpRightPose,
            SamusPoseId.NormalJumpAimUpLeftPose,
            SamusPoseId.NormalJumpTransitionAimUpRightPose,
            SamusPoseId.NormalJumpTransitionAimUpLeftPose,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose,
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose,
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose,
            SamusPoseId.NormalJumpAimDiagonalUpRightPose,
            SamusPoseId.NormalJumpAimDiagonalUpLeftPose,
            SamusPoseId.NormalJumpAimDiagonalDownRightPose,
            SamusPoseId.NormalJumpAimDiagonalDownLeftPose,
            SamusPoseId.FallingAimUpRightPose,
            SamusPoseId.FallingAimUpLeftPose,
            SamusPoseId.FallingAimDiagonalUpRightPose,
            SamusPoseId.FallingAimDiagonalUpLeftPose,
            SamusPoseId.FallingAimDiagonalDownRightPose,
            SamusPoseId.FallingAimDiagonalDownLeftPose,
            SamusPoseId.NormalJumpAimDownRightPose,
            SamusPoseId.NormalJumpAimDownLeftPose,
            SamusPoseId.FallingAimDownRightPose,
            SamusPoseId.FallingAimDownLeftPose,
        ],
        [SamusPoseFamily.ForwardMovingNormalJump] =
        [
            SamusPoseId.NormalJumpForwardRightPose,
            SamusPoseId.NormalJumpForwardLeftPose,
        ],
        [SamusPoseFamily.CompactAerial] =
        [
            SamusPoseId.NormalJumpAimDownRightPose,
            SamusPoseId.NormalJumpAimDownLeftPose,
            SamusPoseId.FallingAimDownRightPose,
            SamusPoseId.FallingAimDownLeftPose,
        ],
        [SamusPoseFamily.DrainedCrouch] =
        [
            SamusPoseId.DrainedCrouchingRightPose,
            SamusPoseId.DrainedCrouchingLeftPose,
        ],
        [SamusPoseFamily.DrainedStanding] =
        [
            SamusPoseId.DrainedStandingRightPose,
            SamusPoseId.DrainedStandingLeftPose,
        ],
        [SamusPoseFamily.DeathSequence] =
        [
            SamusPoseId.DeathSequenceRightPose,
            SamusPoseId.DeathSequenceLeftPose,
        ],
        [SamusPoseFamily.ShinesparkVertical] =
        [
            SamusPoseId.ShinesparkVerticalRightPose,
            SamusPoseId.ShinesparkVerticalLeftPose,
        ],
        [SamusPoseFamily.ShinesparkWindup] =
        [
            SamusPoseId.ShinesparkWindupRightPose,
            SamusPoseId.ShinesparkWindupLeftPose,
        ],
        [SamusPoseFamily.ChestCovered] =
        [
            SamusPoseId.ForwardFacingPowerSuitPose,
        ],
        [SamusPoseFamily.FramesSelectedDirectly] =
        [
            SamusPoseId.XrayingStandingRightPose,
            SamusPoseId.XrayingStandingLeftPose,
            SamusPoseId.XrayingCrouchingRightPose,
            SamusPoseId.XrayingCrouchingLeftPose,
            SamusPoseId.GrappleSwingRightPose,
            SamusPoseId.GrappleSwingLeftPose,
            SamusPoseId.CrystalFlashRightPose,
            SamusPoseId.CrystalFlashLeftPose,
            SamusPoseId.ForwardFacingPowerSuitPose,
            SamusPoseId.ForwardFacingSuitedPose,
        ],
        [SamusPoseFamily.TorizoSitDownBall] =
        [
            SamusPoseId.MorphBallGroundRightPose,
            SamusPoseId.MorphBallMovingRightPose,
            SamusPoseId.MorphBallMovingLeftPose,
            SamusPoseId.SpringBallGroundRightPose,
            SamusPoseId.SpringBallGroundLeftPose,
            SamusPoseId.SpringBallMovingRightPose,
            SamusPoseId.SpringBallMovingLeftPose,
        ],
        [SamusPoseFamily.JumpLaunch] =
        [
            SamusPoseId.NeutralJumpTransitionRightPose,
            SamusPoseId.NeutralJumpTransitionLeftPose,
            SamusPoseId.SpinJumpRightPose,
            SamusPoseId.SpinJumpLeftPose,
        ],
        [SamusPoseFamily.NeutralJumpTransition] =
        [
            SamusPoseId.NeutralJumpTransitionRightPose,
            SamusPoseId.NeutralJumpTransitionLeftPose,
        ],
        [SamusPoseFamily.NormalRun] =
        [
            SamusPoseId.MovingRightNormalPose,
            SamusPoseId.MovingLeftNormalPose,
        ],
        [SamusPoseFamily.AimUpWallStop] =
        [
            SamusPoseId.StandingAimUpRightPose,
            SamusPoseId.StandingAimUpLeftPose,
        ],
        [SamusPoseFamily.PlainGroundTurn] =
        [
            SamusPoseId.TurningRightToLeftPose,
            SamusPoseId.TurningLeftToRightPose,
        ],
        [SamusPoseFamily.PlainCrouchingTurn] =
        [
            SamusPoseId.TurningRightToLeftCrouchingPose,
            SamusPoseId.TurningLeftToRightCrouchingPose,
        ],
        [SamusPoseFamily.HeldInPlaceStandUp] =
        [
            SamusPoseId.DraygonHeldStandingUpLeftPose,
        ],
        [SamusPoseFamily.SpringBallJump] =
        [
            SamusPoseId.SpringBallJumpRightPose,
            SamusPoseId.SpringBallJumpLeftPose,
        ],
        [SamusPoseFamily.PlainCrouching] =
        [
            SamusPoseId.CrouchingRightPose,
            SamusPoseId.CrouchingLeftPose,
        ],
        [SamusPoseFamily.GrappleWallContact] =
        [
            SamusPoseId.GrappleWallContactLeftPose,
            SamusPoseId.GrappleWallContactRightPose,
        ],
        [SamusPoseFamily.MoonwalkAimUp] =
        [
            SamusPoseId.MoonwalkAimUpLeftPose,
            SamusPoseId.MoonwalkAimUpRightPose,
        ],
        [SamusPoseFamily.StationaryBall] =
        [
            SamusPoseId.MorphBallGroundRightPose,
            SamusPoseId.MorphBallGroundLeftPose,
            SamusPoseId.SpringBallGroundRightPose,
            SamusPoseId.SpringBallGroundLeftPose,
        ],
        [SamusPoseFamily.ChozoHandBall] =
        [
            SamusPoseId.MorphBallGroundRightPose,
            SamusPoseId.SpringBallGroundRightPose,
            SamusPoseId.SpringBallGroundLeftPose,
        ],
        [SamusPoseFamily.DamageBoostJumpLeft] =
        [
            SamusPoseId.NeutralJumpLeftPose,
            SamusPoseId.NormalJumpForwardLeftPose,
        ],
        [SamusPoseFamily.DamageBoostJumpRight] =
        [
            SamusPoseId.NeutralJumpRightPose,
            SamusPoseId.NormalJumpForwardRightPose,
        ],
        [SamusPoseFamily.PlainFallingOnly] =
        [
            SamusPoseId.FallingRightPose,
            SamusPoseId.FallingLeftPose,
        ],
        [SamusPoseFamily.GenericSpinJump] =
        [
            SamusPoseId.SpinJumpRightPose,
            SamusPoseId.SpinJumpLeftPose,
        ],
        [SamusPoseFamily.NeutralJump] =
        [
            SamusPoseId.NeutralJumpRightPose,
            SamusPoseId.NeutralJumpLeftPose,
        ],
        [SamusPoseFamily.NeutralStanding] =
        [
            SamusPoseId.FacingRightNormalPose,
            SamusPoseId.FacingLeftNormalPose,
        ],
        [SamusPoseFamily.PlainFalling] =
        [
            SamusPoseId.FallingRightPose,
            SamusPoseId.FallingLeftPose,
            SamusPoseId.FallingGunExtendedRightPose,
            SamusPoseId.FallingGunExtendedLeftPose,
        ],
        [SamusPoseFamily.GrappleSwing] =
        [
            SamusPoseId.GrappleSwingRightPose,
            SamusPoseId.GrappleSwingLeftPose,
        ],
        [SamusPoseFamily.DraygonGrabbedMoving] =
        [
            SamusPoseId.DraygonGrabbedMovingLeftPose,
            SamusPoseId.DraygonGrabbedMovingRightPose,
        ],
    };
}
