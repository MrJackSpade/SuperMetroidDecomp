using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledDeadTorizoArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        // The corpse initializer copies discontinuous portions of the ordinary ED3F
        // enemy sheet to WRAM. Later rotting frames draw sand words from that same
        // sheet. Guard the entire source interval so neither path can accidentally
        // work by reading the cartridge despite a bound installed catalog.
        var nativeBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var installedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        RoomEnemySystem native = InitializeDeadTorizoArtwork(nativeBus, null);
        RoomEnemySystem installed = InitializeDeadTorizoArtwork(installedBus, stock);
        AssertDeadTorizoBufferParity(nativeBus, installedBus, 0x7e2000, 0x1000,
            "installed dead-Torizo initial corpse sheet matches cartridge WRAM");

        CopyDeadTorizoSandLine(native, 0);
        CopyDeadTorizoSandLine(installed, 0);
        AssertDeadTorizoBufferParity(nativeBus, installedBus, 0x7e9500, 0x200,
            "installed dead-Torizo sand line matches cartridge WRAM");

        OamBuffer nativeCorpse = DrawDeadTorizoCorpseFrame(null, nativeBus, 0, 0);
        OamBuffer installedCorpse = DrawDeadTorizoCorpseFrame(stock, installedBus, 0, 0);
        AssertTrue(nativeCorpse.LowTable.SequenceEqual(installedCorpse.LowTable) &&
                   nativeCorpse.HighTable.SequenceEqual(installedCorpse.HighTable) &&
                   nativeCorpse.NextByteOffset == installedCorpse.NextByteOffset,
            "Dead Torizo private corpse hook draws installed native-parity OAM without ROM reads");
        AssertEqual(0, DrawDeadTorizoCorpseFrame(stock, installedBus, 0, 188).NextByteOffset,
            "Dead Torizo private hook retains its above-screen cull");

        var missing = new EnemyTileArtworkCatalog(
            new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>());
        AssertThrows<InvalidDataException>(() => InitializeDeadTorizoArtwork(
                SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"), missing),
            "bound installation missing the dead-Torizo sheet fails instead of reading ROM");

        string fileName = EnemyTileArtworkFormat.FileName(RoomEnemySystem.DeadTorizoDefinition);
        using var source = new MemoryStream(File.ReadAllBytes(Path.Combine(directory, fileName)));
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(0x1800);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(source, columns * 8, rows * 8);
        // Source offset $0120 is tile 9, first row, first pixel. It is copied
        // to $7E:2060, so flipping its low indexed bit flips that planar MSB.
        image.Pixels[9 * 8] ^= 1;
        string overrideDirectory = Path.Combine(directory, "dead-torizo-overrides");
        Directory.CreateDirectory(overrideDirectory);
        using (var output = File.Create(Path.Combine(overrideDirectory, fileName)))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(directory, overrideDirectory);
        var editedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        InitializeDeadTorizoArtwork(editedBus, edited);
        AssertEqual((byte)(installedBus.ReadByte(0x7e2060) ^ 0x80),
            editedBus.ReadByte(0x7e2060),
            "dead-Torizo PNG pixel edit reaches the live corpse staging buffer");
        for (int offset = 0; offset < 0x1000; offset++)
        {
            if (offset == 0x60)
                continue;
            AssertEqual(installedBus.ReadByte(0x7e2000 + offset),
                editedBus.ReadByte(0x7e2000 + offset),
                "dead-Torizo PNG edit leaves neighboring corpse staging bytes unchanged");
        }
        var reloadedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        InitializeDeadTorizoArtwork(reloadedBus,
            EnemyTileArtworkFiles.Load(directory, overrideDirectory));
        AssertEqual(editedBus.ReadByte(0x7e2060), reloadedBus.ReadByte(0x7e2060),
            "dead-Torizo PNG edit survives catalog reload");

        Console.WriteLine("  Dead Torizo artwork: guarded initial corpse and sand-line ROM parity, visible PNG edit and reload pass.");
    }

    private static RoomEnemySystem InitializeDeadTorizoArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog? artwork)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        ISnesAddressSpace source = artwork is null
            ? bus
            : new DeadTorizoArtworkReadGuard(bus);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
        typeof(RoomEnemySystem).GetMethod("InitializeDeadTorizo", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies)(enemies.Slots[0]);
        return enemies;
    }

    private static void CopyDeadTorizoSandLine(RoomEnemySystem enemies, ushort line)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetMethod("CopyDeadTorizoSandLine", flags)!
            .CreateDelegate<Action<ushort>>(enemies)(line);
    }

    private static OamBuffer DrawDeadTorizoCorpseFrame(
        EnemyTileArtworkCatalog? artwork, SuperMetroidAddressSpace bus,
        ushort cameraX, ushort cameraY)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        RoomEnemySystem enemies = InitializeDeadTorizoArtwork(bus, artwork);
        if (artwork is not null)
            typeof(RoomEnemySystem).GetField("_bus", flags)!
                .SetValue(enemies, new DeadTorizoOamReadGuard(bus));
        var oam = new OamBuffer();
        typeof(RoomEnemySystem).GetMethod("DrawDeadTorizoHook", flags)!
            .CreateDelegate<Action<OamBuffer, ushort, ushort>>(enemies)(
                oam, cameraX, cameraY);
        return oam;
    }

    private static void AssertDeadTorizoBufferParity(
        ISnesAddressSpace expected, ISnesAddressSpace actual, int address, int length,
        string description)
    {
        for (int offset = 0; offset < length; offset++)
            AssertEqual(expected.ReadByte(address + offset), actual.ReadByte(address + offset),
                description);
    }

    private sealed class DeadTorizoArtworkReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xb7a800 and < 0xb7c000
                ? throw new InvalidOperationException(
                    $"Dead Torizo read installed artwork from ROM at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }

    private sealed class DeadTorizoOamReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xa9d761 and < 0xa9d77c
                ? throw new InvalidOperationException(
                    $"Dead Torizo drew native OAM from ROM at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
