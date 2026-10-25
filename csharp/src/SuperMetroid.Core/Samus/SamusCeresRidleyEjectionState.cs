using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The two-stage Samus handler installed by Ceres Ridley's Mode-7 getaway at
/// <c>$90:E119-$E21B</c>.
/// </summary>
/// <remarks>
/// The native request is made from room-main code, after Samus has already moved for that
/// frame, so the installed handler first runs on the following gameplay frame.
/// </remarks>
public sealed class SamusCeresRidleyEjectionState
{
    /// <summary>Native signed whole-pixel downward speed threshold during Ridley's shove.</summary>
    private const ushort TerminalDownwardSpeed = 5;

    /// <summary>True while `$90:E12E/$E1C8` owns Samus movement.</summary>
    public bool IsActive { get; private set; }

    /// <summary>True until the first `$90:E12E` call installs pose and velocity.</summary>
    public bool InitializationPending { get; private set; }

    /// <summary>
    /// The wall the shove carries Samus toward (native <c>samus_var62</c>); <see cref="CeresRidleyShoveDirection.None"/>
    /// until initialization chooses a side and again once wall contact ends the shove.
    /// </summary>
    public CeresRidleyShoveDirection PushDirection { get; private set; }

    /// <summary>
    /// <c>SetSamusToBePushedOutOfCeresRidleysWay</c> ($90:E119): installs the handlers
    /// that the following gameplay frame executes.
    /// </summary>
    public void Request(SamusState samus)
    {
        Ensure.NotNull(samus);
        IsActive = true;
        InitializationPending = true;

        // `$90:E119` writes `$E90E` into the one physical movement-handler word. An
        // uncrashed ordinary spark consequently retains its boost words as Blue Suit. If
        // the overwritten pointer was Crystal Flash's raise/drain/finish routine, its
        // independent palette handler and RTS pose-input pointer survive instead. Neither
        // displaced movement owner can resume after Ridley's shove ends.
        samus.Shinespark.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishMovementHandler();

        // `$90:E119` replaces only MovementHandler. The ordinary pose-input handler stays
        // installed: held controller chords are still matched, projectile/HUD input still
        // runs, and `$90:E1C8` specifically discards a prospective `$4F/$50` damage boost
        // while the shove remains active. Runtime owns that narrow prospective-pose clear.
        // Do not install a locked handler pair here; doing so changes controller
        // semantics and made this cutscene indistinguishable from an elevator/message lock.
    }

    /// <summary>Executes one `$90:E12E` or `$90:E1C8` handler call.</summary>
    public CeresRidleyEjectionResult Step(

        RoomLevelData level,
        SamusState samus,
        ushort layer1X,
        int nmiFrameCounter)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!IsActive)
            throw new InvalidOperationException("Ceres Ridley ejection is not active.");

        if (InitializationPending)
        {
            InitializationPending = false;

            // `$90:E12E` selects the ordinary hurt pose from the PREVIOUS pose direction and
            // refreshes the animation. InitializeSamusPose_1 does not publish the hurt pose's
            // radius, so the `21 - radius` alignment uses the source pose's live radius; the
            // ordinary pose epilogue publishes radius 21 on the following frame.
            bool facingLeft = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Left;
            samus.Pose = facingLeft
                ? SamusPoseId.KnockbackLeftPose
                : SamusPoseId.KnockbackRightPose;
            SamusPresentation.InitializeAnimation(samus);
            // The gamma initialization owns this shift. Request/BeginFrame must
            // not publish it early, and subsequent shove frames must not repeat it.
            samus.CommitPoseHistory();
            samus.Position = samus.Position.WithWholeY(unchecked((ushort)(
                samus.Position.Y.Whole - (21 - samus.Kinematics.Radius.Y))));

            // The side is selected from screen X, not world X. Exact center belongs to the
            // rightward branch because the native BMI tests a signed `(screenX - 128)`.
            ushort screenX = unchecked((ushort)(samus.Position.X.Whole - layer1X));
            PushDirection = unchecked((short)(screenX - 128)) < 0
                ? CeresRidleyShoveDirection.Left
                : CeresRidleyShoveDirection.Right;
            samus.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(TerminalDownwardSpeed, 0);

            // `$90:E12E` initializes only handler state on this call. `$90:E1C8` does not
            // perform the first collision/movement pass until the following frame.
            return new CeresRidleyEjectionResult();
        }

        // `Samus_BombJumpFallingXMovement_` deliberately uses the current pose's movement
        // type ($0A for $53/$54) and the persistent base/extra-run words. Direction comes
        // from samus_var62 instead of pose facing, so Ridley always throws toward a wall.
        SamusHorizontalSpeedState horizontalSpeed = samus.HorizontalSpeed;
        horizontalSpeed.SelectEnvironmentSpeedTable(
            samus.LiquidPhysics.DetermineMovementMedium(samus));
        uint baseSpeed = horizontalSpeed.CalculateBaseSpeed(
            SamusPoseMetadata.ReadMovementType(samus));
        var requestedX = SamusHorizontalDisplacement.Toward(PushDirection == CeresRidleyShoveDirection.Left, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedX.Displacement, CollisionMovementDirection = requestedX.CollisionDirection });

        if (horizontal.Collided)
        {
            // MoveSamus_Left/Right end in Kill_SamusXSpeed_IfCollisionDetected ($90:E5CE),
            // which cancels speed boosting and clears the extra-run words as well.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
            // `$90:E1FD/$E21C` restore the ordinary handlers, then
            // HandleKnockbackVerticalCollision ($90:DF6E) sees the still-set collision flag,
            // clears the movement words and aligns the feet to the previous pose. The push
            // never sets knockback direction `$0A52`, so `$90:DDE9` does not convert the
            // hurt pose; `$53/$54` remains for the ordinary movement handler.
            IsActive = false;
            PushDirection = CeresRidleyShoveDirection.None;
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.None;
            samus.AlignBottomAfterPoseChange(
                SamusPoseMetadata.ReadPoseYRadius(samus.PoseHistory.PreviousPose),
                SamusPoseMetadata.ReadPoseYRadius(samus.Pose));
            return new CeresRidleyEjectionResult();
        }

        // `$90:8F5A` accelerates only while the signed whole speed is below five. The
        // resulting 16.16 word is passed directly to MoveDown; unlike the common falling
        // wrapper, this path adds no extra one-pixel downward bias.
        if (unchecked((short)(samus.Kinematics.YSpeed.Whole - TerminalDownwardSpeed)) < 0)
        {
            uint accelerated = unchecked(
                samus.Kinematics.VerticalSpeedFixed +
                (samus.Kinematics.YAcceleration.RawValue));
            samus.Kinematics.YSpeed = SnesUnsignedSixteenSixteen.FromRaw(accelerated);
        }
        SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = unchecked((int)samus.Kinematics.VerticalSpeedFixed), ScanLeftToRight = (nmiFrameCounter & 1) == 0 });

        return new CeresRidleyEjectionResult();
    }
}
