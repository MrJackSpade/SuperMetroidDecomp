using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks ending completion-text OAM against the cartridge and rejection of a missing frame.</summary>
internal static class EndingCompletionTextSpriteArtworkVerification
{
    /// <summary>Compares every installed completion frame at on-screen and wrapped off-screen anchors.</summary>
    internal static void VerifyEndingCompletionTextSpriteArtwork(ISnesAddressSpace bus,
        EndingObjectSourceArtwork stock)
    {
        foreach (CinematicSpriteFrameDefinition frame in
            EndingCompletionTextSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (short)y >= 0;
                int source = (int)new SnesAddress(
                    IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = ScreenSpritesFixtures.Blank();
                installedOam.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.CompletionTextSprites).Draw(frame.Pose, installedOam,
                    new(120, y), EndingShades.Drawn(EndingSpriteColouring.ShootingStars));
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending completion text {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    /// <summary>Checks that a completion text sprite document missing a named frame is rejected on read.</summary>
    internal static void VerifyEndingCompletionTextSpritesRejectMissingFrame()
    {
        EndingCompletionTextSpriteDocument document =
            JsonSerializer.Deserialize<EndingCompletionTextSpriteDocument>(
                File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.EndingObjectDirectoryName), EndingCompletionTextSpriteFormat.FileName)), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock ending completion text sprite JSON is empty.");
        document.Frames.Remove("operation-14");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            EndingCompletionTextSpritePresentation.Write(invalid, document);
            _ = EndingCompletionTextSpritePresentation.Load(new MemoryStream(invalid.ToArray(), writable: false));
        }, "ending completion text sprites reject missing named art on read");
    }
}
