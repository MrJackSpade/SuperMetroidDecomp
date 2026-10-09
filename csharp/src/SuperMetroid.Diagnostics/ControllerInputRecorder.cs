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
    /// <summary>Input-frame interval between asynchronous journal snapshots.</summary>
    private const int FlushIntervalFrames = 120;
    /// <summary>Directory name used for reset recordings under the host data directory.</summary>
    private const string RecordingDirectoryName = "input-recordings";

    /// <summary>Protects the input journal, shutdown state, and scheduled writer task.</summary>
    private readonly object gate = new();
    /// <summary>Source-cartridge digest embedded in each recording.</summary>
    private readonly byte[] romSha256;
    /// <summary>SRAM snapshot from which this recording begins.</summary>
    private readonly byte[] initialSaveRam;
    /// <summary>Host options captured at recording start.</summary>
    private readonly SuperMetroidGameOptions gameOptions;
    /// <summary>Installed-content identity, when recording an installed game.</summary>
    private readonly GameContentIdentitySnapshot? contentIdentity;
    /// <summary>UTC start time written into the recording header.</summary>
    private readonly DateTimeOffset startedUtc;
    /// <summary>Controller words consumed since this recorder started.</summary>
    private readonly List<ushort> inputs = [];
    /// <summary>Process-exit callback removed when the recorder is disposed.</summary>
    private readonly EventHandler processExitHandler;
    /// <summary>Most recently scheduled background write.</summary>
    private Task flushTask = Task.CompletedTask;
    /// <summary>Input count included in the last scheduled asynchronous snapshot.</summary>
    private int frameCountAtLastScheduledFlush;
    /// <summary>Whether shutdown has closed the recorder and captured its final snapshot.</summary>
    private bool disposed;

    /// <summary>Creates a recorder and registers its final-flush process-exit fallback.</summary>
    /// <param name="path">Destination path of the recording file.</param>
    /// <param name="romSha256">Digest identifying the source cartridge.</param>
    /// <param name="initialSaveRam">Complete SRAM image used to seed replay.</param>
    /// <param name="gameOptions">Host options captured for replay.</param>
    /// <param name="contentIdentity">Optional identity of the installed game content.</param>
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
        ArgumentNullException.ThrowIfNull(gameOptions);
        ArgumentNullException.ThrowIfNull(contentIdentity);
        if (!contentIdentity.SourceCartridgeSha256.Equals(
                SupportedCartridge.Sha256,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                "Installed content identity does not name the supported source cartridge revision.");
        }
        if (initialSaveRam.Length != SuperMetroidAddressSpace.SaveRamByteCount)
            throw new ArgumentException("Recorder startup requires a complete 8 KiB SRAM image.", nameof(initialSaveRam));
        return StartCore(
            System.IO.Path.Combine(System.IO.Path.GetFullPath(dataDirectory), RecordingDirectoryName),
            SupportedCartridge.CreateSha256Digest(),
            initialSaveRam,
            gameOptions,
            contentIdentity);
    }

    /// <summary>Creates the recording directory and starts a timestamped input journal.</summary>
    /// <param name="recordingDirectory">Directory in which the journal is stored.</param>
    /// <param name="romDigest">Source-cartridge digest included in the recording.</param>
    /// <param name="initialSaveRam">SRAM seed copied into the recording.</param>
    /// <param name="gameOptions">Host options captured for replay.</param>
    /// <param name="contentIdentity">Optional installed-content identity.</param>
    /// <returns>The initialized recorder.</returns>
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
    /// <param name="controllerInput">Controller bitfield consumed by the current game frame.</param>
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

    /// <summary>Schedules an immutable input snapshot for asynchronous atomic publication.</summary>
    /// <param name="snapshot">Complete input list captured under the recorder lock.</param>
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

    /// <summary>Creates a recording payload from the immutable input snapshot and start metadata.</summary>
    /// <param name="snapshot">Controller inputs to serialize.</param>
    /// <returns>Recording payload ready for serialization.</returns>
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
    /// <exception cref="ObjectDisposedException">The recorder has already been closed.</exception>
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

    /// <summary>Writes and durably flushes a temporary journal before replacing the destination file.</summary>
    /// <param name="recording">Complete recording payload to publish.</param>
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

    /// <summary>Closes the journal and writes its final input snapshot exactly once.</summary>
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

    /// <summary>Flushes the final recording snapshot and unregisters the process-exit fallback.</summary>
    public void Dispose()
    {
        FlushFinal();
        AppDomain.CurrentDomain.ProcessExit -= processExitHandler;
    }
}
