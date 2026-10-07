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
        CartridgeImportAddressSpace bus = CartridgeImportAddressSpace.LoadRetailRom(fullRomPath);
        string temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            $"SuperMetroid-state-audit-{Guid.NewGuid():N}");
        Directory.CreateDirectory(temporaryDirectory);
        try
        {
            GameInstallation installation = GameAssetInstaller.Install(fullRomPath,
                Path.Combine(temporaryDirectory, "installed"));
            var game = new SuperMetroidGame(bus, new SuperMetroidGameOptions());
            var maps = installation.LoadMaps();
            game.BindMapPresentation(maps);
            using var audio = new SpcAudioEngine(installation.AudioDirectory);
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
            VerifyNamedComponentStateCompatibility(store, saved.Path);
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
            loaded.Game.BindMapPresentation(maps);

            GameContentIdentity changedAudioIdentity = GameContentIdentity.Create(
                new string('D', 64),
                contentIdentity.MapContentSha256,
                contentIdentity.ProjectileContentSha256,
                contentIdentity.CompiledDefinitionsBuildId,
                contentIdentity.AdditionalContentSha256);
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

            // Slot three's genuine legacy envelope was built from the same saved graph
            // by VerifyNamedComponentStateCompatibility; new saves require an identity.
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
                "Debugger compatibility: schema-three/four migration, schema-five named content/build " +
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

    /// <summary>
    /// Strip only the new table to reconstruct a genuine schema-four envelope around the
    /// unchanged graph. This proves old states remain loadable and missing identities warn.
    /// </summary>
    private static void VerifyNamedComponentStateCompatibility(DebuggerSaveStateStore store, string savedPath)
    {
        byte[] current = File.ReadAllBytes(savedPath);
        int tableOffset = DebuggerStateFormat.Magic.Length + sizeof(int) +
            2 * DebuggerStateFormat.GuidBytes + DebuggerStateFormat.DigestBytes +
            sizeof(int) + DebuggerStateFormat.GuidBytes + 4 * DebuggerStateFormat.DigestBytes;
        using var table = new MemoryStream(current, writable: false);
        table.Position = tableOffset;
        IReadOnlyDictionary<string, byte[]> components = GameContentComponentFormat.Read(table);
        if (components.Count != 1 || !components.ContainsKey("room-layouts"))
            throw new InvalidDataException("Schema-five state omitted the named room identity.");
        int tableEnd = checked((int)table.Position);
        var old = new byte[current.Length - (tableEnd - tableOffset)];
        current.AsSpan(0, tableOffset).CopyTo(old);
        current.AsSpan(tableEnd).CopyTo(old.AsSpan(tableOffset));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(
            old.AsSpan(DebuggerStateFormat.Magic.Length), DebuggerStateFormat.IdentifiedVersion);
        File.WriteAllBytes(store.GetSlotPath(4), old);
        DebuggerSaveStateLoadResult restored = store.Load(4);
        if (restored.Warnings.Count != 1 ||
            !restored.Warnings[0].Contains("no room-layouts content identity", StringComparison.Ordinal))
            throw new InvalidDataException("Schema-four state did not warn about its absent room identity.");
        if (restored.Game.FrameNumber != store.Load(0).Game.FrameNumber)
            throw new InvalidDataException("Schema-four migration shifted the state graph.");

        int identityOffset = DebuggerStateFormat.Magic.Length + sizeof(int) +
            2 * DebuggerStateFormat.GuidBytes + DebuggerStateFormat.DigestBytes;
        var legacy = new byte[current.Length - (tableEnd - identityOffset)];
        current.AsSpan(0, identityOffset).CopyTo(legacy);
        current.AsSpan(tableEnd).CopyTo(legacy.AsSpan(identityOffset));
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(
            legacy.AsSpan(DebuggerStateFormat.Magic.Length), DebuggerStateFormat.NamedDelegateVersion);
        File.WriteAllBytes(store.GetSlotPath(3), legacy);
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
