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
    internal static void VerifyColoredDoorObjectDrawDefinitions(CartridgeImportAddressSpace rom)
    {
        Suite(nameof(VerifyColoredDoorProgramDefinitions), () => VerifyColoredDoorProgramDefinitions(rom));

        RoomObjectShotBlockDrawDefinitions.DrawList[] lists =
            RoomObjectColoredDoorVisualFiles.All.OrderBy(list => RoomObjectDrawAddressCodec.Of(list.Draw)).ToArray();
        InstalledRoomObjectColoredDoorVisuals stock = InstalledRoomObjectColoredDoorVisuals.Stock();
        AssertEqual("green-left-frame-0", RoomObjectColoredDoorVisualFiles.VisualId(NativeObjectDraws.At<ColoredDoorCaps.Draw>(0xa827)),
            "the green left cap's first frame is the draw the cartridge stores at $A827");
        AssertEqual(RoomLevelWordCodec.Decode(CartridgeImportSource.Require(rom).ReadCartridgeWord(0x840000 | (0xa827 + 2))).Appearance,
            stock.Image(NativeObjectDraws.At<ColoredDoorCaps.Draw>(0xa827)).At(Offset16<RoomBlocks>.Zero),
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
            var roomObjects = RoomObjectFixtures.Unbound();
            var system = new SharedGameState(new SharedRandom());
            var guarded = NewColoredDoorDrawReadGuard(bus, lists);
            AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident colored-door header {header} loads");
            ushort initial = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (RoomObjectNativeWords.Word(header) + 2)));
            ushort firstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (initial + 14)));
            AssertTrue(RoomObjectColoredDoorVisualFiles.TryGet(NativeObjectDraws.At<ColoredDoorCaps.Draw>(firstDraw), out var selected),
                $"resident colored-door header {header} selects compiled art");
            roomObjects.Step(level);
            bool vertical = selected.Runs.Span[0].Direction == RoomObjectRunDirection.Vertical;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
            {
                RoomTerrainBlock written = level.GetCollisionBlockByIndex(origin + block * stride).Terrain;
                AssertEqual(selected.Runs.Span[0].Collisions.Span[block], written.Collision,
                    $"resident colored-door {header} draws physical block {block}");
                AssertEqual(stock.Image(selected.Draw).At(selected.Runs.Span[0].CellAt(block)), written.Appearance,
                    $"resident colored-door {header} shows its installed picture at block {block}");
            }
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

            AssertTrue(DoorObjects.TryIdentifyColoredDoor(header, out ColoredDoorColor doorColor, out _),
                $"resident colored door {header} names its colour");
            SamusProjectileType accepted = SamusProjectileType.Of(doorColor switch
            {
                ColoredDoorColor.Yellow => SamusProjectileFamily.PowerBomb,
                ColoredDoorColor.Green => SamusProjectileFamily.SuperMissile,
                ColoredDoorColor.Red => SamusProjectileFamily.Missile,
                _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorColor)} {doorColor}."),
            });
            int hits = header >= RoomObjectHeaderId.RedDoorFacingLeft ? 5 : 1;
            for (int hit = 0; hit < hits; hit++)
            {
                AssertTrue(roomObjects.TryNotifyColoredDoorHit(level.BlockAt(origin), accepted),
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
            var reopened = RoomObjectFixtures.Unbound();
            AssertEqual(1, RoomObjectFixtures.LoadPopulation(ref reopened, new RoomObjectRoom(reopenedLevel, BgArtworkFixtures.For(new SnesVram()), system, AreaId.Crateria), RoomObjectPopulationImporter.Read(guarded, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"opened colored door {header} reloads");
            // $84:8A72 hands the opened cap to the closed blue list, which shows the blue cap for one frame and then
            // deletes on the next pass.
            reopened.Step(reopenedLevel);
            AssertTrue(reopened.PopulationSlots.Single().Behaviour is BlueDoorCapObject { Phase: BlueDoorCapPhase.Closed },
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
    private static void VerifyColoredDoorProgramDefinitions(CartridgeImportAddressSpace rom) =>
        Suite(nameof(RoomObjectProgramsVerification.VerifyColoredDoorPrograms), () => RoomObjectProgramsVerification.VerifyColoredDoorPrograms(rom));

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyColoredDoorVisualInstallation(CartridgeImportAddressSpace rom)
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
