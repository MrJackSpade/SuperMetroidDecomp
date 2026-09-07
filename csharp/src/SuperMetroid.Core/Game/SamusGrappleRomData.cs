namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge definitions used by Samus's Grapple Beam subsystem.</summary>
/// <remarks>
/// Grapple runtime state deliberately remains elsewhere. This type names the ROM tables,
/// callback offsets, native physics coefficients, and fixed VRAM destinations that define
/// the subsystem's data contract.
/// </remarks>
public static class SamusGrappleRomData
{
    /// <summary>Exact sound queue commands issued by bank-$9B grapple functions.</summary>
    public static class Sounds
    {
        /// <summary>$9B:C51E firing tail: QueueSfx1_Max1(5), start the extending beam.</summary>
        public static readonly SamusSoundRequest Fire = new(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 5), 1);
        /// <summary>$9B:C703 accepted-contact tail: QueueSfx1_Max6(6), attached beam sound.</summary>
        public static readonly SamusSoundRequest Attach = new(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 6), 6);
        /// <summary>$9B:C856/C8C5/C9CE/CB8B: QueueSfx1_Max15(7), stop the active grapple sound.</summary>
        public static readonly SamusSoundRequest Stop = new(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 7), 15);
    }
    /// <summary>Native banks used by grapple's same-bank pointers.</summary>
    public static class Banks
    {
        /// <summary>Bank containing flare animation delays.</summary>
        public const int Movement = 0x900000;
        /// <summary>Bank containing flare spritemap offsets.</summary>
        public const int Projectile = 0x930000;
        /// <summary>Bank containing Grapple graphics and most Grapple tables.</summary>
        public const int Grapple = 0x9b0000;
        /// <summary>Bank containing Grapple character graphics.</summary>
        public const int CharacterData = 0x9a0000;
    }

    /// <summary>Native angular-physics coefficients embedded in bank-$9B behavior.</summary>
    public static class Physics
    {
        /// <summary>Per-frame downward angular acceleration.</summary>
        public const short GravityMagnitude = 24;
        /// <summary>Per-frame angular acceleration contributed by directional input.</summary>
        public const short DirectionInputMagnitude = 12;
        /// <summary>Correction applied when velocity opposes the current swing direction.</summary>
        public const short VelocityCorrectionMagnitude = 5;
        /// <summary>Maximum signed angular velocity.</summary>
        public const short MaximumAngularVelocity = 0x0480;
        /// <summary>Angular impulse applied by Jump during a swing.</summary>
        public const short JumpImpulseMagnitude = 0x0300;
        /// <summary><c>$A0:B3C3</c>, signed sine values indexed by <c>SnesAngle</c>.</summary>
        public const int SignedSineTable = 0xa0b3c3;
    }

    /// <summary>Direction-indexed firing velocity, origin, and flare tables.</summary>
    public static class Firing
    {
        /// <summary>Initial X velocities for ten shot directions.</summary>
        public const int XVelocities = 0x9bc0db;
        /// <summary>Initial Y velocities for ten shot directions.</summary>
        public const int YVelocities = 0x9bc0ef;
        /// <summary>Initial angles for ten shot directions.</summary>
        public const int Angles = 0x9bc104;
        /// <summary>Non-running grapple-point X origins.</summary>
        public const int DefaultOriginX = 0x9bc122;
        /// <summary>Non-running grapple-point Y origins.</summary>
        public const int DefaultOriginY = 0x9bc136;
        /// <summary>Non-running flare X origins.</summary>
        public const int DefaultFlareX = 0x9bc14a;
        /// <summary>Non-running flare Y origins.</summary>
        public const int DefaultFlareY = 0x9bc15e;
        /// <summary>Running grapple-point X origins.</summary>
        public const int RunningOriginX = 0x9bc172;
        /// <summary>Running grapple-point Y origins.</summary>
        public const int RunningOriginY = 0x9bc186;
        /// <summary>Running flare X origins.</summary>
        public const int RunningFlareX = 0x9bc19a;
        /// <summary>Running flare Y origins.</summary>
        public const int RunningFlareY = 0x9bc1ae;
        /// <summary>Main flare animation delays in bank $90.</summary>
        public const int MainFlareAnimationDelays = 0x90c487;
        /// <summary>Right-facing flare spritemap offsets in bank $93.</summary>
        public const int RightFlareSpritemapOffsets = 0x93a225;
        /// <summary>Left-facing flare spritemap offsets in bank $93.</summary>
        public const int LeftFlareSpritemapOffsets = 0x93a22b;
        /// <summary>Number of direction records represented by every firing table.</summary>
        public const int DirectionCount = 10;
    }

    /// <summary>Swing-frame, body-offset, rope-character, and fixed upload definitions.</summary>
    public static class Rendering
    {
        /// <summary>Animation frame selected by each high byte of swing angle.</summary>
        public const int SwingFrameByAngle = 0x9bc1c2;
        /// <summary>Left-facing body offsets indexed by swing frame.</summary>
        public const int LeftPoseOffsetsByFrame = 0x9bc2c2;
        /// <summary>Right-facing body offsets indexed by swing frame.</summary>
        public const int RightPoseOffsetsByFrame = 0x9bc302;
        /// <summary>Pointer to the Grapple point's character data.</summary>
        public const int PointTilePointers = 0x9bc342;
        /// <summary>Pointers to folded-angle rope-segment character data.</summary>
        public const int SegmentTilePointers = 0x9bc346;
        /// <summary>Bytes uploaded for the single Grapple point character.</summary>
        public const ushort PointTileByteCount = 0x20;
        /// <summary>Encoded VRAM destination for the Grapple point character.</summary>
        public const ushort PointTileVramDestination = 0x6200;
        /// <summary>Bytes uploaded for four Grapple segment characters.</summary>
        public const ushort SegmentTileByteCount = 0x80;
        /// <summary>Encoded VRAM destination for Grapple segment characters.</summary>
        public const ushort SegmentTileVramDestination = 0x6210;
    }

    /// <summary>Special connection-angle records and their native handler identities.</summary>
    public static class Connections
    {
        /// <summary>Default pose/angle connection records.</summary>
        public const int DefaultTable = 0x9bc3c6;
        /// <summary>Connection records used while moving vertically.</summary>
        public const int MovingVerticallyTable = 0x9bc3ee;
        /// <summary>Connection records used while crouching.</summary>
        public const int CrouchingTable = 0x9bc416;
        /// <summary>Special-angle records used for fixed, swing, and wall-grab reactions.</summary>
        public const int SpecialAngleTable = 0x9bc43e;
        /// <summary>Bytes in one special-angle record.</summary>
        public const int SpecialAngleRecordByteCount = 10;
        /// <summary>Bank-$9B locked-in-place connection handler.</summary>
        public const ushort LockedInPlaceHandler = 0xc77e;
        /// <summary>Bank-$9B ordinary swinging connection handler.</summary>
        public const ushort SwingingHandler = 0xc79d;
        /// <summary>Bank-$9B wall-grab connection handler.</summary>
        public const ushort WallGrabHandler = 0xc814;
        /// <summary>Bank-$9B clockwise swing-pose connection handler.</summary>
        public const ushort SwingClockwiseHandler = 0xb9d9;
        /// <summary>Bank-$9B anticlockwise swing-pose connection handler.</summary>
        public const ushort SwingAnticlockwiseHandler = 0xb9e2;
        /// <summary>Bank-$9B standing up-right connection handler.</summary>
        public const ushort StandingUpRightHandler = 0xb9ea;
        /// <summary>Bank-$9B standing right connection handler.</summary>
        public const ushort StandingRightHandler = 0xb9f3;
        /// <summary>Bank-$9B standing down connection handler.</summary>
        public const ushort StandingDownHandler = 0xb9fc;
        /// <summary>Bank-$9B standing up-left connection handler.</summary>
        public const ushort StandingUpLeftHandler = 0xba05;
        /// <summary>Bank-$9B crouching up-right connection handler.</summary>
        public const ushort CrouchingUpRightHandler = 0xba0e;
        /// <summary>Bank-$9B crouching right connection handler.</summary>
        public const ushort CrouchingRightHandler = 0xba17;
        /// <summary>Bank-$9B crouching down-left connection handler.</summary>
        public const ushort CrouchingDownLeftHandler = 0xba20;
        /// <summary>Bank-$9B crouching up-left connection handler.</summary>
        public const ushort CrouchingUpLeftHandler = 0xba29;
        /// <summary>Number of shot directions represented by each four-byte connection table.</summary>
        public const int DirectionCount = Firing.DirectionCount;
        /// <summary>Number of fixed-angle special connection records.</summary>
        public const int SpecialAngleRecordCount = 8;
    }

    /// <summary>Pose tables selected when an interrupted grapple drops Samus.</summary>
    public static class Release
    {
        /// <summary>Ten direction-indexed standing release poses.</summary>
        public const int StandingPoseTable = 0x9bc9ba;
        /// <summary>Ten direction-indexed crouching release poses.</summary>
        public const int CrouchingPoseTable = 0x9bc9c4;
    }
}
