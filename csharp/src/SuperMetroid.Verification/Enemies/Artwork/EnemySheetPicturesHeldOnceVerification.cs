using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Assets;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds every installed enemy and spawned-object sheet to the rule that a sheet holds each picture once (#1513): two of
/// a sheet's rectangles never show the same picture, the same size and the same pixel at every place. Frames showing one
/// picture, whatever origin each is drawn from (Kraid's foot, which his walk moves by its origin, or a crawler's frames
/// that one pose shows at two places), name one rectangle; a sheet repeating a picture shows the same artwork over and
/// over. Each image's pictures are spelled by every pixel's colour, alpha, recolour region and blending (#1633), so
/// two rectangles alike there are alike under every shade too.
/// </summary>
internal static class EnemySheetPicturesHeldOnceVerification
{
    /// <summary>Rectangles named in one picture's failure before the rest are counted only.</summary>
    private const int NamedExamples = 6;

    /// <summary>Fails, naming each repeated picture and the frames that show it, when an installed sheet holds a picture twice.</summary>
    internal static void VerifyEnemySheetPicturesHeldOnce()
    {
        var sheets = new List<LabelledSheet>();
        sheets.AddRange(InstalledFixtures.EnemySheets.variants.OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => new LabelledSheet(entry.Key, entry.Value.Sheet)));
        sheets.AddRange(InstalledFixtures.EnemySheets.spawnedVariants.OrderBy(entry => entry.Key, StringComparer.Ordinal)
            .SelectMany(entry => entry.Value.Sheet.Sheets.Select(sheet => new LabelledSheet($"spawned {entry.Key}", sheet))));
        AssertTrue(sheets.Count > 0, "the installation holds enemy sheets");
        LabelledSheet[][] images = [.. sheets.GroupBy(sheet => sheet.Sheet.Image).Select(users => users.ToArray())];
        var findings = new ImagePictures[images.Length];
        Parallel.For(0, images.Length, index => findings[index] = Pictures(images[index]));
        string[] repeated = [.. findings.SelectMany(image => image.Repeated)];
        AssertTrue(repeated.Length == 0,
            $"{repeated.Length} pictures are held more than once on an installed enemy or spawned-object sheet:"
            + $"{Environment.NewLine}{string.Join(Environment.NewLine, repeated)}");
        Console.WriteLine($"Enemy sheet pictures held once: {findings.Sum(image => image.Pictures)} pictures of {images.Length} installed images, each held in one rectangle.");
    }

    /// <summary>The pictures of one image, which <paramref name="users"/> all show, and each picture it holds more than once.</summary>
    private static ImagePictures Pictures(LabelledSheet[] users)
    {
        SpriteImage image = users[0].Sheet.Image;
        string owner = users.Length == 1 ? users[0].Label : $"{users[0].Label} and {users.Length - 1} more";
        // Each rectangle the image's frames name, with the frames naming it.
        var rectangles = new Dictionary<SheetRectangle, SortedSet<string>>();
        foreach (SpriteSheet sheet in users.Select(user => user.Sheet).Distinct())
        {
            foreach ((string name, SpriteSheetFrame frame) in sheet.frames)
            {
                if (frame.IsEmpty)
                    continue;
                var rectangle = new SheetRectangle(frame.Corner, frame.Size);
                if (!rectangles.TryGetValue(rectangle, out SortedSet<string>? names))
                    rectangles.Add(rectangle, names = new SortedSet<string>(StringComparer.Ordinal));
                names.Add(name);
            }
        }
        // Each distinct picture, with the rectangles holding it.
        var holders = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach ((SheetRectangle rectangle, SortedSet<string> names) in rectangles
            .OrderBy(entry => entry.Key.Corner.Y).ThenBy(entry => entry.Key.Corner.X))
        {
            string picture = SheetPixelRules.Spelled(image, rectangle.Corner, rectangle.Size);
            if (!holders.TryGetValue(picture, out List<string>? held))
                holders.Add(picture, held = []);
            held.Add($"({rectangle.Corner.X}, {rectangle.Corner.Y}) {string.Join("/", names)}");
        }
        string[] repeated = [.. holders.Values.Where(held => held.Count > 1).Select(held =>
            $"{owner} ({image.Name}) holds one {held.Count}-times picture at {string.Join(", ", held.Take(NamedExamples))}"
            + (held.Count > NamedExamples ? ", ..." : ""))];
        return new ImagePictures(holders.Count, repeated);
    }

    /// <summary>A rectangle of a sheet image that frames name.</summary>
    /// <param name="Corner">Its top-left corner on the image.</param>
    /// <param name="Size">Its width and height.</param>
    private readonly record struct SheetRectangle(Position16<ImagePixels> Corner, Extent16<ImagePixels> Size);

    /// <summary>What one image holds.</summary>
    /// <param name="Pictures">How many distinct pictures it holds.</param>
    /// <param name="Repeated">Each picture it holds in more than one rectangle, with the frames naming each.</param>
    private sealed record ImagePictures(int Pictures, IReadOnlyList<string> Repeated);

    /// <summary>Runs the installed enemy sheets' one-rectangle-per-picture rule.</summary>
    [VerificationCommand("--enemy-sheet-pictures-held-once")]
    internal static int EnemySheetPicturesHeldOnceCommand()
    {
        Suite(nameof(VerifyEnemySheetPicturesHeldOnce), VerifyEnemySheetPicturesHeldOnce);
        return 0;
    }
}
