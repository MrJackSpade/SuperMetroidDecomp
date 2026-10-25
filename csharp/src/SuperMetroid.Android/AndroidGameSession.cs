using SuperMetroid.Android.Input;
using System.Diagnostics;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Android.Diagnostics;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Android;

/// <summary>
/// Single owner of mutable game/APU state. Activity/UI events only change the run gate or
/// controller latch. Rendering and audio are measured separately so a slow reference PPU
/// cannot masquerade as a controller or cartridge timing bug.
/// </summary>
internal sealed class AndroidGameSession
{
    /// <summary>Installation root holding saves, input traces, timing logs, and error reports.</summary>
    private readonly string root;
    /// <summary>UI presentation surface receiving completed frames and status.</summary>
    private readonly AndroidGameView view;
    /// <summary>Run gate cleared while the Activity is inactive.</summary>
    private readonly ManualResetEventSlim active = new(false);
    /// <summary>Wakes the worker for queued commands or an Activity state change.</summary>
    private readonly AutoResetEvent wake = new(false);
    /// <summary>Serialized save, load, import, and diagnostic requests handled on the game worker.</summary>
    private readonly AndroidSessionCommands commands = new();
    /// <summary>Cancels the worker and any frame deadline wait during shutdown.</summary>
    private readonly CancellationTokenSource stopping = new();
    /// <summary>Task owning mutable game and audio state until Stop awaits it.</summary>
    private readonly Task worker;
    /// <summary>Thread-safe controller latch sampled once for each emulated frame.</summary>
    public FrameInputLatch Input { get; } = new();
    /// <summary>Last game options published for UI access without entering the worker.</summary>
    private HostOptions? effectiveOptions;
    /// <summary>Current merged game and Android audio options, read with volatile ordering.</summary>
    public HostOptions? EffectiveOptions => Volatile.Read(ref effectiveOptions);
    /// <summary>Appends a timestamped controller event to the installation's input log.</summary>
    public void RecordInput(string description) =>
        File.AppendAllText(Path.Combine(root, "input-events.log"), $"{DateTimeOffset.UtcNow:O} {description}\n");

    /// <summary>Starts the single game worker bound to the installation and Android view.</summary>
    public AndroidGameSession(string root, AndroidGameView view)
    {
        this.root = root;
        this.view = view;
        worker = Task.Run(Run);
    }

    /// <summary>Updates the Activity run gate and clears latched input across pause or resume.</summary>
    public void SetActive(bool value)
    {
        view.SetPresentationActive(value);
        Input.Clear();
        if (value) active.Set();
        else active.Reset();
        wake.Set();
    }

    /// <summary>Queues a save-slot write on the game worker.</summary>
    public Task<string> SaveSlot(int slot) => Request(data => data.SaveSlot(slot));
    /// <summary>Queues a save-slot load on the game worker.</summary>
    public Task<string> LoadSlot(int slot) => Request(data => data.LoadSlot(slot));
    /// <summary>Queues a state import into the selected slot.</summary>
    public Task<string> ImportState(string path, int slot) => Request(data => data.ImportState(path, slot));
    /// <summary>Queues a save-file import on the game worker.</summary>
    public Task<string> ImportSave(string path) => Request(data => data.ImportSave(path));

    /// <summary>Persists active diagnostics through the worker, or bundles durable crash files after it stops.</summary>
    public Task<string> ExportDiagnostics(string destination, int slot) => worker.IsCompleted
        // Fatal runtime errors must not make the already durable crash files inaccessible.
        ? Task.Run(() => AndroidDiagnosticBundle.Create(root, destination, slot))
        : Request(data =>
        {
            data.PersistSave();
            data.FlushRecording();
            AndroidDiagnosticBundle.Create(root, destination, slot);
            return "Private diagnostic bundle prepared.";
        });

    /// <summary>Enqueues a serialized session operation and wakes the worker to handle it.</summary>
    private Task<string> Request(Func<AndroidSessionData, string> action)
    {
        Task<string> result = commands.Enqueue(action);
        wake.Set();
        return result;
    }

    /// <summary>Stops frame publication, cancels the worker, and disposes its synchronization gates.</summary>
    public async Task Stop()
    {
        view.SetPresentationActive(false);
        stopping.Cancel();
        wake.Set();
        await worker;
        active.Dispose();
        wake.Dispose();
        stopping.Dispose();
    }

    /// <summary>Owns frame stepping, rasterization, audio, persistence, and queued tools until shutdown.</summary>
    private void Run()
    {
        AndroidPcmOutput? output = null;
        AndroidResumeTrace? resumeTrace = null;
        try
        {
            using var data = new AndroidSessionData(root);
            var rasterBuffer = new SuperMetroid.Core.Assets.RgbaPixel[FrontendFrame.Width * FrontendFrame.Height];
            // Host-owned PCM storage; the session copies each update's samples into it.
            var samples = new short[AudioSession.PcmSamplesPerFrame];
            RefreshEffectiveOptions(data);
            HostOptions options = data.Options;
            long sequence = 0;
            var clock = Stopwatch.StartNew();
            double deadline = clock.Elapsed.TotalSeconds;
            double measuredAt = deadline;
            long measuredFrame = 0, measuredPaint = view.PaintCount;
            long measuredDrawTicks = view.DrawTicks, measuredUploadTicks = view.UploadTicks;
            long measuredReplacements = view.ReplacedFrames;
            double stepMilliseconds = 0, renderMilliseconds = 0, audioMilliseconds = 0;
            double submitMilliseconds = 0, maximumSubmitMilliseconds = 0;
            double previousPublication = 0, minimumPublicationGap = double.PositiveInfinity, maximumPublicationGap = 0;
            string status = "Starting cartridge";

            while (!stopping.IsCancellationRequested)
            {
                if (!active.IsSet)
                {
                    output?.Dispose();
                    view.FlushFrameTrace(report => File.AppendAllText(Path.Combine(root, "frame-handoff.log"), report));
                    resumeTrace?.Flush(report => File.AppendAllText(Path.Combine(root, "resume-timing.log"), report));
                    resumeTrace = null;
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
                    submitMilliseconds = maximumSubmitMilliseconds = previousPublication = 0;
                    minimumPublicationGap = double.PositiveInfinity;
                    maximumPublicationGap = 0;
                    measuredDrawTicks = view.DrawTicks;
                    measuredUploadTicks = view.UploadTicks;
                    measuredReplacements = view.ReplacedFrames;
                }
                DrainCommands(data);
                if (options.AudioEnabled && output is null)
                {
                    resumeTrace = new AndroidResumeTrace();
                    output = new AndroidPcmOutput(resumeTrace);
                }
                double start = clock.Elapsed.TotalMilliseconds;
                ControllerButton input = Input.Sample();
                data.Record(input);
                var previousGameState = data.Game.GameState;
                CapturedFrontendFrame frame = data.Game.StepCaptured(input, ++sequence, data.Generation);
                view.SetRoomIdentity(data.Game.GameplayActiveRoomIdentity is { } room
                    ? $"Room {room} [{data.Game.GameplayActiveRoom?.Name}, state {data.Game.GameplayActiveRoomState?.Name}]"
                    : "No active room");
                double stepped = clock.Elapsed.TotalMilliseconds;
                var pixels = SoftwareFrameSnapshotRenderer.Render(frame.Snapshot, rasterBuffer);
                double rendered = clock.Elapsed.TotalMilliseconds;
                data.Game.CopyAudioFrame(samples);
                if (options.MasterVolumePercent != 100)
                    for (int i = 0; i < samples.Length; i++) samples[i] = (short)(samples[i] * options.MasterVolumePercent / 100);
                double mixed = clock.Elapsed.TotalMilliseconds;
                output?.Submit(samples);
                data.SaveCompletedDoor(previousGameState);
                double submitted = clock.Elapsed.TotalMilliseconds;
                if (resumeTrace is { Full: false })
                    resumeTrace.Record("frame", $"sequence={sequence} startMs={start:F3} endMs={submitted:F3} " +
                        $"stepMs={stepped - start:F3} renderMs={rendered - stepped:F3} mixMs={mixed - rendered:F3} submitMs={submitted - mixed:F3} " +
                        $"gc0={GC.CollectionCount(0)} gc1={GC.CollectionCount(1)} gc2={GC.CollectionCount(2)}");
                double submitDuration = submitted - mixed;
                submitMilliseconds += submitDuration;
                maximumSubmitMilliseconds = Math.Max(maximumSubmitMilliseconds, submitDuration);
                if (previousPublication != 0)
                {
                    double gap = submitted - previousPublication;
                    minimumPublicationGap = Math.Min(minimumPublicationGap, gap);
                    maximumPublicationGap = Math.Max(maximumPublicationGap, gap);
                }
                previousPublication = submitted;
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
                        $"uiDrawMs={drawMs:F3} uploadMs={uploadMs:F3} replaced={replacements - measuredReplacements} " +
                        $"submitMs={submitMilliseconds / count:F3}/{maximumSubmitMilliseconds:F3} " +
                        $"publicationGapMs={minimumPublicationGap:F3}/{maximumPublicationGap:F3} " +
                        $"queuedPcm={output?.PendingFrameCount ?? 0} deadlineLagMs={(now - deadline) * 1000:F3}\n");
                    measuredAt = now;
                    measuredFrame = sequence;
                    measuredPaint = view.PaintCount;
                    measuredDrawTicks = drawTicks;
                    measuredUploadTicks = uploadTicks;
                    measuredReplacements = replacements;
                    stepMilliseconds = renderMilliseconds = audioMilliseconds = 0;
                    submitMilliseconds = maximumSubmitMilliseconds = 0;
                    minimumPublicationGap = double.PositiveInfinity;
                    maximumPublicationGap = 0;
                }
                view.Publish(pixels, status);
                double completedAt = clock.Elapsed.TotalSeconds;
                AndroidFrameDeadline nextFrame = AndroidFrameDeadline.AfterFrame(deadline, completedAt);
                if (nextFrame.Rebased)
                {
                    // Do not skip any emulated frame. Report a missed real-time deadline and
                    // stop accumulating unbounded catch-up debt on an underpowered backend.
                    global::Android.Util.Log.Warn("SuperMetroid", $"Rebased stale host deadline; lag {(completedAt - deadline) * 1000:F1} ms; no emulated frames skipped.");
                }
                deadline = nextFrame.NextDeadline;
                if (nextFrame.WaitSeconds > 0)
                    stopping.Token.WaitHandle.WaitOne(TimeSpan.FromSeconds(nextFrame.WaitSeconds));
            }
            data.PersistSave();
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
        catch (Exception error)
        {
            ReportStopped(error);
        }
        finally
        {
            try
            {
                output?.Dispose();
                view.FlushFrameTrace(report => File.AppendAllText(Path.Combine(root, "frame-handoff.log"), report));
                resumeTrace?.Flush(report => File.AppendAllText(Path.Combine(root, "resume-timing.log"), report));
            }
            catch (Exception error) { ReportStopped(error, append: true); }
            finally
            {
                // Even a failing audio drain must settle every queued tool request.
                // Enqueue and closure share one lock, including arrivals during shutdown.
                commands.Complete(new InvalidOperationException("Game worker stopped; restart before using state tools."));
            }
        }
    }

    /// <summary>Records a worker failure in Android logs and the durable error file.</summary>
    private void ReportStopped(Exception error, bool append = false)
    {
        view.SetPresentationActive(false);
        string report = error.ToString();
        global::Android.Util.Log.Error("SuperMetroid", report);
        try
        {
            string path = Path.Combine(root, "last-error.txt");
            if (append) File.AppendAllText(path, "\nAudio shutdown failure:\n" + report);
            else File.WriteAllText(path, report);
        }
        catch (Exception storageError) { global::Android.Util.Log.Error("SuperMetroid", storageError.ToString()); }
        view.ShowStatus("Stopped: " + error.Message + " (see last-error.txt)");
    }

    /// <summary>Executes queued tools on the worker and republishes the retained display after state changes.</summary>
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

    /// <summary>Publishes game options combined with Android audio controls for UI readers.</summary>
    private void RefreshEffectiveOptions(AndroidSessionData data) => Volatile.Write(ref effectiveOptions,
        // The host shows its INI's options; while it is dormant (#1408) a loaded state's own options are not shown apart.
        data.Options);
}
