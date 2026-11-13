using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.Artwork;
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
    /// Fails unless every installed library background is one period of what the room shows, or two pages that differ: no
    /// background stores the same page twice, and no one-period background repeats down with a shorter period of whole
    /// 16x16 cells, in its part behind or its part in front.
    /// </summary>
    internal static void VerifyLibraryBackgroundsStoreOnePeriod()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        InstalledPictureStore store = InstalledPictureStore.Open(installation.PictureStoreDirectory);
        var repeated = new List<string>();
        var malformed = new List<string>();
        var shortened = new List<string>();
        int onePeriod = 0, twoPages = 0;
        var stacked = new List<string>();
        var besideEachOther = new List<string>();
        string[] parts = [RoomBackgroundImage.FrameName, PictureParts.InFront(RoomBackgroundImage.FrameName)];
        foreach ((string family, string sheet, InstalledSheetDocument document) in InstalledSheetManifests.Read(installation.ContentDirectory))
        {
            if (family != GameInstallationLayout.RoomBlockSheetDirectoryName || !document.Frames.ContainsKey(RoomBackgroundImage.FrameName))
                continue;
            InstalledSheetFrameDocument behind = document.Frames[RoomBackgroundImage.FrameName];
            int width = behind.Size.X, height = behind.Size.Y;
            // The scrolling sky's rows are pictured whole: they are not a library background's pages.
            if (height == RoomBackgroundImageGeometry.ImageSize(SkyTilemap.Rows * RoomBackgroundImageGeometry.PageTiles).Y)
                continue;
            if (width <= RoomBackgroundImageGeometryConstants.PagePixels && height <= RoomBackgroundImageGeometryConstants.PagePixels)
            {
                onePeriod++;
                if (height < RoomBackgroundImageGeometryConstants.PagePixels || width < RoomBackgroundImageGeometryConstants.PagePixels)
                    shortened.Add($"{sheet} {width}x{height}");
                var image = new InstalledImagePlanes(store.Assemble(sheet, document), document.Size);
                foreach (string part in parts.Where(document.Frames.ContainsKey))
                {
                    InstalledSheetFrameDocument frame = document.Frames[part];
                    if (ShorterCellPeriod(image, frame) is { } shorter)
                        repeated.Add($"{sheet} frame {part} repeats every {shorter.X}x{shorter.Y} of its {frame.Size.X}x{frame.Size.Y}");
                }
                continue;
            }
            // Two different pages are laid out as their room shows them (#1617): side by side, or one above the other.
            bool sideBySide = width == 2 * RoomBackgroundImageGeometryConstants.PagePixels && height == RoomBackgroundImageGeometryConstants.PagePixels;
            if (!sideBySide && (width != RoomBackgroundImageGeometryConstants.PagePixels || height != 2 * RoomBackgroundImageGeometryConstants.PagePixels))
            {
                malformed.Add($"{sheet} is {width}x{height}: neither one page nor two");
                continue;
            }
            twoPages++;
            (sideBySide ? besideEachOther : stacked).Add(sheet);
            SpriteImagePixels assembled = store.Assemble(sheet, document);
            bool samePages = parts.Where(document.Frames.ContainsKey).All(part =>
                HalvesMatch(assembled, document.Size.X, document.Frames[part], sideBySide));
            if (samePages)
                repeated.Add($"{sheet} stores one page twice");
        }
        AssertTrue(malformed.Count == 0, $"library backgrounds are one or two pages:{Environment.NewLine}{string.Join(Environment.NewLine, malformed)}");
        AssertTrue(onePeriod > 0, "the installation holds library backgrounds installed as one period");
        AssertTrue(shortened.Count > 0, "the installation holds library backgrounds whose one page repeats down with a shorter period");
        AssertTrue(repeated.Count == 0,
            $"{repeated.Count} library backgrounds store one page twice:{Environment.NewLine}{string.Join(Environment.NewLine, repeated)}");
        Console.WriteLine($"Room backgrounds: {onePeriod} library backgrounds are one period the room repeats, {shortened.Count} of them shorter " +
            $"than a page ({string.Join(", ", shortened)}); {twoPages} hold two different pages, {besideEachOther.Count} side by side " +
            $"({string.Join(", ", besideEachOther)}) and {stacked.Count} one above the other ({string.Join(", ", stacked)}).");
        // Every room but the Tourian entrance shows two pages side by side; the entrance's statues descend through them (#1617).
        AssertTrue(besideEachOther.Count > 0 && stacked.Count > 0, "the installation holds two-page backgrounds laid out side by side and one above the other");
    }

    /// <summary>
    /// Fails unless every installed effect picture (#1675, #1706), a liquid's surface strip or body or an atmosphere's
    /// picture, is one period across of the rows of the effect's transfer it shows: its width is the shortest period of whole
    /// 16x16 cells that every one of those rows repeats with, or the whole row where none does. Two characters showing the
    /// same pixels are still two characters, so a picture's pixels may repeat more often than its words.
    /// </summary>
    internal static void VerifyFxPagesStoreOnePeriodAcross()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        // The pages are import data (#1638): the import's read holds them.
        RoomFxLayer3Pages tilemaps = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.ImportMaps.RoomFxPages;
        // Each effect picture's name, and the rows of the effect's transfer it shows, cut as the import cuts them.
        Dictionary<string, ushort[]> pages = [];
        const int rowWords = RoomBackgroundImageGeometry.PageTiles;
        foreach (RoomFxType type in Enum.GetValues<RoomFxType>().Where(RoomFxOverlayState.ShowsOnOverlay))
        {
            ushort[] transfer = BackgroundTileWords.ToWords(tilemaps.Resolve(type).Span);
            var shown = new List<(string Name, ushort[] Rows)>();
            if (RoomFxTypes.ShowsLiquid(type))
            {
                LiquidTransferRows cut = RoomEffectTransferCuts.Liquid(transfer);
                shown.Add((RoomEffectPictures.SurfaceName(type), transfer[..(cut.BodyFirstRow * rowWords)]));
                shown.Add((RoomEffectPictures.BodyName(type), transfer[(cut.BodyFirstRow * rowWords)..((cut.BodyFirstRow + cut.BodyRows) * rowWords)]));
            }
            else
                shown.Add((RoomEffectPictures.AtmosphereName(type), transfer[(RoomEffectTransferCuts.AtmosphereFirstRow(transfer) * rowWords)..]));
            foreach ((string name, ushort[] rows) in shown)
            {
                pages[name] = rows;
                pages[PictureParts.InFront(name)] = rows;
            }
        }
        var wrong = new List<string>();
        var narrowed = new SortedSet<string>(StringComparer.Ordinal);
        int checkedPages = 0;
        foreach ((string family, string sheet, InstalledSheetDocument document) in InstalledSheetManifests.Read(installation.ContentDirectory))
        {
            if (family != GameInstallationLayout.RoomBlockSheetDirectoryName)
                continue;
            foreach ((string frame, InstalledSheetFrameDocument entry) in document.Frames.Where(frame => pages.ContainsKey(frame.Key)))
            {
                checkedPages++;
                int expected = PeriodAcross(pages[frame]) * TilePixels;
                if (entry.Size.X != expected)
                    wrong.Add($"{sheet} frame {frame} is {entry.Size.X} wide; its words repeat every {expected} pixels across");
                if (entry.Size.X < RoomBackgroundImageGeometryConstants.PagePixels)
                    narrowed.Add($"{frame} {entry.Size.X} wide");
            }
        }
        AssertTrue(checkedPages > 0, "the installation holds effect pictures");
        AssertTrue(wrong.Count == 0, $"{wrong.Count} effect pictures are not one period across of their words:{Environment.NewLine}{string.Join(Environment.NewLine, wrong)}");
        Console.WriteLine($"Effect pictures: {checkedPages} pictures are one period across of their words ({string.Join(", ", narrowed)}).");
    }

    /// <summary>Pixels along a tile's side.</summary>
    private const int TilePixels = 8;

    /// <summary>The shortest period of whole cells, in tiles, that every 32-tile row of <paramref name="words"/> repeats with; 32 where none does.</summary>
    private static int PeriodAcross(ushort[] words)
    {
        const int rowTiles = RoomBackgroundImageGeometry.PageTiles, cellTiles = 2;
        for (int period = cellTiles; period < rowTiles; period += cellTiles)
        {
            bool repeats = rowTiles % period == 0;
            for (int index = 0; repeats && index < words.Length; index++)
                repeats = words[index] == words[(index / rowTiles * rowTiles) + (index % rowTiles % period)];
            if (repeats)
                return period;
        }
        return rowTiles;
    }

    /// <summary>
    /// Fails unless one period of a background's two pages is the first page alone exactly when the second holds the same
    /// tilemap word for word, and both pages otherwise.
    /// </summary>
    internal static void VerifyOnePeriodKeepsOnlyRepeatedPages()
    {
        ushort[] page = [.. Enumerable.Range(0, RoomBackgroundImageGeometry.PageWords).Select(word => (ushort)(word * 7))];
        ushort[] same = [.. page, .. page];
        RoomBackgroundPeriod period = RoomBackgroundImageGeometry.OnePeriod(same);
        AssertTrue(period.Words.AsSpan().SequenceEqual(page) && period.WidthTiles == RoomBackgroundImageGeometry.PageTiles, "two pages holding one tilemap are one page");
        ushort[] differing = [.. page, .. page];
        differing[^1] ^= 1;
        RoomBackgroundPeriod two = RoomBackgroundImageGeometry.OnePeriod(differing);
        AssertTrue(two.Words.AsSpan().SequenceEqual(differing) && two.WidthTiles == RoomBackgroundImageGeometry.PageTiles,
            "two pages differing in one word stay two pages");
        ushort[] band = [.. Enumerable.Range(0, 4 * RoomBackgroundImageGeometry.PageTiles).Select(word => (ushort)(word * 5))];
        ushort[] banded = [.. Enumerable.Range(0, RoomBackgroundImageGeometry.PageWords / band.Length).SelectMany(_ => band)];
        AssertTrue(RoomBackgroundImageGeometry.OnePeriod([.. banded, .. banded]).Words.AsSpan().SequenceEqual(band),
            "a page whose rows repeat every four rows is those four rows");
        ushort[] row = [.. Enumerable.Range(0, RoomBackgroundImageGeometry.PageTiles).Select(word => (ushort)(word + 3))];
        ushort[] striped = [.. Enumerable.Range(0, RoomBackgroundImageGeometry.PageTiles).SelectMany(_ => row)];
        AssertTrue(RoomBackgroundImageGeometry.OnePeriod([.. striped, .. striped]).Words.Length == 2 * RoomBackgroundImageGeometry.PageTiles,
            "a page whose every row is one row is two rows, one 16x16 cell tall");
        // Each row the same eight tiles four times over: one period across is those eight tiles, down two rows (#1675).
        ushort[] eight = [.. Enumerable.Range(0, 8).Select(word => (ushort)(word + 11))];
        ushort[] tiled = [.. Enumerable.Range(0, RoomBackgroundImageGeometry.PageWords / eight.Length).SelectMany(_ => eight)];
        RoomBackgroundPeriod across = RoomBackgroundImageGeometry.OnePeriod([.. tiled, .. tiled]);
        AssertTrue(across.WidthTiles == eight.Length && across.Words.Length == 2 * eight.Length && across.Words.AsSpan(0, eight.Length).SequenceEqual(eight),
            "a page whose rows repeat every eight tiles across is eight tiles wide");
        Console.WriteLine("Room backgrounds: two pages are one period only when every word matches, and one page shortens to its period of whole cells down and across.");
    }

    /// <summary>
    /// The shortest period of whole 16x16 cells, across or down, shorter than <paramref name="frame"/>'s side, that its pixels
    /// and maps repeat with; null where none does. Every period that divides a side is a multiple of the smallest one, so the
    /// shortest period of whole cells is the least multiple of both, where it divides the side.
    /// </summary>
    private static Extent16<ImagePixels>? ShorterCellPeriod(InstalledImagePlanes image, InstalledSheetFrameDocument frame)
    {
        Extent16<ImagePixels> smallest = image.Period(new FrameRectangle(frame.Corner, frame.Size));
        int across = CellPeriod(smallest.X), down = CellPeriod(smallest.Y);
        bool shorterAcross = across < frame.Size.X && frame.Size.X % across == 0, shorterDown = down < frame.Size.Y && frame.Size.Y % down == 0;
        return shorterAcross || shorterDown
            ? new Extent16<ImagePixels>(shorterAcross ? across : frame.Size.X, shorterDown ? down : frame.Size.Y) : null;
    }

    /// <summary>The least multiple of <paramref name="period"/> that is whole 16x16 cells.</summary>
    private static int CellPeriod(int period) => period * RoomBackgroundImage.CellPixels / Gcd(period, RoomBackgroundImage.CellPixels);

    /// <summary>The greatest common divisor of two positive numbers.</summary>
    private static int Gcd(int first, int second) => second == 0 ? first : Gcd(second, first % second);

    /// <summary>Whether the two pages of <paramref name="frame"/>'s rectangle on the assembled image match: its left and right halves where <paramref name="sideBySide"/>, otherwise its upper and lower.</summary>
    private static bool HalvesMatch(SpriteImagePixels assembled, int imageWidth, InstalledSheetFrameDocument frame, bool sideBySide)
    {
        int rows = sideBySide ? frame.Size.Y : frame.Size.Y / 2, width = sideBySide ? frame.Size.X / 2 : frame.Size.X;
        for (int row = 0; row < rows; row++)
        {
            int first = ((frame.Corner.Y + row) * imageWidth) + frame.Corner.X;
            int second = sideBySide ? first + width : first + (rows * imageWidth);
            if (!assembled.Pixels.AsSpan(first, width).SequenceEqual(assembled.Pixels.AsSpan(second, width)))
                return false;
            if (assembled.RecolourRegions is { } regions && !regions.AsSpan(first, width).SequenceEqual(regions.AsSpan(second, width)))
                return false;
            if (assembled.Blending is { } blending && !blending.AsSpan(first, width).SequenceEqual(blending.AsSpan(second, width)))
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
        Suite(nameof(VerifyFxPagesStoreOnePeriodAcross), VerifyFxPagesStoreOnePeriodAcross);
        return 0;
    }
}
