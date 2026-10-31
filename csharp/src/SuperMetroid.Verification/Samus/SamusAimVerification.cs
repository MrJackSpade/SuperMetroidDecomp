using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.SyntheticSamusCartridgeVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks bank-$91 aiming poses, movement transitions and their command-line verification routes.</summary>
internal static class SamusAimVerification
{

/// <summary>Standing, aerial, and gun-extended aim verification.</summary>
internal static void VerifySamusStandingAimMovement()
{
    var bus = new TestAddressSpace();

    // These eight records are copied from `$91:B631-$91:B669`. Byte two is especially
    // important: `$03/$05/$07 -> $01` and `$04/$06/$08 -> $02` when the entire controller
    // word becomes zero. Direction/shot metadata differs, while movement type and radius do not.
    WritePoseDefinition(bus, 0x01, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x02, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x03, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x04, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x05, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x06, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x07, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x08, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x09, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x0A, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x0D, [0x08, 0x01, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x0E, [0x04, 0x01, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x0F, [0x08, 0x01, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x10, [0x04, 0x01, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x11, [0x08, 0x01, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, 0x12, [0x04, 0x01, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]);

    // Running movement type one reads the normal-air record at `$90:9F61`.
    SyntheticSamusCartridge.WriteNormalAirSpeedEntry(bus, SamusMovementType.Running, [
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
        WritePoseAnimationDelays(bus, (SamusPoseId)pose, stream, [0x0a, 0xf6]);
    }

    const int width = 8;
    const int height = 8;
    var blocks = new RoomTerrainBlock[width * height];
    for (int x = 0; x < width; x++)
        blocks[4 * width + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData level = CreateRoom(new(width, height), blocks, new RoomBlockBehavior[blocks.Length]);

    var samus = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.FacingRightNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 43)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    SamusPresentation.InitializeAnimation(samus);

    SamusPoseId[] rightAimRoute = [
        SamusPoseId.StandingAimUpRightPose,
        SamusPoseId.StandingAimDiagonalUpRightPose,
        SamusPoseId.StandingAimDiagonalDownRightPose,
        SamusPoseId.FacingRightNormalPose,
    ];
    foreach (SamusPoseId target in rightAimRoute)
    {
        samus.HorizontalSpeed.BaseSpeed = samus.HorizontalSpeed.BaseSpeed.WithWhole(3);
        SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, target);
        GroundedMovementResult result = SamusGroundedMovement.StepStanding(
level, samus, frameCounter: 0, controllerInput: 0);
        AssertTrue(result.Vertical.Collided, $"right aim pose ${(int)target:X2} remains grounded");
        AssertEqual(0, samus.HorizontalSpeed.BaseSpeed.Whole, $"right aim pose ${(int)target:X2} clears base speed");
    }
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, SamusPoseId.StandingAimUpRightPose);
    AssertEqual(0x01, (byte)SamusPoseMetadata.ReadNoInputFallbackPose(samus), "right aimed no-input fallback");

    // The aimed running records execute the same movement-type-one dispatcher while their
    // own animation and shot-direction metadata remain selected. Cross the post-movement
    // transition seam in both directions to prove speed survives running-to-running aim
    // changes and standing clears it only on the following movement frame.
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, SamusPoseId.RunningAimDiagonalUpRightPose);
    ushort rightXBefore = samus.Position.X.Whole;
    for (ushort frame = 0; frame < 4; frame++)
        SamusGroundedMovement.StepRunning(level, samus, frame);
    AssertTrue(samus.Position.X.Whole > rightXBefore, "right aimed run moves right");
    uint rightRunSpeed = samus.HorizontalSpeed.BaseFixed;
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, SamusPoseId.RunningAimDiagonalDownRightPose);
    AssertEqual(rightRunSpeed, samus.HorizontalSpeed.BaseFixed, "right running aim change preserves speed");
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, SamusPoseId.RunningAimUpRightPose);
    SamusGroundedMovement.StepRunning(level, samus, frameCounter: 1);
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(samus, SamusPoseId.StandingAimUpRightPose);
    SamusGroundedMovement.StepStanding(level, samus, frameCounter: 0, controllerInput: 0);
    AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "right aimed running-to-standing cleanup");

    // Install left-facing normal through a fresh state so the transition helper never
    // crosses families; native left records are an independent mirrored family.
    var leftSamus = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.FacingLeftNormalPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 43)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(leftSamus);
    SamusPresentation.InitializeAnimation(leftSamus);
    SamusPoseId[] leftAimRoute = [
        SamusPoseId.StandingAimUpLeftPose,
        SamusPoseId.StandingAimDiagonalUpLeftPose,
        SamusPoseId.StandingAimDiagonalDownLeftPose,
        SamusPoseId.FacingLeftNormalPose,
    ];
    foreach (SamusPoseId target in leftAimRoute)
    {
        SamusGroundedPoseTransitions.ApplyGroundedAimTransition(leftSamus, target);
        GroundedMovementResult result = SamusGroundedMovement.StepStanding(
level, leftSamus, frameCounter: 1, controllerInput: 0);
        AssertTrue(result.Vertical.Collided, $"left aim pose ${(int)target:X2} remains grounded");
    }
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(leftSamus, SamusPoseId.StandingAimUpLeftPose);
    AssertEqual(0x02, (byte)SamusPoseMetadata.ReadNoInputFallbackPose(leftSamus), "left aimed no-input fallback");

    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(leftSamus, SamusPoseId.RunningAimDiagonalUpLeftPose);
    ushort leftXBefore = leftSamus.Position.X.Whole;
    for (ushort frame = 0; frame < 4; frame++)
        SamusGroundedMovement.StepRunning(level, leftSamus, frame);
    AssertTrue(leftSamus.Position.X.Whole < leftXBefore, "left aimed run moves left");
    uint leftRunSpeed = leftSamus.HorizontalSpeed.BaseFixed;
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(leftSamus, SamusPoseId.RunningAimDiagonalDownLeftPose);
    AssertEqual(leftRunSpeed, leftSamus.HorizontalSpeed.BaseFixed, "left running aim change preserves speed");
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(leftSamus, SamusPoseId.RunningAimUpLeftPose);
    SamusGroundedMovement.StepRunning(level, leftSamus, frameCounter: 0);
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(leftSamus, SamusPoseId.StandingAimUpLeftPose);
    SamusGroundedMovement.StepStanding(level, leftSamus, frameCounter: 1, controllerInput: 0);
    AssertEqual(0u, leftSamus.HorizontalSpeed.BaseFixed, "left aimed running-to-standing cleanup");

    AssertThrows<InvalidOperationException>(
        () => SamusGroundedPoseTransitions.ApplyGroundedAimTransition(leftSamus, 
            SamusPoseId.StandingAimUpRightPose),
        "standing aim transition cannot cross facing families");

    Console.WriteLine("  Samus grounded aim: stationary/running ROM poses, movement, transitions, and fallbacks agree.");
}

/// <summary>
/// Verifies the equal-radius aimed jump/fall family, command-$FD launch, shot-direction
/// landing table, command-$F8 completion, walk-off selection, and velocity preservation.
/// </summary>
internal static void VerifySamusAimedAerialMovement()
{
    var bus = new TestAddressSpace();

    // Literal pose records used by this route. Ordinary aimed airborne bodies have radius
    // 19; straight-down `$17/$2D` use radius 10, and landing expands to radius 21.
    WritePoseDefinition(bus, 0x01, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    WritePoseDefinition(bus, 0x03, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]); // $03
    WritePoseDefinition(bus, 0x05, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]); // $05
    WritePoseDefinition(bus, 0x07, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]); // $07
    WritePoseDefinition(bus, 0x15, [0x08, 0x02, 0x51, 0x00, 0x08, 0x00, 0x13, 0x00]); // $15
    WritePoseDefinition(bus, 0x17, [0x08, 0x02, 0xff, 0x04, 0x06, 0x00, 0x0a, 0x00]); // $17
    WritePoseDefinition(bus, 0x18, [0x04, 0x02, 0xff, 0x05, 0x06, 0x00, 0x0a, 0x00]); // $18
    WritePoseDefinition(bus, 0x27, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]); // $27
    WritePoseDefinition(bus, 0x29, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $29
    WritePoseDefinition(bus, 0x2B, [0x08, 0x06, 0x29, 0x00, 0x08, 0x00, 0x13, 0x00]); // $2B
    WritePoseDefinition(bus, 0x2D, [0x08, 0x06, 0xff, 0x04, 0x06, 0x00, 0x0a, 0x00]); // $2D
    WritePoseDefinition(bus, 0x2E, [0x04, 0x06, 0xff, 0x05, 0x06, 0x00, 0x0a, 0x00]); // $2E
    WritePoseDefinition(bus, 0x51, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $51
    WritePoseDefinition(bus, 0x57, [0x08, 0x02, 0xff, 0x01, 0x03, 0x00, 0x13, 0x00]); // $57
    WritePoseDefinition(bus, 0x69, [0x08, 0x02, 0x51, 0x01, 0x08, 0x00, 0x13, 0x00]); // $69
    WritePoseDefinition(bus, 0x6B, [0x08, 0x02, 0x51, 0x03, 0x08, 0x00, 0x13, 0x00]); // $6B
    WritePoseDefinition(bus, 0x6C, [0x04, 0x02, 0x52, 0x06, 0x08, 0x00, 0x13, 0x00]); // $6C
    WritePoseDefinition(bus, 0x6D, [0x08, 0x06, 0x29, 0x01, 0x08, 0x00, 0x13, 0x00]); // $6D
    WritePoseDefinition(bus, 0x6F, [0x08, 0x06, 0x29, 0x03, 0x08, 0x00, 0x13, 0x00]); // $6F
    WritePoseDefinition(bus, 0xA4, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]); // $A4
    WritePoseDefinition(bus, 0xA5, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]); // $A5
    WritePoseDefinition(bus, 0xE2, [0x08, 0x00, 0xff, 0x01, 0x03, 0x00, 0x15, 0x00]); // $E2
    WritePoseDefinition(bus, 0xE4, [0x08, 0x00, 0xff, 0x03, 0x03, 0x00, 0x15, 0x00]); // $E4

    // Synthetic delay streams expose both command-three seams independently. `$6D` uses
    // the retail `$02,$F0,$10,$FE,$01` sequence verbatim so the active animation-command
    // no-op is checked in the movement family that actually consumes it.
    Animation[] animations = [
        new Animation(SamusPoseId.FacingRightNormalPose, 0xc100, [0x0a, 0xf6]),
        new Animation(SamusPoseId.StandingAimUpRightPose, 0xc110, [0x0a, 0xf6]),
        new Animation(SamusPoseId.StandingAimDiagonalUpRightPose, 0xc120, [0x0a, 0xf6]),
        new Animation(SamusPoseId.StandingAimDiagonalDownRightPose, 0xc130, [0x0a, 0xf6]),
        new Animation(SamusPoseId.NormalJumpAimUpRightPose, 0xc140, [0x02, 0xff]),
        new Animation(SamusPoseId.NormalJumpAimDownRightPose, 0xc148, [0x02, 0xff]),
        new Animation(SamusPoseId.NormalJumpAimDownLeftPose, 0xc14c, [0x02, 0xff]),
        new Animation(SamusPoseId.CrouchingRightPose, 0xc14e, [0x10, 0xff]),
        new Animation(SamusPoseId.FallingRightPose, 0xc150, [0x02, 0xff]),
        new Animation(SamusPoseId.FallingAimUpRightPose, 0xc160, [0x02, 0xff]),
        new Animation(SamusPoseId.FallingAimDownRightPose, 0xc168, [0x02, 0xff]),
        new Animation(SamusPoseId.FallingAimDownLeftPose, 0xc16c, [0x02, 0xff]),
        new Animation(SamusPoseId.NormalJumpForwardRightPose, 0xc170, [0x02, 0xff]),
        new Animation(SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose, 0xc180, [0x01, 0xfd, 0x69]),
        new Animation(SamusPoseId.NormalJumpAimDiagonalUpRightPose, 0xc190, [0x02, 0xff]),
        new Animation(SamusPoseId.NormalJumpAimDiagonalDownRightPose, 0xc1a0, [0x02, 0xff]),
        new Animation(SamusPoseId.NormalJumpAimDiagonalDownLeftPose, 0xc1a8, [0x02, 0xff]),
        new Animation(SamusPoseId.FallingAimDiagonalUpRightPose, 0xc1b0, [0x02, 0xf0, 0x10, 0xfe, 0x01]),
        new Animation(SamusPoseId.FallingAimDiagonalDownRightPose, 0xc1c0, [0x02, 0xff]),
        new Animation(SamusPoseId.LandingAimDiagonalUpRightPose, 0xc1d0, [0x01, 0xf8, 0x05]),
        new Animation(SamusPoseId.LandingAimDiagonalDownRightPose, 0xc1e0, [0x01, 0xf8, 0x07]),
        new Animation(SamusPoseId.NormalLandingRightPose, 0xc1f0, [0x02, 0xf8, 0x01]),
        new Animation(SamusPoseId.NormalLandingLeftPose, 0xc1f8, [0x02, 0xf8, 0x02]),
    ];
    foreach ((SamusPoseId pose, ushort stream, byte[] bytes) in animations)
        WritePoseAnimationDelays(bus, pose, stream, bytes);

    SyntheticSamusCartridge.WriteDryAirJumpSpeed(bus, whole: 0x0004, subspeed: 0xe000);
    SyntheticSamusCartridge.WriteDryAirGravity(bus, whole: 0x0000, subspeed: 0x1c00);
    SyntheticSamusCartridge.WriteNormalAirSpeedEntry(bus, SamusMovementType.NormalJumping, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);
    SyntheticSamusCartridge.WriteNormalAirSpeedEntry(bus, SamusMovementType.Falling, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    const int width = 8;
    const int height = 20;
    var blocks = new RoomTerrainBlock[width * height];
    for (int x = 0; x < width; x++)
        blocks[12 * width + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData level = CreateRoom(new(width, height),
        blocks,
        new RoomBlockBehavior[blocks.Length]);

    var samus = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.StandingAimDiagonalUpRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 171)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    SamusPresentation.InitializeAnimation(samus);
    SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(samus, 
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose);
    AssertEqual(21, samus.Kinematics.Radius.Y, "aimed jump commit retains standing radius");
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    AssertEqual(19, samus.Kinematics.Radius.Y, "next alpha publishes aimed jump radius");
    ushort launchY = samus.Position.Y.Whole;
    AerialMovementResult transitionFrame = SamusAerialMovement.StepNormalJump(
level, samus, ControllerButton.A, frameCounter: 0);
    AssertEqual(launchY, samus.Position.Y.Whole, "aimed transition frame does not move vertically");

    SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus), "aimed transition FD applies");
    AssertEqual(SamusPoseId.NormalJumpAimDiagonalUpRightPose, samus.Pose, "aimed transition FD target");

    int speedBeforeAimChange = samus.Kinematics.YSpeed.Whole;
    SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, SamusPoseId.NormalJumpAimDiagonalDownRightPose);
    AssertEqual(speedBeforeAimChange, samus.Kinematics.YSpeed.Whole, "air aim change preserves Y speed");

    // `$17` is not merely alternate art: Down shrinks the live body from radius 19 to
    // radius 10. Shrinking never probes blocks and does not move the center. Releasing to
    // another aimed-jump pose expands through `$91:FDAE`; these few upward frames put the
    // body in open air so neither the upper nor lower probe needs to displace the center.
    for (ushort index = 0; index < 4; index++)
        SamusAerialMovement.StepNormalJump(level, samus, ControllerButton.A, index);
    ushort compactCenterY = samus.Position.Y.Whole;
    uint compactVelocity = samus.Kinematics.VerticalSpeedFixed;
    SamusVerticalDirection compactDirection = samus.Kinematics.YDirection;
    AssertThrows<InvalidOperationException>(
        () => SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, SamusPoseId.NormalJumpAimDownRightPose),
        "compact aim requires room-aware collision route");
    AssertTrue(
        SamusAerialPoseTransitions.TryApplyCompactAerialTransition(samus, 
level, SamusPoseId.NormalJumpAimDownRightPose, frameCounter: 4),
        "jump enters compact down-right pose");
    AssertEqual(SamusPoseId.NormalJumpAimDownRightPose, samus.Pose, "compact jump pose");
    AssertEqual(19, samus.Kinematics.Radius.Y, "compact jump commit retains old live radius");
    SamusPoseMetadata.RefreshCollisionRadii(samus); // Next alpha publishes compact geometry.
    AssertEqual(10, samus.Kinematics.Radius.Y, "compact jump radius");
    AssertEqual(compactCenterY, samus.Position.Y.Whole, "shrinking compact jump preserves center");
    AssertEqual(compactVelocity, samus.Kinematics.VerticalSpeedFixed, "compact entry preserves 16.16 velocity");
    AssertEqual(compactDirection, samus.Kinematics.YDirection, "compact entry preserves vertical direction");
    AssertTrue(
        SamusAerialPoseTransitions.TryApplyCompactAerialTransition(samus, 
level, SamusPoseId.NormalJumpAimDiagonalDownRightPose, frameCounter: 5),
        "jump exits compact down-right pose");
    AssertEqual(10, samus.Kinematics.Radius.Y, "compact expansion commit retains old live radius");
    SamusPoseMetadata.RefreshCollisionRadii(samus); // Next alpha publishes expanded geometry.
    AssertEqual(19, samus.Kinematics.Radius.Y, "compact jump exit radius");
    AssertEqual(compactCenterY, samus.Position.Y.Whole, "open-air compact expansion preserves center");
    AssertEqual(compactVelocity, samus.Kinematics.VerticalSpeedFixed, "compact exit preserves 16.16 velocity");

    AerialMovementResult frame = default;
    for (int index = 0; index < 160; index++)
    {
        frame = SamusAerialMovement.StepNormalJump(
level, samus, ControllerButton.A, unchecked((ushort)index));
        if (frame.Landed)
            break;
    }
    AssertTrue(frame.Landed, "aimed normal jump lands");
    SamusPoseTransitions.ApplyAerialLanding(samus, wasSpinning: false);
    AssertEqual(SamusPoseId.LandingAimDiagonalDownRightPose, samus.Pose, "shot direction three selects E4 landing");
    AssertEqual(21, samus.Kinematics.Radius.Y, "aimed landing expands radius");
    AssertEqual(171, samus.Position.Y.Whole, "aimed landing retains floor-aligned feet");
    // Pose $E4's compiled landing stream is 05,02,F8,07. Seven native
    // animation ticks reach the return-to-aim command.
    for (int tick = 0; tick < 7; tick++)
        SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus), "aimed landing F8 applies");
    AssertEqual(SamusPoseId.StandingAimDiagonalDownRightPose, samus.Pose, "E4 landing returns to down-right aim");

    // Falling `$2D` uses the movement-type-six dispatcher with the same radius-ten body.
    // Let collision align its bottom to the floor first, then apply shot-direction four's
    // `$A4` landing entry. The 11-pixel upward center correction keeps that exact boundary
    // fixed while collision command five clears all vertical and horizontal motion.
    var compactFall = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.FallingAimDownRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 170)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(compactFall);
    SamusPresentation.InitializeAnimation(compactFall);
    compactFall.Kinematics.YSpeed = new UnsignedSixteenSixteen(1, 0x4000);
    compactFall.Kinematics.YDirection = SamusVerticalDirection.Down;
    SamusAerialMovement.ConfigureDryAirGravity(bus, compactFall);
    compactFall.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(1, 0x8000);
    AerialMovementResult compactFallFrame = default;
    for (ushort index = 0; index < 40 && !compactFallFrame.Landed; index++)
        compactFallFrame = SamusAerialMovement.StepFalling(level, compactFall, 0, index);
    AssertTrue(compactFallFrame.Landed, "compact falling pose reaches floor");
    ushort compactBottom = unchecked((ushort)(compactFall.Position.Y.Whole + compactFall.Kinematics.Radius.Y));
    AssertTrue(
        SamusPoseTransitions.TryApplyCompactAerialLanding(compactFall, level, frameCounter: 0),
        "compact down-right landing expands successfully");
    AssertEqual(SamusPoseId.NormalLandingRightPose, compactFall.Pose, "shot direction four selects A4 landing");
    // $91:FDAE leaves $0B00 at the compact body until alpha's SetSamusRadius, so enemy and
    // projectile collision later in the landing frame still use radius ten.
    AssertEqual(10, compactFall.Kinematics.Radius.Y, "compact landing keeps the compact radius this frame");
    AssertEqual(
        compactBottom,
        unchecked((ushort)(compactFall.Position.Y.Whole + SamusPoseMetadata.ReadPoseYRadius(compactFall.Pose))),
        "compact landing preserves floor boundary");
    SamusPoseMetadata.RefreshCollisionRadii(compactFall);
    AssertEqual(21, compactFall.Kinematics.Radius.Y, "the next alpha publishes the landing radius");
    AssertEqual(0u, compactFall.Kinematics.VerticalSpeedFixed, "compact landing clears vertical speed");
    AssertEqual(SamusVerticalDirection.None, compactFall.Kinematics.YDirection, "compact landing clears vertical direction");
    AssertEqual(0u, compactFall.HorizontalSpeed.BaseFixed, "compact landing clears horizontal speed");

    // Issue #27's slot-zero capture lands radius-ten `$2D` exactly on the boundary between
    // the solid and air quadrants of two square half-blocks, then expands to radius 21.
    // A final-boundary-only eleven-pixel probe sees the air quadrant and grows downward
    // through the platform. `$94:96AB` first probes eight pixels, detects the solid lower
    // quadrant, and makes `$91:FF49` preserve the already-landed bottom boundary.
    var halfPlatformWords = new RoomTerrainBlock[width * height];
    var halfPlatformBts = new RoomBlockBehavior[halfPlatformWords.Length];
    halfPlatformWords[12 * width + 2] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Slope);
    halfPlatformWords[12 * width + 3] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.Slope);
    halfPlatformBts[12 * width + 2] = new RoomBlockBehavior(0x02);
    halfPlatformBts[12 * width + 3] = new RoomBlockBehavior(0x00);
    RoomLevelData halfPlatform = CreateRoom(new(width, height),
        halfPlatformWords,
        halfPlatformBts);
    var capturedCompactLanding = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.FallingAimDownRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(0x002e, 0x00be)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(capturedCompactLanding);
    SamusPresentation.InitializeAnimation(capturedCompactLanding);
    AssertEqual(0x00c8, capturedCompactLanding.Kinematics.BottomBoundary,
        "captured compact body begins on half-platform surface");
    AssertTrue(
        SamusPoseTransitions.TryApplyCompactAerialLanding(capturedCompactLanding, 
halfPlatform, frameCounter: 1),
        "captured half-platform compact landing expands");
    AssertEqual(0x00b3, capturedCompactLanding.Position.Y.Whole,
        "captured half-platform landing shifts center up eleven pixels");
    AssertEqual(0x00c8, unchecked((ushort)(capturedCompactLanding.Position.Y.Whole +
            SamusPoseMetadata.ReadPoseYRadius(capturedCompactLanding.Pose))),
        "captured half-platform landing retains feet on surface");

    // The mirrored definitions carry direction four and shot direction five. Exercise the
    // same family guard in open air so a future right-only shortcut cannot silently pass.
    var compactLeft = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.NormalJumpAimDownLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 100)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(compactLeft);
    SamusPresentation.InitializeAnimation(compactLeft);
    AssertTrue(
        SamusAerialPoseTransitions.TryApplyCompactAerialTransition(compactLeft, 
level, SamusPoseId.NormalJumpAimDiagonalDownLeftPose, frameCounter: 1),
        "mirrored compact jump expands");
    AssertEqual(SamusPoseId.NormalJumpAimDiagonalDownLeftPose, compactLeft.Pose, "mirrored compact jump target");
    AssertEqual(10, compactLeft.Kinematics.Radius.Y, "mirrored expansion retains old radius until alpha");
    SamusPoseMetadata.RefreshCollisionRadii(compactLeft);
    AssertEqual(19, compactLeft.Kinematics.Radius.Y, "mirrored compact jump radius");

    var compactLeftLanding = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.FallingAimDownLeftPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 170)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(compactLeftLanding);
    SamusPresentation.InitializeAnimation(compactLeftLanding);
    compactLeftLanding.Kinematics.YSpeed = new UnsignedSixteenSixteen(1, 0x4000);
    compactLeftLanding.Kinematics.YDirection = SamusVerticalDirection.Down;
    SamusAerialMovement.ConfigureDryAirGravity(bus, compactLeftLanding);
    AerialMovementResult compactLeftFallFrame = default;
    for (ushort index = 0; index < 40 && !compactLeftFallFrame.Landed; index++)
    {
        compactLeftFallFrame = SamusAerialMovement.StepFalling(
level, compactLeftLanding, 0, index);
    }
    AssertTrue(compactLeftFallFrame.Landed, "mirrored compact falling pose reaches floor");
    ushort compactLeftBottom = unchecked((ushort)(
        compactLeftLanding.Position.Y.Whole + compactLeftLanding.Kinematics.Radius.Y));
    AssertTrue(
        SamusPoseTransitions.TryApplyCompactAerialLanding(compactLeftLanding, level, frameCounter: 1),
        "compact down-left landing expands successfully");
    AssertEqual(SamusPoseId.NormalLandingLeftPose, compactLeftLanding.Pose, "shot direction five selects A5 landing");
    AssertEqual(
        compactLeftBottom,
        unchecked((ushort)(compactLeftLanding.Position.Y.Whole + SamusPoseMetadata.ReadPoseYRadius(compactLeftLanding.Pose))),
        "mirrored compact landing preserves floor boundary");

    // Put radius-ten `$17` between a row-nine ceiling and row-twelve floor. The body itself
    // fits, but both independent nine-pixel probes collide. `$91:FFA7` therefore selects
    // ordinary `$27` and `$91:FFD4-$91:FFE9` moves the center up by 10-16 = -6 pixels.
    var boxedBlocks = (RoomTerrainBlock[])blocks.Clone();
    for (int x = 0; x < width; x++)
        boxedBlocks[9 * width + x] = RoomTerrainBlock.Of(0, BlockMirroring.None, RoomCollisionType.SolidBlock);
    RoomLevelData boxedLevel = CreateRoom(new(width, height),
        boxedBlocks,
        new RoomBlockBehavior[boxedBlocks.Length]);
    var boxedCompact = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.NormalJumpAimDownRightPose,
        Position = FixedPosition<RoomPixels>.FromWhole(new(48, 176)),
    };
    SamusPoseMetadata.RefreshCollisionRadii(boxedCompact);
    SamusPresentation.InitializeAnimation(boxedCompact);
    AssertTrue(
        !SamusAerialPoseTransitions.TryApplyCompactAerialTransition(boxedCompact, 
boxedLevel, SamusPoseId.NormalJumpAimDiagonalDownRightPose, frameCounter: 0),
        "boxed compact expansion is rejected");
    AssertEqual(SamusPoseId.CrouchingRightPose, boxedCompact.Pose, "boxed compact expansion selects crouch");
    AssertEqual(10, boxedCompact.Kinematics.Radius.Y, "boxed compact fallback retains live radius until alpha");
    AssertEqual(170, boxedCompact.Position.Y.Whole, "boxed compact fallback applies FFD4 offset");
    SamusPoseMetadata.RefreshCollisionRadii(boxedCompact);
    AssertEqual(16, boxedCompact.Kinematics.Radius.Y, "next alpha publishes compact fallback crouch radius");
    AssertEqual(170, boxedCompact.Position.Y.Whole, "radius publication does not repeat center correction");

    // PSP_Falling chooses ordinary falling from the physics-facing byte. Aim is
    // reconsidered on the next input pass, rather than being preserved by the walk-off.
    samus.Pose = SamusPoseId.StandingAimDiagonalUpRightPose;
    SamusPoseMetadata.RefreshCollisionRadii(samus);
    SamusPresentation.InitializeAnimation(samus);
    AssertEqual(SamusPoseId.FallingRightPose, SamusPoseTransitions.SelectFallingPoseForCurrentAim(samus), "up-right walk-off target");
    SamusPoseTransitions.ApplyWalkedOffFloorTransition(samus, level, SamusPoseId.FallingRightPose);
    AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.YDirection, "aimed walk-off starts downward");
    SamusPoseMetadata.RefreshCollisionRadii(samus); // Next alpha precedes the airborne input/aim transition.
    SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, SamusPoseId.FallingAimDiagonalUpRightPose);

    // `$90:8324-$8345` leaves both the command index and zero timer untouched when `$F0`
    // is reached. One frame later DEC wraps zero to `$FFFF`; the negative result advances
    // over `$F0` and loads the literal `$10` delay. This seemingly odd two-frame sequence
    // is why treating an unknown command as a normal delay—or simply skipping it—drifts.
    SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(1, samus.AnimationFrameTimer.RawValue, "aimed-fall initial delay counts down");
    SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(1, samus.AnimationFrame, "aimed-fall reaches F0 command index");
    AssertEqual(0, samus.AnimationFrameTimer.RawValue, "F0 leaves expired timer at zero");
    SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom() });
    AssertEqual(2, samus.AnimationFrame, "timer underflow advances beyond F0");
    AssertEqual(16, samus.AnimationFrameTimer.RawValue, "post-F0 frame loads literal delay");

    SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, SamusPoseId.FallingAimDiagonalDownRightPose);
    AssertEqual(0x29, (byte)SamusPoseMetadata.ReadNoInputFallbackPose(samus), "aimed fall fallback target");
    SamusAerialPoseTransitions.ApplyAerialAimTransition(samus, SamusPoseId.FallingRightPose);
    AssertEqual(SamusPoseId.FallingRightPose, samus.Pose, "aimed fall applies unaimed fallback");

    Console.WriteLine("  Samus aimed air: FD jump, live aim, F0 cadence, compact hitboxes/landing, walk-off, and fall fallback agree.");
}

/// <summary>
/// Locks the complete ordinary-play horizontal-fire body family to literal bank-$91 pose
/// records and transition semantics. These checks intentionally separate body-pose work
/// from projectile ownership: a fired beam is a bank-$90 system, while `$0B/$13/$67` and
/// their mirrors are bank-$91 movement/animation states selected by the same controller word.
/// </summary>
internal static void VerifySamusGunExtendedMovement()
{
    var bus = new TestAddressSpace();

    // PoseDefinitions begins at `$91:B629` and each entry is exactly eight bytes:
    // X direction, movement type, no-input fallback, shot direction, collision command,
    // unused byte, Y radius, unused byte. These are direct retail bytes, not host fixtures
    // inferred from nearby poses.
    SamusSpecialStatesPose[] definitions = [
        new SamusSpecialStatesPose(SamusPoseId.FacingRightNormalPose, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.FacingLeftNormalPose, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.MovingRightNormalPose, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.MovingLeftNormalPose, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.MovingRightGunExtendedPose, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.MovingLeftGunExtendedPose, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NormalJumpGunExtendedRightPose, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NormalJumpGunExtendedLeftPose, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NeutralJumpTransitionRightPose, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NeutralJumpTransitionLeftPose, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NeutralJumpRightPose, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NeutralJumpLeftPose, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NormalJumpForwardRightPose, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NormalJumpForwardLeftPose, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.FallingRightPose, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.FallingLeftPose, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.FallingGunExtendedRightPose, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.FallingGunExtendedLeftPose, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NormalLandingRightPose, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.NormalLandingLeftPose, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.FiringLandingRightPose, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]),
        new SamusSpecialStatesPose(SamusPoseId.FiringLandingLeftPose, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]),
    ];
    foreach ((SamusPoseId pose, byte[] definition) in definitions)
        WritePoseDefinition(bus, (int)pose, definition);

    // Synthetic storage keeps the tests compact, but each stream's bytes reproduce the
    // relevant retail control flow: running loops ten frames; jump/fall loop; firing
    // landings reach literal command `$F8,$01/$02`.
    Animation[] animations = [
        new Animation(SamusPoseId.FacingRightNormalPose, 0xd000, [0x0a, 0xff]),
        new Animation(SamusPoseId.FacingLeftNormalPose, 0xd010, [0x0a, 0xff]),
        new Animation(SamusPoseId.MovingRightNormalPose, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        new Animation(SamusPoseId.MovingLeftNormalPose, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        new Animation(SamusPoseId.MovingRightGunExtendedPose, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        new Animation(SamusPoseId.MovingLeftGunExtendedPose, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        new Animation(SamusPoseId.NormalJumpGunExtendedRightPose, 0xd040, [0x02, 0x10, 0xfe, 0x01]),
        new Animation(SamusPoseId.NormalJumpGunExtendedLeftPose, 0xd040, [0x02, 0x10, 0xfe, 0x01]),
        new Animation(SamusPoseId.NeutralJumpTransitionRightPose, 0xd048, [0x02, 0xfd, 0x4d]),
        new Animation(SamusPoseId.NeutralJumpTransitionLeftPose, 0xd04c, [0x02, 0xfd, 0x4e]),
        new Animation(SamusPoseId.NeutralJumpRightPose, 0xd050, [0x02, 0x03, 0xfe, 0x01]),
        new Animation(SamusPoseId.NeutralJumpLeftPose, 0xd050, [0x02, 0x03, 0xfe, 0x01]),
        new Animation(SamusPoseId.NormalJumpForwardRightPose, 0xd058, [0x02, 0x03, 0xfe, 0x01]),
        new Animation(SamusPoseId.NormalJumpForwardLeftPose, 0xd058, [0x02, 0x03, 0xfe, 0x01]),
        new Animation(SamusPoseId.FallingRightPose, 0xd060, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        new Animation(SamusPoseId.FallingLeftPose, 0xd060, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        new Animation(SamusPoseId.FallingGunExtendedRightPose, 0xd080, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        new Animation(SamusPoseId.FallingGunExtendedLeftPose, 0xd080, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        new Animation(SamusPoseId.NormalLandingRightPose, 0xd0a0, [0x05, 0x02, 0xf8, 0x01]),
        new Animation(SamusPoseId.NormalLandingLeftPose, 0xd0b0, [0x05, 0x02, 0xf8, 0x02]),
        new Animation(SamusPoseId.FiringLandingRightPose, 0xd0c0, [0x01, 0xf8, 0x01]),
        new Animation(SamusPoseId.FiringLandingLeftPose, 0xd0d0, [0x01, 0xf8, 0x02]),
    ];
    foreach ((SamusPoseId pose, ushort stream, byte[] bytes) in animations)
        WritePoseAnimationDelays(bus, pose, stream, bytes);

    // Minimal transition programs retain the literal held masks from `$91:A1F8`,
    // `$91:A2F6`, and `$91:A70C`. They prove that Shot+forward, Shot in neutral jump,
    // and Shot while falling select the three distinct extended-gun movement families.
    SyntheticSamusCartridge.WritePoseTransitionPointer(bus, SamusPoseId.MovingRightNormalPose, 0xd100);
    SyntheticSamusCartridge.WritePoseTransitionList(bus, 0xd100, [new SyntheticSamusCartridgeRecord(0x0000, 0x0140, 0x000b)]);
    SyntheticSamusCartridge.WritePoseTransitionPointer(bus, SamusPoseId.NeutralJumpRightPose, 0xd110);
    SyntheticSamusCartridge.WritePoseTransitionList(bus, 0xd110, [new SyntheticSamusCartridgeRecord(0x0000, 0x0040, 0x0013)]);
    SyntheticSamusCartridge.WritePoseTransitionPointer(bus, SamusPoseId.FallingRightPose, 0xd120);
    SyntheticSamusCartridge.WritePoseTransitionList(bus, 0xd120, [new SyntheticSamusCartridgeRecord(0x0000, 0x0040, 0x0067)]);

    // `$51/$52` share the complete `$91:A2F6/$A376` tables with their neutral-jump
    // mirrors. These minimal copies retain the exact held-Jump record that caused the
    // playable runtime failure: required-new is zero, held `$0080`, targets `$4D/$4E`.
    SyntheticSamusCartridge.WritePoseTransitionPointer(bus, SamusPoseId.NormalJumpForwardRightPose, 0xd130);
    SyntheticSamusCartridge.WritePoseTransitionList(bus, 0xd130, [new SyntheticSamusCartridgeRecord(0x0000, 0x0080, 0x004d)]);
    SyntheticSamusCartridge.WritePoseTransitionPointer(bus, SamusPoseId.NormalJumpForwardLeftPose, 0xd140);
    SyntheticSamusCartridge.WritePoseTransitionList(bus, 0xd140, [new SyntheticSamusCartridgeRecord(0x0000, 0x0080, 0x004e)]);

    // Preserve the decisive tail of retail pose `$0B`'s table at `$91:AE94`. Right+Shot
    // and Right both select `$0B` itself and therefore return directly; Shot without Right
    // reaches `$FFFF` and enters `$91:82D9`, whose pose-definition byte above is `$01`.
    // Collapsing those two null-transition paths is what made one tapped Right input remain
    // latched for as long as Shoot stayed down in the playable frontend.
    SyntheticSamusCartridge.WritePoseTransitionPointer(bus, SamusPoseId.MovingRightGunExtendedPose, 0xd150);
    SyntheticSamusCartridge.WritePoseTransitionList(bus, 0xd150, [
        new SyntheticSamusCartridgeRecord(0x0000, 0x0140, 0x000b),
        new SyntheticSamusCartridgeRecord(0x0000, 0x0100, 0x000b),
    ]);

    AssertEqual(0x0b,
        (byte)(SamusPoseTransitionTable.Find(SamusPoseId.MovingRightNormalPose, ControllerButton.Right | ControllerButton.X, 0) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find(bus, SamusPoseId.MovingRightNormalPose, ControllerButton.Right | ControllerButton.X, 0).")).ProspectivePose,
        "Shot+Right selects running gun extension");
    AssertEqual(0x13,
        (byte)(SamusPoseTransitionTable.Find(SamusPoseId.NeutralJumpRightPose, ControllerButton.X, 0) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find(bus, SamusPoseId.NeutralJumpRightPose, ControllerButton.X, 0).")).ProspectivePose,
        "Shot selects neutral-jump gun extension");
    AssertEqual(0x67,
        (byte)(SamusPoseTransitionTable.Find(SamusPoseId.FallingRightPose, ControllerButton.X, 0) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find(bus, SamusPoseId.FallingRightPose, ControllerButton.X, 0).")).ProspectivePose,
        "Shot selects falling gun extension");

    SamusPoseTransitionLookup heldRightAndShot = SamusPoseTransitionTable.Lookup(
        SamusPoseId.MovingRightGunExtendedPose,
        (ControllerButton.Right | ControllerButton.X),
        canonicalNewInput: 0);
    SamusPoseTransitionLookup releasedRightHoldingShot = SamusPoseTransitionTable.Lookup(
        SamusPoseId.MovingRightGunExtendedPose,
        ControllerButton.X,
        canonicalNewInput: 0);
    AssertTrue(!heldRightAndShot.UsesPoseDefinitionFallback,
        "running-gun Right+Shot same-pose record suppresses fallback");
    AssertTrue(releasedRightHoldingShot.UsesPoseDefinitionFallback,
        "running-gun Shot-only terminator requests fallback");

    SamusPoseTransition forwardRightRelease =
        (SamusPoseTransitionTable.Find(SamusPoseId.NormalJumpForwardRightPose, ControllerButton.A, 0) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find(bus, SamusPoseId.NormalJumpForwardRightPose, (ushort)Snes...."));
    SamusPoseTransition forwardLeftRelease =
        (SamusPoseTransitionTable.Find(SamusPoseId.NormalJumpForwardLeftPose, ControllerButton.A, 0) ?? throw new InvalidOperationException("Verification requires SamusPoseTransitionTable.Find(bus, SamusPoseId.NormalJumpForwardLeftPose, (ushort)SnesB...."));
    AssertEqual(0x4d, (byte)forwardRightRelease.ProspectivePose,
        "held Jump without Right selects neutral right jump");
    AssertEqual(0x4e, (byte)forwardLeftRelease.ProspectivePose,
        "held Jump without Left selects neutral left jump");

    // Apply both winners through the exact helper used by the runtime switch. Pose and
    // animation restart, while every live movement word survives the same-radius change.
    // Mirroring the assertion is important: `$51` and `$52` use adjacent pointer-table
    // entries, so a one-sided admission fix could make keyboard direction appear random.
    foreach ((SamusPoseId source, SamusPoseId target, string facing) in new[]
    {
        new VerifySamusGunExtendedMovementCase(SamusPoseId.NormalJumpForwardRightPose, forwardRightRelease.ProspectivePose, "right"),
        new VerifySamusGunExtendedMovementCase(SamusPoseId.NormalJumpForwardLeftPose, forwardLeftRelease.ProspectivePose, "left"),
    })
    {
        var releasedForwardJump = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = source };
        SamusPoseMetadata.RefreshCollisionRadii(releasedForwardJump);
        SamusPresentation.InitializeAnimation(releasedForwardJump, initialFrame: 1);
        releasedForwardJump.Kinematics.YSpeed = new UnsignedSixteenSixteen(3, 0x4567);
        releasedForwardJump.Kinematics.YDirection = SamusVerticalDirection.Up;
        releasedForwardJump.HorizontalSpeed.BaseSpeed = new UnsignedSixteenSixteen(2, 0x89ab);

        SamusAerialPoseTransitions.ApplyAerialAimTransition(releasedForwardJump, target);

        AssertEqual(target, releasedForwardJump.Pose,
            $"{facing} forward-jump release installs neutral pose");
        AssertEqual(0, releasedForwardJump.AnimationFrame,
            $"{facing} forward-jump release restarts target animation");
        AssertEqual(0x00034567u, releasedForwardJump.Kinematics.VerticalSpeedFixed,
            $"{facing} forward-jump release preserves vertical velocity");
        AssertEqual(0x000289abu, releasedForwardJump.HorizontalSpeed.BaseFixed,
            $"{facing} forward-jump release preserves horizontal velocity");
        AssertEqual(SamusVerticalDirection.Up, releasedForwardJump.Kinematics.YDirection,
            $"{facing} forward-jump release preserves vertical direction");
    }

    // `$91:F50C` preserves the animation phase across movement-type-one arm changes. Begin
    // on a nonzero index so resetting to frame zero cannot accidentally satisfy the check.
    var running = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.MovingRightNormalPose };
    SamusPoseMetadata.RefreshCollisionRadii(running);
    SamusPresentation.InitializeAnimation(running, initialFrame: 4);
    int runningFrame = running.AnimationFrame;
    ushort runningTimer = running.AnimationFrameTimer.RawValue;
    SamusAnimationProgram runningProgram = SamusAnimationDelays.ProgramForPose(running.AnimatedPose ?? throw new InvalidOperationException("The run has no animation."));
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(running, SamusPoseId.MovingRightGunExtendedPose);
    AssertEqual(SamusPoseId.MovingRightGunExtendedPose, running.Pose, "running gun extension installs pose $0B");
    AssertEqual(runningFrame, running.AnimationFrame, "running gun extension preserves frame");
    AssertEqual(runningTimer, running.AnimationFrameTimer.RawValue, "running gun extension preserves timer");
    AssertTrue(ReferenceEquals(runningProgram, SamusAnimationDelays.ProgramForPose(running.AnimatedPose ?? throw new InvalidOperationException("The run has no animation."))), "running gun extension retains the shared animation program");
    SamusGroundedPoseTransitions.ApplyGroundedAimTransition(running, SamusPoseMetadata.ReadNoInputFallbackPose(running));
    AssertEqual(SamusPoseId.FacingRightNormalPose, running.Pose,
        "releasing Right while holding Shot leaves running-gun pose");

    // Same-radius airborne arm changes must not perturb the live 16.16 velocity or direction.
    var jumping = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.NeutralJumpRightPose };
    SamusPoseMetadata.RefreshCollisionRadii(jumping);
    SamusPresentation.InitializeAnimation(jumping);
    jumping.Kinematics.YSpeed = new UnsignedSixteenSixteen(3, 0x4567);
    jumping.Kinematics.YDirection = SamusVerticalDirection.Up;
    SamusAerialPoseTransitions.ApplyAerialAimTransition(jumping, SamusPoseId.NormalJumpGunExtendedRightPose);
    AssertEqual(SamusPoseId.NormalJumpGunExtendedRightPose, jumping.Pose, "neutral jump installs gun extension $13");
    AssertEqual(0x00034567u, jumping.Kinematics.VerticalSpeedFixed, "jump gun extension preserves velocity");
    AssertEqual(SamusVerticalDirection.Up, jumping.Kinematics.YDirection, "jump gun extension preserves direction");

    var falling = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FallingRightPose, Position = FixedPosition<RoomPixels>.FromWhole(new(0, 100)) };
    SamusPoseMetadata.RefreshCollisionRadii(falling);
    SamusPresentation.InitializeAnimation(falling);
    falling.Kinematics.YSpeed = new UnsignedSixteenSixteen(2, 0xabcd);
    falling.Kinematics.YDirection = SamusVerticalDirection.Down;
    falling.HorizontalSpeed.BaseSpeed = falling.HorizontalSpeed.BaseSpeed.WithWhole(1);
    SamusAerialPoseTransitions.ApplyAerialAimTransition(falling, SamusPoseId.FallingGunExtendedRightPose);
    AssertEqual(SamusPoseId.FallingGunExtendedRightPose, falling.Pose, "fall installs gun extension $67");
    AssertEqual(0x0002abcdu, falling.Kinematics.VerticalSpeedFixed, "fall gun extension preserves velocity");

    // `$91:E99B` samples HELD Shot, not newly pressed Shot. Holding X at horizontal impact
    // selects `$E6`; releasing it selects `$A4` from the same source metadata.
    SamusPoseTransitions.ApplyAerialLanding(falling, wasSpinning: false, ControllerButton.X);
    AssertEqual(SamusPoseId.FiringLandingRightPose, falling.Pose, "held Shot selects firing landing $E6");
    AssertEqual(21, falling.Kinematics.Radius.Y, "firing landing expands to standing radius");
    AssertEqual(0u, falling.Kinematics.VerticalSpeedFixed, "firing landing clears Y speed");
    AssertEqual(0u, falling.HorizontalSpeed.BaseFixed, "firing landing clears X speed");
    // $E6 shares the native 05,02,F8,01 landing program: its F8 command
    // follows seven animation ticks, independent of held-Shot selection.
    for (int tick = 0; tick < 7; tick++)
        SamusPresentation.AnimateNoFx(falling, new SamusAnimationStep { Random = new SharedRandom() });
    AssertTrue(SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(falling), "$E6 F8 transition applies");
    AssertEqual(SamusPoseId.FacingRightNormalPose, falling.Pose, "$E6 returns to standing right");

    var releasedLanding = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FallingGunExtendedRightPose };
    SamusPoseMetadata.RefreshCollisionRadii(releasedLanding);
    SamusPresentation.InitializeAnimation(releasedLanding);
    SamusPoseTransitions.ApplyAerialLanding(releasedLanding, wasSpinning: false, controllerInput: 0);
    AssertEqual(SamusPoseId.NormalLandingRightPose, releasedLanding.Pose, "released Shot selects ordinary landing $A4");

    var mirroredLanding = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FallingGunExtendedLeftPose };
    SamusPoseMetadata.RefreshCollisionRadii(mirroredLanding);
    SamusPresentation.InitializeAnimation(mirroredLanding);
    SamusPoseTransitions.ApplyAerialLanding(mirroredLanding, wasSpinning: false, ControllerButton.X);
    AssertEqual(SamusPoseId.FiringLandingLeftPose, mirroredLanding.Pose, "held Shot selects mirrored firing landing $E7");

    AssertTrue(SamusPoseMetadata.IsRightFacingRunningPose(SamusPoseId.MovingRightGunExtendedPose), "$0B is admitted by running dispatcher");
    AssertTrue(SamusPoseMetadata.IsRightFacingNormalJumpPose(SamusPoseId.NormalJumpGunExtendedRightPose), "$13 is admitted by jump dispatcher");
    AssertTrue(SamusPoseMetadata.IsRightFacingFallingPose(SamusPoseId.FallingGunExtendedRightPose), "$67 is admitted by falling dispatcher");
    AssertTrue(SamusPoseMetadata.IsRightFacingLandingPose(SamusPoseId.FiringLandingRightPose), "$E6 is admitted by landing dispatcher");
    AssertTrue(SamusPoseMetadata.IsLeftFacingLandingPose(SamusPoseId.FiringLandingLeftPose), "$E7 is admitted by mirrored landing dispatcher");

    // All right/left landing pairs use the ordinary standing input table. Exercise every
    // same-facing neutral/aimed jump target so adding landing art can never again leave a
    // syntactically valid ROM transition outside the runtime switch. The private-ROM fight
    // exposed `$E2 -> $57`; it is one member of this complete table product, not a bespoke
    // exception for Bomb Torizo.
    SamusPoseId[] rightLandingFamily = [SamusPoseId.NormalLandingRightPose, SamusPoseId.SpinLandingRightPose, SamusPoseId.LandingAimUpRightPose, SamusPoseId.LandingAimDiagonalUpRightPose, SamusPoseId.LandingAimDiagonalDownRightPose, SamusPoseId.FiringLandingRightPose];
    SamusPoseId[] leftLandingFamily = [SamusPoseId.NormalLandingLeftPose, SamusPoseId.SpinLandingLeftPose, SamusPoseId.LandingAimUpLeftPose, SamusPoseId.LandingAimDiagonalUpLeftPose, SamusPoseId.LandingAimDiagonalDownLeftPose, SamusPoseId.FiringLandingLeftPose];
    SamusPoseId[] rightLandingJumpTargets =
    [
        SamusPoseId.NeutralJumpTransitionRightPose,
        SamusPoseId.NormalJumpTransitionAimUpRightPose,
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose,
        SamusPoseId.NormalJumpTransitionAimDiagonalDownRightPose,
    ];
    SamusPoseId[] leftLandingJumpTargets =
    [
        SamusPoseId.NeutralJumpTransitionLeftPose,
        SamusPoseId.NormalJumpTransitionAimUpLeftPose,
        SamusPoseId.NormalJumpTransitionAimDiagonalUpLeftPose,
        SamusPoseId.NormalJumpTransitionAimDiagonalDownLeftPose,
    ];
    foreach (SamusPoseId sourcePose in rightLandingFamily)
    {
        foreach (SamusPoseId targetPose in rightLandingJumpTargets)
        {
            AssertTrue(
                SamusPoseMetadata.IsLandingToNormalJumpTransition(sourcePose, targetPose),
                $"right landing ${(int)sourcePose:X2} admits jump ${(int)targetPose:X2}");
        }
        foreach (SamusPoseId targetPose in leftLandingJumpTargets)
        {
            AssertTrue(
                !SamusPoseMetadata.IsLandingToNormalJumpTransition(sourcePose, targetPose),
                $"right landing ${(int)sourcePose:X2} rejects mirrored jump ${(int)targetPose:X2}");
        }
    }
    foreach (SamusPoseId sourcePose in leftLandingFamily)
    {
        foreach (SamusPoseId targetPose in leftLandingJumpTargets)
        {
            AssertTrue(
                SamusPoseMetadata.IsLandingToNormalJumpTransition(sourcePose, targetPose),
                $"left landing ${(int)sourcePose:X2} admits jump ${(int)targetPose:X2}");
        }
        foreach (SamusPoseId targetPose in rightLandingJumpTargets)
        {
            AssertTrue(
                !SamusPoseMetadata.IsLandingToNormalJumpTransition(sourcePose, targetPose),
                $"left landing ${(int)sourcePose:X2} rejects mirrored jump ${(int)targetPose:X2}");
        }
    }

    // The private-ROM controller audit first exposed `$E6 -> $4B`: Shoot remained held
    // through landing, then a fresh Jump edge arrived before `$F8`. Run both mirrors through
    // the real initializer so this verifies radius, animation, and vertical launch state in
    // addition to the family predicate used by the runtime switch.
    var firingLandingJumpRight = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FiringLandingRightPose };
    SamusPoseMetadata.RefreshCollisionRadii(firingLandingJumpRight);
    SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(firingLandingJumpRight, 
        SamusPoseId.NeutralJumpTransitionRightPose);
    AssertEqual(SamusPoseId.NeutralJumpTransitionRightPose, firingLandingJumpRight.Pose,
        "right firing landing installs neutral jump $4B");
    AssertEqual(21, firingLandingJumpRight.Kinematics.Radius.Y,
        "right firing-landing jump retains landing radius");
    SamusPoseMetadata.RefreshCollisionRadii(firingLandingJumpRight);
    AssertEqual(19, firingLandingJumpRight.Kinematics.Radius.Y,
        "right firing-landing next alpha installs normal-jump radius");
    AssertEqual(SamusVerticalDirection.Up, firingLandingJumpRight.Kinematics.YDirection,
        "right firing-landing jump launches upward");

    var firingLandingJumpLeft = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FiringLandingLeftPose };
    SamusPoseMetadata.RefreshCollisionRadii(firingLandingJumpLeft);
    SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(firingLandingJumpLeft, 
        SamusPoseId.NeutralJumpTransitionLeftPose);
    AssertEqual(SamusPoseId.NeutralJumpTransitionLeftPose, firingLandingJumpLeft.Pose,
        "left firing landing installs neutral jump $4C");
    AssertEqual(21, firingLandingJumpLeft.Kinematics.Radius.Y,
        "left firing-landing jump retains landing radius");
    SamusPoseMetadata.RefreshCollisionRadii(firingLandingJumpLeft);
    AssertEqual(19, firingLandingJumpLeft.Kinematics.Radius.Y,
        "left firing-landing next alpha installs normal-jump radius");
    AssertEqual(SamusVerticalDirection.Up, firingLandingJumpLeft.Kinematics.YDirection,
        "left firing-landing jump launches upward");

    var aimedLandingJumpRight = new SamusState(InstalledFixtures.AnimationContinuation)
    {
        Pose = SamusPoseId.LandingAimDiagonalUpRightPose,
    };
    SamusAerialPoseTransitions.ApplyOrdinaryJumpTransition(aimedLandingJumpRight, 
        SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose);
    AssertEqual(SamusPoseId.NormalJumpTransitionAimDiagonalUpRightPose,
        aimedLandingJumpRight.Pose,
        "right diagonal-up landing installs aimed jump $57");
    AssertEqual(SamusVerticalDirection.Up, aimedLandingJumpRight.Kinematics.YDirection,
        "right diagonal-up landing jump launches upward");

    Console.WriteLine("  Samus horizontal fire: ROM selection, six extended bodies, forward-jump release, run-phase preservation, and firing landings agree.");
}

    /// <summary>Runs the aimed jump, fall, landing and walk-off pose checks through the suite harness.</summary>
    [VerificationCommand("--samus-aimed-aerial")]
    internal static int SamusAimedAerialCommand()
    {
        Suite(nameof(VerifySamusAimedAerialMovement), () => VerifySamusAimedAerialMovement());
        return 0;
    }

    /// <summary>Runs the horizontal-fire body and held-Shot landing checks through the suite harness.</summary>
    [VerificationCommand("--samus-gun-extended")]
    internal static int SamusGunExtendedCommand()
    {
        Suite(nameof(VerifySamusGunExtendedMovement), () => VerifySamusGunExtendedMovement());
        return 0;
    }
}
