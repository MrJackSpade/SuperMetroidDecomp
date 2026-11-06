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
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks station PLM art through immediate redraw and installation.</summary>
internal static class StationVisualsVerification
{
    /// <summary>Checks the stock map-station tile and its physical collision.</summary>
    internal static void VerifyStationVisuals(SuperMetroidAddressSpace rom)
    {
        InstalledRoomPlmStationVisuals stock = InstalledRoomPlmStationVisuals.Stock();
        StationVisualsRenderResult Render(
            InstalledRoomPlmStationVisuals visuals)
        {
            var bus = new TestAddressSpace();
            SeedRoomPlmPopulationRom(bus);
            const ushort population = 0x9500;
            WriteStationVisualMapPopulation(bus, population);
            const int width = 16;
            const int block = 6 + 6 * width;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmStationVisuals = visuals });
            PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
            plms.Step(level);
            return new StationVisualsRenderResult(level.GetCollisionBlockByIndex(block).Terrain,
                level.DisplayedAppearance(block),
                level.DisplayedAppearance(block));
        }

        var native = Render(stock);
        AssertEqual(RoomTerrainBlock.Of(0x10c, BlockMirroring.None, RoomCollisionType.SolidBlock), native.Physical,
            "stock map-station frame keeps its native physical level word");
        AssertEqual(RoomBlockAppearance.Of(0x10c), native.Immediate,
            "stock station block reaches the immediate redraw");

        Suite(nameof(VerifyStationVisualInstallation), () => VerifyStationVisualInstallation(rom));
    }

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyStationVisualInstallation(SuperMetroidAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmStationVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomPlmStationVisualFilesConstants.ValidateStock(
            installation.RoomPlmStationVisualDirectory);
        VerifyStationStockMapping(rom, installation.LoadRoomPlmStationVisuals());

        string stockPath = Path.Combine(installation.RoomPlmStationVisualDirectory,
            RoomPlmStationVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomPlmStationVisuals(),
            "station stock manifest hash rejects corruption");
    }

    /// <summary>Seeds one map-station `$B6D3` room PLM record at block (6,6) and its terminator.</summary>
    static void WriteStationVisualMapPopulation(TestAddressSpace bus, ushort population) =>
        bus.WriteBytes(0x8f0000 | population,
            [0xd3, 0xb6, 0x06, 0x06, 0x00, 0x00, 0x00, 0x00]);
}
