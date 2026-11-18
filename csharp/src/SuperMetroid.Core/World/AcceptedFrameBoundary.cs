using SuperMetroid.Core.World.RoomStates;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Rooms.Scrolling;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Enemies.MorphBallEye;

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

    /// <summary>The layers shown, latched with the accepted frame.</summary>
    private readonly GameplayScreenLayers _screenLayers;
    /// <summary>Sprite frame the update finalized for the next frame boundary.</summary>
    private readonly SpriteFrameBuffer _sprites;
    /// <summary>Supplies Samus's per-frame graphics for the frame boundary.</summary>
    private readonly SamusFrameState _player;
    /// <summary>The HUD whose images each frame boundary latches.</summary>
    private readonly HudState _hud;
    /// <summary>
    /// The pictures of the HUD's images in the room's HUD picture sheet, looked up once per sheet (#1466, #1731): a lookup of the bound
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
    /// <param name="playerInput">The host's controller buttons.</param>
    public void BeginAcceptedFrame(ControllerButton playerInput)
    {
        PublishAcceptedFrame();
        _input.Latch(playerInput);
    }

    /// <summary>
    /// Publishes the display while a loading update keeps the screen dark. No frame is shown, so neither the
    /// controller latch nor any frame counter changes.
    /// </summary>
    internal void PublishWhileLoading() => PublishAcceptedFrame();

    /// <summary>
    /// Latches what the frame shows, every picture resolved as it is latched (#2498): its finished sprites, its shown
    /// layers, the HUD, and once a room is loaded the room's frame as the kind of frame the room shows.
    /// </summary>
    private void PublishAcceptedFrame()
    {
        // The whole display is latched before the controller is, and the finished sprites first among it, so rendering
        // sees one coherent frame instead of combining old sprites with a newer room main's turning floor.
        _displayedSprites.LatchFrom(_sprites);
        _screenLayers.Latch();
        // The HUD's images as gameplay last placed them, which this frame boundary shows.
        ShownHud hud = _hud.Shown();
        // Before the first room no room shows, and no room's effect has a display of its own to latch.
        _display.Latched = _room.Scope is { } scope ? LatchRoomFrame(scope, hud) : new LatchBeforeFirstRoom(hud);
    }

    /// <summary>
    /// The frame of the loaded room (#2498): the effects that keep their own displayed state latch it, then the views'
    /// scrolls, the room effect's state, the Morph Ball eye's beam, the HUD's pictures and the glows' tints are latched
    /// with the room as it shows: as a Ceres perspective floor while one shows, otherwise as its ordinary layers.
    /// </summary>
    /// <param name="scope">The loaded room.</param>
    /// <param name="hud">The HUD's images this frame boundary shows.</param>
    private RoomFrameLatch LatchRoomFrame(RoomScope scope, ShownHud hud)
    {
        IEnemyDisplaySources displays = scope.Enemies.DisplaySources;
        var scrolls = new GameplayRenderSnapshot(
            _backgroundScroll.ForegroundPosition,
            _backgroundScroll.ForegroundScroll,
            _backgroundScroll.BackgroundScroll,
            scope.Enemies.Earthquake.LastRoomShake);
        RoomFxOverlayRenderSnapshot? roomFxOverlay = scope.FxOverlay.CaptureForDisplay();
        // Phantoon's wave and visibility, the statues' descent and the rainbow beam's colour latch with the rest of the
        // frame, whatever kind of frame the room shows.
        displays.Phantoon?.BodyWave.LatchDisplay();
        if (displays.Phantoon is { } phantoonDisplay)
            phantoonDisplay.BodyVisibility.LatchDisplay(phantoonDisplay.Death.PixelationSize);
        scope.TourianStatues?.LatchDisplay();
        displays.MotherBrain?.RainbowBeam.LatchDisplay();
        MorphBallEyeBeamCone? eyeBeam = displays.MorphBallEyeBeam.LatchDisplay();
        GameplayLatchedScene scene = CaptureScene(scope);
        XrayRevealedBlocks? reveal = CaptureXrayReveal(scope, scene);
        // The room's pictures are named by what gameplay showed over its load, the frames its animations selected and, in
        // the Ceres Ridley arena, Ridley's looks (#1466, #1791).
        var appearance = new RoomAppearanceView(scope.Appearance, scope.ArtworkFrames, scope.Looks);
        RoomLevelData level = scope.LevelData;
        // The background shows the background plane when the room state shows its level background; otherwise it shows
        // what its owner places there.
        RoomLevelPlane? backgroundPlane = _backgroundScroll.BackgroundScrollModes.Source == RoomBackgroundSource.Level
            ? level.BackgroundPlane
            : null;
        RoomBlockPictureTable pictures = BlockPictures(scope, appearance, backgroundPlane, reveal);
        RoomBackgroundLayer? background = backgroundPlane is { } shownPlane ? new RoomBackgroundLayer(shownPlane.Cells, pictures) : null;
        // The HUD's images draw from the HUD's own sheet, on the HUD's own layer (#1466, #1731).
        PicturedHud picturedHud = HudPicturesIn(scope.HudPictures.Sheet).Pictured(hud);
        // The room effect's liquid or atmosphere draws from the effect overlay sheet's pictures, as the effects' animations
        // select them; the overlay holds nothing else (#1466, #1706, #2505).
        RoomEffectOverlay effectOverlay = RoomEffectOverlay.Latch(roomFxOverlay, scope.FxOverlayPictures, appearance);
        GameplayLatchedTints tints = CaptureTints(scope);
        // The floor Ceres Ridley's getaway flies over shows while he flies it (his room main computes it), with the
        // chamber's walls around it and the room's background in a band below it (#1545).
        if (displays.CeresGetawayFloor is { } ridley)
        {
            return new PerspectiveFloorFrameLatch(hud, picturedHud, scrolls, roomFxOverlay, effectOverlay, eyeBeam, tints,
                CeresPerspectiveFloors.RidleyGetaway(ridley, displays.CeresGetawayChamberWalls), FloorMap(scope),
                background ?? throw new InvalidDataException(
                    $"{scope.Room.Definition.Name} shows its background plane below Ridley's floor, but its state shows no level background."));
        }
        // The elevator shaft's floor shows while the shaft has its transform, which exists exactly while the room was
        // entered through the shaft's door; it fills the gameplay view.
        if (_display.ActiveTurningFloor is { } shaft)
        {
            return new PerspectiveFloorFrameLatch(hud, picturedHud, scrolls, roomFxOverlay, effectOverlay, eyeBeam, tints,
                CeresPerspectiveFloors.ElevatorShaft(shaft, scrolls.ForegroundScroll), FloorMap(scope), BackgroundBand: null);
        }
        GameplayLatchedBossBody bosses = new(displays.BossBody, displays.BossBodyPlacement, displays.BossBodyHidden);
        return new OrdinaryRoomFrameLatch(hud, picturedHud, scrolls, roomFxOverlay, effectOverlay, eyeBeam, tints,
            _screenLayers.Displayed,
            RoomBackgroundsOf(scope, appearance, new RoomBackgroundLayer(level.ForegroundPlane.Cells, pictures), background, bosses.Body),
            scene,
            bosses,
            displays.Phantoon is { } phantoon ? PhantoonDisplaySnapshot.Capture(phantoon) : null,
            scope.FxOverlay.Fireflea is { } fireflea ? new LatchedFirefleaDarkness(fireflea.Tint) : null,
            // Only the statue room has statues, so only its frames carry a descent (#2385). The descent stops at its
            // authored distance, which holds the offset between minus that distance and zero (#1627).
            scope.TourianStatues?.DisplayedVerticalOffset,
            reveal is { } revealed ? new XrayRevealPlane(revealed.Origin, revealed.Grid, pictures) : null);
    }

    /// <summary>The room's part of the scene: the room's blending, the sky's vertical scroll, and the room and its boss.</summary>
    private static GameplayLatchedScene CaptureScene(RoomScope scope) => new(
        scope.FxOverlay.Blend,
        LatchedScrollingSky.Capture(scope.ScrollingSky),
        scope.Room.Definition,
        scope.Enemies.RoomStatus.Boss);

    /// <summary>The turning floor a perspective-floor frame of <paramref name="scope"/> shows (#1349, #1467).</summary>
    /// <exception cref="InvalidDataException">The room shows a perspective floor but has no turning floor.</exception>
    private static AffineFloor FloorMap(RoomScope scope) => (scope.Floor ?? throw new InvalidDataException(
        $"{scope.Room.Definition.Name} shows a perspective floor but has no turning floor.")).Publish();

    /// <summary>
    /// The pictures the room's planes and the X-ray reveal's blocks show for the values the room's appearance inputs take
    /// now (#1392, #1431, #1466): the foreground's, the background's where <paramref name="backgroundPlane"/> shows, and
    /// <paramref name="reveal"/>'s. The elevator shaft's screen is its turning floor alone: it draws none of its level's
    /// blocks (#1647).
    /// </summary>
    private static RoomBlockPictureTable BlockPictures(RoomScope scope, RoomAppearanceView appearance, RoomLevelPlane? backgroundPlane,
        XrayRevealedBlocks? reveal) =>
        scope.Room.State.TurningFloor == RoomTurningFloor.ElevatorShaft
            ? RoomBlockPictureTable.NoneDrawn(scope.BlockPictures.Sheet)
            : scope.BlockPictures.Resolve(scope.LevelData.ForegroundPlane, backgroundPlane, appearance,
                reveal?.Grid.Blocks.OfType<RevealedBlock>().Select(block => block.Picture) ?? []);

    /// <summary>
    /// The room backgrounds an ordinary frame of <paramref name="scope"/> displays (#1346, #1555): <paramref name="foreground"/>
    /// and <paramref name="background"/>, the source screen of a door's opening scroll, the sky's rows, the room's library background, the text an escape shows over either layer, the room's turning floor and
    /// the boss's body, each with its pictures.
    /// </summary>
    /// <param name="scope">The loaded room.</param>
    /// <param name="appearance">The values the room's appearance inputs take now.</param>
    /// <param name="foreground">The foreground's level plane with its pictures.</param>
    /// <param name="background">The background's level plane with its pictures, or null when the background shows a loaded picture.</param>
    /// <param name="body">The body the room's boss shows, or null.</param>
    private RoomBackgrounds RoomBackgroundsOf(RoomScope scope, RoomAppearanceView appearance, RoomBackgroundLayer foreground,
        RoomBackgroundLayer? background, BossBodyLayer? body)
    {
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
        // #1649): not over a room's own background, the sky's rows or a boss body that owns the background. A library
        // background the player sees as something else draws no picture.
        RoomBackgroundImagePlane? image = null;
        if (background is null && sky is null && body is null
            && scope.BackgroundImage is RoomLibraryBackgroundPicture.Drawn { Image: var backgroundImage })
        {
            // The image is one period of what the background shows, as its room shows it, repeated across and down (#1651, #1617).
            image = backgroundImage.Resolve(appearance, RoomBackgroundImagePlacement.Whole(backgroundImage.Image));
        }
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
        // The planes are shared until gameplay changes level data. A boss body shows each part's whole picture (#1466, #1513).
        return new RoomBackgrounds(foreground, background, _room.DoorSourceBackground, foregroundText, sky,
            backgroundText, scope.Floor?.Publish(), body?.Show(), BackgroundImage: image);
    }

    /// <summary>The effect overlay plane this frame boundary displays (#1348): the room effect it shows; the HUD is its own layer (#1466).</summary>
    internal FxOverlayPlane CaptureFxOverlayPlane() => _room.FxOverlay?.EffectPlane ?? FxOverlayPlane.Cleared;

    /// <summary>The pictures of the HUD's images in <paramref name="sheet"/>, the room's HUD picture sheet.</summary>
    private HudPictures HudPicturesIn(SpriteSheet sheet) =>
        _hudPictures is { } shown && ReferenceEquals(shown.Sheet, sheet) ? shown : _hudPictures = new HudPictures(sheet);

    /// <summary>The tints the glows chose during the previous update, each from its own effect.</summary>
    private GameplayLatchedTints CaptureTints(RoomScope scope) =>
        new(scope.BombProjectiles.PowerBombExplosion.Blast.GlowColor, _suitPickup.BeamColor);

    /// <summary>
    /// What the X-ray reveals in the frame this boundary shows (#1431, #1514), built from the room as it is now, while
    /// X-ray is active in a room it reveals blocks in; otherwise null, and the background is the room's own.
    /// </summary>
    private XrayRevealedBlocks? CaptureXrayReveal(RoomScope scope, GameplayLatchedScene scene) =>
        // Where X-ray preserves the backgrounds it reveals no blocks: the pictures are another room's.
        _player.Samus?.Xray.IsActive == true && XrayBlockRevealRules.RevealsBlocks(scene.Room, scene.Boss)
            ? _xray.Reveal(scope)
            : null;
}
