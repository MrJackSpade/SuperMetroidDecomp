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
/// One loaded room's state, built by <see cref="RoomScopeConstruction"/> in the cartridge's load order and replaced
/// whole by the next load. Nothing in it survives into another room; values a destination reads from its source room
/// travel through <see cref="RoomTransitionCarryOver"/> instead (#1329, <c>docs/room-scope.md</c>).
/// </summary>
[RoomLifetime]
internal sealed record RoomScope
{
    /// <summary>The door whose destination this room is, or the load station's synthetic door.</summary>
    public required DoorDefinition Door { get; init; }

    /// <summary>The selected room and state header.</summary>
    public required SelectedRoom Room { get; init; }

    /// <summary>The room's decompressed, read-only asset graph.</summary>
    public required LoadedRoomAssets Assets { get; init; }

    /// <summary>The room's BG1/BTS/BG2 allocation, shared by rendering and collision.</summary>
    public required RoomLevelData LevelData { get; init; }

    /// <summary>The room's layer-1 scroll-boundary camera.</summary>
    public required ScrollBoundaryCamera Camera { get; init; }

    /// <summary>
    /// The frame each of the room's animations shows (#1406): its animated tiles, statues, corpses and bosses select their
    /// pictures here, and the display resolves the pictures of the characters they paint from it.
    /// </summary>
    public required ArtworkFrameSelections ArtworkFrames { get; init; }

    /// <summary>
    /// What gameplay has shown over the room's load (#1466): the collectibles its slots show and Kraid's backdrop. With the
    /// frames its animations selected, the display names its blocks', cells' and words' pictures by it.
    /// </summary>
    public required RoomAppearance Appearance { get; init; }

    /// <summary>Resolves the pre-coloured picture each block the room's planes show draws as (#1392).</summary>
    public required RoomBlockPictureResolver BlockPictures { get; init; }

    /// <summary>Resolves what the room's library background shows on BG2, or null for a room without one (#1392).</summary>
    public required RoomBackgroundImageResolver? BackgroundImage { get; init; }

    /// <summary>Resolves what the room's scrolling sky shows on BG2, or null for a room without one (#1392).</summary>
    public required RoomBackgroundImageResolver? SkyImage { get; init; }

    /// <summary>Resolves what pictures written over BG2 show, or null in a tileset nothing writes pictures over (#1392, #1466).</summary>
    public required WrittenPictureResolver? Bg2OverlayPictures { get; init; }

    /// <summary>Resolves what pictures written over BG1 show, or null in a tileset nothing writes pictures over (#1392, #1466).</summary>
    public required WrittenPictureResolver? Bg1OverlayPictures { get; init; }

    /// <summary>Resolves what the HUD and the room effect's page show on BG3 (#1392, #1466).</summary>
    public required WrittenPictureResolver Bg3Pictures { get; init; }

    /// <summary>The images of the message boxes the room shows (#1392).</summary>
    public required MessageBoxSheet MessageBoxes { get; init; }

    /// <summary>
    /// Which artwork the blocks of <paramref name="room"/> show: the Ceres Ridley arena's setup points its BG1 and BG2 at
    /// the sprites' artwork (#1357); every other room, and the frames before a room is active, show the tileset's.
    /// </summary>
    internal static RoomBlockArtwork BlockArtworkOf(SelectedRoom? room) =>
        room?.State.SetupCallback == RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze
            ? RoomBlockArtwork.Sprites
            : RoomBlockArtwork.Tileset;

    /// <summary>Pictures the room's actors wrote over its BG1 (the Ceres escape warning), or null (#1346, #1466).</summary>
    public RoomWrittenOverlay? Bg1Overlay { get; private set; }

    /// <summary>Pictures the room's actors wrote over BG2 (the Zebes escape warning), or null (#1348, #1466).</summary>
    public RoomWrittenOverlay? Bg2Overlay { get; private set; }

    /// <summary>The bank-$88 scrolling-sky updater of a scrolling-sky room, or null.</summary>
    public required ScrollingSkyState? ScrollingSky { get; init; }

    /// <summary>The Mode 7 map a Ceres room shows, or null outside Ceres (#1349).</summary>
    public required FloorMap? FloorMap { get; init; }

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

    /// <summary>The room's Maridia elevatube room main, active only in the elevatube.</summary>
    public required MaridiaElevatubeRoomMainState MaridiaElevatube { get; init; }

    /// <summary>The room's Ceres elevator shaft room main, active only when the elevator door enters it.</summary>
    public required CeresElevatorShaftRoomMainState CeresElevatorShaft { get; init; }

    /// <summary>The room's enemy population and the species owners that run it.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>The room's PLM population and the family owners that run it.</summary>
    public required RoomPlmSystem Plms { get; init; }

    /// <summary>The room's runtime services, bound to its area, shared by its PLMs and enemies.</summary>
    public required RoomEnemyRuntimeServices Services { get; init; }

    /// <summary>The load-station record whose load established this room; null for a room entered otherwise.</summary>
    public LoadStationEntry? LoadStation { get; init; }

    /// <summary>
    /// The two native bank-$86 objects carrying Samus into fresh Ceres, once Ceres-start setup has begun them. They
    /// are projectiles of this room, so the next room's load ends them.
    /// </summary>
    public CeresElevatorArrivalState? CeresElevatorArrival { get; private set; }

    /// <summary>Writes <paramref name="picture"/> over this room's BG1 at its tilemap cell (#1346, #1466).</summary>
    internal void WriteBg1Picture(WrittenPicture picture) =>
        Bg1Overlay = (Bg1Overlay ?? RoomWrittenOverlay.Empty).With(picture);

    /// <summary>Writes <paramref name="picture"/> over this room's BG2 at its tilemap cell (#1348, #1466).</summary>
    internal void WriteBg2Picture(WrittenPicture picture) =>
        Bg2Overlay = (Bg2Overlay ?? RoomWrittenOverlay.Empty).With(picture);

    /// <summary>Begins the Ceres elevator arrival in this room.</summary>
    /// <exception cref="InvalidOperationException">This room's arrival has already begun.</exception>
    internal void BeginCeresElevatorArrival(CeresElevatorArrivalState arrival)
    {
        if (CeresElevatorArrival is not null)
            throw new InvalidOperationException("This room's Ceres elevator arrival has already begun.");
        CeresElevatorArrival = Ensure.NotNull(arrival);
    }
}
