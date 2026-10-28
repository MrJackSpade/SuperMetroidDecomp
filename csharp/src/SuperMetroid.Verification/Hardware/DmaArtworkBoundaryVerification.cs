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
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Hardware;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Checks installed artwork ownership for Kraid background and death-sequence DMA transfers.</summary>
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
        AreaMapPresentationCatalog maps = installation.LoadMaps();
        InstalledEnemyArtwork art = installation.LoadEnemyTiles();
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art.Enemies, BossArtwork = art.Bosses, AreaArtwork = art.Areas, BossArtworkBinding = { HudTileArtwork = maps.HudTiles } };
        var background = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(background));
        Action upload = new Action(enemies.KraidBehavior.Rise.UploadKraidRoomBackgroundTiles);
        upload();
        var expected = new SnesVram();
        ImportedVramOracle.ExecuteQueued(expected, reference,
            KraidSourceDefinitions.RoomBackgroundTiles,
            KraidSourceDefinitions.RoomBackgroundTileBytes,
            KraidBackgroundDefinitions.RoomBackgroundTileVramWord);
        AssertTrue(BgArtworkFixtures.ComposedVram(background, new BgArtworkBytes().WithKraidBackground(art.Bosses.KraidBackground
                ?? throw new InvalidOperationException("Verification requires installed Kraid background characters."))).ImageEquals(expected),
            "Kraid background characters use installed art with exact native VRAM placement");
        enemies.BossArtwork = null;
        AssertThrows<InvalidDataException>(upload,
            "missing Kraid background artwork is not a cartridge fallback");
        enemies.BossArtwork = art.Bosses;

        Action<RoomEnemySlot, KraidEnemyState, int, KraidAiFunction> restore = enemies.KraidBehavior.Death.RestoreKraidDeathBg3Quarter;
        {
            var actual = new SnesVram(); expected = new SnesVram();
            enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(actual));
            var state = new KraidEnemyState();
            enemies.KraidBehavior.Access.Kraid = state;
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            for (int quarter = 0; quarter < KraidHudRestoration.QuarterCount; quarter++)
            {
                restore(body, state, quarter, KraidAiFunction.DeathFadeInBackground);
                ImportedVramOracle.ExecuteQueued(expected, reference,
                    KraidBackgroundDefinitionsConstants.StandardBg3TilesAddress + quarter * KraidHudRestoration.QuarterBytes,
                    KraidHudRestoration.QuarterBytes,
                    (ushort)(KraidBackgroundDefinitions.StandardBg3VramWord + quarter * KraidHudRestoration.QuarterBytes / 2));
                AssertTrue(BgArtworkFixtures.ComposedVram(actual, new BgArtworkBytes().WithHud(maps.HudTiles)).ImageEquals(expected),
                    $"Kraid BG3 quarter {quarter} matches the native reference");
                AssertEqual(quarter + 1, state.RestoredBg3Quarters, "Kraid retains the four-frame restoration cadence");
                AssertEqual(KraidAiFunction.DeathFadeInBackground, state.Parts[0].Function,
                    "artwork restoration preserves the death coroutine successor");
            }
        }
        enemies.BossArtworkBinding.HudTileArtwork = null;
        AssertThrows<InvalidDataException>(() => restore(enemies.SlotTable.Slots[0], new KraidEnemyState(),
            0, KraidAiFunction.DeathFadeInBackground),
            "Kraid death restoration requires installed HUD art");

        Console.WriteLine("DMA artwork boundary: typed memory/asset ports, mixed queues, Kraid background, four BG3 quarter cases and missing-artwork rejection pass.");
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
