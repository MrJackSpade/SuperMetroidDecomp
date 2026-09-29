using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledMotherBrainCorpseArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        RoomCharacterAtlas artwork = stock.MotherBrainCorpse ??
            throw new InvalidDataException("Extracted Mother Brain corpse PNG was not bound.");
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        byte[] native = RomDataReader.ReadFixedBank(rom,
            MotherBrainCorpseArtworkDefinitions.SourceAddress,
            MotherBrainCorpseArtworkDefinitions.ByteCount);
        AssertTrue(artwork.Transfer.Span.SequenceEqual(native),
            "installed Mother Brain corpse PNG preserves all native source bytes");

        SnesVram stockVram = TransferMotherBrainCorpsePages(
            stock, new MotherBrainCorpseArtworkReadGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc")));
        SnesVram cartridgeVram = TransferMotherBrainCorpsePages(null,
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc"));
        for (int page = 0; page < MotherBrainCorpseArtworkDefinitions.VramPageSources.Length; page++)
        {
            int sourceOffset = checked((int)MotherBrainCorpseArtworkDefinitions.VramPageSources[page] -
                MotherBrainCorpseArtworkDefinitions.SourceAddress);
            int destinationOffset =
                MotherBrainCorpseArtworkDefinitions.VramPageDestinations[page] * 2;
            AssertTrue(stockVram.Bytes.Slice(destinationOffset,
                    MotherBrainCorpseArtworkDefinitions.VramPageByteCount)
                .SequenceEqual(native.AsSpan(sourceOffset,
                    MotherBrainCorpseArtworkDefinitions.VramPageByteCount)),
                $"installed Mother Brain corpse VRAM page {page} matches the native transfer");
            AssertTrue(stockVram.Bytes.Slice(destinationOffset,
                    MotherBrainCorpseArtworkDefinitions.VramPageByteCount)
                .SequenceEqual(cartridgeVram.Bytes.Slice(destinationOffset,
                    MotherBrainCorpseArtworkDefinitions.VramPageByteCount)),
                $"installed and cartridge Mother Brain corpse VRAM page {page} agree");
        }

        var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        var installedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        new MotherBrainCorpseRottingState().Initialize(nativeBus);
        new MotherBrainCorpseRottingState().Initialize(
            new MotherBrainCorpseArtworkReadGuard(installedBus), artwork);
        AssertMotherBrainCorpseBufferParity(nativeBus, installedBus);

        // The phase-three state machine owns a separate corpse processor. Verify
        // its explicit setup route as well as the room-entry processor above.
        var sequenceBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        new MotherBrainRainbowBeamAttackSequence(artwork).InitializeCorpseRotting(
            new MotherBrainCorpseArtworkReadGuard(sequenceBus));
        AssertMotherBrainCorpseBufferParity(nativeBus, sequenceBus);

        string fileName = MotherBrainCorpseArtworkDefinitions.FileName;
        using var input = new MemoryStream(File.ReadAllBytes(Path.Combine(directory, fileName)));
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(
            MotherBrainCorpseArtworkDefinitions.ByteCount);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(input, columns * 8, rows * 8);
        // The first native copy starts $C0 bytes into this sheet: tile six,
        // first pixel. That pixel becomes the high bit of $7E:9000.
        image.Pixels[6 * 8] ^= 1;
        string overrideDirectory = Path.Combine(directory, "mother-brain-corpse-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        using (var output = File.Create(overridePath))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
        SnesVram editedVram = TransferMotherBrainCorpsePages(
            edited, new MotherBrainCorpseArtworkReadGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc")));
        int editedVramOffset = MotherBrainCorpseArtworkDefinitions.VramPageDestinations[0] * 2 +
            6 * RoomCharacterAtlasFormat.BytesPerTile;
        AssertEqual((byte)(stockVram.ReadByte(editedVramOffset) ^ 0x80),
            editedVram.ReadByte(editedVramOffset),
            "Mother Brain corpse PNG edit changes live sprite VRAM");
        var editedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        new MotherBrainCorpseRottingState().Initialize(
            new MotherBrainCorpseArtworkReadGuard(editedBus), edited.MotherBrainCorpse);
        AssertEqual((byte)(installedBus.ReadByte(0x7e9000) ^ 0x80),
            editedBus.ReadByte(0x7e9000),
            "Mother Brain corpse PNG pixel edit changes live WRAM staging");
        for (int offset = 1; offset < MotherBrainCorpseRottingState.GraphicsBufferSize; offset++)
            AssertEqual(installedBus.ReadByte(0x7e9000 + offset),
                editedBus.ReadByte(0x7e9000 + offset),
                "Mother Brain corpse PNG edit leaves other staged pixels unchanged");
        var reloadedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        RoomCharacterAtlas reloaded = EnemyTileArtworkFiles.Load(directory, overrideDirectory)
            .MotherBrainCorpse!;
        new MotherBrainCorpseRottingState().Initialize(
            new MotherBrainCorpseArtworkReadGuard(reloadedBus), reloaded);
        AssertEqual(editedBus.ReadByte(0x7e9000), reloadedBus.ReadByte(0x7e9000),
            "Mother Brain corpse override survives catalog reload");

        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
            "malformed Mother Brain corpse override is rejected explicitly");

        Console.WriteLine("  Mother Brain corpse art: indexed PNG source parity, six guarded sprite VRAM pages, room/sequence staging, visible edit, reload and invalid override pass.");
    }

    private static SnesVram TransferMotherBrainCorpsePages(
        EnemyTileArtworkCatalog? artwork, ISnesAddressSpace bus)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        MethodInfo transfer = typeof(RoomEnemySystem).GetMethod(
            "ApplyMotherBrainRainbowTileTransfer", flags)!;
        for (int page = 0; page < MotherBrainCorpseArtworkDefinitions.VramPageSources.Length; page++)
        {
            var request = new MotherBrainSpriteTileTransferRequest(
                (ushort)page,
                MotherBrainCorpseArtworkDefinitions.VramPageByteCount,
                MotherBrainCorpseArtworkDefinitions.VramPageSources[page],
                MotherBrainCorpseArtworkDefinitions.VramPageDestinations[page]);
            transfer.Invoke(enemies, [request]);
        }
        return vram;
    }

    private static void AssertMotherBrainCorpseBufferParity(
        ISnesAddressSpace expected, ISnesAddressSpace actual)
    {
        for (int offset = 0; offset < MotherBrainCorpseRottingState.GraphicsBufferSize; offset++)
            AssertEqual(expected.ReadByte(MotherBrainCorpseRottingState.GraphicsBufferAddress + offset),
                actual.ReadByte(MotherBrainCorpseRottingState.GraphicsBufferAddress + offset),
                "installed Mother Brain corpse staging agrees with cartridge");
    }

    private sealed class MotherBrainCorpseArtworkReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, ISnesMutableMemory, IImportCartridgeSource
    {
        public byte ReadByte(int address)
        {
            RejectCorpseArtRead(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectCorpseArtRead(address);
            return CartridgeImportSource.Require(source).ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Mother Brain corpse test source requires WRAM.")).ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Mother Brain corpse test source requires SRAM.")).ReadSaveRamByte(address);

        private static void RejectCorpseArtRead(int address)
        {
            if (address is >= MotherBrainCorpseArtworkDefinitions.SourceAddress and
                < MotherBrainCorpseArtworkDefinitions.SourceAddress +
                    MotherBrainCorpseArtworkDefinitions.ByteCount)
                throw new InvalidOperationException(
                    $"Mother Brain corpse attempted a visual ROM read at ${address:X6}.");
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
