using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.LookupColoredCapVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupColoredDoorStockVerification;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
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
internal static class ColoredDoorPlmDrawDefinitionsVerification
{
    /// <summary>Runs all twelve resident door orientations through load, physical draw, close, projectile hits, open, and opened-state reload without compiled ROM reads.</summary>
    internal static void VerifyColoredDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyColoredDoorProgramDefinitions), () => VerifyColoredDoorProgramDefinitions(rom));

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            ColoredDoorPlmDrawDefinitions.All.OrderBy(list => PlmDrawAddressCodec.Of(list.Draw)).ToArray();
        RoomPlmVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmVisualEntry(
                ColoredDoorPlmDrawDefinitions.VisualId(draw.Draw),
                draw.Runs.Span[0].LevelWords.Span
                    .SelectToArray(word => new RoomLevelWord(word).VisualWord))).ToArray();
        RoomPlmColoredDoorVisualCatalog stock = RoomPlmColoredDoorVisualCatalog.Stock();
        RoomPlmVisualEntry editedFrame = entries.Single(entry =>
            entry.Id == "green-left-frame-0");
        ushort originalVisual = editedFrame.Blocks[0];
        editedFrame.Blocks[0] = 0x0053;
        var edited = new RoomPlmColoredDoorVisualCatalog(entries);
        editedFrame.Blocks[0] = 0x0054;
        AssertEqual((ushort)0x0053, edited.GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(0xa827), 0),
            "colored-door visual catalog copies author data");
        AssertEqual(originalVisual, stock.GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(0xa827), 0),
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
        PlmHeaderId[] headers =
        [
            PlmHeaderId.YellowDoorFacingLeft,
            PlmHeaderId.YellowDoorFacingRight,
            PlmHeaderId.YellowDoorFacingUp,
            PlmHeaderId.YellowDoorFacingDown,
            PlmHeaderId.GreenDoorFacingLeft,
            PlmHeaderId.GreenDoorFacingRight,
            PlmHeaderId.GreenDoorFacingUp,
            PlmHeaderId.GreenDoorFacingDown,
            PlmHeaderId.RedDoorFacingLeft,
            PlmHeaderId.RedDoorFacingRight,
            PlmHeaderId.RedDoorFacingUp,
            PlmHeaderId.RedDoorFacingDown,
        ];
        foreach (PlmHeaderId header in headers)
        {
            var bus = new TestAddressSpace();
            bus.WriteBytes(0x848000, bank84);
            const ushort population = 0x9000;
            bus.WriteBytes(0x8f0000 | population,
            [
                unchecked((byte)PlmNativeWords.Word(header)), LittleEndianBytes.HighByte(PlmNativeWords.Word(header)),
                4, 4, 0, 0,
                0, 0,
            ]);
            const int width = 16;
            const int origin = 4 + 4 * width;
            byte[] blockDefinitions = new byte[0x400 * 8];
            blockDefinitions[originalVisual * 8] = 0x04;
            blockDefinitions[0x53 * 8] = 0x53;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmColoredDoorVisuals = header == PlmHeaderId.GreenDoorFacingLeft
                    ? edited : stock });
            var system = new SharedGameState();
            var guarded = NewColoredDoorDrawReadGuard(bus, lists);
            AssertEqual(1, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident colored-door header {header} loads");
            ushort initial = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (PlmNativeWords.Word(header) + 2)));
            ushort firstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (initial + 14)));
            AssertTrue(ColoredDoorPlmDrawDefinitions.TryGet(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(firstDraw), out var selected),
                $"resident colored-door header {header} selects compiled art");
            plms.Step(level);
            bool vertical = (selected.Runs.Span[0].DirectionAndCount & 0x8000) != 0;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).PackedWord.Raw,
                    $"resident colored-door {header} draws physical block {block}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored-door {header} avoids program and draw ROM reads");
            if (header == PlmHeaderId.GreenDoorFacingLeft)
            {
                AssertEqual((ushort)0x0053,
                    level.DisplayedBlockTiles(origin).TopLeft.Raw,
                    "edited colored cap survives later camera streaming");
            }

            var enteringDoor = new DoorDefinition(
                Name: "fixture door",
                DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
                Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
                PlmBlock: new(4, 4),
                DestinationScreen: new(0, 0),
                SamusSpeed: DoorSamusSpeed.None,
                SetupCode: DoorSetupCode.None);
            AssertTrue(plms.DoorPlms.TrySpawnDoorClosingPlm(level, enteringDoor, system),
                $"resident colored door {header} selects its native closing list");
            for (int frame = 0; frame < 20 &&
                 plms.ColoredDoors.Single().Phase == ColoredDoorPhase.Closing; frame++)
                plms.Step(level);
            AssertEqual(ColoredDoorPhase.Waiting, plms.ColoredDoors.Single().Phase,
                $"resident colored door {header} finishes closing and resumes its owner");

            SamusProjectileTypeWord accepted = header switch
            {
                PlmHeaderId.YellowDoorFacingLeft or
                    PlmHeaderId.YellowDoorFacingRight or
                    PlmHeaderId.YellowDoorFacingUp or
                    PlmHeaderId.YellowDoorFacingDown => new(0x0300),
                PlmHeaderId.GreenDoorFacingLeft or
                    PlmHeaderId.GreenDoorFacingRight or
                    PlmHeaderId.GreenDoorFacingUp or
                    PlmHeaderId.GreenDoorFacingDown => new(0x0200),
                PlmHeaderId.None or
                    PlmHeaderId.Nothing or
                    PlmHeaderId.CollisionReactionClearCarry or
                    PlmHeaderId.RightwardsScrollExtension or
                    PlmHeaderId.LeftwardsScrollExtension or
                    PlmHeaderId.DownwardsScrollExtension or
                    PlmHeaderId.UpwardsScrollExtension or
                    PlmHeaderId.WreckedShipEntranceTreadmillFromWest or
                    PlmHeaderId.WreckedShipEntranceTreadmillFromEast or
                    PlmHeaderId.InsideReactionNothingFirst or
                    PlmHeaderId.InsideReactionNothingSecond or
                    PlmHeaderId.InsideReactionNothingThird or
                    PlmHeaderId.FillMotherBrainsWall or
                    PlmHeaderId.MotherBrainsRoomEscapeDoor or
                    PlmHeaderId.MotherBrainsBackgroundRow2 or
                    PlmHeaderId.MotherBrainsBackgroundRow3 or
                    PlmHeaderId.MotherBrainsBackgroundRow4 or
                    PlmHeaderId.MotherBrainsBackgroundRow5 or
                    PlmHeaderId.MotherBrainsBackgroundRow6 or
                    PlmHeaderId.MotherBrainsBackgroundRow7 or
                    PlmHeaderId.MotherBrainsBackgroundRow8 or
                    PlmHeaderId.MotherBrainsBackgroundRow9 or
                    PlmHeaderId.MotherBrainsBackgroundRowA or
                    PlmHeaderId.MotherBrainsBackgroundRowB or
                    PlmHeaderId.MotherBrainsBackgroundRowC or
                    PlmHeaderId.MotherBrainsBackgroundRowD or
                    PlmHeaderId.ClearMotherBrainCeilingBlock or
                    PlmHeaderId.ClearMotherBrainCeilingTube or
                    PlmHeaderId.ClearMotherBrainBottomMiddleSideTube or
                    PlmHeaderId.ClearMotherBrainBottomMiddleTubes or
                    PlmHeaderId.ClearMotherBrainBottomLeftTube or
                    PlmHeaderId.ClearMotherBrainBottomRightTube or
                    PlmHeaderId.InsideReactionBrinstarFloorPlant or
                    PlmHeaderId.InsideReactionBrinstarCeilingPlant or
                    PlmHeaderId.MapStation or
                    PlmHeaderId.MapStationRightAccess or
                    PlmHeaderId.MapStationLeftAccess or
                    PlmHeaderId.EnergyStation or
                    PlmHeaderId.EnergyStationRightAccess or
                    PlmHeaderId.EnergyStationLeftAccess or
                    PlmHeaderId.MissileStation or
                    PlmHeaderId.MissileStationRightAccess or
                    PlmHeaderId.MissileStationLeftAccess or
                    PlmHeaderId.NothingSecond or
                    PlmHeaderId.NothingThird or
                    PlmHeaderId.ScrollTriggerCollision or
                    PlmHeaderId.ScrollTrigger or
                    PlmHeaderId.UnusedSolidScrollPLM or
                    PlmHeaderId.ElevatorPlatform or
                    PlmHeaderId.InsideReactionCrateria80 or
                    PlmHeaderId.InsideReactionQuicksandSurface or
                    PlmHeaderId.InsideReactionSubmergingQuicksand or
                    PlmHeaderId.InsideReactionSandFallsSlow or
                    PlmHeaderId.InsideReactionSandFallsFast or
                    PlmHeaderId.CollisionReactionQuicksandSurface or
                    PlmHeaderId.CollisionReactionSubmergingQuicksand or
                    PlmHeaderId.CollisionReactionSandFallsSlow or
                    PlmHeaderId.CollisionReactionSandFallsFast or
                    PlmHeaderId.ClearCrocomireBridge or
                    PlmHeaderId.CrumbleCrocomireBridgeBlock or
                    PlmHeaderId.ClearCrocomireBridgeBlock or
                    PlmHeaderId.ClearCrocomireInvisibleWall or
                    PlmHeaderId.CreateCrocomireInvisibleWall or
                    PlmHeaderId.UnusedDraw13BlankAirTiles or
                    PlmHeaderId.UnusedDraw13BlankSolidTiles or
                    PlmHeaderId.ClearBabyMetroidInvisibleWall or
                    PlmHeaderId.CreateBabyMetroidInvisibleWall or
                    PlmHeaderId.SaveStationTrigger or
                    PlmHeaderId.SaveStation or
                    PlmHeaderId.CrumbleAccessToTourianElevator or
                    PlmHeaderId.ClearAccessToTourianElevator or
                    PlmHeaderId.DrawPhantoonDoorDuringBossFight or
                    PlmHeaderId.RestorePhantoonDoorAfterBossFight or
                    PlmHeaderId.CrumbleSporeSpawnCeiling or
                    PlmHeaderId.ClearSporeSpawnCeiling or
                    PlmHeaderId.ClearBotwoonWall or
                    PlmHeaderId.CrumbleBotwoonWall or
                    PlmHeaderId.UnusedSetKraidCeilingBlockToBackground1 or
                    PlmHeaderId.CrumbleKraidCeilingIntoBackground1 or
                    PlmHeaderId.CrumbleKraidPlatformVariant1 or
                    PlmHeaderId.CrumbleKraidCeilingIntoBackground2 or
                    PlmHeaderId.CrumbleKraidPlatformVariant2 or
                    PlmHeaderId.CrumbleKraidCeilingIntoBackground3 or
                    PlmHeaderId.ClearKraidCeiling or
                    PlmHeaderId.ClearKraidSpikes or
                    PlmHeaderId.CrumbleKraidSpikes or
                    PlmHeaderId.EnableSoundsIn20FramesF0FramesIfCeres or
                    PlmHeaderId.SpeedBoosterEscape or
                    PlmHeaderId.ShaktoolsRoom or
                    PlmHeaderId.MaridiaElevatube or
                    PlmHeaderId.OldTourianEscapeShaftFakeWall or
                    PlmHeaderId.RaiseAcidInEscapeRoomBeforeOldTourianEscapeShaft or
                    PlmHeaderId.GateBlock or
                    PlmHeaderId.ReactionCrittersEscapeBlock or
                    PlmHeaderId.CrittersEscapeBlock or
                    PlmHeaderId.TurnCeresElevatorDoorToSolidBlocksDuringEscape or
                    PlmHeaderId.BombTorizoGreyDoor or
                    PlmHeaderId.WreckedShipAttic or
                    PlmHeaderId.CrateriaMainstreetEscapePassage or
                    PlmHeaderId.LeftGreenGateTrigger or
                    PlmHeaderId.RightGreenGateTrigger or
                    PlmHeaderId.LeftRedGateTrigger or
                    PlmHeaderId.RightRedGateTrigger or
                    PlmHeaderId.LeftBlueGateTrigger or
                    PlmHeaderId.RightBlueGateTrigger or
                    PlmHeaderId.LeftYellowGateTrigger or
                    PlmHeaderId.RightYellowGateTrigger or
                    PlmHeaderId.DownwardsOpenGate or
                    PlmHeaderId.DownwardGate or
                    PlmHeaderId.UpwardsOpenGate or
                    PlmHeaderId.UpwardsClosedGate or
                    PlmHeaderId.DownwardGateShotBlock or
                    PlmHeaderId.UpwardsGateShotblock or
                    PlmHeaderId.GenericShotTriggerForAPLM or
                    PlmHeaderId.GreyDoorFacingLeft or
                    PlmHeaderId.GreyDoorFacingRight or
                    PlmHeaderId.GreyDoorFacingUp or
                    PlmHeaderId.GreyDoorFacingDown or
                    PlmHeaderId.RedDoorFacingLeft or
                    PlmHeaderId.RedDoorFacingRight or
                    PlmHeaderId.RedDoorFacingUp or
                    PlmHeaderId.RedDoorFacingDown or
                    PlmHeaderId.BlueDoorFacingLeft or
                    PlmHeaderId.BlueDoorFacingRight or
                    PlmHeaderId.BlueDoorFacingUp or
                    PlmHeaderId.BlueDoorFacingDown or
                    PlmHeaderId.BlueDoorClosingFacingLeft or
                    PlmHeaderId.BlueDoorClosingFacingRight or
                    PlmHeaderId.BlueDoorClosingFacingUp or
                    PlmHeaderId.BlueDoorClosingFacingDown or
                    PlmHeaderId.MotherBrainEscapeRoomGate or
                    PlmHeaderId.MotherBrainEscapeRoomGateClosing or
                    PlmHeaderId.Plm1x1RespawningCrumbleBlock or
                    PlmHeaderId.Plm2x1RespawningCrumbleBlock or
                    PlmHeaderId.Plm1x2RespawningCrumbleBlock or
                    PlmHeaderId.Plm2x2RespawningCrumbleBlock or
                    PlmHeaderId.BombReactionSpeedBoostBlock or
                    PlmHeaderId.SpeedBlockBrinstarSlowRespawning or
                    PlmHeaderId.SpeedBlockBrinstarSlowPermanent or
                    PlmHeaderId.SpeedBlockRespawning or
                    PlmHeaderId.SpeedBlockDachoraRespawning or
                    PlmHeaderId.SpeedBlockPermanent or
                    PlmHeaderId.ContactCrumble1x1Respawning or
                    PlmHeaderId.ContactCrumble2x1Respawning or
                    PlmHeaderId.ContactCrumble1x2Respawning or
                    PlmHeaderId.ContactCrumble2x2Respawning or
                    PlmHeaderId.ContactCrumble1x1Permanent or
                    PlmHeaderId.ContactCrumble2x1Permanent or
                    PlmHeaderId.ContactCrumble1x2Permanent or
                    PlmHeaderId.ContactCrumble2x2Permanent or
                    PlmHeaderId.Reaction1x1RespawningShotBlock or
                    PlmHeaderId.Reaction2x1RespawningShotBlock or
                    PlmHeaderId.Reaction1x2RespawningShotBlock or
                    PlmHeaderId.Reaction2x2RespawningShotBlock or
                    PlmHeaderId.Reaction1x1ShotBlock or
                    PlmHeaderId.Reaction2x1ShotBlock or
                    PlmHeaderId.Reaction1x2ShotBlock or
                    PlmHeaderId.Reaction2x2ShotBlock or
                    PlmHeaderId.ReactionRespawningPowerBombBlock or
                    PlmHeaderId.ReactionPowerBombBlock or
                    PlmHeaderId.ReactionRespawningSuperMissileBlock or
                    PlmHeaderId.ReactionSuperMissileBlock or
                    PlmHeaderId.EnemyBreakableBlock or
                    PlmHeaderId.Collision1x1RespawningBombBlock or
                    PlmHeaderId.Collision2x1RespawningBombBlock or
                    PlmHeaderId.Collision1x2RespawningBombBlock or
                    PlmHeaderId.Collision2x2RespawningBombBlock or
                    PlmHeaderId.Collision1x1BombBlock or
                    PlmHeaderId.Collision2x1BombBlock or
                    PlmHeaderId.Collision1x2BombBlock or
                    PlmHeaderId.Collision2x2BombBlock or
                    PlmHeaderId.Reaction1x1RespawningBombBlock or
                    PlmHeaderId.Reaction2x1RespawningBombBlock or
                    PlmHeaderId.Reaction1x2RespawningBombBlock or
                    PlmHeaderId.Reaction2x2RespawningBombBlock or
                    PlmHeaderId.Reaction1x1BombBlock or
                    PlmHeaderId.Reaction2x1BombBlock or
                    PlmHeaderId.Reaction1x2BombBlock or
                    PlmHeaderId.Reaction2x2BombBlock or
                    PlmHeaderId.GrappledGrappleBlock or
                    PlmHeaderId.GrappledRespawningBreakableGrappleBlock or
                    PlmHeaderId.GrappledBreakableGrappleBlock or
                    PlmHeaderId.GrappledGenericSpikeBlock or
                    PlmHeaderId.GrappledDraygonsBrokenTurret or
                    PlmHeaderId.UnusedBlueBrinstarFaceBlock or
                    PlmHeaderId.CrumbleLowerNorfairChozoRoomPlug or
                    PlmHeaderId.UnusedShotBlock or
                    PlmHeaderId.UnusedGrappleBlock or
                    PlmHeaderId.LowerNorfairChozoHand or
                    PlmHeaderId.CollisionLowerNorfairChozoHandCheck or
                    PlmHeaderId.MotherBrainGlass or
                    PlmHeaderId.BombTorizoHand or
                    PlmHeaderId.WreckedShipChozoHand or
                    PlmHeaderId.CollisionWreckedShipChozoHandCheck or
                    PlmHeaderId.ClearSlopeAccessForWreckedShipChozo or
                    PlmHeaderId.BlockSlopeAccessForWreckedShipChozo or
                    PlmHeaderId.NoobTube or
                    PlmHeaderId.SetMetroidsClearedStatesWhenRequired or
                    PlmHeaderId.EyeDoorEyeFacingRight or
                    PlmHeaderId.EyeDoorFacingRight or
                    PlmHeaderId.EyeDoorBottomFacingRight or
                    PlmHeaderId.EyeDoorEyeFacingLeft or
                    PlmHeaderId.EyeDoorFacingLeft or
                    PlmHeaderId.EyeDoorBottomFacingLeft or
                    PlmHeaderId.DraygonCannonFacingRight or
                    PlmHeaderId.DraygonCannonFacingRightDestroyed or
                    PlmHeaderId.DraygonCannonFacingLeft or
                    PlmHeaderId.DraygonCannonFacingLeftUnshielded or
                    PlmHeaderId.ItemCollisionDetection or
                    PlmHeaderId.ExposedEnergyTank or
                    PlmHeaderId.ExposedMissileTank or
                    PlmHeaderId.ExposedSuperMissileTank or
                    PlmHeaderId.ExposedPowerBombTank or
                    PlmHeaderId.ExposedBombs or
                    PlmHeaderId.ExposedChargeBeam or
                    PlmHeaderId.ExposedIceBeam or
                    PlmHeaderId.ExposedHiJumpBoots or
                    PlmHeaderId.ExposedSpeedBooster or
                    PlmHeaderId.ExposedWaveBeam or
                    PlmHeaderId.ExposedSpazerBeam or
                    PlmHeaderId.ExposedSpringBall or
                    PlmHeaderId.ExposedVariaSuit or
                    PlmHeaderId.ExposedGravitySuit or
                    PlmHeaderId.ExposedXrayScope or
                    PlmHeaderId.ExposedPlasmaBeam or
                    PlmHeaderId.ExposedGrappleBeam or
                    PlmHeaderId.ExposedSpaceJump or
                    PlmHeaderId.ExposedScrewAttack or
                    PlmHeaderId.ExposedMorphBall or
                    PlmHeaderId.ExposedReserveTank or
                    PlmHeaderId.ChozoEnergyTank or
                    PlmHeaderId.ChozoMissileTank or
                    PlmHeaderId.ChozoSuperMissileTank or
                    PlmHeaderId.ChozoPowerBombTank or
                    PlmHeaderId.ChozoBombs or
                    PlmHeaderId.ChozoChargeBeam or
                    PlmHeaderId.ChozoIceBeam or
                    PlmHeaderId.ChozoHiJumpBoots or
                    PlmHeaderId.ChozoSpeedBooster or
                    PlmHeaderId.ChozoWaveBeam or
                    PlmHeaderId.ChozoSpazerBeam or
                    PlmHeaderId.ChozoSpringBall or
                    PlmHeaderId.ChozoVariaSuit or
                    PlmHeaderId.ChozoGravitySuit or
                    PlmHeaderId.ChozoXrayScope or
                    PlmHeaderId.ChozoPlasmaBeam or
                    PlmHeaderId.ChozoGrappleBeam or
                    PlmHeaderId.ChozoSpaceJump or
                    PlmHeaderId.ChozoScrewAttack or
                    PlmHeaderId.ChozoMorphBall or
                    PlmHeaderId.ChozoReserveTank or
                    PlmHeaderId.ShotBlockEnergyTank or
                    PlmHeaderId.ShotBlockMissileTank or
                    PlmHeaderId.ShotBlockSuperMissileTank or
                    PlmHeaderId.ShotBlockPowerBombTank or
                    PlmHeaderId.ShotBlockBombs or
                    PlmHeaderId.ShotBlockChargeBeam or
                    PlmHeaderId.ShotBlockIceBeam or
                    PlmHeaderId.ShotBlockHiJumpBoots or
                    PlmHeaderId.ShotBlockSpeedBooster or
                    PlmHeaderId.ShotBlockWaveBeam or
                    PlmHeaderId.ShotBlockSpazerBeam or
                    PlmHeaderId.ShotBlockSpringBall or
                    PlmHeaderId.ShotBlockVariaSuit or
                    PlmHeaderId.ShotBlockGravitySuit or
                    PlmHeaderId.ShotBlockXrayScope or
                    PlmHeaderId.ShotBlockPlasmaBeam or
                    PlmHeaderId.ShotBlockGrappleBeam or
                    PlmHeaderId.ShotBlockSpaceJump or
                    PlmHeaderId.ShotBlockScrewAttack or
                    PlmHeaderId.ShotBlockMorphBall or
                    PlmHeaderId.ShotBlockReserveTank => new(0x0100),
                _ => new(0x0100),
            };
            int hits = header >= PlmHeaderId.RedDoorFacingLeft ? 5 : 1;
            for (int hit = 0; hit < hits; hit++)
            {
                AssertTrue(plms.TryNotifyColoredDoorHit(origin, accepted),
                    $"resident colored door {header} accepts hit {hit + 1}");
                plms.Step(level);
            }
            AssertEqual(ColoredDoorPhase.Opening, plms.ColoredDoors.Single().Phase,
                $"resident colored door {header} reaches opening threshold");
            for (int frame = 0; frame < 128 && plms.SlotTable.ActiveCount != 0; frame++)
                plms.Step(level);
            AssertEqual(0, plms.SlotTable.ActiveCount,
                $"resident colored door {header} completes opening and deletes");
            AssertTrue(system.HasOpenedDoorBit(0),
                $"resident colored door {header} persists the opened state");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored door {header} completes without program/draw ROM reads");

            var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
            var reopened = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmColoredDoorVisuals = stock });
            AssertEqual(1, PlmFixtures.LoadPopulation(ref reopened, new PlmRoom(reopenedLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"opened colored door {header} reloads");
            // $84:8A72 hands the opened cap to the closed blue list, which shows the blue cap for one frame and then
            // deletes on the next pass.
            reopened.Step(reopenedLevel);
            AssertTrue(reopened.PopulationSlots.Single().Animation is BlueDoorPlmAnimation,
                $"opened colored door {header} converts to a blue cap");
            reopened.Step(reopenedLevel);
            AssertEqual(0, reopened.SlotTable.ActiveCount,
                $"opened colored door {header} deletes after its blue frame");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"opened colored door {header} converts without program/draw ROM reads");
        }
        editedFrame.Blocks[0] = originalVisual;
        Suite(nameof(VerifyColoredDoorVisualInstallation), () => VerifyColoredDoorVisualInstallation(rom));
        Console.WriteLine(
            "  Colored doors: 1164 compiled program bytes, 12 close/hit/open/reload paths, 48 physical draws, and editable visual blocks pass with source reads blocked.");
    }

    /// <summary>Compares every coloured cap's typed programs with its bank-$84 lists.</summary>
    private static void VerifyColoredDoorProgramDefinitions(SuperMetroidAddressSpace rom) =>
        Suite(nameof(RoomPlmProgramsVerification.VerifyColoredDoorPrograms), () => RoomPlmProgramsVerification.VerifyColoredDoorPrograms(rom));

    /// <summary>Checks extracted stock, an edited green-cap override, survival across stock refresh, and rejection of malformed override or stock files.</summary>
    private static void VerifyColoredDoorVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("colored-door-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmColoredDoorVisualFiles.Extract(rom,
                installation.RoomPlmColoredDoorVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmColoredDoorVisualFiles.ValidateStock(
                installation.RoomPlmColoredDoorVisualDirectory);
            var installed = installation.LoadRoomPlmColoredDoorVisuals();
            VerifyColoredDoorStockMapping(rom, installed);
            ushort stock = installed.GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(0xa827), 0);
            string stockPath = Path.Combine(installation.RoomPlmColoredDoorVisualDirectory,
                RoomPlmColoredDoorVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted colored-door JSON is empty.");
            JsonNode frame = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "green-left-frame-0");
            frame.Required("blocks")[0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmColoredDoorVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmColoredDoorVisualOverrideDirectory,
                RoomPlmColoredDoorVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmColoredDoorVisuals().GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(0xa827), 0),
                "installed colored-door override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmColoredDoorVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmColoredDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmColoredDoorVisualOverrideDirectory)
                    .GetWord(NativePlmDraws.At<ColoredDoorPlmDrawDefinitions.Draw>(0xa827), 0),
                "colored-door override survives stock replacement");

            frame.Required("blocks")[0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmColoredDoorVisuals(),
                "invalid colored-door override fails loudly");
            frame.Required("blocks")[0] = stock;
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmColoredDoorVisuals(),
                "tampered colored-door stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    /// <summary>Forbids runtime reads of the compiled bank-$84 colored-door instruction range and every owned draw payload.</summary>
    private static CartridgeReadGuard NewColoredDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x84bffd and <= 0x84c488,
                address => $"Resident colored door reread compiled instruction ${address:X6}.")
            .Forbid(address => lists.Any(list =>
                    address >= (0x840000 | PlmDrawAddressCodec.Of(list.Draw)) &&
                    address < (0x840000 | PlmDrawAddressCodec.Of(list.Draw)) + 12),
                address => $"Resident colored door reread draw payload ${address:X6}.");
}
