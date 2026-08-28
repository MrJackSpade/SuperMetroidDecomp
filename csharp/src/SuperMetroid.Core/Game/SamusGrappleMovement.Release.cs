using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Pendulum positioning, swing release velocity, cleanup, and ROM-table helpers.
/// </summary>
public static partial class SamusGrappleMovement
{
    private static void PositionSamusFromPendulum(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $94:AC11 calls the same position helper used by terrain probes. Besides scaling the
        // signed table components, it applies the block-side 7/8 endpoint bias documented in
        // CalculateCollisionPoint; using one helper prevents visible art from disagreeing
        // with the collision body by one pixel.
        GrappleCollisionPoint ropeStart = CalculateCollisionPoint(
            bus,
            grapple,
            unchecked((byte)(grapple.Angle >> 8)),
            grapple.RopeLength);
        grapple.RopeStartX = ropeStart.X;
        grapple.RopeStartY = ropeStart.Y;

        // $9B:BD95 copies native Start to native Flare during swinging. BeamStart retains
        // the older public name because the renderer and debugger already consume it, but
        // it semantically represents the Flare/draw origin throughout this translation.
        grapple.BeamStartX = ropeStart.X;
        grapple.BeamStartY = ropeStart.Y;

        // $9B:BD95 maps all 256 angle bytes onto the authentic swing-art frame, then adds
        // a frame-specific origin correction so Samus's hand remains attached to the beam.
        byte artFrame = bus.ReadByte(SwingFrameByAngle + (grapple.MirroredAngle >> 8));
        int offsetAddress = SamusState.IsFacingLeft(bus, samus.Pose)
            ? LeftPoseOffsetsByFrame
            : RightPoseOffsetsByFrame;
        int pairAddress = offsetAddress + artFrame * 2;
        sbyte xOffset = unchecked((sbyte)bus.ReadByte(pairAddress));
        sbyte yOffset = unchecked((sbyte)bus.ReadByte(pairAddress + 1));

        samus.SetGrappleSwingAnimationFrame(artFrame);
        samus.XPosition = unchecked((ushort)(ropeStart.X + xOffset));
        samus.YPosition = unchecked((ushort)(ropeStart.Y + yOffset));
    }

    private static void PropelSamusFromSwing(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        int absoluteVelocity = Math.Abs((int)grapple.AngularVelocity);
        int doubledVelocity = absoluteVelocity * 2;
        short cosine = ReadSignedSine(bus, (grapple.Angle >> 8) + 64);

        uint verticalFixed = unchecked((uint)(Math.Abs(cosine) * doubledVelocity));
        samus.Kinematics.YSpeed = unchecked((ushort)(verticalFixed >> 16));
        samus.Kinematics.YSubspeed = unchecked((ushort)verticalFixed);

        // $9B:CA65 selects Y direction from both angular-velocity sign and cosine sign.
        // Keeping that branch literal avoids deriving a screen-coordinate convention.
        bool movingDown = grapple.AngularVelocity >= 0 ? cosine >= 0 : cosine < 0;
        samus.Kinematics.YDirection = movingDown ? (ushort)2 : (ushort)1;

        samus.HorizontalSpeed.AccelerationMode = 2;
        int phaseOffset = 64 - 3 * (doubledVelocity >> 9);
        byte horizontalAngle = unchecked((byte)((grapple.Angle >> 8) - phaseOffset));
        int horizontalSine = Math.Abs(ReadSignedSine(bus, horizontalAngle + 64));
        uint horizontalFixed = unchecked((uint)(horizontalSine * doubledVelocity));
        samus.HorizontalSpeed.BaseSpeed = unchecked((ushort)(horizontalFixed >> 16));
        samus.HorizontalSpeed.BaseSubspeed = unchecked((ushort)horizontalFixed);
    }

    private static void CompleteQueuedRelease(
        ISnesAddressSpace bus,
        SamusState samus,
        SamusGrappleState grapple)
    {
        // $9B:CB8B bases facing on the angular-velocity word retained from the swing. A
        // nonnegative value selects left-facing $52; a negative value selects right $51.
        samus.Pose = grapple.AngularVelocity >= 0
            ? SamusState.NormalJumpForwardLeftPose
            : SamusState.NormalJumpForwardRightPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, initialFrame: 0);
        grapple.Phase = GrapplePhase.Inactive;
        grapple.DirectionInputAcceleration = 0;
        grapple.GravityAcceleration = 0;
        grapple.VelocityCorrection = 0;
        grapple.JumpImpulse = 0;
        grapple.RopeLengthDelta = 0;
        grapple.CollisionBounceTimer = 0;
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        ClearFlareAnimation(grapple);
    }

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
        grapple.WallJumpTimer = 0;
        grapple.CancelFromConnectedPose = false;
        grapple.ValidateAnchorBlock = false;
        grapple.ValidateAnchorEnemy = false;
        ClearFlareAnimation(grapple);
    }

    private static void ClearFlareAnimation(SamusGrappleState grapple)
    {
        // `$9B:C856/$C8C5/$C9CE/$CB8B` all clear these same shared charge/grapple WRAM
        // words before restoring the ordinary draw handler and projectile palette.
        grapple.FlareCounter = 0;
        grapple.FlareAnimationFrame = 0;
        grapple.FlareAnimationTimer = 0;
    }

    private static int ScaleCoordinate(short sine, int length) => sine switch
    {
        -256 => -length,
        256 => length,
        < 0 => -((-sine * length) >> 8),
        _ => (sine * length) >> 8,
    };

    private static short ReadSignedSine(ISnesAddressSpace bus, int index)
    {
        int address = SignedSineTable + index * 2;
        return unchecked((short)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
