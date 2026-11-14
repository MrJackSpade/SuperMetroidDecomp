using System.Collections.Frozen;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Samus.Poses;

/// <summary>Reads of the game pose tables for Samus: collision radii, facing and movement kind, and the per-pose metadata her movement consults, including the bomb-jump direction publication.</summary>
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
    /// <see cref="SamusPostureTransitionAdmission.BeyondTable"/> for every pose they do not list.
    /// </summary>
    public static SamusPostureTransitionAdmission PostureTransitionAdmission(SamusPoseId pose) =>
        PostureTransitionAdmissions.TryGetValue(pose, out SamusPostureTransitionAdmission admission)
            ? admission
            : SamusPostureTransitionAdmission.BeyondTable;

    /// <summary>
    /// Sets Samus's collision radius: every pose is five pixels wide each way and has its own vertical radius.
    /// </summary>
    public static void RefreshCollisionRadii(SamusState samus)
    {
        samus.Kinematics.Radius = new(5, ReadPoseYRadius(samus.Pose));
    }

    /// <summary>Reads a prospective pose's radius without publishing it to live collision state.</summary>
    public static int ReadPoseYRadius(SamusPoseId pose) =>
        SamusPoseCollisionRadii.ReadVerticalRadius(pose);

    /// <summary>The way the current pose faces.</summary>
    public static SamusFacingDirection ReadFacingDirection(SamusState samus) =>
        ReadFacingDirection(samus.InitializedPose);

    /// <summary>The way an arbitrary current or prospective pose faces.</summary>
    public static SamusFacingDirection ReadFacingDirection(SamusPoseId pose) =>
        SamusPoseTraits.ReadFacing(pose);

    /// <summary>Whether the current pose faces <paramref name="direction"/>.</summary>
    public static bool IsFacing(SamusState samus, SamusFacingDirection direction) =>
        ReadFacingDirection(samus) == direction;

    /// <summary>Left-facing query for an arbitrary pose without mutating live state.</summary>
    public static bool IsFacingLeft(SamusPoseId pose) =>
        ReadFacingDirection(pose) == SamusFacingDirection.Left;

    /// <summary>
    /// Reads and validates the current pose's movement type, which selects the one movement Samus runs.
    /// </summary>
    public static SamusMovementType ReadMovementType(SamusState samus)
    {
        return ReadMovementType(samus.InitializedPose);
    }

    /// <summary>
    /// Compatibility spelling retained for callers that describe the value as a movement
    /// kind. Both APIs now return the same validated discriminator.
    /// </summary>
    public static SamusMovementType ReadMovementKind(SamusState samus) =>
        ReadMovementType(samus);

    /// <summary>
    /// Reads a prospective pose's movement type without mutating Samus.
    /// Values beyond the retail pose list are corrupt pose metadata and
    /// fail here, before they can be mistaken for a valid gameplay state downstream.
    /// </summary>
    public static SamusMovementType ReadMovementType(SamusPoseId pose) =>
        SamusPoseTraits.ReadMovement(pose);

    /// <summary>Compatibility spelling for an arbitrary prospective pose.</summary>
    public static SamusMovementType ReadMovementKind(SamusPoseId pose) =>
        ReadMovementType(pose);

    /// <summary>
    /// Reads the current pose's no-input pose: the pose selected when no buttons are held and the
    /// transition table therefore is not consulted.
    /// </summary>
    public static SamusPoseId ReadNoInputFallbackPose(SamusState samus)
    {
        return SamusPoseTraits.ReadNoInputPose(samus.Pose);
    }

    /// <summary>What Samus's pose does with her arm cannon.</summary>
    public static SamusPoseAim ReadAim(SamusState samus) => ReadAim(samus.Pose);

    /// <summary>
    /// The direction Samus's pose aims her arm cannon, or none for a pose that fires nothing (turning, shooting
    /// disabled, and the unauthored poses).
    /// </summary>
    public static SamusProjectileDirection? ReadAimDirection(SamusState samus) => ReadAim(samus).Direction;

    /// <summary>What a current or prospective pose does with Samus's arm cannon.</summary>
    public static SamusPoseAim ReadAim(SamusPoseId pose) => SamusPoseAims.Read(pose);

    /// <summary>
    /// Reads the pose's signed vertical drawing offset for presentation, including Grapple's flare.
    /// Physical beam/Grapple origins use their separate compiled correction; replacing
    /// artwork must not shift a projectile's collision position.
    /// </summary>
    public static int ReadGraphicsYOffset(SamusState samus)
    {
        return (samus.Presentation.Artwork ?? throw new InvalidOperationException(
            "Samus pose presentation requires installed body artwork."))
            .GraphicsYOffset(samus.Pose);
    }

    /// <summary>True for the four right-facing standing poses that move as <see cref="SamusMovementType.Standing"/>.</summary>
    public static bool IsRightFacingStandingPose(SamusPoseId pose) => pose is
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.StandingAimUpRightPose or
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.StandingAimDiagonalDownRightPose;

    /// <summary>True for the four left-facing standing poses that move as <see cref="SamusMovementType.Standing"/>.</summary>
    public static bool IsLeftFacingStandingPose(SamusPoseId pose) => IsRightFacingStandingPose(SamusPoseMirror.Of(pose));

    /// <summary>
    /// True for the two front-view poses selected when Samus faces forward.
    /// Their pose-X direction is zero and must never be guessed as left/right.
    /// </summary>
    public static bool IsForwardFacingPose(SamusPoseId pose) => pose is
        SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose;

    /// <summary>
    /// Selects the suit's front-view pose as facing forward
    /// and the Zebes start setup do. The Gravity and Varia suit bits
    /// have equal precedence: either selects the shared suited pose <see cref="SamusPoseId.ForwardFacingSuitedPose"/>.
    /// </summary>
    private static void SelectForwardFacingPose(SamusState samus)
    {
        samus.Pose = samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit)
            ? SamusPoseId.ForwardFacingSuitedPose
            : SamusPoseId.ForwardFacingPowerSuitPose;
        samus.AnimationFrame = 0;
    }

    /// <summary>
    /// Applies the pose part of the setup for the Zebes start:
    /// the suit's front-view pose and its initialization. Unlike
    /// <see cref="SamusPoseMetadata.ApplyForwardFacingPoseSetup"/> it neither shifts pose history, lifts
    /// Samus nor clears motion. Callers own the suit's colours and the special frame.
    /// </summary>
    public static void ApplyZebesStartPoseSetup(SamusState samus)
    {
        SelectForwardFacingPose(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
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
        // admitted Flash keeps only its independent palette timer. Neither movement owner
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
    /// The behaviour also locks the player out of Samus's control, kills grapple,
    /// clears beam-flare presentation state, and reloads the suit palette. Those owners do
    /// not live in this state object. This method deliberately covers only the state it can
    /// own exactly: equipment-selected pose, its animation, the radius-dependent lift and
    /// all motion values cleared by the setup. It does not write the collision radius;
    /// the next input phase's radius update does. Runtime/debug callers remain responsible for
    /// input lock, palette and grapple before their first displayed frame.
    /// </remarks>
    public static void ApplyForwardFacingPoseSetup(SamusState samus)
    {
        SelectForwardFacingPose(samus);
        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);

        // This forced owner shifts history even when already facing forward.
        // It does not pass through the ordinary input-transition epilogue.
        samus.CommitPoseHistory();

        // Compares the live radius, still the previous pose's, and
        // lifts both current and previous Y by three unless it is already the front view's 24.
        if (samus.Kinematics.Radius.Y != 24)
        {
            samus.Kinematics.Position -= new Offset16<RoomPixels>(0, 3);
            samus.WritePreviousYPosition(samus.Kinematics.Position.Y.Whole);
        }

        samus.HorizontalSpeed.ExtraRunSpeed = default;
        samus.HorizontalSpeed.BaseSpeed = default;
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YDirection = SamusVerticalDirection.None;
        samus.MorphBallBounceState = SamusBallBounceState.None;
    }

    /// <summary>
    /// True for all five live running, right-moving pose-table entries. The gun-extended running pose
    /// belongs here even though its name describes firing: the game moves its body
    /// through exactly the same running physics as the unaimed and aimed running poses.
    /// </summary>
    public static bool IsRightFacingRunningPose(SamusPoseId pose) => pose is
        SamusPoseId.MovingRightNormalPose or SamusPoseId.MovingRightGunExtendedPose or
        SamusPoseId.RunningAimUpRightPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or
        SamusPoseId.RunningAimDiagonalDownRightPose;

    /// <summary>True for all five live left-moving <see cref="SamusMovementType.Running"/> pose-table entries.</summary>
    public static bool IsLeftFacingRunningPose(SamusPoseId pose) => IsRightFacingRunningPose(SamusPoseMirror.Of(pose));

    /// <summary>
    /// True for the six ordinary-play bodies whose names explicitly say “gun extended.”
    /// The firing landings are kept separate because they are short transition art,
    /// not stable horizontal-fire bodies selected directly by an input table.
    /// </summary>
    public static bool IsGunExtendedPose(SamusPoseId pose) => pose is
        SamusPoseId.MovingRightGunExtendedPose or SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.NormalJumpGunExtendedRightPose or SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingGunExtendedLeftPose;

    /// <summary>
    /// True for the three poses whose artwork faces left while the moonwalking body travels
    /// right. The naming is intentionally visual; <see cref="ReadFacingDirection(SamusPoseId)"/> answers right.
    /// </summary>
    public static bool IsMoonwalkingFacingLeftPose(SamusPoseId pose) => IsMoonwalkingFacingRightPose(SamusPoseMirror.Of(pose));

    /// <summary>True for the three right-facing moonwalk poses that travel left.</summary>
    public static bool IsMoonwalkingFacingRightPose(SamusPoseId pose) => pose is
        SamusPoseId.MoonwalkFacingRightPose or SamusPoseId.MoonwalkAimUpRightPose or SamusPoseId.MoonwalkAimDownRightPose;

    /// <summary>True for the six neutral or aimed moonwalk poses of the moonwalking movement type, whose artwork faces opposite their travel direction.</summary>
    /// <param name="pose">Current or prospective pose; no pose metadata or live state is changed.</param>
    /// <returns>Whether the pose belongs to either visual-facing moonwalk family.</returns>
    public static bool IsMoonwalkingPose(SamusPoseId pose) =>
        IsMoonwalkingFacingLeftPose(pose) || IsMoonwalkingFacingRightPose(pose);

    /// <summary>True for all five left-facing Draygon-held poses.</summary>
    public static bool IsLeftFacingDraygonGrabbedPose(SamusPoseId pose) => IsRightFacingDraygonGrabbedPose(SamusPoseMirror.Of(pose));

    /// <summary>True for all five right-facing Draygon-held poses.</summary>
    public static bool IsRightFacingDraygonGrabbedPose(SamusPoseId pose) => pose is
        SamusPoseId.DraygonGrabbedNeutralRightPose or SamusPoseId.DraygonGrabbedAimUpRightPose or
        SamusPoseId.DraygonGrabbedFiringRightPose or SamusPoseId.DraygonGrabbedAimDownRightPose or
        SamusPoseId.DraygonGrabbedMovingRightPose;

    /// <summary>True for the complete ten-pose grabbed-by-Draygon family.</summary>
    public static bool IsDraygonGrabbedPose(SamusPoseId pose) =>
        IsLeftFacingDraygonGrabbedPose(pose) || IsRightFacingDraygonGrabbedPose(pose);

    /// <summary>
    /// Applies one ordinary Draygon-held transition without inventing pose physics.
    /// Every admitted pose stays within one facing, retains radius 21, and restarts the
    /// target's animation at its start through the normal pose initializer.
    /// </summary>
    public static void ApplyDraygonGrabbedPoseChange(SamusState samus, SamusPoseId targetPose)
    {

        bool leftFamily = IsLeftFacingDraygonGrabbedPose(samus.Pose) &&
            IsLeftFacingDraygonGrabbedPose(targetPose);
        bool rightFamily = IsRightFacingDraygonGrabbedPose(samus.Pose) &&
            IsRightFacingDraygonGrabbedPose(targetPose);
        if (!leftFamily && !rightFamily)
        {
            // The left-facing table contains only the left-facing grabbed poses; the right-facing one only the right-facing.
            // Neither controller-input table can cross facing families. Draygon's owner AI
            // performs facing changes by re-entering its grabbed controller directly, so a
            // cross-family request to this table-transition API is a caller contract error,
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

        SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
    }

    /// <summary>
    /// True for the moonwalk turn jumps, the six grounded turn frames selected only when Jump begins a
    /// direction reversal from moonwalking. Odd/even descriptive names follow the
    /// literal poses, while the predicates below group them by destination facing.
    /// </summary>
    public static bool IsMoonwalkTurnJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.MoonwalkTurnJumpLeftPose or SamusPoseId.MoonwalkTurnJumpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or SamusPoseId.MoonwalkTurnJumpAimDownRightPose;

    /// <summary>True for the moonwalk turn-jump family whose transition tables lead to left.</summary>
    public static bool IsMoonwalkTurnJumpLeftPose(SamusPoseId pose) => IsMoonwalkTurnJumpRightPose(SamusPoseMirror.Of(pose));

    /// <summary>True for the moonwalk turn-jump family whose transition tables lead to right.</summary>
    public static bool IsMoonwalkTurnJumpRightPose(SamusPoseId pose) => pose is
        SamusPoseId.MoonwalkTurnJumpRightPose or SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose;

    /// <summary>True for the complete right-facing ran-into-wall family.</summary>
    public static bool IsRightFacingRanIntoWallPose(SamusPoseId pose) => pose is
        SamusPoseId.RanIntoWallRightPose or SamusPoseId.RanIntoWallAimUpRightPose or SamusPoseId.RanIntoWallAimDownRightPose;

    /// <summary>True for the complete left-facing ran-into-wall family.</summary>
    public static bool IsLeftFacingRanIntoWallPose(SamusPoseId pose) => IsRightFacingRanIntoWallPose(SamusPoseMirror.Of(pose));

    /// <summary>True for the six ran-into-wall poses: neutral and the two aimed variants for each facing.</summary>
    /// <param name="pose">Current or prospective pose.</param>
    /// <returns>Whether the pose is a supported standing body pressed against a wall, rather than a wall-jump body.</returns>
    public static bool IsRanIntoWallPose(SamusPoseId pose) =>
        IsRightFacingRanIntoWallPose(pose) || IsLeftFacingRanIntoWallPose(pose);

    /// <summary>True for the four aimed wall-contact poses; neutral wall-contact <see cref="SamusPoseId.RanIntoWallRightPose"/> and <see cref="SamusPoseId.RanIntoWallLeftPose"/> are excluded.</summary>
    /// <param name="pose">Current or prospective pose.</param>
    /// <returns>Whether the ran-into-wall body uses an aimed wall-contact variant.</returns>
    public static bool IsAimedRanIntoWallPose(SamusPoseId pose) => pose is
        SamusPoseId.RanIntoWallAimUpRightPose or SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.RanIntoWallAimDownRightPose or SamusPoseId.RanIntoWallAimDownLeftPose;

    /// <summary>True when a supported standing, running, crouching, or landing pose carries aim metadata.</summary>
    public static bool IsGroundedAimPose(SamusPoseId pose) => pose is
        SamusPoseId.StandingAimUpRightPose or SamusPoseId.StandingAimUpLeftPose or
        SamusPoseId.StandingAimDiagonalUpRightPose or SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.StandingAimDiagonalDownRightPose or SamusPoseId.StandingAimDiagonalDownLeftPose or
        SamusPoseId.RunningAimUpRightPose or SamusPoseId.RunningAimUpLeftPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or SamusPoseId.RunningAimDiagonalUpLeftPose or
        SamusPoseId.RunningAimDiagonalDownRightPose or SamusPoseId.RunningAimDiagonalDownLeftPose or
        SamusPoseId.CrouchingAimUpRightPose or SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalDownRightPose or SamusPoseId.CrouchingAimDiagonalDownLeftPose or
        SamusPoseId.LandingAimUpRightPose or SamusPoseId.LandingAimUpLeftPose or
        SamusPoseId.LandingAimDiagonalUpRightPose or SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.LandingAimDiagonalDownRightPose or SamusPoseId.LandingAimDiagonalDownLeftPose or
        SamusPoseId.RanIntoWallAimUpRightPose or SamusPoseId.RanIntoWallAimUpLeftPose or
        SamusPoseId.RanIntoWallAimDownRightPose or SamusPoseId.RanIntoWallAimDownLeftPose;

    /// <summary>True for right-facing aimed normal-jump landing <see cref="SamusPoseId.LandingAimUpRightPose"/>, <see cref="SamusPoseId.LandingAimDiagonalUpRightPose"/> and <see cref="SamusPoseId.LandingAimDiagonalDownRightPose"/>.</summary>
    public static bool IsRightFacingAimedLandingPose(SamusPoseId pose) => pose is
        SamusPoseId.LandingAimUpRightPose or SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.LandingAimDiagonalDownRightPose;

    /// <summary>
    /// True for the complete right-facing landing set that shares the ordinary standing
    /// transition table: normal, spin, aimed up, diagonally up and diagonally down, and firing.
    /// Centralizing this prevents a newly modelled landing from silently losing crouch,
    /// turn, run, or wall-probe exits in one of the several pose-change seams.
    /// </summary>
    public static bool IsRightFacingLandingPose(SamusPoseId pose) => pose is
        SamusPoseId.NormalLandingRightPose or SamusPoseId.SpinLandingRightPose or SamusPoseId.FiringLandingRightPose ||
        IsRightFacingAimedLandingPose(pose);

    /// <summary>True for the complete mirrored left-facing landing set.</summary>
    public static bool IsLeftFacingLandingPose(SamusPoseId pose) => IsRightFacingLandingPose(SamusPoseMirror.Of(pose));

    /// <summary>True for every landing, facing either way: the normal, spin, aimed and firing landings.</summary>
    public static bool IsLandingPose(SamusPoseId pose) => IsRightFacingLandingPose(pose) || IsLeftFacingLandingPose(pose);

    /// <summary>
    /// True when the ordinary grounded input step may interrupt a landing stream
    /// with any same-facing normal-jump transition, neutral or aimed.
    /// </summary>
    /// <remarks>
    /// Every landing pose above points at the ordinary standing input table. That includes
    /// the aimed and firing landing poses, not merely the older
    /// normal/spin subset. That standing table can preserve Up, diagonal-up, or
    /// diagonal-down shoulder input on the fresh Jump edge, so restricting the target to
    /// the neutral jump transition incorrectly rejects routes such as a right diagonal-up
    /// landing into the right diagonal-up jump transition. Keeping this
    /// relation beside the complete landing classifiers prevents individual runtime switch
    /// sites from accidentally admitting only part of the authored family.
    /// </remarks>
    public static bool IsLandingToNormalJumpTransition(SamusPoseId sourcePose, SamusPoseId targetPose) =>
        IsRightFacingLandingPose(sourcePose) && targetPose is
            SamusPoseId.NeutralJumpTransitionRightPose or
            SamusPoseId.NormalJumpTransitionAimUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose ||
        IsLeftFacingLandingPose(sourcePose) && targetPose is
            SamusPoseId.NeutralJumpTransitionLeftPose or
            SamusPoseId.NormalJumpTransitionAimUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
            SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the complete right-facing <see cref="SamusMovementType.Crouching"/> family. All four
    /// poses use radius 16 and the shared transition table.
    /// </summary>
    public static bool IsRightFacingCrouchingPose(SamusPoseId pose) => pose is
        SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingAimUpRightPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalDownRightPose;

    /// <summary>
    /// True for the complete left-facing <see cref="SamusMovementType.Crouching"/> family. All four
    /// poses use radius 16 and the mirrored table.
    /// </summary>
    public static bool IsLeftFacingCrouchingPose(SamusPoseId pose) => IsRightFacingCrouchingPose(SamusPoseMirror.Of(pose));

    /// <summary>True for the six crouching poses carrying a real shot direction.</summary>
    public static bool IsAimedCrouchingPose(SamusPoseId pose) => pose is
        SamusPoseId.CrouchingAimUpRightPose or SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalDownRightPose or SamusPoseId.CrouchingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for admitted grounded turning poses that began facing right and turn left.
    /// Their definition direction is already four (the destination facing), so callers
    /// must use this semantic grouping when preserving old rightward momentum.
    /// </summary>
    public static bool IsRightToLeftGroundTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftPose or SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose;

    /// <summary>True for admitted grounded turning poses that began facing left and turn right.</summary>
    public static bool IsLeftToRightGroundTurnPose(SamusPoseId pose) => IsRightToLeftGroundTurnPose(SamusPoseMirror.Of(pose));

    /// <summary>
    /// True for a authored standing turn interrupted by an ordinary Jump edge.
    /// </summary>
    /// <remarks>
    /// All eight turning-on-ground standing turn poses consult the same
    /// transition table while their art is active. Holding the completed facing selects
    /// the spin jump; releasing it on the Jump edge selects the neutral jump transition. Crouched turns are
    /// deliberately excluded because their radius expansion requires the room-aware
    /// crouch-jump collision path rather than the direct ordinary initializer.
    /// </remarks>
    public static bool IsStandingGroundTurnToJumpTransition(
        SamusPoseId sourcePose,
        SamusPoseId targetPose) =>
        (sourcePose is
            SamusPoseId.TurningLeftToRightPose or SamusPoseId.TurningLeftToRightAimUpPose or
            SamusPoseId.TurningLeftToRightAimDiagonalUpPose or SamusPoseId.TurningLeftToRightAimDiagonalDownPose &&
         targetPose is SamusPoseId.SpinJumpRightPose or SamusPoseId.NeutralJumpTransitionRightPose) ||
        (sourcePose is
            SamusPoseId.TurningRightToLeftPose or SamusPoseId.TurningRightToLeftAimUpPose or
            SamusPoseId.TurningRightToLeftAimDiagonalUpPose or SamusPoseId.TurningRightToLeftAimDiagonalDownPose &&
         targetPose is SamusPoseId.SpinJumpLeftPose or SamusPoseId.NeutralJumpTransitionLeftPose);

    /// <summary>
    /// True for the six aimed crouched-turn poses moved by the turning-while-jumping movement type.
    /// The unaimed crouching turns are intentionally absent because the retail pose definitions assign those
    /// otherwise similar-looking turns to the grounded turning movement type.
    /// </summary>
    public static bool IsAimedCrouchingTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>True for every turning-while-jumping normal-jump turn pose.</summary>
    public static bool IsJumpingTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftJumpPose or SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose;

    /// <summary>True for every turning-while-falling falling-turn pose.</summary>
    public static bool IsFallingTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftFallingPose or SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose;

    /// <summary>True for the sixteen normal-jump or falling turn poses of the two airborne turning movement types, excluding grounded and aimed crouching turns.</summary>
    /// <param name="pose">Current or prospective pose.</param>
    /// <returns>Whether either jumping-turn or falling-turn family contains the pose.</returns>
    public static bool IsAerialTurnPose(SamusPoseId pose) =>
        IsJumpingTurnPose(pose) || IsFallingTurnPose(pose);

    /// <summary>True for the eight jumping/falling turn poses that change from right to left, including their up, down, and diagonal-up aim variants.</summary>
    /// <param name="pose">Prospective or active turn pose; direction describes the facing change, not horizontal velocity.</param>
    /// <returns>Whether the pose is a right-to-left aerial turn admitted by the transition selector.</returns>
    public static bool IsRightToLeftAerialTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftJumpPose or SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingPose or SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose;

    /// <summary>True for the eight jumping/falling turn poses that change from left to right, including their up, down, and diagonal-up aim variants.</summary>
    /// <param name="pose">Prospective or active turn pose; direction describes the facing change, not horizontal velocity.</param>
    /// <returns>Whether the pose is a left-to-right aerial turn admitted by the transition selector.</returns>
    public static bool IsLeftToRightAerialTurnPose(SamusPoseId pose) => IsRightToLeftAerialTurnPose(SamusPoseMirror.Of(pose));

    /// <summary>
    /// Every pose some path of the game gives Samus: each pose except the three unauthored poses
    /// (<see cref="SamusUnauthoredPoseDefinitions.Poses"/>) and <see cref="SamusUnreachablePoseDefinitions.Poses"/> (#1513).
    /// Among them are the glitch balls, their knockbacks, the Draygon-held ball and its stand-up, which the Baby's Hyper
    /// Beam freeze and rainbow stop reach by a frame read past a transition's animation (#1535).
    /// </summary>
    internal static IReadOnlyList<SamusPoseId> ReachablePoses { get; } =
        [.. Enum.GetValues<SamusPoseId>().Where(pose => !SamusUnauthoredPoseDefinitions.Poses.TryRead(pose, out _))
            .Except(SamusUnreachablePoseDefinitions.Poses)];

    /// <summary>True for the four drained crouching and standing bodies that Mother Brain's controllers install.</summary>
    public static bool IsDrainedPose(SamusPoseId pose) => pose is
        SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or
        SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose;

    /// <summary>True for the two wall-jumping launch poses.</summary>
    public static bool IsWallJumpPose(SamusPoseId pose) => pose is SamusPoseId.WallJumpRightPose or SamusPoseId.WallJumpLeftPose;

    /// <summary>
    /// True for all six stable spin-jumping bodies. The transition tables use
    /// the ordinary spin jump as generic outputs; the spin-jump initializer can then substitute Space Jump or Screw
    /// Attack without changing the movement step.
    /// </summary>
    public static bool IsSpinJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose or
        SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose or
        SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose;

    /// <summary>True only for the two Screw Attack animation poses.</summary>
    public static bool IsScrewAttackPose(SamusPoseId pose) => pose is
        SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose;

    /// <summary>
    /// A respawning bomb block's collision breaks the block when the boost
    /// stage is active or Samus is screw attacking or shinesparking.
    /// </summary>
    public static bool BreaksCollisionBombBlocks(SamusState samus) =>
        samus.HorizontalSpeed.IsActivelySpeedBoosting ||
        IsScrewAttackPose(samus.Pose) ||
        IsShinesparkPose(samus.Pose);

    /// <summary>True only for the two Space Jump animation poses.</summary>
    public static bool IsSpaceJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose;

    /// <summary>
    /// True only for the admitted posture-transition crouch/stand animation poses.
    /// Morph/unmorph poses share that step but are intentionally not hidden here.
    /// </summary>
    public static bool IsCrouchStandTransitionPose(SamusPoseId pose) => pose is
        SamusPoseId.CrouchingTransitionRightPose or SamusPoseId.CrouchingTransitionLeftPose or
        SamusPoseId.StandingTransitionRightPose or SamusPoseId.StandingTransitionLeftPose or
        SamusPoseId.CrouchingTransitionAimUpRightPose or SamusPoseId.CrouchingTransitionAimUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalUpRightPose or SamusPoseId.CrouchingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingTransitionAimDiagonalDownRightPose or SamusPoseId.CrouchingTransitionAimDiagonalDownLeftPose or
        SamusPoseId.StandingTransitionAimUpRightPose or SamusPoseId.StandingTransitionAimUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalUpRightPose or SamusPoseId.StandingTransitionAimDiagonalUpLeftPose or
        SamusPoseId.StandingTransitionAimDiagonalDownRightPose or SamusPoseId.StandingTransitionAimDiagonalDownLeftPose;

    /// <summary>True for the four ordinary, non-Spring-Ball grounded morph poses.</summary>
    public static bool IsGroundedMorphBallPose(SamusPoseId pose) => pose is
        SamusPoseId.MorphBallGroundRightPose or SamusPoseId.MorphBallGroundLeftPose or
        SamusPoseId.MorphBallMovingRightPose or SamusPoseId.MorphBallMovingLeftPose;

    /// <summary>True for the two ordinary, non-Spring-Ball airborne morph poses.</summary>
    public static bool IsAirborneMorphBallPose(SamusPoseId pose) => pose is
        SamusPoseId.MorphBallFallingRightPose or SamusPoseId.MorphBallFallingLeftPose;

    /// <summary>True for the four grounded Spring Ball poses.</summary>
    public static bool IsGroundedSpringBallPose(SamusPoseId pose) => pose is
        SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingRightPose or SamusPoseId.SpringBallMovingLeftPose;

    /// <summary>True for Spring Ball jump/fall poses across the in-air and falling Spring Ball movement types.</summary>
    public static bool IsAirborneSpringBallPose(SamusPoseId pose) => pose is
        SamusPoseId.SpringBallFallingRightPose or SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose;

    /// <summary>True for every stable ordinary or Spring Ball pose using radius seven.</summary>
    public static bool IsStableBallPose(SamusPoseId pose) =>
        IsGroundedMorphBallPose(pose) || IsAirborneMorphBallPose(pose) ||
        IsGroundedSpringBallPose(pose) || IsAirborneSpringBallPose(pose);

    /// <summary>True for the four morph entry/exit poses handled by the posture-transition movement type.</summary>
    public static bool IsMorphTransitionPose(SamusPoseId pose) => pose is
        SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or
        SamusPoseId.UnmorphingTransitionRightPose or SamusPoseId.UnmorphingTransitionLeftPose;

    /// <summary>
    /// Publishes a bomb overlap's launch direction as a request Samus's pose handling has not yet accepted. The
    /// gameplay loop does this after the input phase; setup considers it after movement and hurt arbitration.
    /// </summary>
    public static void PublishBombJumpDirection(SamusState samus, SuperMetroid.Core.Samus.Projectiles.SamusBombJumpDirection direction) =>
        // Replaces the whole request, so a blast during an accepted jump publishes a fresh, unaccepted one.
        samus.BombJumpRequest = SamusBombJumpRequest.Published(direction);

    /// <summary>
    /// Executes the bomb-jump movement table's entry for Samus's movement before pose selection, dropping the request where
    /// the entry turns it down.
    /// </summary>
    /// <remarks>
    /// Standing and crouching reject the request only while time is frozen. Running,
    /// falling, moonwalking, wall-jump, wall-stop, and grapple families first select the
    /// ordinary forward-jump body. Ball, unused glitch-ball, and knockback-family
    /// entries retain their current pose. Jump/turn/transition/damage-boost and actor-owned
    /// families clear the published direction exactly like the table's rejecting entries.
    /// </remarks>
    /// <returns>True when the pending request was rejected and dropped.</returns>
    public static bool RejectUnsupportedPublishedBombJump(SamusState samus, bool timeIsFrozen)
    {
        if (!samus.BombJumpRequest.AwaitsPoseHandling)
            return false;
        SamusMovementType movement = ReadMovementKind(samus);
        bool reject = movement is SamusMovementType.NormalJumping or SamusMovementType.SpinJumping or
            SamusMovementType.TurningOnGround or SamusMovementType.PostureTransition or
            SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
            SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special ||
            timeIsFrozen && movement is (SamusMovementType.Standing or SamusMovementType.Crouching);
        if (reject) samus.BombJumpRequest = SamusBombJumpRequest.None;
        return reject;
    }

    /// <summary>Applies the admitted bomb-jump setup after the interruption's rejection side effects.</summary>
    /// <remarks>The newly pressed buttons feed the transition-shot test.</remarks>
    public static bool TrySetupPublishedBombJump(SamusState samus, 

        RoomLevelData level,
        bool timeIsFrozen,
        int frameCounter,
        ControllerButton controllerNewInput,
        ISamusRoomObjects? roomObjects = null)
    {

        Ensure.NotNull(level);
        if (RejectUnsupportedPublishedBombJump(samus, timeIsFrozen) || !samus.BombJumpRequest.AwaitsPoseHandling)
            return false;

        SamusMovementType movementType = ReadMovementKind(samus);
        switch (movementType)
        {
            case SamusMovementType.Standing:
            case SamusMovementType.Crouching:
                // Is the only setup branch gated by frozen time; it drops the request.
                if (timeIsFrozen)
                {
                    samus.BombJumpRequest = SamusBombJumpRequest.None;
                    return false;
                }
                goto case SamusMovementType.Running;

            case SamusMovementType.Running:
            case SamusMovementType.Falling:
            case SamusMovementType.CompactAirborne:
            case SamusMovementType.Moonwalking:
            case SamusMovementType.WallJumping:
            case SamusMovementType.RanIntoWall:
            case SamusMovementType.Grappling:
                // Tests the pose-definition direction, not the descriptive pose
                // number. Any value other than literal four follows the right-facing path.
                SamusPoseId sourcePose = samus.Pose;
                SamusPoseId targetPose = ReadFacingDirection(samus) == SamusFacingDirection.Left
                    ? SamusPoseId.NormalJumpForwardLeftPose
                    : SamusPoseId.NormalJumpForwardRightPose;

                // The pose update routes the special target through the same larger-radius
                // expansion collision used by controller-selected pose changes. This is
                // observable for a crouching bomb jump: radius 16 grows to 21 and may be
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
                samus.Kinematics.Position = samus.Kinematics.Position.WithWrappedWholeY(
                    samus.Kinematics.Position.Y.Whole + centerAdjustment);

                // The special target installs through the pose initializer, which runs the new
                // normal-jumping type to the normal-jump initializer (the forward jump cannot start a spark there). It
                // then restores the suit palette after a wall jump with Screw Attack.
                SamusAerialPoseTransitions.InitializeInstalledNormalJumpPose(samus, movementType, controllerNewInput);
                if (movementType == SamusMovementType.WallJumping &&
                    samus.EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
                    samus.HorizontalSpeed.RequestNormalSuitPaletteRestore();

                // The selected forward-jump poses are NOT the transition poses that
                // initialize ordinary jump speed (or apply the crouch jump offset).
                // Preserve this frame's velocity; bomb start replaces it next frame.
                SamusPresentation.InitializeAnimation(samus, initialFrame: 0);
                ArmPublishedBombJump(samus);
                return true;

            case SamusMovementType.MorphBallGround:
            case SamusMovementType.GlitchBall:
            case SamusMovementType.MorphBallFalling:
            case SamusMovementType.GlitchBallKnockback:
            case SamusMovementType.Knockback:
            case SamusMovementType.SpringBallGround:
            case SamusMovementType.SpringBallInAir:
            case SamusMovementType.SpringBallFalling:
                // Copies the current pose into the special prospective pose. Because
                // that pose already matches, Samus's pose handling immediately arms the bomb jump.
                ArmPublishedBombJump(samus);
                return true;

            case SamusMovementType.NormalJumping:
            case SamusMovementType.SpinJumping:
            case SamusMovementType.TurningOnGround:
            case SamusMovementType.PostureTransition:
            case SamusMovementType.TurningWhileJumping:
            case SamusMovementType.TurningWhileFalling:
            case SamusMovementType.DamageBoost:
            case SamusMovementType.DraygonHeld:
            case SamusMovementType.Special:
                samus.BombJumpRequest = SamusBombJumpRequest.None;
                return false;

            default:
                // The retail pose table never exceeds the special movement type; this is corrupt metadata,
                // analogous to indexing beyond the end of a movement-type table into unrelated data.
                throw new InvalidDataException(
                    $"Bomb-jump setup cannot run invalid movement type {movementType} for pose {samus.Pose}.");
        }
    }

    /// <summary>Arms the published bomb jump.</summary>
    internal static void ArmPublishedBombJump(SamusState samus)
    {
        // Arming the bomb jump takes, rather than suspends, Samus's movement.
        // A suit-interrupted spark may still hold it despite the ordinary body pose.
        // Keep its independently running palette/boost state, but never resume it
        // when the bomb arc returns her movement and input to her pose. The same
        // applies when suit pickup suspended Flash: its palette keeps running, but its
        // raising/drain must never resume after the bomb arc.
        SamusMovementHandOver.To(samus, SamusMovementOwner.BombJump);
        samus.InputSnapshot.ShinesparkPoseInputLocked = false;
        samus.InputSnapshot.CrystalFlashPoseInputLocked = false;
        samus.BombJumpRequest = samus.BombJumpRequest.Accept();
        samus.BombJumpStarting = true;
        samus.BombJumpActive = false;
        samus.BombJumpPoseInputLocked = true;
    }

    /// <summary>True for the admitted right-facing <see cref="SamusMovementType.NormalJumping"/> poses.</summary>
    public static bool IsRightFacingNormalJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.NeutralJumpTransitionRightPose or SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.NormalJumpForwardRightPose or SamusPoseId.NormalJumpAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpAimDiagonalUpRightPose or SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpAimDownRightPose;

    /// <summary>True for the admitted left-facing <see cref="SamusMovementType.NormalJumping"/> poses.</summary>
    public static bool IsLeftFacingNormalJumpPose(SamusPoseId pose) => IsRightFacingNormalJumpPose(SamusPoseMirror.Of(pose));

    /// <summary>True for admitted right-facing <see cref="SamusMovementType.Falling"/> poses.</summary>
    public static bool IsRightFacingFallingPose(SamusPoseId pose) => pose is
        SamusPoseId.FallingRightPose or SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingAimUpRightPose or
        SamusPoseId.FallingAimDiagonalUpRightPose or SamusPoseId.FallingAimDiagonalDownRightPose or
        SamusPoseId.FallingAimDownRightPose;

    /// <summary>True for admitted left-facing <see cref="SamusMovementType.Falling"/> poses.</summary>
    public static bool IsLeftFacingFallingPose(SamusPoseId pose) => IsRightFacingFallingPose(SamusPoseMirror.Of(pose));

    /// <summary>True for the aimed normal-jump/falling poses, including compact Down aim.</summary>
    public static bool IsAimedAerialPose(SamusPoseId pose) => pose is
        SamusPoseId.NormalJumpAimUpRightPose or SamusPoseId.NormalJumpAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimUpRightPose or SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpAimDiagonalUpRightPose or SamusPoseId.NormalJumpAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpAimDiagonalDownRightPose or SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.FallingAimUpRightPose or SamusPoseId.FallingAimUpLeftPose or
        SamusPoseId.FallingAimDiagonalUpRightPose or SamusPoseId.FallingAimDiagonalUpLeftPose or
        SamusPoseId.FallingAimDiagonalDownRightPose or SamusPoseId.FallingAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpAimDownRightPose or SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.FallingAimDownRightPose or SamusPoseId.FallingAimDownLeftPose;

    /// <summary>
    /// True for the forward jumps, the two normal-jumping bodies used while Samus
    /// has a same-facing horizontal direction held. These are ordinary aerial poses, not
    /// aimed or spinning poses; naming the pair prevents their transition semantics from
    /// being hidden inside another aim-specific condition.
    /// </summary>
    public static bool IsForwardMovingNormalJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.NormalJumpForwardRightPose or SamusPoseId.NormalJumpForwardLeftPose;

    /// <summary>
    /// Reports whether a same-facing normal-jump/falling pose change belongs to the shared
    /// aim, horizontal-fire, or moving-forward transition seam.
    /// </summary>
    /// <remarks>
    /// Deliberately share one input table among the neutral jump, the forward
    /// jump, and the non-compact aimed jump bodies. In particular, holding Jump after
    /// releasing Right/Left turns the forward jump into the neutral jump. Both endpoints must therefore
    /// participate in this classification. The previous target-only forward-jump check admitted
    /// entering the forward body but rejected leaving it, even though both routes execute the
    /// same pose/animation initializer and preserve live velocity.
    /// </remarks>
    public static bool IsSameFacingAerialAimFireOrForwardTransition(SamusPoseId source, SamusPoseId target)
    {
        bool hasTranslatedBodyChange =
            IsAimedAerialPose(source) || IsAimedAerialPose(target) ||
            IsGunExtendedPose(source) || IsGunExtendedPose(target) ||
            IsForwardMovingNormalJumpPose(source) ||
            IsForwardMovingNormalJumpPose(target);
        if (!hasTranslatedBodyChange)
            return false;

        return
            (IsRightFacingNormalJumpPose(source) && IsRightFacingNormalJumpPose(target)) ||
            (IsLeftFacingNormalJumpPose(source) && IsLeftFacingNormalJumpPose(target)) ||
            (IsRightFacingFallingPose(source) && IsRightFacingFallingPose(target)) ||
            (IsLeftFacingFallingPose(source) && IsLeftFacingFallingPose(target));
    }

    /// <summary>True only for the four radius-ten straight-down aerial bodies.</summary>
    public static bool IsCompactAerialPose(SamusPoseId pose) => pose is
        SamusPoseId.NormalJumpAimDownRightPose or SamusPoseId.NormalJumpAimDownLeftPose or
        SamusPoseId.FallingAimDownRightPose or SamusPoseId.FallingAimDownLeftPose;
}
