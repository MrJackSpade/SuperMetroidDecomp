using SuperMetroid.Core.Audio;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Rendering.Direct3D11;

using static SuperMetroid.DesktopVerification.Harness.DesktopVerificationSupport;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Desktop.Audio;
using SuperMetroid.Rendering.Direct3D11.Device;
using SuperMetroid.Rendering.Direct3D11.Frames;
namespace SuperMetroid.DesktopVerification.Soak;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>Measures paced hidden-window gameplay and pause rendering with real audio output.</summary>
internal static class HiddenSoakTests
{
    /// <summary>Runs both retail scenes with captured packets, hardware D3D11, and silent waveOut timing.</summary>
    internal static async Task RunHiddenSoak(int seconds)
    {
        if (seconds is < 5 or > 300) throw new ArgumentOutOfRangeException(nameof(seconds), "Use 5..300 seconds per scene.");
        var results = new List<object>();
        foreach (bool paused in new[] { false, true })
        {
            var game = InstalledFixtures.CreateGame(SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc"),
                new SuperMetroidGameOptions { SkipOpeningCinematic = true, Invincibility = true });
            game.BindAudio(DesktopAccess.LoadAudioClips());
            var audio = new short[CartridgeAudioSession.PcmSamplesPerFrame];
            long sequence = 0;
            CapturedFrontendFrame Step(SnesButton input)
            {
                return game.StepCaptured(input, ++sequence, 1);
            }
            for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
                Step(tick % 47 == 0 ? SnesButton.Start : SnesButton.None);
            Check(game.GameState == SuperMetroidGameState.MainGameplay, "soak startup failed");
            var runtime = game.RuntimeForVerification ?? throw new InvalidOperationException("Soak has no active runtime.");
            ushort room = paused ? HiddenSoakRooms.MaridiaTube : HiddenSoakRooms.AlphaPowerBomb;
            runtime.LoadCartridgeRoomForDebug(room, new Position16<RoomPixels>(0, 0));
            runtime.FrameBoundary.BeginAcceptedFrame(0);
            Step(0);
            if (paused) Step(SnesButton.Start);
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
                    bool isPaused = game.GameState is SuperMetroidGameState.PausedA or SuperMetroidGameState.PausedB;
                    Check(paused ? isPaused : game.GameState == SuperMetroidGameState.MainGameplay, "soak left requested scene");
                    long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                    long began = Stopwatch.GetTimestamp();
                    var frame = game.StepCaptured(0, ++sequence, 1);
                    // The real producer order: capture and synthesize inside Step, copy and queue PCM, publish.
                    MixAndSubmit(audio, output, game, ref nonzero, ref pcmSamples);
                    worker.Publish(frame.Snapshot);
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
                results.Add(new
                {
                    Room = $"8F:{room:X4}", Paused = paused, Adapter = adapter, Seconds = clock.Elapsed.TotalSeconds, Frames = frames,
                    ProducerP50Ms = producerMs[(int)Math.Ceiling(frames * .5) - 1],
                    ProducerP95Ms = producerMs[(int)Math.Ceiling(frames * .95) - 1],
                    ProducerP99Ms = producerMs[(int)Math.Ceiling(frames * .99) - 1],
                    MaximumLatenessMs = latenessMs[^1], AverageAllocatedBytes = allocation / frames,
                    PcmSamples = pcmSamples, NonzeroPcmSamples = nonzero, Audio = health, Renderer = timing,
                    worker.PresentedFrames, worker.OccludedFrames, worker.MailboxMetrics, worker.DeviceRecoveries,
                });
                Console.WriteLine($"Hidden soak paused={paused}: finished {frames} frames; producer p95={producerMs[(int)Math.Ceiling(frames * .95) - 1]:F3}ms, drains={health.EmptyBeforeRefill}.");
            }
            finally { await worker.StopAsync(); }
        }
        Console.WriteLine("Hidden soak report: " + JsonSerializer.Serialize(new
        {
            Scope = "Paced standalone producer + real silent waveOut + hidden GPU HWND. Not visible Present or the production WinForms timer loop; not cross-backend PCM parity.",
            TimestampUtc = DateTimeOffset.UtcNow, Runtime = RuntimeInformation.FrameworkDescription,
            OS = RuntimeInformation.OSDescription, CoreBuild = typeof(SuperMetroidGame).Module.ModuleVersionId,
            DiagnosticBuild = typeof(Program).Module.ModuleVersionId, Results = results,
        }, new JsonSerializerOptions { WriteIndented = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals }));
    }

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
