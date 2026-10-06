using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledDeadTourianCorpseArtwork(
        string directory, EnemyTileArtworkCatalog stock)
    {
        VerifyDeadTourianCorpseVisuals(stock);
        // Compare the installed initialization callbacks with independent native
        // copy layouts. The installed path forbids reads from the visual source bank.
        foreach (ushort parameter in new ushort[] { 0, 2 })
        {
            var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
            var installedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
            ReferenceDeadTourianCorpseGraphics(nativeBus, RoomEnemySystem.DeadSidehopperDefinition, parameter / 2);
            InitializeDeadSidehopperArtwork(installedBus, stock, parameter);
            for (int offset = 0; offset < 0x1000; offset++)
                AssertEqual(nativeBus.ReadByte(0x7e2000 + offset),
                    installedBus.ReadByte(0x7e2000 + offset),
                    $"dead-sidehopper parameter {parameter} installed corpse WRAM parity");
        }

        foreach ((ushort definition, int variantCount) in new (ushort, int)[]
        {
            (RoomEnemySystem.DeadZoomerDefinition, 3),
            (RoomEnemySystem.DeadRipperDefinition, 2),
            (RoomEnemySystem.DeadSkreeDefinition, 3),
        })
        {
            for (int variant = 0; variant < variantCount; variant++)
            {
                var nativeBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
                var installedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
                ReferenceDeadTourianCorpseGraphics(nativeBus, definition, variant);
                InitializeDeadTourianCorpseArtwork(installedBus, stock, definition, variant);
                for (int offset = 0; offset < 0x1000; offset++)
                    AssertEqual(nativeBus.ReadByte(0x7e2000 + offset),
                        installedBus.ReadByte(0x7e2000 + offset),
                        $"dead ${definition:X4} variant {variant} installed corpse WRAM parity");
            }
        }

        var missing = EnemyTileArtworkCatalog.FromArtworkForVerification(
            new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>());
        AssertThrows<InvalidDataException>(() => InitializeDeadSidehopperArtwork(
                SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc"), missing, 0),
            "bound dead-sidehopper art cannot silently fall back to ROM");

        string fileName = EnemyTileArtworkFormat.FileName(RoomEnemySystem.DeadSidehopperDefinition);
        using var source = new MemoryStream(File.ReadAllBytes(Path.Combine(directory, fileName)));
        int tileCount = RoomCharacterAtlasFormat.ValidateTileCount(0x0e00);
        int columns = Math.Min(RoomCharacterAtlasFormat.TileColumns, tileCount);
        int rows = (tileCount + columns - 1) / columns;
        IndexedPngImage image = IndexedPng.Read(source, columns * 8, rows * 8);
        // The first copy starts at source offset $0040, tile 2. Its first
        // pixel becomes the high bit at WRAM $7E:2040.
        image.Pixels[2 * 8] ^= 1;
        string overrideDirectory = Path.Combine(directory, "dead-sidehopper-overrides");
        Directory.CreateDirectory(overrideDirectory);
        using (var output = File.Create(Path.Combine(overrideDirectory, fileName)))
            IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
        var stockBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        var editedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        InitializeDeadSidehopperArtwork(stockBus, stock, 0);
        InitializeDeadSidehopperArtwork(editedBus,
            EnemyTileArtworkFiles.Load(directory, overrideDirectory), 0);
        AssertEqual((byte)(stockBus.ReadByte(0x7e2040) ^ 0x80),
            editedBus.ReadByte(0x7e2040),
            "dead-sidehopper PNG pixel edit reaches live corpse staging WRAM");
        for (int offset = 0; offset < 0x1000; offset++)
        {
            if (offset == 0x40)
                continue;
            AssertEqual(stockBus.ReadByte(0x7e2000 + offset),
                editedBus.ReadByte(0x7e2000 + offset),
                "dead-sidehopper PNG edit leaves other corpse bytes unchanged");
        }
        var reloadedBus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        InitializeDeadSidehopperArtwork(reloadedBus,
            EnemyTileArtworkFiles.Load(directory, overrideDirectory), 0);
        AssertEqual(editedBus.ReadByte(0x7e2040), reloadedBus.ReadByte(0x7e2040),
            "dead-sidehopper PNG edit survives catalog reload");

        Console.WriteLine("  Tourian corpse artwork: ten guarded sidehopper/Zoomer/Ripper/Skree variants match cartridge WRAM; a PNG edit and reload reach the live buffer.");
    }

    private static void VerifyDeadTourianCorpseVisuals(
        EnemyTileArtworkCatalog stock)
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom("Super Metroid.smc");
        AssertEqual(DeadTourianCorpseVisualDefinitions.CorpseFrameCount,
            DeadTourianCorpseInstructionProgramDefinitions.ProgramCount,
            "every dead Tourian corpse program has an editable composition");
        for (int index = 0;
             index < DeadTourianCorpseInstructionProgramDefinitions.ProgramCount;
             index++)
        {
            ushort operand = DeadTourianCorpseInstructionProgramDefinitions
                .PresentationWordAddress(index);
            int selectorAddress = (DeadTourianCorpseVisualDefinitions.Bank << 16) | operand;
            ushort nativePointer = unchecked((ushort)(
                rom.ReadByte(selectorAddress) | rom.ReadByte(selectorAddress + 1) << 8));
            AssertEqual(nativePointer,
                DeadTourianCorpseVisualDefinitions.FrameAt(operand),
                $"dead Tourian corpse selector {index} matches cartridge");
            ushort definition = index < 3 ? RoomEnemySystem.DeadZoomerDefinition :
                index < 5 ? RoomEnemySystem.DeadRipperDefinition :
                RoomEnemySystem.DeadSkreeDefinition;
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    definition, operand, out ushort installedPointer),
                $"dead Tourian corpse program {index} has an installed selector");
            AssertEqual(nativePointer, installedPointer,
                $"dead Tourian corpse program {index} keeps its native identity");
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    DeadTourianCorpseVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"dead Tourian corpse program {index} has installed OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, DeadTourianCorpseVisualDefinitions.Bank,
                nativePointer, 128, 128, 0, 0);
            installedOam.AddEnemySpritemap(installedParts, 128, 128, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"dead Tourian corpse {index} installed OAM matches cartridge");
        }
        var sidehopperPointers = new HashSet<ushort>();
        for (int index = 0;
             index < DeadSidehopperInstructionProgramDefinitions.PresentationWordCount;
             index++)
        {
            ushort operand = DeadSidehopperInstructionProgramDefinitions
                .PresentationWordAddress(index);
            int selectorAddress = (DeadTourianCorpseVisualDefinitions.Bank << 16) | operand;
            ushort nativePointer = unchecked((ushort)(
                rom.ReadByte(selectorAddress) | rom.ReadByte(selectorAddress + 1) << 8));
            AssertEqual(nativePointer,
                DeadTourianCorpseVisualDefinitions.SidehopperFrameAt(operand),
                $"dead Sidehopper selector {index} matches cartridge");
            AssertTrue(EnemySpritemapDefinitions.TryFrameAt(
                    RoomEnemySystem.DeadSidehopperDefinition, operand,
                    out ushort installedPointer),
                $"dead Sidehopper selector {index} is installed");
            AssertEqual(nativePointer, installedPointer,
                $"dead Sidehopper selector {index} preserves identity");
            sidehopperPointers.Add(installedPointer);
            AssertTrue(stock.Spritemaps!.TryGetDisplay(
                    DeadTourianCorpseVisualDefinitions.Bank, installedPointer,
                    out EnemySpritemapParts installedParts),
                $"dead Sidehopper selector {index} has installed OAM");
            var nativeOam = new OamBuffer();
            var installedOam = new OamBuffer();
            DrawImportedEnemySpritemap(rom, nativeOam, DeadTourianCorpseVisualDefinitions.Bank,
                nativePointer, 128, 128, 0, 0);
            installedOam.AddEnemySpritemap(installedParts, 128, 128, 0, 0);
            AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                       nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                       nativeOam.NextByteOffset == installedOam.NextByteOffset,
                $"dead Sidehopper selector {index} installed OAM matches cartridge");
        }
        AssertEqual(DeadTourianCorpseVisualDefinitions.SidehopperFrameCount,
            sidehopperPointers.Count,
            "eleven Sidehopper selectors choose five distinct OAM compositions");
        AssertThrows<InvalidDataException>(
            () => DeadTourianCorpseVisualDefinitions.FrameAt(
                DeadTourianCorpseInstructionProgramDefinitions.Zoomer0),
            "dead Tourian corpse selector rejects adjacent mechanics");
        Console.WriteLine("  Dead Tourian corpses: eight Zoomer/Ripper/Skree and eleven Sidehopper selectors choose thirteen editable native-parity OAM frames.");
    }

    // Independent copy spans from the pinned sm_a9.c corpse initializers
    // $A9:DEC1-$E052. Keep these separate from the runtime's layout definitions.
    private static void ReferenceDeadTourianCorpseGraphics(
        SuperMetroidAddressSpace bus, ushort definition, int variant)
    {
        (int Source, int Destination, int Length)[] copies = (definition, variant) switch
        {
            (RoomEnemySystem.DeadSidehopperDefinition, 0) =>
                [(64, 0x040, 0x60), (512, 0x0a0, 0xa0), (1024, 0x140, 0xa0),
                 (1536, 0x1e0, 0xa0), (2048, 0x280, 0xa0)],
            (RoomEnemySystem.DeadSidehopperDefinition, 1) =>
                [(288, 0x320, 0x40), (800, 0x3c0, 0xa0), (1312, 0x460, 0xa0),
                 (1824, 0x500, 0xa0), (2336, 0x5a0, 0xa0)],
            (RoomEnemySystem.DeadZoomerDefinition, 0) => [(2656, 0x940, 0x60), (3168, 0x9a0, 0x60)],
            (RoomEnemySystem.DeadZoomerDefinition, 1) => [(2752, 0xa00, 0x60), (3264, 0xa60, 0x60)],
            (RoomEnemySystem.DeadZoomerDefinition, 2) => [(2848, 0xac0, 0x60), (3360, 0xb20, 0x60)],
            (RoomEnemySystem.DeadRipperDefinition, 0) => [(2560, 0xb80, 0x60), (3072, 0xbe0, 0x60)],
            (RoomEnemySystem.DeadRipperDefinition, 1) => [(2944, 0xc40, 0x60), (3456, 0xca0, 0x60)],
            (RoomEnemySystem.DeadSkreeDefinition, 0) =>
                [(672, 0x640, 0x40), (1184, 0x680, 0x40), (1696, 0x6c0, 0x40), (2208, 0x700, 0x40)],
            (RoomEnemySystem.DeadSkreeDefinition, 1) =>
                [(224, 0x740, 0x40), (736, 0x780, 0x40), (1248, 0x7c0, 0x40), (1760, 0x800, 0x40)],
            (RoomEnemySystem.DeadSkreeDefinition, 2) =>
                [(448, 0x840, 0x40), (960, 0x880, 0x40), (1472, 0x8c0, 0x40), (1984, 0x900, 0x40)],
            _ => throw new InvalidDataException($"Unknown native corpse fixture {definition:X4}/{variant}."),
        };
        foreach (var copy in copies)
            for (int offset = 0; offset < copy.Length; offset++)
                bus.WriteByte(0x7e2000 + copy.Destination + offset,
                    bus.ReadByte(0xb7c000 + copy.Source + offset));
    }

    private static void InitializeDeadSidehopperArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog artwork, ushort parameter)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        ISnesAddressSpace source = new DeadTourianCorpseArtworkReadGuard(bus);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.Parameter1 = parameter;
        typeof(RoomEnemySystem).GetMethod("InitializeDeadSidehopper", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies)(slot);
    }

    private static void InitializeDeadTourianCorpseArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog artwork,
        ushort definition, int variant)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        ISnesAddressSpace source = new DeadTourianCorpseArtworkReadGuard(bus);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = definition;
        slot.Parameter1 = checked((ushort)(variant * 2));
        typeof(RoomEnemySystem).GetMethod("InitializeDeadTourianCorpse", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies)(slot);
    }

    private sealed class DeadTourianCorpseArtworkReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace, IImportCartridgeSource
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address) =>
            address is >= 0xb7c000 and < 0xb7ce00
                ? throw new InvalidOperationException(
                    $"Tourian corpse read installed artwork from ROM at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
