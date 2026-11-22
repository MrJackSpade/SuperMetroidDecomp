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
public sealed partial class SamusCeresRidleyEjectionState
{
    /// <summary>True while the shove holds Samus's movement.</summary>
    public bool IsActive => stage is not ShoveStage.Idle;

    /// <summary>True until the first shove update installs pose and velocity.</summary>
    public bool IsStarting => stage is ShoveStage.Starting;

    /// <summary>Where the shove stands (#2554): not shoving, starting, or carrying Samus toward the wall its start chose.</summary>
    private ShoveStage stage = ShoveStage.Idle.Instance;

    /// <summary>The stages of the shove, each holding what it needs.</summary>
    private abstract partial record ShoveStage
    {
        /// <summary>No shove holds Samus.</summary>
        internal sealed partial record Idle : ShoveStage
        {
            /// <summary>The one idle stage.</summary>
            internal static Idle Instance { get; } = new();
        }

        /// <summary>The shove was requested; its first update installs her pose and velocity and chooses its wall.</summary>
        internal sealed partial record Starting : ShoveStage
        {
            /// <summary>The one starting stage.</summary>
            internal static Starting Instance { get; } = new();
        }

        /// <summary>The shove carries Samus toward <paramref name="Toward"/> until she meets it.</summary>
        /// <param name="Toward">The wall the shove carries her toward.</param>
        internal sealed partial record Pushing(HorizontalDirection Toward) : ShoveStage;
    }

    /// <summary>
    /// Sets Samus to be pushed out of Ceres Ridley's way: the shove takes her movement
    /// from the following gameplay frame.
    /// </summary>
    public void Request(SamusState samus)
    {
        Ensure.NotNull(samus);
        stage = ShoveStage.Starting.Instance;

        // The shove takes Samus's movement. An
        // uncrashed ordinary spark consequently keeps its boost as Blue Suit. If
        // Crystal Flash's raise/drain/finish held her, its
        // independent suit colouring and its lock on her pose input survive instead. Neither
        // displaced movement owner can resume after Ridley's shove ends.
        SamusMovementHandOver.ToCeresRidleyEjection(samus);

        // The shove takes only her movement; her control stays unlocked. Her held chords still choose poses and her
        // weapons and HUD still answer, and while the shove lasts the runtime discards any damage boost she would take.
    }

    /// <summary>Runs one update of the shove.</summary>
    internal void Step(
        SamusTerrain terrain,
        SamusState samus,
        int cameraX,
        int frameCounter)
    {
        Ensure.NotNull(terrain);
        Ensure.NotNull(samus);
        if (stage is ShoveStage.Idle)
            throw new InvalidOperationException("Ceres Ridley ejection is not active.");

        if (stage is not ShoveStage.Pushing pushing)
        {
            // The ejection selects the ordinary hurt pose from the PREVIOUS pose direction and
            // refreshes the animation. The pose setup does not publish the hurt pose's
            // radius, so the alignment from the source pose's live radius to the hurt pose's uses that live radius; the
            // ordinary pose change publishes the hurt pose's radius on the following frame.
            bool facingLeft = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Left;
            samus.ChangePose(facingLeft
                ? SamusPoseId.KnockbackLeftPose
                : SamusPoseId.KnockbackRightPose);
            // The shove's first update shifts her pose history, once. Moving her centre by the hurt pose's radius less
            // her live one keeps her in Ceres Ridley's room, whose top row is solid: her top lies at least a block below
            // the room's, and the move is at most 14 pixels either way.
            samus.CommitPoseHistory();
            samus.Position = samus.Position.WithWholeY(
                samus.Position.Y.Whole - (SamusPoseMetadata.ReadPoseYRadius(samus.Pose) - samus.Kinematics.Radius.Y));

            // The side is selected from Samus's place on the screen, not in the room. The exact centre belongs to the
            // rightward shove.
            int screenX = samus.Position.X.Whole - cameraX;
            stage = new ShoveStage.Pushing(screenX < ScreenGeometry.ScreenWidthPixels / 2
                ? HorizontalDirection.Left
                : HorizontalDirection.Right);
            samus.Kinematics.VerticalVelocity = samus.Kinematics.VerticalVelocity.WithSpeed(SamusVerticalMotionDefinitions.CeresRidleyShoveFallingSpeed);

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
        var requestedX = SamusHorizontalDisplacement.Toward(
            pushing.Toward, samus, baseSpeed);
        BlockMoveResult horizontal = SamusBlockCollision.MoveHorizontal(terrain, samus.Kinematics, samus, samus.Pose, new HorizontalBlockMove { Displacement = requestedX.Displacement, CollisionMovementDirection = requestedX.CollisionDirection });

        if (horizontal.Collided)
        {
            // Moving left or right ends by killing X speed when a collision is detected,
            // which cancels speed boosting and clears the extra-run speed as well.
            samus.HorizontalSpeed.ClearHorizontalMomentum(SamusPoseMetadata.ReadFacingDirection(samus));
            // Return her movement to her pose: the knockback lands her and aligns her feet to her earlier pose. The
            // push gives no knockback direction, so her hurt pose is kept, and the knockback pose stays for her pose's
            // own movement.
            stage = ShoveStage.Idle.Instance;
            samus.Kinematics.VerticalVelocity = SamusVerticalVelocity.Still;
            samus.AlignBottomAfterPoseChange(
                SamusPoseMetadata.ReadPoseYRadius(samus.PoseHistory.PreviousPose),
                SamusPoseMetadata.ReadPoseYRadius(samus.Pose));
            return;
        }

        // Her fall speeds up only while its whole pixels are below the shove's falling speed, and she falls by that speed alone, whichever way
        // her velocity heads and without the extra pixel an ordinary fall adds.
        UnsignedSixteenSixteen fallSpeed = samus.Kinematics.VerticalVelocity.Speed;
        if (fallSpeed.IsSlowerThan(SamusVerticalMotionDefinitions.CeresRidleyShoveFallingSpeed))
            samus.Kinematics.VerticalVelocity = samus.Kinematics.VerticalVelocity.WithSpeed(fallSpeed + samus.Kinematics.YAcceleration);
        SamusBlockCollision.MoveVertical(terrain, samus.Kinematics, samus, samus.Pose, new VerticalBlockMove { Displacement = SamusVerticalVelocity.Falling(samus.Kinematics.VerticalVelocity.Speed).Displacement, ScanLeftToRight = SamusCadenceDefinitions.BlockScansLeftToRight.IsDue(frameCounter) });
    }

    /// <summary>Where Ceres Ridley's shove stands: inactive, starting, or pushing toward its side.</summary>
    private abstract partial record Shove
    {
        /// <summary>No shove holds her.</summary>
        internal static Shove Inactive { get; } = new InactiveShove();

        /// <summary>The shove has been requested and starts on its next update.</summary>
        internal static Shove Starting { get; } = new StartingShove();

        /// <summary>No shove holds her.</summary>
        private sealed partial record InactiveShove : Shove;

        /// <summary>The shove starts on its next update.</summary>
        private sealed partial record StartingShove : Shove;
    }

    /// <summary>The shove pushes her toward <paramref name="Side"/>.</summary>
    /// <param name="Side">The side it pushes her toward.</param>
    private sealed partial record Pushing(HorizontalDirection Side) : Shove;
}
