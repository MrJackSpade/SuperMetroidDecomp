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
        Suite(nameof(VerifyDeadTorizoVramTransferDefinitions), () => VerifyDeadTorizoVramTransferDefinitions(stock));
        Suite(nameof(VerifyDeadTorizoStationaryVisual), () => VerifyDeadTorizoStationaryVisual(stock));
        // The corpse initializer copies discontinuous portions of the ordinary ED3F
        // enemy sheet to WRAM. Later rotting frames draw sand words from that same
        // sheet. Guard the entire source interval so neither path can accidentally
        // work by reading the cartridge despite a bound installed catalog.
        var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var installedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        ReferenceDeadTorizoCorpseGraphics(nativeBus);
        RoomEnemySystem installed = InitializeDeadTorizoArtwork(installedBus, stock);
        AssertDeadTorizoBufferParity(nativeBus, installedBus, 0x7e2000, 0x1000,
            "installed dead-Torizo initial corpse sheet matches cartridge WRAM");

        ReferenceDeadTorizoSandLine(nativeBus, 0);
        CopyDeadTorizoSandLine(installed, 0);
        AssertDeadTorizoBufferParity(nativeBus, installedBus, 0x7e9500, 0x200,
            "installed dead-Torizo sand line matches cartridge WRAM");

        var nativeCorpse = new OamBuffer();
        // $A9:D39A draws the fixed corpse map at world (296, 187).
        DrawImportedEnemySpritemap(nativeBus, nativeCorpse, 0xa9, 0xd761, 296, 187, 0, 0);
        OamBuffer installedCorpse = DrawDeadTorizoCorpseFrame(stock, installedBus, 0, 0);
        AssertTrue(nativeCorpse.LowTable.SequenceEqual(installedCorpse.LowTable) &&
                   nativeCorpse.HighTable.SequenceEqual(installedCorpse.HighTable) &&
                   nativeCorpse.NextByteOffset == installedCorpse.NextByteOffset,
            "Dead Torizo private corpse hook draws installed native-parity OAM without ROM reads");
        AssertEqual(0, DrawDeadTorizoCorpseFrame(stock, installedBus, 0, 188).NextByteOffset,
            "Dead Torizo private hook retains its above-screen cull");

        var missing = EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>());
        AssertThrows<InvalidDataException>(() => InitializeDeadTorizoArtwork(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"), missing),
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
        var editedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
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
        var reloadedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        InitializeDeadTorizoArtwork(reloadedBus,
            EnemyTileArtworkFiles.Load(directory, overrideDirectory));
        AssertEqual(editedBus.ReadByte(0x7e2060), reloadedBus.ReadByte(0x7e2060),
            "dead-Torizo PNG edit survives catalog reload");

        Console.WriteLine("  Dead Torizo artwork: guarded initial corpse and sand-line ROM parity, visible PNG edit and reload pass.");
    }

    private static void VerifyDeadTorizoStationaryVisual(
        EnemyTileArtworkCatalog stock)
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
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
                out EnemySpritemapParts installedParts),
            "Dead Torizo stationary OAM has editable installed parts");
        var nativeOam = new OamBuffer();
        var installedOam = new OamBuffer();
        DrawImportedEnemySpritemap(rom, nativeOam, DeadTorizoArtworkDefinitions.SpritemapBank,
            nativePointer, 128, 128, 0, 0);
        installedOam.AddEnemySpritemap(installedParts, 128, 128, 0, 0);
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
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        for (ushort phase = 0; phase < 2; phase++)
        {
            ushort table = phase == 0
                ? DeadTorizoVramTransferDefinitions.EvenTable
                : DeadTorizoVramTransferDefinitions.OddTable;
            DeadTorizoVramTransferDefinitions.PhaseRows records =
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

        var installedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        RoomEnemySystem installed = InitializeDeadTorizoArtwork(installedBus, stock);
        var nativeTransfers = new List<VramWriteEntry>();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(
            installed, new FrontendCartridgeReadGuard(installedBus));
        MethodInfo build = typeof(RoomEnemySystem)
            .GetMethod("BuildDeadTorizoVramTransfers", flags)!;

        Action<DeadTorizoEnemyState> buildInstalled =
            build.CreateDelegate<Action<DeadTorizoEnemyState>>(installed);
        for (int phase = 0; phase < 2; phase++)
        {
            // $A9:D4CF increments the phase before selecting the raw descriptor table.
            int address = phase == 0 ? 0xa9d583 : 0xa9d549;
            for (int index = 0; index < 7; index++, address += 8)
                nativeTransfers.Add(new VramWriteEntry(ReadWord(address),
                    ((ReadWord(address + 2) & 0xff00) << 8) | ReadWord(address + 4),
                    ReadWord(address + 6)));
            AssertEqual((ushort)0, ReadWord(address), "native corpse queue terminates after seven entries");
            buildInstalled(installed.DeadTorizo!);
            AssertTrue(nativeTransfers.SequenceEqual(
                    installed.LastDeadTorizoVramTransfers),
                $"Dead Torizo phase {phase} installed queue matches live native descriptors");
            AssertEqual((phase + 1) * 7, installed.LastDeadTorizoVramTransfers.Count,
                $"Dead Torizo phase {phase} emits every authored transfer in order");
        }
        Console.WriteLine("  Dead Torizo VRAM queues: fourteen native descriptors and both real frame builders match with installed ROM reads forbidden.");

        ushort ReadWord(int address) => unchecked((ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));
    }

    // Independent transcription of Torizo_CorpseRottingInitFunc ($A9:DE18)
    // from the pinned sm_a9.c. Do not derive expected copies from runtime definitions.
    private static void ReferenceDeadTorizoCorpseGraphics(SuperMetroidAddressSpace bus)
    {
        for (int offset = 0; offset < 0x1000; offset++)
            bus.WriteByte(0x7e2000 + offset, 0);
        (int Source, int Destination, int Length)[] copies =
        [
            (288, 0x060, 0xc0), (800, 0x1a0, 0xc0),
            (1280, 0x2c0, 0x100), (1792, 0x400, 0x100),
            (2304, 0x540, 0x100), (2816, 0x680, 0x100),
            (3328, 0x7c0, 0x100), (3840, 0x900, 0x100),
            (4352, 0xa40, 0x100), (4832, 0xb60, 0x120),
            (5312, 0xc80, 0x140), (5824, 0xdc0, 0x140),
        ];
        foreach (var copy in copies)
            for (int offset = 0; offset < copy.Length; offset++)
                bus.WriteByte(0x7e2000 + copy.Destination + offset,
                    bus.ReadByte(0xb7a800 + copy.Source + offset));
    }

    // $A9:D5EA copies eighteen words selected by the two cartridge offset tables.
    private static void ReferenceDeadTorizoSandLine(SuperMetroidAddressSpace bus, ushort line)
    {
        int destination = ReadWord(0xa9d67c + line * 2);
        int source = ReadWord(0xa9d69c + line * 2);
        for (int row = 0; row < 18; row++)
            for (int part = 0; part < 2; part++)
                bus.WriteByte(0x7e9500 + destination + row * 16 + part,
                    bus.ReadByte(0xb7a800 + source + row * 16 + part));

        ushort ReadWord(int address) =>
            (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
    }

    private static RoomEnemySystem InitializeDeadTorizoArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog artwork)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        ISnesAddressSpace source = new DeadTorizoArtworkReadGuard(bus);
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
        EnemyTileArtworkCatalog artwork, SuperMetroidAddressSpace bus,
        ushort cameraX, ushort cameraY)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        RoomEnemySystem enemies = InitializeDeadTorizoArtwork(bus, artwork);
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
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public byte ReadByte(int address)
        {
            RejectArtworkRead(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectArtworkRead(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Dead Torizo artwork guard requires a cartridge import source."))
                .ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Dead Torizo artwork guard requires WRAM."))
            .ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Dead Torizo artwork guard requires SRAM."))
            .ReadSaveRamByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static void RejectArtworkRead(int address)
        {
            if (address is >= 0xb7a800 and < 0xb7c000)
                throw new InvalidOperationException(
                    $"Dead Torizo read installed artwork from ROM at ${address:X6}.");
        }
    }

    private sealed class DeadTorizoOamReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public byte ReadByte(int address)
        {
            RejectOamRead(address);
            return source.ReadByte(address);
        }

        public byte ReadCartridgeByte(int address)
        {
            RejectOamRead(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Dead Torizo OAM guard requires a cartridge import source."))
                .ReadCartridgeByte(address);
        }

        public byte ReadWorkRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Dead Torizo OAM guard requires WRAM."))
            .ReadWorkRamByte(address);

        public byte ReadSaveRamByte(int address) =>
            (source as ISnesMutableMemory ?? throw new InvalidOperationException(
                "Dead Torizo OAM guard requires SRAM."))
            .ReadSaveRamByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);

        private static void RejectOamRead(int address)
        {
            if (address is >= 0xa9d761 and < 0xa9d77c)
                throw new InvalidOperationException(
                    $"Dead Torizo drew native OAM from ROM at ${address:X6}.");
        }
    }
}
