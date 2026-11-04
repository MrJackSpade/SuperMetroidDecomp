using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Tooling;
using System.Text.Json.Nodes;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.World;

using SuperMetroid.Verification.Enemies.Artwork;
namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// Checks Ninja Space Pirate target colors sourced from shared Gold Pirate artwork (#1512): the gold wall shade the
/// import pre-colours the ninja's sheet in shows the gold wall Pirate's colours, which the cartridge copies into the
/// palette the divekick flashes in.
/// </summary>
internal static class NinjaSpacePiratePaletteArtworkVerification
{
    /// <summary>The gold wall Pirate colour the override edits.</summary>
    private const int EditedColour = 4;

    /// <summary>Compares the installed gold wall shade with the cartridge, checks an isolated colour edit, and verifies override persistence.</summary>
    internal static void VerifyInstalledNinjaSpacePiratePalette(ISnesAddressSpace bus,
        string stockDirectory, InstalledEnemyArtwork stock)
    {
        AssertEqual(NinjaSpacePiratePaletteDefinitions.SharedGoldPirateSource,
            SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.PaletteAddress(bus, EnemyDefinitionId.PirateGoldWall),
            "Installed gold-Pirate sheet owns the native ninja target-palette source");

        // $B2:F5DE copies sixteen raw gold-Pirate colors into target palette seven.
        // Keep the oracle independent of the installed catalog and runtime callback.
        var native = new ushort[16];
        for (int color = 0; color < native.Length; color++)
            native[color] = CartridgeImportSource.Require(bus).ReadCartridgeWord(0xb28727 + color * 2);
        // Native initialization for x=$100, span=$80, facing left: midpoint=$C0;
        // 32 acceleration steps sum to $4200, so the adjusted right post is $102.
        const ushort nativeX = 0x0102;
        (RoomEnemySlot stockSlot, AnimationProgramCursor<NinjaSpacePirateProgram> stockProgram) = Initialize(stock);
        SpriteSheetShadeDocument stockShade = GoldWallShade(stock);
        AssertTrue(stockShade.Inks.Length > 0, "The ninja's sheet installs its gold wall shade over the inks it paints");
        for (int index = 0; index < stockShade.Inks.Length; index++)
            AssertEqual(native[stockShade.Inks[index] % native.Length], stockShade.Colours[index],
                $"Installed ninja gold wall shade ink {stockShade.Inks[index]} shows the gold Pirate's native colour");
        AssertEqual(nativeX, stockSlot.Position.X.Whole,
            "Installing ninja palette does not change initial post placement");
        // Facing left, the ninja starts at the native initial list $B2:F2DA.
        AssertEqual(AnimationProgramCursor<NinjaSpacePirateProgram>.Start(NinjaSpacePirateProgram.InitialFacingLeft), stockProgram,
            "Installing ninja palette does not change initial animation");

        string overrideDirectory = Path.Combine(stockDirectory, "ninja-palette-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string fileName = EnemyTileArtworkFiles.PaletteFileName(
            EnemyDefinitionId.PirateGoldWall);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        JsonNode edit = RequiredJson.Parse(File.ReadAllText(Path.Combine(stockDirectory, fileName)));
        JsonNode red = edit.Required("colors").Required(4).Required("red");
        edit.Required("colors").Required(4)["red"] = red.GetValue<int>() ^ 1;
        File.WriteAllText(overridePath, edit.ToJsonString());
        InstalledEnemyArtwork selected = EnemyArtworkEdits.WithSheets(stock, EnemyTileArtworkFiles.Open(stockDirectory, overrideDirectory));
        (RoomEnemySlot editedSlot, _) = Initialize(selected);
        SpriteSheetShadeDocument editedShade = GoldWallShade(selected);
        AssertTrue(editedShade.Inks.SequenceEqual(stockShade.Inks), "Gold-Pirate color edit keeps the shade's inks");
        for (int index = 0; index < editedShade.Inks.Length; index++)
            AssertEqual((ushort)(native[editedShade.Inks[index] % native.Length] ^ (editedShade.Inks[index] % native.Length == EditedColour ? 1 : 0)),
                editedShade.Colours[index], "Gold-Pirate color edit changes only its shared ninja target color");
        AssertEqual(nativeX, editedSlot.Position.X.Whole,
            "Edited ninja palette does not change physical post placement");

        byte[] savedOverride = File.ReadAllBytes(overridePath);
        EnemyTileStockReExtraction.ReExtractStock(bus, stockDirectory);
        AssertTrue(savedOverride.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Enemy stock re-extraction preserves the shared ninja color override");
        InstalledEnemyArtwork restored = EnemyArtworkEdits.WithSheets(stock, EnemyTileArtworkFiles.Open(stockDirectory, overrideDirectory));
        (RoomEnemySlot restoredSlot, _) = Initialize(restored);
        AssertTrue(GoldWallShade(restored).Colours.SequenceEqual(editedShade.Colours),
            "Reload after stock repair retains the selected ninja target colors");
        AssertEqual(editedSlot.Position.X.Whole, restoredSlot.Position.X.Whole,
            "Reload after stock repair retains ninja placement");
        Console.WriteLine("Ninja Pirate palette: the gold wall shade matches the native gold-Pirate colours; edit, placement isolation and override persistence pass.");

        // The gold wall shade the import pre-colours the first ninja its graphics sets place in, from artwork's enemy colours.
        static SpriteSheetShadeDocument GoldWallShade(InstalledEnemyArtwork artwork)
        {
            EnemySheetInputs inputs = EnemySheetFixtures.Inputs with { Enemies = artwork.Enemies };
            RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.DrawnUnder.First(candidate =>
                candidate.Records.ToArray().Any(record => IsNinja(record.DefinitionId)));
            EnemySheetSource source = EnemySheetComposer.Sources(inputs, set).Enemies.First(sheet => IsNinja(sheet.Enemy));
            ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(source.Sources, 0, 0);
            return (sheet.Manifest.Shades ?? throw new InvalidOperationException("The ninja's sheet installs its shades."))
                [NinjaSpacePirateShades.GoldWall.Key];
        }

        static bool IsNinja(EnemyDefinitionId enemy) => enemy is EnemyDefinitionId.PirateGoldNinja or EnemyDefinitionId.PirateGreenNinja
            or EnemyDefinitionId.PirateGreyNinja or EnemyDefinitionId.PirateMagentaNinja or EnemyDefinitionId.PirateRedNinja
            or EnemyDefinitionId.PirateSilverNinja;

        static InitializeResult Initialize(
            InstalledEnemyArtwork artwork)
        {
            var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = artwork.Enemies, BossArtwork = artwork.Bosses };
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.PirateGreyNinja, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            slot.SetSpawnParameter2(EnemyDefinitionId.PirateGreyNinja, 0x0080);
            NinjaSpacePirateBehavior.InitializeNinjaSpacePirate(slot);
            return new InitializeResult(slot, (enemies.NinjaSpacePirateStates[0]
                ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Program);
        }
    }

}
