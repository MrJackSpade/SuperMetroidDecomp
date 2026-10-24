using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.Frontend.Cinematics;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.Assets;
using SuperMetroid.TestSupport.FrameRendering;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks ending completion-text OAM and live replacement in the actual credits scene.</summary>
internal static class EndingCompletionTextSpriteArtworkVerification
{
    /// <summary>Compares every installed completion frame at on-screen and wrapped off-screen anchors.</summary>
    internal static void VerifyEndingCompletionTextSpriteArtwork(ISnesAddressSpace bus,
        EndingObjectArtworkCatalog stock)
    {
        foreach (CinematicSpriteFrameDefinition frame in
            EndingCompletionTextSpriteDefinitions.Frames)
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
                CinematicSheetFixtures.Drawer(stock.CompletionTextSprites).Draw(frame.Pose, installedOam,
                    new(120, y), 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending completion text {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    /// <summary>Edits operation text art and checks its live pixel change without rereading sprite ROM.</summary>
    internal static void VerifyEndingCompletionTextVisualOverride(GameInstallation installation,
        EndingObjectArtworkCatalog stock, MutableCartridgeReadGuard guard)
    {
        string name = EndingCompletionTextSpriteFormat.FileName;
        string stockPath = Path.Combine(installation.EndingObjectDirectory, name);
        string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory, name);
        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        EndingCompletionTextSpriteDocument document =
            JsonSerializer.Deserialize<EndingCompletionTextSpriteDocument>(
                File.ReadAllBytes(stockPath), ArtworkJson.Options) ??
            throw new InvalidDataException("Stock ending completion text sprite JSON is empty.");
        document.Frames["operation-14"] = document.Frames["operation-14"]
            .Select(part => part with { OffsetX = part.OffsetX + 8 }).ToArray();
        try
        {
            using (var output = File.Create(overridePath))
                EndingCompletionTextSpritePresentation.Write(output, document);
            EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
            var audio = new AudioQueues();
            var scene = CreateRetailEndingFixture(audio, 0, 0);
            scene.InstalledArtwork.BindFixtureObjectArtwork(stock);
            for (int frame = 0; frame < 12000 &&
                scene.Stage.Phase != EndingCreditsPhase.OperationSuccessfulText; frame++)
            {
                scene.Step();
                audio.AdvanceFrame(default);
            }
            AssertEqual(EndingCreditsPhase.OperationSuccessfulText, scene.Stage.Phase,
                "ending completion sprite edit reaches its actual scene");
            bool visible = false;
            for (int frame = 0; frame < 1500 &&
                scene.Stage.Phase == EndingCreditsPhase.OperationSuccessfulText && !visible; frame++)
            {
                scene.InstalledArtwork.BindFixtureObjectArtwork(stock);
                LayeredScene before = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
                scene.InstalledArtwork.BindFixtureObjectArtwork(edited);
                LayeredScene after = EndingCreditsPresentation.CaptureRenderSnapshot(scene);
                visible = !before.Paint.SpriteParts().SequenceEqual(after.Paint.SpriteParts()) &&
                    !DisplayRendering.Render(before).SequenceEqual(
                        DisplayRendering.Render(after));
                if (!visible)
                {
                    scene.Step();
                    audio.AdvanceFrame(default);
                }
            }
            AssertTrue(visible,
                "edited completion text shifts live OAM and visible ending pixels");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                "completion text rebind never rereads installed bank-$8C sprite maps");
            document.Frames.Remove("operation-14");
            AssertThrows<InvalidDataException>(() =>
            {
                using var invalid = new MemoryStream();
                EndingCompletionTextSpritePresentation.Write(invalid, document);
            }, "ending completion text sprites reject missing named art");
        }
        finally
        {
            File.Delete(overridePath);
        }
    }
}
