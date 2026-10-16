using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Scrolling;
using SuperMetroid.Verification.Harness;

/// <summary>Checks resident room-scroll PLMs through center contact, extensions, Morph Ball, and bomb jump.</summary>
internal static class ScrollPlmsVerification
{
    /// <summary>
    /// Locks the resident `$84:B703` room-scroll object and all four extension setup
    /// directions to the same bank-$94 collision path used by ordinary Samus movement.
    /// </summary>
    internal static void VerifyRoomScrollPlms()
    {
        var bus = new TestAddressSpace();
        const int width = 8;
        const int height = 8;
        const ushort population = 0x9000;
        const ushort scrollProgram = 0x9100;

        SeedRoomScrollPlmCartridge(bus, population, scrollProgram);

        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new ushort[width * height], new byte[width * height], new ushort[width * height]), new byte[8]);
        var plms = new RoomPlmSystem();
        AssertEqual(5, plms.PopulationLoader.LoadRoomPopulation(new PlmRoom(bus, level, level.CreateBackgroundStreamer(), new SnesVram(), new Bank80SystemState(), 0), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "single loader parses the trigger and four synchronous extensions");
        AssertEqual(1, plms.Scrolls.Snapshots.Count,
            "extension setup consumes no persistent PLM slot");

        int origin = level.GetBlockIndex(new(3, 3));
        AssertEqual(0x3000, level.GetCollisionBlockByIndex(origin).LevelWord,
            "B703 setup installs type-three special air");
        AssertEqual(0x46, level.GetCollisionBlockByIndex(origin).Behavior,
            "B703 setup installs BTS $46");
        AssertEqual(0x5000, level.GetCollisionBlock(new(4, 3)).LevelWord,
            "right extension setup installs type five");
        AssertEqual(0xff, level.GetCollisionBlock(new(4, 3)).Behavior,
            "right extension points one block left");
        AssertEqual(0x5000, level.GetCollisionBlock(new(2, 3)).LevelWord,
            "left extension setup installs type five");
        AssertEqual(0x01, level.GetCollisionBlock(new(2, 3)).Behavior,
            "left extension points one block right");
        AssertEqual(0xd000, level.GetCollisionBlock(new(3, 4)).LevelWord,
            "down extension setup installs type D");
        AssertEqual(0xff, level.GetCollisionBlock(new(3, 4)).Behavior,
            "down extension points one row up");
        AssertEqual(0xd000, level.GetCollisionBlock(new(3, 2)).LevelWord,
            "up extension setup installs type D");
        AssertEqual(0x01, level.GetCollisionBlock(new(3, 2)).Behavior,
            "up extension points one row down");

        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(
            new RoomScrollStorage(), size: new Extent16<RoomScreens>(2, 2),
            lastRowState: RoomScrollState.RedBoundary);
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();

        // $94:9956 reacts only to the center sample, including extension resolution.
        // Foot/head visits must not substitute for a center skipped by native row deduplication.
        foreach ((ushort centerY, ushort radius, bool shouldWake) in new[]
            { ((ushort)40, (ushort)21, false), ((ushort)72, (ushort)21, false),
              ((ushort)56, (ushort)7, false), ((ushort)56, (ushort)21, true) })
        {
            var insideSamus = new SamusState { Position = FixedPosition<RoomPixels>.FromWhole(new(72, centerY)) };
            insideSamus.Kinematics.Radius = insideSamus.Kinematics.Radius.WithY(radius);
            SamusInsideBlockReactions.PrepareFrame(level, insideSamus, AreaId.Crateria, plms: plms);
            AssertEqual(shouldWake, plms.ScrollPlmTriggered(origin),
                "inside scroll reaction admits only a separately visited center, not feet/head");
            plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, scrolls);
        }

        // A direct touch wakes the resident object. Its next handler pass interprets the
        // room-owned bank-$8F bytecode, restores sleep/type-three state, and remains live.
        AssertTrue(plms.Scrolls.TryNotifyScrollTouch(origin),
            "direct special-air contact finds resident scroll owner");
        AssertTrue(plms.ScrollPlmTriggered(origin),
            "direct contact sets native PLM var bit fifteen");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, scrolls);
        AssertEqual(1, scrolls.ReadStorage(1),
            "scroll byte-pair program mutates active room grid");
        AssertTrue(!plms.ScrollPlmTriggered(origin),
            "scroll object returns to resident sleep after program terminator");

        // Place a seven-pixel-tall test body immediately left of the right extension. The
        // normal horizontal scanner reaches type five, follows signed BTS `$FF` to the
        // origin, and executes the same `$B393` wakeup—there is no route-specific callback.
        scrolls.SetStorage(1, RoomScrollState.RedBoundary);
        var body = new SamusKinematicsState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(56, 56)),
            Radius = new(8, 7),
        };
        BlockMoveResult movement = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Bus = bus, Level = level, Plms = plms }, body, new HorizontalBlockMove { Displacement = 1 << 16 });
        AssertTrue(!movement.Collided,
            "scroll extension remains traversable special air");
        AssertTrue(plms.ScrollPlmTriggered(origin),
            "horizontal extension resolves and wakes B703 owner");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, scrolls);
        AssertEqual(1, scrolls.ReadStorage(1),
            "extension-triggered contact executes the same scroll program");

        // Repeat the identical contact through the public Morph Ball wrapper. This guards
        // the composition seam that originally dropped RoomPlmSystem while ordinary Samus
        // movement passed it correctly: type-five resolves to the resident `$B703` owner,
        // and the ball remains controller/collision driven throughout the call.
        scrolls.SetStorage(1, RoomScrollState.RedBoundary);
        WritePoseDefinition(bus, (int)SamusPoseId.MorphBallGroundRightPose,
            [0x08, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
        var ball = new SamusState
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(56, 56)),
        };
        ball.PoseMetadata.RefreshCollisionRadii();
        ball.Kinematics.ExtraDisplacement = ball.Kinematics.ExtraDisplacement.WithX(SnesSignedSixteenSixteen.FromParts(1, 0));
        ushort ballStartX = ball.Position.X.Whole;
        SamusMorphBallMovement.StepGrounded(
            bus,
            level,
            ball,
            nmiFrameCounter: 0,
            plms: plms);
        AssertTrue(ball.Position.X.Whole > ballStartX,
            "Morph Ball traverses scroll extension as special air");
        AssertTrue(plms.ScrollPlmTriggered(origin),
            "Morph Ball wrapper forwards live PLM owner to block collision");
        plms.Step(level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 }, scrolls);
        AssertEqual(1, scrolls.ReadStorage(1),
            "Morph Ball scroll contact executes resident bank-$8F program");

        // Reproduce the recorded block-940 failure through the distinct `$90:E032`
        // bomb-jump handler. The upward scanner reaches the type-D extension immediately
        // above the resident trigger and must resolve it back to the same `$B703` owner.
        // Ordinary Morph Ball movement already forwards that owner; this assertion guards
        // the special movement-handler composition seam used only during bomb-jump ascent.
        scrolls.SetStorage(1, RoomScrollState.RedBoundary);
        var bombJump = new SamusState
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = (ushort)SamusEquipmentFlags.Bombs,
            Position = FixedPosition<RoomPixels>.FromWhole(new(56, 40)),
        };
        bombJump.PoseMetadata.RefreshCollisionRadii();
        bombJump.Kinematics.YAcceleration = 0;
        bombJump.Kinematics.YSubacceleration = 0;
        bombJump.RequestMorphedBombJump(2);
        SamusBombJumpMovement.Start(bombJump);
        bombJump.Kinematics.YSpeed = 1;
        bombJump.Kinematics.YSubspeed = 0;
        SamusBombJumpMovement.Step(
            bus,
            level,
            bombJump,
            nmiFrameCounter: 0,
            plms);
        AssertTrue(bombJump.BombJumpActive,
            "unobstructed scroll extension does not terminate bomb-jump ascent");
        AssertTrue(plms.ScrollPlmTriggered(origin),
            "bomb-jump wrapper forwards live PLM owner to block collision");

        Console.WriteLine(
            "  Scroll PLMs: population setup, four extensions, collision wakeup, and bank-$8F programs agree.");
    }

    /// <summary>
    /// Seeds the room-scroll PLM population and its scroll program. Six-byte room records are
    /// copied in their native order: one resident `$B703` trigger whose argument selects
    /// <paramref name="scrollProgram"/>, then right/left/down/up extensions. Extension setup
    /// mutates terrain immediately and deletes its temporary slot; only `$B703` remains active.
    /// `$84:8B55` consumes `(scroll index,value)` byte pairs until a negative index.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for the room-scroll PLM population and scroll program")]
    static void SeedRoomScrollPlmCartridge(TestAddressSpace bus, ushort population, ushort scrollProgram)
    {
        bus.WriteBytes(0x8f0000 | population, [
            0x03, 0xb7, 0x03, 0x03, 0x00, 0x91,
            0x3b, 0xb6, 0x04, 0x03, 0x00, 0x80,
            0x3f, 0xb6, 0x02, 0x03, 0x00, 0x80,
            0x43, 0xb6, 0x03, 0x04, 0x00, 0x80,
            0x47, 0xb6, 0x03, 0x02, 0x00, 0x80,
            0x00, 0x00,
        ]);
        bus.WriteBytes(0x8f0000 | scrollProgram, [0x01, 0x01, 0x80]);
    }
}
