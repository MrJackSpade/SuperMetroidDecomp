using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyMotherBrainCorpseStockArtwork()
    {
        using var temporary = new TestTempDirectory("map-catalog");
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        EnemyTileArtworkFiles.Extract(source, temporary.Root, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(temporary.Root);
        Suite(nameof(VerifyInstalledMotherBrainCorpseArtwork), () => VerifyInstalledMotherBrainCorpseArtwork(temporary.Root, EnemyTileArtworkFiles.Load(temporary.Root, null)));
    }

    private static void VerifyInstalledMotherBrainCorpseArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        RoomCharacterAtlas artwork = stock.MotherBrainCorpse ??
            throw new InvalidDataException("Extracted Mother Brain corpse PNG was not bound.");
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        byte[] native = RomDataReader.ReadFixedBank(rom,
            MotherBrainCorpseArtworkDefinitions.SourceAddress,
            MotherBrainCorpseArtworkDefinitions.ByteCount);
        AssertTrue(artwork.Transfer.Span.SequenceEqual(native),
            "installed Mother Brain corpse PNG preserves all native source bytes");

        // Only the oracle/import above receives cartridge bytes. Production transfers and
        // staging run against the same cartridge-incapable memory type as installed games.
        SnesVram stockVram = TransferMotherBrainCorpsePages(stock,
            SuperMetroidAddressSpace.CreateWithoutCartridge());
        for (int page = 0; page < MotherBrainCorpseArtworkDefinitions.RowCount; page++)
        {
            int sourceOffset = checked((int)MotherBrainCorpseArtworkDefinitions.VramPageSource(page) -
                MotherBrainCorpseArtworkDefinitions.SourceAddress);
            int destinationOffset =
                MotherBrainCorpseArtworkDefinitions.VramPageDestination(page) * 2;
            AssertTrue(stockVram.Bytes.Slice(destinationOffset,
                    MotherBrainCorpseArtworkDefinitions.VramPageByteCount)
                .SequenceEqual(native.AsSpan(sourceOffset,
                    MotherBrainCorpseArtworkDefinitions.VramPageByteCount)),
                $"installed Mother Brain corpse VRAM page {page} matches the native transfer");
        }

        var nativeBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var installedBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        // Independent $A9:E08B oracle: copy each right-hand frame's seven-tile row,
        // omitting the transparent last tile in the first four rows. No runtime fallback.
        for (int row = 0; row < 6; row++)
            native.AsSpan(row * 0x200 + 0xc0, row < 4 ? 0xc0 : 0xe0).CopyTo(
                nativeBus.WorkRam.Slice(0x9000 + row * 0xe0));
        for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
        {
            int address = 0x9700 + index * 4;
            nativeBus.WriteByte(0x7e0000 + address, (byte)(47 - index));
            nativeBus.WriteByte(0x7e0000 + address + 2, (byte)(index * 2));
        }
        new MotherBrainCorpseRottingState().Initialize(installedBus, artwork);
        AssertMotherBrainCorpseBufferParity(nativeBus, installedBus);

        // The phase-three state machine owns a separate corpse processor. Verify
        // its explicit setup route as well as the room-entry processor above.
        var sequenceBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        new MotherBrainRainbowBeamAttackSequence(artwork).InitializeCorpseRotting(sequenceBus);
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
        SnesVram editedVram = TransferMotherBrainCorpsePages(edited,
            SuperMetroidAddressSpace.CreateWithoutCartridge());
        int editedVramOffset = MotherBrainCorpseArtworkDefinitions.VramPageDestination(0) * 2 +
            6 * RoomCharacterAtlasFormat.BytesPerTile;
        AssertEqual((byte)(stockVram.ReadByte(editedVramOffset) ^ 0x80),
            editedVram.ReadByte(editedVramOffset),
            "Mother Brain corpse PNG edit changes live sprite VRAM");
        var editedBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        new MotherBrainCorpseRottingState().Initialize(editedBus, edited.MotherBrainCorpse!);
        AssertEqual((byte)(installedBus.ReadWorkRamByte(0x7e9000) ^ 0x80),
            editedBus.ReadWorkRamByte(0x7e9000),
            "Mother Brain corpse PNG pixel edit changes live WRAM staging");
        for (int offset = 1; offset < MotherBrainCorpseRottingState.GraphicsBufferSize; offset++)
            AssertEqual(installedBus.ReadWorkRamByte(0x7e9000 + offset),
                editedBus.ReadWorkRamByte(0x7e9000 + offset),
                "Mother Brain corpse PNG edit leaves other staged pixels unchanged");
        var reloadedBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        RoomCharacterAtlas reloaded = EnemyTileArtworkFiles.Load(directory, overrideDirectory)
            .MotherBrainCorpse!;
        new MotherBrainCorpseRottingState().Initialize(reloadedBus, reloaded);
        AssertEqual(editedBus.ReadWorkRamByte(0x7e9000), reloadedBus.ReadWorkRamByte(0x7e9000),
            "Mother Brain corpse override survives catalog reload");

        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
            "malformed Mother Brain corpse override is rejected explicitly");

        Console.WriteLine("  Mother Brain corpse art: indexed PNG import parity, six RAM-only sprite VRAM pages, independent staging oracle, room/sequence staging, edit/reload and invalid override pass.");
    }

    private static SnesVram TransferMotherBrainCorpsePages(
        EnemyTileArtworkCatalog artwork, ISnesAddressSpace bus)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        MethodInfo transfer = typeof(RoomEnemySystem).GetMethod(
            "ApplyMotherBrainRainbowTileTransfer", flags)!;
        for (int page = 0; page < MotherBrainCorpseArtworkDefinitions.RowCount; page++)
        {
            var request = new MotherBrainSpriteTileTransferRequest(
                (ushort)page,
                MotherBrainCorpseArtworkDefinitions.VramPageByteCount,
                MotherBrainCorpseArtworkDefinitions.VramPageSource(page),
                MotherBrainCorpseArtworkDefinitions.VramPageDestination(page));
            transfer.Invoke(enemies, [request]);
        }
        return vram;
    }

    private static void AssertMotherBrainCorpseBufferParity(
        SuperMetroidAddressSpace expected, SuperMetroidAddressSpace actual)
    {
        for (int offset = 0; offset < MotherBrainCorpseRottingState.GraphicsBufferSize; offset++)
            AssertEqual(expected.ReadWorkRamByte(MotherBrainCorpseRottingState.GraphicsBufferAddress + offset),
                actual.ReadWorkRamByte(MotherBrainCorpseRottingState.GraphicsBufferAddress + offset),
                "installed Mother Brain corpse staging agrees with cartridge");
        for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
            AssertEqual(MotherBrainCorpseRottingState.ReadEntry(expected, index),
                MotherBrainCorpseRottingState.ReadEntry(actual, index), "installed native rot-table initialization");
    }
}
