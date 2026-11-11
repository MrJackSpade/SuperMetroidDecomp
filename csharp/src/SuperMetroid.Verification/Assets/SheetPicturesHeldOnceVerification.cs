using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Holds every installed sprite sheet to the rule that a sheet holds each picture once (#1513): no two of a sheet's
/// rectangles hold the same pixels. Frames that show one picture name one rectangle, whatever origin each places it at; a
/// picture held twice is a copy the import composed again for another place or another character copy (the pause
/// reserve digits were held once per digit place). Two rectangles are one picture when their size and every pixel's colour,
/// alpha, recolour region and blending are the same (#1633), so they show alike under every shade.
/// </summary>
internal static class SheetPicturesHeldOnceVerification
{
    /// <summary>Fails, naming every pair of frames of one sheet whose distinct rectangles hold one picture.</summary>
    internal static void VerifySheetPicturesHeldOnce()
    {
        var findings = new List<string>();
        int pictures = 0;
        foreach (SpriteSheetId id in SpriteSheetDefinitions.Sheets)
        {
            SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(id);
            // One name per rectangle: frames naming one rectangle already share their picture.
            var rectangles = new Dictionary<PictureRectangle, string>();
            foreach ((string name, SpriteSheetFrame frame) in sheet.frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                if (!frame.IsEmpty)
                    rectangles.TryAdd(new PictureRectangle(frame.Corner, frame.Size), name);
            }
            var held = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach ((PictureRectangle rectangle, string name) in rectangles)
            {
                pictures++;
                string content = SheetPixelRules.Spelled(sheet.Image, rectangle.Corner, rectangle.Size);
                if (!held.TryAdd(content, name))
                    findings.Add($"{id} frames {held[content]} and {name} hold one picture at two places of the sheet");
            }
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} sprite sheet pictures are held more than once:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"  Sprite sheets: each of {pictures} pictures is held once.");
    }

    /// <summary>One rectangle of a sheet.</summary>
    /// <param name="Corner">Its top-left pixel.</param>
    /// <param name="Size">Its width and height.</param>
    private readonly record struct PictureRectangle(Position16<ImagePixels> Corner, Extent16<ImagePixels> Size);

    /// <summary>Runs the sprite sheets' held-once rule.</summary>
    [VerificationCommand("--sheet-pictures-held-once")]
    internal static int SheetPicturesHeldOnceCommand()
    {
        Suite(nameof(VerifySheetPicturesHeldOnce), VerifySheetPicturesHeldOnce);
        return 0;
    }
}
