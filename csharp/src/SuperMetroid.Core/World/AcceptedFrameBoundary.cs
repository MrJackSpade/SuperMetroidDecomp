using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Ridley;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Gameplay;

namespace SuperMetroid.Core.World;

/// <summary>
/// The NMI: controller latching, frame-counter advance, accepted and forced-blank transfers, hardware-wait NMIs,
/// and the display captures published at the NMI.
/// </summary>
internal sealed class AcceptedFrameBoundary
{
    /// <summary>Latches the controllers and advances the frame counters after publication.</summary>
    private readonly AcceptedFrameInput _input;
    /// <summary>Publishes rebound HUD, beam, and trail art after accepted VRAM transfers.</summary>
    private readonly RuntimeArtwork _artwork;
    /// <summary>Captures the BG1 and BG2 scroll registers visible with the accepted NMI.</summary>
    private readonly BackgroundScrollState _backgroundScroll;
    /// <summary>Retains the accepted-NMI display latch.</summary>
    private readonly GameplayDisplayState _display;
    /// <summary>Sprite frame made visible at each accepted NMI.</summary>
    private readonly SpriteFrameBuffer _displayedSprites;
    /// <summary>The session's OBJ character table, which the displayed gameplay object page binds.</summary>
    private readonly EnemySession _enemySession;

    /// <summary>
    /// The loading or loaded room's enemy displays, whose bosses the publication latches; null before the gameplay
    /// runtime's first room, when no enemy is displayed.
    /// </summary>
    private IEnemyDisplaySources? Displays => _room.Enemies?.DisplaySources;
    /// <summary>The main-screen layers latched with the accepted frame.</summary>
    private readonly GameplayScreenLayers _screenLayers;
    /// <summary>Sprite frame the main loop finalized for the next NMI.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Supplies Samus graphics DMA for accepted NMI transfers.</summary>
    private readonly SamusFrameState _player;
    /// <summary>Supplies the room's fixed colour.</summary>
    private readonly RoomFxSession _roomFx;
    /// <summary>The HUD whose rows the BG3 plane shows.</summary>
    private readonly HudState _hud;
    /// <summary>Supplies the loaded room, door, and scrolling sky whose register shadows are latched.</summary>
    private readonly ActiveRoomState _room;

    /// <summary>The background character bindings the room's block pictures are resolved over (#1392).</summary>
    private readonly BgArtworkBindings _backgrounds;
    /// <summary>Supplies the suit pickup whose colour is latched.</summary>
    private readonly SamusSuitPickupState _suitPickup;
    /// <summary>Supplies the frozen X-ray reveal map the publication latches.</summary>
    private readonly XrayReveal _xray;

    /// <summary>Connects controller, PPU, sprite, video-queue, and frame-clock owners to the NMI boundary.</summary>
    internal AcceptedFrameBoundary(AcceptedFrameBoundaryDependencies dependencies)
    {
        _input = dependencies.Input;
        _artwork = dependencies.Artwork;
        _backgroundScroll = dependencies.BackgroundScroll;
        _display = dependencies.Display;
        _displayedSprites = dependencies.DisplayedSprites;
        _enemySession = dependencies.EnemySession;
        _screenLayers = dependencies.ScreenLayers;
        _sprites = dependencies.Sprites;
        _player = dependencies.Player;
        _room = dependencies.Room;
        _backgrounds = dependencies.Backgrounds;
        _roomFx = dependencies.RoomFx;
        _hud = dependencies.Hud;
        _suitPickup = dependencies.SuitPickup;
        _xray = dependencies.Xray;
    }

    /// <summary>
    /// Monotonic host sequence advanced only after a complete state-eight gameplay owner
    /// pass has published its per-frame audio requests.
    /// </summary>
    /// <remarks>
    /// Door transitions and other frontend coroutines can run accepted NMIs without
    /// running enemy, projectile, or PLM owners. Consumers must distinguish those waits
    /// from a fresh gameplay publication; otherwise the last request is re-enqueued on
    /// every transition NMI and the native sound-queue drain can never finish.
    /// </remarks>
    public ulong CompletedGameplayAudioPublication { get; internal set; }

    /// <summary>
    /// Runs the accepted NMI portion currently translated from <c>$80:9583</c>: publication, the controller latch, then
    /// the frame counters. The port has no lagged NMIs; hardware lag is eliminated.
    /// </summary>
    /// <param name="controller1Input">Host-provided raw SNES controller word.</param>
    public void BeginAcceptedFrame(ControllerButton controller1Input)
    {
        PublishAcceptedFrameTransfers();
        _input.Latch(controller1Input);
    }

    /// <summary>
    /// Applies queued graphics while a loading dispatch holds forced blank with NMI
    /// disabled. Native loaders write these directly; no NMI runs, so neither the
    /// controller latch nor any NMI counter changes.
    /// </summary>
    internal void PublishForcedBlankTransfers() => PublishAcceptedFrameTransfers();

    /// <summary>The OAM, Samus, video-queue and register publication of $80:959E..$80:95DE.</summary>
    private void PublishAcceptedFrameTransfers()
    {
        // Preserve $80:95A1 -> $80:95D0 -> $80:95E1 order: dedicated Samus graphics
        // DMA precedes the general video queue, and both precede controller latching.
        // $80:959E uploads the finalized main-loop OAM image immediately before that
        // Samus DMA. `$80:95A7` subsequently copies the Mode 7 shadow words to
        // $211B..$2120. Retain both displayed values so software rendering sees one
        // coherent PPU phase instead of combining old OAM with a newer room-main matrix.
        _displayedSprites.LatchFrom(_sprites);
        _screenLayers.Latch();
        var displayScroll = new GameplayRenderSnapshot(
            _backgroundScroll.Layer1Position,
            _backgroundScroll.Bg1Scroll,
            _backgroundScroll.Bg2Scroll,
            // Before the first room no enemy earthquake shakes the room.
            _room.Enemies?.Earthquake.LastRoomShake ?? default);
        RoomLayer3FxRenderSnapshot? roomLayer3Fx = _room.Layer3Fx?.CaptureForDisplay();
        Displays?.Phantoon?.Wave.LatchDisplay();
        if (Displays?.Phantoon is { } phantoonDisplay)
            phantoonDisplay.Blending.LatchDisplay(phantoonDisplay.MosaicBlockSize);
        _room.TourianStatues?.LatchDisplay();
        GameplayLatchedScene scene = CaptureScene();
        ImmutableArray<RevealedBlock?>? reveal = CaptureXrayReveal(scene);
        _display.Latched = new GameplayDisplayLatch(
            displayScroll,
            _screenLayers.Displayed,
            CapturePerspectiveFloor(displayScroll.Bg1Scroll),
            roomLayer3Fx,
            // Before the first room no eye beam is shown.
            _room.Enemies?.DisplaySources.MorphBallEyeBeam.LatchDisplay(),
            Displays?.Phantoon is { } phantoon ? PhantoonDisplaySnapshot.Capture(phantoon) : null,
            // Before the first room there are no statues, so no descent is displayed.
            checked((short)(_room.TourianStatues?.DisplayedVerticalOffset ?? 0)),
            scene,
            CaptureBossBackgrounds(),
            CaptureFixedColors(),
            // The reveal and the room's planes are drawn once this NMI's uploads have bound (WithBlockPictures).
            null,
            CaptureRoomBackgrounds(),
            _room.Scope is null ? null : CaptureLayer3Plane());
        _artwork.PublishReboundHudArtwork(_backgrounds);
        // The planes' blocks show what this NMI's pictures bound, as the characters they read did (#1392).
        if (_display.Latched.Rooms is { } rooms)
            _display.Latched = WithBlockPictures(_display.Latched, rooms, reveal);
        // A rebound snapshot may retain old CGRAM; apply current content only at this accepted NMI.
        _artwork.PublishReboundBeamPalette();
    }

    /// <summary>
    /// The room's part of the scene: the characters layer 3 reads, the room's blending (<c>$88:8000</c>), and the
    /// sky's layer-2 map and vertical scroll (<c>$88:AFB2</c>).
    /// </summary>
    private GameplayLatchedScene CaptureScene()
    {
        var room = _room.ActiveRoom;
        return new(
            _display.Layer3Characters,
            // Before the first room no FX record is selected and no blending is configured.
            _room.Layer3Fx?.Type ?? RoomFxType.None,
            _room.Layer3Fx?.LayerBlendConfiguration ?? LayerBlendingConfiguration.Unconfigured,
            LatchedScrollingSky.Capture(_room.ScrollingSky),
            room?.Definition,
            // Before the first room no boss is loaded.
            _room.Enemies?.RoomStatus.BossId ?? 0);
    }

    /// <summary>
    /// The Ceres perspective floor this frame shows: the floor Ceres Ridley's getaway flies over while he flies it (his
    /// room main <c>$A6:AAAF</c> computes it), otherwise the elevator shaft's while the shaft has its transform. The
    /// shaft's transform exists exactly while the room was entered through the shaft's door.
    /// </summary>
    /// <param name="layer1Scroll">Layer 1's scroll this frame shows, which scrolls the shaft's floor.</param>
    private PerspectiveFloor? CapturePerspectiveFloor(Position16<RoomPixels> layer1Scroll) =>
        CeresRidleyGetaway.FloorTransform(Displays?.CeresRidley) is { } ridley ? CeresPerspectiveFloors.RidleyGetaway(ridley)
        : _display.ActiveTurningFloor is { } shaft ? CeresPerspectiveFloors.ElevatorShaft(shaft, layer1Scroll)
        : null;

    /// <summary>
    /// The bodies bosses draw as layer 2: Kraid's body (#1352), the enemy layer-2 image, Draygon's and Mother Brain's
    /// placements, and Crocomire's scroll.
    /// </summary>
    private GameplayLatchedBossBackgrounds CaptureBossBackgrounds()
    {
        var motherBrain = Displays?.MotherBrain;
        return new(
            Displays?.KraidBody,
            Displays?.EnemyBg2,
            Displays?.Draygon is { } draygon
                ? new DraygonBodyPlacement(draygon.Body.Position.Whole, draygon.BodyBg2Scroll, draygon.Body.Properties.HasAny(EnemyProperties.Deleted))
                : null,
            Displays?.CrocomireBg2Scroll,
            motherBrain is { HasBg2ScrollOverride: true } or { DeathBg2Hidden: true }
                ? new MotherBrainBodyPlacement(motherBrain.HasBg2ScrollOverride ? motherBrain.Bg2Scroll : null, motherBrain.DeathBg2Hidden)
                : null);
    }

    /// <summary>
    /// The room backgrounds this NMI displays (#1346): BG1 from the level plane through the BG1 offset, BG2 from the
    /// layer-2 plane when that layer streams (otherwise BG2 shows the tilemap its owner loaded), the source screen of a
    /// door's opening scroll, and the room's BG1 overlay. The planes are shared until gameplay changes level data.
    /// </summary>
    private RoomBackgrounds? CaptureRoomBackgrounds()
    {
        // Before the first room no background is shown.
        if (_room.Scope is not { } scope)
            return null;
        RoomLevelData level = scope.LevelData;
        Layer2ScrollModes layer2 = _backgroundScroll.Layer2Scroll;
        // Each plane's pictures are resolved once this NMI's uploads have bound (WithBlockPictures).
        return new RoomBackgrounds(
            new RoomBackgroundLayer(level.ForegroundPlane.Cells, _backgroundScroll.Bg1Offset.AsBgPlaneOffset(), null),
            layer2.Horizontal.Streams || layer2.Vertical.Streams
                ? new RoomBackgroundLayer(level.BackgroundPlane.Cells, _backgroundScroll.Bg2Offset.AsBgPlaneOffset(), null)
                : null,
            null,
            _room.DoorSourceBackground,
            // What actors wrote over BG1 and BG2 is drawn once this NMI's uploads have bound (WithBlockPictures).
            null,
            Floor: scope.FloorMap?.Publish(),
            Characters: RoomScope.CharactersOf(_room.ActiveRoom));
    }

    /// <summary>
    /// <paramref name="latched"/> with <paramref name="rooms"/>' BG1 and BG2 planes, and the X-ray reveal showing
    /// <paramref name="reveal"/>'s blocks (#1431), showing the pictures their blocks show over what the bindings show now (#1392);
    /// unchanged while the planes draw from the object character page, where X-ray is never used.
    /// </summary>
    /// <exception cref="InvalidOperationException">X-ray reveals a room whose planes draw from the object character page.</exception>
    private GameplayDisplayLatch WithBlockPictures(GameplayDisplayLatch latched, RoomBackgrounds rooms, ImmutableArray<RevealedBlock?>? reveal)
    {
        // While a Ceres room shows its Mode 7 map, the map draws its tiles' pictures, which its characters must still show
        // (#1392); Ceres Ridley's getaway writes its warning's and doors' characters over them once his Mode 7 ends.
        RoomScope scope = _room.Scope ?? throw new InvalidOperationException("Room backgrounds were captured with no room loaded.");
        if (latched.ShowsPerspectiveFloor && scope.FloorMap is { } floorMap)
            floorMap.Tiles.AssertShownBy(_backgrounds, floorMap.NamedTiles);
        if (rooms.Characters != RoomBackgroundCharacters.Tileset && reveal is not null)
            throw new InvalidOperationException("X-ray reveals a room whose planes draw from the object character page.");
        // The Ceres Ridley arena's backgrounds read the OBJ character table's uploads (#1392).
        RoomCharacterSource characters = RoomCharacterSource.Of(rooms.Characters, _backgrounds, _enemySession,
            scope.AnimatedCharacters, scope.ArtworkFrames);
        // A background image's and the sky's tiles read the room tileset's characters, which its animations paint too (#1406).
        var animatedBlocks = new AnimatedCharacterView(scope.AnimatedCharacters.Blocks, scope.ArtworkFrames);
        // The planes the latch shows are the level data's, which nothing changes before this NMI's pictures resolve.
        RoomBlockPictureTable pictures = scope.BlockPictures.Resolve(scope.LevelData.ForegroundPlane,
            rooms.Bg2 is null ? null : scope.LevelData.BackgroundPlane, characters,
            reveal?.OfType<RevealedBlock>().Select(block => block.Definition) ?? []);
        // A sky room's BG2 shows the sky's rows, drawn from their image (#1392, #1403), once the room has configured the
        // sky's band HDMA; without it, BG2 shows the room's own background.
        SkyPictures? sky = null;
        if (scope.ScrollingSky is { HasBands: true } scrollingSky)
        {
            RoomBackgroundImageResolver skyImage = scope.SkyImage
                ?? throw new InvalidDataException($"{scope.Room.Definition.Name} shows a scrolling sky with no installed sky image.");
            SkyTilemap rows = (_artwork.RoomSkyTilemapArt ?? throw new InvalidOperationException("A sky room has no installed sky rows."))
                .Tilemap(scrollingSky.Variant);
            skyImage.Image.AssertPictures(rows.Tiles);
            sky = new SkyPictures(skyImage.Resolve(_backgrounds, animatedBlocks), rows);
        }
        // Where BG2 shows the tilemap its library background loaded, it draws the background's image (#1392): not over a
        // streaming layer-2 plane, the sky's rows or a boss body that owns BG2.
        RoomBackgroundImagePlane? image = null;
        if (rooms.Bg2 is null && sky is null && latched.Bosses.KraidBody is null && latched.Bosses.EnemyBg2 is null
            && scope.BackgroundImage is { } background)
        {
            image = background.Resolve(_backgrounds, animatedBlocks);
        }
        // A boss body that owns BG2 draws each tile from its body sheet's picture over what the tile's character shows.
        ImmutableArray<BackgroundTile>? body = latched.Bosses.KraidBody?.Tilemap.Tiles ?? latched.Bosses.EnemyBg2?.Tiles;
        ImmutableArray<BgTilePicture?> bodyPictures = body is { } bodyWords
            ? (scope.BodyPictures ?? throw new InvalidDataException($"{scope.Room.Definition.Name} shows a boss body with no installed tile word sheet."))
                .Resolve(bodyWords, characters)
            : default;
        // Words written over BG2 draw from the sheet's pictures the same way.
        RoomOverlayPictures? bg2Overlay = scope.Bg2Overlay is { } overlay
            ? new RoomOverlayPictures((scope.Bg2OverlayPictures ?? throw new InvalidDataException(
                $"{scope.Room.Definition.Name} writes over BG2 with no installed tile word sheet.")).Resolve(overlay, characters))
            : null;
        // Words written over BG1, as the Ceres escape's warning, draw from the sheet's pictures the same way.
        RoomOverlayPictures? bg1Overlay = scope.Bg1Overlay is { } written
            ? new RoomOverlayPictures((scope.Bg1OverlayPictures ?? throw new InvalidDataException(
                $"{scope.Room.Definition.Name} writes over BG1 with no installed tile word sheet.")).Resolve(written, characters))
            : null;
        return latched with
        {
            Bosses = latched.Bosses with { BodyPictures = bodyPictures },
            Rooms = rooms with
            {
                Bg1 = rooms.Bg1 with { Pictures = pictures },
                Bg2 = rooms.Bg2 is { } bg2 ? bg2 with { Pictures = pictures } : null,
                Bg2Image = image,
                Sky = sky,
                Bg2Overlay = bg2Overlay,
                Bg1Overlay = bg1Overlay,
                // The HUD and the room effect's page draw from the BG3 sheet's pictures over the characters BG3 reads.
                Bg3 = new Layer3Cells(scope.Bg3Pictures.Resolve(latched.Layer3 ?? throw new InvalidOperationException("A room's NMI latches its BG3 words."),
                    _backgrounds, latched.Scene.Layer3Characters, new AnimatedCharacterView(scope.AnimatedCharacters.Bg3, scope.ArtworkFrames))),
            },
            XrayReveal = reveal is null ? null : new XrayRevealPlane(reveal.Value.AsSpan(), pictures),
        };
    }

    /// <summary>
    /// The BG3 plane this NMI displays (#1348): the HUD's static top row and its three rows as gameplay last wrote them,
    /// which this NMI latches as the cartridge's NMI uploads them, over the room effect's area. A state import that resumes inside pause takes the plane gameplay would have latched
    /// from it (#1351).
    /// </summary>
    internal Layer3Plane CaptureLayer3Plane()
    {
        var hudRows = new BackgroundTile[Layer3Plane.HudRows * Layer3Plane.Columns];
        // Without installed HUD artwork the HUD shows the stock top row.
        BackgroundTile[] topRow = _artwork.MapPresentation?.GameplayHud.TopRow() ?? GameplayHudTiles.StockTopRow();
        topRow.CopyTo(hudRows, 0);
        _hud.MutableRows.CopyTo(hudRows.AsSpan(Layer3Plane.Columns));
        return Layer3Plane.Compose(hudRows, _room.Layer3Fx?.EffectPageTiles() ?? []);
    }

    /// <summary>The fixed colours effects chose during the previous main loop.</summary>
    private GameplayLatchedFixedColors CaptureFixedColors()
    {
        // Before the first room exists there is no explosion; its colour is black.
        RgbColor powerBomb = _room.BombProjectiles?.PowerBombExplosion.FixedColor ?? RgbColor.Black;
        return new(
            _roomFx.FixedColors.Color,
            powerBomb,
            _suitPickup.FixedColor,
            Displays?.MotherBrain?.RainbowBeam.Color);
    }

    /// <summary>
    /// The reveal's blocks the X-ray setup calls queue for display (#1431). They are copied while X-ray
    /// is active; otherwise the displayed BG2 is the room's own.
    /// </summary>
    private ImmutableArray<RevealedBlock?>? CaptureXrayReveal(GameplayLatchedScene scene) =>
        // Where X-ray preserves the backgrounds ($91:D143) its setup copies no blocks: the tiles are another room's.
        _player.Samus?.Xray.IsActive == true && scene.Room is { } room && XrayBlockRevealRules.RevealsBlocks(room, scene.BossId)
            ? ImmutableArray.Create(_xray.RevealedBlocks)
            : null;
}
