using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Crocomire.Artwork;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Bosses.Crocomire;

/// <summary>
/// Confirms Crocomire's melt image, installed once, shows every step of its dissolve as the cartridge's erase leaves it
/// (#1652, #2478). The cartridge's erase (<see cref="CrocomireMeltDissolveConstants.Steps"/>) clears bits of the melt image's
/// four-bit characters, which the cartridge's melt tilemap places on his body; Core hides the pixels the installed dissolve
/// map says each band's steps erase (<see cref="PictureDissolve"/>). For every pass, band and step, the pixels the picture
/// shows must be exactly the pixels the erased characters still ink where the tilemap places them.
/// </summary>
internal static class CrocomireMeltMaskVerification
{
    /// <summary>
    /// Fails unless each melt image's picture shows its band characters' pixels where the cartridge's tilemap places them, and
    /// the picture at every band's every step shows exactly the pixels the cartridge's erase leaves inked.
    /// </summary>
    internal static void VerifyMaskShowsTheCartridgeDissolve()
    {
        BossBodyArtworkFixture stock = BossBodySheetFixtures.Stock;
        CrocomireMeltingArtwork melt = stock.Frames.CrocomireMelting
            ?? throw new InvalidOperationException("Verification failed: the installation has no Crocomire melt images.");
        BossBodySheet sheet = stock.Sheets.Of(BossBodyOwner.Crocomire);
        var failures = new List<string>();
        int compared = 0;
        foreach (CrocomireMeltingIllustration pass in Enum.GetValues<CrocomireMeltingIllustration>())
        {
            BossBodyFrame image = sheet.Frame(CrocomireMeltingFrameNames.Of(pass));
            SpriteSheetFrame picture = image.Picture
                ?? throw new InvalidOperationException($"Verification failed: Crocomire's {pass} melt image shows no picture.");
            PictureDissolve dissolve = image.Dissolve
                ?? throw new InvalidOperationException($"Verification failed: Crocomire's {pass} melt image has no dissolve.");
            byte[][] steps = [.. CrocomireMeltDissolveConstants.Steps(melt, pass)];
            AssertEqual(CrocomireMeltingDefinitions.ColumnCount + 1, steps.Length, $"Crocomire's {pass} dissolve leaves its image after every step");
            ShownCharacter[] shown = [.. ShownCharacters(melt, pass)];
            AssertTrue(shown.Length != 0, $"Crocomire's {pass} melt tilemap places band characters on his body");
            foreach (ShownCharacter cell in shown)
            {
                for (int y = 0; y < CharacterPixels; y++)
                {
                    for (int x = 0; x < CharacterPixels; x++)
                    {
                        bool inked = cell.Ink(steps[0], x, y) != 0;
                        bool opaque = Pixel(picture, cell.Left + x, cell.Top + y).A != 0;
                        if (inked != opaque && failures.Count < 20)
                            failures.Add($"{pass} pixel ({cell.Left + x}, {cell.Top + y}): the picture is {(opaque ? "opaque" : "clear")} where band {cell.Band} character {cell.Character} {(inked ? "inks" : "inks nothing")}");
                    }
                }
            }
            var bandSteps = new int[CrocomireMeltingDefinitions.MeltImageBandCount];
            for (int band = 0; band < CrocomireMeltingDefinitions.MeltImageBandCount; band++)
            {
                for (int step = 0; step < steps.Length; step++)
                {
                    Array.Clear(bandSteps);
                    bandSteps[band] = step;
                    HiddenPicturePixels hidden = dissolve.Hidden(bandSteps);
                    foreach (ShownCharacter cell in shown.Where(cell => cell.Band == band))
                    {
                        for (int y = 0; y < CharacterPixels; y++)
                        {
                            for (int x = 0; x < CharacterPixels; x++)
                            {
                                bool before = cell.Ink(steps[0], x, y) != 0;
                                bool after = cell.Ink(steps[step], x, y) != 0;
                                bool showing = before && !hidden.Hides(Position16<ImagePixels>.Of(cell.Left + x, cell.Top + y));
                                compared++;
                                if (showing != after && failures.Count < 20)
                                    failures.Add($"{pass} band {band} step {step} pixel ({cell.Left + x}, {cell.Top + y}): the picture {(showing ? "shows" : "hides")} it, the cartridge's erase leaves it {(after ? "inked" : "clear")}");
                            }
                        }
                    }
                }
            }
        }
        AssertTrue(failures.Count == 0, $"Crocomire's melt dissolve disagrees with the cartridge's:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Crocomire melt: each melt image is pictured once; {compared} pixels over every pass, band and step show as the cartridge's erase leaves them.");
    }

    /// <summary>Pixels along a character's side.</summary>
    private const int CharacterPixels = 8;

    /// <summary>
    /// Each cell of <paramref name="pass"/>'s melt picture the cartridge's melt tilemap writes with a character of one of the
    /// pass's bands, where it lies in the picture.
    /// </summary>
    private static List<ShownCharacter> ShownCharacters(CrocomireMeltingArtwork melt, CrocomireMeltingIllustration pass)
    {
        var shown = new List<ShownCharacter>();
        BossBodyFrameSource source = BossBodyFrameSource.Written(CrocomireMeltingFrameNames.Of(pass), BossBodySourceDefinitions.CrocomireMeltFirstCell, melt.Tilemap(pass));
        Position16<BackgroundPixels> corner = BossBodyPainter.CornerOf(source.Cells);
        foreach ((int layerCell, BackgroundTile tile) in source.Cells)
        {
            int band = tile.Tile / CrocomireMeltDissolve.BandRowCharacters, group = tile.Tile % CrocomireMeltDissolve.BandRowCharacters;
            if (band >= CrocomireMeltingDefinitions.MeltImageBandCount || group >= CrocomireMeltDissolve.BandCharacterCount)
                continue;
            Position16<BackgroundPixels> at = BossBodyPainter.PixelOf(layerCell);
            shown.Add(new ShownCharacter(at.X - corner.X, at.Y - corner.Y, band, tile.Tile, tile.FlipHorizontally, tile.FlipVertically));
        }
        return shown;
    }

    /// <summary>The pixel of <paramref name="picture"/> at (<paramref name="x"/>, <paramref name="y"/>) of its rectangle.</summary>
    private static RgbaPixel Pixel(SpriteSheetFrame picture, int x, int y) =>
        picture.Sheet.Pixels[((picture.Corner.Y + y) * picture.Sheet.Size.X) + picture.Corner.X + x];

    /// <summary>One band character the melt tilemap places in the picture.</summary>
    /// <param name="Left">The picture column of the cell's left edge.</param>
    /// <param name="Top">The picture row of the cell's top edge.</param>
    /// <param name="Band">The band the character belongs to.</param>
    /// <param name="Character">The character the cell shows.</param>
    /// <param name="FlipHorizontally">Whether the cell shows the character mirrored left to right.</param>
    /// <param name="FlipVertically">Whether the cell shows the character upside down.</param>
    private readonly record struct ShownCharacter(int Left, int Top, int Band, int Character, bool FlipHorizontally, bool FlipVertically)
    {
        /// <summary>The colour index the cell shows at its pixel (<paramref name="x"/>, <paramref name="y"/>) from four-bit <paramref name="image"/>.</summary>
        internal int Ink(byte[] image, int x, int y)
        {
            int column = FlipHorizontally ? CharacterPixels - 1 - x : x, line = FlipVertically ? CharacterPixels - 1 - y : y;
            int row = (Character * 32) + (2 * line), bit = 7 - column;
            return ((image[row] >> bit) & 1) | (((image[row + 1] >> bit) & 1) << 1) | (((image[row + 16] >> bit) & 1) << 2) | (((image[row + 17] >> bit) & 1) << 3);
        }
    }

    /// <summary>Runs the Crocomire melt mask check.</summary>
    [VerificationCommand("--crocomire-melt-mask")]
    internal static int CrocomireMeltMaskCommand()
    {
        Suite(nameof(VerifyMaskShowsTheCartridgeDissolve), VerifyMaskShowsTheCartridgeDissolve);
        return 0;
    }
}
