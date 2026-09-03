using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Locks the resident `$84:B703` room-scroll object and all four extension setup
    /// directions to the same bank-$94 collision path used by ordinary Samus movement.
    /// </summary>
    static void VerifyRoomScrollPlms()
    {
        var bus = new TestAddressSpace();
        const int width = 8;
        const int height = 8;
        const ushort population = 0x9000;
        const ushort scrollProgram = 0x9100;

        // Six-byte room records are copied in their native order: one resident trigger,
        // then right/left/down/up extensions. Extension setup mutates terrain immediately
        // and deletes its temporary slot; only `$B703` remains active afterward.
        bus.WriteBytes(0x8f0000 | population, [
            0x03, 0xb7, 0x03, 0x03, 0x00, 0x91,
            0x3b, 0xb6, 0x04, 0x03, 0x00, 0x80,
            0x3f, 0xb6, 0x02, 0x03, 0x00, 0x80,
            0x43, 0xb6, 0x03, 0x04, 0x00, 0x80,
            0x47, 0xb6, 0x03, 0x02, 0x00, 0x80,
            0x00, 0x00,
        ]);
        // `$84:8B55` consumes `(scroll index,value)` byte pairs until a negative index.
        bus.WriteBytes(0x8f0000 | scrollProgram, [0x01, 0x01, 0x80]);

        var level = new RoomLevelData(
            width,
            height,
            new ushort[width * height],
            new byte[width * height],
            new ushort[width * height],
            new byte[8]);
        var plms = new RoomPlmSystem();
        AssertEqual(5, plms.LoadRoomPopulation(
                bus,
                level,
                level.CreateBackgroundStreamer(),
                new SnesVram(),
                population,
                new Bank80SystemState(),
                0,
                () => new SamusState(),
                () => false),
            "single loader parses the trigger and four synchronous extensions");
        AssertEqual(1, plms.ScrollPlms.Count,
            "extension setup consumes no persistent PLM slot");

        int origin = level.GetBlockIndex(3, 3);
        AssertEqual(0x3000, level.GetCollisionBlockByIndex(origin).LevelWord,
            "B703 setup installs type-three special air");
        AssertEqual(0x46, level.GetCollisionBlockByIndex(origin).Behavior,
            "B703 setup installs BTS $46");
        AssertEqual(0x5000, level.GetCollisionBlock(4, 3).LevelWord,
            "right extension setup installs type five");
        AssertEqual(0xff, level.GetCollisionBlock(4, 3).Behavior,
            "right extension points one block left");
        AssertEqual(0x5000, level.GetCollisionBlock(2, 3).LevelWord,
            "left extension setup installs type five");
        AssertEqual(0x01, level.GetCollisionBlock(2, 3).Behavior,
            "left extension points one block right");
        AssertEqual(0xd000, level.GetCollisionBlock(3, 4).LevelWord,
            "down extension setup installs type D");
        AssertEqual(0xff, level.GetCollisionBlock(3, 4).Behavior,
            "down extension points one row up");
        AssertEqual(0xd000, level.GetCollisionBlock(3, 2).LevelWord,
            "up extension setup installs type D");
        AssertEqual(0x01, level.GetCollisionBlock(3, 2).Behavior,
            "up extension points one row down");

        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(
            bus, widthInScreens: 2, heightInScreens: 2,
            lastRowState: RoomScrollState.RedBoundary);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();

        // A direct touch wakes the resident object. Its next handler pass interprets the
        // room-owned bank-$8F bytecode, restores sleep/type-three state, and remains live.
        AssertTrue(plms.TryNotifyScrollTouch(origin),
            "direct special-air contact finds resident scroll owner");
        AssertTrue(plms.ScrollPlms[0].Triggered,
            "direct contact sets native PLM var bit fifteen");
        plms.Step(bus, level, streamer, 0, 0, 0, scrolls);
        AssertEqual(1, scrolls.ReadStorage(1),
            "scroll byte-pair program mutates active room grid");
        AssertTrue(!plms.ScrollPlms[0].Triggered,
            "scroll object returns to resident sleep after program terminator");

        // Place a seven-pixel-tall test body immediately left of the right extension. The
        // normal horizontal scanner reaches type five, follows signed BTS `$FF` to the
        // origin, and executes the same `$B393` wakeup—there is no route-specific callback.
        scrolls.SetStorage(1, RoomScrollState.RedBoundary);
        var body = new SamusKinematicsState
        {
            XPosition = 56,
            YPosition = 56,
            XRadius = 8,
            YRadius = 7,
        };
        BlockMoveResult movement = SamusBlockCollision.MoveHorizontal(
            bus,
            level,
            body,
            displacement: 1 << 16,
            plms: plms);
        AssertTrue(!movement.Collided,
            "scroll extension remains traversable special air");
        AssertTrue(plms.ScrollPlms[0].Triggered,
            "horizontal extension resolves and wakes B703 owner");
        plms.Step(bus, level, streamer, 0, 0, 0, scrolls);
        AssertEqual(1, scrolls.ReadStorage(1),
            "extension-triggered contact executes the same scroll program");

        // Repeat the identical contact through the public Morph Ball wrapper. This guards
        // the composition seam that originally dropped RoomPlmSystem while ordinary Samus
        // movement passed it correctly: type-five resolves to the resident `$B703` owner,
        // and the ball remains controller/collision driven throughout the call.
        scrolls.SetStorage(1, RoomScrollState.RedBoundary);
        WritePoseDefinition(bus, SamusPoseIds.MorphBallGroundRightPose,
            [0x08, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
        var ball = new SamusState
        {
            Pose = SamusPoseIds.MorphBallGroundRightPose,
            XPosition = 56,
            YPosition = 56,
        };
        ball.RefreshCollisionRadii(bus);
        ball.Kinematics.ExtraXDisplacement = 1;
        ball.Kinematics.ExtraXSubdisplacement = 0;
        MorphBallMovementResult ballMovement = SamusMorphBallMovement.StepGrounded(
            bus,
            level,
            ball,
            nmiFrameCounter: 0,
            plms: plms);
        AssertTrue(!ballMovement.Horizontal.Collided,
            "Morph Ball traverses scroll extension as special air");
        AssertTrue(plms.ScrollPlms[0].Triggered,
            "Morph Ball wrapper forwards live PLM owner to block collision");
        plms.Step(bus, level, streamer, 0, 0, 0, scrolls);
        AssertEqual(1, scrolls.ReadStorage(1),
            "Morph Ball scroll contact executes resident bank-$8F program");

        Console.WriteLine(
            "  Scroll PLMs: population setup, four extensions, collision wakeup, and bank-$8F programs agree.");
    }
}
