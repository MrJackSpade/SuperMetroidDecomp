using System.Text.Json;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Checks suitless reward sprite art against native OAM and the live ending scenes.</summary>
internal static class EndingRewardSpriteArtworkVerification
{
    /// <summary>Compares every installed reward frame at screen and wrapped off-screen origins.</summary>
    internal static void VerifyEndingRewardSpriteArtwork(ISnesAddressSpace bus,
        EndingObjectArtworkCatalog stock)
    {
        foreach (EndingRewardSpriteFrameDefinition frame in EndingRewardSpriteDefinitions.Frames)
        {
            foreach (ushort y in new ushort[] { 0x0048, 0xfff8 })
            {
                bool onScreen =
                    ((y & CinematicSpriteDrawDefinitions.OriginYHighByteMask)) == 0;
                int source = (int)new SnesAddress(
                    IntroCinematicRomData.Banks.Spritemaps, frame.Pointer);
                var nativeOam = new OamBuffer();
                nativeOam.BeginFrame();
                if (onScreen)
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800);
                else
                    DrawImportedSpritemap(bus, nativeOam, source, new(120, y), 0x0800, originIsOnScreen: false);
                nativeOam.FinalizeFrame();
                var installedOam = new OamBuffer();
                installedOam.BeginFrame();
                stock.RewardSprites.Draw(frame.Pointer, installedOam,
                    new(120, y), 0x0800, onScreen);
                installedOam.FinalizeFrame();
                AssertTrue(nativeOam.ImageEquals(installedOam) &&
                        nativeOam.LastFinalizedSpriteCount == installedOam.LastFinalizedSpriteCount,
                    $"ending reward {frame.Name} at Y=${y:X4} preserves cartridge OAM");
            }
        }
    }

    /// <summary>Edits idle, gesture, and jump art and checks visible changes without sprite-ROM rereads.</summary>
    internal static void VerifyEndingRewardVisualOverride(GameInstallation installation,
        EndingObjectArtworkCatalog stock, MutableCartridgeReadGuard guard)
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
            var scene = CreateRetailEndingFixture(guard, audio, 2, 0);
            scene.BindStaffCredits(CreditsPresentation.Load(new MemoryStream(
                CreditsPresentationExtractor.Extract(
                    SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc")))));
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
                    LayeredRenderSnapshot before = scene.Presentation.CaptureRenderSnapshot();
                    scene.BindObjectArtwork(edited);
                    LayeredRenderSnapshot after = scene.Presentation.CaptureRenderSnapshot();
                    if (!before.Memory.Oam.SequenceEqual(after.Memory.Oam) &&
                        !SoftwareLayeredSnapshotRenderer.Render(before).SequenceEqual(
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
