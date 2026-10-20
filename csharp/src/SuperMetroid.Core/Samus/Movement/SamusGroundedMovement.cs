using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// Verified grounded movement slices from Samus's ordinary bank-$90 movement dispatcher.
/// </summary>
/// <remarks>
/// This is deliberately not a generic platformer controller. The entry points below
/// correspond only to movement types zero, one, $0E, $10, and $15. Air/water/lava ROM X
/// tables and submerged Dash gating are translated; enemy collision, conveyor displacement,
/// and knockback remain separate. Ordinary Dash and equipped-Speed-Booster accumulation/
/// staging are translated; palette/echo rendering lives outside movement. Each
/// omitted system has observable native state and must be ported before its branch is
/// enabled; none is silently replaced with desktop physics.
/// </remarks>
public static class SamusGroundedMovement
{
    /// <summary>
    /// Ports front-view movement at `$90:A383-$90:A3AB`, including the active-elevator
    /// one-pixel downward block scan.
    /// </summary>
    /// <remarks>
    /// Poses `$00/$9B` share movement type zero with ordinary standing, but the cartridge
    /// returns before horizontal movement, speed cleanup, and the one-pixel grounding probe.
    /// It only clears <c>SamusSolidVerticalCollisionResult</c> while elevator status is zero.
    /// A nonzero status calls `$94:9763` with exactly `1.0000` downward displacement before
    /// clearing that result. The elevator actor remains a separate producer of the status
    /// word and platform art; this method translates the complete Samus-side consumer.
    /// </remarks>
    public static BlockMoveResult? StepFacingForward(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        bool elevatorIsMoving = false,
        RoomPlmSystem? plms = null,
        bool publishDoorSideEffects = true)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SamusPoseMetadata.IsForwardFacingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Forward-facing movement requires pose $00/$9B, not ${(int)samus.Pose:X2}.");
        }

        BlockMoveResult? vertical = null;
        if (elevatorIsMoving)
        {
            // `$90:A397-$A3A4` publishes collision direction two and calls the bank-$94
            // downward mover with `$12.$14 = 0001.0000`. That entry deliberately skips
            // solid-enemy collision but retains ordinary room-block/BTS scanning and the
            // alternating left/right order selected from accepted-NMI parity.
            vertical = SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new VerticalBlockMove { Displacement = 1 << 16, ScanLeftToRight = (nmiFrameCounter & 1) == 0, IncludeSolidEnemies = false, PublishDoorSideEffects = publishDoorSideEffects });
        }

        // `$90:A3A8` is the only write in the ordinary no-elevator path. In particular,
        // stale base/extra X speed is retained; MakeSamusFaceForward clears those words at
        // setup time, not every frame in this movement dispatcher.
        samus.SolidVerticalCollisionResult = SamusVerticalCollisionResult.None;
        return vertical;
    }

    /// <summary>
    /// Ports the movement-relevant portion of <c>Samus_Movement_00_Standing</c> at <c>$90:A383</c> for either facing:
    /// the right-facing $01/$03/$05/$07 family or the left-facing $02/$04/$06/$08 family.
    /// </summary>
    public static GroundedMovementResult StepStanding(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        SnesButton controllerInput,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose);
        if (!facingLeft && !SamusPoseMetadata.IsRightFacingStandingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Standing movement requires pose $01-$08, not ${(int)samus.Pose:X2}.");
        }

        ResetStandingShotAnimation(samus, controllerInput);
        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // $90:A39E calls Samus_Move_NoBaseSpeed_X, which still passes zero through
        // Samus_MoveX and the pose direction's displacement calculator ($90:8EA9). That
        // calculation publishes total-X speed before collision; do not skip it merely because
        // a freshly spawned standing Samus normally has zero base/extra speed. A left-facing
        // pose's direction byte is $04, so the native helper is the LEFT form even though its
        // base argument is zero, which matters once extra displacement contributes.
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(facingLeft, samus, baseSpeed: 0);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            nmiFrameCounter,
            plms);

        // $90:A3A7-$90:A3D7 cancels speed boost and clears extra speed, base speed, and
        // acceleration mode after both movement calls, whichever way Samus faces. Speed-booster
        // bookkeeping itself is outside this no-equipment slice, but these five WRAM words are
        // exact and live.
        speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>Restarts neutral standing's shot pose on held Shoot even when no projectile was admitted; angled poses keep their own animation.</summary>
    private static void ResetStandingShotAnimation(SamusState samus, SnesButton controllerInput)
    {
        if (samus.Pose is SamusPoseId.FacingRightNormalPose or SamusPoseId.FacingLeftNormalPose &&
            (controllerInput & SnesButton.X) != 0)
            samus.Presentation.SetAnimationFrameFromSpecialHandler(0, SamusMovementRomData.StandingShotAnimationTimer);
    }

    /// <summary>
    /// Ports <c>Samus_Movement_01_Running</c> at <c>$90:A3E5</c> in dry air, including ordinary Dash, for either
    /// facing: the $09/$0D/$0F/$11 right-moving family or the $0A/$0E/$10/$12 left-moving family.
    /// </summary>
    public static GroundedMovementResult StepRunning(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        SnesButton controllerInput = SnesButton.None,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        bool facingLeft = SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose);
        if (!facingLeft && !SamusPoseMetadata.IsRightFacingRunningPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Running movement requires pose $09-$12 of the running families, not ${(int)samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // `$90:9BD1` can replace the block reaction's ordinary `$9F55` pointer with the
        // complete water/lava table after comparing Samus's bottom boundary.
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectEnvironmentSpeedTable(liquidMedium);

        // `$90:8E64` handles Dash before calculating base speed. With no equipped Speed
        // Booster, B establishes momentum, adds exactly 0.1000 per frame, and caps the
        // extra component at 2.0000. The flag retains that component after B is released.
        speed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Running,
            controllerInput,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // `$90:8E64 -> $90:9A7E` advances the split 16.16 base speed. The following
        // `$90:8EA9` direction selector is shared by every horizontal movement family:
        // modes zero and two obey the pose's direction byte, while every other nonzero
        // value reverses it. Mode one is therefore not an invalid running state; it is the
        // one-frame-old momentum that can survive a pose transition into this body. Do not
        // special-case it, even if another input-side transition has already changed the
        // visible running pose.
        uint baseSpeed = speed.CalculateBaseSpeed(movementType: SamusMovementType.Running);
        bool movesLeft = (speed.AccelerationMode is SamusAccelerationMode.Accelerating or SamusAccelerationMode.Decelerating) == facingLeft;
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(movesLeft, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });

        // $90:93B1 invokes Samus_ClearXSpeedIfColl immediately after block collision. Total
        // speed deliberately remains published: the following grounding routine reads it
        // even if base speed was just cleared by a wall.
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            nmiFrameCounter,
            plms);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports the grounded branches of <c>SamusMovement_TurningAround_OnGround</c> at
    /// <c>$90:A67C</c> and <c>SamusMovement_TurningAround_Jumping</c> at <c>$90:A790</c>.
    /// The latter name is misleading for `$97-$9A/$A2/$A3`: crouched entry leaves Y direction
    /// zero, so native runs its no-speed grounding probe rather than airborne gravity.
    /// </summary>
    public static GroundedMovementResult StepTurningOnGround(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        bool turnsLeft = SamusPoseMetadata.IsRightToLeftGroundTurnPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkTurnJumpLeftPose(samus.Pose);
        bool turnsRight = SamusPoseMetadata.IsLeftToRightGroundTurnPose(samus.Pose) ||
            SamusPoseMetadata.IsMoonwalkTurnJumpRightPose(samus.Pose);
        if (!turnsLeft && !turnsRight)
        {
            throw new InvalidOperationException(
                $"Grounded-turn movement requires a verified standing, crouched, or moonwalk turn pose, not ${(int)samus.Pose:X2}.");
        }

        SamusMovementType movementType = samus.PoseMetadata.ReadMovementType();
        if (movementType is not (SamusMovementType.TurningOnGround or SamusMovementType.TurningWhileJumping))
            throw new InvalidOperationException(
                $"Grounded turn pose ${(int)samus.Pose:X2} has movement type ${(byte)movementType:X2}.");
        if (movementType == SamusMovementType.TurningWhileJumping && !SamusPoseMetadata.IsAimedCrouchingTurnPose(samus.Pose))
            throw new InvalidOperationException($"Grounded type-$17 admission requires an aimed crouched turn, not ${(int)samus.Pose:X2}.");
        if (movementType == SamusMovementType.TurningWhileJumping && samus.Kinematics.YDirection != SamusVerticalDirection.None)
        {
            throw new InvalidOperationException(
                $"Airborne type-$17 turn pose ${(int)samus.Pose:X2} must execute through SamusAerialMovement.StepTurningInAir.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectEnvironmentSpeedTable(liquidMedium);

        // The native type-$0E/$17 handler calls the complete `$90:8E64` X routine before
        // cancelling boost. Because this movement type is not running, `$90:973E` cannot
        // add speed; it either clears an unowned pair or preserves an already established
        // momentum pair for this final movement frame. The unconditional cancellation and
        // zero writes below then retire it in the cartridge's original order.
        speed.HandleExtraRunSpeed(
            movementType,
            controllerInput: 0,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // The pose's literal `$0E` or `$17` twelve-byte speed-table record supplies the
        // deceleration. If subtraction crosses below zero,
        // $90:9B0A clears both speed halves AND mode before $90:8EA9 chooses direction.
        uint baseSpeed = speed.CalculateBaseSpeed(movementType);

        // `$90:8EA9` reads the pose definition rather than the English-facing name. Keeping
        // the literal direction byte also preserves the cartridge's behavior for unexpected
        // nonzero acceleration-mode values instead of manufacturing another unsupported arm.
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            nmiFrameCounter,
            plms);

        // `$90:A685` calls `Samus_CancelSpeedBoost` on every turn frame. This matters even
        // without the Speed Booster item: `$91:F8D3` folded the numeric extra component but
        // deliberately left `$0B3C` set until this movement handler. Then `$90:A689-$A68C`
        // perform the separately observable zero writes to the two extra-speed words.
        speed.CancelRunningMomentum(samus.PoseMetadata.ReadFacingDirection());
        speed.ExtraRunSpeed = default;
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports <c>SamusMovement_Moonwalking</c> at <c>$90:A694</c> for stable poses
    /// `$49/$4A/$75-$78`, including the complete shared extra-run-speed prepass.
    /// </summary>
    public static GroundedMovementResult StepMoonwalking(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SamusPoseMetadata.IsMoonwalkingPose(samus.Pose) ||
            samus.PoseMetadata.ReadMovementType() != SamusMovementType.Moonwalking)
        {
            throw new InvalidOperationException(
                $"Moonwalking movement requires pose $49/$4A/$75-$78, not ${(int)samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        SamusLiquidMedium liquidMedium = samus.LiquidPhysics.DetermineMovementMedium(samus);
        speed.SelectEnvironmentSpeedTable(liquidMedium);

        // `$90:A697` calls `Samus_X_Movement`, whose first operation is `$90:973E`.
        // Movement type `$10` can never enter that routine's Dash-acceleration arm, but an
        // existing momentum flag deliberately carries the extra pair through moonwalking;
        // without the flag the pair is cleared before displacement is calculated.
        speed.HandleExtraRunSpeed(
            movementType: SamusMovementType.Moonwalking,
            controllerInput: 0,
            speedBoosterEquipped: samus.EquippedItems.HasAny(SamusEquipmentFlags.SpeedBooster),
            liquidImpeded: liquidMedium != SamusLiquidMedium.Air);

        // Type `$10` has its own twelve-byte speed record. Its pose-X bytes intentionally
        // produce travel opposite the visible facing; mode one reverses that byte just as it
        // does for every other family. Do not derive either direction from the pose's name.
        uint baseSpeed = speed.CalculateBaseSpeed(movementType: SamusMovementType.Moonwalking);
        var requestedHorizontal = SamusHorizontalDisplacement.ForPoseDirection(samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());

        // `$90:A69A` is the same no-speed-calculation grounding probe used by running.
        // It deliberately scales the downward probe by the total horizontal magnitude.
        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            nmiFrameCounter,
            plms);
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Ports <c>Samus_Movement_15_RanIntoWall</c> at <c>$90:A75F</c> for
    /// `$89/$8A/$CF-$D2` in the block-only dry-room slice.
    /// </summary>
    public static GroundedMovementResult StepRanIntoWall(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!SamusPoseMetadata.IsRanIntoWallPose(samus.Pose) ||
            samus.PoseMetadata.ReadMovementType() != SamusMovementType.RanIntoWall)
        {
            throw new InvalidOperationException(
                $"Ran-into-wall movement requires pose $89/$8A/$CF-$D2, not ${(int)samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;

        // `$90:A75F` calls the same no-base-speed X routine as standing. Direction still
        // comes from the literal pose definition: `$89/$CF/$D1` store eight (right), while
        // `$8A/$D0/$D2` store four (left). Extra speed is included in the requested move
        // before the handler's unconditional cleanup, matching the native call order.
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(samus.PoseMetadata.IsFacing(SamusFacingDirection.Left), samus, baseSpeed: 0);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());

        // The one-or-more-pixel downward grounding probe runs before all X words are
        // cleared. This preserves the same ledge behavior as `$90:A762` even though the
        // ordinary wall-stop state normally begins with zero speed.
        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            nmiFrameCounter,
            plms);

        // `$90:A766-$A77F` cancels speed boost and clears both run/base components plus
        // acceleration mode on every frame. The speed-booster counters themselves remain
        // outside this slice; the five movement words are exact and debugger-visible.
        speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Runs movement type zero for every translated landing pose: `$A4-$A7` for the ordinary
    /// and spin families, plus `$E0-$E7` for aimed and horizontally firing landings. Native
    /// landing animation is cosmetic with respect to movement: every member uses standing's
    /// zero-base-speed horizontal pass, grounding probe, and momentum cleanup until animation
    /// command `$F8` returns to pose `$01/$02`.
    /// </summary>
    public static GroundedMovementResult StepLanding(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        // Do not duplicate the pose list here. The predicates are the single audited definition
        // of movement type zero's landing family and deliberately include firing landings `$E6`
        // and `$E7`. Keeping dispatch and this defensive guard on the same predicates prevents a
        // newly translated landing from reaching this real movement handler only to be rejected.
        if (!SamusPoseMetadata.IsRightFacingLandingPose(samus.Pose) &&
            !SamusPoseMetadata.IsLeftFacingLandingPose(samus.Pose))
        {
            throw new InvalidOperationException(
                $"Landing movement requires pose $A4-$A7/$E0-$E7, not ${(int)samus.Pose:X2}.");
        }

        SamusHorizontalSpeedState speed = samus.HorizontalSpeed;
        bool facingLeft = samus.PoseMetadata.IsFacing(SamusFacingDirection.Left);
        var requestedHorizontal = SamusHorizontalDisplacement.Toward(facingLeft, samus, baseSpeed: 0);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new HorizontalBlockMove { Displacement = requestedHorizontal.Displacement, CollisionMovementDirection = requestedHorizontal.CollisionDirection });
        if (horizontal.Collided)
            speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());

        BlockMoveResult vertical = RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            nmiFrameCounter,
            plms);
        speed.ClearHorizontalMomentum(samus.PoseMetadata.ReadFacingDirection());
        // Landing art still uses native standing movement: it does not reset Y words.
        // Collision command five owns landing cleanup; a later hurt expiry may write
        // direction two while this pose remains active, and that value must survive.
        return new GroundedMovementResult(horizontal, vertical);
    }

    /// <summary>
    /// Executes movement type <c>$0A</c>'s ordinary handler at <c>$90:A5FC</c> after a
    /// special owner (normal damage knockback or Ceres Ridley's ejection) has restored the
    /// normal dispatcher while leaving pose <c>$53/$54</c> current. Native clears the
    /// vertical-collision result and performs only the shared no-speed Y probe; it does not
    /// invent another knockback arc or silently reinterpret the hurt body as standing.
    /// </summary>
    public static BlockMoveResult StepKnockbackOrCrystalFlashEnding(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        RoomPlmSystem? plms = null)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (samus.PoseMetadata.ReadMovementType() != SamusMovementType.Knockback)
        {
            throw new InvalidOperationException(
                $"Knockback-ending movement requires type $0A, not pose ${(int)samus.Pose:X2}.");
        }

        return RunNoSpeedCalculationGroundingProbe(

            level,
            samus,
            nmiFrameCounter,
            plms);
    }

    /// <summary>
    /// Ports <c>Samus_Move_NoSpeedCalc_Y</c> at <c>$90:923F</c>, including the external-Y
    /// replacement path and its asymmetric positive one-pixel bias.
    /// </summary>
    internal static BlockMoveResult RunNoSpeedCalculationGroundingProbe(

        RoomLevelData level,
        SamusState samus,
        int nmiFrameCounter,
        RoomPlmSystem? plms)
    {
        int displacement = SamusExtraDisplacement.CalculateNoSpeedVerticalDisplacement(
            samus.Kinematics,
            samus.HorizontalSpeed);

        // $94:9763 alternates its left-to-right and right-to-left horizontal scan order on
        // the low bit of the accepted-NMI word. That parity can affect which simultaneous
        // collision side effect wins, so it remains an explicit input even in this slice.
        return SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, Plms = plms }, samus.Kinematics, new VerticalBlockMove { Displacement = displacement, ScanLeftToRight = (nmiFrameCounter & 1) == 0 });
    }

}
