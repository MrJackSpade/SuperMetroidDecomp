using System.Security.Cryptography;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using Audio = SuperMetroid.Core.Audio;

/// <summary>
/// Headless consumer for desktop <c>.smrec</c> files. It drives the same public frontend
/// dispatcher as the window and retains a compact tail of cartridge-owned state so a user
/// report can be diagnosed from the exact controller history rather than reconstructed.
/// </summary>
internal static class InputReplayAudit
{
    // A failed desktop session normally ends within a few seconds of the condition the
    // player is reporting.  Keeping the final second of raw states makes the console
    // output small enough to survive the test harness while still showing held/new
    // input and every movement-state field involved in a lock.
    private const int RetainedTailFrames = 60;

    public static int Run(
        string recordingPath,
        string romPath,
        bool enforcePlatformCrossingInvariant = false,
        int traceStartFrame = -1,
        int traceEndFrame = -1,
        bool enforceStationaryMissileExplosions = false)
    {
        ControllerInputRecording recording = ControllerInputRecording.Read(recordingPath);
        PrintInputRuns(
            recording.ControllerInputs,
            startFrame: traceStartFrame >= 0 ? traceStartFrame : 5400,
            endFrame: traceEndFrame);
        string fullRomPath = Path.GetFullPath(romPath);
        byte[] digest;
        using (FileStream rom = File.OpenRead(fullRomPath))
            digest = SHA256.HashData(rom);
        if (!CryptographicOperations.FixedTimeEquals(digest, recording.RomSha256))
            throw new InvalidDataException("Replay ROM SHA-256 does not match the recording.");

        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(fullRomPath);
        recording.InitialSaveRam.CopyTo(bus.SaveRam);
        var game = new SuperMetroidGame(
            bus,
            recording.GameOptions,
            renderGameplayFrames: false);
        var apuPortEchoes = new byte[4];
        var movingExplosions = new MovingMissileExplosionAudit(bus, enforceStationaryMissileExplosions);
        var tail = new Queue<ReplayFrameState>(RetainedTailFrames);
        ushort? previousRoom = null;
        ushort previousCeresStatus = ushort.MaxValue;
        ElevatorActorStatus? previousElevatorStatus = null;
        bool previousEjection = false;
        int previousPlmCount = -1;
        int previousScrollPlmCount = -1;
        PowerBombExplosionPhase previousPowerBombPhase = PowerBombExplosionPhase.Inactive;
        bool previousPowerBombArmed = false;
        bool previousPowerBombSlotActive = false;
        bool previousPowerBombDamagingRadius = false;
        SuperMetroidRuntime? lastRuntime = null;
        int wallJumpFrames = 0;

        for (int index = 0; index < recording.ControllerInputs.Length; index++)
        {
            ushort input = recording.ControllerInputs[index];
            SuperMetroidRuntime? runtimeBeforeStep = game.RuntimeForVerification;
            SamusState? samusBeforeStep = runtimeBeforeStep?.Samus;
            ushort? roomBeforeStep = runtimeBeforeStep?.ActiveRoom?.Pointer;
            ushort yBeforeStep = samusBeforeStep?.YPosition ?? 0;
            ushort xBeforeStep = samusBeforeStep?.XPosition ?? 0;
            ushort xRadiusBeforeStep = samusBeforeStep?.Kinematics.XRadius ?? 0;
            ushort yRadiusBeforeStep = samusBeforeStep?.Kinematics.YRadius ?? 0;
            byte? poseBeforeStep = samusBeforeStep?.Pose;
            if (poseBeforeStep is byte wallPose && SamusState.IsWallJumpPose(wallPose)) wallJumpFrames++;
            ushort? selectedHudItemBeforeStep = samusBeforeStep?.SelectedHudItem;
            FrontendFrame frontend;
            try
            {
                frontend = game.Step(input);
            }
            catch (Exception exception)
            {
                throw new InvalidDataException(
                    $"Replay failed on exact recorded frame {index}: state=" +
                    $"${(ushort)game.GameState:X2}, room=$8F:{roomBeforeStep.GetValueOrDefault():X4}, " +
                    $"input=${input:X4}, latched=${runtimeBeforeStep?.Controller1.Current ?? 0:X4}/" +
                    $"${runtimeBeforeStep?.Controller1.NewlyPressed ?? 0:X4}, pose=" +
                    $"${poseBeforeStep.GetValueOrDefault():X2}, movement=" +
                    $"${(byte)(samusBeforeStep?.ReadMovementType(bus) ?? SamusMovementType.Standing):X2}, " +
                    $"xy=(${xBeforeStep:X4},${yBeforeStep:X4}), " +
                    $"y-direction=${samusBeforeStep?.Kinematics.YDirection ?? 0:X4}, " +
                    $"y-speed=${samusBeforeStep?.Kinematics.YSpeed ?? 0:X4}. ",
                    exception);
            }
            foreach (Audio.CartridgeAudioCommand command in frontend.AudioCommands)
            {
                if (command.Kind == Audio.CartridgeAudioCommandKind.WritePort)
                    apuPortEchoes[command.Port] = command.Value;
            }
            game.SetAudioAcknowledgements(new Audio.CartridgeAudioAcknowledgements(
                apuPortEchoes[0],
                apuPortEchoes[1],
                apuPortEchoes[2],
                apuPortEchoes[3]));
            SuperMetroidRuntime? runtime = game.RuntimeForVerification;
            lastRuntime = runtime;
            if (runtime is not null) movingExplosions.Observe(index, runtime.ActiveRoom?.Pointer, runtime.Projectiles);
            SamusState? samus = runtime?.Samus;
            if (poseBeforeStep is byte beforePose && SamusState.IsWallJumpPose(beforePose) &&
                samus is not null && !SamusState.IsWallJumpPose(samus.Pose) && !SamusState.IsSpinJumpPose(samus.Pose))
                Console.WriteLine($"WALL-JUMP EXIT frame={index} room={runtime!.ActiveRoom?.Pointer:X4} " +
                    $"pose={beforePose:X2}->{samus.Pose:X2} input={runtime.Controller1.Current:X4} " +
                    $"landed={runtime.LastAerialSamusMovement?.Landed} ceiling={runtime.LastAerialSamusMovement?.HitCeiling}");
            RidleyEnemyState? ridley = runtime?.Enemies.CeresRidley;
            ushort? room = runtime?.ActiveRoom?.Pointer;
            ushort ceresStatus = runtime?.Enemies.CeresStatus ?? 0;
            ElevatorActorStatus? elevatorStatus = runtime?.Enemies.ElevatorStatus;
            bool ejection = samus?.CeresRidleyEjection.IsActive ?? false;
            int plmCount = runtime?.Plms.ActiveCount ?? 0;
            int scrollPlmCount = runtime?.Plms.ScrollPlms.Count ?? 0;
            SamusPowerBombExplosionState? powerBomb =
                runtime?.BombProjectiles.PowerBombExplosion;
            PowerBombExplosionPhase powerBombPhase =
                powerBomb?.Phase ?? PowerBombExplosionPhase.Inactive;
            bool powerBombArmed = powerBomb?.IsArmed ?? false;
            bool powerBombSlotActive = runtime?.BombProjectiles.Slots.Any(slot =>
                slot.IsActive && slot.PackedType.Family == SamusProjectileFamily.PowerBomb) ?? false;
            bool powerBombDamagingRadius = powerBomb?.ExplosionRadius != 0;

            // Projectile reports are often intermittent because allocation depends on
            // which of five shared beam/missile slots still owns an explosion or an
            // invisible Super-Missile link. Log only producer frames, but retain every
            // field needed to distinguish a correctly initialized missile from a slot
            // that inherited its predecessor's animation program. This remains cheap
            // enough for the always-headless recording audit and avoids modifying live
            // desktop behavior merely to diagnose a rare visual frame.
            if (index >= traceStartFrame && index <= traceEndFrame &&
                selectedHudItemBeforeStep == 2 &&
                runtime?.Projectiles.LastFrameResult.FiredSlot is int firedSlotIndex)
            {
                SamusProjectileSlot firedSlot = runtime.Projectiles.Slots[firedSlotIndex];
                Console.WriteLine(
                    $"rec={index,6} projectile spawn slot={firedSlotIndex} " +
                    $"type=${firedSlot.Type:X4} damage=${firedSlot.Damage:X4} " +
                    $"instruction=${firedSlot.InstructionPointer:X4}/" +
                    $"timer=${firedSlot.InstructionTimer:X4} map=${firedSlot.SpritemapPointer:X4} " +
                    $"pre={firedSlot.PreInstruction} collision=" +
                    $"{runtime.Projectiles.LastFrameResult.CollisionStartedExplosion} " +
                    $"counter={runtime.Projectiles.ProjectileCounter}.");
            }

            if (runtime is not null && index >= traceStartFrame && index <= traceEndFrame)
            {
                // Door-exit and short-input reports require the speed owners, not just
                // integer positions: base speed, retained run momentum, deceleration,
                // and liquid selection can produce the same displacement differently.
                if (samus is not null)
                    Console.WriteLine($"rec={index,6} movement " +
                        $"x={samus.Kinematics.XFixed:X8} " +
                        $"base={samus.HorizontalSpeed.BaseSpeed:X4}.{samus.HorizontalSpeed.BaseSubspeed:X4} " +
                        $"extra={samus.HorizontalSpeed.ExtraRunSpeed:X4}.{samus.HorizontalSpeed.ExtraRunSubspeed:X4} " +
                        $"accel={samus.HorizontalSpeed.AccelerationMode} momentum={samus.HorizontalSpeed.HasRunningMomentum} " +
                        $"medium={samus.LiquidPhysics.DetermineMovementMedium(samus)} " +
                        $"fx={samus.LiquidPhysics.FxType} water={samus.LiquidPhysics.FxYPosition:X4} " +
                        $"lava={samus.LiquidPhysics.LavaAcidYPosition:X4}");
                string projectileState = string.Join(", ", runtime.Projectiles.Slots
                    .Where(slot => slot.IsActive)
                    .Select(slot =>
                        $"{slot.SlotIndex}:${slot.Type:X4}@${slot.XPosition:X4},${slot.YPosition:X4}" +
                        $"/v${slot.XVelocity:X4},${slot.YVelocity:X4}/var${slot.Variable:X4}" +
                        $"/i${slot.InstructionPointer:X4}/m${slot.SpritemapPointer:X4}/" +
                        $"{slot.PreInstruction}"));
                string enemyState = string.Join(", ", runtime.Enemies.Slots
                    .Where(slot => slot.EnemyDefinitionPointer != 0)
                    .Select(slot =>
                        $"{slot.SlotIndex}:${slot.EnemyDefinitionPointer:X4}" +
                        $"@${slot.XPosition:X4},${slot.YPosition:X4}" +
                        $"/r${slot.XRadius:X2},${slot.YRadius:X2}" +
                        $"/hp${slot.Health:X4}/inv${slot.InvincibilityTimer:X4}" +
                        $"/prop${slot.Properties:X4}"));
                string enemyProjectileState = string.Join(", ", runtime.Enemies.EnemyProjectiles
                    .Where(slot => slot.IsActive)
                    .Select(slot =>
                        $"{slot.SlotIndex}:{slot.Kind}" +
                        $"@${slot.XPosition:X4},${slot.YPosition:X4}" +
                        $"/r${slot.XRadius:X2},${slot.YRadius:X2}" +
                        $"/block={slot.BlocksSamusProjectiles}" +
                        $"/option=${slot.CollisionOption:X4}" +
                        $"/i${slot.InstructionPointer:X4}"));
                Console.WriteLine(
                    $"rec={index,6} projectile slots=[{projectileState}] " +
                    $"enemies=[{enemyState}] enemy-projectiles=[{enemyProjectileState}]");
                if (runtime.LevelData is { } collisionLevel)
                    foreach (var slot in runtime.Projectiles.Slots.Where(slot => slot.IsActive))
                        for (int dx = -16; dx <= 16; dx += 8)
                        {
                            int bx = (slot.XPosition + dx) >> 4, by = slot.YPosition >> 4;
                            if ((uint)bx < collisionLevel.WidthInBlocks && (uint)by < collisionLevel.HeightInBlocks)
                                Console.WriteLine($"rec={index} slot={slot.SlotIndex} sample-x={slot.XPosition + dx:X4} block={collisionLevel.GetCollisionBlock(bx, by)}");
                        }
            }

            // A repeated player report says a downward jump/fall can cross a platform in
            // live Zebes gameplay even though the earlier scripted Climb route landed.
            // Audit the actual recorded trajectory at the geometric boundary, before any
            // screen-space wrapping can disguise it. Types $8/$C/$E all enter the solid
            // vertical branch in `$94:959E/$95F5`; crossing their top edge without a room
            // transition is therefore never a legal cartridge result.
            if (enforcePlatformCrossingInvariant &&
                runtimeBeforeStep is not null && ReferenceEquals(runtimeBeforeStep, runtime) &&
                samusBeforeStep is not null && samus is not null &&
                roomBeforeStep == room && runtime?.LevelData is RoomLevelData level &&
                samus.YPosition > yBeforeStep)
            {
                int bottomBefore = yBeforeStep + yRadiusBeforeStep - 1;
                int bottomAfter = samus.YPosition + samus.Kinematics.YRadius - 1;
                if (bottomAfter > bottomBefore)
                {
                    int left = Math.Max(0,
                        (Math.Min(xBeforeStep, samus.XPosition) -
                            Math.Max(xRadiusBeforeStep, samus.Kinematics.XRadius)) >> 4);
                    int right = Math.Min(level.WidthInBlocks - 1,
                        (Math.Max(xBeforeStep, samus.XPosition) +
                            Math.Max(xRadiusBeforeStep, samus.Kinematics.XRadius) - 1) >> 4);
                    int firstRow = Math.Max(0, (bottomBefore + 1) >> 4);
                    int lastRow = Math.Min(level.HeightInBlocks - 1, bottomAfter >> 4);
                    for (int blockY = firstRow; blockY <= lastRow; blockY++)
                    {
                        int surfaceY = blockY << 4;
                        if (surfaceY <= bottomBefore || surfaceY > bottomAfter)
                            continue;
                        for (int blockX = left; blockX <= right; blockX++)
                        {
                            RoomCollisionBlock block = level.GetCollisionBlock(blockX, blockY);
                            if (block.CollisionType is not (
                                RoomCollisionType.SolidBlock or
                                RoomCollisionType.ShootableBlock or
                                RoomCollisionType.GrappleBlock))
                                continue;
                            throw new InvalidDataException(
                                $"Replay frame {index} crossed solid platform " +
                                $"room $8F:{room.GetValueOrDefault():X4} block " +
                                $"({blockX:X2},{blockY:X2}) type ${(byte)block.CollisionType:X1}/" +
                                $"BTS ${block.Behavior:X2}: bottom {bottomBefore:X4}->" +
                                $"{bottomAfter:X4}, Samus (${xBeforeStep:X4},${yBeforeStep:X4})" +
                                $"->(${samus.XPosition:X4},${samus.YPosition:X4}), pose " +
                                $"${poseBeforeStep.GetValueOrDefault():X2}->${samus.Pose:X2}, " +
                                $"input=${input:X4}.");
                        }
                    }
                }
            }
            var state = new ReplayFrameState(
                index,
                frontend.GameState,
                room,
                input,
                runtime?.Controller1.NewlyPressed ?? 0,
                samus?.Pose,
                samus?.XPosition,
                samus?.YPosition,
                runtime?.Camera?.XPosition,
                runtime?.Camera?.YPosition,
                runtime?.BackgroundScroll.Bg1HorizontalScroll,
                runtime?.BackgroundScroll.Bg1VerticalScroll,
                elevatorStatus,
                runtime?.Enemies.LastElevatorEvent,
                samus?.Health,
                ceresStatus,
                ejection,
                samus?.CeresRidleyEjection.IsPending ?? false,
                samus?.InputLocked ?? false,
                samus?.KnockbackActive ?? false,
                samus?.KnockbackTimer ?? 0,
                samus?.KnockbackDirection ?? 0,
                samus?.KnockbackXDirection ?? 0,
                ridley?.Function,
                ridley?.Mode7Active ?? false,
                runtime?.HasPendingDoorTransition ?? false);
            if (tail.Count == RetainedTailFrames)
                tail.Dequeue();
            tail.Enqueue(state);

            if (room != previousRoom || ceresStatus != previousCeresStatus ||
                elevatorStatus != previousElevatorStatus || ejection != previousEjection)
                Console.WriteLine(state.Format());
            else if (index >= traceStartFrame && index <= traceEndFrame)
                Console.WriteLine(state.Format());
            if (plmCount != previousPlmCount || scrollPlmCount != previousScrollPlmCount)
            {
                Console.WriteLine(
                    $"rec={index,6} PLMs active={plmCount}, scroll={scrollPlmCount}, " +
                    $"origins=[{string.Join(',', runtime?.Plms.ScrollPlms.Select(x => x.BlockIndex) ?? [])}]");
            }
            if (powerBombPhase != previousPowerBombPhase ||
                powerBombArmed != previousPowerBombArmed ||
                powerBombSlotActive != previousPowerBombSlotActive ||
                powerBombDamagingRadius != previousPowerBombDamagingRadius ||
                runtime?.BombProjectiles.LastFrameResult.ExplosionStarted == true)
            {
                BombProjectileFrameResult bombFrame =
                    runtime?.BombProjectiles.LastFrameResult ?? default;
                Console.WriteLine(
                    $"rec={index,6} power-bomb phase={powerBombPhase}, " +
                    $"armed={powerBombArmed}, active-slot={powerBombSlotActive}, " +
                    $"pre-radius=${powerBomb?.PreExplosionRadius ?? 0:X4}, " +
                    $"damage-radius=${powerBomb?.ExplosionRadius ?? 0:X4}, " +
                    $"started={bombFrame.ExplosionStarted}, deleted={bombFrame.ProjectileDeleted}, " +
                    $"block-visits={bombFrame.BlockReactions?.Count ?? 0}, plms={plmCount}.");
            }
            previousRoom = room;
            previousCeresStatus = ceresStatus;
            previousElevatorStatus = elevatorStatus;
            previousEjection = ejection;
            previousPlmCount = plmCount;
            previousScrollPlmCount = scrollPlmCount;
            previousPowerBombPhase = powerBombPhase;
            previousPowerBombArmed = powerBombArmed;
            previousPowerBombSlotActive = powerBombSlotActive;
            previousPowerBombDamagingRadius = powerBombDamagingRadius;

            // A bounded trace is an investigation tool, not a semantic full-recording
            // replay. Stop at its requested endpoint so inspecting a late transition does
            // not spend another minute simulating unrelated gameplay after the evidence.
            if (traceEndFrame >= 0 && index >= traceEndFrame)
                break;
        }

        Console.WriteLine($"Replay completed {recording.ControllerInputs.Length} recorded calls. Tail:");
        foreach (ReplayFrameState state in tail)
            Console.WriteLine(state.Format());
        if (lastRuntime?.Samus is SamusState finalSamus)
        {
            Console.WriteLine(
                $"Final liquid: type={lastRuntime.RoomLayer3Fx.Type}, " +
                $"surface=${lastRuntime.RoomLayer3Fx.CurrentYPosition:X4}, " +
                $"options=${lastRuntime.RoomLayer3Fx.LiquidOptions:X4}, " +
                $"movement-medium=" +
                $"{finalSamus.LiquidPhysics.DetermineMovementMedium(finalSamus)}.");
        }

        // A controller trace is most useful when it also preserves the exact final image
        // produced from that deterministic state. Render only once after replay: the
        // software PPU is intentionally faithful rather than fast, and recording/replay
        // diagnostics must never multiply its cost by every input frame.
        if (lastRuntime?.ActiveDoor is not null)
        {
            string captureDirectory = Path.Combine("csharp", "test-temp", "input-replays");
            Directory.CreateDirectory(captureDirectory);
            string capturePath = Path.Combine(
                captureDirectory,
                $"{Path.GetFileNameWithoutExtension(recordingPath)}.last.png");
            PngWriter.WriteRgba(
                capturePath,
                FrontendFrame.Width,
                FrontendFrame.Height,
                SuperMetroidRuntimeFrameRenderer.Render(lastRuntime));
            SamusMode7Transform? mode7 = lastRuntime.DisplayedSamusMode7Transform;
            Console.WriteLine(
                $"Final PPU capture: {Path.GetFullPath(capturePath)}; displayed Mode 7=" +
                (mode7 is { } transform
                    ? $"A/D=${transform.MatrixA:X4}, B=${transform.MatrixB:X4}, " +
                      $"C=${transform.MatrixC:X4}, center=(${transform.CenterX:X4},${transform.CenterY:X4})"
                    : "inactive"));
        }
        Console.WriteLine($"Wall-jump audit observed {wallJumpFrames} wall-jump frames.");
        return 0;
    }

    /// <summary>
    /// Prints only non-zero controller runs near the reported failure.  This distinguishes
    /// a cartridge state that rejected player input from a session in which the controller
    /// was actually neutral, without flooding the diagnostic with one line per video frame.
    /// </summary>
    private static void PrintInputRuns(
        ReadOnlySpan<ushort> inputs,
        int startFrame,
        int endFrame = -1)
    {
        int exclusiveEnd = endFrame >= 0
            ? Math.Min(inputs.Length, endFrame + 1)
            : inputs.Length;
        for (int start = Math.Max(0, startFrame); start < exclusiveEnd;)
        {
            ushort value = inputs[start];
            int end = start + 1;
            while (end < exclusiveEnd && inputs[end] == value)
                end++;
            if (value != 0)
                Console.WriteLine($"input rec={start}..{end - 1} value=${value:X4}");
            start = end;
        }
    }

    private readonly record struct ReplayFrameState(
        int RecordingFrame,
        SuperMetroidGameState GameState,
        ushort? Room,
        ushort Input,
        ushort NewInput,
        byte? Pose,
        ushort? X,
        ushort? Y,
        ushort? CameraX,
        ushort? CameraY,
        ushort? Bg1ScrollX,
        ushort? Bg1ScrollY,
        ElevatorActorStatus? ElevatorStatus,
        ElevatorFrameEvent? ElevatorEvent,
        ushort? Health,
        ushort CeresStatus,
        bool EjectionActive,
        bool EjectionPending,
        bool InputLocked,
        bool KnockbackActive,
        ushort KnockbackTimer,
        ushort KnockbackDirection,
        ushort KnockbackXDirection,
        RidleyAiFunction? RidleyFunction,
        bool Mode7Active,
        bool PendingDoor)
    {
        public string Format() =>
            $"rec={RecordingFrame,6} gs=${(ushort)GameState:X2} room=" +
            (Room is ushort room ? $"${room:X4}" : "----") +
            $" in=${Input:X4}/${NewInput:X4} pose=" +
            (Pose is byte pose ? $"${pose:X2}" : "--") +
            $" xy=" + (X is ushort x && Y is ushort y ? $"${x:X4},${y:X4}" : "----,----") +
            $" cam=" + (CameraX is ushort cameraX && CameraY is ushort cameraY
                ? $"${cameraX:X4},${cameraY:X4}"
                : "----,----") +
            $" bg1=" + (Bg1ScrollX is ushort bg1X && Bg1ScrollY is ushort bg1Y
                ? $"${bg1X:X4},${bg1Y:X4}"
                : "----,----") +
            " elev=" + (ElevatorStatus is { } elevatorStatus
                ? $"{elevatorStatus}/{ElevatorEvent}"
                : "--/--") +
            $" hp={Health?.ToString() ?? "-"} ceres=${CeresStatus:X4}" +
            $" eject={EjectionActive}/{EjectionPending} lock={InputLocked}" +
            $" kb={KnockbackActive}/${KnockbackTimer}/${KnockbackDirection}/{KnockbackXDirection} ridley=" +
            (RidleyFunction is { } function ? $"${(ushort)function:X4}" : "----") +
            $" m7={Mode7Active} door={PendingDoor}";
    }
}
