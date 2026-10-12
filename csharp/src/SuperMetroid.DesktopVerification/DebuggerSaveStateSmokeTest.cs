using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Desktop;

/// <summary>Measurements and rejection outcomes from the headless debugger-state compatibility and deterministic-continuation check.</summary>
/// <param name="SavedFrame">Sixteen-bit frontend frame number stored in the saved state.</param>
/// <param name="ContinuationFrames">Updates compared after restoration against the uninterrupted gameplay, pixel, PCM, and acknowledgement sequence.</param>
/// <param name="StateFileBytes">Saved state file size in bytes after its build identifiers are changed.</param>
/// <param name="WrongRomRejected">Whether loading with a modified cartridge digest produced the expected rejection.</param>
/// <param name="EmptySlotReported">Whether an unused slot was correctly reported as absent.</param>
public readonly record struct DebuggerSaveStateSmokeTestResult(
    ushort SavedFrame,
    int ContinuationFrames,
    long StateFileBytes,
    bool WrongRomRejected,
    bool EmptySlotReported);

/// <summary>Headless save/advance/load deterministic-continuation audit.</summary>
public static class DebuggerSaveStateSmokeTest
{
    /// <summary>Uses a temporary installation to check content/build warnings, exact saved continuation, and wrong-ROM/schema rejection.</summary>
    /// <param name="romPath">Retail cartridge path.</param>
    /// <remarks>Deletes the temporary installation and state slots afterward; mismatches throw rather than returning a failed result.</remarks>
    public static DebuggerSaveStateSmokeTestResult Run(string romPath)
    {
        string fullRomPath = Path.GetFullPath(romPath);
        CartridgeImportAddressSpace bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(fullRomPath);
        using var temporaryDirectoryScratch = new TestScratchDirectory("state-audit");
        string temporaryDirectory = temporaryDirectoryScratch.Root;
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(fullRomPath,
                Path.Combine(temporaryDirectory, "installed"));
            var game = new SuperMetroidGame(bus, new SuperMetroidGameOptions());
            var maps = installation.LoadMaps();
            game.BindArtwork(game.Artwork with
            {
                MapPresentation = maps,
            });
            using var audio = DesktopAccess.CreateAudioEngine(installation.AudioDirectory);
            for (int frame = 0; frame < 90; frame++)
            {
                FrontendFrame current = game.Step(0);
                audio.RenderFrame(current.AudioCommands);
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            }
            GameContentIdentity contentIdentity = GameContentIdentity.Create(
                new string('A', 64),
                new string('B', 64),
                new string('C', 64),
                Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
                new Dictionary<string, string> { ["room-layouts"] = new string('E', 64) });
            var store = DebuggerSaveStateStoreTooling.ForCartridge(fullRomPath, bus.Rom, contentIdentity, temporaryDirectory);
            bool emptySlotReported = !store.TryLoad(9, out _);
            if (!emptySlotReported)
                throw new InvalidDataException("An empty debugger slot was reported as occupied.");
            DebuggerSaveStateMetadata saved = store.Save(0, bus, game, audio.Player);
            if (store.Load(0).Warnings.Count != 0)
                throw new InvalidDataException("Same-build debugger state emitted a build warning.");
            DebuggerSaveStateLoadResult installedLoad = DebuggerSaveStateStore.ForInstalledGame(
                temporaryDirectory,
                hostOptions: null,
                contentIdentity,
                directoryOverride: temporaryDirectory).Load(0);
            if (installedLoad.Warnings.Count != 0)
                throw new InvalidDataException("Installer-verified debugger state identity did not match the retail source revision.");
            VerifyInstalledRecorder(temporaryDirectory, bus.SaveRam, contentIdentity);
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
            var expectedPcm = new short[continuationFrames][];
            var expectedAcknowledgements = new CartridgeAudioAcknowledgements[continuationFrames];
            for (int frame = 0; frame < continuationFrames; frame++)
            {
                expected[frame] = game.Step(0).WithCopiedPixels();
                expectedPcm[frame] = audio.RenderFrame(expected[frame].AudioCommands).ToArray();
                expectedAcknowledgements[frame] = audio.ReadAcknowledgements();
                game.SetAudioAcknowledgements(expectedAcknowledgements[frame]);
            }

            DebuggerSaveStateLoadResult loaded = store.Load(0);
            if (loaded.Warnings.Count != 2)
                throw new InvalidDataException("Compatible cross-build state did not emit both build warnings.");
            // The real hosts rebind nonserialized installed catalogs after restoration.
            // The audit must use that contract too, not an implicit cartridge fallback.
            loaded.Game.BindArtwork(loaded.Game.Artwork with
            {
                MapPresentation = maps,
            });

            GameContentIdentity changedAudioIdentity = GameContentIdentity.Create(
                new string('D', 64),
                contentIdentity.MapContentSha256,
                contentIdentity.ProjectileContentSha256,
                contentIdentity.CompiledDefinitionsBuildId,
                contentIdentity.AdditionalContentSha256);
            DebuggerSaveStateLoadResult changedAudio = DebuggerSaveStateStoreTooling.ForCartridge(
                fullRomPath, bus.Rom, changedAudioIdentity, temporaryDirectory).Load(0);
            if (changedAudio.Warnings.Count != 3 ||
                !changedAudio.Warnings.Any(warning => warning.Contains("audio", StringComparison.Ordinal)))
            {
                throw new InvalidDataException(
                    "Selected-audio drift did not supplement the two assembly-build warnings.");
            }
            using var restoredAudio = new SpcAudioEngine(
                installation.LoadAudio(),
                loaded.AudioPlayer ?? throw new InvalidDataException("Restored state omitted managed audio."));
            if (loaded.Game.FrameNumber != saved.FrameNumber)
            {
                throw new InvalidDataException(
                    $"Restored frame {loaded.Game.FrameNumber} != saved frame {saved.FrameNumber}.");
            }
            for (int frame = 0; frame < continuationFrames; frame++)
            {
                FrontendFrame actual = loaded.Game.Step(0);
                short[] actualPcm = restoredAudio.RenderFrame(actual.AudioCommands).ToArray();
                CartridgeAudioAcknowledgements actualAcknowledgements =
                    restoredAudio.ReadAcknowledgements();
                loaded.Game.SetAudioAcknowledgements(actualAcknowledgements);
                FrontendFrame wanted = expected[frame];
                if (actual.GameState != wanted.GameState ||
                    actual.FrameNumber != wanted.FrameNumber ||
                    actual.Phase != wanted.Phase ||
                    !actual.Pixels.AsSpan().SequenceEqual(wanted.Pixels) ||
                    !actual.AudioCommands.SequenceEqual(wanted.AudioCommands) ||
                    !actualPcm.AsSpan().SequenceEqual(expectedPcm[frame]) ||
                    actualAcknowledgements != expectedAcknowledgements[frame])
                {
                    throw new InvalidDataException(
                        $"Debugger-state continuation diverged on relative frame {frame} " +
                        $"(actual {actual.GameState}/{actual.FrameNumber}/{actual.Phase}, " +
                        $"expected {wanted.GameState}/{wanted.FrameNumber}/{wanted.Phase}).");
                }
            }

            byte[] wrongRom = bus.Rom.ToArray();
            wrongRom[0] ^= 1;
            var wrongStore = DebuggerSaveStateStoreTooling.ForCartridge(fullRomPath, wrongRom, contentIdentity, temporaryDirectory);
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
                   new SuperMetroidGameOptions(),
                   contentIdentity))
        {
            recordingPath = recorder.Path;
            recorder.RecordFrame(0x1234);
        }

        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        if (!recording.RomSha256.AsSpan().SequenceEqual(SupportedCartridge.CreateSha256Digest()) ||
            recording.ControllerInputs is not [0x1234] ||
            recording.ContentIdentity is null)
        {
            throw new InvalidDataException(
                "Installed recorder did not persist the verified source and selected-content identities.");
        }
    }

    private static T Identity<T>(T value) => value;
}
