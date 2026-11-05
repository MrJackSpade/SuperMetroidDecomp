using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms the installed sprite sheets (#1392): extraction writes each sheet's composed image and frames, a player's
/// in-place edit of an installed image loads as edited, and a damaged image or a frame manifest missing a frame fails loudly.
/// </summary>
internal static class SpriteSheetFilesVerification
{
    /// <summary>Checks that composing the sheets again reproduces the installed files, then loading, in-place edits and rejections.</summary>
    internal static void VerifySpriteSheetFiles(ISnesAddressSpace bus)
    {
        VerifySpriteSheetRecomposition(bus);
        // A private installation copy, whose installed sheet files are edited and damaged in place below.
        // Loading names the body, pause, intro and ending sheets from the body tables, map presentation, intro and
        // ending compositions installed beside the sheets, so those directories are copied with them.
        using PrivateInstallationCopy copy = InstalledFixtures.CreatePrivateCopyOf(layout => layout.SpriteSheetDirectory,
            layout => layout.SamusBodyDirectory, layout => layout.MapDirectory,
            layout => layout.IntroCinematicDirectory, layout => layout.EndingObjectDirectory);
        string directory = copy.Installation.SpriteSheetDirectory;
        InstalledSpriteSheets installed = SpriteSheetFiles.Load(directory);

        ComposedSpriteSheet composed = EscapeTimerSheetComposer.Compose(bus);
        SpriteSheet timer = installed.Get(SpriteSheetId.EscapeTimer);
        AssertEqual(composed.Size, timer.Image.Size, "the installed timer image has the composed size");
        AssertTrue(timer.Image.Pixels.SequenceEqual(composed.Pixels), "the installed timer image holds the composed pixels");
        AssertTrue(timer.HoldsExactly(EscapeTimerLayoutDefinitions.Glyphs.Select(EscapeTimerComposition.FrameName)),
            "the installed timer sheet holds exactly the timer's frames");

        string image = Path.Combine(directory, SpriteSheetStore.ImageFile(SpriteSheetDefinitions.FileNames[SpriteSheetId.EscapeTimer]));
        byte[] installedImage = File.ReadAllBytes(image);
        RgbaPixel[] edited = [.. composed.Pixels];
        int opaque = Array.FindIndex(edited, pixel => pixel.A != 0);
        edited[opaque] = new RgbaPixel(1, 2, 3, 255);
        using (var pngBuffers = new RgbaPngBuffers())
            File.WriteAllBytes(image, RgbaPng.Encode(composed.Size, edited, pngBuffers));
        InstalledSpriteSheets editedSheets = SpriteSheetFiles.Load(directory);
        AssertEqual(new RgbaPixel(1, 2, 3, 255), editedSheets.Get(SpriteSheetId.EscapeTimer).Image.Pixels[opaque],
            "a player's in-place edit of a sheet image loads as edited");
        AssertTrue(editedSheets.ContentIdentity != installed.ContentIdentity, "an in-place sheet image edit changes the content identity");
        File.WriteAllBytes(image, installedImage);

        string frames = Path.Combine(directory, SpriteSheetFiles.FrameFile(SpriteSheetId.EscapeTimer));
        byte[] installedFrames = File.ReadAllBytes(frames);
        SpriteSheetDocument missing = composed.Manifest with
        {
            Frames = composed.Manifest.Frames.Where(pair => pair.Key != "Digit.9").ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
        };
        File.WriteAllBytes(frames, SpriteSheetManifestWriter.Write(missing));
        AssertThrows<InvalidDataException>(() => SpriteSheetFiles.Load(directory), "a sheet's frame manifest edited to miss a frame fails loudly");
        File.WriteAllBytes(frames, installedFrames);

        byte[] damagedImage = [.. installedImage];
        damagedImage[^20] ^= 1;
        File.WriteAllBytes(image, damagedImage);
        // An installed image is decoded when it is first drawn (#1430), so a damaged one fails then, not at load.
        SpriteSheet damaged = SpriteSheetFiles.Load(directory).Get(SpriteSheetId.EscapeTimer);
        AssertThrows<InvalidDataException>(() => { _ = damaged.Image.Pixels.Length; }, "a damaged installed sheet fails when it is first decoded");

        using (var corrupt = new MemoryStream(damagedImage, writable: false))
            AssertThrows<InvalidDataException>(() => RgbaPng.Read(corrupt, out Extent16<ImagePixels> _), "a PNG chunk failing its CRC is rejected");
        Console.WriteLine("Sprite sheet files: composed timer sheet installed exactly, in-place image edit loaded, missing frame and damaged image rejected.");
    }

    /// <summary>Composes the sheets again into a scratch directory and compares them with the installed files byte for byte.</summary>
    [PrivateInstallation("Checks SpriteSheetFiles.Extract itself: composing every sprite sheet again from the cartridge must reproduce the installed sheet images and frame manifests byte for byte, which only a fresh extraction can show.")]
    private static void VerifySpriteSheetRecomposition(ISnesAddressSpace bus)
    {
        DirectoryInfo scratch = Directory.CreateTempSubdirectory("sm-sprite-sheets-");
        try
        {
            string stock = Path.Combine(scratch.FullName, "stock");
            // The body sheets are named from the body tables installed beside them, the pause sheet from the menus'
            // selector presentation, the intro sheet from the intro's compositions and the ending sheets from the ending's,
            // which extraction reads from the shared installation's content directory.
            SpriteSheetFiles.Extract(bus, RepositoryInstallation.Installation.ContentDirectory, stock, SupportedCartridge.Sha256);
            // Composing the sheets again reproduces the installed files byte for byte.
            string installedDirectory = Path.Combine(RepositoryInstallation.Installation.ContentDirectory, GameInstallationLayout.SpriteSheetDirectoryName);
            string[] installedFiles = [.. Directory.GetFiles(installedDirectory).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal)];
            AssertTrue(installedFiles.SequenceEqual(Directory.GetFiles(stock).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal)),
                "the recomposed sprite sheets are the installed files");
            foreach (string name in installedFiles)
                AssertTrue(File.ReadAllBytes(Path.Combine(installedDirectory, name)).AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(stock, name))),
                    $"sprite sheet file {name} recomposes byte for byte");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    /// <summary>Runs the sprite sheet installation checks against the repository cartridge.</summary>
    [VerificationCommand("--sprite-sheet-files")]
    internal static int SpriteSheetFilesCommand()
    {
        Suite(nameof(VerifySpriteSheetFiles), () => VerifySpriteSheetFiles(
            CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath(RepositoryRomPath))));
        return 0;
    }
}
