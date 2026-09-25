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
    private static void VerifyEndingRewardSpriteArtwork(ISnesAddressSpace bus,
        EndingObjectArtworkCatalog stock)
    {
        foreach (EndingRewardSpriteFrameDefinition frame in EndingRewardSpriteDefinitions.Frames)
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
                stock.RewardSprites.Draw(frame.Pointer, installedOam,
                    120, y, 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.LowTable.SequenceEqual(installedOam.LowTable) &&
                        nativeOam.HighTable.SequenceEqual(installedOam.HighTable) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending reward {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    private static void VerifyEndingRewardVisualOverride(GameInstallation installation,
        EndingObjectArtworkCatalog stock, EndingObjectSourceReadGuard guard)
    {
        string name = EndingRewardSpriteFormat.FileName;
        string stockPath = Path.Combine(installation.EndingObjectDirectory, name);
        string overridePath = Path.Combine(installation.EndingObjectOverrideDirectory, name);
        Directory.CreateDirectory(installation.EndingObjectOverrideDirectory);
        EndingRewardSpriteDocument document =
            JsonSerializer.Deserialize<EndingRewardSpriteDocument>(
                File.ReadAllBytes(stockPath), MapPresentationFormat.JsonOptions) ??
            throw new InvalidDataException("Stock ending reward sprite JSON is empty.");
        string[] editedNames =
            ["suitless-idle-upper", "suitless-hair-1", "suitless-jumping"];
        foreach (string frameName in editedNames)
            document.Frames[frameName] = document.Frames[frameName]
                .Select(part => part with { OffsetX = part.OffsetX + 8 }).ToArray();
        try
        {
            using (var output = File.Create(overridePath))
                EndingRewardSpritePresentation.Write(output, document);
            EndingObjectArtworkCatalog edited = installation.LoadEndingObjectArt();
            var audio = new CartridgeAudioState();
            var scene = new EndingCreditsState(guard, audio, 2, 0);
            scene.BindStaffCredits(CreditsPresentation.Load(new MemoryStream(
                CreditsPresentationExtractor.Extract(
                    SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc")))));
            scene.BindObjectArtwork(stock);
            scene.BindMode7Artwork(installation.LoadEndingMode7Art());
            AssertTrue(FindVisibleEdit(EndingCreditsPhase.PostCreditsReward, 60000, 150),
                "edited suitless idle frame changes live OAM and visible reward pixels");
            AssertTrue(FindVisibleEdit(EndingCreditsPhase.PostCreditsGesture, 2000, 400),
                "edited suitless hair frame changes live gesture OAM and pixels");
            AssertTrue(FindVisibleEdit(EndingCreditsPhase.PostCreditsJump, 2000, 300),
                "edited suitless jump frame changes live flight OAM and pixels");
            AssertEqual(0, guard.ForbiddenReadAttempts,
                "reward art rebind never rereads installed bank-$8C sprite maps");
            foreach (string frameName in editedNames)
                document.Frames.Remove(frameName);
            AssertThrows<InvalidDataException>(() =>
            {
                using var invalid = new MemoryStream();
                EndingRewardSpritePresentation.Write(invalid, document);
            }, "ending reward sprites reject missing named art");

            bool FindVisibleEdit(EndingCreditsPhase target, int arrivalLimit, int visibleLimit)
            {
                for (int frame = 0; frame < arrivalLimit && scene.Phase != target; frame++)
                {
                    scene.Step();
                    audio.AdvanceFrame(guard, default);
                }
                AssertEqual(target, scene.Phase,
                    $"reward sprite edit reaches {target}");
                for (int frame = 0; frame < visibleLimit && scene.Phase == target; frame++)
                {
                    scene.BindObjectArtwork(stock);
                    LayeredRenderSnapshot before = scene.CaptureRenderSnapshot();
                    scene.BindObjectArtwork(edited);
                    LayeredRenderSnapshot after = scene.CaptureRenderSnapshot();
                    if (!before.Memory.Oam.SequenceEqual(after.Memory.Oam) &&
                        !SoftwareLayeredSnapshotRenderer.Render(before).AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(after)))
                        return true;
                    scene.Step();
                    audio.AdvanceFrame(guard, default);
                }
                return false;
            }
        }
        finally
        {
            File.Delete(overridePath);
        }
    }
}
