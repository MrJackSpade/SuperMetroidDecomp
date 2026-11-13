using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Writes an installation's picture store (#1635, #1644), the one place pictures are written. Every family's sheets are
/// walked in one fixed order: the families as given, each family's sheets by ordinal name, each sheet's frame rectangles by
/// corner Y, then X. A rectangle's picture is the box of what it shows (<see cref="InstalledImagePlanes.ContentBox"/>,
/// #1646), its clear margin left out, and its frames name where the box lies in the rectangle; a sheet the game shows whole
/// (<see cref="InstalledSheet.ShownWhole"/>) keeps each rectangle whole. The first rectangle showing a picture's pixels
/// stores it; every later rectangle showing the same pixels (<see cref="InstalledPictureContent"/>), on any sheet and at any
/// place in its rectangle, names that copy, whatever recolour regions and blending it shows them with. Those maps are the
/// rectangle's own (<see cref="PictureMap{T}"/>), read over the same box: each distinct map is stored once, under a tag of
/// its own, in its own file apart from the pages, and every rectangle names the maps it shows its picture with. A
/// rectangle that shows nothing at all stores nothing. The stored pictures are shelf-packed onto pages in that order
/// (<see cref="PicturePageShelves"/>), every picture admitted to the installation's <see cref="SheetPictureRegistry"/> as
/// its page is written, so a second copy of the same pixels fails the install. The pages are encoded in parallel, each into
/// its own file, and the map files, the provenance manifest and each family's sheet manifests are written after them, so the
/// bytes are the same whatever the thread timing.
/// </summary>
internal static class PictureStoreWriter
{
    /// <summary>The first letter of every recolour-region map's tag.</summary>
    private const string RecolourRegionMapTag = "R";

    /// <summary>The first letter of every blend map's tag.</summary>
    private const string BlendMapTag = "B";

    /// <summary>Writes the store of <paramref name="families"/> under <paramref name="content"/>, then each family's sheet manifests.</summary>
    /// <param name="content">The content directory the store and the families' directories lie in.</param>
    /// <param name="families">The families, in the order their pictures are written.</param>
    /// <param name="sourceCartridgeSha256">Identity written into every provenance manifest.</param>
    /// <exception cref="ArgumentException">A family's directory does not lie directly under the content directory, or two families share one.</exception>
    /// <exception cref="InvalidDataException">
    /// A sheet is installed twice under one name, a frame leaves its sheet or shows maps but no pixel, a picture does not fit
    /// a page, or a picture or a map is written twice.
    /// </exception>
    internal static void Write(string content, IReadOnlyList<InstalledSheetFamily> families, string sourceCartridgeSha256)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        Ensure.NotNull(families);
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(content));
        var store = new StoreContents();
        var placed = new List<SheetPictures>();
        var familyNames = new HashSet<string>(StringComparer.Ordinal);
        foreach (InstalledSheetFamily family in families)
        {
            Ensure.NotNull(family);
            string? parent = Path.GetDirectoryName(Path.TrimEndingDirectorySeparator(Path.GetFullPath(family.Directory)));
            if (!string.Equals(parent, root, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Sheet family directory {family.Directory} does not lie directly under content directory {root}.", nameof(families));
            if (!familyNames.Add(family.DirectoryName))
                throw new ArgumentException($"Sheet family {family.DirectoryName} is written twice.", nameof(families));
            var sheetNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (NamedInstalledSheet sheet in family.Sheets.OrderBy(sheet => sheet.Name, StringComparer.Ordinal))
            {
                if (!sheetNames.Add(sheet.Name))
                    throw new InvalidDataException($"Sheet family {family.DirectoryName} installs two sheets named {sheet.Name}.");
                placed.Add(new SheetPictures(family, sheet, Store(family, sheet, store)));
            }
        }
        List<StoredPicture> stored = store.Pictures;
        var shelves = new PicturePageShelves(stored.Count == 0 ? 0 : stored.Max(picture => picture.Content.Rectangle.Size.X));
        foreach (StoredPicture picture in stored)
        {
            picture.Page = shelves.Place(picture.Content.Rectangle.Size, out Position16<ImagePixels> corner);
            picture.Corner = corner;
        }
        // Each picture's tag is its page and its place in the page's write order, so it is the same at every install (#1639).
        var placedOnPage = new Dictionary<int, int>();
        foreach (StoredPicture picture in stored)
        {
            int place = placedOnPage.GetValueOrDefault(picture.Page);
            placedOnPage[picture.Page] = place + 1;
            picture.Tag = $"P{picture.Page}-{place:D4}";
        }
        IReadOnlyList<Extent16<ImagePixels>> pages = shelves.Pages;
        WriteStore(root, pages, store, sourceCartridgeSha256);
        foreach (InstalledSheetFamily family in families)
            WriteManifests(family, [.. placed.Where(sheet => ReferenceEquals(sheet.Family, family))]);
    }

    /// <summary>
    /// Finds where the store holds the picture of each of <paramref name="sheet"/>'s frame rectangles, the box of what the
    /// rectangle shows or, for a sheet shown whole, the whole rectangle, and the maps the rectangle shows it with over that
    /// box, storing each picture and each map no earlier rectangle showed; a rectangle that shows nothing has none.
    /// </summary>
    /// <returns>Each rectangle showing a picture, with the stored copy, where it lies in the rectangle, and the maps it names.</returns>
    /// <exception cref="InvalidDataException">A frame leaves the sheet, or shows a recolour region or blending but no pixel.</exception>
    private static Dictionary<FrameRectangle, ShownPicture> Store(InstalledSheetFamily family, NamedInstalledSheet sheet, StoreContents store)
    {
        InstalledSheet installed = Ensure.NotNull(sheet.Sheet);
        InstalledImagePlanes image = installed.Image;
        string sheetName = $"{family.DirectoryName}/{sheet.Name}";
        var shownAt = new Dictionary<FrameRectangle, ShownPicture>();
        foreach (IGrouping<FrameRectangle, string> rectangle in installed.Manifest.Frames
            .GroupBy(frame => new FrameRectangle(frame.Value.Corner, frame.Value.Size), frame => frame.Key)
            .Where(rectangle => !rectangle.Key.IsEmpty)
            .OrderBy(rectangle => rectangle.Key.Corner.Y).ThenBy(rectangle => rectangle.Key.Corner.X)
            .ThenBy(rectangle => rectangle.Key.Size.Y).ThenBy(rectangle => rectangle.Key.Size.X))
        {
            InstalledPictureContent whole;
            try
            {
                whole = new InstalledPictureContent(image, rectangle.Key);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Sheet {sheetName}: {error.Message}", error);
            }
            string frames = string.Join(", ", rectangle.Order(StringComparer.Ordinal));
            if (whole.IsClear)
            {
                // A rectangle of clear black pixels shows nothing: the import keys no clear pixel, so it has no maps either.
                if (!image.IsClear(rectangle.Key))
                    throw new InvalidDataException(
                        $"Sheet {sheetName} frame(s) [{frames}] at {rectangle.Key} show no pixel but a recolour region or blending.");
                continue;
            }
            // Outside the box the rectangle holds only what an assembled image starts as, so the box alone lands every pixel,
            // region and blend where the whole rectangle would (#1646). A sheet shown whole is a turning floor the game shows
            // repeated beyond its edges: its picture is the rectangle's smallest period, which tiling over the rectangle
            // gives back, so the floor repeats the period with no copy of it stored (#1651).
            FrameRectangle box = installed.ShownWhole
                ? new FrameRectangle(rectangle.Key.Corner, image.Period(rectangle.Key))
                : image.ContentBox(rectangle.Key);
            bool repeats = installed.ShownWhole && box != rectangle.Key;
            InstalledPictureContent picture = box == rectangle.Key ? whole : new InstalledPictureContent(image, box);
            if (!store.PictureOf.TryGetValue(picture, out StoredPicture? copy))
            {
                copy = new StoredPicture(picture, sheetName, frames);
                store.PictureOf.Add(picture, copy);
                store.Pictures.Add(copy);
            }
            shownAt.Add(rectangle.Key, new ShownPicture(copy, box.Corner - rectangle.Key.Corner,
                store.RecolourRegionMaps.Named(picture.RecolourRegions), store.BlendMaps.Named(picture.Blending), repeats));
        }
        return shownAt;
    }

    /// <summary>
    /// Writes every page of the store's pictures under <paramref name="root"/>, each picture admitted to the installation's
    /// registry first, then each kind of map the pictures are shown with, and the store's provenance manifest.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture or a map is written twice.</exception>
    private static void WriteStore(string root, IReadOnlyList<Extent16<ImagePixels>> pages, StoreContents store, string sourceCartridgeSha256)
    {
        var registry = new SheetPictureRegistry();
        var onPage = new List<StoredPicture>[pages.Count];
        for (int page = 0; page < pages.Count; page++)
            onPage[page] = [];
        foreach (StoredPicture picture in store.Pictures)
        {
            registry.Admit(picture.Content, picture.Sheet, picture.Frames);
            onPage[picture.Page].Add(picture);
        }
        string directory = Path.Combine(root, PictureStoreFiles.DirectoryName);
        Directory.CreateDirectory(directory);
        var written = new string[pages.Count];
        // PNG compression is the slow part; each worker reuses one encoder's buffers for every page it encodes, and each
        // page goes to its own file, so the files are the same as writing the pages one by one.
        Parallel.For(0, pages.Count, () => new RgbaPngBuffers(), (page, _, png) =>
        {
            written[page] = WritePage(directory, page, pages[page], onPage[page], png);
            return png;
        }, png => png.Dispose());
        var pageHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int page = 0; page < pages.Count; page++)
            pageHashes.Add(PictureStoreFiles.PageFile(page), written[page]);
        StoredMapsDocument? regions = store.RecolourRegionMaps.Write(PictureStoreFiles.RecolourRegionMapsFile(root), RecolourRegionMapFile.Encode);
        StoredMapsDocument? blending = store.BlendMaps.Write(PictureStoreFiles.BlendMapsFile(root), BlendMapFile.Encode);
        Dictionary<string, StoredPictureDocument> tagged = store.Pictures.ToDictionary(picture => picture.Tag,
            picture => new StoredPictureDocument { Page = picture.Page, Corner = picture.Corner, Size = picture.Content.Rectangle.Size },
            StringComparer.Ordinal);
        File.WriteAllBytes(Path.Combine(directory, PictureStoreFiles.ProvenanceFileName), JsonSerializer.SerializeToUtf8Bytes(
            new PictureStoreProvenance(PictureStoreProvenance.FormatVersion, sourceCartridgeSha256, pageHashes, tagged, regions, blending),
            PictureStoreProvenance.JsonOptions));
    }

    /// <summary>
    /// Writes page <paramref name="page"/> of <paramref name="size"/> pixels holding the pixels of <paramref name="pictures"/>,
    /// clear black elsewhere, into the store's <paramref name="directory"/>.
    /// </summary>
    /// <returns>The page file's hash.</returns>
    private static string WritePage(string directory, int page, Extent16<ImagePixels> size, List<StoredPicture> pictures, RgbaPngBuffers png)
    {
        var planes = new InstalledImagePlanes(new SpriteImagePixels(new RgbaPixel[size.X * size.Y], RecolourRegions: null, Blending: null), size);
        foreach (StoredPicture picture in pictures)
            picture.Content.CopyPixelsTo(planes, picture.Corner);
        return WriteFile(Path.Combine(directory, PictureStoreFiles.PageFile(page)), RgbaPng.Encode(size, planes.Planes.Pixels, png));
    }

    /// <summary>
    /// Writes each of <paramref name="sheets"/>' manifests, every frame naming where the store holds its picture and the
    /// maps it shows it with; the manifests are serialized in parallel and written in the sheets' order. No provenance manifest
    /// lists them (#1693).
    /// </summary>
    private static void WriteManifests(InstalledSheetFamily family, SheetPictures[] sheets)
    {
        var manifests = new byte[sheets.Length][];
        Parallel.For(0, sheets.Length, index => manifests[index] = Manifest(sheets[index]));
        SpriteSheetStore.Write(family.Directory,
            [.. sheets.Select((sheet, index) => KeyValuePair.Create(sheet.Sheet.Name, manifests[index]))]);
    }

    /// <summary>
    /// The installed manifest of <paramref name="sheet"/>, each frame showing a picture naming the store's copy of it and the
    /// maps it shows it with.
    /// </summary>
    private static byte[] Manifest(SheetPictures sheet)
    {
        InstalledSheetDocument manifest = sheet.Sheet.Sheet.Manifest;
        Dictionary<string, InstalledSheetFrameDocument> frames = manifest.Frames.ToDictionary(frame => frame.Key,
            frame => sheet.ShownAt.TryGetValue(new FrameRectangle(frame.Value.Corner, frame.Value.Size), out ShownPicture? shown)
                ? frame.Value with { Picture = shown.Reference }
                : frame.Value,
            StringComparer.Ordinal);
        return SpriteSheetManifestWriter.Write(manifest with { Frames = frames });
    }

    /// <summary>Writes a new installed file at <paramref name="path"/>, creating its folder.</summary>
    /// <returns>The file's SHA-256.</returns>
    private static string WriteFile(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Ensure.NotNull(Path.GetDirectoryName(path)));
        using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            file.Write(bytes);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    /// <summary>What the store holds once every sheet is walked: its pictures and the maps they are shown with.</summary>
    private sealed class StoreContents
    {
        /// <summary>Every stored picture, in write order.</summary>
        internal List<StoredPicture> Pictures { get; } = [];

        /// <summary>Every stored picture, by its pixels.</summary>
        internal Dictionary<InstalledPictureContent, StoredPicture> PictureOf { get; } = new();

        /// <summary>Every recolour-region map a picture is shown with.</summary>
        internal StoredMaps<int> RecolourRegionMaps { get; } = new(RecolourRegionMapTag, "recolour-region");

        /// <summary>Every blend map a picture is shown with.</summary>
        internal StoredMaps<bool> BlendMaps { get; } = new(BlendMapTag, "blend");
    }

    /// <summary>
    /// Every map of one kind the store's pictures are shown with, each once by its values, tagged in the order rectangles
    /// first show it: the kind's letter and its place, such as <c>R0042</c>.
    /// </summary>
    /// <param name="prefix">The kind's letter, the start of every tag.</param>
    /// <param name="kind">Names the kind in a failure.</param>
    /// <typeparam name="T">The value each pixel of a map holds.</typeparam>
    private sealed class StoredMaps<T>(string prefix, string kind)
        where T : unmanaged, IEquatable<T>
    {
        /// <summary>Every map's tag, by its values.</summary>
        private readonly Dictionary<PictureMap<T>, string> tags = new();

        /// <summary>Every map by its tag, in tag order.</summary>
        private readonly List<KeyValuePair<string, PictureMap<T>>> maps = [];

        /// <summary>The tag of <paramref name="map"/>, which is stored when no earlier rectangle showed it; null for no map.</summary>
        internal string? Named(PictureMap<T>? map)
        {
            if (map is null)
                return null;
            if (tags.TryGetValue(map, out string? tag))
                return tag;
            tag = $"{prefix}{maps.Count:D4}";
            tags.Add(map, tag);
            maps.Add(KeyValuePair.Create(tag, map));
            return tag;
        }

        /// <summary>
        /// Writes every map at <paramref name="path"/>, as <paramref name="encode"/> encodes them, each admitted once by its
        /// values first; nothing where no rectangle showed a map of this kind.
        /// </summary>
        /// <returns>What the provenance lists of the maps, or null where none is written.</returns>
        /// <exception cref="InvalidDataException">A map's values are written twice.</exception>
        internal StoredMapsDocument? Write(string path, Func<List<KeyValuePair<string, PictureMap<T>>>, byte[]> encode)
        {
            if (maps.Count == 0)
                return null;
            var admitted = new Dictionary<PictureMap<T>, string>();
            foreach ((string tag, PictureMap<T> map) in maps)
            {
                if (!admitted.TryAdd(map, tag))
                    throw new InvalidDataException(
                        $"The {kind} maps {admitted[map]} and {tag} hold the same values, and both were written: a map is written once per installation (#1644).");
            }
            return new StoredMapsDocument
            {
                Sha256 = WriteFile(path, encode(maps)),
                Maps = maps.ToDictionary(map => map.Key, map => map.Value.Size, StringComparer.Ordinal),
            };
        }
    }

    /// <summary>One picture the store holds, where it was first shown, and where it lies once packed.</summary>
    /// <param name="content">The picture.</param>
    /// <param name="sheet">The sheet first showing it, with its family.</param>
    /// <param name="frames">The frames first showing it there.</param>
    private sealed class StoredPicture(InstalledPictureContent content, string sheet, string frames)
    {
        /// <summary>The picture, read from the sheet first showing it.</summary>
        internal InstalledPictureContent Content { get; } = content;

        /// <summary>The sheet first showing it, with its family.</summary>
        internal string Sheet { get; } = sheet;

        /// <summary>The frames first showing it there.</summary>
        internal string Frames { get; } = frames;

        /// <summary>The page holding it, once packed.</summary>
        internal int Page { get; set; }

        /// <summary>Its top-left pixel on its page, once packed.</summary>
        internal Position16<ImagePixels> Corner { get; set; }

        /// <summary>Its tag, once packed (#1639).</summary>
        internal string Tag { get; set; } = string.Empty;
    }

    /// <summary>What one frame rectangle shows: a stored picture, where it lies in the rectangle, and the maps the rectangle shows it with.</summary>
    /// <param name="Picture">The stored picture.</param>
    /// <param name="Offset">Where the picture's top-left pixel lies in the rectangle, from the rectangle's top-left pixel (#1646).</param>
    /// <param name="RecolourRegionMap">The tag of its recolour-region map, or null for none.</param>
    /// <param name="BlendMap">The tag of its blend map, or null for none.</param>
    /// <param name="Repeats">Whether the picture is one period the rectangle repeats over its whole area (#1651).</param>
    private sealed record ShownPicture(StoredPicture Picture, Offset16<ImagePixels> Offset, string? RecolourRegionMap, string? BlendMap,
        bool Repeats)
    {
        /// <summary>The reference the rectangle's frames name, once the picture is packed.</summary>
        internal InstalledPictureReferenceDocument Reference => new()
        {
            Tag = Picture.Tag,
            Page = Picture.Page,
            Corner = Picture.Corner,
            Size = Picture.Content.Rectangle.Size,
            Offset = Offset,
            RecolourRegionMap = RecolourRegionMap,
            BlendMap = BlendMap,
            Repeats = Repeats,
        };
    }

    /// <summary>One sheet and what each of its frame rectangles that shows a picture shows.</summary>
    /// <param name="Family">The sheet's family.</param>
    /// <param name="Sheet">The sheet.</param>
    /// <param name="ShownAt">Each frame rectangle showing a picture, with the stored copy and the maps it names.</param>
    private sealed record SheetPictures(InstalledSheetFamily Family, NamedInstalledSheet Sheet, Dictionary<FrameRectangle, ShownPicture> ShownAt);
}
