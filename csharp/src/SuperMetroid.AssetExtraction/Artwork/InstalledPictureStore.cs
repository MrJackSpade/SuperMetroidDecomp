using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>
/// An installation's picture store as the game reads it (#1635), the one place pictures are read: its pages, each decoded
/// when a sheet showing one of its pictures is first drawn, as sheets decoded lazily before (#1430), and each sheet's image
/// assembled from them. One store is open per installation in a process, so every family's sheets read one decoded copy of
/// each page; a store whose provenance manifest changed is opened again. A page or map a player edited in place is used as
/// edited, and fails only where it is decoded, when it does not decode.
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
            manifest.PageSha256 is null || manifest.RecolourRegionSha256 is null || manifest.BlendMapSha256 is null)
            throw new InvalidDataException($"Picture store provenance {provenancePath} does not describe this installation.");
        string content = Path.GetDirectoryName(directory)
            ?? throw new ArgumentException($"Picture store directory {directory} has no installation above it.", nameof(directory));
        // The store's pages are the installation's only pictures (#1636): a picture file anywhere else rejects the installation.
        InstalledPictureFiles.RequireOnlyStorePictures(content);
        var regionFiles = new HashSet<string>(manifest.RecolourRegionSha256.Keys, StringComparer.Ordinal);
        var blendFiles = new HashSet<string>(manifest.BlendMapSha256.Keys, StringComparer.Ordinal);
        pages = new InstalledPage[manifest.PageSha256.Count];
        for (int page = 0; page < pages.Length; page++)
        {
            string file = PictureStoreFiles.PageFile(page);
            if (!manifest.PageSha256.ContainsKey(file))
                throw new InvalidDataException($"Picture store provenance {provenancePath} lists {pages.Length} pages but not {file}.");
            string? regions = regionFiles.Remove(PictureStoreFiles.RecolourRegionFile(page))
                ? Path.Combine(PictureStoreFiles.RecolourRegionDirectory(content), PictureStoreFiles.RecolourRegionFile(page)) : null;
            string? blending = blendFiles.Remove(PictureStoreFiles.BlendMapFileName(page))
                ? Path.Combine(PictureStoreFiles.BlendMapDirectory(content), PictureStoreFiles.BlendMapFileName(page)) : null;
            pages[page] = new InstalledPage(Path.Combine(directory, file), regions, blending);
        }
        if (regionFiles.Count > 0 || blendFiles.Count > 0)
            throw new InvalidDataException(
                $"Picture store provenance {provenancePath} lists maps of no page: {string.Join(", ", regionFiles.Concat(blendFiles).Order(StringComparer.Ordinal))}.");
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
    /// picture to name a rectangle of its size on one of the store's pages; frames sharing one rectangle name one picture.
    /// </summary>
    /// <exception cref="InvalidDataException">A frame leaves the image, names a page the store lacks or a rectangle off its page, or shares a rectangle with a frame naming another picture.</exception>
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
                throw new InvalidDataException($"Sprite sheet {sheet} frame {name} shares rectangle {rectangle} with a frame naming another picture.");
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
        }
    }

    /// <summary>
    /// The image of the sheet <paramref name="sheet"/> whose manifest is <paramref name="document"/>, which
    /// <see cref="RequireHolds"/> accepted: clear, every pixel black, in region zero and not blending, but where each frame's
    /// rectangle holds the store's copy of its picture. It has a recolour-region or blend map only where a page it reads has
    /// one. Each stored picture a frame names must show something (#1636): a picture with no visible pixel and no kept
    /// colour is never installed, so a frame naming one is rejected, checked on the pages the image is assembled from.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// A page or map it reads is malformed, a page changed size since the store was opened, or a frame names a stored picture
    /// that shows nothing.
    /// </exception>
    internal SpriteImagePixels Assemble(string sheet, InstalledSheetDocument document)
    {
        Ensure.NotNull(document);
        KeyValuePair<FrameRectangle, InstalledPictureReferenceDocument>[] shown = [.. document.Frames.Values
            .Where(frame => frame.Picture is not null)
            .Select(frame => KeyValuePair.Create(new FrameRectangle(frame.Corner, frame.Size), Ensure.NotNull(frame.Picture)))
            .DistinctBy(frame => frame.Key)];
        int area = document.Size.X * document.Size.Y;
        bool regions = shown.Any(frame => pages[frame.Value.Page].HasRecolourRegions);
        bool blending = shown.Any(frame => pages[frame.Value.Page].HasBlending);
        var image = new InstalledImagePlanes(new SpriteImagePixels(new RgbaPixel[area], regions ? new int[area] : null,
            blending ? new bool[area] : null), document.Size);
        foreach ((FrameRectangle rectangle, InstalledPictureReferenceDocument picture) in shown)
        {
            var stored = new FrameRectangle(picture.Corner, rectangle.Size);
            InstalledImagePlanes page = pages[picture.Page].Image;
            if (page.IsClear(stored))
                throw new InvalidDataException($"Sprite sheet {sheet} frame {string.Join(", ", FramesAt(document, rectangle))} names rectangle {stored} of "
                    + $"picture-store page {PictureStoreFiles.PageFile(picture.Page)}, which shows nothing: an installed picture shows something.");
            page.CopyTo(stored, image, rectangle.Corner);
        }
        return image.Planes;
    }

    /// <summary>The names of <paramref name="document"/>'s frames that lie at <paramref name="rectangle"/>, in name order.</summary>
    private static IEnumerable<string> FramesAt(InstalledSheetDocument document, FrameRectangle rectangle) =>
        document.Frames.Where(frame => new FrameRectangle(frame.Value.Corner, frame.Value.Size) == rectangle)
            .Select(frame => frame.Key).Order(StringComparer.Ordinal);

    /// <summary>One page of the store: its files, its size, and its pixels and maps, decoded when first read.</summary>
    private sealed class InstalledPage
    {
        /// <summary>The page image's path.</summary>
        private readonly string path;

        /// <summary>The page's recolour-region map's path, or null where it has none.</summary>
        private readonly string? regions;

        /// <summary>The page's blend map's path, or null where it has none.</summary>
        private readonly string? blending;

        /// <summary>The page's pixels and maps, decoded when first read.</summary>
        private readonly Lazy<InstalledImagePlanes> image;

        /// <summary>A page whose image is at <paramref name="path"/>, with the maps at the paths given where it has them.</summary>
        /// <exception cref="InvalidDataException">The page's header is invalid.</exception>
        internal InstalledPage(string path, string? regions, string? blending)
        {
            this.path = path;
            this.regions = regions;
            this.blending = blending;
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

        /// <summary>Whether the page has a recolour-region map.</summary>
        internal bool HasRecolourRegions => regions is not null;

        /// <summary>Whether the page has a blend map.</summary>
        internal bool HasBlending => blending is not null;

        /// <summary>The page's pixels and maps.</summary>
        /// <exception cref="InvalidDataException">The page or a map is malformed, or the page changed size since its header was read.</exception>
        internal InstalledImagePlanes Image => image.Value;

        /// <summary>Decodes the page's image and its maps.</summary>
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
            int[]? recolourRegions = regions is null ? null : RecolourRegionMapFile.Decode(File.ReadAllBytes(regions), size, regions);
            bool[]? blends = blending is null ? null : BlendMapFile.Decode(File.ReadAllBytes(blending), size, blending);
            return new InstalledImagePlanes(new SpriteImagePixels(pixels, recolourRegions, blends), size);
        }
    }
}
