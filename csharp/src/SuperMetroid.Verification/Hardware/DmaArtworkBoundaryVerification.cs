using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.DmaSourceRoutingVerification;
using static SuperMetroid.Verification.Hardware.HardwareVerification;
using static SuperMetroid.Verification.Hardware.QueuedVramAssetsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks installed artwork ownership for Kraid's background DMA transfer.</summary>
internal static class DmaArtworkBoundaryVerification
{
    /// <summary>
    /// Verifies the source-identified Kraid DMA conversions, without traversing
    /// gameplay to search for reads. Exact transfer owners are invoked directly.
    /// </summary>
    internal static void VerifyDmaArtworkBoundary(string sourceRom)
    {
        Suite(nameof(VerifyVramWriteQueue), () => VerifyVramWriteQueue());
        Suite(nameof(VerifyDmaSourceRouting), () => VerifyDmaSourceRouting());
        Suite(nameof(VerifyQueuedVramAssets), () => VerifyQueuedVramAssets());
        using var temporary = new TestScratchDirectory("map-catalog");
        GameInstallation installation = GameAssetInstaller.Install(sourceRom, temporary.Root);
        var reference = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        InstalledEnemyArtwork art = installation.LoadEnemyTiles();
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art.Enemies, BossArtwork = art.Bosses, AreaArtwork = art.Areas };
        var background = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(background));
        enemies.KraidBehavior.Rise.ShowKraidRoomBackdrop();
        var expected = new SnesVram();
        ImportedVramOracle.ExecuteQueued(expected, reference,
            KraidSourceDefinitions.RoomBackgroundTiles,
            KraidSourceDefinitions.RoomBackgroundTileBytes,
            KraidBackgroundDefinitions.RoomBackgroundTileVramWord);
        AssertTrue(BgArtworkFixtures.ComposedVram(background, new BgArtworkBytes().WithKraidBackground(art.Bosses.KraidBackground
                ?? throw new InvalidOperationException("Verification requires installed Kraid background characters."))).ImageEquals(expected),
            "Kraid background characters use installed art with exact native VRAM placement");

        Console.WriteLine("DMA artwork boundary: typed memory/asset ports, mixed queues and Kraid background pass.");
    }

    /// <summary>Runs direct DMA artwork-owner checks with the repository cartridge.</summary>
    [VerificationCommand("--dma-artwork-boundary")]
    internal static int DmaArtworkBoundaryCommand()
    {
        string dmaArtworkRom = RepositoryRomPath;
        Suite(nameof(VerifyDmaArtworkBoundary), () => VerifyDmaArtworkBoundary(dmaArtworkRom));
        return 0;
    }
}
