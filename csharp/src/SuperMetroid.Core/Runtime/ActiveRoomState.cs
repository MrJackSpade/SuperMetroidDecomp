using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Runtime;

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

    /// <summary>The selected room/state header for a generic cartridge-backed room.</summary>
    public CartridgeRoomHeader? ActiveRoom { get; internal set; }

    /// <summary>The load-station door whose setup code established the current room.</summary>
    public CartridgeDoorHeader? ActiveDoor { get; internal set; }

    /// <summary>Decompressed assets owned by <see cref="ActiveRoomState.ActiveRoom"/>.</summary>
    public CartridgeRoomAssets? ActiveRoomAssets { get; internal set; }

    /// <summary>The two native bank-$86 objects that carry Samus into fresh Ceres.</summary>
    public CeresElevatorArrivalState? CeresElevatorArrival { get; internal set; }

    /// <summary>Room-main scratch word <c>$07E1</c>, which room loading never clears.</summary>
    public RoomMainScratchState RoomMainScratch { get; internal set; } = new();

    /// <summary>
    /// Active layer-1 scroll-boundary state. It remains null until a room-specific scroll
    /// table is loaded; this prevents the runtime from silently assuming Landing Site.
    /// </summary>
    public ScrollBoundaryCamera? Camera { get; internal set; }

    /// <summary>Landing Site's ROM-backed bank-$80 tilemap producer after room initialization.</summary>
    public BackgroundTilemapStreamer? BackgroundStreamer { get; internal set; }

    /// <summary>Row/column updates produced by the most recent camera-to-BG pass.</summary>
    public int LastBackgroundUpdateCount { get; internal set; }

    /// <summary>
    /// Active room's decompressed BG1/BTS/BG2 allocation. Rendering and collision must
    /// share this instance; null means no room level stream has been loaded yet.
    /// </summary>
    public RoomLevelData? LevelData { get; internal set; }

    /// <summary>Landing Site's bank-$88 scrolling-sky updater after room initialization.</summary>
    public ScrollingSkyState? ScrollingSky { get; internal set; }

    /// <summary>Door/header/library-background data selected for the current Landing Site entry.</summary>
    public LandingSiteEntryState? LandingSiteEntry { get; internal set; }

    /// <summary>
    /// The layer-one position an earlier gameplay owner left in WRAM. Only its fraction words
    /// <c>$090F</c>/<c>$0913</c> matter: no load path writes them, so this runtime's first
    /// camera starts from them.
    /// </summary>
    internal FixedPosition<RoomPixels> InheritedLayer1Position { get; set; }
}
