using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Bosses.Crocomire;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room scope while <see cref="RoomLoader.LoadRoom"/> builds it. The construction starts with the parts the load has
/// before its first step (the room's level, camera and bombs); each load step composes its part in the game's order, and
/// later steps read the parts already built here, never the previous room's. <see cref="Complete"/> takes the parts the
/// load's later steps built, so a finished scope is never missing one. The load runs within one host call, so no frame
/// observes a partial construction.
/// </summary>
[RoomLifetime]
internal sealed class RoomScopeConstruction(DoorDefinition door, SelectedRoom room, LoadedRoomAssets assets,
    LoadStation? loadStation, ScrollBoundaryCamera camera, SamusBombProjectileSystem bombProjectiles,
    RoomMainHandOvers handOvers) : IDoorEventLoad
{
    /// <summary>What the previous room's main handed on, each hand-over given only to the main that carries it.</summary>
    private readonly RoomMainHandOvers _handOvers = Ensure.NotNull(handOvers);

    /// <summary>The destination door.</summary>
    public DoorDefinition Door { get; } = Ensure.NotNull(door);

    /// <summary>The destination room and state definition.</summary>
    public SelectedRoom Room { get; } = Ensure.NotNull(room);

    /// <summary>The destination's asset graph.</summary>
    public LoadedRoomAssets Assets { get; } = Ensure.NotNull(assets);

    /// <summary>The load station whose load establishes this room; null for a room entered otherwise.</summary>
    public LoadStation? LoadStation { get; } = loadStation;

    /// <summary>The frame each of the room's animations shows (#1406), which the animations the load builds select into.</summary>
    public ArtworkFrameSelections ArtworkFrames { get; } = new();

    /// <summary>What gameplay shows over the room's load (#1466), which the room objects and enemies the load builds show into.</summary>
    public RoomAppearance Appearance { get; } = new();

    /// <summary>
    /// The look each of the room's encounters shows (#1538, #2160), which the enemies the load builds show into: each starts
    /// at the look the room's load gives it.
    /// </summary>
    public EncounterLooks Looks { get; } = new();

    /// <summary>The room's level, which its assets loaded.</summary>
    public RoomLevelData LevelData => Assets.LevelData;

    /// <summary>The room's camera, which the camera step places.</summary>
    public ScrollBoundaryCamera Camera { get; } = Ensure.NotNull(camera);

    /// <summary>Samus's bombs in the room, built new for it before its load begins.</summary>
    public SamusBombProjectileSystem BombProjectiles { get; } = Ensure.NotNull(bombProjectiles);


    /// <summary>The scrolling-sky updater, once the definition step has composed a sky room's main.</summary>
    private ScrollingSkyState? ScrollingSky { get; set; }

    /// <summary>The turning floor the room shows, or null when it shows none (#1649).</summary>
    public RoomFloor? Floor { get; set; }

    /// <summary>
    /// The view of the room as a turning floor that the door's room event shows from the room's first frame, or null for
    /// the ordinary room display (#2485).
    /// </summary>
    public TurningFloorTransform? ShownTurningFloor { get; private set; }

    /// <summary>Whether the room's setup has spawned the scrolling sky's drifting bands (#2485).</summary>
    private bool _setupSpawnedSkyBands;

    /// <summary>The Ceres haze, once the room's setup has composed it for a room whose setup spawns one (#2485).</summary>
    public CeresHazeState? CeresHaze { get; private set; }

    /// <summary>The Tourian statues' room, once the statue room's setup has composed it (#2485).</summary>
    public TourianStatueRoom? TourianStatueRoom { get; private set; }

    /// <summary>The room's main, once the definition step has composed the one its state names.</summary>
    public IRoomMain? Main { get; private set; }

    /// <summary>The elevatube's ride, once the definition step has composed the elevatube's main.</summary>
    public MaridiaElevatubeRoomMainState? MaridiaElevatube { get; private set; }

    /// <summary>The Ceres elevator shaft's turn, once the elevator door's room event has started it (#2485).</summary>
    public CeresElevatorShaftRoomMainState? CeresElevatorShaft { get; private set; }

    /// <summary>
    /// Composes the scrolling sky's main (#2287): the sky, whose bands drift only where the room's effect or its setup
    /// spawns them, and in the escape's Landing Site its outdoor explosions.
    /// </summary>
    /// <param name="sky">The sky the main scrolls.</param>
    /// <param name="explosions">Where the room's escape explosions go off, or null for a room that sets off none.</param>
    internal void ComposeSkyMain(ScrollingSky sky, EscapeExplosionSite? explosions)
    {
        bool bands = Room.State.Fx?.Select(Door)?.Type == RoomFxType.ScrollingSky || _setupSpawnedSkyBands;
        var state = new ScrollingSkyState(sky, bands);
        ScrollingSky = state;
        Main = new ScrollingSkyMain(state, explosions);
    }

    /// <summary>
    /// Composes an escape room's main (#2287): its explosions and, for a room whose shaking turns diagonal, the countdown
    /// of its diagonal shake, carried from the previous room's main.
    /// </summary>
    /// <param name="site">Where the room's explosions go off.</param>
    /// <param name="diagonal">How the room's shaking turns diagonal, or null for a room whose shaking never does.</param>
    internal void ComposeEscapeMain(EscapeExplosionSite site, EscapeDiagonalShaking? diagonal) =>
        Main = new EscapeShakingMain(site,
            diagonal is { } shaking ? new EscapeDiagonalQuake(shaking, _handOvers.EscapeQuake) : null);

    /// <summary>Composes the Maridia elevatube's main and its ride, which the tube's doors start (#2287).</summary>
    internal void ComposeElevatubeMain()
    {
        var tube = new MaridiaElevatubeRoomMainState(_handOvers.ElevatubeFraction);
        MaridiaElevatube = tube;
        Main = new MaridiaElevatubeMain(tube);
    }

    /// <summary>Composes the Ceres elevator shaft's main over the turn its elevator door's room event started (#2287, #2485).</summary>
    /// <param name="shaft">The shaft's turn.</param>
    internal void ComposeElevatorShaftMain(CeresElevatorShaftRoomMainState shaft) => Main = new CeresElevatorShaftMain(shaft);

    /// <inheritdoc/>
    /// <remarks>
    /// The shaft's turn starts level, which is the view the door shows; the turn leaves its starting step for the
    /// following room's main.
    /// </remarks>
    public void ShowShaftLevelAndStartTurn()
    {
        var shaft = new CeresElevatorShaftRoomMainState(_handOvers.ShaftTurnToDebris);
        CeresElevatorShaft = shaft;
        ShownTurningFloor = shaft.Transform;
    }

    /// <summary>Records that the room's setup spawns the scrolling sky's drifting bands, which the sky's main then drifts (#2485).</summary>
    internal void ComposeSkyBands() => _setupSpawnedSkyBands = true;

    /// <summary>Composes the Ceres haze the room's setup spawns (#2485).</summary>
    /// <param name="ridleyIsDead">Whether Ceres Ridley is dead, which turns the haze red.</param>
    /// <param name="immediateViewport">Whether the room shows its first view at once, which shows the haze at once.</param>
    [IndependentFlag("independent load facts: whether the area's boss is dead and whether the room shows its first view at once")]
    internal void ComposeCeresHaze(bool ridleyIsDead, bool immediateViewport) =>
        CeresHaze = new CeresHazeState(ridleyIsDead, immediateViewport);

    /// <summary>Composes the Tourian statues' room the statue room's setup builds over the room's effect (#2485).</summary>
    /// <param name="water">The live surface of the room effect's water, which the statues' particles splash through.</param>
    internal void ComposeTourianStatueRoom(RoomLiquidSurfaceLine water) => TourianStatueRoom = new TourianStatueRoom(water);

    /// <summary>Composes the Ceres pre-elevator hall's main and its debris countdown, carried from the previous room's main (#2287).</summary>
    internal void ComposeFallingDebrisMain() =>
        Main = new CeresFallingDebrisMain(new CeresFallingDebrisRoomMainState(_handOvers.ShaftTurnToDebris));

    /// <summary>Composes Ceres Ridley's arena's main, his getaway over the arena's turning <paramref name="floor"/> (#2287).</summary>
    internal void ComposeRidleyGetawayMain(RoomFloor floor) => Main = new CeresRidleyGetawayMain(floor);

    /// <summary>Composes the Crocomire room's main, the rumble while <paramref name="crocomire"/>'s death uncovers the hidden wall (#2287, #2486).</summary>
    internal void ComposeCrocomireRumbleMain(CrocomireEnemyState crocomire) => Main = new CrocomireRoomShakingMain(crocomire);

    /// <summary>Composes the Dachora room's main, which moves the camera toward its shaft while the shaft stands open (#2293).</summary>
    internal void ComposeDachoraShaftCameraMain() => Main = new DachoraShaftCameraMain();

    /// <summary>The finished scope, with the parts the load's later steps built.</summary>
    /// <param name="blockAnimations">The room's block animations.</param>
    /// <param name="fxOverlay">The room's effect.</param>
    /// <param name="colourEffects">The room's colour effects.</param>
    /// <param name="projectiles">Samus's projectiles in the room.</param>
    /// <param name="roomObjects">The room's room objects.</param>
    /// <param name="enemies">The room's enemies.</param>
    /// <param name="tourianStatues">The statues' sequence, which only the statue room has.</param>
    public RoomScope Complete(RoomBlockAnimations blockAnimations, RoomFxOverlayState fxOverlay, ColourEffectSystem colourEffects,
        SamusProjectileSystem projectiles, RoomObjectSystem roomObjects, RoomEnemySystem enemies,
        TourianStatueSequence? tourianStatues) => new()
    {
        Door = Door,
        Room = Room,
        Assets = Assets,
        LoadStation = LoadStation,
        LevelData = LevelData,
        Camera = Camera,
        ArtworkFrames = ArtworkFrames,
        Appearance = Appearance,
        Looks = Looks,
        BlockPictures = new RoomBlockPictureResolver(Assets.BlockSheet),
        // Only a library background the player sees as its own picture has one installed (#1649).
        BackgroundImage = RoomLibraryBackgroundPicture.Of(Room.State.Background, background => new RoomBackgroundImageResolver(
            Assets.Pictures.Background(background, background.PictureFor(Door), Assets.Tileset), Assets.BlockSheet)),
        WrittenPictures = Assets.Pictures.Written.TryGetValue(Assets.Tileset, out WrittenPictureSheet? written) ? new WrittenPictureResolver(written) : null,
        FxOverlayPictures = new WrittenPictureResolver(Assets.Pictures.FxOverlay(Assets.Tileset)),
        HudPictures = Assets.Pictures.Hud(Assets.Tileset),
        MessageBoxes = Assets.Pictures.MessageBox(Assets.Tileset),
        Sky = ScrollingSky is { } sky
            ? new RoomSky(sky, new RoomBackgroundImageResolver(Assets.Pictures.Sky(sky.Sky, Assets.Tileset), Assets.BlockSheet))
            : null,
        Floor = Floor,
        Projectiles = Ensure.NotNull(projectiles),
        BombProjectiles = BombProjectiles,
        FxOverlay = Ensure.NotNull(fxOverlay),
        ColourEffects = Ensure.NotNull(colourEffects),
        CeresHaze = CeresHaze,
        BlockAnimations = Ensure.NotNull(blockAnimations),
        TourianStatues = tourianStatues,
        Main = Main,
        Enemies = Ensure.NotNull(enemies),
        RoomObjects = Ensure.NotNull(roomObjects),
    };
}
