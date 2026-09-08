using SuperMetroid.Android;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;

try
{
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
