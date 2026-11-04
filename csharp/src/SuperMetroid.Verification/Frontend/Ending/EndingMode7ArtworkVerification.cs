using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rendering;
using static SuperMetroid.Verification.Frontend.Ending.Artwork.EndingRewardIconArtworkVerification;
using static SuperMetroid.Verification.Harness.RetailGameFixtureVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.FrameRendering;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Compares installed ending Mode-7 scenes with retail art and checks live override rebinding.</summary>
internal static class EndingMode7ArtworkVerification
{
    /// <summary>Checks three installed scenes against decompressed cartridge data and rendered output, then verifies map/PNG edits and stock repair.</summary>
    [PrivateInstallation("Corrupts the stock ending Mode 7 artwork on a private copy to check that an override cannot hide the damage.")]
    internal static void VerifyEndingMode7Artwork(GameInstallation installation)
    {
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        EndingFloorArtwork stock = installation.LoadEndingMode7Art();
        foreach (EndingFloorSceneId id in Enum.GetValues<EndingFloorSceneId>())
        {
            (int characterSource, int packedSource) = EndingArtworkSources.Floor(id);
            byte[] nativeCharacters = RomDataReader.Decompress(nativeBus, characterSource,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            byte[] nativePacked = RomDataReader.Decompress(nativeBus, packedSource,
                EndingSourceDefinitions.Artwork.DecompressionLimit);
            EndingFloorSceneArtwork compiledScene = EndingFloorCharacters.SceneOf(stock, id);
            AssertTrue(compiledScene.Characters.Span.SequenceEqual(nativeCharacters.AsSpan(0,
                    EndingFloorArtworkFormat.CharacterByteCount)),
                $"installed ending {id} PNG preserves every high-lane character byte");
            for (int cell = 0; cell < EndingFloorArtworkFormat.MapByteCount; cell++)
                AssertEqual(nativePacked[cell * 2], compiledScene.Map.Span[cell],
                    $"installed ending {id} map preserves low-lane cell {cell}");
        }

        var guardedBus = NewEndingMode7SourceReadGuard(
            SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
        var nativeAudio = new AudioQueues();
        var installedAudio = new AudioQueues();
        var native = CreateRetailEndingFixture(nativeAudio, 0, 0);
        var installed = CreateRetailEndingFixture(installedAudio, 0, 0);
        installed.Stage.BindFloorArtwork(stock);
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
                        DisplayRendering.Render(actual).SequenceEqual(
                            DisplayRendering.Render(expected)),
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
                phases.Contains(EndingCreditsPhase.FadeInEscapeSceneB) &&
                phases.Contains(EndingCreditsPhase.FadeInZebesExplosion),
            "all three ending Mode-7 scenes were exercised");
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed ending Mode-7 scenes never read their six visual ROM sources");

        Directory.CreateDirectory(installation.EndingMode7OverrideDirectory);
        foreach (EndingFloorSceneId id in Enum.GetValues<EndingFloorSceneId>())
        {
            EndingCreditsPhase target = id switch
            {
                EndingFloorSceneId.EscapeA => EndingCreditsPhase.WaitForEscapeMusic,
                EndingFloorSceneId.EscapeB => EndingCreditsPhase.FadeInEscapeSceneB,
                EndingFloorSceneId.PlanetExplosion => EndingCreditsPhase.FadeInZebesExplosion,
                _ => throw new ArgumentOutOfRangeException(nameof(id)),
            };
            var sceneBus = NewEndingMode7SourceReadGuard(
                SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
            var audio = new AudioQueues();
            var state = CreateRetailEndingFixture(audio, 0, 0);
            state.Stage.BindFloorArtwork(stock);
            for (int frame = 0; frame < 10000 && state.Stage.Phase != target; frame++)
            {
                state.Step();
                audio.AdvanceFrame(default);
            }
            AssertEqual(target, state.Stage.Phase, $"ending {id} override fixture reaches its scene");
            for (int visibleFrame = 0; visibleFrame < 1200 &&
                state.Stage.Brightness == 0; visibleFrame++)
            {
                state.Step();
                audio.AdvanceFrame(default);
            }
            LayeredScene baselineSnapshot = EndingCreditsPresentation.CaptureRenderSnapshot(state);
            byte[] baselineSnapshotVram = state.ShownVram();
            AssertTrue(baselineSnapshot.Brightness > 0,
                $"ending {id} override fixture reaches a visible frame");
            byte[] baseline = baselineSnapshotVram.ToArray();
            RgbaPixel[] baselinePixels = DisplayRendering.Render(baselineSnapshot);

            string mapName = EndingFloorArtworkFormat.MapFileName(id);
            string mapOverride = Path.Combine(installation.EndingMode7OverrideDirectory, mapName);
            EndingFloorMapDocument map;
            using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory, mapName)))
                map = JsonSerializer.Deserialize<EndingFloorMapDocument>(input,
                    ArtworkJson.Options)
                    ?? throw new InvalidDataException($"Ending {id} map JSON is empty.");
            Array.Fill(map.Tiles, 0);
            using (var output = File.Create(mapOverride))
                EndingFloorSceneCharacters.WriteMap(output, map);
            EndingFloorArtwork editedMap = installation.LoadEndingMode7Art();
            state.Stage.BindFloorArtwork(editedMap);
            // The ending draws its floor from the floor sheet the import composes from the map, so the edited map reaches
            // the screen through the sheets the edited art composes (#1467).
            InstalledSpriteSheets editedMapSheets = EndingSheetFixtures.WithEndingMode7(editedMap);
            state.InstalledArtwork.BindSpriteSheets(editedMapSheets);
            LayeredScene mapSnapshot = EndingCreditsPresentation.CaptureRenderSnapshot(state);
            SpriteSheetId floorSheet = id switch
            {
                EndingFloorSceneId.EscapeA => SpriteSheetId.EndingEscapeAFloor,
                EndingFloorSceneId.EscapeB => SpriteSheetId.EndingEscapeBFloor,
                EndingFloorSceneId.PlanetExplosion => SpriteSheetId.EndingPlanetExplosionFloor,
                _ => throw new ArgumentOutOfRangeException(nameof(id)),
            };
            AssertTrue(!InstalledFixtures.SpriteSheets.Get(floorSheet).Image.Pixels.ToArray()
                    .SequenceEqual(editedMapSheets.Get(floorSheet).Image.Pixels.ToArray()) &&
                    !baselinePixels.SequenceEqual(DisplayRendering.Render(mapSnapshot)),
                $"edited ending {id} map recomposes its floor picture and changes the shown frame");
            File.Delete(mapOverride);
            state.Stage.BindFloorArtwork(stock);
            state.InstalledArtwork.BindSpriteSheets(InstalledFixtures.SpriteSheets);

            string pngName = EndingFloorArtworkFormat.CharacterFileName(id);
            string pngOverride = Path.Combine(installation.EndingMode7OverrideDirectory, pngName);
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory, pngName)))
                image = IndexedPng.Read(input, new(EndingFloorArtworkFormat.CharacterWidth, EndingFloorArtworkFormat.CharacterHeight));
            Array.Fill(image.Pixels, (byte)0);
            using (var output = File.Create(pngOverride))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            EndingFloorArtwork editedCharacters = installation.LoadEndingMode7Art();
            state.Stage.BindFloorArtwork(editedCharacters);
            // The ending draws its Mode 7 characters from the sheets the edited art composes (#1392).
            state.InstalledArtwork.BindSpriteSheets(EndingSheetFixtures.WithEndingMode7(editedCharacters));
            LayeredScene characterSnapshot = EndingCreditsPresentation.CaptureRenderSnapshot(state);
            byte[] characterSnapshotVram = state.ShownVram();
            byte[] characterVram = characterSnapshotVram.ToArray();
            AssertTrue(!baseline.SequenceEqual(characterVram) &&
                    baseline.AsSpan(0x8000).SequenceEqual(characterVram.AsSpan(0x8000)) &&
                    !baselinePixels.SequenceEqual(
                        DisplayRendering.Render(characterSnapshot)),
                $"edited ending {id} PNG changes visible pixels, but only its Mode-7 VRAM lanes");
            File.Delete(pngOverride);
            AssertEqual(0, sceneBus.ForbiddenReadAttempts,
                $"ending {id} edits/rebinds avoid the six cartridge art sources");
        }

        string invalidMapPath = Path.Combine(installation.EndingMode7OverrideDirectory,
            EndingFloorArtworkFormat.MapFileName(EndingFloorSceneId.EscapeA));
        File.WriteAllBytes(invalidMapPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingMode7Art(),
            "malformed ending Mode-7 override fails loudly");
        File.Delete(invalidMapPath);
        EndingFloorMapDocument persistentMap;
        using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory,
                   EndingFloorArtworkFormat.MapFileName(EndingFloorSceneId.EscapeA))))
            persistentMap = JsonSerializer.Deserialize<EndingFloorMapDocument>(input,
                ArtworkJson.Options)
                ?? throw new InvalidDataException("Stock ending map is empty.");
        persistentMap.Tiles[0] = unchecked((byte)(persistentMap.Tiles[0] + 1));
        using (var output = File.Create(invalidMapPath))
            EndingFloorSceneCharacters.WriteMap(output, persistentMap);
        byte[] selectedMap = EndingFloorCharacters.SceneOf(installation.LoadEndingMode7Art(), EndingFloorSceneId.EscapeA)
            .Map.ToArray();
        string stockPngPath = Path.Combine(installation.EndingMode7Directory,
            EndingFloorArtworkFormat.CharacterFileName(EndingFloorSceneId.EscapeA));
        byte[] stockPng = File.ReadAllBytes(stockPngPath);
        File.WriteAllBytes(stockPngPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingMode7Art(),
            "corrupt stock ending Mode-7 PNG cannot be hidden by a valid override");
        // Installed content is never repaired in place (#1532): damaged stock fails where it loads, and the check puts the stock bytes back.
        File.WriteAllBytes(stockPngPath, stockPng);
        AssertTrue(EndingFloorCharacters.SceneOf(installation.LoadEndingMode7Art(), EndingFloorSceneId.EscapeA)
                .Characters.Span.SequenceEqual(EndingFloorCharacters.SceneOf(stock, EndingFloorSceneId.EscapeA).Characters.Span),
            "the restored stock ending Mode-7 PNG shows the cartridge's characters");
        AssertTrue(EndingFloorCharacters.SceneOf(installation.LoadEndingMode7Art(), EndingFloorSceneId.EscapeA)
                .Map.Span.SequenceEqual(selectedMap),
            "the external map override still applies over the restored stock PNG");
        File.Delete(invalidMapPath);
        Suite(nameof(VerifyEndingRewardIconArtwork), () => VerifyEndingRewardIconArtwork(installation));
        Console.WriteLine("Ending Mode-7 art: three native scenes plus the reward icon, guarded runtime and independent edits pass.");
    }

    /// <summary>Denies reads of the ending scene, reward-icon, and post-credits art sources while production presentation uses installed assets.</summary>
    internal static MutableCartridgeReadGuard NewEndingMode7SourceReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address =>
                (address >= EndingRewardGraphicsUploadDefinitions.SourceTable &&
                 address < EndingRewardGraphicsUploadDefinitions.DestinationTable +
                 EndingRewardGraphicsUploadDefinitions.ChunkCount * sizeof(ushort)) ||
                address is EndingSourceDefinitions.Artwork.EscapeACharacters or
                EndingSourceDefinitions.Artwork.EscapeAPackedMap or
                EndingSourceDefinitions.Artwork.EscapeBCharacters or
                EndingSourceDefinitions.Artwork.EscapeBPackedMap or
                EndingSourceDefinitions.Artwork.ExplosionCharacters or
                EndingSourceDefinitions.Artwork.ExplosionPackedMap or
                EndingSourceDefinitions.Artwork.PostCreditsFloorCharacters,
            address => $"Ending art reread cartridge source ${address:X6}.");
        return guard;
    }
}
