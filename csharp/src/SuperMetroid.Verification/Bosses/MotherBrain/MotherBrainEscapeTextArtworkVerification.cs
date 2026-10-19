using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Bosses.MotherBrain;

/// <summary>Checks installed Mother Brain escape-text art and its five OBJ VRAM transfers.</summary>
internal static class MotherBrainEscapeTextArtworkVerification
{
    /// <summary>Compares stock art with the cartridge and checks an edited PNG changes only its intended live VRAM byte.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for the Mother Brain escape-text characters and transfer records")]
    internal static void VerifyInstalledMotherBrainEscapeTextArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        RoomCharacterAtlas artwork = stock.MotherBrainEscapeText ??
            throw new InvalidDataException("Extracted Mother Brain escape-text PNG was not bound.");
        var rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        byte[] native = RomDataReader.ReadFixedBank(rom,
            MotherBrainEscapeTextArtworkDefinitions.SourceAddress,
            MotherBrainEscapeTextArtworkDefinitions.ByteCount);
        AssertTrue(artwork.Transfer.Span.SequenceEqual(native),
            "installed Mother Brain escape-text PNG preserves native characters");

        SnesVram installedVram = TransferMotherBrainEscapeTextPages(stock);
        // The oracle owns cartridge bytes; production has no uninstalled-ROM fallback.
        var cartridgeVram = new SnesVram();
        for (int page = 0; page < 5; page++)
        {
            int record = 0xa6c4d9 + 7 * page;
            int count = SnesLittleEndian.Word(CpuBusRegionRead.Read(rom, record), CpuBusRegionRead.Read(rom, record + 1));
            int source = (int)new SnesAddress(CpuBusRegionRead.Read(rom, record + 4), SnesLittleEndian.Word(CpuBusRegionRead.Read(rom, record + 2), CpuBusRegionRead.Read(rom, record + 3)));
            int destination = SnesLittleEndian.Word(CpuBusRegionRead.Read(rom, record + 5), CpuBusRegionRead.Read(rom, record + 6));
            cartridgeVram.LoadBytes(destination * 2, RomDataReader.ReadFixedBank(rom, source, count));
        }
        for (int page = 0; page < MotherBrainEscapeTextArtworkDefinitions.PageCount; page++)
        {
            int sourceOffset = checked((int)MotherBrainEscapeTextArtworkDefinitions.PageSource(page) -
                MotherBrainEscapeTextArtworkDefinitions.SourceAddress);
            int destinationOffset =
                MotherBrainEscapeTextArtworkDefinitions.PageDestination(page) * 2;
            int byteCount = MotherBrainEscapeTextArtworkDefinitions.PageByteCount(page);
            AssertTrue(installedVram.Bytes.Slice(destinationOffset, byteCount)
                .SequenceEqual(native.AsSpan(sourceOffset, byteCount)),
                $"installed Mother Brain escape-text VRAM page {page} matches native bytes");
            AssertTrue(installedVram.Bytes.Slice(destinationOffset, byteCount)
                .SequenceEqual(cartridgeVram.Bytes.Slice(destinationOffset, byteCount)),
                $"installed and cartridge Mother Brain escape-text VRAM page {page} agree");
        }

        string fileName = MotherBrainEscapeTextArtworkDefinitions.FileName;
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(
            MotherBrainEscapeTextArtworkDefinitions.ByteCount);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        using var input = new MemoryStream(File.ReadAllBytes(Path.Combine(directory, fileName)));
        IndexedPngImage image = IndexedPng.Read(input, new(checked((ushort)(columns * 8)), checked((ushort)(rows * 8))));
        image.Pixels[0] ^= 1;
        string overrideDirectory = Path.Combine(directory, "mother-brain-escape-text-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        using (var output = File.Create(overridePath))
            IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Palette);
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
        SnesVram editedVram = TransferMotherBrainEscapeTextPages(edited);
        int firstDestination = MotherBrainEscapeTextArtworkDefinitions.PageDestination(0) * 2;
        AssertEqual((byte)(installedVram.ReadByte(firstDestination) ^ 0x80),
            editedVram.ReadByte(firstDestination),
            "Mother Brain escape-text PNG pixel edit changes live sprite VRAM");
        AssertTrue(installedVram.Bytes.Slice(firstDestination + 1,
                MotherBrainEscapeTextArtworkDefinitions.PageByteCount(0) - 1)
            .SequenceEqual(editedVram.Bytes.Slice(firstDestination + 1,
                MotherBrainEscapeTextArtworkDefinitions.PageByteCount(0) - 1)),
            "Mother Brain escape-text edit leaves neighboring source bytes unchanged");
        RoomCharacterAtlas reloaded = (EnemyTileArtworkFiles.Load(directory, overrideDirectory)
            .MotherBrainEscapeText ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(directory, overrideDirectory) .MotherBrainEscapeText."));
        AssertTrue((reloaded ?? throw new InvalidOperationException("Verification requires reloaded.")).Transfer.Span.SequenceEqual((edited.MotherBrainEscapeText ?? throw new InvalidOperationException("Verification requires edited.MotherBrainEscapeText.")).Transfer.Span),
            "Mother Brain escape-text override survives catalog reload");

        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
            "malformed Mother Brain escape-text override is rejected explicitly");
        Console.WriteLine(
            "  Mother Brain escape text art: five guarded OBJ pages, cartridge parity, visible edit, reload and invalid override pass.");
    }

    /// <summary>Applies all five escape-text transfer requests through the production Mother Brain path.</summary>
    private static SnesVram TransferMotherBrainEscapeTextPages(
        EnemyTileArtworkCatalog artwork)
    {
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation.BindVram(vram);
        for (int page = 0; page < MotherBrainEscapeTextArtworkDefinitions.PageCount; page++)
        {
            var request = new MotherBrainSpriteTileTransferRequest(
                (ushort)(page + 2),
                MotherBrainEscapeTextArtworkDefinitions.PageByteCount(page),
                MotherBrainEscapeTextArtworkDefinitions.PageSource(page),
                MotherBrainEscapeTextArtworkDefinitions.PageDestination(page));
            enemies.MotherBrainEncounter.ApplyMotherBrainRainbowTileTransfer(request);
        }
        return vram;
    }

}
