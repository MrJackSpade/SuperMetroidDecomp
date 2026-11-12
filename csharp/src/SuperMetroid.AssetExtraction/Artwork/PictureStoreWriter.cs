using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// Writes an installation's picture store (#1635), the one place pictures are written. Every family's sheets are walked in
/// one fixed order: the families as given, each family's sheets by ordinal name, each sheet's frame rectangles by corner Y,
/// then X. The first rectangle showing a picture stores it; every later rectangle showing the same picture
/// (<see cref="InstalledPictureContent"/>), on any sheet, names that copy. A rectangle that shows nothing at all stores
/// nothing. The stored pictures are shelf-packed onto pages in that order (<see cref="PicturePageShelves"/>), every picture
/// admitted to the installation's <see cref="SheetPictureRegistry"/> as its page is written, so a second copy fails the
/// install. The pages and their maps are encoded in parallel, each into its own files, and the provenance manifest and
/// each family's sheet manifests are written after them, so the bytes are the same whatever the thread timing.
/// </summary>
internal static class PictureStoreWriter
{
    /// <summary>Writes the store of <paramref name="families"/> under <paramref name="content"/>, then each family's sheet manifests.</summary>
    /// <param name="content">The content directory the store and the families' directories lie in.</param>
    /// <param name="families">The families, in the order their pictures are written.</param>
    /// <param name="sourceCartridgeSha256">Identity written into every provenance manifest.</param>
    /// <exception cref="ArgumentException">A family's directory does not lie directly under the content directory, or two families share one.</exception>
    /// <exception cref="InvalidDataException">
    /// A sheet is installed twice under one name, a frame leaves its sheet, a picture does not fit a page, or a picture is
    /// written twice.
    /// </exception>
    internal static void Write(string content, IReadOnlyList<InstalledSheetFamily> families, string sourceCartridgeSha256)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        Ensure.NotNull(families);
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(content));
        var stored = new List<StoredPicture>();
        var storedOf = new Dictionary<InstalledPictureContent, StoredPicture>();
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
                placed.Add(new SheetPictures(family, sheet, Store(family, sheet, stored, storedOf)));
            }
        }
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
        WritePages(root, pages, stored, sourceCartridgeSha256);
        foreach (InstalledSheetFamily family in families)
            WriteManifests(family, [.. placed.Where(sheet => ReferenceEquals(sheet.Family, family))], sourceCartridgeSha256);
    }

    /// <summary>
    /// Finds where the store holds the picture of each of <paramref name="sheet"/>'s frame rectangles, storing each picture
    /// no earlier rectangle showed; a rectangle that shows nothing has none.
    /// </summary>
    /// <returns>Each rectangle showing a picture, with the stored copy it names.</returns>
    /// <exception cref="InvalidDataException">A frame leaves the sheet.</exception>
    private static Dictionary<FrameRectangle, StoredPicture> Store(InstalledSheetFamily family, NamedInstalledSheet sheet,
        List<StoredPicture> stored, Dictionary<InstalledPictureContent, StoredPicture> storedOf)
    {
        InstalledSheet installed = Ensure.NotNull(sheet.Sheet);
        InstalledImagePlanes image = installed.Image;
        var shownAt = new Dictionary<FrameRectangle, StoredPicture>();
        foreach (IGrouping<FrameRectangle, string> rectangle in installed.Manifest.Frames
            .GroupBy(frame => new FrameRectangle(frame.Value.Corner, frame.Value.Size), frame => frame.Key)
            .Where(rectangle => !rectangle.Key.IsEmpty)
            .OrderBy(rectangle => rectangle.Key.Corner.Y).ThenBy(rectangle => rectangle.Key.Corner.X)
            .ThenBy(rectangle => rectangle.Key.Size.Y).ThenBy(rectangle => rectangle.Key.Size.X))
        {
            InstalledPictureContent picture;
            try
            {
                picture = new InstalledPictureContent(image, rectangle.Key);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Sheet {family.DirectoryName}/{sheet.Name}: {error.Message}", error);
            }
            if (picture.IsClear)
                continue;
            if (!storedOf.TryGetValue(picture, out StoredPicture? copy))
            {
                copy = new StoredPicture(picture, $"{family.DirectoryName}/{sheet.Name}", string.Join(", ", rectangle.Order(StringComparer.Ordinal)));
                storedOf.Add(picture, copy);
                stored.Add(copy);
            }
            shownAt.Add(rectangle.Key, copy);
        }
        return shownAt;
    }

    /// <summary>
    /// Writes every page of <paramref name="pictures"/> into the store under <paramref name="root"/>, with the maps the
    /// pages need, and the store's provenance manifest; each picture is admitted to the installation's registry first.
    /// </summary>
    /// <exception cref="InvalidDataException">A picture is written twice.</exception>
    private static void WritePages(string root, IReadOnlyList<Extent16<ImagePixels>> pages, List<StoredPicture> pictures, string sourceCartridgeSha256)
    {
        var registry = new SheetPictureRegistry();
        var onPage = new List<StoredPicture>[pages.Count];
        for (int page = 0; page < pages.Count; page++)
            onPage[page] = [];
        foreach (StoredPicture picture in pictures)
        {
            registry.Admit(picture.Content, picture.Sheet, picture.Frames);
            onPage[picture.Page].Add(picture);
        }
        string directory = Path.Combine(root, PictureStoreFiles.DirectoryName);
        Directory.CreateDirectory(directory);
        var written = new WrittenPage[pages.Count];
        // PNG compression is the slow part; each worker reuses one encoder's buffers for every page it encodes, and each
        // page goes to its own files, so the files are the same as writing the pages one by one.
        Parallel.For(0, pages.Count, () => new RgbaPngBuffers(), (page, _, png) =>
        {
            written[page] = WritePage(root, page, pages[page], onPage[page], png);
            return png;
        }, png => png.Dispose());
        var pageHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        var regionHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        var blendHashes = new Dictionary<string, string>(StringComparer.Ordinal);
        for (int page = 0; page < pages.Count; page++)
        {
            pageHashes.Add(PictureStoreFiles.PageFile(page), written[page].Page);
            if (written[page].RecolourRegions is { } regions)
                regionHashes.Add(PictureStoreFiles.RecolourRegionFile(page), regions);
            if (written[page].Blending is { } blending)
                blendHashes.Add(PictureStoreFiles.BlendMapFileName(page), blending);
        }
        Dictionary<string, StoredPictureDocument> tagged = pictures.ToDictionary(picture => picture.Tag,
            picture => new StoredPictureDocument { Page = picture.Page, Corner = picture.Corner, Size = picture.Content.Rectangle.Size },
            StringComparer.Ordinal);
        File.WriteAllBytes(Path.Combine(directory, PictureStoreFiles.ProvenanceFileName), JsonSerializer.SerializeToUtf8Bytes(
            new PictureStoreProvenance(PictureStoreProvenance.FormatVersion, sourceCartridgeSha256, pageHashes, regionHashes, blendHashes, tagged),
            PictureStoreProvenance.JsonOptions));
    }

    /// <summary>
    /// Writes page <paramref name="page"/> of <paramref name="size"/> pixels holding <paramref name="pictures"/>, cleared
    /// elsewhere: its image, and its recolour-region map or blend map where a picture on it needs one.
    /// </summary>
    /// <returns>The hashes of the files written.</returns>
    private static WrittenPage WritePage(string root, int page, Extent16<ImagePixels> size, List<StoredPicture> pictures, RgbaPngBuffers png)
    {
        int area = size.X * size.Y;
        bool regions = pictures.Any(picture => picture.Content.HasRecolourRegions);
        bool blending = pictures.Any(picture => picture.Content.HasBlending);
        var planes = new InstalledImagePlanes(new SpriteImagePixels(new RgbaPixel[area], regions ? new int[area] : null,
            blending ? new bool[area] : null), size);
        foreach (StoredPicture picture in pictures)
            picture.Content.CopyTo(planes, picture.Corner);
        string pageHash = WriteFile(Path.Combine(root, PictureStoreFiles.DirectoryName), PictureStoreFiles.PageFile(page),
            RgbaPng.Encode(size, planes.Planes.Pixels, png));
        string? regionHash = planes.Planes.RecolourRegions is { } map
            ? WriteFile(PictureStoreFiles.RecolourRegionDirectory(root), PictureStoreFiles.RecolourRegionFile(page), RecolourRegionMapFile.Encode(size, map))
            : null;
        string? blendHash = planes.Planes.Blending is { } blends
            ? WriteFile(PictureStoreFiles.BlendMapDirectory(root), PictureStoreFiles.BlendMapFileName(page), BlendMapFile.Encode(size, blends))
            : null;
        return new WrittenPage(pageHash, regionHash, blendHash);
    }

    /// <summary>
    /// Writes each of <paramref name="sheets"/>' manifests, every frame naming where the store holds its picture, and the
    /// family's provenance manifest; the manifests are serialized in parallel and written in the sheets' order.
    /// </summary>
    private static void WriteManifests(InstalledSheetFamily family, SheetPictures[] sheets, string sourceCartridgeSha256)
    {
        var manifests = new byte[sheets.Length][];
        Parallel.For(0, sheets.Length, index => manifests[index] = Manifest(sheets[index]));
        SpriteSheetStore.Write(family.Directory, family.ProvenanceFileName,
            [.. sheets.Select((sheet, index) => KeyValuePair.Create(sheet.Sheet.Name, manifests[index]))], sourceCartridgeSha256);
    }

    /// <summary>The installed manifest of <paramref name="sheet"/>, each frame showing a picture naming the store's copy of it.</summary>
    private static byte[] Manifest(SheetPictures sheet)
    {
        InstalledSheetDocument manifest = sheet.Sheet.Sheet.Manifest;
        Dictionary<string, InstalledSheetFrameDocument> frames = manifest.Frames.ToDictionary(frame => frame.Key,
            frame => sheet.ShownAt.TryGetValue(new FrameRectangle(frame.Value.Corner, frame.Value.Size), out StoredPicture? picture)
                ? frame.Value with { Picture = new InstalledPictureReferenceDocument { Tag = picture.Tag, Page = picture.Page, Corner = picture.Corner } }
                : frame.Value,
            StringComparer.Ordinal);
        return SpriteSheetManifestWriter.Write(manifest with { Frames = frames });
    }

    /// <summary>Writes a new installed file into <paramref name="folder"/>.</summary>
    /// <returns>The file's SHA-256.</returns>
    private static string WriteFile(string folder, string name, byte[] bytes)
    {
        Directory.CreateDirectory(folder);
        using (var file = new FileStream(Path.Combine(folder, name), FileMode.CreateNew, FileAccess.Write))
            file.Write(bytes);
        return Convert.ToHexString(SHA256.HashData(bytes));
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

    /// <summary>One sheet and where the store holds the picture of each of its frame rectangles that shows one.</summary>
    /// <param name="Family">The sheet's family.</param>
    /// <param name="Sheet">The sheet.</param>
    /// <param name="ShownAt">Each frame rectangle showing a picture, with the stored copy it names.</param>
    private sealed record SheetPictures(InstalledSheetFamily Family, NamedInstalledSheet Sheet, Dictionary<FrameRectangle, StoredPicture> ShownAt);

    /// <summary>The hashes of one page's files.</summary>
    /// <param name="Page">The page image's.</param>
    /// <param name="RecolourRegions">Its recolour-region map's, or null where it has none.</param>
    /// <param name="Blending">Its blend map's, or null where it has none.</param>
    private sealed record WrittenPage(string Page, string? RecolourRegions, string? Blending);
}
