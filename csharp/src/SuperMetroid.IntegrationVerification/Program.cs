using SuperMetroid.Core.Rendering;
using SuperMetroid.Diagnostics.Recording;
using SuperMetroid.IntegrationVerification.Android;
using SuperMetroid.IntegrationVerification.Gameplay;
using SuperMetroid.IntegrationVerification.Installation;
namespace SuperMetroid.IntegrationVerification;
using SuperMetroid.Diagnostics.Android;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.WindowsConsole;

/// <summary>
/// Repository-only integration checks: the gitignored ROM, its shared extracted installation and checked-in fixtures.
/// </summary>
internal static class Program
{
    /// <summary>Dispatches repository installation checks and verifies Android state continuation, diagnostic export, and recording integrity under a no-dialog error boundary.</summary>
    private static int Main(string[] args)
    {
        try
        {
            // Match the existing console-host policy before import, replay or any native
            // work. Failures belong in the outer stderr/nonzero boundary, never a dialog.
            if (OperatingSystem.IsWindows())
                NativeConsoleErrors.DisableDialogs();
            // Every command reads only the repository: the gitignored ROM, its shared extracted
            // installation and checked-in fixtures. File-taking tools live in SuperMetroid.DebugRunner.
            if (args is ["--extracted-validation-contract"])
                return ExtractedInstallationValidationVerification.Run(RepositoryInstallation.Installation.Root);
            if (args is ["--map-presentation-json-contracts"])
                return MapPresentationJsonContractVerification.Run(RepositoryInstallation.Installation.Root);
            if (args is ["--projectile-host-binding"])
                return ProjectileHostBindingVerification.Run();
            // CI has no ROM: --asset-import checks only ROM-independent input validation.
            if (args is ["--asset-import"])
                return AssetImportVerification.Run(romPath: null);
            if (args is ["--asset-import-rom"])
                return AssetImportVerification.Run(Path.GetFullPath("Super Metroid.smc"));
            if (args is ["--override-installation-lifecycle"])
                return InstallationOverrideLifecycleVerification.Run(Path.GetFullPath("Super Metroid.smc"));
            if (args is ["--rom-decompression"])
                return RomDecompressionVerification.Run();
            AndroidSessionCommandVerification.Run();
            AndroidFrameMailboxVerification.Run();
            if (args is ["--render-allocation"])
                return GameplayRenderAllocationVerification.Run();
            var boundedTrace = new AndroidResumeTrace(2);
            boundedTrace.Record("frame", "first");
            boundedTrace.Record("write", "second");
            boundedTrace.Record("frame", "overflow");
            if (!boundedTrace.Full) throw new InvalidDataException("Resume trace exceeded its capacity.");
            string? traceText = null;
            boundedTrace.Flush(text => traceText = text);
            if (traceText is null || !traceText.Contains("first") || !traceText.Contains("second") || traceText.Contains("overflow"))
                throw new InvalidDataException("Resume trace lost or exceeded bounded events.");
            boundedTrace.Flush(_ => throw new InvalidDataException("Already flushed trace emitted twice."));
            Console.WriteLine("PASS bounded resume trace: capacity, event retention, flush once.");
            using var rootScratch = new TestScratchDirectory("android-state-test");
        string root = rootScratch.Root;
        Directory.CreateDirectory(root);
            try
            {
                string rom = Path.GetFullPath("Super Metroid.smc");
                string audio = RepositoryInstallation.Installation.AudioDirectory;
                using var data = new AndroidSessionData(root, rom, audio);
                if (!data.LoadSlot(9).Contains("empty", StringComparison.Ordinal))
                    throw new InvalidDataException("Missing slot did not return a nonfatal message.");
                for (int i = 1; i <= 90; i++) Advance(data, i);
                data.SaveSlot(0);
                var expected = Enumerable.Range(91, 30).Select(frame => Advance(data, frame)).ToArray();
                data.LoadSlot(0);
                for (int i = 0; i < expected.Length; i++)
                {
                    var actual = Advance(data, 91 + i);
                    if (actual != expected[i]) throw new InvalidDataException($"State continuation diverged at frame {i}: {actual} != {expected[i]}.");
                }
                data.FlushRecording();
                string recordings = Path.Combine(root, "input-recordings");
                string[] journals = Directory.GetFiles(recordings, "*.smrec");
                if (journals.Length != 2) throw new InvalidDataException("Reset and state-load must create separate journals.");
                if (Directory.GetFiles(recordings, "*.seed.smstate").Length != 1)
                    throw new InvalidDataException("State-load recording omitted its preserved exact seed.");
                string seedPath = Directory.GetFiles(recordings, "*.seed.smstate").Single();
                byte[] originalSeed = File.ReadAllBytes(seedPath);
                data.PersistSave();
                string installed = Directory.CreateDirectory(Path.Combine(root, "game")).FullName;
                File.WriteAllText(Path.Combine(installed, "excluded.smc"), "installed asset sentinel");
                string unrelatedSlot = Path.Combine(root, "debug-states", "SuperMetroid-debug-slot-8.smstate");
                File.WriteAllText(unrelatedSlot, "unselected slot sentinel");
                File.WriteAllText(Path.Combine(root, "resume-timing.log"), "synthetic timing trace");
                File.WriteAllText(Path.Combine(root, "frame-handoff.log"), "synthetic handoff trace");
                string exported = Path.Combine(root, "diagnostics.zip");
                AndroidDiagnosticBundle.Create(root, exported, 0);
                using (var zip = System.IO.Compression.ZipFile.OpenRead(exported))
                {
                    foreach (string required in new[] { "bundle.json", "SuperMetroid.save.json", "SuperMetroid.ini", "resume-timing.log", "frame-handoff.log",
                        "debug-states/SuperMetroid-debug-slot-0.smstate", "input-recordings/" + Path.GetFileName(seedPath) })
                        if (zip.GetEntry(required) is null) throw new InvalidDataException($"Export omitted {required}.");
                    foreach (string journal in journals)
                    {
                        using var entry = (zip.GetEntry("input-recordings/" + Path.GetFileName(journal)) ?? throw new InvalidOperationException("Verification requires zip.GetEntry(\"input-recordings/\" + Path.GetFileName(journal)).")).Open();
                        ControllerInputRecording.Read(entry);
                    }
                    using var seedEntry = (zip.GetEntry("input-recordings/" + Path.GetFileName(seedPath)) ?? throw new InvalidOperationException("Verification requires zip.GetEntry(\"input-recordings/\" + Path.GetFileName(seedPath)).")).Open();
                    using var copy = new MemoryStream();
                    seedEntry.CopyTo(copy);
                    if (!originalSeed.SequenceEqual(copy.ToArray())) throw new InvalidDataException("Export modified the exact replay seed.");
                    if (zip.Entries.Any(entry => entry.FullName.StartsWith("game/", StringComparison.Ordinal)))
                        throw new InvalidDataException("Export included installed game assets.");
                    if (zip.GetEntry("debug-states/SuperMetroid-debug-slot-8.smstate") is not null)
                        throw new InvalidDataException("Export included an unselected user state.");
                }
                File.Delete(unrelatedSlot);
                bool preventedOverwrite = false;
                try { AndroidDiagnosticBundle.Create(root, exported, 0); }
                catch (IOException) { preventedOverwrite = true; }
                if (!preventedOverwrite) throw new InvalidDataException("Export overwrote an existing artifact.");
                Console.WriteLine("PASS Android diagnostic ZIP: save/config/state/recording inclusion, byte-identical seed, valid recordings, and overwrite refusal.");
                data.SaveSlot(0);
                if (!originalSeed.SequenceEqual(File.ReadAllBytes(seedPath)))
                    throw new InvalidDataException("Overwriting the visible slot changed the recording seed.");

                // A corrupt user-selected slot must fail before replacing any live state.
                var originalGame = data.Game;
                var originalBus = data.Bus;
                long originalGeneration = data.Generation;
                string slotPath = Directory.GetFiles(Path.Combine(root, "debug-states"), "*.smstate").Single();
                File.WriteAllBytes(slotPath, [0, 1, 2, 3]);
                bool rejected = false;
                try { data.LoadSlot(0); }
                catch (InvalidDataException) { rejected = true; }
                if (!rejected || !ReferenceEquals(originalGame, data.Game) ||
                    !ReferenceEquals(originalBus, data.Bus) ||
                    originalGeneration != data.Generation)
                    throw new InvalidDataException("Corrupt state load changed the live session or was accepted.");
                foreach (string path in journals)
                {
                    using var input = File.OpenRead(path);
                    var recording = ControllerInputRecording.Read(input);
                    if (recording.ControllerInputs.Length is not (30 or 120))
                        throw new InvalidDataException("Recording contains an unexpected number of frames.");
                }
                Console.WriteLine("PASS Android session: empty/corrupt slots, 30 exact video/audio continuation frames, separate reset/state journals and seed preserved after slot overwrite.");
                AndroidImportVerification.Run(root, rom, audio, seedPath);
            }
            finally
            {
                // This exact directory was created uniquely by this test, never supplied by a user.
                Directory.Delete(root, recursive: true);
            }
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    /// <summary>Advances one captured Android frame and hashes its rendered pixels and PCM samples for exact save-state continuation comparisons.</summary>
    private static SessionAdvance Advance(AndroidSessionData data, long sequence)
    {
        data.Record(0);
        var frame = data.Game.StepCaptured(0, sequence, data.Generation);
        var pixels = SoftwareFrameSnapshotRenderer.Render(frame.Snapshot);
        var samples = new short[CartridgeAudioSession.PcmSamplesPerFrame];
        data.Game.CopyAudioFrame(samples);
        uint pixelHash = 2166136261, pcmHash = 2166136261;
        foreach (var pixel in pixels)
        {
            pixelHash = unchecked((pixelHash ^ pixel.R) * 16777619);
            pixelHash = unchecked((pixelHash ^ pixel.G) * 16777619);
            pixelHash = unchecked((pixelHash ^ pixel.B) * 16777619);
            pixelHash = unchecked((pixelHash ^ pixel.A) * 16777619);
        }
        foreach (short sample in samples) pcmHash = unchecked((pcmHash ^ (ushort)sample) * 16777619);
        return new SessionAdvance(pixelHash, pcmHash, frame.Frame.FrameNumber);
    }
}
