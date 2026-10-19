using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// The room scope while <c>RoomLoader.LoadCartridgeRoom</c> builds it. Each load step sets its part in the cartridge's
/// order, and later steps read the parts already built here, never the previous room's; <see cref="Complete"/> checks
/// every required part. The load runs within one host call, so no frame observes a partial construction.
/// </summary>
internal sealed class RoomScopeConstruction(CartridgeDoorHeader door, CartridgeRoomHeader room, CartridgeRoomAssets assets)
{
    /// <summary>The destination door.</summary>
    public CartridgeDoorHeader Door { get; } = Ensure.NotNull(door);

    /// <summary>The destination room and state header.</summary>
    public CartridgeRoomHeader Room { get; } = Ensure.NotNull(room);

    /// <summary>The destination's asset graph.</summary>
    public CartridgeRoomAssets Assets { get; } = Ensure.NotNull(assets);

    /// <summary>The level allocation, once the level step has run.</summary>
    public RoomLevelData? LevelData { get; set; }

    /// <summary>The camera, once the camera step has run.</summary>
    public ScrollBoundaryCamera? Camera { get; set; }

    /// <summary>The background streamer, once the streamer step has run.</summary>
    public BackgroundTilemapStreamer? BackgroundStreamer { get; set; }

    /// <summary>The scrolling-sky updater, if the room has one.</summary>
    public ScrollingSkyState? ScrollingSky { get; set; }

    /// <summary>The Landing Site entry, if any.</summary>
    public LandingSiteEntryState? LandingSiteEntry { get; set; }

    /// <summary>The runtime services, once the PLM population step has created them.</summary>
    public RoomEnemyRuntimeServices? Services { get; set; }

    /// <summary>The finished scope.</summary>
    /// <exception cref="InvalidOperationException">A required load step has not run.</exception>
    public RoomScope Complete() => new()
    {
        Door = Door,
        Room = Room,
        Assets = Assets,
        LevelData = LevelData ?? throw Missing(nameof(LevelData)),
        Camera = Camera ?? throw Missing(nameof(Camera)),
        BackgroundStreamer = BackgroundStreamer ?? throw Missing(nameof(BackgroundStreamer)),
        ScrollingSky = ScrollingSky,
        LandingSiteEntry = LandingSiteEntry,
        Services = Services ?? throw Missing(nameof(Services)),
    };

    /// <summary>The failure for a load that completed without building <paramref name="part"/>.</summary>
    private static InvalidOperationException Missing(string part) =>
        new($"The room load completed without building its {part}.");
}
