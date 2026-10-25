using SuperMetroid.Core;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Plms.LookupStationStockVerification;
using static SuperMetroid.Verification.Rooms.Plms.RoomPlmPopulationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks editable station PLM art through immediate redraw, streaming, and installation.</summary>
internal static class StationVisualsVerification
{
    /// <summary>Changes a map-station tile while checking physical collision and other station families.</summary>
    internal static void VerifyStationVisuals(SuperMetroidAddressSpace rom)
    {
        RoomPlmStationVisualCatalog stock = RoomPlmStationVisualCatalog.Stock();
        RoomPlmVisualRunEntry[] entries = RoomPlmStationDrawDefinitions.All
            .Select(list => new RoomPlmVisualRunEntry(
                RoomPlmStationDrawDefinitions.VisualId(NativePlmDraws.At<StationDraw>(PlmDrawAddressCodec.Of(list.Draw))),
                list.Runs.Span
                    .SelectToArray(run => run.LevelWords.Span
                        .SelectToArray(word => new RoomLevelWord(word).VisualWord))
                    ))
            .ToArray();
        RoomPlmVisualRunEntry first = entries.Single(entry => entry.Id == "map-frame-0");
        first.Runs[0][0] = 0x0053;
        var edited = new RoomPlmStationVisualCatalog(entries);
        first.Runs[0][0] = 0x0054;
        ushort pointer = PlmDrawAddressCodec.Of(StationAnimationProgramDefinitions.Resolve(
            StationAnimationList.MapIdle, 0).Draw);
        AssertEqual((ushort)0x0053, edited.GetWord(NativePlmDraws.At<StationDraw>(pointer), 0, 0),
            "station visual catalog copies author data");
        StationVisualsRenderResult Render(
            RoomPlmStationVisualCatalog visuals)
        {
            var bus = new TestAddressSpace();
            SeedRoomPlmPopulationRom(bus);
            const ushort population = 0x9500;
            WriteStationVisualMapPopulation(bus, population);
            const int width = 16;
            const int block = 6 + 6 * width;
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
            {
                definitions[0x10c * 8 + tile * 2] = 0x0c;
                definitions[0x053 * 8 + tile * 2] = 0x53;
            }

            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new ushort[width * width], new byte[width * width], new ushort[width * width]), definitions);
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmStationVisuals = visuals });
            PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState()), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
            plms.Step(level);
            return new StationVisualsRenderResult(level.GetCollisionBlockByIndex(block).PackedWord.Raw,
                level.DisplayedBlockTiles(block).TopLeft.Raw,
                level.DisplayedBlockTiles(block).TopLeft.Raw);
        }

        var native = Render(stock);
        var changed = Render(edited);
        AssertEqual((ushort)0x810c, native.Physical,
            "stock map-station frame keeps its native physical level word");
        AssertEqual(native.Physical, changed.Physical,
            "station art cannot change collision or physical level data");
        AssertEqual((ushort)0x000c, native.Immediate,
            "stock station visual reaches the immediate redraw");
        AssertEqual((ushort)0x0053, changed.Immediate,
            "edited station visual reaches the immediate redraw");
        AssertEqual((ushort)0x0053, changed.Streamed,
            "edited station visual survives later camera streaming");

        var gameplayBus = new TestAddressSpace();
        SeedRoomPlmPopulationRom(gameplayBus);
        Suite(nameof(VerifyOtherStationFamilies), () => VerifyOtherStationFamilies(gameplayBus, edited));
        Suite(nameof(VerifyStationVisualInstallation), () => VerifyStationVisualInstallation(rom));
    }

    /// <summary>Checks stock extraction, override survival, and rejection of corrupt or physical-bit edits.</summary>
    private static void VerifyStationVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using var testRootScratch = new TestScratchDirectory("station-visual");
        string testRoot = testRootScratch.Root;
        try
        {
            var installation = new GameInstallation(testRoot);
            RoomPlmStationVisualFiles.Extract(rom,
                installation.RoomPlmStationVisualDirectory, SupportedCartridge.Sha256);
            RoomPlmStationVisualFiles.ValidateStock(
                installation.RoomPlmStationVisualDirectory);
            ushort pointer = PlmDrawAddressCodec.Of(StationAnimationProgramDefinitions.Resolve(
                StationAnimationList.MapIdle, 0).Draw);
            VerifyStationStockMapping(rom, installation.LoadRoomPlmStationVisuals());

            string stockPath = Path.Combine(installation.RoomPlmStationVisualDirectory,
                RoomPlmStationVisualFiles.VisualFileName);
            JsonNode document = JsonNode.Parse(File.ReadAllText(stockPath))
                ?? throw new InvalidDataException("Extracted station JSON is empty.");
            JsonNode frame = document.Required("entries").AsArray().RequiredElements().Single(entry =>
                entry.Required("id").GetValue<string>() == "map-frame-0");
            frame.Required("runs").Required(0)[0] = 0x0053;
            Directory.CreateDirectory(installation.RoomPlmStationVisualOverrideDirectory);
            string overridePath = Path.Combine(
                installation.RoomPlmStationVisualOverrideDirectory,
                RoomPlmStationVisualFiles.VisualFileName);
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertEqual((ushort)0x0053,
                installation.LoadRoomPlmStationVisuals().GetWord(NativePlmDraws.At<StationDraw>(pointer), 0, 0),
                "station visual override selects edited map frame");

            string refreshed = Path.Combine(testRoot, "refreshed-stock");
            RoomPlmStationVisualFiles.Extract(rom, refreshed, SupportedCartridge.Sha256);
            AssertEqual((ushort)0x0053,
                RoomPlmStationVisualFiles.Load(refreshed,
                    installation.RoomPlmStationVisualOverrideDirectory).GetWord(NativePlmDraws.At<StationDraw>(pointer), 0, 0),
                "station visual override survives stock replacement");

            frame.Required("runs").Required(0)[0] = 0xf053;
            File.WriteAllText(overridePath, document.ToJsonString());
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmStationVisuals(),
                "station override rejects collision bits instead of silently restoring stock");

            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmStationVisuals(),
                "station stock manifest hash rejects corruption");
        }
        finally
        {
            if (Directory.Exists(testRoot))
                Directory.Delete(testRoot, recursive: true);
        }
    }

    /// <summary>Seeds one map-station `$B6D3` room PLM record at block (6,6) and its terminator.</summary>
    static void WriteStationVisualMapPopulation(TestAddressSpace bus, ushort population) =>
        bus.WriteBytes(0x8f0000 | population,
            [0xd3, 0xb6, 0x06, 0x06, 0x00, 0x00, 0x00, 0x00]);
}
