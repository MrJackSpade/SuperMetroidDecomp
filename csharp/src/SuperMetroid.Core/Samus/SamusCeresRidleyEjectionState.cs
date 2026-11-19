using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Core.Samus;

/// <summary>
/// The two-stage shove Ceres Ridley's turning-floor getaway gives Samus, which holds her movement
/// (<see cref="SamusMovementOwner.CeresRidleyEjection"/>).
/// </summary>
/// <remarks>
/// The request is made from room-main code, after Samus has already moved for that
/// frame, so the shove first moves her on the following gameplay frame.
/// </remarks>
public sealed class SamusCeresRidleyEjectionState
{
    /// <summary>True while the shove holds Samus's movement.</summary>
    public bool IsActive { get; private set; }

    /// <summary>True until the first shove update installs pose and velocity.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "SME6470", Justification = "The shove's first update installs Samus's pose and velocity: a state of the shove's own course, kept and cleared by the shove alone.")]
    public bool InitializationPending { get; private set; }

    /// <summary>
    /// The wall the shove carries Samus toward; <see cref="CeresRidleyShoveDirection.None"/>
    /// until initialization chooses a side and again once wall contact ends the shove.
    /// </summary>
    public CeresRidleyShoveDirection PushDirection { get; private set; }

    /// <summary>
    /// Sets Samus to be pushed out of Ceres Ridley's way: the shove takes her movement
    /// from the following gameplay frame.
    /// </summary>
    public void Request(SamusState samus)
    {
        Ensure.NotNull(samus);
        IsActive = true;
        InitializationPending = true;

        // The shove takes Samus's movement. An
        // uncrashed ordinary spark consequently keeps its boost as Blue Suit. If
        // Crystal Flash's raise/drain/finish held her, its
        // independent suit colouring and its lock on her pose input survive instead. Neither
        // displaced movement owner can resume after Ridley's shove ends.
        SamusMovementHandOver.To(samus, SamusMovementOwner.CeresRidleyEjection);

        // The shove takes only her movement; her control stays unlocked. Her held chords still choose poses and her
        // weapons and HUD still answer, and while the shove lasts the runtime discards any damage boost she would take.
    }

    /// <summary>Runs one update of the shove.</summary>
    internal void Step(
        RoomLevelData level,
        ISamusRoomObjects roomObjects,
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

            // The ejection selects the ordinary hurt pose from the PREVIOUS pose direction and
            // refreshes the animation. The pose setup does not publish the hurt pose's
            // radius, so the alignment from the source pose's live radius to the hurt pose's uses that live radius; the
            // ordinary pose change publishes the hurt pose's radius on the following frame.
            bool facingLeft = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Left;
            samus.Pose = facingLeft
                ? SamusPoseId.KnockbackLeftPose
                : SamusPoseId.KnockbackRightPose;
            SamusPresentation.InitializeAnimation(samus);
            // The shove's first update shifts her pose history, once. Moving her centre by the hurt pose's radius less
            // her live one keeps her in Ceres Ridley's room, whose top row is solid: her top lies at least a block below
            // the room's, and the move is at most 14 pixels either way.
            samus.CommitPoseHistory();
            samus.Position = samus.Position.WithWholeY(
                samus.Position.Y.Whole - (SamusPoseMetadata.ReadPoseYRadius(samus.Pose) - samus.Kinematics.Radius.Y));

            // The side is selected from Samus's place on the screen, not in the room. The exact centre belongs to the
            // rightward shove.
            int screenX = samus.Position.X.Whole - cameraX;
            PushDirection = screenX < ScreenGeometry.ScreenWidthPixels / 2
                ? CeresRidleyShoveDirection.Left
                : CeresRidleyShoveDirection.Right;
            samus.Kinematics.YSpeed = SamusVerticalMotionDefinitions.CeresRidleyShoveFallingSpeed;

            // Only the shove's state is set up on this update. The shove does not
            // perform the first collision/movement pass until the following frame.
            return;
        }

        // The shove moves her across at the base speed of her pose's movement type (the knockback poses' knockback) with
        // her kept run speeds, toward the shove's own side rather than her facing, so Ridley always throws her toward a
        // wall.
        SamusHorizontalSpeedState horizontalSpeed = samus.HorizontalSpeed;
        horizontalSpeed.SelectSpeedsFor(
            samus.LiquidPhysics.DetermineMovementMedium(samus));
        UnsignedSixteenSixteen baseSpeed = horizontalSpeed.CalculateBaseSpeed(
            SamusPoseMetadata.ReadMovementType(samus));
        var requestedX = SamusHorizontalDisplacement.Toward(PushDirection == CeresRidleyShoveDirection.Left, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedX.Displacement, CollisionMovementDirection = requestedX.CollisionDirection });

        if (horizontal.Collided)
        {
            // Moving left or right ends by killing X speed when a collision is detected,
            // which cancels speed boosting and clears the extra-run speed as well.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
            // Return her movement to her pose. Then
            // the knockback's vertical collision sees the still-set collision flag,
            // clears the movement and aligns the feet to the previous pose. The push
            // never sets knockback direction, so the knockback check does not convert the
            // hurt pose; the knockback pose remains for her pose's own movement.
            IsActive = false;
            PushDirection = CeresRidleyShoveDirection.None;
            samus.Kinematics.YSpeed = default;
            samus.Kinematics.YDirection = SamusVerticalDirection.None;
            samus.AlignBottomAfterPoseChange(
                SamusPoseMetadata.ReadPoseYRadius(samus.PoseHistory.PreviousPose),
                SamusPoseMetadata.ReadPoseYRadius(samus.Pose));
            return;
        }

        // Her fall speeds up only while its whole pixels are below five, and she falls by that speed alone, without the
        // extra pixel an ordinary fall adds.
        if (samus.Kinematics.YSpeed.IsSlowerThan(SamusVerticalMotionDefinitions.CeresRidleyShoveFallingSpeed))
            samus.Kinematics.YSpeed += samus.Kinematics.YAcceleration;
        SamusBlockCollision.MoveVertical(new SamusTerrain { Level = level, RoomObjects = roomObjects }, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = samus.Kinematics.YSpeed.AsDisplacement(), ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
    }
}
