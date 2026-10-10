using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Pose metadata, pose-family queries, and compact-body transition setup.
/// </summary>
public sealed partial class SamusState
{
    /// <summary>
    /// Ports <c>Samus_SetRadius</c> at <c>$90:EC22</c>: every pose is five pixels wide and
    /// reads its vertical radius from pose-definition byte six.
    /// </summary>
    public void RefreshCollisionRadii(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        Kinematics.XRadius = 5;
        Kinematics.YRadius = ReadPoseYRadius(Pose);
    }

    /// <summary>Reads a prospective pose's radius without publishing it to live collision state.</summary>
    public static ushort ReadPoseYRadius(SamusPoseId pose) =>
        SamusPoseCollisionDefinitions.ReadVerticalRadius(pose);

    /// <summary>Reads pose-definition byte zero, the direction consumed by camera tracking.</summary>
    public byte ReadPoseXDirection(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ReadPoseXDirection(bus, InitializedPose);
    }

    /// <summary>
    /// Reads pose-definition byte zero for an arbitrary pose. Native transition
    /// initializers compare the old and prospective records before publishing the new
    /// pose, so making that distinction explicit avoids temporarily corrupting live state.
    /// </summary>
    public static byte ReadPoseXDirection(ISnesAddressSpace bus, SamusPoseId pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return SamusPoseDispatchDefinitions.ReadFacing(pose);
    }

    /// <summary>
    /// Typed view of pose-definition byte zero. Raw access remains available for ROM
    /// diagnostics and unknown records, while gameplay decisions no longer compare an
    /// unexplained byte to literals four and eight.
    /// </summary>
    public SamusFacingDirection ReadFacingDirection(ISnesAddressSpace bus) =>
        (SamusFacingDirection)ReadPoseXDirection(bus);

    /// <summary>Typed facing view for an arbitrary current or prospective pose.</summary>
    public static SamusFacingDirection ReadFacingDirection(ISnesAddressSpace bus, SamusPoseId pose) =>
        (SamusFacingDirection)ReadPoseXDirection(bus, pose);

    /// <summary>True only for the verified left-facing pose-definition value four.</summary>
    public bool IsFacingLeft(ISnesAddressSpace bus) =>
        ReadFacingDirection(bus) == SamusFacingDirection.Left;

    /// <summary>Left-facing query for an arbitrary pose without mutating live state.</summary>
    public static bool IsFacingLeft(ISnesAddressSpace bus, SamusPoseId pose) =>
        ReadFacingDirection(bus, pose) == SamusFacingDirection.Left;

    /// <summary>True only for the verified right-facing pose-definition value eight.</summary>
    public bool IsFacingRight(ISnesAddressSpace bus) =>
        ReadFacingDirection(bus) == SamusFacingDirection.Right;

    /// <summary>
    /// Reads and validates pose-definition byte one, the exclusive movement dispatcher.
    /// </summary>
    public SamusMovementType ReadMovementType(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ReadMovementType(bus, InitializedPose);
    }

    /// <summary>
    /// Compatibility spelling retained for callers that describe the value as a movement
    /// kind. Both APIs now return the same validated discriminator.
    /// </summary>
    public SamusMovementType ReadMovementKind(ISnesAddressSpace bus) =>
        ReadMovementType(bus);

    /// <summary>
    /// Reads pose-definition byte one for a prospective pose without mutating Samus.
    /// Values beyond the complete retail dispatcher are corrupt cartridge metadata and
    /// fail here, before they can be mistaken for a valid gameplay state downstream.
    /// </summary>
    public static SamusMovementType ReadMovementType(ISnesAddressSpace bus, SamusPoseId pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte rawMovementType = SamusPoseDispatchDefinitions.ReadMovement(pose);
        if (rawMovementType > (byte)SamusMovementType.Special)
        {
            throw new InvalidDataException(
                $"Pose ${(int)pose:X2} has invalid movement type ${rawMovementType:X2}; " +
                $"the retail dispatcher ends at ${(byte)SamusMovementType.Special:X2}.");
        }

        return (SamusMovementType)rawMovementType;
    }

    /// <summary>Compatibility spelling for an arbitrary prospective pose.</summary>
    public static SamusMovementType ReadMovementKind(ISnesAddressSpace bus, SamusPoseId pose) =>
        ReadMovementType(bus, pose);

    /// <summary>
    /// Reads pose-definition byte two, the pose selected by <c>$91:82D9</c> when no
    /// controller bits are held and the transition table therefore is not consulted.
    /// </summary>
    public SamusPoseId ReadNoInputFallbackPose(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return SamusPoseDispatchDefinitions.ReadNoInputPose(Pose);
    }

    /// <summary>
    /// Reads pose-definition byte three, the ten-way arm-cannon direction consumed by
    /// the native normal-jump landing selector at <c>$91:E974</c>.
    /// </summary>
    public byte ReadShotDirection(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ReadShotDirection(bus, Pose);
    }

    /// <summary>Reads pose-definition byte three for a current or prospective pose.</summary>
    public static byte ReadShotDirection(ISnesAddressSpace bus, SamusPoseId pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return SamusPoseAimDefinitions.Read(pose);
    }

    /// <summary>
    /// Reads signed pose-definition byte four for presentation, including Grapple's flare.
    /// Physical beam/Grapple origins use their separate compiled correction; replacing
    /// artwork must not shift a projectile's collision position.
    /// </summary>
    public sbyte ReadGraphicsYOffset(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return (TileTransfers.Artwork ?? throw new InvalidOperationException(
            "Samus pose presentation requires installed body artwork."))
            .GraphicsYOffset(Pose);
    }

    /// <summary>True for the four movement-type-zero, right-facing standing poses.</summary>
    public static bool IsRightFacingStandingPose(SamusPoseId pose) => pose is
        SamusPoseId.FacingRightNormalPose or
        SamusPoseId.StandingAimUpRightPose or
        SamusPoseId.StandingAimDiagonalUpRightPose or
        SamusPoseId.StandingAimDiagonalDownRightPose;

    /// <summary>True for the four movement-type-zero, left-facing standing poses.</summary>
    public static bool IsLeftFacingStandingPose(SamusPoseId pose) => pose is
        SamusPoseId.FacingLeftNormalPose or
        SamusPoseId.StandingAimUpLeftPose or
        SamusPoseId.StandingAimDiagonalUpLeftPose or
        SamusPoseId.StandingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the two front-view records selected by <c>MakeSamusFaceForward</c> at
    /// `$91:E3F6`. Their pose-X direction is zero and must never be guessed as left/right.
    /// </summary>
    public static bool IsForwardFacingPose(SamusPoseId pose) => pose is
        SamusPoseId.ForwardFacingPowerSuitPose or SamusPoseId.ForwardFacingSuitedPose;

    /// <summary>
    /// Selects the suit's front-view pose as <c>$91:E3FD-$91:E415</c> and
    /// <c>$90:F23C-$90:F271</c> do. Gravity bit <c>$0020</c> and Varia bit <c>$0001</c>
    /// have equal precedence: either selects the shared suited pose <c>$9B</c>.
    /// </summary>
    private void SelectForwardFacingPose()
    {
        Pose = EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit)
            ? SamusPoseId.ForwardFacingSuitedPose
            : SamusPoseId.ForwardFacingPowerSuitPose;
        AnimationFrame = 0;
    }

    /// <summary>
    /// Applies the pose part of Samus command 9, <c>SetupSamusForZebesStart</c> at
    /// <c>$90:F23C</c>: the suit's front-view pose and its initialization. Unlike
    /// <see cref="ApplyForwardFacingPoseSetup"/> it neither shifts pose history, lifts
    /// Samus nor clears motion. Callers own the palette object and the special frame.
    /// </summary>
    public void ApplyZebesStartPoseSetup(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        SelectForwardFacingPose();
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Samus command 7, <c>SetupSamusForElevator</c> ($90:F1C8): MakeSamusFaceForward, then
    /// the riding-elevator handlers. Used on boarding ($A3:9548) and on a downward
    /// arrival's door finish ($82:E721).
    /// </summary>
    public void SetupForElevator(ISnesAddressSpace bus)
    {
        ApplyForwardFacingPoseSetup(bus);
        // Command seven replaces both physical movement/input pointers. An uncrashed
        // spark keeps its boost counter as the out-of-bounds elevator Blue Suit, while an
        // admitted Flash keeps only its independent palette timer. Neither movement owner
        // may resume after elevator travel; the command restores ordinary pose input.
        Shinespark.RelinquishMovementHandler();
        ShinesparkPoseInputLocked = false;
        CrystalFlash.RelinquishMovementHandler();
        CrystalFlashPoseInputLocked = false;
        InputLocked = true;
        PrimeGraphics(bus);
    }

    /// <summary>
    /// Applies the movement/animation subset of <c>MakeSamusFaceForward</c> at
    /// `$91:E3F6-$91:E4A5`.
    /// </summary>
    /// <remarks>
    /// The native routine also locks the global current/new-state handlers, kills grapple,
    /// clears beam-flare presentation words, and reloads the suit palette. Those owners do
    /// not live in this state object. This method deliberately covers only the state it can
    /// own exactly: equipment-selected pose, its delay list, the radius-dependent lift and
    /// all motion words cleared by the routine. It does not write the collision radius;
    /// the next alpha's SetSamusRadius does. Runtime/debug callers remain responsible for
    /// input lock, palette, grapple, and priming the first graphics DMA before their first
    /// visible NMI.
    /// </remarks>
    public void ApplyForwardFacingPoseSetup(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        SelectForwardFacingPose();
        InitializeAnimation(bus, initialFrame: 0);

        // This forced owner shifts history even when already facing forward.
        // It does not pass through the ordinary input-transition epilogue.
        CommitPoseHistory(bus);

        // `$91:E438-$91:E44A` compares the live radius, still the previous pose's, and
        // lifts both Y words by three unless it is already the front view's 24.
        if (Kinematics.YRadius != 0x18)
        {
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 3));
            WritePreviousYPosition(Kinematics.YPosition);
        }

        HorizontalSpeed.ExtraRunSpeed = 0;
        HorizontalSpeed.ExtraRunSubspeed = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        HorizontalSpeed.AccelerationMode = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YDirection = 0;
        MorphBallBounceState = 0;
    }

    /// <summary>
    /// True for all five live movement-type-one, right-moving pose-table entries. `$0B`
    /// belongs here even though its name describes firing: bank `$90` dispatches its body
    /// through exactly the same running physics as `$09/$0D/$0F/$11`.
    /// </summary>
    public static bool IsRightFacingRunningPose(SamusPoseId pose) => pose is
        SamusPoseId.MovingRightNormalPose or SamusPoseId.MovingRightGunExtendedPose or
        SamusPoseId.RunningAimUpRightPose or
        SamusPoseId.RunningAimDiagonalUpRightPose or
        SamusPoseId.RunningAimDiagonalDownRightPose;

    /// <summary>True for all five live movement-type-one, left-moving pose-table entries.</summary>
    public static bool IsLeftFacingRunningPose(SamusPoseId pose) => pose is
        SamusPoseId.MovingLeftNormalPose or SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.RunningAimUpLeftPose or
        SamusPoseId.RunningAimDiagonalUpLeftPose or
        SamusPoseId.RunningAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the six ordinary-play bodies whose names explicitly say “gun extended.”
    /// Firing landings `$E6/$E7` are kept separate because they are short transition art,
    /// not stable horizontal-fire bodies selected directly by an input table.
    /// </summary>
    public static bool IsGunExtendedPose(SamusPoseId pose) => pose is
        SamusPoseId.MovingRightGunExtendedPose or SamusPoseId.MovingLeftGunExtendedPose or
        SamusPoseId.NormalJumpGunExtendedRightPose or SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingGunExtendedLeftPose;

    /// <summary>
    /// True for the three poses whose artwork faces left while the type-$10 body travels
    /// right. The naming is intentionally visual; <c>ReadPoseXDirection</c> returns eight.
    /// </summary>
    public static bool IsMoonwalkingFacingLeftPose(SamusPoseId pose) => pose is
        SamusPoseId.MoonwalkFacingLeftPose or SamusPoseId.MoonwalkAimUpLeftPose or SamusPoseId.MoonwalkAimDownLeftPose;

    /// <summary>True for the three right-facing moonwalk poses that travel left.</summary>
    public static bool IsMoonwalkingFacingRightPose(SamusPoseId pose) => pose is
        SamusPoseId.MoonwalkFacingRightPose or SamusPoseId.MoonwalkAimUpRightPose or SamusPoseId.MoonwalkAimDownRightPose;

    /// <summary>True for the six neutral or aimed moonwalk records in movement type $10, whose artwork faces opposite their travel direction.</summary>
    /// <param name="pose">Current or prospective native pose byte; no pose metadata or live state is changed.</param>
    /// <returns>Whether the pose belongs to either visual-facing moonwalk family.</returns>
    public static bool IsMoonwalkingPose(SamusPoseId pose) =>
        IsMoonwalkingFacingLeftPose(pose) || IsMoonwalkingFacingRightPose(pose);

    /// <summary>True for all five left-facing movement-type-`$1A` Draygon poses.</summary>
    public static bool IsLeftFacingDraygonGrabbedPose(SamusPoseId pose) => pose is
        SamusPoseId.DraygonGrabbedNeutralLeftPose or SamusPoseId.DraygonGrabbedAimUpLeftPose or
        SamusPoseId.DraygonGrabbedFiringLeftPose or SamusPoseId.DraygonGrabbedAimDownLeftPose or
        SamusPoseId.DraygonGrabbedMovingLeftPose;

    /// <summary>True for all five right-facing movement-type-`$1A` Draygon poses.</summary>
    public static bool IsRightFacingDraygonGrabbedPose(SamusPoseId pose) => pose is
        SamusPoseId.DraygonGrabbedNeutralRightPose or SamusPoseId.DraygonGrabbedAimUpRightPose or
        SamusPoseId.DraygonGrabbedFiringRightPose or SamusPoseId.DraygonGrabbedAimDownRightPose or
        SamusPoseId.DraygonGrabbedMovingRightPose;

    /// <summary>True for the complete ten-pose grabbed-by-Draygon family.</summary>
    public static bool IsDraygonGrabbedPose(SamusPoseId pose) =>
        IsLeftFacingDraygonGrabbedPose(pose) || IsRightFacingDraygonGrabbedPose(pose);

    /// <summary>
    /// Applies one ordinary `$91:AE18/$AE56` transition without inventing pose physics.
    /// Every admitted record stays within one facing, retains radius 21, and restarts the
    /// target's cartridge delay list at byte zero through the normal pose initializer.
    /// </summary>
    public void ApplyDraygonGrabbedPoseChange(ISnesAddressSpace bus, SamusPoseId targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);

        bool leftFamily = IsLeftFacingDraygonGrabbedPose(Pose) &&
            IsLeftFacingDraygonGrabbedPose(targetPose);
        bool rightFamily = IsRightFacingDraygonGrabbedPose(Pose) &&
            IsRightFacingDraygonGrabbedPose(targetPose);
        if (!leftFamily && !rightFamily)
        {
            // `$91:AE18` contains only `$BA-$BE`; `$91:AE56` contains only `$EC-$F0`.
            // Neither controller-input table can cross facing families. Draygon's owner AI
            // performs facing changes by re-entering its grabbed controller directly, so a
            // cross-family request to this table-transition API is a caller contract error,
            // not an omitted input record.
            throw new InvalidOperationException(
                $"Draygon-grabbed input transition ${(int)Pose:X2} -> ${(int)targetPose:X2} crosses owner-controlled facing families.");
        }

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius != oldRadius)
        {
            throw new InvalidDataException(
                $"Draygon pose ${(int)targetPose:X2} unexpectedly changed radius {oldRadius} -> {Kinematics.YRadius}.");
        }

        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// True for `$BF-$C4`, the six grounded turn frames selected only when Jump begins a
    /// direction reversal from movement type `$10`. Odd/even descriptive names follow the
    /// literal pose records, while the predicates below group them by destination facing.
    /// </summary>
    public static bool IsMoonwalkTurnJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.MoonwalkTurnJumpLeftPose or SamusPoseId.MoonwalkTurnJumpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose or SamusPoseId.MoonwalkTurnJumpAimDownRightPose;

    /// <summary>True for the `$BF/$C1/$C3` family whose transition tables lead to left.</summary>
    public static bool IsMoonwalkTurnJumpLeftPose(SamusPoseId pose) => pose is
        SamusPoseId.MoonwalkTurnJumpLeftPose or SamusPoseId.MoonwalkTurnJumpAimUpLeftPose or
        SamusPoseId.MoonwalkTurnJumpAimDownLeftPose;

    /// <summary>True for the `$C0/$C2/$C4` family whose transition tables lead to right.</summary>
    public static bool IsMoonwalkTurnJumpRightPose(SamusPoseId pose) => pose is
        SamusPoseId.MoonwalkTurnJumpRightPose or SamusPoseId.MoonwalkTurnJumpAimUpRightPose or
        SamusPoseId.MoonwalkTurnJumpAimDownRightPose;

    /// <summary>True for `$89/$CF/$D1`, the complete right-facing type-`$15` family.</summary>
    public static bool IsRightFacingRanIntoWallPose(SamusPoseId pose) => pose is
        SamusPoseId.RanIntoWallRightPose or SamusPoseId.RanIntoWallAimUpRightPose or SamusPoseId.RanIntoWallAimDownRightPose;

    /// <summary>True for `$8A/$D0/$D2`, the complete left-facing type-`$15` family.</summary>
    public static bool IsLeftFacingRanIntoWallPose(SamusPoseId pose) => pose is
        SamusPoseId.RanIntoWallLeftPose or SamusPoseId.RanIntoWallAimUpLeftPose or SamusPoseId.RanIntoWallAimDownLeftPose;

    /// <summary>True for the six movement-type-$15 wall-contact records: neutral and the two aimed variants for each facing.</summary>
    /// <param name="pose">Current or prospective native pose byte.</param>
    /// <returns>Whether the pose is a supported standing body pressed against a wall, rather than a wall-jump body.</returns>
    public static bool IsRanIntoWallPose(SamusPoseId pose) =>
        IsRightFacingRanIntoWallPose(pose) || IsLeftFacingRanIntoWallPose(pose);

    /// <summary>True for the four aimed wall-contact records $CF..$D2; neutral wall-contact poses $89/$8A are excluded.</summary>
    /// <param name="pose">Current or prospective native pose byte.</param>
    /// <returns>Whether the movement-type-$15 body uses an aimed wall-contact variant.</returns>
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

    /// <summary>True for right-facing aimed normal-jump landing poses `$E0/$E2/$E4`.</summary>
    public static bool IsRightFacingAimedLandingPose(SamusPoseId pose) => pose is
        SamusPoseId.LandingAimUpRightPose or SamusPoseId.LandingAimDiagonalUpRightPose or
        SamusPoseId.LandingAimDiagonalDownRightPose;

    /// <summary>True for left-facing aimed normal-jump landing poses `$E1/$E3/$E5`.</summary>
    public static bool IsLeftFacingAimedLandingPose(SamusPoseId pose) => pose is
        SamusPoseId.LandingAimUpLeftPose or SamusPoseId.LandingAimDiagonalUpLeftPose or
        SamusPoseId.LandingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the complete right-facing landing set that shares the ordinary standing
    /// transition table: normal `$A4`, spin `$A6`, aimed `$E0/$E2/$E4`, and firing `$E6`.
    /// Centralizing this prevents a newly translated landing from silently losing crouch,
    /// turn, run, or wall-probe exits in one of the several native pose-change seams.
    /// </summary>
    public static bool IsRightFacingLandingPose(SamusPoseId pose) => pose is
        SamusPoseId.NormalLandingRightPose or SamusPoseId.SpinLandingRightPose or SamusPoseId.FiringLandingRightPose ||
        IsRightFacingAimedLandingPose(pose);

    /// <summary>True for the complete mirrored left-facing landing set.</summary>
    public static bool IsLeftFacingLandingPose(SamusPoseId pose) => pose is
        SamusPoseId.NormalLandingLeftPose or SamusPoseId.SpinLandingLeftPose or SamusPoseId.FiringLandingLeftPose ||
        IsLeftFacingAimedLandingPose(pose);

    /// <summary>
    /// True when the ordinary grounded input dispatcher may interrupt a landing stream
    /// with any same-facing normal-jump transition `$4B/$4C/$55-$5A`.
    /// </summary>
    /// <remarks>
    /// Every landing pose above points at the ordinary standing input table. That includes
    /// the aimed `$E0-$E5` and firing `$E6/$E7` records, not merely the older `$A4-$A7`
    /// normal/spin subset. That standing table can preserve Up, diagonal-up, or
    /// diagonal-down shoulder input on the fresh Jump edge, so restricting the target to
    /// neutral `$4B/$4C` incorrectly rejects routes such as `$E2 -> $57`. Keeping this
    /// relation beside the complete landing classifiers prevents individual runtime switch
    /// sites from accidentally admitting only part of the ROM-authored family.
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
    /// True for the complete right-facing movement-type-five crouch family. All four
    /// records use radius 16 and the shared transition table at <c>$91:A66C</c>.
    /// </summary>
    public static bool IsRightFacingCrouchingPose(SamusPoseId pose) => pose is
        SamusPoseId.CrouchingRightPose or SamusPoseId.CrouchingAimUpRightPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalDownRightPose;

    /// <summary>
    /// True for the complete left-facing movement-type-five crouch family. All four
    /// records use radius 16 and the mirrored table at <c>$91:A6BC</c>.
    /// </summary>
    public static bool IsLeftFacingCrouchingPose(SamusPoseId pose) => pose is
        SamusPoseId.CrouchingLeftPose or SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpLeftPose or SamusPoseId.CrouchingAimDiagonalDownLeftPose;

    /// <summary>True for the six crouching poses carrying a real shot direction.</summary>
    public static bool IsAimedCrouchingPose(SamusPoseId pose) => pose is
        SamusPoseId.CrouchingAimUpRightPose or SamusPoseId.CrouchingAimUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalUpRightPose or SamusPoseId.CrouchingAimDiagonalUpLeftPose or
        SamusPoseId.CrouchingAimDiagonalDownRightPose or SamusPoseId.CrouchingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for admitted grounded `$0E/$17` poses that began facing right and turn left.
    /// Their definition direction is already four (the destination facing), so callers
    /// must use this semantic grouping when preserving old rightward momentum.
    /// </summary>
    public static bool IsRightToLeftGroundTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftPose or SamusPoseId.TurningRightToLeftAimUpPose or
        SamusPoseId.TurningRightToLeftAimDiagonalUpPose or SamusPoseId.TurningRightToLeftAimDiagonalDownPose or
        SamusPoseId.TurningRightToLeftCrouchingPose or SamusPoseId.TurningRightToLeftCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose;

    /// <summary>True for admitted grounded `$0E/$17` poses that began facing left and turn right.</summary>
    public static bool IsLeftToRightGroundTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningLeftToRightPose or SamusPoseId.TurningLeftToRightAimUpPose or
        SamusPoseId.TurningLeftToRightAimDiagonalUpPose or SamusPoseId.TurningLeftToRightAimDiagonalDownPose or
        SamusPoseId.TurningLeftToRightCrouchingPose or SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>
    /// True for a cartridge-authored standing turn interrupted by an ordinary Jump edge.
    /// </summary>
    /// <remarks>
    /// All eight movement-type-$0E standing turn records consult the same `$91:8142`
    /// transition table while their art is active. Holding the completed facing selects
    /// `$19/$1A`; releasing it on the Jump edge selects `$4B/$4C`. Crouched turns are
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
    /// True for the six aimed crouched-turn records dispatched through movement type `$17`.
    /// `$43/$44` are intentionally absent because the retail pose definitions assign those
    /// otherwise similar-looking unaimed crouch turns to grounded movement type `$0E`.
    /// </summary>
    public static bool IsAimedCrouchingTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftCrouchingAimUpPose or SamusPoseId.TurningLeftToRightCrouchingAimUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftCrouchingAimDiagonalDownPose or SamusPoseId.TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>True for every movement-type-$17 normal-jump turn record.</summary>
    public static bool IsJumpingTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftJumpPose or SamusPoseId.TurningLeftToRightJumpPose or
        SamusPoseId.TurningRightToLeftJumpAimUpPose or SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or SamusPoseId.TurningLeftToRightJumpAimDownPose or
        SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose;

    /// <summary>True for every movement-type-$18 falling-turn record.</summary>
    public static bool IsFallingTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftFallingPose or SamusPoseId.TurningLeftToRightFallingPose or
        SamusPoseId.TurningRightToLeftFallingAimUpPose or SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or SamusPoseId.TurningLeftToRightFallingAimDownPose or
        SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose or SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose;

    /// <summary>True for the sixteen normal-jump or falling turn records in movement types $17/$18, excluding grounded and aimed crouching turns.</summary>
    /// <param name="pose">Current or prospective native pose byte.</param>
    /// <returns>Whether either jumping-turn or falling-turn family contains the pose.</returns>
    public static bool IsAerialTurnPose(SamusPoseId pose) =>
        IsJumpingTurnPose(pose) || IsFallingTurnPose(pose);

    /// <summary>True for the eight jumping/falling turn records that change from right to left, including their up, down, and diagonal-up aim variants.</summary>
    /// <param name="pose">Prospective or active turn pose; direction describes the facing change, not horizontal velocity.</param>
    /// <returns>Whether the pose is a right-to-left aerial turn admitted by the transition selector.</returns>
    public static bool IsRightToLeftAerialTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningRightToLeftJumpPose or SamusPoseId.TurningRightToLeftJumpAimUpPose or
        SamusPoseId.TurningRightToLeftJumpAimDownPose or SamusPoseId.TurningRightToLeftJumpAimDiagonalUpPose or
        SamusPoseId.TurningRightToLeftFallingPose or SamusPoseId.TurningRightToLeftFallingAimUpPose or
        SamusPoseId.TurningRightToLeftFallingAimDownPose or SamusPoseId.TurningRightToLeftFallingAimDiagonalUpPose;

    /// <summary>True for the eight jumping/falling turn records that change from left to right, including their up, down, and diagonal-up aim variants.</summary>
    /// <param name="pose">Prospective or active turn pose; direction describes the facing change, not horizontal velocity.</param>
    /// <returns>Whether the pose is a left-to-right aerial turn admitted by the transition selector.</returns>
    public static bool IsLeftToRightAerialTurnPose(SamusPoseId pose) => pose is
        SamusPoseId.TurningLeftToRightJumpPose or SamusPoseId.TurningLeftToRightJumpAimUpPose or
        SamusPoseId.TurningLeftToRightJumpAimDownPose or SamusPoseId.TurningLeftToRightJumpAimDiagonalUpPose or
        SamusPoseId.TurningLeftToRightFallingPose or SamusPoseId.TurningLeftToRightFallingAimUpPose or
        SamusPoseId.TurningLeftToRightFallingAimDownPose or SamusPoseId.TurningLeftToRightFallingAimDiagonalUpPose;

    /// <summary>True for the four drained bodies $E8-$EB that Mother Brain's controllers install.</summary>
    public static bool IsDrainedPose(SamusPoseId pose) => pose is
        SamusPoseId.DrainedCrouchingRightPose or SamusPoseId.DrainedCrouchingLeftPose or
        SamusPoseId.DrainedStandingRightPose or SamusPoseId.DrainedStandingLeftPose;

    /// <summary>True for the two movement-type-$14 wall-jump launch records.</summary>
    public static bool IsWallJumpPose(SamusPoseId pose) => pose is SamusPoseId.WallJumpRightPose or SamusPoseId.WallJumpLeftPose;

    /// <summary>
    /// True for all six stable movement-type-three spin bodies. The transition tables use
    /// `$19/$1A` as generic outputs; `$91:F624` can then substitute Space Jump or Screw
    /// Attack without changing the movement dispatcher.
    /// </summary>
    public static bool IsSpinJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.SpinJumpRightPose or SamusPoseId.SpinJumpLeftPose or
        SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose or
        SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose;

    /// <summary>True only for the two Screw Attack animation records `$81/$82`.</summary>
    public static bool IsScrewAttackPose(SamusPoseId pose) => pose is
        SamusPoseId.ScrewAttackRightPose or SamusPoseId.ScrewAttackLeftPose;

    /// <summary>
    /// <c>Setup_Collision_RespawningBombBlock</c> ($84:CE83) breaks the block when the boost
    /// stage is active or Samus is screw attacking ($81/$82) or shinesparking ($C9-$CE).
    /// </summary>
    public bool BreaksCollisionBombBlocks =>
        HorizontalSpeed.IsActivelySpeedBoosting ||
        IsScrewAttackPose(Pose) ||
        Pose is >= SamusPoseId.ShinesparkHorizontalRightPose
            and <= SamusPoseId.ShinesparkDiagonalLeftPose;

    /// <summary>True only for the two Space Jump animation records `$1B/$1C`.</summary>
    public static bool IsSpaceJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.SpaceJumpRightPose or SamusPoseId.SpaceJumpLeftPose;

    /// <summary>
    /// True only for the admitted movement-type-$0F crouch/stand animation records.
    /// Morph/unmorph records share that dispatcher but are intentionally not hidden here.
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

    /// <summary>True for movement type $11's four grounded Spring Ball poses.</summary>
    public static bool IsGroundedSpringBallPose(SamusPoseId pose) => pose is
        SamusPoseId.SpringBallGroundRightPose or SamusPoseId.SpringBallGroundLeftPose or
        SamusPoseId.SpringBallMovingRightPose or SamusPoseId.SpringBallMovingLeftPose;

    /// <summary>True for Spring Ball jump/fall poses across movement types $12/$13.</summary>
    public static bool IsAirborneSpringBallPose(SamusPoseId pose) => pose is
        SamusPoseId.SpringBallFallingRightPose or SamusPoseId.SpringBallFallingLeftPose or
        SamusPoseId.SpringBallJumpRightPose or SamusPoseId.SpringBallJumpLeftPose;

    /// <summary>True for every stable ordinary or Spring Ball pose using radius seven.</summary>
    public static bool IsStableBallPose(SamusPoseId pose) =>
        IsGroundedMorphBallPose(pose) || IsAirborneMorphBallPose(pose) ||
        IsGroundedSpringBallPose(pose) || IsAirborneSpringBallPose(pose);

    /// <summary>True for the four entry/exit poses handled by movement type $0F.</summary>
    public static bool IsMorphTransitionPose(SamusPoseId pose) => pose is
        SamusPoseId.MorphingTransitionRightPose or SamusPoseId.MorphingTransitionLeftPose or
        SamusPoseId.UnmorphingTransitionRightPose or SamusPoseId.UnmorphingTransitionLeftPose;

    /// <summary>
    /// Stores only bank-$A0's low-byte bomb direction. The gameplay loop does this after
    /// frame-handler alpha; setup considers it after movement and hurt arbitration.
    /// </summary>
    public void PublishBombJumpDirection(byte direction)
    {
        if (direction is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "Bomb-jump direction must be left, straight, or right.");
        }

        // $A0:984B writes the complete word, not just its low byte. A timer-eight overlap
        // therefore publishes exactly $0001/$0002/$0003.
        BombJumpDirection = direction;
    }

    /// <summary>
    /// Executes the carry-clear branches of the bomb-jump movement table before pose selection.
    /// </summary>
    /// <remarks>
    /// Standing and crouching reject the request only while time is frozen. Running,
    /// falling, moonwalking, wall-jump, wall-stop, and grapple families first select the
    /// ordinary forward-jump body `$51/$52`. Ball, unused `$07/$09`, and knockback-family
    /// entries retain their current pose. Jump/turn/transition/damage-boost and actor-owned
    /// families clear the published direction exactly like the table's carry-clear routines.
    /// </remarks>
    /// <returns>True when the pending low-byte direction was rejected and cleared.</returns>
    public bool RejectUnsupportedPublishedBombJump(ISnesAddressSpace bus, bool timeIsFrozen)
    {
        if (BombJumpDirection == 0 || (BombJumpDirection & 0xff00) != 0)
            return false;
        SamusMovementType movement = ReadMovementKind(bus);
        bool reject = movement is SamusMovementType.NormalJumping or SamusMovementType.SpinJumping or
            SamusMovementType.TurningOnGround or SamusMovementType.PostureTransition or
            SamusMovementType.TurningWhileJumping or SamusMovementType.TurningWhileFalling or
            SamusMovementType.DamageBoost or SamusMovementType.DraygonHeld or SamusMovementType.Special ||
            timeIsFrozen && movement is (SamusMovementType.Standing or SamusMovementType.Crouching);
        if (reject) BombJumpDirection = 0;
        return reject;
    }

    /// <summary>Applies the admitted bomb-jump setup after the interruption's rejection side effects.</summary>
    /// <remarks>The newly pressed buttons feed the transition-shot test at $91:F5CF.</remarks>
    public bool TrySetupPublishedBombJump(
        ISnesAddressSpace bus,
        RoomLevelData level,
        bool timeIsFrozen,
        ushort nmiFrameCounter,
        ushort controllerNewInput,
        RoomPlmSystem? plms = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        if (RejectUnsupportedPublishedBombJump(bus, timeIsFrozen) ||
            BombJumpDirection == 0 || (BombJumpDirection & 0xff00) != 0)
            return false;

        SamusMovementType movementType = ReadMovementKind(bus);
        switch (movementType)
        {
            case SamusMovementType.Standing:
            case SamusMovementType.Crouching:
                // `$90:DFED` is the only setup branch gated by frozen time. Its clear is a
                // complete 16-bit STZ, so no high-byte provenance survives rejection.
                if (timeIsFrozen)
                {
                    BombJumpDirection = 0;
                    return false;
                }
                goto case SamusMovementType.Running;

            case SamusMovementType.Running:
            case SamusMovementType.Falling:
            case SamusMovementType.Unused0B:
            case SamusMovementType.Unused0C:
            case SamusMovementType.Unused0D:
            case SamusMovementType.Moonwalking:
            case SamusMovementType.WallJumping:
            case SamusMovementType.RanIntoWall:
            case SamusMovementType.Grappling:
                // `$90:DFF7` tests the pose-definition direction, not the descriptive pose
                // number. Any value other than literal four follows the right-facing path.
                SamusPoseId sourcePose = Pose;
                SamusPoseId targetPose = ReadPoseXDirection(bus) == (byte)SamusFacingDirection.Left
                    ? SamusPoseId.NormalJumpForwardLeftPose
                    : SamusPoseId.NormalJumpForwardRightPose;

                // UpdateSamusPose routes the special target through the same `$91:FDAE`
                // expansion collision used by controller-selected pose changes. This is
                // observable for a crouching bomb jump: radius 16 grows to 21 and may be
                // rejected by a low ceiling instead of clipping Samus into terrain.
                LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                    bus,
                    level,
                    targetPose,
                    nmiFrameCounter,
                    plms,
                    out int centerAdjustment);
                if (collision != LargerPoseCollisionOutcome.Allowed)
                {
                    if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                        ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
                    return false;
                }

                Pose = targetPose;
                RefreshCollisionRadii(bus);
                Kinematics.YPosition = unchecked((ushort)(
                    Kinematics.YPosition + centerAdjustment));

                // The special target installs through $91:F433, which dispatches the new
                // normal-jumping type to $91:F543 ($51/$52 cannot start a spark there). $F433
                // then restores the suit palette after a wall jump with Screw Attack.
                InitializeInstalledNormalJumpPose(bus, movementType, controllerNewInput);
                if (movementType == SamusMovementType.WallJumping &&
                    EquippedItems.HasAny(SamusEquipmentFlags.ScrewAttack))
                    HorizontalSpeed.RequestNormalSuitPaletteRestore();

                // The selected forward-jump poses are NOT the transition poses that
                // initialize ordinary jump speed (or apply the crouch jump offset).
                // Preserve this frame's velocity; bomb start replaces it next frame.
                InitializeAnimation(bus, initialFrame: 0);
                ArmPublishedBombJump();
                return true;

            case SamusMovementType.MorphBallGround:
            case SamusMovementType.UnusedGlitchBall:
            case SamusMovementType.MorphBallFalling:
            case SamusMovementType.UnusedGlitchBallAlternate:
            case SamusMovementType.Knockback:
            case SamusMovementType.SpringBallGround:
            case SamusMovementType.SpringBallInAir:
            case SamusMovementType.SpringBallFalling:
                // `$90:E012` copies the current pose into SpecialProspectivePose. Because
                // that pose already matches, bank $91 immediately executes command three.
                ArmPublishedBombJump();
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
                BombJumpDirection = 0;
                return false;

            default:
                // The retail pose table never exceeds `$1B`; this is corrupt metadata,
                // analogous to indexing beyond `$90:DFB5` into unrelated bank words.
                throw new InvalidDataException(
                    $"Bomb-jump setup cannot dispatch invalid movement type ${(byte)movementType:X2} for pose ${(int)Pose:X2}.");
        }
    }

    /// <summary>Executes special prospective-pose command three at `$91:EE80`.</summary>
    private void ArmPublishedBombJump()
    {
        // Command three replaces, rather than suspends, the movement pointer.
        // A suit-interrupted spark may still own it despite the ordinary body pose.
        // Keep its independently running palette/boost words, but never resume it
        // when the bomb arc restores normal movement and input.
        Shinespark.RelinquishMovementHandler();
        ShinesparkPoseInputLocked = false;
        // The same pointer replacement applies when suit pickup suspended Flash.
        // Its palette keeps running, but its raising/drain handler must never
        // resume after the bomb arc returns control to normal movement.
        CrystalFlash.RelinquishMovementHandler();
        CrystalFlashPoseInputLocked = false;
        BombJumpDirection |= 0x0800;
        BombJumpStarting = true;
        BombJumpActive = false;
        BombJumpPoseInputLocked = true;
    }

    /// <summary>True for the admitted right-facing movement-type-two normal-jump poses.</summary>
    public static bool IsRightFacingNormalJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.NeutralJumpTransitionRightPose or SamusPoseId.NeutralJumpRightPose or
        SamusPoseId.NormalJumpGunExtendedRightPose or
        SamusPoseId.NormalJumpForwardRightPose or SamusPoseId.NormalJumpAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpAimDiagonalUpRightPose or SamusPoseId.NormalJumpAimDiagonalDownRightPose or
        SamusPoseId.NormalJumpAimDownRightPose;

    /// <summary>True for the admitted left-facing movement-type-two normal-jump poses.</summary>
    public static bool IsLeftFacingNormalJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.NeutralJumpTransitionLeftPose or SamusPoseId.NeutralJumpLeftPose or
        SamusPoseId.NormalJumpGunExtendedLeftPose or
        SamusPoseId.NormalJumpForwardLeftPose or SamusPoseId.NormalJumpAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose or
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpAimDiagonalUpLeftPose or SamusPoseId.NormalJumpAimDiagonalDownLeftPose or
        SamusPoseId.NormalJumpAimDownLeftPose;

    /// <summary>True for admitted right-facing movement-type-six falling poses.</summary>
    public static bool IsRightFacingFallingPose(SamusPoseId pose) => pose is
        SamusPoseId.FallingRightPose or SamusPoseId.FallingGunExtendedRightPose or SamusPoseId.FallingAimUpRightPose or
        SamusPoseId.FallingAimDiagonalUpRightPose or SamusPoseId.FallingAimDiagonalDownRightPose or
        SamusPoseId.FallingAimDownRightPose;

    /// <summary>True for admitted left-facing movement-type-six falling poses.</summary>
    public static bool IsLeftFacingFallingPose(SamusPoseId pose) => pose is
        SamusPoseId.FallingLeftPose or SamusPoseId.FallingGunExtendedLeftPose or SamusPoseId.FallingAimUpLeftPose or
        SamusPoseId.FallingAimDiagonalUpLeftPose or SamusPoseId.FallingAimDiagonalDownLeftPose or
        SamusPoseId.FallingAimDownLeftPose;

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
    /// True for `$51/$52`, the two movement-type-two normal-jump bodies used while Samus
    /// has a same-facing horizontal direction held. These are ordinary aerial poses, not
    /// aimed or spinning poses; naming the pair prevents their transition semantics from
    /// being hidden inside another aim-specific condition.
    /// </summary>
    public static bool IsForwardMovingNormalJumpPose(SamusPoseId pose) => pose is
        SamusPoseId.NormalJumpForwardRightPose or SamusPoseId.NormalJumpForwardLeftPose;

    /// <summary>
    /// Reports whether a same-facing normal-jump/falling pose change belongs to the shared
    /// bank-$91 aim, horizontal-fire, or moving-forward transition seam.
    /// </summary>
    /// <remarks>
    /// `$91:A2F6/$A376` deliberately share one input table among neutral `$4D/$4E`, forward
    /// `$51/$52`, and the non-compact aimed jump bodies. In particular, holding Jump after
    /// releasing Right/Left produces `$51->$4D` or `$52->$4E`. Both endpoints must therefore
    /// participate in this classification. The previous target-only `$51/$52` check admitted
    /// entering the forward body but rejected leaving it, even though both routes execute the
    /// same native pose/animation initializer and preserve live velocity.
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
