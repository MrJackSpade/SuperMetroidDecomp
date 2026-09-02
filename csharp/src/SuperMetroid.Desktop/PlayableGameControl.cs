using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using System.Diagnostics;
using System.Security.Cryptography;

namespace SuperMetroid.Desktop;

/// <summary>
/// Thin keyboard/gamepad/debugger host for the cartridge-backed top-level game dispatcher.
/// </summary>
public sealed class PlayableGameControl : UserControl
{
    private readonly string romPath;
    private readonly string saveRamPath;
    private readonly SuperMetroidGameOptions gameOptions;
    private readonly ControllerInputRecording? replay;
    private readonly RuntimeCanvas canvas = new() { Dock = DockStyle.Fill, TabStop = true };
    private readonly ToolStripLabel statusLabel = new();
    private readonly System.Windows.Forms.Timer playbackTimer = new() { Interval = 8 };
    private readonly Stopwatch playbackClock = new();
    private readonly HashSet<Keys> heldKeys = [];
    private readonly WindowsGamepadInput gamepad = new();
    private double pendingPlaybackFrames;
    private SuperMetroidAddressSpace addressSpace = null!;
    private SuperMetroidGame game = null!;
    private SpcAudioEngine? audioEngine;
    private WaveOutAudioDevice? audioDevice;
    private ControllerInputRecorder? inputRecorder;
    private int replayFrameIndex;
    private ushort? displayedRoomPointer;

    // The host's wall clock is intentionally separate from the translated frame counter.
    // A WinForms timer has millisecond granularity and does not promise an exact callback
    // cadence; accumulating elapsed time avoids the old integer `1000 / 60 = 16ms` loop,
    // which actually advanced the game at 62.5 frames per second.
    private const double TargetFramesPerSecond = 60.0;
    private const int MaximumCatchUpFrames = 4;

    public PlayableGameControl(
        string romPath,
        SuperMetroidGameOptions gameOptions,
        ControllerInputRecording? replay = null)
    {
        this.romPath = romPath;
        saveRamPath = Path.ChangeExtension(Path.GetFullPath(romPath), ".srm");
        this.gameOptions = gameOptions ?? throw new ArgumentNullException(nameof(gameOptions));
        this.replay = replay;
        Dock = DockStyle.Fill;

        var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        var restartButton = new ToolStripButton("Restart");
        var playButton = new ToolStripButton("Pause") { CheckOnClick = true, Checked = true };
        var stepButton = new ToolStripButton("Step");
        var skipButton = new ToolStripButton("Press Start");
        toolStrip.Items.Add(restartButton);
        toolStrip.Items.Add(playButton);
        toolStrip.Items.Add(stepButton);
        toolStrip.Items.Add(skipButton);
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
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolStrip, 0, 0);
        layout.Controls.Add(canvas, 0, 1);
        Controls.Add(layout);
        Controls.Add(help);

        restartButton.Click += (_, _) => Restart();
        playButton.CheckedChanged += (_, _) =>
        {
            playButton.Text = playButton.Checked ? "Pause" : "Play";
            SetPlaying(playButton.Checked);
            canvas.Focus();
        };
        stepButton.Click += (_, _) => { StepFrame(); canvas.Focus(); };
        skipButton.Click += (_, _) => { StepFrame((ushort)SnesButton.Start); StepFrame(); canvas.Focus(); };
        playbackTimer.Tick += (_, _) => AdvancePlaybackClock();

        // Track held keys on the canvas rather than creating gameplay buttons. The raw word
        // goes straight into ControllerInputState, so newly-pressed and held semantics remain
        // part of the translated core and are easy to inspect at a breakpoint.
        canvas.KeyDown += (_, eventArguments) =>
        {
            heldKeys.Add(eventArguments.KeyCode);
            bool gameplayKey = IsGameplayKey(eventArguments.KeyCode);
            eventArguments.Handled = gameplayKey;
            eventArguments.SuppressKeyPress = gameplayKey;
        };
        canvas.KeyUp += (_, eventArguments) =>
        {
            heldKeys.Remove(eventArguments.KeyCode);
            bool gameplayKey = IsGameplayKey(eventArguments.KeyCode);
            eventArguments.Handled = gameplayKey;
            eventArguments.SuppressKeyPress = gameplayKey;
        };
        // Windows cannot deliver KeyUp to a control after Alt-Tab, a toolbar click, or a
        // debugger focus change. Clearing the host set prevents a direction/button from
        // remaining latched indefinitely when focus later returns to the canvas.
        canvas.LostFocus += (_, _) => heldKeys.Clear();

        Restart();
        SetPlaying(playing: true);
    }

    private void Restart()
    {
        heldKeys.Clear();
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
        if (gameOptions.AudioEnabled)
        {
            audioEngine = new SpcAudioEngine(addressSpace);
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
        if (replay is null)
        {
            game.SaveRamChanged += PersistSaveRamToDisk;
            // The reset seed is captured after disk SRAM has been validated/copied but
            // before the first dispatcher call. A crash in state zero is therefore just as
            // reproducible as a gameplay failure hours later.
            inputRecorder = ControllerInputRecorder.Start(
                romPath,
                addressSpace.SaveRam,
                gameOptions);
            Console.WriteLine($"Recording controller input to {inputRecorder.Path}");
        }
        // Execute reset once so the first visible debugger frame is state one's native setup.
        FrontendFrame resetFrame = AdvanceOneFrame(forcedInput: replay is null ? (ushort)0 : null)
            ?? throw new InvalidDataException("Replay contains no reset frame.");
        RefreshFrame(resetFrame);
        canvas.Focus();
    }

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
        if (!File.Exists(saveRamPath))
            return;

        byte[] bytes = File.ReadAllBytes(saveRamPath);
        if (bytes.Length != SuperMetroidAddressSpace.SaveRamByteCount)
        {
            throw new InvalidDataException(
                $"Save RAM '{saveRamPath}' contains {bytes.Length} bytes; " +
                $"Super Metroid requires exactly {SuperMetroidAddressSpace.SaveRamByteCount} bytes.");
        }
        bytes.CopyTo(addressSpace.SaveRam);
    }

    /// <summary>
    /// Flushes translated SRAM mutations synchronously. The cartridge call is infrequent,
    /// and completing the write before the next frame preserves the save even if the host is
    /// stopped at a breakpoint or closed immediately after the Ceres checkpoint.
    /// </summary>
    private void PersistSaveRamToDisk() =>
        File.WriteAllBytes(saveRamPath, addressSpace.SaveRam.ToArray());

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
            FrontendFrame frame = game.Step(input);

            // Apply every bank upload/port write before generating this NMI's samples.
            // Acknowledgements are fed back for bank $82's next-frame SFX handshake; they
            // never influence controller recording or gameplay state.
            if (audioEngine is not null && audioDevice is not null)
            {
                ReadOnlySpan<short> samples = audioEngine.RenderFrame(frame.AudioCommands);
                audioDevice.Submit(samples);
                game.SetAudioAcknowledgements(audioEngine.ReadAcknowledgements());
            }
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
                throw new AggregateException(
                    "The emulated frame and its emergency input-recording flush both failed.",
                    frameException,
                    recorderException);
            }
            throw;
        }
    }

    /// <summary>
    /// Advances the translated dispatcher according to measured wall-clock time.
    /// </summary>
    private void AdvancePlaybackClock()
    {
        double elapsedSeconds = playbackClock.Elapsed.TotalSeconds;
        playbackClock.Restart();

        // A breakpoint, window drag, or suspended workstation must not cause hundreds of
        // invisible catch-up frames on resume. Four frames retain modest scheduler jitter
        // without making a debug pause alter several seconds of game state.
        elapsedSeconds = Math.Min(
            elapsedSeconds,
            MaximumCatchUpFrames / TargetFramesPerSecond);
        pendingPlaybackFrames += elapsedSeconds * TargetFramesPerSecond;

        int framesToRun = Math.Min((int)pendingPlaybackFrames, MaximumCatchUpFrames);
        if (framesToRun == 0)
            return;

        FrontendFrame frame = default;
        int framesActuallyRun = 0;
        ushort liveInput = BuildControllerWord();
        for (int frameIndex = 0; frameIndex < framesToRun; frameIndex++)
        {
            FrontendFrame? next = AdvanceOneFrame(liveInput);
            if (next is null)
                break;
            frame = next.Value;
            framesActuallyRun++;
        }

        pendingPlaybackFrames -= framesActuallyRun;
        if (framesActuallyRun != 0)
            RefreshFrame(frame);
    }

    /// <summary>Starts or pauses wall-clock playback without changing translated state.</summary>
    private void SetPlaying(bool playing)
    {
        pendingPlaybackFrames = 0;
        playbackClock.Restart();
        playbackTimer.Enabled = playing;
        if (!playing && audioDevice is not null)
            audioDevice.Reset();
    }

    private void RefreshFrame(FrontendFrame frame)
    {
        canvas.ReplaceFrame(RgbaBitmap.Create(FrontendFrame.Width, FrontendFrame.Height, frame.Pixels));
        RefreshRoomIdentity();
        statusLabel.Text =
            $"state ${((ushort)frame.GameState):X2} {frame.GameState}  |  {frame.Phase}  |  frame {frame.FrameNumber}" +
            (gamepad.DeviceName is null ? string.Empty : $"  |  pad: {gamepad.DeviceName}") +
            (replay is null
                ? string.Empty
                : $"  |  replay {replayFrameIndex}/{replay.ControllerInputs.Length}");
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
        byte area = game.GameplayActiveAreaIndex
            ?? throw new InvalidOperationException($"Room $8F:{pointer:X4} has no area index.");
        byte room = game.GameplayActiveRoomIndex
            ?? throw new InvalidOperationException($"Room $8F:{pointer:X4} has no room index.");
        host.Text =
            $"Super Metroid C# — {name} [$8F:{pointer:X4}, state $8F:{state:X4}, area ${area:X2}/room ${room:X2}]";
    }

    /// <summary>
    /// Names the practical Ceres-to-Bombs playthrough slice. Unknown rooms remain fully
    /// reportable by address in the caption, so extending this table never gates gameplay.
    /// </summary>
    private static string GetKnownRoomName(ushort roomPointer) => roomPointer switch
    {
        0x91f8 => "Landing Site",
        0x92fd => "Parlor and Alcatraz",
        0x96ba => "Climb",
        0x975c => "Pit Room",
        0x9804 => "Bomb Torizo Room",
        0x9879 => "Flyway",
        0x9e9f => "Morph Ball Room",
        0x9f11 => "Construction Zone",
        0x9f64 => "Blue Brinstar Energy Tank Room",
        0xdf45 => "Ceres Elevator Shaft",
        0xdf8d => "Ceres Falling Tile Room",
        0xdfd7 => "Ceres Magnet Stairs",
        0xe021 => "Ceres Dead Scientist Room",
        0xe06b => "Ceres Final Hallway",
        0xe0b5 => "Ceres Ridley Room",
        _ => "Room",
    };

    private ushort BuildControllerWord()
    {
        // Keyboard and gamepad are two host producers for the same physical SNES port.
        // Merge them before recording so replay sees one exact cartridge-format word and
        // never depends on which Windows device generated a particular held bit.
        SnesButton input = gamepad.Poll();
        if (heldKeys.Contains(Keys.Left)) input |= SnesButton.Left;
        if (heldKeys.Contains(Keys.Right)) input |= SnesButton.Right;
        if (heldKeys.Contains(Keys.Up)) input |= SnesButton.Up;
        if (heldKeys.Contains(Keys.Down)) input |= SnesButton.Down;
        if (heldKeys.Contains(Keys.Z)) input |= SnesButton.B;
        // Space is a discoverable desktop jump alias; X retains the compact four-face-
        // button layout printed below the viewport. Both become the same retail SNES A bit,
        // so menus and gameplay still observe one authentic controller word.
        if (heldKeys.Contains(Keys.X) || heldKeys.Contains(Keys.Space)) input |= SnesButton.A;
        if (heldKeys.Contains(Keys.A)) input |= SnesButton.Y;
        if (heldKeys.Contains(Keys.S)) input |= SnesButton.X;
        if (heldKeys.Contains(Keys.Q)) input |= SnesButton.L;
        if (heldKeys.Contains(Keys.W)) input |= SnesButton.R;
        if (heldKeys.Contains(Keys.Enter)) input |= SnesButton.Start;
        if (heldKeys.Contains(Keys.ShiftKey)) input |= SnesButton.Select;
        return (ushort)input;
    }

    private static bool IsGameplayKey(Keys key) => key is
        Keys.Left or Keys.Right or Keys.Up or Keys.Down or
        Keys.Z or Keys.X or Keys.Space or Keys.A or Keys.S or Keys.Q or Keys.W or
        Keys.Enter or Keys.ShiftKey;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        // Constructor-time Focus() can run before a native handle exists. Queue the first
        // focus request after handle creation so F5 starts with controller input active.
        BeginInvoke(canvas.Focus);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
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
