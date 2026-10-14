using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>Checks Ninja Space Pirate target colors sourced from shared Gold Pirate artwork.</summary>
internal static class NinjaSpacePiratePaletteArtworkVerification
{
    /// <summary>Compares native CGRAM, checks an isolated live color edit, and verifies guard and override persistence.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for Ninja Space Pirate palette")]
    internal static void VerifyInstalledNinjaSpacePiratePalette(ISnesAddressSpace bus,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        RoomEnemyDefinition shared = SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.Load(bus,
            EnemyDefinitionId.PirateGoldWall);
        AssertEqual(NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource,
            ((int)new SnesAddress(            shared.Bank, shared.PalettePointer)),
            "Installed gold-Pirate sheet owns the native ninja target-palette source");

        // $B2:F5DE copies sixteen raw gold-Pirate colors into target palette seven.
        // Keep the oracle independent of the installed catalog and runtime callback.
        var nativeColors = new SnesCgram();
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            nativeColors.SetColor(color, Bgr555.FromWord((ushort)(color * 17)));
        for (int color = 0; color < 16; color++)
        {
            int address = 0xb28727 + color * 2;
            nativeColors.SetColor(240 + color,
                Bgr555.FromWord(CartridgeImportSource.Require(bus).ReadCartridgeWord(address)));
        }
        // Native initialization for x=$100, span=$80, facing left: midpoint=$C0;
        // 32 acceleration steps sum to $4200, so the adjusted right post is $102.
        const ushort nativeX = 0x0102;
        const ushort nativeInstruction = 0xf2da;
        (RoomEnemySlot stockSlot, SnesCgram stockColors) = Initialize(
            NewNinjaPaletteReadGuard(bus), stock);
        AssertTrue(new CgramPalettes(nativeColors).ImageEquals(new CgramPalettes(stockColors)),
            "Installed ninja target palette preserves full native CGRAM including neighbors");
        AssertEqual(nativeX, stockSlot.Position.X.Whole,
            "Installing ninja palette does not change initial post placement");
        AssertEqual(nativeInstruction, stockSlot.CurrentInstruction,
            "Installing ninja palette does not change initial animation");

        string overrideDirectory = Path.Combine(stockDirectory, "ninja-palette-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string fileName = EnemyTileArtworkFormat.PaletteFileName(
            EnemyDefinitionId.PirateGoldWall);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        JsonNode edit = RequiredJson.Parse(File.ReadAllText(Path.Combine(stockDirectory, fileName)));
        JsonNode red = edit.Required("colors").Required(4).Required("red");
        edit.Required("colors").Required(4)["red"] = red.GetValue<int>() ^ 1;
        File.WriteAllText(overridePath, edit.ToJsonString());
        var selected = EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory);
        (RoomEnemySlot editedSlot, SnesCgram editedColors) = Initialize(
            NewNinjaPaletteReadGuard(bus), selected);
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            AssertEqual((ushort)(nativeColors.Colors[color].ToWord() ^
                (color == NinjaSpacePiratePaletteDefinitions.TargetColor + 4 ? 1 : 0)),
                editedColors.Colors[color],
                "Gold-Pirate color edit changes only its shared ninja target color");
        AssertEqual(nativeX, editedSlot.Position.X.Whole,
            "Edited ninja palette does not change physical post placement");

        byte[] savedOverride = File.ReadAllBytes(overridePath);
        EnemyTileArtworkFiles.Extract(bus, stockDirectory, SupportedCartridge.Sha256);
        AssertTrue(savedOverride.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Enemy stock re-extraction preserves the shared ninja color override");
        (RoomEnemySlot restoredSlot, SnesCgram restoredColors) = Initialize(
            NewNinjaPaletteReadGuard(bus),
            EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory));
        AssertTrue(new CgramPalettes(restoredColors).ImageEquals(new CgramPalettes(editedColors)),
            "Reload after stock repair retains the selected ninja target colors");
        AssertEqual(editedSlot.Position.X.Whole, restoredSlot.Position.X.Whole,
            "Reload after stock repair retains ninja placement");
        Console.WriteLine("Ninja Pirate palette: shared gold-Pirate sheet matches native CGRAM; live edit, ROM guard, placement isolation and override persistence pass.");

        static (RoomEnemySlot Slot, SnesCgram Colors) Initialize(
            ISnesAddressSpace source, EnemyTileArtworkCatalog artwork)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            var colors = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                colors.SetColor(color, Bgr555.FromWord((ushort)(color * 17)));
            enemies.BindFixtureBus(source);
            enemies.Presentation._cgram = colors;
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.EnemyDefinitionPointer = EnemyDefinitionId.PirateGreyNinja;
            slot.Definition = default(RoomEnemyDefinition) with { Bank = 0xb2 };
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            slot.Parameter2 = 0x0080;
            enemies.NinjaSpacePirateBehavior.InitializeNinjaSpacePirate(slot);
            return (slot, colors);
        }
    }

    /// <summary>Rejects migrated palette ROM reads and writes during ninja initialization.</summary>
    private static CartridgeReadGuard NewNinjaPaletteReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .ForbidRange(NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource,
                EnemyPaletteSheet.ColorCount * sizeof(ushort),
                _ => "Installed ninja palette read its migrated ROM source.")
            .RejectWrites(_ => "Ninja palette initialization wrote the bus.");
}
