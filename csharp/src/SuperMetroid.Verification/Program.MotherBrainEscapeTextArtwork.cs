using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledMotherBrainEscapeTextArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        RoomCharacterAtlas artwork = stock.MotherBrainEscapeText ??
            throw new InvalidDataException("Extracted Mother Brain escape-text PNG was not bound.");
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        byte[] native = RomDataReader.ReadFixedBank(rom,
            MotherBrainEscapeTextArtworkDefinitions.SourceAddress,
            MotherBrainEscapeTextArtworkDefinitions.ByteCount);
        AssertTrue(artwork.Transfer.Span.SequenceEqual(native),
            "installed Mother Brain escape-text PNG preserves native characters");

        SnesVram installedVram = TransferMotherBrainEscapeTextPages(stock,
            new MotherBrainEscapeTextReadGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc")));
        SnesVram cartridgeVram = TransferMotherBrainEscapeTextPages(null,
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc"));
        for (int page = 0; page < MotherBrainEscapeTextArtworkDefinitions.PageSources.Length; page++)
        {
            int sourceOffset = checked((int)MotherBrainEscapeTextArtworkDefinitions.PageSources[page] -
                MotherBrainEscapeTextArtworkDefinitions.SourceAddress);
            int destinationOffset =
                MotherBrainEscapeTextArtworkDefinitions.PageDestinations[page] * 2;
            int byteCount = MotherBrainEscapeTextArtworkDefinitions.PageByteCounts[page];
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
        IndexedPngImage image = IndexedPng.Read(input, columns * 8, rows * 8);
        image.Pixels[0] ^= 1;
        string overrideDirectory = Path.Combine(directory, "mother-brain-escape-text-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        using (var output = File.Create(overridePath))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
        SnesVram editedVram = TransferMotherBrainEscapeTextPages(edited,
            new MotherBrainEscapeTextReadGuard(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc")));
        int firstDestination = MotherBrainEscapeTextArtworkDefinitions.PageDestinations[0] * 2;
        AssertEqual((byte)(installedVram.ReadByte(firstDestination) ^ 0x80),
            editedVram.ReadByte(firstDestination),
            "Mother Brain escape-text PNG pixel edit changes live sprite VRAM");
        AssertTrue(installedVram.Bytes.Slice(firstDestination + 1,
                MotherBrainEscapeTextArtworkDefinitions.PageByteCounts[0] - 1)
            .SequenceEqual(editedVram.Bytes.Slice(firstDestination + 1,
                MotherBrainEscapeTextArtworkDefinitions.PageByteCounts[0] - 1)),
            "Mother Brain escape-text edit leaves neighboring source bytes unchanged");
        RoomCharacterAtlas reloaded = EnemyTileArtworkFiles.Load(directory, overrideDirectory)
            .MotherBrainEscapeText!;
        AssertTrue(reloaded.Transfer.Span.SequenceEqual(edited.MotherBrainEscapeText!.Transfer.Span),
            "Mother Brain escape-text override survives catalog reload");

        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
            "malformed Mother Brain escape-text override is rejected explicitly");
        Console.WriteLine(
            "  Mother Brain escape text art: five guarded OBJ pages, cartridge parity, visible edit, reload and invalid override pass.");
    }

    private static SnesVram TransferMotherBrainEscapeTextPages(
        EnemyTileArtworkCatalog? artwork, ISnesAddressSpace bus)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        MethodInfo transfer = typeof(RoomEnemySystem).GetMethod(
            "ApplyMotherBrainRainbowTileTransfer", flags)!;
        for (int page = 0; page < MotherBrainEscapeTextArtworkDefinitions.PageSources.Length; page++)
        {
            var request = new MotherBrainSpriteTileTransferRequest(
                (ushort)(page + 2),
                MotherBrainEscapeTextArtworkDefinitions.PageByteCounts[page],
                MotherBrainEscapeTextArtworkDefinitions.PageSources[page],
                MotherBrainEscapeTextArtworkDefinitions.PageDestinations[page]);
            transfer.Invoke(enemies, [request]);
        }
        return vram;
    }

    private sealed class MotherBrainEscapeTextReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address is >= MotherBrainEscapeTextArtworkDefinitions.SourceAddress and
                < MotherBrainEscapeTextArtworkDefinitions.SourceAddress +
                    MotherBrainEscapeTextArtworkDefinitions.ByteCount
                ? throw new InvalidOperationException(
                    $"Mother Brain escape text attempted a visual ROM read at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
