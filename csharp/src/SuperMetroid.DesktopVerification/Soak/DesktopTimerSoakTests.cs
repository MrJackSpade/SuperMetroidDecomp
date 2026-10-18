using System.Diagnostics;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Rendering.Direct3D11;
using SuperMetroid.Core.Runtime;

using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Desktop.Window;
using SuperMetroid.Diagnostics.SaveStates;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
using SuperMetroid.Rendering.Direct3D11.Telemetry;
namespace SuperMetroid.DesktopVerification.Soak;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms;

/// <summary>Measures bounded desktop timer and GPU telemetry in gameplay and paused scenes.</summary>
internal static class DesktopTimerSoakTests
{
    /// <summary>Runs the production WinForms timer for both scenes and reports frames, latency, memory, and audio health.</summary>
    internal static async Task RunDesktopTimerSoak(int seconds)
    {
        if (seconds is < 5 or > 300) throw new ArgumentOutOfRangeException(nameof(seconds));
        var results = new List<object>();
        foreach (bool paused in new[] { false, true })
        {
            // The installed host path writes player data and debugger slots into its root, so
            // each scene runs on a private copy of the repository installation.
            using var copy = InstalledFixtures.CreatePrivateCopy();
            var options = new SuperMetroidGameOptions { Renderer = RendererSelection.Direct3D11,
                SkipOpeningCinematic = true, Invincibility = true, MasterVolumePercent = 0 };
            CreateDesktopSoakSeed(copy.Installation, options, paused);
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
                counter.EmulatedFrameMeasured += Measure;
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
                    bool isPaused = game.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
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
                counter.EmulatedFrameMeasured -= Measure;
                counter.LateFramesRecorded -= RecordDiscard;
                memory.Add(SoakMemorySample.Capture(process, elapsed));
                Check(unchecked((ushort)(game.FrameNumber - startFrame)) == samples.Count, "desktop frame/timing count mismatch");
                samples.Sort();
                double Percentile(double p) => samples[(int)Math.Ceiling(samples.Count * p) - 1];
                results.Add(new { Paused = paused, Seconds = elapsed, Frames = samples.Count, Fps = samples.Count / elapsed,
                    ProducerP50Ms = Percentile(.5), ProducerP95Ms = Percentile(.95), ProducerP99Ms = Percentile(.99),
                    ProducerMaximumMs = samples[^1],
                    ProducerOverDeadlineFrames = samples.Count(sample => sample > 1000.0 / 60),
                    DiscardedWallClockFrames = discardedWallClockFrames, Memory = memory,
                    Adapter = adapter, Audio = health, Renderer = renderer,
                    UploadCpu = worker.CaptureUploadTimings(), worker.SubmittedUploadBytes, worker.SubmittedUploadCalls,
                    worker.PresentedFrames, worker.OccludedFrames, worker.MailboxMetrics });
                Console.WriteLine($"Desktop timer paused={paused}: {samples.Count / elapsed:F3} fps, p95={Percentile(.95):F3}ms, drains={health.EmptyBeforeRefill}.");
            }
            finally
            {
                counter.EmulatedFrameMeasured -= Measure;
                counter.LateFramesRecorded -= RecordDiscard;
                await control.StopRendererAsync();
            }
            form.Dispose();
        }
        Console.WriteLine("Desktop timer soak report: " + JsonSerializer.Serialize(new { Scope = "Production PlayableGameControl WinForms timer, silent real waveOut, hardware GPU; " +
            "hidden HWND, not visible presentation.",
            OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Processor = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
            TimestampUtc = DateTimeOffset.UtcNow, CoreBuild = typeof(SuperMetroidGame).Module.ModuleVersionId,
            DesktopBuild = typeof(PlayableGameControl).Module.ModuleVersionId, Results = results },
            new JsonSerializerOptions { WriteIndented = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals }));
    }

    /// <summary>Creates a gameplay or paused debugger slot for the desktop timer run.</summary>
    private static void CreateDesktopSoakSeed(SuperMetroid.AssetExtraction.Installation.GameInstallation installation,
        SuperMetroidGameOptions options, bool paused)
    {
        var bus = installation.OpenRuntimeAddressSpace();
        var game = InstalledFixtures.CreateGame(new SuperMetroid.Core.Saves.SaveSlotStore(), options);
        game.BindAudio(DesktopAccess.LoadAudioClips());
        long sequence = 0;
        void Step(SnesButton input) => game.StepCaptured(input, ++sequence, 1);
        for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? SnesButton.Start : SnesButton.None);
        Check(game.GameState == SuperMetroidGameState.MainGameplay, "desktop soak seed startup failed");
        game.GameplayRuntimeForVerification.LoadCartridgeRoomForDebug(paused ? RoomHeaderDefinitions.GlassTunnel : RoomHeaderDefinitions.AlphaPowerBomb, new Position16<RoomPixels>(0, 0));
        game.GameplayRuntimeForVerification.FrameBoundary.BeginAcceptedFrame(0);
        Step(0);
        if (paused) Step(SnesButton.Start);
        for (int warmup = 0; warmup < 120; warmup++) Step(0);
        var identity = SuperMetroid.AssetExtraction.Installation.GameContentIdentity.Create(installation.LoadAudioClips(),
            installation.LoadMaps(), installation.LoadProjectiles());
        DebuggerSaveStateStore.ForInstalledGame(installation.Root, options, identity).Save(0, bus, game);
    }
}
