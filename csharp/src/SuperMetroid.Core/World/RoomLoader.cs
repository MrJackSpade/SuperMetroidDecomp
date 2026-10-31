using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
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
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// Room loading: saved games and new sessions, cartridge rooms and doors, room setup, palettes, the HUD minimap,
/// enemies and PLMs of the loaded room.
/// </summary>
[RoomScopeBoundary("Builds each room's scope in the native load order.")]
internal sealed class RoomLoader
{
    /// <summary>Supplies decoded graphics and palette assets for the room being entered.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Tracks both background layers' scroll positions across door loading.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Receives room and player palette data during a load.</summary>
    private readonly GameplayColours _colours;
    /// <summary>Retains controller state needed when a room transition begins.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Publishes the display setup for a newly active room.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Coordinates the native door loader's staged transition states.</summary>
    private readonly DoorTransitionRunner _doorTransition;
    /// <summary>The enemy words and installed enemy artwork that outlive each room; each room's enemies are built over it.</summary>
    private readonly EnemySession _enemySession;

    /// <summary>The session's installed boss artwork, which each room's bosses read.</summary>
    private readonly BossArtworkBinding _bossArtwork;

    /// <summary>The session's installed area artwork, which each room's area actors read.</summary>
    private readonly AreaArtworkBinding _areaArtwork;
    /// <summary>Applies room-scoped visual effects driven by enemies.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
    /// <summary>Installs escape-specific room behavior when entering the relevant room.</summary>
    private readonly EscapeRoomEffects _escapeEffects;
    /// <summary>Restores the save slot's accumulated game time.</summary>
    private readonly GameTimeState _gameTime;
    /// <summary>Holds HUD values carried into the loaded room.</summary>
    private readonly HudState _hud;

    /// <summary>The frame counters: the minimap reads the short one, and a door's scroll restarts it.</summary>
    private readonly FrameClock _clock;
    /// <summary>Whether the door transition's scroll is restarting the short frame counter.</summary>
    private readonly DoorTransitionFrameCounting _doorFrameCounting;
    /// <summary>Restores the saved controller and movement options.</summary>
    private readonly GameplayOptions _options;
    /// <summary>Holds Samus and her load-time presentation state.</summary>
    private readonly SamusFrameState _player;
    /// <summary>The session's room scroll allocation; it outlives each room so implicit tables keep the previous room's tail.</summary>
    private readonly RoomScrollStorage _scrollStorage = new();
    /// <summary>Constructs room PLMs after persistent state has been restored.</summary>
    private readonly PlmShotBlockVisuals _shotBlockVisuals;
    /// <summary>Resets Samus projectiles for the destination room.</summary>
    private readonly BeamChargeState _beamCharge;
    /// <summary>The installed services reproducing Samus's glitch reads, handed to every Samus and projectile system built here.</summary>
    private readonly SamusOverreadServices _samusOverreads;
    /// <summary>Holds the active room, camera, selected door, and load-station identity.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>The Fireflea state, the room's fixed colour and installed FX artwork each room's layer-3 FX reads.</summary>
    private readonly RoomFxSession _roomFx;
    /// <summary>The palette-FX handler state each room's palette FX reads.</summary>
    private readonly PaletteFxSessionState _paletteFxSession;
    /// <summary>The session owners each room's Tourian statue sequence is built over.</summary>
    private readonly TourianStatueSequenceDependencies _tourianStatueDependencies;
    /// <summary>Provides persistent event and boss bits used by room-state selectors.</summary>
    private readonly SharedGameState _system;
    /// <summary>Applies tester inventory overrides after saved state is restored.</summary>
    private readonly TesterTools _tester;


    /// <summary>Captures the owners of room, player, graphics, and door state used by the loader.</summary>
    internal RoomLoader(RoomLoaderDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _backgroundScroll = dependencies.BackgroundScroll;
        _colours = dependencies.Colours;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _doorTransition = dependencies.DoorTransition;
        _enemySession = dependencies.EnemySession;
        _bossArtwork = dependencies.BossArtwork;
        _areaArtwork = dependencies.AreaArtwork;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _escapeEffects = dependencies.EscapeEffects;
        _gameTime = dependencies.GameTime;
        _hud = dependencies.Hud;

        _clock = dependencies.Clock;
        _doorFrameCounting = Ensure.NotNull(dependencies.DoorFrameCounting);
        _options = dependencies.Options;
        _player = dependencies.Player;
        _shotBlockVisuals = dependencies.ShotBlockVisuals;
        _beamCharge = dependencies.BeamCharge;
        _samusOverreads = Ensure.NotNull(dependencies.SamusOverreads);
        _room = dependencies.Room;
        _roomFx = dependencies.RoomFx;
        _paletteFxSession = dependencies.PaletteFxSession;
        _tourianStatueDependencies = dependencies.TourianStatueDependencies;
        _system = dependencies.System;
        _tester = dependencies.Tester;

    }
    /// <summary>The door's four-pixel scroll trajectory, retained across atomic room construction.</summary>
    private DoorOpeningScrollState? _doorOpeningScroll;
    /// <summary>
    /// Elevator placement produced by the atomic room load but deferred until room loading
    /// has progressed to that enemy's initialization (<see cref="IRoomLoadProgress"/>).
    /// </summary>
    private LoaderSamusPlacement? _pendingLoaderSamusPlacement;
    /// <summary>What the source room showed, kept until the door-opening scroll's offsets are installed.</summary>
    private DoorOpeningSourceView? _pendingDoorOpeningSourceView;

    /// <summary>
    /// Executes the data-loading portion of native loading-state $1F: area six, station
    /// zero, the station-selected room, and its fresh-save state-selection branch.
    /// </summary>
    public InitialViewportResult InitializeStartingCeresRoom()
    {
        // `InitAndLoadGameData_Async` writes these exact indexes before calling
        // LoadFromLoadStation. The compiled catalog preserves that general lookup rather
        // than duplicating the Ceres room/door/placement at this call site.
        LoadStationEntry station = LoadStations.Get(
            areaIndex: AreaId.Ceres,
            stationIndex: 0);
        DoorDefinition door = station.Door ?? throw new InvalidDataException("The load station has no door.");
        if (door.DestinationRoom != station.Room)
        {
            throw new InvalidDataException(
                $"Ceres station room {station.Room?.Name} disagrees with door " +
                $"{door.Name}'s destination {door.DestinationRoom.Name}.");
        }

        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);
        if (room.AreaIndex != AreaId.Ceres)
        {
            throw new InvalidDataException(
                $"Fresh Ceres station targets {room.AreaIndex}, expected {AreaId.Ceres}.");
        }

        return LoadRoom(
            door,
            room,
            station.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { LoadStation = station });
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

        AreaId requestedArea = AreaIds.FromIndex(
            unchecked((byte)slot.Area),
            $"Save slot {slot.Slot}");
        int stationIndex = unchecked((byte)slot.SaveStation);

        LoadStationEntry station = LoadStations.Get(
            requestedArea,
            checked((byte)stationIndex));
        DoorDefinition door = station.Door ?? throw new InvalidDataException("The load station has no door.");
        if (door.DestinationRoom != station.Room)
        {
            throw new InvalidDataException(
                $"Load station {requestedArea}:{stationIndex} room {station.Room?.Name} " +
                $"disagrees with door {door.Name}'s destination " +
                $"{door.DestinationRoom.Name}.");
        }

        // Apply progression before room construction. Enemy and PLM initialization query
        // boss/event/item bytes synchronously, while room-state selection and beam graphics
        // depend on those same words. In particular, loading a post-Morph-Ball save must not
        // accidentally construct the untouched new-game version of Blue Brinstar.
        RestoreSavedPlayerState(slot, resetBossesOnLoad);
        _player.IntroducedSamus.Position = _player.IntroducedSamus.Position.WithWholeX(station.SamusPosition.X);
        _player.IntroducedSamus.Position = _player.IntroducedSamus.Position.WithWholeY(station.SamusPosition.Y);
        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);
        if (room.AreaIndex != requestedArea)
        {
            throw new InvalidDataException(
                $"Load station {requestedArea}:{stationIndex} targets room area " +
                $"{room.AreaIndex}.");
        }
        InitialViewportResult viewport = LoadRoom(
            door,
            room,
            station.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { LoadStation = station });
        // LoadFromLoadStation writes `$1E75 = 1`. Without this room-entry lockout, the
        // first standing floor probe after loading directly on a save pod immediately
        // reopens its confirmation message.
        _room.LoadedScope.Plms.Stations.LockSaveStationForCurrentRoomEntry();

        // Command nine does not begin at its endpoint. It selects a front-facing pose,
        // frame two/timer three, a suit-specific bank-$8D palette object, and the 360-call
        // appearance handler before restoring ordinary input.
        _player.IntroducedSamus.Presentation.LoadSuitPalette(_player.IntroducedSamus, _player.Colours);
        SamusPoseMetadata.ApplyZebesStartPoseSetup(_player.IntroducedSamus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(_player.IntroducedSamus, frame: 2, timer: 3);
        _player.IntroducedSamus.LiquidPhysics.RoomIdentity = room.Identity;
        _player.IntroducedSamus.Presentation.PrimeGraphics(_player.IntroducedSamus);
        PaletteFxObject loadAppearance =
            _player.IntroducedSamus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? SavedGameAppearanceDefinitions.GravitySuitGlow
                : _player.IntroducedSamus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? SavedGameAppearanceDefinitions.VariaSuitGlow
                    : SavedGameAppearanceDefinitions.PowerSuitGlow;
        _player.BeginLoadAppearance(loadAppearance, WrappingCountdown.Start(SavedGameAppearanceDefinitions.FrameCount));
        _room.LoadedScope.PaletteFx.SpawnDefinition(
            loadAppearance,
            _player.IntroducedSamus.EquippedItems,
            _system.HasAnyBossBits(room.AreaIndex, BossBits.AreaMiniBoss));
        _player.IntroducedSamus.Control.LockHandlers();
        _player.PreviousMovementTypeForXray = SamusPoseMetadata.ReadMovementType(_player.IntroducedSamus);
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
        _player.Samus = new SamusState(_samusOverreads.AnimationContinuation)
        {
            FloorQueueResidue = _enemySession.FloorQueueResidue,
            Pose = SamusPoseId.FacingRightNormalPose,
            AnimationFrame = 0,
        };
        _artwork.BindSamusPalettePresentation();
        slot.ApplyTo(_player.Samus, _system);
        if (resetBossesOnLoad) _tester.ResetSavedEncounters();
        _tester.ApplyTesterInventory();
        // $91:E00D clears both persisted HUD selection words after mirror restore.
        // Otherwise the arm-cannon cover can leak into the front-facing load pose.
        _player.Samus.SelectedHudItem = HudItem.Beam;
        _player.Samus.AutoCancelHudItemIndex = HudItem.Beam;
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
        var samus = new SamusState(_samusOverreads.AnimationContinuation) { FloorQueueResidue = _enemySession.FloorQueueResidue, Pose = 0, AnimationFrame = 0 };
        samus.CopyInventoryFrom(carried);
        _player.Samus = samus;
        _tester.CarryInventoryGrant(carried, samus);
        _artwork.BindSamusPalettePresentation();

        // `$82:8038-$8048` assigns these indexes immediately before LoadFromLoadStation.
        // Station eighteen is a cutscene-only entry high above Landing Site; station zero
        // is not selected until GunshipTop_7 finishes the landing and performs the save.
        LoadStationEntry station = LoadStations.Get(
            areaIndex: AreaId.Crateria,
            stationIndex: 18);
        DoorDefinition door = station.Door ?? throw new InvalidDataException("The load station has no door.");
        if (door.DestinationRoom != station.Room)
        {
            throw new InvalidDataException(
                $"Post-Ceres station room {station.Room?.Name} disagrees with " +
                $"door {door.Name}'s destination {door.DestinationRoom.Name}.");
        }

        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);
        if (room.AreaIndex != AreaId.Crateria)
        {
            throw new InvalidDataException(
                $"Post-Ceres station eighteen targets area {room.AreaIndex}, expected Crateria.");
        }

        // CADF refilled health on the preceding cinematic frame; $91:E164 then seeds the
        // hurt check from it. The $22 branch locks Samus's state handlers.
        samus.Health = samus.MaxHealth;
        samus.PreviousHealthForHurtCheck = samus.Health;
        samus.Kinematics.Position = samus.Kinematics.Position.WithX(new FixedCoordinate(station.SamusPosition.X, 0)).WithY(new FixedCoordinate(station.SamusPosition.Y, 0));
        samus.Control.LockHandlers();

        // The special loader still executes the normal room-object teardown before it
        // constructs Landing Site. CeresElevatorArrival models two bank-$86 projectiles
        // of the abandoned station room, so Landing Site's scope ends them; retaining them
        // would let their delayed Y=$0048 completion overwrite the Samus position currently
        // carried by the gunship.
        InitialViewportResult viewport = LoadRoom(
            door,
            room,
            station.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { GunshipScenario = GunshipLoadScenario.EscapingCeres, LoadStation = station });
        samus.Presentation.LoadSuitPalette(samus, _player.Colours);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.LiquidPhysics.RoomIdentity = room.Identity;
        samus.Presentation.PrimeGraphics(samus);
        _player.PreviousMovementTypeForXray = SamusPoseMetadata.ReadMovementType(samus);
        _player.GroundedSamusMovementEnabled = false;
        return viewport;
    }

    /// <summary>
    /// True after bank-$94's type-$9 handler has selected a normal destination door and
    /// before the frontend's states $09-$0B consume it.
    /// </summary>
    public bool HasPendingDoorTransition => _room.LevelData?.PendingDoorTransition is not null;

    /// <summary>The unconsumed bank-$83 door selected by the type-$9 collision handler.</summary>
    public DoorDefinition? PendingDoorTransition => _room.LevelData?.PendingDoorTransition;

    /// <summary>
    /// Runs one <c>$82:E310</c> source-room alignment call. Horizontal doors converge the
    /// camera Y low byte; vertical doors converge X. Signed low-byte motion deliberately
    /// takes <c>$FF -> $00</c> and <c>$01 -> $00</c> in one-pixel steps.
    /// </summary>
    /// <returns>True only when the relevant low byte was already zero on entry.</returns>
    public bool AlignPendingDoorCameraOnePixel()
    {
        DoorDefinition door = PendingDoorTransition
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
        // convergence step; the gameplay scroll owner performs the same parallax math.
        _backgroundScroll.Layer1Position = _room.Camera.PixelPosition;
        _backgroundScroll.StepScrolling();
        return false;
    }

    /// <summary>
    /// The axis aligned by $82:E310, perpendicular to travel: right/left converge camera Y,
    /// while down/up converge camera X. This literal branch matters because aligning the
    /// travel axis instead moves a horizontal door frame by a tile row while its room
    /// scroll is being staged.
    /// </summary>
    internal static bool DoorTransitionAlignsX(DoorDirection direction) => direction.IsVertical();

    /// <summary>Whether the pending door's destination room header selects the isolated door transition.</summary>
    public bool PendingDoorDestinationUsesIsolatedDoorTransition => PendingDoorTransition is { } door
        ? LoadSelectedRoom(door.DestinationRoom).UsesIsolatedDoorTransition
        : throw new InvalidOperationException("No pending door destination exists.");

    /// <summary>
    /// Chooses the layers a door transition shows below the HUD for its whole scroll. The
    /// background layer is always hidden; the foreground is hidden too when either room
    /// selects the isolated door transition.
    /// </summary>
    [IndependentFlag("Whether the source and destination rooms are isolated are independent room properties.")]
    internal void BeginDoorTransitionDisplay(bool sourceIsolated, bool destinationIsolated)
    {
        if (_room.DoorTransitionMainScreenLayers is not null)
            throw new InvalidOperationException("The door-transition display is already active.");

        bool suppressBg1 = sourceIsolated || destinationIsolated;
        _room.DoorTransitionMainScreenLayers = suppressBg1
            ? ScreenLayers.Sprites
            : ScreenLayers.Foreground | ScreenLayers.Sprites;
    }

    /// <summary>Returns the screen to the destination room's ordinary layers.</summary>
    internal void EndDoorTransitionDisplay()
    {
        if (_room.DoorTransitionMainScreenLayers is null)
            throw new InvalidOperationException("The door-transition display is not active.");
        // The update that hands the screen back still counts as a scrolling update; the
        // room's ordinary display takes over on the following frame.
        RestartShortCounterDuringDoorScroll();
        _room.DoorTransitionMainScreenLayers = null;
        _doorFrameCounting.EndScroll();
    }

    /// <summary>
    /// Restarts the short frame counter on an update of the door scroll, from the update that places Samus for tile
    /// loading on (<see cref="DoorTransitionFrameCounting"/>); enemies and projectiles that read the counter see the
    /// restart.
    /// </summary>
    internal void RestartShortCounterDuringDoorScroll()
    {
        if (_doorFrameCounting.RestartsShortCounter) _clock.RestartShortCounter();
    }

    /// <summary>
    /// Publishes setup's Samus displacement before destination-coordinate rebasing.
    /// The later atomic room constructor uses the captured pre-setup coordinates.
    /// </summary>
    internal void ApplyDoorScrollingSetupMovement()
    {
        DoorDefinition door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        if (_player.Samus is null)
            throw new InvalidOperationException("Door scrolling requires Samus.");
        _player.Samus.Position = DoorOpeningScrollState.ApplySetupMovement(door, _player.Samus.Position);
        PublishDoorCameraPosition(DoorOpeningScrollState.GetSetupCamera(door));
    }


    /// <summary>Rebases Samus and publishes the scroll's first moving step during tile loading.</summary>
    internal void PlaceSamusForDoorTileLoading()
    {
        DoorDefinition door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        if (_player.Samus is null || _room.Camera is null)
            throw new InvalidOperationException("Door tile loading requires Samus and camera.");
        FixedPosition<RoomPixels> position = DoorOpeningScrollState.RebaseSamus(door, _player.Samus.Position);
        _room.Camera.PublishDoorSamusPosition(position.Whole);
        _player.Samus.Position = DoorOpeningScrollState.AdvanceSamus(door, position);
        DoorDirection direction = door.Orientation.Direction;
        int delta = direction is DoorDirection.Right or DoorDirection.Down ? (short)4 : (short)-4;
        bool vertical = direction.IsVertical();
        Offset16<RoomPixels> firstStep = vertical ? new(0, checked((short)delta)) : new(checked((short)delta), 0);
        PublishDoorCameraPosition(DoorOpeningScrollState.GetSetupCamera(door) + firstStep);
        _room.Camera.PublishDoorSamusTravelAxis(vertical, _player.Samus.Position.Whole);
        _doorFrameCounting.BeginScroll();
        RestartShortCounterDuringDoorScroll();
    }

    /// <summary>
    /// Rewinds an atomically loaded destination to the position established by the
    /// scroll's setup and Samus's placement for tile loading.
    /// </summary>
    /// <remarks>
    /// The room constructor loads tiles, PLMs, FX and enemies in one operation, while the
    /// door's scroll is already moving. Capturing the source fixed coordinates before that
    /// constructor and rewinding only the four scroll-owned coordinate pairs preserves the
    /// same observable trajectory; how far loading had progressed on each scroll update is
    /// <see cref="IRoomLoadProgress"/>'s.
    /// </remarks>
    internal void BeginDoorOpeningScroll(
        DoorDefinition door,
        FixedPosition<RoomPixels> sourceSamus,
        IRoomLoadProgress loaderProgress,
        int completedLoadingScrollUpdates = 0)
    {
        if (_room.Camera is null || _player.Samus is null)
            throw new InvalidOperationException("A loaded destination and Samus are required.");
        if (_doorOpeningScroll is not null)
            throw new InvalidOperationException("A door-opening scroll is already active.");
        if (_room.LoadedScope.Enemies.Elevator.ConsumeElevatorArrivalPlacementSlot() is int placementSlot)
            _pendingLoaderSamusPlacement = new LoaderSamusPlacement(placementSlot, _player.Samus.Position.Whole);

        _doorOpeningScroll = DoorOpeningScrollState.Create(
            door,
            sourceSamus,
            _room.Camera.PixelPosition,
            _backgroundScroll.Layer2Position,
            _player.Samus.Position);
        DoorDirection direction = door.Orientation.Direction;
        DoorOpeningSourceView sourceScroll = _pendingDoorOpeningSourceView
            ?? throw new InvalidOperationException(
                "Door-opening scroll lost the source room's view.");
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
        Position16<RoomPixels> retainedBg1Scroll = direction == DoorDirection.Down
            ? sourceScroll.Bg1Scroll + new Offset16<RoomPixels>(0, 1)
            : sourceScroll.Bg1Scroll;
        _backgroundScroll.ConfigureDoorOpeningOffsets(retainedBg1Scroll, stagedLayer1);
        _pendingDoorOpeningSourceView = null;
        // The source screen stays where it was while the destination scrolls in around it.
        _room.DoorSourceBackground = RoomDoorBackground.Begin(sourceScroll.SourceBackground, sourceScroll.Bg1Scroll.AsBgPlaneScroll());
        PublishDoorOpeningLayers(_doorOpeningScroll);
        // Each directional setup immediately takes its first step, which recomputes the
        // layer scrolls from the already-advanced layer positions. Down's first update
        // restores the scrolls it moved, so only the horizontal and upward setups leave
        // new scrolls behind.
        if (direction != DoorDirection.Down)
            _backgroundScroll.CalculateLayerScrolls();
        _player.Samus.Position = _doorOpeningScroll.SamusPosition;
        _room.Camera.PublishDoorSamusPosition(_player.Samus.Position.Whole);
        // Replay the scroll updates already shown before the atomic destination load.
        // Those steps do not consume more input.
        for (int step = 0; step < completedLoadingScrollUpdates; step++)
            StepDoorOpeningScroll(loaderProgress);
    }

    /// <summary>
    /// Runs one update of the door's scroll. True means this update finished the scroll.
    /// </summary>
    internal bool StepDoorOpeningScroll(IRoomLoadProgress loaderProgress)
    {
        DoorOpeningScrollState state = _doorOpeningScroll
            ?? throw new InvalidOperationException("No door-opening scroll is active.");
        if (_room.Camera is null || _player.Samus is null)
            throw new InvalidOperationException("Door-opening scroll lost its room actors.");
        bool completed = state.Advance();
        PublishDoorOpeningLayers(state);
        // Each four-pixel step recomputes the layer scrolls, except during an upward
        // door's first frames.
        if (state.RecalculatesScrollAfterAdvance)
            _backgroundScroll.CalculateLayerScrolls();

        if (completed)
        {
            // The final directional step has already recalculated the layer scrolls from
            // the stepped coordinates; only the camera snaps to the exact destination,
            // without another scroll calculation.
            state.SnapLayerOneToDestination();
            PublishDoorCameraPosition(state.CameraPosition);
        }
        // The scroll moves at the start of the update; room loading's progress follows.
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
        _room.LoadedScope.Enemies.LoaderHooks.CompleteRinkaLoaderTimeCameraReads(loaderProgress.HasInitializedEnemySlot, _room.Camera.PixelPosition);
        if (completed && _room.LoadedScope.Enemies.LoaderHooks.HasDeferredRinkaLoaderTimeCameraReads)
            throw new InvalidDataException("The door scroll finished before the loader initialized every special Rinka.");
        _player.Samus.Position = state.SamusPosition;
        _room.Camera.PublishDoorSamusTravelAxis(state.Direction.IsVertical(), _player.Samus.Position.Whole);
        return completed;
    }

    /// <summary>
    /// Publishes the scroll's camera and second-layer positions to the camera (whose
    /// fractions the scroll never writes) and to the background-scroll owner.
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
    /// Applies the horizontal eight-pixel doorway alignment on the update after the
    /// scroll ends, before the music wait and the later narrow-door nudges.
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
    /// Applies the doorway nudges after music has drained: the nudges to Samus's live
    /// position, then an elevator door's Samus command (7 going down, 0 going up). Releases
    /// the scroll's trajectory. This is deliberately later than the scroll.
    /// </summary>
    internal void FinishDoorOpeningScroll()
    {
        DoorOpeningScrollState state = _doorOpeningScroll
            ?? throw new InvalidOperationException("No door-opening scroll is active.");
        if (state.RemainingFrames != 0 || _player.Samus is null)
            throw new InvalidOperationException("Door-opening scroll has not reached its endpoint.");
        if (_pendingLoaderSamusPlacement is not null)
            throw new InvalidOperationException("The door loader's Samus placement is still pending.");

        _player.Samus.Position = _player.Samus.Position.WithWhole(new(
            NudgeOffScreenRightEdge(NudgeOffScreenLeftEdge(_player.Samus.Position.X.Whole)),
            NudgeOffScreenLeftEdge(_player.Samus.Position.Y.Whole)));

        if (_room.LoadedScope.Enemies.Elevator.ElevatorFlags != ElevatorJourneyFlag.None)
        {
            if (!_room.LoadedScope.Enemies.Elevator.ElevatorTravelsUpward)
                SamusPoseMetadata.SetupForElevator(_player.Samus);
            else
                _player.Samus.Control.LockHandlers();
        }
        _doorOpeningScroll = null;
        _room.DoorSourceBackground = null;
    }

    /// <summary>
    /// Loads the door's destination while the source room stays on screen; the following
    /// scroll updates slide the destination in.
    /// </summary>
    internal InitialViewportResult LoadPendingDoorDestinationForTransition(FixedPosition<RoomPixels> sourceSamus)
    {
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
        DoorDefinition door = sourceLevel.PendingDoorTransition
            ?? throw new InvalidOperationException("No type-$9 door collision is pending.");
        DoorTransitionPlacement placement = CalculateDoorTransitionPlacement(door, sourcePosition ?? _player.Samus.Position);
        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);

        // The transition's teardown deletes direct G-Mode's stranded X-ray beam,
        // but it does not restore the four subsystem enable words. Destination room setup
        // therefore begins in indirect G-Mode and cannot create a visible gate actor.
        _player.Samus.Xray.TransitionDirectGModeToIndirect();

        // The desktop's opt-in error reporter resumes at the next emulated-frame boundary.
        // Cartridge code cannot throw, but translated room construction can still reject an
        // unsupported definition. Build the read-only asset graph before consuming the
        // source room's pending door so such a failure remains retryable instead of leaving
        // state $0B with no destination on the following frame.
        LoadedRoomAssets assets = LoadedRoomAssets.Load(_scrollStorage, room, RequireRoomAssetCatalogs());
        DoorDefinition consumedDoor = sourceLevel.ConsumePendingDoorTransition()
            ?? throw new InvalidOperationException(
                "Pending door disappeared while its destination assets were prepared.");
        if (consumedDoor != door)
        {
            throw new InvalidOperationException(
                $"Pending door changed from {door.Name} to " +
                $"{consumedDoor.Name} while its destination assets were prepared.");
        }

        // `$82:E8DD/$82:EB93` promotes a departing elevator's global status from one to
        // two after the destination PLMs, door ASM, and setup ASM have been created. Our
        // room constructor initializes enemies in one host call, so publish that carried
        // status immediately before Enemies.Load: the destination elevator initializer
        // must see two and place itself at parameter 2 instead of clearing the journey.
        bool arrivingByElevator = _room.LoadedScope.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Departing;
        if (arrivingByElevator)
            _room.LoadedScope.Enemies.Elevator.PrepareElevatorArrival();

        // The load-station record established only the first room. Once bank $94 publishes
        // a door definition, that definition and its destination header become authoritative:
        // the destination's scope is built without a load station.

        // The door's scroll has already placed Samus before the destination room is
        // constructed. This ordering is observable for elevator arrivals:
        // Elevator_Init sees status two, moves the platform to parameter 2, and then pins
        // Samus to it. Applying the generic placement after Enemies.Load used to overwrite
        // that native attachment and exposed the upward arrival one row too low.
        _player.Samus.Position = placement.SamusPosition;
        // The door's scroll moves Samus at its own speed. Ordinary jump velocity survives
        // room loading and the final nudge; clearing it here restarts a fall
        // at its apex and lets horizontal momentum carry Samus too far after exit.
        _player.Samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.Zero;

        InitialViewportResult viewport = LoadRoom(
            door,
            room,
            placement.CameraPosition,
            viewportLoadMode,
            new RoomEntry { RunDoorClosingPlm = true, PreloadedAssets = assets });

        // `$82:E4B6` calls Samus_LoadSuitTargetPalette after room/enemy palettes have been
        // loaded and before state $0B captures the destination fade target. The source fade
        // has already driven OBJ palette four to black at this point. Omitting this shared
        // reload therefore retained valid OAM and tile data but rendered Samus as a solid
        // black silhouette after every ordinary desktop door transition.
        _player.Samus.Presentation.LoadSuitPalette(_player.Samus, _player.Colours);

        // The door loader writes no SamusYRadius; the next unlocked alpha's SetSamusRadius
        // does, so an elevator rider keeps the boarding pose's radius through the door.
        _player.Samus.LiquidPhysics.RoomIdentity = room.Identity;
        _player.Samus.Presentation.PrimeGraphics(_player.Samus);
        _player.GroundedSamusMovementEnabled = true;
        return viewport;
    }

    /// <summary>Loads the selected room, its state and its graphics; shared by stations, doors and the attract demo.</summary>
    internal InitialViewportResult LoadRoom(
        DoorDefinition door,
        SelectedRoom room,
        Position16<RoomPixels> camera,
        RoomViewportLoadMode viewportLoadMode,
        RoomEntry entry = default)
    {
        DoorOpeningSourceView? doorOpeningSourceView = viewportLoadMode switch
        {
            RoomViewportLoadMode.DisplayInitialViewport => null,
            RoomViewportLoadMode.StreamThroughDoor => new DoorOpeningSourceView(_backgroundScroll.Bg1Scroll,
                new RoomBackgroundLayer(_room.LoadedLevelData.ForegroundPlane.Cells, _backgroundScroll.Bg1Offset.AsBgPlaneOffset(),
                    SourceScreenPictures())),
            _ => throw new ArgumentOutOfRangeException(
                nameof(viewportLoadMode), viewportLoadMode, "Unknown room viewport load mode."),
        };
        LoadedRoomAssets assets = entry.PreloadedAssets ??
            LoadedRoomAssets.Load(_scrollStorage, room, RequireRoomAssetCatalogs());

        // The destination reads the outgoing room only through this capture; from here on the room views show the
        // destination's parts as each step builds them.
        var carryOver = new RoomTransitionCarryOver(_room.Camera, _room.InheritedLayer1Position,
            _room.Scope is { } gated
                ? new EnemyDoorTransitionGates(gated.Enemies.RoomStatus.EnemyDoorTransitionActive,
                    gated.Enemies.Elevator.ElevatorDoorTransitionActive)
                : default);
        // The outgoing room's shots and enemy projectiles leave what the next room's start from (RoomHandOver).
        if (_room.Scope is { } outgoing)
        {
            outgoing.Projectiles.HandOverTo(_room.HandOver);
            outgoing.Enemies.HandOverTo(_room.HandOver);
        }
        RoomScopeConstruction scope = _room.BeginConstruction(door, room, assets, entry.LoadStation);
        var wreckedShipTreadmill = new WreckedShipTreadmillAnimatedTilesState();
        scope.WreckedShipTreadmill = wreckedShipTreadmill;
        var maridiaElevatube = new MaridiaElevatubeRoomMainState(
            active: room.State.MainCallback == RoomMainCallback.MaridiaElevatube,
            _room.RoomMainCarryover);
        scope.MaridiaElevatube = maridiaElevatube;
        var ceresElevatorShaft = new CeresElevatorShaftRoomMainState(
            active: room.State.MainCallback == RoomMainCallback.CeresElevatorShaft &&
                door.EntersCeresElevatorShaft,
            _room.RoomMainCarryover);
        scope.CeresElevatorShaft = ceresElevatorShaft;
        scope.CeresFallingDebris = new CeresFallingDebrisRoomMainState(_room.RoomMainCarryover);
        scope.EscapeDiagonalQuake = new EscapeDiagonalQuakeState(_room.RoomMainCarryover);
        // Door setup `$8F:E4E0` writes these exact five registers before the fresh Ceres
        // elevator room becomes visible. Publishing the immutable transform here gives
        // Samus, her projectiles, and parameter-four/five steam a single authoritative
        // producer. Ordinary doors clear it so stale Mode-7 math cannot leak across rooms.
        TurningFloorTransform? initialFloorTransform = door.EntersCeresElevatorShaft
            ? new TurningFloorTransform(
                MatrixA: 256, // 1.0 in Mode 7's 8.8 fixed point.
                MatrixB: 0,
                MatrixC: 0,
                Center: CeresElevatorShaftRoomMainState.CeresShaftRotationCenter)
            : null;
        _display.ActiveTurningFloor = initialFloorTransform;
        // `$8F:E4E0` writes the initial matrix as part of door setup before the shaft is
        // exposed. Initialize the displayed copy as well; subsequent room-main changes
        // reach it only when the frame's display is published, like every other floor change.
        _display.PublishDoorElevatorFloor(initialFloorTransform);
        scope.LevelData = assets.LevelData;
        ScrollBoundaryCamera? previousCamera = carryOver.PreviousCamera;
        var destinationCamera = new ScrollBoundaryCamera(assets.Scrolls);
        scope.Camera = destinationCamera;
        if (viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor)
        {
            if (previousCamera is not null) destinationCamera.InheritTrackingState(previousCamera);
            destinationCamera.SetDoorTransitionPosition(camera, previousCamera?.Position ?? carryOver.InheritedLayer1Position);
        }
        else
        {
            destinationCamera.SetPosition(camera);
            if (previousCamera is not null)
                destinationCamera.RetainSubpositions(previousCamera);
            else
                destinationCamera.RetainSubpositions(carryOver.InheritedLayer1Position);
        }
        _backgroundScroll.Layer2Scroll = room.State.Layer2Scroll;
        if (viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport)
            _backgroundScroll.CalculateScrolls();
        else
        {
            // Up calculates BG2 from destination+$1F before changing the scroll's camera
            // endpoint to destination+$20. Keeping those adjacent positions distinct prevents
            // a parallax room from accumulating a one-pixel layer mismatch at the snap.
            Position16<RoomPixels> parallaxBasis = door.Orientation.Direction == DoorDirection.Up
                ? camera - new Offset16<RoomPixels>(0, 1)
                : camera;
            _backgroundScroll.PrepareDoorOpeningDestination(camera, parallaxBasis);
        }

        // The main callback runs even in Crateria's rock room, whose BG2 stays its own background.
        // The sky's banded horizontal motion belongs to the separately spawned sky effect, which the room's FX or
        // setup callback starts, not to the room's main callback.
        RoomFxRecordDefinition? selectedFx = room.State.Fx?.Select(door);
        bool skyHasBands =
            selectedFx?.Type == RoomFxType.ScrollingSky ||
            room.State.SetupCallback is RoomSetupCallback.ScrollingSkyLand or
                RoomSetupCallback.ScrollingSkyOcean or RoomSetupCallback.ShakeScreenAndCallScrollingSkyLandDuringEscape;
        scope.ScrollingSky = ScrollingSkyState.IsScrollingSkyRoomMain(room.State.MainCallback)
            ? new ScrollingSkyState(ScrollingSkyState.VariantOf(room.State.MainCallback), skyHasBands)
            : null;

        assets.LoadGraphics(_colours.BackgroundSets);
        // Ceres rooms show their tileset's turning floor, which the door and Ridley then draw over (#1349, #1467).
        scope.Floor = room.Definition.AreaIndex == AreaId.Ceres ? assets.CreateFloor() : null;

        // `$82:E139` is part of every retail room-load/door-transition pipeline. It
        // replaces OBJ palette zero with `kCommonSpritesPalette1` before enemy setup;
        // bank-$86 actors created with SpawnEprojWithRoomGfx (graphics index zero) depend
        // on this row for lasers, death debris, and pickups. The old host loader restored
        // enemy, beam, and Samus palettes but left this row at a stale fade/menu value,
        // turning otherwise valid common projectiles solid black after a door.
        LoadGameplaySpritePalettes();

        // LoadFXHeader selects the door-specific sixteen-byte record, applies its palette
        // blend, chooses the effect page the BG3 plane shows, and creates the type-owned per-line
        // effect and animtile state before palette-FX objects and enemies begin running.
        var roomLayer3Fx = RoomLayer3FxState.ForRoom(
            _roomFx,
            _colours.BackgroundSets,
            room.State.Fx,
            door,
            _system.RandomNumber,
            room.Definition);
        scope.Layer3Fx = roomLayer3Fx;
        roomLayer3Fx.PrimeViewport(destinationCamera.PixelPosition);
        scope.CeresHaze = new CeresHazeState(RoomCallbackDefinitions.CeresHazeSetups.Contains(room.State.SetupCallback),
            _system.HasAnyBossBits(room.AreaIndex, BossBits.AreaBoss),
            viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport);
        scope.SandTiles = new RoomSandAnimatedTilesState(room.State.Fx, door,
            room.AreaIndex);
        scope.SpikeTiles = new RoomSpikeAnimatedTilesState(room.State.Fx, door,
            room.AreaIndex);
        scope.TreadmillTiles = new RoomTreadmillAnimatedTilesState(room.State.Fx, door,
            room.AreaIndex);
        if (_player.Samus is not null)
            _player.Samus.LiquidPhysics.ConfigureFor(roomLayer3Fx);

        // `$82:E4A9` calls LoadFXHeader after room setup and before enemies. Its selected
        // sixteen-byte record owns two independent object bitsets. The bank-$8D half must
        // be created on every shared room-load path; Landing Site bit zero replaces CGRAM
        // colors used by the scrolling-sky horizon on the first gameplay handler call.
        var roomPaletteFx = RoomPaletteFxSystem.ForRoom(
            _paletteFxSession,
            room.State.Fx,
            door,
            room.AreaIndex,
            _player.Samus?.EquippedItems ?? 0,
            _system.HasAnyBossBits(room.AreaIndex, BossBits.AreaMiniBoss));
        scope.PaletteFx = roomPaletteFx;


        // `$82:E43A-$E472` destroys room-owned objects before constructing the destination: the
        // destination's projectiles and bombs are built new here, before Load/beam upload, so no
        // stale index addresses its arrays; its PLM system is built with the population below.
        // The held beam charge survives on the session; Samus's own echo storage is cleared.
        if (_player.Samus is { } loadingSamus)
        {
            loadingSamus.HorizontalSpeed.ResetSpeedEchoPositionsForRoomTransition();
            loadingSamus.Shinespark.ResetProjectileEchoStorage();
        }
        var bombs = new SamusBombProjectileSystem();
        var projectiles = new SamusProjectileSystem(_beamCharge, _samusOverreads);
        projectiles.TakeOverFrom(_room.HandOver);
        scope.BombProjectiles = bombs;
        scope.Projectiles = projectiles;
        _artwork.BindProjectiles(projectiles, bombs);
        _player.Samus?.Shinespark.BindProjectileOwners(_beamCharge, bombs.PowerBombExplosion);

        // `$82:EB6C` walks this zero-terminated list once. Each six-byte record allocates
        // the highest free one of forty IDs before its bank-$84 setup executes. Immediate
        // deletion can therefore free that same ID for the next ROM record, and enemy code
        // can observe the resulting physical header/argument ordering. All room families
        // enter through this one parser; an unsupported header throws with complete record
        // context instead of being skipped by a collection of sibling scans.
        // The room's PLM system is built with its bindings, then one set of runtime services
        // serves the room's PLMs and enemies.
        var progress = new RoomAreaProgress(_system, room.AreaIndex);
        var plmServices = new RoomPlmRuntimeServices(scope, _player, _room, _system);
        var plmRoom = new PlmRoom(assets.LevelData, scope.Appearance, _system, room.AreaIndex);
        var plmBindings = new RoomPlmServices
        {
            Samus = plmServices,
            Bosses = progress,
            Events = progress,
            TourianStatue = plmServices,
            RoomFx = roomLayer3Fx is { } fx ? new RoomPlmFx(fx, () => _player.Samus) : null,
            Earthquake = plmServices,
            Enemies = plmServices,
        };
        var plms = new RoomPlmSystem(plmRoom, plmBindings, _shotBlockVisuals);
        scope.Plms = plms;
        plms.Sounds.BindPowerBombAudio(bombs.PowerBombExplosion.Activity);
        var areaServices = new RoomEnemyRuntimeServices(_system, _room, _player, plms, _display, progress);
        scope.Services = areaServices;
        plms.PopulationLoader.LoadRoomPopulation(plmRoom, room.State.PlmPopulation, plmBindings);

        // `$82:E8DD/$82:EB93` runs the bank-$8F door program only after destination PLMs
        // exist and before enemy initialization/initial viewport construction. Several of
        // those programs rewrite the room scroll array, so postponing this call until the
        // first gameplay frame would already have streamed a viewport with wrong camera
        // limits.
        _doorTransition.RunDoorSetupCode(door, plms, wreckedShipTreadmill, ceresElevatorShaft, maridiaElevatube);
        if (room.State.SetupCallback == RoomSetupCallback.SetupShaktoolRoomPlm)
            plms.Shaktool.TrySpawnShaktoolRoomController(destinationCamera.Scrolls);
        if (room.State.SetupCallback == RoomSetupCallback.TurnWallIntoShotBlocksDuringEscape)
            plms.EscapeAnimals.SetupCrittersEscapeBlock(assets.LevelData,
                assets.LevelData.GetBlockIndex(new(EscapeAnimalWallDefinitions.WallX, EscapeAnimalWallDefinitions.WallY)));
        // `$8F:91A9` spawns the fake-wall PLM; SpawnHardcodedPLM silently drops it when all
        // forty native slots are occupied.
        if (room.State.SetupCallback == RoomSetupCallback.AutoDestroyWallDuringEscape)
            plms.Tourian.TrySpawnOldTourianEscapeShaftWall(assets.LevelData);
        // `$8F:9194` also spawns this PLM before writing the quake SetupEscapeRoomEffects owns.
        if (room.State.SetupCallback == RoomSetupCallback.ClearBlocksAfterSavingAnimalsAndShakeScreen)
            plms.EscapeAnimals.TrySpawnCrateriaMainstreetEscapePassage(assets.LevelData);

        if (entry.RunDoorClosingPlm)
        {
            // `$82:E4C9` performs this handoff after destination PLMs, door/room setup,
            // and FX construction but before enemy initialization. The C# room constructor
            // groups those owners differently, yet this point preserves the observable
            // PLM-pool order: every room-authored actor exists before the closer requests
            // the next highest free slot, and no enemy-authored PLM has run yet.
            plms.DoorPlms.TrySpawnDoorClosingPlm(assets.LevelData, door, _system);
        }

        // $82:E4CC updates the entering beam before $82:E4D8 initializes enemies. Golden Torizo's code mutates
        // equipment during initialization; its grant must not retroactively select the beam loaded for this room.
        _artwork.UpdateBeamTilesAndPalette(_player.Colours, _player.Samus?.EquippedBeams ?? 0);

        // The destination's enemies load with the door-transition gates clear; their projectile places start from what
        // the outgoing room's left (RoomHandOver).
        var enemies = new RoomEnemySystem(_enemySession, CompiledRoomEnemyDefinitions.Instance, areaServices,
            _bossArtwork, _areaArtwork);
        enemies.TakeOverFrom(_room.HandOver);
        scope.Enemies = enemies;
        projectiles.Earthquakes.BindRoomEarthquake(enemies.Earthquake);
        enemies.Load(new RoomEnemyLoadRequest(
            room.State.EnemyPopulation,
            room.State.EnemyGraphicsSet,
            scope.Appearance,
            _colours)
        {
            Level = assets.LevelData,
            Samus = _player.Samus,
            ControllerInput = _controller1.Current,
            Camera = destinationCamera.PixelPosition,
            GunshipScenario = entry.GunshipScenario,
            DeferLoaderTimeCameraReads = viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor,
            ArtworkFrames = scope.ArtworkFrames,
        });
        // The shared Ridley initializer calls $90:A7E2 only for a live boss.
        // Ceres also disables the HUD map, but has no matching exploration list.
        if (enemies.Ceres.RidleyPresent)
        {
            _hud.DisableMinimapForBoss();
            // Only Norfair's Ridley marks his arena explored.
            if (enemies.RoomStatus.Boss == AreaBoss.Ridley)
                for (int row = 0; row < RidleyMapDefinitions.ArenaScreenRows; row++)
                    _system.MarkExploredMapTile(room.AreaIndex, room.MapPosition + new Offset16<MapTiles>(0, checked((short)(row + 1))));
        }
        _escapeEffects.SetupEscapeRoomEffects(room.State.SetupCallback, enemies, scope.EscapeDiagonalQuake);
        // Gate setup runs while the room PLM population is constructed, but Enemies.Load
        // subsequently clears the shared bank-$86 projectile pool. Consume those setup
        // requests here—the first point matching the cartridge's completed room teardown—
        // so the closed actor survives into the initial viewport.
        _enemyRoomEffects.ApplyPendingDownwardGateProjectileRequests(plms, enemies);
        // Setup ASM $8F:C8C8 spawns into the pool that Enemies.Load has just cleared.
        if (room.State.SetupCallback == RoomSetupCallback.SpawnPrePhantoonRoomEnemyProjectile)
            enemies.LoaderHooks.SpawnPrePhantoonRoomProjectile(_backgroundScroll);
        _enemyRoomEffects.ApplyEnemyRequestedRoomEffects(enemies.LastEnemyFrame.RoomEffects, plms, roomLayer3Fx, roomPaletteFx, enemies);
        scope.TourianStatues = new TourianStatueSequence(_tourianStatueDependencies, roomLayer3Fx, roomPaletteFx, plms, enemies);
        enemies.GraphicsSet.ShowGraphics();
        // The door-transition code that owns $0795/$0797 keeps them over the destination once its enemies are loaded.
        enemies.ResumeDoorTransitionGates(carryOver.DoorGates);

        if (viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor)
        {
            // State-$0B never calls DisplayViewablePartOfRoom: the source screen stays
            // visible while the opening scroll moves the destination in around it.
            _pendingDoorOpeningSourceView = doorOpeningSourceView;
            _room.Adopt(scope.Complete());
            return new InitialViewportResult();
        }

        _room.Adopt(scope.Complete());
        _pendingDoorOpeningSourceView = null;
        _backgroundScroll.Layer1Position = destinationCamera.PixelPosition;
        _backgroundScroll.CalculateScrolls();
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
        GameplayBasePalette artwork = _artwork.InitialPaletteArt ?? throw new InvalidOperationException(
            "Room loading requires the installed gameplay palette catalog.");
        artwork.LoadCommonSprites(SpriteColourSetDefinitions.Common.In(_colours.SpriteSets));
        artwork.LoadEnemyProjectileSprites(SpriteColourSetDefinitions.EnemyProjectiles.In(_colours.SpriteSets));
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
    internal SelectedRoom LoadSelectedRoom(RoomHeaderDefinition room)
    {
        AreaId areaIndex = Ensure.NotNull(room).AreaIndex;

        // RoomStateSelectionContext owns a detached copy of the events, so selection never
        // observes or changes the live progress.
        SamusState? samus = _player.Samus;
        var selection = new RoomStateSelectionContext(
            _system.Progress.Events.Copy(),
            BossBits: _system.GetBossBits(areaIndex),
            HasMorphBallAndMissiles:
                samus is not null &&
                samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall) &&
                samus.MaxMissiles != 0,
            HasPowerBombs: samus?.MaxPowerBombs != 0);
        return SelectedRoom.LoadUsingCompiledSelection(room, selection);
    }

    /// <summary>
    /// Replays the position arithmetic from <c>$80:AD30-$AF89</c> and <c>$82:E3C0/E6A2</c>
    /// to obtain the final camera/Samus coordinates after the native scrolling phase.
    /// </summary>
    private static DoorTransitionPlacement CalculateDoorTransitionPlacement(
        DoorDefinition door,
        FixedPosition<RoomPixels> source)
    {
        DoorDirection direction = door.Orientation.Direction;

        // Setup's first directional call (none for down), then PlaceSamusLoadTiles replaces
        // both whole positions, then every remaining scroll update moves along the travel axis.
        FixedPosition<RoomPixels> samus = DoorOpeningScrollState.RebaseSamus(
            door, DoorOpeningScrollState.ApplySetupMovement(door, source));
        FixedVector<RoomPixels> step = DoorOpeningScrollState.GetSamusStep(door);
        for (int update = DoorOpeningScrollState.MovingScrollUpdates(direction); update > 0; update--)
            samus = samus.Advance(step);

        int finalX = NudgeOffScreenRightEdge(NudgeOffScreenLeftEdge(samus.X.Whole));

        // LoadMoreThings applies this eight-pixel doorway alignment only horizontally.
        if (!direction.IsVertical())
            finalX = direction == DoorDirection.Right ? (ushort)(finalX | 7) : unchecked((ushort)(finalX & ~7));
        samus = samus.WithWhole(new Position16<RoomPixels>(checked((ushort)finalX), NudgeOffScreenLeftEdge(samus.Y.Whole)));
        // `$80:ADC8` adds $20 to door_destination_y_pos after using the unmodified value
        // for Samus's setup origin. The scroll's completion later snaps an upward transition's
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

        _player.Samus = new SamusState(_samusOverreads.AnimationContinuation)
        {
            FloorQueueResidue = _enemySession.FloorQueueResidue,
            Pose = SamusPoseId.ForwardFacingPowerSuitPose,
            AnimationFrame = 0,
            Position = FixedPosition<RoomPixels>.FromWhole(_room.ActiveLoadStation.SamusPosition.WithY(0)),
        };
        _artwork.BindSamusPalettePresentation();
        SamusPresentation.LoadPowerSuitPalette(_player.Colours.Suit, _artwork.MapPresentation?.SamusSuitColors);
        _tester.ApplyTesterInventory();

        // Fresh-game loading has one deliberately non-general palette write after copying
        // every target color into the live palette: `$82:8190` clears color $DF (CGRAM
        // color 223) immediately before Samus command $08 spawns the two Ceres elevator
        // projectiles. The stationary level-data concealer is four solid pixels of common
        // OBJ tile $20, whose opaque pixel selects precisely color $DF. Leaving the normal
        // blue value from `Initial_Palette_spritePalette5` in that one entry makes the
        // elevator landing recess appear pre-filled; the cartridge's zero makes it black
        // until the moving pad reaches it and both projectiles delete themselves.
        // The concealer's colour is Samus's grapple flare colour, which the cartridge's zero clears ($82:8190).
        _player.Colours.GrappleFlare.Fill(RgbColor.Black);

        SamusPoseMetadata.RefreshCollisionRadii(_player.Samus);
        SamusPresentation.InitializeAnimation(_player.Samus);
        _player.Samus.LiquidPhysics.RoomIdentity = _room.ActiveRoom.Identity;
        _player.Samus.Presentation.PrimeGraphics(_player.Samus);
        _player.PreviousMovementTypeForXray = SamusPoseMetadata.ReadMovementType(_player.Samus);

        // `$90:F21B/$90:F226` use SpawnEprojWithGfx and thus briefly seed both objects
        // from enemy slot zero. Their shared initializer at `$86:A301`, however, runs
        // before the first draw and explicitly clears that graphics index. The specialized
        // arrival model owns that native post-initialization value; passing the transient
        // enemy word here previously recolored the level-data concealer teal.
        _room.LoadedScope.BeginCeresElevatorArrival(new CeresElevatorArrivalState(_player.Samus));

        // The locked Ceres-start frame handler still publishes the initial minimap. The
        // ordinary update below used to be gated on GroundedSamusMovementEnabled, leaving
        // the ROM HUD template's unrelated map glyphs visible for the entire descent. Seed
        // the real area-six room coordinate now; subsequent normal frames keep updating it.
        ActiveRoomGeometry roomGeometry = GetActiveRoomGeometry();
        _hud.UpdateMinimap(_system, new MinimapFrame { AreaIndex = roomGeometry.AreaIndex, RoomMap = roomGeometry.MapPosition, RoomSize = _room.LevelData?.Size ?? _room.ActiveRoom.Size.ToRoomBlocks(), Samus = _player.Samus.Position.Whole, FrameCounter = _clock.ShortCounter, MapRevealMode = _options.MapRevealMode, PresentationMap = _artwork.MapPresentation?.Get(roomGeometry.AreaIndex) });

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

    /// <summary>
    /// The pictures the outgoing room's BG1 blocks show as a door begins (#1392), which its screen keeps through the
    /// door's opening scroll.
    /// </summary>
    private RoomBlockPictureTable SourceScreenPictures()
    {
        RoomScope scope = _room.Scope ?? throw new InvalidOperationException("A door began with no room loaded.");
        return scope.BlockPictures.Resolve(scope.LevelData.ForegroundPlane, null, RoomScope.BlockArtworkOf(_room.ActiveRoom),
            new RoomAppearanceView(scope.Appearance, scope.ArtworkFrames, _enemySession), []);
    }

    /// <summary>The host-bound room catalogs; room loading never substitutes cartridge artwork.</summary>
    private RoomAssetCatalogs RequireRoomAssetCatalogs() =>
        _artwork.RoomPaletteArt is { } palettes && _artwork.RoomVisualLayouts is { } layouts &&
        _artwork.RoomLevelStreams is { } levelStreams && _artwork.RoomBlockSheets is { } blockSheets
            ? new RoomAssetCatalogs(palettes, layouts, levelStreams, blockSheets)
            : throw new InvalidOperationException(
                "Room loading requires installed palette, visual-layout, level-stream and block-sheet assets.");


    /// <summary>The 16-pixel block a coordinate lies in within its 256-pixel screen.</summary>
    private static int BlockInScreen(int pixel) => (pixel >> 4) & 15;

    /// <summary>
    /// The doorway nudge of <c>$82:E6A2</c> and <c>LoadMoreThings</c>: a coordinate in its screen's second block moves
    /// to eight pixels past that block.
    /// </summary>
    private static ushort NudgeOffScreenLeftEdge(int pixel) =>
        BlockInScreen(pixel) == 1 ? unchecked((ushort)((pixel | 15) + 8)) : checked((ushort)pixel);

    /// <summary>The doorway nudge's other edge: a coordinate in its screen's fifteenth block moves to eight pixels before it.</summary>
    private static ushort NudgeOffScreenRightEdge(int pixel) =>
        BlockInScreen(pixel) == 14 ? unchecked((ushort)((pixel & ~15) - 8)) : checked((ushort)pixel);
}
