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
    private readonly ToolStripButton holdRunButton = new("Hold Run");
    private readonly ToolStripButton holdShootButton = new("Hold Shoot");
    private readonly ToolStripButton holdUpButton = new("Hold Up");
    private readonly ToolStripButton holdDownButton = new("Hold Down");
    private readonly ToolStripButton holdAimUpButton = new("Hold Aim Up");
    private readonly ToolStripButton holdAimDownButton = new("Hold Aim Down");
    private readonly ToolStripButton chargeBeamButton = new("Charge Beam equipped");
    private readonly ToolStripButton moonwalkButton = new("Moonwalk enabled");
    private readonly ToolStripButton springBallButton = new("Spring Ball equipped");
    private readonly ToolStripButton spaceJumpButton = new("Space Jump equipped");
    private readonly ToolStripButton screwAttackButton = new("Screw Attack equipped");
    private readonly ToolStripButton speedBoosterButton = new("Speed Booster equipped");
    private readonly ToolStripButton waterPhysicsButton = new("Water at current Y");
    private readonly ToolStripButton gravitySuitButton = new("Gravity Suit equipped");
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
        var crystalFlashButton = new ToolStripButton("Restart Crystal Flash");
        var cameraLeftButton = new ToolStripButton("Room ←");
        var cameraRightButton = new ToolStripButton("Room →");
        var cameraUpButton = new ToolStripButton("Room ↑");
        var cameraDownButton = new ToolStripButton("Room ↓");

        stepButton.Click += (_, _) => StepFrames(1);
        stepSixtyButton.Click += (_, _) => StepFrames(60);
        ceresButton.Click += (_, _) => Restart(motherBrain: false, groundedRun: false);
        motherBrainButton.Click += (_, _) => Restart(motherBrain: true, groundedRun: false);
        groundedRunButton.Click += (_, _) => Restart(motherBrain: false, groundedRun: true);
        crystalFlashButton.Click += (_, _) => RestartCrystalFlash();
        crystalFlashButton.ToolTipText =
            "Restarts grounded Samus, grants the exact 10/10/10 ammo fixture, and publishes the missing power-bomb-cleanup call with Down+L+R+Shoot.";
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
        holdRunButton.CheckOnClick = true;
        holdRunButton.ToolTipText =
            "Feeds canonical Dash/B bit $8000. Hold with a direction for ROM-authored Dash; Speed Booster changes its cap and animation stages.";
        holdShootButton.CheckOnClick = true;
        holdShootButton.ToolTipText =
            "Feeds canonical Shoot/X bit $0040. Click on for a fresh bomb-placement edge, then off before placing another.";
        holdUpButton.CheckOnClick = true;
        holdUpButton.ToolTipText = "Feeds Up; a new Up press starts the ROM crouch-to-standing transition.";
        holdDownButton.CheckOnClick = true;
        holdDownButton.ToolTipText = "Feeds Down; a new Down press starts the ROM standing-to-crouch transition.";
        holdAimUpButton.CheckOnClick = true;
        holdAimUpButton.ToolTipText = "Feeds canonical aim-up bit $0010 (default R shoulder), selecting diagonal-up poses.";
        holdAimDownButton.CheckOnClick = true;
        holdAimDownButton.ToolTipText = "Feeds canonical aim-down bit $0020 (default L shoulder), selecting diagonal-down poses.";
        chargeBeamButton.CheckOnClick = true;
        chargeBeamButton.ToolTipText =
            "Toggles beam bit $1000. Hold Shoot for 60 frames, then release to fire the cartridge's charged power beam.";
        chargeBeamButton.CheckedChanged += (_, _) =>
        {
            // Pause/equipment code is not translated yet. This switch owns only Charge
            // Beam bit `$1000`; every counter, flare frame, release threshold, projectile
            // record, and OAM sprite remains selected by the cartridge-backed subsystem.
            if (runtime?.Samus is null || !groundedRunScenario)
                return;
            if (chargeBeamButton.Checked)
                runtime.Samus.EquippedBeams |= 0x1000;
            else
                runtime.Samus.EquippedBeams &= unchecked((ushort)~0x1000);
        };
        moonwalkButton.CheckOnClick = true;
        moonwalkButton.ToolTipText =
            "Mirrors the cartridge Moonwalk option word. Enable it, then hold Shoot plus the direction behind Samus.";
        moonwalkButton.CheckedChanged += (_, _) =>
        {
            // `$91:F88C` reads a persistent options word only when a standing input table
            // proposes `$49/$4A/$75-$78`. Exposing that word as a debugger toggle changes
            // no transition records and can safely take effect without restarting.
            if (runtime is not null)
                runtime.MoonwalkEnabled = moonwalkButton.Checked;
        };
        springBallButton.CheckOnClick = true;
        springBallButton.ToolTipText =
            "Adds equipped-item bit $0002 to the grounded debugger inventory. Toggle before morphing; restart if already in ball form.";
        springBallButton.CheckedChanged += (_, _) =>
        {
            // Save-file/equipment-pause processing is not translated yet. Make the debug
            // inventory write visible and narrow: toggling changes only Spring Ball bit
            // `$0002`, while Morph Ball bit `$0004` remains the grounded sandbox baseline.
            if (runtime?.Samus is null || !groundedRunScenario)
                return;
            if (springBallButton.Checked)
                runtime.Samus.EquippedItems |= 0x0002;
            else
                runtime.Samus.EquippedItems &= unchecked((ushort)~0x0002);
        };
        spaceJumpButton.CheckOnClick = true;
        spaceJumpButton.ToolTipText =
            "Toggles item bit $0200. While descending, release and freshly press Jump inside the native velocity window to jump again.";
        spaceJumpButton.CheckedChanged += (_, _) =>
        {
            // Pause/equipment normalization is not translated yet. This debug switch owns
            // exactly Space Jump bit `$0200`; the next spin launch reads it through the
            // native `$91:F624` selector. Toggling during a spin changes repeat permission
            // immediately, just as the movement handler's direct equipment test does.
            if (runtime?.Samus is null || !groundedRunScenario)
                return;
            if (spaceJumpButton.Checked)
                runtime.Samus.EquippedItems |= 0x0200;
            else
                runtime.Samus.EquippedItems &= unchecked((ushort)~0x0200);
        };
        screwAttackButton.CheckOnClick = true;
        screwAttackButton.ToolTipText =
            "Toggles item bit $0008. Screw Attack takes pose priority over Space Jump; toggle before jumping or restart the sandbox.";
        screwAttackButton.CheckedChanged += (_, _) =>
        {
            // The real pause close routine also rewrites a currently spinning pose. Until
            // that pause seam exists, keep the debug mutation explicit and narrow: it
            // affects the next equipment-aware spin transition and never fabricates art.
            if (runtime?.Samus is null || !groundedRunScenario)
                return;
            if (screwAttackButton.Checked)
                runtime.Samus.EquippedItems |= 0x0008;
            else
                runtime.Samus.EquippedItems &= unchecked((ushort)~0x0008);
        };
        speedBoosterButton.CheckOnClick = true;
        speedBoosterButton.ToolTipText =
            "Toggles item bit $2000. Hold Run plus a direction to charge; crouch at stage four to store shine, then Jump and tap a direction during windup.";
        speedBoosterButton.CheckedChanged += (_, _) =>
        {
            // Equipment-menu processing is not present yet, so expose the one retail item
            // bit directly. Turning it off cancels staged state immediately; native pause
            // equipment code performs equivalent normalization before gameplay resumes.
            if (runtime?.Samus is null || !groundedRunScenario)
                return;
            if (speedBoosterButton.Checked)
            {
                runtime.Samus.EquippedItems |= 0x2000;
            }
            else
            {
                runtime.Samus.EquippedItems &= unchecked((ushort)~0x2000);
                runtime.Samus.HorizontalSpeed.CancelRunningMomentum(
                    runtime.Samus.ReadPoseXDirection(bus));
            }
        };
        waterPhysicsButton.CheckOnClick = true;
        waterPhysicsButton.ToolTipText =
            "Installs a fixed water-physics surface at Samus's current center Y. This is a documented FX stimulus; it does not fake a water overlay.";
        waterPhysicsButton.CheckedChanged += (_, _) =>
        {
            if (runtime?.Samus is null || !groundedRunScenario)
                return;

            if (waterPhysicsButton.Checked)
            {
                // Landing Site's real FX is scrolling sky, so the room supplies no water
                // surface of its own. Publish exactly the three room-FX words that a water
                // room would own, at one fixed world Y captured when the switch is clicked.
                // Every resulting table lookup, boundary comparison, launch, and animation
                // delay remains cartridge-derived and can be stepped in the debugger.
                runtime.Samus.LiquidPhysics.ConfigureWater(runtime.Samus.YPosition);
                runtime.Samus.LiquidPhysics.InitializeRememberedMedium(runtime.Samus);
            }
            else
            {
                runtime.Samus.LiquidPhysics.Clear();
            }

            RefreshFrame();
        };
        gravitySuitButton.CheckOnClick = true;
        gravitySuitButton.ToolTipText =
            "Toggles retail equipped-item bit $0020. Gravity Suit bypasses water/lava movement while retaining the room FX surface.";
        gravitySuitButton.CheckedChanged += (_, _) =>
        {
            if (runtime?.Samus is null || !groundedRunScenario)
                return;
            if (gravitySuitButton.Checked)
                runtime.Samus.EquippedItems |= SamusLiquidPhysicsState.GravitySuitItem;
            else
                runtime.Samus.EquippedItems &= unchecked((ushort)~SamusLiquidPhysicsState.GravitySuitItem);
            RefreshFrame();
        };

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
        holdUpButton.CheckedChanged += (_, _) =>
        {
            if (holdUpButton.Checked)
                holdDownButton.Checked = false;
        };
        holdDownButton.CheckedChanged += (_, _) =>
        {
            if (holdDownButton.Checked)
                holdUpButton.Checked = false;
        };
        holdAimUpButton.CheckedChanged += (_, _) =>
        {
            if (holdAimUpButton.Checked)
                holdAimDownButton.Checked = false;
        };
        holdAimDownButton.CheckedChanged += (_, _) =>
        {
            if (holdAimDownButton.Checked)
                holdAimUpButton.Checked = false;
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
        toolStrip.Items.Add(crystalFlashButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(holdLeftButton);
        toolStrip.Items.Add(holdRightButton);
        toolStrip.Items.Add(holdJumpButton);
        toolStrip.Items.Add(holdRunButton);
        toolStrip.Items.Add(holdShootButton);
        toolStrip.Items.Add(holdUpButton);
        toolStrip.Items.Add(holdDownButton);
        toolStrip.Items.Add(holdAimUpButton);
        toolStrip.Items.Add(holdAimDownButton);
        toolStrip.Items.Add(chargeBeamButton);
        toolStrip.Items.Add(moonwalkButton);
        toolStrip.Items.Add(springBallButton);
        toolStrip.Items.Add(spaceJumpButton);
        toolStrip.Items.Add(screwAttackButton);
        toolStrip.Items.Add(speedBoosterButton);
        toolStrip.Items.Add(waterPhysicsButton);
        toolStrip.Items.Add(gravitySuitButton);
        toolStrip.Items.Add(livePpuLayersButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(cameraLeftButton);
        toolStrip.Items.Add(cameraRightButton);
        toolStrip.Items.Add(cameraUpButton);
        toolStrip.Items.Add(cameraDownButton);
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(statusLabel);

        // This is a movement-development sandbox, and there is no save-file/equipment menu
        // to grant the item naturally yet. Default the visible toggle on so a fresh launch
        // can exercise the newly translated route immediately; unchecking it still removes
        // only retail inventory bit `$2000` and cancels the corresponding momentum state.
        speedBoosterButton.Checked = true;

        // Space Jump is useful only after an ordinary jump has entered its descending
        // window, so grant it by default in the debugger sandbox. Screw Attack remains off
        // by default, making both `$1B/$1C` art and repeat timing immediately inspectable;
        // checking Screw demonstrates its native priority without requiring a new build.
        spaceJumpButton.Checked = true;

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
        holdRunButton.Checked = false;
        holdShootButton.Checked = false;
        holdUpButton.Checked = false;
        holdDownButton.Checked = false;
        holdAimUpButton.Checked = false;
        holdAimDownButton.Checked = false;
        motherBrainScenario = motherBrain;
        groundedRunScenario = groundedRun;
        haltedAtUntranslatedBoundary = null;

        // A fresh runtime means fresh VRAM/OAM/counters but reuses the immutable ROM and
        // bus allocations. Queueing these exact $A6:C4CB transfers before frame one mirrors
        // the game's timer setup and makes the first accepted NMI consume them naturally.
        runtime = new SuperMetroidRuntime(bus);
        runtime.MoonwalkEnabled = moonwalkButton.Checked;
        runtime.InitializeLandingSiteCamera();
        camera = runtime.Camera!;

        // The grounded scenario derives its resting Y from the visible ROM solid floor at
        // X=$0440/Y-block $4D and moves the camera before the 17-column fill. Selecting the
        // lower floor is deliberate: an earlier valid slope at row $45 has transparent art,
        // which made collision correct but gave a useless terrain-verification viewport.
        // The cinematic scenarios preserve the landing-cutscene door's real (4,0) camera.
        if (groundedRun)
        {
            runtime.InitializeDebugGroundedSamus();

            // The sandbox has no save-file loader yet, so its inventory would otherwise be
            // empty forever. Grant Morph Ball and Bomb item bits as explicit debugger
            // stimuli. The interactive route still uses the cartridge's real
            // input records: tap Down to crouch, release it, then tap Down again to morph.
            // Cinematic diagnostics receive no inventory, and later save-state work should
            // replace this one deliberately visible host grant rather than hiding it.
            runtime.Samus!.EquippedItems |= 0x1004;
            if (springBallButton.Checked)
                runtime.Samus.EquippedItems |= 0x0002;
            if (spaceJumpButton.Checked)
                runtime.Samus.EquippedItems |= 0x0200;
            if (screwAttackButton.Checked)
                runtime.Samus.EquippedItems |= 0x0008;
            if (speedBoosterButton.Checked)
                runtime.Samus.EquippedItems |= 0x2000;
            if (gravitySuitButton.Checked)
                runtime.Samus.EquippedItems |= SamusLiquidPhysicsState.GravitySuitItem;
            if (chargeBeamButton.Checked)
                runtime.Samus.EquippedBeams |= 0x1000;
            if (waterPhysicsButton.Checked)
            {
                // Restart gives the debugger a new Samus center, so capture a new fixed
                // surface here rather than carrying a coordinate from the previous runtime.
                runtime.Samus.LiquidPhysics.ConfigureWater(runtime.Samus.YPosition);
                runtime.Samus.LiquidPhysics.InitializeRememberedMedium(runtime.Samus);
            }
        }
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
            if (holdRunButton.Checked)
            {
                // B is not a host-side speed multiplier. It reaches the translated
                // `$90:973E` state machine and the `$90:852C` animation-delay selector.
                input |= (ushort)SnesButton.B;
            }
            if (holdShootButton.Checked)
                input |= (ushort)SnesButton.X;
            if (holdUpButton.Checked)
                input |= (ushort)SnesButton.Up;
            else if (holdDownButton.Checked)
                input |= (ushort)SnesButton.Down;
            if (holdAimUpButton.Checked)
                input |= (ushort)SnesButton.R;
            else if (holdAimDownButton.Checked)
                input |= (ushort)SnesButton.L;
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

    /// <summary>
    /// Creates the deterministic interactive equivalent of bank `$88:8B5F` reaching
    /// <c>CrystalFlash</c> after a centred power-bomb explosion finishes.
    /// </summary>
    private void RestartCrystalFlash()
    {
        // Restart first so the native zero-vertical-speed precondition is meaningful and
        // stale movement from a prior experiment cannot be silently erased just to make the
        // move succeed. The one startup step performed by Restart leaves grounded Samus at
        // rest and also primes the normal VRAM/OAM producer-consumer pipeline.
        Restart(motherBrain: false, groundedRun: true);

        // Save-file loading and the complete power-bomb HDMA producer are still outside the
        // current runtime. These values are explicit debugger stimuli at that boundary; the
        // called routine still checks every current word exactly as `$90:D5A2` does.
        runtime.Samus!.Health = 1;
        runtime.Samus.MaxHealth = 99;
        runtime.Samus.ReserveEnergy = 0;
        runtime.Samus.MaxReserveEnergy = 0;
        runtime.Samus.Missiles = 10;
        runtime.Samus.SuperMissiles = 10;
        runtime.Samus.PowerBombs = 10;
        const ushort chord =
            (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
        if (!runtime.TryBeginCrystalFlashFromPowerBombCleanup(chord))
        {
            haltedAtUntranslatedBoundary =
                "Crystal Flash initiation rejected the documented grounded 10/10/10 fixture.";
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
                  SamusState.TurningRightToLeftAimUpPose or
                  SamusState.TurningLeftToRightAimUpPose or
                  SamusState.TurningRightToLeftAimDiagonalUpPose or
                  SamusState.TurningLeftToRightAimDiagonalUpPose or
                  SamusState.TurningRightToLeftAimDiagonalDownPose or
                  SamusState.TurningLeftToRightAimDiagonalDownPose or
                  SamusState.TurningRightToLeftCrouchingPose or
                  SamusState.TurningLeftToRightCrouchingPose or
                  SamusState.TurningRightToLeftCrouchingAimUpPose or
                  SamusState.TurningLeftToRightCrouchingAimUpPose or
                  SamusState.TurningRightToLeftCrouchingAimDiagonalUpPose or
                  SamusState.TurningLeftToRightCrouchingAimDiagonalUpPose or
                  SamusState.TurningRightToLeftCrouchingAimDiagonalDownPose or
                  SamusState.TurningLeftToRightCrouchingAimDiagonalDownPose or
                  SamusState.NeutralJumpTransitionRightPose or
                  SamusState.NeutralJumpTransitionLeftPose or
                  SamusState.SpinJumpRightPose or
                  SamusState.SpinJumpLeftPose or
                  SamusState.CrouchingTransitionRightPose or
                  SamusState.CrouchingTransitionLeftPose or
                  SamusState.StandingTransitionRightPose or
                  SamusState.StandingTransitionLeftPose or
                  SamusState.StandingAimUpRightPose or
                  SamusState.StandingAimUpLeftPose or
                  SamusState.StandingAimDiagonalUpRightPose or
                  SamusState.StandingAimDiagonalUpLeftPose or
                  SamusState.StandingAimDiagonalDownRightPose or
                  SamusState.StandingAimDiagonalDownLeftPose or
                  SamusState.RunningAimUpRightPose or
                  SamusState.RunningAimUpLeftPose or
                  SamusState.RunningAimDiagonalUpRightPose or
                  SamusState.RunningAimDiagonalUpLeftPose or
                  SamusState.RunningAimDiagonalDownRightPose or
                  SamusState.RunningAimDiagonalDownLeftPose or
                  SamusState.NormalJumpForwardRightPose or
                  SamusState.NormalJumpForwardLeftPose or
                  SamusState.NormalJumpAimUpRightPose or
                  SamusState.NormalJumpAimUpLeftPose or
                  SamusState.NormalJumpTransitionAimUpRightPose or
                  SamusState.NormalJumpTransitionAimUpLeftPose or
                  SamusState.NormalJumpTransitionAimDiagonalUpRightPose or
                  SamusState.NormalJumpTransitionAimDiagonalUpLeftPose or
                  SamusState.NormalJumpTransitionAimDiagonalDownRightPose or
                  SamusState.NormalJumpTransitionAimDiagonalDownLeftPose or
                  SamusState.NormalJumpAimDiagonalUpRightPose or
                  SamusState.NormalJumpAimDiagonalUpLeftPose or
                  SamusState.NormalJumpAimDiagonalDownRightPose or
                  SamusState.NormalJumpAimDiagonalDownLeftPose or
                  SamusState.FallingAimUpRightPose or
                  SamusState.FallingAimUpLeftPose or
                  SamusState.FallingAimDiagonalUpRightPose or
                  SamusState.FallingAimDiagonalUpLeftPose or
                  SamusState.FallingAimDiagonalDownRightPose or
                  SamusState.FallingAimDiagonalDownLeftPose or
                  SamusState.CrouchingAimUpRightPose or
                  SamusState.CrouchingAimUpLeftPose or
                  SamusState.CrouchingAimDiagonalUpRightPose or
                  SamusState.CrouchingAimDiagonalUpLeftPose or
                  SamusState.CrouchingAimDiagonalDownRightPose or
                  SamusState.CrouchingAimDiagonalDownLeftPose or
                  SamusState.LandingAimUpRightPose or
                  SamusState.LandingAimUpLeftPose or
                  SamusState.LandingAimDiagonalUpRightPose or
                  SamusState.LandingAimDiagonalUpLeftPose or
                  SamusState.LandingAimDiagonalDownRightPose or
                  SamusState.LandingAimDiagonalDownLeftPose or
                  SamusState.CrouchingTransitionAimUpRightPose or
                  SamusState.CrouchingTransitionAimUpLeftPose or
                  SamusState.CrouchingTransitionAimDiagonalUpRightPose or
                  SamusState.CrouchingTransitionAimDiagonalUpLeftPose or
                  SamusState.CrouchingTransitionAimDiagonalDownRightPose or
                  SamusState.CrouchingTransitionAimDiagonalDownLeftPose or
                  SamusState.StandingTransitionAimUpRightPose or
                  SamusState.StandingTransitionAimUpLeftPose or
                  SamusState.StandingTransitionAimDiagonalUpRightPose or
                  SamusState.StandingTransitionAimDiagonalUpLeftPose or
                  SamusState.StandingTransitionAimDiagonalDownRightPose or
                  SamusState.StandingTransitionAimDiagonalDownLeftPose or
                  SamusState.MorphingTransitionRightPose or
                  SamusState.MorphingTransitionLeftPose or
                  SamusState.UnmorphingTransitionRightPose or
                  SamusState.UnmorphingTransitionLeftPose or
                  SamusState.MorphBallGroundRightPose or
                  SamusState.MorphBallGroundLeftPose or
                  SamusState.MorphBallMovingRightPose or
                  SamusState.MorphBallMovingLeftPose
                  or SamusState.MoonwalkFacingLeftPose
                  or SamusState.MoonwalkFacingRightPose
                  or SamusState.MoonwalkAimUpLeftPose
                  or SamusState.MoonwalkAimUpRightPose
                  or SamusState.MoonwalkAimDownLeftPose
                  or SamusState.MoonwalkAimDownRightPose
                  or SamusState.MoonwalkTurnJumpLeftPose
                  or SamusState.MoonwalkTurnJumpRightPose
                  or SamusState.MoonwalkTurnJumpAimUpLeftPose
                  or SamusState.MoonwalkTurnJumpAimUpRightPose
                  or SamusState.MoonwalkTurnJumpAimDownLeftPose
                  or SamusState.MoonwalkTurnJumpAimDownRightPose
                  or SamusState.RanIntoWallRightPose
                  or SamusState.RanIntoWallLeftPose
                  or SamusState.RanIntoWallAimUpRightPose
                  or SamusState.RanIntoWallAimUpLeftPose
                  or SamusState.RanIntoWallAimDownRightPose
                  or SamusState.RanIntoWallAimDownLeftPose
                  or SamusState.ShinesparkWindupRightPose
                  or SamusState.ShinesparkWindupLeftPose
                  or SamusState.ShinesparkHorizontalRightPose
                  or SamusState.ShinesparkHorizontalLeftPose
                  or SamusState.ShinesparkVerticalRightPose
                  or SamusState.ShinesparkVerticalLeftPose
                  or SamusState.ShinesparkDiagonalRightPose
                  or SamusState.ShinesparkDiagonalLeftPose
                  or SamusState.SpringBallGroundRightPose
                  or SamusState.SpringBallGroundLeftPose
                  or SamusState.SpringBallMovingRightPose
                  or SamusState.SpringBallMovingLeftPose
                  or SamusState.SpringBallFallingRightPose
                  or SamusState.SpringBallFallingLeftPose
                  or SamusState.SpringBallJumpRightPose
                  or SamusState.SpringBallJumpLeftPose
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
            $"shine={runtime.Samus.Shinespark.Phase}/timer {runtime.Samus.Shinespark.ShineTimer} " +
            $"crash {runtime.Samus.Shinespark.CrashSubphase}:{runtime.Samus.Shinespark.CrashRadius} " +
            $"released {runtime.Samus.Shinespark.ReleasedCrashEchoCount}  |  " +
            $"crystal={runtime.Samus.CrystalFlash.Phase} " +
            $"ammo {runtime.Samus.Missiles}/{runtime.Samus.SuperMissiles}/{runtime.Samus.PowerBombs} " +
            $"energy {runtime.Samus.Health}/{runtime.Samus.MaxHealth}  |  " +
            // `$0AAE` is deliberately shown as raw hexadecimal because zero/two are the
            // active capture slots, `$FFFF` is ordinary cancellation departure, and the
            // shinespark crash overloads both bytes with radius/subphase state.
            $"speed echoes ${runtime.Samus.HorizontalSpeed.SpeedEchoIndex:X4} " +
            $"({runtime.Samus.HorizontalSpeed.FirstSpeedEchoXPosition:X4}/" +
            $"{runtime.Samus.HorizontalSpeed.SecondSpeedEchoXPosition:X4})  |  " +
            $"bombs {runtime.BombProjectiles.BombCounter}/5 " +
            $"cooldown {runtime.BombProjectiles.CooldownTimer} " +
            $"jump ${runtime.Samus.BombJumpDirection:X4}  |  " +
            // Ordinary beams and bombs share native cooldown `$0CCC`; showing the ordinary
            // counter beside that clock makes allocation, terrain impact, explosion, and
            // eventual bank-$93 deletion directly visible while stepping Hold Shoot.
            $"beams {runtime.Projectiles.ProjectileCounter}/5 " +
            $"charge {runtime.Projectiles.FlareCounter}/120 " +
            $"trails {runtime.Projectiles.ActiveTrailCount}/18 " +
            $"lastShot={(runtime.Projectiles.LastFrameResult.FiredSlot is int shot ? shot : -1)} " +
            $"impact={(runtime.Projectiles.LastFrameResult.CollisionStartedExplosion ? "yes" : "no")}  |  " +
            $"moonwalk={(runtime.MoonwalkEnabled ? "on" : "off")}  |  " +
            $"liquid={runtime.Samus.LiquidPhysics.LiquidPhysicsType}/" +
            $"FX ${runtime.Samus.LiquidPhysics.FxType:X2}@" +
            $"${runtime.Samus.LiquidPhysics.FxYPosition:X4} " +
            $"gravity={((runtime.Samus.EquippedItems & SamusLiquidPhysicsState.GravitySuitItem) != 0 ? "on" : "off")}  |  " +
            $"wall={(runtime.ProspectiveSamusWallCollisionPose is byte wall ? $"${wall:X2}" : "--")}  |  " +
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
