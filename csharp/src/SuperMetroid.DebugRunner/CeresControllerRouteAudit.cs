using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Rendering;

/// <summary>
/// Drives a fresh file from the public frontend into Ceres and, incrementally, through the
/// complete station route. Unlike the focused actor audits, this class may observe the live
/// runtime to choose controller buttons but may never publish positions, room pointers,
/// progression bits, enemy state, or door collisions.
/// </summary>
internal static partial class CeresControllerRouteAudit
{
    /// <summary>
    /// Loads only the post-Ridley elevator room for rapid controller-policy iteration. This
    /// focused harness is allowed to seed its starting room/position and therefore never
    /// substitutes for <see cref="Run"/>; that full audit must still prove every prior room
    /// and the native frontend departure publication from a fresh file.
    /// </summary>
    public static int RunElevatorClimbFocus(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var runtime = new SuperMetroidRuntime(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RunNmi(controller1Input: 0, mainLoopRequestedNmi: true);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.System.SetBossBits(areaIndex: 6, bits: BossBits.AreaBoss);
        runtime.LoadCartridgeRoomForDebug(roomPointer: 0xdf45, cameraY: 0x0200);
        runtime.ActiveSamusMode7Transform = new SamusMode7Transform(
            MatrixA: 0x0100,
            MatrixB: 0,
            MatrixC: 0,
            CenterX: 0x0080,
            CenterY: 0x03f0);
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Focused Ceres elevator load did not retain Samus.");
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = 0x00d8;
        samus.YPosition = 0x028b;
        samus.Kinematics.YDirection = 0;
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.InputLocked = false;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        runtime.Enemies.CeresStatus = 0x8000;
        var host = new EarlyControllerRouteAudit.ControllerRouteHost(
            StepFrame: input => runtime.StepFrame(input),
            LoadPendingDoor: () => runtime.LoadPendingDoorDestination());
        int frames = DriveCeresElevatorClimb(
            bus,
            game: null,
            runtime,
            host,
            maximumFrames: 7200);
        Console.WriteLine(
            $"Focused Ceres elevator controller policy reached the native trigger bounds " +
            $"in {frames} frames at Samus=(${samus.XPosition:X4},${samus.YPosition:X4}).");
        return 0;
    }

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var game = new SuperMetroidGame(
            bus,
            new SuperMetroidGameOptions { SkipOpeningCinematic = true });
        FrontendFrame frame = EnterFreshCeres(game);
        SuperMetroidRuntime runtime = game.RuntimeForVerification
            ?? throw new InvalidOperationException("Fresh frontend did not construct Ceres gameplay.");
        var host = EarlyControllerRouteAudit.CreateFrontendHost(game);

        DescribeRoom(bus, runtime, "Ceres elevator");
        int elevatorFrames = DriveAlternatingDescentUntilDoor(
            runtime,
            host,
            maximumFrames: 2400,
            "Ceres elevator -> falling-tile room");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xab4c, expectedRoomPointer: 0xdf8d);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres falling-tile room");

        int fallingTileFrames = DriveHorizontalUntilDoor(
            runtime,
            host,
            SnesButton.Right,
            maximumFrames: 2400,
            "Ceres falling-tile room -> next station room");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xab64, expectedRoomPointer: 0xdfd7);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres station room $DFD7");

        int stationDescentFrames = DriveAlternatingDescentUntilDoor(
            runtime,
            host,
            maximumFrames: 2400,
            "Ceres station room $DFD7 -> $E021");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xab7c, expectedRoomPointer: 0xe021);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres station room $E021");

        int computerRoomFrames = DriveHorizontalUntilDoor(
            runtime,
            host,
            SnesButton.Right,
            maximumFrames: 2400,
            "Ceres station room $E021 -> $E06B");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xab94, expectedRoomPointer: 0xe06b);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres station room $E06B");

        int ridleyApproachFrames = DriveHorizontalUntilDoor(
            runtime,
            host,
            SnesButton.Right,
            maximumFrames: 2400,
            "Ceres station room $E06B -> Ridley");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xabac, expectedRoomPointer: 0xe0b5);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres Ridley room");

        RidleyBattleResult ridleyBattle = DriveRidleyBattle(
            bus,
            runtime,
            host,
            maximumFrames: 12000);
        int escapePresentationFrames = DriveRidleyEscapePresentation(
            runtime,
            host,
            maximumFrames: 3600);
        EarlyControllerRouteAudit.AssertCommonEnemyProjectilePalette(
            bus,
            runtime,
            "Ceres escape timer/steam");
        if (!runtime.System.HasAnyBossBits(runtime.ActiveRoom!.AreaIndex, BossBits.AreaBoss))
        {
            throw new InvalidDataException(
                "Ceres escape presentation did not set the area-boss bit used by red haze.");
        }
        int ridleyExitFrames = DriveLeftDoorFromWall(
            runtime,
            host,
            maximumFrames: 2400,
            "Ceres Ridley escape -> $E06B");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xabb8, expectedRoomPointer: 0xe06b);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres escape room $E06B");

        int escapeHallFrames = DriveHorizontalUntilDoor(
            runtime,
            host,
            SnesButton.Left,
            maximumFrames: 2400,
            "Ceres escape room $E06B -> $E021");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xaba0, expectedRoomPointer: 0xe021);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres escape room $E021");

        int computerReturnFrames = DriveHorizontalUntilDoor(
            runtime,
            host,
            SnesButton.Left,
            maximumFrames: 2400,
            "Ceres escape room $E021 -> $DFD7");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xab88, expectedRoomPointer: 0xdfd7);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres escape shaft $DFD7");

        int stationAscentFrames = DriveCeresAscentUntilDoor(
            runtime,
            host,
            maximumFrames: 4800,
            "Ceres escape shaft $DFD7 -> $DF8D");
        AssertPendingDoor(runtime, expectedDoorPointer: 0xab70, expectedRoomPointer: 0xdf8d);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres escape room $DF8D");

        var fallingDebrisSpawns = new List<(int Frame, ushort X, RoomEnemyProjectileKind Kind)>();
        var debrisSlotWasActive = new bool[runtime.Enemies.EnemyProjectiles.Count];
        for (int slotIndex = 0; slotIndex < runtime.Enemies.EnemyProjectiles.Count; slotIndex++)
        {
            RoomEnemyProjectileSlot projectile = runtime.Enemies.EnemyProjectiles[slotIndex];
            debrisSlotWasActive[slotIndex] = projectile.IsActive &&
                projectile.Kind is RoomEnemyProjectileKind.CeresFallingDebrisLight or
                    RoomEnemyProjectileKind.CeresFallingDebrisDark;
        }
        int finalReturnHallFrames = DriveHorizontalUntilDoor(
            runtime,
            host,
            SnesButton.Left,
            maximumFrames: 2400,
            "Ceres escape room $DF8D -> elevator shaft",
            afterFrame: frame =>
            {
                for (int slotIndex = 0;
                    slotIndex < runtime.Enemies.EnemyProjectiles.Count;
                    slotIndex++)
                {
                    RoomEnemyProjectileSlot projectile =
                        runtime.Enemies.EnemyProjectiles[slotIndex];
                    bool isDebris = projectile.IsActive &&
                        projectile.Kind is RoomEnemyProjectileKind.CeresFallingDebrisLight or
                            RoomEnemyProjectileKind.CeresFallingDebrisDark;
                    if (isDebris && !debrisSlotWasActive[slotIndex])
                    {
                        fallingDebrisSpawns.Add((frame, projectile.XPosition, projectile.Kind));
                    }
                    debrisSlotWasActive[slotIndex] = isDebris;
                }
            });
        if (fallingDebrisSpawns.Count < 3)
        {
            throw new InvalidDataException(
                $"Ceres falling-tile room main exposed only {fallingDebrisSpawns.Count} " +
                "bank-$86 debris spawns during escape.");
        }
        if (fallingDebrisSpawns.Select(spawn => spawn.X).Distinct().Count() < 2)
        {
            throw new InvalidDataException(
                $"All {fallingDebrisSpawns.Count} Ceres debris actors spawned from " +
                $"the same ceiling X=${fallingDebrisSpawns[0].X:X4}.");
        }
        for (int index = 1; index < fallingDebrisSpawns.Count; index++)
        {
            int gap = fallingDebrisSpawns[index].Frame - fallingDebrisSpawns[index - 1].Frame;
            // A full projectile pool may reject a native allocation, but every observed
            // allocation must still land on the room-main timer's exact nine-call cadence.
            if (gap <= 0 || gap % 9 != 0)
            {
                throw new InvalidDataException(
                    $"Ceres debris spawn frames {fallingDebrisSpawns[index - 1].Frame} and " +
                    $"{fallingDebrisSpawns[index].Frame} are {gap} calls apart, not a " +
                    "multiple of `$8F:E525`'s nine-call cadence.");
            }
        }
        AssertPendingDoor(runtime, expectedDoorPointer: 0xab58, expectedRoomPointer: 0xdf45);
        host.LoadPendingDoor();
        DescribeRoom(bus, runtime, "Ceres escape elevator shaft $DF45");

        int elevatorClimbFrames = DriveCeresElevatorClimb(
            bus,
            game,
            runtime,
            host,
            maximumFrames: 7200);
        int destructionFrames = DriveCeresDestructionToLandingSite(
            game,
            runtime,
            host,
            maximumFrames: 4800);

        Console.WriteLine(
            $"Fresh frontend Ceres route crossed the elevator room in {elevatorFrames} " +
            $"controller frames, the falling-tile room in {fallingTileFrames}, and the " +
            $"second descent in {stationDescentFrames}; the next hall took " +
            $"{computerRoomFrames} and Ridley's approach took {ridleyApproachFrames}, reaching " +
            $"$8F:{runtime.ActiveRoom!.Pointer:X4}. Ridley retreated after " +
            $"{ridleyBattle.Frames} battle frames/{ridleyBattle.FireEdges} fire edges; Mode 7 " +
            $"and the escape warning took {escapePresentationFrames}, and the first return " +
            $"door took {ridleyExitFrames} through the production door dispatcher. The two " +
            $"return halls took {escapeHallFrames}/{computerReturnFrames} frames and the " +
            $"controller-only shaft ascent took {stationAscentFrames}, reaching " +
            $"the last hall; it reached the elevator shaft after another " +
            $"{finalReturnHallFrames} frames. The final shaft took {elevatorClimbFrames} " +
            $"controller frames and the destruction/Zebes frontend handoff took " +
            $"{destructionFrames} frames.");

        // Continue against this exact runtime and SRAM-bearing frontend instance. This is
        // the decisive end-to-end seam: the established Landing Site-to-Bombs route is no
        // longer allowed to begin from a debug-created room after this Ceres audit succeeds.
        return EarlyControllerRouteAudit.RunLoadedRoute(bus, runtime, host);
    }

    /// <summary>
    /// Reproduces the desktop host's ordinary new-slot choices, with only the user-configured
    /// story skip enabled. That option joins state $1F; it does not bypass the Ceres room,
    /// elevator actor, initial SRAM checkpoint, or movement-unlock sequence.
    /// </summary>
    private static FrontendFrame EnterFreshCeres(SuperMetroidGame game)
    {
        FrontendFrame frame = game.Step(0);
        frame = game.Step((ushort)SnesButton.Start);
        frame = FrontendAuditDriver.StepUntil(
            game,
            frame,
            candidate => candidate.Phase == nameof(TitleSequencePhase.TitleScreen),
            maximumFrames: 120,
            "title montage skip did not reach title");
        frame = game.Step((ushort)SnesButton.Start);
        frame = FrontendAuditDriver.StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.FileSelectMenus,
            maximumFrames: 120,
            "title did not reach file select");
        for (int index = 0; index < 16; index++)
            frame = game.Step(0);
        frame = game.Step((ushort)SnesButton.A);
        frame = FrontendAuditDriver.StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.GameOptionsMenu,
            maximumFrames: 180,
            "fresh slot did not reach options");
        for (int index = 0; index < 16; index++)
            frame = game.Step(0);
        frame = game.Step((ushort)SnesButton.A);
        frame = FrontendAuditDriver.StepUntil(
            game,
            frame,
            candidate => candidate.GameState == SuperMetroidGameState.MainGameplay,
            maximumFrames: 360,
            "Ceres elevator did not restore gameplay");
        return frame;
    }

    /// <summary>
    /// Holds an ordinary horizontal direction, supplies discrete beam edges for blue caps,
    /// and periodically jumps only after collision has stopped both coordinates. This is a
    /// controller policy, not a route shortcut: gravity, platforms, terrain, and type-$9
    /// publication all remain owned by the runtime.
    /// </summary>
    private static int DriveHorizontalUntilDoor(
        SuperMetroidRuntime runtime,
        EarlyControllerRouteAudit.ControllerRouteHost host,
        SnesButton direction,
        int maximumFrames,
        string segment,
        bool allowObstacleJump = true,
        Action<int>? afterFrame = null)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            $"{segment} began without Samus.");
        ushort previousX = samus.XPosition;
        ushort previousY = samus.YPosition;
        int stationaryFrames = 0;
        int jumpHoldFrames = 0;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            ushort input = (ushort)direction;
            if (frame % 24 == 0)
                input |= (ushort)SnesButton.X;
            if (allowObstacleJump && stationaryFrames == 24 && jumpHoldFrames == 0)
                jumpHoldFrames = 40;
            if (jumpHoldFrames != 0)
            {
                input |= (ushort)SnesButton.A;
                jumpHoldFrames--;
            }
            host.StepFrame(input);
            afterFrame?.Invoke(frame);
            if (runtime.HasPendingDoorTransition)
                return frame + 1;

            if (samus.XPosition == previousX && samus.YPosition == previousY)
                stationaryFrames++;
            else
                stationaryFrames = 0;
            previousX = samus.XPosition;
            previousY = samus.YPosition;
        }

        WriteFailureFrame(runtime, segment);
        throw new InvalidDataException(
            $"{segment} produced no door after {maximumFrames} controller frames; " +
            $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), pose=${samus.Pose:X2}.");
    }

    /// <summary>
    /// Opens a left cap after Ridley's forced ejection has deposited Samus against it. The
    /// muzzle cannot create a useful travelling projectile while embedded in the cap, so a
    /// player first takes a few steps right, turns left, fires discrete beam edges from the
    /// room, and only then walks into the opened type-$9 cells.
    /// </summary>
    private static int DriveLeftDoorFromWall(
        SuperMetroidRuntime runtime,
        EarlyControllerRouteAudit.ControllerRouteHost host,
        int maximumFrames,
        string segment)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            $"{segment} began without Samus.");
        bool reachedFiringLane = false;
        int firingFrames = 0;
        int obstacleJumpFramesRemaining = 0;
        bool obstacleJumpStarted = false;
        ushort minimumX = samus.XPosition;
        ushort minimumY = samus.YPosition;
        ushort jumpStartX = 0;
        ushort jumpStartY = 0;
        byte jumpStartPose = 0;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            ushort input;
            if (!reachedFiringLane)
            {
                input = (ushort)SnesButton.Right;
                reachedFiringLane = samus.XPosition >= 0x0048;
            }
            else if (firingFrames < 120)
            {
                // One direction frame turns the body; released locomotion thereafter keeps
                // the muzzle in the lane instead of walking back into the closed cap.
                input = firingFrames == 0 ? (ushort)SnesButton.Left : (ushort)0;
                if (firingFrames % 12 == 2)
                    input |= (ushort)SnesButton.X;
                firingFrames++;
            }
            else
            {
                input = (ushort)SnesButton.Left;
                // Ridley's destroyed-room overlay leaves the native three-block pedestal
                // between the firing lane and the cap. One full-height leftward jump clears
                // it; permanently enabling the generic retry jump would instead wall-jump
                // up the cap and strand Samus near the ceiling.
                if (!obstacleJumpStarted && samus.XPosition <= 0x0040)
                {
                    obstacleJumpStarted = true;
                    jumpStartX = samus.XPosition;
                    jumpStartY = samus.YPosition;
                    jumpStartPose = samus.Pose;
                    // The twelve-frame trial never raised Samus's lower collision boundary
                    // over the two-block pedestal; a full 36-frame hold crossed it but then
                    // climbed onto the tall cap at world Y=$004A. Twenty frames preserve a
                    // normal variable-height jump between those two observed bounds.
                    obstacleJumpFramesRemaining = 20;
                }
                if (obstacleJumpFramesRemaining != 0)
                {
                    input |= (ushort)SnesButton.A;
                    obstacleJumpFramesRemaining--;
                }
                if (frame % 24 == 0)
                    input |= (ushort)SnesButton.X;
            }

            host.StepFrame(input);
            minimumX = Math.Min(minimumX, samus.XPosition);
            minimumY = Math.Min(minimumY, samus.YPosition);
            if (runtime.HasPendingDoorTransition)
                return frame + 1;
        }

        WriteFailureFrame(runtime, segment);
        throw new InvalidDataException(
            $"{segment} produced no door after {maximumFrames} controller frames; " +
            $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), pose=${samus.Pose:X2}, " +
            $"minimum=(${minimumX:X4},${minimumY:X4}), " +
            $"jumpStart=(${jumpStartX:X4},${jumpStartY:X4})/${jumpStartPose:X2}, " +
            $"colored=[{string.Join(' ', runtime.Plms.ColoredDoors)}], " +
            $"leftBlocks=[{DescribeLeftCollision(runtime.LevelData)}].");
    }

    /// <summary>
    /// Formats the live four-column door approach after a controller-route failure. Each
    /// cell is collision type followed by BTS, grouped by row; this records PLM-authored
    /// terrain mutations without changing or reloading the room under diagnosis.
    /// </summary>
    private static string DescribeLeftCollision(RoomLevelData? level)
    {
        if (level is null)
            return "no-level";
        var rows = new List<string>();
        for (int y = 4; y < Math.Min(10, level.HeightInBlocks); y++)
        {
            var cells = new List<string>();
            for (int x = 0; x < Math.Min(4, level.WidthInBlocks); x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                cells.Add($"{(byte)block.CollisionType:X1}{block.Behavior:X2}");
            }
            rows.Add($"y{y:X1}:{string.Join('/', cells)}");
        }
        return string.Join(' ', rows);
    }

    /// <summary>
    /// Descends the station's alternating ledges exactly as a player does: walk off one
    /// shelf, let gravity choose the next support, then reverse after the far wall stops X.
    /// Jumping here is actively wrong because it can keep Samus above the next walk-off lip.
    /// </summary>
    private static int DriveAlternatingDescentUntilDoor(
        SuperMetroidRuntime runtime,
        EarlyControllerRouteAudit.ControllerRouteHost host,
        int maximumFrames,
        string segment)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            $"{segment} began without Samus.");
        SnesButton direction = SnesButton.Right;
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            $"{segment} began without level data.");
        (int X, int Y) targetDoor = FindLowestDoorBlock(level);
        SnesButton finalDirection = targetDoor.X >= level.WidthInBlocks / 2
            ? SnesButton.Right
            : SnesButton.Left;
        ushort previousX = samus.XPosition;
        int horizontallyStationaryFrames = 0;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            // Once the bottom floor is reached, the only bank-$83 exit is the right cap;
            // the preceding zig-zag direction is no longer a meaningful ledge target.
            if (samus.YPosition >= Math.Max(0, targetDoor.Y * 16 - 32))
            {
                direction = finalDirection;
            }
            else if (horizontallyStationaryFrames == 20)
            {
                direction = direction == SnesButton.Right
                    ? SnesButton.Left
                    : SnesButton.Right;
            }
            ushort input = (ushort)direction;
            if (frame % 24 == 0)
                input |= (ushort)SnesButton.X;
            host.StepFrame(input);
            if (runtime.HasPendingDoorTransition)
                return frame + 1;

            if (samus.XPosition == previousX)
                horizontallyStationaryFrames++;
            else
                horizontallyStationaryFrames = 0;
            previousX = samus.XPosition;
        }

        throw new InvalidDataException(
            $"{segment} produced no door after {maximumFrames} controller frames; " +
            $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), pose=${samus.Pose:X2}.");
    }

    /// <summary>
    /// Climbs room $DFD7 by following its three cartridge-authored openings. The lower
    /// rubble funnels Samus to the left opening, the middle shelf can only be passed at its
    /// right edge, and the upper shelf leads back to the left door. Jump is deliberately
    /// pulsed so every landing produces a new controller edge; no pose, speed, or coordinate
    /// is injected by the audit.
    /// </summary>
    private static int DriveCeresAscentUntilDoor(
        SuperMetroidRuntime runtime,
        EarlyControllerRouteAudit.ControllerRouteHost host,
        int maximumFrames,
        string segment)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            $"{segment} began without Samus.");
        ushort minimumY = samus.YPosition;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            // These boundaries are the centers of the cartridge's solid rows, expressed in
            // ordinary room-space pixels. They select a visible platform target only; normal
            // acceleration, jump arcs, slopes, and collision still decide whether it works.
            SnesButton direction = samus.YPosition switch
            {
                >= 0x0130 => SnesButton.Left,
                >= 0x00A8 => SnesButton.Right,
                _ => SnesButton.Left,
            };
            ushort input = (ushort)direction;
            int jumpPhase = frame % 40;
            if (jumpPhase < 30)
                input |= (ushort)SnesButton.A;
            if (frame % 24 == 0)
                input |= (ushort)SnesButton.X;

            host.StepFrame(input);
            minimumY = Math.Min(minimumY, samus.YPosition);
            if (runtime.HasPendingDoorTransition)
                return frame + 1;
            if (frame % 600 == 599)
            {
                Console.WriteLine(
                    $"  Ceres ascent f{frame + 1}: Samus=(${samus.XPosition:X4}," +
                    $"${samus.YPosition:X4})/${samus.Pose:X2}, minimumY=${minimumY:X4}.");
            }
        }

        WriteFailureFrame(runtime, segment);
        throw new InvalidDataException(
            $"{segment} produced no door after {maximumFrames} controller frames; " +
            $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4}), pose=${samus.Pose:X2}, " +
            $"minimumY=${minimumY:X4}.");
    }

    /// <summary>
    /// Climbs Ceres's final elevator shaft and stops only when room main <c>$89:ACC3</c>
    /// publishes native state <c>$20</c>. The ordinary path uses each cartridge ledge and
    /// its necessary run-up; a missed transfer may use the game's own wall-jump input as a
    /// recovery. The runtime owns every probe, launch, landing, and final trigger test.
    /// </summary>
    private static int DriveCeresElevatorClimb(
        ISnesAddressSpace bus,
        SuperMetroidGame? game,
        SuperMetroidRuntime runtime,
        EarlyControllerRouteAudit.ControllerRouteHost host,
        int maximumFrames)
    {
        SamusState samus = runtime.Samus ?? throw new InvalidOperationException(
            "Ceres elevator climb began without Samus.");
        int stage = 0;
        int runUpFrames = 0;
        bool wasAirborne = false;
        bool jumpWasPressed = false;
        bool preparingWallJump = false;
        SnesButton wallJumpDirection = SnesButton.Left;
        byte lastLaunchPose = samus.Pose;
        ushort lastLaunchX = samus.XPosition;
        ushort lastLaunchY = samus.YPosition;
        ushort stageMinimumY = samus.YPosition;
        ushort minimumY = samus.YPosition;
        ushort stageOneMaximumX = 0;
        ushort stageOneMaximumWallAnimation = 0;
        int wallJumpPreparations = 0;
        int wallJumpLaunches = 0;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            if (stage == 1)
            {
                stageOneMaximumX = Math.Max(stageOneMaximumX, samus.XPosition);
                if (samus.XPosition >= 0x00c0)
                {
                    stageOneMaximumWallAnimation = Math.Max(
                        stageOneMaximumWallAnimation,
                        samus.AnimationFrame);
                }
            }
            bool grounded = samus.Kinematics.YDirection == 0;
            ushort input;
            if (grounded)
            {
                preparingWallJump = false;
                // The full-width recovery floor lies below the right entrance ledge. Jump
                // right to regain that ledge; every higher support launches toward the
                // opposite shaft wall, where an ordinary controller wall jump can extend
                // the otherwise 4.E000-pixel cartridge arc to the next platform.
                bool recoveringFromFloor = samus.YPosition >= 0x0290;
                bool positioningFirstWallJump = stage == 1 && samus.XPosition > 0x0038;
                SnesButton direction = recoveringFromFloor
                    ? SnesButton.Right
                    : positioningFirstWallJump
                        ? SnesButton.Left
                    : samus.XPosition < 0x0080
                        ? SnesButton.Right
                        : SnesButton.Left;
                input = (ushort)(SnesButton.B | direction);

                if (wasAirborne || jumpWasPressed || positioningFirstWallJump)
                    runUpFrames = 0;
                SamusMovementType movement = samus.ReadMovementKind(bus);
                if (!jumpWasPressed &&
                    !positioningFirstWallJump &&
                    (recoveringFromFloor ||
                        runUpFrames >= 2 && movement == SamusMovementType.Running))
                {
                    input |= (ushort)SnesButton.A;
                }
                else
                {
                    runUpFrames++;
                }
            }
            else
            {
                bool nearLeftWall = samus.XPosition <= 0x002e;
                bool nearRightWall = samus.XPosition >= 0x00d2;
                if (stage > 0 &&
                    !preparingWallJump &&
                    samus.ReadMovementKind(bus) == SamusMovementType.SpinJumping &&
                    samus.AnimationFrame >= 0x0b &&
                    (nearLeftWall || nearRightWall))
                {
                    // Native `$90:9D35` requires the direction away from the contacted wall
                    // plus a *new* Jump edge after spin frame $0B. Release A for at least
                    // this frame and remember which wall owns the following edge.
                    preparingWallJump = true;
                    wallJumpPreparations++;
                    wallJumpDirection = nearLeftWall
                        ? SnesButton.Right
                        : SnesButton.Left;
                    input = (ushort)(SnesButton.B | wallJumpDirection);
                }
                else if (preparingWallJump)
                {
                    input = (ushort)(SnesButton.B | wallJumpDirection);
                    if (!jumpWasPressed && samus.AnimationFrame >= 0x0b)
                        input |= (ushort)SnesButton.A;
                }
                else
                {
                    // Preserve the current spin/wall-jump facing direction until the next
                    // side probe. This keeps the full variable-height arc and lets normal
                    // horizontal collision—not a waypoint teleport—deliver Samus to a wall.
                    if (stage == 0 && samus.XPosition <= 0x0058)
                    {
                        // The first authored platform ends at X=$6F. Release horizontal
                        // input once Samus's centre is safely over it so native spin
                        // deceleration drops her onto the platform instead of the left wall.
                        input = (ushort)SnesButton.A;
                    }
                    else
                    {
                        SnesButton direction = SamusState.IsFacingLeft(bus, samus.Pose)
                            ? SnesButton.Left
                            : SnesButton.Right;
                        input = (ushort)(SnesButton.B | direction | SnesButton.A);
                    }
                }
            }

            bool jumpEdge = (input & (ushort)SnesButton.A) != 0 && !jumpWasPressed;
            host.StepFrame(input);
            jumpWasPressed = (input & (ushort)SnesButton.A) != 0;
            wasAirborne = samus.Kinematics.YDirection != 0;
            if (preparingWallJump && SamusState.IsWallJumpPose(samus.Pose))
            {
                preparingWallJump = false;
                wallJumpLaunches++;
            }
            if (jumpEdge)
            {
                lastLaunchPose = samus.Pose;
                lastLaunchX = samus.XPosition;
                lastLaunchY = samus.YPosition;
            }
            minimumY = Math.Min(minimumY, samus.YPosition);
            stageMinimumY = Math.Min(stageMinimumY, samus.YPosition);
            if (runtime.LastCeresElevatorShaftRoomMain.DepartureRequestedThisFrame)
                return frame + 1;
            if (game is not null && game.GameState != SuperMetroidGameState.MainGameplay)
            {
                throw new InvalidDataException(
                    $"Ceres elevator climb left gameplay through {game.GameState} " +
                    "without the cartridge room-main departure publication.");
            }
            if (game is null &&
                samus.XPosition > 112 &&
                samus.XPosition <= 144 &&
                samus.YPosition >= 75 &&
                samus.YPosition < 128 &&
                samus.Kinematics.YSpeed == 0 &&
                samus.Kinematics.YSubspeed == 0)
            {
                return frame + 1;
            }

            if (samus.Kinematics.YDirection == 0)
            {
                // A missed transfer can fall several shelves. Recompute the next target
                // from the support actually reached, so the audit retries the physical
                // route from there rather than steering toward an inaccessible upper ledge.
                int groundedStage = samus.YPosition switch
                {
                    <= 0x0053 => 6,
                    <= 0x00b3 => 5,
                    <= 0x0113 => 4,
                    <= 0x0173 => 3,
                    <= 0x01e3 => 2,
                    <= 0x0243 => 1,
                    _ => 0,
                };
                if (groundedStage != stage)
                {
                    stage = groundedStage;
                    runUpFrames = 0;
                    stageMinimumY = samus.YPosition;
                }
            }
            if (frame % 600 == 599)
            {
                Console.WriteLine(
                    $"  Ceres elevator climb f{frame + 1}: stage={stage}, " +
                    $"Samus=(${samus.XPosition:X4},${samus.YPosition:X4})/${samus.Pose:X2}, " +
                    $"minimumY=${minimumY:X4}/${stageMinimumY:X4}, last launch=" +
                    $"${lastLaunchPose:X2}@(${lastLaunchX:X4},${lastLaunchY:X4}), " +
                    $"wall={wallJumpLaunches}/{wallJumpPreparations}/" +
                    $"maxX${stageOneMaximumX:X4}/anim${stageOneMaximumWallAnimation:X2}, " +
                    $"status=${runtime.Enemies.CeresStatus:X4}.");
            }
        }

        WriteFailureFrame(runtime, "Ceres final elevator climb");
        throw new InvalidDataException(
            $"Ceres elevator did not publish departure after {maximumFrames} frames; " +
            $"stage={stage}, Samus=(${samus.XPosition:X4},${samus.YPosition:X4})/" +
            $"${samus.Pose:X2}, minimumY=${minimumY:X4}, " +
            $"status=${runtime.Enemies.CeresStatus:X4}.");
    }

    /// <summary>
    /// Advances the actual outer dispatcher through state $20's hold, state $21's fade,
    /// the Ceres explosion/Planet Zebes sequence, station-eighteen loading, and gameplay
    /// fade-in. No button skips the cinematic; completion requires a live Crateria room.
    /// </summary>
    private static int DriveCeresDestructionToLandingSite(
        SuperMetroidGame game,
        SuperMetroidRuntime runtime,
        EarlyControllerRouteAudit.ControllerRouteHost host,
        int maximumFrames)
    {
        bool sawBlackout = false;
        bool sawDestruction = false;
        bool sawLandingLoad = false;
        for (int frame = 0; frame < maximumFrames; frame++)
        {
            host.StepFrame(0);
            sawBlackout |= game.GameState == SuperMetroidGameState.BlackoutFromCeres;
            sawDestruction |= game.GameState == SuperMetroidGameState.CeresGoesBoom;
            sawLandingLoad |= game.GameState is SuperMetroidGameState.LoadingGameData or
                SuperMetroidGameState.MainGameplayFadeIn;
            if (game.GameState == SuperMetroidGameState.MainGameplay &&
                runtime.ActiveRoom?.AreaIndex == 0)
            {
                if (!sawBlackout || !sawDestruction || !sawLandingLoad)
                {
                    throw new InvalidDataException(
                        $"Ceres-to-Zebes handoff skipped a frontend owner: blackout={sawBlackout}, " +
                        $"destruction={sawDestruction}, load={sawLandingLoad}.");
                }
                return frame + 1;
            }
        }

        throw new InvalidDataException(
            $"Ceres destruction did not reach Landing Site after {maximumFrames} frames; " +
            $"frontend={game.GameState}, room=$8F:{runtime.ActiveRoom?.Pointer ?? 0:X4}, " +
            $"blackout={sawBlackout}, destruction={sawDestruction}, load={sawLandingLoad}.");
    }

    /// <summary>
    /// Selects the lowest type-$9 cell in a vertical Ceres room. Repeated cells belonging
    /// to the same four-tile door are harmless; only its side and approach row are needed
    /// to choose D-pad input near the floor.
    /// </summary>
    private static (int X, int Y) FindLowestDoorBlock(RoomLevelData level)
    {
        (int X, int Y)? selected = null;
        for (int y = 0; y < level.HeightInBlocks; y++)
        {
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                if (level.GetCollisionBlock(x, y).CollisionType == RoomCollisionType.DoorBlock &&
                    (selected is null || y > selected.Value.Y))
                {
                    selected = (x, y);
                }
            }
        }
        return selected ?? throw new InvalidDataException(
            "Vertical Ceres route contains no type-$9 exit.");
    }

    private static void AssertPendingDoor(
        SuperMetroidRuntime runtime,
        ushort expectedDoorPointer,
        ushort expectedRoomPointer)
    {
        CartridgeDoorHeader door = runtime.LevelData?.PendingDoorTransition
            ?? throw new InvalidDataException("Ceres route ended without a pending door.");
        if (door.Pointer != expectedDoorPointer ||
            door.DestinationRoomPointer != expectedRoomPointer)
        {
            throw new InvalidDataException(
                $"Ceres route selected $83:{door.Pointer:X4}/$8F:{door.DestinationRoomPointer:X4}, " +
                $"expected $83:{expectedDoorPointer:X4}/$8F:{expectedRoomPointer:X4}.");
        }
    }

    /// <summary>
    /// Preserves the exact failed PPU-visible frame for route diagnosis. This is output-only
    /// instrumentation; rendering does not step or repair the live runtime.
    /// </summary>
    private static void WriteFailureFrame(SuperMetroidRuntime runtime, string segment)
    {
        string directory = Path.Combine("csharp", "test-temp", "ceres-controller");
        Directory.CreateDirectory(directory);
        string safeName = string.Concat(segment.Select(character =>
            char.IsLetterOrDigit(character) ? character : '-'));
        string path = Path.Combine(directory, $"{safeName}.png");
        PngWriter.WriteRgba(
            path,
            FrontendFrame.Width,
            FrontendFrame.Height,
            SuperMetroidRuntimeFrameRenderer.Render(runtime));
        Console.WriteLine($"  Wrote failed Ceres route frame to {Path.GetFullPath(path)}.");
    }

    private static void DescribeRoom(
        ISnesAddressSpace bus,
        SuperMetroidRuntime runtime,
        string name)
    {
        RoomLevelData level = runtime.LevelData ?? throw new InvalidOperationException(
            $"{name} has no level data.");
        var doors = new List<string>();
        for (int y = 0; y < level.HeightInBlocks; y++)
        {
            for (int x = 0; x < level.WidthInBlocks; x++)
            {
                RoomCollisionBlock block = level.GetCollisionBlock(x, y);
                if (block.CollisionType != RoomCollisionType.DoorBlock)
                    continue;
                CartridgeDoorHeader door = level.ResolveDoorCollision(
                    bus,
                    block.Behavior,
                    runtime.Samus?.Pose ?? 0,
                    publishDoorSideEffects: false);
                doors.Add(
                    $"({x:X2},{y:X2})->$83:{door.Pointer:X4}/$8F:{door.DestinationRoomPointer:X4}");
            }
        }
        Console.WriteLine(
            $"  {name}: room=$8F:{runtime.ActiveRoom!.Pointer:X4}, " +
            $"Samus=(${runtime.Samus!.XPosition:X4},${runtime.Samus.YPosition:X4}), " +
            $"size={level.WidthInBlocks:X2}x{level.HeightInBlocks:X2}, " +
            $"doors=[{string.Join(' ', doors)}].");
    }

}
