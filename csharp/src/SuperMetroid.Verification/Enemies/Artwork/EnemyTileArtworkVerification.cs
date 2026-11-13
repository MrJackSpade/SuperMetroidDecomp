using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Areas.Ceres.CeresEscapeOverlayTilemapsVerification;
using static SuperMetroid.Verification.Areas.Ceres.Artwork.CeresEscapeTileArtworkVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBg2ScrollVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireBodyVisualsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.CrocomireMeltingDefinitionsVerification;
using static SuperMetroid.Verification.Bosses.Crocomire.Artwork.CrocomireSkeletonArtworkVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTorizoArtworkVerification;
using static SuperMetroid.Verification.Enemies.Corpses.DeadTourianCorpseArtworkVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyExtendedFrameArtworkVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemySpritemapArtworkVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkBabyMetroidCutsceneColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkBotwoonColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkCeresDoorVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkChozoAndTubeColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkCrocomireColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDachoraColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDraygonBg2FramesVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDraygonColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkGunshipLiftoffVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkKraidColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkKraidVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkMagdollitePaletteVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkMotherBrainDeathColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkNorfairRidleyColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkPhantoonBg2FramesVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkPhantoonColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkProjectileSpritemapsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkShitroidColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkSporeSpawnColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkTourianStatueColorsVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkWorkRobotPaletteVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkZebetiteColorsVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainCorpseArtworkVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainEscapeTextArtworkVerification;
using static SuperMetroid.Verification.Bosses.MotherBrain.MotherBrainSpecialSpriteArtworkVerification;
using static SuperMetroid.Verification.Enemies.SpacePirates.NinjaSpacePiratePaletteArtworkVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.FullRunSuitesVerification;
using static SuperMetroid.Verification.Bosses.Torizo.Artwork.TorizoSwapArtworkVerification;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;

using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Enemies.Artwork;

/// <summary>Coordinates retail tile and palette comparisons with installed enemy artwork and live consumers.</summary>
internal static class EnemyTileArtworkVerification
{
    /// <summary>Reads the exact tile upload bytes named by an enemy definition for the retail oracle.</summary>
    private static byte[] ReadNativeEnemyTiles(ISnesAddressSpace bus, EnemyDefinitionId definition, int byteCount) =>
        RomDataReader.ReadFixedBank(CartridgeImportSource.Require(bus), SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.TileDataSource(bus, definition), byteCount);

    /// <summary>
    /// The melt image the cartridge assembles for <paramref name="pass"/> from its retail graphics: each band's $0402-byte
    /// copy, $0200 bytes after the last, from its $0200-byte chunk of bank $A4, sized as the installed image.
    /// </summary>
    private static byte[] ReadNativeCrocomireMeltingPassImage(ISnesAddressSpace rom, CrocomireMeltingIllustration pass)
    {
        bool first = pass == CrocomireMeltingIllustration.First;
        var expected = new byte[first ? CrocomireMeltingArtworkFormat.FirstByteCount : CrocomireMeltingArtworkFormat.SecondByteCount];
        int firstChunk = first ? 0xa4a07d : 0xa4ac7d;
        for (int band = 0; band < (first ? 6 : 7); band++)
        {
            for (int index = 0; index < 0x402; index++)
                expected[band * 0x200 + index] = rom.ReadCartridgeByte(firstChunk + band * 0x200 + index);
        }
        return expected;
    }

    /// <summary>The repository installation's enemy artwork, which the enemy artwork suites read.</summary>
    private sealed class EnemyTileFixture
    {
        /// <summary>Retail cartridge oracle for native comparisons.</summary>
        private readonly CartridgeImportAddressSpace bus =
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        /// <summary>The installation's enemy-tile directory.</summary>
        private readonly string directory = RepositoryInstallation.Installation.EnemyTileDirectory;
        /// <summary>The extraction's enemy-tile import data, holding the colour documents only the import reads (#1637).</summary>
        private readonly string importData = CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.EnemyTileDirectoryName);
        /// <summary>The installation's enemy artwork.</summary>
        private readonly ImportEnemyArtwork stock = InstalledFixtures.EnemyArtwork;

        /// <summary>Supplies the cartridge, stock enemy-tile directory and stock catalog.</summary>
        internal void Deconstruct(out CartridgeImportAddressSpace bus, out string directory, out ImportEnemyArtwork stock)
        {
            bus = this.bus;
            directory = this.directory;
            stock = this.stock;
        }

        /// <summary>Supplies the cartridge, stock enemy-tile directory, its colour documents' import data and stock catalog.</summary>
        internal void Deconstruct(out CartridgeImportAddressSpace bus, out string directory, out string importData, out ImportEnemyArtwork stock)
        {
            (bus, directory, stock) = this;
            importData = this.importData;
        }
    }

    /// <summary>Checks installed Torizo, Ceres escape, Mother Brain, corpse, Ninja Pirate, Zebetite and Norfair Ridley artwork against retail.</summary>
    private static void VerifyBossEnemyArtwork()
    {
        var fixture = new EnemyTileFixture();
        var (bus, _, colours, stock) = fixture;
        Suite(nameof(VerifyTorizoSwapArtwork), () => VerifyTorizoSwapArtwork(bus, stock));
        Suite(nameof(VerifyCeresEscapeTileArtwork), () => VerifyCeresEscapeTileArtwork(bus, stock));
        Suite(nameof(VerifyCeresEscapeOverlayTilemaps), () => VerifyCeresEscapeOverlayTilemaps(bus, stock));
        Suite(nameof(VerifyInstalledMotherBrainCorpseArtwork), () => VerifyInstalledMotherBrainCorpseArtwork(stock));
        Suite(nameof(VerifyInstalledMotherBrainEscapeTextArtwork), () => VerifyInstalledMotherBrainEscapeTextArtwork(stock));
        Suite(nameof(VerifyInstalledMotherBrainSpecialSpriteArtwork), () => VerifyInstalledMotherBrainSpecialSpriteArtwork(stock));
        Suite(nameof(VerifyInstalledDeadTorizoArtwork), () => VerifyInstalledDeadTorizoArtwork(stock));
        Suite(nameof(VerifyInstalledDeadTourianCorpseArtwork), () => VerifyInstalledDeadTourianCorpseArtwork(stock));
        Suite(nameof(VerifyInstalledNinjaSpacePiratePalette), () => VerifyInstalledNinjaSpacePiratePalette(bus, stock));
        Suite(nameof(VerifyInstalledZebetiteColors), () => VerifyInstalledZebetiteColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledNorfairRidleyColors), () => VerifyInstalledNorfairRidleyColors(bus, colours, stock));
    }

    /// <summary>Checks installed enemy and boss color, palette-cycle and Crocomire skeleton artwork against retail.</summary>
    private static void VerifyEnemyColorArtwork()
    {
        var fixture = new EnemyTileFixture();
        var (bus, _, colours, stock) = fixture;
        Suite(nameof(VerifyInstalledTourianStatueColors), () => VerifyInstalledTourianStatueColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledMagdollitePaletteCycle), () => VerifyInstalledMagdollitePaletteCycle(bus, colours, stock));
        Suite(nameof(VerifyInstalledWorkRobotPaletteCycle), () => VerifyInstalledWorkRobotPaletteCycle(bus, colours, stock.Enemies));
        Suite(nameof(VerifyInstalledCrocomireColors), () => VerifyInstalledCrocomireColors(bus, colours, stock));
        Suite(nameof(VerifyCrocomireBg2ScrollDefinitions), () => VerifyCrocomireBg2ScrollDefinitions(bus));
        Suite(nameof(VerifyCrocomireSkeletonArtwork), () => VerifyCrocomireSkeletonArtwork(bus, stock));
        Suite(nameof(VerifyInstalledDraygonColors), () => VerifyInstalledDraygonColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledPhantoonColors), () => VerifyInstalledPhantoonColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledChozoAndTubeColors), () => VerifyInstalledChozoAndTubeColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledSporeSpawnColors), () => VerifyInstalledSporeSpawnColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledDachoraColors), () => VerifyInstalledDachoraColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledShitroidColors), () => VerifyInstalledShitroidColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledBabyMetroidCutsceneColors), () => VerifyInstalledBabyMetroidCutsceneColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledBotwoonColors), () => VerifyInstalledBotwoonColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledMotherBrainDeathColors), () => VerifyInstalledMotherBrainDeathColors(bus, colours, stock));
        Suite(nameof(VerifyInstalledCeresDoorVisuals), () => VerifyInstalledCeresDoorVisuals(bus, stock));
    }

    /// <summary>Checks installed enemy projectile spritemaps against retail.</summary>
    private static void VerifyEnemyProjectileFrameArtwork()
    {
        var (bus, _, stock) = new EnemyTileFixture();
        // The compositions are import data (#1638): their document is the cartridge-derived extraction's.
        string importData = CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.EnemyTileDirectoryName);
        Suite(nameof(VerifyInstalledEnemyProjectileSpritemaps), () => VerifyInstalledEnemyProjectileSpritemaps(bus, importData, stock));
    }

    /// <summary>Checks Kraid's installed background and colours against retail.</summary>
    private static void VerifyKraidFrameArtwork()
    {
        var (bus, _, colours, stock) = new EnemyTileFixture();
        Suite(nameof(VerifyInstalledKraidBackground), () => VerifyInstalledKraidBackground(bus, stock));
        Suite(nameof(VerifyInstalledKraidColors), () => VerifyInstalledKraidColors(bus, colours, stock));
    }

    /// <summary>Checks installed enemy spritemaps and extended frames against retail.</summary>
    private static void VerifyEnemySpritemapFrameArtwork()
    {
        var (bus, _, stock) = new EnemyTileFixture();
        Suite(nameof(VerifyInstalledEnemySpritemaps), () => VerifyInstalledEnemySpritemaps(bus, stock));
        Suite(nameof(VerifyInstalledEnemyExtendedFrames), () => VerifyInstalledEnemyExtendedFrames(bus, stock));
    }

    /// <summary>Checks Crocomire's, Phantoon's and Draygon's installed bodies against retail.</summary>
    private static void VerifyBossBodyFrameArtwork()
    {
        var (bus, _, stock) = new EnemyTileFixture();
        Suite(nameof(VerifyInstalledCrocomireBodyVisuals), () => VerifyInstalledCrocomireBodyVisuals(bus, stock));
        // The BG2 frames are import data (#1638): their documents are the cartridge-derived extraction's.
        string importData = CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.EnemyTileDirectoryName);
        Suite(nameof(VerifyInstalledPhantoonBg2Frames), () => VerifyInstalledPhantoonBg2Frames(bus, importData, stock));
        Suite(nameof(VerifyInstalledDraygonBg2Frames), () => VerifyInstalledDraygonBg2Frames(bus, importData, stock));
    }

    /// <summary>Checks the gunship's installed liftoff artwork against retail.</summary>
    private static void VerifyGunshipFrameArtwork()
    {
        var (bus, _, stock) = new EnemyTileFixture();
        Suite(nameof(VerifyInstalledGunshipLiftoffArtwork), () => VerifyInstalledGunshipLiftoffArtwork(bus, stock));
    }

    /// <summary>Runs tile, palette, BG and spritemap checks on the installed enemy art.</summary>
    internal static void VerifyEnemyTileArtwork()
    {
        string romPath = Path.GetFullPath("Super Metroid.smc");
        if (!File.Exists(romPath))
        {
            Console.WriteLine("  Enemy tile artwork: private ROM absent; retail extraction skipped.");
            return;
        }
        var bus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(romPath);
        // The enemy characters are import data no installation holds (#1636): the extraction's are read from the
        // cartridge-derived extraction and compared with the cartridge.
        string importData = CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.EnemyTileDirectoryName);
        {
            ImportEnemyArtwork stock = InstalledFixtures.EnemyArtwork;
            string[] files = Directory.GetFiles(importData, "enemy-????-tiles.png");
            AssertEqual(EnemyTileArtworkFormat.RetailDefinitionCount, files.Length,
                "one indexed page per distinct retail enemy graphics definition");
            _ = VerifyInstalledCrocomireMeltingTilemap(
                bus, stock, CrocomireMeltingIllustration.First,
                CrocomireBodyProgram.MeltingOneTopRow);
            _ = VerifyInstalledCrocomireMeltingTilemap(
                bus, stock, CrocomireMeltingIllustration.Second,
                CrocomireBodyProgram.MeltingTwoTopRow);
            foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
            {
                byte[] actual = VerifyInstalledCrocomireMeltingPass(stock, pass);
                byte[] expected = ReadNativeCrocomireMeltingPassImage(bus, pass);
                AssertTrue(actual.SequenceEqual(expected),
                    $"installed Crocomire {pass} melt image preserves the image the cartridge assembles");
            }
            foreach (string file in files)
            {
                EnemyDefinitionId pointer = EnemyDefinitionIds.FromHeaderPointer(
                    Convert.ToUInt16(Path.GetFileName(file).Substring(6, 4), 16));
                int byteCount = SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.TileSet(bus, pointer).ByteCount;
                byte[] native = ReadNativeEnemyTiles(bus, pointer, byteCount);
                AssertTrue(stock.Enemies.TilesOf(pointer).Span.SequenceEqual(native),
                    $"enemy {pointer}'s extracted tiles hold the cartridge tiles its header names");
                var vram = new SnesVram();
                stock.Enemies.LoadTo(pointer, byteCount, new VramRegion(vram, PpuMemoryLayout.Vram), 0);
                AssertTrue(vram.Bytes[..byteCount].SequenceEqual(native),
                    $"enemy {pointer} page preserves every native tile byte");
                // The installed palette the import colours the enemy's sheets in (#1512) is the cartridge's, colour for colour.
                ReadOnlySpan<RgbColor> installedColours = stock.Enemies.PaletteOf(pointer).Colours;
                int paletteAddress = SuperMetroid.AssetExtraction.Enemies.RoomEnemyDefinitionImporter.PaletteAddress(bus, pointer);
                for (int color = 0; color < EnemyPaletteSheetFile.ColorCount; color++)
                {
                    int word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(bus), paletteAddress + color * sizeof(ushort));
                    AssertEqual(new FadeLevelColour(word & 31, word >> 5 & 31, word >> 10 & 31).ToColour(), installedColours[color],
                        $"enemy {pointer} RGB5 JSON preserves native palette colour {color}");
                }
            }
        }
            Console.WriteLine("  Enemy artwork: 122 retail tile/color sheets, Crocomire melts, Kraid body/head maps, backdrop PNG, palette effects and four HUD-derived death restores, plus Boyon/Cacatac/Boulder/Atomic/Skultera/Waver/Skree/Metaree/Zoa/Pipe Bug/Fake Kraid/Kraid nail/Owtch/Stoke/Ripper/Fireflea/Magdollite/Ceres door/Ceres Baby/Rio/Lower Norfair Rio/Norfair Rio/Puyo/Bull/Alcoon/Beetom/Sidehopper/Dessgeega/Choot/HZoomer/Sbug/Fune/Namihe/Kamer platform/Elevator/Zeela/Sova/Zoomer/Stone Zoomer visual frames pass stock parity.");
            Console.WriteLine("  Sciser, shared Mellow/Mella/Memu, Kago, face-block, Morph Ball eye, shutter, ordinary Metroid, Shaktool, Tripper/Kamer, Dragon, Multiviola, Norfair lava-jumper, Chozo statue, Viola, and Rinka OAM frames: cartridge parity and installed draws pass.");
    }

    /// <summary>
    /// The installed enemy artwork family. Each suite extracts its own stock artwork, so the full
    /// run schedules them independently.
    /// </summary>
    internal static RegisteredSuite[] EnemyTileArtworkSuites() =>
    [
        new(nameof(VerifyEnemyTileArtwork), () => VerifyEnemyTileArtwork()),
        new(nameof(VerifyBossEnemyArtwork), () => VerifyBossEnemyArtwork()),
        new(nameof(VerifyEnemyColorArtwork), () => VerifyEnemyColorArtwork()),
        new(nameof(VerifyEnemyProjectileFrameArtwork), () => VerifyEnemyProjectileFrameArtwork()),
        new(nameof(VerifyKraidFrameArtwork), () => VerifyKraidFrameArtwork()),
        new(nameof(VerifyEnemySpritemapFrameArtwork), () => VerifyEnemySpritemapFrameArtwork()),
        new(nameof(VerifyBossBodyFrameArtwork), () => VerifyBossBodyFrameArtwork()),
        new(nameof(VerifyGunshipFrameArtwork), () => VerifyGunshipFrameArtwork()),
    ];

    /// <summary>Runs only the installed Zebetite pulse-colour comparison against retail.</summary>
    [VerificationCommand("--zebetite-colors")]
    internal static int ZebetiteColorsCommand()
    {
        var fixture = new EnemyTileFixture();
        var (bus, _, colours, stock) = fixture;
        VerifyInstalledZebetiteColors(bus, colours, stock);
        return 0;
    }

    /// <summary>Runs only the installed Phantoon colour comparison against retail.</summary>
    [VerificationCommand("--phantoon-colors")]
    internal static int PhantoonColorsCommand()
    {
        var fixture = new EnemyTileFixture();
        var (bus, _, colours, stock) = fixture;
        VerifyInstalledPhantoonColors(bus, colours, stock);
        return 0;
    }

    /// <summary>Runs only the installed Dachora palette-colour comparison against retail.</summary>
    [VerificationCommand("--dachora-colors")]
    internal static int DachoraColorsCommand()
    {
        var fixture = new EnemyTileFixture();
        var (bus, _, colours, stock) = fixture;
        VerifyInstalledDachoraColors(bus, colours, stock);
        return 0;
    }

}
