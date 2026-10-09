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
    /// <summary>Directory containing session recordings, save metadata, and diagnostic logs.</summary>
    private readonly string root;
    /// <summary>Android presentation endpoint used to publish frames and lifecycle status.</summary>
    private readonly AndroidGameView view;
    /// <summary>Run gate that suspends emulation while the activity is not presenting.</summary>
    private readonly ManualResetEventSlim active = new(false);
    /// <summary>Wake signal for paused-loop command draining and lifecycle changes.</summary>
    private readonly AutoResetEvent wake = new(false);
    /// <summary>Serialized state-tool requests serviced by the game-owning worker.</summary>
    private readonly AndroidSessionCommands commands = new();
    /// <summary>Cancellation source used to stop the worker and interrupt host-frame waits.</summary>
    private readonly CancellationTokenSource stopping = new();
    /// <summary>Background task that exclusively owns mutable game, save, and audio state.</summary>
    private readonly Task worker;
    /// <summary>Latched controller state sampled by the worker once per emulated frame.</summary>
    public FrameInputLatch Input { get; } = new();
    /// <summary>Effective options snapshot published for activity controls without exposing session data.</summary>
    private SuperMetroidGameOptions? effectiveOptions;
    /// <summary>Current published options, or null until the session data has been loaded.</summary>
    public SuperMetroidGameOptions? EffectiveOptions => Volatile.Read(ref effectiveOptions);

    /// <summary>Appends a timestamped controller or lifecycle event to the session input log.</summary>
    /// <param name="description">Human-readable event description supplied by the caller.</param>
    public void RecordInput(string description) =>
        File.AppendAllText(Path.Combine(root, "input-events.log"), $"{DateTimeOffset.UtcNow:O} {description}\n");

    /// <summary>Creates the worker and binds it to the persistent session directory and Android view.</summary>
    /// <param name="root">Directory used for session files and diagnostic output.</param>
    /// <param name="view">Presentation surface controlled by this session.</param>
    public AndroidGameSession(string root, AndroidGameView view)
    {
        this.root = root;
        this.view = view;
        worker = Task.Run(Run);
    }

    /// <summary>Enables or pauses frame production, clearing latched controls at the lifecycle boundary.</summary>
    /// <param name="value">Whether the activity is currently allowed to present and advance the game.</param>
    public void SetActive(bool value)
    {
        view.SetPresentationActive(value);
        Input.Clear();
        if (value) active.Set();
        else active.Reset();
        wake.Set();
    }

    /// <summary>Queues persistence of the selected SRAM slot on the session worker.</summary>
    /// <param name="slot">Save-slot index to write.</param>
    /// <returns>A task completed with the operation status or its exception.</returns>
    public Task<string> SaveSlot(int slot) => Request(data => data.SaveSlot(slot));

    /// <summary>Queues restoration of a saved slot before the next worker frame.</summary>
    /// <param name="slot">Save-slot index to load.</param>
    /// <returns>A task completed with the operation status or its exception.</returns>
    public Task<string> LoadSlot(int slot) => Request(data => data.LoadSlot(slot));

    /// <summary>Queues import of a snapshot file into the requested save slot.</summary>
    /// <param name="path">Snapshot file path accessible to the application.</param>
    /// <param name="slot">Destination save-slot index.</param>
    /// <returns>A task completed with the import result or its exception.</returns>
    public Task<string> ImportState(string path, int slot) => Request(data => data.ImportState(path, slot));

    /// <summary>Queues import of a cartridge save file into the session's selected save state.</summary>
    /// <param name="path">Save file path accessible to the application.</param>
    /// <returns>A task completed with the import result or its exception.</returns>
    public Task<string> ImportSave(string path) => Request(data => data.ImportSave(path));

    /// <summary>Prepares a private diagnostic bundle, persisting current save and recording data when the worker is live.</summary>
    /// <param name="destination">Path where the bundle should be created.</param>
    /// <param name="slot">Save-slot context included in the bundle.</param>
    /// <returns>A task completed when the bundle is prepared or when creation fails.</returns>
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

    /// <summary>Schedules a state operation on the game-owning worker and wakes its command loop.</summary>
    /// <param name="action">Operation to execute against the worker-owned session data.</param>
    /// <returns>A task carrying the operation's status or exception.</returns>
    private Task<string> Request(Func<AndroidSessionData, string> action)
    {
        Task<string> result = commands.Enqueue(action);
        wake.Set();
        return result;
    }

    /// <summary>Stops presentation and worker activity, waits for cleanup, and disposes lifecycle signals.</summary>
    /// <returns>A task that completes after the worker has shut down.</returns>
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

    /// <summary>Owns the emulation loop, dispatches queued state commands, and persists state at pause and shutdown boundaries.</summary>
    private void Run()
    {
        AndroidPcmOutput? output = null;
        AndroidResumeTrace? resumeTrace = null;
        try
        {
            using var data = new AndroidSessionData(root);
            var rasterBuffer = new SuperMetroid.Core.Assets.Rgba32[FrontendFrame.Width * FrontendFrame.Height];
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
                data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
                ushort input = Input.Sample();
                data.Record(input);
                var previousGameState = data.Game.GameState;
                CapturedFrontendFrame frame = data.Game.StepCaptured(input, ++sequence, data.Generation);
                view.SetRoomIdentity(data.Game.GameplayActiveAreaIndex is { } area && data.Game.GameplayActiveRoomIndex is { } room
                    ? $"Room ${(byte)area:X2}/${room:X2} [$8F:{data.Game.GameplayActiveRoomPointer:X4}, state $8F:{data.Game.GameplayActiveRoomStatePointer:X4}]"
                    : "No active room");
                double stepped = clock.Elapsed.TotalMilliseconds;
                var pixels = frame.Snapshot is { } snapshot
                    ? SoftwareFrameSnapshotRenderer.Render(snapshot, rasterBuffer) : frame.Frame.Pixels;
                double rendered = clock.Elapsed.TotalMilliseconds;
                short[] samples = data.Audio.RenderFrame(frame.Frame.AudioCommands);
                if (options.MasterVolumePercent != 100)
                    for (int i = 0; i < samples.Length; i++) samples[i] = (short)(samples[i] * options.MasterVolumePercent / 100);
                double mixed = clock.Elapsed.TotalMilliseconds;
                output?.Submit(samples);
                data.Game.SetAudioAcknowledgements(data.Audio.ReadAcknowledgements());
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
                HostFrameDeadline nextFrame = HostFrameDeadline.AfterFrame(deadline, completedAt);
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

    /// <summary>Stops presentation and records a worker failure in logcat, persistent diagnostics, and the view status.</summary>
    /// <param name="error">Failure that stopped the worker or its audio cleanup.</param>
    /// <param name="append">When true, appends a shutdown failure to the existing error report.</param>
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

    /// <summary>Executes queued tool operations between frames, updates effective options, and settles each request task.</summary>
    /// <param name="data">Worker-owned state targeted by pending operations.</param>
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

    /// <summary>Publishes gameplay options together with audio settings that remain owned by Android preferences.</summary>
    /// <param name="data">Session whose current game and persisted Android options supply the merged snapshot.</param>
    private void RefreshEffectiveOptions(AndroidSessionData data) => Volatile.Write(ref effectiveOptions,
        data.Game.ConfiguredOptions with
        {
            AudioEnabled = data.Options.AudioEnabled,
            MasterVolumePercent = data.Options.MasterVolumePercent,
        });
}
