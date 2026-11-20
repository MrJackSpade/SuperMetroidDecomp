using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// The loaded room's live state: room, door, level data, camera, scrolling sky, load station, Ceres elevator arrival,
/// effects, statues, enemies and bombs, what one room leaves the next, and the layers the door transition shows.
/// </summary>
[RoomScopeBoundary("Holds the loaded room and the room under construction.")]
internal sealed class ActiveRoomState
{

    /// <summary>
    /// The layers the active door transition shows below the HUD, or null while gameplay chooses them.
    /// </summary>
    internal ScreenLayers? DoorTransitionShownLayers { get; set; }

    /// <summary>The source room's foreground screen during a door's opening scroll, or null (#1346).</summary>
    internal RoomDoorBackground? DoorSourceBackground { get; set; }

    /// <summary>The load station that established the loading or loaded room; null for a room entered
    /// otherwise.</summary>
    public LoadStation? ActiveLoadStation => _construction is { } construction ? construction.LoadStation : Scope?.LoadStation;

    /// <summary>The completed scope of the loaded room, or null before the first load.</summary>
    internal RoomScope? Scope { get; private set; }

    /// <summary>How many rooms the runtime has loaded, counting each completed room load (#2562).</summary>
    internal int RoomsLoaded { get; private set; }

    /// <summary>The scope a room load is building; null between loads.</summary>
    private RoomScopeConstruction? _construction;

    /// <summary>The loaded room's scope; gameplay runs only in a loaded room.</summary>
    internal RoomScope LoadedScope => Scope ?? throw new InvalidOperationException("No room is loaded.");

    /// <summary>Starts building a room's scope; until <see cref="Adopt"/>, the room views below read its parts.</summary>
    /// <exception cref="InvalidOperationException">A load is already in progress.</exception>
    internal RoomScopeConstruction BeginConstruction(DoorDefinition door, SelectedRoom room, LoadedRoomAssets assets,
        LoadStation? loadStation)
    {
        if (_construction is not null)
            throw new InvalidOperationException("A room load is already building its scope.");
        _construction = new RoomScopeConstruction(door, room, assets, loadStation, RoomMainHandOvers);
        return _construction;
    }

    /// <summary>Makes <paramref name="scope"/> the loaded room's, ending any construction.</summary>
    internal void Adopt(RoomScope scope)
    {
        Scope = Ensure.NotNull(scope);
        RoomsLoaded++;
        _construction = null;
    }

    /// <summary>The value one room view reads: the part under construction during a load, otherwise the loaded scope's.</summary>
    private T? View<T>(Func<RoomScopeConstruction, T?> building, Func<RoomScope, T?> loaded) where T : class =>
        _construction is { } construction ? building(construction) : Scope is { } scope ? loaded(scope) : null;

    /// <summary>The selected room and state of the room loading or loaded.</summary>
    public SelectedRoom? ActiveRoom => View(c => c.Room, s => s.Room);
    /// <summary>The loaded room's definition; gameplay runs only in a loaded room.</summary>
    public SelectedRoom LoadedRoom => ActiveRoom ??
        throw new InvalidOperationException("No room is loaded.");

    /// <summary>The load-station door whose setup established the current room.</summary>
    public DoorDefinition? ActiveDoor => View(c => c.Door, s => s.Door);

    /// <summary>What one room's main hands the next, which room loading never clears.</summary>
    public RoomMainHandOvers RoomMainHandOvers { get; } = new();

    /// <summary>What one room's shots and enemy projectiles leave for the next room's, which room loading never clears.</summary>
    public RoomHandOver HandOver { get; } = new();

    /// <summary>
    /// The loaded room's camera, which keeps the view within the room's scroll limits; null until a room loads.
    /// </summary>
    public ScrollBoundaryCamera? Camera => View(c => c.Camera, s => s.Camera);
    /// <summary>The loaded room's scroll camera.</summary>
    public ScrollBoundaryCamera LoadedCamera => Camera ??
        throw new InvalidOperationException("No room camera is loaded.");


    /// <summary>
    /// Active room's loaded foreground, block behaviours and background. Rendering and collision must
    /// share this instance; null means no room level data has been loaded yet.
    /// </summary>
    private RoomLevelData? LevelData => View(c => c.LevelData, s => s.LevelData);
    /// <summary>The loaded room's level data.</summary>
    public RoomLevelData LoadedLevelData => LevelData ??
        throw new InvalidOperationException("No room level data is loaded.");

    /// <summary>The loading or loaded room's bombs, for frame roots such as the accepted-frame publication.</summary>
    internal SamusBombProjectileSystem? BombProjectiles => View(c => c.BombProjectiles, s => s.BombProjectiles);
}
