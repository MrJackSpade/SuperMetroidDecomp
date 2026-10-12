using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static SuiteTimingVerification;

/// <summary>Represents mother brain corpse artwork verification.</summary>
internal static class MotherBrainCorpseArtworkVerification
{
    /// <summary>Verifies mother brain corpse stock artwork.</summary>
    internal static void VerifyMotherBrainCorpseStockArtwork()
    {
        using var temporary = new TestScratchDirectory("map-catalog");
        var source = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        EnemyTileArtworkFiles.Extract(source, temporary.Root, SupportedCartridge.Sha256);
        EnemyTileArtworkFiles.ValidateStock(temporary.Root);
        Suite(nameof(VerifyInstalledMotherBrainCorpseArtwork), () => VerifyInstalledMotherBrainCorpseArtwork(temporary.Root, EnemyTileArtworkFiles.Load(temporary.Root, null)));
    }

    /// <summary>Reads native mother brain corpse artwork.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain corpse character sheet")]
    private static byte[] ReadNativeMotherBrainCorpseArtwork(CartridgeImportAddressSpace rom) =>
        RomDataReader.ReadFixedBank(rom,
            MotherBrainCorpseArtworkDefinitions.SourceAddress,
            MotherBrainCorpseArtworkDefinitions.ByteCount);

    /// <summary>Verifies installed mother brain corpse artwork.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    internal static void VerifyInstalledMotherBrainCorpseArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        RoomCharacterAtlas artwork = stock.MotherBrainCorpse ??
            throw new InvalidDataException("Extracted Mother Brain corpse PNG was not bound.");
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        byte[] native = ReadNativeMotherBrainCorpseArtwork(rom);
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
        WorkRamByteTable nativeTiles = MotherBrainCorpseTiles(nativeBus);
        for (int row = 0; row < 6; row++)
        {
            ReadOnlySpan<byte> rowBytes = native.AsSpan(row * 0x200 + 0xc0, row < 4 ? 0xc0 : 0xe0);
            for (int offset = 0; offset < rowBytes.Length; offset++)
                nativeTiles[row * 0xe0 + offset] = rowBytes[offset];
        }
        CorpseRotTable nativeRotTable = new EnemyScratchMemory(nativeBus).MotherBrainRotTable;
        for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
        {
            nativeRotTable.SetYOffset(index, (ushort)(47 - index));
            nativeRotTable.SetDelay(index, (ushort)(index * 2));
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
        IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
        // The first native copy starts $C0 bytes into this sheet: tile six,
        // first pixel. That pixel becomes the high bit of $7E:9000.
        image.Pixels[6 * 8] ^= 1;
        string overrideDirectory = Path.Combine(directory, "mother-brain-corpse-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        using (var output = File.Create(overridePath))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
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
        WorkRamByteTable installedTiles = MotherBrainCorpseTiles(installedBus);
        WorkRamByteTable editedTiles = MotherBrainCorpseTiles(editedBus);
        AssertEqual((byte)(installedTiles[0] ^ 0x80),
            editedTiles[0],
            "Mother Brain corpse PNG pixel edit changes live WRAM staging");
        for (int offset = 1; offset < MotherBrainCorpseRottingState.GraphicsBufferSize; offset++)
            AssertEqual(installedTiles[offset],
                editedTiles[offset],
                "Mother Brain corpse PNG edit leaves other staged pixels unchanged");
        var reloadedBus = SuperMetroidAddressSpace.CreateWithoutCartridge();
        RoomCharacterAtlas reloaded = EnemyTileArtworkFiles.Load(directory, overrideDirectory)
            .MotherBrainCorpse!;
        new MotherBrainCorpseRottingState().Initialize(reloadedBus, reloaded);
        AssertEqual(editedTiles[0], MotherBrainCorpseTiles(reloadedBus)[0],
            "Mother Brain corpse override survives catalog reload");

        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
            "malformed Mother Brain corpse override is rejected explicitly");

        Console.WriteLine("  Mother Brain corpse art: indexed PNG import parity, six RAM-only sprite VRAM pages, independent staging oracle, room/sequence staging, edit/reload and invalid override pass.");
    }

    /// <summary>Transfer mother brain corpse pages.</summary>
    private static SnesVram TransferMotherBrainCorpsePages(
        EnemyTileArtworkCatalog artwork, ISnesAddressSpace bus)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        enemies.BindFixtureBus(bus);
        enemies.Presentation._vram = vram;
        for (int page = 0; page < MotherBrainCorpseArtworkDefinitions.RowCount; page++)
        {
            var request = new MotherBrainSpriteTileTransferRequest(
                (ushort)page,
                MotherBrainCorpseArtworkDefinitions.VramPageByteCount,
                MotherBrainCorpseArtworkDefinitions.VramPageSource(page),
                MotherBrainCorpseArtworkDefinitions.VramPageDestination(page));
            enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(request);
        }
        return vram;
    }

    /// <summary>Mother brain corpse tiles.</summary>
    private static WorkRamByteTable MotherBrainCorpseTiles(ISnesMemoryBus bus) =>
        new EnemyScratchMemory(bus).MotherBrainCorpseTiles;

    /// <summary>Asserts mother brain corpse buffer parity.</summary>
    private static void AssertMotherBrainCorpseBufferParity(
        SuperMetroidAddressSpace expected, SuperMetroidAddressSpace actual)
    {
        WorkRamByteTable expectedTiles = MotherBrainCorpseTiles(expected);
        WorkRamByteTable actualTiles = MotherBrainCorpseTiles(actual);
        for (int offset = 0; offset < MotherBrainCorpseRottingState.GraphicsBufferSize; offset++)
            AssertEqual(expectedTiles[offset], actualTiles[offset],
                "installed Mother Brain corpse staging agrees with cartridge");
        for (int index = 0; index < MotherBrainCorpseRottingState.EntryCount; index++)
            AssertEqual(MotherBrainCorpseRottingState.ReadEntry(expected, index),
                MotherBrainCorpseRottingState.ReadEntry(actual, index), "installed native rot-table initialization");
    }

    /// <summary>Mother brain corpse stock artwork command.</summary>
    [VerificationCommand("--mother-brain-corpse-stock-artwork")]
    internal static int MotherBrainCorpseStockArtworkCommand()
    {
        Suite(nameof(VerifyMotherBrainCorpseStockArtwork), () => VerifyMotherBrainCorpseStockArtwork());
        return 0;
    }
}
