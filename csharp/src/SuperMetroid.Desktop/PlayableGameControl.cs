using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using System.Diagnostics;

namespace SuperMetroid.Desktop;

/// <summary>
/// Thin keyboard/debugger host for the cartridge-backed top-level game dispatcher.
/// </summary>
public sealed class PlayableGameControl : UserControl
{
    private readonly string romPath;
    private readonly SuperMetroidGameOptions gameOptions;
    private readonly RuntimeCanvas canvas = new() { Dock = DockStyle.Fill, TabStop = true };
    private readonly ToolStripLabel statusLabel = new();
    private readonly System.Windows.Forms.Timer playbackTimer = new() { Interval = 8 };
    private readonly Stopwatch playbackClock = new();
    private readonly HashSet<Keys> heldKeys = [];
    private double pendingPlaybackFrames;
    private SuperMetroidGame game = null!;

    // The host's wall clock is intentionally separate from the translated frame counter.
    // A WinForms timer has millisecond granularity and does not promise an exact callback
    // cadence; accumulating elapsed time avoids the old integer `1000 / 60 = 16ms` loop,
    // which actually advanced the game at 62.5 frames per second.
    private const double TargetFramesPerSecond = 60.0;
    private const int MaximumCatchUpFrames = 4;

    public PlayableGameControl(string romPath, SuperMetroidGameOptions gameOptions)
    {
        this.romPath = romPath;
        this.gameOptions = gameOptions ?? throw new ArgumentNullException(nameof(gameOptions));
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
            Height = 40,
            TextAlign = ContentAlignment.MiddleCenter,
            Text = "Arrows: move/aim  |  Space or X: jump (SNES A)  |  Z: dash (B)  |  " +
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
        // Restart must retain host configuration. Re-reading the INI here would make an
        // ordinary in-window reset depend on a mid-session disk edit and would obscure the
        // exact options with which the debugger-visible session was constructed.
        game = new SuperMetroidGame(
            SuperMetroidAddressSpace.LoadRetailRom(romPath),
            gameOptions);
        // Execute reset once so the first visible debugger frame is state one's native setup.
        RefreshFrame(game.Step(0));
        canvas.Focus();
    }

    private void StepFrame(ushort? forcedInput = null)
    {
        ushort input = forcedInput ?? BuildControllerWord();
        RefreshFrame(game.Step(input));
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

        ushort input = BuildControllerWord();
        FrontendFrame frame = default;
        for (int frameIndex = 0; frameIndex < framesToRun; frameIndex++)
            frame = game.Step(input);

        pendingPlaybackFrames -= framesToRun;
        RefreshFrame(frame);
    }

    /// <summary>Starts or pauses wall-clock playback without changing translated state.</summary>
    private void SetPlaying(bool playing)
    {
        pendingPlaybackFrames = 0;
        playbackClock.Restart();
        playbackTimer.Enabled = playing;
    }

    private void RefreshFrame(FrontendFrame frame)
    {
        canvas.ReplaceFrame(RgbaBitmap.Create(FrontendFrame.Width, FrontendFrame.Height, frame.Pixels));
        statusLabel.Text =
            $"state ${((ushort)frame.GameState):X2} {frame.GameState}  |  {frame.Phase}  |  frame {frame.FrameNumber}";
    }

    private ushort BuildControllerWord()
    {
        SnesButton input = SnesButton.None;
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
        }
        base.Dispose(disposing);
    }
}
