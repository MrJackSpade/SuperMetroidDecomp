using SuperMetroid.Core.Game;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Runtime;

/// <summary>Cartridge-driven room loading kept separate from the gameplay-frame monolith.</summary>
public sealed partial class SuperMetroidRuntime
{
    // The IRQ owns these coordinates while state $0B waits inside LoadMoreThings. Keep
    // that ownership explicit instead of reducing the native scroll to a frontend timer.
    private DoorOpeningScrollState? _doorOpeningScroll;
    private DoorOpeningPpuScroll? _pendingDoorOpeningPpuScroll;

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
        LoadStationEntry station = LoadStationEntry.Load(
            _addressSpace,
            areaIndex: AreaId.Ceres,
            stationIndex: 0);
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(_addressSpace, station.DoorPointer);
        if (door.DestinationRoomPointer != station.RoomPointer)
        {
            throw new InvalidDataException(
                $"Ceres station room $8F:{station.RoomPointer:X4} disagrees with door " +
                $"$83:{door.Pointer:X4}'s destination $8F:{door.DestinationRoomPointer:X4}.");
        }

        CartridgeRoomHeader room = LoadCartridgeRoomHeader(door.DestinationRoomPointer);
        if (room.AreaIndex != AreaId.Ceres)
        {
            throw new InvalidDataException(
                $"Fresh Ceres station targets area ${(byte)room.AreaIndex:X2}, expected area $06.");
        }

        ActiveLoadStation = station;
        return LoadCartridgeRoom(
            door,
            room,
            station.CameraX,
            station.CameraY,
            RoomViewportLoadMode.DisplayInitialViewport);
    }

    /// <summary>
    /// Restores an existing slot through the cartridge's area/load-station tables and
    /// enters the normal playable state at the station-authored Samus coordinates.
    /// </summary>
    /// <remarks>
    /// Native startup displays a short front-facing Samus appearance before settling into
    /// ordinary standing control. That presentation coroutine is not a room-loading
    /// responsibility, so this method establishes its deterministic endpoint rather than
    /// replaying guessed timing. Room selection, coordinates, inventory, progression bits,
    /// enemies, PLMs, graphics, beam upload, and camera all remain cartridge driven.
    /// </remarks>
    public InitialViewportResult InitializeSavedGame(SuperMetroidSaveSlot slot)
    {
        ArgumentNullException.ThrowIfNull(slot);
        if (slot.Area > byte.MaxValue || slot.SaveStation > byte.MaxValue)
        {
            throw new InvalidDataException(
                $"Save slot {slot.Slot} contains an invalid area/station pair " +
                $"{slot.Area}:{slot.SaveStation}.");
        }

        AreaId requestedArea = AreaIds.FromCartridge(
            unchecked((byte)slot.Area),
            $"Save slot {slot.Slot}");
        byte stationIndex = unchecked((byte)slot.SaveStation);

        LoadStationEntry station = LoadStationEntry.Load(
            _addressSpace,
            requestedArea,
            stationIndex);
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(_addressSpace, station.DoorPointer);
        if (door.DestinationRoomPointer != station.RoomPointer)
        {
            throw new InvalidDataException(
                $"Load station {requestedArea}:{stationIndex} room $8F:{station.RoomPointer:X4} " +
                $"disagrees with door $83:{door.Pointer:X4}'s destination " +
                $"$8F:{door.DestinationRoomPointer:X4}.");
        }

        // Apply progression before room construction. Enemy and PLM initialization query
        // boss/event/item bytes synchronously, while room-state selection and beam graphics
        // depend on those same words. In particular, loading a post-Morph-Ball save must not
        // accidentally construct the untouched new-game version of Blue Brinstar.
        Samus = new SamusState
        {
            Pose = SamusPoseIds.FacingRightNormalPose,
            AnimationFrame = 0,
            XPosition = station.SamusX,
            YPosition = station.SamusY,
        };
        slot.ApplyTo(Samus, System);
        ControllerBindings = slot.ControllerBindings;
        MoonwalkEnabled = slot.MoonwalkEnabled;
        IconCancelEnabled = slot.IconCancelEnabled;
        GameTime.Load(
            slot.GameTimeFrames,
            slot.GameTimeSeconds,
            slot.GameTimeMinutes,
            slot.GameTimeHours);
        CartridgeRoomHeader room = LoadCartridgeRoomHeader(door.DestinationRoomPointer);
        if (room.AreaIndex != requestedArea)
        {
            throw new InvalidDataException(
                $"Load station {requestedArea}:{stationIndex} targets room area " +
                $"{room.AreaIndex}.");
        }
        ActiveLoadStation = station;
        CeresElevatorArrival = null;
        InitialViewportResult viewport = LoadCartridgeRoom(
            door,
            room,
            station.CameraX,
            station.CameraY,
            RoomViewportLoadMode.DisplayInitialViewport);
        // LoadFromLoadStation writes `$1E75 = 1`. Without this room-entry lockout, the
        // first standing floor probe after loading directly on a save pod immediately
        // reopens its confirmation message.
        Plms.LockSaveStationForCurrentRoomEntry();

        // Command nine does not begin at its endpoint. It selects a front-facing pose,
        // frame two/timer three, a suit-specific bank-$8D palette object, and the 360-call
        // appearance handler before restoring ordinary input.
        Samus.LoadSuitPalette(_addressSpace, Cgram);
        Samus.ApplyForwardFacingPoseSetup(_addressSpace);
        Samus.SetAnimationFrameFromSpecialHandler(frame: 2, timer: 3);
        Samus.LiquidPhysics.RoomIdentity = room.Identity;
        Samus.PrimeGraphics(_addressSpace);
        _samusLoadAppearancePaletteFxDefinition =
            Samus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? RoomLoadingRomData.GravitySuitLoadPaletteFx
                : Samus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? RoomLoadingRomData.VariaSuitLoadPaletteFx
                    : RoomLoadingRomData.PowerSuitLoadPaletteFx;
        RoomPaletteFx.SpawnDefinition(
            _addressSpace,
            _samusLoadAppearancePaletteFxDefinition,
            Samus.EquippedItems,
            System.HasAnyBossBits(room.AreaIndex, BossBits.AreaMiniBoss));
        _samusLoadAppearanceFramesRemaining = RoomLoadingRomData.SavedGameAppearanceFrameCount;
        Samus.InputLocked = true;
        PreviousMovementTypeForXray = Samus.ReadMovementType(_addressSpace);
        GroundedSamusMovementEnabled = false;
        return viewport;
    }

    /// <summary>
    /// Executes loading state <c>$06</c>'s special <c>loading_game_state=$22</c> branch:
    /// Crateria area zero, load-station eighteen, with the airborne Landing Site gunship.
    /// </summary>
    public InitialViewportResult InitializePostCeresZebesRoom()
    {
        SamusState samus = Samus
            ?? throw new InvalidOperationException(
                "The Ceres escape must retain its live Samus state through the cinematic.");

        // `$82:8038-$8048` assigns these indexes immediately before LoadFromLoadStation.
        // Station eighteen is a cutscene-only entry high above Landing Site; station zero
        // is not selected until GunshipTop_7 finishes the landing and performs the save.
        LoadStationEntry station = LoadStationEntry.Load(
            _addressSpace,
            areaIndex: AreaId.Crateria,
            stationIndex: 18);
        CartridgeDoorHeader door = CartridgeDoorHeader.Load(_addressSpace, station.DoorPointer);
        if (door.DestinationRoomPointer != station.RoomPointer)
        {
            throw new InvalidDataException(
                $"Post-Ceres station room $8F:{station.RoomPointer:X4} disagrees with " +
                $"door $83:{door.Pointer:X4}'s destination $8F:{door.DestinationRoomPointer:X4}.");
        }

        CartridgeRoomHeader room = LoadCartridgeRoomHeader(door.DestinationRoomPointer);
        if (room.AreaIndex != AreaId.Crateria)
        {
            throw new InvalidDataException(
                $"Post-Ceres station eighteen targets area {room.AreaIndex}, expected Crateria.");
        }

        // Samus_Initialize clears motion/pose state but deliberately retains inventory and
        // progression. CADF refilled health on the preceding cinematic frame.
        samus.Health = samus.MaxHealth;
        samus.Pose = 0;
        samus.AnimationFrame = 0;
        samus.Kinematics.SetXFixed((uint)station.SamusX << 16);
        samus.Kinematics.SetYFixed((uint)station.SamusY << 16);
        samus.Kinematics.YSpeed = 0;
        samus.Kinematics.YSubspeed = 0;
        samus.InputLocked = true;
        ActiveLoadStation = station;

        // The special loader still executes the normal room-object teardown before it
        // constructs Landing Site. CeresElevatorArrival models two bank-$86 projectiles
        // from the abandoned station room; retaining it here lets its delayed Y=$0048
        // completion overwrite the Samus position currently carried by the gunship.
        CeresElevatorArrival = null;

        InitialViewportResult viewport = LoadCartridgeRoom(
            door,
            room,
            station.CameraX,
            station.CameraY,
            RoomViewportLoadMode.DisplayInitialViewport,
            GunshipLoadScenario.EscapingCeres);
        samus.LoadSuitPalette(_addressSpace, Cgram);
        samus.RefreshCollisionRadii(_addressSpace);
        samus.InitializeAnimation(_addressSpace);
        samus.LiquidPhysics.RoomIdentity = room.Identity;
        samus.PrimeGraphics(_addressSpace);
        PreviousMovementTypeForXray = samus.ReadMovementType(_addressSpace);
        GroundedSamusMovementEnabled = false;
        return viewport;
    }

    /// <summary>
    /// True after bank-$94's type-$9 handler has selected a normal destination door and
    /// before the frontend's states $09-$0B consume it.
    /// </summary>
    public bool HasPendingDoorTransition => LevelData?.PendingDoorTransition is not null;

    /// <summary>The unconsumed bank-$83 door selected by the type-$9 collision handler.</summary>
    public CartridgeDoorHeader? PendingDoorTransition => LevelData?.PendingDoorTransition;

    /// <summary>
    /// Runs one <c>$82:E310</c> source-room alignment call. Horizontal doors converge the
    /// camera Y low byte; vertical doors converge X. Signed low-byte motion deliberately
    /// takes <c>$FF -> $00</c> and <c>$01 -> $00</c> in one-pixel steps.
    /// </summary>
    /// <returns>True only when the relevant low byte was already zero on entry.</returns>
    public bool AlignPendingDoorCameraOnePixel()
    {
        CartridgeDoorHeader door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door exists to align.");
        if (Camera is null)
            throw new InvalidOperationException("Door alignment requires an active camera.");

        bool alignsX = DoorTransitionAlignsX(door.Orientation);
        ushort coordinate = alignsX ? Camera.XPosition : Camera.YPosition;
        byte low = unchecked((byte)coordinate);
        if (low == 0)
            return true;

        coordinate = (low & 0x80) != 0
            ? unchecked((ushort)(coordinate + 1))
            : unchecked((ushort)(coordinate - 1));
        Camera.SetPosition(
            alignsX ? coordinate : Camera.XPosition,
            alignsX ? Camera.YPosition : coordinate);

        // CalculateLayer2PosAndScrollsWhenScrolling updates the PPU mirrors on every
        // convergence step. The gameplay scroll owner performs the same parallax math;
        // its generated block requests are intentionally discarded because native door
        // alignment does not enter the row/column streaming dispatcher here.
        BackgroundScroll.Layer1XPosition = Camera.XPosition;
        BackgroundScroll.Layer1YPosition = Camera.YPosition;
        _ = BackgroundScroll.StepScrolling();
        return false;
    }

    /// <summary>
    /// Decodes bank-$83's two-bit door direction into the axis aligned by $82:E310.
    /// The alignment is perpendicular to travel: right/left (zero/one) converge camera Y,
    /// while down/up (two/three) converge camera X. This literal branch matters because
    /// aligning the travel axis instead moves a horizontal door frame by a tile row while
    /// its room scroll is being staged.
    /// </summary>
    internal static bool DoorTransitionAlignsX(byte orientation) =>
        (orientation & 2) != 0;

    /// <summary>
    /// Runs the upward-only source-room tilemap correction at <c>$80:AD1D</c> before the
    /// destination loader replaces the active level-data producer.
    /// </summary>
    internal void FixPendingDoorTilesMovingUp()
    {
        CartridgeDoorHeader door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        if ((door.Orientation & 3) != 3)
            return;
        IReadOnlyList<BackgroundUpdateRequest> requests = BackgroundScroll.FixDoorsMovingUp();
        ExecuteBackgroundStreamRequests(requests, "upward source-door repair");
    }

    /// <summary>CRE bitset selected from the pending door's destination room header.</summary>
    public byte PendingDoorDestinationCreBitset => PendingDoorTransition is { } door
        ? LoadCartridgeRoomHeader(door.DestinationRoomPointer).CreBitset
        : throw new InvalidOperationException("No pending door destination exists.");

    /// <summary>
    /// Rewinds an atomically loaded destination to the position established by
    /// <c>DoorTransitionScrollingSetup</c> and <c>PlaceSamusLoadTiles</c>.
    /// </summary>
    /// <remarks>
    /// The room constructor necessarily loads tiles, PLMs, FX, and enemies in one host
    /// operation. Native code performs those operations while the door-opening IRQ is
    /// already active. Capturing the source fixed coordinates before that constructor and
    /// rewinding only the four IRQ-owned coordinate pairs preserves the same observable
    /// trajectory without pretending the loader itself is incremental.
    /// </remarks>
    internal void BeginDoorOpeningScroll(
        CartridgeDoorHeader door,
        uint sourceSamusXFixed,
        uint sourceSamusYFixed)
    {
        if (Camera is null || Samus is null)
            throw new InvalidOperationException("A loaded destination and Samus are required.");
        if (_doorOpeningScroll is not null)
            throw new InvalidOperationException("A door-opening scroll is already active.");

        _doorOpeningScroll = DoorOpeningScrollState.Create(
            door,
            sourceSamusXFixed,
            sourceSamusYFixed,
            Camera.XPosition,
            Camera.YPosition,
            BackgroundScroll.Layer2XPosition,
            BackgroundScroll.Layer2YPosition,
            Samus.Kinematics.XFixed,
            Samus.Kinematics.YFixed);
        int direction = door.Orientation & 3;
        DoorOpeningPpuScroll sourceScroll = _pendingDoorOpeningPpuScroll
            ?? throw new InvalidOperationException(
                "Door-opening scroll lost the source-room PPU scroll snapshot.");
        ushort stagedLayer1X = direction switch
        {
            0 => unchecked((ushort)(_doorOpeningScroll.CameraX - 4)),
            1 => unchecked((ushort)(_doorOpeningScroll.CameraX + 4)),
            _ => _doorOpeningScroll.CameraX,
        };
        ushort stagedLayer1Y = direction switch
        {
            2 => _doorOpeningScroll.CameraY,
            3 => unchecked((ushort)(_doorOpeningScroll.CameraY + 5)),
            _ => _doorOpeningScroll.CameraY,
        };
        ushort stagedLayer2Y = direction switch
        {
            2 => _doorOpeningScroll.Layer2Y,
            3 => unchecked((ushort)(_doorOpeningScroll.Layer2Y + 4)),
            _ => _doorOpeningScroll.Layer2Y,
        };
        BackgroundScroll.ConfigureDoorOpeningOffsets(
            sourceScroll.Bg1Horizontal,
            direction == 2
                ? unchecked((ushort)(sourceScroll.Bg1Vertical + 1))
                : sourceScroll.Bg1Vertical,
            stagedLayer1X,
            stagedLayer1Y);
        _pendingDoorOpeningPpuScroll = null;
        Camera.SetDoorTransitionPosition(
            _doorOpeningScroll.CameraX,
            _doorOpeningScroll.CameraY);
        BackgroundScroll.Layer1XPosition = _doorOpeningScroll.CameraX;
        BackgroundScroll.Layer1YPosition = _doorOpeningScroll.CameraY;
        BackgroundScroll.Layer2XPosition = _doorOpeningScroll.Layer2X;
        BackgroundScroll.Layer2YPosition = _doorOpeningScroll.Layer2Y;
        // Each native directional setup immediately invokes its first DoorTransition_*
        // step. That call both advances from +/-$100 to +/-$FC and streams the boundary
        // row/column exposed by the off-screen starting viewport. DoorOpeningScrollState
        // already incorporates that first four-pixel coordinate step, so execute the
        // matching producer request here as well. Merely calculating and discarding it
        // updated the previous-block words but left the corresponding VRAM ring-buffer
        // column stale—the source-side column visible on a left transition then appeared
        // one block too high when interpreted using the destination room's row origin.
        if ((door.Orientation & 2) == 0)
        {
            BackgroundScroll.PrimeHorizontalDoorOpeningBlocks(door.Orientation);
            IReadOnlyList<BackgroundUpdateRequest> initialRequests =
                BackgroundScroll.CalculateScrollsAndUpdates();
            ExecuteBackgroundStreamRequests(initialRequests, "horizontal door-opening setup");
        }
        else
        {
            IReadOnlyList<BackgroundUpdateRequest> initialRequests =
                BackgroundScroll.PrimeVerticalDoorOpeningBlocks(
                    door.Orientation,
                    stagedLayer1Y,
                    stagedLayer2Y);
            ExecuteBackgroundStreamRequests(initialRequests, "vertical door-opening setup");
        }
        Samus.Kinematics.SetXFixed(_doorOpeningScroll.SamusXFixed);
        Samus.Kinematics.SetYFixed(_doorOpeningScroll.SamusYFixed);
    }

    /// <summary>
    /// Runs one <c>Irq_FollowDoorTransition</c> coordinate update. True means the IRQ set
    /// bit $8000 in <c>door_transition_flag</c> on this call.
    /// </summary>
    internal bool StepDoorOpeningScroll()
    {
        DoorOpeningScrollState state = _doorOpeningScroll
            ?? throw new InvalidOperationException("No door-opening scroll is active.");
        if (Camera is null || Samus is null)
            throw new InvalidOperationException("Door-opening scroll lost its room actors.");
        bool completed = state.Advance();
        Camera.SetDoorTransitionPosition(state.CameraX, state.CameraY);
        BackgroundScroll.Layer1XPosition = state.CameraX;
        BackgroundScroll.Layer1YPosition = state.CameraY;
        BackgroundScroll.Layer2XPosition = state.Layer2X;
        BackgroundScroll.Layer2YPosition = state.Layer2Y;
        // The IRQ moves four pixels per call. Every fourth call crosses a 16-pixel block
        // boundary and `$80:A3E4` produces the newly exposed row/column DMA. Discarding
        // those requests left one stale ring-buffer column on horizontal transitions; on
        // a left door that stale source-door column was interpreted with the destination
        // row origin and appeared to jump upward by one 16x16 block.
        IReadOnlyList<BackgroundUpdateRequest> requests = state.ShouldStreamAfterAdvance
            ? BackgroundScroll.CalculateScrollsAndUpdates()
            : Array.Empty<BackgroundUpdateRequest>();
        ExecuteBackgroundStreamRequests(requests, "door-opening scroll");

        if (completed)
        {
            // `$80:AE4E` calls the direction routine first. That routine performs the
            // last `$80:A3A0` row/column calculation using the stepped coordinates.
            // The wrapper then snaps only layer one to the exact destination without
            // recalculating PPU scroll or streaming another row. Publishing the snap
            // before CalculateScrollsAndUpdates shifted an upward door's last request
            // into the next 16-pixel row and corrupted the elevator shaft ring buffer.
            state.SnapLayerOneToDestination();
            Camera.SetDoorTransitionPosition(state.CameraX, state.CameraY);
            BackgroundScroll.Layer1XPosition = state.CameraX;
            BackgroundScroll.Layer1YPosition = state.CameraY;
        }
        Samus.Kinematics.SetXFixed(state.SamusXFixed);
        Samus.Kinematics.SetYFixed(state.SamusYFixed);
        return completed;
    }

    /// <summary>
    /// Applies <c>$82:E6A2</c>'s doorway alignment after music has drained, then releases
    /// the temporary IRQ-owned trajectory. This is deliberately later than the scroll.
    /// </summary>
    internal void FinishDoorOpeningScroll()
    {
        DoorOpeningScrollState state = _doorOpeningScroll
            ?? throw new InvalidOperationException("No door-opening scroll is active.");
        if (state.RemainingFrames != 0 || Samus is null)
            throw new InvalidOperationException("Door-opening scroll has not reached its endpoint.");

        Samus.Kinematics.SetXFixed(state.FinalSamusXFixed);
        Samus.Kinematics.SetYFixed(state.FinalSamusYFixed);
        _doorOpeningScroll = null;
    }

    /// <summary>
    /// Immediately loads and displays the destination selected by a live type-$9 collision.
    /// This compatibility seam is reserved for headless room/mechanics diagnostics that do
    /// not execute frontend state $0B; playable code must use the incremental transition.
    /// </summary>
    public InitialViewportResult LoadPendingDoorDestination() =>
        LoadPendingDoorDestination(RoomViewportLoadMode.DisplayInitialViewport);

    /// <summary>
    /// Loads state $0B's destination without destroying the source-room VRAM ring. The
    /// following door IRQ replaces that ring incrementally while the screen slides.
    /// </summary>
    internal InitialViewportResult LoadPendingDoorDestinationForTransition()
    {
        CartridgeDoorHeader door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        return LoadPendingDoorDestination(RoomViewportLoadMode.StreamThroughDoor);
    }

    private InitialViewportResult LoadPendingDoorDestination(RoomViewportLoadMode viewportLoadMode)
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
        CartridgeRoomHeader room = LoadCartridgeRoomHeader(door.DestinationRoomPointer);

        // `$82:E8DD/$82:EB93` promotes a departing elevator's global status from one to
        // two after the destination PLMs, door ASM, and setup ASM have been created. Our
        // room constructor initializes enemies in one host call, so publish that carried
        // status immediately before Enemies.Load: the destination elevator initializer
        // must see two and place itself at parameter 2 instead of clearing the journey.
        bool arrivingByElevator = Enemies.ElevatorStatus == ElevatorActorStatus.Departing;
        if (arrivingByElevator)
            Enemies.PrepareElevatorArrival();

        // The load-station record established only the first room. Once bank $94 publishes
        // a door definition, that definition and its destination header become authoritative.
        ActiveLoadStation = null;
        CeresElevatorArrival = null;

        // State $0B's door IRQ has already placed Samus before LoadMoreThings constructs
        // the destination room. This ordering is observable for elevator arrivals:
        // Elevator_Init sees status two, moves the platform to parameter 2, and then pins
        // Samus to it. Applying the generic placement after Enemies.Load used to overwrite
        // that native attachment and exposed the upward arrival one row too low.
        Samus.Kinematics.SetXFixed(placement.SamusXFixed);
        Samus.Kinematics.SetYFixed(placement.SamusYFixed);
        Samus.Kinematics.YSpeed = 0;
        Samus.Kinematics.YSubspeed = 0;
        Samus.Kinematics.ExtraXDisplacement = 0;
        Samus.Kinematics.ExtraXSubdisplacement = 0;
        Samus.Kinematics.ExtraYDisplacement = 0;
        Samus.Kinematics.ExtraYSubdisplacement = 0;

        InitialViewportResult viewport = LoadCartridgeRoom(
            door,
            room,
            placement.CameraX,
            placement.CameraY,
            viewportLoadMode,
            runDoorClosingPlm: true);

        // `$82:E4B6` calls Samus_LoadSuitTargetPalette after room/enemy palettes have been
        // loaded and before state $0B captures the destination fade target. The source fade
        // has already driven OBJ palette four to black at this point. Omitting this shared
        // reload therefore retained valid OAM and tile data but rendered Samus as a solid
        // black silhouette after every ordinary desktop door transition.
        Samus.LoadSuitPalette(_addressSpace, Cgram);

        Samus.LiquidPhysics.RoomIdentity = room.Identity;
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

        CartridgeRoomHeader room = LoadCartridgeRoomHeader(roomPointer);
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
        InitialViewportResult viewport = LoadCartridgeRoom(
            door,
            room,
            cameraX,
            cameraY,
            RoomViewportLoadMode.DisplayInitialViewport);
        Samus.LiquidPhysics.RoomIdentity = room.Identity;
        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.PrimeGraphics(_addressSpace);
        GroundedSamusMovementEnabled = true;
        return viewport;
    }

    /// <summary>
    /// Loads a retail destination through its real bank-$83 header for exhaustive callback
    /// verification. Production reaches the same private loader through door collision.
    /// </summary>
    internal InitialViewportResult LoadCartridgeRoomThroughDoorForVerification(
        CartridgeDoorHeader door,
        ushort cameraX = 0,
        ushort cameraY = 0)
    {
        ArgumentNullException.ThrowIfNull(door);
        if (Samus is null)
            throw new InvalidOperationException("Door verification requires initialized Samus state.");

        CartridgeRoomHeader room = LoadCartridgeRoomHeader(door.DestinationRoomPointer);
        return LoadCartridgeRoom(
            door,
            room,
            cameraX,
            cameraY,
            RoomViewportLoadMode.DisplayInitialViewport,
            runDoorClosingPlm: true);
    }

    /// <summary>Shared cartridge room/state/graphics load used by stations and doors.</summary>
    private InitialViewportResult LoadCartridgeRoom(
        CartridgeDoorHeader door,
        CartridgeRoomHeader room,
        ushort cameraX,
        ushort cameraY,
        RoomViewportLoadMode viewportLoadMode,
        GunshipLoadScenario gunshipLoadScenario = GunshipLoadScenario.Ordinary,
        bool runDoorClosingPlm = false)
    {
        DoorOpeningPpuScroll? doorOpeningPpuScroll = viewportLoadMode switch
        {
            RoomViewportLoadMode.DisplayInitialViewport => null,
            RoomViewportLoadMode.StreamThroughDoor => new DoorOpeningPpuScroll(
                BackgroundScroll.Bg1HorizontalScroll,
                BackgroundScroll.Bg1VerticalScroll),
            _ => throw new ArgumentOutOfRangeException(
                nameof(viewportLoadMode), viewportLoadMode, "Unknown room viewport load mode."),
        };
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(_addressSpace, room);

        ActiveDoor = door;
        ActiveRoom = room;
        ActiveRoomAssets = assets;
        // Gameplay PPU initialization selects BG34NBA=$04 for every ordinary room load.
        // A later library-background command eight may override it to $02 for Kraid.
        GameplayHudCharacterBaseWord = SnesPpuLayout.GameplayHudCharacterBaseWord;
        _ceresFallingDebrisTimer = 0;
        WreckedShipTreadmill.Reset();
        MaridiaElevatube.Reset(
            active: room.State.MainCodePointer == RoomMainCodePointers.MaridiaElevatube);
        CeresElevatorShaft.Reset(
            active: room.State.MainCodePointer == RoomMainCodePointers.CeresElevatorShaft &&
                door.UsesCeresElevatorMode7);
        // Door setup `$8F:E4E0` writes these exact five registers before the fresh Ceres
        // elevator room becomes visible. Publishing the immutable transform here gives
        // Samus, her projectiles, and parameter-four/five steam a single authoritative
        // producer. Ordinary doors clear it so stale Mode-7 math cannot leak across rooms.
        SamusMode7Transform? initialMode7Transform = door.UsesCeresElevatorMode7
            ? new SamusMode7Transform(
                MatrixA: 0x0100,
                MatrixB: 0,
                MatrixC: 0,
                CenterX: 0x0080,
                CenterY: 0x03f0)
            : null;
        ActiveSamusMode7Transform = initialMode7Transform;
        // `$8F:E4E0` writes the initial matrix as part of door setup before the shaft is
        // exposed. Initialize the displayed copy as well; subsequent room-main changes
        // reach it only through RunNmi, matching the normal shadow-register path.
        DisplayedSamusMode7Transform = initialMode7Transform;
        LevelData = assets.LevelData;
        Camera = new ScrollBoundaryCamera(assets.Scrolls);
        if (viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor)
            Camera.SetDoorTransitionPosition(cameraX, cameraY);
        else
            Camera.SetPosition(cameraX, cameraY);
        BackgroundScroll.Layer2ScrollX = room.State.Layer2ScrollX;
        BackgroundScroll.Layer2ScrollY = room.State.Layer2ScrollY;
        if (viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport)
            BackgroundScroll.PrimePreviousBlocks();
        else
        {
            // Up calculates BG2 from destination+$1F before changing the IRQ's layer-one
            // endpoint to destination+$20. Keeping those adjacent words distinct prevents
            // a parallax room from accumulating a one-pixel layer mismatch at the snap.
            ushort layer2CalculationY = (door.Orientation & 3) == 3
                ? unchecked((ushort)(cameraY - 1))
                : cameraY;
            BackgroundScroll.PrepareDoorOpeningDestination(cameraX, layer2CalculationY);
            BackgroundScroll.Layer1YPosition = cameraY;
        }

        // The selected room-state main pointer, rather than the room or entry door, owns
        // the BG2 producer. `$8F:C116` calls the land-sky routine at `$88:AF8D`; `$8F:C120`
        // calls that same routine before its escape-quake work. Both set BG2SC=$4A and use
        // a 32x64 circular map at $4800. Ordinary gameplay instead uses BG2SC=$49 and keeps
        // its second 32x32 screen horizontally adjacent at $4C00.
        //
        // This distinction must be established in the shared loader. The older dedicated
        // Landing Site cinematic path already did so, but loading station 0 from SRAM came
        // through this method and therefore interpreted the vertical sky page at $4C00 as
        // the right half of a 64x32 map. Depending on BG2HOFS, that stale neighboring page
        // appeared as a broad vertical band of repeating purple tiles.
        bool usesLandScrollingSky =
            ScrollingSkyState.IsLandRoomMain(room.State.MainCodePointer);
        BackgroundStreamer = LevelData.CreateBackgroundStreamer(
            sizeOfBg2: usesLandScrollingSky ? (ushort)0 : (ushort)0x0800);
        ScrollingSky = usesLandScrollingSky
            ? new ScrollingSkyState(_addressSpace)
            : null;
        LandingSiteEntry = null;
        assets.LoadGraphics(Vram, Cgram);

        // `$82:E139` is part of every retail room-load/door-transition pipeline. It
        // replaces OBJ palette zero with `kCommonSpritesPalette1` before enemy setup;
        // bank-$86 actors created with SpawnEprojWithRoomGfx (graphics index zero) depend
        // on this row for lasers, death debris, and pickups. The old host loader restored
        // enemy, beam, and Samus palettes but left this row at a stale fade/menu value,
        // turning otherwise valid common projectiles solid black after a door.
        LoadGameplaySpritePalettes();

        // LoadFXHeader selects the door-specific sixteen-byte record, applies its palette
        // blend, uploads the common $8A BG3 tilemap, and creates the type-owned HDMA/
        // animtile state before palette-FX objects and enemies begin running.
        RoomLayer3Fx.Load(
            _addressSpace,
            Vram,
            Cgram,
            room.State.FxPointer,
            door.Pointer,
            System.RandomNumber);
        RoomLayer3Fx.PrimeViewport(Camera.XPosition, Camera.YPosition);
        if (Samus is not null)
            RoomLayer3Fx.ApplyToSamusLiquidPhysics(Samus.LiquidPhysics);

        // `$82:E4A9` calls LoadFXHeader after room setup and before enemies. Its selected
        // sixteen-byte record owns two independent object bitsets. The bank-$8D half must
        // be created on every shared room-load path; Landing Site bit zero replaces CGRAM
        // colors used by the scrolling-sky horizon on the first gameplay handler call.
        RoomPaletteFx.LoadRoom(
            _addressSpace,
            room.State.FxPointer,
            door.Pointer,
            room.AreaIndex,
            Samus?.EquippedItems ?? 0,
            System.HasAnyBossBits(room.AreaIndex, BossBits.AreaMiniBoss));

        // A negative background-data pointer names bank-$82's command interpreter. Rooms
        // whose layer-2 scroll mode is fixed/odd rely on this list as their only BG2 source;
        // it must run before the initial BG1 fill so a door cannot expose the previous
        // room's tilemap. Ceres $DF8D, for example, decompresses the Tourian statue hall to
        // $7E:4000 and copies it to both VRAM $4800 and $4C00.
        if (unchecked((short)room.State.BackgroundDataPointer) < 0)
        {
            LibraryBackgroundExecutionResult backgroundResult = LibraryBackgroundLoader.Execute(
                _addressSpace,
                Vram,
                room.State.BackgroundDataPointer,
                door.Pointer);
            if (backgroundResult.Bg3CharacterBaseWord is ushort bg3CharacterBaseWord)
                GameplayHudCharacterBaseWord = bg3CharacterBaseWord;
        }

        // `$82:E43A-$E472` destroys room-owned objects before constructing the destination.
        // Resetting these owners before Load/beam upload prevents stale projectile and PLM
        // indices from addressing a different room's new arrays on the first visible frame.
        Plms.Reset();
        BombProjectiles.Reset();
        Projectiles.Reset();

        // `$82:EB6C` walks this zero-terminated list once. Each six-byte record allocates
        // the highest free one of forty IDs before its bank-$84 setup executes. Immediate
        // deletion can therefore free that same ID for the next ROM record, and enemy code
        // can observe the resulting physical header/argument ordering. All room families
        // enter through this one parser; an unsupported header throws with complete record
        // context instead of being skipped by a collection of sibling scans.
        Plms.LoadRoomPopulation(
            _addressSpace,
            LevelData,
            BackgroundStreamer,
            Vram,
            room.State.PlmPointer,
            System,
            room.AreaIndex,
            getSamus: () => Samus,
            isAreaTorizoDefeated: () =>
                System.HasAnyBossBits(room.AreaIndex, BossBits.AreaTorizo),
            isTourianStatueFinished: () => Enemies.TourianEntranceStatueFinished,
            hasAreaBossBit: mask => System.HasAnyBossBits(room.AreaIndex, mask),
            hasEvent: System.HasEvent,
            setEvent: System.SetEvent,
            roomFx: RoomLayer3Fx,
            setEarthquakeTimer: value => Enemies.EarthquakeTimer = value,
            setEarthquakeType: value => Enemies.EarthquakeType = value,
            spawnNoobTubeProjectile: request =>
                Enemies.SpawnNoobTubeProjectile(request, LevelData.WidthInBlocks),
            spawnEyeDoorProjectile: request =>
                Enemies.SpawnEyeDoorProjectile(
                    request,
                    LevelData.WidthInBlocks,
                    System),
            disableDraygonCannon: Enemies.DisableDraygonCannon);

        // `$82:E8DD/$82:EB93` runs the bank-$8F door program only after destination PLMs
        // exist and before enemy initialization/initial viewport construction. Several of
        // those programs rewrite the room scroll array, so postponing this call until the
        // first gameplay frame would already have streamed a viewport with wrong camera
        // limits.
        RunDoorSetupCode(door);

        if (runDoorClosingPlm)
        {
            // `$82:E4C9` performs this handoff after destination PLMs, door/room setup,
            // and FX construction but before enemy initialization. The C# room constructor
            // groups those owners differently, yet this point preserves the observable
            // PLM-pool order: every room-authored actor exists before the closer requests
            // the next highest free slot, and no enemy-authored PLM has run yet.
            Plms.TrySpawnDoorClosingPlm(_addressSpace, LevelData, door, System);
        }

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
            setRoomScrollState: Camera.Scrolls.SetStorage,
            incrementMotherBrainGlassRoomArgument: Plms.IncrementMotherBrainGlassRoomArgument,
            readRoomScrollState: Camera.Scrolls.ReadState,
            setMotherBrainLayerBlendingDefaultConfig:
                value => LayerBlendingDefaultConfig = value,
            setMotherBrainBg2Scroll:
                (horizontal, vertical) =>
                    BackgroundScroll.SetBg2ScrollRegisters(horizontal, vertical),
            isRoomPlmPresent: Plms.HasActiveHeader,
            gunshipLoadScenario: gunshipLoadScenario);
        // Gate setup runs while the room PLM population is constructed, but Enemies.Load
        // subsequently clears the shared bank-$86 projectile pool. Consume those setup
        // requests here—the first point matching the cartridge's completed room teardown—
        // so the closed actor survives into the initial viewport.
        ApplyPendingDownwardGateProjectileRequests();
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
        SamusProjectileSystem.QueueBeamTilesAndLoadPalette(
            _addressSpace,
            VramWrites,
            Cgram,
            Samus?.EquippedBeams ?? 0);

        if (viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor)
        {
            // State-$0B never calls DisplayViewablePartOfRoom. The source-room tilemap must
            // remain in VRAM until each four-pixel IRQ crossing replaces one boundary row
            // or column. An eager destination fill destroys the circular-map seam before
            // the opening IRQ and visibly offsets door/elevator arrivals.
            _pendingDoorOpeningPpuScroll = doorOpeningPpuScroll;
            LastBackgroundUpdateCount = 0;
            return new InitialViewportResult(0, 0);
        }

        _pendingDoorOpeningPpuScroll = null;
        BackgroundScroll.Layer1XPosition = Camera.XPosition;
        BackgroundScroll.Layer1YPosition = Camera.YPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = BackgroundScroll.BuildInitialViewportRequests();
        LastBackgroundUpdateCount = requests.Count;
        int segmentCount = 0;
        foreach (BackgroundUpdateRequest request in requests)
        {
            TilemapStreamUpdate update = BackgroundStreamer.Build(request)
                ?? throw new InvalidOperationException("Initial room display unexpectedly requested Mode 7 streaming.");
            update.ExecuteTo(Vram);
            segmentCount += update.Segments.Count;
        }
        BackgroundScroll.PrimePreviousBlocks();
        return new InitialViewportResult(requests.Count, segmentCount);
    }

    /// <summary>
    /// Applies the two ROM-owned OBJ rows written by
    /// <c>LoadColorsForSpritesBeamsAndEnemies</c> at <c>$82:E139</c>.
    /// </summary>
    /// <remarks>
    /// OBJ palette five is not an enemy-specific palette. The cartridge restores it from
    /// <c>kInitialPalette</c> on every room load and uses it for the common bank-$86 sheet:
    /// enemy death explosions and drops, Pirate lasers, Ceres steam/timer sprites, and the
    /// Ceres elevator projectiles. A door fade blackens the live CGRAM row, so omitting this
    /// copy makes every one of those otherwise-correct objects render solid black after a
    /// transition. OBJ palette six is deliberately not copied here: native <c>$82:E139</c>
    /// preserves that row from the current palette buffer before Samus installs her suit
    /// target palette later in the load sequence.
    /// </remarks>
    private void LoadGameplaySpritePalettes()
    {
        Cgram.LoadFromBus(
            _addressSpace,
            RoomLoadingRomData.CommonGameplaySpritePalette,
            colorCount: 16,
            destinationIndex: RoomLoadingRomData.CommonGameplaySpritePaletteCgramIndex);
        Cgram.LoadFromBus(
            _addressSpace,
            RoomLoadingRomData.InitialEnemyProjectilePalette,
            colorCount: 16,
            destinationIndex: RoomLoadingRomData.InitialEnemyProjectilePaletteCgramIndex);
    }

    /// <summary>
    /// Resolves a bank-$8F room selector against the live SRAM-mirror and Samus inventory.
    /// </summary>
    /// <remarks>
    /// The fixed eleven-byte header contains the area index needed to choose that area's
    /// boss byte, but it precedes the selector program in ROM. A first lossless read obtains
    /// only that fixed metadata; the second read executes the selector with the exact facts
    /// consumed by $8F:E5FF-$E675. Keeping this at the shared loader boundary prevents a
    /// door, save station, or debug audit from quietly choosing a different room state.
    /// </remarks>
    private CartridgeRoomHeader LoadCartridgeRoomHeader(ushort roomPointer)
    {
        CartridgeRoomHeader fixedHeader = CartridgeRoomHeader.Load(_addressSpace, roomPointer);

        // RoomStateSelectionContext owns an immutable snapshot. Copying eight bytes is both
        // cheaper and safer than exposing Bank80SystemState's writable SRAM-mirror arrays.
        var events = new byte[Bank80SystemState.EventByteCount];
        for (int byteIndex = 0; byteIndex < events.Length; byteIndex++)
            events[byteIndex] = System.GetEventByteRaw(byteIndex);

        SamusState? samus = Samus;
        var selection = new RoomStateSelectionContext(
            events,
            BossBits: System.GetBossBits(fixedHeader.AreaIndex),
            HasMorphBallAndMissiles:
                samus is not null &&
                samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall) &&
                samus.MaxMissiles != 0,
            HasPowerBombs: samus?.MaxPowerBombs != 0);
        return CartridgeRoomHeader.Load(_addressSpace, roomPointer, selection);
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

        if (header == RoomPlmHeaders.ClearBotwoonWall)
        {
            // The already-defeated branch performs this 16-bit `$0101` store directly in
            // `$B3:959E`; unlike the live crumble PLM, it does not wait for instruction
            // `$84:AB51` during the first handler pass.
            Camera.Scrolls.SetLogicalState(0, 0, RoomScrollState.Blue);
            Camera.Scrolls.SetLogicalState(1, 0, RoomScrollState.Blue);
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
    /// Transfers Kraid's bank-$A7 hardcoded ceiling/platform calls into the shared bank-$84
    /// PLM pool before the current frame's handler pass.
    /// </summary>
    private void ApplyPendingKraidPlms()
    {
        if (Enemies.KraidPlmRequests.Count == 0)
            return;
        if (LevelData is null)
            throw new InvalidOperationException("Kraid published room PLMs without active level data.");

        foreach (KraidPlmRequest request in Enemies.KraidPlmRequests)
        {
            Plms.TrySpawnKraidRoomMutation(
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

    /// <summary>Transfers bank-$84 downward-gate spawn/wake work into bank $86.</summary>
    private void ApplyPendingDownwardGateProjectileRequests()
    {
        if (LevelData is null)
            throw new InvalidOperationException("Gate projectile work requires active room level data.");

        foreach (DownwardGateProjectileRequest request in Plms.TakeDownwardGateProjectileRequests())
            Enemies.ApplyDownwardGateProjectileRequest(request, LevelData.WidthInBlocks);
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

            case 3: // FixDoorsMovingUp carries counter one into setup's first moving call.
                yFixed = unchecked(yFixed - step);
                yFixed = ReplaceWholePosition(
                    unchecked((ushort)(destinationY + 251 + (byte)(yFixed >> 16))),
                    yFixed);
                for (int frame = 2; frame <= 56; frame++)
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
            Pose = SamusPoseIds.ForwardFacingPowerSuitPose,
            AnimationFrame = 0,
            XPosition = ActiveLoadStation.SamusX,
            YPosition = 0,
        };
        SamusState.LoadPowerSuitPalette(_addressSpace, Cgram);

        // Fresh-game loading has one deliberately non-general palette write after copying
        // every target color into the live palette: `$82:8190` clears color $DF (CGRAM
        // color 223) immediately before Samus command $08 spawns the two Ceres elevator
        // projectiles. The stationary level-data concealer is four solid pixels of common
        // OBJ tile $20, whose opaque pixel selects precisely color $DF. Leaving the normal
        // blue value from `Initial_Palette_spritePalette5` in that one entry makes the
        // elevator landing recess appear pre-filled; the cartridge's zero makes it black
        // until the moving pad reaches it and both projectiles delete themselves.
        Cgram.SetColor(223, 0);

        Samus.RefreshCollisionRadii(_addressSpace);
        Samus.InitializeAnimation(_addressSpace);
        Samus.LiquidPhysics.RoomIdentity = ActiveRoom.Identity;
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
            System,
            roomGeometry.AreaIndex,
            roomGeometry.MapX,
            roomGeometry.MapY,
            LevelData?.WidthInBlocks ?? ActiveRoom.WidthInScreens * 16,
            LevelData?.HeightInBlocks ?? ActiveRoom.HeightInScreens * 16,
            Samus.XPosition,
            Samus.YPosition,
            NmiFrameCounter8);

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
    AreaId AreaIndex,
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
