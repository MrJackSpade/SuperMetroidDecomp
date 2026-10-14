using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.DebugRunner.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Map;
namespace SuperMetroid.Verification.Frontend.Map;
using SuperMetroid.Verification.Harness;

/// <summary>Checks area map exports and station reveal masks against cartridge content.</summary>
internal static class AreaMapAssetsVerification
{
    /// <summary>
    /// Compares every freshly exported lossless area-map artifact with its originating cartridge
    /// range and checks the station-visible and secret-only coverage counts.
    /// </summary>
    internal static void VerifyAreaMapAssets()
    {
        using var scratch = new TestScratchDirectory("area-map-export");
        string root = scratch.Root;
        string romPath = Path.GetFullPath("Super Metroid.smc");
        AssertEqual(AreaIds.RetailCount, MapAssetExtractor.Extract(romPath, root), "exported area-map count");
        SuperMetroidAddressSpace bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            int index = AreaIds.ToIndex(area);
            AreaMapCartridgeData map = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, area);
            AssertEqual(AreaMapAssetVerificationDefinitions.TilemapAddresses[index],
                map.TilemapAddress,
                $"{area} tilemap address");

            string stem = Path.Combine(root, "areas", MapAssetExtractor.FileStem(area));
            string tilemapPath = stem + ".tilemap.bin";
            string revealPath = stem + ".station-reveal-mask.bin";
            AssertTrue(File.Exists(stem + ".coverage.png"), $"{area} coverage image exported");
            AssertSequenceEqual(map.RawTilemapBytes, File.ReadAllBytes(tilemapPath),
                $"{area} extracted tilemap bytes");
            AssertSequenceEqual(map.StationRevealMaskBytes, File.ReadAllBytes(revealPath),
                $"{area} extracted station mask bytes");

            int publicCount = 0;
            int secretCount = 0;
            for (int y = 0; y < AreaMapLayout.HeightInTiles; y++)
            {
                for (int x = 0; x < AreaMapLayout.WidthInTiles; x++)
                {
                    bool discoverable = map.IsDiscoverable(new(checked((ushort)x), checked((ushort)y)));
                    bool stationVisible = map.IsRevealedByMapStation(new(checked((ushort)x), checked((ushort)y)));
                    AssertTrue(!stationVisible || discoverable,
                        $"{area} station mask cell ({x},{y}) exists in tilemap");
                    publicCount += stationVisible ? 1 : 0;
                    secretCount += discoverable && !stationVisible ? 1 : 0;
                }
            }
            AssertEqual(AreaMapAssetVerificationDefinitions.StationVisibleCellCounts[index],
                publicCount,
                $"{area} station-visible cell count");
            AssertEqual(AreaMapAssetVerificationDefinitions.SecretOnlyCellCounts[index],
                secretCount,
                $"{area} secret-only cell count");
        }

        AreaMapCartridgeData brinstar = SuperMetroid.AssetExtraction.Frontend.Map.AreaMapImporter.Load(bus, AreaId.Brinstar);
        AssertTrue(brinstar.IsRevealedByMapStation(new(9, 1)),
            "representative Brinstar public map cell");
        AssertTrue(brinstar.IsDiscoverable(new(23, 2)) &&
                   !brinstar.IsRevealedByMapStation(new(23, 2)),
            "representative Brinstar secret-only map cell");
        Console.WriteLine(
            "  Area maps: seven tilemaps/masks and coverage counts agree.");
    }

    /// <summary>Runs the seven-area map asset and coverage comparison.</summary>
    [VerificationCommand("--area-map-assets")]
    internal static int AreaMapAssetsCommand()
    {
        Suite(nameof(VerifyAreaMapAssets), () => VerifyAreaMapAssets());
        return 0;
    }
}
