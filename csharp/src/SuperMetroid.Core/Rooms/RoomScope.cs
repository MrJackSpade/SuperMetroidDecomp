using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Enemies.WreckedShip;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// One loaded room's state, built by <see cref="RoomScopeConstruction"/> in the cartridge's load order and replaced
/// whole by the next load. Nothing in it survives into another room; values a destination reads from its source room
/// travel through <see cref="RoomTransitionCarryOver"/> instead (#1329, <c>docs/room-scope.md</c>).
/// </summary>
internal sealed record RoomScope
{
    /// <summary>The door whose destination this room is, or the load station's synthetic door.</summary>
    public required CartridgeDoorHeader Door { get; init; }

    /// <summary>The selected room and state header.</summary>
    public required CartridgeRoomHeader Room { get; init; }

    /// <summary>The room's decompressed, read-only asset graph.</summary>
    public required CartridgeRoomAssets Assets { get; init; }

    /// <summary>The room's BG1/BTS/BG2 allocation, shared by rendering and collision.</summary>
    public required RoomLevelData LevelData { get; init; }

    /// <summary>The room's layer-1 scroll-boundary camera.</summary>
    public required ScrollBoundaryCamera Camera { get; init; }

    /// <summary>The room's background tilemap producer.</summary>
    public required BackgroundTilemapStreamer BackgroundStreamer { get; init; }

    /// <summary>The bank-$88 scrolling-sky updater of a scrolling-sky room, or null.</summary>
    public required ScrollingSkyState? ScrollingSky { get; init; }

    /// <summary>Door, header and library-background data of a Landing Site entry, or null.</summary>
    public required LandingSiteEntryState? LandingSiteEntry { get; init; }

    /// <summary>Samus's beam and missile projectiles in this room; the held charge lives on the session.</summary>
    public required SamusProjectileSystem Projectiles { get; init; }

    /// <summary>Samus's bombs, power bombs and the shared projectile cooldown in this room.</summary>
    public required SamusBombProjectileSystem BombProjectiles { get; init; }

    /// <summary>The room's layer-3 FX: liquid, rain, fog and the BG3 tilemap.</summary>
    public required RoomLayer3FxState Layer3Fx { get; init; }

    /// <summary>The room's palette-FX objects.</summary>
    public required RoomPaletteFxSystem PaletteFx { get; init; }

    /// <summary>The room's Ceres haze, enabled only where the setup callback spawns it.</summary>
    public required CeresHazeState CeresHaze { get; init; }

    /// <summary>The room's sand animated tiles.</summary>
    public required RoomSandAnimatedTilesState SandTiles { get; init; }

    /// <summary>The room's horizontal-spike animated tiles.</summary>
    public required RoomSpikeAnimatedTilesState SpikeTiles { get; init; }

    /// <summary>The room's FX-selected treadmill animated tiles.</summary>
    public required RoomTreadmillAnimatedTilesState TreadmillTiles { get; init; }

    /// <summary>The Wrecked Ship entrance treadmill tiles a door's setup code may start.</summary>
    public required WreckedShipTreadmillAnimatedTilesState WreckedShipTreadmill { get; init; }

    /// <summary>The room's Tourian statue tile programs and descent, enabled only in the statue room.</summary>
    public required TourianStatueSequence TourianStatues { get; init; }

    /// <summary>The room's PLM population and the family owners that run it.</summary>
    public required RoomPlmSystem Plms { get; init; }

    /// <summary>The room's runtime services, bound to its area, shared by its PLMs and enemies.</summary>
    public required RoomEnemyRuntimeServices Services { get; init; }
}
