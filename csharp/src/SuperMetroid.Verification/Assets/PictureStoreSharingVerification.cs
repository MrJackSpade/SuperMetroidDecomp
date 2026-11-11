using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms that the installation stores a picture two sheets show once (#1635): the energy tank's message box is the same
/// picture in the Ceres rooms' message boxes and in the refill rooms', so both sheets' manifests name one rectangle of one
/// picture store page for it, and both loaded sheets show that rectangle's pixels.
/// </summary>
internal static class PictureStoreSharingVerification
{
    /// <summary>
    /// Fails unless the Ceres and refill message box sheets name the same stored picture for the energy tank's message box
    /// and both loaded sheets show its stored pixels.
    /// </summary>
    internal static void VerifySharedPictureStoredOnce()
    {
        GameInstallation installation = RepositoryInstallation.Installation;
        var energyTank = new ShownMessage(GameplayMessageId.EnergyTank, null, null);
        MessageBoxSheet ceres = MessagesOf(RoomGraphicsSet.BlueCeres);
        MessageBoxSheet refill = MessagesOf(RoomGraphicsSet.BlueRefill);
        InstalledPictureReferenceDocument stored = StoredPictureOf(installation, ceres, energyTank.FrameName);
        InstalledPictureReferenceDocument alsoStored = StoredPictureOf(installation, refill, energyTank.FrameName);
        AssertEqual(stored.Page, alsoStored.Page, $"{ceres.Name} and {refill.Name} store their {energyTank.FrameName} picture on one page");
        AssertEqual(stored.Corner, alsoStored.Corner, $"{ceres.Name} and {refill.Name} store their {energyTank.FrameName} picture in one rectangle");

        string pagePath = Path.Combine(installation.PictureStoreDirectory, PictureStoreFiles.PageFile(stored.Page));
        RgbaPixel[] page;
        Extent16<ImagePixels> pageSize;
        using (FileStream file = File.OpenRead(pagePath))
            page = RgbaPng.Read(file, out pageSize);
        foreach (MessageBoxSheet messages in new[] { ceres, refill })
        {
            SpriteSheetFrame shown = messages.Picture(energyTank);
            AssertTrue(!shown.IsEmpty, $"loaded {messages.Name} pictures its {energyTank.FrameName} message box");
            AssertTrue(stored.Corner.X + shown.Size.X <= pageSize.X && stored.Corner.Y + shown.Size.Y <= pageSize.Y,
                $"{messages.Name}'s {energyTank.FrameName} picture lies on store page {pagePath}");
            AssertTrue(ShowsStoredPixels(shown, page, pageSize, stored.Corner),
                $"loaded {messages.Name} shows the stored {energyTank.FrameName} picture's pixels");
        }
        Console.WriteLine($"Picture store: {ceres.Name} and {refill.Name} show the {energyTank.FrameName} picture stored once, "
            + $"at ({stored.Corner.X}, {stored.Corner.Y}) of {PictureStoreFiles.PageFile(stored.Page)}.");
    }

    /// <summary>The installed message box sheet of the tileset of <paramref name="graphicsSet"/>.</summary>
    private static MessageBoxSheet MessagesOf(RoomGraphicsSet graphicsSet) =>
        InstalledFixtures.RoomAssets.BlockSheets.MessageBoxes.Values.Single(messages => messages.Tileset.GraphicsSet == graphicsSet);

    /// <summary>Where the installed manifest of <paramref name="messages"/> says frame <paramref name="frame"/>'s picture is stored.</summary>
    /// <exception cref="InvalidOperationException">The manifest lists no such frame, or names no stored picture for it.</exception>
    private static InstalledPictureReferenceDocument StoredPictureOf(GameInstallation installation, MessageBoxSheet messages, string frame)
    {
        InstalledSheetDocument document;
        using (FileStream manifest = File.OpenRead(Path.Combine(installation.RoomBlockSheetDirectory, messages.Name + ".json")))
            document = SpriteSheetFile.ReadDocument(manifest, messages.Name);
        InstalledSheetFrameDocument entry = document.Frames.TryGetValue(frame, out InstalledSheetFrameDocument? listed) ? listed
            : throw new InvalidOperationException($"Verification failed: {messages.Name}'s manifest lists no frame {frame}.");
        return entry.Picture
            ?? throw new InvalidOperationException($"Verification failed: {messages.Name}'s manifest names no stored picture for frame {frame}.");
    }

    /// <summary>
    /// Whether every pixel of <paramref name="shown"/> is the pixel of the stored rectangle at <paramref name="corner"/> of
    /// <paramref name="page"/>, a store page of <paramref name="pageSize"/> pixels.
    /// </summary>
    private static bool ShowsStoredPixels(SpriteSheetFrame shown, RgbaPixel[] page, Extent16<ImagePixels> pageSize,
        Position16<ImagePixels> corner)
    {
        ReadOnlySpan<RgbaPixel> pixels = shown.Sheet.Pixels;
        int width = shown.Sheet.Size.X;
        for (int row = 0; row < shown.Size.Y; row++)
        {
            int sheetLine = (shown.Corner.Y + row) * width + shown.Corner.X;
            int pageLine = (corner.Y + row) * pageSize.X + corner.X;
            if (!pixels.Slice(sheetLine, shown.Size.X).SequenceEqual(page.AsSpan(pageLine, shown.Size.X)))
                return false;
        }
        return true;
    }

    /// <summary>Runs the picture store's shared-picture confirmation.</summary>
    [VerificationCommand("--picture-store-sharing")]
    internal static int PictureStoreSharingCommand()
    {
        Suite(nameof(VerifySharedPictureStoredOnce), VerifySharedPictureStoredOnce);
        return 0;
    }
}
