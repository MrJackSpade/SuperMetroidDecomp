using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Verification.Rooms.Doors.LookupColoredCapVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupColoredDoorStockVerification;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Doors;
using SuperMetroid.Verification.Harness;

/// <summary>Retail and guarded-runtime checks for colored-door PLM programs, draw lists, and editable cap visuals.</summary>
internal static class ColoredDoorPlmDrawDefinitionsVerification
{
    /// <summary>Runs all twelve resident door orientations through load, physical draw, close, projectile hits, open, and opened-state reload without compiled ROM reads.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for colored door PLM draw definitions")]
    internal static void VerifyColoredDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyColoredDoorProgramDefinitions), () => VerifyColoredDoorProgramDefinitions(rom));

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            ColoredDoorPlmDrawDefinitions.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmColoredDoorVisualEntry[] entries = lists.Select(draw =>
            new RoomPlmColoredDoorVisualEntry(
                ColoredDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span
                    .SelectToArray(word => new RoomLevelWord(word).VisualWord))).ToArray();
        RoomPlmColoredDoorVisualCatalog stock = RoomPlmColoredDoorVisualCatalog.Stock();
        RoomPlmColoredDoorVisualEntry editedFrame = entries.Single(entry =>
            entry.Id == "green-left-frame-0");
        ushort originalVisual = editedFrame.Blocks[0];
        editedFrame.Blocks[0] = 0x0053;
        var edited = new RoomPlmColoredDoorVisualCatalog(entries);
        editedFrame.Blocks[0] = 0x0054;
        AssertEqual((ushort)0x0053, edited.GetWord(0xa827, 0),
            "colored-door visual catalog copies author data");
        AssertEqual(originalVisual, stock.GetWord(0xa827, 0),
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
        ushort[] headers =
        [
            (ushort)PlmHeaderId.YellowDoorFacingLeft,
            (ushort)PlmHeaderId.YellowDoorFacingRight,
            (ushort)PlmHeaderId.YellowDoorFacingUp,
            (ushort)PlmHeaderId.YellowDoorFacingDown,
            (ushort)PlmHeaderId.GreenDoorFacingLeft,
            (ushort)PlmHeaderId.GreenDoorFacingRight,
            (ushort)PlmHeaderId.GreenDoorFacingUp,
            (ushort)PlmHeaderId.GreenDoorFacingDown,
            (ushort)PlmHeaderId.RedDoorFacingLeft,
            (ushort)PlmHeaderId.RedDoorFacingRight,
            (ushort)PlmHeaderId.RedDoorFacingUp,
            (ushort)PlmHeaderId.RedDoorFacingDown,
        ];
        foreach (PlmHeaderId header in headers)
        {
            var bus = new TestAddressSpace();
            bus.WriteBytes(0x848000, bank84);
            const ushort population = 0x9000;
            bus.WriteBytes(0x8f0000 | population,
            [
                unchecked((byte)(ushort)header), SnesLittleEndian.HighByte((ushort)header),
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
            var system = new Bank80SystemState();
            var guarded = NewColoredDoorDrawReadGuard(bus, lists);
            AssertEqual(1, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, new SnesVram(), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"resident colored-door header ${(int)header:X4} loads");
            ushort initial = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | ((ushort)header + 2)));
            ushort firstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (initial + 14)));
            AssertTrue(ColoredDoorPlmDrawDefinitions.TryGet(firstDraw, out var selected),
                $"resident colored-door header ${(int)header:X4} selects compiled art");
            plms.Step(level);
            bool vertical = (selected.Runs.Span[0].DirectionAndCount & 0x8000) != 0;
            int stride = vertical ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).PackedWord.Raw,
                    $"resident colored-door ${(int)header:X4} draws physical block {block}");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored-door ${(int)header:X4} avoids program and draw ROM reads");
            if (header == PlmHeaderId.GreenDoorFacingLeft)
            {
                AssertEqual((ushort)0x0053,
                    level.DisplayedBlockTiles(origin).TopLeft.Raw,
                    "edited colored cap survives later camera streaming");
            }

            var enteringDoor = new CartridgeDoorHeader(
                Name: "fixture door",
                DestinationRoom: FixtureRoomHeaders.UnloadedDestination,
                Orientation: new(DoorDirection.Left, DoorClosingBehavior.BlueDoorCloses),
                PlmBlock: new(4, 4),
                DestinationScreen: new(0, 0),
                SamusDistance: 0,
                SetupCodePointer: DoorSetupCode.None);
            AssertTrue(plms.DoorPlms.TrySpawnDoorClosingPlm(level, enteringDoor, system),
                $"resident colored door ${(int)header:X4} selects its native closing list");
            for (int frame = 0; frame < 20 &&
                 plms.ColoredDoors.Single().Phase == ColoredDoorPhase.Closing; frame++)
                plms.Step(level);
            AssertEqual(ColoredDoorPhase.Waiting, plms.ColoredDoors.Single().Phase,
                $"resident colored door ${(int)header:X4} finishes closing and resumes its owner");

            SamusProjectileTypeWord accepted = header switch
            {
                >= PlmHeaderId.YellowDoorFacingLeft and
                    <= PlmHeaderId.YellowDoorFacingDown => new(0x0300),
                >= PlmHeaderId.GreenDoorFacingLeft and
                    <= PlmHeaderId.GreenDoorFacingDown => new(0x0200),
                _ => new(0x0100),
            };
            int hits = header >= PlmHeaderId.RedDoorFacingLeft ? 5 : 1;
            for (int hit = 0; hit < hits; hit++)
            {
                AssertTrue(plms.TryNotifyColoredDoorHit(origin, accepted),
                    $"resident colored door ${(int)header:X4} accepts hit {hit + 1}");
                plms.Step(level);
            }
            AssertEqual(ColoredDoorPhase.Opening, plms.ColoredDoors.Single().Phase,
                $"resident colored door ${(int)header:X4} reaches opening threshold");
            for (int frame = 0; frame < 128 && plms.SlotTable.ActiveCount != 0; frame++)
                plms.Step(level);
            AssertEqual(0, plms.SlotTable.ActiveCount,
                $"resident colored door ${(int)header:X4} completes opening and deletes");
            AssertTrue(system.HasOpenedDoorBit(0),
                $"resident colored door ${(int)header:X4} persists the opened state");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"resident colored door ${(int)header:X4} completes without program/draw ROM reads");

            var reopenedLevel = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
            var reopened = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmColoredDoorVisuals = stock });
            AssertEqual(1, PlmFixtures.LoadPopulation(ref reopened, new PlmRoom(reopenedLevel, new SnesVram(), system, AreaId.Crateria), RoomPlmPopulationImporter.Read(guarded, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                $"opened colored door ${(int)header:X4} reloads");
            // $84:8A72 hands the opened cap to the closed blue list, which shows the blue cap for one frame and then
            // deletes on the next pass.
            reopened.Step(reopenedLevel);
            AssertTrue(reopened.PopulationSlots.Single().Animation is BlueDoorPlmAnimation,
                $"opened colored door ${(int)header:X4} converts to a blue cap");
            reopened.Step(reopenedLevel);
            AssertEqual(0, reopened.SlotTable.ActiveCount,
                $"opened colored door ${(int)header:X4} deletes after its blue frame");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"opened colored door ${(int)header:X4} converts without program/draw ROM reads");
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
            ushort stock = installed.GetWord(0xa827, 0);
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
                installation.LoadRoomPlmColoredDoorVisuals().GetWord(0xa827, 0),
                "installed colored-door override changes selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmColoredDoorVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmColoredDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmColoredDoorVisualOverrideDirectory)
                    .GetWord(0xa827, 0),
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
                    address >= (0x840000 | list.Pointer) &&
                    address < (0x840000 | list.Pointer) + ColoredDoorPlmDrawDefinitions.DrawListBytes),
                address => $"Resident colored door reread draw payload ${address:X6}.");
}
