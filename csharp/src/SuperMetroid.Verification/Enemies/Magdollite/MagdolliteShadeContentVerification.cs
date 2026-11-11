using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>
/// Checks the Magdollite glow shades the import paints on an installed Magdollite sheet (#1512): under each glow frame's
/// shade, the four glow colours of the Magdollite's row show the cartridge's colours for that frame, and every other colour of
/// the row keeps the sheet's standard colour.
/// </summary>
internal static class MagdolliteShadeContentVerification
{
    /// <summary>Compares each installed glow shade with the cartridge's glow colours.</summary>
    internal static void VerifyMagdolliteShadeContent()
    {
        ISnesAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.DrawnUnder.First(candidate =>
            RoomEnemyGraphicsSetLists.MembersOf(candidate).Records.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.Magdollite));
        int row = new GraphicsSetRows(RoomEnemyGraphicsSetLists.MembersOf(set), EnemyGraphicsSetPaletteRowDefinitions.Rows).RowOf(EnemyDefinitionId.Magdollite);
        // The sheet composed in-process, which knows the palette index each pixel was composed as (#1633).
        SpriteImage image = EnemySheetFixtures.ComposedSheet(set, EnemyDefinitionId.Magdollite);
        int first = row * LoadedSetColours.RowColours;
        // The sheet's own colours, laid out by the palette index each was composed as (#1544); a colour of the row none of
        // its pixels shows is not one of its colours, and nothing shows it.
        RgbColor[] standard = InstalledPictureColoursConstants.ComposedColours(image);
        for (int frame = 0; frame < MagdolliteShades.GlowFrames; frame++)
        {
            RgbColor[] shown = InstalledPictureColoursConstants.ByComposedIndex(image,
                PictureShading.Shaded(MagdolliteShades.Glow(frame)).KeyColours(image));
            for (int color = 0; color < LoadedSetColours.RowColours; color++)
            {
                if (InstalledPictureColoursConstants.KeyComposedAs(image, first + color) is null)
                    continue;
                int glow = color - MagdollitePaletteCycle.FirstAnimatedColor;
                if (glow is >= 0 and < MagdollitePaletteCycle.AnimatedColorCount)
                {
                    int source = EnemySpeciesSourceDefinitions.Magdollite.CycleColors +
                        (frame * EnemySpeciesSourceDefinitions.Magdollite.RowColors + color) * EnemySpeciesSourceDefinitions.ColorWordBytes;
                    int word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source);
                    AssertEqual(new FadeLevelColour(word & 31, word >> 5 & 31, word >> 10 & 31).ToColour(), shown[first + color],
                        $"{set.Name} Magdollite glow frame {frame} colour {color} shows the cartridge's colour");
                }
                else
                {
                    AssertEqual(standard[first + color], shown[first + color],
                        $"{set.Name} Magdollite glow frame {frame} leaves colour {color} its standard colour");
                }
            }
        }
        Console.WriteLine($"Magdollite shade content: {MagdolliteShades.GlowFrames} glow shades in {set.Name}'s row {row} match the cartridge.");
    }

    /// <summary>Runs the focused Magdollite shade-content comparison.</summary>
    [VerificationCommand("--magdollite-shade-content")]
    internal static int MagdolliteShadeContentCommand()
    {
        Suite(nameof(VerifyMagdolliteShadeContent), () => VerifyMagdolliteShadeContent());
        return 0;
    }
}
