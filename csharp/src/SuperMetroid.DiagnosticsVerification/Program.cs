using SuperMetroid.Android;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;

try
{
    if (args is ["--maridia-pipe-entry"])
        return MaridiaPipeEntryAudit.Run();
    if (args is ["--legacy-options-migration"])
        return LegacyOptionsMigrationVerification.Run();
    if (args is ["--export-autonomous-performance-state", var frameText, var autonomousDestination])
        return AutonomousPerformanceStateFixture.Export(int.Parse(frameText), autonomousDestination);
    if (args is ["--compare-assembly-metadata", var originalAssembly, var linkedAssembly])
        return AssemblyMetadataVerification.Run(originalAssembly, linkedAssembly);
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
    if (args is ["--compare-file-select-capture", var journalPath, var wavePath])
        return FileSelectCaptureComparison.Run(journalPath, wavePath);
    if (args is ["--replay-android-bundle", var bundle, var recordingName])
        return AndroidBundleReplay.Run(bundle, recordingName);
    if (args is ["--export-ceres-descent-state", var destination])
        return CeresDescentStateFixture.Export(destination);
    if (args is ["--export-reported-eye-state", var eyeDestination])
        return ReportedEyeStateFixture.Export(eyeDestination);
    if (args is ["--export-room-performance-state", var scene, var performanceDestination])
        return RoomPerformanceStateFixture.Export(scene, performanceDestination);
    string root = Directory.CreateTempSubdirectory("SuperMetroid-android-state-test-").FullName;
    try
    {
        string rom = Path.GetFullPath("Super Metroid.smc");
        string audio = Path.GetFullPath("standalone-assets/audio");
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
                using var entry = zip.GetEntry("input-recordings/" + Path.GetFileName(journal))!.Open();
                ControllerInputRecording.Read(entry);
            }
            using var seedEntry = zip.GetEntry("input-recordings/" + Path.GetFileName(seedPath))!.Open();
            using var copy = new MemoryStream();
            seedEntry.CopyTo(copy);
            if (!originalSeed.AsSpan().SequenceEqual(copy.ToArray())) throw new InvalidDataException("Export modified the exact replay seed.");
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
        if (!originalSeed.AsSpan().SequenceEqual(File.ReadAllBytes(seedPath)))
            throw new InvalidDataException("Overwriting the visible slot changed the recording seed.");

        // A corrupt user-selected slot must fail before replacing any live state.
        var originalGame = data.Game;
        var originalAudio = data.Audio;
        var originalBus = data.Bus;
        long originalGeneration = data.Generation;
        string slotPath = Directory.GetFiles(Path.Combine(root, "debug-states"), "*.smstate").Single();
        File.WriteAllBytes(slotPath, [0, 1, 2, 3]);
        bool rejected = false;
        try { data.LoadSlot(0); }
        catch (InvalidDataException) { rejected = true; }
        if (!rejected || !ReferenceEquals(originalGame, data.Game) ||
            !ReferenceEquals(originalAudio, data.Audio) || !ReferenceEquals(originalBus, data.Bus) ||
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

static (uint Pixels, uint Pcm, ushort Frame) Advance(AndroidSessionData data, long sequence)
{
    data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
    data.Record(0);
    var frame = data.Game.StepCaptured(0, sequence, data.Generation);
    var pixels = frame.Snapshot is { } snapshot ? SoftwareFrameSnapshotRenderer.Render(snapshot) : frame.Frame.Pixels;
    short[] samples = data.Audio.RenderFrame(frame.Frame.AudioCommands);
    uint pixelHash = 2166136261, pcmHash = 2166136261;
    foreach (var pixel in pixels)
    {
        pixelHash = unchecked((pixelHash ^ pixel.R) * 16777619);
        pixelHash = unchecked((pixelHash ^ pixel.G) * 16777619);
        pixelHash = unchecked((pixelHash ^ pixel.B) * 16777619);
        pixelHash = unchecked((pixelHash ^ pixel.A) * 16777619);
    }
    foreach (short sample in samples) pcmHash = unchecked((pcmHash ^ (ushort)sample) * 16777619);
    return (pixelHash, pcmHash, frame.Frame.FrameNumber);
}
