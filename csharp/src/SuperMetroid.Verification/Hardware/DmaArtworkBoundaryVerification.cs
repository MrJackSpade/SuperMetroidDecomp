using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Hardware.DmaSourceRoutingVerification;
using static SuperMetroid.Verification.Hardware.HardwareVerification;
using static SuperMetroid.Verification.Hardware.QueuedVramAssetsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.VerificationOracles;
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
        EnemyTileArtworkCatalog art = installation.LoadEnemyTiles();
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art, Presentation = { HudTileArtwork = maps.HudTiles } };
        var background = new SnesVram();
        enemies.Presentation.BindVram(BgArtworkFixtures.For(background));
        Action upload = new Action(enemies.KraidBehavior.Rise.UploadKraidRoomBackgroundTiles);
        upload();
        var expected = new SnesVram();
        ImportedVramOracle.ExecuteQueued(expected, reference,
            KraidBackgroundRomData.RoomBackgroundTileAddress,
            KraidBackgroundRomData.RoomBackgroundTileBytes,
            KraidBackgroundRomData.RoomBackgroundTileVramWord);
        AssertTrue(background.ImageEquals(expected),
            "Kraid background characters use installed art with exact native VRAM placement");
        enemies.TileArtwork = null;
        AssertThrows<InvalidDataException>(upload,
            "missing Kraid background artwork is not a cartridge fallback");
        enemies.TileArtwork = art;

        Action<RoomEnemySlot, KraidEnemyState, int, KraidAiFunction, VramWriteQueue?> transfer = enemies.KraidBehavior.Death.AdvanceKraidDeathBg3Transfer;
        foreach (bool queued in new[] { false, true })
        {
            var actual = new SnesVram(); expected = new SnesVram();
            enemies.Presentation.BindVram(BgArtworkFixtures.For(actual));
            var state = new KraidEnemyState(); var writes = new VramWriteQueue();
            enemies.KraidBehavior.Access.Kraid = state;
            RoomEnemySlot body = enemies.SlotTable.Slots[0];
            for (int quarter = 0; quarter < KraidBackgroundRomData.StandardBg3TransferCount; quarter++)
            {
                transfer(body, state, quarter, KraidAiFunction.DeathFadeInBackground, queued ? writes : null);
                if (queued)
                {
                    AssertEqual(1, writes.Entries.Count, "Kraid quarter produces one NMI record");
                    AssertEqual(KraidBackgroundRomData.StandardBg3AssetForQuarter(quarter),
                        writes.Entries[0].AssetId, "Kraid restoration queues a typed installed resource");
                    writes.DrainTo(actual, maps, null, BgArtworkFixtures.For(actual));
                }
                ImportedVramOracle.ExecuteQueued(expected, reference,
                    KraidBackgroundRomData.StandardBg3TilesAddress + quarter * KraidBackgroundRomData.StandardBg3TransferBytes,
                    KraidBackgroundRomData.StandardBg3TransferBytes,
                    (ushort)(KraidBackgroundRomData.StandardBg3VramWord + quarter * KraidBackgroundRomData.StandardBg3TransferBytes / 2));
                AssertTrue(actual.ImageEquals(expected),
                    $"Kraid BG3 quarter {quarter} matches the native reference, queued={queued}");
                AssertEqual(quarter + 1, state.DeathBg3TransferCount, "Kraid retains the four-frame restoration cadence");
                AssertEqual(KraidAiFunction.DeathFadeInBackground, state.Parts[0].Function,
                    "artwork transfer preserves the death coroutine successor");
            }
        }
        enemies.Presentation.HudTileArtwork = null;
        AssertThrows<InvalidDataException>(() => transfer(enemies.SlotTable.Slots[0], new KraidEnemyState(),
            0, KraidAiFunction.DeathFadeInBackground, new VramWriteQueue()),
            "Kraid death restoration requires installed HUD art even when queued");

        var animation = new RoomFxAnimatedTilesState();
        animation.LoadDefinition(AnimatedTileObject.Lava);
        AssertThrows<InvalidOperationException>(() => animation.Step(BgArtworkFixtures.For(new SnesVram())),
            "direct animated characters require installed art, not a bus reader");
        Console.WriteLine("DMA artwork boundary: typed memory/asset ports, mixed queues, Kraid background, eight BG3 transfer cases and missing-artwork rejection pass.");
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
