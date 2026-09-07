using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;

/// <summary>Unpaced, room-local CPU baseline. Not a presentation, waveOut-underrun, or five-minute soak gate.</summary>
internal static class SimulationProfile
{
    internal static void Run()
    {
        VerifyPublicationProfiler();
        var results = new[]
        {
            Measure(SlowConsumerFixture.AlphaPowerBombRoom, paused: false),
            Measure(SimulationProfileRooms.MaridiaTube, paused: true),
        };
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "render-performance", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "simulation-profile.json");
        File.WriteAllText(path, JsonSerializer.Serialize(new
        {
            Scope = "Unpaced captured simulation + real managed desktop audio adapter; no GPU, waveOut or presentation",
            TimestampUtc = DateTimeOffset.UtcNow,
            OS = RuntimeInformation.OSDescription,
            Runtime = RuntimeInformation.FrameworkDescription,
            Cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            CoreBuild = typeof(SuperMetroidGame).Module.ModuleVersionId,
            DiagnosticBuild = typeof(SimulationProfile).Module.ModuleVersionId,
            WarmupFrames = 120,
            MeasuredFrames = 600,
            Results = results,
        }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Simulation timing baseline written to {path}");
    }

    private static object Measure(ushort room, bool paused)
    {
        var game = new SuperMetroidGame(SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"),
            new SuperMetroidGameOptions { SkipOpeningCinematic = true, Invincibility = true });
        using var audio = new SpcAudioEngine();
        using var publicationProfile = new RenderPublicationProfile();
        long sequence = 0;
        long nonzeroPcmSamples = 0;
        (double Capture, double Audio, double Total, long Allocation, double Publication, long PublicationAllocation) Step(ushort input)
        {
            publicationProfile.Reset();
            long allocationStart = GC.GetAllocatedBytesForCurrentThread();
            long start = Stopwatch.GetTimestamp();
            var frame = game.StepCaptured(input, ++sequence, 1);
            long captured = Stopwatch.GetTimestamp();
            if (frame.Snapshot is null || frame.Frame.Pixels.Length != 0)
                throw new InvalidOperationException("Profile unexpectedly entered a CPU raster path.");
            var pcm = audio.RenderFrame(frame.Frame.AudioCommands);
            if (pcm.Length != SpcAudioEngine.StereoFramesPerVideoFrame * SpcAudioEngine.ChannelCount)
                throw new InvalidOperationException("Profile lost an audio frame.");
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
            long end = Stopwatch.GetTimestamp();
            foreach (short value in pcm) if (value != 0) nonzeroPcmSamples++;
            return ((captured - start) * 1000.0 / Stopwatch.Frequency,
                (end - captured) * 1000.0 / Stopwatch.Frequency,
                (end - start) * 1000.0 / Stopwatch.Frequency,
                GC.GetAllocatedBytesForCurrentThread() - allocationStart,
                publicationProfile.Ticks * 1000.0 / Stopwatch.Frequency, publicationProfile.AllocatedBytes);
        }
        for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0);
        if (game.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Profile failed startup.");
        game.RuntimeForVerification!.LoadCartridgeRoomForDebug(room, 0, 0);
        game.RuntimeForVerification.RunNmi(0, true);
        Step(0);
        if (paused)
        {
            Step((ushort)SnesButton.Start);
            for (int tick = 0; tick < 120 && !IsPaused(game); tick++) Step(0);
            if (!IsPaused(game)) throw new InvalidOperationException("Profile did not reach pause menu.");
        }
        for (int tick = 0; tick < 120; tick++) Step(0);
        double[] capture = new double[600], mixing = new double[600], total = new double[600], allocated = new double[600];
        double[] publication = new double[600], publicationAllocated = new double[600];
        nonzeroPcmSamples = 0;
        for (int tick = 0; tick < 600; tick++)
        {
            if (paused ? !IsPaused(game) : game.GameState != SuperMetroidGameState.MainGameplay)
                throw new InvalidOperationException("Profile left its requested scene.");
            var sample = Step(0);
            capture[tick] = sample.Capture; mixing[tick] = sample.Audio;
            total[tick] = sample.Total; allocated[tick] = sample.Allocation;
            publication[tick] = sample.Publication; publicationAllocated[tick] = sample.PublicationAllocation;
            if (publicationProfile.Publications == 0 || sample.Publication > sample.Capture || sample.PublicationAllocation > sample.Allocation)
                throw new InvalidOperationException("Publication profile missing or exceeds its enclosing frame interval.");
        }
        var combined = Distribution(total);
        if (nonzeroPcmSamples == 0) throw new InvalidOperationException("Audio timing baseline contained only silence.");
        Console.WriteLine($"Room $8F:{room:X4}, paused={paused}: simulation+audio p50/p95/p99 {combined.P50:F3}/{combined.P95:F3}/{combined.P99:F3}ms; allocation p50 {Distribution(allocated).P50:F0} bytes/frame.");
        return new { Room = $"8F:{room:X4}", Paused = paused, CaptureMs = Distribution(capture), AudioMs = Distribution(mixing),
            PublicationMs = Distribution(publication), PublicationAllocationBytes = Distribution(publicationAllocated),
            TotalMs = combined, AllocationBytes = Distribution(allocated), PcmSamples = 600 * SpcAudioEngine.StereoFramesPerVideoFrame * SpcAudioEngine.ChannelCount,
            NonzeroPcmSamples = nonzeroPcmSamples };
    }

    private static bool IsPaused(SuperMetroidGame game) => game.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
    private static void VerifyPublicationProfiler()
    {
        // Exclude the CLR's first thread-static/type initialization from steady-state recording.
        using (RenderPublicationProfile.Measure()) { }
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) { using var ignored = RenderPublicationProfile.Measure(); }
        if (GC.GetAllocatedBytesForCurrentThread() != before)
            throw new InvalidOperationException("Disabled publication profiling allocated memory.");
        using var profile = new RenderPublicationProfile();
        using (RenderPublicationProfile.Measure()) GC.KeepAlive(new byte[128]);
        if (profile.Publications != 1 || profile.AllocatedBytes < 128 || profile.Ticks < 0)
            throw new InvalidOperationException("Publication profiling missed work.");
        profile.Reset();
        if (profile.Publications != 0 || profile.AllocatedBytes != 0 || profile.Ticks != 0)
            throw new InvalidOperationException("Publication profile reset retained prior-frame work.");
        bool nestedRejected = false;
        try { using var nested = new RenderPublicationProfile(); }
        catch (InvalidOperationException) { nestedRejected = true; }
        if (!nestedRejected) throw new InvalidOperationException("Nested profiling would double count work.");
    }
    private static Timing Distribution(double[] values)
    {
        Array.Sort(values);
        return new(values[(int)Math.Ceiling(values.Length * .50) - 1], values[(int)Math.Ceiling(values.Length * .95) - 1],
            values[(int)Math.Ceiling(values.Length * .99) - 1], values[^1]);
    }
    private readonly record struct Timing(double P50, double P95, double P99, double Maximum);
}

internal static class SimulationProfileRooms
{
    /// <summary>Retail $8F:CEFB room header: Maridia glass tube, logical room $04/$01.</summary>
    internal const ushort MaridiaTube = 0xcefb;
}
