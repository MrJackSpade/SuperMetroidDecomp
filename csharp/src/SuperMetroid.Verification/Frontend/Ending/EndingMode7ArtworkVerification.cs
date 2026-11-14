using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.EndingRewardIconArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Ending;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Compares installed ending Mode-7 scenes with retail art.</summary>
internal static class EndingMode7ArtworkVerification
{
    /// <summary>Checks three installed scenes against decompressed cartridge data and rendered output, and the reward icon.</summary>
    internal static void VerifyEndingMode7Artwork()
    {
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        EndingFloorCharacters stock = SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.EndingFloors;
        foreach (EndingFloorSceneId id in Enum.GetValues<EndingFloorSceneId>())
        {
            (int characterSource, int packedSource) = EndingArtworkSources.Floor(id);
            byte[] nativeCharacters = RomDataReader.Decompress(nativeBus, characterSource,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            byte[] nativePacked = RomDataReader.Decompress(nativeBus, packedSource,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            EndingFloorSceneArtwork compiledScene = stock[id];
            AssertTrue(compiledScene.Characters.Span.SequenceEqual(nativeCharacters.AsSpan(0,
                    EndingFloorArtworkFormat.CharacterByteCount)),
                $"installed ending {id} PNG preserves every high-lane character byte");
            for (int cell = 0; cell < EndingFloorArtworkFormat.MapByteCount; cell++)
                AssertEqual(nativePacked[cell * 2], compiledScene.Map.Span[cell],
                    $"installed ending {id} map preserves low-lane cell {cell}");
        }

        var guardedBus = NewEndingMode7SourceReadGuard(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
        var nativeAudio = AudioQueues.Standalone();
        var installedAudio = AudioQueues.Standalone();
        var native = CreateRetailEndingFixture(nativeAudio, 0, 0);
        var installed = CreateRetailEndingFixture(installedAudio, 0, 0);
        var phases = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 10000 &&
            native.Stage.Phase != EndingCreditsPhase.ZebesExplosionHold; frame++)
        {
            AssertEqual(native.Stage.Phase, installed.Stage.Phase,
                $"installed ending Mode-7 art preserves phase at frame {frame}");
            bool first = phases.Add(native.Stage.Phase);
            if (first || frame % 173 == 0)
            {
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                RgbColor[] expectedColours = EndingShownColours.Of(native);
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                RgbColor[] actualColours = EndingShownColours.Of(installed);
                byte[] actualVram = installed.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actualColours.SequenceEqual(expectedColours) &&
                        ScenePixels.Same(actual, expected),
                    $"installed ending art preserves native VRAM and pixels at frame {frame}");
            }
            native.Step();
            installed.Step();
            nativeAudio.AdvanceFrame(default);
            installedAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionHold, native.Stage.Phase,
            "ending art fixture reaches the explosion's hold");
        AssertEqual(EndingCreditsPhase.ZebesExplosionHold, installed.Stage.Phase,
            "installed art preserves the explosion's hold");
        AssertTrue(phases.Contains(EndingCreditsPhase.WaitForEscapeMusic) &&
                phases.Contains(EndingCreditsPhase.FadeInTopAndBottomCloudEscape) &&
                phases.Contains(EndingCreditsPhase.FadeInZebesExplosion),
            "all three ending Mode-7 scenes were exercised");
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed ending Mode-7 scenes never read their six visual ROM sources");

        Suite(nameof(VerifyEndingRewardIconArtwork), () => VerifyEndingRewardIconArtwork());
        Console.WriteLine("Ending Mode-7 art: three native scenes plus the reward icon and guarded runtime pass.");
    }

    /// <summary>Denies reads of the ending scene, reward-icon, and post-credits art sources while production presentation uses installed assets.</summary>
    internal static MutableCartridgeReadGuard NewEndingMode7SourceReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address =>
                (address >= EndingRewardGraphicsUploadDefinitions.SourceTable &&
                 address < EndingRewardGraphicsUploadDefinitions.DestinationTable +
                 EndingRewardGraphicsUploadDefinitions.ChunkCount * sizeof(ushort)) ||
                address is EndingSourceDefinitions.Artwork.SideCloudEscapeCharacters or
                EndingSourceDefinitions.Artwork.SideCloudEscapePackedMap or
                EndingSourceDefinitions.Artwork.TopAndBottomCloudEscapeCharacters or
                EndingSourceDefinitions.Artwork.TopAndBottomCloudEscapePackedMap or
                EndingSourceDefinitions.Artwork.ExplosionCharacters or
                EndingSourceDefinitions.Artwork.ExplosionPackedMap or
                EndingSourceDefinitions.Artwork.PostCreditsFloorCharacters,
            address => $"Ending art reread cartridge source ${address:X6}.");
        return guard;
    }
}
