using SuperMetroid.Core;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

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
            ISnesAddressSpace bus,
            SamusState samus,
            Position16<RoomPixels> anchor,
            byte ropeLength,
            SnesAngle angle,
            short angularVelocity,
            bool faceRight)
        {
            Ensure.NotNull(bus);
            Ensure.NotNull(samus);
            if (ropeLength is < 8 or > 63)
                throw new ArgumentOutOfRangeException(nameof(ropeLength), "Retail connected rope length is 8..63 pixels.");
            if (angularVelocity is < -SamusGrappleRomData.Physics.MaximumAngularVelocity or
                > SamusGrappleRomData.Physics.MaximumAngularVelocity)
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
            grapple.DirectionInputAcceleration = 0;
            grapple.GravityAcceleration = 0;
            grapple.JumpImpulse = 0;
            grapple.CollisionBounceTimer = NativeCountdown.Start(0);
            grapple.Submerged = false;
            // This public helper is the debugger's already-accepted-anchor seam. Its caller may
            // deliberately place an anchor where the current room has no grapple block (the
            // Landing Site regression does exactly that), so only a beam acquired through the
            // block dispatcher opts into bank-$9B's per-frame connection revalidation.
            grapple.ValidateAnchorBlock = false;
            grapple.ValidateAnchorEnemy = false;
            grapple.SpecialAngleHandling = false;
            grapple.WallJumpTimer = NativeCountdown.Start(0);
            grapple.CancelFromConnectedPose = false;
            SamusGrappleMovement.InitializeBeamAnimation((SamusGrappleState)(grapple));

            samus.Pose = faceRight
                ? SamusPoseId.GrappleSwingRightPose
                : SamusPoseId.GrappleSwingLeftPose;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus, initialFrame: 0);

            // $94:AC11 and $9B:BD95 run as soon as the connection is accepted. Publishing the
            // first pendulum position now prevents one frame of stale pre-grapple coordinates.
            SamusGrappleMovement.PositionSamusFromPendulum((ISnesAddressSpace)(bus), (SamusState)(samus), (SamusGrappleState)(grapple));

            // This helper represents a connection that has already passed `$9B:C79D`; publish
            // the handler-tail flag that retail would leave for the following swing frame.
            SamusGrappleMovement.RefreshLiquidPhysicsFlag(samus);
        }
    }
}
