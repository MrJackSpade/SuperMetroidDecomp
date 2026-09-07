using System.Diagnostics;
using System.Text.Json;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using SuperMetroid.Game;
using SuperMetroid.Rendering.Direct3D11;

internal static partial class Program
{
    private static async Task RunDesktopTimerSoak(int seconds, bool visible = false)
    {
        if (seconds is < 5 or > 300) throw new ArgumentOutOfRangeException(nameof(seconds));
        var results = new List<object>();
        foreach (bool paused in new[] { false, true })
        {
            string directory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "desktop-renderer", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(directory);
            string rom = Path.Combine(directory, "Super Metroid.smc");
            File.Copy(Path.GetFullPath("Super Metroid.smc"), rom);
            var options = new SuperMetroidGameOptions { Renderer = RendererSelection.Direct3D11,
                SkipOpeningCinematic = true, Invincibility = true, MasterVolumePercent = 0 };
            CreateDesktopSoakSeed(rom, options, paused);
            using var form = new GameForm(rom, options);
            var control = Field<PlayableGameControl>(form, "gameControl");
            _ = form.Handle; _ = control.Handle;
            Call(control, "SetPlaying", false);
            Call(control, "LoadDebuggerState", 0);
            var game = Field<SuperMetroidGame>(control, "game");
            var counter = Field<FrameTimingCounter>(control, "frameTimings");
            var samples = new List<double>(seconds * 60 + 120);
            void Measure(long ticks)
            {
                if (samples.Count == samples.Capacity) throw new InvalidOperationException("Desktop timer exceeded bounded 60 Hz sample capacity.");
                samples.Add(ticks * 1000.0 / Stopwatch.Frequency);
            }
            try
            {
                await control.InitializeRendererAsync();
                var worker = Field<D3D11RenderWorker>(control, "gpuWorker");
                string adapter = await worker.Ready;
                if (visible) VisibleSoakWindow.ShowWithoutActivation(form.Handle);
                var output = Field<WaveOutAudioDevice>(control, "audioDevice");
                counter.EmulatedFrameMeasured += Measure;
                ushort startFrame = game.FrameNumber;
                Call(control, "SetPlaying", true);
                var clock = Stopwatch.StartNew();
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
                        nextReport += 30;
                    }
                }
                double elapsed = clock.Elapsed.TotalSeconds;
                // Take health before SetPlaying(false) intentionally resets the native queue.
                var health = output.QueueHealth;
                var renderer = worker.CaptureTimings();
                Check(worker.SubmittedUploadBytes > 0 && worker.SubmittedUploadCalls > 0
                    && worker.CaptureUploadTimings().Observed > 0, "desktop renderer did not publish upload telemetry");
                if (visible) Check(worker.PresentedFrames > 0, "visible soak produced no successful Present calls");
                Call(control, "SetPlaying", false);
                counter.EmulatedFrameMeasured -= Measure;
                Check(unchecked((ushort)(game.FrameNumber - startFrame)) == samples.Count, "desktop frame/timing count mismatch");
                samples.Sort();
                double Percentile(double p) => samples[(int)Math.Ceiling(samples.Count * p) - 1];
                results.Add(new { Paused = paused, Seconds = elapsed, Frames = samples.Count, Fps = samples.Count / elapsed,
                    ProducerP50Ms = Percentile(.5), ProducerP95Ms = Percentile(.95), ProducerP99Ms = Percentile(.99),
                    Adapter = adapter, Visible = visible, Audio = health, Renderer = renderer,
                    UploadCpu = worker.CaptureUploadTimings(), worker.SubmittedUploadBytes, worker.SubmittedUploadCalls,
                    worker.PresentedFrames, worker.OccludedFrames, worker.MailboxMetrics });
                Console.WriteLine($"Desktop timer paused={paused}: {samples.Count / elapsed:F3} fps, p95={Percentile(.95):F3}ms, drains={health.EmptyBeforeRefill}.");
            }
            finally
            {
                counter.EmulatedFrameMeasured -= Measure;
                await control.StopRendererAsync();
            }
            form.Dispose();
            Directory.Delete(directory, recursive: true);
        }
        string reportDirectory = Path.GetFullPath(Path.Combine("csharp", "test-temp", "render-performance", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(reportDirectory);
        string report = Path.Combine(reportDirectory, "desktop-timer-soak.json");
        File.WriteAllText(report, JsonSerializer.Serialize(new { Scope = "Production PlayableGameControl WinForms timer, silent real waveOut, hardware GPU; " +
            (visible ? "visible nonactivating HWND; successful Present is not proof of RDP delivery." : "hidden HWND, not visible presentation."),
            OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
            Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            Processor = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
            TimestampUtc = DateTimeOffset.UtcNow, CoreBuild = typeof(SuperMetroidGame).Module.ModuleVersionId,
            DesktopBuild = typeof(PlayableGameControl).Module.ModuleVersionId, Results = results },
            new JsonSerializerOptions { WriteIndented = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals }));
        Console.WriteLine($"Desktop timer soak report: {report}");
    }

    private static void CreateDesktopSoakSeed(string rom, SuperMetroidGameOptions options, bool paused)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        var game = new SuperMetroidGame(bus, options);
        using var audio = new SpcAudioEngine();
        long sequence = 0;
        void Step(ushort input)
        {
            var frame = game.StepCaptured(input, ++sequence, 1);
            audio.RenderFrame(frame.Frame.AudioCommands);
            game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
        }
        for (int tick = 0; tick < 2000 && game.GameState != SuperMetroidGameState.MainGameplay; tick++)
            Step(tick % 47 == 0 ? (ushort)SnesButton.Start : (ushort)0);
        Check(game.GameState == SuperMetroidGameState.MainGameplay, "desktop soak seed startup failed");
        game.RuntimeForVerification!.LoadCartridgeRoomForDebug(paused ? HiddenSoakRooms.MaridiaTube : HiddenSoakRooms.AlphaPowerBomb, 0, 0);
        game.RuntimeForVerification.RunNmi(0, true);
        Step(0);
        if (paused) Step((ushort)SnesButton.Start);
        for (int warmup = 0; warmup < 120; warmup++) Step(0);
        new DebuggerSaveStateStore(rom, bus.Rom).Save(0, bus, game, audio.Player);
    }
}

/// <summary>Opt-in diagnostic display; does not focus the game or initialize its renderer twice.</summary>
internal static partial class VisibleSoakWindow
{

    internal static void ShowWithoutActivation(nint window)
    {
        // The renderer was explicitly initialized while hidden. Native showing avoids
        // GameForm.OnShown's second initialization and leaves foreground input alone.
        ShowWindow(window, VisibleSoakWindowCommands.ShowNoActivate);
        if (!IsWindowVisible(window)) throw new InvalidOperationException("Could not show visible soak window.");
    }

    [System.Runtime.InteropServices.LibraryImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial bool ShowWindow(nint window, int command);
    [System.Runtime.InteropServices.LibraryImport("user32.dll")]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static partial bool IsWindowVisible(nint window);
}

internal static class VisibleSoakWindowCommands
{
    /// <summary>WinUser.h SW_SHOWNOACTIVATE: show at the current size without activating.</summary>
    internal const int ShowNoActivate = 4;
}
