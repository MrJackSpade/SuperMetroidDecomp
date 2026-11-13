using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.RoomObjects.LookupStationStockVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectPopulationVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationImporters;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks station PLM art through immediate redraw and installation.</summary>
internal static class StationVisualsVerification
{
    /// <summary>Checks the stock map-station tile and its physical collision.</summary>
    internal static void VerifyStationVisuals(CartridgeImportAddressSpace rom)
    {
        InstalledRoomObjectStationVisuals stock = InstalledRoomObjectStationVisuals.Stock();
        StationVisualsRenderResult Render(
            InstalledRoomObjectStationVisuals visuals)
        {
            var bus = new TestAddressSpace();
            SeedRoomObjectPopulationRom(bus);
            const ushort population = 0x9500;
            WriteStationVisualMapPopulation(bus, population);
            const int width = 16;
            const int block = 6 + 6 * width;
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(new RoomTerrainBlock[width * width], new RoomBlockBehavior[width * width], new RoomBlockAppearance[width * width]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectStationVisuals = visuals });
            RoomObjectFixtures.LoadPopulation(ref roomObjects, new RoomObjectRoom(level, new SharedGameState(new SharedRandom()), AreaId.Crateria), RoomObjectPopulationImporter.Read(bus, population), new RoomObjectServices { Samus = new FixtureRoomSamus(() => new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState())), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } });
            roomObjects.Step(level);
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
    private static void VerifyStationVisualInstallation(CartridgeImportAddressSpace rom)
    {
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomObjectStationVisualDirectory);
        GameInstallation installation = copy.Installation;
        RoomObjectStationVisualFilesConstants.ValidateStock(
            installation.RoomObjectStationVisualDirectory);
        VerifyStationStockMapping(rom, installation.LoadRoomObjectStationVisuals());

        string stockPath = Path.Combine(installation.RoomObjectStationVisualDirectory,
            RoomObjectStationVisualFiles.VisualFileName);
        File.WriteAllText(stockPath, "{}");
        AssertThrows<InvalidDataException>(
            () => installation.LoadRoomObjectStationVisuals(),
            "station stock manifest hash rejects corruption");
    }

    /// <summary>Seeds one map-station `$B6D3` room PLM record at block (6,6) and its terminator.</summary>
    static void WriteStationVisualMapPopulation(TestAddressSpace bus, ushort population) =>
        bus.WriteBytes(0x8f0000 | population,
            [0xd3, 0xb6, 0x06, 0x06, 0x00, 0x00, 0x00, 0x00]);
}
