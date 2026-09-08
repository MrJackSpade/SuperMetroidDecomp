using System.Diagnostics;
using System.Collections.Concurrent;
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
    private readonly AutoResetEvent wake = new(false);
    private readonly ConcurrentQueue<(Func<AndroidSessionData, string> Action, TaskCompletionSource<string> Result)> commands = new();
    private readonly CancellationTokenSource stopping = new();
    private readonly Task worker;
    public FrameInputLatch Input { get; } = new();
    private SuperMetroidGameOptions? effectiveOptions;
    public SuperMetroidGameOptions? EffectiveOptions => Volatile.Read(ref effectiveOptions);
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
        wake.Set();
    }

    public Task<string> SaveSlot(int slot) => Request(data => data.SaveSlot(slot));
    public Task<string> LoadSlot(int slot) => Request(data => data.LoadSlot(slot));

    private Task<string> Request(Func<AndroidSessionData, string> action)
    {
        if (worker.IsCompleted) return Task.FromException<string>(new InvalidOperationException("Game worker stopped; restart before using state tools."));
        var result = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        commands.Enqueue((action, result));
        wake.Set();
        return result.Task;
    }

    public async Task Stop()
    {
        stopping.Cancel();
        wake.Set();
        await worker;
        active.Dispose();
        wake.Dispose();
        stopping.Dispose();
    }

    private void Run()
    {
        AndroidPcmOutput? output = null;
        try
        {
            using var data = new AndroidSessionData(root);
            RefreshEffectiveOptions(data);
            SuperMetroidGameOptions options = data.Options;
            long sequence = 0;
            var clock = Stopwatch.StartNew();
            double deadline = clock.Elapsed.TotalSeconds;
            double measuredAt = deadline;
            long measuredFrame = 0, measuredPaint = view.PaintCount;
            long measuredDrawTicks = view.DrawTicks, measuredUploadTicks = view.UploadTicks;
            long measuredReplacements = view.ReplacedFrames;
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
                    data.PersistSave();
                    data.FlushRecording();
                    while (!active.IsSet && !stopping.IsCancellationRequested)
                    {
                        DrainCommands(data);
                        wake.WaitOne();
                    }
                    if (stopping.IsCancellationRequested) break;
                    deadline = measuredAt = clock.Elapsed.TotalSeconds;
                    measuredFrame = sequence;
                    measuredPaint = view.PaintCount;
                    stepMilliseconds = renderMilliseconds = audioMilliseconds = 0;
                }
                DrainCommands(data);
                if (options.AudioEnabled) output ??= new AndroidPcmOutput();
                double start = clock.Elapsed.TotalMilliseconds;
                data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
                ushort input = Input.Sample();
                data.Record(input);
                CapturedFrontendFrame frame = data.Game.StepCaptured(input, ++sequence, data.Generation);
                view.SetRoomIdentity(data.Game.GameplayActiveAreaIndex is { } area && data.Game.GameplayActiveRoomIndex is { } room
                    ? $"Room ${(byte)area:X2}/${room:X2} [$8F:{data.Game.GameplayActiveRoomPointer:X4}, state $8F:{data.Game.GameplayActiveRoomStatePointer:X4}]"
                    : "No active room");
                double stepped = clock.Elapsed.TotalMilliseconds;
                var pixels = frame.Snapshot is { } snapshot
                    ? SoftwareFrameSnapshotRenderer.Render(snapshot) : frame.Frame.Pixels;
                double rendered = clock.Elapsed.TotalMilliseconds;
                short[] samples = data.Audio.RenderFrame(frame.Frame.AudioCommands);
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
                    long paints = view.PaintCount - measuredPaint;
                    long drawTicks = view.DrawTicks, uploadTicks = view.UploadTicks;
                    long replacements = view.ReplacedFrames;
                    // CPU-only presenter measurements distinguish scheduling loss from
                    // expensive bitmap conversion. GPU timing remains in adb gfxinfo.
                    double tickMilliseconds = 1000.0 / Stopwatch.Frequency;
                    double drawMs = paints == 0 ? 0 : (drawTicks - measuredDrawTicks) * tickMilliseconds / paints;
                    double uploadMs = paints == 0 ? 0 : (uploadTicks - measuredUploadTicks) * tickMilliseconds / paints;
                    status = $"emu {count / (now - measuredAt):F1} paint {(view.PaintCount - measuredPaint) / (now - measuredAt):F1} " +
                        $"step {stepMilliseconds / count:F1} render {renderMilliseconds / count:F1} audio {audioMilliseconds / count:F1} " +
                        $"underrun {output?.UnderrunCount ?? 0} {frame.Frame.GameState}";
                    global::Android.Util.Log.Info("SuperMetroid", status);
                    File.AppendAllText(Path.Combine(root, "timing.log"),
                        $"{DateTimeOffset.UtcNow:O} {status} frame={sequence} phase={frame.Frame.Phase} " +
                        $"uiDrawMs={drawMs:F3} uploadMs={uploadMs:F3} replaced={replacements - measuredReplacements}\n");
                    measuredAt = now;
                    measuredFrame = sequence;
                    measuredPaint = view.PaintCount;
                    measuredDrawTicks = drawTicks;
                    measuredUploadTicks = uploadTicks;
                    measuredReplacements = replacements;
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
            data.PersistSave();
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
        finally
        {
            output?.Dispose();
            while (commands.TryDequeue(out var request))
                request.Result.TrySetException(new InvalidOperationException("Game worker stopped before completing the request."));
        }
    }

    private void DrainCommands(AndroidSessionData data)
    {
        while (commands.TryDequeue(out var request))
        {
            try
            {
                string result = request.Action(data);
                RefreshEffectiveOptions(data);
                Input.Clear();
                var retained = data.Game.GetRetainedDisplay(1, data.Generation);
                if (retained is not null) view.Publish(SoftwareFrameSnapshotRenderer.Render(retained), result);
                request.Result.TrySetResult(result);
            }
            catch (Exception error) { request.Result.TrySetException(error); }
        }
    }

    private void RefreshEffectiveOptions(AndroidSessionData data) => Volatile.Write(ref effectiveOptions,
        data.Game.ConfiguredOptions with
        {
            AudioEnabled = data.Options.AudioEnabled,
            MasterVolumePercent = data.Options.MasterVolumePercent,
        });
}
