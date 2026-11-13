using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Frontend;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms #1646: a stored picture is the box of what its frames show, and its frames place it where the whole rectangle's
/// pixels were. Over the installation, no stored picture has a clear border row or column (a row or column every pixel of
/// which is clear black; a clear pixel keeping a colour of its own, which an opaque layer shows, is part of the box), except
/// the pictures of the turning floors the cartridge repeats beyond their edges, which are stored whole; and the title's floor,
/// reported as a 1024 by 1024 picture holding its 256 by 256 scene in one corner, is stored as that scene. On a synthetic
/// store of one sheet of each family the import writes, the store's assembly of every sheet equals the untrimmed
/// composition, pixel for pixel and map for map.
/// </summary>
internal static class PictureStoreVisibleBoxVerification
{
    /// <summary>
    /// Fails unless no stored picture of the installation has a clear border row or column, except a turning floor shown
    /// whole, which is stored as its frame's whole rectangle, and unless the title's floor is stored as its 256 by 256 scene.
    /// </summary>
    internal static void VerifyStoredPicturesAreTheirVisibleBoxes()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        string content = installation.ContentDirectory;
        string storeDirectory = installation.PictureStoreDirectory;
        string provenancePath = Path.Combine(storeDirectory, PictureStoreFiles.ProvenanceFileName);
        PictureStoreProvenance provenance;
        using (FileStream file = File.OpenRead(provenancePath))
            provenance = JsonAssetDocument.Read<PictureStoreProvenance>(file, PictureStoreProvenance.JsonOptions, $"picture store provenance {provenancePath}");

        HashSet<string> wholeSheets = ShownWholeSheets();
        var shownWhole = new HashSet<string>(StringComparer.Ordinal);
        var notWhole = new List<string>();
        var titleFloors = new List<InstalledPictureReferenceDocument>();
        string titleSheet = SpriteSheetDefinitions.FileNames[SpriteSheetId.TitleFloor];
        foreach ((string family, string sheet, InstalledSheetDocument document) in InstalledSheetManifests.Read(content))
        {
            bool whole = wholeSheets.Contains($"{family}/{sheet}");
            foreach ((string frame, InstalledSheetFrameDocument entry) in document.Frames)
            {
                if (entry.Picture is not { } picture)
                    continue;
                if (family == GameInstallationLayout.SpriteSheetDirectoryName && sheet == titleSheet && frame == TitleFloors.FloorFrameName)
                    titleFloors.Add(picture);
                if (!whole)
                    continue;
                shownWhole.Add(picture.Tag);
                // A floor repeated beyond its edges is stored whole, or as one period it repeats over its rectangle (#1651).
                if (picture.Repeats ? !picture.TilesExactly(entry.Size) : picture.Offset != default || picture.Size != entry.Size)
                    notWhole.Add($"{family}/{sheet} frame {frame} shows {picture.Tag}, {picture.Size.X}x{picture.Size.Y} at ({picture.Offset.X}, {picture.Offset.Y}) of its {entry.Size.X}x{entry.Size.Y} rectangle");
            }
        }
        AssertTrue(shownWhole.Count > 0, "the installation stores the pictures of the turning floors repeated beyond their edges");
        AssertTrue(notWhole.Count == 0,
            $"{notWhole.Count} turning floors repeated beyond their edges are not stored whole:{Environment.NewLine}{string.Join(Environment.NewLine, notWhole)}");

        if (titleFloors is not [InstalledPictureReferenceDocument title])
            throw new InvalidOperationException($"Verification failed: {titleSheet} frame {TitleFloors.FloorFrameName} shows {titleFloors.Count} stored pictures, not one.");
        AssertEqual(TitleFloorScene, title.Size, $"{titleSheet} frame {TitleFloors.FloorFrameName} is stored as its scene");
        AssertEqual(default(Offset16<ImagePixels>), title.Offset, $"{titleSheet} frame {TitleFloors.FloorFrameName}'s scene lies at its rectangle's top-left pixel");

        var margined = new List<string>();
        int checkedPictures = 0;
        foreach (IGrouping<int, KeyValuePair<string, StoredPictureDocument>> onPage in provenance.Pictures.GroupBy(entry => entry.Value.Page))
        {
            RgbaPixel[] pixels;
            Extent16<ImagePixels> size;
            using (FileStream file = File.OpenRead(Path.Combine(storeDirectory, PictureStoreFiles.PageFile(onPage.Key))))
                pixels = RgbaPng.Read(file, out size);
            foreach ((string tag, StoredPictureDocument picture) in onPage)
            {
                if (shownWhole.Contains(tag))
                    continue;
                checkedPictures++;
                if (ClearBorder(pixels, size, picture) is { } border)
                    margined.Add($"{tag} ({picture.Size.X}x{picture.Size.Y}) has a clear {border}");
            }
        }
        AssertTrue(checkedPictures > 0, "the installation stores pictures that are not shown whole");
        AssertTrue(margined.Count == 0,
            $"{margined.Count} stored pictures have a clear border row or column:{Environment.NewLine}{string.Join(Environment.NewLine, margined)}");
        Console.WriteLine($"Picture store: {checkedPictures} stored pictures are the boxes of what their frames show; "
            + $"{shownWhole.Count} pictures of turning floors repeated beyond their edges are stored whole; the title's floor is its "
            + $"{TitleFloorScene.X}x{TitleFloorScene.Y} scene.");
    }

    /// <summary>
    /// Writes a synthetic store of one sheet of each family the import writes and fails unless every sheet the store assembles
    /// equals the untrimmed composition, pixel for pixel, region for region and blend for blend, every frame names the box it
    /// was given at the place it lies, a sheet shown whole keeps its rectangle whole, and two frames whose pictures differ only
    /// in their margins name one stored picture.
    /// </summary>
    internal static void VerifyTrimmedPicturesLandWhereTheyWere()
    {
        using var scratch = new TestScratchDirectory("picture-store-visible-box");
        string content = Path.GetFullPath(scratch.Root);
        FixtureSheet[] sprites = [SpriteFixture(), FloorFixture(SpriteFloorWhole, shownWhole: true), FloorFixture(SpriteFloorTrimmed, shownWhole: false)];
        FixtureSheet[] rooms = [RoomFixture()];
        FixtureSheet[] enemies = [EnemyFixture()];
        FixtureFamily[] families =
        [
            new(GameInstallationLayout.SpriteSheetDirectoryName, sprites),
            new(GameInstallationLayout.RoomBlockSheetDirectoryName, rooms),
            new(GameInstallationLayout.EnemySheetDirectoryName, enemies),
        ];
        PictureStoreWriter.Write(content, [.. families.Select(family => new InstalledSheetFamily(Path.Combine(content, family.Directory),
            [.. family.Sheets.Select(sheet => new NamedInstalledSheet(sheet.Name, sheet.Sheet))]))],
            SupportedCartridge.Sha256);

        InstalledPictureStore store = InstalledPictureStore.Open(Path.Combine(content, PictureStoreFiles.DirectoryName));
        var references = new Dictionary<string, InstalledPictureReferenceDocument>(StringComparer.Ordinal);
        foreach ((string directory, FixtureSheet[] sheets) in families)
        {
            foreach (FixtureSheet sheet in sheets)
            {
                InstalledSheetDocument document;
                using (FileStream manifest = File.OpenRead(Path.Combine(content, directory, SpriteSheetStore.FrameFile(sheet.Name))))
                    document = SpriteSheetFile.ReadDocument(manifest, sheet.Name);
                store.RequireHolds(sheet.Name, document);
                SpriteImagePixels assembled = store.Assemble(sheet.Name, document);
                SpriteImagePixels untrimmed = AssembledSheetImages.Assemble(sheet.Sheet);
                AssertTrue(assembled.Pixels.AsSpan().SequenceEqual(untrimmed.Pixels), $"{sheet.Name}: the trimmed pictures land on every pixel the untrimmed ones did");
                AssertTrue(SameMap(assembled.RecolourRegions, untrimmed.RecolourRegions, untrimmed.Pixels.Length),
                    $"{sheet.Name}: the trimmed pictures' recolour regions land where the untrimmed ones' did");
                AssertTrue(SameMap(assembled.Blending, untrimmed.Blending, untrimmed.Pixels.Length),
                    $"{sheet.Name}: the trimmed pictures' blending lands where the untrimmed ones' did");
                foreach (FixtureFrame frame in sheet.Frames)
                {
                    InstalledPictureReferenceDocument picture = document.Frames[frame.Name].Picture
                        ?? throw new InvalidOperationException($"Verification failed: {sheet.Name} frame {frame.Name} names no stored picture.");
                    AssertEqual(frame.Box.Size, picture.Size, $"{sheet.Name} frame {frame.Name} is stored as the box of what it shows");
                    AssertEqual(frame.Box.Corner - frame.Rectangle.Corner, picture.Offset, $"{sheet.Name} frame {frame.Name} places its box where it lies");
                    references.Add($"{sheet.Name}/{frame.Name}", picture);
                }
            }
        }
        AssertEqual(references[$"{SpriteFixtureName}/{IdleFrame}"].Tag, references[$"{SpriteFixtureName}/{ShiftedFrame}"].Tag,
            "two frames whose pictures differ only in their margins name one stored picture");
        AssertEqual(references[$"{SpriteFixtureName}/{IdleFrame}"].BlendMap, references[$"{SpriteFixtureName}/{ShiftedFrame}"].BlendMap,
            "two frames blending alike in one stored picture name one blend map");
        AssertTrue(references[$"{EnemyFixtureName}/{WalkFrame}"].RecolourRegionMap is not null, "a frame whose pixels of one colour recolour apart names its region map");
        Console.WriteLine("Picture store: trimmed pictures of sprite, room, background, enemy and turning-floor sheets land where the untrimmed "
            + "ones did, maps included; a floor shown whole stays whole; pictures differing only in margin are stored once.");
    }

    /// <summary>The title floor's scene, the part of its floor that shows anything, as the owner's report measured it (#1646).</summary>
    private static Extent16<ImagePixels> TitleFloorScene => new(256, 256);

    /// <summary>
    /// Fails unless each picture of a floor repeated beyond its edges is its frame's smallest period (#1651): the assembled
    /// frame repeats with no smaller period than the stored picture's size, and the frame marks the picture repeated exactly
    /// where that period is smaller than the frame, so no floor stores a repeated copy of itself.
    /// </summary>
    internal static void VerifyRepeatedFloorsStoreOnePeriod()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        InstalledPictureStore store = InstalledPictureStore.Open(installation.PictureStoreDirectory);
        HashSet<string> wholeSheets = ShownWholeSheets();
        var wrong = new List<string>();
        var repeated = new List<string>();
        foreach ((string family, string sheet, InstalledSheetDocument document) in InstalledSheetManifests.Read(installation.ContentDirectory))
        {
            if (!wholeSheets.Contains($"{family}/{sheet}"))
                continue;
            var image = new InstalledImagePlanes(store.Assemble(sheet, document), document.Size);
            foreach ((string frame, InstalledSheetFrameDocument entry) in document.Frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                if (entry.Picture is not { } picture)
                    continue;
                var rectangle = new FrameRectangle(entry.Corner, entry.Size);
                Extent16<ImagePixels> period = image.Period(rectangle);
                if (picture.Size != period || picture.Repeats != (period != entry.Size))
                    wrong.Add($"{family}/{sheet} frame {frame} stores {picture.Size.X}x{picture.Size.Y} (repeats: {picture.Repeats}) of a {entry.Size.X}x{entry.Size.Y} frame whose smallest period is {period.X}x{period.Y}");
                else if (picture.Repeats)
                    repeated.Add($"{sheet} {frame}: {period.X}x{period.Y} of {entry.Size.X}x{entry.Size.Y}");
            }
        }
        AssertTrue(wrong.Count == 0, $"{wrong.Count} repeated floors do not store their smallest period:{Environment.NewLine}{string.Join(Environment.NewLine, wrong)}");
        AssertTrue(repeated.Count > 0, "a floor repeated beyond its edges stores one period it repeats");
        Console.WriteLine($"Picture store: {repeated.Count} repeated floors store one period each ({string.Join("; ", repeated)}).");
    }

    /// <summary>
    /// The sheets shown whole, by family and name: each frontend and room turning floor the cartridge repeats beyond its edges
    /// (<see cref="RepeatingFloors"/>).
    /// </summary>
    private static HashSet<string> ShownWholeSheets() =>
    [
        .. FrontendFloorSheetComposer.Sheets.Where(RepeatingFloors.Repeats)
            .Select(sheet => $"{GameInstallationLayout.SpriteSheetDirectoryName}/{SpriteSheetDefinitions.FileNames[sheet]}"),
        .. RoomBlockUploads.FloorTilesets().Where(RepeatingFloors.Repeats)
            .Select(tileset => $"{GameInstallationLayout.RoomBlockSheetDirectoryName}/{RoomFloorSheet.NameOf(tileset)}"),
    ];

    /// <summary>
    /// The first border of <paramref name="picture"/>, on a page of <paramref name="size"/> holding <paramref name="pixels"/>,
    /// whose every pixel is clear black: its top or bottom row, or its left or right column; null where none is.
    /// </summary>
    private static string? ClearBorder(RgbaPixel[] pixels, Extent16<ImagePixels> size, StoredPictureDocument picture)
    {
        bool Clear(int x, int y) => pixels[((picture.Corner.Y + y) * size.X) + picture.Corner.X + x] == default;
        int width = picture.Size.X, height = picture.Size.Y;
        if (Enumerable.Range(0, width).All(x => Clear(x, 0)))
            return "top row";
        if (Enumerable.Range(0, width).All(x => Clear(x, height - 1)))
            return "bottom row";
        if (Enumerable.Range(0, height).All(y => Clear(0, y)))
            return "left column";
        return Enumerable.Range(0, height).All(y => Clear(width - 1, y)) ? "right column" : null;
    }

    /// <summary>Whether two maps of an image of <paramref name="area"/> pixels hold the same values, a missing map holding the default throughout.</summary>
    private static bool SameMap<T>(T[]? first, T[]? second, int area)
        where T : struct, IEquatable<T>
    {
        for (int pixel = 0; pixel < area; pixel++)
        {
            T left = first is null ? default : first[pixel];
            T right = second is null ? default : second[pixel];
            if (!left.Equals(right))
                return false;
        }
        return true;
    }

    /// <summary>The sprite fixture's name.</summary>
    private const string SpriteFixtureName = "visible-box-sprites";

    /// <summary>The enemy fixture's name.</summary>
    private const string EnemyFixtureName = "visible-box-enemy";

    /// <summary>The whole turning floor fixture's name.</summary>
    private const string SpriteFloorWhole = "visible-box-floor-whole";

    /// <summary>The trimmed turning floor fixture's name.</summary>
    private const string SpriteFloorTrimmed = "visible-box-floor-trimmed";

    /// <summary>The sprite fixture's frame whose picture lies with one margin.</summary>
    private const string IdleFrame = "Idle";

    /// <summary>The sprite fixture's frame showing the same picture with another margin.</summary>
    private const string ShiftedFrame = "Shifted";

    /// <summary>The enemy fixture's frame.</summary>
    private const string WalkFrame = "Walk";

    /// <summary>An opaque colour the fixtures paint.</summary>
    private static readonly RgbaPixel Ink = new(0xC0, 0x40, 0x20);

    /// <summary>A second opaque colour the fixtures paint.</summary>
    private static readonly RgbaPixel OtherInk = new(0x20, 0xA0, 0xE0);

    /// <summary>The colour a clear pixel of a room fixture keeps of its own, which an opaque layer shows.</summary>
    private static readonly RgbaPixel KeptClear = new(0x20, 0x40, 0x60, 0);

    /// <summary>
    /// A sprite sheet: two frames showing one 8 by 7 picture, part of which blends, with different margins, so the picture is
    /// stored once and each frame names its own place.
    /// </summary>
    private static FixtureSheet SpriteFixture()
    {
        var sheet = new FixturePainter(new Extent16<ImagePixels>(64, 32), regions: false, blending: true);
        FrameRectangle idle = Rectangle(0, 0, 24, 24), shifted = Rectangle(32, 0, 24, 24);
        FrameRectangle idleBox = Rectangle(5, 4, 8, 7), shiftedBox = Rectangle(41, 2, 8, 7);
        foreach (FrameRectangle box in new[] { idleBox, shiftedBox })
        {
            sheet.Fill(box, Ink);
            sheet.Paint(box.Corner.X + 1, box.Corner.Y + 1, OtherInk, blends: true);
            sheet.Paint(box.Corner.X + 2, box.Corner.Y + 1, OtherInk, blends: true);
        }
        return sheet.Sheet(SpriteFixtureName, shownWhole: false, [new(IdleFrame, idle, idleBox), new(ShiftedFrame, shifted, shiftedBox)]);
    }

    /// <summary>
    /// A room block sheet: a block whose box starts at a clear pixel keeping a colour of its own, and a background whose one
    /// opaque pixel lies far inside its rectangle.
    /// </summary>
    private static FixtureSheet RoomFixture()
    {
        var sheet = new FixturePainter(new Extent16<ImagePixels>(32, 32), regions: false, blending: false);
        sheet.Paint(2, 2, KeptClear);
        sheet.Paint(9, 11, Ink);
        sheet.Paint(20, 25, OtherInk);
        return sheet.Sheet("visible-box-room", shownWhole: false,
            [new("Block", Rectangle(0, 0, 16, 16), Rectangle(2, 2, 8, 10)), new("Background", Rectangle(16, 0, 16, 32), Rectangle(20, 25, 1, 1))]);
    }

    /// <summary>An enemy sheet: one frame whose two pixels of one colour lie in different recolour regions.</summary>
    private static FixtureSheet EnemyFixture()
    {
        var sheet = new FixturePainter(new Extent16<ImagePixels>(40, 20), regions: true, blending: false);
        sheet.Paint(10, 3, Ink);
        sheet.Paint(20, 15, Ink, region: 1);
        return sheet.Sheet(EnemyFixtureName, shownWhole: false, [new(WalkFrame, Rectangle(0, 0, 40, 20), Rectangle(10, 3, 11, 13))]);
    }

    /// <summary>A turning floor whose one scene lies in a corner of its plane: shown whole, or trimmed to the scene.</summary>
    private static FixtureSheet FloorFixture(string name, bool shownWhole)
    {
        var sheet = new FixturePainter(new Extent16<ImagePixels>(32, 32), regions: false, blending: false);
        FrameRectangle plane = Rectangle(0, 0, 32, 32), scene = Rectangle(3, 3, 4, 4);
        sheet.Fill(scene, shownWhole ? Ink : OtherInk);
        return sheet.Sheet(name, shownWhole, [new("floor", plane, shownWhole ? plane : scene)]);
    }

    /// <summary>The rectangle at (<paramref name="x"/>, <paramref name="y"/>) of <paramref name="width"/> by <paramref name="height"/> pixels.</summary>
    private static FrameRectangle Rectangle(int x, int y, int width, int height) =>
        new(Position16<ImagePixels>.Of(x, y), new Extent16<ImagePixels>(width, height));

    /// <summary>One fixture frame: its name, its rectangle, and the box the store must keep of it.</summary>
    /// <param name="Name">The frame's name.</param>
    /// <param name="Rectangle">The frame's rectangle on its sheet.</param>
    /// <param name="Box">The rectangle of the sheet its stored picture must cover.</param>
    private sealed record FixtureFrame(string Name, FrameRectangle Rectangle, FrameRectangle Box);

    /// <summary>One fixture family: its directory under the content, and its sheets.</summary>
    /// <param name="Directory">The family's directory name.</param>
    /// <param name="Sheets">The family's sheets.</param>
    private sealed record FixtureFamily(string Directory, FixtureSheet[] Sheets);

    /// <summary>One fixture sheet: its name, the converted sheet the store writes, and its frames.</summary>
    /// <param name="Name">The sheet's installed name.</param>
    /// <param name="Sheet">The converted sheet.</param>
    /// <param name="Frames">Its frames, each with the box the store must keep of it.</param>
    private sealed record FixtureSheet(string Name, InstalledSheet Sheet, FixtureFrame[] Frames);

    /// <summary>Paints a fixture sheet's pixels, recolour regions and blending, every pixel starting clear black.</summary>
    private sealed class FixturePainter
    {
        /// <summary>The sheet's size.</summary>
        private readonly Extent16<ImagePixels> size;

        /// <summary>The sheet's pixels.</summary>
        private readonly RgbaPixel[] pixels;

        /// <summary>The sheet's recolour regions, or null for none.</summary>
        private readonly int[]? regions;

        /// <summary>The sheet's blending, or null for none.</summary>
        private readonly bool[]? blending;

        /// <summary>A clear sheet of <paramref name="size"/>, with a recolour-region map and a blend map where asked.</summary>
        internal FixturePainter(Extent16<ImagePixels> size, bool regions, bool blending)
        {
            this.size = size;
            pixels = new RgbaPixel[size.Area];
            this.regions = regions ? new int[size.Area] : null;
            this.blending = blending ? new bool[size.Area] : null;
        }

        /// <summary>Paints the pixel at (<paramref name="x"/>, <paramref name="y"/>) in <paramref name="region"/>, blending where asked.</summary>
        internal void Paint(int x, int y, RgbaPixel colour, int region = 0, bool blends = false)
        {
            int pixel = (y * size.X) + x;
            pixels[pixel] = colour;
            if (region != 0)
                Ensure.NotNull(regions)[pixel] = region;
            if (blends)
                Ensure.NotNull(blending)[pixel] = true;
        }

        /// <summary>Paints every pixel of <paramref name="rectangle"/> in <paramref name="colour"/>.</summary>
        internal void Fill(FrameRectangle rectangle, RgbaPixel colour)
        {
            for (int y = 0; y < rectangle.Size.Y; y++)
                for (int x = 0; x < rectangle.Size.X; x++)
                    Paint(rectangle.Corner.X + x, rectangle.Corner.Y + y, colour);
        }

        /// <summary>The converted sheet named <paramref name="name"/> holding the painted planes and <paramref name="frames"/>.</summary>
        internal FixtureSheet Sheet(string name, bool shownWhole, FixtureFrame[] frames) => new(name,
            new InstalledSheet(new InstalledSheetDocument
            {
                Version = SpriteSheetFile.Version,
                Size = size,
                Frames = frames.ToDictionary(frame => frame.Name, frame => new InstalledSheetFrameDocument
                {
                    Corner = frame.Rectangle.Corner,
                    Size = frame.Rectangle.Size,
                    Origin = default,
                }, StringComparer.Ordinal),
            }, new SpriteImagePixels(pixels, regions, blending))
            {
                ShownWhole = shownWhole,
            },
            frames);
    }

    /// <summary>Runs #1646's confirmation over the installation.</summary>
    [VerificationCommand("--picture-store-visible-box")]
    internal static int PictureStoreVisibleBoxCommand()
    {
        Suite(nameof(VerifyStoredPicturesAreTheirVisibleBoxes), VerifyStoredPicturesAreTheirVisibleBoxes);
        Suite(nameof(VerifyRepeatedFloorsStoreOnePeriod), VerifyRepeatedFloorsStoreOnePeriod);
        return 0;
    }

    /// <summary>Runs #1646's placement confirmation on a synthetic store.</summary>
    [VerificationCommand("--picture-store-placement")]
    internal static int PictureStorePlacementCommand()
    {
        Suite(nameof(VerifyTrimmedPicturesLandWhereTheyWere), VerifyTrimmedPicturesLandWhereTheyWere);
        return 0;
    }
}
