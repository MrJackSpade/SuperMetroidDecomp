using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>Checks that the installed shared gold-Pirate palette matches native CGRAM and that edits persist without changing ninja placement.</summary>
    /// <param name="bus">Cartridge address space used for native palette and enemy-definition data.</param>
    /// <param name="stockDirectory">Directory containing the extracted stock enemy artwork files.</param>
    /// <param name="stock">Loaded stock catalog used for the initial palette comparison.</param>
    private static void VerifyInstalledNinjaSpacePiratePalette(ISnesAddressSpace bus,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        RoomEnemyDefinition shared = SuperMetroid.AssetExtraction.RoomEnemyDefinitionImporter.Load(bus,
            NinjaSpacePiratePaletteDefinitions.SharedGoldPirateDefinition);
        AssertEqual(NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource,
            shared.Bank << 16 | shared.PalettePointer,
            "Installed gold-Pirate sheet owns the native ninja target-palette source");

        // $B2:F5DE copies sixteen raw gold-Pirate colors into target palette seven.
        // Keep the oracle independent of the installed catalog and runtime callback.
        var nativeColors = new SnesCgram();
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            nativeColors.SetColor(color, (ushort)(color * 17));
        for (int color = 0; color < 16; color++)
        {
            int address = 0xb28727 + color * 2;
            nativeColors.SetColor(240 + color,
                (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8));
        }
        // Native initialization for x=$100, span=$80, facing left: midpoint=$C0;
        // 32 acceleration steps sum to $4200, so the adjusted right post is $102.
        const ushort nativeX = 0x0102;
        const ushort nativeInstruction = 0xf2da;
        (RoomEnemySlot stockSlot, SnesCgram stockColors) = Initialize(
            new NinjaPaletteReadGuard(bus), stock);
        AssertTrue(nativeColors.Colors.SequenceEqual(stockColors.Colors),
            "Installed ninja target palette preserves full native CGRAM including neighbors");
        AssertEqual(nativeX, stockSlot.XPosition,
            "Installing ninja palette does not change initial post placement");
        AssertEqual(nativeInstruction, stockSlot.CurrentInstruction,
            "Installing ninja palette does not change initial animation");

        string overrideDirectory = Path.Combine(stockDirectory, "ninja-palette-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string fileName = EnemyTileArtworkFormat.PaletteFileName(
            NinjaSpacePiratePaletteDefinitions.SharedGoldPirateDefinition);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        JsonNode edit = JsonNode.Parse(File.ReadAllText(Path.Combine(stockDirectory, fileName)))!;
        JsonNode red = edit["colors"]![4]!["red"]!;
        edit["colors"]![4]!["red"] = red.GetValue<int>() ^ 1;
        File.WriteAllText(overridePath, edit.ToJsonString());
        var selected = EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory);
        (RoomEnemySlot editedSlot, SnesCgram editedColors) = Initialize(
            new NinjaPaletteReadGuard(bus), selected);
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            AssertEqual((ushort)(nativeColors.Colors[color] ^
                (color == NinjaSpacePiratePaletteDefinitions.TargetColor + 4 ? 1 : 0)),
                editedColors.Colors[color],
                "Gold-Pirate color edit changes only its shared ninja target color");
        AssertEqual(nativeX, editedSlot.XPosition,
            "Edited ninja palette does not change physical post placement");

        byte[] savedOverride = File.ReadAllBytes(overridePath);
        EnemyTileArtworkFiles.Extract(bus, stockDirectory, SupportedCartridge.Sha256);
        AssertTrue(savedOverride.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Enemy stock re-extraction preserves the shared ninja color override");
        (RoomEnemySlot restoredSlot, SnesCgram restoredColors) = Initialize(
            new NinjaPaletteReadGuard(bus),
            EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory));
        AssertTrue(restoredColors.Colors.SequenceEqual(editedColors.Colors),
            "Reload after stock repair retains the selected ninja target colors");
        AssertEqual(editedSlot.XPosition, restoredSlot.XPosition,
            "Reload after stock repair retains ninja placement");
        Console.WriteLine("Ninja Pirate palette: shared gold-Pirate sheet matches native CGRAM; live edit, ROM guard, placement isolation and override persistence pass.");

        static (RoomEnemySlot Slot, SnesCgram Colors) Initialize(
            ISnesAddressSpace source, EnemyTileArtworkCatalog artwork)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            var colors = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                colors.SetColor(color, (ushort)(color * 17));
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, colors);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.GreyNinjaSpacePirateDefinition;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.XPosition = 0x0100;
            slot.YPosition = 0x0100;
            slot.Parameter2 = 0x0080;
            typeof(RoomEnemySystem).GetMethod("InitializeNinjaSpacePirate", flags)!
                .Invoke(enemies, [slot]);
            return (slot, colors);
        }
    }

    /// <summary>Prevents ninja palette initialization from reading its migrated color range or writing to the cartridge bus.</summary>
    /// <param name="source">Address space used to service permitted reads.</param>
    private sealed class NinjaPaletteReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Routes cartridge import reads through the palette source guard.</summary>
        /// <param name="address">SNES address requested by the importer.</param>
        /// <returns>The source byte when the address is outside the migrated palette range.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Rejects reads from the migrated shared gold-Pirate palette range and forwards other reads.</summary>
        /// <param name="address">SNES address to read.</param>
        /// <returns>The wrapped source byte for an allowed address.</returns>
        /// <exception cref="InvalidOperationException">The address is within the migrated palette color bytes.</exception>
        public byte ReadByte(int address)
        {
            if (address >= NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource &&
                address < NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource +
                EnemyPaletteSheet.ColorCount * sizeof(ushort))
                throw new InvalidOperationException("Installed ninja palette read its migrated ROM source.");
            return source.ReadByte(address);
        }

        /// <summary>Rejects writes because palette initialization is expected to update CGRAM without modifying the bus.</summary>
        /// <param name="address">SNES address that the caller attempted to write.</param>
        /// <param name="value">Byte that the caller attempted to store.</param>
        /// <exception cref="InvalidOperationException">A write is attempted through this read-only verification guard.</exception>
        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException("Ninja palette initialization wrote the bus.");
    }
}
