using SuperMetroid.Core;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Microsoft.Win32;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Rendering.Direct3D11;

namespace SuperMetroid.Desktop;

/// <summary>
/// Thin keyboard/gamepad/debugger host for the installed-content game dispatcher.
/// </summary>
public sealed class PlayableGameControl : UserControl
{
    /// <summary>Installed map art used for file loading and save-file migration.</summary>
    private SuperMetroid.Core.Assets.AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Installed projectile display data selected at session restart.</summary>
    private SuperMetroid.AssetExtraction.InstalledProjectilePresentation? projectilePresentation;
    /// <summary>Installed gameplay and frontend artwork rebound after debugger restores.</summary>
    private GameArtwork? artwork;
    /// <summary>Human-readable JSON save path in the player data directory.</summary>
    private readonly string saveFilePath;
    /// <summary>Prior .srm path read only for one-time save migration.</summary>
    private readonly string legacySaveRamPath;
    /// <summary>Host options retained unchanged through in-window restarts.</summary>
    private readonly SuperMetroidGameOptions gameOptions;
    /// <summary>Optional immutable controller recording driving frame input.</summary>
    private readonly ControllerInputRecording? replay;
    /// <summary>Recoverable error reporter installed by the desktop host.</summary>
    private readonly GitHubErrorReporter? errorReporter;
    /// <summary>Retains audio commands across recoverable device failures.</summary>
    private readonly AudioFrameRecovery audioFrameRecovery = new();
    /// <summary>Software-rendered canvas and keyboard-focus target.</summary>
    internal readonly RuntimeCanvas canvas = new() { Dock = DockStyle.Fill, TabStop = true };
    /// <summary>Toolbar text for frame, room, state, and recovery feedback.</summary>
    private readonly ToolStripLabel statusLabel = HostToolbarLayout.CreateStatusLabel();
    /// <summary>WinForms wakeup timer; elapsed time determines actual game updates.</summary>
    private readonly System.Windows.Forms.Timer playbackTimer = new() { Interval = 8 };
    /// <summary>Monotonic clock used to accumulate 60 Hz gameplay updates.</summary>
    private readonly Stopwatch playbackClock = new();
    /// <summary>Tracks simulation, paint, and renderer timing for the status display.</summary>
    internal readonly FrameTimingCounter frameTimings = new();
    /// <summary>Pressed keyboard buttons cleared on host deactivation.</summary>
    private readonly HostKeyboardInputState keyboard = new();
    /// <summary>Prevents stale inputs when the desktop session changes activation.</summary>
    private readonly HostInputActivationGate inputActivation = new();
    /// <summary>Current Windows gamepad sample merged with keyboard input.</summary>
    private readonly WindowsGamepadInput gamepad = new();
    /// <summary>Elapsed wall-clock updates waiting to be simulated.</summary>
    private double pendingPlaybackFrames;
    /// <summary>Mutable emulated memory for the current installed-content session.</summary>
    internal SuperMetroidAddressSpace addressSpace = null!;
    /// <summary>Current frontend dispatcher and its retained display state.</summary>
    internal SuperMetroidGame game = null!;
    /// <summary>Installed audio catalog the session binds for synthesis.</summary>
    private ExtractedAudioAssetCatalog? audioAssets;
    /// <summary>Host-owned PCM storage; the device copies each frame on submit.</summary>
    private readonly short[] audioFrame = new short[CartridgeAudioSession.PcmSamplesPerFrame];
    /// <summary>Optional selected audio override directory for this session.</summary>
    private readonly string? installedAudioDirectory;
    /// <summary>Installation root holding player saves, recordings, and debugger states.</summary>
    private readonly string playerDataDirectory;
    /// <summary>Digest of selected installed content used for replay and state compatibility.</summary>
    private GameContentIdentity? installedContentIdentity;
    /// <summary>Recoverable wave output fed by accepted audio frames.</summary>
    internal RecoveringAudioOutput? audioDevice;
    /// <summary>Crash-reproduction controller recorder for the active session.</summary>
    private ControllerInputRecorder? inputRecorder;
    /// <summary>Persistent debugger snapshots bound to installed content identity.</summary>
    private DebuggerSaveStateStore stateStore = null!;
    /// <summary>Next controller word to read from an active replay.</summary>
    private int replayFrameIndex;
    /// <summary>Room pointer last shown in the status label.</summary>
    private ushort? displayedRoomPointer;
    /// <summary>Most recent frame timing sample used by the toolbar.</summary>
    private FrameTimingSnapshot? latestFrameTiming;
    /// <summary>Latest error text retained while simulation recovers.</summary>
    private string? lastRecoverableError;
    /// <summary>Form subscribed for activation and deactivation input clearing.</summary>
    private Form? inputLifecycleForm;
    /// <summary>Whether Windows session-switch notifications are currently registered.</summary>
    private bool sessionSwitchAttached;

    /// <summary>
    /// Wall-clock update rate, separate from the translated frame counter. Accumulating
    /// elapsed time avoids treating a 16 ms WinForms tick as an exact 60 Hz interval.
    /// </summary>
    private const double TargetFramesPerSecond = 60.0;
    /// <summary>Maximum updates processed after a delayed UI tick before playback yields.</summary>
    private const int MaximumCatchUpFrames = 4;

    /// <summary>Starts from validated installed assets without naming or opening a ROM file.</summary>
    public PlayableGameControl(
        GameInstallation installation,
        SuperMetroidGameOptions gameOptions,
        ControllerInputRecording? replay = null,
        GitHubErrorReporter? errorReporter = null,
        string? audioDirectory = null)
    {
        Ensure.NotNull(installation);
        installedAudioDirectory = audioDirectory;
        playerDataDirectory = Path.GetFullPath(installation.Root);
        // The installed save basename is stable even when the private import copy
        // has been removed. It matches the basename used by previous releases.
        string saveBase = Path.Combine(playerDataDirectory, GameInstallationLayout.RomFileName);
        saveFilePath = Path.ChangeExtension(saveBase, GameSaveJsonFormat.FileExtension);
        legacySaveRamPath = Path.ChangeExtension(saveBase, ".srm");
        this.gameOptions = Ensure.NotNull(gameOptions);
        this.replay = replay;
        this.errorReporter = errorReporter;
        Dock = DockStyle.Fill;

        var toolStrip = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            Dock = DockStyle.Fill,
        };
        var restartButton = new ToolStripButton("Restart");
        var playButton = new ToolStripButton("Pause") { CheckOnClick = true, Checked = true };
        var stepButton = new ToolStripButton("Step");
        var stateSlot = new ToolStripComboBox
        {
            AutoSize = false,
            Width = 60,
            DropDownStyle = ComboBoxStyle.DropDownList,
            ToolTipText = "Debugger state: manual slots 0-9 or automatic door recovery",
        };
        for (int slot = 0; slot < 10; slot++)
            stateSlot.Items.Add(slot.ToString(System.Globalization.CultureInfo.InvariantCulture));
        stateSlot.Items.Add("auto");
        stateSlot.SelectedIndex = 0;
        var saveStateButton = new ToolStripButton("Save State")
        {
            Enabled = replay is null,
            ToolTipText = "Persist the complete emulated state in the selected slot",
        };
        var loadStateButton = new ToolStripButton("Load State")
        {
            Enabled = replay is null,
            ToolTipText = "Restore the selected debugger state; different builds warn and attempt compatible loading",
        };
        toolStrip.Items.Add(restartButton);
        toolStrip.Items.Add(playButton);
        toolStrip.Items.Add(stepButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(new ToolStripLabel("State slot"));
        toolStrip.Items.Add(stateSlot);
        toolStrip.Items.Add(saveStateButton);
        toolStrip.Items.Add(loadStateButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(statusLabel);

        var help = new Label
        {
            Dock = DockStyle.Bottom,
            AutoSize = false,
            Height = 58,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Gamepad: D-pad/stick move; south dash; east jump; west cancel; north fire; shoulders aim\r\n" +
                   "Keyboard: arrows move  |  Space/X jump  |  Z dash  |  " +
                   "S: fire (X)  |  A: item cancel (Y)\r\n" +
                   HostControlHelp.KeyboardShoulders + "  |  Enter: Start  |  Shift: Select",
        };

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        HostToolbarLayout.ConfigureGameplayColumn(layout);
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolStrip, 0, 0);
        layout.Controls.Add(canvas, 0, 1);
        Controls.Add(layout);
        Controls.Add(help);

        restartButton.Click += async (_, _) => await RestartAsync();
        playButton.CheckedChanged += (_, _) =>
        {
            playButton.Text = playButton.Checked ? "Pause" : "Play";
            SetPlaying(playButton.Checked);
            canvas.Focus();
        };
        stepButton.Click += (_, _) => { StepFrame(); canvas.Focus(); };
        saveStateButton.Click += (_, _) =>
        {
            SaveDebuggerState(stateSlot.SelectedIndex);
            canvas.Focus();
        };
        stateSlot.SelectedIndexChanged += (_, _) =>
            saveStateButton.Enabled = replay is null && stateSlot.SelectedIndex != DebuggerStateFormat.AutomaticSlot;
        loadStateButton.Click += async (_, _) =>
        {
            await LoadDebuggerState(stateSlot.SelectedIndex);
            canvas.Focus();
        };
        playbackTimer.Tick += (_, _) => AdvancePlaybackClock();
        canvas.FramePainted += frameTimings.RecordPaint;

        // Keyboard messages are captured by ProcessKeyPreview below, above every child.
        // Keep the clear tied to the whole gameplay host rather than the canvas: moving
        // focus between the canvas and state controls is no longer allowed to manufacture
        // a release, while Alt-Tab still cannot leave a direction latched indefinitely.
        Leave += (_, _) => keyboard.Clear();

        // Establish a valid timestamp before Restart's reset frame can contribute a timing
        // observation. Restart clears it again after that intentionally synchronous work.
        ResetFrameTimings();
        Restart();
        SetPlaying(playing: true);
    }

    /// <summary>Loads the selected audio override, or the installation's stock catalog.</summary>
    private ExtractedAudioAssetCatalog LoadAudioAssets()
    {
        var installation = new GameInstallation(playerDataDirectory);
        if (installedAudioDirectory is null || Path.GetFullPath(installedAudioDirectory).Equals(
                Path.GetFullPath(installation.AudioDirectory), StringComparison.OrdinalIgnoreCase))
        {
            return installation.LoadAudio();
        }
        return ExtractedAudioAssetCatalog.Load(installedAudioDirectory);
    }

    /// <summary>Starts a new display generation before resetting a software-rendered session.</summary>
    private void Restart()
    {
        if (gpuWorker is not null) throw new InvalidOperationException("Live restart must use the asynchronous generation boundary.");
        BeginDisplayGeneration();
        RestartCore();
    }

    /// <summary>Recreates game, memory, audio, recorder, and artwork from installed content.</summary>
    private void RestartCore()
    {
        keyboard.Clear();
        // waveOut may still own several queued buffers when Restart is clicked. Dispose the
        // device before its pinned storage.
        audioDevice?.Dispose();
        audioDevice = null;
        inputRecorder?.Dispose();
        inputRecorder = null;
        replayFrameIndex = 0;
        // Restart must retain host configuration. Re-reading the INI here would make an
        // ordinary in-window reset depend on a mid-session disk edit and would obscure the
        // exact options with which the debugger-visible session was constructed.
        var installation = new GameInstallation(playerDataDirectory);
        addressSpace = installation.OpenRuntimeAddressSpace();
        mapPresentation = installation.LoadMaps();
        ExtractedAudioAssetCatalog selectedAudioAssets = LoadAudioAssets();
        audioAssets = selectedAudioAssets;
        if (gameOptions.AudioEnabled)
            audioDevice = CreateAudioDevice();
        if (replay is null)
            LoadSaveRamFromDisk();
        else
            LoadReplaySaveRam();
        game = new SuperMetroidGame(addressSpace, gameOptions);
        // The session synthesizes in every configuration so a debugger state always carries
        // its player; whether samples reach a device is the host's choice alone.
        game.BindAudio(selectedAudioAssets);
        projectilePresentation = installation.LoadProjectiles();
        artwork = installation.LoadGameArtwork(
            mapPresentation ?? throw new InvalidOperationException("Installed map identity is unavailable."),
            projectilePresentation);
        game.BindArtwork(artwork);
        Console.WriteLine($"Projectile compositions: stock={projectilePresentation.StockSha256}, selected={projectilePresentation.SelectedSha256} ({playerDataDirectory}).");
        installedContentIdentity = GameContentIdentity.Create(
            selectedAudioAssets,
            mapPresentation,
            projectilePresentation,
            InstalledArtworkIdentity.Create(artwork));
        Console.WriteLine(
            $"Installed content: {installedContentIdentity.CompositeSha256}; " +
            $"definitions={installedContentIdentity.CompiledDefinitionsBuildId:D}, " +
            $"audio={installedContentIdentity.AudioContentSha256}, " +
            $"maps={installedContentIdentity.MapContentSha256}, " +
            $"projectiles={installedContentIdentity.ProjectileContentSha256}.");
        foreach ((string domain, string digest) in installedContentIdentity.AdditionalContentSha256.OrderBy(pair => pair.Key))
            Console.WriteLine($"Installed content component: {domain}={digest}.");
        if (replay is not null)
            ReportReplayContentCompatibility();
        stateStore = CreateStateStore();
        if (replay is null)
        {
            game.SaveRamChanged += PersistSaveRamToDisk;
            // The reset seed is captured after disk SRAM has been validated/copied but
            // before the first dispatcher call. A crash in state zero is therefore just as
            // reproducible as a gameplay failure hours later.
            inputRecorder = StartInputRecorder();
            Console.WriteLine($"Recording controller input to {inputRecorder.Path}");
        }
        // Execute reset once so the first visible debugger frame is state one's native setup.
        FrontendFrame resetFrame = AdvanceOneFrame(forcedInput: replay is null ? (ushort)0 : null)
            ?? throw new InvalidDataException("Replay contains no reset frame.");
        RefreshFrame(resetFrame);
        ResetFrameTimings();
        canvas.Focus();
    }

    /// <summary>Persists the selected debugger slot with current game, memory, and SPC state.</summary>
    internal void SaveDebuggerState(int slot)
    {
        if (replay is not null)
            throw new InvalidOperationException("Debugger states are disabled during an input replay.");
        DebuggerSaveStateMetadata metadata = stateStore.Save(
            slot, addressSpace, game);
        statusLabel.Text =
            $"saved state {slot} | frame {metadata.FrameNumber} | " +
            FormatStateRoom(metadata.RoomPointer, metadata.RoomStatePointer);
        Console.WriteLine($"Saved debugger state slot {slot}: {metadata.Path}");
    }

    /// <summary>Validates a state, drains old presentation work, then rebinds game and audio.</summary>
    internal async Task LoadDebuggerState(int slot)
    {
        if (replay is not null)
            throw new InvalidOperationException("Debugger states are disabled during an input replay.");

        // Decode and check compatibility before changing the live game or its audio.
        DebuggerSaveStateLoadResult loaded;
        try
        {
            if (!stateStore.TryLoad(slot, out loaded))
            {
                statusLabel.Text = $"state slot {DebuggerSaveStateStore.SlotName(slot)} is empty";
                Console.Error.WriteLine($"Debugger save-state slot {DebuggerSaveStateStore.SlotName(slot)} is empty: {stateStore.GetSlotPath(slot)}");
                canvas.Focus();
                return;
            }
            if (gpuWorker is not null && loaded.Game.GetRetainedDisplay(displaySequence + 1, displayGeneration + 1) is null)
                throw new NotSupportedException("This debugger state contains legacy pixels, not a captured scene. Load it with Renderer=Software.");
        }
        catch (Exception exception)
        {
            statusLabel.Text = $"state slot {DebuggerSaveStateStore.SlotName(slot)} could not load: {exception.Message}";
            Console.Error.WriteLine(exception.ToString());
            canvas.Focus();
            return;
        }

        bool resumePlayback = playbackTimer.Enabled;
        SetPlaying(playing: false);
        keyboard.Clear();
        inputRecorder?.Dispose();
        inputRecorder = null;
        audioDevice?.Dispose();
        audioDevice = null;

        // Disable frame-affecting controls while the old presentation drains.
        // Await keeps the UI pump alive; restore only after old GPU work loses
        // authority to present. No new simulation tick runs across this boundary.
        Enabled = false;
        try { if (!await BeginDisplayGenerationAsync()) return; }
        finally { if (!IsDisposed) Enabled = true; }
        if (rendererStopping || IsDisposed) return;
        addressSpace = loaded.AddressSpace;
        game = loaded.Game;
        game.BindArtwork(artwork ?? throw new InvalidOperationException(
            "Restored desktop session has no installed artwork."));
        game.BindAudio(audioAssets ?? throw new InvalidOperationException(
            "Restored desktop session has no installed audio."));
        pendingDisplay = game.GetRetainedDisplay(++displaySequence, displayGeneration);
        game.SaveRamChanged += PersistSaveRamToDisk;
        displayedRoomPointer = null;
        pendingPlaybackFrames = 0;

        if (gameOptions.AudioEnabled)
            audioDevice = CreateAudioDevice();

        // Begin a new crash recorder at the restored boundary. The debugger state itself
        // is the deterministic seed for this continuation and is printed beside the new
        // recording path so both files can be attached to a report.
        inputRecorder = StartInputRecorder();
        Console.WriteLine(
            $"Loaded debugger state slot {slot}: {loaded.Metadata.Path}{Environment.NewLine}" +
            $"Recording post-state controller input to {inputRecorder.Path}");

        PublishGpuDisplay();
        // The retained packet already owns the display. Reading CurrentFrame here
        // would rasterize it solely to populate status labels, even in GPU mode.
        RefreshFrame(pendingDisplay is not null ? game.CurrentFrameMetadata : game.CurrentFrame);
        statusLabel.Text =
            (loaded.Warnings.Count != 0 ? "WARNING: state compatibility differs | " : "") +
            $"loaded state {DebuggerSaveStateStore.SlotName(slot)} | frame {loaded.Metadata.FrameNumber} | " +
            FormatStateRoom(loaded.Metadata.RoomPointer, loaded.Metadata.RoomStatePointer);
        SetPlaying(resumePlayback);
    }

    /// <summary>Formats a room/state identity or a frontend-only state for toolbar text.</summary>
    private static string FormatStateRoom(ushort? room, ushort? state) =>
        room is ushort roomPointer && state is ushort statePointer
            ? $"room $8F:{roomPointer:X4}/state $8F:{statePointer:X4}"
            : "frontend/no active room";

    /// <summary>
    /// Seeds replay from its cartridge-compatible SRAM image and rejects a different ROM
    /// revision before the first input can produce a misleading divergence.
    /// </summary>
    private void LoadReplaySaveRam()
    {
        if (replay is null)
            throw new InvalidOperationException("Replay SRAM requested without a replay.");

        byte[] actualDigest = SupportedCartridge.CreateSha256Digest();
        if (!CryptographicOperations.FixedTimeEquals(actualDigest, replay.RomSha256))
        {
            throw new InvalidDataException(
                "The replay was recorded from a different ROM image (SHA-256 mismatch).");
        }
        SaveRamImage.Load(addressSpace, replay.InitialSaveRam);
    }

    /// <summary>Creates a debugger-state store keyed to options and installed content identity.</summary>
    private DebuggerSaveStateStore CreateStateStore()
    {
        return DebuggerSaveStateStore.ForInstalledGame(
            playerDataDirectory,
            gameOptions,
            installedContentIdentity ?? throw new InvalidOperationException(
                "Installed desktop session has no content identity."));
    }

    /// <summary>Begins a replay recording seeded from the current battery-backed SRAM image.</summary>
    private ControllerInputRecorder StartInputRecorder()
    {
        return ControllerInputRecorder.StartInstalled(
            playerDataDirectory,
            SaveRamImage.Capture(addressSpace),
            gameOptions,
            installedContentIdentity ?? throw new InvalidOperationException(
                "Installed desktop session has no content identity."));
    }

    /// <summary>Writes installed-content differences against the recording before playback.</summary>
    private void ReportReplayContentCompatibility()
    {
        if (replay is null)
            throw new InvalidOperationException("Replay compatibility requested without a replay.");
        var contentIdentity = installedContentIdentity ?? throw new InvalidOperationException(
            "Installed desktop session has no content identity.");
        foreach (string warning in contentIdentity.GetCompatibilityWarnings(
                     replay.ContentIdentity,
                     "controller recording"))
        {
            Console.WriteLine($"REPLAY COMPATIBILITY WARNING: {warning}");
        }
    }

    /// <summary>
    /// Restores the same 8 KiB battery-backed image an emulator associates with the ROM.
    /// A malformed file is rejected explicitly; silently padding/truncating it could turn
    /// corruption into an apparently valid checksum pair.
    /// </summary>
    private void LoadSaveRamFromDisk()
    {
        GameSaveLoadResult result = GameSaveFileStore.LoadOrMigrate(
            addressSpace,
            saveFilePath,
            legacySaveRamPath,
            mapPresentation ?? throw new InvalidOperationException("Save loading requires installed maps."));
        if (result.MigratedLegacySram)
        {
            Console.WriteLine(
                $"Migrated legacy save RAM '{legacySaveRamPath}' to human-readable " +
                $"JSON '{result.JsonPath}'. The original .srm was retained unchanged.");
        }
    }

    /// <summary>
    /// Flushes translated SRAM mutations synchronously. The cartridge call is infrequent,
    /// and completing the write before the next frame preserves the save even if the host is
    /// stopped at a breakpoint or closed immediately after the Ceres checkpoint.
    /// </summary>
    private void PersistSaveRamToDisk() =>
        GameSaveFileStore.WriteAtomic(addressSpace, saveFilePath,
            mapPresentation ?? throw new InvalidOperationException("Save persistence requires installed maps."));

    /// <summary>Defers simulation while the enabled audio output cannot accept another frame.</summary>
    private RecoveringAudioOutput CreateAudioDevice() => new(() => new WaveOutAudioDevice(
        CartridgeAudioRenderer.SampleRate,
        CartridgeAudioRenderer.ChannelCount,
        CartridgeAudioSession.PcmSamplesPerFrame,
        gameOptions.MasterVolumePercent));

    /// <summary>Whether the audio device can take another frame; no device never blocks pacing.</summary>
    private bool CanAdvanceAudioFrame() => audioDevice is null || audioDevice.CanAcceptFrame;

    /// <summary>Advances one accepted frontend frame and refreshes its display if available.</summary>
    internal void StepFrame(ushort? forcedInput = null)
    {
        FrontendFrame? frame = AdvanceOneFrame(forcedInput);
        if (frame is not null)
            RefreshFrame(frame.Value);
    }

    /// <summary>
    /// Selects exactly one live or replay word, records it before execution, and advances
    /// the translated dispatcher. Null means exhausted replay or deferred audio capacity, never input zero.
    /// </summary>
    private FrontendFrame? AdvanceOneFrame(ushort? forcedInput = null)
    {
        if (!CanAdvanceAudioFrame()) return null;
        ushort input;
        if (replay is not null)
        {
            if (replayFrameIndex >= replay.ControllerInputs.Length)
            {
                SetPlaying(playing: false);
                return null;
            }
            input = replay.ControllerInputs[replayFrameIndex++];
        }
        else
        {
            input = forcedInput ?? BuildControllerWord();
            inputRecorder?.RecordFrame(input);
        }
        string beforeFrameContext = game.CaptureGameplayFailureContext();
        var previousGameState = game.GameState;
        try
        {
            long frameStarted = Stopwatch.GetTimestamp();
            var captured = game.StepCaptured(input, ++displaySequence, displayGeneration);
            pendingDisplay = captured.Snapshot;
            FrontendFrame frame = captured.Frame;

            // The session already advanced its SPC model and synthesized this update's samples
            // inside Step; the bank $82 handshake never depends on this host. Only submission
            // is recoverable here: a device failure cannot change gameplay.
            if (audioDevice is not null)
            {
                audioFrameRecovery.Run(() =>
                {
                    game.CopyAudioFrame(audioFrame);
                    audioDevice.Submit(audioFrame);
                }, exception =>
                {
                    lastRecoverableError = errorReporter?.Report(exception, new GitHubErrorContext(
                        Boundary: "audio submission after completed gameplay frame",
                        FrameNumber: game.FrameNumber,
                        GameState: $"${(ushort)game.GameState:X2} {game.GameState}",
                        Phase: game.CurrentFrame.Phase,
                        ControllerInput: input,
                        RoomPointer: game.GameplayActiveRoomPointer,
                        RoomStatePointer: game.GameplayActiveRoomStatePointer,
                        DoorPointer: game.GameplayActiveDoorPointer,
                        InputRecordingPath: inputRecorder?.Path)) ?? "audio failure (see console; GitHub reporting disabled)";
                });
            }
            DoorTransitionAutosave.TrySave(gameOptions.DoorTransitionAutosave, replay is not null,
                previousGameState, stateStore, addressSpace, game);
            frameTimings.RecordEmulatedFrame(Stopwatch.GetTimestamp() - frameStarted);
            PublishGpuDisplay();
            return frame;
        }
        catch (Exception frameException)
        {
            // RecordFrame runs before Step specifically so the throwing controller word is
            // already present. Force that snapshot to disk now: a debugger can leave the
            // process paused indefinitely, so ProcessExit/Dispose and the next periodic
            // flush are not reliable ways to preserve the reproducing frame.
            try
            {
                inputRecorder?.FlushAfterFrameFailure();
            }
            catch (Exception recorderException)
            {
                // Preserve both failures. Replacing a cartridge/runtime exception with a
                // secondary diagnostic-write exception would be another silent loss of the
                // information needed to reproduce the frame.
                frameException = new AggregateException(
                    "The emulated frame and its emergency input-recording flush both failed.",
                    frameException,
                    recorderException);
            }

            string frameDiagnostics = GameplayFrameFailureDiagnostics.Format(
                beforeFrameContext, game.CaptureGameplayFailureContext(), input,
                replay is not null ? "in-memory replay (path unavailable)" : inputRecorder?.Path,
                replay is not null ? replayFrameIndex - 1 : inputRecorder?.FrameCount - 1);
            // Printed before either fatal rethrow or recoverable reporting. The session
            // log/ZIP captures stderr even when GitHub reporting is disabled.
            try
            {
                Console.Error.WriteLine(frameDiagnostics);
                Console.Error.Flush();
            }
            catch (Exception diagnosticException)
            {
                frameException = new AggregateException(
                    "The emulated frame and diagnostic console write both failed.",
                    frameException, diagnosticException);
            }

            if (errorReporter is null)
            {
                ExceptionDispatchInfo.Capture(frameException).Throw();
                throw new UnreachableException();
            }

            // A managed exception cannot resume inside the cartridge routine that threw.
            // The opt-in development mode therefore preserves the complete diagnostic and
            // tries the next dispatcher frame from whatever state the failed frame reached.
            // This is intentionally a host recovery boundary, not a claim that the partial
            // state is cartridge-correct.
            lastRecoverableError = errorReporter.Report(
                frameException,
                new GitHubErrorContext(
                    Boundary: "playable emulated-frame boundary",
                    FrameNumber: game.FrameNumber,
                    GameState: $"${(ushort)game.GameState:X2} {game.GameState}",
                    Phase: game.CurrentFrame.Phase,
                    ControllerInput: input,
                    RoomPointer: game.GameplayActiveRoomPointer,
                    RoomStatePointer: game.GameplayActiveRoomStatePointer,
                    DoorPointer: game.GameplayActiveDoorPointer,
                    InputRecordingPath: inputRecorder?.Path,
                    FrameDiagnostics: frameDiagnostics));
            return game.CurrentFrame;
        }
    }

    /// <summary>
    /// Advances the translated dispatcher according to measured wall-clock time.
    /// </summary>
    private void AdvancePlaybackClock()
    {
        double elapsedSeconds = playbackClock.Elapsed.TotalSeconds;
        playbackClock.Restart();

        double maximumCatchUpSeconds = MaximumCatchUpFrames / TargetFramesPerSecond;
        frameTimings.RecordLateFrames(
            Math.Max(0, elapsedSeconds - maximumCatchUpSeconds) * TargetFramesPerSecond);

        // A breakpoint, window drag, or suspended workstation must not cause hundreds of
        // invisible catch-up frames on resume. Four frames retain modest scheduler jitter
        // without making a debug pause alter several seconds of game state.
        elapsedSeconds = Math.Min(
            elapsedSeconds,
            maximumCatchUpSeconds);
        pendingPlaybackFrames += elapsedSeconds * TargetFramesPerSecond;

        int framesToRun = Math.Min((int)pendingPlaybackFrames, MaximumCatchUpFrames);
        if (framesToRun == 0)
            return;

        var batch = PlaybackFrameBatch.Run(
            framesToRun, BuildControllerWord, input => AdvanceOneFrame(input), CanAdvanceAudioFrame);
        pendingPlaybackFrames -= batch.CompletedFrames;
        // A stalled endpoint must not accumulate an unbounded catch-up debt.
        pendingPlaybackFrames = Math.Min(pendingPlaybackFrames, MaximumCatchUpFrames);
        if (batch.LastFrame is { } frame)
            RefreshFrame(frame);
    }

    /// <summary>Starts or pauses wall-clock playback without changing translated state.</summary>
    internal void SetPlaying(bool playing)
    {
        pendingPlaybackFrames = 0;
        playbackClock.Restart();
        ResetFrameTimings();
        playbackTimer.Enabled = playing;
        if (!playing && audioDevice is not null)
            audioDevice.Reset();
    }

    /// <summary>Refreshes presentation, room caption, frame timing, and recovery status.</summary>
    private void RefreshFrame(FrontendFrame frame)
    {
        RefreshDisplay(frame);
        RefreshRoomIdentity();
        if (frameTimings.TryTakeSnapshot(Stopwatch.GetTimestamp(), out FrameTimingSnapshot timing))
        {
            latestFrameTiming = timing;
            RefreshRendererTimingDetail();
        }

        string gameStatus =
            $"state ${((ushort)frame.GameState):X2} {frame.GameState}  |  {frame.Phase}  |  frame {frame.FrameNumber}" +
            (gamepad.DeviceName is null ? string.Empty : $"  |  pad: {gamepad.DeviceName}") +
            (replay is null
                ? string.Empty
                : $"  |  replay {replayFrameIndex}/{replay.ControllerInputs.Length}");
        string timingStatus = latestFrameTiming?.ToToolbarText() ??
            "emu --.- | paint --.- | step --.-/--.- ms | late --.-";
        statusLabel.Text = timingStatus + GpuTimingText;
        string errorStatus = lastRecoverableError is null
            ? string.Empty
            : $"{Environment.NewLine}Last recoverable error: {lastRecoverableError} (see console/GitHub)";
        statusLabel.ToolTipText = latestFrameTiming is FrameTimingSnapshot snapshot
            ? $"{snapshot.ToDiagnosticText()}{Environment.NewLine}{gameStatus}{errorStatus}"
            : gameStatus + errorStatus;
        statusLabel.ToolTipText += rendererTimingDetail;
        if (audioDevice is { } outputAudio)
        {
            var health = outputAudio.QueueHealth;
            statusLabel.ToolTipText += $"{Environment.NewLine}Audio queue: managed {health.ManagedQueued}; " +
                $"native before refill min/max {health.MinimumNativeQueued}/{health.MaximumNativeQueued}; " +
                $"observed empty refills {health.EmptyBeforeRefill}/{health.RefillObservations} (startup/reset excluded)";
        }
    }

    /// <summary>
    /// Discards statistics from startup, a debugger pause, or a state restore so the next
    /// one-second sample describes continuous playback rather than time spent intentionally stopped.
    /// </summary>
    private void ResetFrameTimings()
    {
        latestFrameTiming = null;
        frameTimings.Reset(Stopwatch.GetTimestamp());
    }

    /// <summary>
    /// Publishes both a human label and the cartridge's exact room/state addresses in the
    /// window caption. The pointer is the durable identifier: labels are debugger sugar and
    /// deliberately fall back to "Room" instead of pretending every bank-$8F record has
    /// already been named by the translation.
    /// </summary>
    private void RefreshRoomIdentity()
    {
        ushort? roomPointer = game.GameplayActiveRoomPointer;
        if (roomPointer == displayedRoomPointer)
            return;
        displayedRoomPointer = roomPointer;

        Form? host = FindForm();
        if (host is null)
            return;
        if (roomPointer is not ushort pointer)
        {
            host.Text = "Super Metroid C#";
            return;
        }

        string name = GetKnownRoomName(pointer);
        ushort state = game.GameplayActiveRoomStatePointer
            ?? throw new InvalidOperationException(
                $"Room $8F:{pointer:X4} has no selected room-state pointer.");
        RoomIdentity identity = game.GameplayActiveRoomIdentity
            ?? throw new InvalidOperationException(
                $"Room $8F:{pointer:X4} has no logical room identity.");
        host.Text =
            $"Super Metroid C# — RoomId: {identity} — {name} " +
            $"[$8F:{pointer:X4}, state $8F:{state:X4}]";
    }

    /// <summary>
    /// Caption names for the practical Ceres-to-Bombs playthrough slice. This is a partial
    /// label table over all 262 room headers: unnamed rooms remain fully reportable by
    /// address in the caption, so extending the table never gates gameplay.
    /// </summary>
    private static readonly FrozenDictionary<ushort, string> KnownRoomNames = new Dictionary<ushort, string>
    {
        [RoomHeaderPointers.LandingSite] = "Landing Site",
        [RoomHeaderPointers.ParlorAndAlcatraz] = "Parlor and Alcatraz",
        [RoomHeaderPointers.Climb] = "Climb",
        [RoomHeaderPointers.PitRoom] = "Pit Room",
        [RoomHeaderPointers.BombTorizoRoom] = "Bomb Torizo Room",
        [RoomHeaderPointers.Flyway] = "Flyway",
        [RoomHeaderPointers.MorphBallRoom] = "Morph Ball Room",
        [RoomHeaderPointers.ConstructionZone] = "Construction Zone",
        [RoomHeaderPointers.BlueBrinstarEnergyTankRoom] = "Blue Brinstar Energy Tank Room",
        [RoomHeaderPointers.CeresElevatorShaft] = "Ceres Elevator Shaft",
        [RoomHeaderPointers.CeresFallingTileRoom] = "Ceres Falling Tile Room",
        [RoomHeaderPointers.CeresMagnetStairs] = "Ceres Magnet Stairs",
        [RoomHeaderPointers.CeresDeadScientistRoom] = "Ceres Dead Scientist Room",
        [RoomHeaderPointers.CeresFinalHallway] = "Ceres Final Hallway",
        [RoomHeaderPointers.CeresRidleyRoom] = "Ceres Ridley Room",
    }.ToFrozenDictionary();

    /// <summary>Uses a known playthrough label when available and a generic room label otherwise.</summary>
    private static string GetKnownRoomName(ushort roomPointer) =>
        KnownRoomNames.TryGetValue(roomPointer, out string? name) ? name : "Room";

    /// <summary>Merges keyboard and gamepad into one filtered cartridge-format input word.</summary>
    private ushort BuildControllerWord()
    {
        // Keyboard and gamepad are two host producers for the same physical SNES port.
        // Merge them before recording so replay sees one exact cartridge-format word and
        // never depends on which Windows device generated a particular held bit.
        ushort input = keyboard.BuildControllerWord(gamepad.Poll());
        return inputActivation.Filter(input);
    }

    /// <summary>Moves the deactivation subscription to the current containing form so lost host focus releases gameplay input.</summary>
    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        AttachInputLifecycleForm(FindForm());
    }

    /// <summary>Moves the deactivation handler when the control changes containing forms.</summary>
    private void AttachInputLifecycleForm(Form? form)
    {
        if (ReferenceEquals(inputLifecycleForm, form))
            return;
        if (inputLifecycleForm is not null)
            inputLifecycleForm.Deactivate -= OnInputHostDeactivated;
        inputLifecycleForm = form;
        if (inputLifecycleForm is not null)
            inputLifecycleForm.Deactivate += OnInputHostDeactivated;
    }

    /// <summary>Releases held input when focus loss might suppress key-up or neutral gamepad samples.</summary>
    private void OnInputHostDeactivated(object? sender, EventArgs e)
    {
        // A deactivated WinForms/RDP window is not guaranteed to receive the matching
        // WM_KEYUP or a fresh joystick sample. Treat the focus boundary as a release and
        // require a genuinely neutral device sample before accepting controls again.
        HostInputDiscontinuity.Release(keyboard, inputActivation);
    }

    /// <summary>Queues an input release on the UI thread after lock or remote disconnect.</summary>
    private void OnWindowsSessionSwitch(object sender, SessionSwitchEventArgs e)
    {
        // Remote disconnect/lock can leave the WinForms window technically active, so its
        // Deactivate event is not a sufficient release boundary. SystemEvents may deliver
        // this callback off the UI thread; queue the state mutation onto the same thread
        // that polls and records controller words.
        if (!IsHandleCreated || IsDisposed)
            return;
        BeginInvoke(ReleaseInputAfterSessionSwitch);
    }

    /// <summary>Clears controller activation state on the frame-polling UI thread.</summary>
    private void ReleaseInputAfterSessionSwitch() =>
        HostInputDiscontinuity.Release(keyboard, inputActivation);

    /// <summary>Consumes gameplay key-down/up messages above all child controls, updating held keys without allowing dialog navigation.</summary>
    /// <returns>True for a handled gameplay key message; otherwise the framework's preview result.</returns>
    protected override bool ProcessKeyPreview(ref Message message)
    {
        // ProcessKeyPreview is called while WinForms walks a focused child's parent chain.
        // Consuming the message here means Enter is Start whether focus is on the canvas,
        // a ToolStrip button, or the state-slot ComboBox; it can never invoke dialog/UI
        // behavior that scrolls or relocates the host viewport.
        Keys key = unchecked((Keys)(long)message.WParam);
        if (keyboard.ApplyWindowMessage(message.Msg, key))
            return true;
        return base.ProcessKeyPreview(ref message);
    }

    /// <summary>Subscribes once to Windows session-switch input release and queues canvas focus after the native handle exists.</summary>
    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        if (!sessionSwitchAttached)
        {
            SystemEvents.SessionSwitch += OnWindowsSessionSwitch;
            sessionSwitchAttached = true;
        }

        // Constructor-time Focus() can run before a native handle exists. Queue the first
        // focus request after handle creation so F5 starts with controller input active.
        BeginInvoke(canvas.Focus);
    }

    /// <summary>Releases host subscriptions, playback timing, audio, and input recording before framework control disposal.</summary>
    /// <remarks>The GPU worker must already have completed through <see cref="StopRendererAsync"/>; disposal rejects an active worker rather than blocking the UI thread.</remarks>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeRendererHost();
            if (sessionSwitchAttached)
            {
                SystemEvents.SessionSwitch -= OnWindowsSessionSwitch;
                sessionSwitchAttached = false;
            }
            AttachInputLifecycleForm(null);
            playbackTimer.Dispose();
            playbackClock.Stop();
            audioDevice?.Dispose();
            audioDevice = null;
            inputRecorder?.Dispose();
            inputRecorder = null;
        }
        base.Dispose(disposing);
    }

    /// <summary>Monotonic identity assigned to each captured display publication.</summary>
    private long displaySequence;
    /// <summary>Invalidates older display packets across reset and state-load boundaries.</summary>
    private long displayGeneration;
    /// <summary>Captured scene awaiting GPU publication or software rasterization.</summary>
    internal RenderFrameSnapshot? pendingDisplay;
    /// <summary>Asynchronous Direct3D worker owning the canvas window when enabled.</summary>
    internal D3D11RenderWorker? gpuWorker;
    /// <summary>Prevents duplicate renderer initialization for this control.</summary>
    private bool rendererStarted;
    /// <summary>Stops new display publication during worker shutdown.</summary>
    internal bool rendererStopping;
    /// <summary>Containing form used to observe resize and minimization.</summary>
    private Form? rendererHost;
    /// <summary>Prior worker present count for computing displayed frames per second.</summary>
    private long previousPresentCount;
    /// <summary>Prior monotonic timestamp for renderer frame-rate sampling.</summary>
    private long previousPresentTimestamp;
    /// <summary>Rate of completed GPU presentations in the latest health interval.</summary>
    private double gpuFramesPerSecond;
    /// <summary>Detailed CPU and GPU timing distribution shown in status tooltip.</summary>
    private string rendererTimingDetail = string.Empty;

    /// <summary>Captures renderer timing distributions without blocking gameplay simulation.</summary>
    private void RefreshRendererTimingDetail()
    {
        if (gpuWorker is not { } worker) { rendererTimingDetail = string.Empty; return; }
        var timings = worker.CaptureTimings();
        static string Format(string name, RenderTimingDistribution value) =>
            $"{name}: p50/p95/p99 {value.P50Milliseconds:F3}/{value.P95Milliseconds:F3}/{value.P99Milliseconds:F3} ms; " +
            $"window {value.Retained}, warmup excluded {value.WarmupExcluded}, observed {value.Observed}";
        rendererTimingDetail = Environment.NewLine + string.Join(Environment.NewLine,
            Format("CPU composition submission", timings.CpuComposition),
            Format("CPU upload submission (within composition/display)", worker.CaptureUploadTimings()),
            $"Upload bytes {worker.SubmittedUploadBytes}, calls {worker.SubmittedUploadCalls}",
            Format("CPU display + Present (includes wait)", timings.CpuDisplayAndPresent),
            Format("GPU composition (excludes display/Present)", timings.GpuComposition),
            Format("GPU composition + display (excludes CPU Present wait)", timings.GpuCompositionAndDisplay),
            $"GPU samples skipped {worker.SkippedGpuTimingSamples}, invalid {worker.InvalidGpuTimingSamples}; recoveries {worker.DeviceRecoveries}");
    }
    /// <summary>UI timer polling worker failures and completed presentation count.</summary>
    private readonly System.Windows.Forms.Timer rendererHealthTimer = new() { Interval = 250 };

    /// <summary>Compact GPU rate and replaced-packet count for the toolbar.</summary>
    private string GpuTimingText => gpuWorker is { } worker
        ? $" | GPU {gpuFramesPerSecond:F1} fps / replaced {worker.MailboxMetrics.Replaced}"
        : string.Empty;

    /// <summary>Advances display identity synchronously before software-only restart.</summary>
    private void BeginDisplayGeneration()
    {
        displayGeneration = checked(displayGeneration + 1);
        gpuWorker?.AdvanceGeneration(displayGeneration);
        pendingDisplay = null;
    }

    /// <summary>Drains older GPU packets before publishing a restored or restarted session.</summary>
    private async Task<bool> BeginDisplayGenerationAsync()
    {
        long next = checked(displayGeneration + 1);
        if (gpuWorker is { } worker && !await worker.AdvanceGenerationAsync(next)) return false;
        if (rendererStopping || IsDisposed) return false;
        displayGeneration = next;
        pendingDisplay = null;
        return true;
    }

    /// <summary>Pauses playback, drains the old display generation, then restarts in place.</summary>
    internal async Task RestartAsync()
    {
        bool resume = playbackTimer.Enabled;
        SetPlaying(false);
        Enabled = false;
        try
        {
            if (!await BeginDisplayGenerationAsync()) return;
            if (rendererStopping || IsDisposed) return;
            RestartCore();
            SetPlaying(resume);
        }
        finally { if (!IsDisposed) Enabled = true; }
    }

    /// <summary>
    /// Starts the window-owned renderer with the UI message pump free to service DXGI.
    /// Call once after showing the form. Auto prefers hardware with logged startup fallback.
    /// </summary>
    public Task InitializeRendererAsync() => InitializeRendererAsync(static (window, clientSize, generation) =>
        new D3D11RenderWorker(window, clientSize, generation, D3D11DeviceKind.Hardware));

    /// <summary>Verification seam for a worker that genuinely fails asynchronous device/window startup.</summary>
    internal async Task InitializeRendererAsync(Func<nint, Extent16<DisplayPixels>, long, D3D11RenderWorker> createWorker)
    {
        Ensure.NotNull(createWorker);
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (rendererStopping) throw new InvalidOperationException("Renderer has already been stopped.");
        if (rendererStarted) throw new InvalidOperationException("Renderer already initialized.");
        rendererStarted = true;
        if (gameOptions.Renderer == RendererSelection.Software) return;
        var worker = createWorker(canvas.Handle,
            DisplayViewport.ClientSize(Math.Max(1, canvas.ClientSize.Width), Math.Max(1, canvas.ClientSize.Height)),
            displayGeneration);
        gpuWorker = worker;
        string adapter;
        try
        {
            adapter = await worker.Ready;
        }
        catch (Exception error)
        {
            // Drain failed creation before letting GDI reclaim this same window.
            try { await worker.StopAsync(); }
            catch (Exception stopError) { Console.Error.WriteLine(stopError); }
            gpuWorker = null;
            canvas.SetGpuOwned(false);
            if (gameOptions.Renderer != RendererSelection.Auto || rendererStopping) throw;
            Console.Error.WriteLine($"Direct3D11 startup failed; Auto selected software rendering.\n{error}");
            RefreshFrame(game.CurrentFrame);
            return;
        }
        if (rendererStopping) return;
        canvas.SetGpuOwned(true);
        canvas.SizeChanged += ResizeGpu;
        rendererHost = FindForm();
        if (rendererHost is not null) rendererHost.Resize += ResizeGpu;
        ResizeGpu(this, EventArgs.Empty);
        // A publication may already have occurred during asynchronous startup.
        // Fresh identity makes this safe without advancing a paused game. Coverage or
        // runtime failures are outside the Auto startup fallback boundary intentionally.
        pendingDisplay = game.GetRetainedDisplay(++displaySequence, displayGeneration);
        PublishGpuDisplay();
        rendererHealthTimer.Tick += CheckRendererHealth;
        previousPresentTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        rendererHealthTimer.Start();
        Console.WriteLine($"Renderer: Direct3D11 on {adapter}");
    }

    /// <summary>Publishes canvas size or zero extent when the containing form is minimized.</summary>
    private void ResizeGpu(object? sender, EventArgs e)
    {
        if (rendererStopping) return;
        // Minimization may retain the child canvas dimensions. The form's state,
        // not just its child SizeChanged event, owns presentation suspension.
        bool minimized = rendererHost?.WindowState == FormWindowState.Minimized;
        gpuWorker?.Resize(minimized ? Extent16<DisplayPixels>.Zero
            : DisplayViewport.ClientSize(canvas.ClientSize.Width, canvas.ClientSize.Height));
    }

    /// <summary>Surfaces worker faults and samples completed presents for GPU frame rate.</summary>
    private void CheckRendererHealth(object? sender, EventArgs e)
    {
        if (gpuWorker?.Completion.IsFaulted == true)
        {
            rendererHealthTimer.Stop();
            SetPlaying(false);
            gpuWorker.ThrowIfFaulted();
        }
        if (gpuWorker is { } worker)
        {
            long now = System.Diagnostics.Stopwatch.GetTimestamp();
            long count = worker.PresentedFrames;
            gpuFramesPerSecond = (count - previousPresentCount) * (double)System.Diagnostics.Stopwatch.Frequency
                / Math.Max(1, now - previousPresentTimestamp);
            previousPresentCount = count;
            previousPresentTimestamp = now;
        }
    }

    /// <summary>Submits the retained scene packet to the GPU worker for this generation.</summary>
    private void PublishGpuDisplay()
    {
        if (gpuWorker is not { } worker || rendererStopping) return;
        if (pendingDisplay is not { } packet)
            throw new NotSupportedException("Direct3D11 requires a captured scene; legacy raster fallback is not permitted.");
        worker.Publish(packet);
    }

    /// <summary>Renders a retained scene or copies frontend pixels into the software canvas.</summary>
    private void RefreshDisplay(FrontendFrame frame)
    {
        if (gpuWorker is not null) return;
        var pixels = pendingDisplay is { } packet
            ? SoftwareFrameSnapshotRenderer.Render(packet)
            : frame.Pixels;
        canvas.ReplaceFrame(new Extent16<ScreenPixels>(FrontendFrame.Width, FrontendFrame.Height), pixels);
    }

    /// <summary>Await before destroying the form/child HWND; never synchronously wait on the UI thread.</summary>
    public async Task StopRendererAsync()
    {
        rendererStopping = true;
        SetPlaying(false);
        rendererHealthTimer.Stop();
        canvas.SizeChanged -= ResizeGpu;
        if (rendererHost is not null) rendererHost.Resize -= ResizeGpu;
        rendererHost = null;
        if (gpuWorker is { } worker) await worker.StopAsync();
    }

    /// <summary>Disposes renderer UI resources after the worker has stopped.</summary>
    private void DisposeRendererHost()
    {
        if (gpuWorker is { Completion.IsCompleted: false })
            throw new InvalidOperationException("Await StopRendererAsync before destroying the GPU window.");
        rendererHealthTimer.Dispose();
    }
}
