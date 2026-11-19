using System.Collections.Immutable;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Gameplay;

using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Runtime;

/// <summary>Resolution of one published gameplay display frame into the layered scene the backend draws.</summary>
public static class GameplayDisplayCapture
{
    /// <summary>
    /// Captures the full gameplay composition, including the HUD band above the room,
    /// room FX, X-ray ownership, message glyphs, and the final suit/power-bomb color effects.
    /// </summary>
    /// <exception cref="InvalidOperationException">No frame boundary has latched a room's frame yet.</exception>
    public static LayeredScene Capture(GameplayDisplayFrame frame)
    {
        Ensure.NotNull(frame);
        return frame.Latched switch
        {
            OrdinaryRoomFrameLatch ordinary => CaptureOrdinary(frame, ordinary),
            PerspectiveFloorFrameLatch perspective => CapturePerspectiveFloor(frame, perspective),
            LatchBeforeFirstRoom => throw new InvalidOperationException("A gameplay frame shows a room once a frame boundary has latched one."),
            _ => throw new InvalidOperationException($"Undefined kind of gameplay frame {frame.Latched.GetType().Name}."),
        };
    }

    /// <summary>
    /// An ordinary room's frame: its base, X-ray's scene when X-ray owns it, and otherwise the effects that blend the room's
    /// scene and the effects drawn over it.
    /// </summary>
    private static LayeredScene CaptureOrdinary(GameplayDisplayFrame frame, OrdinaryRoomFrameLatch latched)
    {
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        OrdinaryGameplayBase basis = CaptureOrdinaryBase(latched, live);
        if (live.DoorTransitionShownLayers is not null)
            return Compose(frame, latched, basis.Room);
        if (CaptureXray(frame, latched, basis) is { } xray) return xray;
        // The room's scene, which the effects lie on, and the effect blending it, of which a room shows one at most:
        // Phantoon's translucent body, Fireflea's darkness, or a room effect lying behind the room.
        GameplaySceneLayer scene = basis.Room;
        GameplayBlendLayer? blended = null;
        if (latched.Phantoon is { } phantoon)
        {
            scene = PhantoonScene(phantoon, scene);
            if (phantoon.BodyVisibility == PhantoonBodyVisibility.Translucent)
                blended = PhantoonTranslucence(scene);
        }
        if (latched.Fireflea is { } darkness)
            blended = CaptureFirefleaDarkness(darkness.Shade, basis.Room);
        // While the rainbow beam shows, its added colour takes the place of the room effect's overlay and blend. An effect
        // whose overlay lies behind the room blends the room with it; one lying over the room is drawn over the finished
        // scene (#1545).
        if (live.RainbowBeam is null && latched.EffectOverlay is ShownRoomEffectOverlay { Blend: OverlayBehindRoomBlend behind } lyingBehind)
        {
            if (blended is not null)
                throw new NotSupportedException("An effect blending behind the room requires a room no other effect blends.");
            blended = GameplayEffectCapture.CaptureEffectBlendingBehind(scene, lyingBehind, behind);
        }
        return Compose(frame, latched, (DisplayLayer?)blended ?? scene);
    }

    /// <summary>
    /// A perspective floor's frame: the latched floor and, during Ridley's getaway, the background band below the floor and
    /// the chamber's walls behind it (#1545), with the effects drawn over them.
    /// </summary>
    /// <exception cref="NotSupportedException">X-ray shows its blended room, or the room's effect lies behind the room.</exception>
    private static LayeredScene CapturePerspectiveFloor(GameplayDisplayFrame frame, PerspectiveFloorFrameLatch latched)
    {
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        if (live.DoorTransitionShownLayers is null)
        {
            if (live.Xray is { WarmUp.ShowsBlendedRoom: true })
                throw new NotSupportedException("X-ray display requires ordinary gameplay layers.");
            if (live.RainbowBeam is null && latched.EffectOverlay is ShownRoomEffectOverlay { Blend: OverlayBehindRoomBlend })
                throw new NotSupportedException("An effect blending behind the room requires an ordinary gameplay base.");
        }
        GameplayRenderSnapshot scrolls = latched.Scrolls;
        RoomShakeFrameResult shake = scrolls.RoomShake;
        PerspectiveFloor perspective = latched.Floor;
        // Room shake moves the floor with the foreground.
        FloorView transform = perspective.Transform with
        {
            Scroll = new Offset16<FloorPixels>(
                perspective.Transform.Scroll.X + shake.Foreground.X,
                perspective.Transform.Scroll.Y + shake.Foreground.Y),
        };
        // The band's background scroll is a position that goes above and left of its origin (#2308), as the ordinary view's
        // does: the latched scroll plus the room's shake.
        TurningRoomLowerBand? band = latched.BackgroundBand is { } background
            ? new(GameplayRenderDefinitions.CeresFloorFirstLine, scrolls.BackgroundScroll + shake.Background, background)
            : null;
        var layer = new TurningRoomLayer(transform, latched.FloorMap, ScreenGeometry.GameplayHudHeightPixels, band)
        {
            Scenery = Scenery(perspective.Scenery, scrolls.ForegroundPosition),
        };
        return Compose(frame, latched, layer);
    }

    /// <summary>
    /// The frame's scene: <paramref name="room"/>, the HUD's layer, and the effects drawn over them. Outside a door's
    /// transition these are the room effect's overlay lying over the room, the Ceres haze, the Morph Ball eye's beam and
    /// the rainbow beam; during it too, above the rest and in this order: the Power Bomb's glow, the message box, then the
    /// suit pickup's light beam over the message's letters.
    /// </summary>
    /// <param name="frame">The published frame.</param>
    /// <param name="latched">The room's frame the frame boundary latched.</param>
    /// <param name="room">The room as the frame shows it, its effects that blend it composed.</param>
    private static LayeredScene Compose(GameplayDisplayFrame frame, RoomFrameLatch latched, DisplayLayer room)
    {
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        Position16<RoomPixels> camera = latched.Scrolls.ForegroundPosition;
        var layers = new List<DisplayLayer> { room, Hud(latched) };
        if (live.DoorTransitionShownLayers is null)
        {
            if (live.RainbowBeam is null && latched.EffectOverlay is ShownRoomEffectOverlay { Blend: OverlayOverRoomBlend } over)
                AddLayer(GameplayEffectCapture.CaptureRoomFxOverlay(over));
            if (live.CeresHaze is { } haze)
                AddLayer(GameplayEffectCapture.CaptureCeresHaze(live.CeresHazeTints, haze.Tint, haze.Strength));
            if (latched.MorphBallEyeBeam is { } eye)
                AddLayer(GameplayEffectCapture.CaptureMorphBallEyeBeam(eye, camera));
            if (live.RainbowBeam is { } beam)
                AddLayer(GameplayEffectCapture.CaptureMotherBrainRainbowBeam(beam, camera));
        }
        AddLayer(GameplayEffectCapture.CapturePowerBombGlow(PowerBomb(live, latched), camera));
        AddLayer(live.MessageBox);
        AddLayer(GameplayEffectCapture.CaptureSuitPickup(SuitPickup(live, latched)));
        return new(frame.Paint, layers.ToArray(), DisplayBrightness.Full);

        // Append active optional effects without introducing empty render layers.
        void AddLayer(DisplayLayer? layer) { if (layer is not null) layers.Add(layer); }
    }

    /// <summary>
    /// Captures an ordinary room frame's base only: the room's scene and the HUD's layer. Room blends, windows, messages and
    /// suit effects are deliberately excluded; this is not a substitute for a complete frame.
    /// </summary>
    /// <param name="latched">The ordinary room frame the frame boundary latched.</param>
    /// <param name="live">The display inputs read as the frame ends.</param>
    public static OrdinaryGameplayBase CaptureOrdinaryBase(OrdinaryRoomFrameLatch latched, GameplayFrameEndDisplayInputs live)
    {
        Ensure.NotNull(latched);
        Ensure.NotNull(live);
        GameplayRenderSnapshot scrolls = latched.Scrolls;
        GameplayLatchedBossBody bosses = latched.Bosses;
        RoomShakeFrameResult shake = scrolls.RoomShake;
        // The foreground's scroll is signed (#2384): the room's shake, like the Crocomire rumble, adds a plain offset, and a
        // scroll above or left of the room's origin needs no wrap.
        Offset16<RoomPixels> foregroundScroll = scrolls.ForegroundScroll + shake.Foreground;
        // The room's background shows where its own scroll puts it, moved by the room's shake as any background is. Its scroll
        // is signed (#2305): the backend repeats each background picture by its own size, so a scroll left of or above the
        // background's origin needs no wrap.
        Position16<RoomPixels> camera = scrolls.ForegroundPosition;
        Offset16<BackgroundPixels> backgroundScroll = scrolls.BackgroundScroll + shake.Background;
        if (latched.TourianStatueVerticalOffset is int descent)
            // The statue room's descent supplies the background's vertical scroll per line after the
            // ordinary scroll. It replaces, rather than adds to, the quake-adjusted
            // scroll. The descent also gives the statue sprites zero quake displacement.
            backgroundScroll = backgroundScroll.WithY(scrolls.ForegroundPosition.Y + descent);
        LatchedScrollingSky? sky = latched.Scene.ScrollingSky;
        ScrollBands? skyX = SkyHorizontalScrolls(live, shake);
        // Lava, water or the sky moves the background's lines.
        BackgroundLineScrolls backgroundLines = LiquidLineScrolls(latched.RoomFxOverlay, backgroundScroll, skyX);
        // A boss's body shows whole where it lies (#1873); while it is hidden, once Draygon is deleted or while Mother Brain's
        // death flicker hides hers, it shows nothing.
        ScreenLayers displayedLayers = bosses.Hidden
            ? latched.ShownLayers & ~ScreenLayers.BossBody
            : latched.ShownLayers;
        // The cameras become the pixels the layers show at the screen's top-left corner (#1514), where the room is shown
        // against them (RoomView, #2573), so the backend draws the view with no offset of its own. A sky room's background
        // shows the sky's rows down the screen.
        var view = new GameplaySceneView(RoomView.Shown(foregroundScroll),
            RoomView.Shown(sky is { } latchedSky ? backgroundScroll.WithY(latchedSky.CameraRow + shake.Background.Y) : backgroundScroll),
            live.DoorTransitionShownLayers ?? displayedLayers);
        // A sky room shows the sky's rows on the background (#1347). A boss's body is its own layer (#2573), its pictures
        // placed where they show against its placement (#1466, #1514, #2031); while Mother Brain rises, the floor's band hides
        // what lies below its top (MotherBrainAscentDisplay).
        var layer = new GameplaySceneLayer(view, latched.Rooms, backgroundLines,
            live.DoorTransitionShownLayers is null && live.MotherBrainAscentShowing
                ? MotherBrainAscentDisplay.Bands : default)
        {
            BossBody = BossBodyOf(latched, live, shake, skyX),
        };
        return new(layer, Hud(latched));
    }

    /// <summary>
    /// The boss's body the frame shows as its own layer (#1352-#1356, #2031, #2573), or null in a room no boss shows a body
    /// in: its parts' pictures where its boss places them against the frame's camera, moved by the room's shake as the room's
    /// background is, rippled by Phantoon's wave or sagged by Crocomire's melt, and otherwise moved as the room's lava, water
    /// or sky moves the room's background, and pixelated by Phantoon's appearance.
    /// </summary>
    private static BossBodyView? BossBodyOf(OrdinaryRoomFrameLatch latched, GameplayFrameEndDisplayInputs live,
        RoomShakeFrameResult shake, ScrollBands? skyX)
    {
        GameplayLatchedBossBody bosses = latched.Bosses;
        if (bosses.Shown is not { } body)
            return null;
        Position16<RoomPixels> camera = latched.Scrolls.ForegroundPosition;
        // The body shows from its placement against the camera; a body no boss places shows where the room's background
        // scroll puts it.
        Offset16<BackgroundPixels> scroll = (bosses.Placement is { } placement ? BossBodyPlacement.Shown(placement, camera) : latched.Scrolls.BackgroundScroll)
            + shake.Background;
        BackgroundLineScrolls liquid = LiquidLineScrolls(latched.RoomFxOverlay, scroll, skyX);
        return new BossBodyView(body, RoomView.Shown(scroll))
        {
            // Phantoon's wave ripples his body about where it lies, and Crocomire's melt sags his.
            Horizontal = latched.Phantoon is { Wave: { } wave } phantoon ? BossLineScrolls.Of(wave, camera, phantoon.BodyPlacement) : liquid.Horizontal,
            Vertical = live.CrocomireMelt is { } melt ? BossLineScrolls.Of(melt, camera) : liquid.Vertical,
            Pixelation = latched.Phantoon is { } phantoonDisplay ? new BackgroundPixelation(phantoonDisplay.PixelationBlockSize) : default,
        };
    }

    /// <summary>
    /// How the room's lava, acid or water, or else its sky's bands, move the lines of a background-like layer scrolled to
    /// <paramref name="scroll"/> (#1468): lava's, acid's or water's horizontal waves before the sky's, and lava's or acid's
    /// vertical haze.
    /// </summary>
    private static BackgroundLineScrolls LiquidLineScrolls(RoomFxOverlayRenderSnapshot? fx,
        Offset16<BackgroundPixels> scroll, ScrollBands? skyX)
    {
        SampledScrollWave? waterX = fx is { } water
            ? GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(water, scroll) : null;
        SampledScrollWave? lavaX = fx is { } lava
            ? GameplayEffectCapture.BuildLavaAcidBackgroundHorizontalScrolls(lava, scroll) : null;
        SampledScrollWave? lavaY = fx is { } vertical
            ? GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(vertical, scroll.Y) : null;
        return new BackgroundLineScrolls { Horizontal = lavaX ?? waterX ?? (LineScroll?)skyX, Vertical = lavaY };
    }

    /// <summary>
    /// The HUD the frame boundary latched, its own layer drawn straight after the room (#1466): gameplay's HUD band shows its
    /// pictures opaque, their background included, in front of everything the band holds.
    /// </summary>
    private static HudLayer Hud(RoomFrameLatch latched) => new(latched.PicturedHud, DrawsOpaque: true);

    /// <summary>
    /// The sky's bands of the background's horizontal scroll (#1468) with the latched room shake added, or null without a
    /// sky. Room shake is added to the background's horizontal scroll after each band's value is supplied.
    /// </summary>
    internal static ScrollBands? SkyHorizontalScrolls(GameplayFrameEndDisplayInputs live, RoomShakeFrameResult shake) =>
        live.ScrollingSky is { } sky ? SkyBandScrolls.Of(sky, shake.Background.X) : null;

    /// <summary>The power bomb's live window inputs drawn in the colour the accepted frame latched.</summary>
    /// <param name="live">The display inputs read as the frame ends.</param>
    /// <param name="latched">The room's frame the frame boundary latched.</param>
    internal static PowerBombRenderSnapshot PowerBomb(GameplayFrameEndDisplayInputs live, RoomFrameLatch latched) =>
        live.PowerBomb with { Oval = live.PowerBomb.Oval with { Colour = latched.Tints.PowerBomb } };

    /// <summary>The suit pickup's live light beam drawn in the colour the accepted frame latched, or null while inactive.</summary>
    /// <param name="live">The display inputs read as the frame ends.</param>
    /// <param name="latched">The room's frame the frame boundary latched.</param>
    internal static SamusSuitPickupFlash? SuitPickup(GameplayFrameEndDisplayInputs live, RoomFrameLatch latched) =>
        live.SuitPickupBeam is { } beam ? new(beam, latched.Tints.SuitPickup) : null;

    /// <summary>
    /// Fireflea's darkness, before and after X-ray owns the window (#1514): its shade darkens the background and the backdrop
    /// where each is frontmost, everywhere on the screen. The foreground and Samus keep their colours.
    /// </summary>
    private static GameplayBlendLayer CaptureFirefleaDarkness(RgbColor shade, GameplaySceneLayer ordinary) =>
        new(ordinary, ScreenWindow.Closed, revealBlocks: false, blend: new GameplayBlend
        {
            Blended = BlendedLayers.Background | BlendedLayers.Backdrop,
            Operation = BlendOperation.Subtract,
            Halved = false,
            Colour = shade,
        });

    /// <summary>
    /// The pictures fixed in the room around a turning floor (#1545), each placed so its origin lands on its room point,
    /// on a plane scrolled with <paramref name="camera"/> as the actors are drawn; null when there are none.
    /// </summary>
    /// <param name="scenery">The pictures and their room points.</param>
    /// <param name="camera">The camera the frame's actors were drawn against.</param>
    private static TurningRoomScenery? Scenery(ImmutableArray<PerspectiveFloorScenery> scenery, Position16<RoomPixels> camera)
    {
        if (scenery.IsDefaultOrEmpty) return null;
        ImmutableArray<PlacedPlanePicture> placed = [.. scenery.Select(picture => new PlacedPlanePicture(picture.Picture,
            Offset16<PlanePixels>.Of(picture.Point.X - picture.Picture.Origin.X, picture.Point.Y - picture.Picture.Origin.Y),
            picture.Shading))];
        return new(new PicturedPlane(placed), camera.AsPlaneOffset());
    }

    /// <summary>
    /// The room's scene as Phantoon's body shows on it (#1545): as the room shows it, or without the background his body
    /// shows on while he is hidden or translucent.
    /// </summary>
    /// <exception cref="InvalidOperationException">His body's visibility is undefined.</exception>
    private static GameplaySceneLayer PhantoonScene(PhantoonDisplaySnapshot phantoon, GameplaySceneLayer ordinary)
    {
        switch (phantoon.BodyVisibility)
        {
            case PhantoonBodyVisibility.Shown:
                return ordinary;
            case PhantoonBodyVisibility.Hidden:
            case PhantoonBodyVisibility.Translucent:
                return new GameplaySceneLayer(ordinary.View with
                {
                    ShownLayers = ordinary.View.ShownLayers & ~ScreenLayers.BossBody,
                }, ordinary.Rooms, ordinary.BackgroundLineScrolls) { BossBody = ordinary.BossBody };
            default:
                throw new InvalidOperationException($"Undefined Phantoon body visibility {phantoon.BodyVisibility}.");
        }
    }

    /// <summary>
    /// Phantoon's translucent body (#1514): the background lying behind <paramref name="gameplay"/>, the room without it,
    /// brightens the foreground, the actors taking part in blends and the backdrop wherever it shows, without ever covering
    /// the foreground.
    /// </summary>
    private static GameplayBlendLayer PhantoonTranslucence(GameplaySceneLayer gameplay) =>
        new(gameplay, ScreenWindow.Closed, false, new GameplayBlend
        {
            Blended = BlendedLayers.Foreground | BlendedLayers.Actors | BlendedLayers.Backdrop,
            Operation = BlendOperation.Add,
            Halved = false,
            Colour = RgbColor.Black,
        }, backgroundBehind: true);

    /// <summary>
    /// The X-ray's scene (#1514): the room, its reveal's blocks inside the cone where the room reveals them, and outside the
    /// cone the room blended toward its liquid or the X-ray's colour.
    /// </summary>
    private static LayeredScene? CaptureXray(GameplayDisplayFrame frame, OrdinaryRoomFrameLatch latched, OrdinaryGameplayBase basis)
    {
        if (frame.FrameEnd.Xray is not { } xray) return null;
        bool finishedThisFrame = xray.FinishedThisFrame;
        if (!xray.WarmUp.ShowsBlendedRoom) return null;
        bool warmingUp = !xray.WarmUp.IsOver;
        GameplaySceneLayer ordinary = basis.Room;
        GameplayLatchedScene scene = latched.Scene;
        RoomDefinition xrayRoom = scene.Room;
        XrayRoomBlendMode mode = XrayRoomDisplayRules.Select(xrayRoom, latched.Fireflea, scene.Boss);
        bool reveal = mode.RevealsBlocks;
        // The reveal shows once a frame boundary has latched it; until then the background shows what the room shows (#1403).
        XrayRevealPlane? shownReveal = reveal ? latched.XrayReveal : null;
        var view = ordinary.View;
        if (shownReveal is not null)
        {
            // The reveal lies over the room's own blocks (#1514), so the background it is drawn on moves with the foreground.
            view = view with
            {
                BackgroundScroll = new Offset16<BackgroundPixels>(view.ForegroundScroll.X, view.ForegroundScroll.Y),
            };
        }
        view = view with
        {
            ShownLayers = xrayRoom.XrayHidesBackground && mode.KeepsBackgrounds
                ? ScreenLayers.Foreground | ScreenLayers.Sprites
                : ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.BossBody | ScreenLayers.Sprites,
        };
        // The reveal must not take the room's parallax or liquid line scrolls. Excluded rooms keep their background and its
        // line scrolls.
        // The reveal is drawn on the background from block pictures (#1392), so it replaces what the room's background shows
        // (#2544), and a boss's body with it (#2573). No boss whose body shows lets X-ray reveal blocks
        // (XrayBlockRevealDefinitions).
        var gameplay = shownReveal is not null
            ? new GameplaySceneLayer(view, ordinary.Rooms with { Background = new XrayRevealBackground(shownReveal) })
            : new GameplaySceneLayer(view, ordinary.Rooms, ordinary.BackgroundLineScrolls) { BossBody = ordinary.BossBody };
        // Outside the cone the room blends with its liquid or fog lying behind it, except in Fireflea's room, whose darkness
        // blends with its shade alone.
        OverlayBlendLayer? overlayBehind = !mode.KeepsDarkness
            ? GameplayEffectCapture.CaptureRoomFxOverlay(latched.EffectOverlay) : null;
        // A room with no visible effect blends with nothing behind (#1514). With nothing lying behind, the room combines with the
        // blend's colour unhalved, as it does wherever its liquid shows nothing.
        // Whether the blend darkens belongs to the room's liquid, even when the liquid is currently off screen and shows
        // nothing: it darkens where the room effect's own blend does (#1545).
        bool roomDarkens = scene.Blend?.Operation == BlendOperation.Subtract;
        // The X-ray's own tint: grey in a reveal room, none elsewhere; in Fireflea's room the room's darkness shade, never
        // lighter than the reveal's grey. Its last pass clears the tint at once, except in Fireflea's room.
        int level = latched.Fireflea is { } darkness
            ? Math.Max(darkness.Shade.FadeLevels.Red, mode.TintLevel)
            : mode.TintLevel;
        if (finishedThisFrame && !mode.KeepsDarkness) level = 0;
        // Releasing Run starts the beam's closing, whose window and backdrop still show until its first update is spent;
        // the X-ray keeps the display until its last step finally unfreezes gameplay.
        bool closing = finishedThisFrame || (xray.BeamPhase is XrayBeamPhase.Closing or XrayBeamPhase.Finish && !xray.Closing.ShowsBeam);
        FramePaint paint = warmingUp ? frame.Paint
            : frame.Paint.WithBackdrop(closing ? RgbColor.Black : XrayWindowRenderDefinitions.ActiveBackdrop);
        bool closed = warmingUp || closing || xray.BeamPhase == XrayBeamPhase.NoBeam;
        ScreenWindow window = closed ? ScreenWindow.Closed : GameplayEffectCapture.CaptureXrayWindow(xray, latched.Scrolls.ForegroundPosition);
        var layer = new GameplayBlendLayer(gameplay, window, reveal, new GameplayBlend
        {
            Blended = mode.Blended,
            Operation = mode.Operation(roomDarkens),
            Halved = mode.Halves && overlayBehind is not null,
            Colour = new FadeLevelColour(level, level, level).ToColour(),
        }, overlayBehind);
        return new(paint, new DisplayLayer[] { layer, basis.Hud }, DisplayBrightness.Full);
    }
}
