using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.LookupBlueCapVerification;
using static SuperMetroid.Verification.Rooms.Doors.LookupBlueDoorStockVerification;
using SuperMetroid.Verification.Rooms.Plms;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>Checks compiled blue-door PLM programs, physical draws, and editable cap artwork.</summary>
internal static class BlueDoorPlmDrawDefinitionsVerification
{
    /// <summary>Runs all four door orientations through opening, closing, and closed lists with native source reads denied.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Blue door PLM draw definitions")]
    internal static void VerifyBlueDoorPlmDrawDefinitions(SuperMetroidAddressSpace rom)
    {
        Suite(nameof(VerifyBlueDoorProgramDefinitions), () => VerifyBlueDoorProgramDefinitions(rom));

        RoomPlmShotBlockDrawDefinitions.DrawList[] lists =
            BlueDoorPlmDrawDefinitionsTooling.All.OrderBy(list => list.Pointer).ToArray();
        RoomPlmBlueDoorVisualEntry[] entries = BlueDoorPlmDrawDefinitions.Editable.Select(draw =>
            new RoomPlmBlueDoorVisualEntry(BlueDoorPlmDrawDefinitions.VisualId(draw.Pointer),
                draw.Runs.Span[0].LevelWords.Span
                    .SelectToArray(word => new RoomLevelWord(word).VisualWord))).ToArray();
        RoomPlmBlueDoorVisualCatalog stock = RoomPlmBlueDoorVisualCatalog.Stock();
        RoomPlmBlueDoorVisualEntry editedFrame = entries.Single(entry =>
            entry.Id == "left-frame-1");
        ushort originalVisual = editedFrame.Blocks[0];
        editedFrame.Blocks[0] = 0x0053;
        var edited = new RoomPlmBlueDoorVisualCatalog(entries);
        editedFrame.Blocks[0] = 0x0054;
        ushort leftFirstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (BlueEntry(BlueDoorPlmProgramDefinitions.Opening(ColoredDoorOrientation.Left)) + 5)));
        AssertEqual((ushort)0x0053, edited.GetWord(leftFirstDraw, 0),
            "blue-door visual catalog copies author data");
        AssertEqual(originalVisual, stock.GetWord(leftFirstDraw, 0),
            "stock blue-door visual retains native tile choice");
        Suite(nameof(VerifyBlueCapGeometry), () => VerifyBlueCapGeometry(rom));
        Suite(nameof(VerifyBlueCapCollision), () => VerifyBlueCapCollision(rom));
        Suite(nameof(VerifyBlueCapVisuals), () => VerifyBlueCapVisuals(rom));

        foreach (ColoredDoorOrientation orientation in Enum.GetValues<ColoredDoorOrientation>())
        {
            ushort openingList = BlueEntry(BlueDoorPlmProgramDefinitions.Opening(orientation));
            ushort firstDraw = CartridgeImportSource.Require(rom).ReadCartridgeWord((0x840000 | (openingList + 5)));
            AssertTrue(BlueDoorPlmDrawDefinitions.TryGet(firstDraw, out var selected),
                $"{orientation} opening program selects a compiled draw");

            const int width = 16;
            const int origin = 4 + 4 * width;
            byte[] blockDefinitions = new byte[0x400 * 8];
            blockDefinitions[originalVisual * 8] = 0x0d;
            blockDefinitions[0x53 * 8] = 0x53;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), blockDefinitions);
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmBlueDoorVisuals = orientation == ColoredDoorOrientation.Left ? edited : stock });
            RoomBlockBehavior behavior = new(unchecked((byte)(
                RoomBlockBehaviorValues.BlueDoorFacingLeft.Value + (byte)orientation)));
            AssertTrue(plms.DoorPlms.TrySpawnBlueDoorOpening(level, origin, behavior,
                new SamusProjectileTypeWord(0)),
                $"{orientation} blue door allocates its native opening actor");
            var guarded = NewBlueDoorDrawReadGuard(rom, lists);
            plms.Step(level);
            int stride = orientation is ColoredDoorOrientation.Left or ColoredDoorOrientation.Right
                ? width : 1;
            for (int block = 0; block < 4; block++)
                AssertEqual(selected.Runs.Span[0].LevelWords.Span[block],
                    level.GetCollisionBlockByIndex(origin + block * stride).PackedWord.Raw,
                    $"{orientation} native draw writes physical block {block} without ROM payload reads");
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} opening avoids compiled program and draw ROM bytes");
            if (orientation == ColoredDoorOrientation.Left)
            {
                AssertEqual((ushort)0x0053,
                    level.DisplayedBlockTiles(origin).TopLeft.Raw,
                    "edited blue-door tile survives later camera streaming");
            }

            // Continue through every opening draw, the long air-cap hold and Delete.
            for (int frame = 0; frame < 128 && plms.SlotTable.ActiveCount != 0; frame++)
                plms.Step(level);
            AssertEqual(0, plms.SlotTable.ActiveCount,
                $"{orientation} blue-door opening reaches its native Delete");

            foreach (BlueDoorProgram program in new[] { BlueDoorPlmProgramDefinitions.Closing(orientation), BlueDoorPlmProgramDefinitions.Closed(orientation) })
            {
                AssertTrue(plms.DoorPlms.TrySpawnBlueDoorOpening(level, origin, behavior,
                    new SamusProjectileTypeWord(0)),
                    $"{orientation} allocates actor for {program}");
                plms.StartSoleProgramForVerification(PlmProgramStart.Typed(program));
                for (int frame = 0; frame < 20 && plms.SlotTable.ActiveCount != 0; frame++)
                    plms.Step(level);
                AssertEqual(0, plms.SlotTable.ActiveCount,
                    $"{orientation} {program} reaches its deletion");
                AssertEqual((int)behavior.Value,
                    level.GetCollisionBlockByIndex(origin).Bts.Value,
                    $"{orientation} {program} restores the blue-door BTS");
            }
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"{orientation} complete blue-door lists avoid all compiled ROM sources");
        }

        editedFrame.Blocks[0] = originalVisual;
        Suite(nameof(VerifyBlueDoorVisualInstallation), () => VerifyBlueDoorVisualInstallation(rom, leftFirstDraw));

        Console.WriteLine("  Blue-door PLMs: 196 compiled program bytes, all opening/closing/closed lists, twenty physical draws and sixteen compatible visual identities pass with source reads forbidden.");
    }

    /// <summary>Compares the blue caps' typed programs with their bank-$84 lists.</summary>
    private static void VerifyBlueDoorProgramDefinitions(SuperMetroidAddressSpace rom) =>
        Suite(nameof(RoomPlmProgramsVerification.VerifyBlueDoorPrograms), () => RoomPlmProgramsVerification.VerifyBlueDoorPrograms(rom));

    /// <summary>The native list a blue program begins at.</summary>
    private static ushort BlueEntry(BlueDoorProgram program) =>
        DoorPlmNativeListAddresses.BlueEntries.Single(entry => entry.Program == program).Entry;

    /// <summary>Checks stock extraction and persistent visual override loading, including invalid artwork and stock tampering.</summary>
    private static void VerifyBlueDoorVisualInstallation(
        SuperMetroidAddressSpace rom, ushort firstDraw)
    {
        using var testRootScratch = new TestScratchDirectory("blue-door-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmBlueDoorVisualFiles.Extract(rom,
                installation.RoomPlmBlueDoorVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmBlueDoorVisualFiles.ValidateStock(
                installation.RoomPlmBlueDoorVisualDirectory);
            var installed = installation.LoadRoomPlmBlueDoorVisuals();
            VerifyBlueDoorStockMapping(rom, installed);
            ushort stock = installed.GetWord(firstDraw, 0);

            string stockPath = Path.Combine(installation.RoomPlmBlueDoorVisualDirectory,
                RoomPlmBlueDoorVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted blue-door JSON is empty.");
            JsonNode frame = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "left-frame-1");
            frame.Required("blocks")[0] = 0x0053;
            Directory.CreateDirectory(installation.RoomPlmBlueDoorVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmBlueDoorVisualOverrideDirectory,
                RoomPlmBlueDoorVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmBlueDoorVisuals().GetWord(firstDraw, 0),
                "installed blue-door override changes the selected frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmBlueDoorVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmBlueDoorVisualFiles.Load(refreshed,
                    installation.RoomPlmBlueDoorVisualOverrideDirectory).GetWord(firstDraw, 0),
                "blue-door override survives stock replacement");

            frame.Required("blocks")[0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmBlueDoorVisuals(),
                "invalid blue-door override fails loudly");
            frame.Required("blocks")[0] = stock;
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmBlueDoorVisuals(),
                "tampered blue-door stock fails manifest validation");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    /// <summary>Forbids production reads from compiled blue-door instructions and all selected draw-list payloads.</summary>
    private static CartridgeReadGuard NewBlueDoorDrawReadGuard(
        ISnesAddressSpace source,
        RoomPlmShotBlockDrawDefinitions.DrawList[] lists) =>
        new CartridgeReadGuard(source)
            .Forbid(address => address is >= 0x84c489 and <= 0x84c54c,
                address => $"Production blue-door instruction reread bank-$84 byte ${address:X6}.")
            .Forbid(address => lists.Any(list =>
                    address >= (0x840000 | list.Pointer) &&
                    address < (0x840000 | list.Pointer) + BlueDoorPlmDrawDefinitions.DrawListBytes),
                address => $"Production blue-door draw reread bank-$84 payload ${address:X6}.");
}
