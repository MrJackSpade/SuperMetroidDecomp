using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation;
using static SuperMetroid.Verification.Harness.RepositoryInputsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;

namespace SuperMetroid.Verification.Assets;

/// <summary>
/// Confirms the installed sprite sheets (#1392): extraction writes each sheet's composed image and frames, a player's
/// image or frame override replaces the stock file, and a damaged stock file or a sheet missing a frame fails loudly.
/// </summary>
internal static class SpriteSheetFilesVerification
{
    /// <summary>Extracts the sheets into a scratch directory and checks loading, overrides and rejections.</summary>
    internal static void VerifySpriteSheetFiles(ISnesAddressSpace bus)
    {
        DirectoryInfo scratch = Directory.CreateTempSubdirectory("sm-sprite-sheets-");
        try
        {
            string stock = Path.Combine(scratch.FullName, "stock");
            string overrides = Path.Combine(scratch.FullName, "overrides");
            // The body sheets are named from the body tables installed beside them, the pause sheet from the menus'
            // selector presentation, the intro sheet from the intro's compositions and the ending sheets from the ending's.
            foreach (string beside in new[] { GameInstallationLayout.SamusBodyDirectoryName, GameInstallationLayout.MapDirectoryName,
                         GameInstallationLayout.IntroCinematicDirectoryName, GameInstallationLayout.EndingObjectDirectoryName })
                CopyDirectory(Path.Combine(RepositoryInstallation.Installation.ContentDirectory, beside), Path.Combine(scratch.FullName, beside));
            SpriteSheetFiles.Extract(bus, RepositoryInstallation.Installation.ContentDirectory, stock, SupportedCartridge.Sha256);
            // Composing the sheets again reproduces the installed files byte for byte.
            string installedDirectory = Path.Combine(RepositoryInstallation.Installation.ContentDirectory, GameInstallationLayout.SpriteSheetDirectoryName);
            string[] installedFiles = [.. Directory.GetFiles(installedDirectory).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal)];
            AssertTrue(installedFiles.SequenceEqual(Directory.GetFiles(stock).Select(Path.GetFileName).OfType<string>().Order(StringComparer.Ordinal)),
                "the recomposed sprite sheets are the installed files");
            foreach (string name in installedFiles)
                AssertTrue(File.ReadAllBytes(Path.Combine(installedDirectory, name)).AsSpan().SequenceEqual(File.ReadAllBytes(Path.Combine(stock, name))),
                    $"sprite sheet file {name} recomposes byte for byte");
            InstalledSpriteSheets installed = SpriteSheetFiles.Load(stock, null);

            ComposedSpriteSheet composed = EscapeTimerSheetComposer.Compose(bus);
            SpriteSheet timer = installed.Get(SpriteSheetId.EscapeTimer);
            AssertEqual(composed.Size, timer.Image.Size, "the installed timer image has the composed size");
            AssertTrue(timer.Image.Pixels.SequenceEqual(composed.Pixels), "the installed timer image holds the composed pixels");
            AssertTrue(timer.HoldsExactly(EscapeTimerPresentationDefinitions.Glyphs.Select(EscapeTimerComposition.FrameName)),
                "the installed timer sheet holds exactly the timer's frames");

            string image = SpriteSheetStore.ImageFile(SpriteSheetDefinitions.FileNames[SpriteSheetId.EscapeTimer]);
            RgbaPixel[] edited = [.. composed.Pixels];
            int opaque = Array.FindIndex(edited, pixel => pixel.A != 0);
            edited[opaque] = new RgbaPixel(1, 2, 3, 255);
            Directory.CreateDirectory(overrides);
            using (FileStream output = File.Create(Path.Combine(overrides, image)))
                RgbaPng.Write(output, composed.Size, edited);
            InstalledSpriteSheets overridden = SpriteSheetFiles.Load(stock, overrides);
            AssertEqual(new RgbaPixel(1, 2, 3, 255), overridden.Get(SpriteSheetId.EscapeTimer).Image.Pixels[opaque],
                "a player's sheet image replaces the stock pixels");
            AssertTrue(overridden.ContentIdentity != installed.ContentIdentity, "a sheet image override changes the content identity");
            File.Delete(Path.Combine(overrides, image));

            string frames = SpriteSheetFiles.FrameFile(SpriteSheetId.EscapeTimer);
            SpriteSheetDocument missing = composed.Manifest with
            {
                Frames = composed.Manifest.Frames.Where(pair => pair.Key != "Digit.9").ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal),
            };
            File.WriteAllBytes(Path.Combine(overrides, frames), SpriteSheet.Write(missing));
            AssertThrows<InvalidDataException>(() => SpriteSheetFiles.Load(stock, overrides), "a sheet override missing a frame fails loudly");
            File.Delete(Path.Combine(overrides, frames));

            byte[] stockImage = File.ReadAllBytes(Path.Combine(stock, image));
            stockImage[^20] ^= 1;
            File.WriteAllBytes(Path.Combine(stock, image), stockImage);
            AssertThrows<InvalidDataException>(() => SpriteSheetFiles.Load(stock, null), "a damaged stock sheet fails its manifest hash");

            using (var corrupt = new MemoryStream(stockImage, writable: false))
                AssertThrows<InvalidDataException>(() => RgbaPng.Read(corrupt, out Extent16<ImagePixels> _), "a PNG chunk failing its CRC is rejected");
            Console.WriteLine("Sprite sheet files: composed timer sheet installed exactly, image override applied, missing frame and damaged stock rejected.");
        }
        finally
        {
            scratch.Delete(recursive: true);
        }
    }

    /// <summary>Copies <paramref name="source"/>'s files and folders into <paramref name="destination"/>.</summary>
    private static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.GetFiles(source))
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)));
        foreach (string folder in Directory.GetDirectories(source))
            CopyDirectory(folder, Path.Combine(destination, Path.GetFileName(folder)));
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
