using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>
/// Holds every frame of the installed Samus body sheet to the whole-Samus rule (owner rulings on #1513: no sprite sheet
/// frame shows half a Samus, and sheets are checked automatically), read from the sheet's installed picture's alpha.
/// <list type="bullet">
/// <item><b>One figure.</b> A frame's opaque pixels form exactly one figure of meaningful size: grouping pixels that lie
/// within <see cref="FigureReach"/> pixels of each other horizontally and vertically, exactly one group holds at least
/// <see cref="MeaningfulPixels"/> pixels. A frame of scattered bodies, a Screw Attack outline without Samus, a pair of
/// legs beside a torso or a frame showing nothing fails.</item>
/// <item><b>Both halves.</b> Each picture frame the body shows is drawn by its pose's own halves rule, and each half the
/// rule draws whose pose-half table holds a composition there must paint a pixel inside that composition's parts on the
/// frame: a frame drawn without the legs or the top its rule shows fails. A half whose table holds no composition at the
/// frame is drawn empty by the game too (the right-facing drained crouch's third frame is its curled top alone), and the
/// one-figure rule vouches for such a frame.</item>
/// <item><b>One selection.</b> Every part of a frame is the selection of the one pose and picture frame that draws it:
/// both halves are drawn at that pose and frame, each composition is the one that pose half's table holds there, and
/// each half picture is the one that frame itself selects. A frame pairing a top and a bottom of different poses or
/// frames, or carrying a half picture another frame selected, fails.</item>
/// </list>
/// </summary>
/// <remarks>
/// Calibration, from the stock sheet the derivation composes: grouping by adjacent pixels alone splits real frames,
/// since the running pose aiming forward has a one-pixel seam between its torso and legs on its seventh frame, so pixels
/// two apart (one transparent pixel between) belong to one figure. At that reach the largest stray piece of any stock
/// frame is eleven pixels, the sparks of the Screw Attack's aura, and the smallest whole figure, the morph ball, is 208
/// pixels; sixteen pixels separates them. The frames the drawing used to show and no longer does, scattered halves of
/// other poses past a program and the Screw Attack outline alone, hold two or more groups of sixteen or more pixels, or
/// none.
/// </remarks>
internal static class SamusBodyWholeFigureVerification
{
    /// <summary>How far apart, in pixels along each axis, two opaque pixels can lie and still belong to one figure.</summary>
    private const int FigureReach = 2;

    /// <summary>The fewest opaque pixels a group needs to count as a figure rather than a stray spark.</summary>
    private const int MeaningfulPixels = 16;

    /// <summary>Checks every installed Samus body sheet frame against the whole-Samus rule.</summary>
    internal static void VerifySamusBodyWholeFigures()
    {
        SpriteSheet sheet = InstalledFixtures.SpriteSheets.Get(SpriteSheetId.SamusBody);
        SpriteImage image = sheet.Image;
        var opaque = new OpaquePixels(image.Pixels.ToArray(), image.Size.X);

        SamusBodySources body = InstalledFixtures.SamusBody;
        SamusSpritemapSources spritemaps = body.Spritemaps;
        var drawers = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var findings = new List<string>();
        foreach (SamusShownBody shown in SamusShownFrames.Of(InstalledFixtures.AnimationContinuation).Bodies(body))
        {
            SamusBodyComposition drawn = shown.Composition;
            if (spritemaps.PictureKey(drawn, shown.Pictures) is not { } key)
                continue;
            string name = shown.Frame.Name;
            string drawer = $"{shown.Frame.Pose} frame {shown.Frame.PictureFrame}";
            if (!drawers.TryGetValue(name, out List<string>? named))
            {
                named = [];
                drawers.Add(name, named);
            }
            named.Add(drawer);
            findings.AddRange(ForeignSelections(drawn, key, spritemaps, body, name, drawer));
            if (sheet.ShowsNothing(name))
                continue;
            SpriteSheetFrame frame = sheet.Frame(name);
            SamusBodyHalfFrame[] halves = drawn.Bottom is { } lower ? [drawn.Top, lower] : [drawn.Top];
            foreach (SamusBodyHalfFrame half in halves)
            {
                if (spritemaps.TryGetFrame(half, out SpriteFrame? composition) && !opaque.PaintsWithin(frame, composition))
                    findings.Add($"frame {name} ({drawer}) lacks its {half.Half} half, which its pose's own rule draws");
            }
        }
        foreach (KeyValuePair<string, List<string>> named in drawers.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            string who = string.Join(", ", named.Value);
            if (sheet.ShowsNothing(named.Key))
            {
                findings.Add($"frame {named.Key} ({who}) shows nothing");
                continue;
            }
            int[] figures = opaque.FigureSizes(sheet.Frame(named.Key));
            int meaningful = figures.Count(size => size >= MeaningfulPixels);
            if (meaningful != 1)
                findings.Add($"frame {named.Key} ({who}) holds {meaningful} figures of {MeaningfulPixels} or more pixels, not one: pixel groups {string.Join(", ", figures)}");
        }
        AssertTrue(findings.Count == 0,
            $"{findings.Count} Samus body sheet frames are not one whole Samus:{Environment.NewLine}{string.Join(Environment.NewLine, findings)}");
        Console.WriteLine($"Samus body whole figures: {drawers.Count} frames each show one whole Samus with every half their pose's own rule draws.");
    }

    /// <summary>
    /// The parts of <paramref name="key"/> that are not the selection of the pose and picture frame <paramref name="drawn"/>
    /// composes: a bottom half drawn at another pose or frame than the top, a composition other than the one the pose half's
    /// table holds at the frame, or a half picture other than the one the frame itself selects.
    /// </summary>
    /// <exception cref="InvalidDataException">The pose's frame has no installed pictures.</exception>
    private static IEnumerable<string> ForeignSelections(SamusBodyComposition drawn, SamusBodyPictureKey key,
        SamusSpritemapSources spritemaps, SamusBodySources body, string name, string drawer)
    {
        SamusBodyHalfFrame top = drawn.Top;
        if (drawn.Bottom is { } lower && (lower.Pose != top.Pose || lower.AnimationFrame != top.AnimationFrame))
            yield return $"frame {name} ({drawer}) draws its bottom half from {lower.Pose} frame {lower.AnimationFrame}";
        if (key.Top != OwnComposition(spritemaps, top))
            yield return $"frame {name} ({drawer}) shows a top composition its pose's table does not hold at the frame";
        SamusSpritemapId? ownBottom = drawn.Bottom is { } bottomHalf ? OwnComposition(spritemaps, bottomHalf) : null;
        if (key.Bottom != ownBottom)
            yield return $"frame {name} ({drawer}) shows a bottom composition its pose's table does not hold at the frame";
        SamusBodyFramePictures own = body.Pictures(top.Pose, top.AnimationFrame);
        if (key.TopPicture is { } topPicture && topPicture != own.Top)
            yield return $"frame {name} ({drawer}) shows top picture {topPicture.Ordinal}, not its frame's own {own.Top.Ordinal}";
        if (key.BottomPicture is { } bottomPicture && bottomPicture != own.Bottom)
            yield return $"frame {name} ({drawer}) shows bottom picture {bottomPicture.Ordinal}, not one its frame selects";
    }

    /// <summary>The composition <paramref name="half"/>'s pose-half table holds at its frame, or none.</summary>
    private static SamusSpritemapId? OwnComposition(SamusSpritemapSources spritemaps, SamusBodyHalfFrame half) =>
        spritemaps.TryGetComposition(half, out SamusSpritemapId composition) ? composition : null;

    /// <summary>Runs the Samus body sheet's whole-Samus rule.</summary>
    [VerificationCommand("--samus-body-whole-figures")]
    internal static int SamusBodyWholeFiguresCommand()
    {
        Suite(nameof(VerifySamusBodyWholeFigures), VerifySamusBodyWholeFigures);
        return 0;
    }

    /// <summary>Which pixels of the body sheet's image are not clear.</summary>
    /// <param name="pixels">Each pixel of the image, row by row from the top.</param>
    /// <param name="width">The image's width.</param>
    private sealed class OpaquePixels(RgbaPixel[] pixels, int width)
    {
        /// <summary>Whether the frame-local pixel at (<paramref name="x"/>, <paramref name="y"/>) of <paramref name="frame"/> is opaque.</summary>
        private bool IsOpaque(SpriteSheetFrame frame, int x, int y) =>
            pixels[(frame.Corner.Y + y) * width + frame.Corner.X + x].A != 0;

        /// <summary>
        /// Whether <paramref name="frame"/> paints a pixel inside any part of <paramref name="composition"/>, placed from the
        /// frame's origin.
        /// </summary>
        internal bool PaintsWithin(SpriteSheetFrame frame, SpriteFrame composition)
        {
            foreach (SpriteFramePart part in composition.Parts)
            {
                int left = frame.Origin.X + part.Offset.X, top = frame.Origin.Y + part.Offset.Y;
                for (int y = Math.Max(0, top); y < Math.Min(frame.Size.Y, top + part.Size.Y); y++)
                {
                    for (int x = Math.Max(0, left); x < Math.Min(frame.Size.X, left + part.Size.X); x++)
                    {
                        if (IsOpaque(frame, x, y))
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// The sizes of the groups <paramref name="frame"/>'s opaque pixels form, joining pixels at most
        /// <see cref="FigureReach"/> apart along each axis, largest first.
        /// </summary>
        internal int[] FigureSizes(SpriteSheetFrame frame)
        {
            int frameWidth = frame.Size.X, frameHeight = frame.Size.Y;
            var grouped = new bool[frameWidth * frameHeight];
            var sizes = new List<int>();
            var pending = new Stack<int>();
            for (int start = 0; start < grouped.Length; start++)
            {
                if (grouped[start] || !IsOpaque(frame, start % frameWidth, start / frameWidth))
                    continue;
                grouped[start] = true;
                pending.Push(start);
                int size = 0;
                while (pending.TryPop(out int pixel))
                {
                    size++;
                    int px = pixel % frameWidth, py = pixel / frameWidth;
                    for (int y = Math.Max(0, py - FigureReach); y <= Math.Min(frameHeight - 1, py + FigureReach); y++)
                    {
                        for (int x = Math.Max(0, px - FigureReach); x <= Math.Min(frameWidth - 1, px + FigureReach); x++)
                        {
                            int neighbour = y * frameWidth + x;
                            if (!grouped[neighbour] && IsOpaque(frame, x, y))
                            {
                                grouped[neighbour] = true;
                                pending.Push(neighbour);
                            }
                        }
                    }
                }
                sizes.Add(size);
            }
            return [.. sizes.OrderDescending()];
        }
    }
}
