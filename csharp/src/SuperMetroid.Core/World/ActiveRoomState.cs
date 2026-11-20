using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Areas.Ceres;
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

    /// <summary>The load-station record that established the loading or loaded room; null for a room entered otherwise.</summary>
    public LoadStation? ActiveLoadStation => _construction is { } construction ? construction.LoadStation : Scope?.LoadStation;

    /// <summary>Whether a room is loaded: none before the runtime's first room load completes, then the last one's.</summary>
    internal RoomOccupancy Occupancy { get; private set; } = RoomOccupancy.NoRoomLoaded.Instance;

    /// <summary>The completed scope of the loaded room, for a reader that also runs before the first load.</summary>
    internal RoomScope? Scope => Occupancy is RoomOccupancy.Loaded loaded ? loaded.Scope : null;

    /// <summary>How many rooms the runtime has loaded, counting each completed room load (#2562).</summary>
    internal int RoomsLoaded { get; private set; }

    /// <summary>The scope a room load is building; null between loads.</summary>
    private RoomScopeConstruction? _construction;

    /// <summary>The loaded room's scope; gameplay runs only once the runtime's first room has loaded.</summary>
    /// <exception cref="InvalidOperationException">Gameplay ran before the runtime's first room load.</exception>
    internal RoomScope LoadedScope => Occupancy switch
    {
        RoomOccupancy.Loaded loaded => loaded.Scope,
        RoomOccupancy.NoRoomLoaded => throw new InvalidOperationException("Gameplay ran before the runtime's first room load."),
        _ => throw new InvalidOperationException($"Undefined {nameof(RoomOccupancy)} {Occupancy}."),
    };

    /// <summary>Starts building a room's scope; until <see cref="Adopt"/>, the room views below read its parts.</summary>
    /// <exception cref="InvalidOperationException">A load is already in progress.</exception>
    internal RoomScopeConstruction BeginConstruction(DoorDefinition door, SelectedRoom room, LoadedRoomAssets assets,
        LoadStation? loadStation, ScrollBoundaryCamera camera, SamusBombProjectileSystem bombProjectiles)
    {
        if (_construction is not null)
            throw new InvalidOperationException("A room load is already building its scope.");
        var construction = new RoomScopeConstruction(door, room, assets, loadStation, camera, bombProjectiles, RoomMainHandOvers);
        _construction = construction;
        return construction;
    }

    /// <summary>Makes <paramref name="scope"/> the loaded room's, ending any construction.</summary>
    internal void Adopt(RoomScope scope)
    {
        Occupancy = new RoomOccupancy.Loaded(scope);
        RoomsLoaded++;
        _construction = null;
    }

    /// <summary>The value one room view reads: the part under construction during a load, otherwise the loaded scope's.</summary>
    internal T? View<T>(Func<RoomScopeConstruction, T?> building, Func<RoomScope, T?> loaded) where T : class =>
        _construction is { } construction ? building(construction) : Scope is { } scope ? loaded(scope) : null;

    /// <summary>The selected room/state definition for a generic backed room.</summary>
    public SelectedRoom? ActiveRoom => View(c => c.Room, s => s.Room);
    /// <summary>The loading or loaded room's definition; gameplay runs only once the runtime's first room has loaded.</summary>
    public SelectedRoom LoadedRoom => _construction is { } construction ? construction.Room : LoadedScope.Room;

    /// <summary>The load-station door whose setup established the current room.</summary>
    public DoorDefinition? ActiveDoor => View(c => c.Door, s => s.Door);


    /// <summary>The loaded room's Ceres elevator arrival; null while a load builds the next room's scope.</summary>
    public CeresElevatorArrivalState? CeresElevatorArrival => _construction is null ? Scope?.CeresElevatorArrival : null;

    /// <summary>What one room's main hands the next, which room loading never clears.</summary>
    public RoomMainHandOvers RoomMainHandOvers { get; } = new();

    /// <summary>What one room's shots and enemy projectiles leave for the next room's, which room loading never clears.</summary>
    public RoomHandOver HandOver { get; } = new();

    /// <summary>
    /// The loaded room's camera, which keeps the view within the room's scroll limits; null until a room loads.
    /// </summary>
    public ScrollBoundaryCamera? Camera => View(c => c.Camera, s => s.Camera);
    /// <summary>The loading or loaded room's scroll camera.</summary>
    public ScrollBoundaryCamera LoadedCamera => _construction is { } construction ? construction.Camera : LoadedScope.Camera;


    /// <summary>
    /// Active room's loaded foreground, block behaviours and background. Rendering and collision must
    /// share this instance; null means no room level data has been loaded yet.
    /// </summary>
    public RoomLevelData? LevelData => View(c => c.LevelData, s => s.LevelData);
    /// <summary>The loading or loaded room's level data.</summary>
    public RoomLevelData LoadedLevelData => _construction is { } construction ? construction.LevelData : LoadedScope.LevelData;

    /// <summary>The loading or loaded room's bombs, for frame roots such as the accepted-frame publication.</summary>
    internal SamusBombProjectileSystem? BombProjectiles => View(c => c.BombProjectiles, s => s.BombProjectiles);
}
