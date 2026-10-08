using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyCrocomireSkeletonArtwork(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        CrocomireSkeletonArtwork artwork = stock.CrocomireSkeleton ??
            throw new InvalidDataException("Installed enemy art omitted Crocomire skeleton tiles.");
        CrocomireSkeletonTransferSequence frames =
            CrocomireSkeletonTransferDefinitions.Frames;
        AssertEqual(6, frames.Length, "Crocomire has six native skeleton uploads");
        for (int index = 0; index < frames.Length; index++)
        {
            CrocomireSkeletonTransferDefinition frame = frames[index];
            ushort destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                EnemyRomTablePointersCrocomire.DeathVramDestinationWords + index * 2);
            ushort source = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                EnemyRomTablePointersCrocomire.DeathGraphicsSourceWords + index * 2);
            AssertEqual(destination, frame.DestinationOffset,
                $"Crocomire skeleton destination {index} matches the cartridge");
            AssertEqual(0xad0000 | source, frame.SourceAddress,
                $"Crocomire skeleton source {index} matches the cartridge");
            AssertEqual(CrocomireSkeletonTransferDefinitions.ChunkByteCount,
                artwork.Chunk(index).Length,
                $"Crocomire skeleton upload {index} has the native length");
            for (int offset = 0; offset < artwork.Chunk(index).Length; offset++)
                AssertEqual(rom.ReadByte(frame.SourceAddress + offset),
                    artwork.Chunk(index).Span[offset],
                    $"Crocomire skeleton chunk {index} byte {offset} matches cartridge art");
        }
        AssertEqual((ushort)0xffff, RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                EnemyRomTablePointersCrocomire.DeathVramDestinationWords +
                frames.Length * 2),
            "Crocomire skeleton seventh destination is the native terminator");
        AssertTrue(!CrocomireSkeletonTransferDefinitions.TryGet(frames.Length, out _),
            "compiled Crocomire skeleton terminator ends uploads");
        int invalidIndex = frames.Length + 1;
        AssertThrows<InvalidDataException>(
            () => CrocomireSkeletonTransferDefinitions.TryGet(invalidIndex, out _),
            "out-of-range Crocomire skeleton upload fails loudly");

        (SnesVram installedVram, CrocomireDeathState installedState,
            CrocomireSkeletonNoReadBus denied) = RunInstalled(stock);
        var nativeVram = new SnesVram();
        for (int index = 0; index < frames.Length; index++)
        {
            int destination = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                EnemyRomTablePointersCrocomire.DeathVramDestinationWords + index * 2);
            int source = 0xad0000 | RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                EnemyRomTablePointersCrocomire.DeathGraphicsSourceWords + index * 2);
            byte[] page = Enumerable.Range(0, 0x200).Select(offset => rom.ReadByte(source + offset)).ToArray();
            nativeVram.LoadBytes((0x6000 + destination) * 2, page);
        }
        AssertTrue(installedVram.Bytes.SequenceEqual(nativeVram.Bytes),
            "all six installed Crocomire skeleton uploads match native full VRAM");
        AssertEqual((ushort)(frames.Length * 2),
            installedState.TargetHeightOrSkeletonTileIndex,
            "terminal skeleton upload does not advance the cursor");
        AssertEqual(0, denied.ReadAttempts,
            "installed Crocomire skeleton upload reads no ROM bytes");

        string fileName = CrocomireSkeletonTransferDefinitions.FileName;
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(stockDirectory, "crocomire-skeleton-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, fileName);
        using (var input = new MemoryStream(original, writable: false))
        {
            IndexedPngImage image = IndexedPng.Read(input, 256, 24);
            image.Pixels[0] ^= 1;
            using var output = File.Create(overridePath);
            IndexedPng.Write(output, image.Width, image.Height,
                image.Pixels, image.Palette);
        }
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrides);
        (SnesVram editedVram, _, CrocomireSkeletonNoReadBus editedGuard) =
            RunInstalled(edited);
        int firstDestination = (CrocomireSkeletonTransferDefinitions.ObselBaseWord +
            frames[0].DestinationOffset) * 2;
        AssertEqual((byte)(installedVram.ReadByte(firstDestination) ^ 0x80),
            editedVram.ReadByte(firstDestination),
            "Crocomire skeleton PNG edit changes the actual uploaded planar pixel");
        AssertTrue(installedVram.Bytes[..firstDestination].SequenceEqual(
                editedVram.Bytes[..firstDestination]) &&
            installedVram.Bytes[(firstDestination + 1)..].SequenceEqual(
                editedVram.Bytes[(firstDestination + 1)..]),
            "Crocomire skeleton edit leaves every other VRAM byte unchanged");
        AssertEqual(0, editedGuard.ReadAttempts,
            "edited Crocomire skeleton remains ROM-free at upload time");
        AssertEqual(editedVram.ReadByte(firstDestination), RunInstalled(
                EnemyTileArtworkFiles.Load(stockDirectory, overrides)).Vram
                .ReadByte(firstDestination),
            "Crocomire skeleton PNG override survives catalog reload");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Crocomire skeleton fails its manifest hash");
        File.WriteAllBytes(stockPath, original);
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "malformed Crocomire skeleton override fails loudly");
        Console.WriteLine("Crocomire skeleton art: six native transfers, full-VRAM parity, " +
            "guarded installed upload, isolated visible PNG edit, reload and strict files pass.");

        static (SnesVram Vram, CrocomireDeathState State,
            CrocomireSkeletonNoReadBus Guard) RunInstalled(EnemyTileArtworkCatalog art)
        {
            var system = new RoomEnemySystem { TileArtwork = art };
            var vram = new SnesVram();
            var guard = new CrocomireSkeletonNoReadBus();
            var state = new CrocomireDeathState();
            SetRuntimeFields(system, guard, vram);
            for (int index = 0;
                 index <= CrocomireSkeletonTransferDefinitions.Frames.Length; index++)
                Upload(system, state);
            return (vram, state, guard);
        }

        static void SetRuntimeFields(RoomEnemySystem system,
            ISnesAddressSpace bus, SnesVram vram)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(system, bus);
            typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(system, vram);
        }

        static void Upload(RoomEnemySystem system, CrocomireDeathState state)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(RoomEnemySystem).GetMethod(
                "UploadNextCrocomireSkeletonTileChunk", flags)!.Invoke(system, [state]);
        }
    }

    private sealed class CrocomireSkeletonNoReadBus : ISnesAddressSpace, IImportCartridgeSource
    {
        internal int ReadAttempts { get; private set; }

        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Installed Crocomire skeleton read cartridge byte ${address:X6}.");
        }

        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed Crocomire skeleton wrote cartridge byte ${address:X6}.");
    }
}
