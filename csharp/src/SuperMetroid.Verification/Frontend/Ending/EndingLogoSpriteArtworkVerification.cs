using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks ending logo OAM and presentation overrides against native spritemaps.</summary>
internal static class EndingLogoSpriteArtworkVerification
{
    /// <summary>Compares every installed logo frame at visible and wrapped origins with native OAM.</summary>
    internal static void VerifyEndingLogoSpriteArtwork(ISnesAddressSpace bus,
        EndingObjectArtworkCatalog stock)
    {
        foreach (CinematicSpriteFrameDefinition frame in EndingLogoSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    ((y & CinematicSpriteDrawDefinitions.OriginYHighByteMask)) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, CinematicSpritemapSources.Of(frame.Pose));
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = ScreenSpritesFixtures.Blank();
                installedOam.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.LogoSprites).Draw(frame.Pose, installedOam,
                    new(120, y), 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending logo {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    /// <summary>Loads a moved circle frame from an override and verifies that missing named art is rejected.</summary>
    internal static EndingObjectArtworkCatalog LoadEditedEndingLogoArtwork(
        GameInstallation installation)
    {
        string name = EndingLogoSpriteFormat.FileName;
        string stockPath = Path.Combine(installation.EndingObjectDirectory, name);
        string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory, name);
        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        EndingLogoSpriteDocument document = JsonSerializer.Deserialize<EndingLogoSpriteDocument>(
            File.ReadAllBytes(stockPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock ending logo sprite JSON is empty.");
        document.Frames["circle-right-3"] = document.Frames["circle-right-3"]
            .Select(part => part with { OffsetX = part.OffsetX + 8 }).ToArray();
        try
        {
            using (var output = File.Create(overridePath))
                EndingLogoSpritePresentation.Write(output, document);
            EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
            document.Frames.Remove("circle-right-3");
            AssertThrows<InvalidDataException>(() =>
            {
                using var invalid = new MemoryStream();
                EndingLogoSpritePresentation.Write(invalid, document);
            }, "ending logo sprites reject missing named art");
            return edited;
        }
        finally
        {
            File.Delete(overridePath);
        }
    }
}
