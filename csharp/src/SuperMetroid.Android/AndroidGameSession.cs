using System.Diagnostics;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Android;

/// <summary>
/// Single owner of mutable game/APU state. Activity/UI events only change the run gate or
/// controller latch. Rendering and audio are measured separately so a slow reference PPU
/// cannot masquerade as a controller or cartridge timing bug.
/// </summary>
internal sealed class AndroidGameSession
{
    private readonly string root;
    private readonly AndroidGameView view;
    private readonly ManualResetEventSlim active = new(false);
    private readonly CancellationTokenSource stopping = new();
    private readonly Task worker;
    public FrameInputLatch Input { get; } = new();
    public void RecordInput(string description) =>
        File.AppendAllText(Path.Combine(root, "input-events.log"), $"{DateTimeOffset.UtcNow:O} {description}\n");

    public AndroidGameSession(string root, AndroidGameView view)
    {
        this.root = root;
        this.view = view;
        worker = Task.Run(Run);
    }

    public void SetActive(bool value)
    {
        Input.Clear();
        if (value) active.Set();
        else active.Reset();
    }

    public async Task Stop()
    {
        stopping.Cancel();
        await worker;
        active.Dispose();
        stopping.Dispose();
    }

    private void Run()
    {
        AndroidPcmOutput? output = null;
        try
        {
            string gameRoot = Path.Combine(root, "game");
            string ini = Path.Combine(root, "SuperMetroid.ini");
            if (!File.Exists(ini)) File.WriteAllText(ini, SuperMetroidGameOptionsIni.DefaultFileContents);
            SuperMetroidGameOptions options = SuperMetroidGameOptionsIni.Parse(File.ReadAllText(ini), ini);
            var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.Combine(gameRoot, "SuperMetroid.smc"));
            string save = Path.Combine(root, "SuperMetroid.save.json");
            GameSaveFileStore.LoadOrMigrate(bus, save, Path.Combine(root, "SuperMetroid.srm"));
            var game = new SuperMetroidGame(bus, options);
            game.SaveRamChanged += () => GameSaveFileStore.WriteAtomic(bus, save);
            var audio = new CartridgeAudioRenderer(ExtractedAudioAssetCatalog.Load(Path.Combine(gameRoot, "audio")));
            long sequence = 0;
            var clock = Stopwatch.StartNew();
            double deadline = clock.Elapsed.TotalSeconds;
            double measuredAt = deadline;
            long measuredFrame = 0, measuredPaint = view.PaintCount;
            double stepMilliseconds = 0, renderMilliseconds = 0, audioMilliseconds = 0;
            string status = "Starting cartridge";

            while (!stopping.IsCancellationRequested)
            {
                if (!active.IsSet)
                {
                    output?.Dispose();
                    output = null;
                    // Native save changes are already atomic. This lifecycle boundary also
                    // persists any unsaved host-side SRAM metadata before the process sleeps.
                    GameSaveFileStore.WriteAtomic(bus, save);
                    active.Wait(stopping.Token);
                    deadline = measuredAt = clock.Elapsed.TotalSeconds;
                    measuredFrame = sequence;
                    measuredPaint = view.PaintCount;
                    stepMilliseconds = renderMilliseconds = audioMilliseconds = 0;
                }
                if (options.AudioEnabled) output ??= new AndroidPcmOutput();
                double start = clock.Elapsed.TotalMilliseconds;
                game.SetAudioAcknowledgements(audio.ReadAcknowledgements());
                ushort input = Input.Sample();
                CapturedFrontendFrame frame = game.StepCaptured(input, ++sequence, 1);
                double stepped = clock.Elapsed.TotalMilliseconds;
                var pixels = frame.Snapshot is { } snapshot
                    ? SoftwareFrameSnapshotRenderer.Render(snapshot) : frame.Frame.Pixels;
                double rendered = clock.Elapsed.TotalMilliseconds;
                short[] samples = audio.RenderFrame(frame.Frame.AudioCommands);
                if (options.MasterVolumePercent != 100)
                    for (int i = 0; i < samples.Length; i++) samples[i] = (short)(samples[i] * options.MasterVolumePercent / 100);
                double mixed = clock.Elapsed.TotalMilliseconds;
                output?.Submit(samples);
                stepMilliseconds += stepped - start;
                renderMilliseconds += rendered - stepped;
                audioMilliseconds += mixed - rendered;

                double now = clock.Elapsed.TotalSeconds;
                if (now - measuredAt >= 1)
                {
                    long count = sequence - measuredFrame;
                    status = $"emu {count / (now - measuredAt):F1} paint {(view.PaintCount - measuredPaint) / (now - measuredAt):F1} " +
                        $"step {stepMilliseconds / count:F1} render {renderMilliseconds / count:F1} audio {audioMilliseconds / count:F1} " +
                        $"underrun {output?.UnderrunCount ?? 0} {frame.Frame.GameState}";
                    global::Android.Util.Log.Info("SuperMetroid", status);
                    File.AppendAllText(Path.Combine(root, "timing.log"),
                        $"{DateTimeOffset.UtcNow:O} {status} frame={sequence} phase={frame.Frame.Phase}\n");
                    measuredAt = now;
                    measuredFrame = sequence;
                    measuredPaint = view.PaintCount;
                    stepMilliseconds = renderMilliseconds = audioMilliseconds = 0;
                }
                view.Publish(pixels, status);
                deadline += 1.0 / 60;
                double wait = deadline - clock.Elapsed.TotalSeconds;
                if (wait > 0) stopping.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(wait));
                else if (wait < -0.25)
                {
                    // Do not skip any emulated frame. Report a missed real-time deadline and
                    // stop accumulating unbounded catch-up debt on an underpowered backend.
                    global::Android.Util.Log.Warn("SuperMetroid", $"Simulation behind real time by {-wait * 1000:F1} ms.");
                    deadline = clock.Elapsed.TotalSeconds;
                }
            }
            GameSaveFileStore.WriteAtomic(bus, save);
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
        catch (Exception error)
        {
            string report = error.ToString();
            global::Android.Util.Log.Error("SuperMetroid", report);
            try { File.WriteAllText(Path.Combine(root, "last-error.txt"), report); }
            catch (Exception storageError) { global::Android.Util.Log.Error("SuperMetroid", storageError.ToString()); }
            view.ShowStatus("Stopped: " + error.Message + " (see last-error.txt)");
        }
        finally { output?.Dispose(); }
    }
}
