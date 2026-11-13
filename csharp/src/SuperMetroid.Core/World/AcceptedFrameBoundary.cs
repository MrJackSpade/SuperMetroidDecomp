using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>
/// The frame boundary at the start of each update: the previous update's frame becomes visible, the controllers are
/// latched and the frame counters advance; the display captures are published here.
/// </summary>
internal sealed class AcceptedFrameBoundary
{
    /// <summary>Latches the controllers and advances the frame counters after publication.</summary>
    private readonly AcceptedFrameInput _input;
    /// <summary>Captures the foreground's and the background's scrolls visible with the frame boundary.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Retains the frame-boundary display latch.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Sprite frame made visible at each frame boundary.</summary>
    private readonly SpriteFrameBuffer _displayedSprites;
    /// <summary>How far the room's encounters have played their artwork, which the Ceres Ridley arena's blocks follow (#1466, #1513).</summary>
    private readonly EnemySession _enemySession;

    /// <summary>
    /// The loading or loaded room's enemy displays, whose bosses the publication latches; null before the gameplay
    /// runtime's first room, when no enemy is displayed.
    /// </summary>
    private IEnemyDisplaySources? Displays => _room.Enemies?.DisplaySources;
    /// <summary>The layers shown, latched with the accepted frame.</summary>
    private readonly GameplayScreenLayers _screenLayers;
    /// <summary>Sprite frame the update finalized for the next frame boundary.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Supplies Samus's per-frame graphics for the frame boundary.</summary>
    private readonly SamusFrameState _player;
    /// <summary>The HUD whose images each frame boundary latches.</summary>
    private readonly HudState _hud;
    /// <summary>
    /// The pictures of the HUD's images in the room's effect overlay sheet, looked up once per sheet (#1466): a lookup of the bound
    /// sheet's frames, remade from the sheet when absent, not game state.
    /// </summary>
    [NonSerialized]
    private HudPictures? _hudPictures;
    /// <summary>Supplies the loaded room, door, and scrolling sky whose scroll and display settings the accepted frame captures.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>Supplies the suit pickup whose colour is latched.</summary>
    private readonly SamusSuitPickupState _suitPickup;
    /// <summary>Builds what the X-ray reveals when a frame shows it.</summary>
    private readonly XrayReveal _xray;

    /// <summary>Connects the controller, display, sprite and frame-clock owners to the frame boundary.</summary>
    internal AcceptedFrameBoundary(AcceptedFrameBoundaryDependencies dependencies)
    {
        _input = dependencies.Input;
        _backgroundScroll = dependencies.BackgroundScroll;
        _display = dependencies.Display;
        _displayedSprites = dependencies.DisplayedSprites;
        _enemySession = dependencies.EnemySession;
        _screenLayers = dependencies.ScreenLayers;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _hud = dependencies.Hud;
        _suitPickup = dependencies.SuitPickup;
        _xray = dependencies.Xray;
    }

    /// <summary>
    /// Runs the frame boundary: publication, the controller latch, then the frame counters. Every update shows exactly
    /// one frame.
    /// </summary>
    /// <param name="controller1Input">The host's controller buttons.</param>
    public void BeginAcceptedFrame(ControllerButton controller1Input)
    {
        PublishAcceptedFrameTransfers();
        _input.Latch(controller1Input);
    }

    /// <summary>
    /// Publishes the display while a loading update keeps the screen dark. No frame is shown, so neither the
    /// controller latch nor any frame counter changes.
    /// </summary>
    internal void PublishWhileLoading() => PublishAcceptedFrameTransfers();

    /// <summary>Publishes the frame's sprites, Samus's graphics, queued artwork and the rotating-view state, in that order.</summary>
    private void PublishAcceptedFrameTransfers()
    {
        // Preserve this order: Samus's own frame graphics precede the general
        // artwork queue, and both precede controller latching. The finished sprite list is
        // published immediately before Samus's graphics, and the turning floor's rotation
        // after them. Retain both displayed values so rendering sees one coherent frame
        // instead of combining old sprites with a newer room-main rotation.
        _displayedSprites.LatchFrom(_sprites);
        _screenLayers.Latch();
        var displayScroll = new GameplayRenderSnapshot(
            _backgroundScroll.ForegroundPosition,
            _backgroundScroll.ForegroundScroll,
            _backgroundScroll.BackgroundScroll,
            // Before the first room no enemy earthquake shakes the room.
            _room.Enemies?.Earthquake.LastRoomShake ?? default);
        RoomFxOverlayRenderSnapshot? roomFxOverlay = _room.FxOverlay?.CaptureForDisplay();
        Displays?.Phantoon?.BodyWave.LatchDisplay();
        if (Displays?.Phantoon is { } phantoonDisplay)
            phantoonDisplay.BodyVisibility.LatchDisplay(phantoonDisplay.MosaicBlockSize);
        _room.TourianStatues?.LatchDisplay();
        GameplayLatchedScene scene = CaptureScene();
        XrayRevealedBlocks? reveal = CaptureXrayReveal(scene);
        _display.Latched = new GameplayDisplayLatch(
            displayScroll,
            _screenLayers.Displayed,
            CapturePerspectiveFloor(displayScroll.ForegroundScroll),
            roomFxOverlay,
            // Before the first room no eye beam is shown.
            _room.Enemies?.DisplaySources.MorphBallEyeBeam.LatchDisplay(),
            Displays?.Phantoon is { } phantoon ? PhantoonDisplaySnapshot.Capture(phantoon) : null,
            // Before the first room there are no statues, so no descent is displayed. The descent stops at its authored
            // distance, which holds the offset between minus that distance and zero (#1627).
            _room.TourianStatues?.DisplayedVerticalOffset ?? 0,
            scene,
            CaptureBossBackgrounds(displayScroll.ForegroundPosition),
            CaptureTints(),
            // The reveal and the room's planes are drawn once this frame boundary's pictures are bound (WithBlockPictures).
            null,
            CaptureRoomBackgrounds(),
            _room.Scope is null ? null : CaptureFxOverlayPlane(),
            // The HUD's images as gameplay last placed them, which this frame boundary shows.
            _hud.Shown());
        // The planes' blocks show the pictures named by what gameplay has shown so far (#1392, #1466).
        if (_display.Latched.Rooms is { } rooms)
            _display.Latched = WithBlockPictures(_display.Latched, rooms, reveal);
    }

    /// <summary>
    /// The room's part of the scene: the room's blending, and the sky's background map and vertical scroll.
    /// </summary>
    private GameplayLatchedScene CaptureScene()
    {
        var room = _room.ActiveRoom;
        return new(
            // Before the first room no FX record is selected and no effect blends.
            _room.FxOverlay?.Type ?? RoomFxType.None,
            _room.FxOverlay?.Blend,
            LatchedScrollingSky.Capture(_room.ScrollingSky),
            room?.Definition,
            // Before the first room no boss is loaded.
            _room.Enemies?.RoomStatus.Boss);
    }

    /// <summary>
    /// The Ceres perspective floor this frame shows: the floor Ceres Ridley's getaway flies over while he flies it (his
    /// room main computes it), with the chamber's walls around it (#1545), otherwise the elevator shaft's while the shaft has its transform. The
    /// shaft's transform exists exactly while the room was entered through the shaft's door.
    /// </summary>
    /// <param name="foregroundScroll">The foreground's scroll this frame shows, which scrolls the shaft's floor.</param>
    private PerspectiveFloor? CapturePerspectiveFloor(Position16<RoomPixels> foregroundScroll) =>
        Displays is { } displays && CeresRidleyGetaway.FloorTransform(displays.CeresRidley) is { } ridley
            ? CeresPerspectiveFloors.RidleyGetaway(ridley, displays.CeresGetawayChamberWalls)
        : _display.ActiveTurningFloor is { } shaft ? CeresPerspectiveFloors.ElevatorShaft(shaft, foregroundScroll)
        : null;

    /// <summary>
    /// The body a boss shows (#1352-#1356, #1466, #1617): its parts, and where Kraid's, Draygon's, Crocomire's and Mother
    /// Brain's lie in the room. Crocomire and Mother Brain place their bodies against the screen, so their bodies lie where
    /// <paramref name="camera"/>, the frame's, shows those places; Crocomire's body shows nothing while he is far from it.
    /// </summary>
    private GameplayLatchedBossBackgrounds CaptureBossBackgrounds(Position16<RoomPixels> camera)
    {
        var motherBrain = Displays?.MotherBrain;
        var crocomire = Displays?.CrocomireBody;
        return new(
            crocomire is { Shown: false } && Displays?.BossBody is not null ? BossBodyLayer.Empty : Displays?.BossBody,
            Displays?.KraidBodyOrigin,
            Displays?.Draygon is { } draygon
                ? new DraygonBodyPlacement(draygon.Body.Position.Whole, draygon.BodyOrigin, draygon.Body.Presence.HasAny(EnemyPresence.Deleted))
                : null,
            crocomire is { Placement: var shown } ? BossBodyPlacement.InRoom(shown, camera) : null,
            motherBrain is { PlacesBodyLayer: true } or { DeathHidesBodyLayer: true }
                ? new MotherBrainBodyPlacement(motherBrain.PlacesBodyLayer ? BossBodyPlacement.InRoom(motherBrain.BodyLayerPlacement, camera) : null,
                    motherBrain.DeathHidesBodyLayer)
                : null);
    }

    /// <summary>
    /// The room backgrounds this frame boundary displays (#1346, #1555): foreground from the level plane, background from the
    /// background plane when the room state shows its level background (otherwise the background shows what its owner places there), the source screen of a
    /// door's opening scroll, and the text shown over the foreground. The planes are shared until gameplay changes level data.
    /// </summary>
    private RoomBackgrounds? CaptureRoomBackgrounds()
    {
        // Before the first room no background is shown.
        if (_room.Scope is not { } scope)
            return null;
        RoomLevelData level = scope.LevelData;
        BackgroundScrollModes backgroundModes = _backgroundScroll.BackgroundScrollModes;
        // Each plane's pictures are resolved once this frame boundary's pictures are bound (WithBlockPictures).
        return new RoomBackgrounds(
            new RoomBackgroundLayer(level.ForegroundPlane.Cells, null),
            backgroundModes.Source == RoomBackgroundSource.Level
                ? new RoomBackgroundLayer(level.BackgroundPlane.Cells, null)
                : null,
            null,
            _room.DoorSourceBackground,
            // The text shown over the foreground and the background is drawn once this frame boundary's pictures are bound (WithBlockPictures).
            null,
            Floor: scope.Floor?.Publish(),
            BlockArtwork: RoomScope.BlockArtworkOf(_room.ActiveRoom));
    }

    /// <summary>
    /// <paramref name="latched"/> with <paramref name="rooms"/>' foreground and the background planes, and the X-ray reveal showing
    /// <paramref name="reveal"/>'s blocks (#1431), showing the pictures their blocks show for the values the room's
    /// appearance inputs take now (#1392, #1466); X-ray is never used while the blocks show the sprites' artwork.
    /// </summary>
    /// <exception cref="InvalidOperationException">X-ray reveals a room whose blocks show the sprites' artwork.</exception>
    private GameplayDisplayLatch WithBlockPictures(GameplayDisplayLatch latched, RoomBackgrounds rooms, XrayRevealedBlocks? reveal)
    {
        // While a Ceres room shows its turning floor, the floor draws its installed pictures (#1467); Ceres Ridley's getaway
        // shows its warning over the arena once his floor ends.
        RoomScope scope = _room.Scope ?? throw new InvalidOperationException("Room backgrounds were captured with no room loaded.");
        if (rooms.BlockArtwork != RoomBlockArtwork.Tileset && reveal is not null)
            throw new InvalidOperationException("X-ray reveals a room whose blocks show the sprites' artwork.");
        // The room's pictures are named by what gameplay showed over its load, the frames its animations selected and, in
        // the Ceres Ridley arena, the looks shown on the sprites' artwork (#1466).
        var appearance = new RoomAppearanceView(scope.Appearance, scope.ArtworkFrames, _enemySession.Looks);
        // The planes the latch shows are the level data's, which nothing changes before this frame boundary's pictures resolve.
        // The elevator shaft's screen is its turning floor alone: it draws none of its level's blocks (#1647).
        RoomBlockPictureTable pictures = RoomTurningFloors.ShowsShaftFloor(scope.Room.State.MainCallback)
            ? RoomBlockPictureTable.NoneDrawn()
            : scope.BlockPictures.Resolve(scope.LevelData.ForegroundPlane,
                rooms.Background is null ? null : scope.LevelData.BackgroundPlane, rooms.BlockArtwork, appearance,
                reveal?.Blocks.OfType<RevealedBlock>().Select(block => block.Definition) ?? []);
        // A sky room's background shows the sky's rows, drawn from their image (#1392, #1403), once the room has configured the
        // sky's bands; without them, background shows the room's own background.
        SkyPictures? sky = null;
        if (scope.ScrollingSky is { HasBands: true })
        {
            RoomBackgroundImageResolver skyImage = scope.SkyImage
                ?? throw new InvalidDataException($"{scope.Room.Definition.Name} shows a scrolling sky with no installed sky image.");
            sky = new SkyPictures(skyImage.Resolve(appearance, RoomBackgroundImagePlacement.Whole(skyImage.Image)));
        }
        // Where background shows the room's library background as its own picture, it draws the background's image (#1392,
        // #1649): not over a streaming background plane, the sky's rows or a boss body that owns the background. A library
        // background the player sees as something else draws no picture.
        RoomBackgroundImagePlane? image = null;
        if (rooms.Background is null && sky is null && latched.Bosses.Body is null
            && scope.BackgroundImage is RoomLibraryBackgroundPicture.Drawn { Image: var background })
        {
            // The image is one period of what the background shows, as its room shows it, repeated across and down (#1651, #1617).
            image = background.Resolve(appearance, RoomBackgroundImagePlacement.Whole(background.Image));
        }
        // A boss body shows each part's whole picture (#1466, #1513).
        ShownBossBody? shownBody = latched.Bosses.Body?.Show();
        // The text an escape shows over a layer, as its sequence describes it (#1555): Zebes's warning over the background, Ceres's
        // over the foreground, each drawn from the written picture sheet's frames for the inputs they depend on (#1466).
        RoomWrittenOverlay zebesWarning = scope.Enemies.DisplaySources.ZebesWarning;
        RoomOverlayPictures? backgroundText = zebesWarning.Pictures.IsEmpty ? null
            : new RoomOverlayPictures((scope.BackgroundWrittenPictures ?? throw new InvalidDataException(
                $"{scope.Room.Definition.Name} shows text over the background with no installed written picture sheet.")).Resolve(zebesWarning, appearance));
        RoomWrittenOverlay ceresWarning = scope.Enemies.DisplaySources.CeresWarning;
        RoomOverlayPictures? foregroundText = ceresWarning.Pictures.IsEmpty ? null
            : new RoomOverlayPictures((scope.ForegroundWrittenPictures ?? throw new InvalidDataException(
                $"{scope.Room.Definition.Name} shows text over the foreground with no installed written picture sheet.")).Resolve(ceresWarning, appearance));
        return latched with
        {
            Bosses = latched.Bosses with { ShownBody = shownBody },
            Rooms = rooms with
            {
                Foreground = rooms.Foreground with { Pictures = pictures },
                Background = rooms.Background is { } shownBackground ? shownBackground with { Pictures = pictures } : null,
                BackgroundImage = image,
                Sky = sky,
                BackgroundText = backgroundText,
                ForegroundText = foregroundText,
                // The room effect's page draws from the effect overlay sheet's pictures, as the effects' animations select them; the
                // plane holds nothing else (#1466).
                FxOverlay = scope.FxOverlayPictures.Resolve(latched.FxOverlay ?? throw new InvalidOperationException("A room's frame boundary latches its effect overlay plane."),
                    appearance),
            },
            // The HUD's images draw from the same sheet, on the HUD's own layer (#1466).
            PicturedHud = HudPicturesIn(scope.FxOverlayPictures.Sheet.Sheet).Pictured(latched.Hud ?? ShownHud.Nothing),
            XrayReveal = reveal is { } revealed ? new XrayRevealPlane(revealed.Origin, revealed.Blocks.AsSpan(), pictures) : null,
        };
    }

    /// <summary>The effect overlay plane this frame boundary displays (#1348): the room effect's page; the HUD is its own layer (#1466).</summary>
    internal FxOverlayPlane CaptureFxOverlayPlane() => _room.FxOverlay?.EffectPlane ?? FxOverlayPlane.Cleared;

    /// <summary>The pictures of the HUD's images in <paramref name="sheet"/>, the room's effect overlay sheet.</summary>
    private HudPictures HudPicturesIn(SpriteSheet sheet) =>
        _hudPictures is { } shown && ReferenceEquals(shown.Sheet, sheet) ? shown : _hudPictures = new HudPictures(sheet);

    /// <summary>The tints effects chose during the previous main loop, each from its own effect.</summary>
    private GameplayLatchedTints CaptureTints()
    {
        // Before the first room exists there is no explosion; its colour is black.
        RgbColor powerBomb = _room.BombProjectiles?.PowerBombExplosion.GlowColor ?? RgbColor.Black;
        return new(
            // Before the first room there is no room effect, so no Fireflea darkness.
            _room.FxOverlay?.FirefleaTint,
            powerBomb,
            _suitPickup.BeamColor,
            Displays?.MotherBrain?.RainbowBeam.Color);
    }

    /// <summary>
    /// What the X-ray reveals in the frame this boundary shows (#1431, #1514), built from the room as it is now, while
    /// X-ray is active in a room it reveals blocks in; otherwise null, and the background is the room's own.
    /// </summary>
    private XrayRevealedBlocks? CaptureXrayReveal(GameplayLatchedScene scene) =>
        // Where X-ray preserves the backgrounds it reveals no blocks: the tiles are another room's.
        _player.Samus?.Xray.IsActive == true && scene.Room is { } room && XrayBlockRevealRules.RevealsBlocks(room, scene.Boss)
            ? _xray.Reveal((_room.Scope ?? throw new InvalidOperationException("The X-ray reveals no loaded room.")).RoomObjects)
            : null;
}
