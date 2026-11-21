using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Reads of Samus's pose definitions (<see cref="SamusPoseDefinitions"/>): collision radii, facing and movement, and the per-pose facts her movement consults, including the bomb-jump direction publication.</summary>
internal static class SamusPoseMetadata
{
    /// <summary>The shinespark flights, from <see cref="SamusPoseFamilyDefinitions.ShinesparkPoses"/>.</summary>
    private static readonly FrozenSet<SamusPoseId> ShinesparkPoses = SamusPoseFamilyDefinitions.ShinesparkPoses.ToFrozenSet();

    /// <summary>The poses an elevator arms under, from <see cref="SamusPoseFamilyDefinitions.ElevatorArmingPoses"/>.</summary>
    private static readonly FrozenSet<SamusPoseId> ElevatorArmingPoses = SamusPoseFamilyDefinitions.ElevatorArmingPoses.ToFrozenSet();

    /// <summary>The posture-transition admissions, from <see cref="SamusPoseFamilyDefinitions.PostureTransitionAdmissions"/>.</summary>
    private static readonly FrozenDictionary<SamusPoseId, SamusPostureTransitionAdmission> PostureTransitionAdmissions =
        SamusPoseFamilyDefinitions.PostureTransitionAdmissions.ToFrozenDictionary();

    /// <summary>True for the six shinespark flights.</summary>
    public static bool IsShinesparkPose(SamusPoseId pose) => ShinesparkPoses.Contains(pose);

    /// <summary>True for the poses an elevator arms under: the power-suit front view and the standing poses.</summary>
    public static bool ArmsElevator(SamusPoseId pose) => ElevatorArmingPoses.Contains(pose);

    /// <summary>
    /// How the posture-transition weapon admission treats <paramref name="pose"/>: the family the definitions list it in, or
    /// <see cref="SamusPostureTransitionAdmission.AnsweredBeyondTransitions"/> for every pose they do not list.
    /// </summary>
    public static SamusPostureTransitionAdmission PostureTransitionAdmission(SamusPoseId pose) =>
        PostureTransitionAdmissions.TryGetValue(pose, out SamusPostureTransitionAdmission admission)
            ? admission
            : SamusPostureTransitionAdmission.AnsweredBeyondTransitions;

    /// <summary>
    /// Sets Samus's collision radius: every pose is five pixels wide each way and has its own vertical radius.
    /// </summary>
    public static void RefreshCollisionRadii(SamusState samus)
    {
        samus.Kinematics.Radius = new(SamusPoseCollisionDefinitions.HorizontalRadius, ReadPoseYRadius(samus.Pose));
    }

    /// <summary>Reads a prospective pose's radius without publishing it to live collision state.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    public static int ReadPoseYRadius(SamusPoseId pose) => SamusPoseTraits.Of(pose).VerticalRadius;

    /// <summary>The way the current pose faces.</summary>
    public static SamusFacingDirection ReadFacingDirection(SamusState samus) =>
        ReadFacingDirection(samus.InitializedPose);

    /// <summary>The way an arbitrary current or prospective pose faces.</summary>
    public static SamusFacingDirection ReadFacingDirection(SamusPoseId pose) => SamusPoseTraits.Of(pose).Facing;

    /// <summary>Whether the current pose faces <paramref name="direction"/>.</summary>
    public static bool IsFacing(SamusState samus, SamusFacingDirection direction) =>
        ReadFacingDirection(samus) == direction;

    /// <summary>Left-facing query for an arbitrary pose without mutating live state.</summary>
    public static bool IsFacingLeft(SamusPoseId pose) =>
        ReadFacingDirection(pose) == SamusFacingDirection.Left;

    /// <summary>Whether <paramref name="pose"/>'s definition moves as <paramref name="movement"/>.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    private static bool Moves(SamusPoseId pose, SamusMovementType movement) => ReadMovementType(pose) == movement;

    /// <summary>Whether <paramref name="pose"/>'s definition faces <paramref name="facing"/> and moves as <paramref name="movement"/>.</summary>
    /// <exception cref="InvalidOperationException">The pose is undefined.</exception>
    private static bool FacesAndMoves(SamusPoseId pose, SamusFacingDirection facing, SamusMovementType movement)
    {
        SamusPoseDefinition definition = SamusPoseTraits.Of(pose);
        return definition.Facing == facing && definition.Movement == movement;
    }

    /// <summary>
    /// Reads and validates the current pose's movement type, which selects the one movement Samus runs.
    /// </summary>
    public static SamusMovementType ReadMovementType(SamusState samus)
    {
        return ReadMovementType(samus.InitializedPose);
    }

    /// <summary>
    /// Reads a prospective pose's movement type without mutating Samus.
    /// An undefined pose fails here, before it can be mistaken for a valid gameplay state downstream.
    /// </summary>
    public static SamusMovementType ReadMovementType(SamusPoseId pose) => SamusPoseTraits.Of(pose).Movement;

    /// <summary>
    /// Reads the current pose's no-input fallback: what is selected when no actions are held and the
    /// input transitions therefore are not consulted.
    /// </summary>
    public static SamusNoInputPose ReadNoInputPose(SamusState samus) => SamusPoseTraits.Of(samus.Pose).NoInput;

    /// <summary>What Samus's pose does with her arm cannon.</summary>
    public static SamusPoseAim ReadAim(SamusState samus) => ReadAim(samus.Pose);

    /// <summary>
    /// The direction Samus's pose aims her arm cannon, or none for a pose that fires nothing (turning or shooting
    /// disabled).
    /// </summary>
    public static SamusProjectileDirection? ReadAimDirection(SamusState samus) => ReadAim(samus) is SamusPoseAim.Aiming aiming ? aiming.Direction : null;

    /// <summary>What a current or prospective pose does with Samus's arm cannon.</summary>
    public static SamusPoseAim ReadAim(SamusPoseId pose) => SamusPoseTraits.Of(pose).Aim;

    /// <summary>
    /// Reads the pose's signed vertical drawing offset for presentation, including Grapple's flare.
    /// A beam's or the grapple's origin uses its own correction, so replacing
    /// artwork never moves where a projectile collides.
    /// </summary>
    public static int ReadArtOrigin(SamusState samus)
    {
        return samus.Presentation.Artwork.ArtOrigin(samus.Pose);
    }

    /// <summary>True for the four right-facing standing poses that move as <see cref="SamusMovementType.Standing"/>.</summary>
    public static bool IsRightFacingStandingPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.RightFacingStanding);

    /// <summary>True for the four left-facing standing poses that move as <see cref="SamusMovementType.Standing"/>.</summary>
    public static bool IsLeftFacingStandingPose(SamusPoseId pose) => IsRightFacingStandingPose(SamusPoseMirror.Of(pose));

    /// <summary>
    /// True for the two front-view poses selected when Samus faces forward.
    /// They face neither way and must never be guessed as left or right.
    /// </summary>
    public static bool IsForwardFacingPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.ForwardFacing);

    /// <summary>
    /// Selects the suit's front-view pose at its first frame, as facing forward and the Zebes start setup both do. The
    /// Gravity and Varia suits have equal precedence: either selects the shared suited pose <see cref="SamusPoseId.ForwardFacingSuitedPose"/>.
    /// </summary>
    private static void SelectForwardFacingPose(SamusState samus)
    {
        samus.Pose = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit)
            ? SamusPoseId.ForwardFacingSuitedPose
            : SamusPoseId.ForwardFacingPowerSuitPose;
        samus.AnimationFrame = SamusAnimationDelays.AnimationForPose(samus.Pose).Start;
    }

    /// <summary>
    /// Applies the pose part of the setup for the Zebes start:
    /// the suit's front-view pose and its initialization. Unlike
    /// <see cref="SamusPoseMetadata.ApplyForwardFacingPoseSetup"/> it neither shifts pose history, lifts
    /// Samus nor clears motion. The suit's colours and the special frame are set separately.
    /// </summary>
    public static void ApplyZebesStartPoseSetup(SamusState samus)
    {
        SelectForwardFacingPose(samus);
        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// The setup for an elevator: facing forward, then
    /// the elevator ride. Used on boarding and on a downward
    /// arrival's door finish.
    /// </summary>
    public static void SetupForElevator(SamusState samus)
    {
        ApplyForwardFacingPoseSetup(samus);
        // The elevator setup takes both Samus's movement and her pose input. An uncrashed
        // spark keeps its boost counter as the out-of-bounds elevator Blue Suit, while an
        // admitted Flash keeps only its independent colouring timer. Neither movement owner
        // may resume after elevator travel; the setup restores ordinary pose input.
        SamusMovementHandOver.To(samus, SamusMovementOwner.Elevator);
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        samus.Control.LockOutPlayer();
    }

    /// <summary>
    /// Applies the movement/animation subset of facing Samus forward.
    /// </summary>
    /// <remarks>
    /// This covers the pose the equipment chooses, its animation, the lift its radius sets and every motion value the
    /// setup clears. It does not set the collision radius; the next input phase's radius update does. Facing forward also
    /// locks the player out of Samus's control, ends the grapple, clears the beam flare and reloads the suit shade before
    /// her first displayed frame; this setup does none of those, and each sequence that faces her forward does what it
    /// needs (the elevator's setup locks the player out, <see cref="SetupForElevator"/>).
    /// </remarks>
    public static void ApplyForwardFacingPoseSetup(SamusState samus)
    {
        SelectForwardFacingPose(samus);
        SamusPresentation.InitializeAnimation(samus);

        // This forced owner shifts history even when already facing forward.
        // It does not finish as an ordinary input transition does.
        samus.CommitPoseHistory();

        // Unless her live radius, still the previous pose's, is already the front view's,
        // Samus is lifted, her current and previous Y both.
        if (samus.Kinematics.Radius.Y != SamusPoseCollisionDefinitions.ForwardFacingYRadius)
        {
            samus.Kinematics.Position -= new Offset16<RoomPixels>(0, SamusPoseCollisionDefinitions.ForwardFacingSetupLift);
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
        }

        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Still;
        samus.MorphBallBounceState = null;
    }

    /// <summary>
    /// True for the right-facing poses that move as <see cref="SamusMovementType.Running"/>: the unaimed, gun-extended and
    /// diagonally aimed runs. The gun-extended run belongs here even though its name describes firing: the game moves its
    /// body through exactly the same running physics as the unaimed and aimed running poses.
    /// </summary>
    public static bool IsRightFacingRunningPose(SamusPoseId pose) =>
        FacesAndMoves(pose, SamusFacingDirection.Right, SamusMovementType.Running);

    /// <summary>True for the left-facing poses that move as <see cref="SamusMovementType.Running"/>.</summary>
    public static bool IsLeftFacingRunningPose(SamusPoseId pose) =>
        FacesAndMoves(pose, SamusFacingDirection.Left, SamusMovementType.Running);

    /// <summary>
    /// True for the six ordinary-play bodies whose names explicitly say “gun extended.”
    /// The firing landings are kept separate because they are short transition art,
    /// not stable horizontal-fire bodies selected directly by an input transition.
    /// </summary>
    public static bool IsGunExtendedPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.GunExtended);

    /// <summary>
    /// True for the three poses whose artwork faces left while the moonwalking body travels
    /// right. The naming is intentionally visual; <see cref="ReadFacingDirection(SamusPoseId)"/> answers right.
    /// </summary>
    public static bool IsMoonwalkingFacingLeftPose(SamusPoseId pose) => IsMoonwalkingFacingRightPose(SamusPoseMirror.Of(pose));

    /// <summary>True for the three right-facing moonwalk poses that travel left.</summary>
    public static bool IsMoonwalkingFacingRightPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.MoonwalkingFacingRight);

    /// <summary>True for the neutral and aimed moonwalk poses, those that move as <see cref="SamusMovementType.Moonwalking"/>, whose artwork faces opposite their travel direction.</summary>
    /// <param name="pose">Current or prospective pose; no pose metadata or live state is changed.</param>
    /// <returns>Whether the pose belongs to either visual-facing moonwalk family.</returns>
    public static bool IsMoonwalkingPose(SamusPoseId pose) => Moves(pose, SamusMovementType.Moonwalking);

    /// <summary>True for all five left-facing Draygon-held poses.</summary>
    public static bool IsLeftFacingDraygonGrabbedPose(SamusPoseId pose) => IsRightFacingDraygonGrabbedPose(SamusPoseMirror.Of(pose));

    /// <summary>
    /// The way <paramref name="pose"/> faces as a member of the pose family whose right-facing members
    /// <paramref name="isRightFacing"/> names and whose left-facing members <paramref name="isLeftFacing"/> names; none for
    /// a pose of neither half (#2560).
    /// </summary>
    internal static HorizontalDirection? FacingIn(SamusPoseId pose, Func<SamusPoseId, bool> isRightFacing,
        Func<SamusPoseId, bool> isLeftFacing) =>
        isRightFacing(pose) ? HorizontalDirection.Right
            : isLeftFacing(pose) ? HorizontalDirection.Left
            : null;

    /// <summary>Whether <paramref name="first"/> is a facing, and <paramref name="second"/> the same one.</summary>
    internal static bool SameFacing(HorizontalDirection? first, HorizontalDirection? second) =>
        first is { } facing && second == facing;

    /// <summary>True for all five right-facing Draygon-held poses.</summary>
    public static bool IsRightFacingDraygonGrabbedPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.RightFacingDraygonGrabbed);

    /// <summary>
    /// Applies one ordinary Draygon-held transition. Every admitted pose stays within one facing, retains the standing radius, and restarts the
    /// target's animation at its start through the normal pose setup.
    /// </summary>
    public static void ApplyDraygonGrabbedPoseChange(SamusState samus, SamusPoseId targetPose)
    {
        if (!SameFacing(FacingIn(samus.Pose, IsRightFacingDraygonGrabbedPose, IsLeftFacingDraygonGrabbedPose),
                FacingIn(targetPose, IsRightFacingDraygonGrabbedPose, IsLeftFacingDraygonGrabbedPose)))
        {
            // The left-facing grabbed poses' input transitions reach only left-facing grabbed poses, and the right-facing
            // ones only right-facing: neither set crosses facing families. Draygon's owner AI
            // performs facing changes by re-entering its grabbed movement directly, so a
            // cross-family request to this input transition is a broken contract,
            // not an omitted input transition.
            throw new InvalidOperationException(
                $"Draygon-grabbed input transition {samus.Pose} -> {targetPose} crosses owner-controlled facing families.");
        }

        int oldRadius = samus.Kinematics.Radius.Y;
        samus.Pose = targetPose;
        RefreshCollisionRadii(samus);
        if (samus.Kinematics.Radius.Y != oldRadius)
        {
            throw new InvalidDataException(
                $"Draygon pose {targetPose} unexpectedly changed radius {oldRadius} -> {samus.Kinematics.Radius.Y}.");
        }

        SamusPresentation.InitializeAnimation(samus);
    }

    /// <summary>
    /// True for the moonwalk turn jumps, the six grounded turn frames selected only when Jump begins a
    /// direction reversal from moonwalking. The predicates below group them by the facing they turn to.
    /// </summary>
    public static bool IsMoonwalkTurnToSpinJumpPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.MoonwalkTurnToSpinJump);

    /// <summary>True for the moonwalk turn-jump family whose input transitions lead to left.</summary>
    public static bool IsMoonwalkTurnToSpinJumpLeftPose(SamusPoseId pose) => IsMoonwalkTurnToSpinJumpRightPose(SamusPoseMirror.Of(pose));

    /// <summary>True for the moonwalk turn-jump family whose input transitions lead to right.</summary>
    public static bool IsMoonwalkTurnToSpinJumpRightPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.MoonwalkTurnToSpinJumpRight);

    /// <summary>True for the right-facing poses that move as <see cref="SamusMovementType.RanIntoWall"/>.</summary>
    public static bool IsRightFacingRanIntoWallPose(SamusPoseId pose) =>
        FacesAndMoves(pose, SamusFacingDirection.Right, SamusMovementType.RanIntoWall);

    /// <summary>True for the left-facing poses that move as <see cref="SamusMovementType.RanIntoWall"/>.</summary>
    public static bool IsLeftFacingRanIntoWallPose(SamusPoseId pose) =>
        FacesAndMoves(pose, SamusFacingDirection.Left, SamusMovementType.RanIntoWall);

    /// <summary>True for the ran-into-wall poses, neutral and aimed, facing either way: those that move as <see cref="SamusMovementType.RanIntoWall"/>.</summary>
    /// <param name="pose">Current or prospective pose.</param>
    /// <returns>Whether the pose is a supported standing body pressed against a wall, rather than a wall-jump body.</returns>
    public static bool IsRanIntoWallPose(SamusPoseId pose) => Moves(pose, SamusMovementType.RanIntoWall);

    /// <summary>True when a supported standing, running, crouching, or landing pose carries aim metadata.</summary>
    public static bool IsGroundedAimPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.GroundedAim);

    /// <summary>True for right-facing aimed normal-jump landing <see cref="SamusPoseId.LandingAimUpRightPose"/>, <see cref="SamusPoseId.LandingAimDiagonalUpRightPose"/> and <see cref="SamusPoseId.LandingAimDiagonalDownRightPose"/>.</summary>
    public static bool IsRightFacingAimedLandingPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.RightFacingAimedLanding);

    /// <summary>
    /// True for the complete right-facing landing set that shares the ordinary standing
    /// input transitions: normal, spin, aimed up, diagonally up and diagonally down, and firing.
    /// Every pose change that tests for a landing reads this set, so each of these landings keeps standing's crouch,
    /// turn, run and wall-probe exits.
    /// </summary>
    public static bool IsRightFacingLandingPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.RightFacingLanding) ||
        IsRightFacingAimedLandingPose(pose);

    /// <summary>True for the complete mirrored left-facing landing set.</summary>
    public static bool IsLeftFacingLandingPose(SamusPoseId pose) => IsRightFacingLandingPose(SamusPoseMirror.Of(pose));

    /// <summary>True for every landing, facing either way: the normal, spin, aimed and firing landings.</summary>
    public static bool IsLandingPose(SamusPoseId pose) => IsRightFacingLandingPose(pose) || IsLeftFacingLandingPose(pose);

    /// <summary>
    /// True for the complete right-facing <see cref="SamusMovementType.Crouching"/> family. All its
    /// poses use the crouching radius and the shared input transitions.
    /// </summary>
    public static bool IsRightFacingCrouchingPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.RightFacingCrouching);

    /// <summary>
    /// True for the complete left-facing <see cref="SamusMovementType.Crouching"/> family. All its
    /// poses use the crouching radius and the mirrored input transitions.
    /// </summary>
    public static bool IsLeftFacingCrouchingPose(SamusPoseId pose) => IsRightFacingCrouchingPose(SamusPoseMirror.Of(pose));

    /// <summary>
    /// True for the grounded poses that start a fall when their movement finds no floor below: standing, landing,
    /// running, moonwalking, knockback, ran-into-wall and crouching, facing either way. The X-ray, death-sequence and
    /// forward-facing bodies share some of these movement types but are not among them, and a turn finishes into its
    /// destination pose before that pose detects the missing floor.
    /// </summary>
    public static bool WalksOffFloor(SamusPoseId pose) =>
        IsRightFacingStandingPose(pose) || IsLeftFacingStandingPose(pose) ||
        IsLandingPose(pose) ||
        IsRightFacingRunningPose(pose) || IsLeftFacingRunningPose(pose) ||
        IsMoonwalkingPose(pose) ||
        IsKnockbackPose(pose) ||
        IsRanIntoWallPose(pose) ||
        IsRightFacingCrouchingPose(pose) || IsLeftFacingCrouchingPose(pose);

    /// <summary>
    /// True for the grounded turning poses that began facing right and turn left. Each already faces left, the facing
    /// it turns to, so preserving Samus's old rightward momentum must use this grouping.
    /// </summary>
    public static bool IsRightToLeftGroundTurnPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.RightToLeftGroundTurn);

    /// <summary>True for admitted grounded turning poses that began facing left and turn right.</summary>
    public static bool IsLeftToRightGroundTurnPose(SamusPoseId pose) => IsRightToLeftGroundTurnPose(SamusPoseMirror.Of(pose));

    /// <summary>
    /// True for the six aimed crouched-turn poses moved by the turning-while-jumping movement type.
    /// The unaimed crouching turns are intentionally absent because the pose definitions assign those
    /// otherwise similar-looking turns to the grounded turning movement type.
    /// </summary>
    public static bool IsAimedCrouchingTurnPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.AimedCrouchingTurn);

    /// <summary>True for every turning-while-jumping normal-jump turn pose.</summary>
    public static bool IsJumpingTurnPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.JumpingTurn);

    /// <summary>True for every turning-while-falling falling-turn pose.</summary>
    public static bool IsFallingTurnPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.FallingTurn);

    /// <summary>True for the sixteen normal-jump or falling turn poses of the two airborne turning movement types, excluding grounded and aimed crouching turns.</summary>
    /// <param name="pose">Current or prospective pose.</param>
    /// <returns>Whether either jumping-turn or falling-turn family contains the pose.</returns>
    public static bool IsAerialTurnPose(SamusPoseId pose) =>
        IsJumpingTurnPose(pose) || IsFallingTurnPose(pose);

    /// <summary>True for the eight jumping/falling turn poses that change from right to left, including their up, down, and diagonal-up aim variants.</summary>
    /// <param name="pose">Prospective or active turn pose; direction describes the facing change, not horizontal velocity.</param>
    /// <returns>Whether the pose is a right-to-left aerial turn admitted by the transition selector.</returns>
    public static bool IsRightToLeftAerialTurnPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.RightToLeftAerialTurn);

    /// <summary>True for the eight jumping/falling turn poses that change from left to right, including their up, down, and diagonal-up aim variants.</summary>
    /// <param name="pose">Prospective or active turn pose; direction describes the facing change, not horizontal velocity.</param>
    /// <returns>Whether the pose is a left-to-right aerial turn admitted by the transition selector.</returns>
    public static bool IsLeftToRightAerialTurnPose(SamusPoseId pose) => IsRightToLeftAerialTurnPose(SamusPoseMirror.Of(pose));

    /// <summary>True for the four drained crouching and standing bodies that Mother Brain's attack puts her in.</summary>
    public static bool IsDrainedPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.Drained);

    /// <summary>True for the wall-jumping launch poses, those that move as <see cref="SamusMovementType.WallJumping"/>.</summary>
    public static bool IsWallJumpPose(SamusPoseId pose) => Moves(pose, SamusMovementType.WallJumping);

    /// <summary>
    /// True for the stable spin-jumping bodies, those that move as <see cref="SamusMovementType.SpinJumping"/>: the
    /// ordinary spin, Space Jump and Screw Attack facing either way. The input transitions use the ordinary spin jump as
    /// generic outputs; the spin-jump setup can then substitute Space Jump or Screw Attack without changing the movement
    /// step.
    /// </summary>
    public static bool IsSpinJumpPose(SamusPoseId pose) => Moves(pose, SamusMovementType.SpinJumping);

    /// <summary>True for the two poses a hit knocks Samus back in, facing right and left.</summary>
    public static bool IsKnockbackPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.Knockback);

    /// <summary>True for the damage boosts, the poses that move as <see cref="SamusMovementType.DamageBoost"/>.</summary>
    public static bool IsDamageBoostPose(SamusPoseId pose) => Moves(pose, SamusMovementType.DamageBoost);

    /// <summary>
    /// True for the jump transitions, neutral and aimed up, diagonally up or diagonally down, facing either way: the
    /// normal-jumping poses a jump begins in before its launch body. They move Samus only by external displacement.
    /// </summary>
    public static bool IsJumpTransitionPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.JumpTransition);

    /// <summary>
    /// True for the normal-jump launch bodies that begin a shinespark wind-up while Samus holds stored shine: the neutral
    /// jump and the jumps aimed up and diagonally up, facing either way.
    /// </summary>
    public static bool BeginsShinesparkFromStoredShine(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.StoredShineShinesparkStart);

    /// <summary>True only for the two Screw Attack animation poses.</summary>
    public static bool IsScrewAttackPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.ScrewAttack);

    /// <summary>
    /// A respawning bomb block's collision breaks the block when the boost
    /// stage is active or Samus is screw attacking or shinesparking.
    /// </summary>
    public static bool BreaksCollisionBombBlocks(SamusState samus) =>
        samus.HorizontalSpeed.IsActivelySpeedBoosting ||
        IsScrewAttackPose(samus.Pose) ||
        IsShinesparkPose(samus.Pose);

    /// <summary>True only for the two Space Jump animation poses.</summary>
    public static bool IsSpaceJumpPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.SpaceJump);

    /// <summary>
    /// True only for the admitted posture-transition crouch/stand animation poses.
    /// Morph/unmorph poses share that step but are intentionally not hidden here.
    /// </summary>
    public static bool IsCrouchStandTransitionPose(SamusPoseId pose) =>
        IsCrouchingTransitionPose(pose) || IsStandingTransitionPose(pose);

    /// <summary>True for the crouching transitions, unaimed and aimed up, diagonally up or diagonally down, facing either way.</summary>
    public static bool IsCrouchingTransitionPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.CrouchingTransition);

    /// <summary>True for the standing transitions, unaimed and aimed up, diagonally up or diagonally down, facing either way.</summary>
    public static bool IsStandingTransitionPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.StandingTransition);

    /// <summary>True for the four ordinary, non-Spring-Ball grounded morph poses.</summary>
    public static bool IsGroundedMorphBallPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.GroundedMorphBall);

    /// <summary>True for the two ordinary, non-Spring-Ball airborne morph poses.</summary>
    public static bool IsAirborneMorphBallPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.AirborneMorphBall);

    /// <summary>True for the four grounded Spring Ball poses.</summary>
    public static bool IsGroundedSpringBallPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.GroundedSpringBall);

    /// <summary>True for Spring Ball jump/fall poses across the in-air and falling Spring Ball movement types.</summary>
    public static bool IsAirborneSpringBallPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.AirborneSpringBall);

    /// <summary>True for every stable ordinary or Spring Ball pose using the ball's radius.</summary>
    public static bool IsStableBallPose(SamusPoseId pose) =>
        IsGroundedMorphBallPose(pose) || IsAirborneMorphBallPose(pose) ||
        IsGroundedSpringBallPose(pose) || IsAirborneSpringBallPose(pose);

    /// <summary>True for the four morph entry/exit poses handled by the posture-transition movement type.</summary>
    public static bool IsMorphTransitionPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.MorphTransition);

    /// <summary>
    /// Publishes a bomb overlap's launch direction as a request Samus's pose handling has not yet accepted. The
    /// gameplay loop does this after the input phase; setup considers it after movement and hurt arbitration.
    /// </summary>
    public static void PublishBombJumpDirection(SamusState samus, SamusBombJumpDirection direction) =>
        // Publishing replaces the whole request, so a blast during an accepted jump publishes a fresh, unaccepted one.
        samus.BombJump = SamusBombJump.FromBlast(direction);

    /// <summary>
    /// Sets up a published bomb jump as Samus's movement does (<see cref="SamusMovementTypeDefinitions"/>) before pose
    /// selection, dropping the request where her movement turns it down.
    /// </summary>
    /// <remarks>
    /// Standing and crouching reject the request only while time is frozen. Running,
    /// falling, moonwalking, wall-jump, wall-stop, and grapple families first select the
    /// ordinary forward-jump body. Ball, glitch-ball, and knockback-family
    /// entries retain their current pose. Jump/turn/transition/damage-boost and actor-owned
    /// families turn it down and clear the published direction.
    /// </remarks>
    /// <returns>True when the pending request was rejected and dropped.</returns>
    public static bool RejectUnsupportedPublishedBombJump(SamusState samus, bool timeIsFrozen)
    {
        if (samus.BombJump is not { AwaitsPoseHandling: true })
            return false;
        SamusBombJumpSetup setup = BombJumpSetupOf(samus);
        bool reject = setup == SamusBombJumpSetup.TurnedDown ||
            timeIsFrozen && setup == SamusBombJumpSetup.ForwardJumpUnlessTimeFrozen;
        if (reject) samus.BombJump = null;
        return reject;
    }

    /// <summary>Applies the admitted bomb-jump setup after the interruption's rejection side effects.</summary>
    /// <remarks>The newly pressed actions feed the transition-shot test.</remarks>
    public static bool TrySetupPublishedBombJump(
        SamusState samus,
        RoomLevelData level,
        bool timeIsFrozen,
        int frameCounter,
        GameplayAction controllerNewInput,
        ISamusRoomObjects roomObjects)
    {
        Ensure.NotNull(level);
        if (RejectUnsupportedPublishedBombJump(samus, timeIsFrozen) || samus.BombJump is not { AwaitsPoseHandling: true } published)
            return false;

        // The rejection above has dropped every turned-down request, and a frozen-time one where frozen time turns it down.
        SamusMovementType movementType = ReadMovementType(samus);
        switch (BombJumpSetupOf(samus))
        {
            case SamusBombJumpSetup.ForwardJump:
            case SamusBombJumpSetup.ForwardJumpUnlessTimeFrozen:
                // The jump reads the pose's facing, not its name: a pose that does not face left takes the right-facing path.
                SamusPoseId sourcePose = samus.Pose;
                SamusPoseId targetPose = ReadFacingDirection(samus) == SamusFacingDirection.Left
                    ? SamusPoseId.NormalJumpForwardLeftPose
                    : SamusPoseId.NormalJumpForwardRightPose;

                // The pose update routes the special target through the same larger-radius
                // expansion collision used by controller-selected pose changes. This is
                // observable for a crouching bomb jump: the crouching radius grows to the standing one and may be
                // rejected by a low ceiling instead of clipping Samus into terrain.
                LargerPoseCollisionOutcome collision = samus.ResolveLargerPoseCollision(
                    level,
                    targetPose,
                    frameCounter,
                    roomObjects,
                    out int centerAdjustment);
                if (collision != LargerPoseCollisionOutcome.Allowed)
                {
                    if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                        samus.ApplyPoseChangeCollisionCrouchFallback(sourcePose);
                    return false;
                }

                samus.Pose = targetPose;
                RefreshCollisionRadii(samus);
                // The collision check keeps the corrected centre in the room (ResolveLargerPoseCollision).
                samus.Kinematics.Position = samus.Kinematics.Position.WithWholeY(
                    samus.Kinematics.Position.Y.Whole + centerAdjustment);

                // The special target installs through the pose setup, which runs the new
                // normal-jumping type to the normal-jump setup (the forward jump cannot start a spark there). It
                // then restores the suit shade after a wall jump with Screw Attack.
                SamusAerialPoseTransitions.InitializeInstalledNormalJumpPose(samus, movementType, controllerNewInput);
                if (movementType == SamusMovementType.WallJumping &&
                    samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
                    samus.HorizontalSpeed.ShowSuitShadeAtColourStep();

                // The selected forward-jump poses are NOT the transition poses that
                // initialize ordinary jump speed (or apply the crouch jump offset).
                // This frame's velocity stays; bomb start replaces it next frame.
                SamusPresentation.InitializeAnimation(samus);
                ArmPublishedBombJump(samus, published);
                return true;

            case SamusBombJumpSetup.KeepPose:
                // Samus's current pose becomes the special prospective pose. Because
                // that pose already matches, Samus's pose handling immediately arms the bomb jump.
                ArmPublishedBombJump(samus, published);
                return true;

            case SamusBombJumpSetup.TurnedDown:
                throw new InvalidOperationException(
                    $"Bomb-jump setup reached a turned-down request for pose {samus.Pose}, which its rejection drops.");

            default:
                throw new InvalidDataException($"Undefined {nameof(SamusBombJumpSetup)} for pose {samus.Pose}.");
        }
    }

    /// <summary>How a published bomb jump is set up from the way Samus moves (<see cref="SamusMovementTypeDefinitions"/>).</summary>
    /// <exception cref="InvalidDataException">Her movement type has no definition.</exception>
    private static SamusBombJumpSetup BombJumpSetupOf(SamusState samus)
    {
        SamusMovementType movementType = ReadMovementType(samus);
        return SamusMovementTypeDefinitions.Movements.TryGetValue(movementType, out SamusMovementTypeDefinition? definition)
            ? definition.BombJump
            : throw new InvalidDataException(
                $"Bomb-jump setup cannot run invalid movement type {movementType} for pose {samus.Pose}.");
    }

    /// <summary>Arms the published bomb jump.</summary>
    private static void ArmPublishedBombJump(SamusState samus, SamusBombJump published)
    {
        // Arming the bomb jump takes, rather than suspends, Samus's movement.
        // A suit-interrupted spark may still hold it despite the ordinary body pose.
        // Keep its independently running colouring and boost state, but never resume it
        // when the bomb arc returns her movement and input to her pose. The same
        // applies when suit pickup suspended Flash: its shade keeps running, but its
        // raising/drain must never resume after the bomb arc.
        SamusMovementHandOver.To(samus, SamusMovementOwner.BombJump);
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        samus.BombJump = published.Arm();
        samus.BombJumpPhase = Projectiles.SamusBombJumpPhase.Starting;
        samus.BombJumpPoseInputLocked = true;
    }

    /// <summary>True for the right-facing poses that move as <see cref="SamusMovementType.NormalJumping"/>.</summary>
    public static bool IsRightFacingNormalJumpPose(SamusPoseId pose) =>
        FacesAndMoves(pose, SamusFacingDirection.Right, SamusMovementType.NormalJumping);

    /// <summary>True for the left-facing poses that move as <see cref="SamusMovementType.NormalJumping"/>.</summary>
    public static bool IsLeftFacingNormalJumpPose(SamusPoseId pose) =>
        FacesAndMoves(pose, SamusFacingDirection.Left, SamusMovementType.NormalJumping);

    /// <summary>True for the right-facing poses that move as <see cref="SamusMovementType.Falling"/>.</summary>
    public static bool IsRightFacingFallingPose(SamusPoseId pose) =>
        FacesAndMoves(pose, SamusFacingDirection.Right, SamusMovementType.Falling);

    /// <summary>True for the left-facing poses that move as <see cref="SamusMovementType.Falling"/>.</summary>
    public static bool IsLeftFacingFallingPose(SamusPoseId pose) =>
        FacesAndMoves(pose, SamusFacingDirection.Left, SamusMovementType.Falling);

    /// <summary>True for the aimed normal-jump/falling poses, including compact Down aim.</summary>
    public static bool IsAimedAerialPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.AimedAerial);

    /// <summary>
    /// True for the forward jumps, the two normal-jumping bodies used while Samus
    /// has a same-facing horizontal direction held. These are ordinary aerial poses, not
    /// aimed or spinning poses; naming the pair prevents their transition semantics from
    /// being hidden inside another aim-specific condition.
    /// </summary>
    public static bool IsForwardMovingNormalJumpPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.ForwardMovingNormalJump);

    /// <summary>
    /// Reports whether a same-facing normal-jump/falling pose change belongs to the shared
    /// aim, horizontal-fire, or moving-forward transition seam.
    /// </summary>
    /// <remarks>
    /// The neutral jump, the forward jump and the non-compact aimed jump bodies share one set of input
    /// transitions. In particular, holding Jump after
    /// releasing Right/Left turns the forward jump into the neutral jump. Both endpoints therefore take part in this
    /// classification: entering and leaving the forward body run the same pose and animation setup and keep the
    /// live velocity.
    /// </remarks>
    public static bool IsSameFacingAerialAimFireOrForwardTransition(SamusPoseId source, SamusPoseId target)
    {
        bool changesAimedOrForwardBody =
            IsAimedAerialPose(source) || IsAimedAerialPose(target) ||
            IsGunExtendedPose(source) || IsGunExtendedPose(target) ||
            IsForwardMovingNormalJumpPose(source) ||
            IsForwardMovingNormalJumpPose(target);
        if (!changesAimedOrForwardBody)
            return false;

        return
            (IsRightFacingNormalJumpPose(source) && IsRightFacingNormalJumpPose(target)) ||
            (IsLeftFacingNormalJumpPose(source) && IsLeftFacingNormalJumpPose(target)) ||
            (IsRightFacingFallingPose(source) && IsRightFacingFallingPose(target)) ||
            (IsLeftFacingFallingPose(source) && IsLeftFacingFallingPose(target));
    }

    /// <summary>True only for the compact straight-down aerial bodies.</summary>
    public static bool IsCompactAerialPose(SamusPoseId pose) => SamusPoseTraits.IsIn(pose, SamusPoseFamily.CompactAerial);
}
