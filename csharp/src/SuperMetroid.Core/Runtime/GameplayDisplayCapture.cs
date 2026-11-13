using System.Collections.Immutable;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
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
    /// Captures the full currently modeled gameplay composition, including mixed video-mode bands,
    /// room FX, X-ray ownership, message glyphs, and the final suit/power-bomb color effects.
    /// </summary>
    public static LayeredScene Capture(GameplayDisplayFrame frame)
    {
        Ensure.NotNull(frame);
        GameplayDisplayLatch latched = frame.Latched;
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        LayeredScene basis = latched.ShowsPerspectiveFloor ? CapturePerspectiveFloorBase(frame) : CaptureOrdinaryBase(frame);
        var layers = new List<DisplayLayer>(basis.Layers.ToArray());
        GameplayRenderSnapshot scrolls = latched.Scrolls;
        bool doorOwnsDisplay = live.DoorTransitionShownLayers is not null;
        if (!doorOwnsDisplay && CaptureXray(frame, basis) is { } xray) return xray;
        if (!doorOwnsDisplay && latched.Phantoon is { } phantoon)
            layers[0] = CapturePhantoonBody(phantoon, basis);
        if (!doorOwnsDisplay && latched.Scene.FxType == RoomFxType.Fireflea)
            layers[0] = CaptureFirefleaDarkness(FirefleaTint(latched), basis);
        if (!doorOwnsDisplay)
        {
            // The rainbow beam's blend hides the effect overlay. Its fixed
            // color window replaces room liquid/fog blending while the beam owns the per-line effects.
            MotherBrainRainbowBeamRenderSnapshot? rainbow = RainbowBeam(frame);
            if (rainbow?.Active != true && latched.RoomFxOverlay is { } fx)
            {
                // An effect whose overlay lies behind the room blends the room with it; one lying over the room is drawn
                // over the finished scene (#1545).
                if (fx.Blend?.BlendsBehind is not null)
                {
                    if (layers[0] is not GameplaySceneLayer ordinary)
                        throw new NotSupportedException("An effect blending behind the room requires an ordinary gameplay base.");
                    layers[0] = GameplayEffectCapture.CaptureEffectBlendingBehind(ordinary, fx);
                }
                else AddLayer(GameplayEffectCapture.CaptureRoomFxOverlay(fx, FxOverlay(latched)));
            }
            if (live.CeresHaze is { } haze)
                AddLayer(GameplayEffectCapture.CaptureCeresHaze(live.CeresHazeTints
                    ?? throw new InvalidOperationException("The Ceres haze shows with no installed tints bound."), haze.IsRed, haze.Intensity));
            if (latched.MorphBallEyeBeam is { } eye)
                AddLayer(GameplayEffectCapture.CaptureMorphBallEyeBeam(eye, scrolls.ForegroundPosition));
            if (rainbow is { } beam)
                AddLayer(GameplayEffectCapture.CaptureMotherBrainRainbowBeam(beam));
        }
        // Preserve the reference ordering, including the effects which remain enabled
        // during the door's scroll. Message glyphs precede the suit's final color window.
        AddLayer(GameplayEffectCapture.CapturePowerBombGlow(PowerBomb(frame), scrolls.ForegroundPosition));
        AddLayer(live.MessageBox);
        AddLayer(GameplayEffectCapture.CaptureSuitPickup(SuitPickup(frame)));
        return new(basis.Paint, layers.ToArray(), basis.Brightness);

        // Append active optional effects without introducing empty render layers.
        void AddLayer(DisplayLayer? layer) { if (layer is not null) layers.Add(layer); }
    }

    /// <summary>
    /// Captures the ordinary base only: the room and the HUD's layer. Room blends, windows, messages and
    /// suit effects are deliberately excluded; this is not a substitute for a complete frame.
    /// Turning-floor rooms are rejected rather than silently drawn as an ordinary room.
    /// </summary>
    public static LayeredScene CaptureOrdinaryBase(GameplayDisplayFrame frame)
    {
        Ensure.NotNull(frame);
        GameplayDisplayLatch latched = frame.Latched;
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        if (latched.ShowsPerspectiveFloor)
            throw new InvalidOperationException("Ordinary base capture cannot represent a turning-floor scene.");

        GameplayRenderSnapshot scrolls = latched.Scrolls;
        GameplayLatchedScene scene = latched.Scene;
        GameplayLatchedBossBackgrounds bosses = latched.Bosses;
        RoomShakeFrameResult shake = scrolls.RoomShake;
        Position16<RoomPixels> foregroundScroll = scrolls.ForegroundScroll + shake.Foreground;
        // Draygon's (#1354) and Mother Brain's (#1356) bodies show where they lie in the room against the frame's camera
        // (#1617), which the room's water waves then displace.
        Position16<RoomPixels> camera = scrolls.ForegroundPosition;
        Position16<BackgroundPixels>? draygonBody = bosses.DraygonBody is { } draygonPlacement ? BossBodyPlacement.Shown(draygonPlacement.BodyOrigin, camera) : null;
        Position16<BackgroundPixels>? motherBrainBody = bosses.MotherBrainBody?.BodyOrigin is { } motherBrainOrigin ? BossBodyPlacement.Shown(motherBrainOrigin, camera) : null;
        Position16<BackgroundPixels> backgroundScroll = (draygonBody ?? motherBrainBody ?? scrolls.BackgroundScroll) + shake.Background;
        if (live.TourianStatuesEnabled)
            // The statues' descent supplies the background's vertical scroll per line after the
            // ordinary scroll. It replaces, rather than adds to, the quake-adjusted
            // scroll. Type 13 also gives the statue sprites zero quake displacement.
            backgroundScroll = backgroundScroll.WithY(CoordinateAxis.Wrap(scrolls.ForegroundPosition.Y + latched.TourianStatueVerticalOffset));
        RoomFxOverlayRenderSnapshot? fx = latched.RoomFxOverlay;
        LatchedScrollingSky? sky = scene.ScrollingSky;
        ScrollBands? skyX = SkyHorizontalScrolls(live, shake);

        SampledScrollWave? waterX = fx is { } water
            ? GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(water, backgroundScroll) : null;
        SampledScrollWave? lavaX = fx is { } lava
            ? GameplayEffectCapture.BuildLavaAcidBackgroundHorizontalScrolls(lava, backgroundScroll) : null;
        SampledScrollWave? lavaY = fx is { } vertical
            ? GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(vertical, backgroundScroll.Y) : null;
        Position16<BackgroundPixels>? kraidBody = bosses.KraidBodyOrigin is { } kraidOrigin ? BossBodyPlacement.Shown(kraidOrigin, camera) : null;
        // Draygon's body shows only on the band of lines around it, placed against the frame's camera (#1354, #1468).
        ScreenLineBand backgroundLines = bosses.DraygonBody is { } draygon
            ? DraygonBodyBand.ShownLines(draygon, scrolls.ForegroundPosition) : ScreenLineBand.Gameplay;
        Position16<BackgroundPixels>? crocomireBody = bosses.CrocomireBodyOrigin is { } crocomireOrigin ? BossBodyPlacement.Shown(crocomireOrigin, camera) : null;
        ScreenLayers displayedLayers = latched.ShownLayers;
        // The cameras become the pixels the layers show at the screen's top-left corner (#1514): the layers show one row
        // below their cameras, so the backend draws the view with no offset of its own.
        var view = new GameplaySceneView(GameplaySceneView.ShownAtScreenTop(foregroundScroll),
            GameplaySceneView.ShownAtScreenTop(new Position16<BackgroundPixels>(kraidBody is { } kraidX ? Add(kraidX.X, shake.Background.X)
                : crocomireBody is { } crocomireX ? Add(crocomireX.X, shake.Background.X) : backgroundScroll.X,
                kraidBody is { } kraidY ? Add(kraidY.Y, shake.Background.Y)
                : crocomireBody is { } crocomireY ? Add(crocomireY.Y, shake.Background.Y)
                : sky is { } latchedSky ? Add(latchedSky.BackgroundScroll.Y, shake.Background.Y) : backgroundScroll.Y)),
            live.DoorTransitionShownLayers ??
                (bosses.MotherBrainBody is { Hidden: true }
                    ? ScreenLayers.Foreground | ScreenLayers.Sprites
                    : displayedLayers),
            backgroundLines.FirstLine, backgroundLines.EndLine,
            BackgroundPixelation: latched.Phantoon is { } phantoonDisplay ? new BackgroundPixelation(phantoonDisplay.MosaicBlockSize) : default);
        // Phantoon's wave owns the background's horizontal scroll while it shows; otherwise lava, water or the sky does.
        LineScroll? backgroundHorizontal = latched.Phantoon?.Wave is { } wave ? BossLineScrolls.Of(wave) : lavaX ?? waterX ?? (LineScroll?)skyX;
        // Crocomire's melt owns the background's vertical scroll while it shows on his body layer; otherwise lava's does.
        LineScroll? backgroundVertical = crocomireBody is null ? lavaY
            : live.CrocomireMelt is { } melt ? BossLineScrolls.Of(melt) : null;
        // A sky room shows the sky's rows on the background (#1347); a boss shows its body layer there while it owns it (#1352,
        // #1353), its pictures placed where they show on the body's area (#1466, #1514).
        RoomBackgrounds rooms = (latched.Rooms
            ?? throw new InvalidOperationException("Ordinary gameplay capture has no room backgrounds.")) with
            {
                BossBody = bosses.Body is null ? null
                    : bosses.ShownBody ?? throw new InvalidOperationException("A boss body is shown once the accepted frame resolves its pictures."),
            };
        var layer = new GameplaySceneLayer(view, rooms, new BackgroundLineScrolls { Horizontal = backgroundHorizontal, Vertical = backgroundVertical },
            live.DoorTransitionShownLayers is null && live.MotherBrainAscentShowing
                ? MotherBrainAscentDisplay.Bands : default);
        return new(frame.Paint, new DisplayLayer[] { layer, Hud(latched) },
            DisplayBrightness.Full);
    }

    /// <summary>
    /// The HUD the frame boundary latched, its own layer drawn straight after the room (#1466): gameplay's HUD band shows its
    /// pictures with their colour zero opaque, in front of everything the band holds.
    /// </summary>
    private static HudLayer Hud(GameplayDisplayLatch latched) => new(latched.PicturedHud
        ?? throw new InvalidOperationException("A gameplay frame shows the HUD once the frame boundary that latched it resolves its pictures."),
        DrawsOpaque: true);

    /// <summary>The room effect's page the frame boundary displays on the effect overlay (#1348, #1514).</summary>
    private static FxOverlayPage FxOverlay(GameplayDisplayLatch latched) => (latched.Rooms
        ?? throw new InvalidOperationException("A gameplay frame shows the effect overlay only once a room is loaded.")).ShownFxOverlay;

    /// <summary>
    /// The sky's bands of the background's horizontal scroll (#1468) with the latched room shake added, or null without a
    /// sky. Room shake is added to the background's horizontal scroll after each band's value is supplied.
    /// </summary>
    internal static ScrollBands? SkyHorizontalScrolls(GameplayFrameEndDisplayInputs live, RoomShakeFrameResult shake) =>
        live.ScrollingSky is { } sky ? SkyBandScrolls.Of(sky, shake.Background.X) : null;

    /// <summary>The rainbow beam's live window description drawn in the colour the accepted frame latched.</summary>
    internal static MotherBrainRainbowBeamRenderSnapshot? RainbowBeam(GameplayDisplayFrame frame)
    {
        MotherBrainRainbowBeamRenderSnapshot? beam = frame.FrameEnd.RainbowBeam;
        if (beam is not { Active: true } active) return beam;
        return active with
        {
            Color = frame.Latched.Tints.RainbowBeam
                ?? throw new InvalidOperationException("An active rainbow beam has no latched colour."),
        };
    }

    /// <summary>The power bomb's live window inputs drawn in the colour the accepted frame latched.</summary>
    internal static PowerBombRenderSnapshot PowerBomb(GameplayDisplayFrame frame)
    {
        return frame.FrameEnd.PowerBomb with { FixedColor = frame.Latched.Tints.PowerBomb };
    }

    /// <summary>The suit pickup's live light beam drawn in the colour the accepted frame latched, or null while inactive.</summary>
    internal static SamusSuitPickupFlash? SuitPickup(GameplayDisplayFrame frame)
    {
        if (frame.FrameEnd.SuitPickupBeam is not { } beam) return null;
        return new(beam, frame.Latched.Tints.SuitPickup);
    }

    /// <summary>The shade Fireflea darkness latched for a frame whose room effect is Fireflea darkness.</summary>
    /// <exception cref="InvalidOperationException">The frame latched no Fireflea shade.</exception>
    private static RgbColor FirefleaTint(GameplayDisplayLatch latched) =>
        latched.Tints.Fireflea ?? throw new InvalidOperationException("A Fireflea room's frame latched no Fireflea shade.");

    /// <summary>Adds signed quake displacement to a layer scroll, wrapping past 65535 to 0.</summary>
    private static int Add(int scroll, int shake) => CoordinateAxis.Wrap(scroll + shake);

    /// <summary>
    /// Fireflea's darkness, before and after X-ray owns the window (#1514): its shade darkens the background and the backdrop
    /// where each is frontmost, everywhere on the screen. The foreground and Samus keep their colours.
    /// </summary>
    private static GameplayBlendLayer CaptureFirefleaDarkness(RgbColor shade, LayeredScene basis)
    {
        if (basis.Layers[0] is not GameplaySceneLayer ordinary)
            throw new InvalidOperationException("Fireflea darkness requires ordinary gameplay.");
        return new(ordinary, ScreenWindow.Closed, revealBlocks: false, blend: new GameplayBlend
        {
            Blended = BlendedLayers.Background | BlendedLayers.Backdrop,
            Operation = BlendOperation.Subtract,
            Halved = false,
            Colour = shade,
        });
    }

    /// <summary>
    /// Captures the latched perspective floor and the HUD's layer, adding the background band below the floor and the chamber's
    /// walls behind it during Ridley's getaway (#1545).
    /// </summary>
    private static LayeredScene CapturePerspectiveFloorBase(GameplayDisplayFrame frame)
    {
        GameplayDisplayLatch latched = frame.Latched;
        GameplayRenderSnapshot scrolls = latched.Scrolls;
        RoomShakeFrameResult shake = scrolls.RoomShake;
        PerspectiveFloor perspective = latched.PerspectiveFloor
            ?? throw new InvalidOperationException("A perspective-floor capture has no latched floor.");
        // Room shake moves the floor with the foreground, wrapping as the scroll does.
        FloorView transform = perspective.Transform with
        {
            Scroll = new Offset16<FloorPixels>(
                CoordinateAxis.AsSigned(perspective.Transform.Scroll.X + shake.Foreground.X),
                CoordinateAxis.AsSigned(perspective.Transform.Scroll.Y + shake.Foreground.Y)),
        };
        TurningRoomLowerBand? floor = perspective.Below == BelowPerspectiveFloor.RoomBackground
            ? new(GameplayRenderDefinitions.CeresFloorFirstLine, scrolls.BackgroundScroll + shake.Background,
                latched.Rooms?.Background ?? throw new InvalidOperationException("Ceres Ridley's room shows its background plane below the turning floor."))
            : null;
        var layer = new TurningRoomLayer(transform, (latched.Rooms?.Floor
                ?? throw new InvalidOperationException("A turning-floor gameplay frame shows a Ceres room's floor.")), ScreenGeometry.GameplayHudHeightPixels, floor)
        {
            Scenery = Scenery(perspective.Scenery, scrolls.ForegroundPosition),
        };
        return new(frame.Paint, new DisplayLayer[] { layer, Hud(latched) }, DisplayBrightness.Full);
    }

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
    /// Phantoon's body as it shows (#1545): as the room shows it, removed when he is hidden, or lying translucent behind
    /// the room. His room shows his body on its background.
    /// </summary>
    private static DisplayLayer CapturePhantoonBody(PhantoonDisplaySnapshot phantoon, LayeredScene basis)
    {
        var ordinary = (GameplaySceneLayer)basis.Layers[0];
        switch (phantoon.BodyVisibility)
        {
            case PhantoonBodyVisibility.Shown:
                return ordinary;
            case PhantoonBodyVisibility.Hidden:
            case PhantoonBodyVisibility.Translucent:
                break;
            default:
                throw new InvalidOperationException($"Undefined Phantoon body visibility {phantoon.BodyVisibility}.");
        }
        var gameplay = new GameplaySceneLayer(ordinary.View with
        {
            ShownLayers = ordinary.View.ShownLayers & ~ScreenLayers.Background,
        }, ordinary.Rooms, ordinary.BackgroundLineScrolls);
        if (phantoon.BodyVisibility == PhantoonBodyVisibility.Hidden) return gameplay;
        // Phantoon's translucent body is the background lying behind the room: it brightens the foreground, the actors taking
        // part in blends and the backdrop wherever it shows, without ever covering the foreground (#1514).
        return new GameplayBlendLayer(gameplay, ScreenWindow.Closed, false, new GameplayBlend
        {
            Blended = BlendedLayers.Foreground | BlendedLayers.Actors | BlendedLayers.Backdrop,
            Operation = BlendOperation.Add,
            Halved = false,
            Colour = RgbColor.Black,
        }, backgroundBehind: true);
    }

    /// <summary>
    /// The X-ray's scene (#1514): the room, its reveal's blocks inside the cone where the room reveals them, and outside the
    /// cone the room blended toward its liquid or the X-ray's colour.
    /// </summary>
    private static LayeredScene? CaptureXray(GameplayDisplayFrame frame, LayeredScene basis)
    {
        if (frame.FrameEnd.Xray is not { } xray) return null;
        bool finishedThisFrame = xray.FinishedThisFrame;
        if (!xray.WarmUp.ShowsBlendedRoom) return null;
        bool warmingUp = !xray.WarmUp.IsOver;
        if (basis.Layers[0] is not GameplaySceneLayer ordinary)
            throw new NotSupportedException("X-ray display requires ordinary gameplay layers.");
        GameplayDisplayLatch latched = frame.Latched;
        GameplayLatchedScene scene = latched.Scene;
        RoomHeaderDefinition xrayRoom = scene.Room ?? throw new InvalidOperationException("X-ray has no room.");
        var scrolls = latched.Scrolls;
        var mode = XrayRoomDisplayRules.Select(xrayRoom, scene.FxType, scene.Boss);
        bool reveal = mode == XrayRoomBlendMode.RevealBlocks;
        // The reveal shows once a frame boundary has latched it; until then the background shows what the room shows (#1403).
        XrayRevealPlane? shownReveal = reveal ? latched.XrayReveal : null;
        var view = ordinary.View;
        if (shownReveal is not null)
        {
            // The reveal lies over the room's own blocks (#1514), so the background it is drawn on moves with the foreground.
            view = view with
            {
                BackgroundScroll = new Position16<BackgroundPixels>(view.ForegroundScroll.X, view.ForegroundScroll.Y),
            };
        }
        view = view with
        {
            ShownLayers = XrayRoomDisplayRules.RemovesBackground(xrayRoom) && mode == XrayRoomBlendMode.PreserveBackgrounds
                ? ScreenLayers.Foreground | ScreenLayers.Sprites
                : ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites,
        };
        // The reveal must not take the room's parallax or liquid line scrolls. Excluded rooms keep their background and its
        // line scrolls.
        // The reveal is drawn on the background from block pictures (#1392), so the room's own background is not shown under it.
        var gameplay = shownReveal is not null
            ? new GameplaySceneLayer(view, ordinary.Rooms with { Background = null, Sky = null, XrayReveal = shownReveal })
            : new GameplaySceneLayer(view, ordinary.Rooms, ordinary.BackgroundLineScrolls);
        // Outside the cone the room blends with its liquid or fog lying behind it, except in Fireflea's room, whose darkness
        // blends with its shade alone.
        OverlayBlendLayer? overlayBehind = mode != XrayRoomBlendMode.Fireflea && latched.RoomFxOverlay is { } fx
            ? GameplayEffectCapture.CaptureRoomFxOverlay(fx, ordinary.Rooms.ShownFxOverlay) : null;
        // A room with no visible effect blends with nothing behind (#1514): the cleared effect area the original game left
        // opaque is a presentation quirk the port does not keep. With nothing lying behind, the room combines with the
        // blend's colour unhalved, as it does wherever its liquid shows nothing.
        // Whether the blend darkens belongs to the room's liquid, even when the liquid is currently off screen and shows
        // nothing: it darkens where the room effect's own blend does (#1545).
        bool roomDarkens = scene.Blend?.Operation == BlendOperation.Subtract;
        // The X-ray's own tint: grey in a reveal room, none elsewhere; in Fireflea's room the room's darkness shade, never
        // lighter than the reveal's grey. Its last pass clears the tint at once, except in Fireflea's room.
        int level = mode switch
        {
            XrayRoomBlendMode.RevealBlocks => XrayWindowRenderDefinitions.RevealTintLevel,
            XrayRoomBlendMode.Fireflea => Math.Max(FirefleaTint(latched).FadeLevels.Red, XrayWindowRenderDefinitions.RevealTintLevel),
            XrayRoomBlendMode.PreserveBackgrounds => 0,
            _ => throw new InvalidOperationException($"Undefined X-ray room blend {mode}."),
        };
        if (finishedThisFrame && mode != XrayRoomBlendMode.Fireflea) level = 0;
        bool restoring = finishedThisFrame || xray.BeamPhase is XrayBeamPhase.RestoreSecondHalf or XrayBeamPhase.Finish;
        FramePaint paint = warmingUp ? basis.Paint
            : basis.Paint.WithBackdrop(restoring ? RgbColor.Black : XrayRoomDisplayRules.ActiveBackdrop);
        // Releasing Run merely advances the beam to phase three. That
        // phase closes the beam's window on the NEXT call and clears the backdrop;
        // restoration keeps ownership until phase five finally unfreezes gameplay.
        bool closed = warmingUp || restoring || xray.BeamPhase == XrayBeamPhase.NoBeam;
        ScreenWindow window = closed ? ScreenWindow.Closed : GameplayEffectCapture.CaptureXrayWindow(xray, scrolls.ForegroundPosition);
        var layer = new GameplayBlendLayer(gameplay, window, reveal, new GameplayBlend
        {
            Blended = XrayRoomDisplayRules.Blended(mode),
            Operation = XrayRoomDisplayRules.Operation(mode, roomDarkens),
            Halved = XrayRoomDisplayRules.Halves(mode) && overlayBehind is not null,
            Colour = new FadeLevelColour(level, level, level).ToColour(),
        }, overlayBehind);
        return new(paint, new DisplayLayer[] { layer, Hud(latched) }, basis.Brightness);
    }
}
