using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Areas.Maridia;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;

namespace SuperMetroid.Core.World;

/// <summary>
/// One loaded room's state, built by <see cref="RoomScopeConstruction"/> in the game's load order and replaced
/// whole by the next load. Nothing in it survives into another room; values a destination reads from its source room
/// travel through <see cref="RoomTransitionCarryOver"/> instead (#1329, <c>docs/room-scope.md</c>).
/// </summary>
[RoomLifetime]
internal sealed record RoomScope
{
    /// <summary>The door whose destination this room is, or the load station's synthetic door.</summary>
    public required DoorDefinition Door { get; init; }

    /// <summary>The selected room and state definition.</summary>
    public required SelectedRoom Room { get; init; }

    /// <summary>The room's decompressed, read-only asset graph.</summary>
    public required LoadedRoomAssets Assets { get; init; }

    /// <summary>The room's foreground, block behaviours and background, shared by rendering and collision.</summary>
    public required RoomLevelData LevelData { get; init; }

    /// <summary>The room's foreground scroll-boundary camera.</summary>
    public required ScrollBoundaryCamera Camera { get; init; }

    /// <summary>
    /// The frame each of the room's animations shows (#1406): its animated tiles, statues, corpses and bosses select their
    /// pictures here, and the display resolves the pictures of the characters they paint from it.
    /// </summary>
    public required ArtworkFrameSelections ArtworkFrames { get; init; }

    /// <summary>
    /// What gameplay has shown over the room's load (#1466): the collectibles its slots show and Kraid's backdrop. With the
    /// frames its animations selected, the display names its blocks', cells' and written cells' pictures by it.
    /// </summary>
    public required RoomAppearance Appearance { get; init; }

    /// <summary>Resolves the pre-coloured picture each block the room's planes show draws as (#1392).</summary>
    public required RoomBlockPictureResolver BlockPictures { get; init; }

    /// <summary>
    /// What the room's background draws from its library background (#1392, #1649): its picture, or none, for a room that
    /// loads none or whose library background the player sees as something else.
    /// </summary>
    public required RoomLibraryBackgroundPicture BackgroundImage { get; init; }

    /// <summary>Resolves what the room's scrolling sky shows on the background, or null for a room without one (#1392).</summary>
    public required RoomBackgroundImageResolver? SkyImage { get; init; }

    /// <summary>Resolves what pictures written over the background show, or null in a tileset nothing writes pictures over (#1392, #1466).</summary>
    public required WrittenPictureResolver? BackgroundWrittenPictures { get; init; }

    /// <summary>Resolves what pictures written over the foreground show, or null in a tileset nothing writes pictures over (#1392, #1466).</summary>
    public required WrittenPictureResolver? ForegroundWrittenPictures { get; init; }

    /// <summary>Resolves what the HUD and the room effect's page show on the effect overlay (#1392, #1466).</summary>
    public required WrittenPictureResolver FxOverlayPictures { get; init; }

    /// <summary>The images of the message boxes the room shows (#1392).</summary>
    public required MessageBoxSheet MessageBoxes { get; init; }

    /// <summary>
    /// Which artwork the blocks of <paramref name="room"/> show: the Ceres Ridley arena's setup points its foreground and the background at
    /// the sprites' artwork (#1357); every other room, and the frames before a room is active, show the tileset's.
    /// </summary>
    internal static RoomBlockArtwork BlockArtworkOf(SelectedRoom? room) =>
        room?.State.SetupCallback == RoomSetupCallback.ShowSpritesOnCeresRidleyBackgroundsAndSpawnHaze
            ? RoomBlockArtwork.Sprites
            : RoomBlockArtwork.Tileset;

    /// <summary>The scrolling-sky updater of a scrolling-sky room, or null.</summary>
    public required ScrollingSkyState? ScrollingSky { get; init; }

    /// <summary>The turning floor the room shows, or null when it shows none (#1349, #1467, #1649).</summary>
    public required RoomFloor? Floor { get; init; }

    /// <summary>Door, room and library-background data of a Landing Site entry, or null.</summary>
    public required LandingSiteEntryState? LandingSiteEntry { get; init; }

    /// <summary>Samus's beam and missile projectiles in this room; the held charge lives on the session.</summary>
    public required SamusProjectileSystem Projectiles { get; init; }

    /// <summary>Samus's bombs, power bombs and the shared projectile cooldown in this room.</summary>
    public required SamusBombProjectileSystem BombProjectiles { get; init; }

    /// <summary>The room's effect-overlay FX: liquid, rain, fog and the effect overlay picture.</summary>
    public required RoomFxOverlayState FxOverlay { get; init; }

    /// <summary>The room's palette-FX objects.</summary>
    public required RoomPaletteFxSystem PaletteFx { get; init; }

    /// <summary>The room's Ceres haze, enabled only where the room's setup spawns it.</summary>
    public required CeresHazeState CeresHaze { get; init; }

    /// <summary>The room's sand animated tiles.</summary>
    public required RoomSandAnimatedTilesState SandTiles { get; init; }

    /// <summary>The room's horizontal-spike animated tiles.</summary>
    public required RoomSpikeAnimatedTilesState SpikeTiles { get; init; }

    /// <summary>The room's FX-selected treadmill animated tiles.</summary>
    public required RoomTreadmillAnimatedTilesState TreadmillTiles { get; init; }

    /// <summary>The Wrecked Ship entrance treadmill tiles a door's setup code may start.</summary>
    public required WreckedShipTreadmillAnimatedTilesState WreckedShipTreadmill { get; init; }

    /// <summary>The room's Tourian statue tile animations and descent, enabled only in the statue room.</summary>
    public required TourianStatueSequence TourianStatues { get; init; }

    /// <summary>The room's Maridia elevatube room main, active only in the elevatube.</summary>
    public required MaridiaElevatubeRoomMainState MaridiaElevatube { get; init; }

    /// <summary>The room's Ceres elevator shaft room main, active only when the elevator door enters it.</summary>
    public required CeresElevatorShaftRoomMainState CeresElevatorShaft { get; init; }

    /// <summary>The room's Ceres falling-debris room main, which runs only in the pre-elevator hall.</summary>
    public required CeresFallingDebrisRoomMainState CeresFallingDebris { get; init; }

    /// <summary>The room's escape diagonal-quake countdown, which the escape rooms' shaking mains keep.</summary>
    public required EscapeDiagonalQuakeState EscapeDiagonalQuake { get; init; }

    /// <summary>The room's enemy population and the species owners that run it.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>The room's room-object population and the family owners that run it.</summary>
    public required RoomObjectSystem RoomObjects { get; init; }

    /// <summary>The load-station record whose load established this room; null for a room entered otherwise.</summary>
    public LoadStation? LoadStation { get; init; }

    /// <summary>
    /// The two objects carrying Samus into fresh Ceres, once Ceres-start setup has begun them. They
    /// are projectiles of this room, so the next room's load ends them.
    /// </summary>
    public CeresElevatorArrivalState? CeresElevatorArrival { get; private set; }

    /// <summary>Begins the Ceres elevator arrival in this room.</summary>
    /// <exception cref="InvalidOperationException">This room's arrival has already begun.</exception>
    internal void BeginCeresElevatorArrival(CeresElevatorArrivalState arrival)
    {
        if (CeresElevatorArrival is not null)
            throw new InvalidOperationException("This room's Ceres elevator arrival has already begun.");
        CeresElevatorArrival = Ensure.NotNull(arrival);
    }

    /// <summary>
    /// Points every system of the room that makes sounds or music at <paramref name="requests"/>: its effect overlay, its
    /// colour effects, Samus's projectiles and bombs, its room objects and its enemies.
    /// </summary>
    internal void BindAudioRequests(GameplayAudioRequests requests)
    {
        FxOverlay.BindAudioRequests(requests);
        PaletteFx.BindAudioRequests(requests);
        Projectiles.BindAudioRequests(requests);
        BombProjectiles.BindAudioRequests(requests);
        RoomObjects.Sounds.BindAudioRequests(requests);
        Enemies.Audio.BindAudioRequests(requests);
    }
}
