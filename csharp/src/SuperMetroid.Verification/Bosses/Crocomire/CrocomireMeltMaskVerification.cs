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
/// (#1652). The cartridge's erase (<see cref="CrocomireMeltDissolveConstants.Steps"/>) clears bits of the melt image's four-bit
/// characters; Core hides pixel columns of the picture's dissolving cells (<see cref="CrocomireMeltMask"/>). For every pass,
/// band and step, the pixels the mask leaves showing must be exactly the pixels the erased characters still ink.
/// </summary>
internal static class CrocomireMeltMaskVerification
{
    /// <summary>
    /// Fails unless each dissolving cell of each melt image's picture shows its character's pixels, and the mask at every
    /// band's every step shows exactly the pixels the cartridge's erase leaves inked.
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
            AssertTrue(!image.Dissolving.IsEmpty, $"Crocomire's {pass} melt image has cells its dissolve erases");
            byte[][] steps = [.. CrocomireMeltDissolveConstants.Steps(melt, pass)];
            AssertEqual(CrocomireMeltingDefinitions.ColumnCount + 1, steps.Length, $"Crocomire's {pass} dissolve leaves its image after every step");
            foreach (DissolvingCell cell in image.Dissolving)
            {
                for (int y = 0; y < HiddenPictureColumns.CellPixels; y++)
                {
                    for (int x = 0; x < HiddenPictureColumns.CellPixels; x++)
                    {
                        bool inked = Ink(steps[0], Character(cell), x, y) != 0;
                        bool opaque = Pixel(picture, (cell.Column * HiddenPictureColumns.CellPixels) + x, (cell.Row * HiddenPictureColumns.CellPixels) + y).A != 0;
                        if (inked != opaque && failures.Count < 20)
                            failures.Add($"{pass} cell ({cell.Column}, {cell.Row}) pixel ({x}, {y}): the picture is {(opaque ? "opaque" : "clear")} where band {cell.Band} character {cell.Group} {(inked ? "inks" : "inks nothing")}");
                    }
                }
            }
            var bandSteps = new int[CrocomireMeltingDefinitions.SecondPassBandCount];
            for (int band = 0; band < CrocomireMelting.BandCount(pass); band++)
            {
                for (int step = 0; step < steps.Length; step++)
                {
                    Array.Clear(bandSteps);
                    bandSteps[band] = step;
                    HiddenPictureColumns hidden = CrocomireMeltMask.Of(image, bandSteps);
                    foreach (DissolvingCell cell in image.Dissolving.Where(cell => cell.Band == band))
                    {
                        for (int y = 0; y < HiddenPictureColumns.CellPixels; y++)
                        {
                            for (int x = 0; x < HiddenPictureColumns.CellPixels; x++)
                            {
                                bool before = Ink(steps[0], Character(cell), x, y) != 0;
                                bool after = Ink(steps[step], Character(cell), x, y) != 0;
                                bool shown = before && !hidden.Hides(Position16<ImagePixels>.Of((cell.Column * HiddenPictureColumns.CellPixels) + x, (cell.Row * HiddenPictureColumns.CellPixels) + y));
                                compared++;
                                if (shown != after && failures.Count < 20)
                                    failures.Add($"{pass} band {band} step {step} cell ({cell.Column}, {cell.Row}) pixel ({x}, {y}): the mask {(shown ? "shows" : "hides")} it, the cartridge's erase leaves it {(after ? "inked" : "clear")}");
                            }
                        }
                    }
                }
            }
        }
        AssertTrue(failures.Count == 0, $"Crocomire's melt mask disagrees with the cartridge's dissolve:{Environment.NewLine}{string.Join(Environment.NewLine, failures)}");
        Console.WriteLine($"Crocomire melt: each melt image is pictured once; {compared} pixels over every pass, band and step show as the cartridge's erase leaves them.");
    }

    /// <summary>The melt image's character a dissolving cell shows: its band's row of characters, at its group.</summary>
    private static int Character(DissolvingCell cell) => (cell.Band * CrocomireMeltDissolve.BandRowCharacters) + cell.Group;

    /// <summary>The colour index character <paramref name="character"/> of four-bit <paramref name="image"/> inks at its pixel (<paramref name="x"/>, <paramref name="y"/>).</summary>
    private static int Ink(byte[] image, int character, int x, int y)
    {
        int row = (character * 32) + (2 * y), bit = 7 - x;
        return ((image[row] >> bit) & 1) | (((image[row + 1] >> bit) & 1) << 1) | (((image[row + 16] >> bit) & 1) << 2) | (((image[row + 17] >> bit) & 1) << 3);
    }

    /// <summary>The pixel of <paramref name="picture"/> at (<paramref name="x"/>, <paramref name="y"/>) of its rectangle.</summary>
    private static RgbaPixel Pixel(SpriteSheetFrame picture, int x, int y) =>
        picture.Sheet.Pixels[((picture.Corner.Y + y) * picture.Sheet.Size.X) + picture.Corner.X + x];

    /// <summary>Runs the Crocomire melt mask check.</summary>
    [VerificationCommand("--crocomire-melt-mask")]
    internal static int CrocomireMeltMaskCommand()
    {
        Suite(nameof(VerifyMaskShowsTheCartridgeDissolve), VerifyMaskShowsTheCartridgeDissolve);
        return 0;
    }
}
