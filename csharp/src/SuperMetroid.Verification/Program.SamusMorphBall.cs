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

/// <summary>Morph Ball, bomb, reaction-PLM, and tunnel verification.</summary>
static void VerifySamusMorphBallMovement()
{
    var bus = new TestAddressSpace();

    // Pose definitions `$91:B711-$91:B819/$91:B831`. Only fields consumed by the current
    // slice are populated, but the complete eight-byte records make direction, movement,
    // fallback, graphics offset, and collision radius independently observable.
    void WritePose(byte pose, byte[] definition) =>
        WritePoseDefinition(bus, pose, definition);
    WritePose(SamusState.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusState.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusState.MorphBallGroundRightPose, [0x08, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphBallMovingRightPose, [0x08, 0x04, 0x1d, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphBallMovingLeftPose, [0x04, 0x04, 0x41, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphBallFallingRightPose, [0x08, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphBallFallingLeftPose, [0x04, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphingTransitionRightPose, [0x08, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphingTransitionLeftPose, [0x04, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.UnmorphingTransitionRightPose, [0x08, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusState.UnmorphingTransitionLeftPose, [0x04, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusState.MorphBallGroundLeftPose, [0x04, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallGroundRightPose, [0x08, 0x11, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallGroundLeftPose, [0x04, 0x11, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallMovingRightPose, [0x08, 0x11, 0x79, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallMovingLeftPose, [0x04, 0x11, 0x7a, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallFallingRightPose, [0x08, 0x13, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallFallingLeftPose, [0x04, 0x13, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallJumpRightPose, [0x08, 0x12, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallJumpLeftPose, [0x04, 0x12, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);

    // Stable ordinary-ball poses all point to `$91:B378`. Separate synthetic storage keeps
    // the production pointer lookup real while making the expected command stream concise.
    const ushort sharedBallDelay = 0xc400;
    foreach (byte pose in new byte[] {
        SamusState.MorphBallGroundRightPose,
        SamusState.MorphBallMovingRightPose,
        SamusState.MorphBallMovingLeftPose,
        SamusState.MorphBallFallingRightPose,
        SamusState.MorphBallFallingLeftPose,
        SamusState.MorphBallGroundLeftPose,
        SamusState.SpringBallGroundRightPose,
        SamusState.SpringBallGroundLeftPose,
        SamusState.SpringBallMovingRightPose,
        SamusState.SpringBallMovingLeftPose,
        SamusState.SpringBallFallingRightPose,
        SamusState.SpringBallFallingLeftPose,
        SamusState.SpringBallJumpRightPose,
        SamusState.SpringBallJumpLeftPose,
    })
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, sharedBallDelay);
    }
    bus.WriteBytes(0x910000 | sharedBallDelay, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]);

    // `$37/$38`: two visible frames, then `$F9 $0002 ground air springGround springAir`.
    WriteTestWord(bus, 0x91b010 + SamusState.MorphingTransitionRightPose * 2, 0xc420);
    WriteTestWord(bus, 0x91b010 + SamusState.MorphingTransitionLeftPose * 2, 0xc430);
    bus.WriteBytes(0x91c420, [0x02, 0x02, 0xf9, 0x02, 0x00, 0x1d, 0x31, 0x79, 0x7d]);
    bus.WriteBytes(0x91c430, [0x02, 0x02, 0xf9, 0x02, 0x00, 0x41, 0x32, 0x7a, 0x7e]);

    // `$3D/$3E` finish through the already translated `$FD pp` command-three seam.
    WriteTestWord(bus, 0x91b010 + SamusState.UnmorphingTransitionRightPose * 2, 0xc440);
    WriteTestWord(bus, 0x91b010 + SamusState.UnmorphingTransitionLeftPose * 2, 0xc450);
    WriteTestWord(bus, 0x91b010 + SamusState.CrouchingRightPose * 2, 0xc460);
    WriteTestWord(bus, 0x91b010 + SamusState.CrouchingLeftPose * 2, 0xc470);
    bus.WriteBytes(0x91c440, [0x02, 0x02, 0xfd, 0x27]);
    bus.WriteBytes(0x91c450, [0x02, 0x02, 0xfd, 0x28]);
    bus.WriteBytes(0x91c460, [0x10, 0xff]);
    bus.WriteBytes(0x91c470, [0x10, 0xff]);

    // Normal-air movement type four receives visible acceleration and deceleration words;
    // type eight remains zero horizontally for isolated bounce assertions.
    bus.WriteBytes(0x909f85, [
        0x00, 0x00, 0x00, 0x80, // acceleration 0.8000
        0x01, 0x00, 0x00, 0x00, // maximum 1.0000
        0x00, 0x00, 0x00, 0x40, // deceleration 0.4000
    ]);
    bus.WriteBytes(0x909f55 + 0x11 * 12, [
        0x00, 0x00, 0x00, 0x80, // Spring-ground acceleration 0.8000
        0x01, 0x00, 0x00, 0x00, // maximum 1.0000
        0x00, 0x00, 0x00, 0x40, // deceleration 0.4000
    ]);
    bus.WriteBytes(0x909eb9, [0x04, 0x00]); // Dry-air jump whole speed 4.
    bus.WriteBytes(0x909ebf, [0x00, 0xe0]); // Dry-air jump subspeed E000.
    bus.WriteBytes(0x909ea1, [0x00, 0x28]); // Dry-air gravity subspeed 2800.
    bus.WriteBytes(0x909ea7, [0x00, 0x00]); // Dry-air gravity whole word.
    bus.WriteBytes(0x909eb5, [0x01, 0x00]); // Whole bounce speed.
    bus.WriteBytes(0x909eb7, [0x00, 0x10]); // Fractional bounce speed.

    // Bomb jumps do not use the normal movement-type speed table. `$90:8EF4` passes
    // the standalone `$90:9F25` record directly to `$90:9A7E`, while `$90:9A2C`
    // reads the dry-air bomb-jump magnitude from `$90:9EF5/$90:9EFB`.
    bus.WriteBytes(0x909f25, [
        0x01, 0x00, 0x00, 0x00, // diagonal acceleration 1.0000
        0x02, 0x00, 0x00, 0x00, // diagonal maximum 2.0000
        0x00, 0x00, 0x00, 0x80, // post-apex deceleration 0.8000
    ]);
    bus.WriteBytes(0x909ef5, [0x02, 0x00]); // Bomb-jump whole speed 2.
    bus.WriteBytes(0x909efb, [0x00, 0xc0]); // Bomb-jump subspeed C000.

    const int width = 8;
    const int height = 8;
    var floorBlocks = new ushort[width * height];
    for (int x = 0; x < width; x++)
        floorBlocks[4 * width + x] = 0x8000;
    RoomLevelData floor = new(
        width,
        height,
        floorBlocks,
        new byte[floorBlocks.Length],
        new ushort[floorBlocks.Length],
        new byte[8]);

    var samus = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);

    AssertTrue(
        !samus.TryApplyMorphTransition(
            bus, floor, SamusState.MorphingTransitionRightPose, nmiFrameCounter: 0),
        "morph entry is rejected without item bit $0004");
    AssertEqual(SamusState.CrouchingRightPose, samus.Pose, "rejected morph retains crouch");
    AssertEqual((ushort)48, samus.YPosition, "rejected morph retains center Y");

    samus.EquippedItems = 0x0004;
    AssertTrue(
        samus.TryApplyMorphTransition(
            bus, floor, SamusState.MorphingTransitionRightPose, nmiFrameCounter: 0),
        "equipped Morph Ball begins entry transition");
    AssertEqual((ushort)7, samus.Kinematics.YRadius, "morph transition radius from ROM");
    AssertEqual((ushort)57, samus.YPosition, "command seven moves center down nine");

    // Delay 2 at frame zero, delay 2 at frame one, then command F9 at frame two.
    for (int tick = 0; tick < 4; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((byte)0xf9, samus.LastAnimationDelayCommand!.Value, "morph transition reaches F9");
    AssertEqual(SamusState.MorphBallGroundRightPose, samus.PendingTransitionalPose!.Value,
        "F9 selects no-spring grounded endpoint");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "grounded F9 endpoint applies");
    AssertEqual(SamusState.MorphBallGroundRightPose, samus.Pose, "morph entry reaches stable ball");

    // Changing to the moving record preserves the shared rolling frame/timer. Movement
    // type four then reads its literal 0.8000 acceleration and remains floor-constrained.
    ushort timerBeforeRoll = samus.AnimationFrameTimer;
    samus.ApplyMorphBallPoseChange(bus, SamusState.MorphBallMovingRightPose);
    AssertEqual(timerBeforeRoll, samus.AnimationFrameTimer, "ball direction change preserves timer");
    MorphBallMovementResult rolling = SamusMorphBallMovement.StepGrounded(
        bus, floor, samus, nmiFrameCounter: 0);
    AssertTrue(rolling.Vertical.Collided, "rolling ball retains floor contact");
    AssertEqual((ushort)0x8000, samus.HorizontalSpeed.BaseSubspeed, "type-four acceleration uses ROM table");

    samus.ApplyMorphBallPoseChange(bus, SamusState.MorphBallMovingLeftPose);
    AssertEqual((ushort)1, samus.HorizontalSpeed.AccelerationMode, "ball reversal selects momentum mode one");
    AssertEqual((ushort)0x8000, samus.HorizontalSpeed.BaseSubspeed, "ball reversal preserves base magnitude");

    // Remove the floor and execute the stable grounded handler. Its failed +1 probe drives
    // the explicit `$1D/$41 -> $31/$32` walk-off transition and starts downward gravity.
    RoomLevelData empty = new(
        width,
        height,
        new ushort[width * height],
        new byte[width * height],
        new ushort[width * height],
        new byte[8]);
    samus.ApplyMorphBallPoseChange(bus, SamusState.MorphBallGroundLeftPose);
    samus.HorizontalSpeed.AccelerationMode = 0;
    MorphBallMovementResult unsupported = SamusMorphBallMovement.StepGrounded(
        bus, empty, samus, nmiFrameCounter: 1);
    AssertTrue(!unsupported.Vertical.Collided, "grounded ball detects missing floor");
    samus.ApplyMorphBallWalkOff(bus);
    AssertEqual(SamusState.MorphBallFallingLeftPose, samus.Pose, "left ball walk-off endpoint");
    AssertEqual((ushort)2, samus.Kinematics.YDirection, "ball walk-off starts falling");

    // Put the airborne body three pixels above the floor and give it a hard downward
    // magnitude. The collision launches bounce one with the two constants at `$90:9EB5`.
    samus.XPosition = 48;
    samus.YPosition = 55;
    samus.Kinematics.YDirection = 2;
    samus.Kinematics.YSpeed = 3;
    samus.Kinematics.YSubspeed = 0;
    samus.Kinematics.YAcceleration = 0;
    samus.Kinematics.YSubacceleration = 0;
    MorphBallMovementResult hardLanding = SamusMorphBallMovement.StepFalling(
        bus, floor, samus, controllerInput: 0, nmiFrameCounter: 0);
    AssertTrue(hardLanding.Landed, "hard ball fall collides with floor");
    AssertTrue(!samus.ApplyMorphBallLanding(bus), "hard landing launches first rebound");
    AssertEqual((ushort)1, samus.MorphBallBounceState, "first rebound state");
    AssertEqual((ushort)1, samus.Kinematics.YSpeed, "first rebound whole speed from ROM");
    AssertEqual((ushort)0x1000, samus.Kinematics.YSubspeed, "first rebound subspeed from ROM");
    AssertEqual((ushort)1, samus.Kinematics.YDirection, "first rebound moves upward");

    // Isolate the two later collision handlers at the exact floor boundary. State one
    // launches the smaller second rebound; state two finally installs stable ground art.
    samus.YPosition = 57;
    samus.Kinematics.YDirection = 2;
    samus.Kinematics.YSpeed = 1;
    samus.Kinematics.YSubspeed = 0;
    AssertTrue(!samus.ApplyMorphBallLanding(bus), "first-bounce collision launches second rebound");
    AssertEqual((ushort)2, samus.MorphBallBounceState, "second rebound state");
    AssertEqual((ushort)0, samus.Kinematics.YSpeed, "second rebound decrements whole constant");

    samus.Kinematics.YDirection = 2;
    samus.Kinematics.YSpeed = 1;
    AssertTrue(samus.ApplyMorphBallLanding(bus), "second-bounce collision grounds ball");
    AssertEqual(SamusState.MorphBallGroundLeftPose, samus.Pose, "bounce recovery uses facing-left ground pose");
    AssertEqual((ushort)0, samus.MorphBallBounceState, "grounding clears bounce state");

    // Unmorphing against only the floor succeeds and moves center up nine, preserving the
    // bottom boundary. A ceiling in row two makes both initial expansion probes collide;
    // radius-seven `$91:FFA7` must retain the ball instead of selecting crouch.
    AssertTrue(
        samus.TryApplyMorphTransition(
            bus, floor, SamusState.UnmorphingTransitionLeftPose, nmiFrameCounter: 0),
        "floor-constrained unmorph succeeds");
    AssertEqual((ushort)16, samus.Kinematics.YRadius, "unmorph transition radius");
    AssertEqual((ushort)48, samus.YPosition, "unmorph expansion keeps bottom boundary");

    var tunnelBlocks = (ushort[])floorBlocks.Clone();
    for (int x = 0; x < width; x++)
        tunnelBlocks[2 * width + x] = 0x8000;
    RoomLevelData tunnel = new(
        width,
        height,
        tunnelBlocks,
        new byte[tunnelBlocks.Length],
        new ushort[tunnelBlocks.Length],
        new byte[8]);
    var boxedBall = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 57,
    };
    boxedBall.RefreshCollisionRadii(bus);
    boxedBall.InitializeAnimation(bus);
    AssertTrue(
        !boxedBall.TryApplyMorphTransition(
            bus, tunnel, SamusState.UnmorphingTransitionRightPose, nmiFrameCounter: 1),
        "boxed Morph Ball rejects unmorph");
    AssertEqual(SamusState.MorphBallGroundRightPose, boxedBall.Pose, "boxed unmorph retains ball pose");
    AssertEqual((ushort)7, boxedBall.Kinematics.YRadius, "boxed unmorph retains ball radius");
    AssertEqual((ushort)57, boxedBall.YPosition, "boxed unmorph retains center");

    // Re-run entry with a nonzero vertical word to prove F9 uses its airborne operand, not
    // current collision radius or pose name. Spring Ball remains unequipped, selecting $31.
    var airborneEntry = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 48,
    };
    airborneEntry.RefreshCollisionRadii(bus);
    airborneEntry.InitializeAnimation(bus);
    AssertTrue(
        airborneEntry.TryApplyMorphTransition(
            bus, floor, SamusState.MorphingTransitionRightPose, nmiFrameCounter: 0),
        "airborne F9 fixture begins morph");
    airborneEntry.Kinematics.YSubspeed = 1;
    for (int tick = 0; tick < 4; tick++)
        airborneEntry.AnimateNoFx(bus);
    AssertEqual(SamusState.MorphBallFallingRightPose, airborneEntry.PendingTransitionalPose!.Value,
        "F9 nonzero Y subspeed selects airborne endpoint");

    // Repeat entry with Spring Ball bit `$0002`. F9 must use its equipped grounded operand,
    // then movement type `$11` reads its own speed record rather than ordinary type four.
    var spring = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        EquippedItems = 0x0006,
        XPosition = 48,
        YPosition = 48,
    };
    spring.RefreshCollisionRadii(bus);
    spring.InitializeAnimation(bus);
    AssertTrue(
        spring.TryApplyMorphTransition(
            bus, floor, SamusState.MorphingTransitionRightPose, nmiFrameCounter: 0),
        "Spring Ball fixture begins morph entry");
    for (int tick = 0; tick < 4; tick++)
        spring.AnimateNoFx(bus);
    AssertEqual(SamusState.SpringBallGroundRightPose, spring.PendingTransitionalPose!.Value,
        "F9 equipped endpoint selects Spring Ball ground");
    AssertTrue(spring.ApplyPendingVerifiedAnimationTransition(bus), "Spring Ball F9 applies");
    spring.ApplyMorphBallPoseChange(bus, SamusState.SpringBallMovingRightPose);
    MorphBallMovementResult springRoll = SamusMorphBallMovement.StepGrounded(
        bus, floor, spring, nmiFrameCounter: 0);
    AssertTrue(springRoll.Vertical.Collided, "Spring Ball roll retains floor contact");
    AssertEqual((ushort)0x8000, spring.HorizontalSpeed.BaseSubspeed,
        "type-$11 acceleration uses its ROM record");

    // `$79 -> $7F` initializes the literal dry-air 4.E000 jump. Releasing Jump on its
    // first movement frame invokes the shared variable-height cutoff before displacement.
    spring.ApplyMorphBallPoseChange(bus, SamusState.SpringBallGroundRightPose);
    spring.ApplySpringBallJump(bus, SamusState.SpringBallJumpRightPose);
    AssertEqual((ushort)4, spring.Kinematics.YSpeed, "Spring Ball launch whole speed");
    AssertEqual((ushort)0xe000, spring.Kinematics.YSubspeed, "Spring Ball launch subspeed");
    SamusMorphBallMovement.StepSpringBallInAir(
        bus, empty, spring, controllerInput: 0, nmiFrameCounter: 0);
    AssertEqual((ushort)2, spring.Kinematics.YDirection, "released Spring Ball jump cuts upward arc");

    // The no-Jump hard impact stores the distinctive `$0601` state. A held-Jump impact
    // instead clears that state and immediately relaunches through Make_Samus_Jump.
    spring.ApplyMorphBallPoseChange(bus, SamusState.SpringBallFallingRightPose);
    spring.Kinematics.YSpeed = 3;
    spring.Kinematics.YSubspeed = 0;
    spring.Kinematics.YDirection = 2;
    AssertTrue(!spring.ApplySpringBallLanding(bus, controllerInput: 0),
        "Spring Ball hard impact rebounds");
    AssertEqual((ushort)0x0601, spring.MorphBallBounceState, "Spring Ball first bounce state");
    AssertTrue(!spring.ApplySpringBallLanding(bus, (ushort)SnesButton.A),
        "held Jump immediately relaunches Spring Ball");
    AssertEqual((ushort)0, spring.MorphBallBounceState, "held-Jump relaunch clears bounce state");
    AssertEqual(SamusState.SpringBallJumpRightPose, spring.Pose, "held-Jump relaunch pose");

    // Bank-$93 projectile fixtures copied byte-for-byte from the normal-bomb pointer/data
    // records and its slow, fast, and explosion instruction lists. The production code
    // must follow these pointers; no test-facing constructor is allowed to inject damage,
    // radii, frame durations, or animation endpoints directly into a slot.
    WriteTestWord(bus, 0x9383fb, 0x8675); // Non-beam type five -> normal bomb data.
    bus.WriteBytes(0x938675, [0x1e, 0x00, 0xbf, 0x9f]);
    WriteTestWord(bus, 0x938683, 0xa06b); // Bomb-explosion instruction pointer.
    bus.WriteBytes(0x939fbf, [
        0x05, 0x00, 0x45, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x4c, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x53, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x5a, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xbf, 0x9f,
    ]);
    bus.WriteBytes(0x939fe3, [
        0x01, 0x00, 0x45, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x4c, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x53, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x5a, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xe3, 0x9f,
    ]);
    bus.WriteBytes(0x93a06b, [
        0x02, 0x00, 0x3e, 0xa8, 0x08, 0x08, 0x00, 0x00,
        0x02, 0x00, 0x54, 0xa8, 0x0c, 0x0c, 0x00, 0x00,
        0x02, 0x00, 0x6a, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x02, 0x00, 0x80, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x02, 0x00, 0x96, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x2f, 0x82,
    ]);

    // Power-bomb type three follows its own literal pointer/data record and three-frame
    // slow/fast loops. These bytes are ROM `$93:83F7/$8671/$9F87-$9FBE`; in particular,
    // damage `$00C8` and pointer `$9F87` are not host-selected stand-ins.
    WriteTestWord(bus, 0x9383f7, 0x8671);
    bus.WriteBytes(0x938671, [0xc8, 0x00, 0x87, 0x9f]);
    bus.WriteBytes(0x939f87, [
        0x05, 0x00, 0x97, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x9e, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0xa5, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0x87, 0x9f,
    ]);
    bus.WriteBytes(0x939fa3, [
        0x01, 0x00, 0x97, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x9e, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0xa5, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xa3, 0x9f,
    ]);

    // `$88:9079` and `$88:8D85` are the fixed-color tables indexed by the high byte of
    // the two authentic 8.8 radii. Keeping the retail bytes makes phase/color assertions
    // detect an incorrect radius update as well as a merely incorrect phase enum.
    bus.WriteBytes(0x889079, [
        0x10, 0x10, 0x10, 0x04, 0x04, 0x04, 0x06, 0x06, 0x06,
        0x08, 0x08, 0x08, 0x0a, 0x0a, 0x0a, 0x0c, 0x0c, 0x0c,
        0x0e, 0x0e, 0x0a, 0x10, 0x10, 0x08, 0x12, 0x12, 0x08,
        0x14, 0x14, 0x08, 0x16, 0x16, 0x08, 0x18, 0x18, 0x08,
        0x1a, 0x1a, 0x0a, 0x18, 0x18, 0x08, 0x16, 0x16, 0x06,
        0x14, 0x14, 0x04,
    ]);
    bus.WriteBytes(0x888d85, [
        0x0e, 0x0e, 0x0a, 0x0f, 0x0f, 0x09, 0x10, 0x10, 0x08,
        0x11, 0x11, 0x07, 0x12, 0x12, 0x06, 0x13, 0x13, 0x05,
        0x14, 0x14, 0x04, 0x15, 0x15, 0x03, 0x16, 0x16, 0x02,
        0x17, 0x17, 0x01, 0x18, 0x18, 0x00, 0x19, 0x19, 0x00,
        0x1a, 0x1a, 0x00, 0x1a, 0x1a, 0x00, 0x1a, 0x1a, 0x1a,
        0x1a, 0x1a, 0x1a, 0x1b, 0x1b, 0x1b,
    ]);

    // The bombable-terrain integration below intentionally supplies the literal bank-$84
    // BTS-zero reaction head and complete shared 1x1 respawn tail. `$84:CEDA` advances a
    // normal bomb's live pointer by three, so the leading `$8C46,$0A` sound instruction is
    // present in ROM but must be skipped. This makes a mistaken collision-list substitution
    // or invented host-side timer immediately observable.
    bus.WriteBytes(0x84cc3c, [
        0x46, 0x8c, 0x0a,       // Reaction-only sound $0A, skipped by normal bomb setup.
        0x04, 0x00, 0x45, 0xa3, // Four frames: visual block $053.
        0x04, 0x00, 0x4b, 0xa3, // Four frames: visual block $054.
        0x04, 0x00, 0x51, 0xa3, // Four frames: visual block $055.
        0x80, 0x01, 0x57, 0xa3, // 384 frames: blank-air visual block $0FF.
        0x04, 0x00, 0x51, 0xa3, // Reverse animation: $055.
        0x04, 0x00, 0x4b, 0xa3, // Reverse animation: $054.
        0x04, 0x00, 0x45, 0xa3, // Reverse animation: $053.
        0x17, 0x8b,             // DrawPLMBlock restores PLM_Vars and seeds timer one.
        0xbc, 0x86,             // Delete on the following PLM handler pass.
    ]);
    bus.WriteBytes(0x84a345, [0x01, 0x00, 0x53, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a34b, [0x01, 0x00, 0x54, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a351, [0x01, 0x00, 0x55, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a357, [0x01, 0x00, 0xff, 0x00, 0x00, 0x00]);

    // Additional bomb-reaction lists used later in this method. These bytes are literal
    // transcriptions of `$84:CADF`, `$84:C91C`, `$84:C922`, `$84:C8FE`, and `$84:C928`.
    // Keeping the instruction words in the sparse bus means the production interpreter—not
    // a test-only animation shortcut—still owns every duration, draw pointer, and deletion.
    bus.WriteBytes(0x84cadf, [
        0x79, 0x8c, 0x0a,       // Queue sound library 2, maximum 1: sound $0A.
        0x04, 0x00, 0x45, 0xa3, // Forward shot-break frames `$053,$054,$055`.
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3, // Blank `$0FF` hold for exactly 384 frames.
        0x04, 0x00, 0x51, 0xa3, // Reverse frames before restoring PLM_Vars.
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);

    // `$84:D084/$D08C` retain these complete power-bomb and Super-Missile respawning
    // programs after their weapon-family setup succeeds. They differ only in the sound
    // opcode's native entry point: `$8C7C` and `$8C10` both consume the odd byte `$0A`, but
    // queue at maximum one and maximum six respectively. The shared draw pointers below are
    // cartridge-authored shot-break art, and DrawPLMBlock restores synthesized `$x057/$x09F`.
    bus.WriteBytes(0x84cb71, [
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cb94, [
        0x7c, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);

    // The adjacent `$D090/$D088` headers select the permanent variants. Their shortened
    // 4/4/4 or 3/2/1 forward animation ends on one blank frame and deletes without executing
    // DrawPLMBlock, so the cleared terrain never grows back.
    bus.WriteBytes(0x84cc0b, [
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x01, 0x00, 0x57, 0xa3,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cc20, [
        0x7c, 0x8c, 0x0a,
        0x03, 0x00, 0x45, 0xa3,
        0x02, 0x00, 0x4b, 0xa3,
        0x01, 0x00, 0x51, 0xa3,
        0x01, 0x00, 0x57, 0xa3,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84c91c, [0x01, 0x00, 0xe7, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c922, [0x01, 0x00, 0xed, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c8fe, [0x01, 0x00, 0xb1, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c928, [0x01, 0x00, 0xf3, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84aae3, [0xbc, 0x86]); // PLMEntries_nothing: delete immediately.

    // The reveal draw data is likewise retail-authored. `$A4B1` contains two horizontal
    // two-word records joined by signed relative offset `(0,+1)`; the other three are
    // ordinary single-record lists terminated by a zero offset pair.
    bus.WriteBytes(0x84a4b1, [
        0x02, 0x00, 0xbc, 0xb0, 0xbc, 0x50, 0x00, 0x01,
        0x02, 0x00, 0xbc, 0xd0, 0xbc, 0xd0, 0x00, 0x00,
    ]);
    bus.WriteBytes(0x84a4e7, [0x01, 0x00, 0x57, 0xc0, 0x00, 0x00]);
    bus.WriteBytes(0x84a4ed, [0x01, 0x00, 0x9f, 0xc0, 0x00, 0x00]);
    bus.WriteBytes(0x84a4f3, [0x01, 0x00, 0xb6, 0xb0, 0x00, 0x00]);

    var noBombItemSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 57,
    };
    noBombItemSamus.RefreshCollisionRadii(bus);
    var noBombItemSystem = new SamusBombProjectileSystem();
    BombProjectileFrameResult rejectedBomb = noBombItemSystem.StepFrame(
        bus,
        floor,
        noBombItemSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(null, rejectedBomb.PlacedSlot, "bomb item bit gates placement");

    var bombProjectileSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x1004,
        XPosition = 48,
        YPosition = 57,
    };
    bombProjectileSamus.RefreshCollisionRadii(bus);
    var bombs = new SamusBombProjectileSystem();
    BombProjectileFrameResult placement = bombs.StepFrame(
        bus,
        floor,
        bombProjectileSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(0, placement.PlacedSlot, "first normal bomb uses physical slot zero");
    AssertEqual((ushort)1, bombs.BombCounter, "placement increments native bomb counter");
    AssertEqual((ushort)0x0010, bombs.CooldownTimer, "normal bomb loads cooldown table entry five");
    AssertEqual((ushort)59, bombs.Slots[0].BombTimer, "placement frame immediately decrements timer 60 to 59");
    AssertEqual((ushort)0x001e, bombs.Slots[0].Damage, "bomb damage follows bank-$93 data pointer");
    AssertEqual((ushort)0xad45, bombs.Slots[0].SpritemapPointer, "first instruction selects ROM bomb spritemap");
    AssertEqual((ushort)4, bombs.Slots[0].XRadius, "first instruction publishes ROM X radius");
    AssertEqual((ushort)4, bombs.Slots[0].YRadius, "first instruction publishes ROM Y radius");

    // A new edge during the active low-byte cooldown is rejected without incrementing the
    // aggregate. Release is a separate frame so ControllerInputState-like edge semantics
    // are represented explicitly in this direct subsystem test.
    bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    BombProjectileFrameResult cooldownRejected = bombs.StepFrame(
        bus,
        floor,
        bombProjectileSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(null, cooldownRejected.PlacedSlot, "active bomb cooldown rejects another edge");
    AssertEqual((ushort)1, bombs.BombCounter, "rejected edge does not alter bomb counter");

    // Run to timer nine, then prove bank-$A0's three X comparisons at timer eight using
    // three independent slots/lifecycles. Distances remain inside the strict radius sum.
    static (SamusBombProjectileSystem System, SamusState Samus) MakeDirectionFixture(
        TestAddressSpace fixtureBus,
        RoomLevelData fixtureFloor)
    {
        var fixtureSamus = new SamusState
        {
            Pose = SamusState.MorphBallGroundRightPose,
            EquippedItems = 0x1004,
            XPosition = 48,
            YPosition = 57,
        };
        fixtureSamus.RefreshCollisionRadii(fixtureBus);
        var fixtureSystem = new SamusBombProjectileSystem();
        fixtureSystem.StepFrame(
            fixtureBus,
            fixtureFloor,
            fixtureSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X);
        while (fixtureSystem.Slots[0].BombTimer > 9)
            fixtureSystem.StepFrame(fixtureBus, fixtureFloor, fixtureSamus, 0, 0);
        return (fixtureSystem, fixtureSamus);
    }

    foreach ((ushort samusX, byte expectedDirection) in new (ushort, byte)[]
    {
        (47, 1),
        (48, 2),
        (49, 3),
    })
    {
        (SamusBombProjectileSystem directionSystem, SamusState directionSamus) =
            MakeDirectionFixture(bus, floor);
        directionSamus.XPosition = samusX;
        BombProjectileFrameResult timerEight = directionSystem.StepFrame(
            bus,
            floor,
            directionSamus,
            0,
            0);
        AssertEqual(expectedDirection, timerEight.PublishedBombJumpDirection,
            $"timer-eight bomb direction at Samus X {samusX}");
        AssertEqual((ushort)expectedDirection, directionSamus.BombJumpDirection,
            "bank-$A0 publishes low byte without command bit");
        AssertTrue(!directionSamus.BombJumpStarting,
            "timer-eight overlap does not start movement in same frame");
    }

    // Continue the original straight fixture through timer zero. It must enter fast art at
    // fifteen, publish direction at eight, emit a five-block no-op reaction cross at zero,
    // animate all five explosion records, execute delete, and decrement BombCounter.
    while (bombs.Slots[0].BombTimer > 15)
        bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    AssertTrue(bombs.Slots[0].InstructionPointer >= 0x9fe3,
        "timer fifteen advances the live instruction pointer into fast animation");
    while (bombs.Slots[0].BombTimer > 8)
        bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    AssertEqual((ushort)2, bombProjectileSamus.BombJumpDirection,
        "same-X timer-eight overlap publishes straight direction");
    AssertTrue(bombProjectileSamus.TrySetupPublishedMorphedBombJump(),
        "following alpha consumes published morphed bomb jump");
    AssertEqual((ushort)0x0802, bombProjectileSamus.BombJumpDirection,
        "morphed setup adds command-three bit on following frame");

    BombProjectileFrameResult explosion = default;
    while (!explosion.ExplosionStarted)
        explosion = bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    AssertTrue(bombs.Slots[0].IsExploding, "timer zero selects bomb explosion list");
    AssertEqual((ushort)0x0501, bombs.Slots[0].Type, "first explosion pass marks block cross handled");
    AssertEqual(5, explosion.BlockReactions!.Count, "bomb explosion visits center/up/right/left/down");
    AssertEqual((byte)8, explosion.BlockReactions[4].CollisionType,
        "bottom reaction reaches fixture solid floor without inventing a PLM");

    for (int tick = 0; tick < 20 && bombs.BombCounter != 0; tick++)
        bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    AssertEqual((ushort)0, bombs.BombCounter, "explosion delete decrements bomb counter");
    AssertTrue(!bombs.Slots[0].IsActive, "delete opcode clears complete bomb slot");

    // Selected HUD item three takes the power-bomb branch even without the normal Bomb
    // item bit. Placement consumes one round, locks `$0CEA`, initializes type `$0300` from
    // the literal bank-$93 record, and installs cooldown table entry three (`$28`).
    var powerBombSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x0004,
        SelectedHudItem = 3,
        PowerBombs = 2,
        XPosition = 48,
        YPosition = 48,
    };
    powerBombSamus.RefreshCollisionRadii(bus);
    var powerBombs = new SamusBombProjectileSystem();
    BombProjectileFrameResult powerBombPlacement = powerBombs.StepFrame(
        bus,
        floor,
        powerBombSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(0, powerBombPlacement.PlacedSlot,
        "selected power bomb uses first physical bomb slot");
    AssertEqual((ushort)1, powerBombSamus.PowerBombs,
        "power-bomb placement decrements ammo exactly once");
    AssertEqual((ushort)3, powerBombSamus.SelectedHudItem,
        "remaining power-bomb ammo retains HUD selection");
    AssertEqual((ushort)0x0028, powerBombs.CooldownTimer,
        "power bomb loads non-beam cooldown table entry three");
    AssertEqual((ushort)0x0300, powerBombs.Slots[0].Type,
        "power-bomb projectile family is HUD index in high byte");
    AssertEqual((ushort)0x00c8, powerBombs.Slots[0].Damage,
        "power-bomb damage follows bank-$93 type-three data");
    AssertEqual((ushort)0xab97, powerBombs.Slots[0].SpritemapPointer,
        "power-bomb placement selects first retail slow-list spritemap");
    AssertTrue(powerBombs.PowerBombExplosion.IsArmed,
        "placement sets negative native power-bomb flag");
    AssertTrue(!powerBombs.PowerBombExplosion.IsActive,
        "bank-$88 explosion waits for projectile fuse");

    // `$90:C157` shares the normal bomb's 60->15 timing and adds `$1C` to the live
    // instruction pointer. A second selected-item edge is rejected by the armed flag
    // before helper two can perturb either aggregate counter.
    powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
    BombProjectileFrameResult armedRejected = powerBombs.StepFrame(
        bus,
        floor,
        powerBombSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(null, armedRejected.PlacedSlot,
        "negative power-bomb flag rejects a second placement");
    AssertEqual((ushort)1, powerBombs.BombCounter,
        "armed rejection preserves bomb aggregate");
    while (powerBombs.Slots[0].BombTimer > 15)
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
    AssertTrue(powerBombs.Slots[0].InstructionPointer >= 0x9fa3,
        "power-bomb timer fifteen enters retail fast list");

    BombProjectileFrameResult powerBombFuse = default;
    while (!powerBombFuse.ExplosionStarted)
        powerBombFuse = powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
    AssertEqual((ushort)0, powerBombs.Slots[0].BombTimer,
        "$FFFF fuse sentinel is consumed by first mode-three collision call");
    AssertEqual(PowerBombExplosionPhase.PreExplosionWhite,
        powerBombs.PowerBombExplosion.Phase,
        "fuse expiry executes $88:8B14 setup");
    AssertEqual((ushort)0x0400, powerBombs.PowerBombExplosion.PreExplosionRadius,
        "pre-explosion begins at retail 4.00-pixel radius");
    AssertEqual((ushort)0x8000, powerBombs.PowerBombExplosion.Status,
        "normal explosion publishes active status $8000");
    AssertEqual(0, powerBombFuse.BlockReactions!.Count,
        "fuse-expiration sentinel frame does not scan terrain");

    // HDMA executes before the next projectile pass. White pre-flash grows 4.00 by 48.00,
    // then the still-zero damaging radius scans the one-block rectangle's four duplicated
    // corners in bank-$94 top/left/bottom/right order.
    BombProjectileFrameResult firstPowerBombRadius = powerBombs.StepFrame(
        bus, floor, powerBombSamus, 0, 0);
    AssertEqual((ushort)0x3400, powerBombs.PowerBombExplosion.PreExplosionRadius,
        "first white pre-explosion frame applies $3000 speed");
    AssertEqual((ushort)0x2f80, powerBombs.PowerBombExplosion.RadiusSpeed,
        "white pre-explosion subtracts $0080 acceleration");
    AssertEqual(4, firstPowerBombRadius.BlockReactions!.Count,
        "zero damaging radius scans four inclusive one-block edges");
    AssertTrue(firstPowerBombRadius.BlockReactions.All(reaction =>
            reaction.BlockX == 3 && reaction.BlockY == 3),
        "zero-radius border duplicates the center exactly four times");

    int whiteFlashFrames = 1;
    while (powerBombs.PowerBombExplosion.Phase == PowerBombExplosionPhase.PreExplosionWhite)
    {
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
        whiteFlashFrames++;
        AssertTrue(whiteFlashFrames < 32, "white pre-explosion phase terminates");
    }
    AssertEqual(PowerBombExplosionPhase.PreExplosionYellow,
        powerBombs.PowerBombExplosion.Phase,
        "white threshold advances to yellow shape phase");
    AssertTrue(powerBombs.PowerBombExplosion.PreExplosionRadius >= 0x9200,
        "white phase crosses literal $9200 radius threshold");
    AssertEqual((ushort)0x9f06, powerBombs.PowerBombExplosion.ShapeDefinitionPointer,
        "yellow pre-explosion starts at shape table $9F06");

    StepFrames(4, _ => powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0));
    AssertEqual(PowerBombExplosionPhase.ExplosionYellow,
        powerBombs.PowerBombExplosion.Phase,
        "four 192-byte yellow shapes advance to damaging explosion");
    AssertEqual((ushort)0x0400, powerBombs.PowerBombExplosion.ExplosionRadius,
        "damaging yellow explosion restarts at 4.00 pixels");
    AssertEqual((ushort)0, powerBombs.PowerBombExplosion.RadiusSpeed,
        "damaging yellow explosion restarts with zero speed");

    int yellowExplosionFrames = 0;
    while (powerBombs.PowerBombExplosion.Phase == PowerBombExplosionPhase.ExplosionYellow)
    {
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
        yellowExplosionFrames++;
        AssertTrue(yellowExplosionFrames < 128, "yellow explosion reaches $8600 threshold");
    }
    AssertEqual(PowerBombExplosionPhase.ExplosionWhite,
        powerBombs.PowerBombExplosion.Phase,
        "yellow radius threshold advances to white shape phase");
    AssertTrue(powerBombs.PowerBombExplosion.ExplosionRadius >= 0x8600,
        "yellow explosion crosses literal $8600 threshold");
    AssertEqual((ushort)0x9246, powerBombs.PowerBombExplosion.ShapeDefinitionPointer,
        "white explosion starts at first retail ellipse table");

    int whiteExplosionFrames = 0;
    while (powerBombs.PowerBombExplosion.Phase == PowerBombExplosionPhase.ExplosionWhite)
    {
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
        whiteExplosionFrames++;
        AssertTrue(whiteExplosionFrames < 32, "white explosion shape sequence terminates");
    }
    AssertEqual(17, whiteExplosionFrames,
        "$9246-$9F06 white phase contains seventeen 192-byte shapes");
    AssertEqual(PowerBombExplosionPhase.Afterglow,
        powerBombs.PowerBombExplosion.Phase,
        "white shapes advance to stage-five afterglow");

    // Moving one pixel guarantees cleanup cannot begin Crystal Flash. The 32-step byte
    // counter performs 31 fades four frames apart, then `$88:8B4E` clears status/radii and
    // releases the flag; the same projectile pass sees flag zero and deletes the slot.
    powerBombSamus.XPosition++;
    int afterglowFrames = 0;
    BombProjectileFrameResult cleanupFrame = default;
    while (powerBombs.PowerBombExplosion.IsActive)
    {
        cleanupFrame = powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
        afterglowFrames++;
        AssertTrue(afterglowFrames < 160, "power-bomb afterglow reaches cleanup");
    }
    AssertEqual(125, afterglowFrames,
        "afterglow uses wrapping timer zero then 31 four-frame waits");
    AssertTrue(cleanupFrame.ProjectileDeleted,
        "cleanup frame deletes released power-bomb projectile");
    AssertEqual((ushort)0, powerBombs.BombCounter,
        "power-bomb cleanup decrements shared bomb counter");
    AssertTrue(!powerBombs.PowerBombExplosion.IsArmed,
        "failed Crystal Flash cleanup releases power-bomb flag");
    AssertEqual((ushort)0, powerBombs.PowerBombExplosion.ExplosionRadius,
        "cleanup clears damaging radius");

    // Put the explosion center on a type-$5 horizontal extension whose signed BTS $FF
    // redirects one column left to a BTS-zero type-$F parent. `$94:9CF4` visits center
    // before left, so the extension must spawn exactly one PLM and synchronously turn the
    // parent into temporary type-$8 terrain; the later left-arm visit sees that mutation.
    var reactionWords = new ushort[width * height];
    var reactionBts = new byte[reactionWords.Length];
    const int reactionParentIndex = 3 * width + 2;
    const int reactionExtensionIndex = 3 * width + 3;
    reactionWords[reactionParentIndex] = 0xf321;
    reactionBts[reactionParentIndex] = 0;
    reactionWords[reactionExtensionIndex] = 0x5058;
    reactionBts[reactionExtensionIndex] = 0xff;
    var reactionDefinitions = new byte[0x400 * 8];
    RoomLevelData reactionLevel = new(
        width,
        height,
        reactionWords,
        reactionBts,
        new ushort[reactionWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer reactionStreamer = reactionLevel.CreateBackgroundStreamer();
    var reactionPlms = new RoomPlmSystem();
    var reactionBombs = new SamusBombProjectileSystem();
    var reactionSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x1004,
        XPosition = 48,
        YPosition = 48,
    };
    reactionSamus.RefreshCollisionRadii(bus);
    reactionBombs.StepFrame(
        bus,
        reactionLevel,
        reactionSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        reactionPlms);

    BombProjectileFrameResult reactionExplosion = default;
    while (!reactionExplosion.ExplosionStarted)
    {
        reactionExplosion = reactionBombs.StepFrame(
            bus,
            reactionLevel,
            reactionSamus,
            0,
            0,
            reactionPlms);
    }

    AssertEqual((byte)5, reactionExplosion.BlockReactions![0].CollisionType,
        "bomb cross records the visited horizontal extension");
    AssertEqual((byte)0xff, reactionExplosion.BlockReactions[0].Behavior,
        "bomb cross preserves the extension's signed redirect BTS");
    AssertEqual((byte)8, reactionExplosion.BlockReactions[3].CollisionType,
        "later left-arm reaction observes the synchronously mutated parent");
    AssertEqual(1, reactionPlms.ActiveCount,
        "extension and later parent visit produce one native reaction PLM");
    AssertEqual((ushort)0x8058,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).LevelWord,
        "CEDA keeps a type-F bomb block temporarily solid through movement beta");

    // PLM_Handler runs after movement beta in the same gameplay frame. Normal-bomb setup
    // began at `$CC3F`, so this first handler pass draws air `$0053` without queueing the
    // reaction head's sound $0A. The bomb explosion itself remains the sound owner.
    reactionPlms.Step(bus, reactionLevel, reactionStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).LevelWord,
        "same-frame PLM pass draws the first bomb-block air frame");
    AssertEqual(0, reactionPlms.SoundRequests.Count,
        "normal bomb setup skips reaction PLM sound $0A");

    // Walk the exact forward, 384-frame blank hold, reverse, restore, and delayed-delete
    // timeline. The restored word is deliberately `$F058`, not the original `$F321`:
    // setup CEDA synthesized PLM_Vars by replacing all twelve low bits with `$058`.
    StepFrames(12, _ => reactionPlms.Step(bus, reactionLevel, reactionStreamer, 0, 0, 0));
    AssertEqual((ushort)0x00ff,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).LevelWord,
        "bomb reaction reaches blank air after three four-frame transitions");
    StepFrames(384 + 12, _ =>
        reactionPlms.Step(bus, reactionLevel, reactionStreamer, 0, 0, 0));
    AssertEqual((ushort)0xf058,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).LevelWord,
        "bomb reaction restores synthesized type-F parent after native hold");
    AssertEqual(1, reactionPlms.ActiveCount,
        "DrawPLMBlock retains reaction PLM through its timer-one restore pass");
    reactionPlms.Step(bus, reactionLevel, reactionStreamer, 0, 0, 0);
    AssertEqual(0, reactionPlms.ActiveCount,
        "bomb reaction PLM deletes on the handler pass after restoration");

    // Exercise the public bomb pipeline as well as the setup methods below. A BTS-eight
    // type-$C parent remains unchanged during CF2E setup, then the same gameplay frame's
    // PLM pass reveals `$C057`. This catches an omitted/wrong collision-type dispatcher
    // branch even if the isolated bank-$84 setup tests continue to pass.
    var integratedRevealWords = new ushort[width * height];
    var integratedRevealBts = new byte[integratedRevealWords.Length];
    const int integratedRevealIndex = 3 * width + 3;
    integratedRevealWords[integratedRevealIndex] = 0xc000;
    integratedRevealBts[integratedRevealIndex] = 8;
    RoomLevelData integratedRevealLevel = new(
        width,
        height,
        integratedRevealWords,
        integratedRevealBts,
        new ushort[integratedRevealWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer integratedRevealStreamer =
        integratedRevealLevel.CreateBackgroundStreamer();
    var integratedRevealPlms = new RoomPlmSystem();
    var integratedRevealBombs = new SamusBombProjectileSystem();
    var integratedRevealSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x1004,
        XPosition = 48,
        YPosition = 48,
    };
    integratedRevealSamus.RefreshCollisionRadii(bus);
    integratedRevealBombs.StepFrame(
        bus,
        integratedRevealLevel,
        integratedRevealSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        integratedRevealPlms);
    BombProjectileFrameResult integratedRevealExplosion = default;
    while (!integratedRevealExplosion.ExplosionStarted)
    {
        integratedRevealExplosion = integratedRevealBombs.StepFrame(
            bus,
            integratedRevealLevel,
            integratedRevealSamus,
            0,
            0,
            integratedRevealPlms);
    }
    AssertEqual((byte)12, integratedRevealExplosion.BlockReactions![0].CollisionType,
        "bomb dispatcher records center shootable-solid parent");
    AssertEqual(1, integratedRevealPlms.ActiveCount,
        "bomb dispatcher installs shootable reveal PLM");
    AssertEqual((ushort)0xc000,
        integratedRevealLevel.GetCollisionBlockByIndex(integratedRevealIndex).LevelWord,
        "CF2E leaves required-weapon parent unchanged through movement beta");
    integratedRevealPlms.Step(
        bus,
        integratedRevealLevel,
        integratedRevealStreamer,
        0,
        0,
        0);
    AssertEqual((ushort)0xc057,
        integratedRevealLevel.GetCollisionBlockByIndex(integratedRevealIndex).LevelWord,
        "same-frame PLM pass reveals required power-bomb block");

    // Shootable reaction setup `$84:CE6B` is easy to get subtly wrong because it does not
    // preserve the original low twelve bits. Prove that a type-C/BTS-zero parent becomes
    // temporary `$8052`, begins at the ROM's `$0053` air frame, queues sound `$0A` through
    // the max-one opcode, and ultimately restores synthesized `$C052` rather than `$C321`.
    var shotWords = new ushort[width * height];
    const int shotIndex = 3 * width + 3;
    shotWords[shotIndex] = 0xc321;
    RoomLevelData shotLevel = new(
        width,
        height,
        shotWords,
        new byte[shotWords.Length],
        new ushort[shotWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer shotStreamer = shotLevel.CreateBackgroundStreamer();
    var shotPlms = new RoomPlmSystem();
    AssertTrue(
        shotPlms.TrySpawnBombedShootableBlock(shotLevel, shotIndex, 0, 0x0500),
        "normal bomb allocates respawning shot-block PLM");
    AssertEqual((ushort)0x8052,
        shotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "CE6B installs synthesized temporary shot-block word");
    shotPlms.Step(bus, shotLevel, shotStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053,
        shotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning shot block begins with retail air frame");
    AssertEqual(1, shotPlms.SoundRequests.Count,
        "shot-block head queues one sound request");
    AssertEqual((byte)0x0a, shotPlms.SoundRequests[0].SoundId,
        "shot-block head queues crumble sound $0A");
    AssertEqual((byte)1, shotPlms.SoundRequests[0].MaximumQueued,
        "$84:8C79 uses sound-library-two maximum one");
    StepFrames(12, _ => shotPlms.Step(bus, shotLevel, shotStreamer, 0, 0, 0));
    AssertEqual((ushort)0x00ff,
        shotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning shot block reaches its blank hold word");
    StepFrames(384 + 12, _ =>
        shotPlms.Step(bus, shotLevel, shotStreamer, 0, 0, 0));
    AssertEqual((ushort)0xc052,
        shotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning shot block restores CE6B's synthesized parent");
    shotPlms.Step(bus, shotLevel, shotStreamer, 0, 0, 0);
    AssertEqual(0, shotPlms.ActiveCount,
        "respawning shot block deletes one handler pass after restoration");

    // `$84:D084` is the respawning power-bomb block. Its CF2E setup accepts family `$0300`,
    // synthesizes `$C057`, and executes `$CB94`; the latter begins with the direct `$8C7C`
    // max-one sound opcode rather than ordinary shot block `$8C79`. Following the entire
    // reverse animation proves RestoreLevelWord is the setup-produced `$C057`, never the
    // fixture's original visual block `$C321`.
    RoomLevelData powerBombShotLevel = new(
        width,
        height,
        shotWords,
        new byte[shotWords.Length],
        new ushort[shotWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer powerBombShotStreamer =
        powerBombShotLevel.CreateBackgroundStreamer();
    var powerBombShotPlms = new RoomPlmSystem();
    AssertTrue(
        powerBombShotPlms.TrySpawnProjectileShotBlock(
            powerBombShotLevel,
            shotIndex,
            behavior: 8,
            projectileType: 0x0300,
            solidBlock: true),
        "power bomb allocates BTS-eight respawning block PLM");
    AssertEqual((ushort)0x8057,
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "CF2E installs synthesized temporary power-bomb word");
    powerBombShotPlms.Step(
        bus, powerBombShotLevel, powerBombShotStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053,
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "power-bomb block begins with retail air frame");
    AssertEqual(new PlmSoundRequest(2, 0x0a, 1), powerBombShotPlms.SoundRequests[0],
        "$8C7C queues power-bomb breakup sound with maximum one");
    StepFrames(12 + 384 + 12, _ =>
        powerBombShotPlms.Step(bus, powerBombShotLevel, powerBombShotStreamer, 0, 0, 0));
    AssertEqual((ushort)0xc057,
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning power-bomb block restores synthesized $C057 parent");
    powerBombShotPlms.Step(
        bus, powerBombShotLevel, powerBombShotStreamer, 0, 0, 0);
    AssertEqual(0, powerBombShotPlms.ActiveCount,
        "respawning power-bomb block deletes after restoration");

    // `$84:D08C` is the corresponding Super Missile block. CF67 accepts family `$0200`,
    // synthesizes `$C09F`, and retains `$CB71`'s max-six sound plus the same 384-frame
    // blank hold. An ordinary missile must fail that setup without touching terrain or
    // leaving an apparently occupied host PLM slot.
    RoomLevelData superShotLevel = new(
        width,
        height,
        shotWords,
        new byte[shotWords.Length],
        new ushort[shotWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer superShotStreamer = superShotLevel.CreateBackgroundStreamer();
    var superShotPlms = new RoomPlmSystem();
    AssertTrue(!superShotPlms.TrySpawnProjectileShotBlock(
            superShotLevel,
            shotIndex,
            behavior: 10,
            projectileType: 0x0100,
            solidBlock: true),
        "ordinary missile is rejected by Super Missile block setup");
    AssertEqual((ushort)0xc321,
        superShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "rejected missile leaves Super Missile block word untouched");
    AssertEqual(0, superShotPlms.ActiveCount,
        "rejected missile leaves no live PLM header");
    AssertTrue(superShotPlms.TrySpawnProjectileShotBlock(
            superShotLevel,
            shotIndex,
            behavior: 10,
            projectileType: 0x0200,
            solidBlock: true),
        "Super Missile allocates BTS-A respawning block PLM");
    AssertEqual((ushort)0x809f,
        superShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "CF67 installs synthesized temporary Super Missile word");
    superShotPlms.Step(bus, superShotLevel, superShotStreamer, 0, 0, 0);
    AssertEqual(new PlmSoundRequest(2, 0x0a, 6), superShotPlms.SoundRequests[0],
        "$CB71 queues Super Missile breakup sound with maximum six");
    StepFrames(12 + 384 + 12, _ =>
        superShotPlms.Step(bus, superShotLevel, superShotStreamer, 0, 0, 0));
    AssertEqual((ushort)0xc09f,
        superShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning Super Missile block restores synthesized $C09F parent");
    superShotPlms.Step(bus, superShotLevel, superShotStreamer, 0, 0, 0);
    AssertEqual(0, superShotPlms.ActiveCount,
        "respawning Super Missile block deletes after restoration");

    // BTS nine and B select the two permanent lists. Run both beyond their final timer so
    // `$CC20`'s asymmetric 3/2/1 timing and `$CC0B`'s ordinary 4/4/4 timing must each parse,
    // publish blank `$00FF`, and delete without executing DrawPLMBlock.
    foreach ((byte behavior, ushort projectileType, string familyName) in new[]
    {
        ((byte)9, (ushort)0x0300, "power-bomb"),
        ((byte)11, (ushort)0x0200, "Super-Missile"),
    })
    {
        RoomLevelData permanentWeaponLevel = new(
            width,
            height,
            shotWords,
            new byte[shotWords.Length],
            new ushort[shotWords.Length],
            reactionDefinitions);
        BackgroundTilemapStreamer permanentWeaponStreamer =
            permanentWeaponLevel.CreateBackgroundStreamer();
        var permanentWeaponPlms = new RoomPlmSystem();
        AssertTrue(permanentWeaponPlms.TrySpawnProjectileShotBlock(
                permanentWeaponLevel,
                shotIndex,
                behavior,
                projectileType,
                solidBlock: true),
            $"{familyName} allocates permanent weapon-gated block PLM");
        for (int frame = 0; frame < 20; frame++)
        {
            permanentWeaponPlms.Step(
                bus, permanentWeaponLevel, permanentWeaponStreamer, 0, 0, 0);
        }
        AssertEqual((ushort)0x00ff,
            permanentWeaponLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
            $"permanent {familyName} block ends on retail blank word");
        AssertEqual(0, permanentWeaponPlms.ActiveCount,
            $"permanent {familyName} block deletes without restoration");
    }

    // The final four nonnegative table entries all point to PLMEntries_nothing. Negative BTS
    // has a collision-nibble asymmetry instead: type C allocates an area-table no-op, while
    // type four returns before Spawn_PLM. These slots are visually invisible, but retaining
    // their one-handler lifetime prevents the finite 40-slot pool from behaving differently.
    RoomLevelData noOpShotLevel = new(
        width,
        height,
        shotWords,
        new byte[shotWords.Length],
        new ushort[shotWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer noOpShotStreamer = noOpShotLevel.CreateBackgroundStreamer();
    var noOpShotPlms = new RoomPlmSystem();
    AssertTrue(noOpShotPlms.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            shotIndex,
            behavior: 12,
            projectileType: 0,
            solidBlock: true),
        "BTS-C allocates retail no-op shot PLM");
    AssertTrue(noOpShotPlms.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            shotIndex,
            behavior: 0x80,
            projectileType: 0,
            solidBlock: true),
        "negative type-C BTS allocates area-table no-op shot PLM");
    AssertTrue(!noOpShotPlms.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            shotIndex,
            behavior: 0x80,
            projectileType: 0,
            solidBlock: false),
        "negative type-four BTS exits before shot PLM allocation");
    AssertEqual(2, noOpShotPlms.ActiveCount,
        "only the two solid/no-op reactions occupy native slots");
    AssertEqual((ushort)0xc321,
        noOpShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "all no-op shot reactions preserve terrain");
    noOpShotPlms.Step(bus, noOpShotLevel, noOpShotStreamer, 0, 0, 0);
    AssertEqual(0, noOpShotPlms.ActiveCount,
        "no-op shot PLMs delete on their first handler pass");

    // Normal bombs striking BTS 8 and A do not break their weapon-gated blocks. CF2E/CF67
    // redirect the PLM pointer to one-frame diagnostic reveals. The complete level word is
    // drawn by the normal bank-$84 draw parser and remains after the slot deletes.
    var revealWords = new ushort[width * height];
    const int powerRevealIndex = 2 * width + 2;
    const int superRevealIndex = 2 * width + 4;
    const int areaNoOpIndex = 4 * width + 2;
    revealWords[powerRevealIndex] = 0x4000;
    revealWords[superRevealIndex] = 0xc000;
    revealWords[areaNoOpIndex] = 0xc222;
    RoomLevelData revealLevel = new(
        width,
        height,
        revealWords,
        new byte[revealWords.Length],
        new ushort[revealWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer revealStreamer = revealLevel.CreateBackgroundStreamer();
    var revealPlms = new RoomPlmSystem();
    AssertTrue(
        revealPlms.TrySpawnBombedShootableBlock(revealLevel, powerRevealIndex, 8, 0x0500),
        "normal bomb allocates power-bomb reveal PLM");
    AssertTrue(
        revealPlms.TrySpawnBombedShootableBlock(revealLevel, superRevealIndex, 10, 0x0500),
        "normal bomb allocates super-missile reveal PLM");
    AssertTrue(
        revealPlms.TrySpawnBombedShootableBlock(revealLevel, areaNoOpIndex, 0x80, 0x0500),
        "negative type-C BTS allocates area-table no-op PLM");
    revealPlms.Step(bus, revealLevel, revealStreamer, 0, 0, 0);
    AssertEqual((ushort)0xc057,
        revealLevel.GetCollisionBlockByIndex(powerRevealIndex).LevelWord,
        "normal bomb reveals visible power-bomb block word");
    AssertEqual((ushort)0xc09f,
        revealLevel.GetCollisionBlockByIndex(superRevealIndex).LevelWord,
        "normal bomb reveals visible super-missile block word");
    AssertEqual(0, revealPlms.SoundRequests.Count,
        "weapon-required reveal lists do not queue shot-break sound");
    AssertEqual((ushort)0xc222,
        revealLevel.GetCollisionBlockByIndex(areaNoOpIndex).LevelWord,
        "area-dependent shootable no-op leaves terrain unchanged");
    AssertEqual(2, revealPlms.ActiveCount,
        "area no-op deletes while both one-frame reveal PLMs remain timed");
    revealPlms.Step(bus, revealLevel, revealStreamer, 0, 0, 0);
    AssertEqual(0, revealPlms.ActiveCount,
        "weapon-required reveal PLMs delete after their one-frame draw");

    // `$94:9DA4` chooses a two-by-two crumble reveal for BTS three. The draw record is two
    // horizontal rows with a signed `(0,+1)` continuation; asserting all four words guards
    // both the special dispatch and the generic multi-record parser. Brinstar's negative
    // BTS `$82` instead selects the area-specific speed-block reveal at `$84:C928`.
    var specialWords = new ushort[width * height];
    const int crumbleIndex = 2 * width + 2;
    const int speedIndex = 5 * width + 5;
    specialWords[crumbleIndex] = 0xb000;
    specialWords[speedIndex] = 0xb000;
    RoomLevelData specialLevel = new(
        width,
        height,
        specialWords,
        new byte[specialWords.Length],
        new ushort[specialWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer specialStreamer = specialLevel.CreateBackgroundStreamer();
    var specialPlms = new RoomPlmSystem();
    AssertTrue(
        specialPlms.TrySpawnBombedSpecialBlock(specialLevel, crumbleIndex, 3, 0, 0x0500),
        "normal bomb allocates two-by-two crumble reveal");
    AssertTrue(
        specialPlms.TrySpawnBombedSpecialBlock(specialLevel, speedIndex, 0x82, 1, 0x0500),
        "Brinstar negative BTS two allocates speed-block reveal");
    specialPlms.Step(bus, specialLevel, specialStreamer, 0, 0, 0);
    AssertEqual((ushort)0xb0bc,
        specialLevel.GetCollisionBlockByIndex(crumbleIndex).LevelWord,
        "crumble reveal writes parent word");
    AssertEqual((ushort)0x50bc,
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + 1).LevelWord,
        "crumble reveal writes right extension");
    AssertEqual((ushort)0xd0bc,
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + width).LevelWord,
        "crumble reveal writes lower vertical extension");
    AssertEqual((ushort)0xd0bc,
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + width + 1).LevelWord,
        "crumble reveal writes lower-right vertical extension");
    AssertEqual((ushort)0xb0b6,
        specialLevel.GetCollisionBlockByIndex(speedIndex).LevelWord,
        "Brinstar area table reveals speed-booster block");
    specialPlms.Step(bus, specialLevel, specialStreamer, 0, 0, 0);
    AssertEqual(0, specialPlms.ActiveCount,
        "special reveal PLMs delete after their one-frame draw");

    // Bank `$A0:97E2-$A0:984E` decides direction from bomb-versus-Samus X. Its bank-$91
    // command-three handoff must retain the stable ball pose and arm `$0801-$0803`; it
    // must also reject non-ball callers instead of silently inventing a normal jump.
    var bombJump = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 48,
    };
    bombJump.RefreshCollisionRadii(bus);
    bombJump.InitializeAnimation(bus);
    bombJump.Kinematics.YAcceleration = 0;
    bombJump.Kinematics.YSubacceleration = 0x4000;
    AssertThrows<ArgumentOutOfRangeException>(
        () => bombJump.RequestMorphedBombJump(0),
        "bomb-jump direction zero rejected");

    bombJump.RequestMorphedBombJump(3);
    AssertEqual((ushort)0x0803, bombJump.BombJumpDirection, "right bomb jump command word");
    ushort startX = bombJump.XPosition;
    ushort startY = bombJump.YPosition;
    BombJumpMovementResult start = SamusBombJumpMovement.Start(bus, bombJump);
    AssertTrue(start.Started && !start.Ended, "bomb-jump start handler reports initialization");
    AssertEqual(startX, bombJump.XPosition, "bomb-jump start frame has no horizontal displacement");
    AssertEqual(startY, bombJump.YPosition, "bomb-jump start frame has no vertical displacement");
    AssertEqual((ushort)2, bombJump.Kinematics.YSpeed, "bomb-jump whole speed comes from $90:9EF5");
    AssertEqual((ushort)0xc000, bombJump.Kinematics.YSubspeed, "bomb-jump subspeed comes from $90:9EFB");
    AssertEqual((ushort)1, bombJump.Kinematics.YDirection, "bomb jump starts upward");

    // The first diagonal handler frame accelerates by exactly the literal 1.0000 record,
    // moves right one pixel, moves upward by the pre-gravity 2.C000 magnitude, then stores
    // the reduced 2.8000 magnitude for the following frame.
    BombJumpMovementResult diagonal = SamusBombJumpMovement.Step(
        bus, empty, bombJump, nmiFrameCounter: 0);
    AssertTrue(!diagonal.Ended, "unobstructed diagonal bomb jump remains active");
    AssertEqual((ushort)(startX + 1), bombJump.XPosition, "right bomb jump uses $90:9F25 displacement");
    AssertEqual((ushort)2, bombJump.Kinematics.YSpeed, "bomb-jump gravity stores next whole speed");
    AssertEqual((ushort)0x8000, bombJump.Kinematics.YSubspeed, "bomb-jump gravity stores next subspeed");

    // `$90:8F1B` interprets a wrapped whole word as signed underflow. It switches to down,
    // installs diagonal deceleration mode two, and `$90:E032` relinquishes control before
    // making another vertical move. The normal type-four ball handler owns the descent.
    bombJump.Kinematics.YSpeed = 0xffff;
    bombJump.Kinematics.YSubspeed = 0;
    BombJumpMovementResult apex = SamusBombJumpMovement.Step(
        bus, empty, bombJump, nmiFrameCounter: 1);
    AssertTrue(apex.Ended, "signed bomb-jump apex ends special handler");
    AssertEqual((ushort)0, bombJump.BombJumpDirection, "bomb-jump apex clears command word");
    AssertEqual((ushort)2, bombJump.Kinematics.YDirection, "bomb-jump apex hands off downward direction");
    AssertEqual((ushort)2, bombJump.HorizontalSpeed.AccelerationMode, "diagonal apex selects mode two");

    // Direction two deliberately omits horizontal calculation. A ceiling collision ends
    // the handler on that same frame and zeros vertical magnitude before normal movement
    // resumes, matching `$90:E077-$90:E094`.
    var ceilingBlocks = new ushort[width * height];
    for (int x = 0; x < width; x++)
        ceilingBlocks[2 * width + x] = 0x8000;
    RoomLevelData ceiling = new(
        width,
        height,
        ceilingBlocks,
        new byte[ceilingBlocks.Length],
        new ushort[ceilingBlocks.Length],
        new byte[8]);
    var straightBombJump = new SamusState
    {
        Pose = SamusState.MorphBallGroundLeftPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 56,
    };
    straightBombJump.RefreshCollisionRadii(bus);
    straightBombJump.InitializeAnimation(bus);
    straightBombJump.Kinematics.YAcceleration = 0;
    straightBombJump.Kinematics.YSubacceleration = 0x4000;
    straightBombJump.RequestMorphedBombJump(2);
    SamusBombJumpMovement.Start(bus, straightBombJump);
    BombJumpMovementResult ceilingHit = SamusBombJumpMovement.Step(
        bus, ceiling, straightBombJump, nmiFrameCounter: 0);
    AssertTrue(ceilingHit.Ended && ceilingHit.Vertical is { Collided: true },
        "straight bomb jump terminates on ceiling");
    AssertTrue(ceilingHit.Horizontal is null, "straight bomb jump performs no horizontal move");
    AssertEqual((ushort)48, straightBombJump.XPosition, "straight bomb jump preserves X");
    AssertEqual((ushort)0, straightBombJump.Kinematics.YSpeed, "ceiling hit clears vertical speed");

    // A grounded pose is intentional after the native special handler: morphed setup
    // preserved it. Prove that its normal falling collision can enter the ordinary bounce
    // state instead of throwing solely because the art still names a grounded ball.
    straightBombJump.YPosition = 55;
    straightBombJump.Kinematics.YDirection = 2;
    straightBombJump.Kinematics.YSpeed = 3;
    straightBombJump.Kinematics.YSubspeed = 0;
    straightBombJump.Kinematics.YAcceleration = 0;
    straightBombJump.Kinematics.YSubacceleration = 0;
    MorphBallMovementResult bombLanding = SamusMorphBallMovement.StepGrounded(
        bus, floor, straightBombJump, nmiFrameCounter: 0);
    AssertTrue(bombLanding.Landed, "post-bomb-jump grounded art collides with floor");
    AssertTrue(!straightBombJump.ApplyMorphBallLanding(bus), "post-bomb-jump landing launches bounce");
    AssertEqual((ushort)1, straightBombJump.MorphBallBounceState, "post-bomb-jump landing enters bounce one");

    Console.WriteLine("  Morph Ball: entry, bomb jump, bombable/shootable/special reaction PLMs, bounce, and tunnel collision agree.");
}

}
