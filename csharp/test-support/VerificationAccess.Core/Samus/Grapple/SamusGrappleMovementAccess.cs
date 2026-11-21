using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Movement;
namespace SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Poses;

/// <summary>Verification access to <see cref="SamusGrappleMovement"/> members production does not use.</summary>
internal static class SamusGrappleMovementAccess
{
    /// <summary>Installs an already-accepted grapple anchor into the connected swing state and publishes its first position.</summary>
    extension(SamusGrappleMovement)
    {
        /// <summary>
        /// Installs an already-connected swinging state, equivalent to the airborne branch of
        /// <c>HandleConnectingGrapple</c> at $9B:B97C followed by $9B:BA61.
        /// </summary>
        /// <remarks>
        /// Anchor acquisition is intentionally outside this method: the caller must supply the
        /// exact grapple point selected by the beam/block collision pass. This is the same kind
        /// of narrow published-state seam used by the translated knockback and bomb-overlap code.
        /// </remarks>
        internal static void ConnectUnobstructedSwing(
            SamusState samus,
            Position16<RoomPixels> anchor,
            byte ropeLength,
            RotationAngle angle,
            AngularSpeed angularVelocity,
            bool faceRight)
        {
            Ensure.NotNull(samus);
            if (ropeLength is < 8 or > 63)
                throw new ArgumentOutOfRangeException(nameof(ropeLength), "Retail connected rope length is 8..63 pixels.");
            if (angularVelocity.Magnitude > GrappleBeamDefinitions.Physics.MaximumAngularVelocity)
                throw new ArgumentOutOfRangeException(nameof(angularVelocity));
            if (samus.Grapple.Phase != GrapplePhase.Inactive)
                throw new InvalidOperationException("A grapple state is already active.");

            SamusGrappleState grapple = samus.Grapple;
            grapple.Phase = GrapplePhase.ConnectedSwinging;
            grapple.Anchor = anchor;
            grapple.RopeLength = ropeLength;
            grapple.RopeLengthDelta = 0;
            grapple.Angle = angle;
            grapple.MirroredAngle = angle;
            grapple.AngularVelocity = angularVelocity;
            grapple.DirectionInputAcceleration = AngularSpeed.Zero;
            grapple.GravityAcceleration = AngularSpeed.Zero;
            grapple.JumpImpulse = AngularSpeed.Zero;
            grapple.CollisionBounceTimer = UpdateCountdown.Ended();
            grapple.Submerged = false;
            // This public helper is the debugger's already-accepted-anchor seam. Its caller may
            // deliberately place an anchor where the current room has no grapple block (the
            // Landing Site regression does exactly that), so only a beam acquired through the
            // block dispatcher opts into bank-$9B's per-frame connection revalidation.
            grapple.ValidatedAnchor = null;
            grapple.SwingFreely();
            grapple.WallJumpTimer = UpdateCountdown.Ended();
            SamusGrappleBeamDrawing.InitializeBeamAnimation(grapple);

            samus.Pose = faceRight
                ? SamusPoseId.GrappleSwingRightPose
                : SamusPoseId.GrappleSwingLeftPose;
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);

            // $94:AC11 and $9B:BD95 run as soon as the connection is accepted. Publishing the
            // first pendulum position now prevents one frame of stale pre-grapple coordinates.
            SamusGrappleSwing.PositionSamusFromPendulum(samus, grapple);

            // This helper represents a connection that has already passed `$9B:C79D`; publish
            // the handler-tail flag that retail would leave for the following swing frame.
            SamusGrappleMovement.RefreshLiquidPhysicsFlag(samus);
        }
    }

    /// <summary>
    /// Commits the pose a grapple step left pending, as the frame's pose update commits it when no higher-priority
    /// transition outranks it: the drop pose, the release pose or the connection pose.
    /// </summary>
    extension(GrappleMovementResult self)
    {
        /// <summary>Commits the step's pending pose through the production commit the frame's pose update calls.</summary>
        /// <returns>The camera's previous position a connection pose snaps to, or none when the step left no connection.</returns>
        internal Position16<RoomPixels>? CommitPendingPose(SamusState samus, RoomLevelData level, int frameCounter, ISamusRoomObjects roomObjects)
        {
            if (self.PendingDropPose is { } dropPose)
            {
                SamusAerialPoseTransitions.ApplyGrappleDropTransition(samus, level, dropPose, frameCounter, roomObjects);
                return null;
            }
            if (self.PendingReleasePose is { } releasePose)
            {
                SamusGrappleMovement.ApplyReleasePose(samus, releasePose);
                return null;
            }
            return self.PendingConnection is { } connection
                ? SamusGrappleMovement.ApplyPendingConnectionPose(samus, connection)
                : null;
        }
    }

    /// <summary>Runs one grapple step followed by the commit of the pose it leaves pending.</summary>
    extension(SamusGrappleMovement)
    {
        /// <summary>
        /// Runs <see cref="SamusGrappleMovement.StepFiring"/> and commits the connection pose it leaves pending, as a
        /// frame whose pose update nothing outranks does.
        /// </summary>
        internal static GrappleMovementResult StepFiringAndCommit(
            RoomLevelData level,
            SamusState samus,
            GameplayAction controllerInput,
            ISamusRoomObjects roomObjects,
            Func<Position16<RoomPixels>, GrappleEnemyCollision?> enemyCollision)
        {
            GrappleMovementResult step = SamusGrappleMovement.StepFiring(level, samus, controllerInput, roomObjects, enemyCollision);
            step.CommitPendingPose(samus, level, frameCounter: 0, roomObjects);
            return step;
        }

        /// <summary>
        /// Runs <see cref="SamusGrappleMovement.Step"/> and commits the release or drop pose it leaves pending, as a
        /// frame whose pose update nothing outranks does.
        /// </summary>
        internal static GrappleMovementResult StepAndCommit(
            SamusTerrain terrain,
            SamusState samus,
            GameplayAction controllerInput,
            GameplayAction newlyPressedInput,
            int frameCounter,
            Func<Position16<RoomPixels>, GrappleEnemyCollision?> enemyCollision)
        {
            GrappleMovementResult step = SamusGrappleMovement.Step(terrain, samus, controllerInput, newlyPressedInput, enemyCollision);
            step.CommitPendingPose(samus, terrain.Level, frameCounter, terrain.RoomObjects);
            return step;
        }
    }
}
