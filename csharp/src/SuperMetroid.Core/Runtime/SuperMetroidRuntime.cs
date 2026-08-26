using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// Incremental, frame-steppable C# runtime shell for translated Super Metroid systems.
/// </summary>
/// <remarks>
/// This is not yet the complete game loop. It establishes the real synchronization seam:
/// an accepted NMI drains video work and latches input, advances NMI counters, and then the
/// currently translated main-thread logic runs. Future bank ports can be added to the logic
/// phase without changing every tool or viewer that wants to step one frame.
/// </remarks>
public sealed class SuperMetroidRuntime
{
    private readonly ISnesAddressSpace _addressSpace;

    public SuperMetroidRuntime(ISnesAddressSpace addressSpace)
    {
        _addressSpace = addressSpace ?? throw new ArgumentNullException(nameof(addressSpace));

        // $82:82C5 copies all 512 bytes of kInitialPalette from ROM $9A:8000. The retail
        // game stages this through WRAM before NMI uploads CGRAM; initializing the modeled
        // PPU here produces the same starting colors while that fade pipeline is ported.
        Cgram.LoadFromBus(_addressSpace, 0x9a8000);
    }

    /// <summary>Bank-$80 shared random/event/input-filter state.</summary>
    public Bank80SystemState System { get; } = new();

    /// <summary>Controller-1 NMI latch from <c>$80:9459</c>.</summary>
    public ControllerInputState Controller1 { get; } = new();

    /// <summary>Escape timer state machine from <c>$80:9DE7</c>.</summary>
    public EscapeTimer EscapeTimer { get; } = new();

    /// <summary>Ordinary seven-byte-record VRAM write table.</summary>
    public VramWriteQueue VramWrites { get; } = new();

    /// <summary>PPU video RAM receiving accepted-NMI transfers.</summary>
    public SnesVram Vram { get; } = new();

    /// <summary>PPU color RAM initialized from <c>kInitialPalette</c> at ROM <c>$9A:8000</c>.</summary>
    public SnesCgram Cgram { get; } = new();

    /// <summary>Current 544-byte sprite table prepared for the next PPU OAM upload.</summary>
    public OamBuffer Oam { get; } = new();

    /// <summary>
    /// Sprite table made PPU-visible by the most recent accepted NMI. Renderers use this
    /// buffer; debugger watches can compare it with <see cref="Oam"/>, which the current
    /// main-loop pass is preparing for the following NMI.
    /// </summary>
    public OamBuffer DisplayedOam { get; } = new();

    /// <summary>
    /// Normal-gameplay Samus rendering state. Null means the current scenario has not
    /// explicitly introduced Samus; the Landing Site cinematic does not do so by itself.
    /// </summary>
    public SamusState? Samus { get; private set; }

    /// <summary>
    /// Winning cartridge pose-transition entry for the most recently latched controller
    /// chord. The grounded debug scenario applies its verified $01/$02/$09/$0A/$25/$26
    /// routes; every other winner remains diagnostic until its native side effects exist.
    /// </summary>
    public SamusPoseTransition? ProspectiveSamusPose { get; private set; }

    /// <summary>
    /// No-button fallback selected by <c>Samus_Pose_CancelGrapple</c> at $91:82D9. This is
    /// separate from <see cref="ProspectiveSamusPose"/> because no six-byte table entry wins.
    /// </summary>
    public ushort? ProspectiveSamusFallbackPose { get; private set; }

    /// <summary>
    /// True only for the explicit grounded gameplay-debug scenario. The cinematic render
    /// stimulus remains stationary and continues treating pose matches as diagnostics.
    /// </summary>
    public bool GroundedSamusMovementEnabled { get; private set; }

    /// <summary>
    /// Horizontal and vertical collision results from the most recent translated grounded
    /// movement pass. Null before movement, and always null for the cinematic stimulus.
    /// </summary>
    public GroundedMovementResult? LastGroundedSamusMovement { get; private set; }

    /// <summary>Mutable gameplay HUD tilemap at WRAM <c>$7E:C608</c>.</summary>
    public HudState Hud { get; } = new();

    /// <summary>
    /// Active layer-1 scroll-boundary state. It remains null until a room-specific scroll
    /// table is loaded; this prevents the runtime from silently assuming Landing Site.
    /// </summary>
    public ScrollBoundaryCamera? Camera { get; private set; }

    /// <summary>
    /// Bank-$80 parallax/register/block dispatcher fed by the active layer-1 camera.
    /// Its requests become row/column staging DMAs when a room loader supplies level data.
    /// </summary>
    public BackgroundScrollState BackgroundScroll { get; } = new();

    /// <summary>Landing Site's ROM-backed bank-$80 tilemap producer after room initialization.</summary>
    public BackgroundTilemapStreamer? BackgroundStreamer { get; private set; }

    /// <summary>Row/column updates produced by the most recent camera-to-BG pass.</summary>
    public int LastBackgroundUpdateCount { get; private set; }

    /// <summary>
    /// Active room's decompressed BG1/BTS/BG2 allocation. Rendering and collision must
    /// share this instance; null means no room level stream has been loaded yet.
    /// </summary>
    public RoomLevelData? LevelData { get; private set; }

    /// <summary>Landing Site's bank-$88 scrolling-sky updater after room initialization.</summary>
    public ScrollingSkyState? ScrollingSky { get; private set; }

    /// <summary>Door/header/library-background data selected for the current Landing Site entry.</summary>
    public LandingSiteEntryState? LandingSiteEntry { get; private set; }

    /// <summary>Loads Landing Site's exact 50-byte scroll buffer and creates its camera.</summary>
    public void InitializeLandingSiteCamera()
    {
        LandingSiteEntry = LandingSiteEntryState.LoadLandingCutscene(_addressSpace);
        RoomScrollGrid scrolls = RoomScrollGrid.LoadLandingSite(_addressSpace);
        Camera = new ScrollBoundaryCamera(scrolls);

        // Door_LandingSite_LandingCutscene ($83:88FE) declares screen position (4,0).
        // Applying it here keeps the selected sky command and camera inseparable; callers
        // can still move through authentic scroll-zone handlers after initialization.
        Camera.SetPosition(LandingSiteEntry.CameraX, LandingSiteEntry.CameraY);

        // Every Landing Site state header stores layer2Scrolls($81, 1). Both axes are odd,
        // so ordinary scrolling does not stream BG2; the scrolling-sky room ASM owns it.
        BackgroundScroll.Layer2ScrollX = 0x81;
        BackgroundScroll.Layer2ScrollY = 0x01;
        BackgroundScroll.PrimePreviousBlocks();
        LevelData = LandingSiteStreamingData.LoadLevel(_addressSpace);
        BackgroundStreamer = LevelData.CreateBackgroundStreamer(sizeOfBg2: 0);
        // Supplying the ROM bus matters at the landing-cutscene's Y=0 edge: bank $88's
        // unsigned cameraY-16 table index intentionally reads adjacent ROM instructions.
        ScrollingSky = new ScrollingSkyState(_addressSpace);
    }

    /// <summary>
    /// Copies the translated layer-1 camera into <c>$80:A3AB</c> scroll bookkeeping and
    /// returns the exact BG update calls selected for this frame.
    /// </summary>
    public IReadOnlyList<BackgroundUpdateRequest> UpdateBackgroundScrollingFromCamera(bool timeIsFrozen = false)
    {
        if (Camera is null)
            throw new InvalidOperationException("A room camera must be initialized before BG scrolling.");

        BackgroundScroll.Layer1XPosition = Camera.XPosition;
        BackgroundScroll.Layer1YPosition = Camera.YPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = BackgroundScroll.StepScrolling(timeIsFrozen);
        LastBackgroundUpdateCount = requests.Count;
        return requests;
    }

    /// <summary>
    /// Loads Landing Site character graphics and performs the native 17-column initial BG1
    /// viewport fill after an entry camera position has been selected.
    /// </summary>
    public InitialViewportResult InitializeLandingSiteViewport()
    {
        if (Camera is null || BackgroundStreamer is null)
            throw new InvalidOperationException("Landing Site camera and stream data must be initialized first.");

        if (LandingSiteEntry is null)
            throw new InvalidOperationException("Landing Site entry metadata was not initialized.");

        LandingSiteStreamingData.LoadCharacterGraphics(_addressSpace, Vram, LandingSiteEntry);
        BackgroundScroll.Layer1XPosition = Camera.XPosition;
        BackgroundScroll.Layer1YPosition = Camera.YPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = BackgroundScroll.BuildInitialViewportRequests();
        LastBackgroundUpdateCount = requests.Count;
        int segmentCount = 0;
        foreach (BackgroundUpdateRequest request in requests)
        {
            TilemapStreamUpdate update = BackgroundStreamer.Build(request)
                ?? throw new InvalidOperationException("Landing Site unexpectedly entered Mode 7 streaming.");
            update.ExecuteTo(Vram);
            segmentCount += update.Segments.Count;
        }

        // $A176 temporarily increments its block words through the fill. Recalculate and
        // seed gameplay comparisons from the actual camera before the first scrolling frame.
        BackgroundScroll.PrimePreviousBlocks();
        ScrollingSky!.ProcessFrame(Camera.YPosition, timeIsFrozen: false, VramWrites);
        return new InitialViewportResult(requests.Count, segmentCount);
    }

    /// <summary>
    /// Loads standard 2-bpp BG3 graphics and initializes the HUD tilemap using the exact
    /// ROM/WRAM/VRAM destinations from <c>$82:82E2</c> and <c>$80:9A79</c>.
    /// </summary>
    public void InitializeHud(HudSnapshot snapshot)
    {
        // The original copies $2000 bytes even though the named standard BG3 graphics are
        // $1000 bytes; the following $1000-byte clear table intentionally fills the rest of
        // VRAM $4000-$4FFF with zeroes.
        VramWrites.Enqueue(sizeInBytes: 0x2000, sourceAddress: 0x9ab200, encodedVramDestination: 0x4000);

        // The immutable first row bypasses WRAM and is DMAed straight from $80:988B.
        VramWrites.Enqueue(sizeInBytes: 0x0040, sourceAddress: 0x80988b, encodedVramDestination: 0x5800);

        Hud.Initialize(_addressSpace, snapshot);
        Hud.QueueUpload(_addressSpace, VramWrites);
    }

    /// <summary>
    /// Decompresses graphics-set-zero's Upper Crateria palette at <c>$C2:AD7C</c> into
    /// CGRAM background colors 0-127. Landing Site and Ceres use this tileset palette.
    /// </summary>
    public void LoadUpperCrateriaBackgroundPalette()
    {
        const int sourceAddress = 0xc2ad7c;
        const int compressedByteCount = 0x00e1;

        // The extracted asset boundary is known from the ROM label map. Reading exactly
        // $E1 bytes allows Decompress to insist the $FF terminator ends the whole stream,
        // catching an incorrect address instead of decoding through unrelated ROM data.
        var compressed = new byte[compressedByteCount];
        for (int index = 0; index < compressed.Length; index++)
            compressed[index] = _addressSpace.ReadByte(sourceAddress + index);

        byte[] paletteBytes = SmCompression.Decompress(compressed, maximumOutputBytes: 0x0100);
        if (paletteBytes.Length != 0x0100)
        {
            throw new InvalidDataException(
                $"Upper Crateria palette expanded to ${paletteBytes.Length:X} bytes instead of $100.");
        }

        // $82:E7C9 originally decompresses these 128 colors to target-palette WRAM and a
        // fade later reaches CGRAM. The desktop preview installs the same final words now;
        // target/current fade buffers remain a future timing layer.
        Cgram.LoadBytes(paletteBytes, destinationIndex: 0);
    }

    /// <summary>
    /// Creates a stationary pose-$01 Samus as an explicit debugger stimulus.
    /// </summary>
    /// <remarks>
    /// The landing-cutscene door enters a cinematic state whose ship/actors are not normal
    /// gameplay Samus. Consequently there is no honest cartridge-defined Samus spawn to
    /// read from that door record. This method chooses only a host placement in the current
    /// viewport; pose tables, palette, graphics DMA, screen-position math,
    /// spritemap pointers, and OAM records all remain cartridge-defined translations.
    /// </remarks>
    public void InitializeDebugStandingSamus()
    {
        if (Camera is null)
            throw new InvalidOperationException("A room camera must be initialized before placing debug Samus.");

        GroundedSamusMovementEnabled = false;
        LastGroundedSamusMovement = null;
        InitializeDebugSamus(
            xPosition: unchecked((ushort)(Camera.XPosition + 64)),
            yPosition: unchecked((ushort)(Camera.YPosition + 166)));
    }

    /// <summary>
    /// Creates pose-$01 Samus on a supported Landing Site floor beneath world X=$0440 and
    /// repositions the room camera so both she and the selected terrain are visible.
    /// </summary>
    /// <remarks>
    /// This is an explicitly host-selected debugger scenario, not a claim that the landing
    /// cinematic door owns a gameplay spawn. The selected X and desired screen Y are host
    /// policy. The default minimum row $4D skips a valid but visually transparent collision
    /// slope and selects the first lower supported surface, a visibly rendered solid floor.
    /// Floor type/BTS, height, pose radius, resting world Y, camera clamps, movement,
    /// collision, animation, graphics, and OAM all come from translated cartridge data.
    /// Call it after <see cref="InitializeLandingSiteCamera"/> and before
    /// <see cref="InitializeLandingSiteViewport"/>, because the latter must fill VRAM for
    /// the newly selected camera position.
    /// </remarks>
    public DebugGroundedSamusPlacement InitializeDebugGroundedSamus(
        ushort xPosition = 0x0440,
        ushort desiredScreenY = 166,
        int minimumFloorBlockY = 0x4d)
    {
        if (Camera is null || LevelData is null)
        {
            throw new InvalidOperationException(
                "Landing Site camera and level data must be initialized before grounding debug Samus.");
        }

        // Radius is pose data, so construct/refresh pose $01 before deriving the surface
        // center. The temporary Y=0 is never rendered or stepped.
        InitializeDebugSamus(xPosition, yPosition: 0);
        ushort yRadius = Samus!.Kinematics.YRadius;

        if ((uint)minimumFloorBlockY >= (uint)LevelData.HeightInBlocks)
            throw new ArgumentOutOfRangeException(nameof(minimumFloorBlockY));

        int blockX = xPosition >> 4;
        for (int blockY = minimumFloorBlockY; blockY < LevelData.HeightInBlocks; blockY++)
        {
            RoomCollisionBlock floor = LevelData.GetCollisionBlock(blockX, blockY);

            byte height;
            if (floor.CollisionType == 8)
            {
                // Solid block type $8 uses the block's top edge as its floor. A height of
                // zero expresses that edge in the same block-local coordinate system used
                // by slope profiles below.
                height = 0;
            }
            else if (floor.CollisionType == 1 &&
                     (floor.Behavior & 0x1f) >= 5 &&
                     (floor.Behavior & 0x80) == 0)
            {
                // Upright non-square slopes use the cartridge's sixteen-sample profile.
                // Square slopes are translated too, but selecting one as a spawn surface
                // would require choosing the occupied quadrant rather than one scalar Y.
                height = SamusSlopePhysics.ReadAlignmentHeight(
                    _addressSpace,
                    floor.Behavior,
                    xPosition);
            }
            else
            {
                continue;
            }

            ushort restingY = unchecked((ushort)(blockY * 16 + height - yRadius));

            // Normal right-facing distance slot zero targets layer1X = SamusX-$60. Seed the
            // camera at that exact target so the first moved frame follows smoothly instead
            // of spending several frames correcting the cutscene door's unrelated X=$400.
            // Vertical framing remains the explicit host stimulus supplied by the caller.
            Camera.SetPosition(xPosition - 0x60, restingY - desiredScreenY);
            Samus.YPosition = restingY;
            GroundedSamusMovementEnabled = true;
            LastGroundedSamusMovement = null;
            return new DebugGroundedSamusPlacement(
                xPosition,
                restingY,
                desiredScreenY,
                blockX,
                blockY,
                floor,
                height);
        }

        throw new InvalidOperationException(
            $"No supported solid or upright non-square floor exists beneath " +
            $"Landing Site X=${xPosition:X4} from block row ${minimumFloorBlockY:X2}.");
    }

    /// <summary>Initializes the ROM-authored standing pose at an explicitly supplied point.</summary>
    private void InitializeDebugSamus(ushort xPosition, ushort yPosition)
    {
        Samus = new SamusState
        {
            Pose = SamusState.FacingRightNormalPose,
            AnimationFrame = 0,
            XPosition = xPosition,
            YPosition = yPosition,
        };

        Samus.LoadPowerSuitPalette(_addressSpace, Cgram);
        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.InitializeAnimation(_addressSpace);

        // StepFrame begins with NMI, so prime the definitions now. Otherwise frame one's
        // OAM would name Samus tiles before any corresponding graphics reached VRAM.
        Samus.PrimeGraphics(_addressSpace);
    }

    /// <summary>
    /// Queues the two number/label tile transfers in the Ceres table at
    /// <c>$A6:C4CB-$A6:C4D8</c>. The following entries are typewriter BG text and are not
    /// needed by the OAM escape-timer renderer.
    /// </summary>
    public void QueueEscapeTimerSpriteTiles()
    {
        VramWrites.Enqueue(sizeInBytes: 0x0200, sourceAddress: 0xb0c000, encodedVramDestination: 0x7e00);
        VramWrites.Enqueue(sizeInBytes: 0x0120, sourceAddress: 0xb0c200, encodedVramDestination: 0x7f00);
    }

    /// <summary>Low-byte NMI frame counter at WRAM <c>$05B5</c>.</summary>
    public byte NmiFrameCounter8 { get; private set; }

    /// <summary>16-bit accepted-NMI frame counter at WRAM <c>$05B6</c>.</summary>
    public ushort NmiFrameCounter { get; private set; }

    /// <summary>Consecutive NMIs for which the main loop had no pending request.</summary>
    public ushort NmiLagCounter { get; private set; }

    /// <summary>Largest observed consecutive lag count.</summary>
    public ushort MaximumNmiLag { get; private set; }

    /// <summary>Counter incremented by every NMI, accepted or lagged.</summary>
    public ushort NmiCounterIncludingLag { get; private set; }

    /// <summary>Runs one accepted NMI followed by the translated per-frame timer logic.</summary>
    public RuntimeFrameResult StepFrame(ushort controller1Input)
    {
        RunNmi(controller1Input, mainLoopRequestedNmi: true);

        // The bank-$82 main loop clears high OAM and resets its stack before dispatching
        // game state, then finalizes unused entries afterward. Samus is emitted before the
        // escape timer so lower OAM indices retain their normal overlap precedence.
        Oam.BeginFrame();
        bool escapeTimerExpired = EscapeTimer.Process(NmiFrameCounter);
        if (Samus is not null && Camera is not null)
        {
            // MainScrollingRoutine compares the post-movement position with the previous
            // frame's stored 16.16 words. Capture them before frame-handler movement mutates
            // Samus, then feed both samples to the translated bank-$90 routines below.
            var previousCameraPoint = new SamusCameraPoint(
                Samus.XPosition,
                Samus.Kinematics.XSubposition,
                Samus.YPosition,
                Samus.Kinematics.YSubposition);

            // Retain the dispatch pose because command $F8 can replace Samus.Pose during
            // animation later in this same frame. Native alpha/beta/transition phases all
            // agree on that order; using the mutable value afterward would apply an input
            // match selected for the old pose to the newly installed one.
            byte poseAtFrameStart = Samus.Pose;

            // Direction bits already use the transition table's canonical layout. Input
            // matching belongs to frame-handler alpha, before beta moves the CURRENT pose;
            // the winning pose is not installed until after animation below.
            ProspectiveSamusPose = SamusPoseTransitionTable.Find(
                _addressSpace,
                Samus.Pose,
                Controller1.Current,
                Controller1.NewlyPressed);
            ProspectiveSamusFallbackPose = null;

            // With no controller bits, $91:82D9 consults pose-definition byte two. Running
            // poses $09/$0A store fallbacks $01/$02, but Samus_Pose_Func2 first preserves
            // the running pose while base speed is nonzero and selects momentum routine
            // one (deceleration). Capture this before movement, where native alpha does.
            if (GroundedSamusMovementEnabled &&
                Samus.Pose is SamusState.MovingRightNormalPose or SamusState.MovingLeftNormalPose &&
                Controller1.Current == 0 &&
                ProspectiveSamusPose is null)
            {
                ProspectiveSamusFallbackPose = Samus.HorizontalSpeed.BaseFixed != 0
                    ? Samus.Pose
                    : Samus.Pose == SamusState.MovingRightNormalPose
                        ? SamusState.FacingRightNormalPose
                        : SamusState.FacingLeftNormalPose;
            }

            if (GroundedSamusMovementEnabled)
            {
                if (LevelData is null)
                    throw new InvalidOperationException("Grounded Samus movement requires active room level data.");

                // $90:E725 dispatches movement type before animation. Every admitted pose
                // below has its own verified direction/mode path; a newly reachable pose
                // cannot accidentally inherit generic standing or running physics.
                LastGroundedSamusMovement = Samus.Pose switch
                {
                    SamusState.FacingRightNormalPose => SamusGroundedMovement.StepStandingRight(
                        _addressSpace,
                        LevelData,
                        Samus,
                        NmiFrameCounter),
                    SamusState.MovingRightNormalPose => SamusGroundedMovement.StepRunningRight(
                        _addressSpace,
                        LevelData,
                        Samus,
                        NmiFrameCounter),
                    SamusState.FacingLeftNormalPose => SamusGroundedMovement.StepStandingLeft(
                        _addressSpace,
                        LevelData,
                        Samus,
                        NmiFrameCounter),
                    SamusState.MovingLeftNormalPose => SamusGroundedMovement.StepRunningLeft(
                        _addressSpace,
                        LevelData,
                        Samus,
                        NmiFrameCounter),
                    SamusState.TurningRightToLeftPose or SamusState.TurningLeftToRightPose =>
                        SamusGroundedMovement.StepTurningOnGround(
                            _addressSpace,
                            LevelData,
                            Samus,
                            NmiFrameCounter),
                    _ => throw new NotSupportedException(
                        $"Grounded runtime movement is not translated for pose ${Samus.Pose:X2}."),
                };
            }

            // Normal gameplay advances animation during frame-handler beta, before the
            // pose-transition handler, draw handler, and next-NMI tile selection.
            Samus.AnimateNoFx(_addressSpace);

            if (GroundedSamusMovementEnabled)
            {
                // Command $F8's command-three “super-special” transition wins at this seam.
                // $25/$26 publish $02/$01 from their ROM byte streams without rerunning the
                // ordinary input transition selected earlier in the frame.
                bool animationTransitionApplied =
                    Samus.ApplyPendingGroundedAnimationTransition(_addressSpace);

                if (!animationTransitionApplied && ProspectiveSamusPose is { } inputTransition)
                {
                    byte targetPose = unchecked((byte)inputTransition.ProspectivePose);

                    // Self-transitions in the turn tables mean “remain in this animation.”
                    // Reinitializing frame zero would prevent $F8 from ever being reached.
                    if (targetPose != poseAtFrameStart)
                    {
                        switch ((poseAtFrameStart, targetPose))
                        {
                            case (SamusState.FacingRightNormalPose, SamusState.MovingRightNormalPose):
                                Samus.ApplyStandingRightToRunningRight(_addressSpace);
                                break;
                            case (SamusState.FacingLeftNormalPose, SamusState.MovingLeftNormalPose):
                                Samus.ApplyStandingLeftToRunningLeft(_addressSpace);
                                break;
                            case (SamusState.FacingRightNormalPose or SamusState.MovingRightNormalPose,
                                  SamusState.TurningRightToLeftPose):
                            case (SamusState.FacingLeftNormalPose or SamusState.MovingLeftNormalPose,
                                  SamusState.TurningLeftToRightPose):
                                Samus.ApplyGroundedTurn(_addressSpace, targetPose);
                                break;
                            default:
                                throw new NotSupportedException(
                                    $"Grounded input transition ${poseAtFrameStart:X2} -> ${targetPose:X2} matched ROM data but its side effects are not translated.");
                        }
                    }
                }
                else if (!animationTransitionApplied &&
                         poseAtFrameStart is SamusState.MovingRightNormalPose or SamusState.MovingLeftNormalPose &&
                         ProspectiveSamusFallbackPose == poseAtFrameStart)
                {
                    // Momentum routine one at $91:EC50 rechecks speed AFTER movement. Any
                    // residue selects mode two; collision or final underflow leaves zero.
                    Samus.HorizontalSpeed.AccelerationMode =
                        Samus.HorizontalSpeed.BaseFixed != 0 ? (ushort)2 : (ushort)0;
                }
                else if (!animationTransitionApplied &&
                         poseAtFrameStart == SamusState.MovingRightNormalPose &&
                         ProspectiveSamusFallbackPose == SamusState.FacingRightNormalPose)
                {
                    Samus.HorizontalSpeed.AccelerationMode = 0;
                    Samus.ApplyRunningRightToStandingRight(_addressSpace);
                }
                else if (!animationTransitionApplied &&
                         poseAtFrameStart == SamusState.MovingLeftNormalPose &&
                         ProspectiveSamusFallbackPose == SamusState.FacingLeftNormalPose)
                {
                    Samus.HorizontalSpeed.AccelerationMode = 0;
                    Samus.ApplyRunningLeftToStandingLeft(_addressSpace);
                }
            }

            if (GroundedSamusMovementEnabled)
            {
                if (LandingSiteEntry is null || LevelData is null || BackgroundStreamer is null)
                    throw new InvalidOperationException("Grounded camera tracking requires active Landing Site room state.");

                var currentCameraPoint = new SamusCameraPoint(
                    Samus.XPosition,
                    Samus.Kinematics.XSubposition,
                    Samus.YPosition,
                    Samus.Kinematics.YSubposition);

                // GameState_8 calls MainScrollingRoutine after movement/PLMs and before
                // DrawSamusEnemiesAndProjectiles. Pose metadata and camera/scroller values
                // are read from their literal ROM records, while ordinary grounded motion
                // has no knockback and uses the normal distance slot zero.
                Camera.TrackMovedSamusHorizontally(
                    previousCameraPoint,
                    currentCameraPoint,
                    new HorizontalCameraContext(
                        KnockbackDirection: 0,
                        MovementType: Samus.ReadMovementType(_addressSpace),
                        XAccelerationMode: Samus.HorizontalSpeed.AccelerationMode,
                        PoseXDirection: Samus.ReadPoseXDirection(_addressSpace),
                        CameraDistanceIndex: 0));
                Camera.TrackMovedSamusVertically(
                    previousCameraPoint,
                    currentCameraPoint,
                    new VerticalCameraContext(
                        YDirection: 0,
                        UpScroller: LandingSiteEntry.UpScroller,
                        DownScroller: LandingSiteEntry.DownScroller));

                // The camera can cross a 16-pixel boundary in the same main-loop pass.
                // Build and execute the exact row/column staging transfers now so both the
                // live PPU diagnostic and the following frame see the newly exposed edge.
                IReadOnlyList<BackgroundUpdateRequest> backgroundRequests =
                    UpdateBackgroundScrollingFromCamera();
                foreach (BackgroundUpdateRequest request in backgroundRequests)
                {
                    TilemapStreamUpdate update = BackgroundStreamer.Build(request)
                        ?? throw new InvalidOperationException("Landing Site unexpectedly entered Mode 7 streaming.");
                    update.ExecuteTo(Vram);
                }

                // UpdateMinimap belongs to Samus's normal frame-handler beta; it uses world
                // position, not camera position. Landing Site begins without Crateria's map
                // station, so only tiles actually visited by this debug run are revealed.
                Hud.UpdateMinimap(
                    _addressSpace,
                    LandingSiteEntry.AreaIndex,
                    LandingSiteEntry.RoomMapX,
                    LandingSiteEntry.RoomMapY,
                    LevelData.WidthInBlocks,
                    LevelData.HeightInBlocks,
                    Samus.XPosition,
                    Samus.YPosition,
                    NmiFrameCounter8,
                    hasAreaMap: false);
            }

            Samus.Draw(_addressSpace, Oam, Camera.XPosition, Camera.YPosition);
        }
        if (EscapeTimer.IsActive)
            EscapeTimerRenderer.Draw(EscapeTimer, Oam, _addressSpace);
        Oam.FinalizeFrame();

        // $80:9B44 appends the mutable three-row HUD upload during main-thread logic; it
        // becomes visible when the next accepted NMI drains this queue. The first frame's
        // initialization upload has already been consumed earlier in this StepFrame call.
        if (Hud.IsInitialized)
            Hud.QueueUpload(_addressSpace, VramWrites);

        // Landing Site's room main ASM appends four sky rows after ordinary gameplay logic;
        // they become visible when the following accepted NMI drains the queue.
        if (ScrollingSky is not null && Camera is not null)
            ScrollingSky.ProcessFrame(Camera.YPosition, timeIsFrozen: false, VramWrites);

        return Snapshot(escapeTimerExpired);
    }

    /// <summary>Runs the NMI handler portion currently translated from <c>$80:9583</c>.</summary>
    /// <param name="controller1Input">Host-provided raw SNES controller word.</param>
    /// <param name="mainLoopRequestedNmi">
    /// Equivalent of WRAM's NMI request flag. False models a lag NMI: it skips transfers,
    /// input, and accepted-frame counters but still advances the all-NMI counter.
    /// </param>
    public void RunNmi(ushort controller1Input, bool mainLoopRequestedNmi)
    {
        if (mainLoopRequestedNmi)
        {
            // Preserve $80:95A1 -> $80:95D0 -> $80:95E1 order: dedicated Samus graphics
            // DMA precedes the general video queue, and both precede controller latching.
            // $80:959E uploads the finalized main-loop OAM image immediately before that
            // Samus DMA; retain a second buffer so software rendering sees the same phase.
            DisplayedOam.CopyFinalizedFrom(Oam);
            Samus?.TileTransfers.TransferToVram(_addressSpace, Vram);
            VramWrites.DrainTo(Vram, _addressSpace);
            Controller1.Latch(controller1Input);

            NmiLagCounter = 0;
            NmiFrameCounter8 = unchecked((byte)(NmiFrameCounter8 + 1));
            NmiFrameCounter = unchecked((ushort)(NmiFrameCounter + 1));
        }
        else
        {
            NmiLagCounter = unchecked((ushort)(NmiLagCounter + 1));
            if (NmiLagCounter >= MaximumNmiLag)
                MaximumNmiLag = NmiLagCounter;
        }

        // $80:95F9 lies after the accepted/lagged branches rejoin, so it always advances.
        NmiCounterIncludingLag = unchecked((ushort)(NmiCounterIncludingLag + 1));
    }

    /// <summary>
    /// Invokes the pause-menu held-input filter with the controller sample latched by the
    /// most recent accepted NMI. The retail caller passes reset value three.
    /// </summary>
    public void UpdatePauseHeldInput(ushort timerReset = 3)
    {
        System.UpdateHeldInput(timerReset, Controller1.Current, Controller1.NewlyPressed);
    }

    private RuntimeFrameResult Snapshot(bool escapeTimerExpired) => new(
        NmiFrameCounter,
        Controller1.Current,
        Controller1.NewlyPressed,
        EscapeTimer.State,
        escapeTimerExpired);
}

/// <summary>
/// Small immutable return value for loggers and debugger watches after a frame step.
/// Mutable detail remains available on <see cref="SuperMetroidRuntime"/> itself.
/// </summary>
public readonly record struct RuntimeFrameResult(
    ushort FrameNumber,
    ushort ControllerInput,
    ushort ControllerNewInput,
    EscapeTimerState EscapeTimerState,
    bool EscapeTimerExpired);

/// <summary>
/// Auditable host placement chosen by <see cref="SuperMetroidRuntime.InitializeDebugGroundedSamus"/>.
/// </summary>
public readonly record struct DebugGroundedSamusPlacement(
    ushort XPosition,
    ushort YPosition,
    ushort DesiredScreenY,
    int BlockX,
    int BlockY,
    RoomCollisionBlock FloorBlock,
    byte FloorHeight);

/// <summary>Debugger-visible work performed by the room's force-blank tilemap fill.</summary>
public readonly record struct InitialViewportResult(int UpdateRequestCount, int DmaSegmentCount);
