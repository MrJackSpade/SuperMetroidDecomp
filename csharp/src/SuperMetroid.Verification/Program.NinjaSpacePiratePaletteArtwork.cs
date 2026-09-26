using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledNinjaSpacePiratePalette(ISnesAddressSpace bus,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        RoomEnemyDefinition shared = RoomEnemySystem.ReadDefinition(bus,
            NinjaSpacePiratePaletteDefinitions.SharedGoldPirateDefinition);
        AssertEqual(NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource,
            shared.Bank << 16 | shared.PalettePointer,
            "Installed gold-Pirate sheet owns the native ninja target-palette source");

        (RoomEnemySlot nativeSlot, SnesCgram nativeColors) = Initialize(bus, null);
        (RoomEnemySlot stockSlot, SnesCgram stockColors) = Initialize(
            new NinjaPaletteReadGuard(bus), stock);
        AssertTrue(nativeColors.Colors.SequenceEqual(stockColors.Colors),
            "Installed ninja target palette preserves full native CGRAM including neighbors");
        AssertEqual(nativeSlot.XPosition, stockSlot.XPosition,
            "Installing ninja palette does not change initial post placement");
        AssertEqual(nativeSlot.CurrentInstruction, stockSlot.CurrentInstruction,
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
        AssertEqual(nativeSlot.XPosition, editedSlot.XPosition,
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
            ISnesAddressSpace source, EnemyTileArtworkCatalog? artwork)
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

    private sealed class NinjaPaletteReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address >= NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource &&
                address < NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource +
                EnemyPaletteSheet.ColorCount * sizeof(ushort))
                throw new InvalidOperationException("Installed ninja palette read its migrated ROM source.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) =>
            throw new InvalidOperationException("Ninja palette initialization wrote the bus.");
    }
}
