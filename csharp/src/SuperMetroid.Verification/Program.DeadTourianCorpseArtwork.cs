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
        // The same ED7F sheet provides two distinct initial corpse layouts. Run
        // their real initialization callback against separate cartridge memories;
        // only the installed instance forbids reads from the visual source bank.
        foreach (ushort parameter in new ushort[] { 0, 2 })
        {
            var nativeBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
            var installedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
            InitializeDeadSidehopperArtwork(nativeBus, null, parameter);
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
                var nativeBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
                var installedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
                InitializeDeadTourianCorpseArtwork(nativeBus, null, definition, variant);
                InitializeDeadTourianCorpseArtwork(installedBus, stock, definition, variant);
                for (int offset = 0; offset < 0x1000; offset++)
                    AssertEqual(nativeBus.ReadByte(0x7e2000 + offset),
                        installedBus.ReadByte(0x7e2000 + offset),
                        $"dead ${definition:X4} variant {variant} installed corpse WRAM parity");
            }
        }

        var missing = new EnemyTileArtworkCatalog(
            new Dictionary<ushort, RoomCharacterAtlas>(),
            new Dictionary<ushort, EnemyPaletteSheet>());
        AssertThrows<InvalidDataException>(() => InitializeDeadSidehopperArtwork(
                SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"), missing, 0),
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
        var stockBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        var editedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
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
        var reloadedBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        InitializeDeadSidehopperArtwork(reloadedBus,
            EnemyTileArtworkFiles.Load(directory, overrideDirectory), 0);
        AssertEqual(editedBus.ReadByte(0x7e2040), reloadedBus.ReadByte(0x7e2040),
            "dead-sidehopper PNG edit survives catalog reload");

        Console.WriteLine("  Tourian corpse artwork: ten guarded sidehopper/Zoomer/Ripper/Skree variants match cartridge WRAM; a PNG edit and reload reach the live buffer.");
    }

    private static void InitializeDeadSidehopperArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog? artwork, ushort parameter)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        ISnesAddressSpace source = artwork is null
            ? bus
            : new DeadTourianCorpseArtworkReadGuard(bus);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.Parameter1 = parameter;
        typeof(RoomEnemySystem).GetMethod("InitializeDeadSidehopper", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies)(slot);
    }

    private static void InitializeDeadTourianCorpseArtwork(
        SuperMetroidAddressSpace bus, EnemyTileArtworkCatalog? artwork,
        ushort definition, int variant)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = artwork };
        ISnesAddressSpace source = artwork is null
            ? bus
            : new DeadTourianCorpseArtworkReadGuard(bus);
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = definition;
        slot.Parameter1 = checked((ushort)(variant * 2));
        typeof(RoomEnemySystem).GetMethod("InitializeDeadTourianCorpse", flags)!
            .CreateDelegate<Action<RoomEnemySlot>>(enemies)(slot);
    }

    private sealed class DeadTourianCorpseArtworkReadGuard(ISnesAddressSpace source) :
        ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address is >= 0xb7c000 and < 0xb7ce00
                ? throw new InvalidOperationException(
                    $"Tourian corpse read installed artwork from ROM at ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
