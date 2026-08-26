using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// First breakpoint-friendly slice of normal gameplay Samus state and rendering.
/// </summary>
/// <remarks>
/// This class owns the state shared by the exact standing/right-running render, animation,
/// and grounded movement slices. It does not claim to port damage, equipment, most poses,
/// or the Landing Site cinematic actor. Keeping those boundaries blunt is preferable to
/// filling unported bank-$90 behavior with plausible-looking host code.
/// </remarks>
public sealed class SamusState
{
    private const int PoseDefinitions = 0x91b629;
    private const int AnimationDelayPointerTable = 0x91b010;
    private const int TopSpritemapBaseIndexTable = 0x929263;
    private const int BottomSpritemapBaseIndexTable = 0x92945d;
    private const int PowerSuitPalette = 0x9b9400;

    /// <summary>Pose $01 is “facing right - normal” in the cartridge table.</summary>
    public const byte FacingRightNormalPose = 0x01;

    /// <summary>Pose $02 is “facing left - normal” in the cartridge table.</summary>
    public const byte FacingLeftNormalPose = 0x02;

    /// <summary>Pose $09 is “moving right - not aiming” in the cartridge table.</summary>
    public const byte MovingRightNormalPose = 0x09;

    /// <summary>Pose $0A is “moving left - not aiming” in the cartridge table.</summary>
    public const byte MovingLeftNormalPose = 0x0a;

    /// <summary>Pose $25 turns a right-facing grounded Samus toward the left.</summary>
    public const byte TurningRightToLeftPose = 0x25;

    /// <summary>Pose $26 turns a left-facing grounded Samus toward the right.</summary>
    public const byte TurningLeftToRightPose = 0x26;

    /// <summary>Pose $19 is the ordinary right-facing spin jump.</summary>
    public const byte SpinJumpRightPose = 0x19;

    /// <summary>Pose $1A is the ordinary left-facing spin jump.</summary>
    public const byte SpinJumpLeftPose = 0x1a;

    /// <summary>Pose $1D is the stationary right-facing ordinary morph ball on the ground.</summary>
    public const byte MorphBallGroundRightPose = 0x1d;

    /// <summary>Pose $1E is the ordinary morph ball rolling to the right on the ground.</summary>
    public const byte MorphBallMovingRightPose = 0x1e;

    /// <summary>Pose $1F is the ordinary morph ball rolling to the left on the ground.</summary>
    public const byte MorphBallMovingLeftPose = 0x1f;

    /// <summary>Pose $31 is the right-facing ordinary morph ball in the air.</summary>
    public const byte MorphBallFallingRightPose = 0x31;

    /// <summary>Pose $32 is the left-facing ordinary morph ball in the air.</summary>
    public const byte MorphBallFallingLeftPose = 0x32;

    /// <summary>Pose $37 is the right-facing crouch-to-morph transition.</summary>
    public const byte MorphingTransitionRightPose = 0x37;

    /// <summary>Pose $38 is the left-facing crouch-to-morph transition.</summary>
    public const byte MorphingTransitionLeftPose = 0x38;

    /// <summary>Pose $3D is the right-facing morph-to-crouch transition.</summary>
    public const byte UnmorphingTransitionRightPose = 0x3d;

    /// <summary>Pose $3E is the left-facing morph-to-crouch transition.</summary>
    public const byte UnmorphingTransitionLeftPose = 0x3e;

    /// <summary>Pose $41 is the stationary left-facing ordinary morph ball on the ground.</summary>
    public const byte MorphBallGroundLeftPose = 0x41;

    /// <summary>Pose $79 is the stationary right-facing Spring Ball on the ground.</summary>
    public const byte SpringBallGroundRightPose = 0x79;

    /// <summary>Pose $7A is the stationary left-facing Spring Ball on the ground.</summary>
    public const byte SpringBallGroundLeftPose = 0x7a;

    /// <summary>Pose $7B is the Spring Ball moving right on the ground.</summary>
    public const byte SpringBallMovingRightPose = 0x7b;

    /// <summary>Pose $7C is the Spring Ball moving left on the ground.</summary>
    public const byte SpringBallMovingLeftPose = 0x7c;

    /// <summary>Pose $7D is the right-facing Spring Ball falling or bouncing.</summary>
    public const byte SpringBallFallingRightPose = 0x7d;

    /// <summary>Pose $7E is the left-facing Spring Ball falling or bouncing.</summary>
    public const byte SpringBallFallingLeftPose = 0x7e;

    /// <summary>Pose $7F is the right-facing Spring Ball in its powered jump.</summary>
    public const byte SpringBallJumpRightPose = 0x7f;

    /// <summary>Pose $80 is the left-facing Spring Ball in its powered jump.</summary>
    public const byte SpringBallJumpLeftPose = 0x80;

    /// <summary>Pose $29 is the unaimed right-facing falling pose.</summary>
    public const byte FallingRightPose = 0x29;

    /// <summary>Pose $2A is the unaimed left-facing falling pose.</summary>
    public const byte FallingLeftPose = 0x2a;

    /// <summary>Pose $03 is stationary, facing right, with the arm cannon aimed straight up.</summary>
    public const byte StandingAimUpRightPose = 0x03;

    /// <summary>Pose $04 is stationary, facing left, with the arm cannon aimed straight up.</summary>
    public const byte StandingAimUpLeftPose = 0x04;

    /// <summary>Pose $05 is stationary, facing right, and aiming diagonally up-right.</summary>
    public const byte StandingAimDiagonalUpRightPose = 0x05;

    /// <summary>Pose $06 is stationary, facing left, and aiming diagonally up-left.</summary>
    public const byte StandingAimDiagonalUpLeftPose = 0x06;

    /// <summary>Pose $07 is stationary, facing right, and aiming diagonally down-right.</summary>
    public const byte StandingAimDiagonalDownRightPose = 0x07;

    /// <summary>Pose $08 is stationary, facing left, and aiming diagonally down-left.</summary>
    public const byte StandingAimDiagonalDownLeftPose = 0x08;

    /// <summary>Pose $0D is the unused right-moving straight-up aim record.</summary>
    public const byte RunningAimUpRightPose = 0x0d;

    /// <summary>Pose $0E is the unused left-moving straight-up aim record.</summary>
    public const byte RunningAimUpLeftPose = 0x0e;

    /// <summary>Pose $0F moves right while aiming diagonally up-right.</summary>
    public const byte RunningAimDiagonalUpRightPose = 0x0f;

    /// <summary>Pose $10 moves left while aiming diagonally up-left.</summary>
    public const byte RunningAimDiagonalUpLeftPose = 0x10;

    /// <summary>Pose $11 moves right while aiming diagonally down-right.</summary>
    public const byte RunningAimDiagonalDownRightPose = 0x11;

    /// <summary>Pose $12 moves left while aiming diagonally down-left.</summary>
    public const byte RunningAimDiagonalDownLeftPose = 0x12;

    /// <summary>Pose $15 is a right-facing normal jump aimed straight up.</summary>
    public const byte NormalJumpAimUpRightPose = 0x15;

    /// <summary>Pose $16 is a left-facing normal jump aimed straight up.</summary>
    public const byte NormalJumpAimUpLeftPose = 0x16;

    /// <summary>Pose $17 is the compact right-facing normal jump aimed straight down.</summary>
    public const byte NormalJumpAimDownRightPose = 0x17;

    /// <summary>Pose $18 is the compact left-facing normal jump aimed straight down.</summary>
    public const byte NormalJumpAimDownLeftPose = 0x18;

    /// <summary>Pose $51 is a right-facing normal jump using the moving-forward art.</summary>
    public const byte NormalJumpForwardRightPose = 0x51;

    /// <summary>Pose $52 is a left-facing normal jump using the moving-forward art.</summary>
    public const byte NormalJumpForwardLeftPose = 0x52;

    /// <summary>Pose $55 is the right-facing jump transition aimed straight up.</summary>
    public const byte NormalJumpTransitionAimUpRightPose = 0x55;

    /// <summary>Pose $56 is the left-facing jump transition aimed straight up.</summary>
    public const byte NormalJumpTransitionAimUpLeftPose = 0x56;

    /// <summary>Pose $57 is the right-facing jump transition aimed diagonally up.</summary>
    public const byte NormalJumpTransitionAimDiagonalUpRightPose = 0x57;

    /// <summary>Pose $58 is the left-facing jump transition aimed diagonally up.</summary>
    public const byte NormalJumpTransitionAimDiagonalUpLeftPose = 0x58;

    /// <summary>Pose $59 is the right-facing jump transition aimed diagonally down.</summary>
    public const byte NormalJumpTransitionAimDiagonalDownRightPose = 0x59;

    /// <summary>Pose $5A is the left-facing jump transition aimed diagonally down.</summary>
    public const byte NormalJumpTransitionAimDiagonalDownLeftPose = 0x5a;

    /// <summary>Pose $69 is a right-facing normal jump aimed diagonally up.</summary>
    public const byte NormalJumpAimDiagonalUpRightPose = 0x69;

    /// <summary>Pose $6A is a left-facing normal jump aimed diagonally up.</summary>
    public const byte NormalJumpAimDiagonalUpLeftPose = 0x6a;

    /// <summary>Pose $6B is a right-facing normal jump aimed diagonally down.</summary>
    public const byte NormalJumpAimDiagonalDownRightPose = 0x6b;

    /// <summary>Pose $6C is a left-facing normal jump aimed diagonally down.</summary>
    public const byte NormalJumpAimDiagonalDownLeftPose = 0x6c;

    /// <summary>Pose $2B is right-facing falling aimed straight up.</summary>
    public const byte FallingAimUpRightPose = 0x2b;

    /// <summary>Pose $2C is left-facing falling aimed straight up.</summary>
    public const byte FallingAimUpLeftPose = 0x2c;

    /// <summary>Pose $2D is the compact right-facing fall aimed straight down.</summary>
    public const byte FallingAimDownRightPose = 0x2d;

    /// <summary>Pose $2E is the compact left-facing fall aimed straight down.</summary>
    public const byte FallingAimDownLeftPose = 0x2e;

    /// <summary>Pose $6D is right-facing falling aimed diagonally up.</summary>
    public const byte FallingAimDiagonalUpRightPose = 0x6d;

    /// <summary>Pose $6E is left-facing falling aimed diagonally up.</summary>
    public const byte FallingAimDiagonalUpLeftPose = 0x6e;

    /// <summary>Pose $6F is right-facing falling aimed diagonally down.</summary>
    public const byte FallingAimDiagonalDownRightPose = 0x6f;

    /// <summary>Pose $70 is left-facing falling aimed diagonally down.</summary>
    public const byte FallingAimDiagonalDownLeftPose = 0x70;

    /// <summary>Pose $71 is right-facing crouching aimed diagonally up.</summary>
    public const byte CrouchingAimDiagonalUpRightPose = 0x71;

    /// <summary>Pose $72 is left-facing crouching aimed diagonally up.</summary>
    public const byte CrouchingAimDiagonalUpLeftPose = 0x72;

    /// <summary>Pose $73 is right-facing crouching aimed diagonally down.</summary>
    public const byte CrouchingAimDiagonalDownRightPose = 0x73;

    /// <summary>Pose $74 is left-facing crouching aimed diagonally down.</summary>
    public const byte CrouchingAimDiagonalDownLeftPose = 0x74;

    /// <summary>Pose $85 is right-facing crouching aimed straight up.</summary>
    public const byte CrouchingAimUpRightPose = 0x85;

    /// <summary>Pose $86 is left-facing crouching aimed straight up.</summary>
    public const byte CrouchingAimUpLeftPose = 0x86;

    /// <summary>Pose $43 turns right-to-left while crouched without retaining aim.</summary>
    public const byte TurningRightToLeftCrouchingPose = 0x43;

    /// <summary>Pose $44 turns left-to-right while crouched without retaining aim.</summary>
    public const byte TurningLeftToRightCrouchingPose = 0x44;

    /// <summary>Pose $8B turns right-to-left on ground while preserving straight-up aim.</summary>
    public const byte TurningRightToLeftAimUpPose = 0x8b;

    /// <summary>Pose $8C turns left-to-right on ground while preserving straight-up aim.</summary>
    public const byte TurningLeftToRightAimUpPose = 0x8c;

    /// <summary>Pose $8D turns right-to-left on ground while preserving diagonal-down aim.</summary>
    public const byte TurningRightToLeftAimDiagonalDownPose = 0x8d;

    /// <summary>Pose $8E turns left-to-right on ground while preserving diagonal-down aim.</summary>
    public const byte TurningLeftToRightAimDiagonalDownPose = 0x8e;

    /// <summary>Pose $9C turns right-to-left on ground while preserving diagonal-up aim.</summary>
    public const byte TurningRightToLeftAimDiagonalUpPose = 0x9c;

    /// <summary>Pose $9D turns left-to-right on ground while preserving diagonal-up aim.</summary>
    public const byte TurningLeftToRightAimDiagonalUpPose = 0x9d;

    /// <summary>Pose $97 turns right-to-left while crouched and preserving straight-up aim.</summary>
    public const byte TurningRightToLeftCrouchingAimUpPose = 0x97;

    /// <summary>Pose $98 turns left-to-right while crouched and preserving straight-up aim.</summary>
    public const byte TurningLeftToRightCrouchingAimUpPose = 0x98;

    /// <summary>Pose $99 turns right-to-left while crouched and preserving diagonal-down aim.</summary>
    public const byte TurningRightToLeftCrouchingAimDiagonalDownPose = 0x99;

    /// <summary>Pose $9A turns left-to-right while crouched and preserving diagonal-down aim.</summary>
    public const byte TurningLeftToRightCrouchingAimDiagonalDownPose = 0x9a;

    /// <summary>Pose $A2 turns right-to-left while crouched and preserving diagonal-up aim.</summary>
    public const byte TurningRightToLeftCrouchingAimDiagonalUpPose = 0xa2;

    /// <summary>Pose $A3 turns left-to-right while crouched and preserving diagonal-up aim.</summary>
    public const byte TurningLeftToRightCrouchingAimDiagonalUpPose = 0xa3;

    /// <summary>Pose $E0 is a right-facing normal-jump landing aimed straight up.</summary>
    public const byte LandingAimUpRightPose = 0xe0;

    /// <summary>Pose $E1 is a left-facing normal-jump landing aimed straight up.</summary>
    public const byte LandingAimUpLeftPose = 0xe1;

    /// <summary>Pose $E2 is a right-facing normal-jump landing aimed diagonally up.</summary>
    public const byte LandingAimDiagonalUpRightPose = 0xe2;

    /// <summary>Pose $E3 is a left-facing normal-jump landing aimed diagonally up.</summary>
    public const byte LandingAimDiagonalUpLeftPose = 0xe3;

    /// <summary>Pose $E4 is a right-facing normal-jump landing aimed diagonally down.</summary>
    public const byte LandingAimDiagonalDownRightPose = 0xe4;

    /// <summary>Pose $E5 is a left-facing normal-jump landing aimed diagonally down.</summary>
    public const byte LandingAimDiagonalDownLeftPose = 0xe5;

    /// <summary>Pose $F1 transitions right-facing standing to crouching while aiming up.</summary>
    public const byte CrouchingTransitionAimUpRightPose = 0xf1;

    /// <summary>Pose $F2 transitions left-facing standing to crouching while aiming up.</summary>
    public const byte CrouchingTransitionAimUpLeftPose = 0xf2;

    /// <summary>Pose $F3 transitions right-facing standing to crouching while aiming diagonally up.</summary>
    public const byte CrouchingTransitionAimDiagonalUpRightPose = 0xf3;

    /// <summary>Pose $F4 transitions left-facing standing to crouching while aiming diagonally up.</summary>
    public const byte CrouchingTransitionAimDiagonalUpLeftPose = 0xf4;

    /// <summary>Pose $F5 transitions right-facing standing to crouching while aiming diagonally down.</summary>
    public const byte CrouchingTransitionAimDiagonalDownRightPose = 0xf5;

    /// <summary>Pose $F6 transitions left-facing standing to crouching while aiming diagonally down.</summary>
    public const byte CrouchingTransitionAimDiagonalDownLeftPose = 0xf6;

    /// <summary>Pose $F7 transitions right-facing crouching to standing while aiming up.</summary>
    public const byte StandingTransitionAimUpRightPose = 0xf7;

    /// <summary>Pose $F8 transitions left-facing crouching to standing while aiming up.</summary>
    public const byte StandingTransitionAimUpLeftPose = 0xf8;

    /// <summary>Pose $F9 transitions right-facing crouching to standing while aiming diagonally up.</summary>
    public const byte StandingTransitionAimDiagonalUpRightPose = 0xf9;

    /// <summary>Pose $FA transitions left-facing crouching to standing while aiming diagonally up.</summary>
    public const byte StandingTransitionAimDiagonalUpLeftPose = 0xfa;

    /// <summary>Pose $FB transitions right-facing crouching to standing while aiming diagonally down.</summary>
    public const byte StandingTransitionAimDiagonalDownRightPose = 0xfb;

    /// <summary>Pose $FC transitions left-facing crouching to standing while aiming diagonally down.</summary>
    public const byte StandingTransitionAimDiagonalDownLeftPose = 0xfc;

    /// <summary>Pose $27 is ordinary right-facing crouching.</summary>
    public const byte CrouchingRightPose = 0x27;

    /// <summary>Pose $28 is ordinary left-facing crouching.</summary>
    public const byte CrouchingLeftPose = 0x28;

    /// <summary>Pose $35 is the right-facing standing-to-crouch transition.</summary>
    public const byte CrouchingTransitionRightPose = 0x35;

    /// <summary>Pose $36 is the left-facing standing-to-crouch transition.</summary>
    public const byte CrouchingTransitionLeftPose = 0x36;

    /// <summary>Pose $3B is the right-facing crouch-to-standing transition.</summary>
    public const byte StandingTransitionRightPose = 0x3b;

    /// <summary>Pose $3C is the left-facing crouch-to-standing transition.</summary>
    public const byte StandingTransitionLeftPose = 0x3c;

    /// <summary>Pose $4B is the one-frame right neutral-jump transition.</summary>
    public const byte NeutralJumpTransitionRightPose = 0x4b;

    /// <summary>Pose $4C is the one-frame left neutral-jump transition.</summary>
    public const byte NeutralJumpTransitionLeftPose = 0x4c;

    /// <summary>Pose $4D is the ordinary right-facing neutral jump.</summary>
    public const byte NeutralJumpRightPose = 0x4d;

    /// <summary>Pose $4E is the ordinary left-facing neutral jump.</summary>
    public const byte NeutralJumpLeftPose = 0x4e;

    /// <summary>Pose $A4 lands facing right after a non-spinning jump or fall.</summary>
    public const byte NormalLandingRightPose = 0xa4;

    /// <summary>Pose $A5 lands facing left after a non-spinning jump or fall.</summary>
    public const byte NormalLandingLeftPose = 0xa5;

    /// <summary>Pose $A6 lands facing right after a spin or wall jump.</summary>
    public const byte SpinLandingRightPose = 0xa6;

    /// <summary>Pose $A7 lands facing left after a spin or wall jump.</summary>
    public const byte SpinLandingLeftPose = 0xa7;

    /// <summary>Current one-byte pose index, corresponding to WRAM <c>$0A1C</c>.</summary>
    public byte Pose { get; set; } = FacingRightNormalPose;

    /// <summary>Current animation-frame index, corresponding to WRAM <c>$0A96</c>.</summary>
    public ushort AnimationFrame { get; set; }

    /// <summary>
    /// Countdown at WRAM <c>$0A94</c>. Zero and negative-after-decrement both advance,
    /// matching the original routine's BEQ/BPL pair.
    /// </summary>
    public ushort AnimationFrameTimer { get; private set; }

    /// <summary>
    /// Delay added for speed/liquid physics at WRAM <c>$0A9A</c>. The translated standing
    /// dry-room slice keeps this equal to <see cref="XSpeedDivisor"/>.
    /// </summary>
    public ushort AnimationFrameBuffer { get; private set; }

    /// <summary>
    /// Exact bank-$90 fixed-point horizontal-speed registers used by ordinary grounded
    /// right-running movement and exposed independently for debugger inspection.
    /// </summary>
    public SamusHorizontalSpeedState HorizontalSpeed { get; } = new();

    /// <summary>
    /// Compatibility/debugger view of <c>samus_x_speed_divisor</c> at WRAM <c>$0A66</c>.
    /// The backing word belongs to <see cref="HorizontalSpeed"/>, just as the native word
    /// is both a movement-speed divisor and the no-FX animation delay buffer.
    /// </summary>
    public ushort XSpeedDivisor
    {
        get => HorizontalSpeed.SpeedDivisor;
        set => HorizontalSpeed.SpeedDivisor = value;
    }

    /// <summary>Current energy used by animation command $F6's low-health branch.</summary>
    public ushort Health { get; set; } = 99;

    /// <summary>
    /// Equipped-item bitfield corresponding to WRAM <c>$09A2</c>. Bit two ($0004) is Morph
    /// Ball, bit one ($0002) is Spring Ball, and bit twelve ($1000) is Bombs. Animation
    /// command $F9 and the Morph-Ball projectile handler read this word directly.
    /// </summary>
    public ushort EquippedItems { get; set; }

    /// <summary>
    /// Morph-ball bounce state at WRAM <c>$0B20</c>: zero is not bouncing, one is the first
    /// rebound, and two is the second rebound. Spring Ball also uses the high byte, but that
    /// separate movement family is intentionally not folded into the ordinary-ball state.
    /// </summary>
    public ushort MorphBallBounceState { get; set; }

    /// <summary>
    /// WRAM `$0A56`: low byte 1/2/3 is left/straight/right; bit `$0800` marks the bank-$91
    /// special prospective command as accepted and prevents the same explosion rearming it.
    /// </summary>
    public ushort BombJumpDirection { get; set; }

    /// <summary>True for `$90:E025`'s one-frame velocity-initialization handler.</summary>
    public bool BombJumpStarting { get; set; }

    /// <summary>True while `$90:E032` owns the rising bomb-jump arc.</summary>
    public bool BombJumpActive { get; set; }

    /// <summary>Bank-$91 address of the active pose's byte-oriented delay program.</summary>
    public int AnimationDelayListAddress { get; private set; }

    /// <summary>
    /// Most recent high-bit delay command, or null when the last advance selected a plain
    /// delay. This is a host-only debugger watch rather than original WRAM.
    /// </summary>
    public byte? LastAnimationDelayCommand { get; private set; }

    /// <summary>
    /// Operand published by animation command $F8 through the native “super-special
    /// prospective pose” seam. Null means the animation has not requested that transition.
    /// </summary>
    /// <remarks>
    /// This is host storage for WRAM $0A2A/$0A2C's command-three route. The grounded slice
    /// consumes it after animation, at the same pose-transition point used by the cartridge.
    /// Keeping it explicit makes the turn's final facing change visible to a debugger.
    /// </remarks>
    public byte? PendingTransitionalPose { get; private set; }

    /// <summary>Exact fixed-point position/radius words consumed by bank-$94 collision.</summary>
    public SamusKinematicsState Kinematics { get; } = new();

    /// <summary>Debugger/rendering view of the whole-pixel world X position.</summary>
    public ushort XPosition
    {
        get => Kinematics.XPosition;
        set => Kinematics.XPosition = value;
    }

    /// <summary>Debugger/rendering view of the whole-pixel world Y position.</summary>
    public ushort YPosition
    {
        get => Kinematics.YPosition;
        set => Kinematics.YPosition = value;
    }

    /// <summary>
    /// Ports <c>Samus_SetRadius</c> at <c>$90:EC22</c>: every pose is five pixels wide and
    /// reads its vertical radius from pose-definition byte six.
    /// </summary>
    public void RefreshCollisionRadii(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        int poseDefinition = AddWithinBank(PoseDefinitions, Pose * 8);
        Kinematics.XRadius = 5;
        Kinematics.YRadius = bus.ReadByte(AddWithinBank(poseDefinition, 6));
    }

    /// <summary>Reads pose-definition byte zero, the direction consumed by camera tracking.</summary>
    public byte ReadPoseXDirection(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, Pose * 8));
    }

    /// <summary>Reads pose-definition byte one, the movement-type dispatcher index.</summary>
    public byte ReadMovementType(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, Pose * 8 + 1));
    }

    /// <summary>
    /// Reads pose-definition byte two, the pose selected by <c>$91:82D9</c> when no
    /// controller bits are held and the transition table therefore is not consulted.
    /// </summary>
    public byte ReadNoInputFallbackPose(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, Pose * 8 + 2));
    }

    /// <summary>
    /// Reads pose-definition byte three, the ten-way arm-cannon direction consumed by
    /// the native normal-jump landing selector at <c>$91:E974</c>.
    /// </summary>
    public byte ReadShotDirection(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        return bus.ReadByte(AddWithinBank(PoseDefinitions, Pose * 8 + 3));
    }

    /// <summary>True for the four movement-type-zero, right-facing standing poses.</summary>
    public static bool IsRightFacingStandingPose(byte pose) => pose is
        FacingRightNormalPose or
        StandingAimUpRightPose or
        StandingAimDiagonalUpRightPose or
        StandingAimDiagonalDownRightPose;

    /// <summary>True for the four movement-type-zero, left-facing standing poses.</summary>
    public static bool IsLeftFacingStandingPose(byte pose) => pose is
        FacingLeftNormalPose or
        StandingAimUpLeftPose or
        StandingAimDiagonalUpLeftPose or
        StandingAimDiagonalDownLeftPose;

    /// <summary>True for the four movement-type-one, right-moving pose-table entries.</summary>
    public static bool IsRightFacingRunningPose(byte pose) => pose is
        MovingRightNormalPose or
        RunningAimUpRightPose or
        RunningAimDiagonalUpRightPose or
        RunningAimDiagonalDownRightPose;

    /// <summary>True for the four movement-type-one, left-moving pose-table entries.</summary>
    public static bool IsLeftFacingRunningPose(byte pose) => pose is
        MovingLeftNormalPose or
        RunningAimUpLeftPose or
        RunningAimDiagonalUpLeftPose or
        RunningAimDiagonalDownLeftPose;

    /// <summary>True when a supported standing, running, crouching, or landing pose carries aim metadata.</summary>
    public static bool IsGroundedAimPose(byte pose) => pose is
        StandingAimUpRightPose or StandingAimUpLeftPose or
        StandingAimDiagonalUpRightPose or StandingAimDiagonalUpLeftPose or
        StandingAimDiagonalDownRightPose or StandingAimDiagonalDownLeftPose or
        RunningAimUpRightPose or RunningAimUpLeftPose or
        RunningAimDiagonalUpRightPose or RunningAimDiagonalUpLeftPose or
        RunningAimDiagonalDownRightPose or RunningAimDiagonalDownLeftPose or
        CrouchingAimUpRightPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalUpLeftPose or
        CrouchingAimDiagonalDownRightPose or CrouchingAimDiagonalDownLeftPose or
        LandingAimUpRightPose or LandingAimUpLeftPose or
        LandingAimDiagonalUpRightPose or LandingAimDiagonalUpLeftPose or
        LandingAimDiagonalDownRightPose or LandingAimDiagonalDownLeftPose;

    /// <summary>True for right-facing aimed normal-jump landing poses `$E0/$E2/$E4`.</summary>
    public static bool IsRightFacingAimedLandingPose(byte pose) => pose is
        LandingAimUpRightPose or LandingAimDiagonalUpRightPose or
        LandingAimDiagonalDownRightPose;

    /// <summary>True for left-facing aimed normal-jump landing poses `$E1/$E3/$E5`.</summary>
    public static bool IsLeftFacingAimedLandingPose(byte pose) => pose is
        LandingAimUpLeftPose or LandingAimDiagonalUpLeftPose or
        LandingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for the complete right-facing movement-type-five crouch family. All four
    /// records use radius 16 and the shared transition table at <c>$91:A66C</c>.
    /// </summary>
    public static bool IsRightFacingCrouchingPose(byte pose) => pose is
        CrouchingRightPose or CrouchingAimUpRightPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalDownRightPose;

    /// <summary>
    /// True for the complete left-facing movement-type-five crouch family. All four
    /// records use radius 16 and the mirrored table at <c>$91:A6BC</c>.
    /// </summary>
    public static bool IsLeftFacingCrouchingPose(byte pose) => pose is
        CrouchingLeftPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpLeftPose or CrouchingAimDiagonalDownLeftPose;

    /// <summary>True for the six crouching poses carrying a real shot direction.</summary>
    public static bool IsAimedCrouchingPose(byte pose) => pose is
        CrouchingAimUpRightPose or CrouchingAimUpLeftPose or
        CrouchingAimDiagonalUpRightPose or CrouchingAimDiagonalUpLeftPose or
        CrouchingAimDiagonalDownRightPose or CrouchingAimDiagonalDownLeftPose;

    /// <summary>
    /// True for admitted grounded `$0E/$17` poses that began facing right and turn left.
    /// Their definition direction is already four (the destination facing), so callers
    /// must use this semantic grouping when preserving old rightward momentum.
    /// </summary>
    public static bool IsRightToLeftGroundTurnPose(byte pose) => pose is
        TurningRightToLeftPose or TurningRightToLeftAimUpPose or
        TurningRightToLeftAimDiagonalUpPose or TurningRightToLeftAimDiagonalDownPose or
        TurningRightToLeftCrouchingPose or TurningRightToLeftCrouchingAimUpPose or
        TurningRightToLeftCrouchingAimDiagonalUpPose or
        TurningRightToLeftCrouchingAimDiagonalDownPose;

    /// <summary>True for admitted grounded `$0E/$17` poses that began facing left and turn right.</summary>
    public static bool IsLeftToRightGroundTurnPose(byte pose) => pose is
        TurningLeftToRightPose or TurningLeftToRightAimUpPose or
        TurningLeftToRightAimDiagonalUpPose or TurningLeftToRightAimDiagonalDownPose or
        TurningLeftToRightCrouchingPose or TurningLeftToRightCrouchingAimUpPose or
        TurningLeftToRightCrouchingAimDiagonalUpPose or
        TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>
    /// True for the six aimed crouched-turn records dispatched through movement type `$17`.
    /// `$43/$44` are intentionally absent because the retail pose definitions assign those
    /// otherwise similar-looking unaimed crouch turns to grounded movement type `$0E`.
    /// </summary>
    public static bool IsAimedCrouchingTurnPose(byte pose) => pose is
        TurningRightToLeftCrouchingAimUpPose or TurningLeftToRightCrouchingAimUpPose or
        TurningRightToLeftCrouchingAimDiagonalUpPose or TurningLeftToRightCrouchingAimDiagonalUpPose or
        TurningRightToLeftCrouchingAimDiagonalDownPose or TurningLeftToRightCrouchingAimDiagonalDownPose;

    /// <summary>
    /// True only for the admitted movement-type-$0F crouch/stand animation records.
    /// Morph/unmorph records share that dispatcher but are intentionally not hidden here.
    /// </summary>
    public static bool IsCrouchStandTransitionPose(byte pose) => pose is
        CrouchingTransitionRightPose or CrouchingTransitionLeftPose or
        StandingTransitionRightPose or StandingTransitionLeftPose or
        CrouchingTransitionAimUpRightPose or CrouchingTransitionAimUpLeftPose or
        CrouchingTransitionAimDiagonalUpRightPose or CrouchingTransitionAimDiagonalUpLeftPose or
        CrouchingTransitionAimDiagonalDownRightPose or CrouchingTransitionAimDiagonalDownLeftPose or
        StandingTransitionAimUpRightPose or StandingTransitionAimUpLeftPose or
        StandingTransitionAimDiagonalUpRightPose or StandingTransitionAimDiagonalUpLeftPose or
        StandingTransitionAimDiagonalDownRightPose or StandingTransitionAimDiagonalDownLeftPose;

    /// <summary>True for the four ordinary, non-Spring-Ball grounded morph poses.</summary>
    public static bool IsGroundedMorphBallPose(byte pose) => pose is
        MorphBallGroundRightPose or MorphBallGroundLeftPose or
        MorphBallMovingRightPose or MorphBallMovingLeftPose;

    /// <summary>True for the two ordinary, non-Spring-Ball airborne morph poses.</summary>
    public static bool IsAirborneMorphBallPose(byte pose) => pose is
        MorphBallFallingRightPose or MorphBallFallingLeftPose;

    /// <summary>True for movement type $11's four grounded Spring Ball poses.</summary>
    public static bool IsGroundedSpringBallPose(byte pose) => pose is
        SpringBallGroundRightPose or SpringBallGroundLeftPose or
        SpringBallMovingRightPose or SpringBallMovingLeftPose;

    /// <summary>True for Spring Ball jump/fall poses across movement types $12/$13.</summary>
    public static bool IsAirborneSpringBallPose(byte pose) => pose is
        SpringBallFallingRightPose or SpringBallFallingLeftPose or
        SpringBallJumpRightPose or SpringBallJumpLeftPose;

    /// <summary>True for every stable ordinary or Spring Ball pose using radius seven.</summary>
    public static bool IsStableBallPose(byte pose) =>
        IsGroundedMorphBallPose(pose) || IsAirborneMorphBallPose(pose) ||
        IsGroundedSpringBallPose(pose) || IsAirborneSpringBallPose(pose);

    /// <summary>True for the four entry/exit poses handled by movement type $0F.</summary>
    public static bool IsMorphTransitionPose(byte pose) => pose is
        MorphingTransitionRightPose or MorphingTransitionLeftPose or
        UnmorphingTransitionRightPose or UnmorphingTransitionLeftPose;

    /// <summary>
    /// Convenience debugger/test entry that performs both native phases: publishing the
    /// bank-$A0 timer-eight overlap direction, then consuming it through $90:DF99 and
    /// special command three $91:EE80. Live runtime code calls those phases on separate
    /// frames through <see cref="PublishBombJumpDirection"/> and
    /// <see cref="TrySetupPublishedMorphedBombJump"/>.
    /// </summary>
    public void RequestMorphedBombJump(byte direction)
    {
        PublishBombJumpDirection(direction);
        TrySetupPublishedMorphedBombJump();
    }

    /// <summary>
    /// Stores only bank-$A0's low-byte bomb direction. The gameplay loop does this after
    /// frame-handler alpha; setup consequently cannot consume it until the next frame.
    /// </summary>
    public void PublishBombJumpDirection(byte direction)
    {
        if (direction is < 1 or > 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(direction),
                direction,
                "Bomb-jump direction must be left, straight, or right.");
        }

        // $A0:984B writes the complete word, not just its low byte. A timer-eight overlap
        // therefore publishes exactly $0001/$0002/$0003.
        BombJumpDirection = direction;
    }

    /// <summary>
    /// Consumes a direction published by the previous frame's projectile collision using
    /// the morphed branch at $90:E010 and command-three branch at $91:EE80.
    /// </summary>
    /// <returns>True when a pending low-byte direction installed the start handler.</returns>
    public bool TrySetupPublishedMorphedBombJump()
    {
        if (BombJumpDirection == 0 || (BombJumpDirection & 0xff00) != 0)
            return false;

        if (!IsStableBallPose(Pose))
        {
            // Standing/running/falling bomb jumps deliberately select different poses at
            // $90:DFAD-$90:E00D. Silently preserving a non-ball pose would invent behavior.
            throw new NotSupportedException(
                $"Bomb-jump setup for non-ball pose ${Pose:X2} is not translated yet.");
        }

        // Morphed movement types `$04/$08/$11/$12/$13` preserve the current pose in
        // SpecialProspectivePose. Command three ORs `$0800` and installs start handler.
        BombJumpDirection |= 0x0800;
        BombJumpStarting = true;
        BombJumpActive = false;
        return true;
    }

    /// <summary>True for the admitted right-facing movement-type-two normal-jump poses.</summary>
    public static bool IsRightFacingNormalJumpPose(byte pose) => pose is
        NeutralJumpTransitionRightPose or NeutralJumpRightPose or
        NormalJumpForwardRightPose or NormalJumpAimUpRightPose or
        NormalJumpTransitionAimUpRightPose or
        NormalJumpTransitionAimDiagonalUpRightPose or
        NormalJumpTransitionAimDiagonalDownRightPose or
        NormalJumpAimDiagonalUpRightPose or NormalJumpAimDiagonalDownRightPose or
        NormalJumpAimDownRightPose;

    /// <summary>True for the admitted left-facing movement-type-two normal-jump poses.</summary>
    public static bool IsLeftFacingNormalJumpPose(byte pose) => pose is
        NeutralJumpTransitionLeftPose or NeutralJumpLeftPose or
        NormalJumpForwardLeftPose or NormalJumpAimUpLeftPose or
        NormalJumpTransitionAimUpLeftPose or
        NormalJumpTransitionAimDiagonalUpLeftPose or
        NormalJumpTransitionAimDiagonalDownLeftPose or
        NormalJumpAimDiagonalUpLeftPose or NormalJumpAimDiagonalDownLeftPose or
        NormalJumpAimDownLeftPose;

    /// <summary>True for admitted right-facing movement-type-six falling poses.</summary>
    public static bool IsRightFacingFallingPose(byte pose) => pose is
        FallingRightPose or FallingAimUpRightPose or
        FallingAimDiagonalUpRightPose or FallingAimDiagonalDownRightPose or
        FallingAimDownRightPose;

    /// <summary>True for admitted left-facing movement-type-six falling poses.</summary>
    public static bool IsLeftFacingFallingPose(byte pose) => pose is
        FallingLeftPose or FallingAimUpLeftPose or
        FallingAimDiagonalUpLeftPose or FallingAimDiagonalDownLeftPose or
        FallingAimDownLeftPose;

    /// <summary>True for the aimed normal-jump/falling poses, including compact Down aim.</summary>
    public static bool IsAimedAerialPose(byte pose) => pose is
        NormalJumpAimUpRightPose or NormalJumpAimUpLeftPose or
        NormalJumpTransitionAimUpRightPose or NormalJumpTransitionAimUpLeftPose or
        NormalJumpTransitionAimDiagonalUpRightPose or NormalJumpTransitionAimDiagonalUpLeftPose or
        NormalJumpTransitionAimDiagonalDownRightPose or NormalJumpTransitionAimDiagonalDownLeftPose or
        NormalJumpAimDiagonalUpRightPose or NormalJumpAimDiagonalUpLeftPose or
        NormalJumpAimDiagonalDownRightPose or NormalJumpAimDiagonalDownLeftPose or
        FallingAimUpRightPose or FallingAimUpLeftPose or
        FallingAimDiagonalUpRightPose or FallingAimDiagonalUpLeftPose or
        FallingAimDiagonalDownRightPose or FallingAimDiagonalDownLeftPose or
        NormalJumpAimDownRightPose or NormalJumpAimDownLeftPose or
        FallingAimDownRightPose or FallingAimDownLeftPose;

    /// <summary>True only for the four radius-ten straight-down aerial bodies.</summary>
    public static bool IsCompactAerialPose(byte pose) => pose is
        NormalJumpAimDownRightPose or NormalJumpAimDownLeftPose or
        FallingAimDownRightPose or FallingAimDownLeftPose;

    /// <summary>
    /// Applies a same-facing input/fallback transition within normal-jump type two or
    /// falling type six without replacing the live 16.16 velocity words.
    /// </summary>
    public void ApplyAerialAimTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (IsCompactAerialPose(Pose) || IsCompactAerialPose(targetPose))
        {
            throw new NotSupportedException(
                "Radius-ten aerial transitions require TryApplyCompactAerialTransition and active room collision data.");
        }
        bool rightJump = IsRightFacingNormalJumpPose(Pose) &&
            IsRightFacingNormalJumpPose(targetPose);
        bool leftJump = IsLeftFacingNormalJumpPose(Pose) &&
            IsLeftFacingNormalJumpPose(targetPose);
        bool rightFall = IsRightFacingFallingPose(Pose) &&
            IsRightFacingFallingPose(targetPose);
        bool leftFall = IsLeftFacingFallingPose(Pose) &&
            IsLeftFacingFallingPose(targetPose);
        if (!rightJump && !leftJump && !rightFall && !leftFall)
        {
            throw new NotSupportedException(
                $"Aerial aim transition ${Pose:X2} -> ${targetPose:X2} crosses an untranslated family.");
        }
        if (!IsAimedAerialPose(Pose) && !IsAimedAerialPose(targetPose) &&
            targetPose is not (NormalJumpForwardRightPose or NormalJumpForwardLeftPose))
        {
            throw new InvalidOperationException("Aerial aim transition requires aimed or forward-jump metadata.");
        }

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius != oldRadius)
        {
            // The admitted family deliberately excludes compact straight-down `$17/$18`
            // and `$2D/$2E`; reaching a different radius proves a caller crossed that seam.
            throw new NotSupportedException(
                $"Aerial pose ${targetPose:X2} changes radius {oldRadius} -> {Kinematics.YRadius}; pose-change collision is not translated for it.");
        }
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies same-facing transitions where either endpoint is compact straight-down
    /// `$17/$18/$2D/$2E`, preserving live velocity while reproducing pose-change collision.
    /// </summary>
    /// <remarks>
    /// Entering radius ten from radius nineteen requires no collision work at `$91:FDAE`.
    /// Leaving it probes all nine newly occupied pixels above and below. If both initial
    /// directions are blocked, `$91:FFA7` selects ordinary crouch; if a compensating shift's
    /// second probe is blocked, `$91:FE82` retains the source. Neither case installs the target.
    /// </remarks>
    public bool TryApplyCompactAerialTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightJump = IsRightFacingNormalJumpPose(Pose) &&
            IsRightFacingNormalJumpPose(targetPose);
        bool leftJump = IsLeftFacingNormalJumpPose(Pose) &&
            IsLeftFacingNormalJumpPose(targetPose);
        bool rightFall = IsRightFacingFallingPose(Pose) &&
            IsRightFacingFallingPose(targetPose);
        bool leftFall = IsLeftFacingFallingPose(Pose) &&
            IsLeftFacingFallingPose(targetPose);
        if ((!rightJump && !leftJump && !rightFall && !leftFall) ||
            (!IsCompactAerialPose(Pose) && !IsCompactAerialPose(targetPose)))
        {
            throw new NotSupportedException(
                $"Compact aerial transition ${Pose:X2} -> ${targetPose:X2} is not a same-family ROM route.");
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies a same-facing transition within the movement-type-zero standing family and
    /// movement-type-one running family when either endpoint is an aimed pose.
    /// </summary>
    /// <remarks>
    /// All admitted records have radius 21 and ordinary animation-list initialization.
    /// Movement type changes take effect on the following frame, exactly because pose
    /// transitions occur after movement; jump, turn, crouch, and firing targets still go
    /// through their separate side-effect paths.
    /// </remarks>
    public void ApplyGroundedAimTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool sourceRight = IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
            IsRightFacingAimedLandingPose(Pose);
        bool targetRight = IsRightFacingStandingPose(targetPose) || IsRightFacingRunningPose(targetPose);
        bool sourceLeft = IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsLeftFacingAimedLandingPose(Pose);
        bool targetLeft = IsLeftFacingStandingPose(targetPose) || IsLeftFacingRunningPose(targetPose);
        bool sameRightFamily = sourceRight && targetRight;
        bool sameLeftFamily = sourceLeft && targetLeft;
        bool sameRightCrouch = IsRightFacingCrouchingPose(Pose) &&
            IsRightFacingCrouchingPose(targetPose);
        bool sameLeftCrouch = IsLeftFacingCrouchingPose(Pose) &&
            IsLeftFacingCrouchingPose(targetPose);
        if (!sameRightFamily && !sameLeftFamily && !sameRightCrouch && !sameLeftCrouch)
        {
            throw new NotSupportedException(
                $"Grounded aim transition ${Pose:X2} -> ${targetPose:X2} crosses an untranslated pose family.");
        }
        if (!IsGroundedAimPose(Pose) && !IsGroundedAimPose(targetPose))
            throw new InvalidOperationException("Grounded aim transition requires an aimed source or target pose.");

        // This is $91:F404/$91:FB08's ordinary target installation. Preserve it as an
        // explicit method: replacing Pose directly would omit radius refresh, animation
        // frame zero, delay-list selection, and pending-command cleanup.
        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Grounded aim");
    }

    /// <summary>
    /// Applies the verified ordinary transition from standing-right pose $01 to running-
    /// right pose $09 at the end-of-frame bank-$91 transition seam.
    /// </summary>
    /// <remarks>
    /// This is intentionally not a general pose setter. For this transition, momentum
    /// routine index zero is a no-op, both poses have radius 21, and pose $09 uses the
    /// already-supported default render/tile paths. Other transitions can change momentum,
    /// align radii, invoke collision, or run specialized handlers and remain rejected.
    /// </remarks>
    public void ApplyStandingRightToRunningRight(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != FacingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Standing-right to running-right transition requires pose $01, not ${Pose:X2}.");
        }

        Pose = MovingRightNormalPose;
        RefreshCollisionRadii(bus);

        // $91:F404 -> $91:FB08 resets animation frame zero and loads pose $09's first
        // delay byte after SamusFunc_F433 refreshes pose direction/movement metadata.
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies the verified no-button fallback from running-right pose $09 to standing-right
    /// pose $01 after ordinary running momentum has decelerated to zero.
    /// </summary>
    public void ApplyRunningRightToStandingRight(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != MovingRightNormalPose)
        {
            throw new InvalidOperationException(
                $"Running-right to standing-right transition requires pose $09, not ${Pose:X2}.");
        }

        Pose = FacingRightNormalPose;
        RefreshCollisionRadii(bus);

        // Pose $09's new-pose-unless-buttons byte is $01. Once Samus_Pose_Func2 selects
        // momentum index two, $91:F404 installs that pose and $91:FB08 starts its frame-zero
        // delay stream. The following standing movement pass clears any base-speed residue.
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>Applies the ordinary grounded $02 -> $0A start-moving-left transition.</summary>
    public void ApplyStandingLeftToRunningLeft(ISnesAddressSpace bus) =>
        ApplySimpleGroundedPoseChange(
            bus,
            FacingLeftNormalPose,
            MovingLeftNormalPose,
            "Standing-left to running-left");

    /// <summary>Applies the no-button grounded $0A -> $02 fallback.</summary>
    public void ApplyRunningLeftToStandingLeft(ISnesAddressSpace bus) =>
        ApplySimpleGroundedPoseChange(
            bus,
            MovingLeftNormalPose,
            FacingLeftNormalPose,
            "Running-left to standing-left");

    /// <summary>
    /// Applies the shared standing/landing transition-table route from $A4/$A6 to running
    /// right $09, or from $A5/$A7 to running left $0A. Landing movement has already cleared
    /// momentum in this frame; the new running pose begins accelerating on the next frame.
    /// </summary>
    public void ApplyLandingToRunning(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool right = (Pose is NormalLandingRightPose or SpinLandingRightPose) &&
            targetPose == MovingRightNormalPose;
        bool left = (Pose is NormalLandingLeftPose or SpinLandingLeftPose) &&
            targetPose == MovingLeftNormalPose;
        if (!right && !left)
        {
            throw new InvalidOperationException(
                $"Landing-to-run transition ${Pose:X2} -> ${targetPose:X2} is not verified.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Landing-to-run");
    }

    /// <summary>
    /// Applies bank-$91's grounded turn initialization at <c>$91:F8D3</c>. Input tables
    /// first publish generic `$25/$26`; the initializer indexes the previous pose's shot
    /// direction through `$91:F9C2` and may replace it with `$8B-$8E/$9C/$9D`.
    /// </summary>
    public void ApplyGroundedTurn(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);

        // Crouching transition tables publish `$43/$44` directly, whereas every admitted
        // standing/running/landing table publishes `$25/$26`. The initializer still uses
        // previous movement type five to choose the full crouched aim-preserving table.
        bool wasCrouching = ReadMovementType(bus) == 5;

        bool rightSource = IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
            IsRightFacingCrouchingPose(Pose) ||
            IsRightFacingAimedLandingPose(Pose) ||
            Pose is NormalLandingRightPose or SpinLandingRightPose;
        bool leftSource = IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsLeftFacingCrouchingPose(Pose) ||
            IsLeftFacingAimedLandingPose(Pose) ||
            Pose is NormalLandingLeftPose or SpinLandingLeftPose;
        bool turnsLeft = targetPose == (wasCrouching
            ? TurningRightToLeftCrouchingPose
            : TurningRightToLeftPose) && rightSource;
        bool turnsRight = targetPose == (wasCrouching
            ? TurningLeftToRightCrouchingPose
            : TurningLeftToRightPose) && leftSource;
        if (!turnsLeft && !turnsRight)
        {
            throw new InvalidOperationException(
                $"Grounded turn ${Pose:X2} -> ${targetPose:X2} is not a verified transition.");
        }

        // `$91:F8D3` reads the PREVIOUS pose record before it installs the final turn art.
        // Previous movement type five selects `$91:F9CC`; every other admitted source uses
        // `$91:F9C2`. Both tables have ten entries, but shot directions four/five belong to
        // the compact straight-down family that remains untranslated.
        byte shotDirection = ReadShotDirection(bus);
        byte selectedTurnPose = wasCrouching
            ? shotDirection switch
            {
                0 => TurningRightToLeftCrouchingAimUpPose,
                1 => TurningRightToLeftCrouchingAimDiagonalUpPose,
                2 => TurningRightToLeftCrouchingPose,
                3 => TurningRightToLeftCrouchingAimDiagonalDownPose,
                6 => TurningLeftToRightCrouchingAimDiagonalDownPose,
                7 => TurningLeftToRightCrouchingPose,
                8 => TurningLeftToRightCrouchingAimDiagonalUpPose,
                9 => TurningLeftToRightCrouchingAimUpPose,
                _ => throw new NotSupportedException(
                    $"Crouched turn shot direction ${shotDirection:X2} is not translated."),
            }
            : shotDirection switch
            {
                0 => TurningRightToLeftAimUpPose,
                1 => TurningRightToLeftAimDiagonalUpPose,
                2 => TurningRightToLeftPose,
                3 => TurningRightToLeftAimDiagonalDownPose,
                6 => TurningLeftToRightAimDiagonalDownPose,
                7 => TurningLeftToRightPose,
                8 => TurningLeftToRightAimDiagonalUpPose,
                9 => TurningLeftToRightAimUpPose,
                _ => throw new NotSupportedException(
                    $"Grounded turn shot direction ${shotDirection:X2} is not translated."),
            };
        if ((turnsLeft && !IsRightToLeftGroundTurnPose(selectedTurnPose)) ||
            (turnsRight && !IsLeftToRightGroundTurnPose(selectedTurnPose)))
        {
            throw new InvalidOperationException(
                $"Grounded turn source ${Pose:X2} has direction metadata inconsistent with target ${targetPose:X2}.");
        }

        SamusHorizontalSpeedState speed = HorizontalSpeed;

        // $91:F931-$91:F941 folds the run-button/speed-booster component into base speed
        // with two independent 16-bit ADCs and their carry. Use one wrapping 16.16 add to
        // preserve exactly that pair of operations, including overflow at the high word.
        uint combinedSpeed = unchecked(speed.BaseFixed +
            ((uint)speed.ExtraRunSpeed << 16) + speed.ExtraRunSubspeed);
        speed.BaseSpeed = unchecked((ushort)(combinedSpeed >> 16));
        speed.BaseSubspeed = unchecked((ushort)combinedSpeed);

        // The native handler consumes the extra component and selects acceleration mode
        // one. $90:8EA9 interprets that mode as “move opposite the NEW facing direction”
        // while $90:9A7E decelerates, which is how reversal preserves old momentum.
        speed.ExtraRunSpeed = 0;
        speed.ExtraRunSubspeed = 0;
        speed.AccelerationMode = 1;

        ApplySimpleGroundedPoseChange(bus, Pose, selectedTurnPose, "Grounded turn");
    }

    /// <summary>
    /// Applies the verified ordinary-input jump transitions selected from the cartridge's
    /// bank-$91 table, including <c>HandleJumpTransition</c>'s call to
    /// <c>Make_Samus_Jump</c>. Only the four no-equipment/no-aim routes admitted by the
    /// current runtime are accepted.
    /// </summary>
    public void ApplyOrdinaryJumpTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool verified = (Pose, targetPose) is
            (FacingRightNormalPose or StandingAimUpRightPose or
                StandingAimDiagonalUpRightPose or StandingAimDiagonalDownRightPose,
             NeutralJumpTransitionRightPose) or
            (FacingLeftNormalPose or StandingAimUpLeftPose or
                StandingAimDiagonalUpLeftPose or StandingAimDiagonalDownLeftPose,
             NeutralJumpTransitionLeftPose) or
            (FacingRightNormalPose or StandingAimUpRightPose or
                StandingAimDiagonalUpRightPose or StandingAimDiagonalDownRightPose,
             NormalJumpTransitionAimUpRightPose or
                NormalJumpTransitionAimDiagonalUpRightPose or
                NormalJumpTransitionAimDiagonalDownRightPose) or
            (FacingLeftNormalPose or StandingAimUpLeftPose or
                StandingAimDiagonalUpLeftPose or StandingAimDiagonalDownLeftPose,
             NormalJumpTransitionAimUpLeftPose or
                NormalJumpTransitionAimDiagonalUpLeftPose or
                NormalJumpTransitionAimDiagonalDownLeftPose) or
            (MovingRightNormalPose or RunningAimUpRightPose or
                RunningAimDiagonalUpRightPose or RunningAimDiagonalDownRightPose,
             SpinJumpRightPose) or
            (MovingLeftNormalPose or RunningAimUpLeftPose or
                RunningAimDiagonalUpLeftPose or RunningAimDiagonalDownLeftPose,
             SpinJumpLeftPose);
        if (!verified)
        {
            throw new NotSupportedException(
                $"Ordinary jump transition ${Pose:X2} -> ${targetPose:X2} is not translated.");
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);
        SamusAerialMovement.InitializeDryAirJump(bus, this);
    }

    /// <summary>
    /// Applies the crouching table's direct `$27/$71/$73/$85 -> $01` and mirrored
    /// `$28/$72/$74/$86 -> $02` exits. These are real six-byte transition-table records;
    /// unlike the animated `$F7-$FC` stand-up family, they install the final standing pose
    /// immediately after pose-change collision has made room for its larger radius.
    /// </summary>
    /// <returns>
    /// False when the two-sided pose-change collision resolver falls back to stable
    /// crouch because both the floor and ceiling constrain the larger body.
    /// </returns>
    public bool TryApplyDirectCrouchToStandingTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == FacingRightNormalPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == FacingLeftNormalPose;
        if (!rightRoute && !leftRoute)
        {
            throw new NotSupportedException(
                $"Direct crouch exit ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // $91:FFA7 does not restore an aimed crouch when the attempted standing body
            // is boxed in. It selects the ordinary stable crouch from facing metadata.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies the crouching table's `$4B/$4C` jump entry through the native ordering:
    /// enlarge radius 16 -> 19 with `$91:FDAE`, initialise movement type two, run the
    /// `$91:FC66` crouch-only Y adjustment, then call `Make_Samus_Jump`.
    /// </summary>
    /// <remarks>
    /// `$91:FC7D` tests the literal previous pose, not merely movement type five. Therefore
    /// only ordinary `$27/$28` subtract ten extra pixels; aimed `$71-$74/$85/$86` still
    /// jump, but retain only the collision resolver's bottom-alignment adjustment.
    /// </remarks>
    /// <returns>
    /// False when expansion is impossible. Simultaneous initial hits select ordinary stable
    /// crouch; rejection by a compensating opposite-side probe retains the source pose.
    /// </returns>
    public bool TryApplyCrouchJumpTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool rightRoute = IsRightFacingCrouchingPose(Pose) &&
            targetPose == NeutralJumpTransitionRightPose;
        bool leftRoute = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == NeutralJumpTransitionLeftPose;
        if (!rightRoute && !leftRoute)
        {
            throw new NotSupportedException(
                $"Crouch jump ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));

        // HandleJumpTransition_NormalJumping at $91:FC7D performs this after pose
        // initialization/collision but before Make_Samus_Jump. It writes only current Y;
        // the desktop state has no separately exposed PreviousYPosition word to mirror.
        if (sourcePose is CrouchingRightPose or CrouchingLeftPose)
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - 10));

        InitializeAnimation(bus, initialFrame: 0);
        SamusAerialMovement.InitializeDryAirJump(bus, this);
        return true;
    }

    /// <summary>
    /// Applies the ordinary or aimed crouch-start/stand-start transition records,
    /// including command seven's bottom alignment and the larger-radius collision branch
    /// from <c>HandlePoseChangeCollision</c>. The admitted target set is exactly
    /// `$35/$36/$3B/$3C/$F1-$FC`.
    /// </summary>
    /// <returns>
    /// False only when simultaneous floor and ceiling collision leave no verified room to
    /// expand from radius 16 to radius 21; native behavior then retains the crouching pose.
    /// </returns>
    public bool TryApplyPostureTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool startsCrouchingRight =
            (IsRightFacingStandingPose(Pose) || IsRightFacingRunningPose(Pose) ||
             (Pose is NormalLandingRightPose or SpinLandingRightPose) ||
             IsRightFacingAimedLandingPose(Pose)) &&
            targetPose is
                CrouchingTransitionRightPose or CrouchingTransitionAimUpRightPose or
                CrouchingTransitionAimDiagonalUpRightPose or
                CrouchingTransitionAimDiagonalDownRightPose;
        bool startsCrouchingLeft =
            (IsLeftFacingStandingPose(Pose) || IsLeftFacingRunningPose(Pose) ||
             (Pose is NormalLandingLeftPose or SpinLandingLeftPose) ||
             IsLeftFacingAimedLandingPose(Pose)) &&
            targetPose is
                CrouchingTransitionLeftPose or CrouchingTransitionAimUpLeftPose or
                CrouchingTransitionAimDiagonalUpLeftPose or
                CrouchingTransitionAimDiagonalDownLeftPose;
        bool startsCrouching = startsCrouchingRight || startsCrouchingLeft;
        bool startsStandingRight = IsRightFacingCrouchingPose(Pose) &&
            targetPose is
                StandingTransitionRightPose or StandingTransitionAimUpRightPose or
                StandingTransitionAimDiagonalUpRightPose or
                StandingTransitionAimDiagonalDownRightPose;
        bool startsStandingLeft = IsLeftFacingCrouchingPose(Pose) &&
            targetPose is
                StandingTransitionLeftPose or StandingTransitionAimUpLeftPose or
                StandingTransitionAimDiagonalUpLeftPose or
                StandingTransitionAimDiagonalDownLeftPose;
        bool startsStanding = startsStandingRight || startsStandingLeft;
        if (!startsCrouching && !startsStanding)
        {
            throw new NotSupportedException(
                $"Posture transition ${Pose:X2} -> ${targetPose:X2} is not translated.");
        }

        if (startsCrouching)
        {
            Pose = targetPose;
            RefreshCollisionRadii(bus);

            // Prospective command seven reads five from $91:ED36, installs the target's
            // radius 16, then moves center Y down five. Old radius 21 and new radius 16
            // therefore share exactly the same bottom collision boundary.
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + 5));
            InitializeAnimation(bus, initialFrame: 0);
            return true;
        }

        // Expanding from crouch radius 16 to standing-transition radius 21 invokes the
        // common pose-change collision routine before initialization. The shared helper
        // reads the target radius from this ROM rather than assuming the ordinary value 21.
        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
                bus,
                level,
                targetPose,
                nmiFrameCounter,
                out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Applies the ordinary Morph-Ball entry and exit records selected by the crouch and
    /// ball transition tables. This includes item gating, command-seven bottom alignment,
    /// and the block-only expansion collision used when unmorphing.
    /// </summary>
    /// <remarks>
    /// `$37/$38` shrink radius 16 to 7 and command seven moves center Y down nine pixels;
    /// this keeps the old crouching bottom boundary exactly fixed. `$3D/$3E` expand 7 to
    /// 16 through `$91:FDAE`; floor collision normally moves center up nine. If both sides
    /// constrain a radius-seven body, `$91:FFA7` rejects the target and keeps Samus morphed.
    /// </remarks>
    public bool TryApplyMorphTransition(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);

        bool startsMorphingRight = IsRightFacingCrouchingPose(Pose) &&
            targetPose == MorphingTransitionRightPose;
        bool startsMorphingLeft = IsLeftFacingCrouchingPose(Pose) &&
            targetPose == MorphingTransitionLeftPose;
        bool startsMorphing = startsMorphingRight || startsMorphingLeft;
        bool startsUnmorphingRight =
            Pose is MorphBallGroundRightPose or MorphBallMovingRightPose or
                MorphBallFallingRightPose or SpringBallGroundRightPose or
                SpringBallMovingRightPose or SpringBallFallingRightPose or
                SpringBallJumpRightPose &&
            targetPose == UnmorphingTransitionRightPose;
        bool startsUnmorphingLeft =
            Pose is MorphBallGroundLeftPose or MorphBallMovingLeftPose or
                MorphBallFallingLeftPose or SpringBallGroundLeftPose or
                SpringBallMovingLeftPose or SpringBallFallingLeftPose or
                SpringBallJumpLeftPose &&
            targetPose == UnmorphingTransitionLeftPose;
        bool startsUnmorphing = startsUnmorphingRight || startsUnmorphingLeft;
        if (!startsMorphing && !startsUnmorphing)
        {
            throw new NotSupportedException(
                $"Morph transition ${Pose:X2} -> ${targetPose:X2} is not a ROM-table route.");
        }

        if (startsMorphing)
        {
            // InitializeSamusPose_MorphingTransition at $91:F7CE restores PreviousPose and
            // returns carry set when bit $0004 is absent. No radius, animation, or position
            // write from the rejected prospective pose survives that native frame.
            if ((EquippedItems & 0x0004) == 0)
                return false;

            Pose = targetPose;
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + 9));

            // Prospective command seven also cancels an active bounce before starting the
            // transition. Ordinary crouch entry normally sees zero, but retaining the
            // literal writes makes externally stimulated debugger states deterministic.
            if (MorphBallBounceState != 0)
            {
                MorphBallBounceState = 0;
                Kinematics.YSubspeed = 0;
                Kinematics.YSpeed = 0;
                Kinematics.YDirection = 0;
            }
            InitializeAnimation(bus, initialFrame: 0);
            return true;
        }

        byte sourcePose = Pose;
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            out int centerAdjustment);
        if (collision != LargerPoseCollisionOutcome.Allowed)
        {
            // A morph source can only produce RetainSource here. Keep the defensive branch
            // explicit so a future caller cannot accidentally apply non-morph crouch fallback.
            if (collision == LargerPoseCollisionOutcome.CrouchFallback)
                ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
            return false;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Installs a stable ordinary Morph-Ball pose while preserving the shared eight-frame
    /// rolling animation exactly as <c>InitializeSamusPose_MorphBall</c> requests.
    /// </summary>
    public void ApplyMorphBallPoseChange(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool sourceSupported = IsStableBallPose(Pose);
        bool targetSupported = IsStableBallPose(targetPose);
        if (!sourceSupported || !targetSupported)
        {
            throw new NotSupportedException(
                $"Stable Morph-Ball transition ${Pose:X2} -> ${targetPose:X2} is not translated.");
        }

        byte previousDirection = ReadPoseXDirection(bus);
        int previousDelayList = AnimationDelayListAddress;
        Pose = targetPose;
        RefreshCollisionRadii(bus);

        // All ordinary ball poses point at `$91:B378`. Native writes $8000 to the new-frame
        // selector, causing `$91:FB5C` to return without changing frame OR timer. Assert the
        // table identity instead of depending on that retail-data fact silently.
        int targetDelayList = ResolveAnimationDelayList(bus);
        if (previousDelayList != targetDelayList)
        {
            throw new InvalidDataException(
                $"Morph-Ball transition selected mismatched animation lists ${previousDelayList:X6}/${targetDelayList:X6}.");
        }

        byte currentDirection = ReadPoseXDirection(bus);
        bool reversed = (previousDirection == 8 && currentDirection == 4) ||
            (previousDirection == 4 && currentDirection == 8);
        if (reversed)
        {
            // `$91:FA32-$91:FA52` folds run momentum into base speed with one 16-bit carry,
            // clears the extra words, and selects mode one so displacement initially keeps
            // travelling in the old direction while the ball decelerates through its turn.
            uint combined = unchecked(HorizontalSpeed.BaseFixed +
                ((uint)HorizontalSpeed.ExtraRunSpeed << 16) +
                HorizontalSpeed.ExtraRunSubspeed);
            HorizontalSpeed.BaseSpeed = unchecked((ushort)(combined >> 16));
            HorizontalSpeed.BaseSubspeed = unchecked((ushort)combined);
            HorizontalSpeed.ExtraRunSpeed = 0;
            HorizontalSpeed.ExtraRunSubspeed = 0;
            HorizontalSpeed.AccelerationMode = 1;
        }
    }

    /// <summary>
    /// Resolves an ordinary Morph-Ball downward collision through `$91:EA07` and
    /// `$91:F1FC`, including both automatic rebounds and the final grounded pose.
    /// </summary>
    /// <returns>True only when the collision ends grounded; false means another rebound.</returns>
    public bool ApplyMorphBallLanding(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsAirborneMorphBallPose(Pose) && !IsGroundedMorphBallPose(Pose))
            throw new InvalidOperationException($"Morph-Ball landing requires airborne pose $31/$32, not ${Pose:X2}.");

        if (MorphBallBounceState == 0 && Kinematics.YSpeed >= 3)
        {
            MorphBallBounceState = 1;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = ReadWord(bus, 0x909eb5);
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        if (MorphBallBounceState == 1)
        {
            MorphBallBounceState = 2;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(ReadWord(bus, 0x909eb5) - 1));
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        byte groundedPose = ReadPoseXDirection(bus) == 4
            ? MorphBallGroundLeftPose
            : MorphBallGroundRightPose;
        ApplyMorphBallPoseChange(bus, groundedPose);
        return true;
    }

    /// <summary>
    /// Resolves `$91:F25E` for Spring Ball. Holding Jump immediately calls the same
    /// cartridge-backed jump initializer as a grounded Spring Ball press; otherwise the
    /// low bounce byte advances through two rebounds while high byte `$0600` records the
    /// native Spring Ball bounce family.
    /// </summary>
    public bool ApplySpringBallLanding(ISnesAddressSpace bus, ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (!IsAirborneSpringBallPose(Pose) && !IsGroundedSpringBallPose(Pose))
            throw new InvalidOperationException($"Spring-Ball landing requires pose $7D-$80, not ${Pose:X2}.");

        if ((controllerInput & (ushort)SnesButton.A) != 0)
        {
            MorphBallBounceState = 0;
            SamusAerialMovement.InitializeDryAirJump(bus, this);
            byte jumpPose = ReadPoseXDirection(bus) == 4
                ? SpringBallJumpLeftPose
                : SpringBallJumpRightPose;
            ApplyMorphBallPoseChange(bus, jumpPose);
            return false;
        }

        byte bounce = unchecked((byte)MorphBallBounceState);
        if (bounce == 0 && Kinematics.YSpeed >= 3)
        {
            MorphBallBounceState = 0x0601;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = ReadWord(bus, 0x909eb5);
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        if (bounce == 1)
        {
            MorphBallBounceState = 0x0602;
            Kinematics.YDirection = 1;
            Kinematics.YSpeed = unchecked((ushort)(ReadWord(bus, 0x909eb5) - 1));
            Kinematics.YSubspeed = ReadWord(bus, 0x909eb7);
            return false;
        }

        MorphBallBounceState = 0;
        Kinematics.YDirection = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        byte groundPose = ReadPoseXDirection(bus) == 4
            ? SpringBallGroundLeftPose
            : SpringBallGroundRightPose;
        ApplyMorphBallPoseChange(bus, groundPose);
        return true;
    }

    /// <summary>
    /// Applies `$91:FC18` when grounded Spring Ball `$79/$7A` changes to `$7F/$80`.
    /// Moving-ground poses first pass through the same table-selected target, but the
    /// initializer only launches when the previous movement type was exactly `$11`.
    /// </summary>
    public void ApplySpringBallJump(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool validTarget = targetPose is SpringBallJumpRightPose or SpringBallJumpLeftPose;
        if (!IsGroundedSpringBallPose(Pose) || !validTarget)
            throw new NotSupportedException($"Spring-Ball jump ${Pose:X2} -> ${targetPose:X2} is not translated.");

        ApplyMorphBallPoseChange(bus, targetPose);
        MorphBallBounceState = 0;
        SamusAerialMovement.InitializeDryAirJump(bus, this);
    }

    /// <summary>
    /// Applies `$91:E8F2`'s type-four walk-off endpoint, retaining the rolling animation
    /// while changing to ordinary airborne pose `$31/$32` and starting dry-air gravity.
    /// </summary>
    public void ApplyMorphBallWalkOff(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool springBall = IsGroundedSpringBallPose(Pose);
        if (!IsGroundedMorphBallPose(Pose) && !springBall)
            throw new InvalidOperationException($"Morph-Ball walk-off requires grounded pose, not ${Pose:X2}.");

        byte fallingPose = ReadPoseXDirection(bus) == 4
            ? springBall ? SpringBallFallingLeftPose : MorphBallFallingLeftPose
            : springBall ? SpringBallFallingRightPose : MorphBallFallingRightPose;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 2;
        SamusAerialMovement.ConfigureDryAirGravity(bus, this);
        ApplyMorphBallPoseChange(bus, fallingPose);
    }

    /// <summary>
    /// Installs the ordinary or aimed falling pose selected by <c>$91:E8F2</c> when a
    /// grounded movement probe finds no floor. The collision command clears vertical
    /// speed and starts downward gravity before the pose is drawn.
    /// </summary>
    public void ApplyWalkedOffFloorTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool supportedSource =
            IsRightFacingStandingPose(Pose) || IsLeftFacingStandingPose(Pose) ||
            IsRightFacingRunningPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            IsRightFacingCrouchingPose(Pose) || IsLeftFacingCrouchingPose(Pose);
        byte expectedTarget = SelectFallingPoseForCurrentAim(bus);
        if (!supportedSource || targetPose != expectedTarget)
        {
            throw new NotSupportedException(
                $"Walk-off transition ${Pose:X2} -> ${targetPose:X2} is not translated.");
        }

        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 2;
        SamusAerialMovement.ConfigureDryAirGravity(bus, this);
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Ports the non-spinning direction lookup used by `$91:E8F2` for a grounded walk-off.
    /// </summary>
    public byte SelectFallingPoseForCurrentAim(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte shotDirection = ReadShotDirection(bus);
        return shotDirection switch
        {
            0 => FallingAimUpRightPose,
            1 => FallingAimDiagonalUpRightPose,
            2 => FallingRightPose,
            3 => FallingAimDiagonalDownRightPose,
            6 => FallingAimDiagonalDownLeftPose,
            7 => FallingLeftPose,
            8 => FallingAimDiagonalUpLeftPose,
            9 => FallingAimUpLeftPose,

            // Turn and crouch records store `$FB`/`$FF` rather than an arm direction.
            // Their facing byte still selects the ordinary unaimed falling pair.
            0xfb or 0xff => ReadPoseXDirection(bus) == 4
                ? FallingLeftPose
                : FallingRightPose,
            _ => throw new NotSupportedException(
                $"Walk-off shot direction ${shotDirection:X2} requires compact/downward falling collision handling."),
        };
    }

    /// <summary>
    /// Applies <c>$91:E95D</c>'s normal/spin/aimed landing choice and grounded collision cleanup
    /// at <c>$91:F010</c>. Expanding radius 19 to 21 moves Samus upward by two pixels so
    /// her feet stay on the same collision boundary, matching <c>$91:FF49</c>.
    /// </summary>
    public void ApplyAerialLanding(ISnesAddressSpace bus, bool wasSpinning)
    {
        ArgumentNullException.ThrowIfNull(bus);
        byte direction = ReadPoseXDirection(bus);
        bool facingLeft = direction == 4;
        byte targetPose;
        if (wasSpinning)
        {
            targetPose = facingLeft ? SpinLandingLeftPose : SpinLandingRightPose;
        }
        else
        {
            // `$91:E9F3` is indexed by pose-definition byte three. Horizontal directions
            // 2/7 select the ordinary `$A4/$A5`; the six admitted aim directions select
            // `$E0-$E5`. Compact straight-down directions 4/5 intentionally remain out.
            targetPose = ReadShotDirection(bus) switch
            {
                0 => LandingAimUpRightPose,
                1 => LandingAimDiagonalUpRightPose,
                2 => NormalLandingRightPose,
                3 => LandingAimDiagonalDownRightPose,
                6 => LandingAimDiagonalDownLeftPose,
                7 => NormalLandingLeftPose,
                8 => LandingAimDiagonalUpLeftPose,
                9 => LandingAimUpLeftPose,
                byte shotDirection => throw new NotSupportedException(
                    $"Landing from shot direction ${shotDirection:X2} requires an untranslated compact/firing route."),
            };
        }

        ushort oldRadius = Kinematics.YRadius;
        Pose = targetPose;
        RefreshCollisionRadii(bus);
        if (Kinematics.YRadius > oldRadius)
        {
            ushort difference = unchecked((ushort)(Kinematics.YRadius - oldRadius));
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition - difference));
        }

        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 0;
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>
    /// Applies `$91:E9F3` directions four/five when radius-ten straight-down Samus lands.
    /// Both entries select ordinary `$A4/$A5`; the 10 -> 21 expansion still runs the full
    /// block pose-change collision resolver before collision command five clears motion.
    /// </summary>
    public bool TryApplyCompactAerialLanding(
        ISnesAddressSpace bus,
        RoomLevelData level,
        ushort nmiFrameCounter)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(level);
        if (!IsCompactAerialPose(Pose))
            throw new InvalidOperationException($"Compact landing requires pose $17/$18/$2D/$2E, not ${Pose:X2}.");

        byte sourcePose = Pose;
        byte targetPose = ReadShotDirection(bus) switch
        {
            4 => NormalLandingRightPose,
            5 => NormalLandingLeftPose,
            byte shotDirection => throw new InvalidOperationException(
                $"Compact pose ${Pose:X2} has unexpected shot direction ${shotDirection:X2}."),
        };
        LargerPoseCollisionOutcome collision = ResolveLargerPoseCollision(
            bus,
            level,
            targetPose,
            nmiFrameCounter,
            out int centerAdjustment);
        if (collision == LargerPoseCollisionOutcome.Allowed)
        {
            Pose = targetPose;
            RefreshCollisionRadii(bus);
            Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
            InitializeAnimation(bus, initialFrame: 0);
        }
        else if (collision == LargerPoseCollisionOutcome.CrouchFallback)
        {
            ApplyPoseChangeCollisionCrouchFallback(bus, sourcePose);
        }

        // `$91:F010` collision command five runs after pose selection even when the larger
        // body falls back to crouch, so no launch/fall residue survives the landing seam.
        HorizontalSpeed.AccelerationMode = 0;
        HorizontalSpeed.BaseSpeed = 0;
        HorizontalSpeed.BaseSubspeed = 0;
        Kinematics.YSpeed = 0;
        Kinematics.YSubspeed = 0;
        Kinematics.YDirection = 0;
        return collision == LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Consumes command $FD/$F8's command-three pose operand for every animation route in
    /// the current grounded/ordinary-air slice.
    /// </summary>
    public bool ApplyPendingVerifiedAnimationTransition(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (PendingTransitionalPose is not byte targetPose)
            return false;

        bool verified = (Pose, targetPose) is
            (TurningRightToLeftPose, FacingLeftNormalPose) or
            (TurningLeftToRightPose, FacingRightNormalPose) or
            (TurningRightToLeftAimUpPose, StandingAimUpLeftPose) or
            (TurningLeftToRightAimUpPose, StandingAimUpRightPose) or
            (TurningRightToLeftAimDiagonalDownPose, StandingAimDiagonalDownLeftPose) or
            (TurningLeftToRightAimDiagonalDownPose, StandingAimDiagonalDownRightPose) or
            (TurningRightToLeftAimDiagonalUpPose, StandingAimDiagonalUpLeftPose) or
            (TurningLeftToRightAimDiagonalUpPose, StandingAimDiagonalUpRightPose) or
            (TurningRightToLeftCrouchingPose, CrouchingLeftPose) or
            (TurningLeftToRightCrouchingPose, CrouchingRightPose) or
            (TurningRightToLeftCrouchingAimUpPose, CrouchingAimUpLeftPose) or
            (TurningLeftToRightCrouchingAimUpPose, CrouchingAimUpRightPose) or
            (TurningRightToLeftCrouchingAimDiagonalDownPose, CrouchingAimDiagonalDownLeftPose) or
            (TurningLeftToRightCrouchingAimDiagonalDownPose, CrouchingAimDiagonalDownRightPose) or
            (TurningRightToLeftCrouchingAimDiagonalUpPose, CrouchingAimDiagonalUpLeftPose) or
            (TurningLeftToRightCrouchingAimDiagonalUpPose, CrouchingAimDiagonalUpRightPose) or
            (NeutralJumpTransitionRightPose, NeutralJumpRightPose) or
            (NeutralJumpTransitionLeftPose, NeutralJumpLeftPose) or
            (NormalJumpTransitionAimUpRightPose, NormalJumpAimUpRightPose) or
            (NormalJumpTransitionAimUpLeftPose, NormalJumpAimUpLeftPose) or
            (NormalJumpTransitionAimDiagonalUpRightPose, NormalJumpAimDiagonalUpRightPose) or
            (NormalJumpTransitionAimDiagonalUpLeftPose, NormalJumpAimDiagonalUpLeftPose) or
            (NormalJumpTransitionAimDiagonalDownRightPose, NormalJumpAimDiagonalDownRightPose) or
            (NormalJumpTransitionAimDiagonalDownLeftPose, NormalJumpAimDiagonalDownLeftPose) or
            (CrouchingTransitionRightPose, CrouchingRightPose) or
            (CrouchingTransitionLeftPose, CrouchingLeftPose) or
            (StandingTransitionRightPose, FacingRightNormalPose) or
            (StandingTransitionLeftPose, FacingLeftNormalPose) or
            (CrouchingTransitionAimUpRightPose, CrouchingAimUpRightPose) or
            (CrouchingTransitionAimUpLeftPose, CrouchingAimUpLeftPose) or
            (CrouchingTransitionAimDiagonalUpRightPose, CrouchingAimDiagonalUpRightPose) or
            (CrouchingTransitionAimDiagonalUpLeftPose, CrouchingAimDiagonalUpLeftPose) or
            (CrouchingTransitionAimDiagonalDownRightPose, CrouchingAimDiagonalDownRightPose) or
            (CrouchingTransitionAimDiagonalDownLeftPose, CrouchingAimDiagonalDownLeftPose) or
            (StandingTransitionAimUpRightPose, StandingAimUpRightPose) or
            (StandingTransitionAimUpLeftPose, StandingAimUpLeftPose) or
            (StandingTransitionAimDiagonalUpRightPose, StandingAimDiagonalUpRightPose) or
            (StandingTransitionAimDiagonalUpLeftPose, StandingAimDiagonalUpLeftPose) or
            (StandingTransitionAimDiagonalDownRightPose, StandingAimDiagonalDownRightPose) or
            (StandingTransitionAimDiagonalDownLeftPose, StandingAimDiagonalDownLeftPose) or
            (MorphingTransitionRightPose, MorphBallGroundRightPose) or
            (MorphingTransitionRightPose, MorphBallFallingRightPose) or
            (MorphingTransitionRightPose, SpringBallGroundRightPose) or
            (MorphingTransitionRightPose, SpringBallFallingRightPose) or
            (MorphingTransitionLeftPose, MorphBallGroundLeftPose) or
            (MorphingTransitionLeftPose, MorphBallFallingLeftPose) or
            (MorphingTransitionLeftPose, SpringBallGroundLeftPose) or
            (MorphingTransitionLeftPose, SpringBallFallingLeftPose) or
            (UnmorphingTransitionRightPose, CrouchingRightPose) or
            (UnmorphingTransitionLeftPose, CrouchingLeftPose) or
            (NormalLandingRightPose, FacingRightNormalPose) or
            (NormalLandingLeftPose, FacingLeftNormalPose) or
            (SpinLandingRightPose, FacingRightNormalPose) or
            (SpinLandingLeftPose, FacingLeftNormalPose) or
            (LandingAimUpRightPose, StandingAimUpRightPose) or
            (LandingAimUpLeftPose, StandingAimUpLeftPose) or
            (LandingAimDiagonalUpRightPose, StandingAimDiagonalUpRightPose) or
            (LandingAimDiagonalUpLeftPose, StandingAimDiagonalUpLeftPose) or
            (LandingAimDiagonalDownRightPose, StandingAimDiagonalDownRightPose) or
            (LandingAimDiagonalDownLeftPose, StandingAimDiagonalDownLeftPose);
        if (!verified)
        {
            throw new NotSupportedException(
                $"Animation transition ${Pose:X2} -> ${targetPose:X2} is outside the translated routes.");
        }

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Animation command");
        MorphBallBounceState = 0;
        return true;
    }

    /// <summary>
    /// Installs one already-validated grounded pose and runs the common $91:F404/$91:FB08
    /// metadata, radius, and frame-zero animation work modeled by this class.
    /// </summary>
    private void ApplySimpleGroundedPoseChange(
        ISnesAddressSpace bus,
        byte expectedPose,
        byte targetPose,
        string transitionName)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if (Pose != expectedPose)
        {
            throw new InvalidOperationException(
                $"{transitionName} requires pose ${expectedPose:X2}, not ${Pose:X2}.");
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        InitializeAnimation(bus, initialFrame: 0);
    }

    /// <summary>Last top-half index passed to bank-$81's Samus spritemap routine.</summary>
    public ushort TopSpritemapIndex { get; private set; }

    /// <summary>Last bottom-half index passed to bank-$81's Samus spritemap routine.</summary>
    public ushort BottomSpritemapIndex { get; private set; }

    /// <summary>Last screen-space origin calculated from world position and layer-1 scroll.</summary>
    public ushort SpritemapXPosition { get; private set; }

    /// <summary>Last screen-space origin calculated from world position and layer-1 scroll.</summary>
    public ushort SpritemapYPosition { get; private set; }

    /// <summary>The exact pending bank-$92 definitions consumed by accepted NMI.</summary>
    public SamusTileTransferState TileTransfers { get; } = new();

    /// <summary>
    /// Copies <c>SamusPalettes_PowerSuit</c> at <c>$9B:9400</c> to palette-buffer/CGRAM
    /// entries 192–207, porting <c>Samus_LoadSuitPalette</c>'s no-suit branch.
    /// </summary>
    public void LoadPowerSuitPalette(ISnesAddressSpace bus, SnesCgram cgram)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(cgram);

        // OBJ palettes begin at CGRAM 128. Spritemap palette 4 therefore resolves to 192,
        // exactly matching CopyToSamusSuitPalette's &palette_buffer[192] destination.
        cgram.LoadFromBus(bus, PowerSuitPalette, colorCount: 16, destinationIndex: 192);
    }

    /// <summary>
    /// Seeds pose/frame-zero graphics before the first NMI, as room/game setup must do
    /// before a visible normal-gameplay Samus can be constructed.
    /// </summary>
    public void PrimeGraphics(ISnesAddressSpace bus) =>
        TileTransfers.SelectForPoseFrame(bus, Pose, AnimationFrame);

    /// <summary>
    /// Seeds the animation frame timer as <c>Set_Samus_AnimationFrame_if_PoseChanged</c>
    /// at <c>$91:FB08</c> does for dry-room, no-speed standing state.
    /// </summary>
    public void InitializeAnimation(ISnesAddressSpace bus, ushort initialFrame = 0)
    {
        ArgumentNullException.ThrowIfNull(bus);

        AnimationFrame = initialFrame;
        AnimationFrameBuffer = XSpeedDivisor;
        AnimationDelayListAddress = ResolveAnimationDelayList(bus);
        byte initialDelay = ReadAnimationByte(bus, AnimationFrame);
        if ((initialDelay & 0x80) != 0)
        {
            throw new InvalidOperationException(
                $"Samus pose ${Pose:X2} cannot initialize directly on delay command ${initialDelay:X2}.");
        }

        AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + initialDelay));
        LastAnimationDelayCommand = null;
        PendingTransitionalPose = null;
    }

    /// <summary>
    /// Ports the dry-room path through <c>AnimateSamus</c> at <c>$90:8000</c>.
    /// </summary>
    /// <remarks>
    /// FX-specific water/lava/acid delay buffering and their damage/splash side effects are
    /// intentionally outside this method. Landing Site uses scrolling-sky FX $20, whose
    /// dispatch slot calls the same no-FX animation-buffer routine used here.
    /// </remarks>
    public void AnimateNoFx(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        EnsureAnimationInitialized(bus);

        // $90:8078 copies the horizontal speed divisor every frame before touching the
        // timer. It is zero for our stationary debugger stimulus.
        AnimationFrameBuffer = XSpeedDivisor;

        // $90:8032 keeps neutral-jump frame one alive in four-tick chunks while Samus is
        // still rising. This is intentionally tested before DEC and applies only when the
        // timer is exactly one; release/apex changes YDirection to two and lets it expire.
        if (Pose is NeutralJumpRightPose or NeutralJumpLeftPose &&
            Kinematics.YDirection != 2 &&
            AnimationFrame == 1 &&
            AnimationFrameTimer == 1)
        {
            AnimationFrameTimer = 4;
        }

        // DEC is 16-bit. A timer accidentally initialized to zero becomes $FFFF; BMI then
        // advances just like BEQ does for an ordinary 1 -> 0 expiration.
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameTimer - 1));
        if (AnimationFrameTimer != 0 && (AnimationFrameTimer & 0x8000) == 0)
            return;

        AnimationFrame = unchecked((ushort)(AnimationFrame + 1));
        HandleAnimationDelay(bus);
    }

    private void HandleAnimationDelay(ISnesAddressSpace bus)
    {
        byte delayOrCommand = ReadAnimationByte(bus, AnimationFrame);
        if ((delayOrCommand & 0x80) == 0)
        {
            LastAnimationDelayCommand = null;
            AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + delayOrCommand));
            return;
        }

        LastAnimationDelayCommand = delayOrCommand;
        switch (delayOrCommand & 0x0f)
        {
            case 6:
                // $90:8346, command $F6: healthy Samus loops to zero; below 30 energy she
                // advances past the command into the alternate breathing sequence.
                AnimationFrame = Health < 30
                    ? unchecked((ushort)(AnimationFrame + 1))
                    : (ushort)0;
                break;

            case 8:
                // $90:8370 falls through to command $FD's one-byte pose operand. For the
                // grounded $25/$26 sequences this publishes $02/$01 through command three;
                // it does NOT select a new delay or advance the visible animation frame.
                // The runtime consumes this after AnimateNoFx, where Samus_HandleTransitions
                // runs in the native frame. Jumping/autojump exclusions remain unsupported.
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + 1)));
                return;

            case 9:
                // $90:839A, command $F9 eeee gg aa GG AA. The mask is a little-endian
                // item word; unequipped/equipped each choose a grounded or airborne target
                // according to BOTH halves of Y speed. `$37/$38` test Spring Ball bit $0002
                // and use this command to finish ordinary morph entry without guessing
                // whether the transition walked off a ledge.
                ushort itemMask = unchecked((ushort)(
                    ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 1))) |
                    (ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 2))) << 8)));
                bool itemEquipped = (EquippedItems & itemMask) != 0;
                bool movingVertically = Kinematics.YSpeed != 0 || Kinematics.YSubspeed != 0;
                ushort targetOffset = itemEquipped
                    ? movingVertically ? (ushort)6 : (ushort)5
                    : movingVertically ? (ushort)4 : (ushort)3;
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + targetOffset)));
                return;

            case 13:
                // $90:83A0, command $FD pp: publish pose pp through the same command-three
                // seam as $F8. Unlike F8, FD has no auto-jump special case before it falls
                // through here. The one-frame $4B/$4C neutral-jump transitions use it.
                PendingTransitionalPose = ReadAnimationByte(
                    bus,
                    unchecked((ushort)(AnimationFrame + 1)));
                return;

            case 14:
                // $90:84C7, command $FE nn: move backward nn byte positions. The operand
                // remains in the same delay stream and is not itself an animation frame.
                byte backwardCount = ReadAnimationByte(bus, unchecked((ushort)(AnimationFrame + 1)));
                AnimationFrame = unchecked((ushort)(AnimationFrame - backwardCount));
                break;

            case 15:
                // $90:84DB, command $FF: unconditional loop to the start of the sequence.
                AnimationFrame = 0;
                break;

            default:
                // Other commands trigger pose transitions, equipment-dependent branches,
                // movement handlers, or speed-booster state. Failing at that boundary is
                // safer than displaying a plausible but fabricated continuation.
                throw new NotSupportedException(
                    $"Samus animation command ${delayOrCommand:X2} is not translated for pose ${Pose:X2}.");
        }

        byte selectedDelay = ReadAnimationByte(bus, AnimationFrame);
        if ((selectedDelay & 0x80) != 0)
        {
            throw new InvalidDataException(
                $"Samus animation command ${delayOrCommand:X2} selected another command ${selectedDelay:X2}.");
        }
        AnimationFrameTimer = unchecked((ushort)(AnimationFrameBuffer + selectedDelay));
    }

    private void EnsureAnimationInitialized(ISnesAddressSpace bus)
    {
        int expectedList = ResolveAnimationDelayList(bus);
        if (AnimationDelayListAddress != expectedList)
        {
            throw new InvalidOperationException(
                "Samus animation was not initialized for the current pose. Call InitializeAnimation after changing Pose.");
        }
    }

    private int ResolveAnimationDelayList(ISnesAddressSpace bus)
    {
        ushort pointer = ReadWord(bus, AddWithinBank(AnimationDelayPointerTable, Pose * 2));
        return 0x910000 | pointer;
    }

    private byte ReadAnimationByte(ISnesAddressSpace bus, ushort byteIndex) =>
        bus.ReadByte(AddWithinBank(AnimationDelayListAddress, byteIndex));

    /// <summary>
    /// Ports the standing and ordinary-running portions of <c>Samus_Draw</c> at
    /// <c>$90:85E2</c>.
    /// </summary>
    /// <remarks>
    /// Movement type zero uses the standing position selector. The admitted movement types
    /// `$01/$02/$04-$06/$08/$0E/$17` use the usual or explicitly table-backed transition
    /// position selector. Morph types `$04/$08` draw only their complete top spritemap;
    /// spin-jump `$03` retains its own conditional bottom rule.
    /// </remarks>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam, ushort layer1X, ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        int poseDefinition = AddWithinBank(PoseDefinitions, Pose * 8);
        byte movementType = bus.ReadByte(AddWithinBank(poseDefinition, 1));
        if (movementType is not (0 or 1 or 2 or 3 or 4 or 5 or 6 or 8 or
            0x0e or 0x0f or 0x11 or 0x12 or 0x13 or 0x17))
        {
            throw new NotSupportedException(
                $"Samus pose ${Pose:X2} uses movement type ${movementType:X2}; its rendering selector is not translated.");
        }

        // $90:8C94 sign-extends the byte at pose-definition offset four. Pose $01 stores
        // +6, moving the art origin six pixels above Samus's world-space center.
        sbyte graphicsYOffset = unchecked((sbyte)bus.ReadByte(AddWithinBank(poseDefinition, 4)));
        SpritemapXPosition = unchecked((ushort)(XPosition - layer1X));
        if (movementType == 0x0f && Pose is
            CrouchingTransitionRightPose or CrouchingTransitionLeftPose or
            MorphingTransitionRightPose or MorphingTransitionLeftPose or
            StandingTransitionRightPose or StandingTransitionLeftPose or
            UnmorphingTransitionRightPose or UnmorphingTransitionLeftPose)
        {
            // $90:8D3C indexes a signed byte by 2*(pose-$35)+animation frame instead of
            // using pose-definition graphics offset. The retail table has exactly two
            // signed bytes per pose `$35-$40`; transition animation commands replace the
            // pose before a command/operand index can reach this draw path.
            int transitionOffset = Pose switch
            {
                CrouchingTransitionRightPose or CrouchingTransitionLeftPose =>
                    AnimationFrame == 0 ? -8 : 0,
                MorphingTransitionRightPose or MorphingTransitionLeftPose =>
                    AnimationFrame == 0 ? -4 : -2,
                StandingTransitionRightPose or StandingTransitionLeftPose =>
                    AnimationFrame == 0 ? -4 : 0,
                UnmorphingTransitionRightPose or UnmorphingTransitionLeftPose =>
                    AnimationFrame == 0 ? 5 : 4,
                _ => throw new NotSupportedException(
                    $"Transition pose ${Pose:X2} does not use the translated `$90:8D80` offset table."),
            };
            SpritemapYPosition = unchecked((ushort)(YPosition + transitionOffset - layer1Y));
        }
        else
        {
            SpritemapYPosition = unchecked((ushort)(YPosition - graphicsYOffset - layer1Y));
        }

        ushort topBase = ReadWord(bus, AddWithinBank(TopSpritemapBaseIndexTable, Pose * 2));
        TopSpritemapIndex = unchecked((ushort)(topBase + AnimationFrame));
        oam.AddSamusSpritemap(bus, TopSpritemapIndex, SpritemapXPosition, SpritemapYPosition);

        // Movement types one, `$0E`, and `$17` use the native unconditional bottom selector.
        // Movement type zero also draws the bottom, except forward-facing pose $00 has an
        // additional visor OBJ that this intentionally narrow slice still rejects.
        if (movementType == 0 && Pose == 0)
            throw new NotSupportedException("Forward-facing Samus requires the standing visor OAM special case.");

        // $90:8686 suppresses the ordinary spin-jump bottom half for art frames 1..A;
        // those frames' top spritemaps contain the complete curled body. Frame zero and
        // frames B+ draw the split bottom. Screw/space-jump poses are future routes and
        // always draw their bottoms, but they are not admitted by this method yet.
        bool drawBottom = movementType is not (4 or 8 or 0x11 or 0x12 or 0x13) &&
            (movementType != 3 || AnimationFrame == 0 || AnimationFrame >= 0x0b);
        if (drawBottom)
        {
            ushort bottomBase = ReadWord(bus, AddWithinBank(BottomSpritemapBaseIndexTable, Pose * 2));
            BottomSpritemapIndex = unchecked((ushort)(bottomBase + AnimationFrame));
            oam.AddSamusSpritemap(bus, BottomSpritemapIndex, SpritemapXPosition, SpritemapYPosition);
        }

        // Native Samus_Draw always performs this selection after its conditional OAM work.
        // Those flags drive the following accepted NMI, so stepping exposes the authentic
        // one-main-loop/one-NMI producer-consumer relationship.
        TileTransfers.SelectForPoseFrame(bus, Pose, AnimationFrame);
    }

    private static ushort ReadWord(ISnesAddressSpace bus, int address) =>
        (ushort)(bus.ReadByte(address) | (bus.ReadByte(AddWithinBank(address, 1)) << 8));

    private static int AddWithinBank(int address, int byteCount) =>
        (address & 0xff0000) | ((address + byteCount) & 0xffff);

    private enum LargerPoseCollisionOutcome
    {
        Allowed,
        RetainSource,
        CrouchFallback,
    }

    /// <summary>
    /// Resolves the block-only portion of `HandlePoseChangeCollision` at `$91:FDAE` for
    /// a target whose Y radius is larger than the live body's radius.
    /// </summary>
    /// <remarks>
    /// The cartridge first probes the radius difference upward and downward while retaining
    /// the old radius. A one-sided hit calculates a compensating center shift, then probes
    /// that shift toward the opposite surface before committing it. Failure of that second
    /// probe restores the source pose; simultaneous initial hits select stable crouch. Solid-
    /// enemy collision is deliberately outside the current room slice, with no substitute.
    /// </remarks>
    private LargerPoseCollisionOutcome ResolveLargerPoseCollision(
        ISnesAddressSpace bus,
        RoomLevelData level,
        byte targetPose,
        ushort nmiFrameCounter,
        out int centerAdjustment)
    {
        int targetDefinition = AddWithinBank(PoseDefinitions, targetPose * 8);
        ushort targetRadius = bus.ReadByte(AddWithinBank(targetDefinition, 6));
        if (targetRadius <= Kinematics.YRadius)
        {
            centerAdjustment = 0;
            return LargerPoseCollisionOutcome.Allowed;
        }

        int radiusDifference = targetRadius - Kinematics.YRadius;

        // The probes run against independent copies. Native stores both available-distance
        // results before choosing a branch, so neither probe may move the live body early.
        SamusKinematicsState upwardProbe = CopyKinematics(Kinematics);
        BlockMoveResult upward = SamusBlockCollision.MoveVertical(
            bus,
            level,
            upwardProbe,
            displacement: unchecked(-radiusDifference << 16),
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
        SamusKinematicsState downwardProbe = CopyKinematics(Kinematics);
        BlockMoveResult downward = SamusBlockCollision.MoveVertical(
            bus,
            level,
            downwardProbe,
            displacement: radiusDifference << 16,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);

        if (upward.Collided && downward.Collided)
        {
            centerAdjustment = 0;
            // `$91:FFA7` compares the OLD live radius with eight. Radius seven means
            // Morph Ball, and that branch restores PreviousPose instead of trying to fit
            // stable crouch. Every non-morph compact/standing body still uses crouch.
            return Kinematics.YRadius < 8
                ? LargerPoseCollisionOutcome.RetainSource
                : LargerPoseCollisionOutcome.CrouchFallback;
        }

        centerAdjustment = 0;
        if (downward.Collided)
        {
            // `$91:FF49`: move away from the floor by
            // radiusDifference-spaceToMoveDown, preserving the old bottom boundary.
            int freeWholePixels = Math.Max(0, downward.AcceptedDisplacement >> 16);
            centerAdjustment = -(radiusDifference - freeWholePixels);

            // `$91:FF58` checks the proposed upward correction against the opposite side.
            // A collision here takes `$91:FE82` and restores PreviousPose; it does not use
            // `$91:FFA7`'s crouch selector because the initial flags were not both set.
            SamusKinematicsState oppositeProbe = CopyKinematics(Kinematics);
            BlockMoveResult opposite = SamusBlockCollision.MoveVertical(
                bus,
                level,
                oppositeProbe,
                displacement: centerAdjustment << 16,
                scanLeftToRight: (nmiFrameCounter & 1) == 0);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
        }
        else if (upward.Collided)
        {
            // `$91:FF20` is the mirror: move down only as far as necessary to clear the
            // ceiling while admitting the enlarged body.
            int acceptedWhole = unchecked((short)(upward.AcceptedDisplacement >> 16));
            int freeWholePixels = Math.Max(0, -acceptedWhole);
            centerAdjustment = radiusDifference - freeWholePixels;

            // `$91:FF2B` is the downward mirror of the opposite-side check above.
            SamusKinematicsState oppositeProbe = CopyKinematics(Kinematics);
            BlockMoveResult opposite = SamusBlockCollision.MoveVertical(
                bus,
                level,
                oppositeProbe,
                displacement: centerAdjustment << 16,
                scanLeftToRight: (nmiFrameCounter & 1) == 0);
            if (opposite.Collided)
                return LargerPoseCollisionOutcome.RetainSource;
        }

        return LargerPoseCollisionOutcome.Allowed;
    }

    /// <summary>
    /// Applies `$91:FFA7`'s non-morph fallback after simultaneous above/below collision.
    /// Direction metadata selects `$27/$28`; an aimed crouch consequently loses its aim.
    /// </summary>
    private void ApplyPoseChangeCollisionCrouchFallback(ISnesAddressSpace bus, byte sourcePose)
    {
        ushort oldRadius = Kinematics.YRadius;
        byte fallbackPose = ReadPoseXDirection(bus) == 4
            ? CrouchingLeftPose
            : CrouchingRightPose;
        if (sourcePose == fallbackPose)
            return;

        Pose = fallbackPose;
        RefreshCollisionRadii(bus);
        if (oldRadius < Kinematics.YRadius)
        {
            // `$91:FFD4-$91:FFE9` subtracts the extra crouch radius from center Y. This is
            // normally invisible for radius-16 aimed crouches, but compact radius ten must
            // move up six pixels when simultaneous initial probes force `$27/$28`.
            Kinematics.YPosition = unchecked((ushort)(
                Kinematics.YPosition - (Kinematics.YRadius - oldRadius)));
        }
        InitializeAnimation(bus, initialFrame: 0);
    }

    private static SamusKinematicsState CopyKinematics(SamusKinematicsState source) => new()
    {
        XPosition = source.XPosition,
        XSubposition = source.XSubposition,
        YPosition = source.YPosition,
        YSubposition = source.YSubposition,
        XRadius = source.XRadius,
        YRadius = source.YRadius,
        YSpeed = source.YSpeed,
        YSubspeed = source.YSubspeed,
        YDirection = source.YDirection,
        YAcceleration = source.YAcceleration,
        YSubacceleration = source.YSubacceleration,
        HorizontalSlopeCollisionEnable = source.HorizontalSlopeCollisionEnable,
        PositionAdjustedBySlope = source.PositionAdjustedBySlope,
    };
}
