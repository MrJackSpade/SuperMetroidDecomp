using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.DmaSourceRoutingVerification;
using static SuperMetroid.Verification.Hardware.HardwareVerification;
using static SuperMetroid.Verification.Hardware.QueuedVramAssetsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
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
        GameInstallation installation = RepositoryInstallation.Installation;
        var reference = CartridgeImportAddressSpaceTooling.LoadRetailRom(sourceRom);
        ImportEnemyArtwork art = InstalledFixtures.EnemyArtwork;
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(art.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = art.Bosses };
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
