using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.SpacePirates.Artwork;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.SpacePirates;

/// <summary>
/// Checks Ninja Space Pirate target colors sourced from shared Gold Pirate artwork (#1512): the gold wall shade the
/// import pre-colours the ninja's sheet in shows the gold wall Pirate's colours, which the cartridge copies into the
/// palette the divekick flashes in.
/// </summary>
internal static class NinjaSpacePiratePaletteArtworkVerification
{
    /// <summary>Compares the installed gold wall shade with the cartridge and checks the ninja's initial placement and animation.</summary>
    internal static void VerifyInstalledNinjaSpacePiratePalette(ISnesAddressSpace bus, ImportEnemyArtwork stock)
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
        (RoomEnemySlot stockSlot, AnimationPlayback<NinjaSpacePirateSegment> stockProgram) = Initialize(stock);
        SpriteSheetShadeDocument stockShade = GoldWallShade(stock);
        AssertTrue(stockShade.Inks.Length > 0, "The ninja's sheet installs its gold wall shade over the inks it paints");
        for (int index = 0; index < stockShade.Inks.Length; index++)
            AssertEqual(native[stockShade.Inks[index] % native.Length], stockShade.Colours[index],
                $"Installed ninja gold wall shade ink {stockShade.Inks[index]} shows the gold Pirate's native colour");
        AssertEqual(nativeX, stockSlot.Position.X.Whole,
            "Installing ninja palette does not change initial post placement");
        // Facing left, the ninja starts at the native initial list $B2:F2DA.
        AssertEqual(new AnimationPlayback<NinjaSpacePirateSegment>(NinjaSpacePirateAnimations.StartingWith(NinjaSpacePirateSegment.InitialFacingLeft), AnimationPlace.StartOfAnimation), stockProgram,
            "Installing ninja palette does not change initial animation");

        Console.WriteLine("Ninja Pirate palette: the gold wall shade matches the native gold-Pirate colours; placement and animation pass.");

        // The gold wall shade the import pre-colours the first ninja its graphics sets place in, from artwork's enemy colours.
        static SpriteSheetShadeDocument GoldWallShade(ImportEnemyArtwork artwork)
        {
            EnemySheetInputs inputs = EnemySheetFixtures.Inputs with { Enemies = artwork.Enemies };
            RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.DrawnUnder.First(candidate =>
                RoomEnemyGraphicsSetLists.MembersOf(candidate).Records.ToArray().Any(record => record.DefinitionId is { } member && IsNinja(member)));
            EnemySheetSource source = EnemySheetComposer.Sources(inputs, set).Enemies.First(sheet => IsNinja(sheet.Enemy));
            ComposedSpriteSheet sheet = SpriteSheetComposer.Compose(source.Sources, 0, 0);
            return (sheet.Manifest.Shades ?? throw new InvalidOperationException("The ninja's sheet installs its shades."))
                [NinjaSpacePirateShades.GoldWall.Key];
        }

        static bool IsNinja(EnemyDefinitionId enemy) => enemy is EnemyDefinitionId.PirateGoldNinja
            or EnemyDefinitionId.PirateSilverNinja;

        static InitializeResult Initialize(
            ImportEnemyArtwork artwork)
        {
            var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(artwork.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = artwork.Bosses };
            RoomEnemySlot slot = enemies.SlotTable.Slots[0];
            slot.Bind(EnemyDefinitionId.PirateSilverNinja, slot.Definition);
            slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
            slot.Position = slot.Position.WithWhole(new(0x0100, 0x0100));
            slot.SetSpawnParameter2(EnemyDefinitionId.PirateSilverNinja, 0x0080);
            NinjaSpacePirateBehavior.InitializeNinjaSpacePirate(slot);
            return new InitializeResult(slot, (enemies.NinjaSpacePirateStates[0]
                ?? throw new InvalidOperationException("Verification requires the initialized ninja state.")).Playback);
        }
    }
}
