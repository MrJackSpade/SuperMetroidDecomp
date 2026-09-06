using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

/// <summary>
/// Reproduces ordinary projectile contact with the retail blue door in room $01/$1D.
/// </summary>
internal static class BlueBrinstarDoorAudit
{
    public static int Run(string romPath)
    {
        RunProjectileCase(romPath, selectedHudItem: 0, SamusBeamFlags.None, "Power Beam");
        RunProjectileCase(romPath, selectedHudItem: 0, SamusBeamFlags.Ice, "Ice Beam");
        RunProjectileCase(romPath, selectedHudItem: 0, SamusBeamFlags.Wave, "Wave Beam");
        RunProjectileCase(
            romPath,
            selectedHudItem: 0,
            SamusBeamFlags.Charge | SamusBeamFlags.Wave | SamusBeamFlags.Ice | SamusBeamFlags.Spazer,
            "Charge/Wave/Ice/Spazer Beam");
        RunProjectileCase(romPath, selectedHudItem: 1, SamusBeamFlags.None, "Missile");
        RunProjectileCase(romPath, selectedHudItem: 2, SamusBeamFlags.None, "Super Missile");
        return 0;
    }

    /// <summary>Checks shot timing against the still-running entry-door close actor.</summary>
    public static int RunTimingSweep(string romPath)
    {
        int failures = 0;
        int earlyMisses = 0;
        for (int closeFrames = 0; closeFrames <= 32; closeFrames++)
        foreach (ushort selection in new ushort[] { 0, 1, 2 })
        {
            try
            {
                if (!RunProjectileCase(romPath, selection, SamusBeamFlags.None,
                    $"close={closeFrames} selection={selection}", closeFrames, retryAfterClosing: true))
                    earlyMisses++;
            }
            catch (InvalidDataException exception)
            {
                failures++;
                Console.WriteLine($"FAIL close={closeFrames} selection={selection}: {exception.Message}");
            }
        }
        Console.WriteLine($"Door timing: 99 cases, {earlyMisses} early misses, {failures} persistent failures after retry. Native closing timing is not asserted.");
        return failures == 0 ? 0 : 1;
    }

    public static int RunContactSweep(string romPath)
    {
        int failures = 0;
        foreach (int distance in new[] { 5, 6, 7, 8, 12, 16, 24 })
        foreach (int height in new[] { 24, 32, 40 })
        foreach (ushort selection in new ushort[] { 0, 1, 2 })
        {
            try
            {
                RunProjectileCase(romPath, selection, SamusBeamFlags.None,
                    $"distance={distance} height={height} selection={selection}", 32, distance, height);
            }
            catch (InvalidDataException exception)
            {
                failures++;
                Console.WriteLine($"FAIL distance={distance} height={height} selection={selection}: {exception.Message}");
            }
        }
        Console.WriteLine($"Door contact: 63 cases, {failures} failures.");
        return failures == 0 ? 0 : 1;
    }

    private static bool RunProjectileCase(
        string romPath,
        ushort selectedHudItem,
        SamusBeamFlags equippedBeams,
        string name,
        int closeFrames = 32,
        int contactDistance = 6,
        int heightOffset = 24,
        bool retryAfterClosing = false)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath(romPath));
        var runtime = new SuperMetroidRuntime(bus, playerInvincibilityEnabled: true);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();

        CartridgeDoorHeader entryDoor = CartridgeDoorHeader.Load(
            bus,
            DoorPointers.BlueBrinstarDoubleMissileFromBoulders);
        if (entryDoor.DestinationRoomPointer != RoomHeaderPointers.BlueBrinstarDoubleMissile)
        {
            throw new InvalidDataException(
                $"Door $83:{DoorPointers.BlueBrinstarDoubleMissileFromBoulders:X4} targets " +
                $"$8F:{entryDoor.DestinationRoomPointer:X4}, not room $01/$1D " +
                $"($8F:{RoomHeaderPointers.BlueBrinstarDoubleMissile:X4}).");
        }
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.BlueBrinstarBoulders);
        PublishRetailDoor(
            runtime,
            bus,
            DoorPointers.BlueBrinstarDoubleMissileFromBoulders);
        var transition = new DoorTransitionState();
        transition.Begin(runtime);
        var audio = new CartridgeAudioState();
        for (int frame = 0; transition.IsActive && frame < 320; frame++)
            transition.Step(runtime, audio, controllerInput: 0);
        if (transition.IsActive)
            throw new InvalidDataException("Transition into room $01/$1D did not complete.");
        if (runtime.ActiveRoom?.Identity != RoomIdentities.BlueBrinstarDoubleMissile)
        {
            throw new InvalidDataException(
                $"Door transition ended in {runtime.ActiveRoom?.Identity.ToString() ?? "no room"}, " +
                $"not {RoomIdentities.BlueBrinstarDoubleMissile}.");
        }

        RoomLevelData level = runtime.LevelData ??
            throw new InvalidDataException("Room $01/$1D loaded without level data.");
        SamusState samus = runtime.Samus ??
            throw new InvalidDataException("Room $01/$1D loaded without Samus.");
        RoomCollisionBlock[] capOrigins = Enumerable.Range(0, level.ForegroundEntries.Length)
            .Select(level.GetCollisionBlockByIndex)
            .Where(block => block.Bts.TryGetBlueDoorOrientation(out _))
            .ToArray();
        if (capOrigins.Length != 1)
        {
            throw new InvalidDataException(
                $"Room $01/$1D exposes {capOrigins.Length} blue-door origins, expected one.");
        }

        RoomCollisionBlock cap = capOrigins[0];
        if (!cap.Bts.TryGetBlueDoorOrientation(out ColoredDoorOrientation orientation))
            throw new InvalidDataException("Validated blue-door origin lost its orientation.");
        if (cap.Bts != RoomBlockBehaviorValues.BlueDoorFacingLeft)
        {
            throw new InvalidDataException(
                $"Room $01/$1D door cap is {cap.Bts}, expected left-facing blue BTS $40.");
        }
        int capX = cap.Index % level.WidthInBlocks;
        int capY = cap.Index / level.WidthInBlocks;
        samus.InputLocked = false;
        samus.Pose = orientation == ColoredDoorOrientation.Left
            ? SamusPoseIds.FacingRightNormalPose
            : SamusPoseIds.FacingLeftNormalPose;
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        samus.XPosition = unchecked((ushort)((capX << 4) +
            (orientation == ColoredDoorOrientation.Left ? -contactDistance : contactDistance)));
        samus.YPosition = unchecked((ushort)((capY << 4) + heightOffset));
        samus.SelectedHudItem = selectedHudItem;
        samus.EquippedBeams = equippedBeams.ToNativeWord();
        samus.Missiles = 10;
        samus.SuperMissiles = 10;

        // Finish the room-entry door-closing actor before firing back into the cap. This
        // matches the player's settled-room report and prevents two legitimate door actors
        // from racing only because the diagnostic bypassed frontend transition timing.
        for (int frame = 0; frame < closeFrames; frame++)
            StepPlms(runtime, bus, level);

        bool collided = false;
        bool fired = false;
        for (int frame = 0; frame < 96; frame++)
        {
            ushort input = frame < 2 ? (ushort)SnesButton.X : (ushort)0;
            runtime.StepFrame(input);
            SamusProjectileFrameResult result = runtime.Projectiles.LastFrameResult;
            collided |= result.CollisionStartedExplosion;
            fired |= result.FiredSlot is not null;
        }

        RoomCollisionBlock after = level.GetCollisionBlockByIndex(cap.Index);
        if (!fired)
            throw new InvalidDataException($"{name} did not fire in room $01/$1D.");
        bool openedOnFirstShot = after.CollisionType == RoomCollisionType.Air;
        if (!openedOnFirstShot && retryAfterClosing)
        {
            // An early shot may meet a not-yet-shootable closing tile. Distinguish
            // that transient from a permanently locked cap by retrying the SAME
            // weapon after the closing actor finishes. Do not substitute a Super.
            for (int frame = 0; frame < 96; frame++)
                runtime.StepFrame(frame < 2 ? (ushort)SnesButton.X : (ushort)0);
            after = level.GetCollisionBlockByIndex(cap.Index);
        }
        if (after.CollisionType != RoomCollisionType.Air)
        {
            throw new InvalidDataException(
                $"{name} left room $01/$1D's blue cap {after.CollisionType}; " +
                "an ordinary blue door must not require a Super Missile.");
        }
        Console.WriteLine(
            $"{name}: cap={cap.Index} ({capX},{capY}) bts={cap.Bts} orientation={orientation}, " +
            $"fired={fired}, impact={collided}, first-shot-open={openedOnFirstShot}, type={after.CollisionType}, " +
            $"active-plms={runtime.Plms.ActiveCount}, shots=[{string.Join(';', runtime.Projectiles.Slots.Where(slot => slot.IsActive).Select(slot => $"${slot.Type:X4}@{slot.XPosition},{slot.YPosition}"))}].");
        return openedOnFirstShot;
    }

    private static void PublishRetailDoor(
        SuperMetroidRuntime runtime,
        ISnesAddressSpace bus,
        ushort expectedDoorPointer)
    {
        RoomLevelData level = runtime.LevelData ??
            throw new InvalidDataException("Source room has no level data.");
        byte pose = runtime.Samus?.Pose ?? 0;
        for (int index = 0; index < level.ForegroundEntries.Length; index++)
        {
            RoomCollisionBlock block = level.GetCollisionBlockByIndex(index);
            if (block.CollisionType != RoomCollisionType.DoorBlock)
                continue;
            CartridgeDoorHeader candidate = level.ResolveDoorCollision(
                bus,
                block.Behavior,
                pose,
                publishDoorSideEffects: false);
            if (candidate.Pointer != expectedDoorPointer)
                continue;
            _ = level.ResolveDoorCollision(
                bus,
                block.Behavior,
                pose,
                publishDoorSideEffects: true);
            return;
        }

        throw new InvalidDataException(
            $"Room $8F:{RoomHeaderPointers.BlueBrinstarBoulders:X4} has no collision for " +
            $"door $83:{expectedDoorPointer:X4}.");
    }

    private static void StepPlms(
        SuperMetroidRuntime runtime,
        ISnesAddressSpace bus,
        RoomLevelData level)
    {
        BackgroundTilemapStreamer streamer = runtime.BackgroundStreamer ??
            throw new InvalidDataException("Room $01/$1D has no background streamer.");
        ScrollBoundaryCamera camera = runtime.Camera ??
            throw new InvalidDataException("Room $01/$1D has no camera.");
        runtime.Plms.Step(
            bus,
            level,
            streamer,
            camera.XPosition,
            camera.YPosition,
            runtime.BackgroundScroll.Bg1XOffset,
            camera.Scrolls);
    }
}
