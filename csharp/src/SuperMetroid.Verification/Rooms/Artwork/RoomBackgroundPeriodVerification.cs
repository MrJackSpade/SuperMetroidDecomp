using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// Confirms a library room background is installed as one period of what the room shows (#1651). Its two BG2 pages lie
/// side by side on a map the hardware wraps across and down, so where both pages hold one tilemap the background is that
/// one page repeated. The import then installs the page alone, and only a background whose pages differ keeps both, the
/// first above the second.
/// </summary>
internal static class RoomBackgroundPeriodVerification
{
    /// <summary>
    /// Fails unless every installed library background is one page, or two pages that differ: no background stores the same
    /// page twice, in its part behind or its part in front.
    /// </summary>
    internal static void VerifyLibraryBackgroundsStoreOnePeriod()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        InstalledPictureStore store = InstalledPictureStore.Open(installation.PictureStoreDirectory);
        var repeated = new List<string>();
        var malformed = new List<string>();
        int onePage = 0, twoPages = 0;
        string[] parts = [RoomBackgroundImage.FrameName, PictureParts.InFront(RoomBackgroundImage.FrameName)];
        foreach ((string family, string sheet, InstalledSheetDocument document) in InstalledSheetManifests.Read(installation.ContentDirectory))
        {
            if (family != GameInstallationLayout.RoomBlockSheetDirectoryName || !document.Frames.ContainsKey(RoomBackgroundImage.FrameName))
                continue;
            InstalledSheetFrameDocument behind = document.Frames[RoomBackgroundImage.FrameName];
            int width = behind.Size.X, height = behind.Size.Y;
            // The scrolling sky's rows are pictured whole: they are not a library background's pages.
            if (width != RoomBackgroundImageGeometry.PagePixels || height == RoomBackgroundImageGeometry.ImageSize(SkyTilemap.Rows * RoomBackgroundImageGeometry.PageTiles).Y)
                continue;
            if (height == RoomBackgroundImageGeometry.PagePixels)
            {
                onePage++;
                continue;
            }
            if (height != 2 * RoomBackgroundImageGeometry.PagePixels)
            {
                malformed.Add($"{sheet} is {width}x{height}: neither one page nor two");
                continue;
            }
            twoPages++;
            SpriteImagePixels assembled = store.Assemble(sheet, document);
            bool samePages = parts.Where(document.Frames.ContainsKey).All(part =>
                HalvesMatch(assembled, document.Size.X, document.Frames[part]));
            if (samePages)
                repeated.Add($"{sheet} stores one page twice");
        }
        AssertTrue(malformed.Count == 0, $"library backgrounds are one or two pages:{Environment.NewLine}{string.Join(Environment.NewLine, malformed)}");
        AssertTrue(onePage > 0, "the installation holds library backgrounds installed as one page");
        AssertTrue(repeated.Count == 0,
            $"{repeated.Count} library backgrounds store one page twice:{Environment.NewLine}{string.Join(Environment.NewLine, repeated)}");
        Console.WriteLine($"Room backgrounds: {onePage} library backgrounds are one page the room repeats; {twoPages} hold two different pages.");
    }

    /// <summary>
    /// Fails unless one period of a background's two pages is the first page alone exactly when the second holds the same
    /// tilemap word for word, and both pages otherwise.
    /// </summary>
    internal static void VerifyOnePeriodKeepsOnlyRepeatedPages()
    {
        ushort[] page = [.. Enumerable.Range(0, RoomBackgroundImageGeometry.PageWords).Select(word => (ushort)(word * 7))];
        ushort[] same = [.. page, .. page];
        ushort[] period = RoomBackgroundImageGeometry.OnePeriod(same);
        AssertTrue(period.AsSpan().SequenceEqual(page), "two pages holding one tilemap are one page");
        ushort[] differing = [.. page, .. page];
        differing[^1] ^= 1;
        AssertTrue(RoomBackgroundImageGeometry.OnePeriod(differing).AsSpan().SequenceEqual(differing),
            "two pages differing in one word stay two pages");
        Console.WriteLine("Room backgrounds: two pages are one period only when every word matches.");
    }

    /// <summary>Whether the upper half of <paramref name="frame"/>'s rectangle on the assembled image equals its lower half.</summary>
    private static bool HalvesMatch(SpriteImagePixels assembled, int imageWidth, InstalledSheetFrameDocument frame)
    {
        int half = frame.Size.Y / 2;
        for (int row = 0; row < half; row++)
        {
            int upper = ((frame.Corner.Y + row) * imageWidth) + frame.Corner.X, lower = upper + (half * imageWidth);
            if (!assembled.Pixels.AsSpan(upper, frame.Size.X).SequenceEqual(assembled.Pixels.AsSpan(lower, frame.Size.X)))
                return false;
            if (assembled.RecolourRegions is { } regions && !regions.AsSpan(upper, frame.Size.X).SequenceEqual(regions.AsSpan(lower, frame.Size.X)))
                return false;
            if (assembled.Blending is { } blending && !blending.AsSpan(upper, frame.Size.X).SequenceEqual(blending.AsSpan(lower, frame.Size.X)))
                return false;
        }
        return true;
    }

    /// <summary>Runs the library background period checks.</summary>
    [VerificationCommand("--room-background-period")]
    internal static int RoomBackgroundPeriodCommand()
    {
        Suite(nameof(VerifyOnePeriodKeepsOnlyRepeatedPages), VerifyOnePeriodKeepsOnlyRepeatedPages);
        Suite(nameof(VerifyLibraryBackgroundsStoreOnePeriod), VerifyLibraryBackgroundsStoreOnePeriod);
        return 0;
    }
}
