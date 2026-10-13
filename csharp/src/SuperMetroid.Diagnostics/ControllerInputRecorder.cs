using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.AssetExtraction;

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
    /// <summary>Emulated frames between scheduled journal snapshots, about two seconds at sixty updates per second.</summary>
    private const int FlushIntervalFrames = 120;
    /// <summary>Per-installation folder containing reset-scoped input recordings.</summary>
    private const string RecordingDirectoryName = "input-recordings";

    /// <summary>Serializes frame appends, worker publication, and final shutdown.</summary>
    private readonly Lock gate = new();
    /// <summary>Verified source-cartridge digest embedded in each replay snapshot.</summary>
    private readonly byte[] romSha256;
    /// <summary>Save RAM captured once at reset so replay starts from the same slot state.</summary>
    private readonly byte[] initialSaveRam;
    /// <summary>Game startup options embedded with the input sequence.</summary>
    private readonly SuperMetroidGameOptions gameOptions;
    /// <summary>Installed resource identity captured when the recording starts.</summary>
    private readonly GameContentIdentitySnapshot? contentIdentity;
    /// <summary>UTC start time retained across all periodic snapshots.</summary>
    private readonly DateTimeOffset startedUtc;
    /// <summary>Controller words in exact game-step order.</summary>
    private readonly List<ushort> inputs = [];
    /// <summary>Final-flush hook for process exit outside normal form disposal.</summary>
    private readonly EventHandler processExitHandler;
    /// <summary>Latest asynchronous atomic replacement, observed on later game frames.</summary>
    private Task flushTask = Task.CompletedTask;
    /// <summary>Input count when the most recent periodic snapshot was scheduled.</summary>
    private int frameCountAtLastScheduledFlush;
    /// <summary>Prevents later frames or shutdown paths from publishing after the final snapshot.</summary>
    private bool disposed;

    /// <summary>Captures replay inputs and startup state for one reset-scoped destination.</summary>
    private ControllerInputRecorder(
        string path,
        byte[] romSha256,
        ReadOnlySpan<byte> initialSaveRam,
        SuperMetroidGameOptions gameOptions,
        GameContentIdentity? contentIdentity)
    {
        Path = path;
        this.romSha256 = romSha256;
        this.initialSaveRam = initialSaveRam.ToArray();
        this.gameOptions = gameOptions;
        this.contentIdentity = contentIdentity?.ToSnapshot();
        startedUtc = DateTimeOffset.UtcNow;

        // ProcessExit is a last line of defense for failures outside the WinForms disposal
        // path. Dispose unregisters it, and FlushFinal's lock makes simultaneous shutdown
        // calls harmless instead of racing two atomic replacements.
        processExitHandler = (_, _) => FlushFinal();
        AppDomain.CurrentDomain.ProcessExit += processExitHandler;
    }

    /// <summary>Absolute destination of the current reset's recording.</summary>
    public string Path { get; }

    /// <summary>Number of controller words already recorded, including a failing frame.</summary>
    public int FrameCount
    {
        get { lock (gate) return inputs.Count; }
    }

    /// <summary>
    /// Starts an installed-game journal from the installer-verified source identity without
    /// opening the private cartridge file.
    /// </summary>
    public static ControllerInputRecorder StartInstalled(
        string dataDirectory,
        ReadOnlySpan<byte> initialSaveRam,
        SuperMetroidGameOptions gameOptions,
        GameContentIdentity contentIdentity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectory);
        Ensure.NotNull(gameOptions);
        Ensure.NotNull(contentIdentity);
        if (!contentIdentity.SourceCartridgeSha256.Equals(
                SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Installed content identity does not name the supported source cartridge revision.");
        }
        Ensure.LengthEqual(initialSaveRam, SuperMetroidAddressSpace.SaveRamByteCount);
        return StartCore(
            System.IO.Path.Combine(System.IO.Path.GetFullPath(dataDirectory), RecordingDirectoryName),
            SupportedCartridge.CreateSha256Digest(),
            initialSaveRam,
            gameOptions,
            contentIdentity);
    }

    /// <summary>Creates the recording directory and a timestamped session file destination.</summary>
    private static ControllerInputRecorder StartCore(
        string recordingDirectory,
        byte[] romDigest,
        ReadOnlySpan<byte> initialSaveRam,
        SuperMetroidGameOptions gameOptions,
        GameContentIdentity? contentIdentity)
    {
        Directory.CreateDirectory(recordingDirectory);
        string timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss-fff");
        string path = System.IO.Path.Combine(
            recordingDirectory,
            $"SuperMetroid-input-{timestamp}.smrec");
        return new ControllerInputRecorder(path, romDigest, initialSaveRam, gameOptions, contentIdentity);
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

    /// <summary>Publishes one background atomic write under the same lock as input append decisions.</summary>
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

    /// <summary>Pairs a copied input prefix with the original reset's cartridge, content, save RAM, and options.</summary>
    private ControllerInputRecording CreateRecording(ushort[] snapshot) => new()
    {
        StartedUtc = startedUtc,
        RomSha256 = romSha256,
        ContentIdentity = contentIdentity,
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

    /// <summary>Flushes a temporary recording to disk before replacing the session file.</summary>
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

    /// <summary>Waits for any periodic write and publishes the full input sequence once.</summary>
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

    /// <summary>Publishes the final recording and unregisters the process-exit hook.</summary>
    public void Dispose()
    {
        FlushFinal();
        AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
    }
}
