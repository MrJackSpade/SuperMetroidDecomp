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

/// <summary>Checks suitless reward sprite art against native OAM and the live ending scenes.</summary>
internal static class EndingRewardSpriteArtworkVerification
{
    /// <summary>Compares every installed reward frame at screen and wrapped off-screen origins.</summary>
    internal static void VerifyEndingRewardSpriteArtwork(ISnesAddressSpace bus,
        EndingObjectSourceArtwork stock)
    {
        foreach (CinematicSpriteFrameDefinition frame in EndingRewardSpriteDefinitions.Frames)
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
                CinematicSheetFixtures.Drawer(stock.RewardSprites).Draw(frame.Pose, installedOam,
                    new(120, y), EndingShades.Drawn(EndingSpriteColouring.ShootingStars));
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending reward {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    /// <summary>Checks that a reward sprite document missing named frames is rejected on read.</summary>
    internal static void VerifyEndingRewardSpritesRejectMissingFrame()
    {
        EndingRewardSpriteDocument document =
            JsonSerializer.Deserialize<EndingRewardSpriteDocument>(
                File.ReadAllBytes(Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.EndingObjectDirectoryName), EndingRewardSpriteFormat.FileName)), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock ending reward sprite JSON is empty.");
        document.Frames.Remove("suitless-idle-upper");
        document.Frames.Remove("suitless-hair-1");
        document.Frames.Remove("suitless-jumping");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            EndingRewardSpritePresentation.Write(invalid, document);
            _ = EndingRewardSpritePresentation.Load(new MemoryStream(invalid.ToArray(), writable: false));
        }, "ending reward sprites reject missing named art on read");
    }
}
