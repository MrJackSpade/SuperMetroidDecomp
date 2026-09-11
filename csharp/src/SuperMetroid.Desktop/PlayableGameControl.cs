using SuperMetroid.Core.Game;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace SuperMetroid.Desktop;

/// <summary>
/// Thin keyboard/gamepad/debugger host for the cartridge-backed top-level game dispatcher.
/// </summary>
public sealed partial class PlayableGameControl : UserControl
{
    private readonly string romPath;
    private readonly string saveFilePath;
    private readonly string legacySaveRamPath;
    private readonly SuperMetroidGameOptions gameOptions;
    private readonly ControllerInputRecording? replay;
    private readonly GitHubErrorReporter? errorReporter;
    private readonly RuntimeCanvas canvas = new() { Dock = DockStyle.Fill, TabStop = true };
    private readonly ToolStripLabel statusLabel = HostToolbarLayout.CreateStatusLabel();
    private readonly System.Windows.Forms.Timer playbackTimer = new() { Interval = 8 };
    private readonly Stopwatch playbackClock = new();
    private readonly FrameTimingCounter frameTimings = new();
    private readonly HostKeyboardInputState keyboard = new();
    private readonly HostInputActivationGate inputActivation = new();
    private readonly WindowsGamepadInput gamepad = new();
    private double pendingPlaybackFrames;
    private SuperMetroidAddressSpace addressSpace = null!;
    private SuperMetroidGame game = null!;
    private SpcAudioEngine? audioEngine;
    private readonly string? installedAudioDirectory;
    private readonly string? playerDataDirectory;
    private WaveOutAudioDevice? audioDevice;
    private ControllerInputRecorder? inputRecorder;
    private DebuggerSaveStateStore stateStore = null!;
    private int replayFrameIndex;
    private ushort? displayedRoomPointer;
    private FrameTimingSnapshot? latestFrameTiming;
    private string? lastRecoverableError;
    private Form? inputLifecycleForm;
    private bool sessionSwitchAttached;

    // The host's wall clock is intentionally separate from the translated frame counter.
    // A WinForms timer has millisecond granularity and does not promise an exact callback
    // cadence; accumulating elapsed time avoids the old integer `1000 / 60 = 16ms` loop,
    // which actually advanced the game at 62.5 frames per second.
    private const double TargetFramesPerSecond = 60.0;
    private const int MaximumCatchUpFrames = 4;

    public PlayableGameControl(
        string romPath,
        SuperMetroidGameOptions gameOptions,
        ControllerInputRecording? replay = null,
        GitHubErrorReporter? errorReporter = null,
        string? audioDirectory = null,
        string? dataDirectory = null)
    {
        this.romPath = romPath;
        installedAudioDirectory = audioDirectory;
        playerDataDirectory = dataDirectory is null ? null : Path.GetFullPath(dataDirectory);
        string fullRomPath = Path.GetFullPath(romPath);
        string saveBase = playerDataDirectory is null ? fullRomPath : Path.Combine(playerDataDirectory, Path.GetFileName(fullRomPath));
        saveFilePath = Path.ChangeExtension(saveBase, GameSaveJsonFormat.FileExtension);
        legacySaveRamPath = Path.ChangeExtension(saveBase, ".srm");
        this.gameOptions = gameOptions ?? throw new ArgumentNullException(nameof(gameOptions));
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
            Width = 48,
            DropDownStyle = ComboBoxStyle.DropDownList,
            ToolTipText = "Debugger save-state slot (0-9)",
        };
        for (int slot = 0; slot < 10; slot++)
            stateSlot.Items.Add(slot.ToString(System.Globalization.CultureInfo.InvariantCulture));
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
                   "Q: aim up (L)  |  W: aim down (R)  |  Enter: Start  |  Shift: Select",
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

    private void Restart()
    {
        if (gpuWorker is not null) throw new InvalidOperationException("Live restart must use the asynchronous generation boundary.");
        BeginDisplayGeneration();
        RestartCore();
    }

    private void RestartCore()
    {
        keyboard.Clear();
        // waveOut may still own several queued buffers when Restart is clicked. Dispose the
        // device before its pinned storage and dispose the SPC player before replacing the
        // ROM whose upload addresses it consumes.
        audioDevice?.Dispose();
        audioDevice = null;
        audioEngine?.Dispose();
        audioEngine = null;
        inputRecorder?.Dispose();
        inputRecorder = null;
        replayFrameIndex = 0;
        // Restart must retain host configuration. Re-reading the INI here would make an
        // ordinary in-window reset depend on a mid-session disk edit and would obscure the
        // exact options with which the debugger-visible session was constructed.
        addressSpace = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        stateStore = new DebuggerSaveStateStore(romPath, addressSpace.Rom,
            playerDataDirectory is null ? null : Path.Combine(playerDataDirectory, "debug-states"));
        if (gameOptions.AudioEnabled)
        {
            audioEngine = new SpcAudioEngine(installedAudioDirectory);
            audioDevice = new WaveOutAudioDevice(
                SpcAudioEngine.SampleRate,
                SpcAudioEngine.ChannelCount,
                SpcAudioEngine.StereoFramesPerVideoFrame * SpcAudioEngine.ChannelCount,
                gameOptions.MasterVolumePercent);
        }
        if (replay is null)
            LoadSaveRamFromDisk();
        else
            LoadReplaySaveRam();
        game = new SuperMetroidGame(addressSpace, gameOptions);
        mapPresentation = playerDataDirectory is null ? null :
            new SuperMetroid.AssetExtraction.GameInstallation(playerDataDirectory).LoadMaps();
        game.BindMapPresentation(mapPresentation);
        if (replay is null)
        {
            game.SaveRamChanged += PersistSaveRamToDisk;
            // The reset seed is captured after disk SRAM has been validated/copied but
            // before the first dispatcher call. A crash in state zero is therefore just as
            // reproducible as a gameplay failure hours later.
            inputRecorder = ControllerInputRecorder.Start(
                romPath,
                addressSpace.SaveRam,
                gameOptions,
                playerDataDirectory is null ? null : Path.Combine(playerDataDirectory, "input-recordings"));
            Console.WriteLine($"Recording controller input to {inputRecorder.Path}");
        }
        // Execute reset once so the first visible debugger frame is state one's native setup.
        FrontendFrame resetFrame = AdvanceOneFrame(forcedInput: replay is null ? (ushort)0 : null)
            ?? throw new InvalidDataException("Replay contains no reset frame.");
        RefreshFrame(resetFrame);
        ResetFrameTimings();
        canvas.Focus();
    }

    private void SaveDebuggerState(int slot)
    {
        if (replay is not null)
            throw new InvalidOperationException("Debugger states are disabled during an input replay.");
        DebuggerSaveStateMetadata metadata = stateStore.Save(
            slot, addressSpace, game, audioEngine?.Player);
        statusLabel.Text =
            $"saved state {slot} | frame {metadata.FrameNumber} | " +
            FormatStateRoom(metadata.RoomPointer, metadata.RoomStatePointer);
        Console.WriteLine($"Saved debugger state slot {slot}: {metadata.Path}");
    }

    private async Task LoadDebuggerState(int slot)
    {
        if (replay is not null)
            throw new InvalidOperationException("Debugger states are disabled during an input replay.");

        // Probe the slot before stopping playback or disposing the current recorder/audio
        // graph. An empty slot is a normal ten-slot UI state, not a runtime failure, and the
        // live game must remain fully usable after the informational message is dismissed.
        if (!stateStore.TryLoad(slot, out DebuggerSaveStateLoadResult loaded))
        {
            statusLabel.Text = $"state slot {slot} is empty";
            MessageBox.Show(
                this,
                $"Debugger save-state slot {slot} is empty.",
                "Load State",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            canvas.Focus();
            return;
        }

        if (gpuWorker is not null && loaded.Game.GetRetainedDisplay(displaySequence + 1, displayGeneration + 1) is null)
            throw new NotSupportedException("This debugger state contains legacy pixels, not a captured scene. Load it with Renderer=Software.");

        bool resumePlayback = playbackTimer.Enabled;
        SetPlaying(playing: false);
        keyboard.Clear();
        inputRecorder?.Dispose();
        inputRecorder = null;
        audioDevice?.Dispose();
        audioDevice = null;
        audioEngine?.Dispose();
        audioEngine = null;

        // Disable frame-affecting controls while the old presentation drains.
        // Await keeps the UI pump alive; restore only after old GPU work loses
        // authority to present. No new simulation tick runs across this boundary.
        Enabled = false;
        try { if (!await BeginDisplayGenerationAsync()) return; }
        finally { if (!IsDisposed) Enabled = true; }
        if (rendererStopping || IsDisposed) return;
        addressSpace = loaded.AddressSpace;
        game = loaded.Game;
        game.BindMapPresentation(mapPresentation);
        pendingDisplay = game.GetRetainedDisplay(++displaySequence, displayGeneration);
        game.SaveRamChanged += PersistSaveRamToDisk;
        displayedRoomPointer = null;
        pendingPlaybackFrames = 0;

        if (gameOptions.AudioEnabled)
        {
            ManagedSpcPlayer restoredAudio = loaded.AudioPlayer
                ?? throw new InvalidDataException(
                    "Audio-enabled debugger state does not contain managed SPC state.");
            audioEngine = new SpcAudioEngine(
                ExtractedAudioAssetCatalog.Load(installedAudioDirectory ?? ExtractedAudioAssetLocator.FindAudioDirectory()),
                restoredAudio);
            audioDevice = new WaveOutAudioDevice(
                SpcAudioEngine.SampleRate,
                SpcAudioEngine.ChannelCount,
                SpcAudioEngine.StereoFramesPerVideoFrame * SpcAudioEngine.ChannelCount,
                gameOptions.MasterVolumePercent);
        }

        // Begin a new crash recorder at the restored boundary. The debugger state itself
        // is the deterministic seed for this continuation and is printed beside the new
        // recording path so both files can be attached to a report.
        inputRecorder = ControllerInputRecorder.Start(
            romPath,
            addressSpace.SaveRam,
            gameOptions,
            playerDataDirectory is null ? null : Path.Combine(playerDataDirectory, "input-recordings"));
        Console.WriteLine(
            $"Loaded debugger state slot {slot}: {loaded.Metadata.Path}{Environment.NewLine}" +
            $"Recording post-state controller input to {inputRecorder.Path}");

        PublishGpuDisplay();
        // The retained packet already owns the display. Reading CurrentFrame here
        // would rasterize it solely to populate status labels, even in GPU mode.
        RefreshFrame(pendingDisplay is not null ? game.CurrentFrameMetadata : game.CurrentFrame);
        statusLabel.Text =
            (loaded.Warnings.Count != 0 ? "WARNING: state from another build | " : "") +
            $"loaded state {slot} | frame {loaded.Metadata.FrameNumber} | " +
            FormatStateRoom(loaded.Metadata.RoomPointer, loaded.Metadata.RoomStatePointer);
        SetPlaying(resumePlayback);
    }

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

        byte[] actualDigest;
        using (FileStream rom = File.OpenRead(romPath))
            actualDigest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(actualDigest, replay.RomSha256))
        {
            throw new InvalidDataException(
                "The replay was recorded from a different ROM image (SHA-256 mismatch).");
        }
        replay.InitialSaveRam.CopyTo(addressSpace.SaveRam);
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
            legacySaveRamPath);
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
        GameSaveFileStore.WriteAtomic(addressSpace, saveFilePath);

    private void StepFrame(ushort? forcedInput = null)
    {
        FrontendFrame? frame = AdvanceOneFrame(forcedInput);
        if (frame is not null)
            RefreshFrame(frame.Value);
    }

    /// <summary>
    /// Selects exactly one live or replay word, records it before execution, and advances
    /// the translated dispatcher. Null means an exhausted replay and never means input zero.
    /// </summary>
    private FrontendFrame? AdvanceOneFrame(ushort? forcedInput = null)
    {
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
        try
        {
            long frameStarted = Stopwatch.GetTimestamp();
            var captured = game.StepCaptured(input, ++displaySequence, displayGeneration);
            pendingDisplay = captured.Snapshot;
            FrontendFrame frame = captured.Frame;

            // Apply every bank upload/port write before generating this NMI's samples.
            // Acknowledgements are fed back for bank $82's next-frame SFX handshake; they
            // never influence controller recording or gameplay state.
            if (audioEngine is not null && audioDevice is not null)
            {
                ReadOnlySpan<short> samples = audioEngine.RenderFrame(frame.AudioCommands);
                audioDevice.Submit(samples);
                game.SetAudioAcknowledgements(audioEngine.ReadAcknowledgements());
            }
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
                    InputRecordingPath: inputRecorder?.Path));
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
            framesToRun, BuildControllerWord, input => AdvanceOneFrame(input));
        pendingPlaybackFrames -= batch.CompletedFrames;
        if (batch.LastFrame is { } frame)
            RefreshFrame(frame);
    }

    /// <summary>Starts or pauses wall-clock playback without changing translated state.</summary>
    private void SetPlaying(bool playing)
    {
        pendingPlaybackFrames = 0;
        playbackClock.Restart();
        ResetFrameTimings();
        playbackTimer.Enabled = playing;
        if (!playing && audioDevice is not null)
            audioDevice.Reset();
    }

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
            $"Super Metroid C# — {name} [$8F:{pointer:X4}, state $8F:{state:X4}, " +
            $"room {identity}]";
    }

    /// <summary>
    /// Names the practical Ceres-to-Bombs playthrough slice. Unknown rooms remain fully
    /// reportable by address in the caption, so extending this table never gates gameplay.
    /// </summary>
    private static string GetKnownRoomName(ushort roomPointer) => roomPointer switch
    {
        RoomHeaderPointers.LandingSite => "Landing Site",
        RoomHeaderPointers.ParlorAndAlcatraz => "Parlor and Alcatraz",
        RoomHeaderPointers.Climb => "Climb",
        RoomHeaderPointers.PitRoom => "Pit Room",
        RoomHeaderPointers.BombTorizoRoom => "Bomb Torizo Room",
        RoomHeaderPointers.Flyway => "Flyway",
        RoomHeaderPointers.MorphBallRoom => "Morph Ball Room",
        RoomHeaderPointers.ConstructionZone => "Construction Zone",
        RoomHeaderPointers.BlueBrinstarEnergyTankRoom => "Blue Brinstar Energy Tank Room",
        RoomHeaderPointers.CeresElevatorShaft => "Ceres Elevator Shaft",
        RoomHeaderPointers.CeresFallingTileRoom => "Ceres Falling Tile Room",
        RoomHeaderPointers.CeresMagnetStairs => "Ceres Magnet Stairs",
        RoomHeaderPointers.CeresDeadScientistRoom => "Ceres Dead Scientist Room",
        RoomHeaderPointers.CeresFinalHallway => "Ceres Final Hallway",
        RoomHeaderPointers.CeresRidleyRoom => "Ceres Ridley Room",
        _ => "Room",
    };

    private ushort BuildControllerWord()
    {
        // Keyboard and gamepad are two host producers for the same physical SNES port.
        // Merge them before recording so replay sees one exact cartridge-format word and
        // never depends on which Windows device generated a particular held bit.
        ushort input = keyboard.BuildControllerWord(gamepad.Poll());
        return inputActivation.Filter(input);
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        AttachInputLifecycleForm(FindForm());
    }

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

    private void OnInputHostDeactivated(object? sender, EventArgs e)
    {
        // A deactivated WinForms/RDP window is not guaranteed to receive the matching
        // WM_KEYUP or a fresh joystick sample. Treat the focus boundary as a release and
        // require a genuinely neutral device sample before accepting controls again.
        HostInputDiscontinuity.Release(keyboard, inputActivation);
    }

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

    private void ReleaseInputAfterSessionSwitch() =>
        HostInputDiscontinuity.Release(keyboard, inputActivation);

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
            audioEngine?.Dispose();
            audioEngine = null;
            inputRecorder?.Dispose();
            inputRecorder = null;
        }
        base.Dispose(disposing);
    }
}
