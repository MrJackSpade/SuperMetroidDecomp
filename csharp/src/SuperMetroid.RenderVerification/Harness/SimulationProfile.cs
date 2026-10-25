using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
namespace SuperMetroid.RenderVerification.Harness;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

/// <summary>Unpaced, room-local CPU baseline. Not a presentation, waveOut-underrun, or five-minute soak gate.</summary>
internal static class SimulationProfile
{
    /// <summary>JSON options: Indented output.</summary>
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    /// <summary>Prints 600-frame capture, audio, and allocation distributions for gameplay and pause fixtures.</summary>
    internal static void Run()
    {
        var results = new[]
        {
            Measure(RoomHeaderDefinitions.AlphaPowerBomb, paused: false),
            Measure(RoomHeaderDefinitions.GlassTunnel, paused: true),
        };
        Console.WriteLine("Simulation timing baseline: " + JsonSerializer.Serialize(new
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
        }, IndentedJson));
    }

    /// <summary>Warms one installed room, captures unpaced frames with desktop audio, and summarizes their timing samples.</summary>
    private static object Measure(RoomHeaderDefinition room, bool paused)
    {
        var game = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(),
            new SuperMetroidGameOptions { SkipOpeningCinematic = true, Invincibility = true });
        game.BindAudio(DesktopAccess.LoadAudioClips());
        var pcm = new short[AudioSession.PcmSamplesPerFrame];
        long sequence = 0;
        long nonzeroPcmSamples = 0;
        SimulationStepCost Step(SnesButton input)
        {
            long allocationStart = GC.GetAllocatedBytesForCurrentThread();
            long start = Stopwatch.GetTimestamp();
            game.StepCaptured(input, ++sequence, 1);
            long captured = Stopwatch.GetTimestamp();
            game.CopyAudioFrame(pcm);
            long end = Stopwatch.GetTimestamp();
            foreach (short value in pcm) if (value != 0) nonzeroPcmSamples++;
            return new SimulationStepCost((captured - start) * 1000.0 / Stopwatch.Frequency,
                (end - captured) * 1000.0 / Stopwatch.Frequency,
                (end - start) * 1000.0 / Stopwatch.Frequency,
                GC.GetAllocatedBytesForCurrentThread() - allocationStart);
        }
        for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? SnesButton.Start : SnesButton.None);
        if (game.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Profile failed startup.");
        game.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
        game.GameplayRuntimeForVerification.FrameBoundary.BeginAcceptedFrame(0);
        Step(0);
        if (paused)
        {
            Step(SnesButton.Start);
            for (int tick = 0; tick < 120 && !IsPaused(game); tick++) Step(0);
            if (!IsPaused(game)) throw new InvalidOperationException("Profile did not reach pause menu.");
        }
        for (int tick = 0; tick < 120; tick++) Step(0);
        double[] capture = new double[600], mixing = new double[600], total = new double[600], allocated = new double[600];
        nonzeroPcmSamples = 0;
        for (int tick = 0; tick < 600; tick++)
        {
            if (paused ? !IsPaused(game) : game.GameState != SuperMetroidGameState.MainGameplay)
                throw new InvalidOperationException("Profile left its requested scene.");
            var sample = Step(0);
            capture[tick] = sample.Capture; mixing[tick] = sample.Audio;
            total[tick] = sample.Total; allocated[tick] = sample.Allocation;
        }
        var combined = Distribution(total);
        if (nonzeroPcmSamples == 0) throw new InvalidOperationException("Audio timing baseline contained only silence.");
        Console.WriteLine($"Room {room.Name}, paused={paused}: simulation+audio p50/p95/p99 {combined.P50:F3}/{combined.P95:F3}/{combined.P99:F3}ms; allocation p50 {Distribution(allocated).P50:F0} bytes/frame.");
        return new { Room = room.Name, Paused = paused, CaptureMs = Distribution(capture), AudioMs = Distribution(mixing),
            TotalMs = combined, AllocationBytes = Distribution(allocated), PcmSamples = 600 * AudioSession.PcmSamplesPerFrame,
            NonzeroPcmSamples = nonzeroPcmSamples };
    }

    /// <summary>Accepts either of the game's two stable pause states during the paused profile.</summary>
    private static bool IsPaused(SuperMetroidGame game) => game.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
    /// <summary>Sorts measured samples and selects nearest-rank percentiles plus the maximum.</summary>
    private static Timing Distribution(double[] values)
    {
        Array.Sort(values);
        return new(values[(int)Math.Ceiling(values.Length * .50) - 1], values[(int)Math.Ceiling(values.Length * .95) - 1],
            values[(int)Math.Ceiling(values.Length * .99) - 1], values[^1]);
    }
    /// <summary>Nearest-rank latency or allocation values at the reported percentiles and maximum.</summary>
    private readonly record struct Timing(double P50, double P95, double P99, double Maximum);
}
