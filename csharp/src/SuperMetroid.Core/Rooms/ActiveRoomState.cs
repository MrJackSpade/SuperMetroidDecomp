using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The loaded room's live state: room, door and assets, level data, camera, background streamer and scrolling
/// sky, landing-site entry, inherited layer-1 position, load station, Ceres elevator arrival, room-main scratch and
/// the door transition's main-screen layers.
/// </summary>
internal sealed class ActiveRoomState
{

    /// <summary>
    /// Main-screen layers selected below the HUD by the active door IRQ, or null while
    /// ordinary gameplay IRQ commands own <c>TM ($212C)</c>.
    /// </summary>
    internal SnesMainScreenLayers? DoorTransitionMainScreenLayers { get; set; }

    /// <summary>The load-station record that most recently established this runtime.</summary>
    public LoadStationEntry? ActiveLoadStation { get; internal set; }

    /// <summary>The completed scope of the loaded room, or null before the first load.</summary>
    internal RoomScope? Scope { get; private set; }

    /// <summary>The scope a room load is building; null between loads.</summary>
    private RoomScopeConstruction? _construction;

    /// <summary>Starts building a room's scope; until <see cref="Adopt"/>, the room views below read its parts.</summary>
    /// <exception cref="InvalidOperationException">A load is already in progress.</exception>
    internal RoomScopeConstruction BeginConstruction(CartridgeDoorHeader door, CartridgeRoomHeader room, CartridgeRoomAssets assets)
    {
        if (_construction is not null)
            throw new InvalidOperationException("A room load is already building its scope.");
        _construction = new RoomScopeConstruction(door, room, assets);
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
    public CartridgeRoomHeader? ActiveRoom => View(c => c.Room, s => s.Room);
    /// <summary>The loaded room's header; gameplay runs only in a loaded room.</summary>
    public CartridgeRoomHeader LoadedRoom => ActiveRoom ??
        throw new InvalidOperationException("No room is loaded.");

    /// <summary>The load-station door whose setup code established the current room.</summary>
    public CartridgeDoorHeader? ActiveDoor => View(c => c.Door, s => s.Door);


    /// <summary>The two native bank-$86 objects that carry Samus into fresh Ceres.</summary>
    public CeresElevatorArrivalState? CeresElevatorArrival { get; internal set; }

    /// <summary>Room-main scratch word <c>$07E1</c>, which room loading never clears.</summary>
    public RoomMainScratchState RoomMainScratch { get; internal set; } = new();

    /// <summary>
    /// Active layer-1 scroll-boundary state. It remains null until a room-specific scroll
    /// table is loaded; this prevents the runtime from silently assuming Landing Site.
    /// </summary>
    public ScrollBoundaryCamera? Camera => View(c => c.Camera, s => s.Camera);
    /// <summary>The loaded room's scroll camera.</summary>
    public ScrollBoundaryCamera LoadedCamera => Camera ??
        throw new InvalidOperationException("No room camera is loaded.");

    /// <summary>Landing Site's ROM-backed bank-$80 tilemap producer after room initialization.</summary>
    public BackgroundTilemapStreamer? BackgroundStreamer => View(c => c.BackgroundStreamer, s => s.BackgroundStreamer);


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

    /// <summary>
    /// The layer-one position an earlier gameplay owner left in WRAM. Only its fraction words
    /// <c>$090F</c>/<c>$0913</c> matter: no load path writes them, so this runtime's first
    /// camera starts from them.
    /// </summary>
    internal FixedPosition<RoomPixels> InheritedLayer1Position { get; set; }
}
