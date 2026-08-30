using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>Cartridge-driven room loading kept separate from the gameplay-frame monolith.</summary>
public sealed partial class SuperMetroidRuntime
{
    /// <summary>The load-station record that most recently established this runtime.</summary>
    public LoadStationEntry? ActiveLoadStation { get; private set; }

    /// <summary>The selected room/state header for a generic cartridge-backed room.</summary>
    public CartridgeRoomHeader? ActiveRoom { get; private set; }

    /// <summary>The load-station door whose setup code established the current room.</summary>
    public CartridgeDoorHeader? ActiveDoor { get; private set; }

    /// <summary>Decompressed assets owned by <see cref="ActiveRoom"/>.</summary>
    public CartridgeRoomAssets? ActiveRoomAssets { get; private set; }

    /// <summary>The two native bank-$86 objects that carry Samus into fresh Ceres.</summary>
    public CeresElevatorArrivalState? CeresElevatorArrival { get; private set; }

    /// <summary>
    /// Executes the data-loading portion of native loading-state $1F: area six, station
    /// zero, the station-selected room, and its fresh-save state-selection branch.
    /// </summary>
    public InitialViewportResult InitializeStartingCeresRoom()
    {
        // `InitAndLoadGameData_Async` writes these exact indexes before calling
        // LoadFromLoadStation. Reading them through the general table proves which room the
        // retail cartridge selected; no Ceres room pointer is duplicated in host code.
        LoadStationEntry station = LoadStationEntry.Load(_addressSpace, areaIndex: 6, stationIndex: 0);
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(_addressSpace, station.DoorPointer);
        if (door.DestinationRoomPointer != station.RoomPointer)
        {
            throw new InvalidDataException(
                $"Ceres station room $8F:{station.RoomPointer:X4} disagrees with door " +
                $"$83:{door.Pointer:X4}'s destination $8F:{door.DestinationRoomPointer:X4}.");
        }

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(_addressSpace, door.DestinationRoomPointer);
        if (room.AreaIndex != 6)
        {
            throw new InvalidDataException(
                $"Fresh Ceres station targets area ${room.AreaIndex:X2}, expected area $06.");
        }

        ActiveLoadStation = station;
        return LoadCartridgeRoom(door, room, station.CameraX, station.CameraY);
    }

    /// <summary>
    /// True after bank-$94's type-$9 handler has selected a normal destination door and
    /// before the frontend's states $09-$0B consume it.
    /// </summary>
    public bool HasPendingDoorTransition => LevelData?.PendingDoorTransition is not null;

    /// <summary>
    /// Loads the destination selected by the live room's type-$9 collision. This is the
    /// cartridge-data portion of states $0A/$0B; the scrolling/fade presentation remains a
    /// separately visible frontend phase rather than being disguised as ordinary gameplay.
    /// </summary>
    public InitialViewportResult LoadPendingDoorDestination()
    {
        if (LevelData is null || Samus is null)
            throw new InvalidOperationException("A live room and Samus are required for a door transition.");

        CartridgeDoorHeader door = LevelData.ConsumePendingDoorTransition()
            ?? throw new InvalidOperationException("No type-$9 door collision is pending.");
        if ((door.DestinationRoomPointer & 0x8000) == 0)
        {
            throw new InvalidOperationException(
                $"Elevator pseudo-door $83:{door.Pointer:X4} cannot enter the normal room loader.");
        }

        DoorTransitionPlacement placement = CalculateDoorTransitionPlacement(door, Samus);
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(_addressSpace, door.DestinationRoomPointer);

        // The load-station record established only the first room. Once bank $94 publishes
        // a door definition, that definition and its destination header become authoritative.
        ActiveLoadStation = null;
        CeresElevatorArrival = null;
        InitialViewportResult viewport = LoadCartridgeRoom(
            door,
            room,
            placement.CameraX,
            placement.CameraY);

        Samus.Kinematics.SetXFixed(placement.SamusXFixed);
        Samus.Kinematics.SetYFixed(placement.SamusYFixed);
        Samus.Kinematics.YSpeed = 0;
        Samus.Kinematics.YSubspeed = 0;
        Samus.Kinematics.ExtraXDisplacement = 0;
        Samus.Kinematics.ExtraXSubdisplacement = 0;
        Samus.Kinematics.ExtraYDisplacement = 0;
        Samus.Kinematics.ExtraYSubdisplacement = 0;
        Samus.LiquidPhysics.AreaIndex = room.AreaIndex;
        Samus.LiquidPhysics.RoomIndex = room.RoomIndex;
        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.PrimeGraphics(_addressSpace);
        GroundedSamusMovementEnabled = true;
        return viewport;
    }

    /// <summary>
    /// Loads one retail room header directly for the ROM-backed debug runner. Gameplay never
    /// calls this seam: normal play must still arrive through a bank-$83 door so placement
    /// and setup code remain authoritative. Keeping the helper internal lets end-to-end
    /// audits exercise the complete runtime, renderer, enemy scheduler, and Samus handlers
    /// in a late room without duplicating several minutes of controller input.
    /// </summary>
    internal InitialViewportResult LoadCartridgeRoomForDebug(
        ushort roomPointer,
        ushort cameraX = 0,
        ushort cameraY = 0)
    {
        if (Samus is null)
            throw new InvalidOperationException("Direct debug room loading requires initialized Samus state.");

        CartridgeRoomHeader room = CartridgeRoomHeader.Load(_addressSpace, roomPointer);
        // The debug seam intentionally supplies an inert synthetic door. Any room whose
        // correctness depends on setup code must instead be audited through its real door;
        // Ceres Ridley's ordinary mode-nine room has no incoming setup routine dependency.
        var door = new CartridgeDoorHeader(
            Pointer: 0,
            DestinationRoomPointer: roomPointer,
            BitFlags: 0,
            Orientation: 0,
            PlmX: 0,
            PlmY: 0,
            DestinationScreenX: unchecked((byte)(cameraX >> 8)),
            DestinationScreenY: unchecked((byte)(cameraY >> 8)),
            SamusDistance: 0,
            SetupCodePointer: 0);

        ActiveLoadStation = null;
        CeresElevatorArrival = null;
        InitialViewportResult viewport = LoadCartridgeRoom(door, room, cameraX, cameraY);
        Samus.LiquidPhysics.AreaIndex = room.AreaIndex;
        Samus.LiquidPhysics.RoomIndex = room.RoomIndex;
        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.PrimeGraphics(_addressSpace);
        GroundedSamusMovementEnabled = true;
        return viewport;
    }

    /// <summary>Shared cartridge room/state/graphics load used by stations and doors.</summary>
    private InitialViewportResult LoadCartridgeRoom(
        CartridgeDoorHeader door,
        CartridgeRoomHeader room,
        ushort cameraX,
        ushort cameraY)
    {
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(_addressSpace, room);
        ActiveDoor = door;
        ActiveRoom = room;
        ActiveRoomAssets = assets;
        // Door setup `$8F:E4E0` writes these exact five registers before the fresh Ceres
        // elevator room becomes visible. Publishing the immutable transform here gives
        // Samus, her projectiles, and parameter-four/five steam a single authoritative
        // producer. Ordinary doors clear it so stale Mode-7 math cannot leak across rooms.
        ActiveSamusMode7Transform = door.UsesCeresElevatorMode7
            ? new SamusMode7Transform(
                MatrixA: 0x0100,
                MatrixB: 0,
                MatrixC: 0,
                CenterX: 0x0080,
                CenterY: 0x03f0)
            : null;
        LevelData = assets.LevelData;
        Camera = new ScrollBoundaryCamera(assets.Scrolls);
        Camera.SetPosition(cameraX, cameraY);
        BackgroundScroll.Layer2ScrollX = room.State.Layer2ScrollX;
        BackgroundScroll.Layer2ScrollY = room.State.Layer2ScrollY;
        BackgroundScroll.PrimePreviousBlocks();

        // Gameplay initializes BG1SC=$51 and BG2SC=$49. DisplayViewablePartOfRoom stores
        // their $0800-word separation so the same streamer can address both circular maps.
        BackgroundStreamer = LevelData.CreateBackgroundStreamer(sizeOfBg2: 0x0800);
        ScrollingSky = null;
        LandingSiteEntry = null;
        assets.LoadGraphics(Vram, Cgram);

        // A negative background-data pointer names bank-$82's command interpreter. Rooms
        // whose layer-2 scroll mode is fixed/odd rely on this list as their only BG2 source;
        // it must run before the initial BG1 fill so a door cannot expose the previous
        // room's tilemap. Ceres $DF8D, for example, decompresses the Tourian statue hall to
        // $7E:4000 and copies it to both VRAM $4800 and $4C00.
        if (unchecked((short)room.State.BackgroundDataPointer) < 0)
        {
            LibraryBackgroundLoader.Execute(
                _addressSpace,
                Vram,
                room.State.BackgroundDataPointer,
                door.Pointer);
        }

        // `$82:E43A-$E472` destroys room-owned objects before constructing the destination.
        // Resetting these owners before Load/beam upload prevents stale projectile and PLM
        // indices from addressing a different room's new arrays on the first visible frame.
        Plms.Reset();
        BombProjectiles.Reset();
        Projectiles.Reset();

        // Load the room-authored glass before enemy initialization. The head's bank-$A9
        // shot callback hardcodes the highest PLM room-argument word, so slot allocation is
        // part of the encounter ABI rather than a visual afterthought.
        Plms.TryLoadMotherBrainGlassPopulation(
            _addressSpace,
            LevelData,
            BackgroundStreamer,
            room.State.PlmPointer,
            hasAreaBossBit: mask =>
                System.HasAnyBossBits(room.AreaIndex, (BossBits)mask),
            hasEvent: System.HasEvent,
            setEvent: System.SetEvent);

        Enemies.Load(
            _addressSpace,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            Vram,
            Cgram,
            System.NextRandom,
            System.SetRandomNumber,
            readRandomNumber: () => System.RandomNumber,
            level: LevelData,
            samus: Samus,
            controllerInput: Controller1.Current,
            isAreaBossDefeated: () =>
                System.HasAnyBossBits(room.AreaIndex, BossBits.AreaBoss),
            setAreaBossDefeated: () =>
                System.SetBossBits(room.AreaIndex, BossBits.AreaBoss),
            cameraX: Camera.XPosition,
            cameraY: Camera.YPosition,
            hasEvent: System.HasEvent,
            setEvent: System.SetEvent,
            clearEvent: System.ClearEvent,
            isAreaMiniBossDefeated: () =>
                System.HasAnyBossBits(room.AreaIndex, BossBits.AreaMiniBoss),
            setAreaMiniBossDefeated: () =>
                System.SetBossBits(room.AreaIndex, BossBits.AreaMiniBoss),
            isAreaTorizoDefeated: () =>
                System.HasAnyBossBits(room.AreaIndex, BossBits.AreaTorizo),
            setAreaTorizoDefeated: () =>
                System.SetBossBits(room.AreaIndex, BossBits.AreaTorizo),
            setSamusControlsEnabled: enabled => GroundedSamusMovementEnabled = enabled,
            setRoomScrollByte: (index, value) => Camera.Scrolls.SetStorage(index, value),
            incrementMotherBrainGlassRoomArgument: Plms.IncrementMotherBrainGlassRoomArgument,
            readRoomScrollByte: index => Camera.Scrolls.ReadStorage(index),
            setMotherBrainLayerBlendingDefaultConfig:
                value => LayerBlendingDefaultConfig = value,
            setMotherBrainBg2Scroll:
                (horizontal, vertical) =>
                    BackgroundScroll.SetBg2ScrollRegisters(horizontal, vertical));
        ApplyPendingBotwoonWallPlm();
        ApplyPendingSporeSpawnCeilingPlm();
        ApplyPendingCrocomireArenaPlms();
        ApplyPendingMotherBrainPlms();
        ApplyPendingShitroidWallPlms();
        Enemies.QueueGraphicsUploads(VramWrites);

        // `$90:AC8D` follows the standard-sprite and room-enemy uploads during gameplay
        // setup. Power-beam spritemaps address VRAM words $6300-$637F; without this final
        // $0100-byte transfer, fresh Ceres leaves that range containing the overlapping
        // standard OBJ sheet. Input and projectile physics still work, but the first shot
        // appears as a small patch of unrelated pixels—the desktop corruption that made
        // Shoot look unwired. The starting-room call has no Samus yet and therefore selects
        // power beam zero; door calls preserve the live equipment combination.
        Projectiles.QueueBeamTilesAndLoadPalette(
            _addressSpace,
            VramWrites,
            Cgram,
            Samus?.EquippedBeams ?? 0);

        BackgroundScroll.Layer1XPosition = Camera.XPosition;
        BackgroundScroll.Layer1YPosition = Camera.YPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = BackgroundScroll.BuildInitialViewportRequests();
        LastBackgroundUpdateCount = requests.Count;
        int segmentCount = 0;
        foreach (BackgroundUpdateRequest request in requests)
        {
            TilemapStreamUpdate update = BackgroundStreamer.Build(request)
                ?? throw new InvalidOperationException("Starting Ceres room unexpectedly requested Mode 7 streaming.");
            update.ExecuteTo(Vram);
            segmentCount += update.Segments.Count;
        }
        BackgroundScroll.PrimePreviousBlocks();
        return new InitialViewportResult(requests.Count, segmentCount);
    }

    /// <summary>
    /// Applies the cross-bank effect published by Botwoon's bank-$B3 AI through the shared
    /// bank-$84 PLM owner. This is called at both native producer sites: room initialization
    /// for an already-defeated boss and EnemyMain when the final body segment lands.
    /// </summary>
    private void ApplyPendingBotwoonWallPlm()
    {
        if (Enemies.LastBotwoonWallPlm is not ushort header)
            return;
        if (LevelData is null || Camera is null)
        {
            throw new InvalidOperationException(
                "Botwoon published a wall PLM without an active room level and scroll grid.");
        }

        // SpawnHardcodedPLM silently returns when all native slots are occupied. Preserve
        // that allocator behavior; normal load begins from Reset's empty pool, while an
        // artificial full-pool audit must not gain a host-only exception or terrain edit.
        Plms.TrySpawnBotwoonWall(LevelData, header);

        if (header == 0xb797)
        {
            // The already-defeated branch performs this 16-bit `$0101` store directly in
            // `$B3:959E`; unlike the live crumble PLM, it does not wait for instruction
            // `$84:AB51` during the first handler pass.
            Camera.Scrolls.SetLogicalCell(0, 0, (byte)RoomScrollState.Blue);
            Camera.Scrolls.SetLogicalCell(1, 0, (byte)RoomScrollState.Blue);
        }
    }

    /// <summary>
    /// Applies Spore Spawn's bank-$A5 ceiling request through the shared bank-$84 PLM
    /// owner. Initialization publishes the clear entry for a defeated save; the live death
    /// reaction publishes the animated crumble entry on the exact collision frame.
    /// </summary>
    private void ApplyPendingSporeSpawnCeilingPlm()
    {
        if (Enemies.LastSporeSpawnPlm is not SporeSpawnPlmRequest request)
            return;
        if (LevelData is null)
        {
            throw new InvalidOperationException(
                "Spore Spawn published a ceiling PLM without active room level data.");
        }
        if (request.BlockX != 7 || request.BlockY != 30)
        {
            throw new InvalidDataException(
                $"Spore Spawn published non-cartridge ceiling coordinates " +
                $"({request.BlockX},{request.BlockY}).");
        }

        // SpawnHardcodedPLM silently drops the request if all forty native slots are full.
        Plms.TrySpawnSporeSpawnCeiling(LevelData, request.Header);
    }

    /// <summary>
    /// Applies Crocomire's bank-$A4 hardcoded arena mutations through the shared bank-$84
    /// PLM owner. The enemy publishes a frame-local list because one collapse step can clear
    /// ten bridge cells before adding its invisible wall; consuming the entire list here
    /// preserves both native order and fixed-pool exhaustion behavior.
    /// </summary>
    private void ApplyPendingCrocomireArenaPlms()
    {
        if (Enemies.CrocomirePlmRequests.Count == 0)
            return;
        if (LevelData is null)
        {
            throw new InvalidOperationException(
                "Crocomire published an arena PLM without an active room level.");
        }

        foreach (CrocomirePlmRequest request in Enemies.CrocomirePlmRequests)
        {
            // SpawnHardcodedPLM silently loses a request when all forty native slots are
            // occupied. TrySpawnCrocomireArenaMutation deliberately mirrors that result.
            Plms.TrySpawnCrocomireArenaMutation(
                LevelData,
                request.BlockX,
                request.BlockY,
                request.Header);
        }
    }

    /// <summary>
    /// Transfers Mother Brain's bank-$A9 hardcoded-PLM requests to the shared bank-$84
    /// owner. The enemy state publishes requests in cartridge call order; consuming them
    /// here retains the native descending-slot allocation and one-frame execution seam.
    /// </summary>
    private void ApplyPendingMotherBrainPlms()
    {
        MotherBrainEnemyState? state = Enemies.MotherBrain;
        if (state is null || state.PlmRequests.Count == 0)
            return;
        if (LevelData is null)
        {
            throw new InvalidOperationException(
                "Mother Brain published a room PLM without active room level data.");
        }

        foreach (MotherBrainPlmRequest request in state.PlmRequests)
        {
            Plms.TrySpawnMotherBrainMutation(
                LevelData,
                request.BlockX,
                request.BlockY,
                request.Header);
        }
    }

    /// <summary>
    /// Transfers Shitroid's bank-$A9 hardcoded wall requests to the shared bank-$84 PLM
    /// pool at both producer seams: enemy initialization and the gameplay PLM handler.
    /// </summary>
    private void ApplyPendingShitroidWallPlms()
    {
        if (Enemies.ShitroidPlmRequests.Count == 0)
            return;
        if (LevelData is null)
        {
            throw new InvalidOperationException(
                "Shitroid published wall PLMs without active room level data.");
        }

        foreach (ShitroidPlmRequest request in Enemies.ShitroidPlmRequests)
        {
            Plms.TrySpawnShitroidWallMutation(
                LevelData,
                request.BlockX,
                request.BlockY,
                request.Header);
        }
    }

    /// <summary>
    /// Replays the position arithmetic from <c>$80:AD30-$AF89</c> and <c>$82:E3C0/E6A2</c>
    /// to obtain the final camera/Samus coordinates after the native scrolling phase.
    /// </summary>
    private static DoorTransitionPlacement CalculateDoorTransitionPlacement(
        CartridgeDoorHeader door,
        SamusState samus)
    {
        int direction = door.Orientation & 3;
        int distance = unchecked((short)door.SamusDistance);
        if (distance < 0)
            distance = (direction & 2) != 0 ? 384 : 200;
        uint step = unchecked((uint)(distance << 8));

        ushort destinationX = unchecked((ushort)(door.DestinationScreenX << 8));
        ushort destinationY = unchecked((ushort)(door.DestinationScreenY << 8));
        uint xFixed = samus.Kinematics.XFixed;
        uint yFixed = samus.Kinematics.YFixed;

        switch (direction)
        {
            case 0: // Right: setup executes frame zero before PlaceSamusLoadTiles.
                xFixed = unchecked(xFixed + step);
                xFixed = ReplaceWholePosition(
                    unchecked((ushort)(destinationX - 252 + (byte)(xFixed >> 16))),
                    xFixed);
                for (int frame = 1; frame < 64; frame++)
                    xFixed = unchecked(xFixed + step);
                break;

            case 1: // Left is the exact subtracting mirror of the right-door path.
                xFixed = unchecked(xFixed - step);
                xFixed = ReplaceWholePosition(
                    unchecked((ushort)(destinationX + 252 + (byte)(xFixed >> 16))),
                    xFixed);
                for (int frame = 1; frame < 64; frame++)
                    xFixed = unchecked(xFixed - step);
                break;

            case 2: // Down waits on setup frame zero, then advances on frames 1..56.
                yFixed = ReplaceWholePosition(
                    unchecked((ushort)(destinationY - 224 + (byte)(yFixed >> 16))),
                    yFixed);
                for (int frame = 1; frame <= 56; frame++)
                    yFixed = unchecked(yFixed + step);
                break;

            case 3: // Up starts one pixel below destination+screen after its setup fixup.
                yFixed = ReplaceWholePosition(
                    unchecked((ushort)(destinationY + 255 + (byte)(yFixed >> 16))),
                    yFixed);
                for (int frame = 1; frame <= 56; frame++)
                    yFixed = unchecked(yFixed - step);
                break;
        }

        // PlaceSamusLoadTiles replaces both whole positions, not only the transition axis.
        if ((direction & 2) == 0)
        {
            yFixed = ReplaceWholePosition(
                unchecked((ushort)(destinationY + (byte)(samus.YPosition))),
                yFixed);
        }
        else
        {
            xFixed = ReplaceWholePosition(
                unchecked((ushort)(destinationX + (byte)(samus.XPosition))),
                xFixed);
        }

        ushort finalX = unchecked((ushort)(xFixed >> 16));
        ushort finalY = unchecked((ushort)(yFixed >> 16));
        if ((finalX & 0x00f0) == 0x0010)
            finalX = unchecked((ushort)((finalX | 0x000f) + 8));
        else if ((finalX & 0x00f0) == 0x00e0)
            finalX = unchecked((ushort)((finalX & 0xfff0) - 8));
        if ((finalY & 0x00f0) == 0x0010)
            finalY = unchecked((ushort)((finalY | 0x000f) + 8));

        // LoadMoreThings applies this eight-pixel doorway alignment only horizontally.
        if ((direction & 2) == 0)
            finalX = direction == 0 ? (ushort)(finalX | 7) : (ushort)(finalX & 0xfff8);
        xFixed = ReplaceWholePosition(finalX, xFixed);
        yFixed = ReplaceWholePosition(finalY, yFixed);
        // `$80:ADC8` adds $20 to door_destination_y_pos after using the unmodified value
        // for Samus's setup origin. The IRQ completion later snaps an upward transition's
        // camera to that adjusted destination; the other three directions retain theirs.
        ushort finalCameraY = direction == 3
            ? unchecked((ushort)(destinationY + 32))
            : destinationY;
        return new DoorTransitionPlacement(destinationX, finalCameraY, xFixed, yFixed);
    }

    private static uint ReplaceWholePosition(ushort whole, uint fixedPosition) =>
        ((uint)whole << 16) | (fixedPosition & 0xffff);

    /// <summary>
    /// Applies $90:F1E9's Ceres-start Samus pose/state after room loading.
    /// </summary>
    /// <remarks>
    /// The native routine intentionally replaces the load-station Y coordinate with zero;
    /// elevator platform/projectile motion later carries front-facing Samus into the room.
    /// That elevator actor is a separate pending subsystem, so this method does not invent a
    /// floor placement or prematurely unlock controls.
    /// </remarks>
    public void InitializeCeresStartSamus()
    {
        if (ActiveRoom is null || ActiveLoadStation is null)
            throw new InvalidOperationException("The starting Ceres room must be loaded before Samus setup.");

        Samus = new SamusState
        {
            Pose = 0x00,
            AnimationFrame = 0,
            XPosition = ActiveLoadStation.SamusX,
            YPosition = 0,
        };
        Samus.LoadPowerSuitPalette(_addressSpace, Cgram);
        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.InitializeAnimation(_addressSpace);
        Samus.LiquidPhysics.AreaIndex = ActiveRoom.AreaIndex;
        Samus.LiquidPhysics.RoomIndex = ActiveRoom.RoomIndex;
        Samus.PrimeGraphics(_addressSpace);
        PreviousMovementTypeForXray = Samus.ReadMovementType(_addressSpace);

        // `$90:F21B/$90:F226` use SpawnEprojWithGfx and thus briefly seed both objects
        // from enemy slot zero. Their shared initializer at `$86:A301`, however, runs
        // before the first draw and explicitly clears that graphics index. The specialized
        // arrival model owns that native post-initialization value; passing the transient
        // enemy word here previously recolored the level-data concealer teal.
        CeresElevatorArrival = new CeresElevatorArrivalState(
            _addressSpace,
            Samus);

        // The locked Ceres-start frame handler still publishes the initial minimap. The
        // ordinary update below used to be gated on GroundedSamusMovementEnabled, leaving
        // the ROM HUD template's unrelated map glyphs visible for the entire descent. Seed
        // the real area-six room coordinate now; subsequent normal frames keep updating it.
        ActiveRoomGeometry roomGeometry = GetActiveRoomGeometry();
        Hud.UpdateMinimap(
            _addressSpace,
            roomGeometry.AreaIndex,
            roomGeometry.MapX,
            roomGeometry.MapY,
            LevelData?.WidthInBlocks ?? ActiveRoom.WidthInScreens * 16,
            LevelData?.HeightInBlocks ?? ActiveRoom.HeightInScreens * 16,
            Samus.XPosition,
            Samus.YPosition,
            NmiFrameCounter8,
            hasAreaMap: false);

        // `SamusCode_08_SetupForCeresStart` installs a locked frame handler. Reuse the
        // existing runtime switch to ensure ordinary movement cannot begin before the
        // elevator arrival sequence has been translated.
        GroundedSamusMovementEnabled = false;
    }

    /// <summary>Returns room-header geometry for either supported cartridge room path.</summary>
    private ActiveRoomGeometry GetActiveRoomGeometry()
    {
        if (ActiveRoom is not null)
        {
            return new ActiveRoomGeometry(
                ActiveRoom.AreaIndex,
                ActiveRoom.MapX,
                ActiveRoom.MapY,
                ActiveRoom.UpScroller,
                ActiveRoom.DownScroller);
        }

        if (LandingSiteEntry is not null)
        {
            return new ActiveRoomGeometry(
                LandingSiteEntry.AreaIndex,
                LandingSiteEntry.RoomMapX,
                LandingSiteEntry.RoomMapY,
                LandingSiteEntry.UpScroller,
                LandingSiteEntry.DownScroller);
        }

        throw new InvalidOperationException("No cartridge room header is active.");
    }
}

/// <summary>Room-header fields shared by camera tracking and minimap publication.</summary>
internal readonly record struct ActiveRoomGeometry(
    byte AreaIndex,
    byte MapX,
    byte MapY,
    byte UpScroller,
    byte DownScroller);

/// <summary>Final native door-scroll camera and 16.16 Samus coordinates.</summary>
internal readonly record struct DoorTransitionPlacement(
    ushort CameraX,
    ushort CameraY,
    uint SamusXFixed,
    uint SamusYFixed);
