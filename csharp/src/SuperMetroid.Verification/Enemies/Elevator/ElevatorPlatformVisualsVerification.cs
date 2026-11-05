using SuperMetroid.Core.Enemies.Elevator.Artwork;
using SuperMetroid.Core;
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
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks elevator PLM art, platform collision and stock installation rules.</summary>
internal static class ElevatorPlatformVisualsVerification
{
    /// <summary>Checks the real PLM draws its stock physical block and streams its native tile.</summary>
    internal static void VerifyElevatorPlatformVisuals()
    {
        RoomPlmElevatorPlatformVisualCatalog stock =
            RoomPlmElevatorPlatformVisualCatalog.Stock();
        RenderResult Render(RoomPlmElevatorPlatformVisualCatalog visuals)
        {
            const ushort population = 0x9400;
            var bus = new TestAddressSpace();
            SeedSoleElevatorPlatformPlm(bus, population, new(0x0c, 0x08), ElevatorPlatformPlmDefinitionsConstants.InstructionLoop);
            RoomLevelData level = CreateEmptyRoom(new(32, 16));
            var plms = PlmFixtures.Unbound(new PlmShotBlockVisuals { RoomPlmElevatorPlatformVisuals = visuals });
            AssertEqual(1, PlmFixtures.LoadPopulation(ref plms, new PlmRoom(level, BgArtworkFixtures.For(new SnesVram()), new SharedGameState(), AreaId.Crateria), RoomPlmPopulationImporter.Read(bus, population), new RoomPlmServices { Samus = new FixtureRoomSamus(() => null), Bosses = new FixtureAreaBossFlags { IsTorizoDefeated = () => false } }),
                "elevator visual fixture loads one real PLM");
            plms.Step(level);
            int index = Enumerable.Range(0, 32 * 16).Single(block =>
                level.GetCollisionBlockByIndex(block).Terrain == PlatformEdge);
            return new RenderResult(level.GetCollisionBlockByIndex(index).Terrain,
                level.DisplayedAppearance(index));
        }

        var native = Render(stock);
        AssertEqual(PlatformEdge, native.Physical,
            "stock elevator keeps cartridge physical block");
        AssertEqual(PlatformEdge.Appearance, native.Rendered,
            "stock elevator streams its native block");

        Suite(nameof(VerifyElevatorPlatformVisualInstallation), () => VerifyElevatorPlatformVisualInstallation());
    }

    /// <summary>The first frame's upper edge cell: block $85, unmirrored, solid.</summary>
    private static readonly RoomTerrainBlock PlatformEdge = RoomTerrainBlock.Of(0x85, BlockMirroring.None, RoomCollisionType.SolidBlock);

    /// <summary>Checks the extracted stock visuals and that a malformed installed visual file fails loudly where it is read.</summary>
    private static void VerifyElevatorPlatformVisualInstallation()
    {
        // A private installation copy, whose stock file is tampered below.
        using (PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.RoomPlmElevatorPlatformVisualDirectory))
        {
            GameInstallation installation = copy.Installation;
            SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
                Path.GetFullPath("Super Metroid.smc"));
            RoomPlmElevatorPlatformVisualFilesConstants.ValidateStock(
                installation.RoomPlmElevatorPlatformVisualDirectory);
            VerifyElevatorPlatformStockMapping(rom, installation.LoadRoomPlmElevatorPlatformVisuals());

            string stockPath = Path.Combine(
                installation.RoomPlmElevatorPlatformVisualDirectory,
                RoomPlmElevatorPlatformVisualFiles.VisualFileName);
            File.WriteAllText(stockPath, "{}");
            AssertThrows<InvalidDataException>(
                () => installation.LoadRoomPlmElevatorPlatformVisuals(),
                "elevator stock manifest rejects corruption");
        }
    }

}
