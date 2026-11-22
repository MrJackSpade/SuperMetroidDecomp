using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.Chozo;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.World;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Rooms.RoomObjects;

/// <summary>
/// Confirms #2502: the room objects whose original setups and instructions rewrite only a block's type, or its type and
/// parameter, change only each block's collision kind and behaviour, keeping its picture and what it shows. Each case runs
/// its setup in a room whose layout shows its terrain's own pictures, as retail layouts do, and in the same room whose layout
/// shows another picture, as a user-edited installation may: both rooms' blocks then collide and behave alike, and the
/// edited room's blocks go on showing what they showed. A suite names every case whose blocks did not.
/// </summary>
internal static class RoomObjectCollisionOnlyWritesVerification
{
    /// <summary>The picture every edited fixture block shows instead of its terrain's.</summary>
    private static readonly RoomBlockAppearance OtherPicture = RoomBlockAppearance.Of(0x77);

    /// <summary>The fixture's room size, wide and tall enough for the Wrecked Ship statue's fixed slope blocks.</summary>
    private static readonly Extent16<RoomBlocks> FixtureSize = new(48, 32);

    /// <summary>A plain solid block of the fixture's picture.</summary>
    private static readonly RoomTerrainBlock Solid = Terrain(RoomCollisionType.SolidBlock);

    /// <summary>
    /// The coloured, grey and blue door setups (<c>$84:C7B1</c>, <c>$84:C794</c>, <c>$84:C7BB</c>) and the passable
    /// block's (<c>$84:B3C1</c>), which Mother Brain's escape-room gate and a door's closing cap run.
    /// </summary>
    internal static void VerifyDoorSetupsKeepWhatTheirBlocksShow()
    {
        var kept = new KeptPictures();
        var block = new Position16<RoomBlocks>(6, 6);
        var door = new DoorLock(LockedDoorDefinitions.KraidEyeDoorEyeDoorFacingLeft, DoorOpeningPersistence.Persistent);
        kept.Check("the coloured door's setup", Solid, [block], level =>
            DoorObjects.SetupColoredDoor(level, PlaceAt(RoomObjectFixtures.Unbound(), block),
                new ColoredDoorVariant(ColoredDoorColor.Red, ColoredDoorOrientation.Left), door));
        kept.Check("the grey door's setup", Solid, [block], level =>
        {
            RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
            roomObjects.GreyDoors.SetupGreyDoor(level, PlaceAt(roomObjects, block),
                new GreyDoorVariant(ColoredDoorOrientation.Left, GreyDoorClosing.AtOnce), new GreyDoorOpening(GreyDoorCondition.Never, door));
        });
        kept.Check("the blue door's opening", Terrain(RoomCollisionType.ShootableBlock), [block], level =>
            RoomObjectFixtures.Unbound().DoorObjects.SpawnBlueDoorOpening(level, block,
                RoomBlockBehavior.Reacting(BlockReactionDefinitions.BlueDoorCaps.ReactionFor(ColoredDoorOrientation.Left)),
                BlockStrike.GrappleTouch.Instance));
        kept.Check("the passable block's setup", Terrain(RoomCollisionType.SpecialAir), [block], level =>
            DoorObjects.SetupPassableBlock(level, PlaceAt(RoomObjectFixtures.Unbound(), block)));
        kept.AssertEveryCaseKept();
    }

    /// <summary>
    /// Draygon's cannon setup (<c>$84:DE94</c>) and its destruction (<c>$84:DB8E</c>), Mother Brain's glass
    /// (<c>$84:D5F6</c>), the Baby Metroid's wall (<c>$84:B551</c>), Kraid's room changes and Phantoon's door, whose setups
    /// deactivate their block (<c>$84:B3C1</c>).
    /// </summary>
    internal static void VerifyBossRoomSetupsKeepWhatTheirBlocksShow()
    {
        var kept = new KeptPictures();
        var cannon = new Position16<RoomBlocks>(6, 6);
        Position16<RoomBlocks>[] cannonBlocks = [cannon, cannon + new Offset16<RoomBlocks>(0, 1)];
        kept.Check("Draygon's cannon setup", Solid, cannonBlocks, level =>
        {
            RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
            roomObjects.DraygonCannons.SetupDraygonCannon(level, PlaceAt(roomObjects, cannon),
                new DraygonWallCannonSetup(DraygonWallCannon.UpperLeft, DraygonCannonOrientation.Right, DraygonCannonStart.Whole));
        });
        kept.Check("Draygon's destroyed cannon", Solid, cannonBlocks, level => DraygonCannonObjects.WriteDestroyedTerrain(level, cannon));
        var glass = new Position16<RoomBlocks>(8, 8);
        kept.Check("Mother Brain's glass", Solid, [glass], level =>
        {
            RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
            roomObjects.MotherBrainGlass.SetupMotherBrainGlass(level, PlaceAt(roomObjects, glass));
        });
        var wall = new Position16<RoomBlocks>(8, 2);
        kept.Check("the Shitroid's wall", Solid, [.. Enumerable.Range(0, 10).Select(row => wall + new Offset16<RoomBlocks>(0, row))],
            level =>
            RoomObjectFixtures.Unbound().BossRoomMutations.BabyMetroidWall.Spawn(level, new(wall, BabyMetroidWallMutation.Clear)));
        var kraid = new Position16<RoomBlocks>(5, 5);
        kept.Check("Kraid's ceiling clearing", Terrain(RoomCollisionType.SpecialBlock), [kraid], level =>
            RoomObjectFixtures.Unbound().BossRoomMutations.KraidRoom.Spawn(level, new(kraid, KraidRoomMutation.ClearCeiling)));
        kept.Check("Phantoon's door", Terrain(RoomCollisionType.SpecialBlock), [PhantoonDoorChangeDefinitions.Block], level =>
            RoomObjectFixtures.Unbound().BossRoomMutations.PhantoonDoor.Spawn(level, PhantoonDoorChange.FightDoor));
        kept.AssertEveryCaseKept();
    }

    /// <summary>
    /// The Wrecked Ship statue's hand trigger (<c>$84:D620</c>), its hand's setup (<c>$84:D616</c>) and its slopes'
    /// changes (<c>$84:D3D7</c>, <c>$84:D3F4</c>).
    /// </summary>
    internal static void VerifyChozoStatueSetupsKeepWhatTheirBlocksShow()
    {
        var kept = new KeptPictures();
        var hand = new Position16<RoomBlocks>(10, 10);
        SamusState samus = NewSamus();
        kept.Check("the Wrecked Ship hand's trigger", Terrain(RoomCollisionType.SpecialBlock), [hand], level =>
        {
            level.SetBehavior(hand, ChozoStatueRoomDefinitions.WreckedShipHandBehavior);
            var system = new SharedGameState(new SharedRandom());
            system.RecordBossesDefeated(AreaId.WreckedShip, DefeatedBosses.AreaBoss);
            int activations = 0;
            RoomObjectSystem roomObjects = RoomObjectFixtures.Bound(new RoomObjectRoom(level, system, AreaId.WreckedShip),
                RoomObjectServiceFixtures.Uninstalled with { Enemies = new FixtureObjectEnemyEffects { ChozoStatueHandActivated = _ => activations++ } },
                RoomObjectFixtures.Unbound());
            roomObjects.ChozoStatue.NotifyChozoStatueHandCollision(level, hand, samus, SamusPoseId.MorphBallGroundRightPose, movingDown: true);
            AssertEqual(1, activations, "the morphed Samus landing on the hand wakes the statue");
        });
        kept.Check("the Wrecked Ship hand's setup", Solid, [hand], level =>
            RoomObjectFixtures.Unbound().ChozoStatue.SpawnChozoStatueObject(level, new(hand, ChozoStatueRoomObject.WreckedShipHand)));
        Position16<RoomBlocks>[] slopes = [ChozoStatueRoomDefinitions.FirstSlopeBlock, ChozoStatueRoomDefinitions.FirstSlopeBlock + new Offset16<RoomBlocks>(1, 0)];
        kept.Check("the Wrecked Ship spikes' turn into slopes", Solid, slopes, ChozoStatueObjects.TransformSpikesToSlopes);
        kept.Check("the Wrecked Ship slopes' return to spikes", Solid, slopes, ChozoStatueObjects.RevertSlopesToSpikes);
        kept.AssertEveryCaseKept();
    }

    /// <summary>
    /// An enemy's break of a breakable block (<c>$84:B3D4</c>), a permanent shot block's break (<c>$84:B3C1</c>) and a
    /// Brinstar plant's catch (<c>$84:B0DC</c>), whose setups clear or deactivate only their block's type.
    /// </summary>
    internal static void VerifyTerrainBreakSetupsKeepWhatTheirBlocksShow()
    {
        var kept = new KeptPictures();
        var block = new Position16<RoomBlocks>(4, 4);
        kept.Check("an enemy's break of a breakable block", Solid, [block], level =>
            RoomObjectFixtures.Unbound().BreakableTerrain.SpawnEnemyBreakableBlock(level, block));
        kept.Check("a permanent shot block's break", Terrain(RoomCollisionType.ShootableBlock), [block], level =>
        {
            RoomBlockBehavior behavior = RoomBlockBehavior.Reacting(ShootableReaction.PermanentBreak1x1);
            level.SetBehavior(block, behavior);
            AssertTrue(RoomObjectFixtures.Unbound().BlockReactions.TrySpawnProjectileShotBlock(level, block, ShotBlockReactions.ShotBlocks[behavior.Shootable], BlockStrike.GrappleTouch.Instance,
                solidBlock: true), "the break takes a place");
        });
        var plant = new Position16<RoomBlocks>(1, 1);
        SamusState samus = NewSamus();
        samus.Position = samus.Position.WithWhole(new(24, 0));
        samus.Kinematics.Radius = new(8, 16);
        kept.Check("a floor plant's catch", Terrain(RoomCollisionType.SpecialAir), [plant], level =>
        {
            RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
            roomObjects.SamusEater.TrySpawnSamusEater(level, level.GetCollisionBlock(plant), SamusEaterObjectDefinitions.Floor, samus.RoomView);
            AssertEqual(1, roomObjects.Places.ActiveCount, "the aligned plant takes a place");
        });
        kept.AssertEveryCaseKept();
    }

    /// <summary>An item's setup (<c>$84:EE64</c>, <c>$84:EE8E</c>) and the n00b tube's (<c>$84:D6CC</c>).</summary>
    internal static void VerifyItemAndTubeSetupsKeepWhatTheirBlocksShow()
    {
        var kept = new KeptPictures();
        var item = new Position16<RoomBlocks>(5, 5);
        foreach (CollectiblePresentation presentation in new[] { CollectiblePresentation.Exposed, CollectiblePresentation.ShotBlock })
        {
            kept.Check($"an energy tank's setup ({presentation})", Solid, [item], level =>
            {
                RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
                roomObjects.CollectibleObjects.SetupCollectible(new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria),
                    PlaceAt(roomObjects, item), new PermanentCollectibleObject(InWorldCollectibleKind.EnergyTank, presentation),
                    new CollectibleIdentity(ItemLocationDefinitions.CrateriaPowerBombsExposedPowerBombTank));
            });
        }
        var tube = new Position16<RoomBlocks>(7, 7);
        kept.Check("the n00b tube's setup", Terrain(RoomCollisionType.SpecialBlock), [tube], level =>
        {
            RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
            roomObjects.NoobTube.SetupNoobTube(level, PlaceAt(roomObjects, tube));
        });
        kept.AssertEveryCaseKept();
    }

    /// <summary>
    /// A save station's setup (<c>$84:B5EE</c>), an energy station's with its access blocks (<c>$84:B21D</c>) and an
    /// elevator platform's, which deactivates its block (<c>$84:B3C1</c>).
    /// </summary>
    internal static void VerifyStationSetupsKeepWhatTheirBlocksShow()
    {
        var kept = new KeptPictures();
        var station = new Position16<RoomBlocks>(6, 6);
        kept.Check("a save station's setup", Solid, [station], level =>
            StationObjects.SetupSaveStation(level, PlaceAt(RoomObjectFixtures.Unbound(), station), LoadStationDefinitions.CrateriaSave));
        kept.Check("an energy station's setup", Terrain(RoomCollisionType.SpecialAir),
            [station + new Offset16<RoomBlocks>(-1, 0), station, station + new Offset16<RoomBlocks>(1, 0)], level =>
            {
                RoomObjectSystem roomObjects = RoomObjectFixtures.Unbound();
                roomObjects.Stations.SetupAccessStation(level, AreaId.Crateria, PlaceAt(roomObjects, station), StationAccessRuleDefinitions.Energy);
            });
        kept.Check("an elevator platform's setup", Terrain(RoomCollisionType.SpecialBlock), [station], level =>
            StationObjects.SetupElevatorPlatform(level, PlaceAt(RoomObjectFixtures.Unbound(), station)));
        kept.AssertEveryCaseKept();
    }

    /// <summary>A scroll trigger's setup (<c>$84:B371</c>), its rest after its writes (<c>$84:8B55</c>) and a scroll extension's setup (<c>$84:B33A</c>).</summary>
    internal static void VerifyScrollSetupsKeepWhatTheirBlocksShow()
    {
        var kept = new KeptPictures();
        var block = new Position16<RoomBlocks>(3, 3);
        kept.Check("a scroll trigger's setup", Solid, [block], level =>
            ScrollObjects.SetupScrollTrigger(level, PlaceAt(RoomObjectFixtures.Unbound(), block), RoomScrollChange.LandingSite));
        kept.Check("a scroll trigger's rest", Solid, [block], level =>
            new ScrollTriggerObject(RoomScrollChange.LandingSite).Rest(level, block));
        kept.Check("a scroll extension's setup", Solid, [block], level =>
            ScrollObjects.SetupScrollExtension(level, PlaceAt(RoomObjectFixtures.Unbound(), block),
                new ScrollExtension(ScrollExtensionDirection.Left)));
        kept.AssertEveryCaseKept();
    }

    /// <summary>
    /// One suite's cases. Each runs its setup in a room of its terrain whose layout shows its terrain's picture and in one
    /// whose layout shows <see cref="OtherPicture"/>, and asserts at once that the setup changed its blocks and that they
    /// collide and behave alike in both rooms, keeping their terrain's picture; a case whose edited room's blocks no longer
    /// show what they showed is recorded, and the suite's last assertion names every such case.
    /// </summary>
    private sealed class KeptPictures
    {
        /// <summary>The cases whose edited room's blocks no longer show what they showed.</summary>
        private readonly List<string> lost = [];

        /// <summary>Runs the case <paramref name="what"/>: <paramref name="setUp"/> over <paramref name="blocks"/> of <paramref name="terrain"/>.</summary>
        internal void Check(string what, RoomTerrainBlock terrain, Position16<RoomBlocks>[] blocks, Action<RoomLevelData> setUp)
        {
            RoomLevelData retail = LevelShowing(terrain, terrain.Appearance);
            RoomLevelData edited = LevelShowing(terrain, OtherPicture);
            AssertTrue(blocks.All(block => edited.ForegroundPlane.Cells[block] != retail.ForegroundPlane.Cells[block]),
                $"{what}: the fixture's blocks show a picture other than their terrain's");
            RoomCollisionBlock[] before = [.. blocks.Select(edited.GetCollisionBlock)];
            RoomBlockCell[] shown = [.. blocks.Select(block => edited.ForegroundPlane.Cells[block])];

            setUp(retail);
            setUp(edited);

            AssertTrue(blocks.Select(retail.GetCollisionBlock).Where((block, index) => block != before[index]).Any(),
                $"{what} changes its blocks");
            foreach (Position16<RoomBlocks> block in blocks)
            {
                AssertEqual(retail.GetCollisionBlock(block), edited.GetCollisionBlock(block),
                    $"{what}: block {block} collides and behaves as in a layout showing its terrain's picture");
                AssertEqual(terrain.Appearance, edited.GetCollisionBlock(block).Terrain.Appearance, $"{what}: block {block} keeps its terrain's picture");
            }
            if (shown.SequenceEqual(blocks.Select(block => edited.ForegroundPlane.Cells[block])))
                Console.WriteLine($"Collision-only writes: {what} keeps what its blocks show.");
            else
                lost.Add(what);
        }

        /// <summary>Asserts every case kept what its blocks show, naming each that did not.</summary>
        internal void AssertEveryCaseKept() =>
            AssertTrue(lost.Count == 0, $"these reset what their blocks show to their terrain's picture instead of keeping it: {string.Join("; ", lost)}");
    }

    /// <summary>A block of the fixture's picture colliding as <paramref name="collision"/>.</summary>
    private static RoomTerrainBlock Terrain(RoomCollisionType collision) => RoomTerrainBlock.Of(0x123, BlockMirroring.None, collision);

    /// <summary>A place spawned for an object at <paramref name="block"/>.</summary>
    private static RoomObjectPlace PlaceAt(RoomObjectSystem roomObjects, Position16<RoomBlocks> block)
    {
        RoomObjectPlace place = roomObjects.Places.Spawn() ?? throw new InvalidOperationException("The fixture room has a free place.");
        place.Block = block;
        return place;
    }

    /// <summary>A Samus for the setups that read her.</summary>
    private static SamusState NewSamus() => new(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation,
        new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SamusCompatibility.ForNewSamus(),
        new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), InstalledFixtures.SamusArtwork());

    /// <summary>A room of <paramref name="terrain"/> whose installed layout shows <paramref name="shown"/> at every block.</summary>
    private static RoomLevelData LevelShowing(RoomTerrainBlock terrain, RoomBlockAppearance shown)
    {
        RoomTerrainBlock[] foreground = [.. Enumerable.Repeat(terrain, FixtureSize.Area)];
        RoomBlockBehavior[] behaviors = [.. Enumerable.Repeat(RoomBlockBehavior.None, FixtureSize.Area)];
        RoomBlockAppearance[] background = new RoomBlockAppearance[FixtureSize.Area];
        RoomBlockAppearance[] shownForeground = [.. Enumerable.Repeat(shown, FixtureSize.Area)];
        return new RoomLevelData(FixtureSize, new RoomLevelPlanes(foreground, behaviors, background), RoomLevelData.NothingBeyondGrid(), [],
            new RoomLevelArtwork(new RoomLevelId("CollisionOnlyWritesFixture"), FixtureSize, shownForeground, background));
    }
}
