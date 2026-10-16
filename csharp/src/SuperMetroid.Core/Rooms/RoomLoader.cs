using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.WreckedShip;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Room loading: saved games and new sessions, cartridge rooms and doors, room setup, palettes, the HUD minimap,
/// enemies and PLMs of the loaded room.
/// </summary>
internal sealed class RoomLoader
{
    /// <summary>Supplies decoded graphics and palette assets for the room being entered.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Executes VRAM background uploads requested during room transitions.</summary>
    private readonly BackgroundScrollPhase _backgroundPhase;
    /// <summary>Tracks both background layers' scroll positions across door loading.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Resets bomb and projectile actors when constructing a destination room.</summary>
    private readonly SamusBombProjectileSystem _bombProjectiles;
    /// <summary>Owns the Ceres elevator shaft's room-specific startup state.</summary>
    private readonly CeresElevatorShaftRoomMainState _ceresElevatorShaft;
    /// <summary>Owns the haze effect installed by Ceres room loading.</summary>
    private readonly CeresHazeState _ceresHaze;
    /// <summary>Receives room and player palette data during a load.</summary>
    private readonly SnesCgram _cgram;
    /// <summary>Retains controller state needed when a room transition begins.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Publishes the display setup for a newly active room.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Coordinates the native door loader's staged transition states.</summary>
    private readonly DoorTransitionRunner _doorTransition;
    /// <summary>Creates and initializes the destination room's enemy slots.</summary>
    private readonly RoomEnemySystem _enemies;
    /// <summary>The session's BG tilemap staging, which library-background programs fill.</summary>
    private readonly Bg2TilemapStaging _bg2TilemapStaging;
    /// <summary>Applies room-scoped visual effects driven by enemies.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
    /// <summary>Installs escape-specific room behavior when entering the relevant room.</summary>
    private readonly EscapeRoomEffects _escapeEffects;
    /// <summary>Restores the save slot's accumulated game time.</summary>
    private readonly GameTimeState _gameTime;
    /// <summary>Holds HUD values carried into the loaded room.</summary>
    private readonly HudState _hud;
    /// <summary>Initializes the HUD presentation after room assets are available.</summary>
    private readonly GameplayHudSetup _hudSetup;
    /// <summary>Owns Maridia elevatube room setup during a load.</summary>
    private readonly MaridiaElevatubeRoomMainState _maridiaElevatube;
    /// <summary>Receives the door IRQ's request to advance the NMI counter.</summary>
    private readonly FrameClock _clock;
    /// <summary>Restores the saved controller and movement options.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Holds Samus and her load-time presentation state.</summary>
    private readonly SamusFrameState _player;
    /// <summary>The session's room scroll allocation; it outlives each room so implicit tables keep the previous room's tail.</summary>
    private readonly RoomScrollStorage _scrollStorage = new();
    /// <summary>Constructs room PLMs after persistent state has been restored.</summary>
    private readonly RoomPlmSystem _plms;
    /// <summary>Resets Samus projectiles for the destination room.</summary>
    private readonly SamusProjectileSystem _projectiles;
    /// <summary>Holds the active room, camera, selected door, and load-station identity.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Installs layer-three effects selected by the room state.</summary>
    private readonly RoomLayer3FxState _roomLayer3Fx;
    /// <summary>Starts palette effects belonging to the destination room.</summary>
    private readonly RoomPaletteFxSystem _roomPaletteFx;
    /// <summary>Resets animated spike tiles for the destination room.</summary>
    private readonly RoomSpikeAnimatedTilesState _roomSpikes;
    /// <summary>Resets animated treadmill tiles for the destination room.</summary>
    private readonly RoomTreadmillAnimatedTilesState _roomTreadmills;
    /// <summary>Resets animated sand tiles for the destination room.</summary>
    private readonly RoomSandAnimatedTilesState _sandAnimatedTiles;
    /// <summary>Provides persistent event and boss bits used by room-state selectors.</summary>
    private readonly Bank80SystemState _system;
    /// <summary>Applies tester inventory overrides after saved state is restored.</summary>
    private readonly TesterTools _tester;
    /// <summary>Coordinates the Tourian statue room sequence on room entry.</summary>
    private readonly TourianStatueSequence _tourianStatues;
    /// <summary>Receives the initial tile and sprite graphics uploads.</summary>
    private readonly SnesVram _vram;
    /// <summary>Queues VRAM writes that must occur at the next display boundary.</summary>
    private readonly VramWriteQueue _vramWrites;
    /// <summary>Resets Wrecked Ship's animated treadmill tiles on entry.</summary>
    private readonly WreckedShipTreadmillAnimatedTilesState _wreckedShipTreadmill;


    /// <summary>Captures the owners of room, player, graphics, and door state used by the loader.</summary>
    internal RoomLoader(RoomLoaderDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _backgroundPhase = dependencies.BackgroundPhase;
        _backgroundScroll = dependencies.BackgroundScroll;
        _bombProjectiles = dependencies.BombProjectiles;
        _ceresElevatorShaft = dependencies.CeresElevatorShaft;
        _ceresHaze = dependencies.CeresHaze;
        _cgram = dependencies.Cgram;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _doorTransition = dependencies.DoorTransition;
        _enemies = dependencies.Enemies;
        _bg2TilemapStaging = dependencies.Bg2TilemapStaging;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _escapeEffects = dependencies.EscapeEffects;
        _gameTime = dependencies.GameTime;
        _hud = dependencies.Hud;
        _hudSetup = dependencies.HudSetup;
        _maridiaElevatube = dependencies.MaridiaElevatube;
        _clock = dependencies.Clock;
        _options = dependencies.Options;
        _player = dependencies.Player;
        _plms = dependencies.Plms;
        _projectiles = dependencies.Projectiles;
        _room = dependencies.Room;
        _roomLayer3Fx = dependencies.RoomLayer3Fx;
        _roomPaletteFx = dependencies.RoomPaletteFx;
        _roomSpikes = dependencies.RoomSpikes;
        _roomTreadmills = dependencies.RoomTreadmills;
        _sandAnimatedTiles = dependencies.SandAnimatedTiles;
        _system = dependencies.System;
        _tester = dependencies.Tester;
        _tourianStatues = dependencies.TourianStatues;
        _vram = dependencies.Vram;
        _vramWrites = dependencies.VramWrites;
        _wreckedShipTreadmill = dependencies.WreckedShipTreadmill;

    }
    /// <summary>
    /// Whether the active door IRQ requests the native NMI-counter reset while state
    /// $0B waits in <c>LoadMoreThings</c>.
    /// </summary>
    private bool _doorScrollingIrqRequestsNmi;
    /// <summary>The four-pixel IRQ trajectory retained across atomic room construction.</summary>
    private DoorOpeningScrollState? _doorOpeningScroll;
    /// <summary>
    /// Elevator placement produced by the atomic room load but deferred until the native
    /// door loader reaches that enemy's initialization slot.
    /// </summary>
    private LoaderSamusPlacement? _pendingLoaderSamusPlacement;
    /// <summary>Source-room PPU scroll retained until door-opening IRQ offsets are installed.</summary>
    private DoorOpeningPpuScroll? _pendingDoorOpeningPpuScroll;

    /// <summary>
    /// Executes the data-loading portion of native loading-state $1F: area six, station
    /// zero, the station-selected room, and its fresh-save state-selection branch.
    /// </summary>
    public InitialViewportResult InitializeStartingCeresRoom()
    {
        // `InitAndLoadGameData_Async` writes these exact indexes before calling
        // LoadFromLoadStation. The compiled catalog preserves that general lookup rather
        // than duplicating the Ceres room/door/placement at this call site.
        LoadStationEntry station = LoadStationDefinitions.Get(
            areaIndex: AreaId.Ceres,
            stationIndex: 0);
        CartridgeDoorHeader door = DoorDefinitions.Get(station.DoorPointer);
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

        _room.ActiveLoadStation = station;
        return LoadCartridgeRoom(
            door,
            room,
            station.CameraPosition,
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
    public InitialViewportResult InitializeSavedGame(SuperMetroidSaveSlot slot, bool resetBossesOnLoad = false)
    {
        Ensure.NotNull(slot);
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

        LoadStationEntry station = LoadStationDefinitions.Get(
            requestedArea,
            stationIndex);
        CartridgeDoorHeader door = DoorDefinitions.Get(station.DoorPointer);
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
        RestoreSavedPlayerState(slot, resetBossesOnLoad);
        _player.IntroducedSamus.Position = _player.IntroducedSamus.Position.WithWholeX(station.SamusPosition.X);
        _player.IntroducedSamus.Position = _player.IntroducedSamus.Position.WithWholeY(station.SamusPosition.Y);
        CartridgeRoomHeader room = LoadCartridgeRoomHeader(door.DestinationRoomPointer);
        if (room.AreaIndex != requestedArea)
        {
            throw new InvalidDataException(
                $"Load station {requestedArea}:{stationIndex} targets room area " +
                $"{room.AreaIndex}.");
        }
        _room.ActiveLoadStation = station;
        _room.CeresElevatorArrival = null;
        InitialViewportResult viewport = LoadCartridgeRoom(
            door,
            room,
            station.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport);
        // LoadFromLoadStation writes `$1E75 = 1`. Without this room-entry lockout, the
        // first standing floor probe after loading directly on a save pod immediately
        // reopens its confirmation message.
        _plms.Stations.LockSaveStationForCurrentRoomEntry();

        // Command nine does not begin at its endpoint. It selects a front-facing pose,
        // frame two/timer three, a suit-specific bank-$8D palette object, and the 360-call
        // appearance handler before restoring ordinary input.
        _player.IntroducedSamus.Presentation.LoadSuitPalette(_cgram);
        _player.IntroducedSamus.PoseMetadata.ApplyZebesStartPoseSetup();
        _player.IntroducedSamus.Presentation.SetAnimationFrameFromSpecialHandler(frame: 2, timer: 3);
        _player.IntroducedSamus.LiquidPhysics.RoomIdentity = room.Identity;
        _player.IntroducedSamus.Presentation.PrimeGraphics();
        _player._samusLoadAppearancePaletteFxDefinition =
            _player.IntroducedSamus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? RoomLoadingRomData.GravitySuitLoadPaletteFx
                : _player.IntroducedSamus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? RoomLoadingRomData.VariaSuitLoadPaletteFx
                    : RoomLoadingRomData.PowerSuitLoadPaletteFx;
        _roomPaletteFx.SpawnDefinition(
            _player._samusLoadAppearancePaletteFxDefinition,
            _player.IntroducedSamus.EquippedItems,
            _system.HasAnyBossBits(room.AreaIndex, BossBits.AreaMiniBoss));
        _player._samusLoadAppearanceFramesRemaining = NativeCountdown.Start(RoomLoadingRomData.SavedGameAppearanceFrameCount);
        _player.IntroducedSamus.InputLocked = true;
        _player.PreviousMovementTypeForXray = _player.IntroducedSamus.PoseMetadata.ReadMovementType();
        _player.GroundedSamusMovementEnabled = false;
        return viewport;
    }

    /// <summary>
    /// Restores the player and persistent mirror before selecting rooms or a saved
    /// cinematic. A Ceres-destruction checkpoint has no playable Ceres room to load;
    /// its inventory must nevertheless survive into the Zebes landing sequence.
    /// </summary>
    internal void RestoreSavedPlayerState(SuperMetroidSaveSlot slot, bool resetBossesOnLoad = false)
    {
        Ensure.NotNull(slot);
        _player.Samus = new SamusState
        {
            Mode7QueueResidue = _enemies.Room.Mode7QueueResidue,
            Pose = SamusPoseId.FacingRightNormalPose,
            AnimationFrame = 0,
        };
        _artwork.BindSamusPalettePresentation();
        slot.ApplyTo(_player.Samus, _system);
        if (resetBossesOnLoad) _tester.ResetSavedEncounters();
        _tester.ApplyTesterInventory();
        // $91:E00D clears both persisted HUD selection words after mirror restore.
        // Otherwise the arm-cannon cover can leak into the front-facing load pose.
        _player.Samus.SelectedHudItem = 0;
        _player.Samus.AutoCancelHudItemIndex = 0;
        _options.ControllerBindings = slot.ControllerBindings;
        _options.MoonwalkEnabled = slot.MoonwalkEnabled;
        _options.IconCancelEnabled = slot.IconCancelEnabled;
        _gameTime.Load(slot.GameTimeFrames, slot.GameTimeSeconds,
            slot.GameTimeMinutes, slot.GameTimeHours);
    }

    /// <summary>
    /// Executes loading state <c>$06</c>'s special <c>loading_game_state=$22</c> branch:
    /// Crateria area zero, load-station eighteen, with the airborne Landing Site gunship.
    /// </summary>
    public InitialViewportResult InitializePostCeresZebesRoom()
    {
        SamusState carried = _player.Samus
            ?? throw new InvalidOperationException(
                "The Ceres escape must retain its live Samus state through the cinematic.");

        // `$82:8057` InitializeSamus clears all of Samus RAM, including the speeds and pose
        // state the escape left behind; only the inventory below it survives. The player is
        // the same one, so a tester grant already applied to her is not repeated.
        // $91:E12F clears Pose; the port's constructor defaults to the right-facing pose.
        var samus = new SamusState { Mode7QueueResidue = _enemies.Room.Mode7QueueResidue, Pose = 0, AnimationFrame = 0 };
        samus.CopyInventoryFrom(carried);
        _player.Samus = samus;
        if (ReferenceEquals(_tester.testerInventoryRecipient, carried))
            _tester.testerInventoryRecipient = samus;
        _artwork.BindSamusPalettePresentation();

        // `$82:8038-$8048` assigns these indexes immediately before LoadFromLoadStation.
        // Station eighteen is a cutscene-only entry high above Landing Site; station zero
        // is not selected until GunshipTop_7 finishes the landing and performs the save.
        LoadStationEntry station = LoadStationDefinitions.Get(
            areaIndex: AreaId.Crateria,
            stationIndex: 18);
        CartridgeDoorHeader door = DoorDefinitions.Get(station.DoorPointer);
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

        // CADF refilled health on the preceding cinematic frame; $91:E164 then seeds the
        // hurt check from it. The $22 branch locks Samus's state handlers.
        samus.Health = samus.MaxHealth;
        samus.PreviousHealthForHurtCheck = samus.Health;
        samus.Kinematics.Position = samus.Kinematics.Position.WithX(new SnesFixedPosition(station.SamusPosition.X, 0)).WithY(new SnesFixedPosition(station.SamusPosition.Y, 0));
        samus.InputLocked = true;
        _room.ActiveLoadStation = station;

        // The special loader still executes the normal room-object teardown before it
        // constructs Landing Site. CeresElevatorArrival models two bank-$86 projectiles
        // from the abandoned station room; retaining it here lets its delayed Y=$0048
        // completion overwrite the Samus position currently carried by the gunship.
        _room.CeresElevatorArrival = null;

        InitialViewportResult viewport = LoadCartridgeRoom(
            door,
            room,
            station.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            GunshipLoadScenario.EscapingCeres);
        samus.Presentation.LoadSuitPalette(_cgram);
        samus.PoseMetadata.RefreshCollisionRadii();
        samus.Presentation.InitializeAnimation();
        samus.LiquidPhysics.RoomIdentity = room.Identity;
        samus.Presentation.PrimeGraphics();
        _player.PreviousMovementTypeForXray = samus.PoseMetadata.ReadMovementType();
        _player.GroundedSamusMovementEnabled = false;
        return viewport;
    }

    /// <summary>
    /// True after bank-$94's type-$9 handler has selected a normal destination door and
    /// before the frontend's states $09-$0B consume it.
    /// </summary>
    public bool HasPendingDoorTransition => _room.LevelData?.PendingDoorTransition is not null;

    /// <summary>The unconsumed bank-$83 door selected by the type-$9 collision handler.</summary>
    public CartridgeDoorHeader? PendingDoorTransition => _room.LevelData?.PendingDoorTransition;

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
        if (_room.Camera is null)
            throw new InvalidOperationException("Door alignment requires an active camera.");

        DoorCameraAlignmentState alignment = DoorCameraAlignmentState.Step(
            door.Orientation.Direction,
            _room.Camera.PixelPosition);
        if (alignment.Completed)
            return true;

        // $82:E327-$E340 INC/DEC the whole layer-one word only; fractions, the ideal
        // camera position and Samus's previous position are untouched.
        _room.Camera.SetDoorTransitionPosition(alignment.CameraPosition);

        // CalculateLayer2PosAndScrollsWhenScrolling updates the PPU mirrors on every
        // convergence step. The gameplay scroll owner performs the same parallax math;
        // its generated block requests are intentionally discarded because native door
        // alignment does not enter the row/column streaming dispatcher here.
        _backgroundScroll.Layer1Position = _room.Camera.PixelPosition;
        _ = _backgroundScroll.StepScrolling();
        return false;
    }

    /// <summary>
    /// The axis aligned by $82:E310, perpendicular to travel: right/left converge camera Y,
    /// while down/up converge camera X. This literal branch matters because aligning the
    /// travel axis instead moves a horizontal door frame by a tile row while its room
    /// scroll is being staged.
    /// </summary>
    internal static bool DoorTransitionAlignsX(DoorDirection direction) => direction.IsVertical();

    /// <summary>
    /// Runs the upward-only source-room tilemap correction at <c>$80:AD1D</c> before the
    /// destination loader replaces the active level-data producer.
    /// </summary>
    internal void FixPendingDoorTilesMovingUp()
    {
        CartridgeDoorHeader door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        if (door.Orientation.Direction != DoorDirection.Up)
            return;
        IReadOnlyList<BackgroundUpdateRequest> requests = _backgroundScroll.FixDoorsMovingUp();
        _backgroundPhase.ExecuteBackgroundStreamRequests(requests, "upward source-door repair");
    }

    /// <summary>CRE bitset selected from the pending door's destination room header.</summary>
    public byte PendingDoorDestinationCreBitset => PendingDoorTransition is { } door
        ? LoadCartridgeRoomHeader(door.DestinationRoomPointer).CreBitset
        : throw new InvalidOperationException("No pending door destination exists.");

    /// <summary>
    /// Applies the layer designation shared by start, horizontal, and vertical door IRQs.
    /// BG2 is always disabled; BG1 is also disabled when either adjacent room requests the
    /// cartridge's CRE transition path.
    /// </summary>
    internal void BeginDoorTransitionIrqDisplay(byte previousCreBitset, byte creBitset)
    {
        if (_room.DoorTransitionMainScreenLayers is not null)
            throw new InvalidOperationException("Door-transition IRQ display is already active.");

        bool suppressBg1 =
            ((previousCreBitset | creBitset) & RoomCreBitsets.SuppressDoorTransitionBg1) != 0;
        _room.DoorTransitionMainScreenLayers = suppressBg1
            ? SnesMainScreenLayers.Obj
            : SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj;
    }

    /// <summary>Returns main-screen ownership to the room's ordinary gameplay IRQ.</summary>
    internal void EndDoorTransitionIrqDisplay()
    {
        if (_room.DoorTransitionMainScreenLayers is null)
            throw new InvalidOperationException("Door-transition IRQ display is not active.");
        // The final door scanout still uses the installed end-drawing command;
        // the new gameplay IRQ takes ownership on the following frame.
        PublishDoorScrollingIrqNmiRequest();
        _room.DoorTransitionMainScreenLayers = null;
        _doorScrollingIrqRequestsNmi = false;
    }

    /// <summary>Publishes the door scanout's end-of-drawing IRQ, which clears the 8-bit frame counter ($80:97BA/$9823).</summary>
    internal void PublishDoorScrollingIrqNmiRequest()
    {
        if (_doorScrollingIrqRequestsNmi) _clock.ClearCounter8ForDoorIrq();
    }

    /// <summary>
    /// Publishes setup's Samus displacement before destination-coordinate rebasing.
    /// The later atomic room constructor uses the captured pre-setup coordinates.
    /// </summary>
    internal void ApplyDoorScrollingSetupMovement()
    {
        CartridgeDoorHeader door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        if (_player.Samus is null)
            throw new InvalidOperationException("Door scrolling requires Samus.");
        _player.Samus.Position = DoorOpeningScrollState.ApplySetupMovement(door, _player.Samus.Position);
        PublishDoorCameraPosition(DoorOpeningScrollState.GetSetupCamera(door));
    }

    /// <summary>Rebases Samus and publishes the first moving IRQ during tile loading.</summary>
    internal void PlaceSamusForDoorTileLoading()
    {
        CartridgeDoorHeader door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        if (_player.Samus is null || _room.Camera is null)
            throw new InvalidOperationException("Door tile loading requires Samus and camera.");
        FixedPosition<RoomPixels> position = DoorOpeningScrollState.RebaseSamus(door, _player.Samus.Position);
        _room.Camera.PublishDoorSamusPosition(position.Whole);
        _player.Samus.Position = DoorOpeningScrollState.AdvanceSamus(door, position);
        DoorDirection direction = door.Orientation.Direction;
        short delta = direction is DoorDirection.Right or DoorDirection.Down ? (short)4 : (short)-4;
        bool vertical = direction.IsVertical();
        Offset16<RoomPixels> firstStep = vertical ? new(0, delta) : new(delta, 0);
        PublishDoorCameraPosition(DoorOpeningScrollState.GetSetupCamera(door) + firstStep);
        _room.Camera.PublishDoorSamusTravelAxis(vertical, _player.Samus.Position.Whole);
        _doorScrollingIrqRequestsNmi = true;
        PublishDoorScrollingIrqNmiRequest();
    }

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
        FixedPosition<RoomPixels> sourceSamus,
        IDoorLoaderProgressSource loaderProgress,
        int completedLoadingIrqSteps = 0)
    {
        if (_room.Camera is null || _player.Samus is null)
            throw new InvalidOperationException("A loaded destination and Samus are required.");
        if (_doorOpeningScroll is not null)
            throw new InvalidOperationException("A door-opening scroll is already active.");
        if (_enemies.Elevator.ConsumeElevatorArrivalPlacementSlot() is int placementSlot)
            _pendingLoaderSamusPlacement = new LoaderSamusPlacement(placementSlot, _player.Samus.Position.Whole);

        _doorOpeningScroll = DoorOpeningScrollState.Create(
            door,
            sourceSamus,
            _room.Camera.PixelPosition,
            _backgroundScroll.Layer2Position,
            _player.Samus.Position);
        DoorDirection direction = door.Orientation.Direction;
        DoorOpeningPpuScroll sourceScroll = _pendingDoorOpeningPpuScroll
            ?? throw new InvalidOperationException(
                "Door-opening scroll lost the source-room PPU scroll snapshot.");
        // The offsets derive from the layer-one words staged before setup's first call:
        // horizontal setups had not yet taken their four-pixel step, and up's
        // FixDoorsMovingUp path stages layer one five (layer two four) pixels lower.
        Offset16<RoomPixels> layer1Staging = direction switch
        {
            DoorDirection.Right => new(-4, 0),
            DoorDirection.Left => new(4, 0),
            DoorDirection.Down => Offset16<RoomPixels>.Zero,
            DoorDirection.Up => new(0, 5),
            _ => throw new InvalidOperationException($"Undefined door direction {direction}."),
        };
        Position16<RoomPixels> stagedLayer1 = _doorOpeningScroll.CameraPosition + layer1Staging;
        ushort stagedLayer2Y = direction == DoorDirection.Up
            ? unchecked((ushort)(_doorOpeningScroll.Layer2Position.Y + 4))
            : _doorOpeningScroll.Layer2Position.Y;
        Position16<RoomPixels> retainedBg1Scroll = direction == DoorDirection.Down
            ? sourceScroll.Bg1Scroll + new Offset16<RoomPixels>(0, 1)
            : sourceScroll.Bg1Scroll;
        _backgroundScroll.ConfigureDoorOpeningOffsets(retainedBg1Scroll, stagedLayer1);
        _pendingDoorOpeningPpuScroll = null;
        PublishDoorOpeningLayers(_doorOpeningScroll);
        // Each native directional setup immediately invokes its first DoorTransition_*
        // step. That call both advances from +/-$100 to +/-$FC and streams the boundary
        // row/column exposed by the off-screen starting viewport. DoorOpeningScrollState
        // already incorporates that first four-pixel coordinate step, so execute the
        // matching producer request here as well. Merely calculating and discarding it
        // updated the previous-block words but left the corresponding VRAM ring-buffer
        // column stale—the source-side column visible on a left transition then appeared
        // one block too high when interpreted using the destination room's row origin.
        if (!door.Orientation.IsVertical)
        {
            _backgroundScroll.PrimeHorizontalDoorOpeningBlocks(door.Orientation.Direction);
            IReadOnlyList<BackgroundUpdateRequest> initialRequests =
                _backgroundScroll.CalculateScrollsAndUpdates();
            _backgroundPhase.ExecuteBackgroundStreamRequests(initialRequests, "horizontal door-opening setup");
        }
        else
        {
            IReadOnlyList<BackgroundUpdateRequest> initialRequests =
                _backgroundScroll.PrimeVerticalDoorOpeningBlocks(
                    door.Orientation.Direction,
                    stagedLayer1.Y,
                    stagedLayer2Y);
            _backgroundPhase.ExecuteBackgroundStreamRequests(initialRequests, "vertical door-opening setup");
        }
        _player.Samus.Position = _doorOpeningScroll.SamusPosition;
        _room.Camera.PublishDoorSamusPosition(_player.Samus.Position.Whole);
        // Recreate the VRAM producer requests for IRQ progress already published
        // before the atomic destination load. Those steps do not consume more input.
        for (int step = 0; step < completedLoadingIrqSteps; step++)
            StepDoorOpeningScroll(loaderProgress);
    }

    /// <summary>
    /// Runs one <c>Irq_FollowDoorTransition</c> coordinate update. True means the IRQ set
    /// bit $8000 in <c>door_transition_flag</c> on this call.
    /// </summary>
    internal bool StepDoorOpeningScroll(IDoorLoaderProgressSource loaderProgress)
    {
        DoorOpeningScrollState state = _doorOpeningScroll
            ?? throw new InvalidOperationException("No door-opening scroll is active.");
        if (_room.Camera is null || _player.Samus is null)
            throw new InvalidOperationException("Door-opening scroll lost its room actors.");
        bool completed = state.Advance();
        PublishDoorOpeningLayers(state);
        // The IRQ moves four pixels per call. Every fourth call crosses a 16-pixel block
        // boundary and `$80:A3E4` produces the newly exposed row/column DMA. Discarding
        // those requests left one stale ring-buffer column on horizontal transitions; on
        // a left door that stale source-door column was interpreted with the destination
        // row origin and appeared to jump upward by one 16x16 block.
        IReadOnlyList<BackgroundUpdateRequest> requests = state.ShouldStreamAfterAdvance
            ? _backgroundScroll.CalculateScrollsAndUpdates()
            : Array.Empty<BackgroundUpdateRequest>();
        _backgroundPhase.ExecuteBackgroundStreamRequests(requests, "door-opening scroll");

        if (completed)
        {
            // `$80:AE4E` calls the direction routine first. That routine performs the
            // last `$80:A3A0` row/column calculation using the stepped coordinates.
            // The wrapper then snaps only layer one to the exact destination without
            // recalculating PPU scroll or streaming another row. Publishing the snap
            // before CalculateScrollsAndUpdates shifted an upward door's last request
            // into the next 16-pixel row and corrupted the elevator shaft ring buffer.
            state.SnapLayerOneToDestination();
            PublishDoorCameraPosition(state.CameraPosition);
        }
        // The IRQ runs at the start of the update; the loader's main-thread work follows.
        if (_pendingLoaderSamusPlacement is { } placement)
        {
            if (loaderProgress.HasInitializedEnemySlot(placement.EnemySlot))
            {
                state.PlaceSamusOnElevator(placement.Position);
                _pendingLoaderSamusPlacement = null;
            }
            else if (completed)
            {
                throw new InvalidDataException(
                    $"The door scroll finished before the loader initialized elevator slot {placement.EnemySlot}.");
            }
        }
        _enemies.LoaderHooks.CompleteRinkaLoaderTimeCameraReads(loaderProgress.HasInitializedEnemySlot, _room.Camera.PixelPosition);
        if (completed && _enemies.LoaderHooks.HasDeferredRinkaLoaderTimeCameraReads)
            throw new InvalidDataException("The door scroll finished before the loader initialized every special Rinka.");
        _player.Samus.Position = state.SamusPosition;
        _room.Camera.PublishDoorSamusTravelAxis(state.Direction.IsVertical(), _player.Samus.Position.Whole);
        return completed;
    }

    /// <summary>
    /// Publishes the IRQ-owned layer-one and layer-two words to the camera (whose fraction
    /// words the IRQ never writes) and to the background-scroll owner.
    /// </summary>
    private void PublishDoorOpeningLayers(DoorOpeningScrollState state)
    {
        PublishDoorCameraPosition(state.CameraPosition);
        _backgroundScroll.Layer2Position = state.Layer2Position;
    }

    /// <summary>Publishes a door-owned layer-one position to both the camera and the scroll owner.</summary>
    private void PublishDoorCameraPosition(Position16<RoomPixels> camera)
    {
        _room.LoadedCamera.SetDoorTransitionPosition(camera);
        _backgroundScroll.Layer1Position = camera;
    }

    /// <summary>
    /// Applies <c>$82:E544</c>'s horizontal eight-pixel alignment after the
    /// post-scroll NMI, before the music wait and the later narrow-door nudges.
    /// </summary>
    internal void AlignSamusAfterDoorLoading()
    {
        DoorOpeningScrollState state = _doorOpeningScroll
            ?? throw new InvalidOperationException("No door-opening scroll is active.");
        if (state.RemainingFrames != 0 || _player.Samus is null)
            throw new InvalidOperationException("Door loading has not finished scrolling.");
        switch (state.Direction)
        {
            case DoorDirection.Right:
                _player.Samus.Position = _player.Samus.Position.WithWholeX(unchecked((ushort)(_player.Samus.Position.X.Whole | (7))));
                break;
            case DoorDirection.Left:
                _player.Samus.Position = _player.Samus.Position.WithWholeX(unchecked((ushort)(_player.Samus.Position.X.Whole & ~7)));
                break;
            case DoorDirection.Down:
            case DoorDirection.Up:
                break;
            default:
                throw new InvalidOperationException($"Undefined door direction {state.Direction}.");
        }
    }

    /// <summary>
    /// Applies <c>$82:E6A2</c> after music has drained: the doorway nudges to Samus's live
    /// position, then an elevator door's Samus command (7 going down, 0 going up). Releases
    /// the temporary IRQ-owned trajectory. This is deliberately later than the scroll.
    /// </summary>
    internal void FinishDoorOpeningScroll()
    {
        DoorOpeningScrollState state = _doorOpeningScroll
            ?? throw new InvalidOperationException("No door-opening scroll is active.");
        if (state.RemainingFrames != 0 || _player.Samus is null)
            throw new InvalidOperationException("Door-opening scroll has not reached its endpoint.");
        if (_pendingLoaderSamusPlacement is not null)
            throw new InvalidOperationException("The door loader's Samus placement is still pending.");

        ushort x = NudgeOffScreenRightEdge(NudgeOffScreenLeftEdge(_player.Samus.Position.X.Whole));
        ushort y = NudgeOffScreenLeftEdge(_player.Samus.Position.Y.Whole);
        _player.Samus.Position = _player.Samus.Position.WithWhole(new(x, y));

        if (_enemies.Elevator.ElevatorFlags != 0)
        {
            if ((short)_enemies.Elevator.ElevatorDirection >= 0)
                _player.Samus.PoseMetadata.SetupForElevator();
            else
                _player.Samus.InputLocked = true;
        }
        _doorOpeningScroll = null;
    }

    /// <summary>
    /// Loads state $0B's destination without destroying the source-room VRAM ring. The
    /// following door IRQ replaces that ring incrementally while the screen slides.
    /// </summary>
    internal InitialViewportResult LoadPendingDoorDestinationForTransition(FixedPosition<RoomPixels> sourceSamus)
    {
        CartridgeDoorHeader door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        return LoadPendingDoorDestination(RoomViewportLoadMode.StreamThroughDoor, sourceSamus);
    }

    /// <summary>
    /// Builds the selected door's destination while preserving the source position needed
    /// to replay door-opening scroll when the viewport is streamed rather than replaced.
    /// </summary>
    private InitialViewportResult LoadPendingDoorDestination(
        RoomViewportLoadMode viewportLoadMode, FixedPosition<RoomPixels>? sourcePosition = null)
    {
        if (_room.LevelData is null || _player.Samus is null)
            throw new InvalidOperationException("A live room and Samus are required for a door transition.");

        RoomLevelData sourceLevel = _room.LevelData;
        CartridgeDoorHeader door = sourceLevel.PendingDoorTransition
            ?? throw new InvalidOperationException("No type-$9 door collision is pending.");
        if (door.DestinationRoomPointer < SnesBusLayout.RomWindowStart)
        {
            throw new InvalidOperationException(
                $"Elevator pseudo-door $83:{door.Pointer:X4} cannot enter the normal room loader.");
        }

        DoorTransitionPlacement placement = CalculateDoorTransitionPlacement(door, sourcePosition ?? _player.Samus.Position);
        CartridgeRoomHeader room = LoadCartridgeRoomHeader(door.DestinationRoomPointer);

        // Native transition teardown deletes direct G-Mode's stranded X-Ray HDMA object,
        // but it does not restore the four subsystem enable words. Destination room setup
        // therefore begins in indirect G-Mode and cannot create a visible gate actor.
        _player.Samus.Xray.TransitionDirectGModeToIndirect();

        // The desktop's opt-in error reporter resumes at the next emulated-frame boundary.
        // Cartridge code cannot throw, but translated room construction can still reject an
        // unsupported definition. Build the read-only asset graph before consuming the
        // source room's pending door so such a failure remains retryable instead of leaving
        // state $0B with no destination on the following frame.
        CartridgeRoomAssets assets = CartridgeRoomAssets.Load(_scrollStorage, room, RequireRoomAssetCatalogs());
        CartridgeDoorHeader consumedDoor = sourceLevel.ConsumePendingDoorTransition()
            ?? throw new InvalidOperationException(
                "Pending door disappeared while its destination assets were prepared.");
        if (consumedDoor.Pointer != door.Pointer)
        {
            throw new InvalidOperationException(
                $"Pending door changed from $83:{door.Pointer:X4} to " +
                $"$83:{consumedDoor.Pointer:X4} while its destination assets were prepared.");
        }

        // `$82:E8DD/$82:EB93` promotes a departing elevator's global status from one to
        // two after the destination PLMs, door ASM, and setup ASM have been created. Our
        // room constructor initializes enemies in one host call, so publish that carried
        // status immediately before Enemies.Load: the destination elevator initializer
        // must see two and place itself at parameter 2 instead of clearing the journey.
        bool arrivingByElevator = _enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Departing;
        if (arrivingByElevator)
            _enemies.Elevator.PrepareElevatorArrival();

        // The load-station record established only the first room. Once bank $94 publishes
        // a door definition, that definition and its destination header become authoritative.
        _room.ActiveLoadStation = null;
        _room.CeresElevatorArrival = null;

        // State $0B's door IRQ has already placed Samus before LoadMoreThings constructs
        // the destination room. This ordering is observable for elevator arrivals:
        // Elevator_Init sees status two, moves the platform to parameter 2, and then pins
        // Samus to it. Applying the generic placement after Enemies.Load used to overwrite
        // that native attachment and exposed the upward arrival one row too low.
        _player.Samus.Position = placement.SamusPosition;
        // Door IRQ travel has its own speed words. Ordinary jump velocity survives
        // native room loading and the final nudge; clearing it here restarts a fall
        // at its apex and lets horizontal momentum carry Samus too far after exit.
        _player.Samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.Zero;

        InitialViewportResult viewport = LoadCartridgeRoom(
            door,
            room,
            placement.CameraPosition,
            viewportLoadMode,
            runDoorClosingPlm: true,
            preloadedAssets: assets);

        // `$82:E4B6` calls Samus_LoadSuitTargetPalette after room/enemy palettes have been
        // loaded and before state $0B captures the destination fade target. The source fade
        // has already driven OBJ palette four to black at this point. Omitting this shared
        // reload therefore retained valid OAM and tile data but rendered Samus as a solid
        // black silhouette after every ordinary desktop door transition.
        _player.Samus.Presentation.LoadSuitPalette(_cgram);

        // The door loader writes no SamusYRadius; the next unlocked alpha's SetSamusRadius
        // does, so an elevator rider keeps the boarding pose's radius through the door.
        _player.Samus.LiquidPhysics.RoomIdentity = room.Identity;
        _player.Samus.Presentation.PrimeGraphics();
        _player.GroundedSamusMovementEnabled = true;
        return viewport;
    }

    /// <summary>Shared cartridge room/state/graphics load used by stations and doors.</summary>
    internal InitialViewportResult LoadCartridgeRoom(
        CartridgeDoorHeader door,
        CartridgeRoomHeader room,
        Position16<RoomPixels> camera,
        RoomViewportLoadMode viewportLoadMode,
        GunshipLoadScenario gunshipLoadScenario = GunshipLoadScenario.Ordinary,
        bool runDoorClosingPlm = false,
        CartridgeRoomAssets? preloadedAssets = null)
    {
        DoorOpeningPpuScroll? doorOpeningPpuScroll = viewportLoadMode switch
        {
            RoomViewportLoadMode.DisplayInitialViewport => null,
            RoomViewportLoadMode.StreamThroughDoor => new DoorOpeningPpuScroll(_backgroundScroll.Bg1Scroll),
            _ => throw new ArgumentOutOfRangeException(
                nameof(viewportLoadMode), viewportLoadMode, "Unknown room viewport load mode."),
        };
        CartridgeRoomAssets assets = preloadedAssets ??
            CartridgeRoomAssets.Load(_scrollStorage, room, RequireRoomAssetCatalogs());

        _room.ActiveDoor = door;
        _room.ActiveRoom = room;
        _room.ActiveRoomAssets = assets;
        // Gameplay PPU initialization selects BG34NBA=$04 for every ordinary room load.
        // A later library-background command eight may override it to $02 for Kraid.
        _hudSetup.SelectGameplayHudCharacterBase();
        _wreckedShipTreadmill.Reset();
        _maridiaElevatube.Reset(
            active: room.State.MainCallback == RoomMainCallback.MaridiaElevatube);
        _ceresElevatorShaft.Reset(
            active: room.State.MainCallback == RoomMainCallback.CeresElevatorShaft &&
                door.UsesCeresElevatorMode7,
            _room.RoomMainScratch);
        // Door setup `$8F:E4E0` writes these exact five registers before the fresh Ceres
        // elevator room becomes visible. Publishing the immutable transform here gives
        // Samus, her projectiles, and parameter-four/five steam a single authoritative
        // producer. Ordinary doors clear it so stale Mode-7 math cannot leak across rooms.
        SamusMode7Transform? initialMode7Transform = door.UsesCeresElevatorMode7
            ? new SamusMode7Transform(
                MatrixA: 256, // 1.0 in Mode 7's 8.8 fixed point.
                MatrixB: 0,
                MatrixC: 0,
                Center: CeresElevatorShaftRoomMainState.CeresShaftMode7Center)
            : null;
        _display.ActiveSamusMode7Transform = initialMode7Transform;
        // `$8F:E4E0` writes the initial matrix as part of door setup before the shaft is
        // exposed. Initialize the displayed copy as well; subsequent room-main changes
        // reach it only through RunNmi, matching the normal shadow-register path.
        _display.PublishDoorMode7(initialMode7Transform);
        _room.LevelData = assets.LevelData;
        ScrollBoundaryCamera? previousCamera = _room.Camera;
        _room.Camera = new ScrollBoundaryCamera(assets.Scrolls);
        if (viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor)
        {
            if (previousCamera is not null) _room.Camera.InheritTrackingState(previousCamera);
            _room.Camera.SetDoorTransitionPosition(camera, previousCamera?.Position ?? _room.InheritedLayer1Position);
        }
        else
        {
            _room.Camera.SetPosition(camera);
            if (previousCamera is not null)
                _room.Camera.RetainSubpositions(previousCamera);
            else
                _room.Camera.RetainSubpositions(_room.InheritedLayer1Position);
        }
        _backgroundScroll.Layer2Scroll = room.State.Layer2Scroll;
        if (viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport)
            _backgroundScroll.PrimePreviousBlocks();
        else
        {
            // Up calculates BG2 from destination+$1F before changing the IRQ's layer-one
            // endpoint to destination+$20. Keeping those adjacent words distinct prevents
            // a parallax room from accumulating a one-pixel layer mismatch at the snap.
            Position16<RoomPixels> parallaxBasis = door.Orientation.Direction == DoorDirection.Up
                ? camera - new Offset16<RoomPixels>(0, 1)
                : camera;
            _backgroundScroll.PrepareDoorOpeningDestination(camera, parallaxBasis);
        }

        // The main callback streams offscreen tilemap rows even in Crateria's rock room.
        // Horizontal motion and BG2SC=$4A belong to the separately spawned sky HDMA object
        // ($88:A7D8 / $88:ADC2), not to the tilemap callback ($88:AFA3).
        ushort selectedFx = RoomFxRecordDefinitions.Select(room.State.FxPointer, door.Pointer);
        bool skyHdmaConfigured =
            (selectedFx != 0 && RoomFxRecordDefinitions.Get(selectedFx).Type == (byte)RoomFxType.ScrollingSky) ||
            room.State.SetupCallback is RoomSetupCallback.ScrollingSkyLand or
                RoomSetupCallback.ScrollingSkyOcean or RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape;
        _room.BackgroundStreamer = _room.LevelData.CreateBackgroundStreamer(
            sizeOfBg2: skyHdmaConfigured ? (ushort)0 : (ushort)2048);
        _room.ScrollingSky = ScrollingSkyState.IsScrollingSkyRoomMain(room.State.MainCallback)
            ? new ScrollingSkyState(skyHdmaConfigured)
            : null;
        _room.LandingSiteEntry = null;
        assets.LoadGraphics(_vram, _cgram);

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
        _roomLayer3Fx.Load(
            _vram,
            _cgram,
            room.State.FxPointer,
            door.Pointer,
            _system.RandomNumber,
            room.Pointer);
        _roomLayer3Fx.PrimeViewport(_room.Camera.PixelPosition);
        _ceresHaze.Load(RoomCallbackDefinitions.SpawnsCeresHaze(room.State.SetupCallback),
            _system.HasAnyBossBits(room.AreaIndex, BossBits.AreaBoss),
            viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport);
        _sandAnimatedTiles.LoadRoom(room.State.FxPointer, door.Pointer,
            room.AreaIndex);
        _roomSpikes.LoadRoom(room.State.FxPointer, door.Pointer,
            room.AreaIndex);
        _roomTreadmills.LoadRoom(room.State.FxPointer, door.Pointer,
            room.AreaIndex);
        if (_player.Samus is not null)
            _roomLayer3Fx.ApplyToSamusLiquidPhysics(_player.Samus.LiquidPhysics);

        // `$82:E4A9` calls LoadFXHeader after room setup and before enemies. Its selected
        // sixteen-byte record owns two independent object bitsets. The bank-$8D half must
        // be created on every shared room-load path; Landing Site bit zero replaces CGRAM
        // colors used by the scrolling-sky horizon on the first gameplay handler call.
        _roomPaletteFx.LoadRoom(
            room.State.FxPointer,
            door.Pointer,
            room.AreaIndex,
            _player.Samus?.EquippedItems ?? 0,
            _system.HasAnyBossBits(room.AreaIndex, BossBits.AreaMiniBoss));

        // A negative background-data pointer names bank-$82's command interpreter. Rooms
        // whose layer-2 scroll mode is fixed/odd rely on this list as their only BG2 source;
        // it must run before the initial BG1 fill so a door cannot expose the previous
        // room's tilemap. Ceres $DF8D, for example, decompresses the Tourian statue hall to
        // $7E:4000 and copies it to both VRAM $4800 and $4C00.
        if (unchecked((short)room.State.BackgroundDataPointer) < 0)
        {
            LibraryBackgroundExecutionResult backgroundResult = LibraryBackgroundLoader.Execute(new LibraryBackgroundStaging(_bg2TilemapStaging, _enemies.Room.Staging), _vram, room.State.BackgroundDataPointer, door.Pointer, new LibraryBackgroundArtwork { TilemapArt = _artwork.RoomBackgroundTilemapArt, SkyArt = _artwork.RoomSkyTilemapArt, HudArt = _artwork.MapPresentation?.HudTiles, CharacterArt = _artwork.RoomCharacterArt });
            if (backgroundResult.Bg3CharacterBaseWord is ushort bg3CharacterBaseWord)
                _display.GameplayHudCharacterBaseWord = bg3CharacterBaseWord;
        }

        // `$82:E43A-$E472` destroys room-owned objects before constructing the destination.
        // Resetting these owners before Load/beam upload prevents stale projectile and PLM
        // indices from addressing a different room's new arrays on the first visible frame.
        _plms.Reset();
        _bombProjectiles.Reset();
        _projectiles.ResetForRoomTransition(_player.Samus);

        // `$82:EB6C` walks this zero-terminated list once. Each six-byte record allocates
        // the highest free one of forty IDs before its bank-$84 setup executes. Immediate
        // deletion can therefore free that same ID for the next ROM record, and enemy code
        // can observe the resulting physical header/argument ordering. All room families
        // enter through this one parser; an unsupported header throws with complete record
        // context instead of being skipped by a collection of sibling scans.
        var areaServices = new RoomEnemyRuntimeServices(_system, _room, _player, _plms, _display, _backgroundScroll, room.AreaIndex);
        var plmServices = new RoomPlmRuntimeServices(_enemies, _player, _room, _system);
        _plms.PopulationLoader.LoadRoomPopulation(
            new PlmRoom(_room.LevelData, _room.BackgroundStreamer, _vram, _system, room.AreaIndex),
            RoomPlmPopulationDefinition.FromCompiled(room.State.PlmPointer),
            new RoomPlmServices
            {
                Samus = plmServices,
                Bosses = areaServices.Bosses,
                Events = areaServices.Events,
                TourianStatue = plmServices,
                RoomFx = _roomLayer3Fx,
                Earthquake = plmServices,
                Enemies = plmServices,
            });

        // `$82:E8DD/$82:EB93` runs the bank-$8F door program only after destination PLMs
        // exist and before enemy initialization/initial viewport construction. Several of
        // those programs rewrite the room scroll array, so postponing this call until the
        // first gameplay frame would already have streamed a viewport with wrong camera
        // limits.
        _doorTransition.RunDoorSetupCode(door);
        if (room.State.SetupCallback == RoomSetupCallback.SetupShaktoolRoomPlm)
            _plms.Shaktool.TrySpawnShaktoolRoomController(_room.Camera.Scrolls);
        if (room.State.SetupCallback == RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape)
            _plms.EscapeAnimals.SetupCrittersEscapeBlock(_room.LevelData,
                _room.LevelData.GetBlockIndex(new(EscapeAnimalPlmRomData.WallX, EscapeAnimalPlmRomData.WallY)));
        // `$8F:91A9` spawns the fake-wall PLM; SpawnHardcodedPLM silently drops it when all
        // forty native slots are occupied.
        if (room.State.SetupCallback == RoomSetupCallback.AutoDestroyWallDuringEscape)
            _plms.Tourian.TrySpawnOldTourianEscapeShaftWall(_room.LevelData);
        // `$8F:9194` also spawns this PLM before writing the quake SetupEscapeRoomEffects owns.
        if (room.State.SetupCallback == RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen)
            _plms.EscapeAnimals.TrySpawnCrateriaMainstreetEscapePassage(_room.LevelData);

        if (runDoorClosingPlm)
        {
            // `$82:E4C9` performs this handoff after destination PLMs, door/room setup,
            // and FX construction but before enemy initialization. The C# room constructor
            // groups those owners differently, yet this point preserves the observable
            // PLM-pool order: every room-authored actor exists before the closer requests
            // the next highest free slot, and no enemy-authored PLM has run yet.
            _plms.DoorPlms.TrySpawnDoorClosingPlm(_room.LevelData, door, _system);
        }

        // $82:E4CC updates the entering beam before $82:E4D8 initializes enemies. Golden Torizo's code mutates
        // equipment during initialization; its grant must not retroactively select the beam loaded for this room.
        _artwork.UpdateBeamTilesAndPalette(_cgram, _player.Samus?.EquippedBeams ?? 0);

        _enemies.Load(new RoomEnemyLoadRequest(
            CompiledRoomEnemyDefinitions.Instance,
            room.State.EnemyPopulationPointer,
            room.State.EnemyTilesetPointer,
            _vram,
            _cgram,
            new RoomEnemyRuntimeServices(_system, _room, _player, _plms, _display, _backgroundScroll, room.AreaIndex))
        {
            Level = _room.LevelData,
            Samus = _player.Samus,
            ControllerInput = _controller1.Current,
            Camera = _room.Camera.PixelPosition,
            GunshipScenario = gunshipLoadScenario,
            DeferLoaderTimeCameraReads = viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor,
        });
        // The shared Ridley initializer calls $90:A7E2 only for a live boss.
        // Ceres also disables the HUD map, but has no matching exploration list.
        if (_enemies.Ceres.RidleyPresent)
        {
            _hud.DisableMinimapForBoss();
            if (_enemies.RoomStatus.BossId == RidleyMapDefinitions.NorfairBossId)
                for (int row = 0; row < RidleyMapDefinitions.ArenaScreenRows; row++)
                    _system.MarkExploredMapTile(room.AreaIndex, room.MapPosition + new Offset16<MapTiles>(0, checked((short)(row + 1))));
        }
        _escapeEffects.SetupEscapeRoomEffects(room.State.SetupCallback);
        // Gate setup runs while the room PLM population is constructed, but Enemies.Load
        // subsequently clears the shared bank-$86 projectile pool. Consume those setup
        // requests here—the first point matching the cartridge's completed room teardown—
        // so the closed actor survives into the initial viewport.
        _enemyRoomEffects.ApplyPendingDownwardGateProjectileRequests();
        // Setup ASM $8F:C8C8 spawns into the pool that Enemies.Load has just cleared.
        if (room.State.SetupCallback == RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile)
            _enemies.LoaderHooks.SpawnPrePhantoonRoomProjectile(_backgroundScroll);
        _enemyRoomEffects.ApplyEnemyRequestedRoomEffects(_enemies.LastEnemyFrame.RoomEffects);
        _tourianStatues.Load();
        _enemies.PopulationLoader.QueueGraphicsUploads(_vramWrites);

        if (viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor)
        {
            // State-$0B never calls DisplayViewablePartOfRoom. The source-room tilemap must
            // remain in VRAM until each four-pixel IRQ crossing replaces one boundary row
            // or column. An eager destination fill destroys the circular-map seam before
            // the opening IRQ and visibly offsets door/elevator arrivals.
            _pendingDoorOpeningPpuScroll = doorOpeningPpuScroll;
            return new InitialViewportResult();
        }

        _pendingDoorOpeningPpuScroll = null;
        _backgroundScroll.Layer1Position = _room.Camera.PixelPosition;
        IReadOnlyList<BackgroundUpdateRequest> requests = _backgroundScroll.BuildInitialViewportRequests();
        int segmentCount = 0;
        foreach (BackgroundUpdateRequest request in requests)
        {
            TilemapStreamUpdate update = _room.BackgroundStreamer.Build(request)
                ?? throw new InvalidOperationException("Initial room display unexpectedly requested Mode 7 streaming.");
            update.ExecuteTo(_vram);
            segmentCount += update.Segments.Count;
        }
        _backgroundScroll.PrimePreviousBlocks();
        return new InitialViewportResult();
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
    internal void LoadGameplaySpritePalettes()
    {
        GameplayBasePaletteCatalog artwork = _artwork.InitialPaletteArt ?? throw new InvalidOperationException(
            "Room loading requires the installed gameplay palette catalog.");
        artwork.LoadCommonSprites(_cgram,
            RoomLoadingRomData.CommonGameplaySpritePaletteCgramIndex);
        artwork.LoadEnemyProjectileSprites(_cgram,
            RoomLoadingRomData.InitialEnemyProjectilePaletteCgramIndex);
    }

    /// <summary>
    /// Resolves a compiled room selector against the live SRAM-mirror and Samus inventory.
    /// </summary>
    /// <remarks>
    /// The fixed eleven-byte header still supplies the area index needed to choose that
    /// area's boss byte. The ordered selector program is application-owned definition data;
    /// no production path executes bank-$8F selector bytes. Keeping this at the shared loader
    /// boundary prevents doors, save stations, and attract scenes from choosing differently.
    /// </remarks>
    internal CartridgeRoomHeader LoadCartridgeRoomHeader(ushort roomPointer)
    {
        AreaId areaIndex = CartridgeRoomHeader.ReadAreaIndex(roomPointer);

        // RoomStateSelectionContext owns an immutable snapshot. Copying eight bytes is both
        // cheaper and safer than exposing Bank80SystemState's writable SRAM-mirror arrays.
        var events = new byte[Bank80SystemState.EventByteCount];
        for (int byteIndex = 0; byteIndex < events.Length; byteIndex++)
            events[byteIndex] = _system.GetEventByteRaw(byteIndex);

        SamusState? samus = _player.Samus;
        var selection = new RoomStateSelectionContext(
            events,
            BossBits: _system.GetBossBits(areaIndex),
            HasMorphBallAndMissiles:
                samus is not null &&
                samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall) &&
                samus.MaxMissiles != 0,
            HasPowerBombs: samus?.MaxPowerBombs != 0);
        return CartridgeRoomHeader.LoadUsingCompiledSelection(roomPointer, selection);
    }

    /// <summary>
    /// Replays the position arithmetic from <c>$80:AD30-$AF89</c> and <c>$82:E3C0/E6A2</c>
    /// to obtain the final camera/Samus coordinates after the native scrolling phase.
    /// </summary>
    private static DoorTransitionPlacement CalculateDoorTransitionPlacement(
        CartridgeDoorHeader door,
        FixedPosition<RoomPixels> source)
    {
        DoorDirection direction = door.Orientation.Direction;

        // Setup's first directional call (none for down), then PlaceSamusLoadTiles replaces
        // both whole positions, then every remaining IRQ call moves along the travel axis.
        FixedPosition<RoomPixels> samus = DoorOpeningScrollState.RebaseSamus(
            door, DoorOpeningScrollState.ApplySetupMovement(door, source));
        FixedVector<RoomPixels> step = DoorOpeningScrollState.GetSamusStep(door);
        for (int call = DoorOpeningScrollState.MovingIrqCalls(direction); call > 0; call--)
            samus = samus.Advance(step);

        ushort finalX = samus.X.Whole;
        ushort finalY = samus.Y.Whole;
        finalX = NudgeOffScreenRightEdge(NudgeOffScreenLeftEdge(finalX));
        finalY = NudgeOffScreenLeftEdge(finalY);

        // LoadMoreThings applies this eight-pixel doorway alignment only horizontally.
        if (!direction.IsVertical())
            finalX = direction == DoorDirection.Right ? (ushort)(finalX | 7) : unchecked((ushort)(finalX & ~7));
        samus = samus.WithWhole(new Position16<RoomPixels>(finalX, finalY));
        // `$80:ADC8` adds $20 to door_destination_y_pos after using the unmodified value
        // for Samus's setup origin. The IRQ completion later snaps an upward transition's
        // camera to that adjusted destination; the other three directions retain theirs.
        Position16<RoomPixels> destination = DoorOpeningScrollState.GetDestination(door);
        Position16<RoomPixels> finalCamera = direction == DoorDirection.Up
            ? destination + new Offset16<RoomPixels>(0, 32)
            : destination;
        return new DoorTransitionPlacement(finalCamera, samus);
    }

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
        if (_room.ActiveRoom is null || _room.ActiveLoadStation is null)
            throw new InvalidOperationException("The starting Ceres room must be loaded before Samus setup.");

        _player.Samus = new SamusState
        {
            Mode7QueueResidue = _enemies.Room.Mode7QueueResidue,
            Pose = SamusPoseId.ForwardFacingPowerSuitPose,
            AnimationFrame = 0,
            Position = FixedPosition<RoomPixels>.FromWhole(_room.ActiveLoadStation.SamusPosition.WithY(0)),
        };
        _artwork.BindSamusPalettePresentation();
        SamusPresentation.LoadPowerSuitPalette(_cgram, _artwork.MapPresentation?.SamusSuitColors);
        _tester.ApplyTesterInventory();

        // Fresh-game loading has one deliberately non-general palette write after copying
        // every target color into the live palette: `$82:8190` clears color $DF (CGRAM
        // color 223) immediately before Samus command $08 spawns the two Ceres elevator
        // projectiles. The stationary level-data concealer is four solid pixels of common
        // OBJ tile $20, whose opaque pixel selects precisely color $DF. Leaving the normal
        // blue value from `Initial_Palette_spritePalette5` in that one entry makes the
        // elevator landing recess appear pre-filled; the cartridge's zero makes it black
        // until the moving pad reaches it and both projectiles delete themselves.
        new NewGamePalettes(_cgram).ClearCeresElevatorConcealer();

        _player.Samus.PoseMetadata.RefreshCollisionRadii();
        _player.Samus.Presentation.InitializeAnimation();
        _player.Samus.LiquidPhysics.RoomIdentity = _room.ActiveRoom.Identity;
        _player.Samus.Presentation.PrimeGraphics();
        _player.PreviousMovementTypeForXray = _player.Samus.PoseMetadata.ReadMovementType();

        // `$90:F21B/$90:F226` use SpawnEprojWithGfx and thus briefly seed both objects
        // from enemy slot zero. Their shared initializer at `$86:A301`, however, runs
        // before the first draw and explicitly clears that graphics index. The specialized
        // arrival model owns that native post-initialization value; passing the transient
        // enemy word here previously recolored the level-data concealer teal.
        _room.CeresElevatorArrival = new CeresElevatorArrivalState(
            _player.Samus,
            _enemies.TileArtwork?.ProjectileSpritemaps);

        // The locked Ceres-start frame handler still publishes the initial minimap. The
        // ordinary update below used to be gated on GroundedSamusMovementEnabled, leaving
        // the ROM HUD template's unrelated map glyphs visible for the entire descent. Seed
        // the real area-six room coordinate now; subsequent normal frames keep updating it.
        ActiveRoomGeometry roomGeometry = GetActiveRoomGeometry();
        _hud.UpdateMinimap(_system, new MinimapFrame { AreaIndex = roomGeometry.AreaIndex, RoomMap = roomGeometry.MapPosition, RoomSize = _room.LevelData?.Size ?? _room.ActiveRoom.Size.ToRoomBlocks(), Samus = _player.Samus.Position.Whole, NmiFrameCounter = _clock.Counter8, MapRevealMode = _options.MapRevealMode, PresentationMap = _artwork.MapPresentation?.Get(roomGeometry.AreaIndex) });

        // `SamusCode_08_SetupForCeresStart` installs a locked frame handler. Reuse the
        // existing runtime switch to ensure ordinary movement cannot begin before the
        // elevator arrival sequence has been translated.
        _player.GroundedSamusMovementEnabled = false;
    }

    /// <summary>Returns room-header geometry for either supported cartridge room path.</summary>
    internal ActiveRoomGeometry GetActiveRoomGeometry()
    {
        if (_room.ActiveRoom is not null)
        {
            return new ActiveRoomGeometry(
                _room.ActiveRoom.AreaIndex,
                _room.ActiveRoom.MapPosition,
                _room.ActiveRoom.UpScroller,
                _room.ActiveRoom.DownScroller);
        }

        if (_room.LandingSiteEntry is not null)
        {
            return new ActiveRoomGeometry(
                _room.LandingSiteEntry.AreaIndex,
                _room.LandingSiteEntry.RoomMapPosition,
                _room.LandingSiteEntry.UpScroller,
                _room.LandingSiteEntry.DownScroller);
        }

        throw new InvalidOperationException("No cartridge room header is active.");
    }

    /// <summary>The host-bound room catalogs; room loading never substitutes cartridge artwork.</summary>
    private RoomAssetCatalogs RequireRoomAssetCatalogs() =>
        _artwork.RoomCharacterArt is { } characters && _artwork.RoomPaletteArt is { } palettes &&
        _artwork.RoomMetatileArt is { } metatiles && _artwork.RoomVisualLayouts is { } layouts &&
        _artwork.RoomLevelStreams is { } levelStreams
            ? new RoomAssetCatalogs(characters, palettes, metatiles, layouts, levelStreams)
            : throw new InvalidOperationException(
                "Room loading requires installed character, palette, metatile, visual-layout, and level-stream assets.");


    /// <summary>The 16-pixel block a coordinate lies in within its 256-pixel screen.</summary>
    private static int BlockInScreen(ushort pixel) => (pixel >> 4) & 15;

    /// <summary>
    /// The doorway nudge of <c>$82:E6A2</c> and <c>LoadMoreThings</c>: a coordinate in its screen's second block moves
    /// to eight pixels past that block.
    /// </summary>
    private static ushort NudgeOffScreenLeftEdge(ushort pixel) =>
        BlockInScreen(pixel) == 1 ? unchecked((ushort)((pixel | 15) + 8)) : pixel;

    /// <summary>The doorway nudge's other edge: a coordinate in its screen's fifteenth block moves to eight pixels before it.</summary>
    private static ushort NudgeOffScreenRightEdge(ushort pixel) =>
        BlockInScreen(pixel) == 14 ? unchecked((ushort)((pixel & ~15) - 8)) : pixel;
}
