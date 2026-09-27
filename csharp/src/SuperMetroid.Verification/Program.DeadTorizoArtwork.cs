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
        VerifyDeadTorizoVramTransferDefinitions(stock);
        VerifyDeadTorizoStationaryVisual(stock);
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

    private static void VerifyDeadTorizoStationaryVisual(
        EnemyTileArtworkCatalog stock)
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        int selector = 0xa90000 | DeadTorizoArtworkDefinitions.StationaryOperand;
        ushort nativePointer = unchecked((ushort)(
            rom.ReadByte(selector) | rom.ReadByte(selector + 1) << 8));
        AssertEqual(DeadTorizoArtworkDefinitions.StationarySpritemap, nativePointer,
            "Dead Torizo stationary visual identity matches its native list");
        AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                RoomEnemySystem.DeadTorizoDefinition,
                DeadTorizoArtworkDefinitions.StationaryOperand, out ushort compiledPointer),
            "Dead Torizo stationary visual selector is installed");
        AssertEqual(nativePointer, compiledPointer,
            "installed stationary selector preserves the physical frame identity");
        AssertTrue(stock.Spritemaps!.TryGetDisplay(
                DeadTorizoArtworkDefinitions.SpritemapBank, nativePointer,
                out ReadOnlyMemory<EnemySpritemapPart> installedParts),
            "Dead Torizo stationary OAM has editable installed parts");
        var nativeOam = new OamBuffer();
        var installedOam = new OamBuffer();
        nativeOam.AddEnemySpritemap(rom, DeadTorizoArtworkDefinitions.SpritemapBank,
            nativePointer, 128, 128, 0, 0);
        installedOam.AddEnemySpritemap(installedParts.Span, 128, 128, 0, 0);
        AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                   nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                   nativeOam.NextByteOffset == installedOam.NextByteOffset,
            "Dead Torizo stationary installed OAM matches all native sprite entries");
        AssertEqual(25 * 4, installedOam.NextByteOffset,
            "Dead Torizo stationary map has 25 native OAM parts");
        Console.WriteLine("  Dead Torizo stationary visual: compiled selector, editable 25-part OAM, and native composition parity pass.");
    }

    private static void VerifyDeadTorizoVramTransferDefinitions(
        EnemyTileArtworkCatalog stock)
    {
        var rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        for (ushort phase = 0; phase < 2; phase++)
        {
            ushort table = phase == 0
                ? DeadTorizoVramTransferDefinitions.EvenTable
                : DeadTorizoVramTransferDefinitions.OddTable;
            ReadOnlySpan<DeadTorizoVramTransferDefinition> records =
                DeadTorizoVramTransferDefinitions.ForPhase(phase);
            AssertEqual(7, records.Length,
                $"Dead Torizo phase {phase} compiled descriptor count");
            for (int index = 0; index < records.Length; index++)
            {
                int address = 0xa90000 | unchecked((ushort)(table +
                    index * DeadTorizoVramTransferDefinitions.RecordByteCount));
                DeadTorizoVramTransferDefinition record = records[index];
                AssertEqual(record.SizeInBytes, ReadWord(address),
                    $"Dead Torizo phase {phase} transfer {index} size");
                AssertEqual(record.SourceBankWord, ReadWord(address + 2),
                    $"Dead Torizo phase {phase} transfer {index} source bank");
                AssertEqual(record.SourceOffset, ReadWord(address + 4),
                    $"Dead Torizo phase {phase} transfer {index} source offset");
                AssertEqual(record.EncodedVramDestination, ReadWord(address + 6),
                    $"Dead Torizo phase {phase} transfer {index} VRAM destination");
            }
            AssertEqual((ushort)0, ReadWord(0xa90000 | unchecked((ushort)(
                table + records.Length *
                DeadTorizoVramTransferDefinitions.RecordByteCount))),
                $"Dead Torizo phase {phase} native zero terminator");
        }

        var nativeBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var installedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        RoomEnemySystem native = InitializeDeadTorizoArtwork(nativeBus, null);
        RoomEnemySystem installed = InitializeDeadTorizoArtwork(installedBus, stock);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            installed, new FrontendCartridgeReadGuard(installedBus));
        MethodInfo build = typeof(RoomEnemySystem)
            .GetMethod("BuildDeadTorizoVramTransfers", flags)!;
        Action<DeadTorizoEnemyState> buildNative =
            build.CreateDelegate<Action<DeadTorizoEnemyState>>(native);
        Action<DeadTorizoEnemyState> buildInstalled =
            build.CreateDelegate<Action<DeadTorizoEnemyState>>(installed);
        for (int phase = 0; phase < 2; phase++)
        {
            buildNative(native.DeadTorizo!);
            buildInstalled(installed.DeadTorizo!);
            AssertTrue(native.LastDeadTorizoVramTransfers.SequenceEqual(
                    installed.LastDeadTorizoVramTransfers),
                $"Dead Torizo phase {phase} installed queue matches live native descriptors");
            AssertEqual((phase + 1) * 7, installed.LastDeadTorizoVramTransfers.Count,
                $"Dead Torizo phase {phase} emits every authored transfer in order");
        }
        Console.WriteLine("  Dead Torizo VRAM queues: fourteen native descriptors and both real frame builders match with installed ROM reads forbidden.");

        ushort ReadWord(int address) => unchecked((ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
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
