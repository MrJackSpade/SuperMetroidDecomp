using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.World;
namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed gunship takeoff PNGs against five native DMA
/// transfers and the resulting VRAM image.</summary>
internal static class EnemyTileArtworkGunshipLiftoffVerification
{
    /// <summary>Compares transfer sources, destinations, and bytes, then checks
    /// a selected pixel override reaches pending and legacy queued uploads.</summary>
    internal static void VerifyInstalledGunshipLiftoffArtwork(
        SuperMetroidAddressSpace bus, string directory, InstalledEnemyArtwork stock)
    {
        GunshipLiftoffArtworkCatalog installed = stock.Enemies.GunshipLiftoff ??
            throw new InvalidDataException("Installed enemy art omitted gunship takeoff frames.");
        GunshipLiftoffTransferDefinitions.TransferSequence transfers =
            GunshipLiftoffTransferDefinitions.Frames;
        AssertEqual(5, transfers.Length, "gunship has five native takeoff transfers");
        for (int index = 0; index < transfers.Length; index++)
        {
            GunshipLiftoffTransferDefinition transfer = transfers[index];
            ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                EnemyRomTablePointersGunship.LiftoffGraphicsSourceWords + index * 2);
            ushort destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus),
                EnemyRomTablePointersGunship.LiftoffVramDestinationWords + index * 2);
            AssertEqual(0x940000 | source, EnemySourceDefinitions.GunshipLiftoffPageSource(index),
                $"gunship frame {index}'s import source matches the cartridge's takeoff list");
            AssertEqual(destination, ObjCharacterNativeWords.Of(PlacedUploads.Of(GunshipLiftoffTransferDefinitions.Pages)[index].FirstByte / 32),
                $"gunship frame {index} compiled place among the OBJ characters matches the cartridge");
            ReadOnlySpan<byte> pixels = installed.Resolve(transfer.Asset).Span;
            AssertEqual(GunshipLiftoffTransferDefinitions.ByteCount, pixels.Length,
                $"gunship frame {index} transfer length");
            for (int offset = 0; offset < pixels.Length; offset++)
                AssertEqual(CpuBusRegionRead.Read(bus, EnemySourceDefinitions.GunshipLiftoffPageSource(index) + offset), pixels[offset],
                    $"gunship frame {index} byte {offset} matches cartridge art");
        }

        var runtime = CreateRetailRuntimeFixture();
        runtime.EnemySession.TileArtwork = stock.Enemies;
        runtime.BossArtwork.Artwork = stock.Bosses;
        runtime.AreaArtwork.Artwork = stock.Areas;
        SpriteArtworkPlacements stockPages = ShowAllTakeoffPages(stock);
        var nativeQueue = new VramWriteQueue();
        for (int index = 0; index < transfers.Length; index++)
        {
            GunshipLiftoffTransferDefinition transfer = transfers[index];
            AssertEqual<SpriteArtworkSource>(new InstalledArtworkCharacters(transfer.Asset), stockPages.Uploads[index].Source,
                $"takeoff frame {index} shows its installed page");
            AssertEqual(PlacedUploads.Of(GunshipLiftoffTransferDefinitions.Pages)[index].FirstByte, stockPages.Uploads[index].FirstByte,
                $"takeoff frame {index} keeps its compiled place among the OBJ characters");
            nativeQueue.Enqueue(GunshipLiftoffTransferDefinitions.ByteCount, EnemySourceDefinitions.GunshipLiftoffPageSource(index),
                VramDmaDestination.FromNative(ObjCharacterNativeWords.Of(stockPages.Uploads[index].FirstByte / 32)));
        }
        var nativeVram = new SnesVram();
        ImportedVramOracle.Drain(nativeQueue, nativeVram, bus);
        byte[] installedPage = new ObjCharacterTablePage().Page(stockPages, runtime.Artwork).Characters.ToArray();
        int pagesByte = stockPages.Uploads[0].FirstByte, pagesBytes = transfers.Length * GunshipLiftoffTransferDefinitions.ByteCount;
        AssertTrue(installedPage.AsSpan(pagesByte, pagesBytes).SequenceEqual(
                nativeVram.Bytes.Slice(GameplayVramLayoutConstants.ObjectCharacters.Offset * sizeof(ushort) + pagesByte, pagesBytes)),
            "all five installed takeoff pages match the native OBJ characters");

        string fileName = EnemyTileArtworkFormat.GunshipLiftoffFileName(0);
        string stockPath = Path.Combine(directory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(directory, "gunship-liftoff-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, fileName);
        using (var stream = new MemoryStream(original, writable: false))
        {
            IndexedPngImage image = IndexedPng.Read(stream, new(256, 8));
            image.Pixels[0] ^= 1;
            using var output = File.Create(overridePath);
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y),
                image.Pixels, image.Colours);
        }
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(directory, overrides);
        runtime.EnemySession.TileArtwork = edited.Enemies;
        runtime.BossArtwork.Artwork = edited.Bosses;
        runtime.AreaArtwork.Artwork = edited.Areas;
        byte[] editedPage = new ObjCharacterTablePage().Page(ShowAllTakeoffPages(stock), runtime.Artwork).Characters.ToArray();
        AssertEqual((byte)(installedPage[pagesByte] ^ 0x80), editedPage[pagesByte],
            "rebound PNG changes the takeoff page shown");
        AssertTrue(editedPage.AsSpan(..pagesByte).SequenceEqual(installedPage.AsSpan(..pagesByte)) &&
            editedPage.AsSpan((pagesByte + 1)..).SequenceEqual(installedPage.AsSpan((pagesByte + 1)..)),
            "gunship PNG edit changes only its selected planar pixel");
        AssertTrue(EnemyTileArtworkFiles.Load(directory, overrides).Enemies.GunshipLiftoff is not null,
            "gunship PNG override survives reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(directory, null),
            "corrupt stock gunship frame fails its manifest hash");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(directory, overrides),
            "malformed gunship override fails loudly");
        Console.WriteLine(
            "Gunship takeoff art: five compiled pages, installed PNG parity, " +
            "live/rebound page edit and strict resources pass.");

        static SpriteArtworkPlacements ShowAllTakeoffPages(InstalledEnemyArtwork art)
        {
            var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = art.Enemies, BossArtwork = art.Bosses, AreaArtwork = art.Areas };
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            var part = new GunshipPartState();
            top.SetSpeciesState(part, part.ClearWithRecord);
            for (int index = 0; index < GunshipLiftoffTransferDefinitions.Frames.Length; index++)
                enemies.GunshipBehavior.ShowGunshipTakeoffPage(top);
            AssertEqual(GunshipLiftoffTransferDefinitions.Frames.Length,
                enemies.Room.Session.SpriteCharacters.Uploads.Count, "five native takeoff calls show five pages");
            return enemies.Room.Session.SpriteCharacters;
        }
    }
}
