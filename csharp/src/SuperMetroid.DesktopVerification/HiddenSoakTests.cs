using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Desktop;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class Program
{
    private static async Task RunHiddenSoak(int seconds)
    {
        if (seconds is < 5 or > 300) throw new ArgumentOutOfRangeException(nameof(seconds), "Use 5..300 seconds per scene.");
        var results = new List<object>();
        foreach (bool paused in new[] { false, true })
        {
            var game = new SuperMetroidGame(SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc"),
                new SuperMetroidGameOptions { SkipOpeningCinematic = true, Invincibility = true });
            using var audio = new SpcAudioEngine();
            long sequence = 0;
            CapturedFrontendFrame Step(ushort input)
            {
                var frame = game.StepCaptured(input, ++sequence, 1);
                if (frame.Snapshot is null || frame.Frame.Pixels.Length != 0)
                    throw new InvalidOperationException("Soak entered a raster fallback.");
                audio.RenderFrame(frame.Frame.AudioCommands);
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
                return frame;
            }
            for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
                Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0);
            Check(game.GameState == SuperMetroidGameState.MainGameplay, "soak startup failed");
            var runtime = game.RuntimeForVerification ?? throw new InvalidOperationException("Soak has no active runtime.");
            ushort room = paused ? HiddenSoakRooms.MaridiaTube : HiddenSoakRooms.AlphaPowerBomb;
            runtime.LoadCartridgeRoomForDebug(room, 0, 0);
            runtime.RunNmi(0, true);
            Step(0);
            if (paused) Step((ushort)SnesButton.Start);
            for (int warmup = 0; warmup < 120; warmup++) Step(0);
            using var window = new Form { ClientSize = new(900, 672), ShowInTaskbar = false };
            var worker = new D3D11RenderWorker(window.Handle, 900, 672, 1, D3D11DeviceKind.Hardware);
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
                    Check(frame.Snapshot is not null && frame.Frame.Pixels.Length == 0, "soak raster fallback");
                    // The real producer order: capture, synthesize, queue PCM, acknowledge, publish.
                    // Keep the span inside a synchronous helper so no PCM crosses an await.
                    MixAndSubmit(audio, output, game, frame, ref nonzero, ref pcmSamples);
                    worker.Publish(frame.Snapshot!);
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
        string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "render-performance", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        string report = Path.Combine(directory, "hidden-soak.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new
        {
            Scope = "Paced standalone producer + real silent waveOut + hidden GPU HWND. Not visible Present or the production WinForms timer loop; not cross-backend PCM parity.",
            TimestampUtc = DateTimeOffset.UtcNow, Runtime = RuntimeInformation.FrameworkDescription,
            OS = RuntimeInformation.OSDescription, CoreBuild = typeof(SuperMetroidGame).Module.ModuleVersionId,
            DiagnosticBuild = typeof(Program).Module.ModuleVersionId, Results = results,
        }, new JsonSerializerOptions { WriteIndented = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals }));
        Console.WriteLine($"Hidden soak report: {report}");
    }

    private static void MixAndSubmit(SpcAudioEngine audio, WaveOutAudioDevice output, SuperMetroidGame game,
        CapturedFrontendFrame frame, ref long nonzero, ref long count)
    {
        var samples = audio.RenderFrame(frame.Frame.AudioCommands);
        output.Submit(samples);
        game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
        count += samples.Length;
        foreach (short sample in samples) if (sample != 0) nonzero++;
    }
}

internal static class HiddenSoakRooms
{
    /// <summary>Retail $8F:A3AE room-local ordinary gameplay fixture.</summary>
    internal const ushort AlphaPowerBomb = 0xa3ae;
    /// <summary>Retail $8F:CEFB Maridia glass tube, logical room $04/$01.</summary>
    internal const ushort MaridiaTube = 0xcefb;
}
