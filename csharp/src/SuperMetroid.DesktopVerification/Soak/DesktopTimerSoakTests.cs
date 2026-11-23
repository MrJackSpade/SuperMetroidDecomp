using System.Diagnostics;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Runtime;

using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
using SuperMetroid.Desktop.Window;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Rendering.Direct3D11.Telemetry;
namespace SuperMetroid.DesktopVerification.Soak;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

/// <summary>Measures bounded desktop timer and GPU telemetry in gameplay and paused scenes.</summary>
internal static class DesktopTimerSoakTests
{
    /// <summary>Runs the production WinForms timer for both scenes and reports frames, latency, memory, and audio health.</summary>
    internal static async Task RunDesktopTimerSoak(int seconds)
    {
        if (seconds is < 5 or > 300) throw new ArgumentOutOfRangeException(nameof(seconds));
        var results = new List<TimerSoakResult>();
        foreach (bool paused in new[] { false, true })
        {
            // The installed host path writes player data and debugger slots into its root, so
            // each scene runs on a private copy of the repository installation.
            using var copy = InstalledFixtures.CreatePrivateCopy();
            var options = new HostOptions { MasterVolumePercent = 0,
                Game = new SuperMetroidGameOptions { SkipOpeningCinematic = true, Invincibility = true } };
            CreateDesktopSoakSeed(copy.Installation, options.Game, paused);
            using var form = new GameForm(copy.Installation, options);
            var control = form.gameControl;
            _ = form.Handle; _ = control.Handle;
            control.SetPlaying(false);
            await control.LoadDebuggerState(0);
            var game = control.game ?? throw new InvalidOperationException("Host has no game.");
            var counter = control.frameTimings;
            var samples = new List<double>(seconds * 60 + 120);
            double discardedWallClockFrames = 0;
            void RecordDiscard(double count) => discardedWallClockFrames += count;
            using var process = Process.GetCurrentProcess();
            var memory = new List<SoakMemorySample>(seconds / 30 + 2);
            void Measure(long ticks)
            {
                if (samples.Count == samples.Capacity) throw new InvalidOperationException("Desktop timer exceeded bounded 60 Hz sample capacity.");
                samples.Add(ticks * 1000.0 / Stopwatch.Frequency);
            }
            try
            {
                // The bounded five-minute run fits completely in this history.
                // Normal desktop workers retain their smaller rolling window.
                await control.InitializeRendererAsync((window, clientSize, generation) =>
                    new D3D11RenderWorker(window, clientSize, generation, D3D11DeviceKind.Hardware,
                        RenderTelemetryLimits.MaximumHistoryCapacity));
                var worker = control.gpuWorker ?? throw new InvalidOperationException("Host has no GPU worker.");
                string adapter = await worker.Ready;
                var output = control.audioDevice ?? throw new InvalidOperationException("Host has no audio device.");
                counter.GameUpdateMeasured += Measure;
                counter.LateFramesRecorded += RecordDiscard;
                int startFrame = game.FrameNumber;
                control.SetPlaying(true);
                var clock = Stopwatch.StartNew();
                memory.Add(SoakMemorySample.Capture(process, 0));
                int nextReport = 30;
                while (clock.Elapsed.TotalSeconds < seconds)
                {
                    // Only the production WinForms timer advances the game here.
                    await Task.Delay(25);
                    worker.ThrowIfFaulted();
                    bool isPaused = game.IsAt(PauseStep.FadingIn) || game.IsAt(PauseStep.Menu);
                    Check(paused ? isPaused : game.GameState == SuperMetroidGameState.MainGameplay, "desktop soak left its requested scene");
                    if (clock.Elapsed.TotalSeconds >= nextReport)
                    {
                        Console.WriteLine($"Desktop timer paused={paused}: {nextReport}/{seconds}s, frames={samples.Count}, drains={output.QueueHealth.EmptyBeforeRefill}");
                        memory.Add(SoakMemorySample.Capture(process, clock.Elapsed.TotalSeconds));
                        nextReport += 30;
                    }
                }
                double elapsed = clock.Elapsed.TotalSeconds;
                // Take health before SetPlaying(false) intentionally resets the native queue.
                var health = output.QueueHealth;
                var renderer = worker.CaptureTimings();
                foreach (var history in new[] { renderer.CpuComposition, renderer.CpuDisplayAndPresent,
                    renderer.GpuComposition, renderer.GpuCompositionAndDisplay, worker.CaptureUploadTimings() })
                    Check(history.Retained == history.Observed - history.WarmupExcluded,
                        "soak timing history discarded samples; whole-run percentile claim is invalid");
                Check(worker.SubmittedUploadBytes > 0 && worker.SubmittedUploadCalls > 0
                    && worker.CaptureUploadTimings().Observed > 0, "desktop renderer did not publish upload telemetry");
                control.SetPlaying(false);
                counter.GameUpdateMeasured -= Measure;
                counter.LateFramesRecorded -= RecordDiscard;
                memory.Add(SoakMemorySample.Capture(process, elapsed));
                Check(unchecked((ushort)(game.FrameNumber - startFrame)) == samples.Count, "desktop frame/timing count mismatch");
                samples.Sort();
                double Percentile(double p) => samples[(int)Math.Ceiling(samples.Count * p) - 1];
                results.Add(new TimerSoakResult(Paused: paused, Seconds: elapsed, Frames: samples.Count, Fps: samples.Count / elapsed,
                    ProducerP50Ms: Percentile(.5), ProducerP95Ms: Percentile(.95), ProducerP99Ms: Percentile(.99),
                    ProducerMaximumMs: samples[^1],
                    ProducerOverDeadlineFrames: samples.Count(sample => sample > 1000.0 / 60),
                    DiscardedWallClockFrames: discardedWallClockFrames, Memory: memory,
                    Adapter: adapter, Audio: health, Renderer: renderer,
                    UploadCpu: worker.CaptureUploadTimings(), SubmittedUploadBytes: worker.SubmittedUploadBytes, SubmittedUploadCalls: worker.SubmittedUploadCalls,
                    PresentedFrames: worker.PresentedFrames, OccludedFrames: worker.OccludedFrames, MailboxMetrics: worker.MailboxMetrics));
                Console.WriteLine($"Desktop timer paused={paused}: {samples.Count / elapsed:F3} fps, p95={Percentile(.95):F3}ms, drains={health.EmptyBeforeRefill}.");
            }
            finally
            {
                counter.GameUpdateMeasured -= Measure;
                counter.LateFramesRecorded -= RecordDiscard;
                await control.StopRendererAsync();
            }
            form.Dispose();
        }
        Console.WriteLine("Desktop timer soak report: " + JsonSerializer.Serialize(new TimerSoakReport(Scope: "Production PlayableGameControl WinForms timer, silent real waveOut, hardware GPU; " +
            "hidden HWND, not visible presentation.",
            OperatingSystem: System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            Runtime: System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Processor: Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
            TimestampUtc: DateTimeOffset.UtcNow, CoreBuild: global::SuperMetroid.Core.BuildIdentity.Version,
            DesktopBuild: global::SuperMetroid.Desktop.BuildIdentity.Version, Results: results),
            global::SuperMetroid.DesktopVerification.DesktopVerificationJson.Info<TimerSoakReport>(
                new JsonSerializerOptions { WriteIndented = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals })));
    }

    /// <summary>The desktop timer soak's report: what ran, on which machine and builds, and each run's result.</summary>
    /// <param name="Scope">What the soak exercises.</param>
    /// <param name="OperatingSystem">The operating system.</param>
    /// <param name="Runtime">The .NET runtime.</param>
    /// <param name="Processor">The processor, when the environment names it.</param>
    /// <param name="TimestampUtc">When the report was written.</param>
    /// <param name="CoreBuild">The simulation's build.</param>
    /// <param name="DesktopBuild">The desktop host's build.</param>
    /// <param name="Results">Each run's result.</param>
    internal sealed record TimerSoakReport(string Scope, string OperatingSystem, string Runtime, string? Processor,
        DateTimeOffset TimestampUtc, Guid CoreBuild, Guid DesktopBuild, List<TimerSoakResult> Results);

    /// <summary>One desktop timer run's timing, memory, audio and renderer measurements.</summary>
    /// <param name="Paused">Whether the run was paused.</param>
    /// <param name="Seconds">The run's length.</param>
    /// <param name="Frames">The frames it produced.</param>
    /// <param name="Fps">Its frames per second.</param>
    /// <param name="ProducerP50Ms">The producer's median frame time.</param>
    /// <param name="ProducerP95Ms">The producer's 95th-percentile frame time.</param>
    /// <param name="ProducerP99Ms">The producer's 99th-percentile frame time.</param>
    /// <param name="ProducerMaximumMs">The producer's longest frame time.</param>
    /// <param name="ProducerOverDeadlineFrames">The frames that took longer than a sixtieth of a second.</param>
    /// <param name="DiscardedWallClockFrames">The late frames the timer discarded.</param>
    /// <param name="Memory">The process's memory, sampled through the run.</param>
    /// <param name="Adapter">The GPU adapter.</param>
    /// <param name="Audio">The audio queue's health.</param>
    /// <param name="Renderer">The renderer's timings.</param>
    /// <param name="UploadCpu">The renderer's upload timings.</param>
    /// <param name="SubmittedUploadBytes">The bytes the renderer uploaded.</param>
    /// <param name="SubmittedUploadCalls">The uploads the renderer made.</param>
    /// <param name="PresentedFrames">The frames presented.</param>
    /// <param name="OccludedFrames">The frames the hidden window occluded.</param>
    /// <param name="MailboxMetrics">The render mailbox's counters.</param>
    internal sealed record TimerSoakResult(bool Paused, double Seconds, int Frames, double Fps, double ProducerP50Ms,
        double ProducerP95Ms, double ProducerP99Ms, double ProducerMaximumMs, int ProducerOverDeadlineFrames,
        double DiscardedWallClockFrames, List<SoakMemorySample> Memory, string Adapter,
        global::SuperMetroid.Desktop.Audio.WaveOutQueueHealthSnapshot Audio,
        global::SuperMetroid.Rendering.Direct3D11.Telemetry.RenderWorkerTimings Renderer,
        global::SuperMetroid.Rendering.Direct3D11.Telemetry.RenderTimingDistribution UploadCpu, long SubmittedUploadBytes,
        long SubmittedUploadCalls, long PresentedFrames, long OccludedFrames,
        global::SuperMetroid.Rendering.Direct3D11.Frames.RenderMailboxMetrics MailboxMetrics);

    /// <summary>Creates a gameplay or paused debugger slot for the desktop timer run.</summary>
    private static void CreateDesktopSoakSeed(SuperMetroid.AssetExtraction.Installation.GameInstallation installation,
        SuperMetroidGameOptions options, bool paused)
    {
        var clips = DesktopAccess.LoadAudioClips();
        var game = InstalledFixtures.CreateGameWithAudio(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps),
            clips, options.ToAssists());
        void Step(ControllerButton input) => game.StepCaptured(input);
        for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? ControllerButton.Start : ControllerButton.None);
        Check(game.GameState == SuperMetroidGameState.MainGameplay, "desktop soak seed startup failed");
        game.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(paused ? RoomDefinitions.GlassTunnel : RoomDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        game.GameplayRuntimeForVerification.FrameBoundary.BeginAcceptedFrame(0);
        Step(0);
        if (paused) Step(ControllerButton.Start);
        for (int warmup = 0; warmup < 120; warmup++) Step(0);
        var identity = SuperMetroid.AssetExtraction.Installation.GameContentIdentity.Create(installation.LoadAudioClips(),
            installation.LoadMaps(), installation.LoadProjectiles());
        DebuggerSaveStateStore.ForInstalledGame(installation.Root, options, identity,
            new SuperMetroid.Diagnostics.Sessions.InstalledSessionContent(RepositoryInstallation.Artwork, clips)).Save(0, game);
    }
}
