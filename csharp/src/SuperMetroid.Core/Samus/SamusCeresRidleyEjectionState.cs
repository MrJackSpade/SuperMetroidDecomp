using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The two-stage Samus handler installed by Ceres Ridley's turning-floor getaway.
/// </summary>
/// <remarks>
/// The request is made from room-main code, after Samus has already moved for that
/// frame, so the installed handler first runs on the following gameplay frame.
/// </remarks>
public sealed class SamusCeresRidleyEjectionState
{
    /// <summary>Signed whole-pixel downward speed threshold during Ridley's shove.</summary>
    private const int TerminalDownwardSpeed = 5;

    /// <summary>True while the shove's handlers own Samus movement.</summary>
    public bool IsActive { get; private set; }

    /// <summary>True until the first shove call installs pose and velocity.</summary>
    public bool InitializationPending { get; private set; }

    /// <summary>
    /// The wall the shove carries Samus toward; <see cref="CeresRidleyShoveDirection.None"/>
    /// until initialization chooses a side and again once wall contact ends the shove.
    /// </summary>
    public CeresRidleyShoveDirection PushDirection { get; private set; }

    /// <summary>
    /// Sets Samus to be pushed out of Ceres Ridley's way: installs the handlers
    /// that the following gameplay frame executes.
    /// </summary>
    public void Request(SamusState samus)
    {
        Ensure.NotNull(samus);
        IsActive = true;
        InitializationPending = true;

        // The shove replaces Samus's one movement handler. An
        // uncrashed ordinary spark consequently keeps its boost as Blue Suit. If
        // the replaced handler was Crystal Flash's raise/drain/finish, its
        // independent suit colouring and empty pose-input handler survive instead. Neither
        // displaced movement owner can resume after Ridley's shove ends.
        samus.Shinespark.RelinquishMovementHandler();
        samus.CrystalFlash.RelinquishMovementHandler();

        // Replaces only MovementHandler. The ordinary pose-input handler stays
        // installed: held controller chords are still matched, projectile/HUD input still
        // runs, and the shove specifically discards a prospective damage boost
        // while the shove remains active. Runtime owns that narrow prospective-pose clear.
        // Do not install a locked handler pair here; doing so changes controller
        // semantics and made this cutscene indistinguishable from an elevator/message lock.
    }

    /// <summary>Executes one shove handler call.</summary>
    public CeresRidleyEjectionResult Step(

        RoomLevelData level,
        SamusState samus,
        int cameraX,
        int frameCounter)
    {

        Ensure.NotNull(level);
        Ensure.NotNull(samus);
        if (!IsActive)
            throw new InvalidOperationException("Ceres Ridley ejection is not active.");

        if (InitializationPending)
        {
            InitializationPending = false;

            // Selects the ordinary hurt pose from the PREVIOUS pose direction and
            // refreshes the animation. The pose initializer does not publish the hurt pose's
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
            samus.Position = samus.Position.WithWrappedWholeY(
                samus.Position.Y.Whole - (21 - samus.Kinematics.Radius.Y));

            // The side is selected from screen X, not world X. Exact center belongs to the
            // rightward branch because the game checks whether `(screenX - 128)` is negative.
            int screenX = CoordinateAxis.Wrap(samus.Position.X.Whole - cameraX);
            PushDirection = CoordinateAxis.Difference(screenX, 128) < 0
                ? CeresRidleyShoveDirection.Left
                : CeresRidleyShoveDirection.Right;
            samus.Kinematics.YSpeed = new UnsignedSixteenSixteen(TerminalDownwardSpeed, 0);

            // Initializes only handler state on this call. The shove does not
            // perform the first collision/movement pass until the following frame.
            return new CeresRidleyEjectionResult();
        }

        // The bomb jump's falling horizontal movement deliberately uses the current pose's movement
        // type (knockback for the knockback poses) and the persistent base/extra-run speeds. Direction comes
        // from the ejection's own direction instead of pose facing, so Ridley always throws toward a wall.
        SamusHorizontalSpeedState horizontalSpeed = samus.HorizontalSpeed;
        horizontalSpeed.SelectEnvironmentSpeedTable(
            samus.LiquidPhysics.DetermineMovementMedium(samus));
        UnsignedSixteenSixteen baseSpeed = horizontalSpeed.CalculateBaseSpeed(
            SamusPoseMetadata.ReadMovementType(samus));
        var requestedX = SamusHorizontalDisplacement.Toward(PushDirection == CeresRidleyShoveDirection.Left, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level }, samus.Kinematics, samus, new HorizontalBlockMove { Displacement = requestedX.Displacement, CollisionMovementDirection = requestedX.CollisionDirection });

        if (horizontal.Collided)
        {
            // Moving left or right ends by killing X speed when a collision is detected,
            // which cancels speed boosting and clears the extra-run speed as well.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
            // Restore the ordinary handlers, then
            // The knockback's vertical collision sees the still-set collision flag,
            // clears the movement and aligns the feet to the previous pose. The push
            // never sets knockback direction, so the knockback check does not convert the
            // hurt pose; the knockback pose remains for the ordinary movement handler.
            IsActive = false;
            PushDirection = CeresRidleyShoveDirection.None;
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.None;
            samus.AlignBottomAfterPoseChange(
                SamusPoseMetadata.ReadPoseYRadius(samus.PoseHistory.PreviousPose),
                SamusPoseMetadata.ReadPoseYRadius(samus.Pose));
            return new CeresRidleyEjectionResult();
        }

        // Accelerates only while the signed whole speed is below five. The
        // resulting 16.16 speed is passed directly to MoveDown; unlike the common falling
        // wrapper, this path adds no extra one-pixel downward bias.
        if (samus.Kinematics.YSpeed.Whole < TerminalDownwardSpeed)
            samus.Kinematics.YSpeed += samus.Kinematics.YAcceleration;
        SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level }, samus.Kinematics, samus, new VerticalBlockMove { Displacement = samus.Kinematics.YSpeed.AsDisplacement(), ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });

        return new CeresRidleyEjectionResult();
    }
}
