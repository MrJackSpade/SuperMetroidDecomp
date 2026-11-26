using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.World;

/// <summary>
/// One loaded room's state, built by <see cref="RoomScopeConstruction"/> in the game's load order and replaced
/// whole by the next load. Nothing in it survives into another room; values a destination reads from its source room
/// travel through <see cref="RoomTransitionCarryOver"/> instead (#1329).
/// </summary>
internal sealed partial record RoomScope
{
    /// <summary>The door whose destination this room is, or the load station's synthetic door.</summary>
    public required DoorDefinition Door { get; init; }

    /// <summary>The selected room and state definition.</summary>
    public required SelectedRoom Room { get; init; }

    /// <summary>The room's loaded, read-only asset graph.</summary>
    public required LoadedRoomAssets Assets { get; init; }

    /// <summary>The room's foreground, block behaviours and background, shared by rendering and collision.</summary>
    public required RoomLevelData LevelData { get; init; }

    /// <summary>The room's terrain Samus meets (#2553): its level, whose reacting blocks reach its room objects.</summary>
    internal RoomTerrain SamusTerrain => new(LevelData, RoomObjects);

    /// <summary>The room's foreground scroll-boundary camera.</summary>
    public required ScrollBoundaryCamera Camera { get; init; }

    /// <summary>
    /// The frame each of the room's animations shows (#1406): its block animations, statues, corpses and bosses select their
    /// pictures here, and the display resolves the pictures they show from it.
    /// </summary>
    public required ArtworkFrameSelections ArtworkFrames { get; init; }

    /// <summary>
    /// What gameplay has shown over the room's load (#1466): the collectibles its room objects show and Kraid's backdrop. With the
    /// frames its animations selected, the display names its blocks', cells' and written cells' pictures by it.
    /// </summary>
    public required RoomAppearance Appearance { get; init; }

    /// <summary>
    /// The look each of the room's encounters shows (#1538, #2160), which the frames its looks change and the Ceres Ridley
    /// arena's backgrounds follow. The room's enemies show their looks into it; the next room's load builds its own.
    /// </summary>
    public required EncounterLooks Looks { get; init; }

    /// <summary>Resolves the pre-coloured picture each block the room's planes show draws as (#1392).</summary>
    public required RoomBlockPictureResolver BlockPictures { get; init; }

    /// <summary>
    /// What the room's background draws from its library background (#1392, #1649): its picture, or none, for a room that
    /// loads none or whose library background the player sees as something else.
    /// </summary>
    public required RoomLibraryBackgroundPicture BackgroundImage { get; init; }

    /// <summary>
    /// The warnings the room's escapes write over its layers, or none in a room whose tileset has no warning picture sheet
    /// (#1392, #1466, #2554): Zebes's warning over the background and Ceres's over the foreground, which no room shows both of.
    /// </summary>
    public required RoomWarnings? Warnings { get; init; }

    /// <summary>Resolves what the room effect's liquid or atmosphere shows on the effect overlay (#1392, #1466, #1706).</summary>
    public required WrittenPictureResolver FxOverlayPictures { get; init; }

    /// <summary>The pictures of the HUD's images in the room's tileset's colours, which the HUD's own layer draws (#1466, #1731).</summary>
    public required HudPictureSheet HudPictures { get; init; }

    /// <summary>The images of the message boxes the room shows (#1392).</summary>
    public required MessageBoxSheet MessageBoxes { get; init; }

    /// <summary>The scrolling sky of a sky room, its state with its image (#2513), or null for a room without one.</summary>
    public required RoomSky? Sky { get; init; }

    /// <summary>The turning floor the room shows, or null when it shows none (#1349, #1467, #1649).</summary>
    public required RoomFloor? Floor { get; init; }

    /// <summary>Samus's beam and missile projectiles in this room; the held charge lives on the session.</summary>
    public required SamusProjectileSystem Projectiles { get; init; }

    /// <summary>Samus's bombs, power bombs and the shared projectile cooldown in this room.</summary>
    public required SamusBombProjectileSystem BombProjectiles { get; init; }

    /// <summary>The room's effect-overlay FX: liquid, rain, fog and the effect overlay picture.</summary>
    public required RoomFxOverlayState FxOverlay { get; init; }

    /// <summary>
    /// The ground Samus's running footsteps and landings go by in this room (#2416), by its area and its own policy; the
    /// room sets it as it loads, beside the liquid its effect gives her physics.
    /// </summary>
    public required RoomGround SamusGround { get; init; }

    /// <summary>The room's colour effects.</summary>
    public required ColourEffectSystem ColourEffects { get; init; }

    /// <summary>The room's Ceres haze, or null for a room whose setup spawns none (#2287).</summary>
    public required CeresHazeState? CeresHaze { get; init; }

    /// <summary>The room's block animations: those its effect lists and the Wrecked Ship entrance treadmill a door's setup may start.</summary>
    public required RoomBlockAnimations BlockAnimations { internal get; init; }

    /// <summary>The room's Tourian statue animations and descent, or null for a room whose setup does not unlock the statues (#2287).</summary>
    public required TourianStatueSequence? TourianStatues { get; init; }

    /// <summary>
    /// The room's main behaviour, composed from the main its state names with the state it keeps, or null for a room whose
    /// state runs none (#2287).
    /// </summary>
    public required IRoomMain? Main { get; init; }

    /// <summary>The room's enemy population and the species owners that run it.</summary>
    public required RoomEnemySystem Enemies { get; init; }

    /// <summary>The room's room-object population and the family owners that run it.</summary>
    public required RoomObjectSystem RoomObjects { get; init; }

    /// <summary>The load station whose load established this room; null for a room entered otherwise.</summary>
    public LoadStation? LoadStation { get; init; }

    /// <summary>
    /// The two objects carrying Samus into fresh Ceres, built with the Ceres start's room (#2553), so it begins once; the
    /// arrival belongs to this room and ends with the next room's load. Null for every other room.
    /// </summary>
    public CeresElevatorArrivalState? CeresElevatorArrival { get; init; }

    /// <summary>
    /// The block animations' step: gameplay, the door transition's scroll and every fade step take it. The X-Ray Scope's
    /// setup disables the block animations, freezing every animation until the scope ends.
    /// </summary>
    /// <param name="samus">Samus, whose X-Ray Scope holds the animations still.</param>
    /// <param name="session">The session's shared game state, whose defeated bosses the treadmills wait for.</param>
    internal void StepBlockAnimations(SamusState samus, SharedGameState session)
    {
        if (Ensure.NotNull(samus).Xray.AreBlockAnimationsSuspended)
            return;
        // The treadmills, the effect's and the one a Wrecked Ship entrance door starts, show a frame a pass only after
        // Phantoon is defeated.
        BlockAnimations.Step(Ensure.NotNull(session).HasDefeatedAnyOf(Room.Area, DefeatedBosses.AreaBoss), ArtworkFrames);
        TourianStatues?.StepBlockAnimations();
    }

    /// <summary>
    /// Runs the cartridge's animated tiles handler once during a door's transition (<c>DoorTransitionFunction_HandleAnimatedTiles</c>
    /// and each update of the fade in): the room's block animations and the room effect's own animation, which the
    /// cartridge holds as animated tiles objects alike (#3037), so lava or acid shows its animated pictures from the fade
    /// in's first frame.
    /// </summary>
    internal void StepAnimatedTilesDuringDoor(SamusState samus, SharedGameState session)
    {
        StepBlockAnimations(samus, session);
        FxOverlay.StepAnimation(ArtworkFrames);
    }

    /// <summary>
    /// Runs the cartridge's HDMA objects of the room's effect for one update of a door's transition (#3047): its overlay
    /// placed against the camera and moved on, where the cartridge's HDMA handler runs during the door.
    /// </summary>
    internal void StepEffectDisplayDuringDoor(bool timeIsFrozen) => FxOverlay.StepDisplayDuringDoor(Camera.PixelPosition, timeIsFrozen);

    /// <summary>
    /// The room's gameplay shading (#1512): each owner of a colour effect names the shades and effects its state shows,
    /// each later one showing over the earlier: the room's colour effects and the effect overlay colours, then its
    /// bosses', then <paramref name="shownOver"/>, the display's fades, over all of them.
    /// </summary>
    /// <param name="shownOver">The owners of the fades of the whole display, in the order they are laid.</param>
    internal GameplayShading Shading(IEnumerable<IGameplayShadingSource> shownOver)
    {
        var shading = new GameplayShadingBuilder();
        foreach (IGameplayShadingSource source in ShadingSources.Concat(Ensure.NotNull(shownOver)))
            source.ShadeGameplay(shading);
        return shading.Build(Assets.BlockSheet.Sheet.Image, Assets.BlockSheet.Backdrop);
    }

    /// <summary>The owners of the colour effects the room shows, in the order they are laid over each other.</summary>
    private IEnumerable<IGameplayShadingSource> ShadingSources
    {
        get
        {
            yield return ColourEffects;
            yield return FxOverlay;
            foreach (IGameplayShadingSource boss in Enemies.DisplaySources.ShadingSources)
                yield return boss;
        }
    }

    /// <summary>The room's actors that stand in front of Samus (#2571): the Ceres elevator arrival once it has begun.</summary>
    internal IEnumerable<IActorInFrontOfSamus> ActorsInFrontOfSamus => CeresElevatorArrival is { } arrival ? [arrival] : [];

    /// <summary>
    /// The displayed states the accepted frame's boundary latches (#2571): the enemies' and, in the statue room, the
    /// statues' descent.
    /// </summary>
    internal IEnumerable<IDisplayLatch> DisplayLatches =>
        TourianStatues is { } statues ? [.. Enemies.DisplaySources.DisplayLatches, statues] : Enemies.DisplaySources.DisplayLatches;

}
