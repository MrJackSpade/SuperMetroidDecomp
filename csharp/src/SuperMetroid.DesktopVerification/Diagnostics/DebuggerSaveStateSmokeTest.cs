using SuperMetroid.Core.Assets;
using SuperMetroid.Diagnostics.Saves;
using SuperMetroid.Diagnostics.Sessions;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core;
using SuperMetroid.Diagnostics;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.Diagnostics.SaveStates;

namespace SuperMetroid.DesktopVerification.Diagnostics;

/// <summary>Headless save/advance/load deterministic-continuation audit.</summary>
public static class DebuggerSaveStateSmokeTest
{
    /// <summary>Uses the repository installation to check content/build warnings, exact saved continuation, and wrong-ROM/schema rejection.</summary>
    /// <param name="romPath">Retail cartridge path.</param>
    /// <remarks>Deletes the temporary state slots afterward; mismatches throw rather than returning a failed result.</remarks>
    public static DebuggerSaveStateSmokeTestResult Run(string romPath)
    {
        string fullRomPath = Path.GetFullPath(romPath);
        CartridgeImportAddressSpace bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(fullRomPath);
        using var temporaryDirectoryScratch = new TestScratchDirectory("state-audit");
        string temporaryDirectory = temporaryDirectoryScratch.Root;
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            GameInstallation installation = RepositoryInstallation.Installation;
            var maps = installation.LoadMaps();
            var content = new InstalledSessionContent(installation.LoadGameArtwork(maps, installation.LoadProjectiles(), installation.LoadEnemyTiles(), installation.LoadSamusBody()), installation.LoadAudioClips().Clips);
            var game = GameSessionComposer.Create(new SuperMetroid.Core.Saves.SaveSlotStore(), SuperMetroid.Core.Gameplay.GameplayAssists.None, content, storage: null);
            for (int frame = 0; frame < 90; frame++)
                game.Step(0);
            GameContentIdentity contentIdentity = GameContentIdentity.Create(
                new string('A', 64),
                new string('B', 64),
                new string('C', 64),
                Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
                new Dictionary<string, string> { ["room-layouts"] = new string('E', 64) });
            var store = DebuggerSaveStateStoreTooling.ForCartridge(fullRomPath, bus.Rom, contentIdentity, content, temporaryDirectory);
            bool emptySlotReported = !store.TryLoad(9, out _);
            if (!emptySlotReported)
                throw new InvalidDataException("An empty debugger slot was reported as occupied.");
            DebuggerSaveStateMetadata saved = store.Save(0, game);
            if (store.Load(0).Warnings.Count != 0)
                throw new InvalidDataException("Same-build debugger state emitted a build warning.");
            DebuggerSaveStateLoadResult installedLoad = DebuggerSaveStateStore.ForInstalledGame(
                temporaryDirectory,
                hostOptions: null,
                contentIdentity,
                content,
                directoryOverride: temporaryDirectory).Load(0);
            if (installedLoad.Warnings.Count != 0)
                throw new InvalidDataException("Installer-verified debugger state identity did not match the retail source revision.");
            VerifyInstalledRecorder(temporaryDirectory, SuperMetroid.Diagnostics.Saves.SaveRamCodec.Encode(game.Saves, maps), contentIdentity);
            // Change only the two build IDs: the exact graph must remain loadable and its
            // deterministic continuation below must still match, including PCM and pixels.
            using (var changedBuild = new FileStream(saved.Path, FileMode.Open, FileAccess.Write))
            {
                changedBuild.Position = DebuggerStateFormat.Magic.Length + sizeof(int);
                changedBuild.Write(Guid.NewGuid().ToByteArray());
                changedBuild.Write(Guid.NewGuid().ToByteArray());
            }
            long stateBytes = new FileInfo(saved.Path).Length;
            const int continuationFrames = 45;
            var expected = new FrontendFrame[continuationFrames];
            var expectedPixels = new RgbaPixel[continuationFrames][];
            var expectedPcm = new short[continuationFrames][];
            var expectedAcknowledgements = new SoundAcknowledgements<SoundEffectLibrary, SoundEffectId>[continuationFrames];
            var expectedAudioEvents = new AudioEvent[continuationFrames][];
            for (int frame = 0; frame < continuationFrames; frame++)
            {
                expected[frame] = game.Step(0);
                expectedPixels[frame] = game.PublishedPixels();
                expectedPcm[frame] = new short[AudioSession.PcmSamplesPerFrame];
                game.CopyAudioFrame(expectedPcm[frame]);
                expectedAcknowledgements[frame] = game.AudioAcknowledgementsForVerification;
                expectedAudioEvents[frame] = [.. game.AudioEventsForVerification];
            }

            DebuggerSaveStateLoadResult loaded = store.Load(0);
            if (loaded.Warnings.Count != 2)
                throw new InvalidDataException("Compatible cross-build state did not emit both build warnings.");
            // The real hosts bind a restored game to its content and save storage as they bind a new one.
            loaded.BindGame(content, new GameSaveFile(Path.Combine(temporaryDirectory, "SuperMetroid.save.json")));

            GameContentIdentity changedAudioIdentity = GameContentIdentity.Create(
                new string('D', 64),
                contentIdentity.MapContentSha256,
                contentIdentity.ProjectileContentSha256,
                contentIdentity.CompiledDefinitionsBuildId,
                contentIdentity.AdditionalContentSha256);
            DebuggerSaveStateLoadResult changedAudio = DebuggerSaveStateStoreTooling.ForCartridge(
                fullRomPath, bus.Rom, changedAudioIdentity, content, temporaryDirectory).Load(0);
            if (changedAudio.Warnings.Count != 3 ||
                !changedAudio.Warnings.Any(warning => warning.Contains("audio", StringComparison.Ordinal)))
            {
                throw new InvalidDataException(
                    "Selected-audio drift did not supplement the two assembly-build warnings.");
            }
            if (loaded.Game.FrameNumber != saved.FrameNumber)
            {
                throw new InvalidDataException(
                    $"Restored frame {loaded.Game.FrameNumber} != saved frame {saved.FrameNumber}.");
            }
            for (int frame = 0; frame < continuationFrames; frame++)
            {
                FrontendFrame actual = loaded.Game.Step(0);
                var actualPcm = new short[AudioSession.PcmSamplesPerFrame];
                loaded.Game.CopyAudioFrame(actualPcm);
                SoundAcknowledgements<SoundEffectLibrary, SoundEffectId> actualAcknowledgements =
                    loaded.Game.AudioAcknowledgementsForVerification;
                FrontendFrame wanted = expected[frame];
                if (actual.GameState != wanted.GameState ||
                    actual.FrameNumber != wanted.FrameNumber ||
                    actual.Phase != wanted.Phase ||
                    !loaded.Game.PublishedPixels().SequenceEqual(expectedPixels[frame]) ||
                    !loaded.Game.AudioEventsForVerification.SequenceEqual(expectedAudioEvents[frame]) ||
                    !actualPcm.SequenceEqual(expectedPcm[frame]) ||
                    !actualAcknowledgements.Equals(expectedAcknowledgements[frame]))
                {
                    throw new InvalidDataException(
                        $"Debugger-state continuation diverged on relative frame {frame} " +
                        $"(actual {actual.GameState}/{actual.FrameNumber}/{actual.Phase}, " +
                        $"expected {wanted.GameState}/{wanted.FrameNumber}/{wanted.Phase}).");
                }
            }

            byte[] wrongRom = bus.Rom.ToArray();
            wrongRom[0] ^= 1;
            var wrongStore = DebuggerSaveStateStoreTooling.ForCartridge(fullRomPath, wrongRom, contentIdentity, content, temporaryDirectory);
            bool wrongRomRejected = false;
            try
            {
                wrongStore.Load(0);
            }
            catch (InvalidDataException error) when (
                error.Message.Contains("different ROM", StringComparison.Ordinal))
            {
                wrongRomRejected = true;
            }
            if (!wrongRomRejected)
                throw new InvalidDataException("Debugger state did not reject a different ROM digest.");

            byte[] incompatible = File.ReadAllBytes(saved.Path);
            BitConverter.GetBytes(int.MaxValue).CopyTo(incompatible, DebuggerStateFormat.Magic.Length);
            File.WriteAllBytes(store.GetSlotPath(1), incompatible);
            bool schemaRejected = false;
            try { store.Load(1); }
            catch (InvalidDataException error) when (error.Message.StartsWith(
                DebuggerObjectGraphSerializer.UnsupportedVersion, StringComparison.Ordinal))
            { schemaRejected = true; }
            if (!schemaRejected) throw new InvalidDataException("Unknown state schema was accepted.");

            VerifyNamedDelegateRoundTrip();
            Console.WriteLine(
                "Debugger states: named content/build warnings, exact continuation, ROM and unknown-schema " +
                "rejection and named delegates agree.");

            return new DebuggerSaveStateSmokeTestResult(
                saved.FrameNumber,
                continuationFrames,
                stateBytes,
                wrongRomRejected,
                emptySlotReported);
        }
        finally
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }

    /// <summary>Checks that graph serialization restores a named generic delegate with its original method identity.</summary>
    private static void VerifyNamedDelegateRoundTrip()
    {
        using var stream = new MemoryStream();
        Func<int, int> method = Identity<int>;
        DebuggerObjectGraphSerializer.Serialize(stream, method);
        stream.Position = 0;
        Func<int, int> restored = DebuggerObjectGraphSerializer.Deserialize<Func<int, int>>(stream);
        if (restored(42) != 42 || restored.Method != method.Method)
            throw new InvalidDataException("Named generic delegate identity did not round-trip.");
    }

    /// <summary>Checks that an installed recording retains its retail source digest, controller input, and selected content identity.</summary>
    private static void VerifyInstalledRecorder(
        string dataDirectory,
        ReadOnlySpan<byte> saveRam,
        GameContentIdentity contentIdentity)
    {
        string recorderRoot = Path.Combine(dataDirectory, "installed-recorder");
        string recordingPath;
        using (ControllerInputRecorder recorder = ControllerInputRecorder.StartInstalled(
                   recorderRoot,
                   saveRam,
                   new SuperMetroid.Diagnostics.Configuration.SuperMetroidGameOptions(),
                   contentIdentity))
        {
            recordingPath = recorder.Path;
            recorder.RecordFrame(RecordedButtons);
        }

        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        if (!recording.RomSha256.SequenceEqual(SupportedCartridge.CreateSha256Digest()) ||
            recording.ControllerInputs.Length != 1 || recording.InputAt(0) != RecordedButtons ||
            recording.ContentIdentity is null)
        {
            throw new InvalidDataException(
                "Installed recorder did not persist the verified source and selected-content identities.");
        }
    }

    /// <summary>The buttons the installed recorder check records and reads back.</summary>
    private const ControllerButton RecordedButtons = ControllerButton.R | ControllerButton.L | ControllerButton.Left | ControllerButton.Start;

    /// <summary>Provides a named generic method whose identity can be compared after debugger-state serialization.</summary>
    private static T Identity<T>(T value) => value;
}
