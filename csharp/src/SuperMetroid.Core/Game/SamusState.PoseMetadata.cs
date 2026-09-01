using SuperMetroid.Core.Hardware;
using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// ROM-backed pose metadata, pose-family queries, and compact-body transition setup.
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
        int poseDefinition = AddWithinBank(PoseDefinitions, Pose * 8);
        Kinematics.XRadius = 5;
        Kinematics.YRadius = bus.ReadByte(AddWithinBank(poseDefinition, 6));
    }

    /// <summary>Reads pose-definition byte zero, the direction consumed by camera tracking.</summary>
    public byte ReadPoseXDirection(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ReadPoseXDirection(bus, Pose);
    }

    /// <summary>
    /// Reads pose-definition byte zero for an arbitrary pose. Native transition
    /// initializers compare the old and prospective records before publishing the new
    /// pose, so making that distinction explicit avoids temporarily corrupting live state.
    /// </summary>
    public static byte ReadPoseXDirection(ISnesAddressSpace bus, byte pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, pose * 8));
    }

    /// <summary>
    /// Typed view of pose-definition byte zero. Raw access remains available for ROM
    /// diagnostics and unknown records, while gameplay decisions no longer compare an
    /// unexplained byte to literals four and eight.
    /// </summary>
    public SamusFacingDirection ReadFacingDirection(ISnesAddressSpace bus) =>
        (SamusFacingDirection)ReadPoseXDirection(bus);

    /// <summary>Typed facing view for an arbitrary current or prospective pose.</summary>
    public static SamusFacingDirection ReadFacingDirection(ISnesAddressSpace bus, byte pose) =>
        (SamusFacingDirection)ReadPoseXDirection(bus, pose);

    /// <summary>True only for the verified left-facing pose-definition value four.</summary>
    public bool IsFacingLeft(ISnesAddressSpace bus) =>
        ReadFacingDirection(bus) == SamusFacingDirection.Left;

    /// <summary>Left-facing query for an arbitrary pose without mutating live state.</summary>
    public static bool IsFacingLeft(ISnesAddressSpace bus, byte pose) =>
        ReadFacingDirection(bus, pose) == SamusFacingDirection.Left;

    /// <summary>True only for the verified right-facing pose-definition value eight.</summary>
    public bool IsFacingRight(ISnesAddressSpace bus) =>
        ReadFacingDirection(bus) == SamusFacingDirection.Right;

    /// <summary>Reads pose-definition byte one, the movement-type dispatcher index.</summary>
    public byte ReadMovementType(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return ReadMovementType(bus, Pose);
    }

    /// <summary>
    /// Typed view of pose-definition byte one. The raw reader remains available for exact
    /// ROM diagnostics and for as-yet-unnamed dispatcher entries.
    /// </summary>
    public SamusMovementType ReadMovementKind(ISnesAddressSpace bus) =>
        (SamusMovementType)ReadMovementType(bus);

    /// <summary>Reads pose-definition byte one for a prospective pose without mutating Samus.</summary>
    public static byte ReadMovementType(ISnesAddressSpace bus, byte pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, pose * 8 + 1));
    }

    /// <summary>Typed movement view for an arbitrary prospective pose.</summary>
    public static SamusMovementType ReadMovementKind(ISnesAddressSpace bus, byte pose) =>
        (SamusMovementType)ReadMovementType(bus, pose);

    /// <summary>
    /// Reads pose-definition byte two, the pose selected by <c>$91:82D9</c> when no
    /// controller bits are held and the transition table therefore is not consulted.
    /// </summary>
    public byte ReadNoInputFallbackPose(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, Pose * 8 + 2));
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
    public static byte ReadShotDirection(ISnesAddressSpace bus, byte pose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, pose * 8 + 3));
    }

    /// <summary>
    /// Reads signed pose-definition byte four. Grapple firing uses this graphics-origin
    /// correction before applying the direction-specific hand offset at <c>$9B:C51E</c>.
    /// Keeping the byte behind a named accessor prevents the grapple port from duplicating
    /// the bank-$91 pose-table address or silently treating a negative offset as unsigned.
    /// </summary>
    public sbyte ReadGraphicsYOffset(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return unchecked((sbyte)bus.ReadByte(AddWithinBank(PoseDefinitions, Pose * 8 + 4)));
    }

    /// <summary>True for the four movement-type-zero, right-facing standing poses.</summary>
    public static bool IsRightFacingStandingPose(byte pose) => pose is
        FacingRightNormalPose or
        StandingAimUpRightPose or
        StandingAimDiagonalUpRightPose or
        StandingAimDiagonalDownRightPose;

    /// <summary>True for the four movement-type-zero, left-facing standing poses.</summary>
    public static bool IsLeftFacingStandingPose(byte pose) => pose is
        FacingLeftNormalPose or
        StandingAimUpLeftPose or
        StandingAimDiagonalUpLeftPose or
        StandingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the two front-view records selected by <c>MakeSamusFaceForward</c> at
    /// `$91:E3F6`. Their pose-X direction is zero and must never be guessed as left/right.
    /// </summary>
    public static bool IsForwardFacingPose(byte pose) => pose is
        ForwardFacingPowerSuitPose or ForwardFacingSuitedPose;

    /// <summary>
    /// Applies the movement/animation subset of <c>MakeSamusFaceForward</c> at
    /// `$91:E3F6-$91:E4A5`.
    /// </summary>
    /// <remarks>
    /// The native routine also locks the global current/new-state handlers, kills grapple,
    /// clears beam-flare presentation words, and reloads the suit palette. Those owners do
    /// not live in this state object. This method deliberately covers only the state it can
    /// own exactly: equipment-selected pose, ROM collision radius/delay list, and all motion
    /// words cleared by the routine. Runtime/debug callers remain responsible for input lock,
    /// palette, grapple, and priming the first graphics DMA before their first visible NMI.
    /// </remarks>
    public void ApplyForwardFacingPoseSetup(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // `$91:E3FD-$91:E415` gives Gravity bit `$0020` and Varia bit `$0001` equal
        // precedence: either selects the shared suited front-view pose `$9B`; neither
        // selects power-suit pose `$00`.
        Pose = EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit | SamusEquipmentFlags.GravitySuit)
            ? ForwardFacingSuitedPose
            : ForwardFacingPowerSuitPose;
        AnimationFrame = 0;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);

        // `$91:E438-$91:E44A` adjusts center Y upward three only if the initialized pose
        // did not produce radius 24. Retail `$00/$9B` both do, but retaining the branch
        // makes a corrupt/modified pose table observable rather than silently normalizing it.
        if (Kinematics.YRadius != 0x18)
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 3));

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
    public static bool IsRightFacingRunningPose(byte pose) => pose is
        MovingRightNormalPose or MovingRightGunExtendedPose or
        RunningAimUpRightPose or
        RunningAimDiagonalUpRightPose or
        RunningAimDiagonalDownRightPose;

    /// <summary>True for all five live movement-type-one, left-moving pose-table entries.</summary>
    public static bool IsLeftFacingRunningPose(byte pose) => pose is
        MovingLeftNormalPose or MovingLeftGunExtendedPose or
        RunningAimUpLeftPose or
        RunningAimDiagonalUpLeftPose or
        RunningAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the six ordinary-play bodies whose names explicitly say “gun extended.”
    /// Firing landings `$E6/$E7` are kept separate because they are short transition art,
    /// not stable horizontal-fire bodies selected directly by an input table.
    /// </summary>
    public static bool IsGunExtendedPose(byte pose) => pose is
        MovingRightGunExtendedPose or MovingLeftGunExtendedPose or
        NormalJumpGunExtendedRightPose or NormalJumpGunExtendedLeftPose or
        FallingGunExtendedRightPose or FallingGunExtendedLeftPose;

    /// <summary>
    /// True for the three poses whose artwork faces left while the type-$10 body travels
    /// right. The naming is intentionally visual; <c>ReadPoseXDirection</c> returns eight.
    /// </summary>
    public static bool IsMoonwalkingFacingLeftPose(byte pose) => pose is
        MoonwalkFacingLeftPose or MoonwalkAimUpLeftPose or MoonwalkAimDownLeftPose;

    /// <summary>True for the three right-facing moonwalk poses that travel left.</summary>
    public static bool IsMoonwalkingFacingRightPose(byte pose) => pose is
        MoonwalkFacingRightPose or MoonwalkAimUpRightPose or MoonwalkAimDownRightPose;

    public static bool IsMoonwalkingPose(byte pose) =>
        IsMoonwalkingFacingLeftPose(pose) || IsMoonwalkingFacingRightPose(pose);

    /// <summary>True for all five left-facing movement-type-`$1A` Draygon poses.</summary>
    public static bool IsLeftFacingDraygonGrabbedPose(byte pose) => pose is
        DraygonGrabbedNeutralLeftPose or DraygonGrabbedAimUpLeftPose or
        DraygonGrabbedFiringLeftPose or DraygonGrabbedAimDownLeftPose or
        DraygonGrabbedMovingLeftPose;

    /// <summary>True for all five right-facing movement-type-`$1A` Draygon poses.</summary>
    public static bool IsRightFacingDraygonGrabbedPose(byte pose) => pose is
        DraygonGrabbedNeutralRightPose or DraygonGrabbedAimUpRightPose or
        DraygonGrabbedFiringRightPose or DraygonGrabbedAimDownRightPose or
        DraygonGrabbedMovingRightPose;

    /// <summary>True for the complete ten-pose grabbed-by-Draygon family.</summary>
    public static bool IsDraygonGrabbedPose(byte pose) =>
        IsLeftFacingDraygonGrabbedPose(pose) || IsRightFacingDraygonGrabbedPose(pose);

    /// <summary>
    /// Applies one ordinary `$91:AE18/$AE56` transition without inventing pose physics.
    /// Every admitted record stays within one facing, retains radius 21, and restarts the
    /// target's cartridge delay list at byte zero through the normal pose initializer.
    /// </summary>
    public void ApplyDraygonGrabbedPoseChange(ISnesAddressSpace bus, byte targetPose)
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
                $"Draygon-grabbed input transition ${Pose:X2} -> ${targetPose:X2} crosses owner-controlled facing families.");
        }

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius != oldRadius)
        {
            throw new InvalidDataException(
                $"Draygon pose ${targetPose:X2} unexpectedly changed radius {oldRadius} -> {Kinematics.YRadius}.");
        }

        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// True for `$BF-$C4`, the six grounded turn frames selected only when Jump begins a
    /// direction reversal from movement type `$10`. Odd/even descriptive names follow the
    /// literal pose records, while the predicates below group them by destination facing.
    /// </summary>
    public static bool IsMoonwalkTurnJumpPose(byte pose) => pose is
        MoonwalkTurnJumpLeftPose or MoonwalkTurnJumpRightPose or
        MoonwalkTurnJumpAimUpLeftPose or MoonwalkTurnJumpAimUpRightPose or
        MoonwalkTurnJumpAimDownLeftPose or MoonwalkTurnJumpAimDownRightPose;

    /// <summary>True for the `$BF/$C1/$C3` family whose transition tables lead to left.</summary>
    public static bool IsMoonwalkTurnJumpLeftPose(byte pose) => pose is
        MoonwalkTurnJumpLeftPose or MoonwalkTurnJumpAimUpLeftPose or
        MoonwalkTurnJumpAimDownLeftPose;

    /// <summary>True for the `$C0/$C2/$C4` family whose transition tables lead to right.</summary>
    public static bool IsMoonwalkTurnJumpRightPose(byte pose) => pose is
        MoonwalkTurnJumpRightPose or MoonwalkTurnJumpAimUpRightPose or
        MoonwalkTurnJumpAimDownRightPose;

    /// <summary>True for `$89/$CF/$D1`, the complete right-facing type-`$15` family.</summary>
    public static bool IsRightFacingRanIntoWallPose(byte pose) => pose is
        RanIntoWallRightPose or RanIntoWallAimUpRightPose or RanIntoWallAimDownRightPose;

    /// <summary>True for `$8A/$D0/$D2`, the complete left-facing type-`$15` family.</summary>
    public static bool IsLeftFacingRanIntoWallPose(byte pose) => pose is
        RanIntoWallLeftPose or RanIntoWallAimUpLeftPose or RanIntoWallAimDownLeftPose;

    public static bool IsRanIntoWallPose(byte pose) =>
        IsRightFacingRanIntoWallPose(pose) || IsLeftFacingRanIntoWallPose(pose);

    public static bool IsAimedRanIntoWallPose(byte pose) => pose is
        RanIntoWallAimUpRightPose or RanIntoWallAimUpLeftPose or
        RanIntoWallAimDownRightPose or RanIntoWallAimDownLeftPose;

    /// <summary>True when a supported standing, running, crouching, or landing pose carries aim metadata.</summary>
    public static bool IsGroundedAimPose(byte pose) => pose is
        StandingAimUpRightPose or StandingAimUpLeftPose or
        StandingAimDiagonalUpRightPose or StandingAimDiagonalUpLeftPose or
        StandingAimDiagonalDownRightPose or StandingAimDiagonalDownLeftPose or
        RunningAimUpRightPose or RunningAimUpLeftPose or
        RunningAimDiagonalUpRightPose or RunningAimDiagonalUpLeftPose or
        RunningAimDiagonalDownRightPose or RunningAimDiagonalDownLeftPose or
        CrouchingAimUpRightPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalUpLeftPose or
        CrouchingAimDiagonalDownRightPose or CrouchingAimDiagonalDownLeftPose or
        LandingAimUpRightPose or LandingAimUpLeftPose or
        LandingAimDiagonalUpRightPose or LandingAimDiagonalUpLeftPose or
        LandingAimDiagonalDownRightPose or LandingAimDiagonalDownLeftPose or
        RanIntoWallAimUpRightPose or RanIntoWallAimUpLeftPose or
        RanIntoWallAimDownRightPose or RanIntoWallAimDownLeftPose;

    /// <summary>True for right-facing aimed normal-jump landing poses `$E0/$E2/$E4`.</summary>
    public static bool IsRightFacingAimedLandingPose(byte pose) => pose is
        LandingAimUpRightPose or LandingAimDiagonalUpRightPose or
        LandingAimDiagonalDownRightPose;

    /// <summary>True for left-facing aimed normal-jump landing poses `$E1/$E3/$E5`.</summary>
    public static bool IsLeftFacingAimedLandingPose(byte pose) => pose is
        LandingAimUpLeftPose or LandingAimDiagonalUpLeftPose or
        LandingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the complete right-facing landing set that shares the ordinary standing
    /// transition table: normal `$A4`, spin `$A6`, aimed `$E0/$E2/$E4`, and firing `$E6`.
    /// Centralizing this prevents a newly translated landing from silently losing crouch,
    /// turn, run, or wall-probe exits in one of the several native pose-change seams.
    /// </summary>
    public static bool IsRightFacingLandingPose(byte pose) => pose is
        NormalLandingRightPose or SpinLandingRightPose or FiringLandingRightPose ||
        IsRightFacingAimedLandingPose(pose);

    /// <summary>True for the complete mirrored left-facing landing set.</summary>
    public static bool IsLeftFacingLandingPose(byte pose) => pose is
        NormalLandingLeftPose or SpinLandingLeftPose or FiringLandingLeftPose ||
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
    public static bool IsLandingToNormalJumpTransition(byte sourcePose, byte targetPose) =>
        IsRightFacingLandingPose(sourcePose) && targetPose is
            NeutralJumpTransitionRightPose or
            NormalJumpTransitionAimUpRightPose or
            NormalJumpTransitionAimDiagonalUpRightPose or
            NormalJumpTransitionAimDiagonalDownRightPose ||
        IsLeftFacingLandingPose(sourcePose) && targetPose is
            NeutralJumpTransitionLeftPose or
            NormalJumpTransitionAimUpLeftPose or
            NormalJumpTransitionAimDiagonalUpLeftPose or
            NormalJumpTransitionAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the complete right-facing movement-type-five crouch family. All four
    /// records use radius 16 and the shared transition table at <c>$91:A66C</c>.
    /// </summary>
    public static bool IsRightFacingCrouchingPose(byte pose) => pose is
        CrouchingRightPose or CrouchingAimUpRightPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalDownRightPose;

    /// <summary>
    /// True for the complete left-facing movement-type-five crouch family. All four
    /// records use radius 16 and the mirrored table at <c>$91:A6BC</c>.
    /// </summary>
    public static bool IsLeftFacingCrouchingPose(byte pose) => pose is
        CrouchingLeftPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpLeftPose or CrouchingAimDiagonalDownLeftPose;

    /// <summary>True for the six crouching poses carrying a real shot direction.</summary>
    public static bool IsAimedCrouchingPose(byte pose) => pose is
        CrouchingAimUpRightPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalUpLeftPose or
        CrouchingAimDiagonalDownRightPose or CrouchingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for admitted grounded `$0E/$17` poses that began facing right and turn left.
    /// Their definition direction is already four (the destination facing), so callers
    /// must use this semantic grouping when preserving old rightward momentum.
    /// </summary>
    public static bool IsRightToLeftGroundTurnPose(byte pose) => pose is
        TurningRightToLeftPose or TurningRightToLeftAimUpPose or
        TurningRightToLeftAimDiagonalUpPose or TurningRightToLeftAimDiagonalDownPose or
        TurningRightToLeftCrouchingPose or TurningRightToLeftCrouchingAimUpPose or
        TurningRightToLeftCrouchingAimDiagonalUpPose or
        TurningRightToLeftCrouchingAimDiagonalDownPose;

    /// <summary>True for admitted grounded `$0E/$17` poses that began facing left and turn right.</summary>
    public static bool IsLeftToRightGroundTurnPose(byte pose) => pose is
        TurningLeftToRightPose or TurningLeftToRightAimUpPose or
        TurningLeftToRightAimDiagonalUpPose or TurningLeftToRightAimDiagonalDownPose or
        TurningLeftToRightCrouchingPose or TurningLeftToRightCrouchingAimUpPose or
        TurningLeftToRightCrouchingAimDiagonalUpPose or
        TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>
    /// True for the six aimed crouched-turn records dispatched through movement type `$17`.
    /// `$43/$44` are intentionally absent because the retail pose definitions assign those
    /// otherwise similar-looking unaimed crouch turns to grounded movement type `$0E`.
    /// </summary>
    public static bool IsAimedCrouchingTurnPose(byte pose) => pose is
        TurningRightToLeftCrouchingAimUpPose or TurningLeftToRightCrouchingAimUpPose or
        TurningRightToLeftCrouchingAimDiagonalUpPose or TurningLeftToRightCrouchingAimDiagonalUpPose or
        TurningRightToLeftCrouchingAimDiagonalDownPose or TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>True for every movement-type-$17 normal-jump turn record.</summary>
    public static bool IsJumpingTurnPose(byte pose) => pose is
        TurningRightToLeftJumpPose or TurningLeftToRightJumpPose or
        TurningRightToLeftJumpAimUpPose or TurningLeftToRightJumpAimUpPose or
        TurningRightToLeftJumpAimDownPose or TurningLeftToRightJumpAimDownPose or
        TurningRightToLeftJumpAimDiagonalUpPose or TurningLeftToRightJumpAimDiagonalUpPose;

    /// <summary>True for every movement-type-$18 falling-turn record.</summary>
    public static bool IsFallingTurnPose(byte pose) => pose is
        TurningRightToLeftFallingPose or TurningLeftToRightFallingPose or
        TurningRightToLeftFallingAimUpPose or TurningLeftToRightFallingAimUpPose or
        TurningRightToLeftFallingAimDownPose or TurningLeftToRightFallingAimDownPose or
        TurningRightToLeftFallingAimDiagonalUpPose or TurningLeftToRightFallingAimDiagonalUpPose;

    public static bool IsAerialTurnPose(byte pose) =>
        IsJumpingTurnPose(pose) || IsFallingTurnPose(pose);

    public static bool IsRightToLeftAerialTurnPose(byte pose) => pose is
        TurningRightToLeftJumpPose or TurningRightToLeftJumpAimUpPose or
        TurningRightToLeftJumpAimDownPose or TurningRightToLeftJumpAimDiagonalUpPose or
        TurningRightToLeftFallingPose or TurningRightToLeftFallingAimUpPose or
        TurningRightToLeftFallingAimDownPose or TurningRightToLeftFallingAimDiagonalUpPose;

    public static bool IsLeftToRightAerialTurnPose(byte pose) => pose is
        TurningLeftToRightJumpPose or TurningLeftToRightJumpAimUpPose or
        TurningLeftToRightJumpAimDownPose or TurningLeftToRightJumpAimDiagonalUpPose or
        TurningLeftToRightFallingPose or TurningLeftToRightFallingAimUpPose or
        TurningLeftToRightFallingAimDownPose or TurningLeftToRightFallingAimDiagonalUpPose;

    /// <summary>True for the two movement-type-$14 wall-jump launch records.</summary>
    public static bool IsWallJumpPose(byte pose) => pose is WallJumpRightPose or WallJumpLeftPose;

    /// <summary>
    /// True for all six stable movement-type-three spin bodies. The transition tables use
    /// `$19/$1A` as generic outputs; `$91:F624` can then substitute Space Jump or Screw
    /// Attack without changing the movement dispatcher.
    /// </summary>
    public static bool IsSpinJumpPose(byte pose) => pose is
        SpinJumpRightPose or SpinJumpLeftPose or
        SpaceJumpRightPose or SpaceJumpLeftPose or
        ScrewAttackRightPose or ScrewAttackLeftPose;

    /// <summary>True only for the two Screw Attack animation records `$81/$82`.</summary>
    public static bool IsScrewAttackPose(byte pose) => pose is
        ScrewAttackRightPose or ScrewAttackLeftPose;

    /// <summary>True only for the two Space Jump animation records `$1B/$1C`.</summary>
    public static bool IsSpaceJumpPose(byte pose) => pose is
        SpaceJumpRightPose or SpaceJumpLeftPose;

    /// <summary>
    /// True only for the admitted movement-type-$0F crouch/stand animation records.
    /// Morph/unmorph records share that dispatcher but are intentionally not hidden here.
    /// </summary>
    public static bool IsCrouchStandTransitionPose(byte pose) => pose is
        CrouchingTransitionRightPose or CrouchingTransitionLeftPose or
        StandingTransitionRightPose or StandingTransitionLeftPose or
        CrouchingTransitionAimUpRightPose or CrouchingTransitionAimUpLeftPose or
        CrouchingTransitionAimDiagonalUpRightPose or CrouchingTransitionAimDiagonalUpLeftPose or
        CrouchingTransitionAimDiagonalDownRightPose or CrouchingTransitionAimDiagonalDownLeftPose or
        StandingTransitionAimUpRightPose or StandingTransitionAimUpLeftPose or
        StandingTransitionAimDiagonalUpRightPose or StandingTransitionAimDiagonalUpLeftPose or
        StandingTransitionAimDiagonalDownRightPose or StandingTransitionAimDiagonalDownLeftPose;

    /// <summary>True for the four ordinary, non-Spring-Ball grounded morph poses.</summary>
    public static bool IsGroundedMorphBallPose(byte pose) => pose is
        MorphBallGroundRightPose or MorphBallGroundLeftPose or
        MorphBallMovingRightPose or MorphBallMovingLeftPose;

    /// <summary>True for the two ordinary, non-Spring-Ball airborne morph poses.</summary>
    public static bool IsAirborneMorphBallPose(byte pose) => pose is
        MorphBallFallingRightPose or MorphBallFallingLeftPose;

    /// <summary>True for movement type $11's four grounded Spring Ball poses.</summary>
    public static bool IsGroundedSpringBallPose(byte pose) => pose is
        SpringBallGroundRightPose or SpringBallGroundLeftPose or
        SpringBallMovingRightPose or SpringBallMovingLeftPose;

    /// <summary>True for Spring Ball jump/fall poses across movement types $12/$13.</summary>
    public static bool IsAirborneSpringBallPose(byte pose) => pose is
        SpringBallFallingRightPose or SpringBallFallingLeftPose or
        SpringBallJumpRightPose or SpringBallJumpLeftPose;

    /// <summary>True for every stable ordinary or Spring Ball pose using radius seven.</summary>
    public static bool IsStableBallPose(byte pose) =>
        IsGroundedMorphBallPose(pose) || IsAirborneMorphBallPose(pose) ||
        IsGroundedSpringBallPose(pose) || IsAirborneSpringBallPose(pose);

    /// <summary>True for the four entry/exit poses handled by movement type $0F.</summary>
    public static bool IsMorphTransitionPose(byte pose) => pose is
        MorphingTransitionRightPose or MorphingTransitionLeftPose or
        UnmorphingTransitionRightPose or UnmorphingTransitionLeftPose;

    /// <summary>
    /// Convenience debugger/test entry that performs both native phases: publishing the
    /// bank-$A0 timer-eight overlap direction, then consuming it through $90:DF99 and
    /// special command three $91:EE80. Live runtime code calls those phases on separate
    /// frames through <see cref="PublishBombJumpDirection"/> and
    /// <see cref="TrySetupPublishedBombJump"/>.
    /// </summary>
    public void RequestMorphedBombJump(byte direction)
    {
        if (!IsStableBallPose(Pose))
        {
            throw new InvalidOperationException(
                $"The morphed bomb-jump fixture requires a stable ball pose, not ${Pose:X2}.");
        }

        PublishBombJumpDirection(direction);
        ArmPublishedBombJump();
    }

    /// <summary>
    /// Stores only bank-$A0's low-byte bomb direction. The gameplay loop does this after
    /// frame-handler alpha; setup consequently cannot consume it until the next frame.
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
    /// Consumes a direction published by the previous frame's projectile collision through
    /// the complete movement-type table at `$90:DFB5-$DFEB` and command three at `$91:EE80`.
    /// </summary>
    /// <remarks>
    /// Standing and crouching reject the request only while time is frozen. Running,
    /// falling, moonwalking, wall-jump, wall-stop, and grapple families first select the
    /// ordinary forward-jump body `$51/$52`. Ball, unused `$07/$09`, and knockback-family
    /// entries retain their current pose. Jump/turn/transition/damage-boost and actor-owned
    /// families clear the published direction exactly like the table's carry-clear routines.
    /// </remarks>
    /// <returns>True when a pending low-byte direction installed the start handler.</returns>
    public bool TrySetupPublishedBombJump(
        ISnesAddressSpace bus,
        RoomLevelData level,
        bool timeIsFrozen,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        if (BombJumpDirection == 0 || (BombJumpDirection & 0xff00) != 0)
            return false;

        byte movementType = ReadMovementType(bus);
        switch (movementType)
        {
            case 0x00: // Standing.
            case 0x05: // Crouching.
                // `$90:DFED` is the only setup branch gated by frozen time. Its clear is a
                // complete 16-bit STZ, so no high-byte provenance survives rejection.
                if (timeIsFrozen)
                {
                    BombJumpDirection = 0;
                    return false;
                }
                goto case 0x01;

            case 0x01: // Running.
            case 0x06: // Falling.
            case 0x0b: // Unused.
            case 0x0c: // Unused.
            case 0x0d: // Unused.
            case 0x10: // Moonwalking.
            case 0x14: // Wall jumping.
            case 0x15: // Ran into a wall.
            case 0x16: // Grappling.
                // `$90:DFF7` tests the pose-definition direction, not the descriptive pose
                // number. Any value other than literal four follows the right-facing path.
                byte sourcePose = Pose;
                byte targetPose = ReadPoseXDirection(bus) == (byte)SamusFacingDirection.Left
                    ? NormalJumpForwardLeftPose
                    : NormalJumpForwardRightPose;

                // UpdateSamusPose routes the special target through the same `$91:FDAE`
                // expansion collision used by controller-selected pose changes. This is
                // observable for a crouching bomb jump: radius 16 grows to 21 and may be
                // rejected by a low ceiling instead of clipping Samus into terrain.
                LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                    bus,
                    level,
                    targetPose,
                    nmiFrameCounter,
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

                // HandleJumpTransition_NormalJumping runs before special command three.
                // Literal stable crouch receives its extra ten-pixel upward adjustment;
                // aimed crouch deliberately does not. The ordinary jump velocity is then
                // replaced by `$90:E025` on this same beta pass, but its gravity/pose side
                // effects still occur and must not be skipped.
                if (sourcePose is CrouchingRightPose or CrouchingLeftPose)
                    Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 10));
                InitializeAnimation(bus, initialFrame: 0);
                SamusAerialMovement.InitializeJump(bus, this);
                ArmPublishedBombJump();
                return true;

            case 0x04: // Morph ball on ground.
            case 0x07: // Unused/glitch ball.
            case 0x08: // Morph ball falling.
            case 0x09: // Unused/glitch ball.
            case 0x0a: // Knockback / Crystal Flash ending.
            case 0x11: // Spring Ball on ground.
            case 0x12: // Spring Ball in air.
            case 0x13: // Spring Ball falling.
                // `$90:E012` copies the current pose into SpecialProspectivePose. Because
                // that pose already matches, bank $91 immediately executes command three.
                ArmPublishedBombJump();
                return true;

            case 0x02: // Normal jumping.
            case 0x03: // Spin jumping.
            case 0x0e: // Turning on ground.
            case 0x0f: // Posture/morph transition.
            case 0x17: // Turning while jumping.
            case 0x18: // Turning while falling.
            case 0x19: // Damage boost.
            case 0x1a: // Grabbed by Draygon.
            case 0x1b: // Shinespark / Crystal Flash / drained / Mother Brain.
                BombJumpDirection = 0;
                return false;

            default:
                // The retail pose table never exceeds `$1B`; this is corrupt metadata,
                // analogous to indexing beyond `$90:DFB5` into unrelated bank words.
                throw new InvalidDataException(
                    $"Bomb-jump setup cannot dispatch invalid movement type ${movementType:X2} for pose ${Pose:X2}.");
        }
    }

    /// <summary>Executes special prospective-pose command three at `$91:EE80`.</summary>
    private void ArmPublishedBombJump()
    {
        BombJumpDirection |= 0x0800;
        BombJumpStarting = true;
        BombJumpActive = false;
    }

    /// <summary>True for the admitted right-facing movement-type-two normal-jump poses.</summary>
    public static bool IsRightFacingNormalJumpPose(byte pose) => pose is
        NeutralJumpTransitionRightPose or NeutralJumpRightPose or
        NormalJumpGunExtendedRightPose or
        NormalJumpForwardRightPose or NormalJumpAimUpRightPose or
        NormalJumpTransitionAimUpRightPose or
        NormalJumpTransitionAimDiagonalUpRightPose or
        NormalJumpTransitionAimDiagonalDownRightPose or
        NormalJumpAimDiagonalUpRightPose or NormalJumpAimDiagonalDownRightPose or
        NormalJumpAimDownRightPose;

    /// <summary>True for the admitted left-facing movement-type-two normal-jump poses.</summary>
    public static bool IsLeftFacingNormalJumpPose(byte pose) => pose is
        NeutralJumpTransitionLeftPose or NeutralJumpLeftPose or
        NormalJumpGunExtendedLeftPose or
        NormalJumpForwardLeftPose or NormalJumpAimUpLeftPose or
        NormalJumpTransitionAimUpLeftPose or
        NormalJumpTransitionAimDiagonalUpLeftPose or
        NormalJumpTransitionAimDiagonalDownLeftPose or
        NormalJumpAimDiagonalUpLeftPose or NormalJumpAimDiagonalDownLeftPose or
        NormalJumpAimDownLeftPose;

    /// <summary>True for admitted right-facing movement-type-six falling poses.</summary>
    public static bool IsRightFacingFallingPose(byte pose) => pose is
        FallingRightPose or FallingGunExtendedRightPose or FallingAimUpRightPose or
        FallingAimDiagonalUpRightPose or FallingAimDiagonalDownRightPose or
        FallingAimDownRightPose;

    /// <summary>True for admitted left-facing movement-type-six falling poses.</summary>
    public static bool IsLeftFacingFallingPose(byte pose) => pose is
        FallingLeftPose or FallingGunExtendedLeftPose or FallingAimUpLeftPose or
        FallingAimDiagonalUpLeftPose or FallingAimDiagonalDownLeftPose or
        FallingAimDownLeftPose;

    /// <summary>True for the aimed normal-jump/falling poses, including compact Down aim.</summary>
    public static bool IsAimedAerialPose(byte pose) => pose is
        NormalJumpAimUpRightPose or NormalJumpAimUpLeftPose or
        NormalJumpTransitionAimUpRightPose or NormalJumpTransitionAimUpLeftPose or
        NormalJumpTransitionAimDiagonalUpRightPose or NormalJumpTransitionAimDiagonalUpLeftPose or
        NormalJumpTransitionAimDiagonalDownRightPose or NormalJumpTransitionAimDiagonalDownLeftPose or
        NormalJumpAimDiagonalUpRightPose or NormalJumpAimDiagonalUpLeftPose or
        NormalJumpAimDiagonalDownRightPose or NormalJumpAimDiagonalDownLeftPose or
        FallingAimUpRightPose or FallingAimUpLeftPose or
        FallingAimDiagonalUpRightPose or FallingAimDiagonalUpLeftPose or
        FallingAimDiagonalDownRightPose or FallingAimDiagonalDownLeftPose or
        NormalJumpAimDownRightPose or NormalJumpAimDownLeftPose or
        FallingAimDownRightPose or FallingAimDownLeftPose;

    /// <summary>
    /// True for `$51/$52`, the two movement-type-two normal-jump bodies used while Samus
    /// has a same-facing horizontal direction held. These are ordinary aerial poses, not
    /// aimed or spinning poses; naming the pair prevents their transition semantics from
    /// being hidden inside another aim-specific condition.
    /// </summary>
    public static bool IsForwardMovingNormalJumpPose(byte pose) => pose is
        NormalJumpForwardRightPose or NormalJumpForwardLeftPose;

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
    public static bool IsSameFacingAerialAimFireOrForwardTransition(byte source, byte target)
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
    public static bool IsCompactAerialPose(byte pose) => pose is
        NormalJumpAimDownRightPose or NormalJumpAimDownLeftPose or
        FallingAimDownRightPose or FallingAimDownLeftPose;

}
