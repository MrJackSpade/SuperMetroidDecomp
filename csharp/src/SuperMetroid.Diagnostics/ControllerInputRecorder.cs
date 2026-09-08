using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using System.Security.Cryptography;

namespace SuperMetroid.Desktop;

/// <summary>
/// Always-on host journal which turns each reset into a replayable input recording.
/// </summary>
/// <remarks>
/// Gameplay never waits for a periodic disk write. Every two seconds the UI thread copies
/// the small input array and a background task atomically replaces the session file. Dispose
/// waits for that task and performs one final synchronous replacement, covering normal form
/// closure, debugger stop paths which dispose controls, and the registered process-exit hook.
/// </remarks>
internal sealed class ControllerInputRecorder : IDisposable
{
    private const int FlushIntervalFrames = 120;
    private const string RecordingDirectoryName = "input-recordings";

    private readonly object gate = new();
    private readonly byte[] romSha256;
    private readonly byte[] initialSaveRam;
    private readonly SuperMetroidGameOptions gameOptions;
    private readonly DateTimeOffset startedUtc;
    private readonly List<ushort> inputs = [];
    private readonly EventHandler processExitHandler;
    private Task flushTask = Task.CompletedTask;
    private int frameCountAtLastScheduledFlush;
    private bool disposed;

    private ControllerInputRecorder(
        string path,
        byte[] romSha256,
        ReadOnlySpan<byte> initialSaveRam,
        SuperMetroidGameOptions gameOptions)
    {
        Path = path;
        this.romSha256 = romSha256;
        this.initialSaveRam = initialSaveRam.ToArray();
        this.gameOptions = gameOptions;
        startedUtc = DateTimeOffset.UtcNow;

        // ProcessExit is a last line of defense for failures outside the WinForms disposal
        // path. Dispose unregisters it, and FlushFinal's lock makes simultaneous shutdown
        // calls harmless instead of racing two atomic replacements.
        processExitHandler = (_, _) => FlushFinal();
        AppDomain.CurrentDomain.ProcessExit += processExitHandler;
    }

    /// <summary>Absolute destination of the current reset's recording.</summary>
    public string Path { get; }

    /// <summary>Creates a recorder beside the private ROM and captures reset-time SRAM.</summary>
    public static ControllerInputRecorder Start(
        string romPath,
        ReadOnlySpan<byte> initialSaveRam,
        SuperMetroidGameOptions gameOptions,
        string? recordingDirectoryOverride = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(romPath);
        ArgumentNullException.ThrowIfNull(gameOptions);
        if (initialSaveRam.Length != SuperMetroidAddressSpace.SaveRamByteCount)
            throw new ArgumentException("Recorder startup requires a complete 8 KiB SRAM image.", nameof(initialSaveRam));

        string fullRomPath = System.IO.Path.GetFullPath(romPath);
        string romDirectory = System.IO.Path.GetDirectoryName(fullRomPath)
            ?? throw new InvalidOperationException($"ROM path has no containing directory: {fullRomPath}");
        string recordingDirectory = recordingDirectoryOverride is null
            ? System.IO.Path.Combine(romDirectory, RecordingDirectoryName)
            : System.IO.Path.GetFullPath(recordingDirectoryOverride);
        Directory.CreateDirectory(recordingDirectory);
        string timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss-fff");
        string path = System.IO.Path.Combine(
            recordingDirectory,
            $"SuperMetroid-input-{timestamp}.smrec");

        byte[] digest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            digest = SHA256.HashData(rom);
        return new ControllerInputRecorder(path, digest, initialSaveRam, gameOptions);
    }

    /// <summary>
    /// Appends the word before the corresponding game step, preserving the input which
    /// caused an exception even when that frame never returned a rendered image.
    /// </summary>
    public void RecordFrame(ushort controllerInput)
    {
        ushort[]? snapshot = null;
        Task? completedFlush = null;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            inputs.Add(controllerInput);
            if (flushTask.IsCompleted)
                completedFlush = flushTask;
            if (inputs.Count - frameCountAtLastScheduledFlush >= FlushIntervalFrames &&
                flushTask.IsCompleted)
            {
                // Observe the completed worker before replacing it. Without this call a
                // faulted Task looked just as schedulable as a successful one, so every
                // later periodic write could fail while gameplay continued unaware.
                snapshot = inputs.ToArray();
                frameCountAtLastScheduledFlush = inputs.Count;
            }
        }

        // Poll the worker on every emulated frame, not merely at the next two-second flush
        // boundary. This is the earliest safe point at which its exception can be moved
        // back onto the UI thread and made visible to the developer.
        completedFlush?.GetAwaiter().GetResult();
        if (snapshot is not null)
        {
            ScheduleFlush(snapshot);
        }
    }

    private void ScheduleFlush(ushort[] snapshot)
    {
        // Publication under the same lock as RecordFrame prevents a second caller from
        // observing Completed between the decision above and assignment of the new task.
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            flushTask = Task.Run(() => WriteAtomically(CreateRecording(snapshot)));
        }
    }

    private ControllerInputRecording CreateRecording(ushort[] snapshot) => new()
    {
        StartedUtc = startedUtc,
        RomSha256 = romSha256,
        InitialSaveRam = initialSaveRam,
        GameOptions = gameOptions,
        ControllerInputs = snapshot,
    };

    /// <summary>
    /// Durably publishes the snapshot containing a frame which just threw from the game
    /// dispatcher. This is intentionally synchronous only on the exceptional path: leaving
    /// the word in the normal 120-frame asynchronous window would omit the one input that is
    /// most valuable for reproducing the failure while Visual Studio is stopped at the throw.
    /// </summary>
    public void FlushAfterFrameFailure()
    {
        Task pendingFlush;
        ushort[] snapshot;
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            pendingFlush = flushTask;
            snapshot = inputs.ToArray();
        }

        // An older asynchronous snapshot may still own the temporary path. Let it finish
        // first, then atomically replace the destination with the failure-inclusive copy.
        // GetResult deliberately rethrows a worker failure on the UI thread.
        pendingFlush.GetAwaiter().GetResult();
        WriteAtomically(CreateRecording(snapshot));
    }

    private void WriteAtomically(ControllerInputRecording recording)
    {
        string temporaryPath = Path + ".tmp";
        using (var destination = new FileStream(
            temporaryPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 64 * 1024,
            FileOptions.SequentialScan))
        {
            recording.Write(destination);
            destination.Flush(flushToDisk: true);
        }
        File.Move(temporaryPath, Path, overwrite: true);
    }

    private void FlushFinal()
    {
        Task pendingFlush;
        ushort[] snapshot;
        lock (gate)
        {
            if (disposed)
                return;
            disposed = true;
            pendingFlush = flushTask;
            snapshot = inputs.ToArray();
        }

        pendingFlush.GetAwaiter().GetResult();
        WriteAtomically(CreateRecording(snapshot));
    }

    public void Dispose()
    {
        FlushFinal();
        AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
    }
}
