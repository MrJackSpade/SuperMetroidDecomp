using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Audio;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Desktop;

public readonly record struct DebuggerSaveStateSmokeTestResult(
    ushort SavedFrame,
    int ContinuationFrames,
    long StateFileBytes,
    bool WrongRomRejected,
    bool EmptySlotReported);

/// <summary>Headless save/advance/load deterministic-continuation audit.</summary>
public static class DebuggerSaveStateSmokeTest
{
    public static DebuggerSaveStateSmokeTestResult Run(string romPath)
    {
        string fullRomPath = Path.GetFullPath(romPath);
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        var game = new SuperMetroidGame(bus, new SuperMetroidGameOptions());
        using var audio = new SpcAudioEngine();
        for (int frame = 0; frame < 90; frame++)
        {
            FrontendFrame current = game.Step(0);
            audio.RenderFrame(current.AudioCommands);
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
        }

        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"SuperMetroid-state-audit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            GameContentIdentity contentIdentity = GameContentIdentity.Create(
                new string('A', 64),
                new string('B', 64),
                new string('C', 64),
                Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"));
            var store = new DebuggerSaveStateStore(
                fullRomPath,
                bus.Rom,
                temporaryDirectory,
                contentIdentity: contentIdentity);
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
                expected[frame] = game.Step(0);
                expectedPcm[frame] = audio.RenderFrame(expected[frame].AudioCommands).ToArray();
                expectedAcknowledgements[frame] = audio.ReadAcknowledgements();
                game.SetAudioAcknowledgements(expectedAcknowledgements[frame]);
            }

            DebuggerSaveStateLoadResult loaded = store.Load(0);
            if (loaded.Warnings.Count != 2)
                throw new InvalidDataException("Compatible cross-build state did not emit both build warnings.");

            GameContentIdentity changedAudioIdentity = GameContentIdentity.Create(
                new string('D', 64),
                contentIdentity.MapContentSha256,
                contentIdentity.ProjectileContentSha256,
                contentIdentity.CompiledDefinitionsBuildId);
            DebuggerSaveStateLoadResult changedAudio = new DebuggerSaveStateStore(
                fullRomPath,
                bus.Rom,
                temporaryDirectory,
                contentIdentity: changedAudioIdentity).Load(0);
            if (changedAudio.Warnings.Count != 3 ||
                !changedAudio.Warnings.Any(warning => warning.Contains("audio", StringComparison.Ordinal)))
            {
                throw new InvalidDataException(
                    "Selected-audio drift did not supplement the two assembly-build warnings.");
            }
            using var restoredAudio = new SpcAudioEngine(
                ExtractedAudioAssetCatalog.Load(ExtractedAudioAssetLocator.FindAudioDirectory()),
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
            var wrongStore = new DebuggerSaveStateStore(
                fullRomPath,
                wrongRom,
                temporaryDirectory);
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
            catch (InvalidDataException error) when (error.Message.Contains("schema", StringComparison.Ordinal))
            { schemaRejected = true; }
            if (!schemaRejected) throw new InvalidDataException("Unknown state schema was accepted.");

            var legacyStore = new DebuggerSaveStateStore(fullRomPath, bus.Rom, temporaryDirectory);
            legacyStore.Save(3, bus, game, audio.Player);
            DebuggerSaveStateLoadResult legacy = store.Load(3);
            if (legacy.Warnings.Count != 1 ||
                !legacy.Warnings[0].Contains("no installed-content identity", StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    "Named-delegate schema-three state did not produce the legacy content warning.");
            }

            VerifyNamedDelegateRoundTrip();
            string repository = Path.GetDirectoryName(fullRomPath)!;
            foreach (var fixture in new[]
            {
                ("issue-350-grounded-grapple-floor-clip", "slot-0-named.smstate"),
                ("issue-353-gravity-chozo-hands", "slot-9-named.smstate"),
            })
            {
                string fixturePath = Path.Combine(repository, "csharp", "test-fixtures", fixture.Item1, fixture.Item2);
                File.Copy(fixturePath, store.GetSlotPath(2), overwrite: true);
                DebuggerSaveStateLoadResult preserved = store.Load(2);
                Console.WriteLine($"Loaded preserved named fixture {fixture.Item1}: room={preserved.Metadata.RoomPointer:X4}.");
            }
            Console.WriteLine(
                "Debugger compatibility: schema-three migration, schema-four content/build " +
                "warnings, exact continuation, ROM rejection and named delegates agree.");

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
