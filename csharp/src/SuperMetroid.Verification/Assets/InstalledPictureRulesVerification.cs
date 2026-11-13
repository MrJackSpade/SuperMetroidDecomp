using System.Security.Cryptography;
using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms #1636's load-time rules for installed pictures on synthetic installations: a picture file outside the picture
/// store rejects the installation, naming the file, and a sheet frame naming a stored picture that shows nothing is rejected
/// as its sheet is assembled from the store's pages, naming the frame.
/// </summary>
internal static class InstalledPictureRulesVerification
{
    /// <summary>A stray character page's directory under the synthetic content.</summary>
    private const string StrayDirectory = "room-characters";

    /// <summary>A stray character page's file name.</summary>
    private const string StrayFile = "room-characters-crateria.png";

    /// <summary>
    /// Content whose only picture file is a store page passes; the same content with a character page beside the store is
    /// rejected, naming the page.
    /// </summary>
    internal static void VerifyStrayPictureFileRejected()
    {
        using var scratch = new TestScratchDirectory("stray-installed-picture");
        string store = Path.Combine(scratch.Root, PictureStoreFiles.DirectoryName);
        Directory.CreateDirectory(store);
        File.WriteAllBytes(Path.Combine(store, PictureStoreFiles.PageFile(0)), ClearPage());
        InstalledPictureFiles.RequireOnlyStorePictures(scratch.Root);

        Directory.CreateDirectory(Path.Combine(scratch.Root, StrayDirectory));
        File.WriteAllBytes(Path.Combine(scratch.Root, StrayDirectory, StrayFile), ClearPage());
        InvalidDataException error = AssertThrows<InvalidDataException>(() => InstalledPictureFiles.RequireOnlyStorePictures(scratch.Root),
            "a picture file outside the picture store rejects the installation");
        string named = Path.Combine(StrayDirectory, StrayFile);
        AssertTrue(error.Message.Contains(named, StringComparison.Ordinal), $"the rejection names the stray picture {named}: {error.Message}");
        Console.WriteLine("Installed picture files: a picture file outside the picture store rejects the installation, naming it.");
    }

    /// <summary>
    /// A store whose page holds only clear pixels, read by a sheet frame naming that page's rectangle, is rejected as the
    /// sheet's image is assembled, naming the sheet's frame and the page.
    /// </summary>
    internal static void VerifyEmptyStoredPictureRejected()
    {
        using var scratch = new TestScratchDirectory("empty-stored-picture");
        string store = Path.Combine(scratch.Root, PictureStoreFiles.DirectoryName);
        Directory.CreateDirectory(store);
        byte[] page = ClearPage();
        string pageFile = PictureStoreFiles.PageFile(0);
        File.WriteAllBytes(Path.Combine(store, pageFile), page);
        File.WriteAllBytes(Path.Combine(store, PictureStoreFiles.IndexFileName), JsonSerializer.SerializeToUtf8Bytes(
            new PictureStoreIndex(PictureStoreIndex.FormatVersion, PageCount: 1,
                new Dictionary<string, StoredPictureDocument>(StringComparer.Ordinal)
                {
                    [EmptyTag] = new StoredPictureDocument { Page = 0, Corner = default, Size = PageSize },
                },
                RecolourRegionMaps: null, BlendMaps: null),
            PictureStoreIndex.JsonOptions));
        InstalledPictureStore pictures = InstalledPictureStore.Open(store);
        var document = new InstalledSheetDocument
        {
            Version = SpriteSheetFile.Version,
            Size = PageSize,
            Frames = new Dictionary<string, InstalledSheetFrameDocument>(StringComparer.Ordinal)
            {
                ["empty frame"] = new InstalledSheetFrameDocument
                {
                    Corner = default,
                    Size = PageSize,
                    Origin = default,
                    Picture = new InstalledPictureReferenceDocument { Tag = EmptyTag, Page = 0, Corner = default, Size = PageSize },
                },
            },
        };
        pictures.RequireHolds("empty-picture-fixture", document);
        InvalidDataException error = AssertThrows<InvalidDataException>(() => pictures.Assemble("empty-picture-fixture", document),
            "a frame naming a stored picture that shows nothing is rejected");
        AssertTrue(error.Message.Contains("empty frame", StringComparison.Ordinal) && error.Message.Contains(pageFile, StringComparison.Ordinal),
            $"the rejection names the frame and the page {pageFile}: {error.Message}");
        Console.WriteLine("Empty stored picture: a frame naming a stored picture that shows nothing is rejected as its sheet is assembled.");
    }

    /// <summary>The tag the synthetic store lists for its one picture.</summary>
    private const string EmptyTag = "P0-0000";

    /// <summary>
    /// A frame naming a tag the store does not list is rejected, naming the frame and the tag (#1639): every installed
    /// picture carries the tag its store lists.
    /// </summary>
    internal static void VerifyUnlistedPictureTagRejected()
    {
        using var scratch = new TestScratchDirectory("unlisted-picture-tag");
        string store = Path.Combine(scratch.Root, PictureStoreFiles.DirectoryName);
        Directory.CreateDirectory(store);
        byte[] page = ClearPage();
        string pageFile = PictureStoreFiles.PageFile(0);
        File.WriteAllBytes(Path.Combine(store, pageFile), page);
        File.WriteAllBytes(Path.Combine(store, PictureStoreFiles.IndexFileName), JsonSerializer.SerializeToUtf8Bytes(
            new PictureStoreIndex(PictureStoreIndex.FormatVersion, PageCount: 1,
                new Dictionary<string, StoredPictureDocument>(StringComparer.Ordinal)
                {
                    [EmptyTag] = new StoredPictureDocument { Page = 0, Corner = default, Size = PageSize },
                },
                RecolourRegionMaps: null, BlendMaps: null),
            PictureStoreIndex.JsonOptions));
        InstalledPictureStore pictures = InstalledPictureStore.Open(store);
        const string unlisted = "P0-0001";
        var document = new InstalledSheetDocument
        {
            Version = SpriteSheetFile.Version,
            Size = PageSize,
            Frames = new Dictionary<string, InstalledSheetFrameDocument>(StringComparer.Ordinal)
            {
                ["unlisted frame"] = new InstalledSheetFrameDocument
                {
                    Corner = default,
                    Size = PageSize,
                    Origin = default,
                    Picture = new InstalledPictureReferenceDocument { Tag = unlisted, Page = 0, Corner = default, Size = PageSize },
                },
            },
        };
        InvalidDataException error = AssertThrows<InvalidDataException>(() => pictures.RequireHolds("unlisted-tag-fixture", document),
            "a frame naming a tag the store does not list is rejected");
        AssertTrue(error.Message.Contains("unlisted frame", StringComparison.Ordinal) && error.Message.Contains(unlisted, StringComparison.Ordinal),
            $"the rejection names the frame and the tag {unlisted}: {error.Message}");
        Console.WriteLine("Picture tags: a frame naming a tag the picture store does not list is rejected.");
    }

    /// <summary>The synthetic store page's size.</summary>
    private static Extent16<ImagePixels> PageSize => new(16, 16);

    /// <summary>A store page of <see cref="PageSize"/> whose every pixel is clear black.</summary>
    private static byte[] ClearPage()
    {
        using var buffers = new RgbaPngBuffers();
        return RgbaPng.Encode(PageSize, new RgbaPixel[PageSize.X * PageSize.Y], buffers);
    }

    /// <summary>Runs #1636's installed-picture load rules.</summary>
    [VerificationCommand("--installed-picture-rules")]
    internal static int InstalledPictureRulesCommand()
    {
        Suite(nameof(VerifyStrayPictureFileRejected), VerifyStrayPictureFileRejected);
        Suite(nameof(VerifyEmptyStoredPictureRejected), VerifyEmptyStoredPictureRejected);
        Suite(nameof(VerifyUnlistedPictureTagRejected), VerifyUnlistedPictureTagRejected);
        return 0;
    }
}
