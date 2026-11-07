using SuperMetroid.AssetExtraction;
using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.LookupColoredCapVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupColoredDoorStockVerification;
using SuperMetroid.Verification.Rooms.RoomObjects;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail and guarded-runtime checks for colored-door PLM programs, draw lists, and editable cap visuals.</summary>
internal static class ColoredDoorObjectDrawDefinitionsVerification
{
    /// <summary>Runs all twelve resident door orientations through load, physical draw, close, projectile hits, open, and opened-state reload without compiled ROM reads.</summary>
    internal static void VerifyColoredDoorObjectDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyColoredDoorProgramDefinitions), () => VerifyColoredDoorProgramDefinitions(rom));

        RoomObjectShotBlockDrawDefinitions.DrawList[] lists =
            RoomObjectColoredDoorVisualFiles.All.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        RoomObjectVisualEntry[] entries = lists.Select(draw =>
            new RoomObjectVisualEntry(
                RoomObjectColoredDoorVisualFiles.VisualId(draw.Draw),
                draw.Runs.Span[0].Blocks.Span
                    .SelectToArray(block => block.Appearance))).ToArray();
        InstalledRoomObjectColoredDoorVisuals stock = InstalledRoomObjectColoredDoorVisuals.Stock();
        AssertEqual(entries.Single(entry => entry.Id == "green-left-frame-0").Blocks[0], stock.GetAppearance(NativeObjectDraws.At<ColoredDoorCaps.Draw>(0xa827), 0),
            "stock colored-door visual retains native tile choice");
        Suite(nameof(VerifyColoredCapGeometry), () => VerifyColoredCapGeometry(rom));
        Suite(nameof(VerifyColoredCapCollision), () => VerifyColoredCapCollision(rom));
        Suite(nameof(VerifyColoredCapVisuals), () => VerifyColoredCapVisuals(rom));

        // A synthetic population selects each real resident header. Only the bank-$8F
        // population is synthetic; setup and first-draw instruction bytes are copied
        // from the pinned bank-$84 cartridge. Blocking all authored draw payloads
        // proves that the actual PLM handler uses the compiled definitions.
        var bank84 = new byte[0x8000];
        for (int offset = 0; offset < bank84.Length; offset++)
            bank84[offset] = rom.ReadCartridgeByte(0x848000 + offset);
        RoomObjectHeaderId[] headers =
        [
            RoomObjectHeaderId.YellowDoorFacingLeft,
            RoomObjectHeaderId.YellowDoorFacingRight,
            RoomObjectHeaderId.YellowDoorFacingUp,
            RoomObjectHeaderId.YellowDoorFacingDown,
            RoomObjectHeaderId.GreenDoorFacingLeft,
            RoomObjectHeaderId.GreenDoorFacingRight,
            RoomObjectHeaderId.GreenDoorFacingUp,
            RoomObjectHeaderId.GreenDoorFacingDown,
            RoomObjectHeaderId.RedDoorFacingLeft,
            RoomObjectHeaderId.RedDoorFacingRight,
            RoomObjectHeaderId.RedDoorFacingUp,
            RoomObjectHeaderId.RedDoorFacingDown,
        ];
        foreach (RoomObjectHeaderId header in headers)
        {
            var bus = new TestAddressSpace();
            bus.WriteBytes(0x848000, bank84);
            const ushort population = 0x9000;
            bus.WriteBytes(0x8f0000 | population,
            [
                unchecked((byte)RoomObjectNativeWords.Word(header)), LittleEndianBytes.HighByte(RoomObjectNativeWords.Word(header)),
                4, 4, 0, 0,
                0, 0,
            ]);
            const int width = 16;
            const int origin = 4 + 4 * width;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectColoredDoorVisuals = stock });
            var system = new SharedGameState();
            var guarded = NewColoredDoorDrawReadGuard(bus, lists);
            AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident colored-door header {header} loads");
            ushort initial = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (RoomObjectNativeWords.Word(header) + 2)));
            ushort firstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (initial + 14)));
            AssertTrue(RoomObjectColoredDoorVisualFiles.TryGet(NativeObjectDraws.At<ColoredDoorCaps.Draw>(firstDraw), out var selected),
                $"resident colored-door header {header} selects compiled art");
            roomObjects.Step(level);
            bool vertical = selected.Runs.Span[0].Direction == RoomObjectRunDirection.Vertical;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].Blocks.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).Terrain,
                    $"resident colored-door {header} draws physical block {block}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored-door {header} avoids program and draw ROM reads");

            var enteringDoor = new DoorDefinition(
                Name: "fixture door",
                DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
                Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
                RoomObjectBlock: new(4, 4),
                DestinationScreen: new(0, 0),
                SamusSpeed: DoorSamusSpeed.None,
                SetupCode: DoorSetupCode.None);
            AssertTrue(roomObjects.DoorObjects.TrySpawnDoorClosingObject(level, enteringDoor, system),
                $"resident colored door {header} selects its native closing list");
            for (int frame = 0; frame < 20 &&
                 roomObjects.ColoredDoors.Single().Phase == ColoredDoorPhase.Closing; frame++)
                roomObjects.Step(level);
            AssertEqual(ColoredDoorPhase.Waiting, roomObjects.ColoredDoors.Single().Phase,
                $"resident colored door {header} finishes closing and resumes its owner");

            SamusProjectileType accepted = header switch
            {
                RoomObjectHeaderId.YellowDoorFacingLeft or
                    RoomObjectHeaderId.YellowDoorFacingRight or
                    RoomObjectHeaderId.YellowDoorFacingUp or
                    RoomObjectHeaderId.YellowDoorFacingDown => SamusProjectileType.Of(SamusProjectileFamily.PowerBomb),
                RoomObjectHeaderId.GreenDoorFacingLeft or
                    RoomObjectHeaderId.GreenDoorFacingRight or
                    RoomObjectHeaderId.GreenDoorFacingUp or
                    RoomObjectHeaderId.GreenDoorFacingDown => SamusProjectileType.Of(SamusProjectileFamily.SuperMissile),
                RoomObjectHeaderId.None or
                    RoomObjectHeaderId.Nothing or
                    RoomObjectHeaderId.CollisionReactionClearCarry or
                    RoomObjectHeaderId.RightwardsScrollExtension or
                    RoomObjectHeaderId.LeftwardsScrollExtension or
                    RoomObjectHeaderId.DownwardsScrollExtension or
                    RoomObjectHeaderId.UpwardsScrollExtension or
                    RoomObjectHeaderId.WreckedShipEntranceTreadmillFromWest or
                    RoomObjectHeaderId.WreckedShipEntranceTreadmillFromEast or
                    RoomObjectHeaderId.InsideReactionNothingFirst or
                    RoomObjectHeaderId.InsideReactionNothingSecond or
                    RoomObjectHeaderId.InsideReactionNothingThird or
                    RoomObjectHeaderId.FillMotherBrainsWall or
                    RoomObjectHeaderId.MotherBrainsRoomEscapeDoor or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow2 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow3 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow4 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow5 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow6 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow7 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow8 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRow9 or
                    RoomObjectHeaderId.MotherBrainsBackgroundRowA or
                    RoomObjectHeaderId.MotherBrainsBackgroundRowB or
                    RoomObjectHeaderId.MotherBrainsBackgroundRowC or
                    RoomObjectHeaderId.MotherBrainsBackgroundRowD or
                    RoomObjectHeaderId.ClearMotherBrainCeilingBlock or
                    RoomObjectHeaderId.ClearMotherBrainCeilingTube or
                    RoomObjectHeaderId.ClearMotherBrainBottomMiddleSideTube or
                    RoomObjectHeaderId.ClearMotherBrainBottomMiddleTubes or
                    RoomObjectHeaderId.ClearMotherBrainBottomLeftTube or
                    RoomObjectHeaderId.ClearMotherBrainBottomRightTube or
                    RoomObjectHeaderId.InsideReactionBrinstarFloorPlant or
                    RoomObjectHeaderId.InsideReactionBrinstarCeilingPlant or
                    RoomObjectHeaderId.MapStation or
                    RoomObjectHeaderId.MapStationRightAccess or
                    RoomObjectHeaderId.MapStationLeftAccess or
                    RoomObjectHeaderId.EnergyStation or
                    RoomObjectHeaderId.EnergyStationRightAccess or
                    RoomObjectHeaderId.EnergyStationLeftAccess or
                    RoomObjectHeaderId.MissileStation or
                    RoomObjectHeaderId.MissileStationRightAccess or
                    RoomObjectHeaderId.MissileStationLeftAccess or
                    RoomObjectHeaderId.NothingSecond or
                    RoomObjectHeaderId.NothingThird or
                    RoomObjectHeaderId.ScrollTriggerCollision or
                    RoomObjectHeaderId.ScrollTrigger or
                    RoomObjectHeaderId.UnusedSolidScrollPLM or
                    RoomObjectHeaderId.ElevatorPlatform or
                    RoomObjectHeaderId.InsideReactionCrateria80 or
                    RoomObjectHeaderId.InsideReactionQuicksandSurface or
                    RoomObjectHeaderId.InsideReactionSubmergingQuicksand or
                    RoomObjectHeaderId.InsideReactionSandFallsSlow or
                    RoomObjectHeaderId.InsideReactionSandFallsFast or
                    RoomObjectHeaderId.CollisionReactionQuicksandSurface or
                    RoomObjectHeaderId.CollisionReactionSubmergingQuicksand or
                    RoomObjectHeaderId.CollisionReactionSandFallsSlow or
                    RoomObjectHeaderId.CollisionReactionSandFallsFast or
                    RoomObjectHeaderId.ClearCrocomireBridge or
                    RoomObjectHeaderId.CrumbleCrocomireBridgeBlock or
                    RoomObjectHeaderId.ClearCrocomireBridgeBlock or
                    RoomObjectHeaderId.ClearCrocomireInvisibleWall or
                    RoomObjectHeaderId.CreateCrocomireInvisibleWall or
                    RoomObjectHeaderId.UnusedDraw13BlankAirTiles or
                    RoomObjectHeaderId.UnusedDraw13BlankSolidTiles or
                    RoomObjectHeaderId.ClearBabyMetroidInvisibleWall or
                    RoomObjectHeaderId.CreateBabyMetroidInvisibleWall or
                    RoomObjectHeaderId.SaveStationTrigger or
                    RoomObjectHeaderId.SaveStation or
                    RoomObjectHeaderId.CrumbleAccessToTourianElevator or
                    RoomObjectHeaderId.ClearAccessToTourianElevator or
                    RoomObjectHeaderId.DrawPhantoonDoorDuringBossFight or
                    RoomObjectHeaderId.RestorePhantoonDoorAfterBossFight or
                    RoomObjectHeaderId.CrumbleSporeSpawnCeiling or
                    RoomObjectHeaderId.ClearSporeSpawnCeiling or
                    RoomObjectHeaderId.ClearBotwoonWall or
                    RoomObjectHeaderId.CrumbleBotwoonWall or
                    RoomObjectHeaderId.UnusedSetKraidCeilingBlockToBackground1 or
                    RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground1 or
                    RoomObjectHeaderId.CrumbleKraidPlatformVariant1 or
                    RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground2 or
                    RoomObjectHeaderId.CrumbleKraidPlatformVariant2 or
                    RoomObjectHeaderId.CrumbleKraidCeilingIntoBackground3 or
                    RoomObjectHeaderId.ClearKraidCeiling or
                    RoomObjectHeaderId.ClearKraidSpikes or
                    RoomObjectHeaderId.CrumbleKraidSpikes or
                    RoomObjectHeaderId.EnableSoundsIn20FramesF0FramesIfCeres or
                    RoomObjectHeaderId.SpeedBoosterEscape or
                    RoomObjectHeaderId.ShaktoolsRoom or
                    RoomObjectHeaderId.MaridiaElevatube or
                    RoomObjectHeaderId.OldTourianEscapeShaftFakeWall or
                    RoomObjectHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft or
                    RoomObjectHeaderId.GateBlock or
                    RoomObjectHeaderId.ReactionCrittersEscapeBlock or
                    RoomObjectHeaderId.CrittersEscapeBlock or
                    RoomObjectHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape or
                    RoomObjectHeaderId.BombTorizoGreyDoor or
                    RoomObjectHeaderId.WreckedShipAttic or
                    RoomObjectHeaderId.CrateriaMainstreetEscapePassage or
                    RoomObjectHeaderId.LeftGreenGateTrigger or
                    RoomObjectHeaderId.RightGreenGateTrigger or
                    RoomObjectHeaderId.LeftRedGateTrigger or
                    RoomObjectHeaderId.RightRedGateTrigger or
                    RoomObjectHeaderId.LeftBlueGateTrigger or
                    RoomObjectHeaderId.RightBlueGateTrigger or
                    RoomObjectHeaderId.LeftYellowGateTrigger or
                    RoomObjectHeaderId.RightYellowGateTrigger or
                    RoomObjectHeaderId.DownwardsOpenGate or
                    RoomObjectHeaderId.DownwardGate or
                    RoomObjectHeaderId.UpwardsOpenGate or
                    RoomObjectHeaderId.UpwardsClosedGate or
                    RoomObjectHeaderId.DownwardGateShotBlock or
                    RoomObjectHeaderId.UpwardsGateShotblock or
                    RoomObjectHeaderId.GenericShotTriggerForAPLM or
                    RoomObjectHeaderId.GreyDoorFacingLeft or
                    RoomObjectHeaderId.GreyDoorFacingRight or
                    RoomObjectHeaderId.GreyDoorFacingUp or
                    RoomObjectHeaderId.GreyDoorFacingDown or
                    RoomObjectHeaderId.RedDoorFacingLeft or
                    RoomObjectHeaderId.RedDoorFacingRight or
                    RoomObjectHeaderId.RedDoorFacingUp or
                    RoomObjectHeaderId.RedDoorFacingDown or
                    RoomObjectHeaderId.BlueDoorFacingLeft or
                    RoomObjectHeaderId.BlueDoorFacingRight or
                    RoomObjectHeaderId.BlueDoorFacingUp or
                    RoomObjectHeaderId.BlueDoorFacingDown or
                    RoomObjectHeaderId.BlueDoorClosingFacingLeft or
                    RoomObjectHeaderId.BlueDoorClosingFacingRight or
                    RoomObjectHeaderId.BlueDoorClosingFacingUp or
                    RoomObjectHeaderId.BlueDoorClosingFacingDown or
                    RoomObjectHeaderId.MotherBrainEscapeRoomGate or
                    RoomObjectHeaderId.MotherBrainEscapeRoomGateClosing or
                    RoomObjectHeaderId.RoomObject1x1RespawningCrumbleBlock or
                    RoomObjectHeaderId.RoomObject2x1RespawningCrumbleBlock or
                    RoomObjectHeaderId.RoomObject1x2RespawningCrumbleBlock or
                    RoomObjectHeaderId.RoomObject2x2RespawningCrumbleBlock or
                    RoomObjectHeaderId.BombReactionSpeedBoostBlock or
                    RoomObjectHeaderId.SpeedBlockBrinstarSlowRespawning or
                    RoomObjectHeaderId.SpeedBlockBrinstarSlowPermanent or
                    RoomObjectHeaderId.SpeedBlockRespawning or
                    RoomObjectHeaderId.SpeedBlockDachoraRespawning or
                    RoomObjectHeaderId.SpeedBlockPermanent or
                    RoomObjectHeaderId.ContactCrumble1x1Respawning or
                    RoomObjectHeaderId.ContactCrumble2x1Respawning or
                    RoomObjectHeaderId.ContactCrumble1x2Respawning or
                    RoomObjectHeaderId.ContactCrumble2x2Respawning or
                    RoomObjectHeaderId.ContactCrumble1x1Permanent or
                    RoomObjectHeaderId.ContactCrumble2x1Permanent or
                    RoomObjectHeaderId.ContactCrumble1x2Permanent or
                    RoomObjectHeaderId.ContactCrumble2x2Permanent or
                    RoomObjectHeaderId.Reaction1x1RespawningShotBlock or
                    RoomObjectHeaderId.Reaction2x1RespawningShotBlock or
                    RoomObjectHeaderId.Reaction1x2RespawningShotBlock or
                    RoomObjectHeaderId.Reaction2x2RespawningShotBlock or
                    RoomObjectHeaderId.Reaction1x1ShotBlock or
                    RoomObjectHeaderId.Reaction2x1ShotBlock or
                    RoomObjectHeaderId.Reaction1x2ShotBlock or
                    RoomObjectHeaderId.Reaction2x2ShotBlock or
                    RoomObjectHeaderId.ReactionRespawningPowerBombBlock or
                    RoomObjectHeaderId.ReactionPowerBombBlock or
                    RoomObjectHeaderId.ReactionRespawningSuperMissileBlock or
                    RoomObjectHeaderId.ReactionSuperMissileBlock or
                    RoomObjectHeaderId.EnemyBreakableBlock or
                    RoomObjectHeaderId.Collision1x1RespawningBombBlock or
                    RoomObjectHeaderId.Collision2x1RespawningBombBlock or
                    RoomObjectHeaderId.Collision1x2RespawningBombBlock or
                    RoomObjectHeaderId.Collision2x2RespawningBombBlock or
                    RoomObjectHeaderId.Collision1x1BombBlock or
                    RoomObjectHeaderId.Collision2x1BombBlock or
                    RoomObjectHeaderId.Collision1x2BombBlock or
                    RoomObjectHeaderId.Collision2x2BombBlock or
                    RoomObjectHeaderId.Reaction1x1RespawningBombBlock or
                    RoomObjectHeaderId.Reaction2x1RespawningBombBlock or
                    RoomObjectHeaderId.Reaction1x2RespawningBombBlock or
                    RoomObjectHeaderId.Reaction2x2RespawningBombBlock or
                    RoomObjectHeaderId.Reaction1x1BombBlock or
                    RoomObjectHeaderId.Reaction2x1BombBlock or
                    RoomObjectHeaderId.Reaction1x2BombBlock or
                    RoomObjectHeaderId.Reaction2x2BombBlock or
                    RoomObjectHeaderId.GrappledGrappleBlock or
                    RoomObjectHeaderId.GrappledRespawningBreakableGrappleBlock or
                    RoomObjectHeaderId.GrappledBreakableGrappleBlock or
                    RoomObjectHeaderId.GrappledGenericSpikeBlock or
                    RoomObjectHeaderId.GrappledDraygonsBrokenTurret or
                    RoomObjectHeaderId.UnusedBlueBrinstarFaceBlock or
                    RoomObjectHeaderId.CrumbleLowerNorfairChozoRoomPlug or
                    RoomObjectHeaderId.UnusedShotBlock or
                    RoomObjectHeaderId.UnusedGrappleBlock or
                    RoomObjectHeaderId.LowerNorfairChozoHand or
                    RoomObjectHeaderId.CollisionLowerNorfairChozoHandCheck or
                    RoomObjectHeaderId.MotherBrainGlass or
                    RoomObjectHeaderId.BombTorizoHand or
                    RoomObjectHeaderId.WreckedShipChozoHand or
                    RoomObjectHeaderId.CollisionWreckedShipChozoHandCheck or
                    RoomObjectHeaderId.ClearSlopeAccessForWreckedShipChozo or
                    RoomObjectHeaderId.BlockSlopeAccessForWreckedShipChozo or
                    RoomObjectHeaderId.NoobTube or
                    RoomObjectHeaderId.SetMetroidsClearedStatesWhenRequired or
                    RoomObjectHeaderId.EyeDoorEyeFacingRight or
                    RoomObjectHeaderId.EyeDoorFacingRight or
                    RoomObjectHeaderId.EyeDoorBottomFacingRight or
                    RoomObjectHeaderId.EyeDoorEyeFacingLeft or
                    RoomObjectHeaderId.EyeDoorFacingLeft or
                    RoomObjectHeaderId.EyeDoorBottomFacingLeft or
                    RoomObjectHeaderId.DraygonCannonFacingRight or
                    RoomObjectHeaderId.DraygonCannonFacingRightDestroyed or
                    RoomObjectHeaderId.DraygonCannonFacingLeft or
                    RoomObjectHeaderId.DraygonCannonFacingLeftUnshielded or
                    RoomObjectHeaderId.ItemCollisionDetection or
                    RoomObjectHeaderId.ExposedEnergyTank or
                    RoomObjectHeaderId.ExposedMissileTank or
                    RoomObjectHeaderId.ExposedSuperMissileTank or
                    RoomObjectHeaderId.ExposedPowerBombTank or
                    RoomObjectHeaderId.ExposedBombs or
                    RoomObjectHeaderId.ExposedChargeBeam or
                    RoomObjectHeaderId.ExposedIceBeam or
                    RoomObjectHeaderId.ExposedHiJumpBoots or
                    RoomObjectHeaderId.ExposedSpeedBooster or
                    RoomObjectHeaderId.ExposedWaveBeam or
                    RoomObjectHeaderId.ExposedSpazerBeam or
                    RoomObjectHeaderId.ExposedSpringBall or
                    RoomObjectHeaderId.ExposedVariaSuit or
                    RoomObjectHeaderId.ExposedGravitySuit or
                    RoomObjectHeaderId.ExposedXrayScope or
                    RoomObjectHeaderId.ExposedPlasmaBeam or
                    RoomObjectHeaderId.ExposedGrappleBeam or
                    RoomObjectHeaderId.ExposedSpaceJump or
                    RoomObjectHeaderId.ExposedScrewAttack or
                    RoomObjectHeaderId.ExposedMorphBall or
                    RoomObjectHeaderId.ExposedReserveTank or
                    RoomObjectHeaderId.ChozoEnergyTank or
                    RoomObjectHeaderId.ChozoMissileTank or
                    RoomObjectHeaderId.ChozoSuperMissileTank or
                    RoomObjectHeaderId.ChozoPowerBombTank or
                    RoomObjectHeaderId.ChozoBombs or
                    RoomObjectHeaderId.ChozoChargeBeam or
                    RoomObjectHeaderId.ChozoIceBeam or
                    RoomObjectHeaderId.ChozoHiJumpBoots or
                    RoomObjectHeaderId.ChozoSpeedBooster or
                    RoomObjectHeaderId.ChozoWaveBeam or
                    RoomObjectHeaderId.ChozoSpazerBeam or
                    RoomObjectHeaderId.ChozoSpringBall or
                    RoomObjectHeaderId.ChozoVariaSuit or
                    RoomObjectHeaderId.ChozoGravitySuit or
                    RoomObjectHeaderId.ChozoXrayScope or
                    RoomObjectHeaderId.ChozoPlasmaBeam or
                    RoomObjectHeaderId.ChozoGrappleBeam or
                    RoomObjectHeaderId.ChozoSpaceJump or
                    RoomObjectHeaderId.ChozoScrewAttack or
                    RoomObjectHeaderId.ChozoMorphBall or
                    RoomObjectHeaderId.ChozoReserveTank or
                    RoomObjectHeaderId.ShotBlockEnergyTank or
                    RoomObjectHeaderId.ShotBlockMissileTank or
                    RoomObjectHeaderId.ShotBlockSuperMissileTank or
                    RoomObjectHeaderId.ShotBlockPowerBombTank or
                    RoomObjectHeaderId.ShotBlockBombs or
                    RoomObjectHeaderId.ShotBlockChargeBeam or
                    RoomObjectHeaderId.ShotBlockIceBeam or
                    RoomObjectHeaderId.ShotBlockHiJumpBoots or
                    RoomObjectHeaderId.ShotBlockSpeedBooster or
                    RoomObjectHeaderId.ShotBlockWaveBeam or
                    RoomObjectHeaderId.ShotBlockSpazerBeam or
                    RoomObjectHeaderId.ShotBlockSpringBall or
                    RoomObjectHeaderId.ShotBlockVariaSuit or
                    RoomObjectHeaderId.ShotBlockGravitySuit or
                    RoomObjectHeaderId.ShotBlockXrayScope or
                    RoomObjectHeaderId.ShotBlockPlasmaBeam or
                    RoomObjectHeaderId.ShotBlockGrappleBeam or
                    RoomObjectHeaderId.ShotBlockSpaceJump or
                    RoomObjectHeaderId.ShotBlockScrewAttack or
                    RoomObjectHeaderId.ShotBlockMorphBall or
                    RoomObjectHeaderId.ShotBlockReserveTank => SamusProjectileType.Of(SamusProjectileFamily.Missile),
                _ => SamusProjectileType.Of(SamusProjectileFamily.Missile),
            };
            int hits = header >= RoomObjectHeaderId.RedDoorFacingLeft ? 5 : 1;
            for (int hit = 0; hit < hits; hit++)
            {
                AssertTrue(roomObjects.TryNotifyColoredDoorHit(origin, accepted),
                    $"resident colored door {header} accepts hit {hit + 1}");
                roomObjects.Step(level);
            }
            AssertEqual(ColoredDoorPhase.Opening, roomObjects.ColoredDoors.Single().Phase,
                $"resident colored door {header} reaches opening threshold");
            for (int frame = 0; frame < 128 && roomObjects.SlotTable.ActiveCount != 0; frame++)
                roomObjects.Step(level);
            AssertEqual(0, roomObjects.SlotTable.ActiveCount,
                $"resident colored door {header} completes opening and deletes");
            AssertTrue(system.HasOpenedDoorBit(0),
                $"resident colored door {header} persists the opened state");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored door {header} completes without program/draw ROM reads");

            var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
            var reopened = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectColoredDoorVisuals = stock });
            AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref reopened, new RoomObjectRoom(reopenedLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"opened colored door {header} reloads");
            // $84:8A72 hands the opened cap to the closed blue list, which shows the blue cap for one frame and then
            // deletes on the next pass.
            reopened.Step(reopenedLevel);
            AssertTrue(reopened.PopulationSlots.Single().Animation is BlueDoorObjectAnimation,
                $"opened colored door {header} converts to a blue cap");
            reopened.Step(reopenedLevel);
            AssertEqual(0, reopened.SlotTable.ActiveCount,
                $"opened colored door {header} deletes after its blue frame");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"opened colored door {header} converts without program/draw ROM reads");
        }
        Suite(nameof(VerifyColoredDoorVisualInstallation), () => VerifyColoredDoorVisualInstallation(rom));
        Console.WriteLine(
            "  Colored doors: 1164 compiled program bytes, 12 close/hit/open/reload paths, 48 physical draws, and editable visual blocks pass with source reads blocked.");
    }

    /// <summary>Compares every coloured cap's typed programs with its bank-$84 lists.</summary>
    private static void VerifyColoredDoorProgramDefinitions(SuperMetroidAddressSpace rom) =>
        Suite(nameof(RoomObjectProgramsVerification.VerifyColoredDoorPrograms), () => RoomObjectProgramsVerification.VerifyColoredDoorPrograms(rom));

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyColoredDoorVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectColoredDoorVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectColoredDoorVisualFilesConstants.ValidateStock(
            installation.RoomObjectColoredDoorVisualDirectory);
        VerifyColoredDoorStockMapping(rom, installation.LoadRoomObjectColoredDoorVisuals());
        string stockPath = Path.Combine(installation.RoomObjectColoredDoorVisualDirectory,
            RoomObjectColoredDoorVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectColoredDoorVisuals(),
            "tampered colored-door stock fails loudly where it is read");
    }

    /// <summary>Forbids runtime reads of the compiled bank-$84 colored-door instruction range and every owned draw payload.</summary>
    private static CartridgeReadGuard NewColoredDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomObjectShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x84bffd and <= 0x84c488,
                address => $"Resident colored door reread compiled instruction ${address:X6}.")
            .Forbid(address => lists.Any(list =>
                    address >= (0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw)) &&
                    address < (0x840000 | RoomObjectDrawAddressCodec.Of(list.Draw)) + 12),
                address => $"Resident colored door reread draw payload ${address:X6}.");
}
