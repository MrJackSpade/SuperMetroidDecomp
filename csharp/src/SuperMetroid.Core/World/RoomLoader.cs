using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
using SuperMetroid.Core.Enemies.Shaktool;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// Room loading: saved games and new sessions, rooms and doors, room setup, palettes, the HUD minimap,
/// enemies and room objects of the loaded room.
/// </summary>
[RoomScopeBoundary("Builds each room's scope in the game's load order.")]
internal sealed class RoomLoader
{
    /// <summary>Supplies decoded graphics and palette assets for the room being entered.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Tracks both background layers' scroll positions across door loading.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Retains controller state needed when a room transition begins.</summary>
    private readonly ControllerInputState _controller1;
    /// <summary>Publishes the display setup for a newly active room.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Coordinates the door loader's staged transition states.</summary>
    private readonly DoorTransitionRunner _doorTransition;
    /// <summary>The enemy state and installed enemy artwork that outlive each room; each room's enemies are built over it.</summary>
    private readonly EnemySession _enemySession;

    /// <summary>The session's installed boss artwork, which each room's bosses read.</summary>
    private readonly BossArtworkBinding _bossArtwork;

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
    /// <summary>The session's kept scroll cells; they outlive each room, so an implicit layout keeps the previous room's cells past its own.</summary>
    private readonly ScrollCellsKeptAcrossRooms<RoomScrollState, AreaId> _scrollCells;
    /// <summary>Constructs room objects after persistent state has been restored.</summary>
    private readonly RoomObjectVisuals _shotBlockVisuals;
    /// <summary>Resets Samus projectiles for the destination room.</summary>
    private readonly BeamChargeState _beamCharge;
    /// <summary>The runtime's audio requests, which every room system and every Samus built here queue theirs into.</summary>
    private readonly GameplayAudioRequests _audioRequests;
    /// <summary>The installed services reproducing Samus's glitch reads, handed to every Samus and projectile system built here.</summary>
    private readonly SamusOverreadServices _samusOverreads;
    /// <summary>
    /// The session's unauthored Speed Booster cadence, handed to every Samus built here and to every room's enemies, whose
    /// Ceres Ridley getaway leaves its delays behind.
    /// </summary>
    private readonly UnauthoredBoostCadence _unauthoredBoostCadence;
    /// <summary>
    /// The runtime's shared fade step count, handed to every room's enemies, whose backdrop and body fades count their steps
    /// on it with the runtime's door, death and time-up fades.
    /// </summary>
    private readonly SharedFadeStepCount _fadeStepCount;
    /// <summary>Holds the active room, camera, selected door, and load-station identity.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>The Fireflea state, the room's fixed colour and installed FX artwork each room's effect-overlay FX reads.</summary>
    private readonly RoomFxSession _roomFx;
    /// <summary>The colour effect handler state each room's colour effects reads.</summary>
    private readonly ColourEffectSessionState _colourEffectSession;
    /// <summary>The session owners each room's Tourian statue sequence is built over.</summary>
    private readonly TourianStatueSequenceDependencies _tourianStatueDependencies;
    /// <summary>Provides persistent events and defeated bosses used by room-state selectors.</summary>
    private readonly SharedGameState _system;
    /// <summary>Grants the assisted loadout and resets saved encounters after a save is restored.</summary>
    private readonly ActiveAssists _assists;


    /// <summary>Captures the owners of room, player, graphics, and door state used by the loader.</summary>
    internal RoomLoader(RoomLoaderDependencies dependencies)
    {
        _artwork = dependencies.Artwork;
        _backgroundScroll = dependencies.BackgroundScroll;
        _controller1 = dependencies.Controller1;
        _display = dependencies.Display;
        _doorTransition = dependencies.DoorTransition;
        _enemySession = dependencies.EnemySession;
        _bossArtwork = dependencies.BossArtwork;
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
        _audioRequests = Ensure.NotNull(dependencies.AudioRequests);
        _samusOverreads = Ensure.NotNull(dependencies.SamusOverreads);
        _unauthoredBoostCadence = Ensure.NotNull(dependencies.UnauthoredBoostCadence);
        _fadeStepCount = Ensure.NotNull(dependencies.FadeStepCount);
        _room = dependencies.Room;
        _roomFx = dependencies.RoomFx;
        _colourEffectSession = dependencies.ColourEffectSession;
        _tourianStatueDependencies = dependencies.TourianStatueDependencies;
        _system = dependencies.System;
        _assists = dependencies.Assists;
        _scrollCells = RoomScrollGrid.KeepCellsAcrossRooms(_system.StoredExploredMaps);

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
    /// Executes the data-loading portion of a new game's setup: area six, station
    /// zero, the station-selected room, and its fresh-save state-selection branch.
    /// </summary>
    public InitialViewportResult InitializeStartingCeresRoom()
    {
        // A new game loads at the Ceres start; the station names the room, door and placement.
        LoadStation station = LoadStationDefinitions.CeresElevator;
        DoorDefinition door = station.Placement.Door;
        if (door.DestinationRoom != station.Placement.Room)
        {
            throw new InvalidDataException(
                $"Ceres station room {station.Placement.Room.Name} disagrees with door " +
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
            station.Placement.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { LoadStation = station });
    }

    /// <summary>
    /// Restores an existing slot through the game's area/load-station tables and
    /// enters the normal playable state at the station-authored Samus coordinates.
    /// </summary>
    /// <remarks>
    /// Startup displays a short front-facing Samus appearance before settling into
    /// ordinary standing control. That presentation coroutine is not a room-loading
    /// responsibility, so this method establishes its deterministic endpoint rather than
    /// replaying guessed timing. Room selection, coordinates, inventory, progression bits,
    /// enemies, room objects, graphics, beam graphics, and camera all remain driven.
    /// </remarks>
    public InitialViewportResult InitializeSavedGame(SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(slot);
        LoadStation station = slot.Checkpoint;
        AreaId requestedArea = station.Area;
        DoorDefinition door = station.Placement.Door;
        if (door.DestinationRoom != station.Placement.Room)
        {
            throw new InvalidDataException(
                $"Load station {station} room {station.Placement.Room.Name} " +
                $"disagrees with door {door.Name}'s destination " +
                $"{door.DestinationRoom.Name}.");
        }

        // Apply progression before room construction. Enemy and room-object initialization query
        // boss/event/item flags synchronously, while room-state selection and beam graphics
        // depend on those same flags. In particular, loading a post-Morph-Ball save must not
        // accidentally construct the untouched new-game version of Blue Brinstar.
        RestoreSavedPlayerState(slot);
        _player.IntroducedSamus.Position = _player.IntroducedSamus.Position.WithWholeX(station.Placement.SamusPosition.X);
        _player.IntroducedSamus.Position = _player.IntroducedSamus.Position.WithWholeY(station.Placement.SamusPosition.Y);
        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);
        if (room.AreaIndex != requestedArea)
        {
            throw new InvalidDataException(
                $"Load station {station} targets room area " +
                $"{room.AreaIndex}.");
        }
        InitialViewportResult viewport = LoadRoom(
            door,
            room,
            station.Placement.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { LoadStation = station });
        // Loading from a load station sets the save-station lockout. Without this room-entry lockout, the
        // first standing floor probe after loading directly on a save pod immediately
        // reopens its confirmation message.
        _room.LoadedScope.RoomObjects.Stations.LockSaveStationForCurrentRoomEntry();

        // The Zebes start setup does not begin at its endpoint. It selects a front-facing pose,
        // frame two/timer three, a suit-specific palette object, and the 360-call
        // appearance handler before restoring ordinary input.
        SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(_player.Colours, _player.IntroducedSamus.EquippedItems);
        SamusPoseMetadata.ApplyZebesStartPoseSetup(_player.IntroducedSamus);
        SamusPresentation.SetAnimationFrameFromSpecialHandler(_player.IntroducedSamus, frame: 2, timer: 3);
        _player.IntroducedSamus.LiquidPhysics.RoomIdentity = room.Identity;
        ColourEffectId loadAppearance =
            _player.IntroducedSamus.EquippedItems.HasAny(SamusEquipmentFlags.GravitySuit)
                ? SavedGameAppearanceDefinitions.GravitySuitGlow
                : _player.IntroducedSamus.EquippedItems.HasAny(SamusEquipmentFlags.VariaSuit)
                    ? SavedGameAppearanceDefinitions.VariaSuitGlow
                    : SavedGameAppearanceDefinitions.PowerSuitGlow;
        _player.BeginLoadAppearance(loadAppearance, SavedGameAppearanceDefinitions.FrameCount);
        _room.LoadedScope.ColourEffects.Start(
            loadAppearance,
            _player.IntroducedSamus.EquippedItems,
            _system.HasDefeatedAnyOf(room.AreaIndex, DefeatedBosses.AreaMiniBoss));
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
    internal void RestoreSavedPlayerState(SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(slot);
        _player.Samus = new SamusState(_samusOverreads.AnimationContinuation, _unauthoredBoostCadence, SamusCompatibility.ForNewSamus(), _beamCharge)
        {
            Pose = SamusPoseId.FacingRightNormalPose,
            AnimationFrame = 0,
        };
        _player.Samus.BindAudioRequests(_audioRequests);
        _artwork.BindSamusPalettePresentation();
        slot.ApplyTo(_player.Samus, _system);
        // Loading the save sets the stored explored maps from it; the live map is loaded only at file select.
        StoredExploredMapLoads.LoadFile(_system.StoredExploredMaps, slot.Progression);
        _assists.ResetSavedEncounters();
        _assists.GrantLoadoutToNewSamus();
        // Loading clears both persisted HUD selection values after the save restore.
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
    /// Executes the game-data loading's special branch for arriving on Zebes after Ceres:
    /// Crateria area zero, load-station eighteen, with the airborne Landing Site gunship.
    /// </summary>
    public InitialViewportResult InitializePostCeresZebesRoom()
    {
        SamusState carried = _player.Samus
            ?? throw new InvalidOperationException(
                "The Ceres escape must retain its live Samus state through the cinematic.");

        // Initializing Samus clears all of her state, including the speeds and pose
        // state the escape left behind; only the inventory below it survives. The player is
        // the same one, so a loadout already granted to her is not granted again.
        // Initialization clears Pose; the model's constructor defaults to the right-facing pose.
        var samus = new SamusState(_samusOverreads.AnimationContinuation, _unauthoredBoostCadence, SamusCompatibility.ForNewSamus(), _beamCharge) { Pose = 0, AnimationFrame = 0 };
        samus.CopyInventoryFrom(carried);
        samus.BindAudioRequests(_audioRequests);
        _player.Samus = samus;
        _assists.CarryLoadoutGrant(carried, samus);
        _artwork.BindSamusPalettePresentation();

        // The arrival on Zebes is a cutscene-only entry high above Landing Site; the gunship is
        // not the checkpoint until the gunship's landing finishes and performs the save.
        LoadStation station = LoadStationDefinitions.ZebesArrival;
        DoorDefinition door = station.Placement.Door;
        if (door.DestinationRoom != station.Placement.Room)
        {
            throw new InvalidDataException(
                $"Post-Ceres station room {station.Placement.Room.Name} disagrees with " +
                $"door {door.Name}'s destination {door.DestinationRoom.Name}.");
        }

        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);
        if (room.AreaIndex != AreaId.Crateria)
        {
            throw new InvalidDataException(
                $"Post-Ceres station {station} targets area {room.AreaIndex}, expected Crateria.");
        }

        // The cinematic refilled health on the preceding frame; the Ceres start then seeds the
        // hurt check from it. The Ceres start's control setup locks Samus's state handlers.
        samus.Health = samus.MaxHealth;
        samus.PreviousHealthForHurtCheck = samus.Health;
        samus.Kinematics.Position = samus.Kinematics.Position.WithX(new FixedCoordinate(station.Placement.SamusPosition.X, 0)).WithY(new FixedCoordinate(station.Placement.SamusPosition.Y, 0));
        samus.Control.LockHandlers();

        // The landing branch moves the game to Crateria and loads the live explored map from
        // Crateria's stored one before the room loads.
        _system.StoredExploredMaps.LoadLiveMap(AreaId.Crateria);

        // The special loader still executes the normal room-object teardown before it
        // constructs Landing Site. CeresElevatorArrival models two projectiles
        // of the abandoned station room, so Landing Site's scope ends them; retaining them
        // would let their delayed completion at Y=72 overwrite the Samus position currently
        // carried by the gunship.
        InitialViewportResult viewport = LoadRoom(
            door,
            room,
            station.Placement.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { GunshipScenario = GunshipLoadScenario.EscapingCeres, LoadStation = station });
        SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(_player.Colours, samus.EquippedItems);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.LiquidPhysics.RoomIdentity = room.Identity;
        _player.PreviousMovementTypeForXray = SamusPoseMetadata.ReadMovementType(samus);
        _player.GroundedSamusMovementEnabled = false;
        return viewport;
    }

    /// <summary>
    /// True after the door-block handler has selected a normal destination door and
    /// before the frontend's door-transition states consume it.
    /// </summary>
    public bool HasPendingDoorTransition => _room.LevelData?.PendingDoorTransition is not null;

    /// <summary>The unconsumed door selected by the door-block collision handler.</summary>
    public DoorDefinition? PendingDoorTransition => _room.LevelData?.PendingDoorTransition;

    /// <summary>
    /// Runs one source-room alignment call at the start of a door transition. Horizontal doors move the
    /// camera's Y toward the nearest screen boundary; vertical doors move X. The offset from the boundary, read as
    /// -128 through 127, deliberately takes -1 to 0 and 1 to 0 in one-pixel steps.
    /// </summary>
    /// <returns>True only when the camera already lay on that screen boundary on entry.</returns>
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

        // The alignment increments or decrements the whole camera position only; fractions, the ideal
        // camera position and Samus's previous position are untouched.
        _room.Camera.SetDoorTransitionPosition(alignment.CameraPosition);

        // The game updates the background's position and scroll on every
        // convergence step; the gameplay scroll owner performs the same parallax math.
        _backgroundScroll.ForegroundPosition = _room.Camera.PixelPosition;
        _backgroundScroll.StepScrolling();
        return false;
    }

    /// <summary>
    /// The axis the door alignment aligns, perpendicular to travel: right/left converge camera Y,
    /// while down/up converge camera X. This literal branch matters because aligning the
    /// travel axis instead moves a horizontal door frame by a row while its room
    /// scroll is being staged.
    /// </summary>
    internal static bool DoorTransitionAlignsX(DoorDirection direction) => direction.IsVertical();

    /// <summary>Whether the pending door's destination room definition selects the isolated door transition.</summary>
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
        if (_room.DoorTransitionShownLayers is not null)
            throw new InvalidOperationException("The door-transition display is already active.");

        bool suppressForeground = sourceIsolated || destinationIsolated;
        _room.DoorTransitionShownLayers = suppressForeground
            ? ScreenLayers.Sprites
            : ScreenLayers.Foreground | ScreenLayers.Sprites;
    }

    /// <summary>Returns the screen to the destination room's ordinary layers.</summary>
    internal void EndDoorTransitionDisplay()
    {
        if (_room.DoorTransitionShownLayers is null)
            throw new InvalidOperationException("The door-transition display is not active.");
        // The update that hands the screen back still counts as a scrolling update; the
        // room's ordinary display takes over on the following frame.
        RestartShortCounterDuringDoorScroll();
        _room.DoorTransitionShownLayers = null;
        _doorFrameCounting.EndScroll();
    }

    /// <summary>
    /// Restarts the short frame counter on an update of the door scroll, from the update that places Samus for the destination's
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


    /// <summary>Rebases Samus and publishes the scroll's first moving step during the destination's loading.</summary>
    internal void PlaceSamusForDestinationLoading()
    {
        DoorDefinition door = PendingDoorTransition
            ?? throw new InvalidOperationException("No pending door destination exists.");
        if (_player.Samus is null || _room.Camera is null)
            throw new InvalidOperationException("Door tile loading requires Samus and camera.");
        FixedPosition<RoomPixels> position = DoorOpeningScrollState.RebaseSamus(door, _player.Samus.Position);
        _room.Camera.PublishDoorSamusPosition(position.Whole);
        _player.Samus.Position = DoorOpeningScrollState.AdvanceSamus(door, position);
        DoorDirection direction = door.Orientation.Direction;
        int delta = direction is DoorDirection.Right or DoorDirection.Down ? 4 : -4;
        bool vertical = direction.IsVertical();
        Offset16<RoomPixels> firstStep = vertical ? new(0, delta) : new(delta, 0);
        PublishDoorCameraPosition(DoorOpeningScrollState.GetSetupCamera(door) + firstStep);
        _room.Camera.PublishDoorSamusTravelAxis(vertical, _player.Samus.Position.Whole);
        _doorFrameCounting.BeginScroll();
        RestartShortCounterDuringDoorScroll();
    }

    /// <summary>
    /// Rewinds an atomically loaded destination to the position established by the
    /// scroll's setup and Samus's placement for the destination's loading.
    /// </summary>
    /// <remarks>
    /// The room constructor loads the room's blocks, room objects, FX and enemies in one operation, while the
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
            _backgroundScroll.BackgroundPosition);
        DoorDirection direction = door.Orientation.Direction;
        DoorOpeningSourceView sourceScroll = _pendingDoorOpeningSourceView
            ?? throw new InvalidOperationException(
                "Door-opening scroll lost the source room's view.");
        // The offsets derive from the foreground position staged before setup's first call:
        // horizontal setups had not yet taken their four-pixel step, and an upward door's
        // correction of the source room's top row stages the foreground five (the background four) pixels lower.
        Offset16<RoomPixels> foregroundStaging = direction switch
        {
            DoorDirection.Right => new(-4, 0),
            DoorDirection.Left => new(4, 0),
            DoorDirection.Down => Offset16<RoomPixels>.Zero,
            DoorDirection.Up => new(0, 5),
            _ => throw new InvalidOperationException($"Undefined door direction {direction}."),
        };
        Position16<RoomPixels> stagedForeground = _doorOpeningScroll.CameraPosition + foregroundStaging;
        _backgroundScroll.BeginDoorOpeningViews(stagedForeground);
        _pendingDoorOpeningSourceView = null;
        // The source screen stays where it was on the screen, lying where the destination's view begins, while the
        // destination scrolls in around it.
        _room.DoorSourceBackground = RoomDoorBackground.Begin(sourceScroll.SourceBackground, sourceScroll.ForegroundScroll.AsPlaneScroll(),
            stagedForeground.AsPlaneScroll(), direction == DoorDirection.Down);
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
            state.SnapCameraToDestination();
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
        _backgroundScroll.BackgroundPosition = state.BackgroundPosition;
    }

    /// <summary>Publishes a door-owned camera position to both the camera and the scroll owner.</summary>
    private void PublishDoorCameraPosition(Position16<RoomPixels> camera)
    {
        _room.LoadedCamera.SetDoorTransitionPosition(camera);
        _backgroundScroll.ForegroundPosition = camera;
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
                _player.Samus.Position = _player.Samus.Position.WithWrappedWholeX(_player.Samus.Position.X.Whole | (7));
                break;
            case DoorDirection.Left:
                _player.Samus.Position = _player.Samus.Position.WithWrappedWholeX(_player.Samus.Position.X.Whole & ~7);
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
    /// position, then an elevator door's control setup (the elevator setup going down, the lock going up). Releases
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
        // but it does not restore the four suspended subsystems. Destination room setup
        // therefore begins in indirect G-Mode and cannot create a visible gate actor.
        _player.Samus.Xray.TransitionDirectGModeToIndirect();

        // The desktop's opt-in error reporter resumes at the next frame boundary.
        // Code cannot throw, but room construction can still reject an
        // unsupported definition. Build the read-only asset graph before consuming the
        // source room's pending door so such a failure remains retryable instead of leaving
        // the door transition with no destination on the following frame.
        LoadedRoomAssets assets = LoadedRoomAssets.Load(_scrollCells, room, RequireRoomAssetCatalogs());
        DoorDefinition consumedDoor = sourceLevel.ConsumePendingDoorTransition()
            ?? throw new InvalidOperationException(
                "Pending door disappeared while its destination assets were prepared.");
        if (consumedDoor != door)
        {
            throw new InvalidOperationException(
                $"Pending door changed from {door.Name} to " +
                $"{consumedDoor.Name} while its destination assets were prepared.");
        }

        // A door that exchanges explored maps stores the live map as the departed area's,
        // then, with the destination's definition loaded, loads the live map from the entered area's.
        if (ExploredMapExchangeDoorDefinitions.Doors.Contains(door))
        {
            _system.StoredExploredMaps.StoreLiveMap(_room.LoadedRoom.AreaIndex);
            _system.StoredExploredMaps.LoadLiveMap(room.AreaIndex);
        }

        // The door transition promotes a departing elevator's global status from one to
        // two after the destination room objects, door code, and door setup have been created. The
        // room load initializes enemies in the same call, so publish that carried
        // status immediately before Enemies.Load: the destination elevator initializer
        // must see two and place itself at its arrival height instead of clearing the journey.
        bool arrivingByElevator = _room.LoadedScope.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Departing;
        if (arrivingByElevator)
            _room.LoadedScope.Enemies.Elevator.PrepareElevatorArrival();

        // The load-station record established only the first room. Once the door collision publishes
        // a door definition, that definition and its destination definition become authoritative:
        // the destination's scope is built without a load station.

        // The door's scroll has already placed Samus before the destination room is
        // constructed. This ordering is observable for elevator arrivals:
        // The elevator's initialization sees status two, moves the platform to its arrival height, and then pins
        // Samus to it. Applying the generic placement after Enemies.Load used to overwrite
        // that attachment and exposed the upward arrival one row too low.
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
            new RoomEntry { RunDoorClosingObject = true, PreloadedAssets = assets });

        // The door transition loads Samus's suit target palette after room/enemy palettes have been
        // loaded and before the door fade captures the destination fade target. The source fade
        // has already driven Samus's colours to black at this point. Omitting this shared
        // reload therefore retained valid sprite and room pictures but rendered Samus as a solid
        // black silhouette after every ordinary desktop door transition.
        SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.Load(_player.Colours, _player.Samus.EquippedItems);

        // The door loader sets no Samus radius; the next unlocked input mode's radius update
        // does, so an elevator rider keeps the boarding pose's radius through the door.
        _player.Samus.LiquidPhysics.RoomIdentity = room.Identity;
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
            RoomViewportLoadMode.StreamThroughDoor => new DoorOpeningSourceView(_backgroundScroll.ForegroundScroll,
                new RoomBackgroundLayer(_room.LoadedLevelData.ForegroundPlane.Cells, SourceScreenPictures())),
            _ => throw new ArgumentOutOfRangeException(
                nameof(viewportLoadMode), viewportLoadMode, "Unknown room viewport load mode."),
        };
        LoadedRoomAssets assets = entry.PreloadedAssets ??
            LoadedRoomAssets.Load(_scrollCells, room, RequireRoomAssetCatalogs());

        // The destination reads the outgoing room only through this capture; from here on the room views show the
        // destination's parts as each step builds them.
        var carryOver = new RoomTransitionCarryOver(_room.Camera, _room.InheritedCameraPosition,
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
        var wreckedShipTreadmill = new WreckedShipTreadmillBlockAnimationsState();
        scope.WreckedShipTreadmill = wreckedShipTreadmill;
        var maridiaElevatube = new MaridiaElevatubeRoomMainState(
            active: room.State.MainBehavior == RoomMainBehavior.MaridiaElevatube,
            _room.RoomMainCarryover);
        scope.MaridiaElevatube = maridiaElevatube;
        var ceresElevatorShaft = new CeresElevatorShaftRoomMainState(
            active: room.State.MainBehavior == RoomMainBehavior.CeresElevatorShaft &&
                door.EntersCeresElevatorShaft,
            _room.RoomMainCarryover);
        scope.CeresElevatorShaft = ceresElevatorShaft;
        scope.CeresFallingDebris = new CeresFallingDebrisRoomMainState(_room.RoomMainCarryover);
        scope.EscapeDiagonalQuake = new EscapeDiagonalQuakeState(_room.RoomMainCarryover);
        // Ceres's elevator door setup levels the shaft about its centre before the fresh Ceres
        // elevator room becomes visible. Publishing the immutable transform here gives
        // Samus, her projectiles, and the rotating elevator's steam a single authoritative
        // producer. Ordinary doors clear it so stale rotating-view math cannot leak across rooms.
        TurningFloorTransform? initialFloorTransform = door.EntersCeresElevatorShaft
            ? TurningFloorTransform.Level(CeresElevatorShaftRoomMainState.CeresShaftRotationCenter)
            : null;
        _display.ActiveTurningFloor = initialFloorTransform;
        // The Ceres elevator door setup levels the shaft before it is
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
            destinationCamera.SetDoorTransitionPosition(camera, previousCamera?.Position ?? carryOver.InheritedCameraPosition);
        }
        else
        {
            destinationCamera.SetPosition(camera);
            if (previousCamera is not null)
                destinationCamera.RetainSubpositions(previousCamera);
            else
                destinationCamera.RetainSubpositions(carryOver.InheritedCameraPosition);
        }
        _backgroundScroll.BackgroundScrollModes = room.State.BackgroundScrollModes;
        if (viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport)
            _backgroundScroll.CalculateScrolls();
        else
        {
            // Up calculates background from destination+31 before changing the scroll's camera
            // endpoint to destination+32. Keeping those adjacent positions distinct prevents
            // a parallax room from accumulating a one-pixel layer mismatch at the snap.
            Position16<RoomPixels> parallaxBasis = door.Orientation.Direction == DoorDirection.Up
                ? camera - new Offset16<RoomPixels>(0, 1)
                : camera;
            _backgroundScroll.PrepareDoorOpeningDestination(camera, parallaxBasis);
        }

        // The room-main behaviour runs even in Crateria's rock room, whose background stays its own background.
        // The sky's banded horizontal motion belongs to the separately spawned sky effect, which the room's FX or
        // setup starts, not to the room's main behaviour.
        RoomEffectDefinition? selectedFx = room.State.Fx?.Select(door);
        bool skyHasBands = ScrollingSkyState.SpawnsBands(selectedFx?.Type, room.State.SetupBehavior);
        scope.ScrollingSky = ScrollingSkyState.IsScrollingSkyRoomMain(room.State.MainBehavior)
            ? new ScrollingSkyState(ScrollingSkyState.VariantOf(room.State.MainBehavior), skyHasBands)
            : null;

        // The room's pictures carry its tileset's colours (#1512), so the load colours nothing.
        // The elevator shaft and Ceres Ridley's arena show their tileset's turning floor, which the shaft's door and Ridley
        // then draw over (#1349, #1467); every other room shows none (#1649).
        scope.Floor = assets.CreateFloor();

        // Loading the FX definition selects the door-specific record, applies its palette
        // blend, chooses the effect page the effect overlay plane shows, and creates the type-owned per-line
        // effect and block animation state before colour effects and enemies begin running.
        var roomFxOverlay = RoomFxOverlayState.ForRoom(
            _roomFx,
            room.State.Fx,
            door,
            _system.RandomNumber,
            room.Definition);
        roomFxOverlay.BindAudioRequests(_audioRequests);
        scope.FxOverlay = roomFxOverlay;
        roomFxOverlay.PrimeViewport(destinationCamera.PixelPosition);
        scope.CeresHaze = new CeresHazeState(RoomBehaviorDefinitions.CeresHazeSetups.Contains(room.State.SetupBehavior),
            _system.HasDefeatedAnyOf(room.AreaIndex, DefeatedBosses.AreaBoss),
            viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport);
        scope.SandBlockAnimations = new RoomSandBlockAnimationsState(room.State.Fx, door);
        scope.SpikeBlockAnimations = new RoomSpikeBlockAnimationsState(room.State.Fx, door);
        scope.TreadmillBlockAnimations = new RoomTreadmillBlockAnimationsState(room.State.Fx, door);
        if (_player.Samus is not null)
            _player.Samus.LiquidPhysics.ConfigureFor(roomFxOverlay);

        // Loading reads the FX definition after room setup and before enemies. Its selected
        // record names two independent effect lists. The palette half must
        // be created on every shared room-load path; Landing Site's lightning replaces the
        // colors used by the scrolling-sky horizon on the first gameplay handler call.
        var roomColourEffects = ColourEffectSystem.ForRoom(
            _colourEffectSession,
            room.State.Fx,
            door,
            _player.Samus?.EquippedItems ?? 0,
            _system.HasDefeatedAnyOf(room.AreaIndex, DefeatedBosses.AreaMiniBoss));
        roomColourEffects.BindAudioRequests(_audioRequests);
        scope.ColourEffects = roomColourEffects;


        // Loading destroys room-owned objects before constructing the destination: the
        // destination's projectiles and bombs are built new here, before Load/beam graphics, so no
        // stale index reaches into them; its room-object system is built with the population below.
        // The held beam charge survives on the session; Samus's own echo storage is cleared.
        if (_player.Samus is { } loadingSamus)
        {
            loadingSamus.HorizontalSpeed.ClearSpeedEchoes();
            loadingSamus.Shinespark.ResetProjectileEchoStorage();
        }
        var bombs = new SamusBombProjectileSystem();
        // Each room's shot places start keeping nothing; TakeOverFrom then hands them what the outgoing room's kept.
        var projectiles = new SamusProjectileSystem(_beamCharge, _samusOverreads,
            new LeftMuzzleSubpixelCarry(SamusProjectileSystem.SlotCount));
        projectiles.TakeOverFrom(_room.HandOver);
        bombs.BindAudioRequests(_audioRequests);
        projectiles.BindAudioRequests(_audioRequests);
        scope.BombProjectiles = bombs;
        scope.Projectiles = projectiles;
        _artwork.BindProjectiles(projectiles, bombs);

        // The room-object loader walks this list once. Each record allocates
        // the highest free one of forty IDs before its setup executes. Immediate
        // deletion can therefore free that same ID for the next authored record, and enemy code
        // can observe the resulting physical identity/argument ordering. All room families
        // enter through this one parser; an unsupported definition throws with complete record
        // context instead of being skipped by a collection of sibling scans.
        // The room's room-object system is built with its bindings, then one set of runtime services
        // serves the room's room objects and enemies.
        var progress = new RoomAreaProgress(_system, room.AreaIndex);
        var roomObjectServices = new RoomObjectRuntimeServices(scope, _player, _system);
        var roomObjectRoom = new RoomObjectRoom(assets.LevelData, _system, room.AreaIndex);
        var roomObjectBindings = new RoomObjectServices
        {
            Samus = roomObjectServices,
            Bosses = progress,
            Events = progress,
            TourianStatue = roomObjectServices,
            RoomFx = roomFxOverlay is { } fx ? new RoomObjectFx(fx, () => _player.Samus) : null,
            Earthquake = roomObjectServices,
            Enemies = roomObjectServices,
        };
        // Each room's room objects get their own collectible block pairs, the room's first collectible that shows its own
        // picture taking the first pair, and their own places, every one free.
        var roomObjects = new RoomObjectSystem(roomObjectRoom, roomObjectBindings, _shotBlockVisuals, new CollectibleBlockPairs(),
            new RoomObjectPlaces());
        scope.RoomObjects = roomObjects;
        roomObjects.Sounds.BindAudioRequests(_audioRequests);
        var areaServices = new RoomEnemyRuntimeServices(_system, _room, _player, roomObjects, _display, progress);
        roomObjects.PopulationLoader.LoadRoomPopulation(roomObjectRoom, room.State.ObjectPopulation, roomObjectBindings);

        // The door transition runs the door setup only after destination room objects
        // exist and before enemy initialization/initial viewport construction. Several of
        // those setups rewrite the room scroll array, so postponing this call until the
        // first gameplay frame would already have streamed a viewport with wrong camera
        // limits.
        _doorTransition.RunDoorSetup(door, roomObjects, wreckedShipTreadmill, ceresElevatorShaft, maridiaElevatube);
        if (room.State.SetupBehavior == RoomSetupBehavior.SetupShaktoolRoomObject)
            roomObjects.Shaktool.TrySpawnShaktoolRoomController(destinationCamera.Scrolls);
        if (room.State.SetupBehavior == RoomSetupBehavior.TurnWallIntoShotBlocksDuringEscape)
            roomObjects.EscapeAnimals.SetupCrittersEscapeBlock(assets.LevelData,
                assets.LevelData.GridBlock(new(EscapeAnimalWallDefinitions.WallX, EscapeAnimalWallDefinitions.WallY)));
        // The Climb's escape setup spawns the fake-wall room object; the spawn silently drops it when all
        // forty slots are occupied.
        if (room.State.SetupBehavior == RoomSetupBehavior.AutoDestroyWallDuringEscape)
            roomObjects.Tourian.TrySpawnOldTourianEscapeShaftWall(assets.LevelData);
        // The mainstreet's escape setup also spawns this room object before writing the quake SetupEscapeRoomEffects owns.
        if (room.State.SetupBehavior == RoomSetupBehavior.ClearBlocksAfterSavingAnimalsAndShakeScreen)
            roomObjects.EscapeAnimals.TrySpawnCrateriaMainstreetEscapePassage(assets.LevelData);

        if (entry.RunDoorClosingObject)
        {
            // The door transition performs this handoff after destination room objects, door/room setup,
            // and FX construction but before enemy initialization. The C# room constructor
            // groups those owners differently, yet this point preserves the observable
            // room-object-pool order: every room-authored actor exists before the closer requests
            // the next highest free slot, and no enemy-authored room object has run yet.
            roomObjects.DoorObjects.TrySpawnDoorClosingObject(assets.LevelData, door, _system);
        }

        // Loading shows the entering beams' colours before it initializes enemies. Golden Torizo's code changes the
        // equipment during initialization and leaves those colours shown; the shots fired after it show the granted
        // beams' own pictures (#1586).
        SamusProjectileSystem.LoadBeamPalette(_player.Colours, _player.Samus?.EquippedBeams ?? 0);

        // The destination's enemies load with the door-transition gates clear; their projectile places start from what
        // the outgoing room's left (RoomHandOver).
        // Each enemy system gets its own compatibility services: the sweep, the Shaktool chain, the slots and the sprite-object places start
        // with no earlier kill, occupant or taken place, and a Puyo's ceiling turn starts from what loading the beams'
        // colours above left. The fade step count and the boost cadence outlive the room and are handed on.
        var compatibility = new RoomEnemyCompatibility
        {
            PowerBombRadius = new PowerBombRadiusAfterKill(),
            BullInheritance = new SlotInheritance<BullEnemyState>(EnemySlotOrder.Capacity),
            BeetomInheritance = new SlotInheritance<BeetomEnemyState>(EnemySlotOrder.Capacity),
            PuyoCeilingTurn = new PuyoCeilingTurnFromLastDraw(),
            SpriteObjectPlaces = new SpriteObjectPlaces(),
            FadeStepCount = _fadeStepCount,
            UnauthoredBoostCadence = _unauthoredBoostCadence,
            KilledShaktoolSegment = new KilledShaktoolSegment<RoomEnemySlot, ShaktoolSegmentState>(),
        };
        var enemies = new RoomEnemySystem(_enemySession, compatibility, CompiledRoomEnemyDefinitions.Instance, areaServices,
            _bossArtwork);
        enemies.TakeOverFrom(_room.HandOver);
        enemies.Audio.BindAudioRequests(_audioRequests);
        scope.Enemies = enemies;
        // A wall jump's waiting shake and Samus's solid-enemy contacts name enemies of the room being replaced, which
        // leave with it.
        _player.Samus?.Kinematics.ForgetUnloadedRoomEnemies();
        projectiles.Earthquakes.BindRoomEarthquake(enemies.Earthquake);
        enemies.Load(new RoomEnemyLoadRequest(
            room.State.EnemyPopulation,
            scope.Appearance)
        {
            Level = assets.LevelData,
            Samus = _player.Samus,
            ControllerInput = _controller1.Current,
            Camera = destinationCamera.PixelPosition,
            GunshipScenario = entry.GunshipScenario,
            DeferLoaderTimeCameraReads = viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor,
        });
        // The shared Ridley initializer marks the map explored only for a live boss.
        // Ceres also disables the HUD map, but has no matching exploration list.
        if (enemies.Ceres.RidleyPresent)
        {
            _hud.DisableMinimapForBoss();
            // Only Norfair's Ridley marks his arena explored.
            if (enemies.RoomStatus.Boss == AreaBoss.Ridley)
                for (int row = 0; row < RidleyMapDefinitions.ArenaScreenRows; row++)
                    _system.MarkExploredMapTile(room.AreaIndex, room.MapPosition + new Offset16<MapTiles>(0, row + 1));
        }
        _escapeEffects.SetupEscapeRoomEffects(room.State.SetupBehavior, enemies, scope.EscapeDiagonalQuake);
        // Gate setup runs while the room-object population is constructed, but Enemies.Load
        // subsequently clears the shared projectile pool. Consume those setup
        // requests here—the first point matching the game's completed room teardown—
        // so the closed actor survives into the initial viewport.
        _enemyRoomEffects.ApplyPendingDownwardGateProjectileRequests(roomObjects, enemies);
        // The Tourian escape setup spawns into the pool that Enemies.Load has just cleared.
        if (room.State.SetupBehavior == RoomSetupBehavior.SpawnPrePhantoonRoomEnemyProjectile)
            enemies.LoaderHooks.SpawnPrePhantoonRoomProjectile();
        _enemyRoomEffects.ApplyEnemyRequestedRoomEffects(enemies.LastEnemyFrame.RoomEffects, roomObjects, roomFxOverlay, roomColourEffects, enemies);
        // The gunship's placement starts its lights' glow after the room's own colour effects (#1733).
        _enemyRoomEffects.StartEnemyColourEffect(enemies.LastEnemyFrame.ColourEffect, roomColourEffects);
        scope.TourianStatues = new TourianStatueSequence(_tourianStatueDependencies, roomFxOverlay, roomColourEffects, roomObjects, enemies);
        // The door-transition code that owns the door's camera targets keeps them over the destination once its enemies are loaded.
        enemies.ResumeDoorTransitionGates(carryOver.DoorGates);

        if (viewportLoadMode == RoomViewportLoadMode.StreamThroughDoor)
        {
            // The door transition never redraws the destination's whole viewport: the source screen stays
            // visible while the opening scroll moves the destination in around it.
            _pendingDoorOpeningSourceView = doorOpeningSourceView;
            _room.Adopt(scope.Complete());
            return new InitialViewportResult();
        }

        _room.Adopt(scope.Complete());
        _pendingDoorOpeningSourceView = null;
        _backgroundScroll.ForegroundPosition = destinationCamera.PixelPosition;
        _backgroundScroll.CalculateScrolls();
        return new InitialViewportResult();
    }

    /// <summary>
    /// Resolves a compiled room selector against the live saved progress and Samus inventory.
    /// </summary>
    /// <remarks>
    /// The fixed room definition still supplies the area needed to choose that
    /// area's boss flags. The ordered state choice is application-owned definition data;
    /// no production path executes selector data. Keeping this at the shared loader
    /// boundary prevents doors, save stations, and attract scenes from choosing differently.
    /// </remarks>
    internal SelectedRoom LoadSelectedRoom(RoomDefinition room)
    {
        AreaId areaIndex = Ensure.NotNull(room).AreaIndex;

        // RoomStateSelectionContext owns a detached copy of the events, so selection never
        // observes or changes the live progress.
        SamusState? samus = _player.Samus;
        var selection = new RoomStateSelectionContext(
            _system.Progress.Events.Copy(),
            DefeatedBosses: _system.BossesDefeatedIn(areaIndex),
            HasMorphBallAndMissiles:
                samus is not null &&
                samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall) &&
                samus.MaxMissiles != 0,
            HasPowerBombs: samus?.MaxPowerBombs != 0);
        return SelectedRoom.LoadUsingCompiledSelection(room, selection);
    }

    /// <summary>
    /// Replays the position arithmetic and the door's coordinate rebase
    /// to obtain the final camera/Samus coordinates after the scrolling phase.
    /// </summary>
    private static DoorTransitionPlacement CalculateDoorTransitionPlacement(
        DoorDefinition door,
        FixedPosition<RoomPixels> source)
    {
        DoorDirection direction = door.Orientation.Direction;

        // Setup's first directional call (none for down), then placing Samus for the destination's loading replaces
        // both whole positions, then every remaining scroll update moves along the travel axis.
        FixedPosition<RoomPixels> samus = DoorOpeningScrollState.RebaseSamus(
            door, DoorOpeningScrollState.ApplySetupMovement(door, source));
        FixedVector<RoomPixels> step = DoorOpeningScrollState.GetSamusStep(door);
        for (int update = DoorOpeningScrollState.MovingScrollUpdates(direction); update > 0; update--)
            samus = samus.Advance(step);

        int finalX = NudgeOffScreenRightEdge(NudgeOffScreenLeftEdge(samus.X.Whole));

        // The room load applies this eight-pixel doorway alignment only horizontally.
        if (!direction.IsVertical())
            finalX = direction == DoorDirection.Right ? CoordinateAxis.Wrap(finalX | 7) : CoordinateAxis.Wrap(finalX & ~7);
        samus = samus.WithWhole(new Position16<RoomPixels>(finalX, NudgeOffScreenLeftEdge(samus.Y.Whole)));
        // The door scroll adds 32 to the destination Y after using the unmodified value
        // for Samus's setup origin. The scroll's completion later snaps an upward transition's
        // camera to that adjusted destination; the other three directions retain theirs.
        Position16<RoomPixels> destination = DoorOpeningScrollState.GetDestination(door);
        Position16<RoomPixels> finalCamera = direction == DoorDirection.Up
            ? destination + new Offset16<RoomPixels>(0, 32)
            : destination;
        return new DoorTransitionPlacement(finalCamera, samus);
    }

    /// <summary>
    /// Applies the Ceres-start Samus pose/state after room loading.
    /// </summary>
    /// <remarks>
    /// The behaviour intentionally replaces the load-station Y coordinate with zero;
    /// elevator platform/projectile motion later carries front-facing Samus into the room.
    /// That elevator actor is a separate pending subsystem, so this method does not invent a
    /// floor placement or prematurely unlock controls.
    /// </remarks>
    public void InitializeCeresStartSamus()
    {
        if (_room.ActiveRoom is null || _room.ActiveLoadStation is null)
            throw new InvalidOperationException("The starting Ceres room must be loaded before Samus setup.");

        _player.Samus = new SamusState(_samusOverreads.AnimationContinuation, _unauthoredBoostCadence, SamusCompatibility.ForNewSamus(), _beamCharge)
        {
            Pose = SamusPoseId.ForwardFacingPowerSuitPose,
            AnimationFrame = 0,
            Position = FixedPosition<RoomPixels>.FromWhole(_room.ActiveLoadStation.Placement.SamusPosition.WithY(0)),
        };
        _player.Samus.BindAudioRequests(_audioRequests);
        _artwork.BindSamusPalettePresentation();
        SuperMetroid.Core.Samus.Palettes.SamusNormalSuitPalette.LoadPower(_player.Colours);
        _assists.GrantLoadoutToNewSamus();

        SamusPoseMetadata.RefreshCollisionRadii(_player.Samus);
        SamusPresentation.InitializeAnimation(_player.Samus);
        _player.Samus.LiquidPhysics.RoomIdentity = _room.ActiveRoom.Identity;
        _player.PreviousMovementTypeForXray = SamusPoseMetadata.ReadMovementType(_player.Samus);

        // The Ceres elevator projectiles are spawned with enemy graphics and thus briefly seed both objects
        // from enemy slot zero. Their shared initializer, however, runs
        // before the first draw and explicitly clears that graphics index. The specialized
        // arrival model owns that post-initialization value; passing the transient
        // enemy graphics index here previously recolored the level-data concealer teal.
        _room.LoadedScope.BeginCeresElevatorArrival(new CeresElevatorArrivalState(_player.Samus));

        // The locked Ceres-start frame handler still publishes the initial minimap. The
        // ordinary update below used to be gated on GroundedSamusMovementEnabled, leaving
        // the HUD template's unrelated map glyphs visible for the entire descent. Seed
        // the real area-six room coordinate now; subsequent normal frames keep updating it.
        ActiveRoomGeometry roomGeometry = GetActiveRoomGeometry();
        _hud.UpdateMinimap(_system, new MinimapFrame { AreaIndex = roomGeometry.AreaIndex, RoomMap = roomGeometry.MapPosition, RoomSize = _room.LevelData?.Size ?? _room.ActiveRoom.Size.ToRoomBlocks(), Samus = _player.Samus.Position.Whole, FrameCounter = _clock.ShortCounter, MapRevealMode = _assists.Current.MapReveal, PresentationMap = _artwork.MapPresentation?.Get(roomGeometry.AreaIndex) });

        // Samus's Ceres-start control setup installs a locked frame handler. Reuse the
        // existing runtime switch to ensure ordinary movement cannot begin before the
        // elevator arrival sequence has been modelled.
        _player.GroundedSamusMovementEnabled = false;
    }

    /// <summary>Returns room geometry for either supported room path.</summary>
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

        throw new InvalidOperationException("No room is active.");
    }

    /// <summary>
    /// The pictures the outgoing room's foreground blocks show as a door begins (#1392), which its screen keeps through the
    /// door's opening scroll.
    /// </summary>
    private RoomBlockPictureTable SourceScreenPictures()
    {
        RoomScope scope = _room.Scope ?? throw new InvalidOperationException("A door began with no room loaded.");
        // The elevator shaft's screen is its turning floor, which the door's scroll keeps showing: none of its blocks (#1647).
        if (RoomTurningFloors.ShowsShaftFloor(scope.Room.State.MainBehavior))
            return RoomBlockPictureTable.NoneDrawn();
        return scope.BlockPictures.Resolve(scope.LevelData.ForegroundPlane, null, RoomScope.BlockArtworkOf(_room.ActiveRoom),
            new RoomAppearanceView(scope.Appearance, scope.ArtworkFrames, _enemySession.Looks), []);
    }

    /// <summary>The host-bound room catalogs; room loading never substitutes artwork.</summary>
    private RoomAssetCatalogs RequireRoomAssetCatalogs() =>
        _artwork.RoomVisualLayouts is { } layouts &&
        _artwork.RoomLevelStreams is { } levelStreams && _artwork.RoomBlockSheets is { } blockSheets
            ? new RoomAssetCatalogs(layouts, levelStreams, blockSheets)
            : throw new InvalidOperationException(
                "Room loading requires installed visual-layout, level-stream and block-sheet assets.");


    /// <summary>
    /// The doorway nudge the door transition and room load apply: a room coordinate in its screen's second block moves
    /// to eight pixels past that block; any other stays.
    /// </summary>
    private static int NudgeOffScreenLeftEdge(int pixel) =>
        RoomGridConversions.BlockRowWithinScreen(pixel) == 1 ? CoordinateAxis.Wrap(RoomGridConversions.BlockEnd(pixel) + 8) : pixel;

    /// <summary>
    /// The doorway nudge's other edge: a room coordinate in its screen's fifteenth block moves to eight pixels before it;
    /// any other stays.
    /// </summary>
    private static int NudgeOffScreenRightEdge(int pixel) =>
        RoomGridConversions.BlockRowWithinScreen(pixel) == 14 ? CoordinateAxis.Wrap(RoomGridConversions.BlockStart(pixel) - 8) : pixel;
}
