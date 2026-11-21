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
            Measure(RoomDefinitions.AlphaPowerBomb, paused: false),
            Measure(RoomDefinitions.GlassTunnel, paused: true),
        };
        Console.WriteLine("Simulation timing baseline: " + JsonSerializer.Serialize(new ProfileReport
        {
            Scope = "Unpaced captured simulation + real managed desktop audio adapter; no GPU, waveOut or presentation",
            TimestampUtc = DateTimeOffset.UtcNow,
            OS = RuntimeInformation.OSDescription,
            Runtime = RuntimeInformation.FrameworkDescription,
            Cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            CoreBuild = global::SuperMetroid.Core.BuildIdentity.Version,
            DiagnosticBuild = global::SuperMetroid.RenderVerification.BuildIdentity.Version,
            WarmupFrames = 120,
            MeasuredFrames = 600,
            Results = results,
        }, global::SuperMetroid.RenderVerification.RenderVerificationJson.Info<ProfileReport>(IndentedJson)));
    }

    /// <summary>Warms one installed room, captures unpaced frames with desktop audio, and summarizes their timing samples.</summary>
    private static ProfileResult Measure(RoomDefinition room, bool paused)
    {
        var game = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps),
            new SuperMetroid.Core.Gameplay.GameplayAssists { SkipOpeningCinematic = true, SamusCannotDie = true });
        game.BindAudio(DesktopAccess.LoadAudioClips());
        var pcm = new short[AudioSession.PcmSamplesPerFrame];
        long nonzeroPcmSamples = 0;
        SimulationStepCost Step(ControllerButton input)
        {
            long allocationStart = GC.GetAllocatedBytesForCurrentThread();
            long start = Stopwatch.GetTimestamp();
            game.StepCaptured(input);
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
            Step(tick % 47 == 0 ? ControllerButton.Start : ControllerButton.None);
        if (game.GameState != SuperMetroidGameState.MainGameplay) throw new InvalidOperationException("Profile failed startup.");
        game.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
        game.GameplayRuntimeForVerification.FrameBoundary.BeginAcceptedFrame(0);
        Step(0);
        if (paused)
        {
            Step(ControllerButton.Start);
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
        return new ProfileResult { Room = room.Name, Paused = paused, CaptureMs = Distribution(capture), AudioMs = Distribution(mixing),
            TotalMs = combined, AllocationBytes = Distribution(allocated), PcmSamples = 600 * AudioSession.PcmSamplesPerFrame,
            NonzeroPcmSamples = nonzeroPcmSamples };
    }

    /// <summary>Accepts either of the game's two stable pause states during the paused profile.</summary>
    private static bool IsPaused(SuperMetroidGame game) => game.IsAt(PauseStep.FadingIn) || game.IsAt(PauseStep.Menu);
    /// <summary>Sorts measured samples and selects nearest-rank percentiles plus the maximum.</summary>
    private static Timing Distribution(double[] values)
    {
        Array.Sort(values);
        return new(values[(int)Math.Ceiling(values.Length * .50) - 1], values[(int)Math.Ceiling(values.Length * .95) - 1],
            values[(int)Math.Ceiling(values.Length * .99) - 1], values[^1]);
    }
    /// <summary>Nearest-rank latency or allocation values at the reported percentiles and maximum.</summary>
    internal readonly record struct Timing(double P50, double P95, double P99, double Maximum);

    /// <summary>The timing baseline's report: what was measured, on which machine and builds, and each fixture's result.</summary>
    internal sealed record ProfileReport
    {
        /// <summary>What the baseline measures.</summary>
        public required string Scope { get; init; }
        /// <summary>When the baseline ran.</summary>
        public required DateTimeOffset TimestampUtc { get; init; }
        /// <summary>The operating system.</summary>
        public required string OS { get; init; }
        /// <summary>The .NET runtime.</summary>
        public required string Runtime { get; init; }
        /// <summary>The processor, when the environment names it.</summary>
        public required string? Cpu { get; init; }
        /// <summary>The process architecture.</summary>
        public required string ProcessArchitecture { get; init; }
        /// <summary>The simulation's build.</summary>
        public required Guid CoreBuild { get; init; }
        /// <summary>The render checks' build.</summary>
        public required Guid DiagnosticBuild { get; init; }
        /// <summary>The frames run before measuring.</summary>
        public required int WarmupFrames { get; init; }
        /// <summary>The frames measured.</summary>
        public required int MeasuredFrames { get; init; }
        /// <summary>Each fixture's result.</summary>
        public required ProfileResult[] Results { get; init; }
    }

    /// <summary>One fixture's measured distributions.</summary>
    internal sealed record ProfileResult
    {
        /// <summary>The room measured.</summary>
        public required string Room { get; init; }
        /// <summary>Whether the game was paused.</summary>
        public required bool Paused { get; init; }
        /// <summary>Capture time per frame, in milliseconds.</summary>
        public required Timing CaptureMs { get; init; }
        /// <summary>Audio mixing time per frame, in milliseconds.</summary>
        public required Timing AudioMs { get; init; }
        /// <summary>Capture and mixing together per frame, in milliseconds.</summary>
        public required Timing TotalMs { get; init; }
        /// <summary>Bytes allocated per frame.</summary>
        public required Timing AllocationBytes { get; init; }
        /// <summary>The PCM samples the measured frames produced.</summary>
        public required int PcmSamples { get; init; }
        /// <summary>The PCM samples that were not silent.</summary>
        public required long NonzeroPcmSamples { get; init; }
    }
}
