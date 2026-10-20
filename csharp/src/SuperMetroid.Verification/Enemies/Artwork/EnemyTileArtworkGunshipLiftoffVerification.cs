using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Rendering;
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
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed gunship liftoff transfers")]
    internal static void VerifyInstalledGunshipLiftoffArtwork(
        SuperMetroidAddressSpace bus, string directory, EnemyTileArtworkCatalog stock)
    {
        GunshipLiftoffArtworkCatalog installed = stock.GunshipLiftoff ??
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
            AssertEqual(0x940000 | source, transfer.SourceAddress,
                $"gunship frame {index} compiled source matches the cartridge");
            AssertEqual(destination, transfer.DestinationWord,
                $"gunship frame {index} compiled destination matches the cartridge");
            ReadOnlySpan<byte> pixels = installed.Resolve(transfer.Asset).Span;
            AssertEqual(GunshipLiftoffTransferDefinitions.ByteCount, pixels.Length,
                $"gunship frame {index} transfer length");
            for (int offset = 0; offset < pixels.Length; offset++)
                AssertEqual(CpuBusRegionRead.Read(bus, transfer.SourceAddress + offset), pixels[offset],
                    $"gunship frame {index} byte {offset} matches cartridge art");
        }

        var runtime = CreateRetailRuntimeFixture();
        runtime.EnemySession.TileArtwork = stock;
        VramWriteQueue stockQueue = QueueAllTakeoffFrames(stock);
        var nativeQueue = new VramWriteQueue();
        for (int index = 0; index < transfers.Length; index++)
        {
            GunshipLiftoffTransferDefinition transfer = transfers[index];
            AssertEqual(transfer.Asset, stockQueue.Entries[index].AssetId,
                $"takeoff frame {index} is a typed installed upload");
            AssertEqual(transfer.DestinationWord,
                stockQueue.Entries[index].Destination.Encoded,
                $"takeoff frame {index} keeps the compiled VRAM target");
            nativeQueue.Enqueue(GunshipLiftoffTransferDefinitions.ByteCount,
                transfer.SourceAddress, VramDmaDestination.FromNative(transfer.DestinationWord));
        }
        var installedVram = new SnesVram();
        var nativeVram = new SnesVram();
        stockQueue.DrainTo(installedVram, runtime.Artwork, null, BgArtworkFixtures.For(installedVram));
        ImportedVramOracle.Drain(nativeQueue, nativeVram, bus);
        AssertTrue(installedVram.ImageEquals(nativeVram),
            "all five installed takeoff uploads match the native VRAM image");

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
                image.Pixels, image.Palette);
        }
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrides);
        runtime.EnemySession.TileArtwork = edited;
        VramWriteQueue pending = QueueAllTakeoffFrames(stock);
        var editedVram = new SnesVram();
        pending.DrainTo(editedVram, runtime.Artwork, null, BgArtworkFixtures.For(editedVram));
        int firstDestinationByte = transfers[0].DestinationWord * 2;
        AssertEqual((byte)(installedVram.ReadByte(firstDestinationByte) ^ 0x80),
            editedVram.ReadByte(firstDestinationByte),
            "rebound PNG changes the next accepted takeoff upload");
        AssertTrue(editedVram.Bytes[..firstDestinationByte]
                .SequenceEqual(installedVram.Bytes[..firstDestinationByte]) &&
            editedVram.Bytes[(firstDestinationByte + 1)..]
                .SequenceEqual(installedVram.Bytes[(firstDestinationByte + 1)..]),
            "gunship PNG edit changes only its selected planar pixel");
        var legacyQueue = new VramWriteQueue();
        legacyQueue.Enqueue(GunshipLiftoffTransferDefinitions.ByteCount,
            transfers[0].SourceAddress, VramDmaDestination.FromNative(transfers[0].DestinationWord));
        var legacyVram = new SnesVram();
        legacyQueue.DrainTo(legacyVram, runtime.Artwork, null, BgArtworkFixtures.For(legacyVram));
        AssertEqual(editedVram.ReadByte(firstDestinationByte),
            legacyVram.ReadByte(firstDestinationByte),
            "older pending cartridge-source takeoff upload rebinds to current art");
        AssertTrue(EnemyTileArtworkFiles.Load(directory, overrides).GunshipLiftoff is not null,
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
            "Gunship takeoff art: five compiled transfers, installed PNG parity, " +
            "live/rebound VRAM edit, legacy queue rebind and strict resources pass.");

        static VramWriteQueue QueueAllTakeoffFrames(EnemyTileArtworkCatalog art)
        {
            var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = art };
            RoomEnemySlot top = enemies.SlotTable.Slots[0];
            var part = new GunshipPartState();
            top.SetSpeciesState(part, part.ClearWithRecord);
            var queue = new VramWriteQueue();
            for (int index = 0; index < GunshipLiftoffTransferDefinitions.Frames.Length; index++)
                enemies.GunshipBehavior.QueueGunshipTakeoffTiles(top, queue);
            AssertEqual(GunshipLiftoffTransferDefinitions.Frames.Length,
                queue.Entries.Count, "five native takeoff calls append five uploads");
            return queue;
        }
    }
}
