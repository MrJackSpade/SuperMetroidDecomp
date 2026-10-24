using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core.Frontend;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
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

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>Compares installed ending Mode-7 scenes with retail art and checks live override rebinding.</summary>
internal static class EndingMode7ArtworkVerification
{
    /// <summary>Checks three installed scenes against decompressed cartridge data and rendered output, then verifies map/PNG edits, partial DMA preservation, and stock repair.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for ending Mode-7 scene artwork")]
    internal static void VerifyEndingMode7Artwork(GameInstallation installation)
    {
        var nativeBus = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        EndingFloorArtworkCatalog stock = installation.LoadEndingMode7Art();
        foreach (EndingFloorSceneId id in Enum.GetValues<EndingFloorSceneId>())
        {
            (int characterSource, int packedSource) = EndingCreditsRomData.Assets.FloorSources(id);
            byte[] nativeCharacters = RomDataReader.Decompress(nativeBus, characterSource,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            byte[] nativePacked = RomDataReader.Decompress(nativeBus, packedSource,
                EndingCreditsRomData.Rendering.DecompressionLimit);
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
        var nativeAudio = new AudioQueues();
        var installedAudio = new AudioQueues();
        var native = CreateRetailEndingFixture(nativeAudio, 0, 0);
        var installed = CreateRetailEndingFixture(installedAudio, 0, 0);
        installed.Stage.BindFloorArtwork(stock);
        var phases = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 10000 &&
            native.Stage.Phase != EndingCreditsPhase.ZebesExplosionTileUpload; frame++)
        {
            AssertEqual(native.Stage.Phase, installed.Stage.Phase,
                $"installed ending Mode-7 art preserves phase at frame {frame}");
            bool first = phases.Add(native.Stage.Phase);
            if (first || frame % 173 == 0)
            {
                LayeredScene expected = EndingCreditsPresentation.CaptureRenderSnapshot(native);
                byte[] expectedVram = native.ShownVram();
                LayeredScene actual = EndingCreditsPresentation.CaptureRenderSnapshot(installed);
                byte[] actualVram = installed.ShownVram();
                AssertTrue(actualVram.SequenceEqual(expectedVram) &&
                        actual.Paint.Colors.SequenceEqual(expected.Paint.Colors) &&
                        DisplayRendering.Render(actual).SequenceEqual(
                            DisplayRendering.Render(expected)),
                    $"installed ending art preserves native VRAM and pixels at frame {frame}");
            }
            native.Step();
            installed.Step();
            nativeAudio.AdvanceFrame(default);
            installedAudio.AdvanceFrame(default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, native.Stage.Phase,
            "ending art fixture reaches the post-explosion flyaway upload");
        AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, installed.Stage.Phase,
            "installed art preserves the flyaway handoff");
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
            Rgba32[] baselinePixels = DisplayRendering.Render(baselineSnapshot);

            string mapName = EndingFloorArtworkFormat.MapFileName(id);
            string mapOverride = Path.Combine(installation.EndingMode7OverrideDirectory, mapName);
            EndingFloorMapDocument map;
            using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory, mapName)))
                map = JsonSerializer.Deserialize<EndingFloorMapDocument>(input,
                    ArtworkJson.Options)
                    ?? throw new InvalidDataException($"Ending {id} map JSON is empty.");
            Array.Fill(map.Tiles, 0);
            using (var output = File.Create(mapOverride))
                EndingFloorSceneArtwork.WriteMap(output, map);
            EndingFloorArtworkCatalog editedMap = installation.LoadEndingMode7Art();
            state.Stage.BindFloorArtwork(editedMap);
            LayeredScene mapSnapshot = EndingCreditsPresentation.CaptureRenderSnapshot(state);
            byte[] mapSnapshotVram = state.ShownVram();
            byte[] mapVram = mapSnapshotVram.ToArray();
            AssertTrue(!baseline.SequenceEqual(mapVram) &&
                    baseline.AsSpan(0x8000).SequenceEqual(mapVram.AsSpan(0x8000)) &&
                    !baselinePixels.SequenceEqual(
                        DisplayRendering.Render(mapSnapshot)),
                $"edited ending {id} map changes visible pixels, but only its Mode-7 VRAM lanes");
            if (id == EndingFloorSceneId.PlanetExplosion)
            {
                var partialBus = NewEndingMode7SourceReadGuard(
                    SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"));
                var partialAudio = new AudioQueues();
                var partial = CreateRetailEndingFixture(partialAudio, 0, 0);
                partial.Stage.BindFloorArtwork(stock);
                for (int frame = 0; frame < 2000 &&
                    partial.Stage.Phase != EndingCreditsPhase.ZebesExplosionTileUpload; frame++)
                {
                    partial.Step();
                    partialAudio.AdvanceFrame(default);
                }
                AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, partial.Stage.Phase,
                    "explosion override fixture reaches partial flyaway DMA");
                for (int chunk = 0; chunk < 9; chunk++)
                {
                    partial.Step();
                    partialAudio.AdvanceFrame(default);
                }
                byte[] partiallyUploaded = partial.ShownVram().ToArray();
                partial.Stage.BindFloorArtwork(editedMap);
                byte[] rebound = partial.ShownVram().ToArray();
                for (int word = 0; word < EndingCreditsRomData.Rendering.FlyawayChunkBytes; word++)
                    AssertEqual(partiallyUploaded[word * 2], rebound[word * 2],
                        $"edited explosion retains already-transferred map word {word}");
                AssertTrue(!partiallyUploaded.AsSpan(
                        EndingCreditsRomData.Rendering.FlyawayChunkBytes * 2,
                        EndingCreditsRomData.Rendering.FloorBytes * 2 -
                            EndingCreditsRomData.Rendering.FlyawayChunkBytes * 2)
                    .SequenceEqual(rebound.AsSpan(
                        EndingCreditsRomData.Rendering.FlyawayChunkBytes * 2,
                        EndingCreditsRomData.Rendering.FloorBytes * 2 -
                            EndingCreditsRomData.Rendering.FlyawayChunkBytes * 2)),
                    "edited explosion updates only the not-yet-transferred Mode-7 map region");
                AssertEqual(0, partialBus.ForbiddenReadAttempts,
                    "partial explosion rebind never reads authored cartridge sources");
            }
            File.Delete(mapOverride);
            state.Stage.BindFloorArtwork(stock);

            string pngName = EndingFloorArtworkFormat.CharacterFileName(id);
            string pngOverride = Path.Combine(installation.EndingMode7OverrideDirectory, pngName);
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory, pngName)))
                image = IndexedPng.Read(input, new(EndingFloorArtworkFormat.CharacterWidth, EndingFloorArtworkFormat.CharacterHeight));
            Array.Fill(image.Pixels, (byte)0);
            using (var output = File.Create(pngOverride))
                IndexedPng.Write(output, new(image.Size.X, image.Size.Y), image.Pixels, image.Colours);
            EndingFloorArtworkCatalog editedCharacters = installation.LoadEndingMode7Art();
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
            EndingFloorSceneArtwork.WriteMap(output, persistentMap);
        byte[] selectedMap = installation.LoadEndingMode7Art()[EndingFloorSceneId.EscapeA]
            .Map.ToArray();
        string stockPngPath = Path.Combine(installation.EndingMode7Directory,
            EndingFloorArtworkFormat.CharacterFileName(EndingFloorSceneId.EscapeA));
        File.WriteAllBytes(stockPngPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingMode7Art(),
            "corrupt stock ending Mode-7 PNG cannot be hidden by a valid override");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ending art vanished during stock repair.");
        AssertTrue(repaired.LoadEndingMode7Art()[EndingFloorSceneId.EscapeA]
                .Characters.Span.SequenceEqual(stock[EndingFloorSceneId.EscapeA].Characters.Span),
            "stock ending Mode-7 PNG is regenerated from the installed cartridge");
        AssertTrue(repaired.LoadEndingMode7Art()[EndingFloorSceneId.EscapeA]
                .Map.Span.SequenceEqual(selectedMap),
            "stock ending art repair preserves the external map override");
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
                 EndingRewardJumpDefinitions.IconChunkCount * sizeof(ushort)) ||
                address is EndingCreditsRomData.Assets.EscapeMapA or
                EndingCreditsRomData.Assets.EscapeCharactersA or
                EndingCreditsRomData.Assets.EscapeMapB or
                EndingCreditsRomData.Assets.EscapeCharactersB or
                EndingCreditsRomData.Assets.ExplosionMap or
                EndingCreditsRomData.Assets.ExplosionCharacters or
                EndingCreditsRomData.Assets.PostCreditsFloorCharacters,
            address => $"Ending art reread cartridge source ${address:X6}.");
        return guard;
    }
}
