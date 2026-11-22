using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
using SuperMetroid.Desktop.Audio;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.DesktopVerification.Soak;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

/// <summary>Measures paced hidden-window gameplay and pause rendering with real audio output.</summary>
internal static class HiddenSoakTests
{
    /// <summary>Runs both retail scenes with captured packets, hardware D3D11, and silent waveOut timing.</summary>
    internal static async Task RunHiddenSoak(int seconds)
    {
        if (seconds is < 5 or > 300) throw new ArgumentOutOfRangeException(nameof(seconds), "Use 5..300 seconds per scene.");
        var results = new List<HiddenSoakResult>();
        foreach (bool paused in new[] { false, true })
        {
            var game = InstalledFixtures.CreateGameWithAudio(new SuperMetroid.Core.Saves.SaveSlotStore(SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps),
                SuperMetroid.Core.Audio.GameAudioOutput.MixedFrom(DesktopAccess.LoadAudioClips()), new SuperMetroid.Core.Gameplay.GameplayAssists { SkipOpeningCinematic = true, SamusCannotDie = true });
            var audio = new short[AudioSession.PcmSamplesPerFrame];
            long sequence = 0;
            CapturedFrontendFrame Step(ControllerButton input)
            {
                return game.StepCaptured(input);
            }
            for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
                Step(tick % 47 == 0 ? ControllerButton.Start : ControllerButton.None);
            Check(game.GameState == SuperMetroidGameState.MainGameplay, "soak startup failed");
            var runtime = game.RuntimeForVerification ?? throw new InvalidOperationException("Soak has no active runtime.");
            RoomDefinition room = paused ? RoomDefinitions.GlassTunnel : RoomDefinitions.AlphaPowerBomb;
            runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            Step(0);
            if (paused) Step(ControllerButton.Start);
            for (int warmup = 0; warmup < 120; warmup++) Step(0);
            using var window = new Form { ClientSize = new(900, 672), ShowInTaskbar = false };
            var worker = new D3D11RenderWorker(window.Handle, new Extent16<DisplayPixels>(900, 672), 1, D3D11DeviceKind.Hardware);
            try
            {
                string adapter = await worker.Ready;
                using var output = new WaveOutAudioDevice(48000, 2, 1600, volumePercent: 0);
                int frames = seconds * 60;
                var producerMs = new double[frames];
                var latenessMs = new double[frames];
                long nonzero = 0, pcmSamples = 0, allocation = 0;
                var clock = Stopwatch.StartNew();
                for (int tick = 0; tick < frames; tick++)
                {
                    double due = tick / 60.0;
                    while (clock.Elapsed.TotalSeconds < due) await Task.Delay(1);
                    double late = (clock.Elapsed.TotalSeconds - due) * 1000;
                    if (late > 250) throw new InvalidOperationException($"Soak host stalled {late:F1}ms; not silently rebasing evidence.");
                    latenessMs[tick] = late;
                    bool isPaused = game.IsAt(PauseStep.FadingIn) || game.IsAt(PauseStep.Menu);
                    Check(paused ? isPaused : game.GameState == SuperMetroidGameState.MainGameplay, "soak left requested scene");
                    long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                    long began = Stopwatch.GetTimestamp();
                    var frame = game.StepCaptured(0);
                    // The real producer order: capture and synthesize inside Step, copy and queue PCM, publish.
                    MixAndSubmit(audio, output, game, ref nonzero, ref pcmSamples);
                    worker.Publish(new RenderFrame(new(++sequence, 1), frame.Snapshot));
                    producerMs[tick] = Stopwatch.GetElapsedTime(began).TotalMilliseconds;
                    allocation += GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
                    worker.ThrowIfFaulted();
                    if ((tick + 1) % 1800 == 0) Console.WriteLine($"Hidden soak paused={paused}: {(tick + 1) / 60}/{seconds}s, queue drains={output.QueueHealth.EmptyBeforeRefill}");
                }
                while (clock.Elapsed.TotalSeconds < seconds) await Task.Delay(1);
                output.WaitForPendingSubmissions();
                Check(nonzero > 0 && pcmSamples == frames * 1600L, "soak lost PCM or only generated silence");
                var health = output.QueueHealth;
                var timing = worker.CaptureTimings();
                Array.Sort(producerMs); Array.Sort(latenessMs);
                results.Add(new HiddenSoakResult(
                    Room: room.Name, Paused: paused, Adapter: adapter, Seconds: clock.Elapsed.TotalSeconds, Frames: frames,
                    ProducerP50Ms: producerMs[(int)Math.Ceiling(frames * .5) - 1],
                    ProducerP95Ms: producerMs[(int)Math.Ceiling(frames * .95) - 1],
                    ProducerP99Ms: producerMs[(int)Math.Ceiling(frames * .99) - 1],
                    MaximumLatenessMs: latenessMs[^1], AverageAllocatedBytes: allocation / frames,
                    PcmSamples: pcmSamples, NonzeroPcmSamples: nonzero, Audio: health, Renderer: timing,
                    PresentedFrames: worker.PresentedFrames, OccludedFrames: worker.OccludedFrames, MailboxMetrics: worker.MailboxMetrics,
                    DeviceRecoveries: worker.DeviceRecoveries));
                Console.WriteLine($"Hidden soak paused={paused}: finished {frames} frames; producer p95={producerMs[(int)Math.Ceiling(frames * .95) - 1]:F3}ms, drains={health.EmptyBeforeRefill}.");
            }
            finally { await worker.StopAsync(); }
        }
        Console.WriteLine("Hidden soak report: " + JsonSerializer.Serialize(new HiddenSoakReport
        {
            Scope = "Paced standalone producer + real silent waveOut + hidden GPU HWND. Not visible Present or the production WinForms timer loop; not cross-backend PCM parity.",
            TimestampUtc = DateTimeOffset.UtcNow, Runtime = RuntimeInformation.FrameworkDescription,
            OS = RuntimeInformation.OSDescription, CoreBuild = global::SuperMetroid.Core.BuildIdentity.Version,
            DiagnosticBuild = global::SuperMetroid.DesktopVerification.BuildIdentity.Version, Results = results,
        }, global::SuperMetroid.DesktopVerification.DesktopVerificationJson.Info<HiddenSoakReport>(
            new JsonSerializerOptions { WriteIndented = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals })));
    }

    /// <summary>The hidden soak's report: what ran, on which machine and builds, and each run's result.</summary>
    internal sealed record HiddenSoakReport
    {
        /// <summary>What the soak exercises.</summary>
        public required string Scope { get; init; }
        /// <summary>When the report was written.</summary>
        public required DateTimeOffset TimestampUtc { get; init; }
        /// <summary>The .NET runtime.</summary>
        public required string Runtime { get; init; }
        /// <summary>The operating system.</summary>
        public required string OS { get; init; }
        /// <summary>The simulation's build.</summary>
        public required Guid CoreBuild { get; init; }
        /// <summary>The desktop checks' build.</summary>
        public required Guid DiagnosticBuild { get; init; }
        /// <summary>Each run's result.</summary>
        public required List<HiddenSoakResult> Results { get; init; }
    }

    /// <summary>One hidden soak run's timing, audio and renderer measurements.</summary>
    /// <param name="Room">The room the run played.</param>
    /// <param name="Paused">Whether the run was paused.</param>
    /// <param name="Adapter">The GPU adapter.</param>
    /// <param name="Seconds">The run's length.</param>
    /// <param name="Frames">The frames it produced.</param>
    /// <param name="ProducerP50Ms">The producer's median frame time.</param>
    /// <param name="ProducerP95Ms">The producer's 95th-percentile frame time.</param>
    /// <param name="ProducerP99Ms">The producer's 99th-percentile frame time.</param>
    /// <param name="MaximumLatenessMs">The latest a frame was.</param>
    /// <param name="AverageAllocatedBytes">The bytes allocated per frame on average.</param>
    /// <param name="PcmSamples">The PCM samples produced.</param>
    /// <param name="NonzeroPcmSamples">The PCM samples that were not silent.</param>
    /// <param name="Audio">The audio queue's health.</param>
    /// <param name="Renderer">The renderer's timings.</param>
    /// <param name="PresentedFrames">The frames presented.</param>
    /// <param name="OccludedFrames">The frames the hidden window occluded.</param>
    /// <param name="MailboxMetrics">The render mailbox's counters.</param>
    /// <param name="DeviceRecoveries">The device recoveries the renderer made.</param>
    internal sealed record HiddenSoakResult(string Room, bool Paused, string Adapter, double Seconds, int Frames,
        double ProducerP50Ms, double ProducerP95Ms, double ProducerP99Ms, double MaximumLatenessMs, long AverageAllocatedBytes,
        long PcmSamples, long NonzeroPcmSamples, global::SuperMetroid.Desktop.Audio.WaveOutQueueHealthSnapshot Audio,
        global::SuperMetroid.Rendering.Direct3D11.Telemetry.RenderWorkerTimings Renderer, long PresentedFrames, long OccludedFrames,
        global::SuperMetroid.Rendering.Direct3D11.Frames.RenderMailboxMetrics MailboxMetrics, long DeviceRecoveries);

    /// <summary>Preserves host capture, PCM queue, acknowledgement, and publish order within one synchronous call.</summary>
    private static void MixAndSubmit(short[] samples, WaveOutAudioDevice output, SuperMetroidGame game,
        ref long nonzero, ref long count)
    {
        game.CopyAudioFrame(samples);
        output.Submit(samples);
        count += samples.Length;
        foreach (short sample in samples) if (sample != 0) nonzero++;
    }
}
