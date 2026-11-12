using System.Runtime.InteropServices;
using System.Security.Cryptography;
using SuperMetroid.AssetExtraction;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms over the installation that a stored picture is its pixels only, stored once (#1644): no two pictures the store
/// holds have identical RGBA pixels; every frame naming a picture resolves to a picture the store lists at the rectangle it
/// names and to the recolour-region and blend maps it names, each of the frame's size; and pictures whose pixels were stored
/// twice before, because frames showed them with different maps, are one stored picture the frames show through their own
/// maps. The examples are found by content: frames naming one stored picture with different recolour-region maps and the
/// same blending, and frames naming one stored picture with different blend maps. Each example's two sheets are assembled
/// by the store as the game loads them, and must show the stored pixels in both frames, each with the maps its frame names.
/// </summary>
internal static class PictureStorePixelIdentityVerification
{
    /// <summary>
    /// Fails unless the store holds every picture's pixels once, every reference resolves to its picture and maps, and a
    /// picture shown with different recolour-region maps and one shown with different blend maps each show the same stored
    /// pixels through their frames' own maps.
    /// </summary>
    internal static void VerifyStoredPicturesArePixelsOnly()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        string content = installation.ContentDirectory;
        string storeDirectory = installation.PictureStoreDirectory;
        string provenancePath = Path.Combine(storeDirectory, PictureStoreFiles.ProvenanceFileName);
        PictureStoreProvenance provenance;
        using (FileStream file = File.OpenRead(provenancePath))
            provenance = JsonAssetDocument.Read<PictureStoreProvenance>(file, PictureStoreProvenance.JsonOptions, $"picture store provenance {provenancePath}");
        StorePage[] pages = [.. Enumerable.Range(0, provenance.PageSha256.Count).Select(page => StorePage.Read(storeDirectory, page))];

        string[] duplicates = DuplicatePixels(provenance, pages);
        AssertTrue(duplicates.Length == 0,
            $"{duplicates.Length} stored pictures hold the pixels of another:{Environment.NewLine}{string.Join(Environment.NewLine, duplicates)}");

        StoredMapsDocument regionListing = provenance.RecolourRegionMaps
            ?? throw new InvalidOperationException("Verification failed: the picture store lists no recolour-region maps.");
        StoredMapsDocument blendListing = provenance.BlendMaps
            ?? throw new InvalidOperationException("Verification failed: the picture store lists no blend maps.");
        string regionPath = PictureStoreFiles.RecolourRegionMapsFile(content);
        string blendPath = PictureStoreFiles.BlendMapsFile(content);
        Dictionary<string, PictureMap<int>> regionMaps = RecolourRegionMapFile.Decode(File.ReadAllBytes(regionPath), regionPath);
        Dictionary<string, PictureMap<bool>> blendMaps = BlendMapFile.Decode(File.ReadAllBytes(blendPath), blendPath);
        AssertListed(regionListing, regionMaps, "recolour-region");
        AssertListed(blendListing, blendMaps, "blend");

        var references = new List<ShownReference>();
        var unresolved = new List<string>();
        foreach ((string family, string sheet, InstalledSheetDocument document) in InstalledSheetManifests.Read(content))
        {
            foreach ((string frame, InstalledSheetFrameDocument entry) in document.Frames.OrderBy(entry => entry.Key, StringComparer.Ordinal))
            {
                if (entry.Picture is not { } picture)
                    continue;
                var shown = new ShownReference(family, sheet, frame, document, picture, new FrameRectangle(entry.Corner, entry.Size));
                references.Add(shown);
                unresolved.AddRange(Unresolved(shown, provenance, regionMaps, blendMaps));
            }
        }
        AssertTrue(references.Count > 0, "the installation's sheet frames name stored pictures");
        AssertTrue(unresolved.Count == 0,
            $"{unresolved.Count} picture references do not resolve:{Environment.NewLine}{string.Join(Environment.NewLine, unresolved)}");

        IGrouping<string, ShownReference>[] byPicture = [.. references.GroupBy(reference => reference.Picture.Tag, StringComparer.Ordinal)];
        (ShownReference First, ShownReference Second)? regionExample = FirstPair(byPicture, (first, second) =>
            first.Picture.RecolourRegionMap != second.Picture.RecolourRegionMap && first.Picture.BlendMap == second.Picture.BlendMap);
        (ShownReference First, ShownReference Second)? blendExample = FirstPair(byPicture, (first, second) =>
            first.Picture.BlendMap != second.Picture.BlendMap);
        AssertTrue(regionExample is not null, "two frames show one stored picture with different recolour-region maps");
        AssertTrue(blendExample is not null, "two frames show one stored picture with different blend maps");

        InstalledPictureStore store = InstalledPictureStore.Open(storeDirectory);
        foreach ((ShownReference first, ShownReference second) in new[] { regionExample!.Value, blendExample!.Value })
        {
            StoredPictureDocument listed = provenance.Pictures[first.Picture.Tag];
            RgbaPixel[] stored = pages[listed.Page].PixelsOf(listed);
            foreach (ShownReference reference in new[] { first, second })
            {
                SpriteImagePixels assembled = store.Assemble(reference.Sheet, reference.Document);
                var image = new InstalledImagePlanes(assembled, reference.Document.Size);
                AssertTrue(PixelsAt(image, reference.Rectangle).AsSpan().SequenceEqual(stored),
                    $"{reference} shows the pixels of stored picture {first.Picture.Tag}");
                AssertTrue(Equals(PictureMap<int>.Within(assembled.RecolourRegions, image.Size, reference.Rectangle),
                        reference.Picture.RecolourRegionMap is { } region ? regionMaps[region] : null),
                    $"{reference} shows {first.Picture.Tag} with recolour-region map {reference.Picture.RecolourRegionMap ?? "none"}");
                AssertTrue(Equals(PictureMap<bool>.Within(assembled.Blending, image.Size, reference.Rectangle),
                        reference.Picture.BlendMap is { } blend ? blendMaps[blend] : null),
                    $"{reference} shows {first.Picture.Tag} with blend map {reference.Picture.BlendMap ?? "none"}");
            }
            Console.WriteLine($"Picture store: {first} and {second} show stored picture {first.Picture.Tag} through their own maps "
                + $"(recolour regions {first.Picture.RecolourRegionMap ?? "none"} and {second.Picture.RecolourRegionMap ?? "none"}, "
                + $"blending {first.Picture.BlendMap ?? "none"} and {second.Picture.BlendMap ?? "none"}).");
        }
        Console.WriteLine($"Picture store: {provenance.Pictures.Count} pictures each hold pixels no other holds; {references.Count} frame "
            + $"references resolve to them and to {regionMaps.Count} recolour-region and {blendMaps.Count} blend maps.");
    }

    /// <summary>Each stored picture whose pixels another stored picture also holds, named by both tags.</summary>
    private static string[] DuplicatePixels(PictureStoreProvenance provenance, StorePage[] pages)
    {
        var byHash = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach ((string tag, StoredPictureDocument picture) in provenance.Pictures)
        {
            RgbaPixel[] pixels = pages[picture.Page].PixelsOf(picture);
            string hash = $"{picture.Size.X}x{picture.Size.Y}:{Convert.ToHexString(SHA256.HashData(MemoryMarshal.AsBytes(pixels.AsSpan())))}";
            if (!byHash.TryGetValue(hash, out List<string>? tags))
                byHash.Add(hash, tags = []);
            tags.Add(tag);
        }
        var duplicates = new List<string>();
        foreach (List<string> tags in byHash.Values.Where(tags => tags.Count > 1))
        {
            // The hash only narrows the comparison: two pictures are one only when every pixel matches.
            for (int later = 1; later < tags.Count; later++)
            {
                StoredPictureDocument picture = provenance.Pictures[tags[later]];
                RgbaPixel[] pixels = pages[picture.Page].PixelsOf(picture);
                for (int earlier = 0; earlier < later; earlier++)
                {
                    StoredPictureDocument other = provenance.Pictures[tags[earlier]];
                    if (pages[other.Page].PixelsOf(other).AsSpan().SequenceEqual(pixels))
                        duplicates.Add($"{tags[later]} holds the pixels of {tags[earlier]}");
                }
            }
        }
        return [.. duplicates];
    }

    /// <summary>Fails unless the map file holds exactly the maps the provenance lists, each of its listed size.</summary>
    private static void AssertListed<T>(StoredMapsDocument listing, Dictionary<string, PictureMap<T>> maps, string kind)
        where T : unmanaged, IEquatable<T>
    {
        AssertTrue(maps.Count > 0, $"the picture store holds {kind} maps");
        AssertTrue(listing.Maps.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(maps.Keys),
            $"the {kind} map file holds exactly the maps the store's provenance lists");
        string[] misSized = [.. listing.Maps.Where(entry => maps[entry.Key].Size != entry.Value).Select(entry => entry.Key)];
        AssertTrue(misSized.Length == 0, $"every {kind} map is the size the provenance lists: {string.Join(", ", misSized)}");
        string[] distinct = [.. maps.GroupBy(entry => entry.Value).Where(same => same.Count() > 1).Select(same => string.Join(" = ", same.Select(entry => entry.Key)))];
        AssertTrue(distinct.Length == 0, $"every {kind} map is stored once: {string.Join("; ", distinct)}");
    }

    /// <summary>What <paramref name="reference"/> names that the store does not hold as it names it.</summary>
    private static IEnumerable<string> Unresolved(ShownReference reference, PictureStoreProvenance provenance,
        Dictionary<string, PictureMap<int>> regionMaps, Dictionary<string, PictureMap<bool>> blendMaps)
    {
        InstalledPictureReferenceDocument picture = reference.Picture;
        if (!provenance.Pictures.TryGetValue(picture.Tag, out StoredPictureDocument? listed))
            yield return $"{reference} names picture {picture.Tag}, which the store does not list";
        else if (listed.Page != picture.Page || listed.Corner != picture.Corner || listed.Size != reference.Rectangle.Size)
            yield return $"{reference} names picture {picture.Tag} at another place or size than the store lists it";
        if (picture.RecolourRegionMap is { } region && (!regionMaps.TryGetValue(region, out PictureMap<int>? regions) || regions.Size != reference.Rectangle.Size))
            yield return $"{reference} names recolour-region map {region}, which the store does not hold at the frame's size";
        if (picture.BlendMap is { } blend && (!blendMaps.TryGetValue(blend, out PictureMap<bool>? blending) || blending.Size != reference.Rectangle.Size))
            yield return $"{reference} names blend map {blend}, which the store does not hold at the frame's size";
    }

    /// <summary>
    /// The first two references, in the order the sheets name them, that name one stored picture and that
    /// <paramref name="differ"/> accepts; null where no picture has such a pair.
    /// </summary>
    private static (ShownReference First, ShownReference Second)? FirstPair(IGrouping<string, ShownReference>[] byPicture,
        Func<ShownReference, ShownReference, bool> differ)
    {
        foreach (IGrouping<string, ShownReference> shown in byPicture)
        {
            ShownReference[] references = [.. shown];
            for (int later = 1; later < references.Length; later++)
            {
                for (int earlier = 0; earlier < later; earlier++)
                {
                    if (differ(references[earlier], references[later]))
                        return (references[earlier], references[later]);
                }
            }
        }
        return null;
    }

    /// <summary>The pixels of <paramref name="rectangle"/> of <paramref name="image"/>, in reading order.</summary>
    private static RgbaPixel[] PixelsAt(InstalledImagePlanes image, FrameRectangle rectangle)
    {
        var pixels = new RgbaPixel[rectangle.Size.X * rectangle.Size.Y];
        for (int row = 0; row < rectangle.Size.Y; row++)
        {
            image.Planes.Pixels.AsSpan(((rectangle.Corner.Y + row) * image.Size.X) + rectangle.Corner.X, rectangle.Size.X)
                .CopyTo(pixels.AsSpan(row * rectangle.Size.X, rectangle.Size.X));
        }
        return pixels;
    }

    /// <summary>One page of the store, decoded.</summary>
    /// <param name="Size">The page's width and height.</param>
    /// <param name="Pixels">Its pixels, in reading order.</param>
    private sealed record StorePage(Extent16<ImagePixels> Size, RgbaPixel[] Pixels)
    {
        /// <summary>Decodes page <paramref name="page"/> of the store in <paramref name="directory"/>.</summary>
        internal static StorePage Read(string directory, int page)
        {
            using FileStream file = File.OpenRead(Path.Combine(directory, PictureStoreFiles.PageFile(page)));
            RgbaPixel[] pixels = RgbaPng.Read(file, out Extent16<ImagePixels> size);
            return new StorePage(size, pixels);
        }

        /// <summary>The pixels of <paramref name="picture"/>, which lies on this page, in reading order.</summary>
        internal RgbaPixel[] PixelsOf(StoredPictureDocument picture) =>
            PixelsAt(new InstalledImagePlanes(new SpriteImagePixels(Pixels, RecolourRegions: null, Blending: null), Size),
                new FrameRectangle(picture.Corner, picture.Size));
    }

    /// <summary>One sheet frame's reference to a stored picture.</summary>
    /// <param name="Family">The sheet's family.</param>
    /// <param name="Sheet">The sheet's installed name.</param>
    /// <param name="Frame">The frame's name.</param>
    /// <param name="Document">The sheet's manifest.</param>
    /// <param name="Picture">What the frame names.</param>
    /// <param name="Rectangle">The frame's rectangle on its sheet.</param>
    private sealed record ShownReference(string Family, string Sheet, string Frame, InstalledSheetDocument Document,
        InstalledPictureReferenceDocument Picture, FrameRectangle Rectangle)
    {
        /// <summary>Names the frame by its sheet.</summary>
        public override string ToString() => $"{Family}/{Sheet} frame {Frame}";
    }

    /// <summary>Runs the picture store's pixels-only confirmation.</summary>
    [VerificationCommand("--picture-store-pixel-identity")]
    internal static int PictureStorePixelIdentityCommand()
    {
        Suite(nameof(VerifyStoredPicturesArePixelsOnly), VerifyStoredPicturesArePixelsOnly);
        return 0;
    }
}
