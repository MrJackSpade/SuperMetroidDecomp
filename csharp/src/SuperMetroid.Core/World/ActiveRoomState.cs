using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// The loaded room's live state: room, door and assets, level data, camera, background streamer and scrolling
/// sky, landing-site entry, inherited layer-1 position, load station, Ceres elevator arrival, the value one room main leaves the next, and
/// the door transition's main-screen layers.
/// </summary>
[RoomScopeBoundary("Holds the loaded room and the room under construction.")]
internal sealed class ActiveRoomState
{

    /// <summary>
    /// The layers the active door transition shows below the HUD, or null while gameplay chooses them.
    /// </summary>
    internal ScreenLayers? DoorTransitionMainScreenLayers { get; set; }

    /// <summary>The source room's BG1 screen during a door's opening scroll, or null (#1346).</summary>
    internal RoomDoorBackground? DoorSourceBackground { get; set; }

    /// <summary>The load-station record that established the loading or loaded room; null for a room entered otherwise.</summary>
    public LoadStationEntry? ActiveLoadStation => _construction is { } construction ? construction.LoadStation : Scope?.LoadStation;

    /// <summary>The completed scope of the loaded room, or null before the first load.</summary>
    internal RoomScope? Scope { get; private set; }

    /// <summary>The scope a room load is building; null between loads.</summary>
    private RoomScopeConstruction? _construction;

    /// <summary>The loaded room's scope; gameplay runs only in a loaded room.</summary>
    internal RoomScope LoadedScope => Scope ?? throw new InvalidOperationException("No room is loaded.");

    /// <summary>Starts building a room's scope; until <see cref="Adopt"/>, the room views below read its parts.</summary>
    /// <exception cref="InvalidOperationException">A load is already in progress.</exception>
    internal RoomScopeConstruction BeginConstruction(DoorDefinition door, SelectedRoom room, LoadedRoomAssets assets,
        LoadStationEntry? loadStation)
    {
        if (_construction is not null)
            throw new InvalidOperationException("A room load is already building its scope.");
        _construction = new RoomScopeConstruction(door, room, assets, loadStation);
        return _construction;
    }

    /// <summary>Makes <paramref name="scope"/> the loaded room's, ending any construction.</summary>
    internal void Adopt(RoomScope scope)
    {
        Scope = Ensure.NotNull(scope);
        _construction = null;
    }

    /// <summary>The value one room view reads: the part under construction during a load, otherwise the loaded scope's.</summary>
    private T? View<T>(Func<RoomScopeConstruction, T?> building, Func<RoomScope, T?> loaded) where T : class =>
        _construction is { } construction ? building(construction) : Scope is { } scope ? loaded(scope) : null;

    /// <summary>The selected room/state header for a generic cartridge-backed room.</summary>
    public SelectedRoom? ActiveRoom => View(c => c.Room, s => s.Room);
    /// <summary>The loaded room's header; gameplay runs only in a loaded room.</summary>
    public SelectedRoom LoadedRoom => ActiveRoom ??
        throw new InvalidOperationException("No room is loaded.");

    /// <summary>The load-station door whose setup code established the current room.</summary>
    public DoorDefinition? ActiveDoor => View(c => c.Door, s => s.Door);


    /// <summary>The loaded room's Ceres elevator arrival; null while a load builds the next room's scope.</summary>
    public CeresElevatorArrivalState? CeresElevatorArrival => _construction is null ? Scope?.CeresElevatorArrival : null;

    /// <summary>The value one room's main leaves for the next, which room loading never clears.</summary>
    public RoomMainCarryover RoomMainCarryover { get; } = new();

    /// <summary>
    /// Active layer-1 scroll-boundary state. It remains null until a room-specific scroll
    /// table is loaded; this prevents the runtime from silently assuming Landing Site.
    /// </summary>
    public ScrollBoundaryCamera? Camera => View(c => c.Camera, s => s.Camera);
    /// <summary>The loaded room's scroll camera.</summary>
    public ScrollBoundaryCamera LoadedCamera => Camera ??
        throw new InvalidOperationException("No room camera is loaded.");


    /// <summary>
    /// Active room's decompressed BG1/BTS/BG2 allocation. Rendering and collision must
    /// share this instance; null means no room level stream has been loaded yet.
    /// </summary>
    public RoomLevelData? LevelData => View(c => c.LevelData, s => s.LevelData);
    /// <summary>The loaded room's level data.</summary>
    public RoomLevelData LoadedLevelData => LevelData ??
        throw new InvalidOperationException("No room level data is loaded.");

    /// <summary>Landing Site's bank-$88 scrolling-sky updater after room initialization.</summary>
    public ScrollingSkyState? ScrollingSky => View(c => c.ScrollingSky, s => s.ScrollingSky);

    /// <summary>Door/header/library-background data selected for the current Landing Site entry.</summary>
    public LandingSiteEntryState? LandingSiteEntry => View(c => c.LandingSiteEntry, s => s.LandingSiteEntry);

    /// <summary>The loading or loaded room's layer-3 FX, for the accepted-frame publication.</summary>
    internal RoomLayer3FxState? Layer3Fx => View(c => c.Layer3Fx, s => s.Layer3Fx);

    /// <summary>The loading or loaded room's Tourian statues, for the accepted-frame publication.</summary>
    internal TourianStatueSequence? TourianStatues => View(c => c.TourianStatues, s => s.TourianStatues);

    /// <summary>The loading or loaded room's enemies, for the accepted-frame publication.</summary>
    internal RoomEnemySystem? Enemies => View(c => c.Enemies, s => s.Enemies);

    /// <summary>The loading or loaded room's bombs, for frame roots such as the accepted-frame publication.</summary>
    internal SamusBombProjectileSystem? BombProjectiles => View(c => c.BombProjectiles, s => s.BombProjectiles);

    /// <summary>
    /// The layer-one position an earlier gameplay owner left in WRAM. Only its fraction words
    /// <c>$090F</c>/<c>$0913</c> matter: no load path writes them, so this runtime's first
    /// camera starts from them.
    /// </summary>
    internal FixedPosition<RoomPixels> InheritedLayer1Position { get; set; }
}
