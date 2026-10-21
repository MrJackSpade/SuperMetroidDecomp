using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Elevator.LookupElevatorPlatformStockVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.SyntheticRoomCartridgeFixturesVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks editable elevator PLM art while retaining platform collision and stock installation rules.</summary>
internal static class ElevatorPlatformVisualsVerification
{
    /// <summary>Changes one streamed tile and checks the real PLM keeps its physical block.</summary>
    internal static void VerifyElevatorPlatformVisuals()
    {
        RoomPlmElevatorPlatformVisualCatalog stock =
            RoomPlmElevatorPlatformVisualCatalog.Stock();
        RoomPlmVisualRunEntry[] entries =
            ElevatorPlatformPlmDefinitions.DrawLists.Select(list =>
                new RoomPlmVisualRunEntry(
                    ElevatorPlatformPlmDefinitions.VisualId(list.Pointer),
                    list.Runs.Span.SelectToArray(run =>
                        run.LevelWords.Span.SelectToArray(word =>
                            new RoomLevelWord(word).VisualWord))))
                .ToArray();
        RoomPlmVisualRunEntry first = entries.Single(entry =>
            entry.Id == "first-frame");
        first.Runs[0][0] = 0x0053;
        var edited = new RoomPlmElevatorPlatformVisualCatalog(entries);
        first.Runs[0][0] = 0x0054;
        AssertEqual((ushort)0x0053,
            edited.GetWord((ushort)ElevatorPlatformDraw.First, 0, 0),
            "elevator visual catalog copies author data");
        RenderResult Render(RoomPlmElevatorPlatformVisualCatalog visuals)
        {
            const ushort population = 0x9400;
            var bus = new TestAddressSpace();
            SeedSoleElevatorPlatformPlm(bus, population, new(0x0c, 0x08), ElevatorPlatformPlmDefinitionsConstants.InstructionLoop);
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
            {
                definitions[0x085 * 8 + tile * 2] = 0x15;
                definitions[0x053 * 8 + tile * 2] = 0x53;
            }
            RoomLevelData level = CreateRoom(new(32, 16), new ushort[32 * 16],
                new byte[32 * 16], blockDefinitions: definitions);
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmElevatorPlatformVisuals = visuals });
            AssertEqual(1, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new Bank80SystemState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                "elevator visual fixture loads one real PLM");
            plms.Step(level);
            int index = Enumerable.Range(0, 32 * 16).Single(block =>
                level.GetCollisionBlockByIndex(block).PackedWord.Raw == 0x8085);
            return new RenderResult(level.GetCollisionBlockByIndex(index).PackedWord.Raw,
                level.DisplayedBlockTiles(index).TopLeft.Raw);
        }

        var native = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0x8085, native.Physical,
            "stock elevator keeps cartridge physical block");
        AssertEqual(native.Physical, changed.Physical,
            "edited elevator art cannot change platform collision");
        AssertEqual((ushort)0x0015, native.Rendered,
            "stock elevator streams its native tile");
        AssertEqual((ushort)0x0053, changed.Rendered,
            "edited elevator art survives camera streaming");

        Suite(nameof(VerifyElevatorPlatformVisualInstallation), () => VerifyElevatorPlatformVisualInstallation());
    }

    /// <summary>Checks extracted stock, override survival, and rejection of collision-bit or manifest corruption.</summary>
    private static void VerifyElevatorPlatformVisualInstallation()
    {
        using var testRootScratch = new TestScratchDirectory("elevator-platform-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc"));
            RoomPlmElevatorPlatformVisualFiles.Extract(rom,
                installation.RoomPlmElevatorPlatformVisualDirectory,
                SupportedCartridge.Sha256);
            RoomPlmElevatorPlatformVisualFiles.ValidateStock(
                installation.RoomPlmElevatorPlatformVisualDirectory);
            VerifyElevatorPlatformStockMapping(rom, installation.LoadRoomPlmElevatorPlatformVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmElevatorPlatformVisualDirectory,
                RoomPlmElevatorPlatformVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted elevator JSON is empty.");
            JsonNode first = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "first-frame");
            first.Required("runs").Required(0)[0] = 0x0053;
            Directory.CreateDirectory(
                installation.RoomPlmElevatorPlatformVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmElevatorPlatformVisualOverrideDirectory,
                RoomPlmElevatorPlatformVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmElevatorPlatformVisuals().GetWord(
                    (ushort)ElevatorPlatformDraw.First, 0, 0),
                "elevator override selects edited frame art");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmElevatorPlatformVisualFiles.Extract(rom, refreshed,
                SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmElevatorPlatformVisualFiles.Load(refreshed,
                    installation.RoomPlmElevatorPlatformVisualOverrideDirectory).GetWord(
                        (ushort)ElevatorPlatformDraw.First, 0, 0),
                "elevator override survives stock replacement");

            first.Required("runs").Required(0)[0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmElevatorPlatformVisuals(),
                "elevator override rejects collision bits");
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmElevatorPlatformVisuals(),
                "elevator stock manifest rejects corruption");
        }
        finally
        {
            if (Directory.Exists(testRoot)) Directory.Delete(testRoot, recursive: true);
        }
    }
}
