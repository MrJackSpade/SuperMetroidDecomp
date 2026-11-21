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
/// The room scope while <see cref="RoomLoader.LoadRoom"/> builds it. Each load step sets its part in the game's
/// order, and later steps read the parts already built here, never the previous room's; <see cref="Complete"/> checks
/// every required part. The load runs within one host call, so no frame observes a partial construction.
/// </summary>
[RoomLifetime]
internal sealed partial class RoomScopeConstruction(DoorDefinition door, SelectedRoom room, LoadedRoomAssets assets,
    LoadStation? loadStation, RoomMainHandOvers handOvers) : IDoorEventLoad
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

    /// <summary>The level allocation, once the level step has run.</summary>
    public RoomLevelData? LevelData { get; set; }

    /// <summary>The camera, once the camera step has run.</summary>
    public ScrollBoundaryCamera? Camera { get; set; }


    /// <summary>The scrolling-sky updater, once the definition step has composed a sky room's main.</summary>
    internal ScrollingSkyState? ScrollingSky { get; set; }

    /// <summary>The turning floor the room shows, or null when it shows none (#1649).</summary>
    public RoomFloor? Floor { get; set; }

    /// <summary>
    /// The view of the room as a turning floor that the door's room event shows from the room's first frame, or null for
    /// the ordinary room display (#2485).
    /// </summary>
    public TurningFloorTransform? ShownTurningFloor { get; private set; }

    /// <summary>Whether the room's setup has spawned the scrolling sky's drifting bands (#2485).</summary>
    private bool _setupSpawnedSkyBands;

    /// <summary>Samus's projectiles, once the teardown step has built them.</summary>
    public SamusProjectileSystem? Projectiles { get; set; }

    /// <summary>Samus's bombs, once the teardown step has built them.</summary>
    public SamusBombProjectileSystem? BombProjectiles { get; set; }

    /// <summary>The effect-overlay FX, once the FX step has built it.</summary>
    public RoomFxOverlayState? FxOverlay { internal get; set; }

    /// <summary>The colour effects, once the FX step has built them.</summary>
    public ColourEffectSystem? ColourEffects { get; set; }

    /// <summary>The Ceres haze, once the room's setup has composed it for a room whose setup spawns one (#2485).</summary>
    public CeresHazeState? CeresHaze { get; private set; }

    /// <summary>The Tourian statues' room, once the statue room's setup has composed it (#2485).</summary>
    public TourianStatueRoom? TourianStatueRoom { get; private set; }

    /// <summary>The block animations, once the definition step has built them.</summary>
    public RoomBlockAnimations? BlockAnimations { get; set; }

    /// <summary>The Tourian statues, once the post-enemy step has built them for the statue room.</summary>
    public TourianStatueSequence? TourianStatues { get; set; }

    /// <summary>The room's main, once the definition step has composed the one its state names.</summary>
    public IRoomMain? Main { get; private set; }

    /// <summary>The elevatube's ride, once the definition step has composed the elevatube's main.</summary>
    public MaridiaElevatubeRoomMainState? MaridiaElevatube { get; private set; }

    /// <summary>The Ceres elevator shaft's turn, once the elevator door's room event has started it (#2485).</summary>
    public CeresElevatorShaftRoomMainState? CeresElevatorShaft { get; private set; }

    /// <summary>The enemy system, once the enemy step has built it.</summary>
    public RoomEnemySystem? Enemies { get; set; }

    /// <summary>The room-object system, once the room-object population step has built it.</summary>
    public RoomObjectSystem? RoomObjects { get; set; }

    /// <summary>
    /// Composes the scrolling sky's main (#2287): the sky, whose bands drift only where the room's effect or its setup
    /// spawns them, and in the escape's Landing Site its outdoor explosions.
    /// </summary>
    /// <param name="sky">The sky the main scrolls.</param>
    /// <param name="explosions">Where the room's escape explosions go off, or null for a room that sets off none.</param>
    internal void ComposeSkyMain(ScrollingSky sky, EscapeExplosionSite? explosions)
    {
        bool bands = Room.State.Fx?.Select(Door) is { } effect && RoomFxTypes.Of(effect.Type).SpawnsSkyBands || _setupSpawnedSkyBands;
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
    /// <param name="tint">The haze's tint: red once Ceres Ridley is dead, blue before.</param>
    /// <param name="immediateViewport">Whether the room shows its first view at once, which shows the haze at once.</param>
    [IndependentFlag("independent load facts: whether the area's boss is dead and whether the room shows its first view at once")]
    internal void ComposeCeresHaze(CeresHazeTint tint, bool immediateViewport) =>
        CeresHaze = new CeresHazeState(tint, immediateViewport);

    /// <summary>Composes the Tourian statues' room the statue room's setup builds over the room's effect (#2485).</summary>
    /// <param name="fxOverlay">The room's effect, whose water the statues' particles splash through.</param>
    internal void ComposeTourianStatueRoom(RoomFxOverlayState fxOverlay) => TourianStatueRoom = new TourianStatueRoom(fxOverlay);

    /// <summary>Composes the Ceres pre-elevator hall's main and its debris countdown, carried from the previous room's main (#2287).</summary>
    internal void ComposeFallingDebrisMain() =>
        Main = new CeresFallingDebrisMain(new CeresFallingDebrisRoomMainState(_handOvers.ShaftTurnToDebris));

    /// <summary>Composes Ceres Ridley's arena's main, his getaway over the arena's turning <paramref name="floor"/> (#2287).</summary>
    internal void ComposeRidleyGetawayMain(RoomFloor floor) => Main = new CeresRidleyGetawayMain(floor);

    /// <summary>Composes the Crocomire room's main, the rumble while <paramref name="crocomire"/>'s death uncovers the hidden wall (#2287, #2486).</summary>
    internal void ComposeCrocomireRumbleMain(CrocomireEnemyState crocomire) => Main = new CrocomireRoomShakingMain(crocomire);

    /// <summary>Composes the Dachora room's main, which moves the camera toward its shaft while the shaft stands open (#2293).</summary>
    internal void ComposeDachoraShaftCameraMain() => Main = new DachoraShaftCameraMain();

    /// <summary>The finished scope.</summary>
    /// <exception cref="InvalidOperationException">A required load step has not run.</exception>
    public RoomScope Complete() => new()
    {
        Door = Door,
        Room = Room,
        Assets = Assets,
        LoadStation = LoadStation,
        LevelData = LevelData ?? throw Missing(nameof(LevelData)),
        Camera = Camera ?? throw Missing(nameof(Camera)),
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
        Projectiles = Projectiles ?? throw Missing(nameof(Projectiles)),
        BombProjectiles = BombProjectiles ?? throw Missing(nameof(BombProjectiles)),
        FxOverlay = FxOverlay ?? throw Missing(nameof(FxOverlay)),
        ColourEffects = ColourEffects ?? throw Missing(nameof(ColourEffects)),
        CeresHaze = CeresHaze,
        BlockAnimations = BlockAnimations ?? throw Missing(nameof(BlockAnimations)),
        TourianStatues = TourianStatues,
        Main = Main,
        Enemies = Enemies ?? throw Missing(nameof(Enemies)),
        RoomObjects = RoomObjects ?? throw Missing(nameof(RoomObjects)),
    };

    /// <summary>The failure for a load that completed without building <paramref name="part"/>.</summary>
    private static InvalidOperationException Missing(string part) =>
        new($"The room load completed without building its {part}.");
}
