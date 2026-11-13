using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Intro;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks Ceres flight sprites against native OAM and the sprite codec's missing-frame rejection.</summary>
internal static class CeresFlightSpriteArtworkVerification
{
    /// <summary>Compares six retail frames with native OAM and checks that a document missing a frame is rejected.</summary>
    internal static void VerifyCeresFlightSpriteArtwork(SuperMetroidAddressSpace bus, CeresFlightCharacters stock)
    {
        foreach (CinematicSpriteFrameDefinition frame in CeresFlightSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (short)y >= 0;
                int source = (int)new SnesAddress(
                    IntroCinematicSourceDefinitions.SpritemapBank, CinematicSpritemapSources.Of(frame.Pose));
                var native = new OamBuffer();
                native.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, native, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, native, source, new(120, y), 0x0800, originIsOnScreen: false);
                native.FinalizeFrame();
                var installed = ScreenSpritesFixtures.Blank();
                installed.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.Sprites).Draw(frame.Pose, installed, new(120, y), CeresShades.Drawn(CeresSpriteColouring.SpaceObjects));
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"Ceres flight {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }

        string stockPath = Path.Combine(SuperMetroid.TestSupport.RepositoryInstallation.Verification.CartridgeImportContent.ImportData(SuperMetroid.AssetExtraction.Installation.GameInstallationLayout.IntroCinematicDirectoryName), CeresFlightSpriteFormat.FileName);
        CeresFlightSpriteDocument document = JsonSerializer.Deserialize<CeresFlightSpriteDocument>(
            File.ReadAllBytes(stockPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock Ceres flight sprites are empty.");
        document.Frames.Remove("stars");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresFlightSpritePresentation.Write(invalid, document);
            _ = CeresFlightSpritePresentation.Load(new MemoryStream(invalid.ToArray(), writable: false));
        }, "Ceres flight sprites reject a missing named frame on read");

        Console.WriteLine("  Ceres flight sprites: six native OAM frames and strict validation pass.");
    }
}
