using System.Security.Cryptography;
using System.Text;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>
/// Holds the installed spawned-object sheets to the rule that common art is held once (#1513, owner flags 20 and 28): no
/// two installed sheets hold the same frame in the same colouring with identical pixels. A common sprite, such as an
/// explosion, a drop or a projectile, is pictured once in each colouring it is drawn in, and every graphics set that draws
/// it reads that one picture.
/// <para>
/// A frame's picture is compared by everything a draw of it shows: its name, rectangle size, origin, depth and whether it
/// blends, each pixel's colour, alpha and blending, and its colouring (#1544, #1633). Its colouring is the colour keys the
/// picture's pixels hold, in the order the picture first shows them, each pixel naming its key by that order, and what
/// each shade shows of them: a sheet's shades also cover its other pictures, and those say nothing about this one. A shade
/// counts when it recolours one of those keys or blends the whole draw.
/// </para>
/// Frames are fingerprinted in parallel across sheets.
/// </summary>
internal static class SpawnedArtHeldOnceVerification
{
    /// <summary>Fails, naming every frame two or more installed spawned-object sheets hold alike, with the sheets holding it.</summary>
    internal static void VerifySpawnedArtHeldOnce()
    {
        SpriteSheet[] sheets = [.. InstalledFixtures.EnemySheets.spawnedVariants.Values.Select(variant => variant.Sheet).SelectMany(SheetsOf)
            .Distinct<SpriteSheet>(ReferenceEqualityComparer.Instance).OrderBy(sheet => sheet.Image.Name, StringComparer.Ordinal)];
        AssertTrue(sheets.Length > 0, "the installation holds spawned-object sheets");
        var pictures = new HeldPicture[sheets.Length][];
        Parallel.For(0, sheets.Length, index => pictures[index] = Pictures(sheets[index]));
        var holders = new Dictionary<string, List<HeldPicture>>(StringComparer.Ordinal);
        foreach (HeldPicture picture in pictures.SelectMany(held => held))
        {
            if (!holders.TryGetValue(picture.Fingerprint, out List<HeldPicture>? held))
                holders.Add(picture.Fingerprint, held = []);
            held.Add(picture);
        }
        string[] repeated = [.. holders.Values.Where(held => held.Count > 1)
            .Select(held => $"frame {held[0].Frame} is held alike by {held.Count} sheets: {string.Join(", ", held.Select(picture => picture.Sheet))}")
            .Order(StringComparer.Ordinal)];
        int copies = holders.Values.Where(held => held.Count > 1).Sum(held => held.Count - 1);
        AssertTrue(repeated.Length == 0,
            $"{repeated.Length} spawned-object frames are held more than once in one colouring with identical pixels, {copies} extra copies across "
            + $"{sheets.Length} installed sheets:{Environment.NewLine}{string.Join(Environment.NewLine, repeated)}");
        Console.WriteLine($"Spawned art held once: {holders.Count} distinct frame pictures of {sheets.Length} installed spawned-object sheets are each held by one sheet.");
    }

    /// <summary>The installed sheets a variant's spawned-object pictures for one owner are on: its own sheet and the common sheets.</summary>
    private static SpriteSheet[] SheetsOf(SpawnedObjectSheet spawned) => [.. spawned.Sheets];

    /// <summary>Each pictured frame of <paramref name="sheet"/> with the fingerprint of everything a draw of it shows.</summary>
    private static HeldPicture[] Pictures(SpriteSheet sheet)
    {
        SpriteImage image = sheet.Image;
        KeyValuePair<PictureShade, PictureRecolouring>[] shades = [.. image.Shades.OrderBy(shade => shade.Key.Key, StringComparer.Ordinal)];
        return [.. sheet.frames.OrderBy(frame => frame.Key, StringComparer.Ordinal).Select(frame =>
            new HeldPicture(frame.Key, image.Name, Fingerprint(frame.Key, frame.Value, image, shades)))];
    }

    /// <summary>
    /// The fingerprint of frame <paramref name="name"/> as a draw shows it: its rectangle's size, origin, depth and
    /// blending, each pixel's colour, alpha and blending, each pixel naming its colour key by the order the frame first
    /// shows the keys, and what each shade shows of those keys.
    /// </summary>
    private static string Fingerprint(string name, SpriteSheetFrame frame, SpriteImage image, KeyValuePair<PictureShade, PictureRecolouring>[] shades)
    {
        ReadOnlySpan<RgbaPixel> pixels = image.Pixels;
        ReadOnlySpan<bool> blending = image.Blending;
        // The sheet's colour keys the frame shows, by their order in the frame.
        var orderOf = new Dictionary<ColourKey, int>();
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            writer.Write(name);
            writer.Write(frame.Size.X);
            writer.Write(frame.Size.Y);
            writer.Write(frame.Origin.X);
            writer.Write(frame.Origin.Y);
            writer.Write(frame.Depth?.ToString() ?? "none");
            writer.Write(frame.Blends);
            int width = image.Size.X;
            for (int row = 0; row < frame.Size.Y; row++)
            {
                for (int column = 0; column < frame.Size.X; column++)
                {
                    int at = (frame.Corner.Y + row) * width + frame.Corner.X + column;
                    RgbaPixel pixel = pixels[at];
                    writer.Write(pixel.R);
                    writer.Write(pixel.G);
                    writer.Write(pixel.B);
                    writer.Write(pixel.A);
                    writer.Write(!blending.IsEmpty && blending[at]);
                    // A clear pixel holds no colour key: no shade recolours it (#1633).
                    if (pixel.A == 0)
                    {
                        writer.Write(-1);
                        continue;
                    }
                    ColourKey key = image.KeyAt(at);
                    if (!orderOf.TryGetValue(key, out int order))
                        orderOf.Add(key, order = orderOf.Count);
                    writer.Write(order);
                }
            }
            foreach ((PictureShade shade, PictureRecolouring shown) in shades)
            {
                KeyedSubstitution[] entries = [.. shown.Substitutions.Where(entry => orderOf.ContainsKey(entry.Replaced))
                    .OrderBy(entry => orderOf[entry.Replaced])];
                if (entries.Length == 0 && !shown.Blends)
                    continue;
                writer.Write(shade.Key);
                writer.Write(shown.Blends);
                writer.Write(entries.Length);
                foreach (KeyedSubstitution entry in entries)
                {
                    writer.Write(orderOf[entry.Replaced]);
                    WriteColour(writer, entry.Shown);
                }
            }
        }
        return Convert.ToHexString(SHA256.HashData(stream.GetBuffer().AsSpan(0, checked((int)stream.Length))));
    }

    /// <summary>Writes a colour's three channels.</summary>
    private static void WriteColour(BinaryWriter writer, RgbColor colour)
    {
        writer.Write(colour.FadeLevels.Red);
        writer.Write(colour.FadeLevels.Green);
        writer.Write(colour.FadeLevels.Blue);
    }

    /// <summary>One frame an installed sheet pictures.</summary>
    /// <param name="Frame">The frame's name.</param>
    /// <param name="Sheet">The installed sheet holding it.</param>
    /// <param name="Fingerprint">The fingerprint of everything a draw of it shows.</param>
    private sealed record HeldPicture(string Frame, string Sheet, string Fingerprint);

    /// <summary>Runs the installed spawned-object sheets' held-once rule.</summary>
    [VerificationCommand("--spawned-art-held-once")]
    internal static int SpawnedArtHeldOnceCommand()
    {
        Suite(nameof(VerifySpawnedArtHeldOnce), VerifySpawnedArtHeldOnce);
        return 0;
    }
}
