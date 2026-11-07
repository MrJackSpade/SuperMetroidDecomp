using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Magdollite;
using SuperMetroid.Core.Enemies.Magdollite.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Magdollite;

/// <summary>
/// Checks the Magdollite glow shades the import paints on an installed Magdollite sheet (#1512): under each glow frame's
/// shade, the four glow inks of the Magdollite's row show the cartridge's colours for that frame, and every other ink of
/// the row keeps the sheet's standard colour.
/// </summary>
internal static class MagdolliteShadeContentVerification
{
    /// <summary>Compares each installed glow shade with the cartridge's glow colours.</summary>
    internal static void VerifyMagdolliteShadeContent()
    {
        ISnesAddressSpace rom = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        RoomEnemyGraphicsSetDefinition set = RoomEnemyGraphicsSetLists.All.First(candidate =>
            RoomEnemyGraphicsSetLists.MembersOf(candidate).Records.ToArray().Any(record => record.DefinitionId == EnemyDefinitionId.Magdollite));
        int row = new GraphicsSetRows(RoomEnemyGraphicsSetLists.MembersOf(set), EnemyGraphicsSetPaletteRowDefinitions.Rows).RowOf(EnemyDefinitionId.Magdollite);
        SpriteImage image = InstalledFixtures.EnemySheets.For(set, EnemyDefinitionId.Magdollite).Image;
        int first = row * LoadedSetColours.RowColours;
        for (int frame = 0; frame < MagdollitePaletteDefinitions.FrameCount; frame++)
        {
            RgbColor[] inks = PictureShading.Shaded(MagdolliteShades.Glow(frame)).InkColours(image);
            for (int color = 0; color < LoadedSetColours.RowColours; color++)
            {
                int glow = color - MagdollitePaletteCycle.FirstAnimatedColor;
                if (glow is >= 0 and < MagdollitePaletteCycle.AnimatedColorCount)
                {
                    int source = EnemySpeciesSourceDefinitions.Magdollite.CycleColors +
                        (frame * EnemySpeciesSourceDefinitions.Magdollite.RowColors + color) * EnemySpeciesSourceDefinitions.ColorWordBytes;
                    int word = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), source);
                    AssertEqual(new RgbColor(word & 31, word >> 5 & 31, word >> 10 & 31), inks[first + color],
                        $"{set.Name} Magdollite glow frame {frame} ink {color} shows the cartridge's colour");
                }
                else
                {
                    AssertEqual(image.StandardColours[first + color], inks[first + color],
                        $"{set.Name} Magdollite glow frame {frame} leaves ink {color} its standard colour");
                }
            }
        }
        Console.WriteLine($"Magdollite shade content: {MagdollitePaletteDefinitions.FrameCount} glow shades in {set.Name}'s row {row} match the cartridge.");
    }

    /// <summary>Runs the focused Magdollite shade-content comparison.</summary>
    [VerificationCommand("--magdollite-shade-content")]
    internal static int MagdolliteShadeContentCommand()
    {
        Suite(nameof(VerifyMagdolliteShadeContent), () => VerifyMagdolliteShadeContent());
        return 0;
    }
}
