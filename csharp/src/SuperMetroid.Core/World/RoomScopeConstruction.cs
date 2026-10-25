using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// The room scope while <c>RoomLoader.LoadCartridgeRoom</c> builds it. Each load step sets its part in the cartridge's
/// order, and later steps read the parts already built here, never the previous room's; <see cref="Complete"/> checks
/// every required part. The load runs within one host call, so no frame observes a partial construction.
/// </summary>
[RoomLifetime]
internal sealed class RoomScopeConstruction(CartridgeDoorHeader door, CartridgeRoomHeader room, CartridgeRoomAssets assets,
    LoadStationEntry? loadStation)
{
    /// <summary>The destination door.</summary>
    public CartridgeDoorHeader Door { get; } = Ensure.NotNull(door);

    /// <summary>The destination room and state header.</summary>
    public CartridgeRoomHeader Room { get; } = Ensure.NotNull(room);

    /// <summary>The destination's asset graph.</summary>
    public CartridgeRoomAssets Assets { get; } = Ensure.NotNull(assets);

    /// <summary>The load-station record whose load establishes this room; null for a room entered otherwise.</summary>
    public LoadStationEntry? LoadStation { get; } = loadStation;

    /// <summary>The frame each of the room's animations shows (#1406), which the animations the load builds select into.</summary>
    public ArtworkFrameSelections ArtworkFrames { get; } = new();

    /// <summary>The level allocation, once the level step has run.</summary>
    public RoomLevelData? LevelData { get; set; }

    /// <summary>The camera, once the camera step has run.</summary>
    public ScrollBoundaryCamera? Camera { get; set; }


    /// <summary>The scrolling-sky updater, if the room has one.</summary>
    public ScrollingSkyState? ScrollingSky { get; set; }

    /// <summary>The Mode 7 map of a Ceres room.</summary>
    public FloorMap? FloorMap { get; set; }

    /// <summary>The Landing Site entry, if any.</summary>
    public LandingSiteEntryState? LandingSiteEntry { get; set; }

    /// <summary>Samus's projectiles, once the teardown step has built them.</summary>
    public SamusProjectileSystem? Projectiles { get; set; }

    /// <summary>Samus's bombs, once the teardown step has built them.</summary>
    public SamusBombProjectileSystem? BombProjectiles { get; set; }

    /// <summary>The layer-3 FX, once the FX step has built it.</summary>
    public RoomLayer3FxState? Layer3Fx { get; set; }

    /// <summary>The palette FX, once the FX step has built them.</summary>
    public RoomPaletteFxSystem? PaletteFx { get; set; }

    /// <summary>The Ceres haze, once the FX step has built it.</summary>
    public CeresHazeState? CeresHaze { get; set; }

    /// <summary>The sand tiles, once the FX step has built them.</summary>
    public RoomSandAnimatedTilesState? SandTiles { get; set; }

    /// <summary>The spike tiles, once the FX step has built them.</summary>
    public RoomSpikeAnimatedTilesState? SpikeTiles { get; set; }

    /// <summary>The treadmill tiles, once the FX step has built them.</summary>
    public RoomTreadmillAnimatedTilesState? TreadmillTiles { get; set; }

    /// <summary>The Wrecked Ship treadmill tiles, once the header step has built them.</summary>
    public WreckedShipTreadmillAnimatedTilesState? WreckedShipTreadmill { get; set; }

    /// <summary>The Tourian statues, once the post-enemy step has built them.</summary>
    public TourianStatueSequence? TourianStatues { get; set; }

    /// <summary>The elevatube room main, once the header step has built it.</summary>
    public MaridiaElevatubeRoomMainState? MaridiaElevatube { get; set; }

    /// <summary>The Ceres elevator shaft room main, once the header step has built it.</summary>
    public CeresElevatorShaftRoomMainState? CeresElevatorShaft { get; set; }

    /// <summary>The enemy system, once the enemy step has built it.</summary>
    public RoomEnemySystem? Enemies { get; set; }

    /// <summary>The PLM system, once the PLM population step has built it.</summary>
    public RoomPlmSystem? Plms { get; set; }

    /// <summary>The runtime services, once the PLM population step has created them.</summary>
    public RoomEnemyRuntimeServices? Services { get; set; }

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
        AnimatedCharacters = Assets.Pictures.AnimationsOf(Assets.Tileset),
        BlockPictures = new RoomBlockPictureResolver(Assets.BlockSheet),
        BackgroundImage = Room.State.Background is { } program
            ? new RoomBackgroundImageResolver(
                Assets.Pictures.Background(RoomBackgroundImage.NameOf(program, RoomBackgroundImage.DoorVariant(program, Door), Assets.Tileset)),
                Assets.BlockSheet, LoadedArtwork(program))
            : null,
        BodyPictures = Assets.Pictures.TileWords.TryGetValue(Assets.Tileset, out TileWordSheet? words) ? new TileWordPictureResolver(words) : null,
        Bg2OverlayPictures = words is null ? null : new TileWordPictureResolver(words),
        Bg1OverlayPictures = words is null ? null : new TileWordPictureResolver(words),
        Bg3Pictures = new TileWordPictureResolver(Assets.Pictures.Bg3(Assets.Tileset)),
        MessageBoxes = Assets.Pictures.MessageBox(Assets.Tileset),
        SkyImage = ScrollingSky is { } sky
            ? new RoomBackgroundImageResolver(Assets.Pictures.Background(RoomBackgroundImage.SkyNameOf(sky.Variant, Assets.Tileset)),
                Assets.BlockSheet, Room.State.Background is { } skyProgram ? LoadedArtwork(skyProgram) : RoomLoadArtwork())
            : null,
        ScrollingSky = ScrollingSky,
        FloorMap = FloorMap,
        LandingSiteEntry = LandingSiteEntry,
        Projectiles = Projectiles ?? throw Missing(nameof(Projectiles)),
        BombProjectiles = BombProjectiles ?? throw Missing(nameof(BombProjectiles)),
        Layer3Fx = Layer3Fx ?? throw Missing(nameof(Layer3Fx)),
        PaletteFx = PaletteFx ?? throw Missing(nameof(PaletteFx)),
        CeresHaze = CeresHaze ?? throw Missing(nameof(CeresHaze)),
        SandTiles = SandTiles ?? throw Missing(nameof(SandTiles)),
        SpikeTiles = SpikeTiles ?? throw Missing(nameof(SpikeTiles)),
        TreadmillTiles = TreadmillTiles ?? throw Missing(nameof(TreadmillTiles)),
        WreckedShipTreadmill = WreckedShipTreadmill ?? throw Missing(nameof(WreckedShipTreadmill)),
        TourianStatues = TourianStatues ?? throw Missing(nameof(TourianStatues)),
        MaridiaElevatube = MaridiaElevatube ?? throw Missing(nameof(MaridiaElevatube)),
        CeresElevatorShaft = CeresElevatorShaft ?? throw Missing(nameof(CeresElevatorShaft)),
        Enemies = Enemies ?? throw Missing(nameof(Enemies)),
        Plms = Plms ?? throw Missing(nameof(Plms)),
        Services = Services ?? throw Missing(nameof(Services)),
    };

    /// <summary>
    /// The artwork the room binds as it loads, which its library background's image shows: its common room elements and
    /// tileset, and the characters <paramref name="program"/> transfers.
    /// </summary>
    private HashSet<BgArtworkIdentity> LoadedArtwork(LibraryBackgroundProgram program) =>
    [
        .. RoomLoadArtwork(),
        .. program.Instructions.Where(instruction => instruction.Transfers)
            .Select(instruction => RoomBgArtwork.Library(instruction.Source)),
    ];

    /// <summary>The artwork the room's own load binds: its common room elements and tileset.</summary>
    private HashSet<BgArtworkIdentity> RoomLoadArtwork() =>
        [.. CartridgeRoomAssets.CharacterPlacements(Assets.Tileset, Assets.CreCharacters, Assets.RoomCharacters).Select(placement => placement.Identity)];

    /// <summary>The failure for a load that completed without building <paramref name="part"/>.</summary>
    private static InvalidOperationException Missing(string part) =>
        new($"The room load completed without building its {part}.");
}
