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

/// <summary>Slope, block, grounded, reversal, moonwalk, and wall-contact verification.</summary>
static void VerifySamusSlopePhysics()
{
    var bus = new TestAddressSpace();

    // Shape $12 selects multiplier-table word index 2*$12+1 = $25. Retail ROM stores
    // $00C0 there, causing a grounded 1.0 displacement to become 0.C000.
    WriteTestWord(
        bus,
        SamusSlopePhysics.HorizontalMultiplierTableAddress + (2 * 0x12 + 1) * 2,
        0x00c0);
    AssertEqual(
        0x0000c000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, 0x12, 0x00010000, verticalSpeed: 0),
        "BTS $12 grounded positive slope scaling");
    AssertEqual(
        unchecked((int)0xffff4000),
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, 0x12, -0x00010000, verticalSpeed: 0),
        "BTS $12 grounded negative slope scaling");

    // Ceiling bit $80 and any nonzero Y speed are independent native early returns.
    AssertEqual(
        0x00010000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, 0x92, 0x00010000, verticalSpeed: 0),
        "ceiling slope does not scale horizontal speed");
    AssertEqual(
        0x00010000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, 0x12, 0x00010000, verticalSpeed: 1),
        "airborne Samus does not receive grounded slope scaling");

    // Seed two explicit samples in shape $12's 16-byte row. BTS bit $40 mirrors X=0 to
    // sample 15, proving the profile selection comes from ROM rather than a line formula.
    int shapeTwelveRow = SamusSlopePhysics.AlignmentHeightTableAddress + 16 * 0x12;
    bus.WriteByte(shapeTwelveRow + 0, 8);
    bus.WriteByte(shapeTwelveRow + 15, 3);
    AssertEqual(8, SamusSlopePhysics.ReadAlignmentHeight(bus, 0x12, 0), "slope unmirrored height sample");
    AssertEqual(3, SamusSlopePhysics.ReadAlignmentHeight(bus, 0x52, 0), "slope mirrored height sample");

    // A type-1/BTS-$12 block occupies (0,1). At center Y=21 with radius 5, Samus's bottom
    // is Y=25 (low nibble 9). Height 8 yields correction 8-9-1 = -2, so $94:87F4 moves
    // her center upward to 19 and marks slope adjustment for the later grounding branch.
    RoomLevelData level = CreateRoom(
        2,
        2,
        [0x0000, 0x0000, 0x1000, 0x0000],
        [0x00, 0x00, 0x12, 0x00]);
    SlopeAlignmentResult aligned = SamusSlopePhysics.AlignYPosition(
        bus,
        level,
        xPosition: 0,
        yPosition: 21,
        yRadius: 5);
    AssertEqual(19, aligned.YPosition, "non-square floor slope whole-pixel Y correction");
    AssertTrue(aligned.Adjusted, "non-square floor slope sets adjusted flag");
    AssertEqual(0x12, aligned.FloorBlock!.Value.Behavior, "non-square floor reports source BTS");

    SlopeAlignmentResult disabled = SamusSlopePhysics.AlignYPosition(
        bus,
        level,
        xPosition: 0,
        yPosition: 21,
        yRadius: 5,
        horizontalSlopeCollisionEnabled: false);
    AssertEqual(21, disabled.YPosition, "disabled horizontal slope collision preserves Y");
    AssertTrue(!disabled.Adjusted, "disabled horizontal slope collision preserves adjusted flag");

    Console.WriteLine("  Samus slopes: ROM multiplier, mirrored height samples, and non-square Y alignment agree.");
}

/// <summary>
/// Exercises the native block-span scans and supported type-0/type-1/type-8 reactions
/// surrounding the isolated slope primitives.
/// </summary>
static void VerifySamusBlockCollision()
{
    var bus = new TestAddressSpace();
    WriteTestWord(
        bus,
        SamusSlopePhysics.HorizontalMultiplierTableAddress + (2 * 0x12 + 1) * 2,
        0x00c0);
    bus.WriteByte(SamusSlopePhysics.AlignmentHeightTableAddress + 16 * 0x12, 8);

    const int width = 4;
    const int height = 4;
    var foreground = new ushort[width * height];
    var behavior = new byte[foreground.Length];

    // Row one contains a non-square slope in column one and an ordinary type-8 solid in
    // column two. Every other cell is dispatcher type zero air.
    foreground[1 * width + 1] = 0x1000;
    behavior[1 * width + 1] = 0x12;
    foreground[1 * width + 2] = 0x8000;
    RoomLevelData level = CreateRoom(width, height, foreground, behavior);

    // At Y=21/radius5 the body penetrates shape-$12's height-8 sample by two pixels.
    // Horizontal type-1 processing first scales 1.0 to 0.C000, then $94:87F4 raises Y.
    var slopeBody = new SamusKinematicsState
    {
        XPosition = 16,
        YPosition = 21,
        XRadius = 5,
        YRadius = 5,
    };
    BlockMoveResult slopeMove = SamusBlockCollision.MoveHorizontal(
        bus,
        level,
        slopeBody,
        displacement: 0x00010000);
    AssertEqual(0x0000c000, slopeMove.AcceptedDisplacement, "horizontal scan applies BTS $12 multiplier");
    AssertEqual(16, slopeBody.XPosition, "subpixel slope move preserves whole X");
    AssertEqual(0xc000, slopeBody.XSubposition, "subpixel slope move updates X fraction");
    AssertEqual(19, slopeBody.YPosition, "post-horizontal scan aligns non-square floor Y");
    AssertTrue(slopeMove.PositionAdjustedBySlope, "horizontal scan reports slope adjustment");

    // Resting at center Y=19 puts the bottom at 23. The native +1.0 grounding probe
    // targets bottom 24; height 8 yields correction -1 and clips the accepted move to zero.
    var groundedBody = new SamusKinematicsState
    {
        XPosition = 16,
        YPosition = 19,
        XRadius = 5,
        YRadius = 5,
    };
    BlockMoveResult groundProbe = SamusBlockCollision.MoveVertical(
        bus,
        level,
        groundedBody,
        displacement: 0x00010000,
        scanLeftToRight: true);
    AssertTrue(groundProbe.Collided, "vertical non-square grounding probe collides");
    AssertEqual(0, groundProbe.AcceptedDisplacement, "vertical non-square grounding probe clips to zero");
    AssertEqual(19, groundedBody.YPosition, "grounding collision preserves resting center Y");
    AssertEqual(0x12, groundProbe.CollisionBlock!.Value.Behavior, "grounding collision reports slope BTS");

    // Moving two pixels right from X=26 would enter the type-8 block at X=32. The solid
    // formula permits one pixel, writes subposition $FFFF, and stops center X at 27.FFFF.
    var wallBody = new SamusKinematicsState
    {
        XPosition = 26,
        YPosition = 24,
        XRadius = 5,
        YRadius = 5,
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult wallMove = SamusBlockCollision.MoveHorizontal(
        bus,
        level,
        wallBody,
        displacement: 0x00020000);
    AssertTrue(wallMove.Collided, "horizontal type-8 solid collision flag");
    AssertEqual(0x00010000, wallMove.AcceptedDisplacement, "horizontal type-8 solid clipped amount");
    AssertEqual(27, wallBody.XPosition, "horizontal type-8 solid whole X");
    AssertEqual(0xffff, wallBody.XSubposition, "horizontal type-8 solid right-wall fraction");

    // An air-only downward move preserves all 16.16 bits and has no collision record.
    var airBody = new SamusKinematicsState
    {
        // X=56 with radius five spans pixels 51..60, entirely inside air column three.
        XPosition = 56,
        YPosition = 19,
        XRadius = 5,
        YRadius = 5,
        YSubposition = 0x4000,
    };
    BlockMoveResult airMove = SamusBlockCollision.MoveVertical(
        bus,
        level,
        airBody,
        displacement: 0x00008000,
        scanLeftToRight: false);
    AssertTrue(!airMove.Collided, "vertical type-0 air has no collision");
    AssertEqual(19, airBody.YPosition, "vertical air fractional move whole Y");
    AssertEqual(0xc000, airBody.YSubposition, "vertical air fractional move subposition");

    // Shape four has all four $94:8E54 quadrant bytes set. Unlike an ordinary solid, its
    // clipping boundary is the leading 8-pixel half, although this fixture reaches the
    // block's first half so the permitted amount is one whole pixel.
    var squareForeground = new ushort[width * height];
    var squareBehavior = new byte[squareForeground.Length];
    squareForeground[1 * width + 2] = 0x1000;
    squareBehavior[1 * width + 2] = 0x04;
    squareForeground[2 * width + 1] = 0x1000;
    squareBehavior[2 * width + 1] = 0x04;
    RoomLevelData squareLevel = CreateRoom(
        width, height, squareForeground, squareBehavior);

    var squareWallBody = new SamusKinematicsState
    {
        XPosition = 26,
        YPosition = 24,
        XRadius = 5,
        YRadius = 5,
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult squareWall = SamusBlockCollision.MoveHorizontal(
        bus,
        squareLevel,
        squareWallBody,
        displacement: 0x00020000);
    AssertTrue(squareWall.Collided, "horizontal fully-solid square slope collision");
    AssertEqual(0x00010000, squareWall.AcceptedDisplacement, "horizontal square-slope 8-pixel clipping");
    AssertEqual(0xffff, squareWallBody.XSubposition, "horizontal square slope writes right-wall fraction");

    var squareFloorBody = new SamusKinematicsState
    {
        XPosition = 16,
        YPosition = 26,
        XRadius = 5,
        YRadius = 5,
    };
    BlockMoveResult squareFloor = SamusBlockCollision.MoveVertical(
        bus,
        squareLevel,
        squareFloorBody,
        displacement: 0x00020000,
        scanLeftToRight: true);
    AssertTrue(squareFloor.Collided, "vertical fully-solid square slope collision");
    AssertEqual(0x00010000, squareFloor.AcceptedDisplacement, "vertical square-slope 8-pixel clipping");
    AssertTrue(squareFloorBody.PositionAdjustedBySlope, "downward square slope sets adjusted flag");
    AssertEqual(0xffff, squareFloorBody.YSubposition, "downward square slope writes floor fraction");

    // `$94:938B` masks the BTS index, reads a bank-$8F door-pointer table, then inspects
    // the bank-$83 destination. Entry zero is a normal room door: carry stays clear and the
    // exact header is published for game state $09. Entry one is an elevator pseudo-door
    // with destination bit 15 clear and therefore retains ordinary solid clipping.
    const ushort doorListPointer = 0x9000;
    const ushort normalDoorPointer = 0xa000;
    const ushort elevatorDoorPointer = 0xa00c;
    WriteTestWord(bus, 0x8f0000 | doorListPointer, normalDoorPointer);
    WriteTestWord(bus, 0x8f0000 | (doorListPointer + 2), elevatorDoorPointer);
    WriteTestWord(bus, 0x830000 | normalDoorPointer, 0x9123);
    WriteTestWord(bus, 0x830000 | elevatorDoorPointer, 0x1234);

    var doorForeground = new ushort[width * height];
    var doorBehavior = new byte[doorForeground.Length];
    doorForeground[1 * width + 2] = 0x9000;
    RoomLevelData doorLevel = new(
        width,
        height,
        doorForeground,
        doorBehavior,
        new ushort[doorForeground.Length],
        new byte[8],
        doorListPointer: doorListPointer);
    var doorBody = new SamusKinematicsState
    {
        XPosition = 26,
        YPosition = 24,
        XRadius = 5,
        YRadius = 5,
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult normalDoorMove = SamusBlockCollision.MoveHorizontal(
        bus,
        doorLevel,
        doorBody,
        displacement: 0x00020000);
    AssertTrue(!normalDoorMove.Collided, "normal type-$9 door returns carry clear");
    AssertEqual(0x00020000, normalDoorMove.AcceptedDisplacement,
        "normal type-$9 door accepts full horizontal displacement");
    AssertEqual(normalDoorPointer, doorLevel.PendingDoorTransition!.Pointer,
        "normal type-$9 door publishes exact bank-$83 pointer");
    AssertEqual(0x9123, doorLevel.PendingDoorTransition!.DestinationRoomPointer,
        "normal type-$9 door preserves destination room header");
    AssertEqual(normalDoorPointer, doorLevel.ConsumePendingDoorTransition()!.Pointer,
        "door transition publication is consumed exactly once");
    AssertTrue(doorLevel.PendingDoorTransition is null,
        "consumed door transition does not leak into later movement");

    doorBehavior[1 * width + 2] = 1;
    RoomLevelData elevatorDoorLevel = new(
        width,
        height,
        doorForeground,
        doorBehavior,
        new ushort[doorForeground.Length],
        new byte[8],
        doorListPointer: doorListPointer);
    doorBody.XPosition = 26;
    doorBody.XSubposition = 0;
    BlockMoveResult elevatorDoorMove = SamusBlockCollision.MoveHorizontal(
        bus,
        elevatorDoorLevel,
        doorBody,
        displacement: 0x00020000);
    AssertTrue(elevatorDoorMove.Collided, "elevator pseudo-door remains solid");
    AssertEqual(0x00010000, elevatorDoorMove.AcceptedDisplacement,
        "elevator pseudo-door uses ordinary solid clipping");
    AssertTrue(elevatorDoorLevel.PendingDoorTransition is null,
        "elevator pseudo-door does not publish normal room transition");

    // The room loader fills all unused level-data allocation words with `$8000` before
    // decompression. Put Samus one pixel inside the logical right and bottom boundaries;
    // a two-pixel request samples the first prefilled word and must clip like type-8 solid.
    var rightEdgeBody = new SamusKinematicsState
    {
        XPosition = 58,
        YPosition = 24,
        XRadius = 5,
        YRadius = 5,
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult rightEdgeMove = SamusBlockCollision.MoveHorizontal(
        bus,
        level,
        rightEdgeBody,
        displacement: 0x00020000);
    AssertTrue(rightEdgeMove.Collided, "prefilled right room edge is solid");
    AssertEqual(0x00010000, rightEdgeMove.AcceptedDisplacement,
        "prefilled right room edge clips at logical boundary");
    AssertEqual(0x8000, rightEdgeMove.CollisionBlock!.Value.LevelWord,
        "prefilled right room edge exposes native $8000 word");

    var bottomEdgeBody = new SamusKinematicsState
    {
        XPosition = 56,
        YPosition = 58,
        XRadius = 5,
        YRadius = 5,
    };
    BlockMoveResult bottomEdgeMove = SamusBlockCollision.MoveVertical(
        bus,
        level,
        bottomEdgeBody,
        displacement: 0x00020000,
        scanLeftToRight: true);
    AssertTrue(bottomEdgeMove.Collided, "prefilled bottom room edge is solid");
    AssertEqual(0x00010000, bottomEdgeMove.AcceptedDisplacement,
        "prefilled bottom room edge clips at logical boundary");
    AssertEqual(0x8000, bottomEdgeMove.CollisionBlock!.Value.LevelWord,
        "prefilled bottom room edge exposes native $8000 word");

    // Grapple endpoint/body collision now consumes this same room-owned seam instead of
    // maintaining a second interpretation of the native prefill. Check coordinate and
    // already-linear forms because bank-$94 uses both during extension redispatch.
    RoomCollisionBlock coordinatePrefill =
        level.GetCollisionBlockOrPrefilledSolid(level.WidthInBlocks, 0);
    AssertEqual(-1, coordinatePrefill.Index,
        "coordinate prefill is outside the logical room plane");
    AssertEqual(0x8000, coordinatePrefill.LevelWord,
        "coordinate prefill exposes native solid word");
    RoomCollisionBlock linearPrefill =
        level.GetCollisionBlockByIndexOrPrefilledSolid(-1);
    AssertEqual(0x8000, linearPrefill.LevelWord,
        "linear extension prefill exposes native solid word");

    // Exercise every formerly open body-dispatch entry through the public movement seam.
    // Types 2/3/4/6/7 are non-solid for Samus's body.  Spike type A and special type B
    // retain their independent solid topology. Their authored damage/PLM producer seams do
    // not alter the clipping result asserted here and are deliberately not claimed by this
    // kinematics-only fixture.
    foreach ((ushort collisionWord, bool expectedCollision) in new[]
    {
        ((ushort)0x2000, false),
        ((ushort)0x3000, false),
        ((ushort)0x4000, false),
        ((ushort)0x6000, false),
        ((ushort)0x7000, false),
        ((ushort)0xa000, true),
        ((ushort)0xb000, true),
    })
    {
        var dispatcherWords = new ushort[width * height];
        dispatcherWords[1 * width + 2] = collisionWord;
        RoomLevelData dispatcherLevel = CreateRoom(
            width,
            height,
            dispatcherWords,
            new byte[dispatcherWords.Length]);
        var dispatcherBody = new SamusKinematicsState
        {
            XPosition = 26,
            YPosition = 24,
            XRadius = 5,
            YRadius = 5,
            HorizontalSlopeCollisionEnable = 0,
        };
        BlockMoveResult dispatcherMove = SamusBlockCollision.MoveHorizontal(
            bus,
            dispatcherLevel,
            dispatcherBody,
            displacement: 0x00020000);

        AssertEqual(expectedCollision, dispatcherMove.Collided,
            $"body dispatcher type ${(collisionWord >> 12):X1} collision topology");
        AssertEqual(expectedCollision ? 0x00010000 : 0x00020000,
            dispatcherMove.AcceptedDisplacement,
            $"body dispatcher type ${(collisionWord >> 12):X1} accepted displacement");
    }

    Console.WriteLine("  Samus blocks: all sixteen dispatcher types, spans, slopes, prefilled edges, and doors agree.");
}

/// <summary>
/// Verifies the bank-$90 ordering around the already-isolated speed and bank-$94 scans:
/// running accelerates, moves horizontally, then probes down; standing probes and clears.
/// </summary>
static void VerifySamusGroundedMovement()
{
    var bus = new TestAddressSpace();

    // Running movement type one reads the normal-air entry at $90:9F61. These are the real
    // retail words already asserted independently by VerifySamusHorizontalSpeed.
    WriteTestWord(bus, 0x909f61, 0x0000); // acceleration whole
    WriteTestWord(bus, 0x909f63, 0x3000); // acceleration fraction
    WriteTestWord(bus, 0x909f65, 0x0002); // maximum whole
    WriteTestWord(bus, 0x909f67, 0xc000); // maximum fraction
    WriteTestWord(bus, 0x909f69, 0x0000); // deceleration whole
    WriteTestWord(bus, 0x909f6b, 0x8000); // deceleration fraction

    // Shape $12 scales grounded horizontal displacement by $00C0/256 = 3/4 and exposes an
    // eight-pixel surface at X nibble zero. Row one is an uninterrupted two-block floor so
    // the radius scan can visit either column without introducing another dispatcher type.
    WriteTestWord(
        bus,
        SamusSlopePhysics.HorizontalMultiplierTableAddress + (2 * 0x12 + 1) * 2,
        0x00c0);
    bus.WriteByte(SamusSlopePhysics.AlignmentHeightTableAddress + 16 * 0x12, 8);
    RoomLevelData level = CreateRoom(
        2,
        3,
        [0, 0, 0x1000, 0x1000, 0, 0],
        [0, 0, 0x12, 0x12, 0, 0]);

    var running = new SamusState { Pose = SamusPoseIds.MovingRightNormalPose };
    running.Kinematics.XPosition = 16;
    running.Kinematics.YPosition = 19;
    running.Kinematics.XRadius = 5;
    running.Kinematics.YRadius = 5;
    GroundedMovementResult first = SamusGroundedMovement.StepRunningRight(
        bus,
        level,
        running,
        nmiFrameCounter: 0);

    AssertEqual(0x0000, running.HorizontalSpeed.BaseSpeed, "running first acceleration whole speed");
    AssertEqual(0x3000, running.HorizontalSpeed.BaseSubspeed, "running first acceleration fractional speed");
    AssertEqual(0x00002400, first.Horizontal.AcceptedDisplacement, "running slope-scaled horizontal amount");
    AssertEqual(0x2400, running.Kinematics.XSubposition, "running horizontal amount reaches X subposition");
    AssertTrue(first.Vertical.Collided, "running total-speed-plus-one grounding probe collides");
    AssertEqual(0, first.Vertical.AcceptedDisplacement, "running grounding probe clips at surface");

    // A second frame proves that acceleration state persists and that odd-NMI scan order
    // produces the same single-surface result in this deliberately symmetric fixture.
    GroundedMovementResult second = SamusGroundedMovement.StepRunningRight(
        bus,
        level,
        running,
        nmiFrameCounter: 1);
    AssertEqual(0x6000, running.HorizontalSpeed.BaseSubspeed, "running second acceleration accumulates");
    AssertEqual(0x00004800, second.Horizontal.AcceptedDisplacement, "second running slope multiplier");
    AssertEqual(0x6c00, running.Kinematics.XSubposition, "second running displacement accumulates");
    AssertTrue(second.Vertical.Collided, "odd-frame grounding scan collides");

    // Momentum routine one selects acceleration mode two after Right is released. The next
    // $90:9A7E pass subtracts $0000.8000; from $0000.6000 this underflows the signed high
    // word and therefore clears both base halves and restores mode zero.
    running.HorizontalSpeed.AccelerationMode = 2;
    GroundedMovementResult decelerated = SamusGroundedMovement.StepRunningRight(
        bus,
        level,
        running,
        nmiFrameCounter: 0);
    AssertEqual(0, running.HorizontalSpeed.BaseSpeed, "running deceleration underflow clears whole speed");
    AssertEqual(0, running.HorizontalSpeed.BaseSubspeed, "running deceleration underflow clears fraction");
    AssertEqual(0, running.HorizontalSpeed.AccelerationMode, "running deceleration underflow restores acceleration mode");
    AssertEqual(0, decelerated.Horizontal.AcceptedDisplacement, "cleared deceleration frame has no X displacement");

    // Standing executes its zero-base MoveX/grounding calls before clearing momentum. Base
    // speed itself is not included in Move_NoBaseSpeed_X, so the body remains on the same X.
    var standing = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose };
    standing.Kinematics.XPosition = 16;
    standing.Kinematics.YPosition = 19;
    standing.Kinematics.XRadius = 5;
    standing.Kinematics.YRadius = 5;
    standing.HorizontalSpeed.BaseSpeed = 2;
    standing.HorizontalSpeed.BaseSubspeed = 0xc000;
    GroundedMovementResult idle = SamusGroundedMovement.StepStandingRight(
        bus,
        level,
        standing,
        nmiFrameCounter: 0);
    AssertEqual(0, idle.Horizontal.AcceptedDisplacement, "standing no-base horizontal amount");
    AssertTrue(idle.Vertical.Collided, "standing one-pixel grounding probe collides");
    AssertEqual(0, standing.HorizontalSpeed.BaseSpeed, "standing clears base speed whole");
    AssertEqual(0, standing.HorizontalSpeed.BaseSubspeed, "standing clears base speed fraction");

    // The front-view branch returns before every ordinary standing movement call. Seed
    // deliberately stale motion/collision values to prove the sole native write is the
    // vertical-result clear and that no host convenience cleanup leaked into this path.
    var forward = new SamusState
    {
        Pose = SamusPoseIds.ForwardFacingPowerSuitPose,
        XPosition = 0x1234,
        YPosition = 0x5678,
        SolidVerticalCollisionResult = 9,
    };
    forward.HorizontalSpeed.BaseSpeed = 2;
    forward.HorizontalSpeed.BaseSubspeed = 0x3456;
    forward.HorizontalSpeed.ExtraRunSpeed = 1;
    BlockMoveResult? stationaryForward = SamusGroundedMovement.StepFacingForward(
        bus,
        level,
        forward,
        nmiFrameCounter: 0);
    AssertTrue(stationaryForward is null, "zero elevator status skips the vertical scan");
    AssertEqual(0, forward.SolidVerticalCollisionResult, "forward movement clears vertical collision result");
    AssertEqual(0x1234, forward.XPosition, "forward movement does not scan or move X");
    AssertEqual(0x5678, forward.YPosition, "stationary forward movement does not move Y");
    AssertEqual(2, forward.HorizontalSpeed.BaseSpeed, "forward dispatcher retains stale base speed");
    AssertEqual(0x3456, forward.HorizontalSpeed.BaseSubspeed, "forward dispatcher retains stale base subspeed");
    AssertEqual(1, forward.HorizontalSpeed.ExtraRunSpeed, "forward dispatcher retains stale extra speed");

    // `$90:A392` consumes actor-owned elevator status but performs Samus's movement itself.
    // Put a valid solid-enemy candidate exactly one pixel below an all-air room body: the
    // ordinary MoveVertical wrapper would stop on it, whereas `$94:9763` must deliberately
    // ignore it and accept the complete 1.0000 downward displacement.
    RoomLevelData elevatorLevel = CreateEmptyRoom(8, 8);
    forward.XPosition = 100;
    forward.YPosition = 100;
    forward.Kinematics.XSubposition = 0x1234;
    forward.Kinematics.YSubposition = 0x5678;
    forward.Kinematics.XRadius = 5;
    forward.Kinematics.YRadius = 10;
    forward.Kinematics.InteractiveEnemies =
    [
        new SolidEnemyCollisionBody(
            Index: 0x01c0,
            XPosition: 100,
            YPosition: 116,
            XRadius: 5,
            YRadius: 5,
            FreezeTimer: 0,
            Properties: 0x8000),
    ];
    forward.SolidVerticalCollisionResult = 9;
    BlockMoveResult? elevatorMove = SamusGroundedMovement.StepFacingForward(
        bus,
        elevatorLevel,
        forward,
        nmiFrameCounter: 1,
        elevatorIsMoving: true);
    AssertTrue(elevatorMove is { Collided: false }, "elevator block scan accepts clear air");
    BlockMoveResult acceptedElevatorMove = elevatorMove ?? throw new InvalidOperationException(
        "A nonzero elevator status must execute the one-pixel vertical scan.");
    AssertTrue(acceptedElevatorMove.EnemyCollision is null,
        "elevator `$94:9763` path skips a colliding solid enemy");
    AssertEqual(0x00010000, acceptedElevatorMove.AcceptedDisplacement,
        "elevator requests exact one-pixel downward displacement");
    AssertEqual(101, forward.YPosition, "elevator moves forward-facing Samus down one pixel");
    AssertEqual(0x5678, forward.Kinematics.YSubposition,
        "whole-pixel elevator motion preserves Y fraction");
    AssertEqual(0, forward.SolidVerticalCollisionResult,
        "elevator path clears vertical collision result after movement");

    // Every nonzero status executes `$90:A392`'s scan; destination shaft scroll PLMs rely
    // on that collision pass while the actor pins Samus. The collapsed host transition must
    // suppress only the redundant pseudo-door publication during statuses two/three.
    AssertTrue(
        SuperMetroidRuntime.ShouldPublishFacingForwardElevatorDoorSideEffects(
            ElevatorActorStatus.Departing),
        "departing elevator publishes forward pseudo-door contact");
    AssertTrue(
        !SuperMetroidRuntime.ShouldPublishFacingForwardElevatorDoorSideEffects(
            ElevatorActorStatus.Inactive),
        "inactive elevator suppresses pseudo-door contact");
    AssertTrue(
        !SuperMetroidRuntime.ShouldPublishFacingForwardElevatorDoorSideEffects(
            ElevatorActorStatus.BeginArrivalReturn),
        "arrival setup suppresses repeated pseudo-door contact");
    AssertTrue(
        !SuperMetroidRuntime.ShouldPublishFacingForwardElevatorDoorSideEffects(
            ElevatorActorStatus.ReturningToRest),
        "arrival return suppresses repeated pseudo-door contact");

    Console.WriteLine("  Samus movement: forward/elevator, standing, and running speed/X/slope/grounding order agree.");
}

/// <summary>
/// Locks down the newly translated leftward and grounded-reversal routes independently of
/// WinForms: literal pose bytes, command $F8 timing, mode-one direction inversion, and the
/// mode-clear boundary are all observable here under a normal debugger.
/// </summary>
static void VerifySamusGroundedReversal()
{
    var bus = new TestAddressSpace();

    // Both ordinary running directions use movement type one's $90:9F61 record. The turn
    // handler uses movement type $0E and therefore reads $90:9FFD. These fixture words are
    // shaped like the retail records but deliberately simple enough to audit by inspection.
    WriteTestWord(bus, 0x909f61, 0x0000);
    WriteTestWord(bus, 0x909f63, 0x3000);
    WriteTestWord(bus, 0x909f65, 0x0002);
    WriteTestWord(bus, 0x909f67, 0xc000);
    WriteTestWord(bus, 0x909f69, 0x0000);
    WriteTestWord(bus, 0x909f6b, 0x8000);
    WriteTestWord(bus, 0x909ffd, 0x0000);
    WriteTestWord(bus, 0x909fff, 0x3000);
    WriteTestWord(bus, 0x90a001, 0x0002);
    WriteTestWord(bus, 0x90a003, 0xc000);
    WriteTestWord(bus, 0x90a005, 0x0000);
    WriteTestWord(bus, 0x90a007, 0x4000);

    // A broad row of ordinary type-$8 solids removes slope scaling from this test. Center
    // Y=11 with radius 5 rests exactly on the row-one top edge at physical Y=16.
    const int width = 8;
    var foreground = new ushort[width * 3];
    for (int x = 0; x < width; x++)
        foreground[width + x] = 0x8000;
    RoomLevelData level = CreateRoom(
        width, 3, foreground, new byte[foreground.Length]);

    var movingLeft = new SamusState { Pose = SamusPoseIds.MovingLeftNormalPose };
    movingLeft.Kinematics.XPosition = 64;
    movingLeft.Kinematics.YPosition = 11;
    movingLeft.Kinematics.XRadius = 5;
    movingLeft.Kinematics.YRadius = 5;
    GroundedMovementResult leftFrame = SamusGroundedMovement.StepRunningLeft(
        bus,
        level,
        movingLeft,
        nmiFrameCounter: 0);
    AssertEqual(-0x00003000, leftFrame.Horizontal.AcceptedDisplacement, "running-left signed displacement");
    AssertEqual(63, movingLeft.XPosition, "running-left borrows into whole X");
    AssertEqual(0xd000, movingLeft.Kinematics.XSubposition, "running-left fractional X");
    AssertTrue(leftFrame.Vertical.Collided, "running-left grounding probe collides");

    // `$90:8EA9` does not reserve acceleration mode one for turn poses. If a transition
    // leaves it installed after the body has already become `$09/$0A`, the visible pose's
    // literal direction is inverted for that frame. Start above the deceleration quantum so
    // `$90:9B0A` does not clear the mode before the direction selector observes it.
    var reversedRunningRight = new SamusState { Pose = SamusPoseIds.MovingRightNormalPose };
    reversedRunningRight.Kinematics.XPosition = 64;
    reversedRunningRight.Kinematics.YPosition = 11;
    reversedRunningRight.Kinematics.XRadius = 5;
    reversedRunningRight.Kinematics.YRadius = 5;
    reversedRunningRight.HorizontalSpeed.BaseSpeed = 1;
    reversedRunningRight.HorizontalSpeed.AccelerationMode = 1;
    GroundedMovementResult reversedRightFrame = SamusGroundedMovement.StepRunningRight(
        bus,
        level,
        reversedRunningRight,
        nmiFrameCounter: 0);
    AssertEqual(-0x00008000, reversedRightFrame.Horizontal.AcceptedDisplacement,
        "mode-one running-right carries leftward momentum");

    var reversedRunningLeft = new SamusState { Pose = SamusPoseIds.MovingLeftNormalPose };
    reversedRunningLeft.Kinematics.XPosition = 64;
    reversedRunningLeft.Kinematics.YPosition = 11;
    reversedRunningLeft.Kinematics.XRadius = 5;
    reversedRunningLeft.Kinematics.YRadius = 5;
    reversedRunningLeft.HorizontalSpeed.BaseSpeed = 1;
    reversedRunningLeft.HorizontalSpeed.AccelerationMode = 1;
    GroundedMovementResult reversedLeftFrame = SamusGroundedMovement.StepRunningLeft(
        bus,
        level,
        reversedRunningLeft,
        nmiFrameCounter: 0);
    AssertEqual(0x00008000, reversedLeftFrame.Horizontal.AcceptedDisplacement,
        "mode-one running-left carries rightward momentum");

    // The production movement port now also validates the pose's literal dispatcher byte,
    // so seed the two ordinary turn definitions before isolating their displacement.
    bus.WriteBytes(0x91b751, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b759, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);

    // Pose $25 is already facing left ($04), but mode one makes $90:8EA9 choose the
    // opposite/right helper while its $0E speed record decelerates old momentum.
    var turnTowardLeft = new SamusState { Pose = SamusPoseIds.TurningRightToLeftPose };
    turnTowardLeft.Kinematics.XPosition = 64;
    turnTowardLeft.Kinematics.YPosition = 11;
    turnTowardLeft.Kinematics.XRadius = 5;
    turnTowardLeft.Kinematics.YRadius = 5;
    turnTowardLeft.HorizontalSpeed.BaseSpeed = 1;
    turnTowardLeft.HorizontalSpeed.AccelerationMode = 1;
    GroundedMovementResult carriedRight = SamusGroundedMovement.StepTurningOnGround(
        bus,
        level,
        turnTowardLeft,
        nmiFrameCounter: 0);
    AssertEqual(0xc000, turnTowardLeft.HorizontalSpeed.BaseSubspeed, "left-turn deceleration amount");
    AssertEqual(0x0000c000, carriedRight.Horizontal.AcceptedDisplacement, "left-turn carries rightward momentum");

    // Pose $26 is the mirror: it displays a right-facing turn but carries old momentum to
    // the left. This is not a host sign choice; it is the other branch of $90:8EA9.
    var turnTowardRight = new SamusState { Pose = SamusPoseIds.TurningLeftToRightPose };
    turnTowardRight.Kinematics.XPosition = 64;
    turnTowardRight.Kinematics.YPosition = 11;
    turnTowardRight.Kinematics.XRadius = 5;
    turnTowardRight.Kinematics.YRadius = 5;
    turnTowardRight.HorizontalSpeed.BaseSpeed = 1;
    turnTowardRight.HorizontalSpeed.AccelerationMode = 1;
    GroundedMovementResult carriedLeft = SamusGroundedMovement.StepTurningOnGround(
        bus,
        level,
        turnTowardRight,
        nmiFrameCounter: 1);
    AssertEqual(-0x0000c000, carriedLeft.Horizontal.AcceptedDisplacement, "right-turn carries leftward momentum");

    // Underflow clears speed and mode inside $90:9A7E before direction dispatch. The last
    // turn frame consequently requests exactly zero rather than crossing into new motion.
    var exhaustedTurn = new SamusState { Pose = SamusPoseIds.TurningRightToLeftPose };
    exhaustedTurn.Kinematics.XPosition = 64;
    exhaustedTurn.Kinematics.YPosition = 11;
    exhaustedTurn.Kinematics.XRadius = 5;
    exhaustedTurn.Kinematics.YRadius = 5;
    exhaustedTurn.HorizontalSpeed.BaseSubspeed = 0x2000;
    exhaustedTurn.HorizontalSpeed.AccelerationMode = 1;
    GroundedMovementResult stopped = SamusGroundedMovement.StepTurningOnGround(
        bus,
        level,
        exhaustedTurn,
        nmiFrameCounter: 0);
    AssertEqual(0, exhaustedTurn.HorizontalSpeed.BaseSubspeed, "turn underflow clears speed");
    AssertEqual(0, exhaustedTurn.HorizontalSpeed.AccelerationMode, "turn underflow clears mode one");
    AssertEqual(0, stopped.Horizontal.AcceptedDisplacement, "turn underflow does not reverse early");

    // Seed the exact retail pose metadata and delay bytecode needed by $09->$25->$02.
    // The compact standing/running streams only provide valid frame-zero delays because
    // this test's subject is the turn stream itself.
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b679, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b751, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b759, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b014, 0xc100); // pose $02
    WriteTestWord(bus, 0x91b022, 0xc110); // pose $09
    WriteTestWord(bus, 0x91b024, 0xc120); // pose $0A
    WriteTestWord(bus, 0x91b05a, 0xc130); // pose $25
    WriteTestWord(bus, 0x91b05c, 0xc140); // pose $26
    bus.WriteBytes(0x91c100, [0x0a]);
    bus.WriteBytes(0x91c110, [0x02]);
    bus.WriteBytes(0x91c120, [0x02]);
    bus.WriteBytes(0x91c130, [0x02, 0x02, 0x02, 0xf8, 0x02]);
    bus.WriteBytes(0x91c140, [0x02, 0x02, 0x02, 0xf8, 0x01]);

    // `$91:F8D3` explicitly compares the OLD pose against `$00/$9B` before it reads any
    // shot-direction metadata. Both front-view bodies must therefore retain the generic
    // direction selected by their own transition tables. Cover both targets and both suit
    // variants; this prevents a future shortcut from silently assigning front-view Samus
    // an invented left or right facing.
    var forwardToLeft = new SamusState { Pose = SamusPoseIds.ForwardFacingPowerSuitPose };
    forwardToLeft.HorizontalSpeed.BaseSubspeed = 0x8000;
    forwardToLeft.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
    forwardToLeft.ApplyGroundedTurn(bus, SamusPoseIds.TurningRightToLeftPose);
    AssertEqual(SamusPoseIds.TurningRightToLeftPose, forwardToLeft.Pose,
        "power-suit forward view retains generic left-turn target");
    AssertEqual(0xc000, forwardToLeft.HorizontalSpeed.BaseSubspeed,
        "forward left turn still folds extra run speed");
    AssertEqual(1, forwardToLeft.HorizontalSpeed.AccelerationMode,
        "forward left turn selects mode one");

    var forwardToRight = new SamusState { Pose = SamusPoseIds.ForwardFacingSuitedPose };
    forwardToRight.ApplyGroundedTurn(bus, SamusPoseIds.TurningLeftToRightPose);
    AssertEqual(SamusPoseIds.TurningLeftToRightPose, forwardToRight.Pose,
        "suited forward view retains generic right-turn target");
    AssertEqual(1, forwardToRight.HorizontalSpeed.AccelerationMode,
        "forward right turn selects mode one");

    var animatedTurn = new SamusState { Pose = SamusPoseIds.MovingRightNormalPose };
    animatedTurn.HorizontalSpeed.BaseSubspeed = 0x8000;
    animatedTurn.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
    animatedTurn.ApplyGroundedTurn(bus, SamusPoseIds.TurningRightToLeftPose);
    AssertEqual(0x25, animatedTurn.Pose, "input reversal installs pose $25");
    AssertEqual(0xc000, animatedTurn.HorizontalSpeed.BaseSubspeed, "turn setup folds extra into base speed");
    AssertEqual(0, animatedTurn.HorizontalSpeed.ExtraRunSubspeed, "turn setup consumes extra speed");
    AssertEqual(1, animatedTurn.HorizontalSpeed.AccelerationMode, "turn setup selects mode one");

    // Three two-tick art frames lead to byte index three, command $F8. Its operand $02 is
    // pending until the transition seam; applying it initializes pose $02 frame zero.
    for (int tick = 0; tick < 6; tick++)
        animatedTurn.AnimateNoFx(bus);
    AssertEqual(0xf8, animatedTurn.LastAnimationDelayCommand!.Value, "turn reaches command $F8");
    AssertEqual(0x02, animatedTurn.PendingTransitionalPose!.Value, "turn $F8 publishes left-standing pose");
    AssertTrue(animatedTurn.ApplyPendingVerifiedAnimationTransition(bus), "turn animation transition applies");
    AssertEqual(0x02, animatedTurn.Pose, "turn animation ends facing left");
    AssertEqual(0, animatedTurn.AnimationFrame, "turn completion resets animation frame");
    AssertEqual(10, animatedTurn.AnimationFrameTimer, "left-standing delay initializes from ROM stream");

    animatedTurn.ApplyStandingLeftToRunningLeft(bus);
    AssertEqual(0x0a, animatedTurn.Pose, "held left starts ordinary left run");
    animatedTurn.ApplyRunningLeftToStandingLeft(bus);
    AssertEqual(0x02, animatedTurn.Pose, "left run no-button fallback stands left");

    // The retail initializer does not blindly accept the generic `$25/$26` produced by
    // the input transition table. It reads byte three of the previous pose definition and
    // indexes `$91:F9C2`, preserving straight-up, diagonal-up, or diagonal-down aim. Seed
    // the six source definitions literally so this test will fail if either their native
    // metadata or the selector mapping is accidentally changed.
    (byte SourcePose, byte[] Definition)[] aimedSources =
    [
        (SamusPoseIds.StandingAimUpRightPose, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.StandingAimUpLeftPose, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.StandingAimDiagonalUpRightPose, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.StandingAimDiagonalUpLeftPose, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.StandingAimDiagonalDownRightPose, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.StandingAimDiagonalDownLeftPose, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte sourcePose, byte[] definition) in aimedSources)
    {
        WritePoseDefinition(bus, sourcePose, definition);

        // A ten-tick frame-zero stream is sufficient after `$F8` installs the standing
        // destination. No later target animation frame is observed by this focused test.
        ushort streamAddress = (ushort)(0xc200 + sourcePose * 2);
        WriteTestWord(bus, 0x91b010 + sourcePose * 2, streamAddress);
        bus.WriteByte(0x910000 + streamAddress, 0x0a);
    }

    // Each turn definition also comes directly from bank $91. `$FA` and `$FC` are the
    // aimed-turn shot-direction markers; they are intentionally preserved here rather
    // than normalized to ordinary direction bytes.
    (byte TurnPose, byte[] Definition)[] aimedTurns =
    [
        (SamusPoseIds.TurningRightToLeftAimUpPose, [0x04, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.TurningLeftToRightAimUpPose, [0x08, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.TurningRightToLeftAimDiagonalDownPose, [0x04, 0x0e, 0xff, 0xfc, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.TurningLeftToRightAimDiagonalDownPose, [0x08, 0x0e, 0xff, 0xfc, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.TurningRightToLeftAimDiagonalUpPose, [0x04, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.TurningLeftToRightAimDiagonalUpPose, [0x08, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte turnPose, byte[] definition) in aimedTurns)
        WritePoseDefinition(bus, turnPose, definition);

    // NTSC uses three two-tick art frames for every aimed grounded turn. `$F8` then
    // installs the corresponding opposite-facing standing-aim pose shown in this table.
    (byte SourcePose, byte GenericTurn, byte SelectedTurn, byte Destination)[] aimedTurnCases =
    [
        (SamusPoseIds.StandingAimUpRightPose, SamusPoseIds.TurningRightToLeftPose,
            SamusPoseIds.TurningRightToLeftAimUpPose, SamusPoseIds.StandingAimUpLeftPose),
        (SamusPoseIds.StandingAimUpLeftPose, SamusPoseIds.TurningLeftToRightPose,
            SamusPoseIds.TurningLeftToRightAimUpPose, SamusPoseIds.StandingAimUpRightPose),
        (SamusPoseIds.StandingAimDiagonalUpRightPose, SamusPoseIds.TurningRightToLeftPose,
            SamusPoseIds.TurningRightToLeftAimDiagonalUpPose, SamusPoseIds.StandingAimDiagonalUpLeftPose),
        (SamusPoseIds.StandingAimDiagonalUpLeftPose, SamusPoseIds.TurningLeftToRightPose,
            SamusPoseIds.TurningLeftToRightAimDiagonalUpPose, SamusPoseIds.StandingAimDiagonalUpRightPose),
        (SamusPoseIds.StandingAimDiagonalDownRightPose, SamusPoseIds.TurningRightToLeftPose,
            SamusPoseIds.TurningRightToLeftAimDiagonalDownPose, SamusPoseIds.StandingAimDiagonalDownLeftPose),
        (SamusPoseIds.StandingAimDiagonalDownLeftPose, SamusPoseIds.TurningLeftToRightPose,
            SamusPoseIds.TurningLeftToRightAimDiagonalDownPose, SamusPoseIds.StandingAimDiagonalDownRightPose),
    ];

    // Real standing/turn poses have radius 21, unlike the compact radius-five fixture used
    // above to isolate ordinary displacement arithmetic. Place their center at Y=27 over
    // a row-three floor: top Y=6 remains inside the room and bottom Y=48 touches that floor.
    var aimedForeground = new ushort[width * 5];
    for (int x = 0; x < width; x++)
        aimedForeground[width * 3 + x] = 0x8000;
    RoomLevelData aimedLevel = CreateRoom(
        width, 5, aimedForeground, new byte[aimedForeground.Length]);

    for (int caseIndex = 0; caseIndex < aimedTurnCases.Length; caseIndex++)
    {
        var testCase = aimedTurnCases[caseIndex];

        // Give every turn its own bytecode location. This catches swapped destinations
        // independently rather than allowing two cases to share a forgiving stream.
        ushort turnStreamAddress = (ushort)(0xc300 + caseIndex * 0x10);
        WriteTestWord(bus, 0x91b010 + testCase.SelectedTurn * 2, turnStreamAddress);
        bus.WriteBytes(
            0x910000 + turnStreamAddress,
            [0x02, 0x02, 0x02, 0xf8, testCase.Destination]);

        var aimedTurn = new SamusState { Pose = testCase.SourcePose };
        aimedTurn.Kinematics.XPosition = 64;
        aimedTurn.Kinematics.YPosition = 27;
        aimedTurn.Kinematics.XRadius = 5;
        aimedTurn.Kinematics.YRadius = 21;

        // `$91:F931` performs a 32-bit fixed-point add. These values force a fractional
        // carry, proving that the extra run component is folded before it is cleared.
        aimedTurn.HorizontalSpeed.BaseSpeed = 1;
        aimedTurn.HorizontalSpeed.BaseSubspeed = 0xd000;
        aimedTurn.HorizontalSpeed.ExtraRunSubspeed = 0x5000;
        aimedTurn.ApplyGroundedTurn(bus, testCase.GenericTurn);
        AssertEqual(testCase.SelectedTurn, aimedTurn.Pose, $"aimed turn selector case {caseIndex}");
        AssertEqual(2, aimedTurn.HorizontalSpeed.BaseSpeed, $"aimed turn speed carry case {caseIndex}");
        AssertEqual(0x2000, aimedTurn.HorizontalSpeed.BaseSubspeed, $"aimed turn folded fraction case {caseIndex}");
        AssertEqual(0, aimedTurn.HorizontalSpeed.ExtraRunSubspeed, $"aimed turn consumes extra fraction case {caseIndex}");
        AssertEqual(1, aimedTurn.HorizontalSpeed.AccelerationMode, $"aimed turn mode one case {caseIndex}");

        GroundedMovementResult carriedMomentum = SamusGroundedMovement.StepTurningOnGround(
            bus,
            aimedLevel,
            aimedTurn,
            nmiFrameCounter: (ushort)caseIndex);
        bool beganFacingRight = (caseIndex & 1) == 0;
        AssertTrue(
            beganFacingRight
                ? carriedMomentum.Horizontal.AcceptedDisplacement > 0
                : carriedMomentum.Horizontal.AcceptedDisplacement < 0,
            $"aimed turn preserves old momentum direction case {caseIndex}");

        for (int tick = 0; tick < 6; tick++)
            aimedTurn.AnimateNoFx(bus);
        AssertEqual(0xf8, aimedTurn.LastAnimationDelayCommand!.Value, $"aimed turn reaches $F8 case {caseIndex}");
        AssertEqual(testCase.Destination, aimedTurn.PendingTransitionalPose!.Value, $"aimed turn publishes destination case {caseIndex}");
        AssertTrue(aimedTurn.ApplyPendingVerifiedAnimationTransition(bus), $"aimed turn transition applies case {caseIndex}");
        AssertEqual(testCase.Destination, aimedTurn.Pose, $"aimed turn destination case {caseIndex}");
        AssertEqual(0, aimedTurn.AnimationFrame, $"aimed turn target frame zero case {caseIndex}");
        AssertEqual(10, aimedTurn.AnimationFrameTimer, $"aimed turn target timer case {caseIndex}");
    }

    // Crouched aim turns are the deliberate type-$17 oddity in this family. Give that
    // movement type the same conspicuous synthetic speed record as type `$0E`; selecting
    // the wrong table address would otherwise read zero-filled fixture memory and stop.
    WriteTestWord(bus, 0x90a069, 0x0000);
    WriteTestWord(bus, 0x90a06b, 0x3000);
    WriteTestWord(bus, 0x90a06d, 0x0002);
    WriteTestWord(bus, 0x90a06f, 0xc000);
    WriteTestWord(bus, 0x90a071, 0x0000);
    WriteTestWord(bus, 0x90a073, 0x4000);

    (byte SourcePose, byte[] Definition)[] crouchedSources =
    [
        (SamusPoseIds.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.CrouchingAimUpRightPose, [0x08, 0x05, 0x27, 0x00, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.CrouchingAimUpLeftPose, [0x04, 0x05, 0x28, 0x09, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.CrouchingAimDiagonalUpRightPose, [0x08, 0x05, 0x27, 0x01, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.CrouchingAimDiagonalUpLeftPose, [0x04, 0x05, 0x28, 0x08, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.CrouchingAimDiagonalDownRightPose, [0x08, 0x05, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.CrouchingAimDiagonalDownLeftPose, [0x04, 0x05, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((byte sourcePose, byte[] definition) in crouchedSources)
    {
        WritePoseDefinition(bus, sourcePose, definition);
        ushort streamAddress = (ushort)(0xc600 + sourcePose * 2);
        WriteTestWord(bus, 0x91b010 + sourcePose * 2, streamAddress);
        bus.WriteByte(0x910000 + streamAddress, 0x0a);
    }

    // `$43/$44` really are movement type `$0E`; the six aimed records really are `$17`.
    // Keeping those literal bytes in the fixture protects the strange native dispatcher
    // split from a future cleanup that might look attractive but be historically wrong.
    (byte TurnPose, byte[] Definition)[] crouchedTurns =
    [
        (SamusPoseIds.TurningRightToLeftCrouchingPose, [0x04, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningLeftToRightCrouchingPose, [0x08, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningRightToLeftCrouchingAimUpPose, [0x04, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningLeftToRightCrouchingAimUpPose, [0x08, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalDownPose, [0x04, 0x17, 0x28, 0xfc, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalDownPose, [0x08, 0x17, 0x28, 0xfc, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalUpPose, [0x04, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        (SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalUpPose, [0x08, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((byte turnPose, byte[] definition) in crouchedTurns)
        WritePoseDefinition(bus, turnPose, definition);

    (byte SourcePose, byte GenericTurn, byte SelectedTurn, byte Destination)[] crouchedTurnCases =
    [
        (SamusPoseIds.CrouchingRightPose, SamusPoseIds.TurningRightToLeftCrouchingPose,
            SamusPoseIds.TurningRightToLeftCrouchingPose, SamusPoseIds.CrouchingLeftPose),
        (SamusPoseIds.CrouchingLeftPose, SamusPoseIds.TurningLeftToRightCrouchingPose,
            SamusPoseIds.TurningLeftToRightCrouchingPose, SamusPoseIds.CrouchingRightPose),
        (SamusPoseIds.CrouchingAimUpRightPose, SamusPoseIds.TurningRightToLeftCrouchingPose,
            SamusPoseIds.TurningRightToLeftCrouchingAimUpPose, SamusPoseIds.CrouchingAimUpLeftPose),
        (SamusPoseIds.CrouchingAimUpLeftPose, SamusPoseIds.TurningLeftToRightCrouchingPose,
            SamusPoseIds.TurningLeftToRightCrouchingAimUpPose, SamusPoseIds.CrouchingAimUpRightPose),
        (SamusPoseIds.CrouchingAimDiagonalUpRightPose, SamusPoseIds.TurningRightToLeftCrouchingPose,
            SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalUpPose, SamusPoseIds.CrouchingAimDiagonalUpLeftPose),
        (SamusPoseIds.CrouchingAimDiagonalUpLeftPose, SamusPoseIds.TurningLeftToRightCrouchingPose,
            SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalUpPose, SamusPoseIds.CrouchingAimDiagonalUpRightPose),
        (SamusPoseIds.CrouchingAimDiagonalDownRightPose, SamusPoseIds.TurningRightToLeftCrouchingPose,
            SamusPoseIds.TurningRightToLeftCrouchingAimDiagonalDownPose, SamusPoseIds.CrouchingAimDiagonalDownLeftPose),
        (SamusPoseIds.CrouchingAimDiagonalDownLeftPose, SamusPoseIds.TurningLeftToRightCrouchingPose,
            SamusPoseIds.TurningLeftToRightCrouchingAimDiagonalDownPose, SamusPoseIds.CrouchingAimDiagonalDownRightPose),
    ];
    for (int caseIndex = 0; caseIndex < crouchedTurnCases.Length; caseIndex++)
    {
        var testCase = crouchedTurnCases[caseIndex];
        ushort turnStreamAddress = (ushort)(0xc800 + caseIndex * 0x10);
        WriteTestWord(bus, 0x91b010 + testCase.SelectedTurn * 2, turnStreamAddress);
        bus.WriteBytes(
            0x910000 + turnStreamAddress,
            [0x02, 0x02, 0x02, 0xf8, testCase.Destination]);

        var crouchedTurn = new SamusState { Pose = testCase.SourcePose };
        crouchedTurn.Kinematics.XPosition = 64;
        crouchedTurn.Kinematics.YPosition = 32;
        crouchedTurn.Kinematics.XRadius = 5;
        crouchedTurn.Kinematics.YRadius = 16;
        crouchedTurn.HorizontalSpeed.BaseSpeed = 1;
        crouchedTurn.HorizontalSpeed.BaseSubspeed = 0xd000;
        crouchedTurn.HorizontalSpeed.ExtraRunSubspeed = 0x5000;
        crouchedTurn.ApplyGroundedTurn(bus, testCase.GenericTurn);
        AssertEqual(testCase.SelectedTurn, crouchedTurn.Pose, $"crouched turn selector case {caseIndex}");
        AssertEqual(16, crouchedTurn.Kinematics.YRadius, $"crouched turn radius case {caseIndex}");
        AssertEqual(1, crouchedTurn.HorizontalSpeed.AccelerationMode, $"crouched turn mode one case {caseIndex}");

        GroundedMovementResult crouchedMomentum = SamusGroundedMovement.StepTurningOnGround(
            bus,
            aimedLevel,
            crouchedTurn,
            nmiFrameCounter: (ushort)caseIndex);
        bool beganFacingRight = (caseIndex & 1) == 0;
        AssertTrue(
            beganFacingRight
                ? crouchedMomentum.Horizontal.AcceptedDisplacement > 0
                : crouchedMomentum.Horizontal.AcceptedDisplacement < 0,
            $"crouched turn preserves old momentum direction case {caseIndex}");
        AssertTrue(crouchedMomentum.Vertical.Collided, $"crouched turn grounding branch case {caseIndex}");

        if (caseIndex == 2)
        {
            // The same `$97-$A3` pose is allowed to enter `$90:A790`'s simple aerial arm
            // when an actor or external displacement has made Y direction nonzero. This was
            // formerly an explicit open-dispatch crash despite requiring no new pose data.
            var airborneCrouchedTurn = new SamusState { Pose = testCase.SelectedTurn };
            airborneCrouchedTurn.Kinematics.XPosition = 64;
            airborneCrouchedTurn.Kinematics.YPosition = 32;
            airborneCrouchedTurn.Kinematics.XRadius = 5;
            airborneCrouchedTurn.Kinematics.YRadius = 16;
            airborneCrouchedTurn.Kinematics.YDirection = 1;
            airborneCrouchedTurn.Kinematics.YSpeed = 1;
            AerialMovementResult airborneResult = SamusAerialMovement.StepTurningInAir(
                bus,
                aimedLevel,
                airborneCrouchedTurn,
                nmiFrameCounter: 0);
            AssertTrue(airborneResult.Vertical is not null,
                "airborne crouched type-$17 turn executes simple Y movement");
        }

        for (int tick = 0; tick < 6; tick++)
            crouchedTurn.AnimateNoFx(bus);
        AssertEqual(0xf8, crouchedTurn.LastAnimationDelayCommand!.Value, $"crouched turn reaches $F8 case {caseIndex}");
        AssertEqual(testCase.Destination, crouchedTurn.PendingTransitionalPose!.Value, $"crouched turn publishes destination case {caseIndex}");
        AssertTrue(crouchedTurn.ApplyPendingVerifiedAnimationTransition(bus), $"crouched turn transition applies case {caseIndex}");
        AssertEqual(testCase.Destination, crouchedTurn.Pose, $"crouched turn destination case {caseIndex}");
    }

    Console.WriteLine("  Samus reversal: standing/crouched selectors, mode-one carry, grounded type-$17, and $F8 agree.");
}

/// <summary>
/// Locks movement type `$10` to the literal bank-$90/$91 behavior: pose direction is the
/// travel direction despite opposite-facing art, the options word gates entry, definition
/// byte two exits on zero input, and `$BF-$C4` remain grounded until their `$F8` command or
/// transition table starts a real jump.
/// </summary>
static void VerifySamusMoonwalking()
{
    var bus = new TestAddressSpace();

    // `$90:9F55 + 10h * 0Ch = $90:A015`. Use an unmistakable quarter-pixel acceleration
    // with a two-pixel cap and eighth-pixel deceleration; all stable moonwalk records must
    // select this entry instead of borrowing running's type-one record.
    WriteTestWord(bus, 0x90a015, 0x0000);
    WriteTestWord(bus, 0x90a017, 0x4000);
    WriteTestWord(bus, 0x90a019, 0x0002);
    WriteTestWord(bus, 0x90a01b, 0x0000);
    WriteTestWord(bus, 0x90a01d, 0x0000);
    WriteTestWord(bus, 0x90a01f, 0x2000);

    // `$BF-$C4` dispatch through ordinary grounded-turn movement type `$0E`, whose own
    // deceleration record remains independently visible during their three art frames.
    WriteTestWord(bus, 0x909ffd, 0x0000);
    WriteTestWord(bus, 0x909fff, 0x4000);
    WriteTestWord(bus, 0x90a001, 0x0002);
    WriteTestWord(bus, 0x90a003, 0x0000);
    WriteTestWord(bus, 0x90a005, 0x0000);
    WriteTestWord(bus, 0x90a007, 0x2000);

    (byte Pose, byte[] Definition, byte Fallback, int Direction)[] stable =
    [
        (SamusPoseIds.MoonwalkFacingLeftPose,
            [0x08, 0x10, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00], 0x02, 1),
        (SamusPoseIds.MoonwalkFacingRightPose,
            [0x04, 0x10, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00], 0x01, -1),
        (SamusPoseIds.MoonwalkAimUpLeftPose,
            [0x08, 0x10, 0x06, 0x08, 0x06, 0x00, 0x15, 0x00], 0x06, 1),
        (SamusPoseIds.MoonwalkAimUpRightPose,
            [0x04, 0x10, 0x05, 0x01, 0x06, 0x00, 0x15, 0x00], 0x05, -1),
        (SamusPoseIds.MoonwalkAimDownLeftPose,
            [0x08, 0x10, 0x08, 0x06, 0x06, 0x00, 0x15, 0x00], 0x08, 1),
        (SamusPoseIds.MoonwalkAimDownRightPose,
            [0x04, 0x10, 0x07, 0x03, 0x06, 0x00, 0x15, 0x00], 0x07, -1),
    ];
    foreach ((byte pose, byte[] definition, _, _) in stable)
        WritePoseDefinition(bus, pose, definition);

    // The standing definitions are the actual sources and no-button destinations for the
    // six records above. Their one-byte streams are enough because these focused checks do
    // not advance standing animation.
    (byte Pose, byte[] Definition)[] standing =
    [
        (0x01, [0x08, 0x00, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (0x02, [0x04, 0x00, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (0x05, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]),
        (0x06, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]),
        (0x07, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]),
        (0x08, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte pose, byte[] definition) in standing)
    {
        WritePoseDefinition(bus, pose, definition);
        ushort stream = (ushort)(0xc000 + pose);
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteByte(0x910000 + stream, 10);
    }

    // The disabled-option substitution enters ordinary right-to-left `$25`, including the
    // initializer's momentum fold and mode-one selection. `$25`'s short stream need not
    // complete here; merely initializing it proves the candidate was not retained.
    bus.WriteBytes(0x91b751, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b05a, 0xc100);
    bus.WriteBytes(0x91c100, [0x02, 0x02, 0x02, 0xf8, 0x02]);
    var disabled = new SamusState { Pose = SamusPoseIds.FacingRightNormalPose };
    disabled.HorizontalSpeed.BaseSubspeed = 0x4000;
    disabled.HorizontalSpeed.ExtraRunSubspeed = 0x2000;
    disabled.ApplyMoonwalkPoseChange(bus, SamusPoseIds.MoonwalkFacingRightPose, moonwalkEnabled: false);
    AssertEqual(SamusPoseIds.TurningRightToLeftPose, disabled.Pose,
        "disabled Moonwalk option substitutes ordinary turn");
    AssertEqual(0x6000, disabled.HorizontalSpeed.BaseSubspeed,
        "disabled Moonwalk substitution folds extra momentum");
    AssertEqual(1, disabled.HorizontalSpeed.AccelerationMode,
        "disabled Moonwalk substitution starts mode one");

    // `$A4-$A7/$E0-$E7` all use the ordinary standing input records. A backward direction
    // during any short landing animation can therefore nominate `$4A/$49`; the target's
    // native Moonwalk initializer still performs the option-disabled turn substitution.
    // Exercise the whole named family so spin/aim/fire landing coverage cannot regress to
    // the original `$A4/$A5`-only approximation.
    byte[] rightLandingPoses =
    [
        SamusPoseIds.NormalLandingRightPose,
        SamusPoseIds.SpinLandingRightPose,
        SamusPoseIds.LandingAimUpRightPose,
        SamusPoseIds.LandingAimDiagonalUpRightPose,
        SamusPoseIds.LandingAimDiagonalDownRightPose,
        SamusPoseIds.FiringLandingRightPose,
    ];
    foreach (byte landingPose in rightLandingPoses)
    {
        WritePoseDefinition(bus, landingPose,
            [0x08, 0x00, 0x01, 0x02, 0x00, 0x00, 0x15, 0x00]);
        var landingMoonwalk = new SamusState { Pose = landingPose };
        landingMoonwalk.ApplyMoonwalkPoseChange(
            bus,
            SamusPoseIds.MoonwalkFacingRightPose,
            moonwalkEnabled: false);
        AssertEqual(SamusPoseIds.TurningRightToLeftPose, landingMoonwalk.Pose,
            $"right landing ${landingPose:X2} honors disabled Moonwalk substitution");
    }

    byte[] leftLandingPoses =
    [
        SamusPoseIds.NormalLandingLeftPose,
        SamusPoseIds.SpinLandingLeftPose,
        SamusPoseIds.LandingAimUpLeftPose,
        SamusPoseIds.LandingAimDiagonalUpLeftPose,
        SamusPoseIds.LandingAimDiagonalDownLeftPose,
        SamusPoseIds.FiringLandingLeftPose,
    ];
    foreach (byte landingPose in leftLandingPoses)
    {
        WritePoseDefinition(bus, landingPose,
            [0x04, 0x00, 0x02, 0x07, 0x00, 0x00, 0x15, 0x00]);
        var landingMoonwalk = new SamusState { Pose = landingPose };
        landingMoonwalk.ApplyMoonwalkPoseChange(
            bus,
            SamusPoseIds.MoonwalkFacingLeftPose,
            moonwalkEnabled: false);
        AssertEqual(SamusPoseIds.TurningLeftToRightPose, landingMoonwalk.Pose,
            $"left landing ${landingPose:X2} honors disabled Moonwalk substitution");
    }

    // `$91:F8F3-$F903` is specific to a turn whose PREVIOUS movement type is Moonwalk.
    // It publishes the source pose's shot direction with tag `$0100`; ordinary standing
    // turns, including the disabled-option substitution above, must not create this word.
    AssertEqual(0, disabled.PoseTransitionShotDirection,
        "ordinary standing turn does not publish moonwalk shot bridge");
    var moonwalkBridge = new SamusState { Pose = SamusPoseIds.MoonwalkFacingRightPose };
    moonwalkBridge.ApplyGroundedTurn(bus, SamusPoseIds.TurningRightToLeftPose);
    AssertEqual(0x0102, moonwalkBridge.PoseTransitionShotDirection,
        "moonwalk turn publishes tagged source shot direction");
    moonwalkBridge.ClearPoseTransitionShotDirection();
    AssertEqual(0, moonwalkBridge.PoseTransitionShotDirection,
        "moonwalk shot bridge is one-current-handler state");

    // Enabled entry must preserve every exact candidate, not merely the unaimed pair.
    foreach ((byte target, _, byte fallback, _) in stable)
    {
        var candidate = new SamusState { Pose = fallback };
        ushort stream = (ushort)(0xc200 + target);
        WriteTestWord(bus, 0x91b010 + target * 2, stream);
        bus.WriteByte(0x910000 + stream, 2);
        candidate.ApplyMoonwalkPoseChange(bus, target, moonwalkEnabled: true);
        AssertEqual(target, candidate.Pose, $"enabled Moonwalk retains candidate ${target:X2}");

        // Command two's zero-controller fallback is immediate for movement type `$10` and
        // uses the target record's byte two. This helper applies that already-read byte at
        // the normal end-of-frame transition seam.
        candidate.ApplyMoonwalkPoseChange(bus, fallback, moonwalkEnabled: true);
        AssertEqual(fallback, candidate.Pose, $"moonwalk ${target:X2} fallback byte");
    }

    // A flat row of type-$8 solids isolates horizontal sign and the shared downward probe.
    const int width = 12;
    var foreground = new ushort[width * 3];
    for (int x = 0; x < width; x++)
        foreground[width + x] = 0x8000;
    RoomLevelData floor = CreateRoom(
        width, 3, foreground, new byte[foreground.Length]);
    foreach ((byte pose, _, _, int direction) in stable)
    {
        var walker = new SamusState { Pose = pose, XPosition = 80, YPosition = 11 };
        walker.Kinematics.XRadius = 5;
        walker.Kinematics.YRadius = 5;
        GroundedMovementResult movement = SamusGroundedMovement.StepMoonwalking(
            bus, floor, walker, nmiFrameCounter: 0);
        AssertEqual(direction * 0x4000, movement.Horizontal.AcceptedDisplacement,
            $"moonwalk ${pose:X2} uses literal reversed direction");
        AssertTrue(movement.Vertical.Collided, $"moonwalk ${pose:X2} probes floor");
    }

    // `$90:A697` enters the complete shared X routine. An owned run-speed pair survives
    // because movement type `$10` cannot accelerate but `$0B3C` is still set; an unowned
    // pair is cleared at `$90:9808` before displacement is assembled.
    var momentumMoonwalk = new SamusState
    {
        Pose = SamusPoseIds.MoonwalkFacingLeftPose,
        XPosition = 80,
        YPosition = 11,
    };
    momentumMoonwalk.Kinematics.XRadius = 5;
    momentumMoonwalk.Kinematics.YRadius = 5;
    momentumMoonwalk.HorizontalSpeed.HasRunningMomentum = true;
    momentumMoonwalk.HorizontalSpeed.ExtraRunSpeed = 1;
    GroundedMovementResult momentumMoonwalkFrame = SamusGroundedMovement.StepMoonwalking(
        bus,
        floor,
        momentumMoonwalk,
        nmiFrameCounter: 0);
    AssertEqual(0x00014000, momentumMoonwalkFrame.Horizontal.AcceptedDisplacement,
        "moonwalk preserves owned extra run speed");

    var unownedMoonwalk = new SamusState
    {
        Pose = SamusPoseIds.MoonwalkFacingLeftPose,
        XPosition = 80,
        YPosition = 11,
    };
    unownedMoonwalk.Kinematics.XRadius = 5;
    unownedMoonwalk.Kinematics.YRadius = 5;
    unownedMoonwalk.HorizontalSpeed.ExtraRunSpeed = 1;
    GroundedMovementResult unownedMoonwalkFrame = SamusGroundedMovement.StepMoonwalking(
        bus,
        floor,
        unownedMoonwalk,
        nmiFrameCounter: 0);
    AssertEqual(0x00004000, unownedMoonwalkFrame.Horizontal.AcceptedDisplacement,
        "moonwalk clears unowned extra run speed");
    AssertEqual(0, unownedMoonwalk.HorizontalSpeed.ExtraRunSpeed,
        "moonwalk publishes native unowned-speed zero write");

    // Seed the six literal turn/jump records and delay streams. Each stream contains three
    // two-tick frames followed by `$F8,$1A/$19`, exactly `$91:B45B-$B478` for NTSC.
    (byte Source, byte Target, byte[] Definition, byte SpinTarget)[] turns =
    [
        (0x4a, 0xbf, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00], 0x1a),
        (0x49, 0xc0, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00], 0x19),
        (0x76, 0xc1, [0x04, 0x0e, 0xff, 0xfa, 0x08, 0x00, 0x15, 0x00], 0x1a),
        (0x75, 0xc2, [0x08, 0x0e, 0xff, 0xfa, 0x08, 0x00, 0x15, 0x00], 0x19),
        (0x78, 0xc3, [0x04, 0x0e, 0xff, 0xfc, 0x08, 0x00, 0x15, 0x00], 0x1a),
        (0x77, 0xc4, [0x08, 0x0e, 0xff, 0xfc, 0x08, 0x00, 0x15, 0x00], 0x19),
    ];
    foreach ((_, byte target, byte[] definition, byte spinTarget) in turns)
    {
        WritePoseDefinition(bus, target, definition);
        ushort stream = (ushort)(0xc300 + (target - 0xbf) * 8);
        WriteTestWord(bus, 0x91b010 + target * 2, stream);
        bus.WriteBytes(0x910000 + stream, [0x02, 0x02, 0x02, 0xf8, spinTarget]);
    }

    // Spin endpoints and dry-air constants are read through production code after `$F8`.
    bus.WriteBytes(0x91b6f1, [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
    bus.WriteBytes(0x91b6f9, [0x04, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
    WriteTestWord(bus, 0x91b042, 0xc400);
    WriteTestWord(bus, 0x91b044, 0xc401);
    bus.WriteByte(0x91c400, 2);
    bus.WriteByte(0x91c401, 2);
    WriteTestWord(bus, 0x909eb9, 0x0004);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0x0000);

    foreach ((byte source, byte target, _, _) in turns)
    {
        var turn = new SamusState { Pose = source, XPosition = 80, YPosition = 27 };
        turn.RefreshCollisionRadii(bus);
        turn.HorizontalSpeed.BaseSpeed = 1;
        turn.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
        turn.ApplyMoonwalkTurnJump(bus, target);
        AssertEqual(target, turn.Pose, $"moonwalk ${source:X2} selects exact turn ${target:X2}");
        AssertEqual(1, turn.HorizontalSpeed.AccelerationMode,
            $"moonwalk turn ${target:X2} preserves reversal mode");
        AssertEqual(0, turn.Kinematics.YDirection,
            $"moonwalk turn ${target:X2} remains grounded before completion");
    }

    // Exercise the terminal command on one mirrored route. Six decrements consume three
    // two-tick art frames; applying `$F8,$1A` then creates the real 4.E000 upward launch.
    var animated = new SamusState { Pose = SamusPoseIds.MoonwalkFacingRightPose };
    animated.ApplyMoonwalkTurnJump(bus, SamusPoseIds.MoonwalkTurnJumpLeftPose);
    for (int tick = 0; tick < 6; tick++)
        animated.AnimateNoFx(bus);
    AssertEqual(0xf8, animated.LastAnimationDelayCommand!.Value,
        "moonwalk turn reaches command $F8");
    AssertEqual(SamusPoseIds.SpinJumpLeftPose, animated.PendingTransitionalPose!.Value,
        "moonwalk turn publishes literal spin-left operand");
    AssertTrue(animated.ApplyPendingVerifiedAnimationTransition(bus),
        "moonwalk terminal spin transition applies");
    AssertEqual(SamusPoseIds.SpinJumpLeftPose, animated.Pose,
        "moonwalk terminal command enters spin jump");
    AssertEqual(4, animated.Kinematics.YSpeed,
        "moonwalk terminal command loads dry-air jump speed");
    AssertEqual(0xe000, animated.Kinematics.YSubspeed,
        "moonwalk terminal command loads dry-air jump subspeed");
    AssertEqual(1, animated.Kinematics.YDirection,
        "moonwalk terminal command begins upward motion");

    Console.WriteLine("  Moonwalk: option gate, six stable routes, reversed X, fallback, and $BF-$C4 jump art agree.");
}

/// <summary>
/// Verifies movement type `$15` and `$91:EADE`'s block-only producer, including the
/// otherwise easy-to-erase one-pixel arm-pump movement performed for prospective runs.
/// </summary>
static void VerifySamusRanIntoWall()
{
    var bus = new TestAddressSpace();

    (byte Pose, byte[] Definition)[] wallPoses =
    [
        (SamusPoseIds.RanIntoWallRightPose,
            [0x08, 0x15, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.RanIntoWallLeftPose,
            [0x04, 0x15, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.RanIntoWallAimUpRightPose,
            [0x08, 0x15, 0x89, 0x01, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.RanIntoWallAimUpLeftPose,
            [0x04, 0x15, 0x8a, 0x08, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.RanIntoWallAimDownRightPose,
            [0x08, 0x15, 0x89, 0x03, 0x06, 0x00, 0x15, 0x00]),
        (SamusPoseIds.RanIntoWallAimDownLeftPose,
            [0x04, 0x15, 0x8a, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte pose, byte[] definition) in wallPoses)
    {
        WritePoseDefinition(bus, pose, definition);
        WriteTestWord(bus, 0x91b010 + pose * 2, 0xc500);
    }
    bus.WriteBytes(0x91c500, [0x10, 0xff]);

    // Minimal current/prospective definitions make direction and movement-type selection
    // auditable without copying unrelated animation data into this focused fixture.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b679, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);

    byte[] selectedByShotDirection =
    [
        SamusPoseIds.StandingAimUpRightPose,
        SamusPoseIds.RanIntoWallAimUpRightPose,
        SamusPoseIds.RanIntoWallRightPose,
        SamusPoseIds.RanIntoWallAimDownRightPose,
        SamusPoseIds.RanIntoWallRightPose,
        SamusPoseIds.RanIntoWallLeftPose,
        SamusPoseIds.RanIntoWallAimDownLeftPose,
        SamusPoseIds.RanIntoWallLeftPose,
        SamusPoseIds.RanIntoWallAimUpLeftPose,
        SamusPoseIds.StandingAimUpLeftPose,
    ];
    for (byte direction = 0; direction < selectedByShotDirection.Length; direction++)
    {
        WritePoseDefinitionByte(bus, SamusPoseIds.MovingRightNormalPose, 3, direction);
        AssertEqual(
            selectedByShotDirection[direction],
            SamusState.SelectRanIntoWallPose(bus, SamusPoseIds.MovingRightNormalPose),
            $"ran-into-wall shot selector {direction}");
    }
    WritePoseDefinitionByte(bus, SamusPoseIds.MovingRightNormalPose, 3, 2);

    const int width = 12;
    var openForeground = new ushort[width * 4];
    for (int x = 0; x < width; x++)
        openForeground[width * 2 + x] = 0x8000;
    RoomLevelData openFloor = CreateRoom(
        width, 4, openForeground, new byte[openForeground.Length]);

    // A clear prospective run really moves one pixel; it is not merely a collision query.
    var armPump = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 80,
        YPosition = 27,
    };
    armPump.Kinematics.XRadius = 5;
    armPump.Kinematics.YRadius = 5;
    byte? clearResult = armPump.CheckProspectiveRunningPoseForWall(
        bus,
        openFloor,
        SamusPoseIds.MovingRightNormalPose,
        currentXSpeedKilledByBlock: false,
        out BlockMoveResult? clearProbe);
    AssertTrue(clearResult is null, "clear arm-pump probe keeps prospective run");
    AssertTrue(clearProbe is { Collided: false }, "clear arm-pump probe reports no wall");
    AssertEqual(81, armPump.XPosition, "clear arm-pump probe retains one-pixel move");

    // Put a two-block-high wall immediately at X=96. Center 91/radius five has a current
    // right boundary at 95; the same +1.0000 request advances the sampled boundary to 96.
    var blockedForeground = (ushort[])openForeground.Clone();
    blockedForeground[6] = 0x8000;
    blockedForeground[width + 6] = 0x8000;
    RoomLevelData blockedFloor = CreateRoom(
        width, 4, blockedForeground, new byte[blockedForeground.Length]);
    var blocked = new SamusState
    {
        Pose = SamusPoseIds.FacingRightNormalPose,
        XPosition = 91,
        YPosition = 27,
    };
    blocked.Kinematics.XRadius = 5;
    blocked.Kinematics.YRadius = 5;
    byte? blockedResult = blocked.CheckProspectiveRunningPoseForWall(
        bus,
        blockedFloor,
        SamusPoseIds.MovingRightNormalPose,
        currentXSpeedKilledByBlock: false,
        out BlockMoveResult? blockedProbe);
    AssertEqual((byte?)SamusPoseIds.RanIntoWallRightPose, blockedResult,
        "blocked prospective run selects $89");
    AssertTrue(blockedProbe is { Collided: true }, "blocked arm-pump probe reports wall");
    AssertEqual(91, blocked.XPosition, "blocked arm-pump probe retains last-safe X");

    // A killed type-one move uses the CURRENT shot direction and performs no second probe.
    var killed = new SamusState { Pose = SamusPoseIds.MovingRightNormalPose };
    byte? killedResult = killed.CheckProspectiveRunningPoseForWall(
        bus,
        openFloor,
        prospectivePose: null,
        currentXSpeedKilledByBlock: true,
        out BlockMoveResult? killedProbe);
    AssertEqual((byte?)SamusPoseIds.RanIntoWallRightPose, killedResult,
        "killed running speed selects current wall pose");
    AssertTrue(killedProbe is null, "killed running speed skips one-pixel probe");

    // Every type-$15 pose executes no-base X, the shared grounding probe, and then clears
    // all five horizontal momentum words unconditionally.
    foreach ((byte pose, _) in wallPoses)
    {
        var stopped = new SamusState { Pose = pose, XPosition = 80, YPosition = 27 };
        stopped.Kinematics.XRadius = 5;
        stopped.Kinematics.YRadius = 5;
        stopped.HorizontalSpeed.BaseSpeed = 1;
        stopped.HorizontalSpeed.BaseSubspeed = 0x4000;
        GroundedMovementResult movement = SamusGroundedMovement.StepRanIntoWall(
            bus,
            openFloor,
            stopped,
            nmiFrameCounter: 0);
        AssertTrue(movement.Vertical.Collided, $"wall pose ${pose:X2} remains grounded");
        AssertEqual(0u, stopped.HorizontalSpeed.BaseFixed,
            $"wall pose ${pose:X2} clears base speed");
        AssertEqual(0, stopped.HorizontalSpeed.AccelerationMode,
            $"wall pose ${pose:X2} clears acceleration mode");
    }

    Console.WriteLine("  Ran into wall: ten-way selector, arm-pump pixel, six stable poses, grounding, and cleanup agree.");
}

/// <summary>
/// Sends a synthetic planar tile through DMA, OAM, OBSEL, and CGRAM so this tests the
/// complete sprite-to-pixel path rather than a helper decoder in isolation.
/// </summary>
}
