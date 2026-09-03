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

/// <summary>Aerial, Space Jump, Screw Attack, liquid, and atmosphere verification.</summary>
static void VerifySamusAerialMovement()
{
    var bus = new TestAddressSpace();

    // Minimal literal pose records for the verified right-facing route. Byte one is the
    // movement dispatcher index, byte four is the signed graphics offset, and byte six is
    // the collision radius. All values mirror the retail definitions.
    bus.WriteBytes(0x91b881, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    bus.WriteBytes(0x91b889, [0x04, 0x02, 0xff, 0x07, 0x03, 0x00, 0x13, 0x00]); // $4C
    bus.WriteBytes(0x91b891, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $4D
    bus.WriteBytes(0x91bb49, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]); // $A4
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01

    // Animation streams are byte-indexed. $4B spends one frame as a transition then FD
    // publishes $4D. $4D's first two frames support the rising-frame hold assertion below.
    // $A4 reaches F8 and publishes standing-right pose $01.
    WriteTestWord(bus, 0x91b0a6, 0xc100); // pose $4B pointer
    WriteTestWord(bus, 0x91b0a8, 0xc108); // pose $4C pointer
    WriteTestWord(bus, 0x91b0aa, 0xc110); // pose $4D pointer
    WriteTestWord(bus, 0x91b158, 0xc120); // pose $A4 pointer
    WriteTestWord(bus, 0x91b012, 0xc130); // pose $01 pointer
    bus.WriteBytes(0x91c100, [0x01, 0xfd, 0x4d]);
    bus.WriteBytes(0x91c108, [0x01, 0xfd, 0x4e]);
    bus.WriteBytes(0x91c110, [0x02, 0x03, 0x03, 0x03, 0x03, 0x50, 0xfe, 0x01]);
    bus.WriteBytes(0x91c120, [0x04, 0x02, 0xf8, 0x01]);
    bus.WriteBytes(0x91c130, [0x0a]);

    // Dry-air physics constants come from the same bank-$90 words used by production.
    WriteTestWord(bus, 0x909eb9, 0x0004);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0x0000);

    // Type two reads normal-air base $9F55 + 2*12 = $9F6D. With no direction input the
    // native handler clears the tentative acceleration again, but seed all six words so
    // the pointer and arithmetic remain real rather than relying on sparse-bus zeroes.
    WriteTestWord(bus, 0x909f6d, 0x0000);
    WriteTestWord(bus, 0x909f6f, 0x1000);
    WriteTestWord(bus, 0x909f71, 0x0001);
    WriteTestWord(bus, 0x909f73, 0x0000);
    WriteTestWord(bus, 0x909f75, 0x0000);
    WriteTestWord(bus, 0x909f77, 0x1000);

    const int width = 8;
    const int height = 8;
    var foreground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        foreground[6 * width + x] = 0x8000; // ordinary solid floor begins at Y=96
    RoomLevelData level = CreateRoom(
        width, height, foreground, new byte[foreground.Length]);

    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 77, // radius 19 will place jump-pose feet at floor Y=96
    };
    samus.ApplyOrdinaryJumpTransition(bus, SamusPoseIds.NeutralJumpTransitionRightPose);
    AssertEqual(0x0004, samus.Kinematics.YSpeed, "jump reads initial whole Y speed");
    AssertEqual(0xe000, samus.Kinematics.YSubspeed, "jump reads initial fractional Y speed");
    AssertEqual(1, samus.Kinematics.YDirection, "jump begins upward");

    // The $4B transition movement deliberately does not consume the initialized 4.E000.
    AerialMovementResult transitionFrame = SamusAerialMovement.StepNormalJump(
        bus, level, samus, (ushort)SnesButton.A, nmiFrameCounter: 0);
    AssertTrue(transitionFrame.Vertical is null, "neutral-jump transition skips vertical movement");
    AssertEqual(77, samus.YPosition, "neutral-jump transition preserves Y");
    samus.AnimateNoFx(bus);
    AssertEqual(0xfd, samus.LastAnimationDelayCommand!.Value, "neutral-jump transition reaches FD");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "FD installs neutral jump pose");
    AssertEqual(0x4d, samus.Pose, "FD target pose");

    // The first real airborne frame moves by OLD 4.E000, then stores 4.B800 after gravity.
    AerialMovementResult firstRise = SamusAerialMovement.StepNormalJump(
        bus, level, samus, (ushort)SnesButton.A, nmiFrameCounter: 1);
    AssertTrue(firstRise.Vertical is { Collided: false }, "first rise remains in air");
    AssertEqual(72, samus.YPosition, "first rise old-speed whole displacement");
    AssertEqual(0x2000, samus.Kinematics.YSubposition, "first rise old-speed fraction");
    AssertEqual(0x0004, samus.Kinematics.YSpeed, "first rise stored whole speed");
    AssertEqual(0xb800, samus.Kinematics.YSubspeed, "first rise subtracts gravity afterward");

    // Releasing jump cuts velocity before the common routine copies it. The frame has no
    // displacement, changes direction to down, and only primes 0.2800 for the next frame.
    ushort releaseY = samus.YPosition;
    ushort releaseSubY = samus.Kinematics.YSubposition;
    SamusAerialMovement.StepNormalJump(bus, level, samus, controllerInput: 0, nmiFrameCounter: 2);
    AssertEqual(2, samus.Kinematics.YDirection, "jump release starts falling");
    AssertEqual(releaseY, samus.YPosition, "jump release stationary whole Y frame");
    AssertEqual(releaseSubY, samus.Kinematics.YSubposition, "jump release stationary fractional Y frame");
    AssertEqual(0x2800, samus.Kinematics.YSubspeed, "jump release primes falling gravity");

    // Continue the native recurrence until the solid floor clips a downward displacement.
    // This is bounded well above the roughly 50 frames needed by the synthetic room.
    AerialMovementResult result = default;
    StepUntil(
        () => result.Landed,
        frame => result = SamusAerialMovement.StepNormalJump(
            bus, level, samus, 0, unchecked((ushort)(frame + 3))),
        maximumFrames: 197,
        context: "neutral jump solid-floor landing");
    AssertEqual(77, samus.YPosition, "aerial radius rests at floor before pose expansion");

    samus.ApplyAerialLanding(bus, wasSpinning: false);
    AssertEqual(0xa4, samus.Pose, "normal right-facing landing pose");
    AssertEqual(75, samus.YPosition, "landing radius expansion keeps feet fixed");
    AssertEqual(0, samus.Kinematics.YDirection, "landing clears vertical direction");

    // Four ticks plus two ticks reach $F8 at byte index two; its operand returns to $01.
    for (int tick = 0; tick < 6; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual(0xf8, samus.LastAnimationDelayCommand!.Value, "landing reaches F8");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "landing F8 transition applies");
    AssertEqual(0x01, samus.Pose, "landing animation returns to standing");

    // Aerial mode two accelerates because $90:9B22 tests only bit zero. This guards the
    // counterintuitive distinction from the grounded deceleration-allowed routine.
    var aerialSpeed = new SamusHorizontalSpeedState { AccelerationMode = 2 };
    aerialSpeed.SelectNormalAirSpeedTable();
    AerialBaseSpeedResult accelerated =
        aerialSpeed.CalculateBaseSpeedDecelerationDisallowed(bus, movementType: SamusMovementType.NormalJumping);
    AssertEqual(0x00001000u, accelerated.Speed, "aerial mode two accelerates");
    AssertTrue(!accelerated.ReachedMaximum, "aerial sub-cap call clears carry");

    // Mirror the launch through the left-facing spin-jump route. This checks that mode two
    // uses the pose direction normally, and that the type-three speed entry (not running's
    // type-one entry) supplies the in-air cap/acceleration.
    bus.WriteBytes(0x91b6f1, [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]); // $19
    bus.WriteBytes(0x91b6f9, [0x04, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]); // $1A
    WriteTestWord(bus, 0x91b042, 0xc138);
    WriteTestWord(bus, 0x91b044, 0xc140);
    bus.WriteBytes(0x91c138, [0x03, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03]);
    bus.WriteBytes(0x91c140, [0x03, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0xfe, 0x08]);
    WriteTestWord(bus, 0x909f79, 0x0000);
    WriteTestWord(bus, 0x909f7b, 0x2000);
    WriteTestWord(bus, 0x909f7d, 0x0001);
    WriteTestWord(bus, 0x909f7f, 0x6000);
    WriteTestWord(bus, 0x909f81, 0x0000);
    WriteTestWord(bus, 0x909f83, 0x1000);
    var spinLeft = new SamusState
    {
        Pose = SamusPoseIds.MovingLeftNormalPose,
        XPosition = 48,
        YPosition = 77,
    };
    spinLeft.HorizontalSpeed.BaseSpeed = 1;
    spinLeft.ApplyOrdinaryJumpTransition(bus, SamusPoseIds.SpinJumpLeftPose);
    AerialMovementResult spinFrame = SamusAerialMovement.StepSpinJump(
        bus,
        level,
        spinLeft,
        (ushort)(SnesButton.Left | SnesButton.A),
        nmiFrameCounter: 0);
    AssertEqual(-0x00012000, spinFrame.Horizontal.AcceptedDisplacement, "left spin jump displacement");
    AssertEqual(2, spinLeft.HorizontalSpeed.AccelerationMode, "spin jump selects aerial mode two");
    AssertEqual(46, spinLeft.XPosition, "left spin jump whole X");
    AssertEqual(0xe000, spinLeft.Kinematics.XSubposition, "left spin jump fractional X");

    // Turning input handler `$91:8142` uses the ordinary transition table. The reported
    // retail match `$26 -> $19` is therefore a real spin-jump launch, not an unsupported
    // turning-only side effect. Lock down both mirrors so the runtime cannot regress to a
    // an open-dispatch crash when Jump is pressed during either one-frame ground turn.
    var turnJumpRight = new SamusState { Pose = SamusPoseIds.TurningLeftToRightPose };
    turnJumpRight.ApplyOrdinaryJumpTransition(bus, SamusPoseIds.SpinJumpRightPose);
    AssertEqual(SamusPoseIds.SpinJumpRightPose, turnJumpRight.Pose,
        "left-to-right ground turn accepts spin jump");
    AssertEqual(1, turnJumpRight.Kinematics.YDirection,
        "left-to-right turn jump launches upward");

    var turnJumpLeft = new SamusState { Pose = SamusPoseIds.TurningRightToLeftPose };
    turnJumpLeft.ApplyOrdinaryJumpTransition(bus, SamusPoseIds.SpinJumpLeftPose);
    AssertEqual(SamusPoseIds.SpinJumpLeftPose, turnJumpLeft.Pose,
        "right-to-left ground turn accepts spin jump");
    AssertEqual(1, turnJumpLeft.Kinematics.YDirection,
        "right-to-left turn jump launches upward");

    // Releasing horizontal direction before the fresh Jump edge selects the neutral-jump
    // records `$4B/$4C`, not spin `$19/$1A`. These are the exact mirrors exercised by a
    // player who turns in place and immediately jumps to aim a diagonal shot.
    var neutralTurnJumpRight = new SamusState { Pose = SamusPoseIds.TurningLeftToRightPose };
    neutralTurnJumpRight.ApplyOrdinaryJumpTransition(
        bus,
        SamusPoseIds.NeutralJumpTransitionRightPose);
    AssertEqual(SamusPoseIds.NeutralJumpTransitionRightPose, neutralTurnJumpRight.Pose,
        "left-to-right ground turn accepts neutral jump");
    AssertEqual(1, neutralTurnJumpRight.Kinematics.YDirection,
        "left-to-right neutral turn jump launches upward");

    var neutralTurnJumpLeft = new SamusState { Pose = SamusPoseIds.TurningRightToLeftPose };
    neutralTurnJumpLeft.ApplyOrdinaryJumpTransition(
        bus,
        SamusPoseIds.NeutralJumpTransitionLeftPose);
    AssertEqual(SamusPoseIds.NeutralJumpTransitionLeftPose, neutralTurnJumpLeft.Pose,
        "right-to-left ground turn accepts neutral jump");
    AssertEqual(1, neutralTurnJumpLeft.Kinematics.YDirection,
        "right-to-left neutral turn jump launches upward");

    // Aimed standing turns do not become inert until their three-frame stream reaches
    // `$F8`. They keep using `$91:8142`, so Jump may interrupt every one of the six records
    // with the same spin/neutral pair as `$25/$26`. Exercise both outputs for both facings;
    // `$8D -> $1A` is the live Climb route which first exposed this omitted family.
    (byte Source, byte SpinTarget, byte NeutralTarget)[] aimedTurnJumps =
    [
        (SamusPoseIds.TurningRightToLeftAimUpPose,
            SamusPoseIds.SpinJumpLeftPose, SamusPoseIds.NeutralJumpTransitionLeftPose),
        (SamusPoseIds.TurningLeftToRightAimUpPose,
            SamusPoseIds.SpinJumpRightPose, SamusPoseIds.NeutralJumpTransitionRightPose),
        (SamusPoseIds.TurningRightToLeftAimDiagonalDownPose,
            SamusPoseIds.SpinJumpLeftPose, SamusPoseIds.NeutralJumpTransitionLeftPose),
        (SamusPoseIds.TurningLeftToRightAimDiagonalDownPose,
            SamusPoseIds.SpinJumpRightPose, SamusPoseIds.NeutralJumpTransitionRightPose),
        (SamusPoseIds.TurningRightToLeftAimDiagonalUpPose,
            SamusPoseIds.SpinJumpLeftPose, SamusPoseIds.NeutralJumpTransitionLeftPose),
        (SamusPoseIds.TurningLeftToRightAimDiagonalUpPose,
            SamusPoseIds.SpinJumpRightPose, SamusPoseIds.NeutralJumpTransitionRightPose),
    ];
    foreach ((byte source, byte spinTarget, byte neutralTarget) in aimedTurnJumps)
    {
        var spin = new SamusState { Pose = source };
        spin.ApplyOrdinaryJumpTransition(bus, spinTarget);
        AssertEqual(spinTarget, spin.Pose,
            $"aimed standing turn ${source:X2} accepts facing spin jump");
        AssertEqual(1, spin.Kinematics.YDirection,
            $"aimed standing turn ${source:X2} spin launches upward");

        var neutral = new SamusState { Pose = source };
        neutral.ApplyOrdinaryJumpTransition(bus, neutralTarget);
        AssertEqual(neutralTarget, neutral.Pose,
            $"aimed standing turn ${source:X2} accepts facing neutral jump");
    }
    AssertThrows<InvalidOperationException>(
        () => new SamusState { Pose = SamusPoseIds.TurningRightToLeftCrouchingPose }
            .ApplyOrdinaryJumpTransition(bus, SamusPoseIds.SpinJumpLeftPose),
        "crouched turn remains outside direct ordinary-jump initializer");

    // Walking off a ledge is the movement-type-six entry point. $91:E8F2 selects pose $2A
    // from the old left-facing direction and command five begins with a stationary falling
    // frame before gravity produces displacement. Landing from type six uses normal $A5.
    bus.WriteBytes(0x91b779, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]); // $2A
    bus.WriteBytes(0x91bb51, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]); // $A5
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]); // $02
    WriteTestWord(bus, 0x91b064, 0xc150);
    WriteTestWord(bus, 0x91b15a, 0xc160);
    WriteTestWord(bus, 0x91b014, 0xc170);
    bus.WriteBytes(0x91c150, [0x05, 0x04, 0x04, 0xfe, 0x01, 0x06, 0x10, 0xfe, 0x01]);
    bus.WriteBytes(0x91c160, [0x04, 0x02, 0xf8, 0x02]);
    bus.WriteBytes(0x91c170, [0x0a]);
    WriteTestWord(bus, 0x909f9d, 0x0000); // type-six entry at $9F55 + 6*12
    WriteTestWord(bus, 0x909f9f, 0x1000);
    WriteTestWord(bus, 0x909fa1, 0x0001);
    WriteTestWord(bus, 0x909fa3, 0x0000);
    WriteTestWord(bus, 0x909fa5, 0x0000);
    WriteTestWord(bus, 0x909fa7, 0x1000);
    var fallLeft = new SamusState
    {
        Pose = SamusPoseIds.FacingLeftNormalPose,
        XPosition = 48,
        YPosition = 77,
    };
    fallLeft.ApplyWalkedOffFloorTransition(bus, SamusPoseIds.FallingLeftPose);
    AssertEqual(0x2a, fallLeft.Pose, "walk-off chooses left falling pose");
    AerialMovementResult firstFall = SamusAerialMovement.StepFalling(
        bus, level, fallLeft, controllerInput: 0, nmiFrameCounter: 0);
    AssertEqual(0, firstFall.Vertical!.Value.AcceptedDisplacement, "walk-off starts with stationary fall frame");
    AssertEqual(0x2800, fallLeft.Kinematics.YSubspeed, "first fall frame primes gravity");
    AerialMovementResult fallResult = default;
    StepUntil(
        () => fallResult.Landed,
        frame => fallResult = SamusAerialMovement.StepFalling(
            bus, level, fallLeft, 0, unchecked((ushort)(frame + 1))),
        maximumFrames: 199,
        context: "left falling pose floor landing");
    fallLeft.ApplyAerialLanding(bus, wasSpinning: false);
    AssertEqual(0xa5, fallLeft.Pose, "left fall selects normal landing pose");

    // The standing transition table is shared by `$A4/$A5`, so Jump may interrupt normal
    // landing before its `$F8` fallback executes. Exercise both mirrors through the same
    // public initializer used by the runtime; this locks down actual jump velocity/radius
    // setup rather than merely accepting the target pose byte in the dispatcher.
    var interruptRightLanding = new SamusState { Pose = SamusPoseIds.NormalLandingRightPose };
    interruptRightLanding.ApplyOrdinaryJumpTransition(
        bus, SamusPoseIds.NeutralJumpTransitionRightPose);
    AssertEqual(SamusPoseIds.NeutralJumpTransitionRightPose, interruptRightLanding.Pose,
        "right normal landing accepts fresh jump");
    AssertEqual(1, interruptRightLanding.Kinematics.YDirection,
        "right landing jump starts upward");

    var interruptLeftLanding = new SamusState { Pose = SamusPoseIds.NormalLandingLeftPose };
    interruptLeftLanding.ApplyOrdinaryJumpTransition(
        bus, SamusPoseIds.NeutralJumpTransitionLeftPose);
    AssertEqual(SamusPoseIds.NeutralJumpTransitionLeftPose, interruptLeftLanding.Pose,
        "left normal landing accepts fresh jump");
    AssertEqual(1, interruptLeftLanding.Kinematics.YDirection,
        "left landing jump starts upward");

    var interruptRightSpinLanding = new SamusState
    {
        Pose = SamusPoseIds.SpinLandingRightPose,
    };
    interruptRightSpinLanding.ApplyOrdinaryJumpTransition(
        bus, SamusPoseIds.NeutralJumpTransitionRightPose);
    AssertEqual(SamusPoseIds.NeutralJumpTransitionRightPose, interruptRightSpinLanding.Pose,
        "right spin landing accepts fresh neutral jump");
    AssertEqual(1, interruptRightSpinLanding.Kinematics.YDirection,
        "right spin-landing jump starts upward");

    var interruptLeftSpinLanding = new SamusState
    {
        Pose = SamusPoseIds.SpinLandingLeftPose,
    };
    interruptLeftSpinLanding.ApplyOrdinaryJumpTransition(
        bus, SamusPoseIds.NeutralJumpTransitionLeftPose);
    AssertEqual(SamusPoseIds.NeutralJumpTransitionLeftPose, interruptLeftSpinLanding.Pose,
        "left spin landing accepts fresh neutral jump");
    AssertEqual(1, interruptLeftSpinLanding.Kinematics.YDirection,
        "left spin-landing jump starts upward");

    // The engine alternates $94:959E left-to-right and $94:95F5 right-to-left vertical
    // scans on successive NMIs. Their $1A counters run in opposite numerical directions,
    // but the physical result must be identical. Exercise every square-slope shape,
    // orientation, and grazed column with exactly one candidate block; this is the case
    // that previously made a fall beside a wall report landing every other frame.
    for (int shape = 0; shape < 5; shape++)
    {
        for (int orientation = 0; orientation < 4; orientation++)
        {
            for (int solidBlockX = 1; solidBlockX <= 4; solidBlockX++)
            {
                var squareForeground = new ushort[width * height];
                var squareBehavior = new byte[width * height];
                squareForeground[3 * width + solidBlockX] = 0x1000;
                squareBehavior[3 * width + solidBlockX] = unchecked((byte)(
                    shape | (orientation << 6)));
                RoomLevelData squareRoom = CreateRoom(
                    width,
                    height,
                    squareForeground,
                    squareBehavior);

                for (ushort x = 20; x <= 68; x += 4)
                {
                    SamusKinematicsState leftToRight = new()
                    {
                        XPosition = x,
                        YPosition = 36,
                        XRadius = 12,
                        YRadius = 12,
                    };
                    SamusKinematicsState rightToLeft = new()
                    {
                        XPosition = x,
                        YPosition = 36,
                        XRadius = 12,
                        YRadius = 12,
                    };
                    BlockMoveResult ltr = SamusBlockCollision.MoveVertical(
                        bus, squareRoom, leftToRight, 1 << 16, scanLeftToRight: true);
                    BlockMoveResult rtl = SamusBlockCollision.MoveVertical(
                        bus, squareRoom, rightToLeft, 1 << 16, scanLeftToRight: false);
                    AssertEqual(ltr.Collided, rtl.Collided,
                        $"square slope {shape}/{orientation} X={x} collision scan parity");
                    AssertEqual(ltr.AcceptedDisplacement, rtl.AcceptedDisplacement,
                        $"square slope {shape}/{orientation} X={x} displacement scan parity");
                }
            }
        }
    }

    Console.WriteLine("  Samus aerial: FD launch, exact 16.16 arc, jump cut, floor landing, radius, F8, and alternating square-slope scans agree.");
}

/// <summary>
/// Exercises the equipment-aware spin initializer, every edge of the dry-air Space Jump
/// velocity window, Screw Attack contact/wall frames, and both bank-$91/bank-$9B palette
/// indirections. These checks deliberately keep Space Jump physics independent from the
/// visible Screw Attack pose used when both retail item bits are equipped.
/// </summary>
static void VerifySamusSpaceJumpAndScrewAttack()
{
    var bus = new TestAddressSpace();

    // All six stable spin records share movement type three and radius twelve. The generic
    // `$19/$1A` records are transition-table outputs; the four equipment records are the
    // actual bodies selected by `$91:F624` after that table lookup.
    foreach ((byte pose, byte direction) in new (byte, byte)[]
    {
        (SamusPoseIds.SpinJumpRightPose, 8),
        (SamusPoseIds.SpinJumpLeftPose, 4),
        (SamusPoseIds.SpaceJumpRightPose, 8),
        (SamusPoseIds.SpaceJumpLeftPose, 4),
        (SamusPoseIds.ScrewAttackRightPose, 8),
        (SamusPoseIds.ScrewAttackLeftPose, 4),
    })
    {
        WritePoseDefinition(bus, pose, [direction, 3, 0xff, 0xff, 0, 0, 12, 0]);
        ushort stream = unchecked((ushort)(0xc000 + pose * 0x20));
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        for (int frame = 0; frame < 31; frame++)
            bus.WriteByte(0x910000 | unchecked((ushort)(stream + frame)), 4);
    }

    // Running sources and spin-landing endpoints are sufficient to prove equipment
    // substitution and Screw palette restoration without mocking pose metadata in code.
    WritePoseDefinition(bus, SamusPoseIds.MovingRightNormalPose,
        [8, 1, 0xff, 2, 0, 0, 21, 0]);
    WritePoseDefinition(bus, SamusPoseIds.MovingLeftNormalPose,
        [4, 1, 0xff, 7, 0, 0, 21, 0]);
    WritePoseDefinition(bus, SamusPoseIds.SpinLandingRightPose,
        [8, 0, 0xff, 2, 0, 0, 21, 0]);
    WritePoseDefinition(bus, SamusPoseIds.SpinLandingLeftPose,
        [4, 0, 0xff, 7, 0, 0, 21, 0]);
    WritePoseDefinition(bus, SamusPoseIds.NormalJumpGunExtendedRightPose,
        [8, 2, 0xff, 2, 0, 0, 24, 0]);
    WritePoseDefinition(bus, SamusPoseIds.WallJumpRightPose,
        [8, 0x14, SamusPoseIds.SpinJumpRightPose, 0xff, 8, 0, 19, 0]);
    WritePoseDefinition(bus, SamusPoseIds.NormalJumpAimDownLeftPose,
        [4, 2, 0xff, 5, 0, 0, 10, 0]);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.SpinLandingRightPose * 2, 0xc800);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.SpinLandingLeftPose * 2, 0xc810);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.NormalJumpGunExtendedRightPose * 2, 0xc820);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.WallJumpRightPose * 2, 0xc828);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.NormalJumpAimDownLeftPose * 2, 0xc830);
    bus.WriteByte(0x91c800, 4);
    bus.WriteByte(0x91c810, 4);
    bus.WriteByte(0x91c820, 4);
    bus.WriteByte(0x91c828, 4);
    bus.WriteByte(0x91c830, 4);

    // Dry-air Samus_InitJump and gravity words. The type-three horizontal record is zeroed
    // intentionally so the assertions isolate the vertical 8.8 gate from X acceleration.
    WriteTestWord(bus, 0x909eb9, 4);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0);
    for (int byteOffset = 0; byteOffset < SpeedTableEntry.ByteCount; byteOffset += 2)
        WriteTestWord(bus, 0x909f79 + byteOffset, 0);

    RoomLevelData empty = CreateEmptyRoom(16, 16);

    var spaceLaunch = new SamusState
    {
        Pose = SamusPoseIds.MovingRightNormalPose,
        EquippedItems = 0x0200,
        XPosition = 128,
        YPosition = 128,
    };
    spaceLaunch.ApplyOrdinaryJumpTransition(bus, SamusPoseIds.SpinJumpRightPose);
    AssertEqual(SamusPoseIds.SpaceJumpRightPose, spaceLaunch.Pose,
        "Space Jump substitutes right spin pose");

    var screwLaunch = new SamusState
    {
        Pose = SamusPoseIds.MovingLeftNormalPose,
        EquippedItems = 0x0208,
        XPosition = 128,
        YPosition = 128,
    };
    screwLaunch.ApplyOrdinaryJumpTransition(bus, SamusPoseIds.SpinJumpLeftPose);
    AssertEqual(SamusPoseIds.ScrewAttackLeftPose, screwLaunch.Pose,
        "Screw Attack takes priority over Space Jump pose");

    // `$81/$82` have their own retail input tables, so an opposite-direction match may
    // publish the specialized target directly rather than generic `$19/$1A`. The common
    // F624 initializer must accept that record, preserve Screw's equipment priority, and
    // still start a direction change at animation frame one.
    screwLaunch.ApplySpinJumpDirectionTransition(bus, SamusPoseIds.ScrewAttackRightPose);
    AssertEqual(SamusPoseIds.ScrewAttackRightPose, screwLaunch.Pose,
        "direct Screw table target preserves equipped art");
    AssertEqual(1, screwLaunch.AnimationFrame,
        "direct Screw direction transition starts at frame one");

    // Fire from a spin uses `$19/$1B/$81 -> $13`, expanding radius 12 -> 24 through
    // changed-pose collision but preserving the live jump arc. `$91:F543` also selects
    // mode two from residual extra-run speed and publishes the target's shot direction.
    var spinFire = new SamusState
    {
        Pose = SamusPoseIds.ScrewAttackRightPose,
        EquippedItems = SamusEquipmentFlags.ScrewAttack.ToNativeWord(),
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            XRadius = 5,
            YRadius = 12,
            YDirection = 1,
            YSpeed = 3,
            YSubspeed = 0x4567,
        },
    };
    spinFire.HorizontalSpeed.ExtraRunSubspeed = 1;
    AssertTrue(spinFire.TryApplySpinToNormalJumpFireTransition(
        bus,
        empty,
        SamusPoseIds.NormalJumpGunExtendedRightPose,
        nmiFrameCounter: 0,
        controllerNewInput: (ushort)SnesButton.X),
        "spin Fire body expansion fits empty room");
    AssertEqual(SamusPoseIds.NormalJumpGunExtendedRightPose, spinFire.Pose,
        "spin Fire selects gun-extended normal jump");
    AssertEqual(24, spinFire.Kinematics.YRadius,
        "spin Fire expands to normal-jump radius");
    AssertEqual(3, spinFire.Kinematics.YSpeed,
        "spin Fire preserves whole vertical speed");
    AssertEqual(0x4567, spinFire.Kinematics.YSubspeed,
        "spin Fire preserves fractional vertical speed");
    AssertEqual(2, spinFire.HorizontalSpeed.AccelerationMode,
        "spin Fire retains extra-run aerial acceleration");
    AssertEqual(0x8002, spinFire.PoseTransitionShotDirection,
        "spin Fire publishes target shot direction");
    AssertTrue(spinFire.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
        "leaving Screw Attack requests normal suit palette");

    // `$91:A9EC` is the retail `$83` wall-jump input table. Its Shot record is literally
    // `$0000,$0040,$0013`; `$91:F404` then sends that target through the same changed-pose,
    // type-two initializer used by a spinning source. Prove the wall route independently so
    // the exhaustive runtime dispatcher cannot silently regress to spin-only admission.
    var wallFire = new SamusState
    {
        Pose = SamusPoseIds.WallJumpRightPose,
        EquippedItems = SamusEquipmentFlags.ScrewAttack.ToNativeWord(),
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            XRadius = 5,
            YRadius = 19,
            YDirection = 1,
            YSpeed = 4,
            YSubspeed = 0x2345,
        },
    };
    AssertTrue(wallFire.TryApplySpinOrWallJumpToNormalJumpTransition(
        bus,
        empty,
        SamusPoseIds.NormalJumpGunExtendedRightPose,
        nmiFrameCounter: 0,
        controllerNewInput: (ushort)SnesButton.X),
        "wall-jump Shot body expansion fits empty room");
    AssertEqual(SamusPoseIds.NormalJumpGunExtendedRightPose, wallFire.Pose,
        "wall-jump Shot selects cartridge target $13");
    AssertEqual(24, wallFire.Kinematics.YRadius,
        "wall-jump Shot expands to normal-jump radius");
    AssertEqual(4, wallFire.Kinematics.YSpeed,
        "wall-jump Shot preserves whole vertical speed");
    AssertEqual(0x2345, wallFire.Kinematics.YSubspeed,
        "wall-jump Shot preserves fractional vertical speed");
    AssertEqual(0x8002, wallFire.PoseTransitionShotDirection,
        "wall-jump Shot publishes target shot direction");
    AssertTrue(wallFire.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
        "leaving Screw wall-jump requests normal suit palette");

    // Down during a left spin selects compact normal-jump pose `$18`. It enters the same
    // `$91:F543` initializer as Fire but shrinks radius 12 -> 10, preserves the running jump
    // arc, and does not invent the `$8000` projectile bridge without a fresh Shoot edge.
    var spinAimDown = new SamusState
    {
        Pose = SamusPoseIds.SpinJumpLeftPose,
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            XRadius = 5,
            YRadius = 12,
            YDirection = 1,
            YSpeed = 2,
            YSubspeed = 0x3456,
        },
    };
    AssertTrue(spinAimDown.TryApplySpinOrWallJumpToNormalJumpTransition(
        bus,
        empty,
        SamusPoseIds.NormalJumpAimDownLeftPose,
        nmiFrameCounter: 0,
        controllerNewInput: 0),
        "spin Down body contraction fits empty room");
    AssertEqual(SamusPoseIds.NormalJumpAimDownLeftPose, spinAimDown.Pose,
        "spin Down selects compact left normal jump");
    AssertEqual(10, spinAimDown.Kinematics.YRadius,
        "spin Down contracts to straight-down radius");
    AssertEqual(128, spinAimDown.YPosition,
        "spin Down contraction does not move the body center");
    AssertEqual(2, spinAimDown.Kinematics.YSpeed,
        "spin Down preserves whole vertical speed");
    AssertEqual(0x3456, spinAimDown.Kinematics.YSubspeed,
        "spin Down preserves fractional vertical speed");
    AssertEqual(0, spinAimDown.PoseTransitionShotDirection,
        "spin Down without Shoot does not publish a projectile bridge");

    static SamusState CreateFallingSpin(byte pose, ushort items, ushort speed, ushort subspeed) => new()
    {
        Pose = pose,
        EquippedItems = items,
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            YDirection = 2,
            YSpeed = speed,
            YSubspeed = subspeed,
            YAcceleration = 0,
            YSubacceleration = 0x2800,
            XRadius = 5,
            YRadius = 12,
        },
    };

    // `$0280` is inclusive. A fresh edge restarts at 4.E000, moves upward by that OLD
    // magnitude, then stores 4.B800 after the shared spin routine subtracts gravity.
    SamusState minimum = CreateFallingSpin(
        SamusPoseIds.SpaceJumpRightPose, 0x0200, speed: 2, subspeed: 0x8000);
    SamusAerialMovement.StepSpinJump(
        bus,
        empty,
        minimum,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: (ushort)SnesButton.A);
    AssertEqual(1, minimum.Kinematics.YDirection, "Space Jump minimum velocity restarts upward");
    AssertEqual(4, minimum.Kinematics.YSpeed, "Space Jump restart whole speed");
    AssertEqual(0xb800, minimum.Kinematics.YSubspeed, "Space Jump restart applies gravity after movement");
    AssertEqual(123, minimum.YPosition, "Space Jump restart moves by old 4.E000 magnitude");

    SamusState below = CreateFallingSpin(
        SamusPoseIds.SpaceJumpRightPose, 0x0200, speed: 2, subspeed: 0x7fff);
    SamusAerialMovement.StepSpinJump(
        bus, empty, below, (ushort)SnesButton.A, 0, (ushort)SnesButton.A);
    AssertEqual(2, below.Kinematics.YDirection, "Space Jump rejects velocity $027F");

    SamusState maximum = CreateFallingSpin(
        SamusPoseIds.SpaceJumpRightPose, 0x0200, speed: 5, subspeed: 0);
    SamusAerialMovement.StepSpinJump(
        bus, empty, maximum, (ushort)SnesButton.A, 0, (ushort)SnesButton.A);
    AssertEqual(2, maximum.Kinematics.YDirection, "Space Jump maximum $0500 is exclusive");

    SamusState heldOnly = CreateFallingSpin(
        SamusPoseIds.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    SamusAerialMovement.StepSpinJump(bus, empty, heldOnly, (ushort)SnesButton.A, 0);
    AssertEqual(2, heldOnly.Kinematics.YDirection, "Space Jump requires a fresh Jump edge");

    // Both item bits retain Space Jump physics while Screw Attack owns art and damage.
    SamusState screwRepeat = CreateFallingSpin(
        SamusPoseIds.ScrewAttackRightPose, 0x0208, speed: 3, subspeed: 0);
    SamusAerialMovement.StepSpinJump(
        bus, empty, screwRepeat, (ushort)SnesButton.A, 0, (ushort)SnesButton.A);
    AssertEqual(1, screwRepeat.Kinematics.YDirection,
        "Screw Attack with Space Jump repeats upward");
    AssertEqual(3, screwRepeat.HorizontalSpeed.ContactDamageIndex,
        "Screw Attack republishes contact damage index three");

    // `$84:CE91-$CEA3` explicitly admits poses `$81/$82`, independently of the boost-stage
    // branch above them. Place a BTS-4 permanent 1x1 block under a falling Screw body so the
    // shared vertical movement must allocate the room PLM and continue through new air.
    var screwBombForeground = new ushort[16 * 16];
    var screwBombBts = new byte[screwBombForeground.Length];
    int screwBombIndex = 8 * 16 + 8;
    screwBombForeground[screwBombIndex] = 0xf123;
    screwBombBts[screwBombIndex] = 4;
    RoomLevelData screwBombLevel = CreateRoom(
        16, 16, screwBombForeground, screwBombBts);
    var screwBombPlms = new RoomPlmSystem();
    SamusState screwBombSamus = CreateFallingSpin(
        SamusPoseIds.ScrewAttackRightPose, 0x0008, speed: 3, subspeed: 0);
    AerialMovementResult screwBombFrame = SamusAerialMovement.StepSpinJump(
        bus,
        screwBombLevel,
        screwBombSamus,
        controllerInput: 0,
        nmiFrameCounter: 0,
        controllerNewInput: 0,
        plms: screwBombPlms);
    AssertTrue(screwBombFrame.Vertical is { Collided: false },
        "Screw collision-bomb setup returns carry clear and preserves vertical travel");
    AssertEqual(screwBombIndex,
        screwBombFrame.Vertical!.Value.BrokenBombBlock!.Value.Index,
        "Screw vertical collision publishes the exact BTS-4 block");
    AssertEqual(0x0123,
        screwBombLevel.GetCollisionBlockByIndex(screwBombIndex).LevelWord,
        "Screw setup clears only the collision nibble before the PLM handler");
    AssertEqual(1, screwBombPlms.ActiveCount,
        "Screw pose installs the BTS-4 bank-$84 lifecycle in the room owner");

    // A fully charged beam makes ordinary/Space-Jump spin damaging only in dry physics.
    // Full submersion suppresses that contact mode and, on animation frames zero/eight's
    // final tick, emits the literal library-one sound $2F instead.
    SamusState chargedSpin = CreateFallingSpin(
        SamusPoseIds.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    chargedSpin.ProjectileFlareCounter = 0x003c;
    SamusAerialMovement.StepSpinJump(bus, empty, chargedSpin, 0, 0, 0);
    AssertEqual(4, chargedSpin.HorizontalSpeed.ContactDamageIndex,
        "fully charged dry spin publishes contact damage index four");

    SamusState submergedSpin = CreateFallingSpin(
        SamusPoseIds.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    submergedSpin.ProjectileFlareCounter = 0x003c;
    submergedSpin.InitializeAnimation(bus, initialFrame: 0);
    for (int tick = 0; tick < 3; tick++)
        submergedSpin.AnimateNoFx(bus);
    submergedSpin.LiquidPhysics.ConfigureWater(surfaceY: 100);
    submergedSpin.LiquidPhysics.BeginFrameSoundRequests();
    SamusAerialMovement.StepSpinJump(bus, empty, submergedSpin, 0, 0, 0);
    AssertEqual(0, submergedSpin.HorizontalSpeed.ContactDamageIndex,
        "full liquid physics suppresses charged-spin contact damage");
    AssertEqual(1, submergedSpin.LiquidPhysics.SoundRequests.Count,
        "underwater Space Jump sound frame publishes once");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x2f), 6), submergedSpin.LiquidPhysics.SoundRequests[0],
        "underwater Space Jump uses library-one sound $2F max six");

    screwRepeat.AnimationFrame = 4;
    screwRepeat.ApplyWallContactAnimationRewind();
    AssertEqual(0x1a, screwRepeat.AnimationFrame,
        "early Screw wall contact rewinds to frame 26");
    minimum.AnimationFrame = 4;
    minimum.ApplyWallContactAnimationRewind();
    AssertEqual(0x0a, minimum.AnimationFrame,
        "Space Jump wall contact uses ordinary frame 10 rewind");

    // Three suit-list entries exist in retail; this fixture exercises Power Suit offset
    // zero and gives all six Screw frames unique first colors to prove wrapping order.
    WriteTestWord(bus, 0x91d727, 0x9400);
    WriteTestWord(bus, 0x9b9400, 0x0111);
    WriteTestWord(bus, 0x91da4a, 0xd000);
    for (int frame = 0; frame < 6; frame++)
    {
        ushort palette = unchecked((ushort)(0xe000 + frame * 0x20));
        WriteTestWord(bus, 0x91d000 + frame * 2, palette);
        WriteTestWord(bus, 0x9b0000 | palette, unchecked((ushort)(0x1200 + frame)));
    }

    var palettes = new SamusHorizontalSpeedState();
    var cgram = new SnesCgram();
    AssertTrue(palettes.UpdateSpeedBoosterPalette(
        bus, cgram, movementType: SamusMovementType.SpinJumping, animationFrame: 1, equippedItems: 0x0008),
        "early Screw frame copies normal suit palette");
    AssertEqual(0x0111, cgram.Colors[192], "early Screw frame normal palette");
    for (int frame = 0; frame < 6; frame++)
    {
        AssertTrue(palettes.UpdateSpeedBoosterPalette(
            bus, cgram, movementType: SamusMovementType.SpinJumping, animationFrame: 0x1b, equippedItems: 0x0008),
            $"Screw palette frame {frame} copies");
        AssertEqual(unchecked((ushort)(0x1200 + frame)), cgram.Colors[192],
            $"Screw palette frame {frame} ROM color");
    }
    AssertEqual(0, palettes.SpecialPaletteFrame, "six Screw palettes wrap to offset zero");

    // A Screw landing requests the same normal palette reload performed by `$91:F433`.
    screwRepeat.ApplyAerialLanding(bus, wasSpinning: true);
    AssertEqual(SamusPoseIds.SpinLandingRightPose, screwRepeat.Pose, "Screw Attack lands through spin landing");
    AssertTrue(screwRepeat.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
        "Screw Attack landing requests normal palette restore");

    Console.WriteLine(
        "  Space Jump/Screw Attack: pose priority, wall-jump Shot exit, repeat window, collision PLMs, charged/Screw damage, underwater sound, palette cycle, and landing agree.");
}

/// <summary>
/// Exercises the deliberately different top/bottom/bottom-minus-one liquid boundaries,
/// all three ROM jump/gravity/X-table selections, Gravity Suit bypass, submerged dash
/// behavior, and the water-specific Space Jump velocity window.
/// </summary>
static void VerifySamusLiquidPhysics()
{
    var bus = new TestAddressSpace();

    // Seed exactly the adjacent word tables read by `$90:98BC/$90:9C5B`. Distinct values
    // make a wrong byte offset or accidental host constant immediately observable.
    ushort[] launchWhole = [4, 1, 2];
    ushort[] launchFraction = [0xe000, 0xc000, 0xc000];
    ushort[] hiWhole = [6, 2, 3];
    ushort[] hiFraction = [0, 0x8000, 0x8000];
    ushort[] gravityFraction = [0x1c00, 0x0800, 0x0900];
    for (int medium = 0; medium < 3; medium++)
    {
        WriteTestWord(bus, 0x909eb9 + medium * 2, launchWhole[medium]);
        WriteTestWord(bus, 0x909ebf + medium * 2, launchFraction[medium]);
        WriteTestWord(bus, 0x909ec5 + medium * 2, hiWhole[medium]);
        WriteTestWord(bus, 0x909ecb + medium * 2, hiFraction[medium]);
        WriteTestWord(bus, 0x909ea1 + medium * 2, gravityFraction[medium]);
        WriteTestWord(bus, 0x909ea7 + medium * 2, 0);
    }

    var sample = new SamusState { XPosition = 64, YPosition = 100 };
    sample.Kinematics.YRadius = 12; // top 88, bottom 112
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111);
    AssertEqual(SamusLiquidPhysicsState.Water,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "water surface one pixel above bottom affects movement");
    AssertTrue(sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "palette liquid gate sees water above the bottom boundary");
    AssertTrue(!sample.LiquidPhysics.IsTopBoundarySubmerged(sample),
        "partially submerged body leaves top above water");

    sample.LiquidPhysics.ConfigureWater(surfaceY: 112);
    AssertEqual(SamusLiquidPhysicsState.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "liquid equality is not submerged");
    AssertTrue(!sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "palette liquid gate treats surface equality as dry");
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111, liquidOptions: 4);
    AssertEqual(SamusLiquidPhysicsState.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "water option bit two disables physics");
    AssertTrue(!sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "palette liquid gate also honors disabled-water option bit");

    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111);
    AssertEqual(SamusLiquidPhysicsState.LavaAcid,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "negative general FX Y selects lava/acid surface");
    AssertTrue(sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "palette liquid gate falls through negative FX Y to lava/acid surface");
    sample.EquippedItems = SamusEquipmentFlags.GravitySuit.ToNativeWord();
    AssertEqual(SamusLiquidPhysicsState.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "Gravity Suit bypasses liquid movement physics");
    AssertTrue(sample.LiquidPhysics.IsBottomBoundarySubmerged(sample),
        "raw palette boundary probe remains submerged before its Gravity exemption");

    // Normal and Hi-Jump launch tables are orthogonal to medium selection. Gravity Suit
    // forces the air entry even while the raw water surface still contains Samus's feet.
    sample.EquippedItems = 0;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111);
    SamusAerialMovement.InitializeJump(bus, sample);
    AssertEqual(1, sample.Kinematics.YSpeed, "water normal-jump whole speed");
    AssertEqual(0xc000, sample.Kinematics.YSubspeed, "water normal-jump fraction");
    AssertEqual(0x0800, sample.Kinematics.YSubacceleration, "water gravity fraction");

    sample.EquippedItems = 0x0100;
    SamusAerialMovement.InitializeJump(bus, sample);
    AssertEqual(2, sample.Kinematics.YSpeed, "water Hi-Jump whole speed");
    AssertEqual(0x8000, sample.Kinematics.YSubspeed, "water Hi-Jump fraction");

    sample.EquippedItems = (SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.HiJumpBoots).ToNativeWord();
    SamusAerialMovement.InitializeJump(bus, sample);
    AssertEqual(6, sample.Kinematics.YSpeed, "Gravity Suit forces air Hi-Jump entry");
    AssertEqual(0x1c00, sample.Kinematics.YSubacceleration,
        "Gravity Suit forces air acceleration");

    sample.EquippedItems = 0;
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111);
    SamusAerialMovement.InitializeJump(bus, sample);
    AssertEqual(2, sample.Kinematics.YSpeed, "lava normal-jump whole speed");
    AssertEqual(0x0900, sample.Kinematics.YSubacceleration, "lava gravity fraction");

    var speed = sample.HorizontalSpeed;
    speed.SelectEnvironmentSpeedTable(SamusLiquidPhysicsState.Air);
    AssertEqual(SamusMovementRomData.HorizontalMotion.NormalAirSpeedTable,
        speed.ActiveSpeedTableBaseAddress, "air X table base");
    speed.SelectEnvironmentSpeedTable(SamusLiquidPhysicsState.Water);
    AssertEqual(SamusMovementRomData.HorizontalMotion.WaterSpeedTable,
        speed.ActiveSpeedTableBaseAddress, "water X table base");
    speed.SelectEnvironmentSpeedTable(SamusLiquidPhysicsState.LavaAcid);
    AssertEqual(SamusMovementRomData.HorizontalMotion.LavaAcidSpeedTable,
        speed.ActiveSpeedTableBaseAddress, "lava X table base");

    // Submersion reaches `$90:9808` before the running/Dash test. It cannot establish new
    // momentum, but a pre-existing momentum flag preserves the accumulated pair exactly.
    var submergedDash = new SamusHorizontalSpeedState();
    submergedDash.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false,
        liquidImpeded: true);
    AssertTrue(!submergedDash.HasRunningMomentum, "submerged Dash cannot establish momentum");
    AssertEqual(0, submergedDash.ExtraRunSubspeed, "submerged no-momentum Dash stays zero");
    submergedDash.HandleExtraRunSpeed(SamusMovementType.Running, (ushort)SnesButton.B, false);
    ushort carriedFraction = submergedDash.ExtraRunSubspeed;
    submergedDash.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false,
        liquidImpeded: true);
    AssertTrue(submergedDash.HasRunningMomentum, "existing Dash momentum survives submersion");
    AssertEqual(carriedFraction, submergedDash.ExtraRunSubspeed,
        "submersion freezes rather than clears existing extra speed");

    // Pose-change animation samples Y+radius-1. Continuous FX animation samples the full
    // bottom boundary and updates remembered `$0AD2`; prove both edges independently.
    sample.EquippedItems = 0;
    sample.XSpeedDivisor = 7;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111);
    AssertEqual(7, sample.LiquidPhysics.DeterminePoseChangeAnimationBuffer(sample),
        "pose-change surface equality uses speed divisor");
    sample.LiquidPhysics.ConfigureWater(surfaceY: 110);
    AssertEqual(3, sample.LiquidPhysics.DeterminePoseChangeAnimationBuffer(sample),
        "pose-change water delay below bottom-minus-one");
    sample.LiquidPhysics.PrepareAnimationFrame(bus, sample);
    AssertEqual(3, sample.AnimationFrameBuffer, "continuous water animation delay");
    AssertEqual(SamusLiquidPhysicsState.Water, sample.LiquidPhysics.LiquidPhysicsType,
        "continuous water animation remembers medium");
    sample.EquippedItems = SamusEquipmentFlags.GravitySuit.ToNativeWord();
    sample.LiquidPhysics.PrepareAnimationFrame(bus, sample);
    AssertEqual(0, sample.AnimationFrameBuffer, "Gravity Suit cancels submerged frame delay");

    // Lava's FX handler performs the retail speed-boost cancellation before checking
    // Gravity Suit; acid enters the shared delay/damage tail without touching momentum.
    sample.EquippedItems = SamusEquipmentFlags.GravitySuit.ToNativeWord();
    sample.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false);
    sample.HorizontalSpeed.SpeedBoostCounter = 0x0401;
    sample.HorizontalSpeed.ExtraRunSpeed = 3;
    sample.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111);
    sample.LiquidPhysics.PrepareAnimationFrame(bus, sample);
    AssertTrue(!sample.HorizontalSpeed.HasRunningMomentum,
        "lava cancels momentum even with Gravity Suit");
    AssertEqual(0, sample.HorizontalSpeed.SpeedBoostCounter,
        "lava clears speed-boost timer/counter");
    AssertEqual(0, sample.HorizontalSpeed.ExtraRunSpeed,
        "lava explicitly clears extra whole speed");
    AssertEqual(0, sample.HorizontalSpeed.ExtraRunSubspeed,
        "lava explicitly clears extra fractional speed");

    sample.EquippedItems = 0;
    sample.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: SamusMovementType.Running,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false);
    sample.HorizontalSpeed.SpeedBoostCounter = 0x0201;
    sample.HorizontalSpeed.ExtraRunSpeed = 1;
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111, acid: true);
    sample.LiquidPhysics.PrepareAnimationFrame(bus, sample);
    AssertTrue(sample.HorizontalSpeed.HasRunningMomentum, "acid preserves running momentum");
    AssertEqual(0x0201, sample.HorizontalSpeed.SpeedBoostCounter,
        "acid preserves speed-boost timer/counter");
    AssertEqual(1, sample.HorizontalSpeed.ExtraRunSpeed,
        "acid preserves extra run speed");

    // `$9B:C4BE` has its own intentionally narrow definition of grapple liquid physics.
    // It ignores option bit two, samples only general FX Y, and clears on release-function
    // entry so the just-finished swing retains its prior flag for exactly one handler call.
    sample.EquippedItems = 0;
    sample.Grapple.Phase = GrapplePhase.ConnectedSwinging;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111, liquidOptions: 4);
    SamusGrappleMovement.RefreshLiquidPhysicsFlag(sample);
    AssertTrue(sample.Grapple.Submerged, "grapple liquid flag ignores water option bit two");
    sample.Grapple.Phase = GrapplePhase.ReleaseFromSwing;
    SamusGrappleMovement.RefreshLiquidPhysicsFlag(sample);
    AssertTrue(!sample.Grapple.Submerged, "grapple release function clears liquid flag");

    // Seed three distinguishable standalone `$90:9F31/$9F3D/$9F49` records. Mode two makes
    // the release handler subtract the selected fractional deceleration from 2.0000.
    int[] grappleReleaseRecords = [0x909f31, 0x909f3d, 0x909f49];
    ushort[] grappleReleaseDeceleration = [0x1000, 0x2000, 0x3000];
    for (int medium = 0; medium < grappleReleaseRecords.Length; medium++)
    {
        int address = grappleReleaseRecords[medium];
        for (int byteOffset = 0; byteOffset < SpeedTableEntry.ByteCount; byteOffset += 2)
            WriteTestWord(bus, address + byteOffset, 0);
        WriteTestWord(bus, address + 10, grappleReleaseDeceleration[medium]);
    }

    for (int medium = 0; medium < 3; medium++)
    {
        RoomLevelData releaseRoom = CreateEmptyRoom(16, 16);
        var released = new SamusState
        {
            Pose = SamusPoseIds.NormalJumpForwardRightPose,
            XPosition = 128,
            YPosition = 128,
            Kinematics =
            {
                XRadius = 5,
                YRadius = 12,
                YDirection = 1,
                YSpeed = 1,
            },
        };
        WritePoseDefinition(bus, released.Pose, [8, 2, 0xff, 0xff, 0, 0, 12, 0]);
        released.HorizontalSpeed.BaseSpeed = 2;
        released.Grapple.ReleasedMovementActive = true;
        if (medium == SamusLiquidPhysicsState.Water)
            released.LiquidPhysics.ConfigureWater(surfaceY: 127);
        else if (medium == SamusLiquidPhysicsState.LavaAcid)
            released.LiquidPhysics.ConfigureLavaAcid(surfaceY: 127);
        SamusAerialMovement.ConfigureEnvironmentGravity(bus, released);
        SamusAerialMovement.StepReleasedFromGrapple(
            bus,
            releaseRoom,
            released,
            controllerInput: 0,
            nmiFrameCounter: 0);
        AssertEqual(unchecked((ushort)(0 - grappleReleaseDeceleration[medium])),
            released.HorizontalSpeed.BaseSubspeed,
            $"grapple release medium {medium} standalone X record");
        AssertEqual(1, released.HorizontalSpeed.BaseSpeed,
            $"grapple release medium {medium} borrow into whole X speed");
        AssertTrue(released.Grapple.ReleasedMovementActive,
            $"grapple release medium {medium} handler survives before apex/collision");
    }

    // Give the spin routine authentic metadata plus zero horizontal records in all three
    // tables. Water's remembered medium lowers only the inclusive minimum from $0280 to $0080.
    WritePoseDefinition(bus, SamusPoseIds.SpaceJumpRightPose,
        [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.SpaceJumpRightPose * 2, 0xc000);
    for (int frame = 0; frame < 32; frame++)
        bus.WriteByte(0x91c000 + frame, 4);
    foreach (int tableBase in new[] { 0x909f55, 0x90a08d, 0x90a1dd })
    {
        int spinEntry = tableBase + 3 * SpeedTableEntry.ByteCount;
        for (int byteOffset = 0; byteOffset < SpeedTableEntry.ByteCount; byteOffset += 2)
            WriteTestWord(bus, spinEntry + byteOffset, 0);
    }
    RoomLevelData empty = CreateEmptyRoom(16, 16);

    var partialWaterSpaceJump = new SamusState
    {
        Pose = SamusPoseIds.SpaceJumpRightPose,
        EquippedItems = 0x0200,
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            XRadius = 5,
            YRadius = 12,
            YDirection = 2,
            YSpeed = 0,
            YSubspeed = 0x8000,
        },
    };
    partialWaterSpaceJump.LiquidPhysics.ConfigureWater(surfaceY: 128);
    partialWaterSpaceJump.LiquidPhysics.InitializeRememberedMedium(partialWaterSpaceJump);
    SamusAerialMovement.ConfigureEnvironmentGravity(bus, partialWaterSpaceJump);
    SamusAerialMovement.StepSpinJump(
        bus,
        empty,
        partialWaterSpaceJump,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: (ushort)SnesButton.A);
    AssertEqual(1, partialWaterSpaceJump.Kinematics.YDirection,
        "partially submerged Space Jump accepts water minimum $0080");
    AssertEqual(1, partialWaterSpaceJump.Kinematics.YSpeed,
        "water Space Jump reloads water launch whole speed");

    var fullySubmergedScrew = new SamusState
    {
        Pose = SamusPoseIds.ScrewAttackRightPose,
        EquippedItems = 0x0208,
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            XRadius = 5,
            YRadius = 12,
            YDirection = 2,
            YSpeed = 3,
        },
    };
    WritePoseDefinition(bus, SamusPoseIds.ScrewAttackRightPose,
        [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    fullySubmergedScrew.LiquidPhysics.ConfigureWater(surfaceY: 100);
    fullySubmergedScrew.LiquidPhysics.InitializeRememberedMedium(fullySubmergedScrew);
    SamusAerialMovement.ConfigureEnvironmentGravity(bus, fullySubmergedScrew);
    SamusAerialMovement.StepSpinJump(
        bus,
        empty,
        fullySubmergedScrew,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: (ushort)SnesButton.A);
    AssertEqual(2, fullySubmergedScrew.Kinematics.YDirection,
        "fully submerged non-Gravity Space Jump cannot restart");
    AssertEqual(0, fullySubmergedScrew.HorizontalSpeed.ContactDamageIndex,
        "fully submerged Screw Attack does not publish contact damage");

    Console.WriteLine(
        "  Samus liquids: boundaries, palette probe, ROM tables, gravity, Dash/lava cancellation, grapple, animation, Space Jump, and Gravity Suit agree.");
}

/// <summary>
/// Checks the producer, packed-slot interpreter, OAM path, sound publications, and periodic
/// fixed-point damage translated from `$90:8000-$82DB/$8A4C-$8C1E/$A3E5/$E9CE`.
/// </summary>
static void VerifySamusAtmosphericEffects()
{
    var bus = new TestAddressSpace();

    // Pose `$01` is sufficient to expose the literal movement type and X direction used by
    // all producer branches. Its delay metadata also lets the footstep fixture land exactly
    // on running frame two with timer one, matching `$90:A3EE-$A401`.
    WritePoseDefinition(bus, SamusPoseIds.FacingRightNormalPose,
        [8, 1, 0xff, 0, 0, 0, 12, 0]);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.FacingRightNormalPose * 2, 0xc000);
    bus.WriteBytes(0x91c000, [1, 1, 1, 1]);

    // Seed only the bank-$90 table records exercised below. Distinct attributes and timers
    // prove the interpreter follows ROM pointers rather than a duplicated C# animation list.
    WriteTestWord(bus, 0x908b93 + 4 * 2, 0x9000);
    WriteTestWord(bus, 0x909000, 2);
    WriteTestWord(bus, 0x909002, 3);
    WriteTestWord(bus, 0x908bef + 4 * 2, 4);
    WriteTestWord(bus, 0x908bff + 4 * 2, 0x9100);
    WriteTestWord(bus, 0x909100, 0x2a48);
    WriteTestWord(bus, 0x909102, 0x2a49);

    var samus = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 100,
        YPosition = 100,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);

    // Movement type one is a diving splash according to the real `$81A4` table. Keep NMI
    // away from the 128-frame bubble cadence so entry has exactly one sound request.
    bus.WriteByte(0x9081a4 + 1, 0);
    samus.LiquidPhysics.ConfigureWater(surfaceY: 111);
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 1);
    SamusAtmosphericEffectSlot entrySplash = samus.LiquidPhysics.AtmosphericEffects.Slots[0];
    AssertEqual(3, entrySplash.Type, "water entry selects diving-splash type");
    AssertEqual(2, entrySplash.AnimationTimer, "diving splash initial timer");
    AssertEqual(100, entrySplash.XPosition, "diving splash Samus X");
    AssertEqual(111, entrySplash.YPosition, "diving splash surface Y");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0d), 6),
        samus.LiquidPhysics.SoundRequests.Single(), "water-entry library-two sound");

    // Leaving water produces sound `$0E` and replaces the slot through the same movement-
    // type table. A spin/wall-jump-only `$30` request is correctly absent for type one.
    samus.LiquidPhysics.ConfigureWater(surfaceY: 112);
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 2);
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library2, 0x0e), 6),
        samus.LiquidPhysics.SoundRequests.Single(), "water-exit library-two sound");

    // Bubble admission samples top-24, every 128th accepted NMI, and slot two availability.
    // Re-enter on frame one first so the cadence call below is an already-submerged pass.
    samus.LiquidPhysics.ConfigureWater(surfaceY: 50);
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 1);
    var bubbleSystem = new Bank80SystemState(0x0061);
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 128, bubbleSystem);
    SamusAtmosphericEffectSlot bubble = samus.LiquidPhysics.AtmosphericEffects.Slots[2];
    AssertEqual(5, bubble.Type, "128-frame submerged cadence creates bubbles");
    AssertEqual(94, bubble.YPosition, "bubble origin is Samus top plus six");
    AssertTrue(samus.LiquidPhysics.SoundRequests.Any(request =>
        request.SoundEffect.Library == SoundEffectLibrary.Library2 &&
        request.SoundEffect.Value is 0x0f or 0x11),
        "bubble RNG publishes one of the two native sounds");

    // Surface spray uses four type-four slots and exact asymmetric X positions. A half-unit
    // lava rate then borrows from fractional health on the same `$E9CE` consumer call.
    WriteTestWord(bus, 0x909e8b, 0x8000);
    WriteTestWord(bus, 0x909e8d, 0);
    samus.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111);
    samus.Health = 99;
    samus.SubunitHealth = 0;
    samus.EquippedItems = 0;
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 1);
    AssertTrue(samus.LiquidPhysics.AtmosphericEffects.Slots.All(slot => slot.Type == 4),
        "partial lava submersion fills four surface-spray slots");
    AssertEqual(0x8000, samus.LiquidPhysics.PeriodicSubDamage,
        "lava fractional damage accumulates from ROM");
    samus.LiquidPhysics.ApplyPeriodicDamage(samus, timeIsFrozen: false);
    AssertEqual(98, samus.Health, "fractional lava subtraction borrows one energy");
    AssertEqual(0x8000, samus.SubunitHealth, "fractional energy retains wrapped half");
    AssertEqual(0, samus.LiquidPhysics.PeriodicSubDamage,
        "periodic consumer clears fractional accumulator");

    // Type-four slot zero begins with timer two. First update decrements/draws frame zero and
    // applies +1/-1 motion before clipping. The next call reloads ROM timer zero, advances to
    // frame one, and consumes its distinct `$2A49` attribute word.
    var effects = new SamusAtmosphericEffectsState();
    effects.SetSlot(0, type: 4, animationFrame: 0, animationTimer: 2, worldX: 104, worldY: 104);
    var oam = new OamBuffer();
    oam.BeginFrame();
    effects.UpdateAndDraw(bus, oam, cameraX: 0, cameraY: 0, fxYPosition: 0);
    AssertEqual(1, effects.Slots[0].AnimationTimer, "atmospheric positive timer decrements");
    AssertEqual(105, effects.Slots[0].XPosition, "lava slot zero drifts right");
    AssertEqual(103, effects.Slots[0].YPosition, "lava spray rises one pixel");
    AssertEqual(0x048, oam.GetEntry(0).TileNumber, "lava frame zero direct OAM tile");
    AssertEqual(5, oam.GetEntry(0).Palette, "lava direct OAM retains ROM palette");

    oam.BeginFrame();
    effects.UpdateAndDraw(bus, oam, cameraX: 0, cameraY: 0, fxYPosition: 0);
    AssertEqual(1, effects.Slots[0].AnimationFrame,
        "timer zero reloads old frame then advances packed word");
    AssertEqual(2, effects.Slots[0].AnimationTimer,
        "frame advance reloads literal ROM duration");
    AssertEqual(0x049, oam.GetEntry(0).TileNumber, "lava frame one direct OAM tile");

    // `$8002` is not a large positive delay: one call reaches `$8001` and does nothing;
    // the next reaches `$8000`, reloads the current ROM duration, moves, and draws.
    effects.Clear();
    effects.SetSlot(1, type: 4, animationFrame: 0, animationTimer: 0x8002, worldX: 100, worldY: 100);
    oam.BeginFrame();
    effects.UpdateAndDraw(bus, oam, 0, 0, 0);
    AssertEqual(100, effects.Slots[1].XPosition, "$8001 suppresses atmospheric motion");
    AssertEqual(0, oam.NextByteOffset, "$8001 suppresses atmospheric drawing");
    effects.UpdateAndDraw(bus, oam, 0, 0, 0);
    AssertEqual(101, effects.Slots[1].XPosition, "$8000 reload call begins motion");
    AssertEqual(4, oam.NextByteOffset, "$8000 reload call emits one OAM record");

    // Running frame two with stage four produces dry-room type-seven dust. The first slot
    // is delayed and the second immediate, exactly like `$90:EE64-$EEE3`.
    var runner = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 100,
        YPosition = 100,
    };
    runner.RefreshCollisionRadii(bus);
    runner.InitializeAnimation(bus, initialFrame: 2);
    runner.HorizontalSpeed.SpeedBoostCounter = 0x0400;
    bus.WriteByte(0x90a424 + 2, 1);
    runner.LiquidPhysics.PrepareAnimationFrame(bus, runner, nmiFrameCounter: 1);
    AssertEqual(7, runner.LiquidPhysics.AtmosphericEffects.Slots[0].Type,
        "boost-stage-four foot contact creates dust");
    AssertEqual(0x8002,
        runner.LiquidPhysics.AtmosphericEffects.Slots[0].AnimationTimer,
        "first foot effect uses delayed-start sentinel");
    AssertEqual(3,
        runner.LiquidPhysics.AtmosphericEffects.Slots[1].AnimationTimer,
        "second foot effect starts immediately");

    // `$91:F046` runs before animation and before landing clears Y velocity. Norfair selects
    // type-six dust in slots two/three; spin termination precedes the soft-impact request in
    // the native sound queues. The source pose is deliberately ordinary spin rather than a
    // landing pose because `$0A20` has not been replaced yet at this collision seam.
    var landing = new SamusState
    {
        Pose = 0x19,
        XPosition = 100,
        YPosition = 100,
    };
    WritePoseDefinition(bus, 0x19, [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    landing.RefreshCollisionRadii(bus);
    landing.Kinematics.YSpeed = 4;
    landing.Kinematics.YSubspeed = 0;
    landing.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Norfair, 0);
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: SamusMovementType.SpinJumping, previousPose: 0x19,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(2, landing.LiquidPhysics.SoundRequests.Count,
        "spin landing publishes termination and impact sounds");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x32), 6),
        landing.LiquidPhysics.SoundRequests[0], "ordinary spin termination sound");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x05), 6),
        landing.LiquidPhysics.SoundRequests[1], "sub-five soft landing sound");
    AssertEqual(6, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Norfair landing creates right dust slot");
    AssertEqual(6, landing.LiquidPhysics.AtmosphericEffects.Slots[3].Type,
        "Norfair landing creates left dust slot");
    AssertEqual(108, landing.LiquidPhysics.AtmosphericEffects.Slots[2].XPosition,
        "landing dust right X offset");
    AssertEqual(92, landing.LiquidPhysics.AtmosphericEffects.Slots[3].XPosition,
        "landing dust left X offset");
    AssertEqual(112, landing.LiquidPhysics.AtmosphericEffects.Slots[2].YPosition,
        "landing dust uses current bottom boundary");

    // Whole speed five changes only the impact sound to hard `$04`; Screw Attack changes
    // only the preceding library-one termination to `$34`. A cinematic suppresses both
    // sounds but Norfair's graphics handler still creates dust, exactly as the separate
    // native checks dictate.
    landing.Kinematics.YSpeed = 5;
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: SamusMovementType.WallJumping, previousPose: 0x81,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library1, 0x34), 6),
        landing.LiquidPhysics.SoundRequests[0], "Screw Attack termination sound");
    AssertEqual(new SamusSoundRequest(SoundEffectId.FromCartridge(SoundEffectLibrary.Library3, 0x04), 6),
        landing.LiquidPhysics.SoundRequests[1], "speed-five hard landing sound");
    landing.LiquidPhysics.CinematicFunctionActive = true;
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: SamusMovementType.SpinJumping, previousPose: 0x19,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(0, landing.LiquidPhysics.SoundRequests.Count,
        "cinematic landing suppresses both sound libraries");
    AssertEqual(6, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Norfair cinematic still dispatches landing dust");
    landing.LiquidPhysics.CinematicFunctionActive = false;

    // Active liquid returns without touching the landing slots. This is intentionally not
    // deletion: seed an unrelated type-seven record and prove its packed word survives.
    landing.LiquidPhysics.AtmosphericEffects.SetSlot(
        2, type: 7, animationFrame: 2, animationTimer: 9, worldX: 77, worldY: 88);
    landing.LiquidPhysics.ConfigureWater(surfaceY: 111);
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: SamusMovementType.Falling, previousPose: 0x29,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(7, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "submerged landing leaves prior atmospheric slot untouched");
    AssertEqual(9, landing.LiquidPhysics.AtmosphericEffects.Slots[2].AnimationTimer,
        "submerged return does not clear stale timer");

    // Ceres/debug deletion writes only frame/type. Position and timer are observable stale
    // WRAM. Zero vertical speed returns even earlier and therefore does not delete anything.
    landing.LiquidPhysics.RoomIdentity = new RoomIdentity(AreaId.Ceres, 0);
    landing.LiquidPhysics.FxYPosition = ushort.MaxValue;
    landing.Kinematics.YSpeed = 1;
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: SamusMovementType.Falling, previousPose: 0x29,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(0, landing.LiquidPhysics.AtmosphericEffects.Slots[2].FrameAndType,
        "Ceres deletes landing packed word");
    AssertEqual(9, landing.LiquidPhysics.AtmosphericEffects.Slots[2].AnimationTimer,
        "Ceres delete preserves atmospheric timer");
    AssertEqual(77, landing.LiquidPhysics.AtmosphericEffects.Slots[2].XPosition,
        "Ceres delete preserves atmospheric X");
    landing.LiquidPhysics.AtmosphericEffects.SetSlot(
        2, type: 7, animationFrame: 0, animationTimer: 3, worldX: 1, worldY: 2);
    landing.Kinematics.YSpeed = 0;
    landing.Kinematics.YSubspeed = 0;
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: SamusMovementType.Falling, previousPose: 0x29,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(7, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "zero-speed grounding returns before graphics dispatch");

    // Crateria reads its literal inline room byte. Landing Site flag one requires exact FX
    // type `$000A`; with a dry surface it creates type-one splashes at +4/-3 and bottom-4.
    bus.WriteByte(0x91f0f3, 1);
    landing.LiquidPhysics.RoomIdentity = RoomIdentities.LandingSite;
    landing.LiquidPhysics.FxType = 0x000a;
    landing.LiquidPhysics.FxYPosition = ushort.MaxValue;
    landing.Kinematics.YSubspeed = 1;
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: SamusMovementType.Falling, previousPose: 0x29,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(1, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Landing Site type-A FX selects splash");
    AssertEqual(104, landing.LiquidPhysics.AtmosphericEffects.Slots[2].XPosition,
        "landing splash right X offset");
    AssertEqual(97, landing.LiquidPhysics.AtmosphericEffects.Slots[3].XPosition,
        "landing splash left X offset");
    AssertEqual(108, landing.LiquidPhysics.AtmosphericEffects.Slots[2].YPosition,
        "landing splash rises four pixels above feet");

    Console.WriteLine(
        "  Samus atmosphere: liquid FX, footsteps, landing impact, packed timers, sound, and OAM agree.");
}

/// <summary>
/// Verifies the cartridge's complete ten-way jump/fall turn selectors and the block-only
/// wall-jump route. Every expectation below is a literal bank-$90/$91 table value; the test
/// intentionally does not calculate a mirrored target from facing.
/// </summary>
}
