using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// An installation's picture store as the game reads it (#1635, #1644), the one place pictures are read: its pages, each
/// decoded when a sheet showing one of its pictures is first drawn, as sheets decoded lazily before (#1430), the maps the
/// pictures are shown with, each kind decoded when a sheet showing a picture with one is first drawn, and each sheet's image
/// assembled from them: every frame's picture from its page, shown with the maps the frame names. One store is open per
/// installation in a process, so every family's sheets read one decoded copy of each page and map; a store whose provenance
/// manifest changed is opened again. A page or map a player edited in place is used as edited, and fails only where it is
/// decoded, when it does not decode.
/// </summary>
internal sealed class InstalledPictureStore
{
    /// <summary>Taken around every read and write of <see cref="opened"/>.</summary>
    private static readonly Lock gate = new();

    /// <summary>The store open for each installation, by the store directory's full path.</summary>
    private static readonly Dictionary<string, InstalledPictureStore> opened = new(StringComparer.Ordinal);

    /// <summary>The provenance manifest's bytes the store was opened from.</summary>
    private readonly byte[] provenance;

    /// <summary>The store's pages, in page order.</summary>
    private readonly InstalledPage[] pages;

    /// <summary>Every stored picture's page and rectangle, by its tag (#1639).</summary>
    private readonly Dictionary<string, StoredPictureDocument> tagged;

    /// <summary>The recolour-region maps the pictures are shown with, or null where the store lists none.</summary>
    private readonly InstalledMaps<int>? recolourRegionMaps;

    /// <summary>The blend maps the pictures are shown with, or null where the store lists none.</summary>
    private readonly InstalledMaps<bool>? blendMaps;

    /// <summary>Opens the store in <paramref name="directory"/> from its provenance manifest, reading each page's size from its header.</summary>
    /// <exception cref="InvalidDataException">The provenance does not describe this installation's store, or a page's header is invalid.</exception>
    private InstalledPictureStore(string directory, byte[] provenance)
    {
        this.provenance = provenance;
        string provenancePath = Path.Combine(directory, PictureStoreFiles.ProvenanceFileName);
        PictureStoreProvenance manifest = JsonAssetDocument.Read<PictureStoreProvenance>(new MemoryStream(provenance, writable: false),
            PictureStoreProvenance.JsonOptions, $"picture store provenance {provenancePath}");
        if (manifest.Version != PictureStoreProvenance.FormatVersion ||
            !string.Equals(manifest.SourceCartridgeSha256, SupportedCartridge.Sha256, StringComparison.OrdinalIgnoreCase) ||
            manifest.PageSha256 is null || manifest.Pictures is null ||
            manifest.RecolourRegionMaps is { Maps: null } || manifest.BlendMaps is { Maps: null })
            throw new InvalidDataException($"Picture store provenance {provenancePath} does not describe this installation.");
        tagged = manifest.Pictures;
        string content = Path.GetDirectoryName(directory)
            ?? throw new ArgumentException($"Picture store directory {directory} has no installation above it.", nameof(directory));
        // The store's pages are the installation's only pictures (#1636): a picture file anywhere else rejects the installation.
        InstalledPictureFiles.RequireOnlyStorePictures(content);
        pages = new InstalledPage[manifest.PageSha256.Count];
        for (int page = 0; page < pages.Length; page++)
        {
            string file = PictureStoreFiles.PageFile(page);
            if (!manifest.PageSha256.ContainsKey(file))
                throw new InvalidDataException($"Picture store provenance {provenancePath} lists {pages.Length} pages but not {file}.");
            pages[page] = new InstalledPage(Path.Combine(directory, file));
        }
        recolourRegionMaps = manifest.RecolourRegionMaps is { } regions
            ? new InstalledMaps<int>(PictureStoreFiles.RecolourRegionMapsFile(content), regions.Maps, RecolourRegionMapFile.Decode, "recolour-region")
            : null;
        blendMaps = manifest.BlendMaps is { } blending
            ? new InstalledMaps<bool>(PictureStoreFiles.BlendMapsFile(content), blending.Maps, BlendMapFile.Decode, "blend")
            : null;
    }

    /// <summary>
    /// The store in <paramref name="directory"/>: the one already open there, unless its provenance manifest has changed
    /// since, when the store is opened again.
    /// </summary>
    /// <exception cref="InvalidDataException">The provenance does not describe this installation's store, or a page's header is invalid.</exception>
    internal static InstalledPictureStore Open(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        string full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(directory));
        byte[] provenance = File.ReadAllBytes(Path.Combine(full, PictureStoreFiles.ProvenanceFileName));
        lock (gate)
        {
            if (opened.TryGetValue(full, out InstalledPictureStore? store) && store.provenance.SequenceEqual(provenance))
                return store;
            store = new InstalledPictureStore(full, provenance);
            opened[full] = store;
            return store;
        }
    }

    /// <summary>
    /// Requires every frame of sheet <paramref name="sheet"/>'s manifest to lie on the sheet's image, and every frame naming a
    /// picture to name a rectangle of its size on one of the store's pages, by the tag the store lists for that rectangle,
    /// and only maps the store lists, of the frame's size; frames sharing one rectangle name one picture with the same maps.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame leaves the image, names a page the store lacks or a rectangle off its page, names a tag or a map the store does not list there or of its size, or shares a rectangle with a frame naming another picture or other maps.</exception>
    internal void RequireHolds(string sheet, InstalledSheetDocument document)
    {
        Ensure.NotNull(document);
        var named = new Dictionary<FrameRectangle, InstalledPictureReferenceDocument?>();
        foreach ((string name, InstalledSheetFrameDocument? frame) in document.Frames ?? throw new InvalidDataException($"Sprite sheet {sheet} has no frame list."))
        {
            if (frame is null)
                throw new InvalidDataException($"Sprite sheet {sheet} frame {name} is null.");
            var rectangle = new FrameRectangle(frame.Corner, frame.Size);
            if (!rectangle.LiesInside(document.Size))
                throw new InvalidDataException($"Sprite sheet {sheet} frame {name} at {rectangle} leaves its {document.Size.X}x{document.Size.Y} image.");
            if (named.TryGetValue(rectangle, out InstalledPictureReferenceDocument? other) && other != frame.Picture)
                throw new InvalidDataException($"Sprite sheet {sheet} frame {name} shares rectangle {rectangle} with a frame naming another picture or other maps.");
            named[rectangle] = frame.Picture;
            if (frame.Picture is not { } picture)
                continue;
            if (rectangle.IsEmpty)
                throw new InvalidDataException($"Sprite sheet {sheet} frame {name} shows nothing but names a picture.");
            if (picture.Page < 0 || picture.Page >= pages.Length)
                throw new InvalidDataException($"Sprite sheet {sheet} frame {name} names picture-store page {picture.Page}, which the store lacks.");
            var stored = new FrameRectangle(picture.Corner, frame.Size);
            if (!stored.LiesInside(pages[picture.Page].Size))
                throw new InvalidDataException(
                    $"Sprite sheet {sheet} frame {name} names rectangle {stored} of picture-store page {PictureStoreFiles.PageFile(picture.Page)}, which lies off the page.");
            // Every stored picture carries its tag (#1639): the tag must be one the store lists, at this rectangle.
            if (string.IsNullOrEmpty(picture.Tag) || !tagged.TryGetValue(picture.Tag, out StoredPictureDocument? listed))
                throw new InvalidDataException($"Sprite sheet {sheet} frame {name} names picture tag '{picture.Tag}', which the picture store does not list.");
            if (listed.Page != picture.Page || listed.Corner != picture.Corner || listed.Size != frame.Size)
                throw new InvalidDataException(
                    $"Sprite sheet {sheet} frame {name} names picture {picture.Tag} at {stored} of page {picture.Page}, but the store lists it at " +
                    $"{new FrameRectangle(listed.Corner, listed.Size)} of page {listed.Page}.");
            RequireListed(recolourRegionMaps, picture.RecolourRegionMap, "recolour-region", sheet, name, frame.Size);
            RequireListed(blendMaps, picture.BlendMap, "blend", sheet, name, frame.Size);
        }
    }

    /// <summary>
    /// Requires the map tagged <paramref name="tag"/>, which frame <paramref name="frame"/> of sheet <paramref name="sheet"/>
    /// shows its picture with, to be one of <paramref name="maps"/>, the store's maps of its kind, of the frame's size;
    /// nothing for a frame naming no such map.
    /// </summary>
    /// <exception cref="InvalidDataException">The store lists no such map, or lists it of another size.</exception>
    private static void RequireListed<T>(InstalledMaps<T>? maps, string? tag, string kind, string sheet, string frame, Extent16<ImagePixels> size)
        where T : unmanaged, IEquatable<T>
    {
        if (tag is null)
            return;
        if (maps is null || !maps.Listed.TryGetValue(tag, out Extent16<ImagePixels> listed))
            throw new InvalidDataException($"Sprite sheet {sheet} frame {frame} names {kind} map '{tag}', which the picture store does not list.");
        if (listed != size)
            throw new InvalidDataException(
                $"Sprite sheet {sheet} frame {frame} names {kind} map {tag}, which the store lists for a {listed.X}x{listed.Y} picture, not {size.X}x{size.Y}.");
    }

    /// <summary>
    /// The image of the sheet <paramref name="sheet"/> whose manifest is <paramref name="document"/>, which
    /// <see cref="RequireHolds"/> accepted: clear, every pixel black, in region zero and not blending, but where each frame's
    /// rectangle holds the pixels of the store's copy of its picture, shown with the maps the frame names (#1644). It has a
    /// recolour-region or blend map only where a frame names one. Each stored picture a frame names must show something
    /// (#1636): a picture with no visible pixel and no kept colour is never installed, so a frame naming one is rejected,
    /// checked on the pages the image is assembled from.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A page or map it reads is malformed, a page changed size since the store was opened, a map file disagrees with the
    /// store's listing, or a frame names a stored picture that shows nothing.
    /// </exception>
    internal SpriteImagePixels Assemble(string sheet, InstalledSheetDocument document)
    {
        Ensure.NotNull(document);
        KeyValuePair<FrameRectangle, InstalledPictureReferenceDocument>[] shown = [.. document.Frames.Values
            .Where(frame => frame.Picture is not null)
            .Select(frame => KeyValuePair.Create(new FrameRectangle(frame.Corner, frame.Size), Ensure.NotNull(frame.Picture)))
            .DistinctBy(frame => frame.Key)];
        int area = document.Size.X * document.Size.Y;
        int[]? regions = shown.Any(frame => frame.Value.RecolourRegionMap is not null) ? new int[area] : null;
        bool[]? blending = shown.Any(frame => frame.Value.BlendMap is not null) ? new bool[area] : null;
        var image = new InstalledImagePlanes(new SpriteImagePixels(new RgbaPixel[area], regions, blending), document.Size);
        foreach ((FrameRectangle rectangle, InstalledPictureReferenceDocument picture) in shown)
        {
            var stored = new FrameRectangle(picture.Corner, rectangle.Size);
            InstalledImagePlanes page = pages[picture.Page].Image;
            if (page.IsClear(stored))
                throw new InvalidDataException($"Sprite sheet {sheet} frame {string.Join(", ", FramesAt(document, rectangle))} names rectangle {stored} of "
                    + $"picture-store page {PictureStoreFiles.PageFile(picture.Page)}, which shows nothing: an installed picture shows something.");
            page.CopyPixelsTo(stored, image, rectangle.Corner);
            if (picture.RecolourRegionMap is { } regionMap)
                MapOf(recolourRegionMaps, regionMap, "recolour-region").CopyTo(Ensure.NotNull(regions), document.Size, rectangle);
            if (picture.BlendMap is { } blendMap)
                MapOf(blendMaps, blendMap, "blend").CopyTo(Ensure.NotNull(blending), document.Size, rectangle);
        }
        return image.Planes;
    }

    /// <summary>The map tagged <paramref name="tag"/> among <paramref name="maps"/>, the store's maps of its kind.</summary>
    /// <exception cref="InvalidDataException">The store has no maps of this kind, or the map file is malformed or disagrees with the listing.</exception>
    private static PictureMap<T> MapOf<T>(InstalledMaps<T>? maps, string tag, string kind)
        where T : unmanaged, IEquatable<T> =>
        (maps ?? throw new InvalidDataException($"A frame names {kind} map {tag}, but the picture store lists no {kind} maps.")).Map(tag);

    /// <summary>The names of <paramref name="document"/>'s frames that lie at <paramref name="rectangle"/>, in name order.</summary>
    private static IEnumerable<string> FramesAt(InstalledSheetDocument document, FrameRectangle rectangle) =>
        document.Frames.Where(frame => new FrameRectangle(frame.Value.Corner, frame.Value.Size) == rectangle)
            .Select(frame => frame.Key).Order(StringComparer.Ordinal);

    /// <summary>One page of the store: its file, its size, and its pixels, decoded when first read.</summary>
    private sealed class InstalledPage
    {
        /// <summary>The page image's path.</summary>
        private readonly string path;

        /// <summary>The page's pixels, decoded when first read.</summary>
        private readonly Lazy<InstalledImagePlanes> image;

        /// <summary>A page whose image is at <paramref name="path"/>.</summary>
        /// <exception cref="InvalidDataException">The page's header is invalid.</exception>
        internal InstalledPage(string path)
        {
            this.path = path;
            try
            {
                using FileStream header = File.OpenRead(path);
                Size = RgbaPng.ReadSize(header);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Picture-store page {path}: {error.Message}", error);
            }
            image = new Lazy<InstalledImagePlanes>(Decode);
        }

        /// <summary>The page's width and height, as its header names them.</summary>
        internal Extent16<ImagePixels> Size { get; }

        /// <summary>The page's pixels; a page has no maps, which are the frames' (#1644).</summary>
        /// <exception cref="InvalidDataException">The page is malformed, or changed size since its header was read.</exception>
        internal InstalledImagePlanes Image => image.Value;

        /// <summary>Decodes the page's image.</summary>
        private InstalledImagePlanes Decode()
        {
            RgbaPixel[] pixels;
            Extent16<ImagePixels> size;
            try
            {
                pixels = RgbaPng.Read(new MemoryStream(File.ReadAllBytes(path), writable: false), out size);
            }
            catch (InvalidDataException error)
            {
                throw new InvalidDataException($"Picture-store page {path}: {error.Message}", error);
            }
            if (size != Size)
                throw new InvalidDataException($"Picture-store page {path} changed size after its header was read.");
            return new InstalledImagePlanes(new SpriteImagePixels(pixels, RecolourRegions: null, Blending: null), size);
        }
    }

    /// <summary>
    /// One kind of map the store's pictures are shown with (#1644): the maps the provenance lists, by tag with their sizes,
    /// and the map file holding them, decoded when a map is first read.
    /// </summary>
    /// <typeparam name="T">The value each pixel of a map holds.</typeparam>
    private sealed class InstalledMaps<T>
        where T : unmanaged, IEquatable<T>
    {
        /// <summary>The map file's path.</summary>
        private readonly string path;

        /// <summary>Names the kind of map in a failure.</summary>
        private readonly string kind;

        /// <summary>Every map, decoded from the file when first read and checked against the listing.</summary>
        private readonly Lazy<Dictionary<string, PictureMap<T>>> maps;

        /// <summary>The maps <paramref name="listed"/> names, which <paramref name="decode"/> reads from the file at <paramref name="path"/>.</summary>
        internal InstalledMaps(string path, Dictionary<string, Extent16<ImagePixels>> listed,
            Func<byte[], string, Dictionary<string, PictureMap<T>>> decode, string kind)
        {
            this.path = path;
            this.kind = kind;
            Listed = Ensure.NotNull(listed);
            maps = new Lazy<Dictionary<string, PictureMap<T>>>(() => Checked(decode(File.ReadAllBytes(path), path)));
        }

        /// <summary>The size of each map the store lists, by tag.</summary>
        internal Dictionary<string, Extent16<ImagePixels>> Listed { get; }

        /// <summary>The map tagged <paramref name="tag"/>.</summary>
        /// <exception cref="InvalidDataException">The map file is malformed, disagrees with the listing, or holds no such map.</exception>
        internal PictureMap<T> Map(string tag) => maps.Value.TryGetValue(tag, out PictureMap<T>? map) ? map
            : throw new InvalidDataException($"The picture store's {kind} maps {path} hold no map {tag}.");

        /// <summary>Requires <paramref name="decoded"/> to hold exactly the listed maps, each of its listed size.</summary>
        /// <exception cref="InvalidDataException">A listed map is missing or of another size, or an unlisted map is held.</exception>
        private Dictionary<string, PictureMap<T>> Checked(Dictionary<string, PictureMap<T>> decoded)
        {
            foreach ((string tag, Extent16<ImagePixels> size) in Listed)
            {
                if (!decoded.TryGetValue(tag, out PictureMap<T>? map))
                    throw new InvalidDataException($"The picture store's {kind} maps {path} hold no map {tag}, which its provenance lists.");
                if (map.Size != size)
                    throw new InvalidDataException(
                        $"The picture store's {kind} map {tag} in {path} is {map.Size.X}x{map.Size.Y}, but its provenance lists it as {size.X}x{size.Y}.");
            }
            string[] unlisted = [.. decoded.Keys.Where(tag => !Listed.ContainsKey(tag)).Order(StringComparer.Ordinal)];
            if (unlisted.Length > 0)
                throw new InvalidDataException($"The picture store's {kind} maps {path} hold maps its provenance does not list: {string.Join(", ", unlisted)}.");
            return decoded;
        }
    }
}
