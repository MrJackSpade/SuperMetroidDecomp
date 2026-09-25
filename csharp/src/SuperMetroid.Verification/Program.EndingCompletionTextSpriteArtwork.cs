using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingCompletionTextSpriteArtwork(ISnesAddressSpace bus,
        EndingObjectArtworkCatalog stock)
    {
        foreach (EndingCompletionTextSpriteFrameDefinition frame in
            EndingCompletionTextSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    (y & CinematicSpriteDrawDefinitions.OriginYHighByteMask) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    nativeOam.AddOnScreenSpritemap(bus, source, 120, y, 0x0800);
                else
                    nativeOam.AddOffScreenSpritemap(bus, source, 120, y, 0x0800);
                nativeOam.FinalizeFrame();
                var installedOam = new OamBuffer();
                installedOam.BeginFrame();
                stock.CompletionTextSprites.Draw(frame.Pointer, installedOam,
                    120, y, 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                        nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending completion text {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    private static void VerifyEndingCompletionTextVisualOverride(GameInstallation installation,
        EndingObjectArtworkCatalog stock, EndingObjectSourceReadGuard guard)
    {
        string name = EndingCompletionTextSpriteFormat.FileName;
        string stockPath = Path.Combine(installation.EndingObjectDirectory, name);
        string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory, name);
        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        EndingCompletionTextSpriteDocument document =
            JsonSerializer.Deserialize<EndingCompletionTextSpriteDocument>(
                File.ReadAllBytes(stockPath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock ending completion text sprite JSON is empty.");
        document.Frames["operation-14"] = document.Frames["operation-14"]
            .Select(part => part with { OffsetX = part.OffsetX + 8 }).ToArray();
        try
        {
            using (var output = File.Create(overridePath))
                EndingCompletionTextSpritePresentation.Write(output, document);
            EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
            var audio = new CartridgeAudioState();
            var scene = new EndingCreditsState(guard, audio, 0, 0);
            scene.BindObjectArtwork(stock);
            for (int frame = 0; frame < 12000 &&
                scene.Phase != EndingCreditsPhase.OperationSuccessfulText; frame++)
            {
                scene.Step();
                audio.AdvanceFrame(guard, default);
            }
            AssertEqual(EndingCreditsPhase.OperationSuccessfulText, scene.Phase,
                "ending completion sprite edit reaches its actual scene");
            bool visible = false;
            for (int frame = 0; frame < 1500 &&
                scene.Phase == EndingCreditsPhase.OperationSuccessfulText && !visible; frame++)
            {
                scene.BindObjectArtwork(stock);
                LayeredRenderSnapshot before = scene.CaptureRenderSnapshot();
                scene.BindObjectArtwork(edited);
                LayeredRenderSnapshot after = scene.CaptureRenderSnapshot();
                visible = !before.Memory.Oam.SequenceEqual(after.Memory.Oam) &&
                    !SoftwareLayeredSnapshotRenderer.Render(before).AsSpan().SequenceEqual(
                        SoftwareLayeredSnapshotRenderer.Render(after));
                if (!visible)
                {
                    scene.Step();
                    audio.AdvanceFrame(guard, default);
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
