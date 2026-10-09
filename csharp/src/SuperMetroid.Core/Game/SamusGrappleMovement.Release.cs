using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Pendulum positioning, swing release velocity, cleanup, and ROM-table helpers.
/// </summary>
public static partial class SamusGrappleMovement
{
    /// <summary>Queues the requested grapple movement sound through Samus's shared liquid-physics sound queue.</summary>
    /// <param name="samus">Samus state whose sound queue receives the request.</param>
    /// <param name="request">Sound effect and queue limit selected by the caller.</param>
    private static void QueueGrappleSound(SamusState samus, SamusSoundRequest request) =>
        samus.LiquidPhysics.QueueMovementSound(request.SoundEffect, request.MaximumQueued);

    /// <summary>Updates the rope anchor, flare origin, body position, and selected animation frame during a grapple swing.</summary>
    /// <param name="bus">Address space used to resolve Samus's facing state.</param>
    /// <param name="samus">Samus state whose rendered body position and animation are updated.</param>
    /// <param name="grapple">Swing state supplying rope geometry, angle, speed, and installed frame definitions.</param>
    private static void PositionSamusFromPendulum(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $9B:BD9B-BDB1: a fast swing selects the slow-scrolling camera branch.
        ushort angularSpeed = unchecked((ushort)Math.Abs((int)grapple.AngularVelocity));
        grapple.SlowScrolling = unchecked((short)(angularSpeed -
            GrappleSlowScrollDefinitions.FastSwingAngularSpeed)) >= 0;
        // $94:AC11 calls the same position helper used by terrain probes. Besides scaling the
        // signed table components, it applies the block-side 7/8 endpoint bias documented in
        // CalculateCollisionPoint; using one helper prevents visible art from disagreeing
        // with the collision body by one pixel.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            grapple,
            grapple.Angle.TableIndex,
            grapple.RopeLength);
        grapple.RopeStartX = ropeStart.X;
        grapple.RopeStartY = ropeStart.Y;

        // $9B:BD95 copies native Start to native Flare during swinging. BeamStart retains
        // the older public name because the renderer and debugger already consume it, but
        // it semantically represents the Flare/draw origin throughout this translation.
        grapple.BeamStartX = ropeStart.X;
        grapple.BeamStartY = ropeStart.Y;

        // Native shares a selector between physical body placement and displayed art.
        // Keep the authored physical mapping compiled so a visual-frame override cannot
        // move the collision body. Stock art still selects the same native frame.
        byte artFrame = (grapple.SwingFrames ?? throw new InvalidOperationException(
            "Grapple swing requires installed frame definitions."))
            .Resolve(grapple.MirroredAngle.TableIndex);
        var offset = GrappleBodyPlacementDefinitions.Offset(grapple.MirroredAngle.TableIndex,
            SamusState.IsFacingLeft(bus, samus.Pose));

        samus.SetGrappleSwingAnimationFrame(artFrame);
        samus.XPosition = unchecked((ushort)(ropeStart.X + offset.X));
        samus.YPosition = unchecked((ushort)(ropeStart.Y + offset.Y));
    }

    /// <summary>Derives Samus's fixed-point horizontal and vertical launch speeds from the current pendulum velocity and angle.</summary>
    /// <param name="samus">Samus state receiving launch velocity and vertical direction.</param>
    /// <param name="grapple">Swing state supplying angular velocity and angle at release.</param>
    private static void PropelSamusFromSwing(
        SamusState samus,
        SamusGrappleState grapple)
    {
        int absoluteVelocity = Math.Abs((int)grapple.AngularVelocity);
        int doubledVelocity = absoluteVelocity * 2;
        short cosine = ReadSignedSine(
            grapple.Angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);

        uint verticalFixed = unchecked((uint)(Math.Abs(cosine) * doubledVelocity));
        samus.Kinematics.YSpeed = unchecked((ushort)(verticalFixed >> 16));
        samus.Kinematics.YSubspeed = unchecked((ushort)verticalFixed);

        // $9B:CA65 selects Y direction from both angular-velocity sign and cosine sign.
        // Keeping that branch literal avoids deriving a screen-coordinate convention.
        bool movingDown = grapple.AngularVelocity >= 0 ? cosine >= 0 : cosine < 0;
        samus.Kinematics.YDirection = movingDown ? (ushort)2 : (ushort)1;

        samus.HorizontalSpeed.AccelerationMode = 2;
        int phaseOffset = 64 - 3 * (doubledVelocity >> 9);
        byte horizontalAngle = grapple.Angle.AddTableUnits(-phaseOffset).TableIndex;
        int horizontalSine = Math.Abs(ReadSignedSine(horizontalAngle + 64));
        uint horizontalFixed = unchecked((uint)(horizontalSine * doubledVelocity));
        samus.HorizontalSpeed.BaseSpeed = unchecked((ushort)(horizontalFixed >> 16));
        samus.HorizontalSpeed.BaseSubspeed = unchecked((ushort)horizontalFixed);
    }

    /// <returns>
    /// The release pose when <paramref name="deferPoseChange"/> leaves it for the frame's
    /// pose commit; otherwise null, the pose having been applied here.
    /// </returns>
    private static byte? CompleteQueuedRelease(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        SamusGrappleState grapple,
        bool deferPoseChange)
    {
        QueueGrappleSound(samus, SamusGrappleRomData.Sounds.Stop);
        SamusBlockCollision.EjectAfterGrapple(bus, level, samus.Kinematics);
        // $9B:CB8B bases facing on the angular-velocity word retained from the swing. A
        // nonnegative value selects left-facing $52; a negative value selects right $51.
        // It publishes the pose as a super-special prospective pose (command 7), which
        // bank $91 commits after this frame's hit interruption and movement.
        byte releasePose = grapple.AngularVelocity >= 0
            ? SamusPoseIds.NormalJumpForwardLeftPose
            : SamusPoseIds.NormalJumpForwardRightPose;
        if (!deferPoseChange)
            ApplyReleasePose(bus, samus, releasePose);
        grapple.Phase = GrapplePhase.Inactive;
        grapple.SlowScrolling = false; // $9B:CBBA
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.RopeLengthDelta = 0;
        grapple.CollisionBounceTimer = 0;
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        ClearFlareAnimation(grapple);
        return deferPoseChange ? releasePose : null;
    }

    /// <summary>Commits the <c>$9B:CB8B</c> release pose and its radius and animation.</summary>
    internal static void ApplyReleasePose(ISnesAddressSpace bus, SamusState samus, byte releasePose)
    {
        samus.Pose = releasePose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>Resets grapple-owned movement, anchor-validation, wall-jump, and flare state after the connected phase ends.</summary>
    /// <param name="grapple">State object whose connected-phase fields are cleared.</param>
    private static void ClearConnectedGrapple(SamusGrappleState grapple)
    {
        // `$9B:C8C5/$C9CE` share the same long cleanup tail. Palette, sound, and HUD-item
        // producers live outside this state object; every owned movement/rendering word is
        // reset here so a later firing edge cannot inherit wall-grace or collision state.
        grapple.Phase = GrapplePhase.Inactive;
        grapple.RopeLength = 0;
        grapple.RopeLengthDelta = 0;
        grapple.AngularVelocity = 0;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.CollisionBounceTimer = 0;
        grapple.SpecialAngleHandling = false;
        // $9B:C979 (dropped) and $9B:CA24 (wall jumping) clear the slow-scroll flag.
        grapple.SlowScrolling = false;
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        ClearFlareAnimation(grapple);
    }

    /// <summary>Clears the shared charge/grapple flare counter and animation progression fields.</summary>
    /// <param name="grapple">Grapple state whose flare animation fields are reset.</param>
    private static void ClearFlareAnimation(SamusGrappleState grapple)
    {
        // `$9B:C856/$C8C5/$C9CE/$CB8B` all clear these same shared charge/grapple WRAM
        // words before restoring the ordinary draw handler and projectile palette.
        grapple.FlareCounter = 0;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = 0;
    }

    /// <summary>Scales an SNES signed sine-table component by a rope length using the native signed fixed-point rounding behavior.</summary>
    /// <param name="sine">Signed table component, including the exact endpoints -256 and 256.</param>
    /// <param name="length">Rope length in pixels.</param>
    /// <returns>The signed pixel displacement along the coordinate axis.</returns>
    private static int ScaleCoordinate(short sine, int length) => sine switch
    {
        -256 => -length,
        256 => length,
        < 0 => -((-sine * length) >> 8),
        _ => (sine * length) >> 8,
    };

    /// <summary>Reads the signed negative-cosine table word used by the grapple's native angle calculations.</summary>
    /// <param name="index">Table index, with normal angle wrapping applied by the table reader.</param>
    /// <returns>The signed 16-bit trigonometric component.</returns>
    private static short ReadSignedSine(int index)
    {
        return EnemyTrigonometryTables.SignedNegativeCosineWord(index);
    }

}
