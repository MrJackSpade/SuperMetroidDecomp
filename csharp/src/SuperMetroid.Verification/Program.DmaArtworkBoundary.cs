using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Verifies the source-identified Kraid DMA conversions, without traversing
    /// gameplay to search for reads. Exact transfer owners are invoked directly.
    /// </summary>
    private static void VerifyDmaArtworkBoundary(string sourceRom)
    {
        Suite(nameof(VerifyVramWriteQueue), () => VerifyVramWriteQueue());
        Suite(nameof(VerifyDmaSourceRouting), () => VerifyDmaSourceRouting());
        Suite(nameof(VerifyQueuedVramAssets), () => VerifyQueuedVramAssets());
        using var temporary = new TestTempDirectory("map-catalog");
        GameInstallation installation = GameAssetInstaller.Install(sourceRom, temporary.Root);
        var reference = CartridgeImportAddressSpace.LoadRetailRom(sourceRom);
        AreaMapPresentationCatalog maps = installation.LoadMaps();
        EnemyTileArtworkCatalog art = installation.LoadEnemyTiles();
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var enemies = new RoomEnemySystem { TileArtwork = art, HudTileArtwork = maps.HudTiles };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, memory);
        var background = new SnesVram();
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, background);
        Action upload = typeof(RoomEnemySystem).GetMethod("UploadKraidRoomBackgroundTiles", flags)!
            .CreateDelegate<Action>(enemies);
        upload();
        var expected = new SnesVram();
        ImportedVramOracle.ExecuteQueued(expected, reference,
            KraidBackgroundRomData.RoomBackgroundTileAddress,
            KraidBackgroundRomData.RoomBackgroundTileBytes,
            KraidBackgroundRomData.RoomBackgroundTileVramWord);
        AssertTrue(background.Bytes.SequenceEqual(expected.Bytes),
            "Kraid background characters use installed art with exact native VRAM placement");
        enemies.TileArtwork = null;
        AssertThrows<InvalidDataException>(upload,
            "missing Kraid background artwork is not a cartridge fallback");
        enemies.TileArtwork = art;

        var transfer = typeof(RoomEnemySystem).GetMethod("AdvanceKraidDeathBg3Transfer", flags)!
            .CreateDelegate<Action<RoomEnemySlot, KraidEnemyState, int, KraidAiFunction, VramWriteQueue?>>(enemies);
        foreach (bool queued in new[] { false, true })
        {
            var actual = new SnesVram(); expected = new SnesVram();
            typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, actual);
            var state = new KraidEnemyState(); var writes = new VramWriteQueue();
            RoomEnemySlot body = enemies.Slots[0];
            for (int quarter = 0; quarter < KraidBackgroundRomData.StandardBg3TransferCount; quarter++)
            {
                transfer(body, state, quarter, KraidAiFunction.DeathFadeInBackground, queued ? writes : null);
                if (queued)
                {
                    AssertEqual(1, writes.Entries.Count, "Kraid quarter produces one NMI record");
                    AssertEqual(KraidBackgroundRomData.StandardBg3AssetForQuarter(quarter),
                        writes.Entries[0].AssetId, "Kraid restoration queues a typed installed resource");
                    writes.DrainTo(actual, memory, maps);
                }
                ImportedVramOracle.ExecuteQueued(expected, reference,
                    KraidBackgroundRomData.StandardBg3TilesAddress + quarter * KraidBackgroundRomData.StandardBg3TransferBytes,
                    KraidBackgroundRomData.StandardBg3TransferBytes,
                    (ushort)(KraidBackgroundRomData.StandardBg3VramWord + quarter * KraidBackgroundRomData.StandardBg3TransferBytes / 2));
                AssertTrue(actual.Bytes.SequenceEqual(expected.Bytes),
                    $"Kraid BG3 quarter {quarter} matches the native reference, queued={queued}");
                AssertEqual(quarter + 1, state.DeathBg3TransferCount, "Kraid retains the four-frame restoration cadence");
                AssertEqual((ushort)KraidAiFunction.DeathFadeInBackground, body.VariableA,
                    "artwork transfer preserves the death coroutine successor");
            }
        }
        enemies.HudTileArtwork = null;
        AssertThrows<InvalidDataException>(() => transfer(enemies.Slots[0], new KraidEnemyState(),
            0, KraidAiFunction.DeathFadeInBackground, new VramWriteQueue()),
            "Kraid death restoration requires installed HUD art even when queued");

        var animation = new RoomFxAnimatedTilesState();
        animation.LoadDefinition(memory, AnimatedTileObjectPointers.Lava);
        AssertThrows<InvalidOperationException>(() => animation.Step(memory, new SnesVram()),
            "direct animated characters require installed art, not a bus reader");
        Console.WriteLine("DMA artwork boundary: typed memory/asset ports, mixed queues, Kraid background, eight BG3 transfer cases and missing-artwork rejection pass.");
    }
}
