using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.RoomViewer;

/// <summary>
/// Interactive host for frame-stepping the currently translated C# runtime against the
/// user's private cartridge image.
/// </summary>
internal sealed class RuntimePreviewControl : UserControl
{
    private const byte GameplayObsel = 0x03;
    private const int FrameWidth = 256;
    private const int FrameHeight = 224;

    private readonly SuperMetroidAddressSpace bus;
    private readonly RenderedRoom room;
    private ScrollBoundaryCamera camera = null!;
    private readonly RuntimeCanvas canvas = new() { Dock = DockStyle.Fill };
    private readonly ToolStripLabel statusLabel = new();
    private readonly ToolStripButton playButton = new("Play");
    private readonly ToolStripButton holdLeftButton = new("Hold Left");
    private readonly ToolStripButton holdRightButton = new("Hold Right");
    private readonly ToolStripButton holdJumpButton = new("Hold Jump");
    private readonly ToolStripButton livePpuLayersButton = new("Live PPU layers");
    private readonly System.Windows.Forms.Timer playbackTimer = new() { Interval = 16 };
    private SuperMetroidRuntime runtime = null!;
    private bool motherBrainScenario;
    private bool groundedRunScenario;
    private string? haltedAtUntranslatedBoundary;

    public RuntimePreviewControl(string romPath, RenderedRoom room)
    {
        Dock = DockStyle.Fill;
        bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        this.room = room ?? throw new ArgumentNullException(nameof(room));

        var toolStrip = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        var stepButton = new ToolStripButton("Step frame");
        var stepSixtyButton = new ToolStripButton("Step 60");
        var ceresButton = new ToolStripButton("Restart Ceres");
        var motherBrainButton = new ToolStripButton("Restart Mother Brain");
        var groundedRunButton = new ToolStripButton("Restart grounded run");
        var cameraLeftButton = new ToolStripButton("Room ←");
        var cameraRightButton = new ToolStripButton("Room →");
        var cameraUpButton = new ToolStripButton("Room ↑");
        var cameraDownButton = new ToolStripButton("Room ↓");

        stepButton.Click += (_, _) => StepFrames(1);
        stepSixtyButton.Click += (_, _) => StepFrames(60);
        ceresButton.Click += (_, _) => Restart(motherBrain: false, groundedRun: false);
        motherBrainButton.Click += (_, _) => Restart(motherBrain: true, groundedRun: false);
        groundedRunButton.Click += (_, _) => Restart(motherBrain: false, groundedRun: true);
        cameraLeftButton.Click += (_, _) => MoveCamera(-16, 0);
        cameraRightButton.Click += (_, _) => MoveCamera(16, 0);
        cameraUpButton.Click += (_, _) => MoveCamera(0, -16);
        cameraDownButton.Click += (_, _) => MoveCamera(0, 16);
        playButton.CheckOnClick = true;
        playButton.Click += (_, _) =>
        {
            playbackTimer.Enabled = playButton.Checked;
            playButton.Text = playButton.Checked ? "Pause" : "Play";
        };
        holdRightButton.CheckOnClick = true;
        holdRightButton.ToolTipText =
            "Feeds the SNES Right bit to each stepped frame. It only moves Samus in the explicit grounded scenario.";
        holdLeftButton.CheckOnClick = true;
        holdLeftButton.ToolTipText =
            "Feeds the SNES Left bit to each stepped frame. Reversals use the ROM's turn poses and preserve old momentum.";
        holdJumpButton.CheckOnClick = true;
        holdJumpButton.ToolTipText =
            "Feeds canonical jump bit $0080. Tap it for a short jump or leave it held for the native variable-height arc.";

        // The SNES can electrically report both direction bits, but an ordinary D-pad
        // cannot be held left and right at once. Keep this convenience UI physically sane;
        // the underlying controller latch still accepts arbitrary 16-bit test chords.
        holdLeftButton.CheckedChanged += (_, _) =>
        {
            if (holdLeftButton.Checked)
                holdRightButton.Checked = false;
        };
        holdRightButton.CheckedChanged += (_, _) =>
        {
            if (holdRightButton.Checked)
                holdLeftButton.Checked = false;
        };
        livePpuLayersButton.CheckOnClick = true;
        livePpuLayersButton.ToolTipText =
            "Checked: show translated live BG1/BG2 VRAM. Unchecked: show the complete ROM-derived static terrain behind live HUD/OBJ.";
        livePpuLayersButton.CheckedChanged += (_, _) => RefreshFrame();

        // One host timer tick represents one accepted NMI. It is a debugging convenience,
        // not a timing claim: WinForms timers can jitter, while the emulated state advances
        // deterministically by exactly one frame per call regardless of wall-clock delay.
        playbackTimer.Tick += (_, _) => StepFrames(1);

        toolStrip.Items.Add(stepButton);
        toolStrip.Items.Add(stepSixtyButton);
        toolStrip.Items.Add(playButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(ceresButton);
        toolStrip.Items.Add(motherBrainButton);
        toolStrip.Items.Add(groundedRunButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(holdLeftButton);
        toolStrip.Items.Add(holdRightButton);
        toolStrip.Items.Add(holdJumpButton);
        toolStrip.Items.Add(livePpuLayersButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(cameraLeftButton);
        toolStrip.Items.Add(cameraRightButton);
        toolStrip.Items.Add(cameraUpButton);
        toolStrip.Items.Add(cameraDownButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(statusLabel);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.Controls.Add(toolStrip, 0, 0);
        layout.Controls.Add(canvas, 0, 1);
        Controls.Add(layout);

        // The viewer's primary job has shifted from proving the timer/cinematic render seam
        // to making translated gameplay inspectable. Start directly in the grounded ROM-
        // terrain sandbox so a no-argument launch is immediately meaningful; the two
        // cinematic buttons remain available as explicit diagnostics.
        Restart(motherBrain: false, groundedRun: true);
    }

    private void Restart(bool motherBrain, bool groundedRun)
    {
        playbackTimer.Stop();
        playButton.Checked = false;
        playButton.Text = "Play";
        holdLeftButton.Checked = false;
        holdRightButton.Checked = false;
        holdJumpButton.Checked = false;
        motherBrainScenario = motherBrain;
        groundedRunScenario = groundedRun;
        haltedAtUntranslatedBoundary = null;

        // A fresh runtime means fresh VRAM/OAM/counters but reuses the immutable ROM and
        // bus allocations. Queueing these exact $A6:C4CB transfers before frame one mirrors
        // the game's timer setup and makes the first accepted NMI consume them naturally.
        runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeLandingSiteCamera();
        camera = runtime.Camera!;

        // The grounded scenario derives its resting Y from the visible ROM solid floor at
        // X=$0440/Y-block $4D and moves the camera before the 17-column fill. Selecting the
        // lower floor is deliberate: an earlier valid slope at row $45 has transparent art,
        // which made collision correct but gave a useless terrain-verification viewport.
        // The cinematic scenarios preserve the landing-cutscene door's real (4,0) camera.
        if (groundedRun)
            runtime.InitializeDebugGroundedSamus();
        runtime.InitializeLandingSiteViewport();
        runtime.LoadUpperCrateriaBackgroundPalette();
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.QueueEscapeTimerSpriteTiles();

        // This is explicitly a render-path stimulus, not a fabricated landing-cutscene
        // spawn. The method documents its one host-selected position while resolving pose,
        // palette, graphics, DMA definitions, and spritemaps from the private cartridge.
        if (!groundedRun)
            runtime.InitializeDebugStandingSamus();
        if (motherBrain)
            runtime.EscapeTimer.RequestMotherBrainStart();
        else
            runtime.EscapeTimer.RequestCeresStart();

        // The grounded sandbox defaults to the complete ROM-derived room composition so
        // movement can be judged against recognizable terrain now. Cinematic diagnostics
        // retain live PPU layers by default. Do this only after runtime/camera creation,
        // because changing the checked state immediately requests a repaint.
        livePpuLayersButton.Checked = !groundedRun;

        StepFrames(1);
    }

    private void StepFrames(int count)
    {
        if (haltedAtUntranslatedBoundary is not null)
        {
            RefreshFrame();
            return;
        }

        // Deliberately keep the loop here instead of inventing a bulk-update API. A source
        // breakpoint in StepFrame is hit once per authentic frame, even for the "Step 60"
        // convenience button, and no timer transitions can be skipped.
        for (int frame = 0; frame < count; frame++)
        {
            // These buttons feed the cartridge's canonical $0200/$0100 direction bits.
            // StepFrame produces newly-pressed/held words, and the literal ROM transition
            // table decides whether to run, remain in a turn, or begin a reversal.
            ushort input = 0;
            if (holdLeftButton.Checked)
                input |= (ushort)SnesButton.Left;
            else if (holdRightButton.Checked)
                input |= (ushort)SnesButton.Right;
            if (holdJumpButton.Checked)
                input |= (ushort)SnesButton.A;
            try
            {
                runtime.StepFrame(input);
            }
            catch (NotSupportedException exception)
            {
                // Reaching an unported block/pose is expected during an incremental decomp.
                // Freeze on the exact offending frame and surface the exception text in the
                // viewer instead of substituting fake physics or terminating WinForms.
                playbackTimer.Stop();
                playButton.Checked = false;
                playButton.Text = "Play";
                haltedAtUntranslatedBoundary = exception.Message;
                break;
            }
        }

        RefreshFrame();
    }

    private void RefreshFrame()
    {
        Rgba32[] pixels;
        if (livePpuLayersButton.Checked)
        {
            pixels = SnesGameplayFrameRenderer.RenderHudLiveBackgroundsAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                runtime.BackgroundScroll.Bg1HorizontalScroll,
                runtime.BackgroundScroll.Bg1VerticalScroll,
                runtime.ScrollingSky!.VerticalScroll,
                runtime.ScrollingSky.BuildGameplayHorizontalScrolls(camera.YPosition),
                GameplayObsel);
        }
        else
        {
            // RoomRenderer decoded these pixels from the same private level stream, tiles,
            // block definitions, and palette used by the static Landing Site tab. Only the
            // background source is precomposed; HUD, Samus graphics, OAM timing, and movement
            // remain the live runtime state being stepped above.
            pixels = SnesGameplayFrameRenderer.RenderHudRoomAndObjs(
                runtime.Vram,
                runtime.Cgram,
                runtime.DisplayedOam,
                room.Pixels,
                room.Width,
                room.Height,
                camera.XPosition,
                camera.YPosition,
                GameplayObsel);
        }
        canvas.ReplaceFrame(RgbaBitmap.Create(FrameWidth, FrameHeight, pixels));

        EscapeTimer timer = runtime.EscapeTimer;
        string prospectivePose = runtime.ProspectiveSamusPose is SamusPoseTransition transition
            ? runtime.GroundedSamusMovementEnabled &&
              transition.ProspectivePose is
                  SamusState.MovingRightNormalPose or
                  SamusState.MovingLeftNormalPose or
                  SamusState.TurningRightToLeftPose or
                  SamusState.TurningLeftToRightPose or
                  SamusState.NeutralJumpTransitionRightPose or
                  SamusState.NeutralJumpTransitionLeftPose or
                  SamusState.SpinJumpRightPose or
                  SamusState.SpinJumpLeftPose
                ? $"next ${transition.ProspectivePose:X2} translated"
                : $"next ${transition.ProspectivePose:X2} blocked"
            : runtime.ProspectiveSamusFallbackPose is ushort fallback
                ? $"no-button fallback ${fallback:X2}"
                : "no pose transition";
        statusLabel.Text =
            (haltedAtUntranslatedBoundary is null
                ? string.Empty
                : $"HALTED: {haltedAtUntranslatedBoundary}  |  ") +
            $"{(groundedRunScenario ? "Grounded run" : motherBrainScenario ? "Mother Brain" : "Ceres")}  |  " +
            $"frame {runtime.NmiFrameCounter}  |  {timer.State}  |  " +
            $"{timer.MinutesBcd:X2}:{timer.SecondsBcd:X2}.{timer.CentisecondsBcd:X2}  |  " +
            $"position ({timer.XPixel},{timer.YPixel})  |  camera ({camera.XPosition},{camera.YPosition})  |  " +
            $"map ({runtime.Hud.MinimapCenterX},{runtime.Hud.MinimapCenterY})  |  " +
            $"Samus pose ${runtime.Samus!.Pose:X2}/frame {runtime.Samus.AnimationFrame}  |  " +
            $"X {runtime.Samus.XPosition:X4}.{runtime.Samus.Kinematics.XSubposition:X4} " +
            $"speed {runtime.Samus.HorizontalSpeed.BaseSpeed:X4}." +
            $"{runtime.Samus.HorizontalSpeed.BaseSubspeed:X4}  |  " +
            $"Y {runtime.Samus.YPosition:X4}.{runtime.Samus.Kinematics.YSubposition:X4} " +
            $"speed {runtime.Samus.Kinematics.YSpeed:X4}." +
            $"{runtime.Samus.Kinematics.YSubspeed:X4}/dir {runtime.Samus.Kinematics.YDirection}  |  " +
            $"terrain={(livePpuLayersButton.Checked ? "live PPU" : "ROM composite")}  |  " +
            $"{prospectivePose}  |  " +
            $"{runtime.LastBackgroundUpdateCount} BG update(s)  |  " +
            $"{runtime.DisplayedOam.LastFinalizedSpriteCount} displayed OBJs";
    }

    private void MoveCamera(int deltaX, int deltaY)
    {
        // One click proposes one 16x16 room block of movement. The sign only selects which
        // authentic bank-$80 directional handler receives that proposal.
        if (deltaX < 0)
            camera.MoveLeft((ushort)-deltaX);
        else if (deltaX > 0)
            camera.MoveRight((ushort)deltaX);
        else if (deltaY < 0)
            camera.MoveUp((ushort)-deltaY);
        else if (deltaY > 0)
            camera.MoveDown((ushort)deltaY);
        StreamCurrentBackgroundUpdates();
        RefreshFrame();
    }

    private int StreamCurrentBackgroundUpdates()
    {
        IReadOnlyList<BackgroundUpdateRequest> requests = runtime.UpdateBackgroundScrollingFromCamera();
        foreach (BackgroundUpdateRequest request in requests)
        {
            TilemapStreamUpdate update = runtime.BackgroundStreamer!.Build(request)
                ?? throw new InvalidOperationException("Landing Site unexpectedly entered Mode 7 streaming.");
            update.ExecuteTo(runtime.Vram);
        }
        return requests.Count;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            playbackTimer.Dispose();
        base.Dispose(disposing);
    }
}
