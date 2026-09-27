using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledMotherBrainSpecialSpriteArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        MotherBrainSpecialSpriteArtworkCatalog installed =
            stock.MotherBrainSpecialSprites ?? throw new InvalidDataException(
                "Extracted Mother Brain special sprite PNGs were not bound.");
        var rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        foreach (MotherBrainSpecialSpriteSheetDefinition sheet in
                 MotherBrainSpecialSpriteArtworkDefinitions.All)
        {
            byte[] native = RomDataReader.ReadFixedBank(rom,
                sheet.SourceAddress, sheet.ByteCount);
            AssertTrue(installed.Get(sheet).Transfer.Span.SequenceEqual(native),
                $"installed {sheet.FileName} preserves native characters");
            SnesVram installedVram = TransferMotherBrainSpecialPages(stock,
                new MotherBrainSpecialArtworkReadGuard(
                    SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"), sheet), sheet);
            SnesVram cartridgeVram = TransferMotherBrainSpecialPages(null,
                SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"), sheet);
            for (int page = 0; page < sheet.PageCount; page++)
            {
                int sourceOffset = page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount;
                int destinationOffset = (sheet.FirstDestinationWord +
                    page * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride) * 2;
                int count = MotherBrainSpecialSpriteSheetDefinition.PageByteCount;
                AssertTrue(installedVram.Bytes.Slice(destinationOffset, count)
                    .SequenceEqual(native.AsSpan(sourceOffset, count)),
                    $"installed {sheet.FileName} page {page} matches native source");
                AssertTrue(installedVram.Bytes.Slice(destinationOffset, count)
                    .SequenceEqual(cartridgeVram.Bytes.Slice(destinationOffset, count)),
                    $"installed {sheet.FileName} page {page} matches cartridge fallback");
            }

            string filePath = Path.Combine(directory, sheet.FileName);
            int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(sheet.ByteCount);
            int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
            int rows = (tileCount + columns - 1) / columns;
            using var input = new MemoryStream(File.ReadAllBytes(filePath));
            IndexedPngImage image = IndexedPng.Read(input, columns * 8, rows * 8);
            image.Pixels[0] ^= 1;
            string overrideDirectory = Path.Combine(directory,
                "mother-brain-special-" + sheet.SourceAddress.ToString("X6"));
            Directory.CreateDirectory(overrideDirectory);
            string overridePath = Path.Combine(overrideDirectory, sheet.FileName);
            using (var output = File.Create(overridePath))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
            SnesVram editedVram = TransferMotherBrainSpecialPages(edited,
                new MotherBrainSpecialArtworkReadGuard(
                    SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"), sheet), sheet);
            int firstDestination = sheet.FirstDestinationWord * 2;
            AssertEqual((byte)(installedVram.ReadByte(firstDestination) ^ 0x80),
                editedVram.ReadByte(firstDestination),
                $"{sheet.FileName} PNG edit changes live OBJ VRAM");
            AssertTrue(installedVram.Bytes.Slice(firstDestination + 1,
                    MotherBrainSpecialSpriteSheetDefinition.PageByteCount - 1)
                .SequenceEqual(editedVram.Bytes.Slice(firstDestination + 1,
                    MotherBrainSpecialSpriteSheetDefinition.PageByteCount - 1)),
                $"{sheet.FileName} edit leaves neighboring bytes unchanged");
            RoomCharacterAtlas reloaded = EnemyTileArtworkFiles.Load(directory, overrideDirectory)
                .MotherBrainSpecialSprites!.Get(sheet);
            AssertTrue(reloaded.Transfer.Span.SequenceEqual(
                    edited.MotherBrainSpecialSprites!.Get(sheet).Transfer.Span),
                $"{sheet.FileName} override survives catalog reload");

            File.WriteAllBytes(overridePath, [0]);
            AssertThrows<InvalidDataException>(
                () => EnemyTileArtworkFiles.Load(directory, overrideDirectory),
                $"malformed {sheet.FileName} override fails explicitly");
        }

        // Unlike the other two lists, Baby loading reads its four native records
        // from $A9:8FE5. Check every compiled sheet coordinate against that list.
        MotherBrainSpecialSpriteSheetDefinition baby =
            MotherBrainSpecialSpriteArtworkDefinitions.BabyMetroid;
        for (int page = 0; page < baby.PageCount; page++)
        {
            int record = MotherBrainTileTransferRomData.BabyTileList +
                page * MotherBrainTileTransferRomData.RecordSize;
            AssertEqual(MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                RomDataReader.ReadWordFixedBank(rom, record),
                $"Baby tile record {page} size");
            uint source = (uint)(RomDataReader.ReadWordFixedBank(rom, record + 2) |
                rom.ReadByte(record + 4) << 16);
            AssertEqual((uint)(baby.SourceAddress +
                page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount), source,
                $"Baby tile record {page} source");
            AssertEqual((ushort)(baby.FirstDestinationWord +
                page * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride),
                RomDataReader.ReadWordFixedBank(rom, record + 5),
                $"Baby tile record {page} destination");
        }
        Console.WriteLine(
            "  Mother Brain special sprites: Baby, attack and exploded-door pages match cartridge records, guarded live uploads, PNG edits, reload and invalid override pass.");
    }

    private static SnesVram TransferMotherBrainSpecialPages(
        EnemyTileArtworkCatalog? artwork, ISnesAddressSpace bus,
        MotherBrainSpecialSpriteSheetDefinition sheet)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        MethodInfo transfer = typeof(RoomEnemySystem).GetMethod(
            "ApplyMotherBrainRainbowTileTransfer", flags)!;
        for (int page = 0; page < sheet.PageCount; page++)
        {
            var request = new MotherBrainSpriteTileTransferRequest(
                (ushort)page,
                MotherBrainSpecialSpriteSheetDefinition.PageByteCount,
                unchecked((uint)(sheet.SourceAddress +
                    page * MotherBrainSpecialSpriteSheetDefinition.PageByteCount)),
                unchecked((ushort)(sheet.FirstDestinationWord +
                    page * MotherBrainSpecialSpriteSheetDefinition.DestinationWordStride)));
            transfer.Invoke(enemies, [request]);
        }
        return vram;
    }

    private sealed class MotherBrainSpecialArtworkReadGuard(
        ISnesAddressSpace source, MotherBrainSpecialSpriteSheetDefinition sheet) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address >= sheet.SourceAddress && address < sheet.SourceAddress + sheet.ByteCount
                ? throw new InvalidOperationException(
                    $"{sheet.FileName} attempted a visual ROM read at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
