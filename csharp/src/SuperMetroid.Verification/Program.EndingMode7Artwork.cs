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
    private static void VerifyEndingMode7Artwork(GameInstallation installation)
    {
        var nativeBus = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        EndingMode7ArtworkCatalog stock = installation.LoadEndingMode7Art();
        foreach (EndingMode7SceneId id in Enum.GetValues<EndingMode7SceneId>())
        {
            (int characterSource, int packedSource) = Sources(id);
            byte[] nativeCharacters = RomDataReader.Decompress(nativeBus, characterSource,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            byte[] nativePacked = RomDataReader.Decompress(nativeBus, packedSource,
                EndingCreditsRomData.Rendering.DecompressionLimit);
            EndingMode7SceneArtwork compiledScene = stock[id];
            AssertTrue(compiledScene.Characters.Span.SequenceEqual(nativeCharacters.AsSpan(0,
                    EndingMode7ArtworkFormat.CharacterByteCount)),
                $"installed ending {id} PNG preserves every high-lane character byte");
            for (int cell = 0; cell < EndingMode7ArtworkFormat.MapByteCount; cell++)
                AssertEqual(nativePacked[cell * 2], compiledScene.Map.Span[cell],
                    $"installed ending {id} map preserves low-lane cell {cell}");
        }

        var guardedBus = new EndingMode7SourceReadGuard(
            SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"));
        var nativeAudio = new CartridgeAudioState();
        var installedAudio = new CartridgeAudioState();
        var native = new EndingCreditsState(nativeBus, nativeAudio, 0, 0);
        var installed = new EndingCreditsState(guardedBus, installedAudio, 0, 0);
        installed.BindMode7Artwork(stock);
        var phases = new HashSet<EndingCreditsPhase>();
        for (int frame = 0; frame < 10000 &&
            native.Phase != EndingCreditsPhase.ZebesExplosionTileUpload; frame++)
        {
            AssertEqual(native.Phase, installed.Phase,
                $"installed ending Mode-7 art preserves phase at frame {frame}");
            bool first = phases.Add(native.Phase);
            if (first || frame % 173 == 0)
            {
                LayeredRenderSnapshot expected = native.CaptureRenderSnapshot();
                LayeredRenderSnapshot actual = installed.CaptureRenderSnapshot();
                AssertTrue(actual.Memory.Vram.SequenceEqual(expected.Memory.Vram) &&
                        actual.Memory.Cgram.SequenceEqual(expected.Memory.Cgram) &&
                        SoftwareLayeredSnapshotRenderer.Render(actual).AsSpan().SequenceEqual(
                            SoftwareLayeredSnapshotRenderer.Render(expected)),
                    $"installed ending art preserves native VRAM and pixels at frame {frame}");
            }
            native.Step();
            installed.Step();
            nativeAudio.AdvanceFrame(nativeBus, default);
            installedAudio.AdvanceFrame(guardedBus, default);
        }
        AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, native.Phase,
            "ending art fixture reaches the post-explosion flyaway upload");
        AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, installed.Phase,
            "installed art preserves the flyaway handoff");
        AssertTrue(phases.Contains(EndingCreditsPhase.WaitForEscapeMusic) &&
                phases.Contains(EndingCreditsPhase.FadeInEscapeSceneB) &&
                phases.Contains(EndingCreditsPhase.FadeInZebesExplosion),
            "all three ending Mode-7 scenes were exercised");
        AssertEqual(0, guardedBus.ForbiddenReadAttempts,
            "installed ending Mode-7 scenes never read their six visual ROM sources");

        Directory.CreateDirectory(installation.EndingMode7OverrideDirectory);
        foreach (EndingMode7SceneId id in Enum.GetValues<EndingMode7SceneId>())
        {
            EndingCreditsPhase target = id switch
            {
                EndingMode7SceneId.EscapeA => EndingCreditsPhase.WaitForEscapeMusic,
                EndingMode7SceneId.EscapeB => EndingCreditsPhase.FadeInEscapeSceneB,
                EndingMode7SceneId.PlanetExplosion => EndingCreditsPhase.FadeInZebesExplosion,
                _ => throw new ArgumentOutOfRangeException(nameof(id)),
            };
            var sceneBus = new EndingMode7SourceReadGuard(
                SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"));
            var audio = new CartridgeAudioState();
            var state = new EndingCreditsState(sceneBus, audio, 0, 0);
            state.BindMode7Artwork(stock);
            for (int frame = 0; frame < 10000 && state.Phase != target; frame++)
            {
                state.Step();
                audio.AdvanceFrame(sceneBus, default);
            }
            AssertEqual(target, state.Phase, $"ending {id} override fixture reaches its scene");
            for (int visibleFrame = 0; visibleFrame < 1200 &&
                state.Brightness == 0; visibleFrame++)
            {
                state.Step();
                audio.AdvanceFrame(sceneBus, default);
            }
            LayeredRenderSnapshot baselineSnapshot = state.CaptureRenderSnapshot();
            AssertTrue(baselineSnapshot.Brightness > 0,
                $"ending {id} override fixture reaches a visible frame");
            byte[] baseline = baselineSnapshot.Memory.Vram.ToArray();
            Rgba32[] baselinePixels = SoftwareLayeredSnapshotRenderer.Render(baselineSnapshot);

            string mapName = EndingMode7ArtworkFormat.MapFileName(id);
            string mapOverride = Path.Combine(installation.EndingMode7OverrideDirectory, mapName);
            EndingMode7MapDocument map;
            using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory, mapName)))
                map = JsonSerializer.Deserialize<EndingMode7MapDocument>(input,
                    MapPresentationFormat.JsonOptions)
                    ?? throw new InvalidDataException($"Ending {id} map JSON is empty.");
            Array.Fill(map.Tiles, 0);
            using (var output = File.Create(mapOverride))
                EndingMode7SceneArtwork.WriteMap(output, map);
            EndingMode7ArtworkCatalog editedMap = installation.LoadEndingMode7Art();
            state.BindMode7Artwork(editedMap);
            LayeredRenderSnapshot mapSnapshot = state.CaptureRenderSnapshot();
            byte[] mapVram = mapSnapshot.Memory.Vram.ToArray();
            AssertTrue(!baseline.SequenceEqual(mapVram) &&
                    baseline.AsSpan(0x8000).SequenceEqual(mapVram.AsSpan(0x8000)) &&
                    !baselinePixels.AsSpan().SequenceEqual(
                        SoftwareLayeredSnapshotRenderer.Render(mapSnapshot)),
                $"edited ending {id} map changes visible pixels, but only its Mode-7 VRAM lanes");
            if (id == EndingMode7SceneId.PlanetExplosion)
            {
                var partialBus = new EndingMode7SourceReadGuard(
                    SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"));
                var partialAudio = new CartridgeAudioState();
                var partial = new EndingCreditsState(partialBus, partialAudio, 0, 0);
                partial.BindMode7Artwork(stock);
                for (int frame = 0; frame < 2000 &&
                    partial.Phase != EndingCreditsPhase.ZebesExplosionTileUpload; frame++)
                {
                    partial.Step();
                    partialAudio.AdvanceFrame(partialBus, default);
                }
                AssertEqual(EndingCreditsPhase.ZebesExplosionTileUpload, partial.Phase,
                    "explosion override fixture reaches partial flyaway DMA");
                for (int chunk = 0; chunk < 9; chunk++)
                {
                    partial.Step();
                    partialAudio.AdvanceFrame(partialBus, default);
                }
                byte[] partiallyUploaded = partial.CaptureRenderSnapshot().Memory.Vram.ToArray();
                partial.BindMode7Artwork(editedMap);
                byte[] rebound = partial.CaptureRenderSnapshot().Memory.Vram.ToArray();
                for (int word = 0; word < EndingCreditsRomData.Rendering.FlyawayUploadBytes; word++)
                    AssertEqual(partiallyUploaded[word * 2], rebound[word * 2],
                        $"edited explosion retains already-transferred map word {word}");
                AssertTrue(!partiallyUploaded.AsSpan(
                        EndingCreditsRomData.Rendering.FlyawayUploadBytes * 2,
                        EndingCreditsRomData.Rendering.Mode7Bytes * 2 -
                            EndingCreditsRomData.Rendering.FlyawayUploadBytes * 2)
                    .SequenceEqual(rebound.AsSpan(
                        EndingCreditsRomData.Rendering.FlyawayUploadBytes * 2,
                        EndingCreditsRomData.Rendering.Mode7Bytes * 2 -
                            EndingCreditsRomData.Rendering.FlyawayUploadBytes * 2)),
                    "edited explosion updates only the not-yet-transferred Mode-7 map region");
                AssertEqual(0, partialBus.ForbiddenReadAttempts,
                    "partial explosion rebind never reads authored cartridge sources");
            }
            File.Delete(mapOverride);
            state.BindMode7Artwork(stock);

            string pngName = EndingMode7ArtworkFormat.CharacterFileName(id);
            string pngOverride = Path.Combine(installation.EndingMode7OverrideDirectory, pngName);
            IndexedPngImage image;
            using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory, pngName)))
                image = IndexedPng.Read(input, EndingMode7ArtworkFormat.CharacterWidth,
                    EndingMode7ArtworkFormat.CharacterHeight);
            Array.Fill(image.Pixels, (byte)0);
            using (var output = File.Create(pngOverride))
                IndexedPng.Write(output, image.Width, image.Height, image.Pixels, image.Palette);
            EndingMode7ArtworkCatalog editedCharacters = installation.LoadEndingMode7Art();
            state.BindMode7Artwork(editedCharacters);
            LayeredRenderSnapshot characterSnapshot = state.CaptureRenderSnapshot();
            byte[] characterVram = characterSnapshot.Memory.Vram.ToArray();
            AssertTrue(!baseline.SequenceEqual(characterVram) &&
                    baseline.AsSpan(0x8000).SequenceEqual(characterVram.AsSpan(0x8000)) &&
                    !baselinePixels.AsSpan().SequenceEqual(
                        SoftwareLayeredSnapshotRenderer.Render(characterSnapshot)),
                $"edited ending {id} PNG changes visible pixels, but only its Mode-7 VRAM lanes");
            File.Delete(pngOverride);
            AssertEqual(0, sceneBus.ForbiddenReadAttempts,
                $"ending {id} edits/rebinds avoid the six cartridge art sources");
        }

        string invalidMapPath = Path.Combine(installation.EndingMode7OverrideDirectory,
            EndingMode7ArtworkFormat.MapFileName(EndingMode7SceneId.EscapeA));
        File.WriteAllBytes(invalidMapPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingMode7Art(),
            "malformed ending Mode-7 override fails loudly");
        File.Delete(invalidMapPath);
        EndingMode7MapDocument persistentMap;
        using (var input = File.OpenRead(Path.Combine(installation.EndingMode7Directory,
                   EndingMode7ArtworkFormat.MapFileName(EndingMode7SceneId.EscapeA))))
            persistentMap = JsonSerializer.Deserialize<EndingMode7MapDocument>(input,
                MapPresentationFormat.JsonOptions)
                ?? throw new InvalidDataException("Stock ending map is empty.");
        persistentMap.Tiles[0] = (persistentMap.Tiles[0] + 1) & 255;
        using (var output = File.Create(invalidMapPath))
            EndingMode7SceneArtwork.WriteMap(output, persistentMap);
        byte[] selectedMap = installation.LoadEndingMode7Art()[EndingMode7SceneId.EscapeA]
            .Map.ToArray();
        string stockPngPath = Path.Combine(installation.EndingMode7Directory,
            EndingMode7ArtworkFormat.CharacterFileName(EndingMode7SceneId.EscapeA));
        File.WriteAllBytes(stockPngPath, [0]);
        AssertThrows<InvalidDataException>(() => installation.LoadEndingMode7Art(),
            "corrupt stock ending Mode-7 PNG cannot be hidden by a valid override");
        GameInstallation repaired = GameAssetInstaller.EnsureInstalled(installation.Root)
            ?? throw new InvalidOperationException("Ending art vanished during stock repair.");
        AssertTrue(repaired.LoadEndingMode7Art()[EndingMode7SceneId.EscapeA]
                .Characters.Span.SequenceEqual(stock[EndingMode7SceneId.EscapeA].Characters.Span),
            "stock ending Mode-7 PNG is regenerated from the installed cartridge");
        AssertTrue(repaired.LoadEndingMode7Art()[EndingMode7SceneId.EscapeA]
                .Map.Span.SequenceEqual(selectedMap),
            "stock ending art repair preserves the external map override");
        File.Delete(invalidMapPath);
        VerifyEndingRewardIconArtwork(installation);
        Console.WriteLine("Ending Mode-7 art: three native scenes plus the reward icon, guarded runtime and independent edits pass.");
    }

    private static (int Characters, int PackedMap) Sources(EndingMode7SceneId id) => id switch
    {
        EndingMode7SceneId.EscapeA =>
            (EndingCreditsRomData.Assets.EscapeMapA, EndingCreditsRomData.Assets.EscapeCharactersA),
        EndingMode7SceneId.EscapeB =>
            (EndingCreditsRomData.Assets.EscapeMapB, EndingCreditsRomData.Assets.EscapeCharactersB),
        EndingMode7SceneId.PlanetExplosion =>
            (EndingCreditsRomData.Assets.ExplosionMap, EndingCreditsRomData.Assets.ExplosionCharacters),
        _ => throw new ArgumentOutOfRangeException(nameof(id)),
    };

    private sealed class EndingMode7SourceReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address is EndingCreditsRomData.Assets.EscapeMapA or
                EndingCreditsRomData.Assets.EscapeCharactersA or
                EndingCreditsRomData.Assets.EscapeMapB or
                EndingCreditsRomData.Assets.EscapeCharactersB or
                EndingCreditsRomData.Assets.ExplosionMap or
                EndingCreditsRomData.Assets.ExplosionCharacters or
                EndingCreditsRomData.Assets.PostCreditsMode7Characters)
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Ending art reread cartridge source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
