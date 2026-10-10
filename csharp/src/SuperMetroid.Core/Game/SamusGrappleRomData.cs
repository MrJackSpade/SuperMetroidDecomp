using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>Immutable cartridge definitions used by Samus's Grapple Beam subsystem.</summary>
/// <remarks>
/// Grapple runtime state deliberately remains elsewhere. This type names the ROM tables,
/// callback offsets, native physics coefficients, and fixed VRAM destinations that define
/// the subsystem's data contract.
/// </remarks>
public static class SamusGrappleRomData
{
    /// <summary>Palette operands shared by grapple initialization and native cleanup tails.</summary>
    public static class Palettes
    {
        /// <summary>$9B:C67F selects beam palette two before entering the firing function.</summary>
        public const int FiringSelection = 2;
        /// <summary>$9B:C686 writes $7F91 to sprite palette five, color fifteen.</summary>
        public static Bgr555 FlareColor => Bgr555.FromWord(0x7f91);
        /// <summary>$9B:C689 destination, immediately before the beam's sprite palette six.</summary>
        public const int FlareColorIndex = SamusProjectileRomData.Palettes.BeamDestinationIndex - 1;
        /// <summary>$90:ACFC Load_Beam_Palette masks EquippedBeams to its low twelve bits.</summary>
        public const ushort EquippedSelectionMask = 0x0fff;
    }

    /// <summary>Exact sound queue commands issued by bank-$9B grapple functions.</summary>
    public static class Sounds
    {
        /// <summary>$9B:B8AA: QueueSfx1_Max6(7), stop the old beam before pose-change refiring.</summary>
        public static readonly SamusSoundRequest RestartStop = new(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 7), 6);
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
        /// <summary>Bank containing Grapple character graphics.</summary>
        public const int CharacterData = 0x9a0000;
    }

    /// <summary>Native angular-physics coefficients embedded in bank-$9B behavior.</summary>
    public static class Physics
    {
        /// <summary><c>$9B:C745-C74C</c>, firing handler's accepted-connection tail: automatically retract eight pixels per frame after the connection helper clears its delta.</summary>
        public const short InitialConnectionRetraction = -8;
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
    }

    /// <summary>Direction-indexed firing velocity, origin, and flare tables.</summary>
    public static class Firing
    {
        /// <summary>$9B:C51E initializes GrappleBeam_PoseChangeAutoFireTimer to ten; $C490 decrements before checking the pose.</summary>
        public const ushort PoseChangeAutoFireFrames = 10;
        /// <summary>$9B:C51E uses a six-pixel graphics Y offset for moving Draygon-held poses.</summary>
        public const sbyte DraygonMovingGraphicsYOffset = 6;
        /// <summary>Non-running flare X origins.</summary>
        public const int DefaultFlareX = 0x9bc14a;
        /// <summary>Non-running flare Y origins.</summary>
        public const int DefaultFlareY = 0x9bc15e;
        /// <summary>Running flare X origins.</summary>
        public const int RunningFlareX = 0x9bc19a;
        /// <summary>Running flare Y origins.</summary>
        public const int RunningFlareY = 0x9bc1ae;
        /// <summary>Main flare animation delays in bank $90.</summary>
        public const int MainFlareAnimationDelays = 0x90c487;
        /// <summary>Number of direction records represented by every firing table.</summary>
        public const int DirectionCount = 10;
    }

    /// <summary>Swing-frame, body-offset, rope-character, and fixed upload definitions.</summary>
    public static class Rendering
    {
        /// <summary>Animation frame selected by each high byte of swing angle.</summary>
        public const int SwingFrameByAngle = 0x9bc1c2;
        /// <summary>Bytes uploaded for the single Grapple point character.</summary>
        public const ushort PointTileByteCount = 0x20;
        /// <summary>Encoded VRAM destination for the Grapple point character.</summary>
        public const ushort PointTileVramDestination = 0x6200;
        /// <summary>$94:B036, DrawGrappleBeam: distance between consecutive rope objects in pixels.</summary>
        public const int SegmentSpacing = 8;
        /// <summary>$94:AFD9-AFE4: convert a signed 8.8 sample into an eight-pixel 16.16 displacement.</summary>
        public const int SegmentSampleToFixedPoint = SegmentSpacing * 256;
        /// <summary>$94:B048, DrawGrappleBeam: mask applied to the hardware length quotient.</summary>
        public const int SegmentCountMask = 15;
        /// <summary>$94:B051, DrawGrappleBeam: first animation slot, traversed downward.</summary>
        public const int FirstSegmentSlot = 15;
        /// <summary>$94:B016/B025, DrawGrappleBeam: center an eight-pixel character on its beam coordinate.</summary>
        public const int CharacterCenterOffset = 4;
        /// <summary>$94:B07A, DrawGrappleBeam: reject a segment if either coordinate has any high-byte bits.</summary>
        public const int SegmentOutsideViewportMask = 0xff00;
        /// <summary>$94:B18B-B197: each rope animation record lasts five visits to its slot.</summary>
        public const ushort SegmentAnimationDelay = 5;
        /// <summary>$94:B18B-B197: four successive rope animation records before the goto.</summary>
        public const int SegmentAnimationFrameCount = 4;
        /// <summary>$94:B18D: first rope character with palette five and priority three.</summary>
        public const ushort FirstSegmentAttributes = 0x3a21;
        /// <summary>$94:B13C/B17C, DrawGrappleBeamEnd: endpoint tile with palette five and priority three.</summary>
        public const ushort EndpointAttributes = 0x3a20;
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
    }
}
