using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresFlightArtworkVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Verification.Frontend.CeresCinematics;

/// <summary>Checks Ceres flight sprites against native OAM and verifies editable composition behavior.</summary>
internal static class CeresFlightSpriteArtworkVerification
{
    /// <summary>Compares six retail frames, tests a visible star offset edit, and verifies rebind, validation, and stock repair.</summary>
    internal static void VerifyCeresFlightSpriteArtwork(GameInstallation installation,
        SuperMetroidAddressSpace bus, CeresFlightArtworkCatalog stock)
    {
        foreach (CinematicSpriteFrameDefinition frame in CeresFlightSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    ((y & CinematicSpriteDrawDefinitions.OriginYHighByteMask)) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, CinematicSpritemapAddresses.Of(frame.Pose));
                var native = new OamBuffer();
                native.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, native, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, native, source, new(120, y), 0x0800, originIsOnScreen: false);
                native.FinalizeFrame();
                var installed = ScreenSpritesFixtures.Blank();
                installed.BeginFrame();
                CinematicSheetFixtures.Drawer(stock.Sprites).Draw(frame.Pose, installed, new(120, y), 0x0800, onScreen);
                installed.FinalizeFrame();
                AssertTrue(installed.ImageEquals(native) &&
                        installed.LastFinalizedSpriteCount == native.LastFinalizedSpriteCount,
                    $"Ceres flight {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }

        Directory.CreateDirectory(installation.IntroCinematicOverrideDirectory);
        string name = CeresFlightSpriteFormat.FileName;
        string stockPath = Path.Combine(installation.IntroCinematicDirectory, name);
        string overridePath = Path.Combine(installation.IntroCinematicOverrideDirectory, name);
        CeresFlightSpriteDocument document = JsonSerializer.Deserialize<CeresFlightSpriteDocument>(
            File.ReadAllBytes(stockPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock Ceres flight sprites are empty.");
        document.Frames["stars"] = document.Frames["stars"]
            .Select(part => part with { OffsetX = part.OffsetX + 16 }).ToArray();
        using (var output = File.Create(overridePath))
            CeresFlightSpritePresentation.Write(output, document);

        CeresFlightArtworkCatalog edited = installation.LoadIntroCinematicArt().CeresFlight;
        var stockFlight = new IntroCeresFlightState(new CartridgeAudioState(), stock, spriteSheet: CinematicSheetFixtures.CeresFlight(stock));
        var editedFlight = new IntroCeresFlightState(new CartridgeAudioState(), edited, spriteSheet: CinematicSheetFixtures.CeresFlight(edited));
        for (int frame = 0; frame < CeresFrontApproachTicks; frame++)
        {
            stockFlight.StepFrame();
            editedFlight.StepFrame();
        }
        LayeredRenderSnapshot original = stockFlight.CaptureRenderSnapshot();
        LayeredRenderSnapshot replacement = editedFlight.CaptureRenderSnapshot();
        AssertEqual(IntroCeresFlightPhase.FlyingIntoCamera, editedFlight.Phase,
            "Ceres star edit reaches the visible front approach");
        AssertTrue(original.Memory.Vram.SequenceEqual(replacement.Memory.Vram) &&
                original.Memory.Cgram.SequenceEqual(replacement.Memory.Cgram),
            "Ceres sprite composition edit does not change characters or palette");
        AssertTrue(!original.Memory.SpriteParts().SequenceEqual(replacement.Memory.SpriteParts()) &&
                !SoftwareLayeredSnapshotRenderer.Render(original).SequenceEqual(
                    SoftwareLayeredSnapshotRenderer.Render(replacement)),
            "Ceres star composition edit changes production OAM and visible pixels");

        stockFlight.BindFixtureFlightArtwork(edited);
        AssertTrue(stockFlight.CaptureRenderSnapshot().Memory.SpriteParts().SequenceEqual(
                replacement.Memory.SpriteParts()),
            "restored Ceres flight rebinds the selected actor composition without restarting");

        document.Frames.Remove("stars");
        AssertThrows<InvalidDataException>(() =>
        {
            using var invalid = new MemoryStream();
            CeresFlightSpritePresentation.Write(invalid, document);
        }, "Ceres flight sprites reject a missing named frame");

        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadIntroCinematicArt(),
            "a sprite override cannot hide corrupted stock Ceres sprites");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ceres sprites vanished during stock repair.");
        CeresFlightArtworkCatalog repairedArtwork = repaired.LoadIntroCinematicArt().CeresFlight;
        var repairedFlight = new IntroCeresFlightState(new CartridgeAudioState(),
            repairedArtwork, spriteSheet: CinematicSheetFixtures.CeresFlight(repairedArtwork));
        for (int frame = 0; frame < CeresFrontApproachTicks; frame++)
            repairedFlight.StepFrame();
        AssertTrue(repairedFlight.CaptureRenderSnapshot().Memory.SpriteParts().SequenceEqual(
                replacement.Memory.SpriteParts()),
            "Ceres sprite override survives stock extraction repair");
        File.Delete(overridePath);
        Console.WriteLine("  Ceres flight sprites: six native OAM frames, visible edit, rebind, strict validation, and repair preservation pass.");
    }
}
