using SuperMetroid.Core.Hardware;
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

    /// <summary>Pose $6D is right-facing falling aimed diagonally up.</summary>
    public const byte FallingAimDiagonalUpRightPose = 0x6d;

    /// <summary>Pose $6E is left-facing falling aimed diagonally up.</summary>
    public const byte FallingAimDiagonalUpLeftPose = 0x6e;

    /// <summary>Pose $6F is right-facing falling aimed diagonally down.</summary>
    public const byte FallingAimDiagonalDownRightPose = 0x6f;

    /// <summary>Pose $70 is left-facing falling aimed diagonally down.</summary>
    public const byte FallingAimDiagonalDownLeftPose = 0x70;

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

    /// <summary>True when a supported standing/running pose visibly carries an aim direction.</summary>
    public static bool IsGroundedAimPose(byte pose) => pose is
        StandingAimUpRightPose or StandingAimUpLeftPose or
        StandingAimDiagonalUpRightPose or StandingAimDiagonalUpLeftPose or
        StandingAimDiagonalDownRightPose or StandingAimDiagonalDownLeftPose or
        RunningAimUpRightPose or RunningAimUpLeftPose or
        RunningAimDiagonalUpRightPose or RunningAimDiagonalUpLeftPose or
        RunningAimDiagonalDownRightPose or RunningAimDiagonalDownLeftPose or
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

    /// <summary>True for the admitted right-facing movement-type-two normal-jump poses.</summary>
    public static bool IsRightFacingNormalJumpPose(byte pose) => pose is
        NeutralJumpTransitionRightPose or NeutralJumpRightPose or
        NormalJumpForwardRightPose or NormalJumpAimUpRightPose or
        NormalJumpTransitionAimUpRightPose or
        NormalJumpTransitionAimDiagonalUpRightPose or
        NormalJumpTransitionAimDiagonalDownRightPose or
        NormalJumpAimDiagonalUpRightPose or NormalJumpAimDiagonalDownRightPose;

    /// <summary>True for the admitted left-facing movement-type-two normal-jump poses.</summary>
    public static bool IsLeftFacingNormalJumpPose(byte pose) => pose is
        NeutralJumpTransitionLeftPose or NeutralJumpLeftPose or
        NormalJumpForwardLeftPose or NormalJumpAimUpLeftPose or
        NormalJumpTransitionAimUpLeftPose or
        NormalJumpTransitionAimDiagonalUpLeftPose or
        NormalJumpTransitionAimDiagonalDownLeftPose or
        NormalJumpAimDiagonalUpLeftPose or NormalJumpAimDiagonalDownLeftPose;

    /// <summary>True for admitted right-facing movement-type-six falling poses.</summary>
    public static bool IsRightFacingFallingPose(byte pose) => pose is
        FallingRightPose or FallingAimUpRightPose or
        FallingAimDiagonalUpRightPose or FallingAimDiagonalDownRightPose;

    /// <summary>True for admitted left-facing movement-type-six falling poses.</summary>
    public static bool IsLeftFacingFallingPose(byte pose) => pose is
        FallingLeftPose or FallingAimUpLeftPose or
        FallingAimDiagonalUpLeftPose or FallingAimDiagonalDownLeftPose;

    /// <summary>True for the aimed normal-jump/falling poses whose radius remains 19.</summary>
    public static bool IsAimedAerialPose(byte pose) => pose is
        NormalJumpAimUpRightPose or NormalJumpAimUpLeftPose or
        NormalJumpTransitionAimUpRightPose or NormalJumpTransitionAimUpLeftPose or
        NormalJumpTransitionAimDiagonalUpRightPose or NormalJumpTransitionAimDiagonalUpLeftPose or
        NormalJumpTransitionAimDiagonalDownRightPose or NormalJumpTransitionAimDiagonalDownLeftPose or
        NormalJumpAimDiagonalUpRightPose or NormalJumpAimDiagonalUpLeftPose or
        NormalJumpAimDiagonalDownRightPose or NormalJumpAimDiagonalDownLeftPose or
        FallingAimUpRightPose or FallingAimUpLeftPose or
        FallingAimDiagonalUpRightPose or FallingAimDiagonalUpLeftPose or
        FallingAimDiagonalDownRightPose or FallingAimDiagonalDownLeftPose;

    /// <summary>
    /// Applies a same-facing input/fallback transition within normal-jump type two or
    /// falling type six without replacing the live 16.16 velocity words.
    /// </summary>
    public void ApplyAerialAimTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
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
        if (!sameRightFamily && !sameLeftFamily)
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
    /// Applies bank-$91's grounded turn initialization at $91:F8D3 for pose $25 or $26.
    /// </summary>
    public void ApplyGroundedTurn(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);

        bool turnsLeft = targetPose == TurningRightToLeftPose &&
            Pose is FacingRightNormalPose or MovingRightNormalPose;
        bool turnsRight = targetPose == TurningLeftToRightPose &&
            Pose is FacingLeftNormalPose or MovingLeftNormalPose;
        if (!turnsLeft && !turnsRight)
        {
            throw new InvalidOperationException(
                $"Grounded turn ${Pose:X2} -> ${targetPose:X2} is not a verified transition.");
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

        ApplySimpleGroundedPoseChange(bus, Pose, targetPose, "Grounded turn");
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
    /// Applies the ordinary $35/$36 crouch-start or $3B/$3C stand-start transition,
    /// including command seven's bottom alignment and the larger-radius collision branch
    /// from <c>HandlePoseChangeCollision</c>.
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

        bool startsCrouching = (Pose, targetPose) is
            (FacingRightNormalPose or StandingAimUpRightPose or
                StandingAimDiagonalUpRightPose or StandingAimDiagonalDownRightPose or
                MovingRightNormalPose or RunningAimUpRightPose or
                RunningAimDiagonalUpRightPose or RunningAimDiagonalDownRightPose or
                NormalLandingRightPose or SpinLandingRightPose or
                LandingAimUpRightPose or LandingAimDiagonalUpRightPose or
                LandingAimDiagonalDownRightPose,
             CrouchingTransitionRightPose) or
            (FacingLeftNormalPose or StandingAimUpLeftPose or
                StandingAimDiagonalUpLeftPose or StandingAimDiagonalDownLeftPose or
                MovingLeftNormalPose or RunningAimUpLeftPose or
                RunningAimDiagonalUpLeftPose or RunningAimDiagonalDownLeftPose or
                NormalLandingLeftPose or SpinLandingLeftPose or
                LandingAimUpLeftPose or LandingAimDiagonalUpLeftPose or
                LandingAimDiagonalDownLeftPose,
             CrouchingTransitionLeftPose);
        bool startsStanding = (Pose, targetPose) is
            (CrouchingRightPose, StandingTransitionRightPose) or
            (CrouchingLeftPose, StandingTransitionLeftPose);
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
        // pose-change collision routine before initialization. Probe the exact five-pixel
        // radius difference using copies so rejected probes cannot corrupt live subpixels.
        SamusKinematicsState upwardProbe = CopyKinematics(Kinematics);
        BlockMoveResult upward = SamusBlockCollision.MoveVertical(
            bus,
            level,
            upwardProbe,
            displacement: unchecked((int)0xfffb0000),
            scanLeftToRight: (nmiFrameCounter & 1) == 0);
        SamusKinematicsState downwardProbe = CopyKinematics(Kinematics);
        BlockMoveResult downward = SamusBlockCollision.MoveVertical(
            bus,
            level,
            downwardProbe,
            displacement: 0x00050000,
            scanLeftToRight: (nmiFrameCounter & 1) == 0);

        if (upward.Collided && downward.Collided)
            return false;

        int centerAdjustment = 0;
        if (downward.Collided)
        {
            // $91:FF49 moves away from the floor by radiusDifference-spaceToMoveDown.
            int freeWholePixels = Math.Max(0, downward.AcceptedDisplacement >> 16);
            centerAdjustment = -(5 - freeWholePixels);
        }
        else if (upward.Collided)
        {
            // The mirror branch at $91:FF20 moves down when only the ceiling constrains
            // the enlarged body.
            int acceptedWhole = unchecked((short)(upward.AcceptedDisplacement >> 16));
            int freeWholePixels = Math.Max(0, -acceptedWhole);
            centerAdjustment = 5 - freeWholePixels;
        }

        Pose = targetPose;
        RefreshCollisionRadii(bus);
        Kinematics.YPosition = unchecked((ushort)(Kinematics.YPosition + centerAdjustment));
        InitializeAnimation(bus, initialFrame: 0);
        return true;
    }

    /// <summary>
    /// Installs the unaimed falling pose selected by <c>$91:E8F2</c> when a grounded
    /// movement probe finds no floor. The collision command clears vertical speed and
    /// starts downward gravity before the pose is drawn.
    /// </summary>
    public void ApplyWalkedOffFloorTransition(ISnesAddressSpace bus, byte targetPose)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bool supportedSource =
            IsRightFacingStandingPose(Pose) || IsLeftFacingStandingPose(Pose) ||
            IsRightFacingRunningPose(Pose) || IsLeftFacingRunningPose(Pose) ||
            Pose is
                TurningRightToLeftPose or TurningLeftToRightPose or
                CrouchingRightPose or CrouchingLeftPose;
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
    /// Movement type zero uses the standing position selector; movement type one uses the
    /// default selector. Both draw top and bottom halves. Other movement types still select
    /// specialized position or bottom-half rules and remain explicitly rejected.
    /// </remarks>
    public void Draw(ISnesAddressSpace bus, OamBuffer oam, ushort layer1X, ushort layer1Y)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(oam);

        int poseDefinition = AddWithinBank(PoseDefinitions, Pose * 8);
        byte movementType = bus.ReadByte(AddWithinBank(poseDefinition, 1));
        if (movementType is not (0 or 1 or 2 or 3 or 5 or 6 or 0x0e or 0x0f))
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
            StandingTransitionRightPose or StandingTransitionLeftPose)
        {
            // $90:8D3C indexes a signed byte by 2*(pose-$35)+animation frame instead of
            // using pose-definition graphics offset. For the four admitted poses frame
            // zero is -8 (crouch start) or -4 (stand start), and byte one is zero.
            int transitionOffset = Pose is
                CrouchingTransitionRightPose or CrouchingTransitionLeftPose
                    ? AnimationFrame == 0 ? -8 : 0
                    : AnimationFrame == 0 ? -4 : 0;
            SpritemapYPosition = unchecked((ushort)(YPosition + transitionOffset - layer1Y));
        }
        else
        {
            SpritemapYPosition = unchecked((ushort)(YPosition - graphicsYOffset - layer1Y));
        }

        ushort topBase = ReadWord(bus, AddWithinBank(TopSpritemapBaseIndexTable, Pose * 2));
        TopSpritemapIndex = unchecked((ushort)(topBase + AnimationFrame));
        oam.AddSamusSpritemap(bus, TopSpritemapIndex, SpritemapXPosition, SpritemapYPosition);

        // Movement types one and $0E use the native unconditional bottom-half selector.
        // Movement type zero also draws the bottom, except forward-facing pose $00 has an
        // additional visor OBJ that this intentionally narrow slice still rejects.
        if (movementType == 0 && Pose == 0)
            throw new NotSupportedException("Forward-facing Samus requires the standing visor OAM special case.");

        // $90:8686 suppresses the ordinary spin-jump bottom half for art frames 1..A;
        // those frames' top spritemaps contain the complete curled body. Frame zero and
        // frames B+ draw the split bottom. Screw/space-jump poses are future routes and
        // always draw their bottoms, but they are not admitted by this method yet.
        bool drawBottom = movementType != 3 || AnimationFrame == 0 || AnimationFrame >= 0x0b;
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
