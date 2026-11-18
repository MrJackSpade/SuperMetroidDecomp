using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Bull;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.Gunship;
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
/// Room loading: saved games and new sessions, rooms and doors, room setup, colours, the HUD minimap,
/// enemies and room objects of the loaded room.
/// </summary>
[RoomScopeBoundary("Builds each room's scope in the game's load order.")]
internal sealed class RoomLoader
{
    /// <summary>Supplies decoded graphics and colour assets for the room being entered.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Tracks both background layers' scroll positions across door loading.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Retains controller state needed when a room transition begins.</summary>
    private readonly GameplayActionInput _playerActions;
    /// <summary>Publishes the display setup for a newly active room.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Coordinates the door loader's transition steps.</summary>
    private readonly DoorTransitionRunner _doorTransition;
    /// <summary>The enemy state and installed enemy artwork that outlive each room; each room's enemies are built over it.</summary>
    private readonly EnemySession _enemySession;

    /// <summary>The session's installed boss artwork, which each room's bosses read.</summary>
    private readonly BossArtworkBinding _bossArtwork;

    /// <summary>Applies room-scoped visual effects driven by enemies.</summary>
    private readonly EnemyRoomEffects _enemyRoomEffects;
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
    /// <summary>The installed pictures of the room objects' blocks, which each room's room-object system draws with.</summary>
    private readonly RoomObjectVisuals _roomObjectVisuals;
    /// <summary>Resets Samus projectiles for the destination room.</summary>
    private readonly BeamChargeState _beamCharge;
    /// <summary>The runtime's audio requests, which every room system and every Samus built here queue theirs into.</summary>
    private readonly GameplayAudioRequests _audioRequests;
    /// <summary>The installed services reproducing Samus's glitch reads, handed to every Samus and projectile system built here.</summary>
    private readonly SamusOverreadServices _samusOverreads;
    /// <summary>The installed cells each explicit scroll layout's load writes past the room's screens.</summary>
    private readonly ScrollCellsPastRoomScreens<RoomScrollState> _scrollCellsPastRoomScreens;
    /// <summary>
    /// The session's unauthored Speed Booster cadence, handed to every Samus built here and to every room's enemies, whose
    /// Ceres Ridley getaway leaves its delays behind.
    /// </summary>
    private readonly UnauthoredBoostCadence _unauthoredBoostCadence;
    /// <summary>
    /// The camera fractions the previous game left, which the runtime's first room's camera starts from
    /// (<see cref="CameraFractionsKeptAcrossGames"/>); later rooms carry the outgoing camera's own.
    /// </summary>
    private readonly CameraFractionsKeptAcrossGames _cameraFractions;
    /// <summary>
    /// The runtime's shared fade step count, handed to every room's enemies, whose backdrop and body fades count their steps
    /// on it with the runtime's door, death and time-up fades.
    /// </summary>
    private readonly SharedFadeStepCount _fadeStepCount;
    /// <summary>The runtime's camera measure a kicked Yard flies off with, handed to every room's enemies.</summary>
    private readonly YardKickTrackingDistance _yardKickDistance;
    /// <summary>Holds the active room, camera, selected door, and load-station identity.</summary>
    private readonly ActiveRoomState _room;
    /// <summary>Whether the colour effects run, and the heat phases, which each room's colour effects read.</summary>
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
        _playerActions = dependencies.PlayerActions;
        _display = dependencies.Display;
        _doorTransition = dependencies.DoorTransition;
        _enemySession = dependencies.EnemySession;
        _bossArtwork = dependencies.BossArtwork;
        _enemyRoomEffects = dependencies.EnemyRoomEffects;
        _gameTime = dependencies.GameTime;
        _hud = dependencies.Hud;

        _clock = dependencies.Clock;
        _doorFrameCounting = Ensure.NotNull(dependencies.DoorFrameCounting);
        _options = dependencies.Options;
        _player = dependencies.Player;
        _roomObjectVisuals = dependencies.RoomObjectVisuals;
        _beamCharge = dependencies.BeamCharge;
        _audioRequests = Ensure.NotNull(dependencies.AudioRequests);
        _samusOverreads = Ensure.NotNull(dependencies.SamusOverreads);
        _scrollCellsPastRoomScreens = Ensure.NotNull(dependencies.ScrollCellsPastRoomScreens);
        _unauthoredBoostCadence = Ensure.NotNull(dependencies.UnauthoredBoostCadence);
        _cameraFractions = Ensure.NotNull(dependencies.CameraFractions);
        _fadeStepCount = Ensure.NotNull(dependencies.FadeStepCount);
        _yardKickDistance = Ensure.NotNull(dependencies.YardKickDistance);
        _room = dependencies.Room;
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
    /// Executes the data-loading portion of a new game's setup: Ceres, its elevator
    /// checkpoint, the checkpoint-selected room, and its fresh-save state-selection branch.
    /// </summary>
    public void InitializeStartingCeresRoom()
    {
        // A new game loads at the Ceres start; the station names the door into its room and the placement.
        LoadStation station = LoadStationDefinitions.CeresElevator;
        DoorDefinition door = station.Placement.Door;
        // A new game loads Ceres before it sets Samus up (InitializeCeresStartSamus): the room is chosen and loads with
        // Samus as initialization leaves her, unless a restored Ceres-elevator save has already set her up, as the game
        // holds her state through every load. Ceres's room choice reads none of her equipment, so a new game's Ceres is
        // the one an uninitialized Samus would choose.
        _player.Samus ??= InitializeSamus();
        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);
        if (room.Area != AreaId.Ceres)
        {
            throw new InvalidDataException(
                $"Fresh Ceres station targets {room.Area}, expected {AreaId.Ceres}.");
        }

        LoadRoom(
            door,
            room,
            station.Placement.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { LoadStation = station });
    }

    /// <summary>
    /// Restores an existing save slot at its area's load station and
    /// enters the normal playable state at the station-authored Samus coordinates.
    /// </summary>
    /// <remarks>
    /// The room loads with the save's progress, inventory and position, and Samus starts her front-facing appearance
    /// with her suit's glow and her control locked; the appearance gives her control back when it ends.
    /// </remarks>
    public void InitializeSavedGame(SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(slot);
        LoadStation station = slot.Checkpoint;
        AreaId requestedArea = station.Area;
        DoorDefinition door = station.Placement.Door;

        // Apply progression before room construction. Enemy and room-object initialization query
        // boss/event/item flags synchronously, while room-state selection and beam graphics
        // depend on those same flags. In particular, loading a post-Morph-Ball save must not
        // accidentally construct the untouched new-game version of Blue Brinstar.
        RestoreSavedPlayerState(slot);
        _player.IntroducedSamus.Position = _player.IntroducedSamus.Position.WithWholeX(station.Placement.SamusPosition.X);
        _player.IntroducedSamus.Position = _player.IntroducedSamus.Position.WithWholeY(station.Placement.SamusPosition.Y);
        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);
        if (room.Area != requestedArea)
        {
            throw new InvalidDataException(
                $"Load station {station} targets room area " +
                $"{room.Area}.");
        }
        LoadRoom(
            door,
            room,
            station.Placement.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { LoadStation = station });
        // Loading from a load station sets the save-station lockout. Without this room-entry lockout, the
        // first standing floor probe after loading directly on a save pod immediately
        // reopens its confirmation message.
        _room.LoadedScope.RoomObjects.Stations.LockSaveStationForCurrentRoomEntry();

        // The Zebes start setup does not begin at its endpoint. It shows Samus facing forward
        // in her suit's colours at the saved game's appearance (<see cref="SavedGameAppearanceDefinitions"/>)
        // before restoring ordinary input.
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(_player.Colours, _player.IntroducedSamus.EquippedItems);
        SamusPoseMetadata.ApplyZebesStartPoseSetup(_player.IntroducedSamus);
        SamusPresentation.SetAnimationFrameFromMovementOwner(_player.IntroducedSamus, SamusAnimationFrames.ZebesAppearance,
            timer: SavedGameAppearanceDefinitions.FirstFrameTimer);
        _player.IntroducedSamus.Ground = SamusGround.Of(room.Definition);
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
            _system.HasDefeatedAnyOf(room.Area, DefeatedBosses.AreaMiniBoss));
        _player.IntroducedSamus.Control.LockOutPlayer();
        _player.PreviousMovementTypeForXray = SamusPoseMetadata.ReadMovementType(_player.IntroducedSamus);
        _player.GroundedSamusMovementEnabled = false;
    }

    /// <summary>
    /// Initialises a new Samus, as the game does when it loads a saved game, starts at Ceres, arrives on Zebes after
    /// Ceres or loads an attract demo: a fresh Samus in her default state, and the camera's horizontal measure of her
    /// movement set to one pixel (<see cref="YardKickTrackingDistance.RecordSamusInitialization"/>) until the camera
    /// first tracks her. Her pose, place and inventory are set separately.
    /// </summary>
    internal SamusState InitializeSamus()
    {
        _yardKickDistance.RecordSamusInitialization();
        return new SamusState(_audioRequests, _samusOverreads.FramesBeyondAnimation, _unauthoredBoostCadence, SamusCompatibility.ForNewSamus(), _beamCharge,
            _artwork.SamusArtwork);
    }

    /// <summary>
    /// Restores the player and persistent mirror before selecting rooms or a saved
    /// cinematic. A Ceres-destruction checkpoint has no playable Ceres room to load;
    /// its inventory must nevertheless survive into the Zebes landing sequence.
    /// </summary>
    internal void RestoreSavedPlayerState(SuperMetroidSaveSlot slot)
    {
        Ensure.NotNull(slot);
        SamusState samus = InitializeSamus();
        samus.Pose = SamusPoseId.FacingRightNormalPose;
        samus.AnimationFrame = SamusAnimationDelays.AnimationForPose(samus.Pose).Start;
        _player.Samus = samus;
        slot.ApplyTo(_player.Samus, _system);
        // Loading the save sets the stored explored maps from it; the live map is loaded only at file select.
        StoredExploredMapLoads.LoadFile(_system.StoredExploredMaps, slot.Progression);
        _assists.ResetSavedEncounters();
        _assists.GrantLoadoutToNewSamus(samus);
        // Loading a save selects the beam again, whatever the save held, so the front-facing load pose shows no
        // weapon's arm cannon.
        _player.Samus.SelectBeam();
        // The save holds the player's controls and options; loading it restores them.
        _options.ControllerBindings = slot.ControllerBindings;
        _options.MoonwalkEnabled = slot.MoonwalkEnabled;
        _options.IconCancelEnabled = slot.IconCancelEnabled;
        _gameTime.Load(slot.GameTime);
    }

    /// <summary>
    /// Loads the game's arrival on Zebes after Ceres:
    /// Crateria, the Zebes arrival, with the airborne Landing Site gunship.
    /// </summary>
    public void InitializePostCeresZebesRoom()
    {
        SamusState carried = _player.Samus
            ?? throw new InvalidOperationException(
                "The Ceres escape must retain its live Samus state through the cinematic.");

        // Initializing Samus clears all of her state, including the speeds and pose
        // state the escape left behind; only the inventory below it survives. The player is
        // the same one, so a loadout already granted to her is not granted again.
        // Initialization faces Samus forward in the power suit; the model's constructor defaults to the right-facing pose.
        SamusState samus = InitializeSamus();
        samus.Pose = SamusPoseId.ForwardFacingPowerSuitPose;
        samus.AnimationFrame = SamusAnimationDelays.AnimationForPose(samus.Pose).Start;
        samus.CopyInventoryFrom(carried);
        _player.Samus = samus;
        _assists.CarryLoadoutGrant(carried, samus);

        // The arrival on Zebes is a cutscene-only entry high above Landing Site; the gunship is
        // not the checkpoint until the gunship's landing finishes and performs the save.
        LoadStation station = LoadStationDefinitions.ZebesArrival;
        DoorDefinition door = station.Placement.Door;
        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);
        if (room.Area != AreaId.Crateria)
        {
            throw new InvalidDataException(
                $"Post-Ceres station {station} targets area {room.Area}, expected Crateria.");
        }

        // The cinematic refilled health on the preceding frame; the Ceres start then seeds the
        // hurt check from it. The Ceres start's control setup locks the player out.
        samus.Health = samus.MaxHealth;
        samus.PreviousHealthForHurtCheck = samus.Health;
        samus.Kinematics.Position = samus.Kinematics.Position.WithX(new FixedCoordinate(station.Placement.SamusPosition.X, 0)).WithY(new FixedCoordinate(station.Placement.SamusPosition.Y, 0));
        samus.Control.LockOutPlayer();

        // The arrival moves the game to Crateria and loads the live explored map from
        // Crateria's stored one before the room loads.
        _system.StoredExploredMaps.LoadLiveMap(AreaId.Crateria);

        // Arriving on Zebes ends the Ceres room's objects and projectiles before Landing Site loads, the two of the Ceres
        // elevator's arrival among them, so the gunship alone carries Samus.
        LoadRoom(
            door,
            room,
            station.Placement.CameraPosition,
            RoomViewportLoadMode.DisplayInitialViewport,
            new RoomEntry { GunshipScenario = GunshipLoadScenario.EscapingCeres, LoadStation = station });
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(_player.Colours, samus.EquippedItems);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Ground = SamusGround.Of(room.Definition);
        _player.PreviousMovementTypeForXray = SamusPoseMetadata.ReadMovementType(samus);
        _player.GroundedSamusMovementEnabled = false;
    }

    /// <summary>
    /// True after a door block's reaction has selected a normal destination door and
    /// before the frontend's door-transition states consume it.
    /// </summary>
    public bool HasPendingDoorTransition => _room.LevelData?.PendingDoorTransition is not null;

    /// <summary>The unconsumed door selected by a door block's collision reaction.</summary>
    public DoorDefinition? PendingDoorTransition => _room.LevelData?.PendingDoorTransition;

    /// <summary>
    /// Takes one update of the source room's alignment at the start of a door transition. Horizontal doors move the
    /// camera's Y toward the nearest screen boundary; vertical doors move X, one pixel per update: a camera past the middle
    /// of its screen moves on to the next screen's edge and one before it back to its own, so one pixel either side of
    /// the boundary reaches it in one step.
    /// </summary>
    /// <param name="door">The door the transition goes through.</param>
    /// <returns>True only when the camera already lay on that screen boundary on entry.</returns>
    public bool AlignDoorCameraOnePixel(DoorDefinition door)
    {
        Ensure.NotNull(door);
        ScrollBoundaryCamera camera = _room.LoadedCamera;

        DoorCameraAlignmentState alignment = DoorCameraAlignmentState.Step(
            door.Orientation.Direction,
            camera.PixelPosition);
        if (alignment.Completed)
            return true;

        // The alignment increments or decrements the whole camera position only; fractions, the ideal
        // camera position and Samus's previous position are untouched.
        camera.SetDoorTransitionPosition(alignment.CameraPosition);

        // The game updates the background's position and scroll on every
        // convergence step; the gameplay scroll owner performs the same parallax math.
        _backgroundScroll.ForegroundPosition = camera.PixelPosition;
        _backgroundScroll.StepScrolling(timeIsFrozen: false);
        return false;
    }

    /// <summary>
    /// The axis the door alignment aligns, perpendicular to travel: right/left converge camera Y,
    /// while down/up converge camera X. This matters because aligning the
    /// travel axis instead moves a horizontal door frame by a row while its room
    /// scroll is being placed.
    /// </summary>
    internal static bool DoorTransitionAlignsX(DoorDirection direction) => direction.IsVertical();

    /// <summary>Whether <paramref name="door"/>'s destination room definition selects the isolated door transition.</summary>
    /// <param name="door">The door the transition goes through.</param>
    public bool DestinationUsesIsolatedDoorTransition(DoorDefinition door) =>
        LoadSelectedRoom(Ensure.NotNull(door).DestinationRoom).UsesIsolatedDoorTransition;

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
    /// <param name="door">The door the transition goes through.</param>
    internal void ApplyDoorScrollingSetupMovement(DoorDefinition door)
    {
        Ensure.NotNull(door);
        SamusState samus = _player.IntroducedSamus;
        samus.Position = DoorOpeningScrollState.ApplySetupMovement(door, samus.Position);
        PublishDoorCameraPosition(DoorOpeningScrollState.GetSetupCamera(door));
    }


    /// <summary>Rebases Samus and publishes the scroll's first moving step during the destination's loading.</summary>
    /// <param name="door">The door the transition goes through.</param>
    internal void PlaceSamusForDestinationLoading(DoorDefinition door)
    {
        Ensure.NotNull(door);
        SamusState samus = _player.IntroducedSamus;
        ScrollBoundaryCamera camera = _room.LoadedCamera;
        FixedPosition<RoomPixels> position = DoorOpeningScrollState.RebaseSamus(door, samus.Position);
        camera.PublishDoorSamusPosition(position.Whole);
        samus.Position = DoorOpeningScrollState.AdvanceSamus(door, position);
        DoorDirection direction = door.Orientation.Direction;
        bool vertical = direction.IsVertical();
        PublishDoorCameraPosition(DoorOpeningScrollState.GetSetupCamera(door) + DoorOpeningScrollState.LayerStep<RoomPixels>(direction));
        camera.PublishDoorSamusTravelAxis(vertical, samus.Position.Whole);
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
        int completedLoadingScrollUpdates)
    {
        SamusState samus = _player.IntroducedSamus;
        ScrollBoundaryCamera camera = _room.LoadedCamera;
        if (_doorOpeningScroll is not null)
            throw new InvalidOperationException("A door-opening scroll is already active.");
        if (_room.LoadedScope.Enemies.Elevator.ConsumeElevatorArrivalPlacement() is { } elevator)
            _pendingLoaderSamusPlacement = new LoaderSamusPlacement(elevator, samus.Position.Whole);

        _doorOpeningScroll = DoorOpeningScrollState.Create(
            door,
            sourceSamus,
            camera.PixelPosition,
            _backgroundScroll.BackgroundPosition);
        DoorDirection direction = door.Orientation.Direction;
        DoorOpeningSourceView sourceScroll = _pendingDoorOpeningSourceView
            ?? throw new InvalidOperationException(
                "Door-opening scroll lost the source room's view.");
        // The offsets derive from where the foreground stands before setup's first update.
        Position16<RoomPixels> openingForeground = _doorOpeningScroll.CameraPosition +
            Ensure.Listed(DoorOpeningScrollDefinitions.OpeningForegroundOffsets, direction);
        _backgroundScroll.BeginDoorOpeningViews(openingForeground);
        _pendingDoorOpeningSourceView = null;
        // The source screen stays where it was on the screen, lying where the destination's view begins, while the
        // destination scrolls in around it.
        _room.DoorSourceBackground = RoomDoorBackground.Begin(sourceScroll.SourceBackground, sourceScroll.ForegroundScroll.AsPlaneScroll(),
            openingForeground.AsPlaneScroll(), direction == DoorDirection.Down);
        PublishDoorOpeningLayers(_doorOpeningScroll);
        // Each directional setup immediately takes its first step, which recomputes the
        // layer scrolls from the already-advanced layer positions. Down's first update
        // restores the scrolls it moved, so only the horizontal and upward setups leave
        // new scrolls behind.
        if (direction != DoorDirection.Down)
            _backgroundScroll.CalculateLayerScrolls();
        samus.Position = _doorOpeningScroll.SamusPosition;
        camera.PublishDoorSamusPosition(samus.Position.Whole);
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
        SamusState samus = _player.IntroducedSamus;
        ScrollBoundaryCamera camera = _room.LoadedCamera;
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
            if (loaderProgress.HasInitialized(placement.Elevator))
            {
                state.PlaceSamusOnElevator(placement.Position);
                _pendingLoaderSamusPlacement = null;
            }
            else if (completed)
            {
                throw new InvalidDataException(
                    $"The door scroll finished before the loader initialized the arriving elevator in place {placement.Elevator.Ordinal}.");
            }
        }
        _room.LoadedScope.Enemies.LoaderHooks.CompleteRinkaLoaderTimeCameraReads(loaderProgress.HasInitializedEnemyPlace, camera.PixelPosition);
        if (completed && _room.LoadedScope.Enemies.LoaderHooks.HasDeferredRinkaLoaderTimeCameraReads)
            throw new InvalidDataException("The door scroll finished before the loader initialized every special Rinka.");
        samus.Position = state.SamusPosition;
        camera.PublishDoorSamusTravelAxis(state.Direction.IsVertical(), samus.Position.Whole);
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
        if (state.RemainingFrames != 0)
            throw new InvalidOperationException("Door loading has not finished scrolling.");
        SamusState samus = _player.IntroducedSamus;
        switch (state.Direction)
        {
            case DoorDirection.Right:
                samus.Position = samus.Position.WithWholeX(RoomGridConversions.HalfBlockEnd(samus.Position.X.Whole));
                break;
            case DoorDirection.Left:
                samus.Position = samus.Position.WithWholeX(RoomGridConversions.HalfBlockStart(samus.Position.X.Whole));
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
        if (state.RemainingFrames != 0)
            throw new InvalidOperationException("Door-opening scroll has not reached its endpoint.");
        SamusState samus = _player.IntroducedSamus;
        if (_pendingLoaderSamusPlacement is not null)
            throw new InvalidOperationException("The door loader's Samus placement is still pending.");

        samus.Position = samus.Position.WithWhole(new(
            NudgeOffScreenRightEdge(NudgeOffScreenLeftEdge(samus.Position.X.Whole)),
            NudgeOffScreenLeftEdge(samus.Position.Y.Whole)));

        if (_room.LoadedScope.Enemies.Elevator.ElevatorFlags != ElevatorJourneyFlag.None)
        {
            if (_room.LoadedScope.Enemies.Elevator.ElevatorTravel == ElevatorTravelDirection.Down)
                SamusPoseMetadata.SetupForElevator(samus);
            else
                samus.Control.LockOutPlayer();
        }
        _doorOpeningScroll = null;
        _room.DoorSourceBackground = null;
    }

    /// <summary>
    /// Loads the door's destination while the source room stays on screen; the following
    /// scroll updates slide the destination in.
    /// </summary>
    internal void LoadPendingDoorDestinationForTransition(FixedPosition<RoomPixels> sourceSamus) =>
        LoadPendingDoorDestination(RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn, sourceSamus);

    /// <summary>
    /// Builds the selected door's destination, keeping Samus's place in the source room, which the door's opening
    /// scroll starts from when the destination scrolls in rather than appearing whole.
    /// </summary>
    private void LoadPendingDoorDestination(
        RoomViewportLoadMode viewportLoadMode, FixedPosition<RoomPixels>? sourcePosition = null)
    {
        SamusState samus = _player.IntroducedSamus;

        RoomLevelData sourceLevel = _room.LoadedLevelData;
        DoorDefinition door = sourceLevel.PendingDoorTransition
            ?? throw new InvalidOperationException("No door block's collision has chosen a door to go through.");
        DoorTransitionPlacement placement = CalculateDoorTransitionPlacement(door, sourcePosition ?? samus.Position);
        SelectedRoom room = LoadSelectedRoom(door.DestinationRoom);

        // The transition's teardown deletes direct G-Mode's stranded X-ray beam,
        // but it does not restore the four suspended subsystems. Destination room setup
        // therefore begins in indirect G-Mode and cannot create a visible gate actor.
        samus.Xray.TransitionDirectGModeToIndirect();

        // The destination's assets load before the source room's pending door is consumed, so a destination that
        // fails to load leaves the door still pending.
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
            _system.StoredExploredMaps.StoreLiveMap(_room.LoadedRoom.Area);
            _system.StoredExploredMaps.LoadLiveMap(room.Area);
        }

        // An elevator departing the source room arrives in the destination: the door transition turns its departure
        // into an arrival after the destination's room objects and door setup exist. The room load sets up enemies in
        // the same update, so the arrival is handed to the destination's elevator just before they load: it places
        // itself at its arrival height instead of ending the journey.
        bool arrivingByElevator = _room.LoadedScope.Enemies.Elevator.ElevatorStatus == ElevatorActorStatus.Departing;
        if (arrivingByElevator)
            _room.LoadedScope.Enemies.Elevator.PrepareElevatorArrival();

        // The load-station record established only the first room. Once the door collision publishes
        // a door definition, that definition and its destination definition become authoritative:
        // the destination's scope is built without a load station.

        // The door's scroll has already placed Samus before the destination room is
        // constructed. This ordering is observable for elevator arrivals:
        // The elevator's initialization sees the arrival, moves the platform to its arrival height, and then pins
        // Samus to it, so the door's placement comes first and never moves her off the platform.
        samus.Position = placement.SamusPosition;
        // The door's scroll moves Samus at its own speed. Ordinary jump velocity survives
        // room loading and the final nudge; clearing it here restarts a fall
        // at its apex and lets horizontal momentum carry Samus too far after exit.
        samus.Kinematics.ExtraDisplacement = FixedVector<RoomPixels>.Zero;

        LoadRoom(
            door,
            room,
            placement.CameraPosition,
            viewportLoadMode,
            new RoomEntry { RunDoorClosingObject = true, PreloadedAssets = assets });

        // The door transition loads Samus's suit target colours after room/enemy colours have been
        // loaded and before the door fade captures the destination fade target. The source fade
        // has already driven Samus's colours to black at this point, so without this reload she would stay a black
        // silhouette after every ordinary door transition.
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.Show(_player.Colours, samus.EquippedItems);

        // The door loader sets no Samus radius; the next unlocked input mode's radius update
        // does, so an elevator rider keeps the boarding pose's radius through the door.
        samus.Ground = SamusGround.Of(room.Definition);
        _player.GroundedSamusMovementEnabled = true;
    }

    /// <summary>Loads the selected room, its state and its graphics; shared by stations, doors and the attract demo.</summary>
    internal void LoadRoom(
        DoorDefinition door,
        SelectedRoom room,
        Position16<RoomPixels> camera,
        RoomViewportLoadMode viewportLoadMode,
        RoomEntry entry = default)
    {
        DoorOpeningSourceView? doorOpeningSourceView = viewportLoadMode switch
        {
            RoomViewportLoadMode.DisplayInitialViewport => null,
            RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn => new DoorOpeningSourceView(_backgroundScroll.ForegroundScroll,
                new RoomBackgroundLayer(_room.LoadedLevelData.ForegroundPlane.Cells, SourceScreenPictures())),
            _ => throw new ArgumentOutOfRangeException(
                nameof(viewportLoadMode), viewportLoadMode, "Unknown room viewport load mode."),
        };
        LoadedRoomAssets assets = entry.PreloadedAssets ??
            LoadedRoomAssets.Load(_scrollCells, room, RequireRoomAssetCatalogs());

        // The destination reads the outgoing room only through this capture; from here on the room views show the
        // destination's parts as each step builds them.
        var carryOver = new RoomTransitionCarryOver(_room.Camera);
        // The outgoing room's shots and enemy projectiles leave what the next room's start from (RoomHandOver).
        if (_room.Scope is { } outgoing)
        {
            outgoing.Projectiles.HandOverTo(_room.HandOver);
            outgoing.Enemies.HandOverTo(_room.HandOver);
        }
        RoomScopeConstruction scope = _room.BeginConstruction(door, room, assets, entry.LoadStation);
        // The room's block animations: those the door-selected effect lists, and the treadmill a door's setup may start.
        var blockAnimations = new RoomBlockAnimations(room.State.Fx, door);
        scope.BlockAnimations = blockAnimations;
        // The room's pictures carry its tileset's colours (#1512), so the load colours nothing.
        // The elevator shaft and Ceres Ridley's arena show their tileset's turning floor, which the shaft's door and Ridley
        // then draw over (#1349, #1467); every other room shows none (#1649). Ridley's getaway's main shows its frames.
        scope.Floor = assets.CreateFloor();
        // The door's room event composes what the room shows from its first frame (#2485): Ceres's elevator door shows the
        // shaft level about its centre and starts the shaft's turn there before the fresh room becomes visible. Publishing
        // the immutable transform here gives Samus, her projectiles, and the rotating elevator's steam a single
        // authoritative producer. Every other door shows the ordinary display, so stale rotating-view math cannot leak
        // across rooms. The displayed copy starts the same; subsequent room-main changes reach it only when the frame's
        // display is published, like every other floor change.
        door.Setup.RoomEvent.Compose(scope);
        _display.ActiveTurningFloor = scope.ShownTurningFloor;
        _display.PublishDoorElevatorFloor(scope.ShownTurningFloor);
        scope.LevelData = assets.LevelData;
        ScrollBoundaryCamera? previousCamera = carryOver.PreviousCamera;
        var destinationCamera = new ScrollBoundaryCamera(assets.Scrolls);
        scope.Camera = destinationCamera;
        if (viewportLoadMode == RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn)
        {
            if (previousCamera is not null) destinationCamera.InheritTrackingState(previousCamera);
            destinationCamera.SetDoorTransitionPosition(camera, previousCamera?.Position ?? _cameraFractions.Fractions);
        }
        else
        {
            destinationCamera.SetPosition(camera);
            if (previousCamera is not null)
                destinationCamera.RetainSubpositions(previousCamera);
            else
                destinationCamera.RetainSubpositions(_cameraFractions.Fractions);
        }
        _backgroundScroll.BackgroundScrollModes = room.State.BackgroundScrollModes;
        if (viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport)
            _backgroundScroll.CalculateScrolls();
        else
        {
            // Up calculates background from the destination plus 31 before changing the scroll's camera
            // endpoint to the destination plus 32. Keeping those adjacent positions distinct prevents
            // a parallax room from accumulating a one-pixel layer mismatch at the snap.
            Position16<RoomPixels> parallaxBasis = door.Orientation.Direction == DoorDirection.Up
                ? camera + DoorOpeningScrollDefinitions.UpwardParallaxBasis
                : camera;
            _backgroundScroll.PrepareDoorOpeningDestination(camera, parallaxBasis);
        }

        // Loading the room's effect chooses the entering door's own effect where it has one, applies its colour blend,
        // chooses the picture its overlay shows, and creates its distortion and block animation state before colour
        // effects and enemies begin running.
        var roomFxOverlay = RoomFxOverlayState.ForRoom(
            room.State.Fx,
            door,
            _system.RandomNumber,
            room.Definition);
        roomFxOverlay.BindAudioRequests(_audioRequests);
        scope.FxOverlay = roomFxOverlay;
        roomFxOverlay.PrimeViewport(destinationCamera.PixelPosition);
        // The room's setup composes its own parts, the sky's bands, the Ceres haze or the statues' room, which only the
        // rooms whose setups spawn them have (#2287, #2485). The room then composes the main its state names, with the
        // state that main keeps: the elevatube's ride, which the door's setup below starts, the elevator shaft's main over
        // the turn its door started, a sky, debris or a diagonal quake (#2287).
        room.State.SetupBehavior.Compose(new RoomSetupComposition
        {
            Scope = scope,
            System = _system,
            FxOverlay = roomFxOverlay,
            DisplaysInitialViewport = viewportLoadMode == RoomViewportLoadMode.DisplayInitialViewport,
        });
        room.State.MainBehavior.Compose(scope);
        // Every room loads with Samus: a new game's Ceres start has initialized her before its first room loads.
        SamusState loadingSamus = _player.IntroducedSamus;
        loadingSamus.LiquidPhysics.ConfigureFor(roomFxOverlay);

        // Loading reads the room's effect after room setup and before enemies. The chosen effect names two independent
        // lists, its overlay's effects and its colour effects. The colour effects must be created on every shared
        // room-load path; Landing Site's lightning replaces the colours the scrolling sky's horizon shows on the first
        // gameplay update.
        var roomColourEffects = ColourEffectSystem.ForRoom(
            _colourEffectSession,
            room.State.Fx,
            door,
            loadingSamus.EquippedItems,
            _system.HasDefeatedAnyOf(room.Area, DefeatedBosses.AreaMiniBoss));
        roomColourEffects.BindAudioRequests(_audioRequests);
        scope.ColourEffects = roomColourEffects;


        // Loading destroys room-owned objects before constructing the destination: the
        // destination's projectiles and bombs are built new here, before Load/beam graphics, so no
        // stale index reaches into them; its room-object system is built with the population below.
        // The held beam charge survives on the session; Samus's own echo storage is cleared.
        loadingSamus.HorizontalSpeed.ClearSpeedEchoes();
        loadingSamus.Shinespark.ResetProjectileEchoStorage();
        var bombs = new SamusBombProjectileSystem(_audioRequests, _artwork.SamusArtwork);
        // Each room's shot places start keeping nothing; TakeOverFrom then hands them what the outgoing room's kept.
        // The room's earthquake is composed with the room: Super Missile impacts and the room's enemies start it.
        var earthquake = new RoomEarthquake();
        var projectiles = new SamusProjectileSystem(_audioRequests, _beamCharge, _samusOverreads,
            new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount), new SamusShotCount(), _artwork.SamusArtwork, earthquake);
        projectiles.TakeOverFrom(_room.HandOver);
        scope.BombProjectiles = bombs;
        scope.Projectiles = projectiles;

        // The room-object loader walks the room's placements once. Each placement takes the first free place in update
        // order among the room's room objects before its setup runs, so a room object its setup deletes at once frees that place for the
        // next placement, and enemies can observe which room object holds which place. Every room-object family enters
        // through this one loader; an unsupported placement throws, naming it, instead of being skipped.
        // The room's room-object system is built with its bindings, then one set of runtime services
        // serves the room's room objects and enemies.
        var progress = new RoomAreaProgress(_system, room.Area);
        var roomObjectServices = new RoomObjectRuntimeServices(scope, _player, _system);
        var roomObjectRoom = new RoomObjectRoom(assets.LevelData, _system, room.Area);
        var roomObjectBindings = new RoomObjectServices
        {
            Samus = roomObjectServices,
            Bosses = progress,
            Events = progress,
            TourianStatue = scope.TourianStatueRoom,
            RoomFx = new RoomObjectFx(roomFxOverlay, () => _player.IntroducedSamus),
            Earthquake = roomObjectServices,
            Enemies = roomObjectServices,
        };
        // Each room's room objects get their own collectible block pairs, the room's first collectible that shows its own
        // picture taking the first pair, and their own places, every one free; a cap a shot opened consults the Landing
        // Site's green door before it closes behind Samus.
        var roomObjects = new RoomObjectSystem(roomObjectRoom, roomObjectBindings, _roomObjectVisuals, new CollectibleBlockPairs(),
            new RoomObjectPlaces(),
            new ShotOpenedCapClosing<LockedDoor>(LockedDoorDefinitions.LandingSiteDefaultOrWhenZebesAwakeOrWithPowerBombsGreenDoorFacingLeft),
            _audioRequests);
        scope.RoomObjects = roomObjects;
        var areaServices = new RoomEnemyRuntimeServices(_system, destinationCamera.Scrolls, _player, roomObjects, _display, progress);
        roomObjects.PopulationLoader.LoadRoomPopulation(roomObjectRoom, room.State.ObjectPopulation, roomObjectBindings);

        // The door's setup runs after the destination's room objects exist and before its enemies and first view are
        // set up. Several setups change which of the room's screens scroll, and the first view already stands within
        // those limits.
        door.Setup.Run(destinationCamera.Scrolls, new DoorSetupRoom
        {
            Level = assets.LevelData,
            Area = room.Area,
            RoomObjects = roomObjects,
            BlockAnimations = blockAnimations,
            Scope = scope,
            Samus = loadingSamus,
            Display = _display,
            System = _system,
        });
        // The room state's setup runs right after the door's.
        room.State.SetupBehavior.PlaceRoomObjects(new RoomSetupPlacing
        {
            Room = room,
            Level = assets.LevelData,
            RoomObjects = roomObjects,
            Scrolls = destinationCamera.Scrolls,
            System = _system,
        });

        if (entry.RunDoorClosingObject)
        {
            // The door's closing object spawns after the destination's room objects, the door's and the room's setup
            // and the room's effects, and before the enemies initialize: every room object the room lists exists before
            // the closer takes the next free place, and no enemy's room object has run yet.
            roomObjects.DoorObjects.TrySpawnDoorClosingObject(assets.LevelData, door);
        }

        // Loading shows the entering beams' colours before it initializes enemies. Golden Torizo's code changes the
        // equipment during initialization and leaves those colours shown; the shots fired after it show the granted
        // beams' own pictures (#1586).
        SamusProjectileSystem.ShowBeamShade(_player.Colours, loadingSamus.EquippedBeams);

        // The destination's enemy projectile places start from what the outgoing room's left (RoomHandOver).
        // Each enemy system gets its own compatibility services: the sweep, the places and the effect-actor places start
        // with no earlier kill, occupant or taken place, and a Puyo's ceiling turn starts from what loading the beams'
        // colours above left. The fade step count, the boost cadence and the Yard kick's camera measure outlive the room
        // and are handed on.
        var compatibility = new RoomEnemyCompatibility
        {
            PowerBombRadius = new PowerBombRadiusAfterKill(),
            BullInheritance = new PlaceInheritance<BullEnemyState>(EnemyPlaceOrder.Capacity),
            BeetomInheritance = new PlaceInheritance<BeetomEnemyState>(EnemyPlaceOrder.Capacity),
            PuyoCeilingTurn = new PuyoCeilingTurnFromLastDraw(),
            EffectActorPlaces = new EffectActorPlaces(),
            FadeStepCount = _fadeStepCount,
            UnauthoredBoostCadence = _unauthoredBoostCadence,
            YardKickDistance = _yardKickDistance,
            FallingSparkFloorProbe = new FallingSparkFloorProbe(),
        };
        var enemies = new RoomEnemySystem(new RoomEnemySystemDependencies
        {
            Session = _enemySession,
            Compatibility = compatibility,
            Looks = scope.Looks,
            Definitions = CompiledRoomEnemyDefinitions.Instance,
            Services = areaServices,
            Area = room.Area,
            BossArtwork = _bossArtwork,
            BreakableTerrain = roomObjects.BreakableTerrain,
            RoomFx = roomFxOverlay,
            DoorPassage = _doorTransition.Passage,
            Earthquake = earthquake,
            Scenery = new RoomEnemyScenery(scope.Floor, scope.CeresElevatorShaft, scope.TourianStatueRoom),
        });
        enemies.TakeOverFrom(_room.HandOver);
        scope.Enemies = enemies;
        // A wall jump's waiting shake and Samus's solid-enemy contacts name enemies of the room being replaced, which
        // leave with it.
        loadingSamus.Kinematics.ForgetUnloadedRoomEnemies();
        enemies.Load(new RoomEnemyLoadRequest(
            room.State.EnemyPopulation,
            scope.Appearance)
        {
            Level = assets.LevelData,
            Samus = loadingSamus,
            ControllerInput = _playerActions.Current,
            Camera = destinationCamera.PixelPosition,
            GunshipScenario = entry.GunshipScenario,
            DeferLoaderTimeCameraReads = viewportLoadMode == RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn,
        });
        // A main that acts on one of the room's enemies is composed with it now that the enemies have set up (#2486).
        if (room.State.MainBehavior is IRoomMainOverRoomEnemies overEnemies)
            overEnemies.ComposeOverRoomEnemies(scope, enemies);
        // A live Ridley's encounter hides the HUD map and marks the map rows of his arena his encounter names explored:
        // Norfair's Ridley his arena's, Ceres Ridley none (#2485).
        if (enemies.Ceres.RidleyPresent)
        {
            _hud.DisableMinimapForBoss();
            for (int row = 0; row < enemies.Ceres.ExploredArenaMapRows; row++)
                _system.MarkExploredMapTile(room.Area, room.MapPosition + new Offset16<MapTiles>(0, row));
        }
        // The room state's setup starts its earthquake and its projectile once the enemies are loaded. The pre-Phantoon
        // room's setup holds its projectile place in the pool that Enemies.Load has just cleared; its room has no gate whose
        // projectile the step below spawns, so the two never meet (#2287).
        room.State.SetupBehavior.StartWithRoomEnemies(new RoomSetupEnemiesLoaded
        {
            Enemies = enemies,
            System = _system,
            Carryover = _room.RoomMainCarryover,
        });
        // Gate setup runs while the room-object population is constructed, but Enemies.Load
        // subsequently clears the shared projectile pool. Consume those setup
        // requests here—the first point matching the game's completed room teardown—
        // so the closed actor survives into the initial viewport.
        var effectSite = new EnemyRoomEffectSite
        {
            Level = assets.LevelData,
            Scrolls = destinationCamera.Scrolls,
            Area = room.Area,
            RoomObjects = roomObjects,
            FxOverlay = roomFxOverlay,
            ColourEffects = roomColourEffects,
            Enemies = enemies,
            Samus = loadingSamus,
        };
        EnemyRoomEffects.ApplyPendingDownwardGateProjectileRequests(effectSite);
        _enemyRoomEffects.ApplyEnemyRequestedRoomEffects(enemies.LastEnemyFrame.RoomEffects, effectSite);
        // The gunship's placement starts its lights' glow after the room's own colour effects (#1733).
        EnemyRoomEffects.StartEnemyGlow(enemies.LastEnemyFrame.Glow, effectSite);
        // Only the statue room, whose setup composed the statues' room and whose enemies built the statues over it, has their
        // sequence (#2287, #2485, #2486).
        scope.TourianStatues = scope.TourianStatueRoom is { } statueRoom
            ? new TourianStatueSequence(_tourianStatueDependencies, new TourianStatueSequenceRoom
            {
                StatueRoom = statueRoom,
                Statue = enemies.TourianStatue ?? throw new InvalidOperationException(
                    "The statue room's enemies built no statues over the statue room."),
                Level = assets.LevelData,
                Scrolls = destinationCamera.Scrolls,
                ArtworkFrames = scope.ArtworkFrames,
                FxOverlay = roomFxOverlay,
                ColourEffects = roomColourEffects,
                Earthquake = enemies.Earthquake,
                RoomObjects = roomObjects,
            })
            : null;

        if (viewportLoadMode == RoomViewportLoadMode.KeepSourceScreenWhileScrollingIn)
        {
            // The door transition never redraws the destination's whole viewport: the source screen stays
            // visible while the opening scroll moves the destination in around it.
            _pendingDoorOpeningSourceView = doorOpeningSourceView;
            _room.Adopt(scope.Complete());
            return;
        }

        _room.Adopt(scope.Complete());
        _pendingDoorOpeningSourceView = null;
        _backgroundScroll.ForegroundPosition = destinationCamera.PixelPosition;
        _backgroundScroll.CalculateScrolls();
    }

    /// <summary>
    /// Resolves a room selector against the live saved progress and Samus inventory.
    /// </summary>
    /// <remarks>
    /// The fixed room definition still supplies the area needed to choose that
    /// area's boss flags. The ordered state choice is application-owned definition data;
    /// no production path executes selector data. Keeping this at the shared loader
    /// boundary prevents doors, save stations, and attract scenes from choosing differently.
    /// </remarks>
    internal SelectedRoom LoadSelectedRoom(RoomDefinition room)
    {
        AreaId area = Ensure.NotNull(room).Area;

        // RoomStateSelectionContext owns a detached copy of the events, so selection never
        // observes or changes the live progress.
        // Every room is chosen with Samus: a new game's Ceres start initializes her before choosing its first room.
        SamusState samus = _player.IntroducedSamus;
        var selection = new RoomStateSelectionContext(
            _system.Progress.Events.Copy(),
            DefeatedBosses: _system.BossesDefeatedIn(area),
            HasMorphBallAndMissiles:
                samus.CollectedItems.HasAny(SamusEquipmentFlags.MorphBall) &&
                samus.MaxMissiles != 0,
            HasPowerBombs: samus.MaxPowerBombs != 0);
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

        // Setup's first directional update (none for down), then placing Samus for the destination's loading replaces
        // both whole positions, then every remaining scroll update moves along the travel axis.
        FixedPosition<RoomPixels> samus = DoorOpeningScrollState.RebaseSamus(
            door, DoorOpeningScrollState.ApplySetupMovement(door, source));
        FixedVector<RoomPixels> step = DoorOpeningScrollState.GetSamusStep(door);
        for (int update = DoorOpeningScrollState.MovingScrollUpdates(direction); update > 0; update--)
            samus = samus.Advance(step);

        int finalX = NudgeOffScreenRightEdge(NudgeOffScreenLeftEdge(samus.X.Whole));

        // The room load applies this eight-pixel doorway alignment only horizontally.
        if (!direction.IsVertical())
            finalX = direction == DoorDirection.Right ? RoomGridConversions.HalfBlockEnd(finalX) : RoomGridConversions.HalfBlockStart(finalX);
        samus = samus.WithWhole(new Position16<RoomPixels>(finalX, NudgeOffScreenLeftEdge(samus.Y.Whole)));
        // The door scroll adds 32 to the destination Y after using the unmodified value
        // for Samus's setup origin. The scroll's completion later snaps an upward transition's
        // camera to that adjusted destination; the other three directions retain theirs.
        Position16<RoomPixels> destination = DoorOpeningScrollState.GetDestination(door);
        Position16<RoomPixels> finalCamera = direction == DoorDirection.Up
            ? destination + DoorOpeningScrollDefinitions.UpwardCameraSnap
            : destination;
        return new DoorTransitionPlacement(finalCamera, samus);
    }

    /// <summary>
    /// Applies the Ceres-start Samus pose/state after room loading.
    /// </summary>
    /// <remarks>
    /// Samus starts at the top of the room, at the load station's X: the Ceres elevator's arrival carries her down into
    /// the room, front-facing, and unlocks her movement when it ends.
    /// </remarks>
    public void InitializeCeresStartSamus()
    {
        if (_room.ActiveRoom is null || _room.ActiveLoadStation is null)
            throw new InvalidOperationException("The starting Ceres room must be loaded before Samus setup.");

        SamusState samus = InitializeSamus();
        samus.Pose = SamusPoseId.ForwardFacingPowerSuitPose;
        samus.AnimationFrame = SamusAnimationDelays.AnimationForPose(samus.Pose).Start;
        samus.Position = FixedPosition<RoomPixels>.FromWhole(_room.ActiveLoadStation.Placement.SamusPosition.WithY(0));
        _player.Samus = samus;
        SuperMetroid.Core.Samus.Colours.SamusSuitShade.ShowPower(_player.Colours);
        _assists.GrantLoadoutToNewSamus(samus);

        SamusPoseMetadata.RefreshCollisionRadii(_player.Samus);
        SamusPresentation.InitializeAnimation(_player.Samus);
        _player.Samus.Ground = SamusGround.Of(_room.ActiveRoom.Definition);
        _player.PreviousMovementTypeForXray = SamusPoseMetadata.ReadMovementType(_player.Samus);

        // The Ceres elevator's arrival owns its own state from the first draw, the concealer over the room's level drawn in
        // the room's own colours.
        _room.LoadedScope.BeginCeresElevatorArrival(new CeresElevatorArrivalState(_player.Samus));

        // The locked Ceres start's frame publishes the initial minimap itself, so the HUD shows Ceres's room from the
        // descent's first frame; later frames keep updating it.
        ActiveRoomGeometry roomGeometry = GetActiveRoomGeometry();
        _hud.UpdateMinimap(_system, new MinimapFrame { Area = roomGeometry.Area, RoomMap = roomGeometry.MapPosition, RoomSize = _room.LevelData?.Size ?? _room.ActiveRoom.Size.ToRoomBlocks(), Samus = _player.Samus.Position.Whole, FrameCounter = _clock.ShortCounter, MapRevealMode = _assists.Current.MapReveal, PresentationMap = _artwork.ScreenContent.Get(roomGeometry.Area) });

        // Samus's Ceres-start control setup locks her movement until the elevator's arrival ends.
        _player.GroundedSamusMovementEnabled = false;
    }

    /// <summary>Returns the active room's geometry.</summary>
    internal ActiveRoomGeometry GetActiveRoomGeometry()
    {
        SelectedRoom room = _room.ActiveRoom ?? throw new InvalidOperationException("No room is active.");
        return new ActiveRoomGeometry(room.Area, room.MapPosition, room.FallingCameraOffset, room.RisingCameraOffset);
    }

    /// <summary>
    /// The pictures the outgoing room's foreground blocks show as a door begins (#1392), which its screen keeps through the
    /// door's opening scroll.
    /// </summary>
    private RoomBlockPictureTable SourceScreenPictures()
    {
        RoomScope scope = _room.Scope ?? throw new InvalidOperationException("A door began with no room loaded.");
        // The elevator shaft's screen is its turning floor, which the door's scroll keeps showing: none of its blocks (#1647).
        if (scope.Room.State.TurningFloor == RoomTurningFloor.ElevatorShaft)
            return RoomBlockPictureTable.NoneDrawn(scope.BlockPictures.Sheet);
        return scope.BlockPictures.Resolve(scope.LevelData.ForegroundPlane, null,
            new RoomAppearanceView(scope.Appearance, scope.ArtworkFrames, scope.Looks), []);
    }

    /// <summary>The host-bound room catalogs; room loading never substitutes artwork.</summary>
    private RoomAssetCatalogs RequireRoomAssetCatalogs() =>
        new(_artwork.RoomVisualLayouts, _artwork.RoomLevelStreams, _artwork.RoomBlockSheets, _scrollCellsPastRoomScreens);


    /// <summary>
    /// The doorway nudge the door transition and room load apply: a room coordinate in its screen's second block moves
    /// to eight pixels past that block; any other stays.
    /// </summary>
    private static int NudgeOffScreenLeftEdge(int pixel) =>
        RoomGridConversions.BlockRowWithinScreen(pixel) == DoorOpeningScrollDefinitions.DoorwayNudgeNearBlock
            ? RoomGridConversions.BlockEnd(pixel) + DoorOpeningScrollDefinitions.DoorwayNudgePixels
            : pixel;

    /// <summary>
    /// The doorway nudge's other edge: a room coordinate in its screen's fifteenth block moves to eight pixels before it;
    /// any other stays.
    /// </summary>
    private static int NudgeOffScreenRightEdge(int pixel) =>
        RoomGridConversions.BlockRowWithinScreen(pixel) == DoorOpeningScrollDefinitions.DoorwayNudgeFarBlock
            ? RoomGridConversions.BlockStart(pixel) - DoorOpeningScrollDefinitions.DoorwayNudgePixels
            : pixel;
}
