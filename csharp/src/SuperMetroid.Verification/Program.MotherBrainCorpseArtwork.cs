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
        var rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        byte[] native = RomDataReader.ReadFixedBank(rom,
            MotherBrainCorpseArtworkDefinitions.SourceAddress,
            MotherBrainCorpseArtworkDefinitions.ByteCount);
        AssertTrue(artwork.Transfer.Span.SequenceEqual(native),
            "installed Mother Brain corpse PNG preserves all native source bytes");

        var nativeBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var installedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        new MotherBrainCorpseRottingState().Initialize(nativeBus);
        new MotherBrainCorpseRottingState().Initialize(
            new MotherBrainCorpseArtworkReadGuard(installedBus), artwork);
        AssertMotherBrainCorpseBufferParity(nativeBus, installedBus);

        // The phase-three state machine owns a separate corpse processor. Verify
        // its explicit setup route as well as the room-entry processor above.
        var sequenceBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
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
        var editedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        new MotherBrainCorpseRottingState().Initialize(
            new MotherBrainCorpseArtworkReadGuard(editedBus), edited.MotherBrainCorpse);
        AssertEqual((byte)(installedBus.ReadByte(0x7e9000) ^ 0x80),
            editedBus.ReadByte(0x7e9000),
            "Mother Brain corpse PNG pixel edit changes live WRAM staging");
        for (int offset = 1; offset < MotherBrainCorpseRottingState.GraphicsBufferSize; offset++)
            AssertEqual(installedBus.ReadByte(0x7e9000 + offset),
                editedBus.ReadByte(0x7e9000 + offset),
                "Mother Brain corpse PNG edit leaves other staged pixels unchanged");
        var reloadedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
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

        Console.WriteLine("  Mother Brain corpse art: indexed PNG source parity, guarded room/sequence staging, visible edit, reload and invalid override pass.");
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
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= MotherBrainCorpseArtworkDefinitions.SourceAddress and
                < MotherBrainCorpseArtworkDefinitions.SourceAddress +
                    MotherBrainCorpseArtworkDefinitions.ByteCount
                ? throw new InvalidOperationException(
                    $"Mother Brain corpse attempted a visual ROM read at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
