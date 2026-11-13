using SuperMetroid.AssetExtraction;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Rooms.Scrolling;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks resident room-scroll PLMs through center contact, extensions, Morph Ball, and bomb jump.</summary>
internal static class ScrollObjectsVerification
{
    /// <summary>
    /// Locks the resident `$84:B703` room-scroll object and all four extension setup
    /// directions to the same bank-$94 collision path used by ordinary Samus movement.
    /// </summary>
    internal static void VerifyRoomScrollObjects()
    {
        var bus = new TestAddressSpace();
        const int width = 8;
        const int height = 8;
        const ushort population = 0x9000;
        const ushort scrollProgram = 0x9100;

        SeedRoomScrollObjectCartridge(bus, population, scrollProgram);

        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(new RoomTerrainBlock[width * height], new RoomBlockBehavior[width * height], new RoomBlockAppearance[width * height]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertEqual(5, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(new SharedRandom()), 0), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
            "single loader parses the trigger and four synchronous extensions");
        AssertEqual(1, roomObjects.Scrolls.Snapshots.Count,
            "extension setup consumes no persistent PLM slot");

        var origin = new Position16<RoomBlocks>(3, 3);
        AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.SpecialAir), level.GetCollisionBlock(origin).Terrain,
            "scroll trigger setup installs special air");
        AssertEqual(RoomBlockBehavior.Reacting(BlockReactionDefinitions.ScrollTrigger), level.GetCollisionBlock(origin).Behavior,
            "scroll trigger setup installs the scroll-trigger behaviour");
        AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.HorizontalExtension), level.GetCollisionBlock(new(4, 3)).Terrain,
            "right extension setup installs a horizontal extension");
        AssertEqual(-1, level.GetCollisionBlock(new(4, 3)).Behavior.ExtensionOffset,
            "right extension points one block left");
        AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.HorizontalExtension), level.GetCollisionBlock(new(2, 3)).Terrain,
            "left extension setup installs a horizontal extension");
        AssertEqual(1, level.GetCollisionBlock(new(2, 3)).Behavior.ExtensionOffset,
            "left extension points one block right");
        AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.VerticalExtension), level.GetCollisionBlock(new(3, 4)).Terrain,
            "down extension setup installs a vertical extension");
        AssertEqual(-1, level.GetCollisionBlock(new(3, 4)).Behavior.ExtensionOffset,
            "down extension points one row up");
        AssertEqual(RoomTerrainBlock.Of(0x000, BlockMirroring.None, RoomCollisionType.VerticalExtension), level.GetCollisionBlock(new(3, 2)).Terrain,
            "up extension setup installs a vertical extension");
        AssertEqual(0x01, RoomBlockBehaviorCodec.Encode(level.GetCollisionBlock(new(3, 2)).Behavior),
            "up extension points one row down");

        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(
            RoomScrollGrid.PowerOnCells(), size: new Extent16<RoomScreens>(2, 2),
            lastRowState: RoomScrollState.RedBoundary);

        // $94:9956 reacts only to the center sample, including extension resolution.
        // Foot/head visits must not substitute for a center skipped by native row deduplication.
        foreach ((ushort centerY, ushort radius, bool shouldWake) in new[]
            { new VerifyRoomScrollObjectsCase(40, 21, false), new VerifyRoomScrollObjectsCase(72, 21, false),
              new VerifyRoomScrollObjectsCase(56, 7, false), new VerifyRoomScrollObjectsCase(56, 21, true) })
        {
            var insideSamus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()) { Position = FixedPosition<RoomPixels>.FromWhole(new(72, centerY)) };
            insideSamus.Kinematics.Radius = insideSamus.Kinematics.Radius.WithY(radius);
            SamusInsideBlockReactions.PrepareFrame(level, insideSamus, AreaId.Crateria, roomObjects: roomObjects);
            AssertEqual(shouldWake, roomObjects.ScrollObjectTriggered(origin),
                "inside scroll reaction admits only a separately visited center, not feet/head");
            roomObjects.Step(level, scrolls);
        }

        // A direct touch wakes the resident object. Its next handler pass interprets the
        // room-owned bank-$8F bytecode, restores sleep/type-three state, and remains live.
        AssertTrue(roomObjects.Scrolls.TryNotifyScrollTouch(origin),
            "direct special-air contact finds resident scroll owner");
        AssertTrue(roomObjects.ScrollObjectTriggered(origin),
            "direct contact sets native PLM var bit fifteen");
        roomObjects.Step(level, scrolls);
        AssertEqual(RoomScrollState.Blue, scrolls.ReadState(1),
            "scroll program mutates active room grid");
        AssertTrue(!roomObjects.ScrollObjectTriggered(origin),
            "scroll object returns to resident sleep after program terminator");

        // Place a seven-pixel-tall test body immediately left of the right extension. The
        // normal horizontal scanner reaches type five, follows signed BTS `$FF` to the
        // origin, and executes the same `$B393` wakeup—there is no route-specific callback.
        scrolls.SetCell(1, RoomScrollState.RedBoundary);
        var body = new SamusKinematicsState
        {
            Position = FixedPosition<RoomPixels>.FromWhole(new(56, 56)),
            Radius = new(8, 7),
        };
        BlockMoveResult movement = SamusBlockCollision.MoveHorizontal(new SamusTerrain { Level = level, RoomObjects = roomObjects }, body, null, new HorizontalBlockMove { Displacement = SignedSixteenSixteen.FromWholePixels(1) });
        AssertTrue(!movement.Collided,
            "scroll extension remains traversable special air");
        AssertTrue(roomObjects.ScrollObjectTriggered(origin),
            "horizontal extension resolves and wakes B703 owner");
        roomObjects.Step(level, scrolls);
        AssertEqual(RoomScrollState.Blue, scrolls.ReadState(1),
            "extension-triggered contact executes the same scroll program");

        // Repeat the identical contact through the public Morph Ball wrapper. This guards
        // the composition seam that originally dropped RoomObjectSystem while ordinary Samus
        // movement passed it correctly: type-five resolves to the resident `$B703` owner,
        // and the ball remains controller/collision driven throughout the call.
        scrolls.SetCell(1, RoomScrollState.RedBoundary);
        WritePoseDefinition(bus, SamusPoseNumbers.Number(SamusPoseId.MorphBallGroundRightPose),
            [0x08, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
        var ball = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(56, 56)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(ball);
        ball.Kinematics.ExtraDisplacement = ball.Kinematics.ExtraDisplacement.WithX(SignedSixteenSixteen.FromParts(1, 0));
        ushort ballStartX = ball.Position.X.Whole;
        SamusMorphBallMovement.StepGrounded(

            level,
            ball,
            frameCounter: 0,
            roomObjects: roomObjects);
        AssertTrue(ball.Position.X.Whole > ballStartX,
            "Morph Ball traverses scroll extension as special air");
        AssertTrue(roomObjects.ScrollObjectTriggered(origin),
            "Morph Ball wrapper forwards live PLM owner to block collision");
        roomObjects.Step(level, scrolls);
        AssertEqual(RoomScrollState.Blue, scrolls.ReadState(1),
            "Morph Ball scroll contact executes resident bank-$8F program");

        // Reproduce the recorded block-940 failure through the distinct `$90:E032`
        // bomb-jump handler. The upward scanner reaches the type-D extension immediately
        // above the resident trigger and must resolve it back to the same `$B703` owner.
        // Ordinary Morph Ball movement already forwards that owner; this assertion guards
        // the special movement-handler composition seam used only during bomb-jump ascent.
        scrolls.SetCell(1, RoomScrollState.RedBoundary);
        var bombJump = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())
        {
            Pose = SamusPoseId.MorphBallGroundRightPose,
            EquippedItems = SamusEquipmentFlags.Bombs,
            Position = FixedPosition<RoomPixels>.FromWhole(new(56, 40)),
        };
        SamusPoseMetadata.RefreshCollisionRadii(bombJump);
        bombJump.Kinematics.YAcceleration = default;
        bombJump.RequestMorphedBombJump(SamusBombJumpDirection.Straight);
        SamusBombJumpMovement.Start(bombJump);
        bombJump.Kinematics.YSpeed = new UnsignedSixteenSixteen(1, 0);
        SamusBombJumpMovement.Step(

            level,
            bombJump,
            frameCounter: 0,
            roomObjects);
        AssertTrue(bombJump.BombJumpActive,
            "unobstructed scroll extension does not terminate bomb-jump ascent");
        AssertTrue(roomObjects.ScrollObjectTriggered(origin),
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
    static void SeedRoomScrollObjectCartridge(TestAddressSpace bus, ushort population, ushort scrollProgram)
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
