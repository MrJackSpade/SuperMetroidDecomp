using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Standing, aerial, and gun-extended aim verification.</summary>
static void VerifySamusStandingAimMovement()
{
    var bus = new TestAddressSpace();

    // These eight records are copied from `$91:B631-$91:B669`. Byte two is especially
    // important: `$03/$05/$07 -> $01` and `$04/$06/$08 -> $02` when the entire controller
    // word becomes zero. Direction/shot metadata differs, while movement type and radius do not.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b641, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b649, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b651, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b659, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b661, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b669, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b679, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b691, [0x08, 0x01, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b699, [0x04, 0x01, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b6a1, [0x08, 0x01, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b6a9, [0x04, 0x01, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b6b1, [0x08, 0x01, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b6b9, [0x04, 0x01, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]);

    // Running movement type one reads the normal-air record at `$90:9F61`.
    bus.WriteBytes(0x909f61, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    // Every transition must resolve and restart its own bank-$91 delay list. Distinct
    // pointers make an accidental reuse of the source pose observable even though the
    // synthetic frame-zero delay is deliberately identical.
    for (int pose = 1; pose <= 0x12; pose++)
    {
        ushort stream = unchecked((ushort)(0xc100 + pose * 0x10));
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteBytes(0x910000 | stream, [0x0a, 0xf6]);
    }

    const int width = 8;
    const int height = 8;
    var blocks = new ushort[width * height];
    for (int x = 0; x < width; x++)
        blocks[4 * width + x] = 0x8000;
    RoomLevelData level = CreateRoom(
        width, height, blocks, new byte[blocks.Length]);

    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 43,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);

    byte[] rightAimRoute = [
        SamusPoseIds.StandingAimUpRightPose,
        SamusPoseIds.StandingAimDiagonalUpRightPose,
        SamusPoseIds.StandingAimDiagonalDownRightPose,
        SamusPoseIds.FacingRightNormalPose,
    ];
    foreach (byte target in rightAimRoute)
    {
        samus.HorizontalSpeed.BaseSpeed = 3;
        samus.ApplyGroundedAimTransition(bus, target);
        GroundedMovementResult result = SamusGroundedMovement.StepStandingRight(
            bus, level, samus, nmiFrameCounter: 0);
        AssertTrue(result.Vertical.Collided, $"right aim pose ${target:X2} remains grounded");
        AssertEqual(0, samus.HorizontalSpeed.BaseSpeed, $"right aim pose ${target:X2} clears base speed");
    }
    samus.ApplyGroundedAimTransition(bus, SamusPoseIds.StandingAimUpRightPose);
    AssertEqual(0x01, samus.ReadNoInputFallbackPose(bus), "right aimed no-input fallback");

    // The aimed running records execute the same movement-type-one dispatcher while their
    // own animation and shot-direction metadata remain selected. Cross the post-movement
    // transition seam in both directions to prove speed survives running-to-running aim
    // changes and standing clears it only on the following movement frame.
    samus.ApplyGroundedAimTransition(bus, SamusPoseIds.RunningAimDiagonalUpRightPose);
    ushort rightXBefore = samus.XPosition;
    for (ushort frame = 0; frame < 4; frame++)
        SamusGroundedMovement.StepRunningRight(bus, level, samus, frame);
    AssertTrue(samus.XPosition > rightXBefore, "right aimed run moves right");
    uint rightRunSpeed = samus.HorizontalSpeed.BaseFixed;
    samus.ApplyGroundedAimTransition(bus, SamusPoseIds.RunningAimDiagonalDownRightPose);
    AssertEqual(rightRunSpeed, samus.HorizontalSpeed.BaseFixed, "right running aim change preserves speed");
    samus.ApplyGroundedAimTransition(bus, SamusPoseIds.RunningAimUpRightPose);
    SamusGroundedMovement.StepRunningRight(bus, level, samus, nmiFrameCounter: 1);
    samus.ApplyGroundedAimTransition(bus, SamusPoseIds.StandingAimUpRightPose);
    SamusGroundedMovement.StepStandingRight(bus, level, samus, nmiFrameCounter: 0);
    AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "right aimed running-to-standing cleanup");

    // Install left-facing normal through a fresh state so the transition helper never
    // crosses families; native left records are an independent mirrored family.
    var leftSamus = new SamusState
    {
        Pose = SamusPoseIds.FacingLeftNormalPose,
        XPosition = 48,
        YPosition = 43,
    };
    leftSamus.RefreshCollisionRadii(bus);
    leftSamus.InitializeAnimation(bus);
    byte[] leftAimRoute = [
        SamusPoseIds.StandingAimUpLeftPose,
        SamusPoseIds.StandingAimDiagonalUpLeftPose,
        SamusPoseIds.StandingAimDiagonalDownLeftPose,
        SamusPoseIds.FacingLeftNormalPose,
    ];
    foreach (byte target in leftAimRoute)
    {
        leftSamus.ApplyGroundedAimTransition(bus, target);
        GroundedMovementResult result = SamusGroundedMovement.StepStandingLeft(
            bus, level, leftSamus, nmiFrameCounter: 1);
        AssertTrue(result.Vertical.Collided, $"left aim pose ${target:X2} remains grounded");
    }
    leftSamus.ApplyGroundedAimTransition(bus, SamusPoseIds.StandingAimUpLeftPose);
    AssertEqual(0x02, leftSamus.ReadNoInputFallbackPose(bus), "left aimed no-input fallback");

    leftSamus.ApplyGroundedAimTransition(bus, SamusPoseIds.RunningAimDiagonalUpLeftPose);
    ushort leftXBefore = leftSamus.XPosition;
    for (ushort frame = 0; frame < 4; frame++)
        SamusGroundedMovement.StepRunningLeft(bus, level, leftSamus, frame);
    AssertTrue(leftSamus.XPosition < leftXBefore, "left aimed run moves left");
    uint leftRunSpeed = leftSamus.HorizontalSpeed.BaseFixed;
    leftSamus.ApplyGroundedAimTransition(bus, SamusPoseIds.RunningAimDiagonalDownLeftPose);
    AssertEqual(leftRunSpeed, leftSamus.HorizontalSpeed.BaseFixed, "left running aim change preserves speed");
    leftSamus.ApplyGroundedAimTransition(bus, SamusPoseIds.RunningAimUpLeftPose);
    SamusGroundedMovement.StepRunningLeft(bus, level, leftSamus, nmiFrameCounter: 0);
    leftSamus.ApplyGroundedAimTransition(bus, SamusPoseIds.StandingAimUpLeftPose);
    SamusGroundedMovement.StepStandingLeft(bus, level, leftSamus, nmiFrameCounter: 1);
    AssertEqual(0u, leftSamus.HorizontalSpeed.BaseFixed, "left aimed running-to-standing cleanup");

    AssertThrows<InvalidOperationException>(
        () => leftSamus.ApplyGroundedAimTransition(
            bus, SamusPoseIds.StandingAimUpRightPose),
        "standing aim transition cannot cross facing families");

    Console.WriteLine("  Samus grounded aim: stationary/running ROM poses, movement, transitions, and fallbacks agree.");
}

/// <summary>
/// Verifies the equal-radius aimed jump/fall family, command-$FD launch, shot-direction
/// landing table, command-$F8 completion, walk-off selection, and velocity preservation.
/// </summary>
static void VerifySamusAimedAerialMovement()
{
    var bus = new TestAddressSpace();

    // Literal pose records used by this route. Ordinary aimed airborne bodies have radius
    // 19; straight-down `$17/$2D` use radius 10, and landing expands to radius 21.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(0x91b641, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]); // $03
    bus.WriteBytes(0x91b651, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]); // $05
    bus.WriteBytes(0x91b661, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]); // $07
    bus.WriteBytes(0x91b6d1, [0x08, 0x02, 0x51, 0x00, 0x08, 0x00, 0x13, 0x00]); // $15
    bus.WriteBytes(0x91b6e1, [0x08, 0x02, 0xff, 0x04, 0x06, 0x00, 0x0a, 0x00]); // $17
    bus.WriteBytes(0x91b6e9, [0x04, 0x02, 0xff, 0x05, 0x06, 0x00, 0x0a, 0x00]); // $18
    bus.WriteBytes(0x91b761, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]); // $27
    bus.WriteBytes(0x91b771, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $29
    bus.WriteBytes(0x91b781, [0x08, 0x06, 0x29, 0x00, 0x08, 0x00, 0x13, 0x00]); // $2B
    bus.WriteBytes(0x91b791, [0x08, 0x06, 0xff, 0x04, 0x06, 0x00, 0x0a, 0x00]); // $2D
    bus.WriteBytes(0x91b799, [0x04, 0x06, 0xff, 0x05, 0x06, 0x00, 0x0a, 0x00]); // $2E
    bus.WriteBytes(0x91b8b1, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $51
    bus.WriteBytes(0x91b8e1, [0x08, 0x02, 0xff, 0x01, 0x03, 0x00, 0x13, 0x00]); // $57
    bus.WriteBytes(0x91b971, [0x08, 0x02, 0x51, 0x01, 0x08, 0x00, 0x13, 0x00]); // $69
    bus.WriteBytes(0x91b981, [0x08, 0x02, 0x51, 0x03, 0x08, 0x00, 0x13, 0x00]); // $6B
    bus.WriteBytes(0x91b989, [0x04, 0x02, 0x52, 0x06, 0x08, 0x00, 0x13, 0x00]); // $6C
    bus.WriteBytes(0x91b991, [0x08, 0x06, 0x29, 0x01, 0x08, 0x00, 0x13, 0x00]); // $6D
    bus.WriteBytes(0x91b9a1, [0x08, 0x06, 0x29, 0x03, 0x08, 0x00, 0x13, 0x00]); // $6F
    bus.WriteBytes(0x91bb49, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]); // $A4
    bus.WriteBytes(0x91bb51, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]); // $A5
    bus.WriteBytes(0x91bd39, [0x08, 0x00, 0xff, 0x01, 0x03, 0x00, 0x15, 0x00]); // $E2
    bus.WriteBytes(0x91bd49, [0x08, 0x00, 0xff, 0x03, 0x03, 0x00, 0x15, 0x00]); // $E4

    // Synthetic delay streams expose both command-three seams independently. `$6D` uses
    // the retail `$02,$F0,$10,$FE,$01` sequence verbatim so the active animation-command
    // no-op is checked in the movement family that actually consumes it.
    (byte Pose, ushort Stream, byte[] Bytes)[] animations = [
        (0x01, 0xc100, [0x0a, 0xf6]),
        (0x03, 0xc110, [0x0a, 0xf6]),
        (0x05, 0xc120, [0x0a, 0xf6]),
        (0x07, 0xc130, [0x0a, 0xf6]),
        (0x15, 0xc140, [0x02, 0xff]),
        (0x17, 0xc148, [0x02, 0xff]),
        (0x18, 0xc14c, [0x02, 0xff]),
        (0x27, 0xc14e, [0x10, 0xff]),
        (0x29, 0xc150, [0x02, 0xff]),
        (0x2b, 0xc160, [0x02, 0xff]),
        (0x2d, 0xc168, [0x02, 0xff]),
        (0x2e, 0xc16c, [0x02, 0xff]),
        (0x51, 0xc170, [0x02, 0xff]),
        (0x57, 0xc180, [0x01, 0xfd, 0x69]),
        (0x69, 0xc190, [0x02, 0xff]),
        (0x6b, 0xc1a0, [0x02, 0xff]),
        (0x6c, 0xc1a8, [0x02, 0xff]),
        (0x6d, 0xc1b0, [0x02, 0xf0, 0x10, 0xfe, 0x01]),
        (0x6f, 0xc1c0, [0x02, 0xff]),
        (0xe2, 0xc1d0, [0x01, 0xf8, 0x05]),
        (0xe4, 0xc1e0, [0x01, 0xf8, 0x07]),
        (0xa4, 0xc1f0, [0x02, 0xf8, 0x01]),
        (0xa5, 0xc1f8, [0x02, 0xf8, 0x02]),
    ];
    foreach ((byte pose, ushort stream, byte[] bytes) in animations)
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteBytes(0x910000 | stream, bytes);
    }

    bus.WriteBytes(0x909eb9, [0x04, 0x00]);
    bus.WriteBytes(0x909ebf, [0x00, 0xe0]);
    bus.WriteBytes(0x909ea1, [0x00, 0x1c]);
    bus.WriteBytes(0x909ea7, [0x00, 0x00]);
    bus.WriteBytes(0x909f6d, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);
    bus.WriteBytes(0x909f9d, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    const int width = 8;
    const int height = 20;
    var blocks = new ushort[width * height];
    for (int x = 0; x < width; x++)
        blocks[12 * width + x] = 0x8000;
    RoomLevelData level = CreateRoom(
        width,
        height,
        blocks,
        new byte[blocks.Length],
        // Visual block definitions are ROM-native eight-byte records. The collision
        // test does not render them, but the room container deliberately enforces
        // that physical format even for synthetic fixtures.
        blockDefinitions: new byte[24]);

    var samus = new SamusState
    {
        Pose = SamusPoseIds.StandingAimDiagonalUpRightPose,
        XPosition = 48,
        YPosition = 171,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
    samus.ApplyOrdinaryJumpTransition(
        bus, SamusPoseIds.NormalJumpTransitionAimDiagonalUpRightPose);
    AssertEqual(21, samus.Kinematics.YRadius, "aimed jump commit retains standing radius");
    samus.RefreshCollisionRadii(bus);
    AssertEqual(19, samus.Kinematics.YRadius, "next alpha publishes aimed jump radius");
    ushort launchY = samus.YPosition;
    AerialMovementResult transitionFrame = SamusAerialMovement.StepNormalJump(
        bus, level, samus, (ushort)SnesButton.A, nmiFrameCounter: 0);
    AssertEqual(launchY, samus.YPosition, "aimed transition frame does not move vertically");
    AssertTrue(transitionFrame.Vertical is null, "aimed transition omits normal vertical pass");

    samus.AnimateNoFx(bus);
    AssertEqual(0xfd, samus.LastAnimationDelayCommand!.Value, "aimed transition reaches FD");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "aimed transition FD applies");
    AssertEqual(0x69, samus.Pose, "aimed transition FD target");

    ushort speedBeforeAimChange = samus.Kinematics.YSpeed;
    samus.ApplyAerialAimTransition(bus, SamusPoseIds.NormalJumpAimDiagonalDownRightPose);
    AssertEqual(speedBeforeAimChange, samus.Kinematics.YSpeed, "air aim change preserves Y speed");

    // `$17` is not merely alternate art: Down shrinks the live body from radius 19 to
    // radius 10. Shrinking never probes blocks and does not move the center. Releasing to
    // another aimed-jump pose expands through `$91:FDAE`; these few upward frames put the
    // body in open air so neither the upper nor lower probe needs to displace the center.
    for (ushort index = 0; index < 4; index++)
        SamusAerialMovement.StepNormalJump(bus, level, samus, (ushort)SnesButton.A, index);
    ushort compactCenterY = samus.YPosition;
    uint compactVelocity = samus.Kinematics.VerticalSpeedFixed;
    ushort compactDirection = samus.Kinematics.YDirection;
    AssertThrows<InvalidOperationException>(
        () => samus.ApplyAerialAimTransition(bus, SamusPoseIds.NormalJumpAimDownRightPose),
        "compact aim requires room-aware collision route");
    AssertTrue(
        samus.TryApplyCompactAerialTransition(
            bus, level, SamusPoseIds.NormalJumpAimDownRightPose, nmiFrameCounter: 4),
        "jump enters compact down-right pose");
    AssertEqual(0x17, samus.Pose, "compact jump pose");
    AssertEqual(10, samus.Kinematics.YRadius, "compact jump radius");
    AssertEqual(compactCenterY, samus.YPosition, "shrinking compact jump preserves center");
    AssertEqual(compactVelocity, samus.Kinematics.VerticalSpeedFixed, "compact entry preserves 16.16 velocity");
    AssertEqual(compactDirection, samus.Kinematics.YDirection, "compact entry preserves vertical direction");
    AssertTrue(
        samus.TryApplyCompactAerialTransition(
            bus, level, SamusPoseIds.NormalJumpAimDiagonalDownRightPose, nmiFrameCounter: 5),
        "jump exits compact down-right pose");
    AssertEqual(19, samus.Kinematics.YRadius, "compact jump exit radius");
    AssertEqual(compactCenterY, samus.YPosition, "open-air compact expansion preserves center");
    AssertEqual(compactVelocity, samus.Kinematics.VerticalSpeedFixed, "compact exit preserves 16.16 velocity");

    AerialMovementResult frame = default;
    for (int index = 0; index < 160; index++)
    {
        frame = SamusAerialMovement.StepNormalJump(
            bus, level, samus, (ushort)SnesButton.A, unchecked((ushort)index));
        if (frame.Landed)
            break;
    }
    AssertTrue(frame.Landed, "aimed normal jump lands");
    samus.ApplyAerialLanding(bus, wasSpinning: false);
    AssertEqual(0xe4, samus.Pose, "shot direction three selects E4 landing");
    AssertEqual(21, samus.Kinematics.YRadius, "aimed landing expands radius");
    AssertEqual(171, samus.YPosition, "aimed landing retains floor-aligned feet");
    samus.AnimateNoFx(bus);
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "aimed landing F8 applies");
    AssertEqual(0x07, samus.Pose, "E4 landing returns to down-right aim");

    // Falling `$2D` uses the movement-type-six dispatcher with the same radius-ten body.
    // Let collision align its bottom to the floor first, then apply shot-direction four's
    // `$A4` landing entry. The 11-pixel upward center correction keeps that exact boundary
    // fixed while collision command five clears all vertical and horizontal motion.
    var compactFall = new SamusState
    {
        Pose = SamusPoseIds.FallingAimDownRightPose,
        XPosition = 48,
        YPosition = 170,
    };
    compactFall.RefreshCollisionRadii(bus);
    compactFall.InitializeAnimation(bus);
    compactFall.Kinematics.YSpeed = 1;
    compactFall.Kinematics.YSubspeed = 0x4000;
    compactFall.Kinematics.YDirection = 2;
    SamusAerialMovement.ConfigureDryAirGravity(bus, compactFall);
    compactFall.HorizontalSpeed.BaseSpeed = 1;
    compactFall.HorizontalSpeed.BaseSubspeed = 0x8000;
    AerialMovementResult compactFallFrame = default;
    for (ushort index = 0; index < 40 && !compactFallFrame.Landed; index++)
        compactFallFrame = SamusAerialMovement.StepFalling(bus, level, compactFall, 0, index);
    AssertTrue(compactFallFrame.Landed, "compact falling pose reaches floor");
    ushort compactBottom = unchecked((ushort)(compactFall.YPosition + compactFall.Kinematics.YRadius));
    AssertTrue(
        compactFall.TryApplyCompactAerialLanding(bus, level, nmiFrameCounter: 0),
        "compact down-right landing expands successfully");
    AssertEqual(0xa4, compactFall.Pose, "shot direction four selects A4 landing");
    AssertEqual(21, compactFall.Kinematics.YRadius, "compact landing radius");
    AssertEqual(
        compactBottom,
        unchecked((ushort)(compactFall.YPosition + compactFall.Kinematics.YRadius)),
        "compact landing preserves floor boundary");
    AssertEqual(0u, compactFall.Kinematics.VerticalSpeedFixed, "compact landing clears vertical speed");
    AssertEqual(0, compactFall.Kinematics.YDirection, "compact landing clears vertical direction");
    AssertEqual(0u, compactFall.HorizontalSpeed.BaseFixed, "compact landing clears horizontal speed");

    // Issue #27's slot-zero capture lands radius-ten `$2D` exactly on the boundary between
    // the solid and air quadrants of two square half-blocks, then expands to radius 21.
    // A final-boundary-only eleven-pixel probe sees the air quadrant and grows downward
    // through the platform. `$94:96AB` first probes eight pixels, detects the solid lower
    // quadrant, and makes `$91:FF49` preserve the already-landed bottom boundary.
    var halfPlatformWords = new ushort[width * height];
    var halfPlatformBts = new byte[halfPlatformWords.Length];
    halfPlatformWords[12 * width + 2] = 0x1000;
    halfPlatformWords[12 * width + 3] = 0x1000;
    halfPlatformBts[12 * width + 2] = 0x02;
    halfPlatformBts[12 * width + 3] = 0x00;
    RoomLevelData halfPlatform = CreateRoom(
        width,
        height,
        halfPlatformWords,
        halfPlatformBts,
        blockDefinitions: new byte[24]);
    var capturedCompactLanding = new SamusState
    {
        Pose = SamusPoseIds.FallingAimDownRightPose,
        XPosition = 0x002e,
        YPosition = 0x00be,
    };
    capturedCompactLanding.RefreshCollisionRadii(bus);
    capturedCompactLanding.InitializeAnimation(bus);
    AssertEqual(0x00c8, capturedCompactLanding.Kinematics.BottomBoundary,
        "captured compact body begins on half-platform surface");
    AssertTrue(
        capturedCompactLanding.TryApplyCompactAerialLanding(
            bus, halfPlatform, nmiFrameCounter: 1),
        "captured half-platform compact landing expands");
    AssertEqual(0x00b3, capturedCompactLanding.YPosition,
        "captured half-platform landing shifts center up eleven pixels");
    AssertEqual(0x00c8, capturedCompactLanding.Kinematics.BottomBoundary,
        "captured half-platform landing retains feet on surface");

    // The mirrored definitions carry direction four and shot direction five. Exercise the
    // same family guard in open air so a future right-only shortcut cannot silently pass.
    var compactLeft = new SamusState
    {
        Pose = SamusPoseIds.NormalJumpAimDownLeftPose,
        XPosition = 48,
        YPosition = 100,
    };
    compactLeft.RefreshCollisionRadii(bus);
    compactLeft.InitializeAnimation(bus);
    AssertTrue(
        compactLeft.TryApplyCompactAerialTransition(
            bus, level, SamusPoseIds.NormalJumpAimDiagonalDownLeftPose, nmiFrameCounter: 1),
        "mirrored compact jump expands");
    AssertEqual(0x6c, compactLeft.Pose, "mirrored compact jump target");
    AssertEqual(19, compactLeft.Kinematics.YRadius, "mirrored compact jump radius");

    var compactLeftLanding = new SamusState
    {
        Pose = SamusPoseIds.FallingAimDownLeftPose,
        XPosition = 48,
        YPosition = 170,
    };
    compactLeftLanding.RefreshCollisionRadii(bus);
    compactLeftLanding.InitializeAnimation(bus);
    compactLeftLanding.Kinematics.YSpeed = 1;
    compactLeftLanding.Kinematics.YSubspeed = 0x4000;
    compactLeftLanding.Kinematics.YDirection = 2;
    SamusAerialMovement.ConfigureDryAirGravity(bus, compactLeftLanding);
    AerialMovementResult compactLeftFallFrame = default;
    for (ushort index = 0; index < 40 && !compactLeftFallFrame.Landed; index++)
    {
        compactLeftFallFrame = SamusAerialMovement.StepFalling(
            bus, level, compactLeftLanding, 0, index);
    }
    AssertTrue(compactLeftFallFrame.Landed, "mirrored compact falling pose reaches floor");
    ushort compactLeftBottom = unchecked((ushort)(
        compactLeftLanding.YPosition + compactLeftLanding.Kinematics.YRadius));
    AssertTrue(
        compactLeftLanding.TryApplyCompactAerialLanding(bus, level, nmiFrameCounter: 1),
        "compact down-left landing expands successfully");
    AssertEqual(0xa5, compactLeftLanding.Pose, "shot direction five selects A5 landing");
    AssertEqual(
        compactLeftBottom,
        unchecked((ushort)(compactLeftLanding.YPosition + compactLeftLanding.Kinematics.YRadius)),
        "mirrored compact landing preserves floor boundary");

    // Put radius-ten `$17` between a row-nine ceiling and row-twelve floor. The body itself
    // fits, but both independent nine-pixel probes collide. `$91:FFA7` therefore selects
    // ordinary `$27` and `$91:FFD4-$91:FFE9` moves the center up by 10-16 = -6 pixels.
    var boxedBlocks = (ushort[])blocks.Clone();
    for (int x = 0; x < width; x++)
        boxedBlocks[9 * width + x] = 0x8000;
    RoomLevelData boxedLevel = CreateRoom(
        width,
        height,
        boxedBlocks,
        new byte[boxedBlocks.Length],
        blockDefinitions: new byte[24]);
    var boxedCompact = new SamusState
    {
        Pose = SamusPoseIds.NormalJumpAimDownRightPose,
        XPosition = 48,
        YPosition = 176,
    };
    boxedCompact.RefreshCollisionRadii(bus);
    boxedCompact.InitializeAnimation(bus);
    AssertTrue(
        !boxedCompact.TryApplyCompactAerialTransition(
            bus, boxedLevel, SamusPoseIds.NormalJumpAimDiagonalDownRightPose, nmiFrameCounter: 0),
        "boxed compact expansion is rejected");
    AssertEqual(0x27, boxedCompact.Pose, "boxed compact expansion selects crouch");
    AssertEqual(10, boxedCompact.Kinematics.YRadius, "boxed compact fallback retains live radius until alpha");
    AssertEqual(170, boxedCompact.YPosition, "boxed compact fallback applies FFD4 offset");
    boxedCompact.RefreshCollisionRadii(bus);
    AssertEqual(16, boxedCompact.Kinematics.YRadius, "next alpha publishes compact fallback crouch radius");
    AssertEqual(170, boxedCompact.YPosition, "radius publication does not repeat center correction");

    // PSP_Falling chooses ordinary falling from the physics-facing byte. Aim is
    // reconsidered on the next input pass, rather than being preserved by the walk-off.
    samus.Pose = SamusPoseIds.StandingAimDiagonalUpRightPose;
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
    AssertEqual(SamusPoseIds.FallingRightPose, samus.SelectFallingPoseForCurrentAim(bus), "up-right walk-off target");
    samus.ApplyWalkedOffFloorTransition(bus, level, SamusPoseIds.FallingRightPose);
    AssertEqual(2, samus.Kinematics.YDirection, "aimed walk-off starts downward");
    samus.RefreshCollisionRadii(bus); // Next alpha precedes the airborne input/aim transition.
    samus.ApplyAerialAimTransition(bus, SamusPoseIds.FallingAimDiagonalUpRightPose);

    // `$90:8324-$8345` leaves both the command index and zero timer untouched when `$F0`
    // is reached. One frame later DEC wraps zero to `$FFFF`; the negative result advances
    // over `$F0` and loads the literal `$10` delay. This seemingly odd two-frame sequence
    // is why treating an unknown command as a normal delay—or simply skipping it—drifts.
    samus.AnimateNoFx(bus);
    AssertEqual(1, samus.AnimationFrameTimer, "aimed-fall initial delay counts down");
    samus.AnimateNoFx(bus);
    AssertEqual(1, samus.AnimationFrame, "aimed-fall reaches F0 command index");
    AssertEqual(0, samus.AnimationFrameTimer, "F0 leaves expired timer at zero");
    AssertEqual(0xf0, samus.LastAnimationDelayCommand!.Value, "aimed-fall records F0 command");
    samus.AnimateNoFx(bus);
    AssertEqual(2, samus.AnimationFrame, "timer underflow advances beyond F0");
    AssertEqual(16, samus.AnimationFrameTimer, "post-F0 frame loads literal delay");

    samus.ApplyAerialAimTransition(bus, SamusPoseIds.FallingAimDiagonalDownRightPose);
    AssertEqual(0x29, samus.ReadNoInputFallbackPose(bus), "aimed fall fallback target");
    samus.ApplyAerialAimTransition(bus, SamusPoseIds.FallingRightPose);
    AssertEqual(0x29, samus.Pose, "aimed fall applies unaimed fallback");

    Console.WriteLine("  Samus aimed air: FD jump, live aim, F0 cadence, compact hitboxes/landing, walk-off, and fall fallback agree.");
}

/// <summary>
/// Locks the complete ordinary-play horizontal-fire body family to literal bank-$91 pose
/// records and transition semantics. These checks intentionally separate body-pose work
/// from projectile ownership: a fired beam is a bank-$90 system, while `$0B/$13/$67` and
/// their mirrors are bank-$91 movement/animation states selected by the same controller word.
/// </summary>
static void VerifySamusGunExtendedMovement()
{
    var bus = new TestAddressSpace();

    // PoseDefinitions begins at `$91:B629` and each entry is exactly eight bytes:
    // X direction, movement type, no-input fallback, shot direction, collision command,
    // unused byte, Y radius, unused byte. These are direct retail bytes, not host fixtures
    // inferred from nearby poses.
    (byte Pose, byte[] Definition)[] definitions = [
        (0x01, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (0x02, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (0x09, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (0x0a, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (0x0b, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (0x0c, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (0x13, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x14, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0x4b, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x4c, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0x4d, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x4e, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0x51, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x52, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0x29, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x2a, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0x67, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x68, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0xa4, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]),
        (0xa5, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]),
        (0xe6, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]),
        (0xe7, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte pose, byte[] definition) in definitions)
        WritePoseDefinition(bus, pose, definition);

    // Synthetic storage keeps the tests compact, but each stream's bytes reproduce the
    // relevant retail control flow: running loops ten frames; jump/fall loop; firing
    // landings reach literal command `$F8,$01/$02`.
    (byte Pose, ushort Stream, byte[] Bytes)[] animations = [
        (0x01, 0xd000, [0x0a, 0xff]),
        (0x02, 0xd010, [0x0a, 0xff]),
        (0x09, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        (0x0a, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        (0x0b, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        (0x0c, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        (0x13, 0xd040, [0x02, 0x10, 0xfe, 0x01]),
        (0x14, 0xd040, [0x02, 0x10, 0xfe, 0x01]),
        (0x4b, 0xd048, [0x02, 0xfd, 0x4d]),
        (0x4c, 0xd04c, [0x02, 0xfd, 0x4e]),
        (0x4d, 0xd050, [0x02, 0x03, 0xfe, 0x01]),
        (0x4e, 0xd050, [0x02, 0x03, 0xfe, 0x01]),
        (0x51, 0xd058, [0x02, 0x03, 0xfe, 0x01]),
        (0x52, 0xd058, [0x02, 0x03, 0xfe, 0x01]),
        (0x29, 0xd060, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        (0x2a, 0xd060, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        (0x67, 0xd080, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        (0x68, 0xd080, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        (0xa4, 0xd0a0, [0x05, 0x02, 0xf8, 0x01]),
        (0xa5, 0xd0b0, [0x05, 0x02, 0xf8, 0x02]),
        (0xe6, 0xd0c0, [0x01, 0xf8, 0x01]),
        (0xe7, 0xd0d0, [0x01, 0xf8, 0x02]),
    ];
    foreach ((byte pose, ushort stream, byte[] bytes) in animations)
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteBytes(0x910000 | stream, bytes);
    }

    // Minimal transition programs retain the literal held masks from `$91:A1F8`,
    // `$91:A2F6`, and `$91:A70C`. They prove that Shot+forward, Shot in neutral jump,
    // and Shot while falling select the three distinct extended-gun movement families.
    WriteTestWord(bus, 0x919ef4, 0xd100); // pose $09 pointer
    bus.WriteBytes(0x91d100, [0x00, 0x00, 0x40, 0x01, 0x0b, 0x00, 0xff, 0xff]);
    WriteTestWord(bus, 0x919f7c, 0xd110); // pose $4D pointer
    bus.WriteBytes(0x91d110, [0x00, 0x00, 0x40, 0x00, 0x13, 0x00, 0xff, 0xff]);
    WriteTestWord(bus, 0x919f34, 0xd120); // pose $29 pointer
    bus.WriteBytes(0x91d120, [0x00, 0x00, 0x40, 0x00, 0x67, 0x00, 0xff, 0xff]);

    // `$51/$52` share the complete `$91:A2F6/$A376` tables with their neutral-jump
    // mirrors. These minimal copies retain the exact held-Jump record that caused the
    // playable runtime failure: required-new is zero, held `$0080`, targets `$4D/$4E`.
    WriteTestWord(bus, 0x919f84, 0xd130); // pose $51 pointer
    bus.WriteBytes(0x91d130, [0x00, 0x00, 0x80, 0x00, 0x4d, 0x00, 0xff, 0xff]);
    WriteTestWord(bus, 0x919f86, 0xd140); // pose $52 pointer
    bus.WriteBytes(0x91d140, [0x00, 0x00, 0x80, 0x00, 0x4e, 0x00, 0xff, 0xff]);

    // Preserve the decisive tail of retail pose `$0B`'s table at `$91:AE94`. Right+Shot
    // and Right both select `$0B` itself and therefore return directly; Shot without Right
    // reaches `$FFFF` and enters `$91:82D9`, whose pose-definition byte above is `$01`.
    // Collapsing those two null-transition paths is what made one tapped Right input remain
    // latched for as long as Shoot stayed down in the playable frontend.
    WriteTestWord(bus, 0x919ef8, 0xd150); // pose $0B pointer
    bus.WriteBytes(0x91d150, [
        0x00, 0x00, 0x40, 0x01, 0x0b, 0x00,
        0x00, 0x00, 0x00, 0x01, 0x0b, 0x00,
        0xff, 0xff,
    ]);

    AssertEqual(0x0b,
        SamusPoseTransitionTable.Find(bus, 0x09, 0x0140, 0)!.Value.ProspectivePose,
        "Shot+Right selects running gun extension");
    AssertEqual(0x13,
        SamusPoseTransitionTable.Find(bus, 0x4d, 0x0040, 0)!.Value.ProspectivePose,
        "Shot selects neutral-jump gun extension");
    AssertEqual(0x67,
        SamusPoseTransitionTable.Find(bus, 0x29, 0x0040, 0)!.Value.ProspectivePose,
        "Shot selects falling gun extension");

    SamusPoseTransitionLookup heldRightAndShot = SamusPoseTransitionTable.Lookup(
        bus,
        SamusPoseIds.MovingRightGunExtendedPose,
        (ushort)(SnesButton.Right | SnesButton.X),
        canonicalNewInput: 0);
    SamusPoseTransitionLookup releasedRightHoldingShot = SamusPoseTransitionTable.Lookup(
        bus,
        SamusPoseIds.MovingRightGunExtendedPose,
        (ushort)SnesButton.X,
        canonicalNewInput: 0);
    AssertTrue(!heldRightAndShot.UsesPoseDefinitionFallback,
        "running-gun Right+Shot same-pose record suppresses fallback");
    AssertTrue(releasedRightHoldingShot.UsesPoseDefinitionFallback,
        "running-gun Shot-only terminator requests fallback");

    SamusPoseTransition forwardRightRelease =
        SamusPoseTransitionTable.Find(bus, 0x51, (ushort)SnesButton.A, 0)!.Value;
    SamusPoseTransition forwardLeftRelease =
        SamusPoseTransitionTable.Find(bus, 0x52, (ushort)SnesButton.A, 0)!.Value;
    AssertEqual(0x4d, forwardRightRelease.ProspectivePose,
        "held Jump without Right selects neutral right jump");
    AssertEqual(0x4e, forwardLeftRelease.ProspectivePose,
        "held Jump without Left selects neutral left jump");

    // Apply both winners through the exact helper used by the runtime switch. Pose and
    // animation restart, while every live movement word survives the same-radius change.
    // Mirroring the assertion is important: `$51` and `$52` use adjacent pointer-table
    // entries, so a one-sided admission fix could make keyboard direction appear random.
    foreach ((byte source, byte target, string facing) in new[]
    {
        (SamusPoseIds.NormalJumpForwardRightPose, (byte)forwardRightRelease.ProspectivePose, "right"),
        (SamusPoseIds.NormalJumpForwardLeftPose, (byte)forwardLeftRelease.ProspectivePose, "left"),
    })
    {
        var releasedForwardJump = new SamusState { Pose = source };
        releasedForwardJump.RefreshCollisionRadii(bus);
        releasedForwardJump.InitializeAnimation(bus, initialFrame: 1);
        releasedForwardJump.Kinematics.YSpeed = 3;
        releasedForwardJump.Kinematics.YSubspeed = 0x4567;
        releasedForwardJump.Kinematics.YDirection = 1;
        releasedForwardJump.HorizontalSpeed.BaseSpeed = 2;
        releasedForwardJump.HorizontalSpeed.BaseSubspeed = 0x89ab;

        releasedForwardJump.ApplyAerialAimTransition(bus, target);

        AssertEqual(target, releasedForwardJump.Pose,
            $"{facing} forward-jump release installs neutral pose");
        AssertEqual(0, releasedForwardJump.AnimationFrame,
            $"{facing} forward-jump release restarts target animation");
        AssertEqual(0x00034567u, releasedForwardJump.Kinematics.VerticalSpeedFixed,
            $"{facing} forward-jump release preserves vertical velocity");
        AssertEqual(0x000289abu, releasedForwardJump.HorizontalSpeed.BaseFixed,
            $"{facing} forward-jump release preserves horizontal velocity");
        AssertEqual(1, releasedForwardJump.Kinematics.YDirection,
            $"{facing} forward-jump release preserves vertical direction");
    }

    // `$91:F50C` preserves the animation phase across movement-type-one arm changes. Begin
    // on a nonzero index so resetting to frame zero cannot accidentally satisfy the check.
    var running = new SamusState { Pose = SamusPoseIds.MovingRightNormalPose };
    running.RefreshCollisionRadii(bus);
    running.InitializeAnimation(bus, initialFrame: 4);
    ushort runningFrame = running.AnimationFrame;
    ushort runningTimer = running.AnimationFrameTimer;
    int runningDelayList = running.AnimationDelayListAddress;
    running.ApplyGroundedAimTransition(bus, SamusPoseIds.MovingRightGunExtendedPose);
    AssertEqual(0x0b, running.Pose, "running gun extension installs pose $0B");
    AssertEqual(runningFrame, running.AnimationFrame, "running gun extension preserves frame");
    AssertEqual(runningTimer, running.AnimationFrameTimer, "running gun extension preserves timer");
    AssertEqual(runningDelayList, running.AnimationDelayListAddress, "running gun extension retains shared delay list");
    running.ApplyGroundedAimTransition(bus, running.ReadNoInputFallbackPose(bus));
    AssertEqual(SamusPoseIds.FacingRightNormalPose, running.Pose,
        "releasing Right while holding Shot leaves running-gun pose");

    // Same-radius airborne arm changes must not perturb the live 16.16 velocity or direction.
    var jumping = new SamusState { Pose = SamusPoseIds.NeutralJumpRightPose };
    jumping.RefreshCollisionRadii(bus);
    jumping.InitializeAnimation(bus);
    jumping.Kinematics.YSpeed = 3;
    jumping.Kinematics.YSubspeed = 0x4567;
    jumping.Kinematics.YDirection = 1;
    jumping.ApplyAerialAimTransition(bus, SamusPoseIds.NormalJumpGunExtendedRightPose);
    AssertEqual(0x13, jumping.Pose, "neutral jump installs gun extension $13");
    AssertEqual(0x00034567u, jumping.Kinematics.VerticalSpeedFixed, "jump gun extension preserves velocity");
    AssertEqual(1, jumping.Kinematics.YDirection, "jump gun extension preserves direction");

    var falling = new SamusState { Pose = SamusPoseIds.FallingRightPose, YPosition = 100 };
    falling.RefreshCollisionRadii(bus);
    falling.InitializeAnimation(bus);
    falling.Kinematics.YSpeed = 2;
    falling.Kinematics.YSubspeed = 0xabcd;
    falling.Kinematics.YDirection = 2;
    falling.HorizontalSpeed.BaseSpeed = 1;
    falling.ApplyAerialAimTransition(bus, SamusPoseIds.FallingGunExtendedRightPose);
    AssertEqual(0x67, falling.Pose, "fall installs gun extension $67");
    AssertEqual(0x0002abcdu, falling.Kinematics.VerticalSpeedFixed, "fall gun extension preserves velocity");

    // `$91:E99B` samples HELD Shot, not newly pressed Shot. Holding X at horizontal impact
    // selects `$E6`; releasing it selects `$A4` from the same source metadata.
    falling.ApplyAerialLanding(bus, wasSpinning: false, (ushort)SnesButton.X);
    AssertEqual(0xe6, falling.Pose, "held Shot selects firing landing $E6");
    AssertEqual(21, falling.Kinematics.YRadius, "firing landing expands to standing radius");
    AssertEqual(0u, falling.Kinematics.VerticalSpeedFixed, "firing landing clears Y speed");
    AssertEqual(0u, falling.HorizontalSpeed.BaseFixed, "firing landing clears X speed");
    falling.AnimateNoFx(bus);
    AssertEqual(0xf8, falling.LastAnimationDelayCommand!.Value, "$E6 reaches F8 command");
    AssertTrue(falling.ApplyPendingVerifiedAnimationTransition(bus), "$E6 F8 transition applies");
    AssertEqual(0x01, falling.Pose, "$E6 returns to standing right");

    var releasedLanding = new SamusState { Pose = SamusPoseIds.FallingGunExtendedRightPose };
    releasedLanding.RefreshCollisionRadii(bus);
    releasedLanding.InitializeAnimation(bus);
    releasedLanding.ApplyAerialLanding(bus, wasSpinning: false, controllerInput: 0);
    AssertEqual(0xa4, releasedLanding.Pose, "released Shot selects ordinary landing $A4");

    var mirroredLanding = new SamusState { Pose = SamusPoseIds.FallingGunExtendedLeftPose };
    mirroredLanding.RefreshCollisionRadii(bus);
    mirroredLanding.InitializeAnimation(bus);
    mirroredLanding.ApplyAerialLanding(bus, wasSpinning: false, (ushort)SnesButton.X);
    AssertEqual(0xe7, mirroredLanding.Pose, "held Shot selects mirrored firing landing $E7");

    AssertTrue(SamusState.IsRightFacingRunningPose(0x0b), "$0B is admitted by running dispatcher");
    AssertTrue(SamusState.IsRightFacingNormalJumpPose(0x13), "$13 is admitted by jump dispatcher");
    AssertTrue(SamusState.IsRightFacingFallingPose(0x67), "$67 is admitted by falling dispatcher");
    AssertTrue(SamusState.IsRightFacingLandingPose(0xe6), "$E6 is admitted by landing dispatcher");
    AssertTrue(SamusState.IsLeftFacingLandingPose(0xe7), "$E7 is admitted by mirrored landing dispatcher");

    // All right/left landing pairs use the ordinary standing input table. Exercise every
    // same-facing neutral/aimed jump target so adding landing art can never again leave a
    // syntactically valid ROM transition outside the runtime switch. The private-ROM fight
    // exposed `$E2 -> $57`; it is one member of this complete table product, not a bespoke
    // exception for Bomb Torizo.
    byte[] rightLandingFamily = [0xa4, 0xa6, 0xe0, 0xe2, 0xe4, 0xe6];
    byte[] leftLandingFamily = [0xa5, 0xa7, 0xe1, 0xe3, 0xe5, 0xe7];
    byte[] rightLandingJumpTargets =
    [
        SamusPoseIds.NeutralJumpTransitionRightPose,
        SamusPoseIds.NormalJumpTransitionAimUpRightPose,
        SamusPoseIds.NormalJumpTransitionAimDiagonalUpRightPose,
        SamusPoseIds.NormalJumpTransitionAimDiagonalDownRightPose,
    ];
    byte[] leftLandingJumpTargets =
    [
        SamusPoseIds.NeutralJumpTransitionLeftPose,
        SamusPoseIds.NormalJumpTransitionAimUpLeftPose,
        SamusPoseIds.NormalJumpTransitionAimDiagonalUpLeftPose,
        SamusPoseIds.NormalJumpTransitionAimDiagonalDownLeftPose,
    ];
    foreach (byte sourcePose in rightLandingFamily)
    {
        foreach (byte targetPose in rightLandingJumpTargets)
        {
            AssertTrue(
                SamusState.IsLandingToNormalJumpTransition(sourcePose, targetPose),
                $"right landing ${sourcePose:X2} admits jump ${targetPose:X2}");
        }
        foreach (byte targetPose in leftLandingJumpTargets)
        {
            AssertTrue(
                !SamusState.IsLandingToNormalJumpTransition(sourcePose, targetPose),
                $"right landing ${sourcePose:X2} rejects mirrored jump ${targetPose:X2}");
        }
    }
    foreach (byte sourcePose in leftLandingFamily)
    {
        foreach (byte targetPose in leftLandingJumpTargets)
        {
            AssertTrue(
                SamusState.IsLandingToNormalJumpTransition(sourcePose, targetPose),
                $"left landing ${sourcePose:X2} admits jump ${targetPose:X2}");
        }
        foreach (byte targetPose in rightLandingJumpTargets)
        {
            AssertTrue(
                !SamusState.IsLandingToNormalJumpTransition(sourcePose, targetPose),
                $"left landing ${sourcePose:X2} rejects mirrored jump ${targetPose:X2}");
        }
    }

    // The private-ROM controller audit first exposed `$E6 -> $4B`: Shoot remained held
    // through landing, then a fresh Jump edge arrived before `$F8`. Run both mirrors through
    // the real initializer so this verifies radius, animation, and vertical launch state in
    // addition to the family predicate used by the runtime switch.
    var firingLandingJumpRight = new SamusState { Pose = SamusPoseIds.FiringLandingRightPose };
    firingLandingJumpRight.RefreshCollisionRadii(bus);
    firingLandingJumpRight.ApplyOrdinaryJumpTransition(
        bus, SamusPoseIds.NeutralJumpTransitionRightPose);
    AssertEqual(SamusPoseIds.NeutralJumpTransitionRightPose, firingLandingJumpRight.Pose,
        "right firing landing installs neutral jump $4B");
    AssertEqual(21, firingLandingJumpRight.Kinematics.YRadius,
        "right firing-landing jump retains landing radius");
    firingLandingJumpRight.RefreshCollisionRadii(bus);
    AssertEqual(19, firingLandingJumpRight.Kinematics.YRadius,
        "right firing-landing next alpha installs normal-jump radius");
    AssertEqual(1, firingLandingJumpRight.Kinematics.YDirection,
        "right firing-landing jump launches upward");

    var firingLandingJumpLeft = new SamusState { Pose = SamusPoseIds.FiringLandingLeftPose };
    firingLandingJumpLeft.RefreshCollisionRadii(bus);
    firingLandingJumpLeft.ApplyOrdinaryJumpTransition(
        bus, SamusPoseIds.NeutralJumpTransitionLeftPose);
    AssertEqual(SamusPoseIds.NeutralJumpTransitionLeftPose, firingLandingJumpLeft.Pose,
        "left firing landing installs neutral jump $4C");
    AssertEqual(21, firingLandingJumpLeft.Kinematics.YRadius,
        "left firing-landing jump retains landing radius");
    firingLandingJumpLeft.RefreshCollisionRadii(bus);
    AssertEqual(19, firingLandingJumpLeft.Kinematics.YRadius,
        "left firing-landing next alpha installs normal-jump radius");
    AssertEqual(1, firingLandingJumpLeft.Kinematics.YDirection,
        "left firing-landing jump launches upward");

    var aimedLandingJumpRight = new SamusState
    {
        Pose = SamusPoseIds.LandingAimDiagonalUpRightPose,
    };
    aimedLandingJumpRight.ApplyOrdinaryJumpTransition(
        bus,
        SamusPoseIds.NormalJumpTransitionAimDiagonalUpRightPose);
    AssertEqual(SamusPoseIds.NormalJumpTransitionAimDiagonalUpRightPose,
        aimedLandingJumpRight.Pose,
        "right diagonal-up landing installs aimed jump $57");
    AssertEqual(1, aimedLandingJumpRight.Kinematics.YDirection,
        "right diagonal-up landing jump launches upward");

    Console.WriteLine("  Samus horizontal fire: ROM selection, six extended bodies, forward-jump release, run-phase preservation, and firing landings agree.");
}

/// <summary>
/// Fixes the two ROM tables and signed formulas used by Landing Site's actual BTS-$12
/// non-square floor path at <c>$94:84D6</c> and <c>$94:87F4</c>.
/// </summary>
}
