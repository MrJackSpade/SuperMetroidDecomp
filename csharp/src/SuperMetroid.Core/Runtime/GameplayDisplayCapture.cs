using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Gameplay;

using SuperMetroid.Core.Assets;
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
        bool doorOwnsDisplay = live.DoorTransitionMainScreenLayers is not null;
        if (!doorOwnsDisplay && CaptureXray(frame, basis) is { } xray) return xray;
        if (!doorOwnsDisplay && latched.Phantoon is { } phantoon)
            layers[0] = CapturePhantoonBlending(phantoon, basis);
        if (!doorOwnsDisplay && latched.Scene.FxType == RoomFxType.Fireflea)
            layers[0] = CaptureFirefleaDarkness(latched.FixedColors.Shared, basis);
        if (!doorOwnsDisplay)
        {
            // Rainbow configuration $24 removes BG3 from the main screen. Its fixed
            // color window replaces room liquid/fog blending while the beam owns HDMA.
            MotherBrainRainbowBeamRenderSnapshot? rainbow = RainbowBeam(frame);
            if (rainbow?.Active != true && latched.RoomLayer3Fx is { } fx)
            {
                if (fx.Type == RoomFxType.Spores)
                {
                    if (layers[0] is not GameplaySceneLayer ordinary)
                        throw new NotSupportedException("Spore FX requires an ordinary Mode-1 gameplay base.");
                    layers[0] = GameplayEffectCapture.CaptureSpores(ordinary, fx);
                }
                else if (fx.LayerBlendConfiguration == LayerBlendingConfiguration.WaterfallSubtractive)
                {
                    if (layers[0] is not GameplaySceneLayer ordinary)
                        throw new NotSupportedException("Waterfall FX requires an ordinary Mode-1 gameplay base.");
                    layers[0] = GameplayEffectCapture.CaptureWaterfall(ordinary, fx);
                }
                else AddLayer(GameplayEffectCapture.CaptureRoomLayer3Fx(fx, Bg3(latched)));
            }
            if (live.CeresHaze is { } haze)
                AddLayer(GameplayEffectCapture.CaptureCeresHaze(haze.IsRed, haze.Intensity, live.RoomFxPaletteBlends));
            if (latched.MorphBallEyeBeam is { } eye)
                AddLayer(GameplayEffectCapture.CaptureMorphBallEyeBeam(eye, scrolls.Layer1Position));
            if (rainbow is { } beam)
                AddLayer(GameplayEffectCapture.CaptureMotherBrainRainbowBeam(beam));
        }
        // Preserve the reference ordering, including the effects which remain enabled
        // during the door IRQ. Message glyphs precede the suit's final color window.
        AddLayer(GameplayEffectCapture.CapturePowerBombGlow(PowerBomb(frame), scrolls.Layer1Position));
        AddLayer(live.MessageBox);
        AddLayer(GameplayEffectCapture.CaptureSuitPickup(SuitPickup(frame)));
        return new(basis.Paint, layers.ToArray(), basis.Brightness);

        // Append active optional effects without introducing empty render layers.
        void AddLayer(DisplayLayer? layer) { if (layer is not null) layers.Add(layer); }
    }

    /// <summary>
    /// Captures the ordinary Mode-1 base only: the room and the HUD's layer. Room color math, windows, messages and
    /// suit effects are deliberately excluded; this is not a substitute for a complete frame.
    /// Mode-7 rooms are rejected rather than silently interpreting their memory as tiles.
    /// </summary>
    public static LayeredScene CaptureOrdinaryBase(GameplayDisplayFrame frame)
    {
        Ensure.NotNull(frame);
        GameplayDisplayLatch latched = frame.Latched;
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        if (latched.ShowsPerspectiveFloor)
            throw new InvalidOperationException("Ordinary base capture cannot represent a Mode-7 scene.");

        GameplayRenderSnapshot scrolls = latched.Scrolls;
        GameplayLatchedScene scene = latched.Scene;
        GameplayLatchedBossBackgrounds bosses = latched.Bosses;
        RoomShakeFrameResult shake = scrolls.RoomShake;
        Position16<RoomPixels> bg1Scroll = scrolls.Bg1Scroll + shake.Bg1;
        // Draygon (#1354) and Mother Brain (#1356) place their body images at their body layers' placements, which the room's
        // water waves then displace.
        Position16<BackgroundPixels> bg2Scroll = (bosses.DraygonBody?.LayerPlacement ?? bosses.MotherBrainBody?.LayerPlacement ?? scrolls.Bg2Scroll) + shake.Bg2;
        if (live.TourianStatuesEnabled)
            // $88:DBCB supplies BG2VOFS through HDMA after the ordinary scroll
            // register upload. It replaces, rather than adds to, the quake-adjusted
            // register. Type 13 also gives the statue OBJ zero quake displacement.
            bg2Scroll = bg2Scroll.WithY(unchecked((ushort)(scrolls.Layer1Position.Y + latched.TourianStatueVerticalOffset)));
        RoomLayer3FxRenderSnapshot? fx = latched.RoomLayer3Fx;
        LatchedScrollingSky? sky = scene.ScrollingSky;
        ushort[]? skyX = SkyHorizontalScrolls(live, shake);

        ushort[]? waterX = fx is { } water
            ? GameplayEffectCapture.BuildWaterBackgroundHorizontalScrolls(water, bg2Scroll) : null;
        ushort[]? lavaX = fx is { } lava
            ? GameplayEffectCapture.BuildLavaAcidBackgroundHorizontalScrolls(lava, bg2Scroll) : null;
        ushort[]? lavaY = fx is { } vertical
            ? GameplayEffectCapture.BuildLavaAcidBackgroundVerticalScrolls(vertical, bg2Scroll.Y) : null;
        Position16<BackgroundPixels>? kraidBody = bosses.KraidBodyPlacement;
        // Draygon's body shows only on the band of lines around it, placed against the frame's camera (#1354, #1468).
        ScreenLineBand bg2Lines = bosses.DraygonBody is { } draygon
            ? DraygonBodyBand.ShownLines(draygon, scrolls.Layer1Position) : ScreenLineBand.Gameplay;
        Position16<BackgroundPixels>? crocomireBody = bosses.CrocomireBodyPlacement;
        bool verticalStatueMap = scene.FxType == RoomFxType.TourianEntranceStatue;
        ScreenLayers displayedMainScreen = latched.MainScreen;
        var view = new GameplaySceneView(bg1Scroll,
            new Position16<BackgroundPixels>(kraidBody is { } kraidX ? Add(kraidX.X, shake.Bg2.X)
                : crocomireBody is { } crocomireX ? Add(crocomireX.X, shake.Bg2.X) : bg2Scroll.X,
                kraidBody is { } kraidY ? Add(kraidY.Y, shake.Bg2.Y)
                : crocomireBody is { } crocomireY ? Add(crocomireY.Y, shake.Bg2.Y)
                : sky is { } latchedSky ? Add(latchedSky.Bg2Scroll.Y, shake.Bg2.Y) : bg2Scroll.Y),
            kraidBody is not null ? KraidBackgroundDefinitions.BodyPlaneSize
                : bosses.MotherBrainBody is { LayerPlacement: not null } ? new Extent16<TilemapCells>(32, 32)
                : sky?.ShowsBands != true && !verticalStatueMap ? new Extent16<TilemapCells>(64, 32)
                : new Extent16<TilemapCells>(32, 64),
            live.DoorTransitionMainScreenLayers ??
                (bosses.MotherBrainBody is { Hidden: true }
                    ? ScreenLayers.Foreground | ScreenLayers.Sprites
                    : displayedMainScreen),
            bg2Lines.FirstLine, bg2Lines.EndLine,
            BackgroundPixelation: latched.Phantoon is { } phantoonDisplay ? new BackgroundPixelation(phantoonDisplay.MosaicBlockSize) : default);
        // Producers can return longer HDMA storage; the renderer consumes only the
        // gameplay region. Own exactly those visible register values in the packet.
        // Phantoon's wave owns the BG2 horizontal scroll while it shows; otherwise lava, water or the sky does.
        IReadOnlyList<ushort>? bg2HorizontalScrolls = lavaX ?? waterX ?? skyX;
        if (latched.Phantoon?.Wave is { } phantoonWave)
            bg2HorizontalScrolls = phantoonWave.GameplayScrolls();
        // A sky room shows the sky's rows on BG2 (#1347); a boss shows its body layer there while it owns it (#1352,
        // #1353), its pages laid out by BG2's map size, each part from its picture (#1466).
        RoomBackgrounds rooms = (latched.Rooms
            ?? throw new InvalidOperationException("Ordinary gameplay capture has no room backgrounds.")) with
            {
                BossBody = bosses.Body is null ? null
                    : bosses.ShownBody ?? throw new InvalidOperationException("A boss body is shown once the accepted frame resolves its pictures."),
            };
        var layer = new GameplaySceneLayer(view, rooms, new BackgroundLineScrolls(
            VisibleLines(bg2HorizontalScrolls),
            VisibleLines(crocomireBody is not null ? live.CrocomireMeltingScrolls : lavaY)),
            live.DoorTransitionMainScreenLayers is null && live.MotherBrainAscentShowing
                ? MotherBrainAscentDisplay.BuildGameplayLayers() : default);
        return new(frame.Paint, new DisplayLayer[] { layer, Hud(latched) },
            DisplayBrightness.Full);
    }

    /// <summary>
    /// The HUD the accepted NMI latched, its own layer drawn straight after the room (#1466): gameplay's HUD band shows its
    /// pictures with their colour zero opaque, in front of everything the band holds.
    /// </summary>
    private static HudLayer Hud(GameplayDisplayLatch latched) => new(latched.PicturedHud
        ?? throw new InvalidOperationException("A gameplay frame shows the HUD once the accepted NMI that latched it resolves its pictures."),
        TransparentColorZero: false, Priority: null);

    /// <summary>The BG3 plane the accepted NMI displays (#1348).</summary>
    private static Layer3Cells Bg3(GameplayDisplayLatch latched) => (latched.Rooms
        ?? throw new InvalidOperationException("A gameplay frame shows BG3 only once a room is loaded.")).ShownBg3;

    /// <summary>The sky's HDMA BG2HOFS per gameplay line with the latched room shake added, or null without one.</summary>
    internal static ushort[]? SkyHorizontalScrolls(GameplayFrameEndDisplayInputs live, RoomShakeFrameResult shake)
    {
        if (live.ScrollingSky?.GameplayLineScrolls() is not { } table) return null;
        // Room shake is added to the live BG2HOFS register after HDMA supplies each band value.
        var lines = new ushort[table.Length];
        for (int line = 0; line < lines.Length; line++) lines[line] = Add(table[line], shake.Bg2.X);
        return lines;
    }

    /// <summary>The rainbow beam's live window description drawn in the colour the accepted frame latched.</summary>
    internal static MotherBrainRainbowBeamRenderSnapshot? RainbowBeam(GameplayDisplayFrame frame)
    {
        MotherBrainRainbowBeamRenderSnapshot? beam = frame.FrameEnd.RainbowBeam;
        if (beam is not { Active: true } active) return beam;
        return active with
        {
            Color = frame.Latched.FixedColors.RainbowBeam
                ?? throw new InvalidOperationException("An active rainbow beam has no NMI-latched fixed color."),
        };
    }

    /// <summary>The power bomb's live window inputs drawn in the colour the accepted frame latched.</summary>
    internal static PowerBombRenderSnapshot PowerBomb(GameplayDisplayFrame frame)
    {
        return frame.FrameEnd.PowerBomb with { FixedColor = frame.Latched.FixedColors.PowerBomb };
    }

    /// <summary>The suit pickup's live light beam drawn in the colour the accepted frame latched, or null while inactive.</summary>
    internal static SamusSuitPickupFlash? SuitPickup(GameplayDisplayFrame frame)
    {
        if (frame.FrameEnd.SuitPickupBeam is not { } beam) return null;
        return new(beam, frame.Latched.FixedColors.SuitPickup);
    }

    /// <summary>Copies only gameplay scanlines from a published HDMA table and rejects coverage shorter than the viewport.</summary>
    private static ushort[] VisibleLines(IReadOnlyList<ushort>? values)
    {
        if (values is null) return [];
        int count = ScreenGeometry.ScreenHeightPixels - ScreenGeometry.GameplayHudHeightPixels;
        if (values.Count < count) throw new InvalidDataException("Published HDMA does not cover the gameplay viewport.");
        var result = new ushort[count];
        for (int i = 0; i < count; i++) result[i] = values[i];
        return result;
    }

    /// <summary>Adds signed quake displacement to a 16-bit PPU scroll register with native wrapping.</summary>
    private static ushort Add(int scroll, int shake) => unchecked((ushort)(scroll + shake));

    /// <summary>Normal Fireflea blending ($88:80B0), before and after X-ray owns the window.</summary>
    private static GameplayBlendLayer CaptureFirefleaDarkness(RgbColor fixedColor, LayeredScene basis)
    {
        if (basis.Layers[0] is not GameplaySceneLayer ordinary)
            throw new InvalidOperationException("Fireflea darkness requires ordinary Mode-1 gameplay.");
        // The source-aware operation is shared with X-ray, but an empty window means
        // every pixel is eligible. Only the background and the backdrop blend:
        // foreground and Samus must not be tinted by a whole-frame darkening overlay.
        return new(ordinary, ScreenWindow.Closed, revealBlocks: false, blend: new SceneBlend { Layers = ColorBlend.Bg2 | ColorBlend.Backdrop | ColorBlend.Subtract, BlendsWithLayersBehind = false, FixedColor = fixedColor });
    }

    /// <summary>Captures the latched perspective floor and the HUD's layer, adding the layer-2 band below the floor during Ridley's getaway.</summary>
    private static LayeredScene CapturePerspectiveFloorBase(GameplayDisplayFrame frame)
    {
        GameplayDisplayLatch latched = frame.Latched;
        GameplayRenderSnapshot scrolls = latched.Scrolls;
        RoomShakeFrameResult shake = scrolls.RoomShake;
        PerspectiveFloor perspective = latched.PerspectiveFloor
            ?? throw new InvalidOperationException("A perspective-floor capture has no latched floor.");
        // Room shake moves the floor with layer 1, wrapping as a sixteen-bit scroll.
        AffineTransform transform = perspective.Transform with
        {
            Scroll = new Offset16<FloorPixels>(
                unchecked((short)(perspective.Transform.Scroll.X + shake.Bg1.X)),
                unchecked((short)(perspective.Transform.Scroll.Y + shake.Bg1.Y))),
        };
        TurningRoomLowerBand? floor = perspective.Below == BelowPerspectiveFloor.Layer2
            ? new(GameplayRenderDefinitions.CeresFloorFirstLine, scrolls.Bg2Scroll + shake.Bg2,
                new Extent16<TilemapCells>(64, 32),
                latched.Rooms?.Bg2 ?? throw new InvalidOperationException("Ceres Ridley's room shows its layer-2 plane below the Mode 7 floor."))
            : null;
        var layer = new TurningRoomLayer(transform, (latched.Rooms?.Floor
                ?? throw new InvalidOperationException("A Mode 7 gameplay frame shows a Ceres room's Mode 7 map.")), ScreenGeometry.GameplayHudHeightPixels, floor);
        return new(frame.Paint, new DisplayLayer[] { layer, Hud(latched) }, DisplayBrightness.Full);
    }

    /// <summary>Removes BG2 when Phantoon is hidden, or uses it as an additive subscreen when the body is semitransparent.</summary>
    private static DisplayLayer CapturePhantoonBlending(PhantoonDisplaySnapshot phantoon, LayeredScene basis)
    {
        var ordinary = (GameplaySceneLayer)basis.Layers[0];
        var mode = phantoon.Configuration;
        if (mode is not (LayerBlendingConfiguration.PhantoonHidden or LayerBlendingConfiguration.PhantoonSemiTransparent))
            return ordinary;
        var gameplay = new GameplaySceneLayer(ordinary.View with
        {
            MainScreenLayers = ordinary.View.MainScreenLayers & ~ScreenLayers.Background,
        }, ordinary.Rooms, ordinary.BackgroundLineScrolls);
        if (mode == LayerBlendingConfiguration.PhantoonHidden) return gameplay;
        // $88:80D9: TM=$15, TS=$02, CGADSUB=$35. Gameplay HUD is the separate
        // IRQ band; below it BG2 adds to eligible main sources, never occluding BG1.
        return new GameplayBlendLayer(gameplay, ScreenWindow.Closed, false, new SceneBlend { Layers = ColorBlend.Bg1 | ColorBlend.Bg3 | ColorBlend.Sprites | ColorBlend.Backdrop, BlendsWithLayersBehind = true, FixedColor = RgbColor.Black }, subscreenUsesBg2: true);
    }

    /// <summary>Captures X-ray's layer ownership before priority resolution and color math.</summary>
    private static LayeredScene? CaptureXray(GameplayDisplayFrame frame, LayeredScene basis)
    {
        if (frame.FrameEnd.Xray is not { } xray) return null;
        bool finishedThisFrame = xray.FinishedThisFrame;
        if (!xray.SetupStage.ShowsBlendedRoom()) return null;
        bool settingUp = xray.SetupStage != XraySetupStage.Complete;
        if (basis.Layers[0] is not GameplaySceneLayer ordinary)
            throw new NotSupportedException("X-ray display requires ordinary gameplay layers.");
        GameplayDisplayLatch latched = frame.Latched;
        GameplayLatchedScene scene = latched.Scene;
        RoomHeaderDefinition xrayRoom = scene.Room ?? throw new InvalidOperationException("X-ray has no room.");
        var scrolls = latched.Scrolls;
        var mode = XrayRoomDisplayRules.Select(xrayRoom, scene.FxType, scene.BossId);
        bool reveal = mode == XrayRoomBlendMode.RevealBlocks;
        // The reveal shows once an accepted NMI has latched it; until then BG2 shows what the room shows (#1403).
        XrayRevealPlane? shownReveal = reveal ? latched.XrayReveal : null;
        var view = ordinary.View;
        if (shownReveal is not null)
        {
            view = view with
            {
                // The reveal map is drawn on BG2 at BG1's fine scroll within one 16-pixel block.
                BackgroundScroll = new Position16<BackgroundPixels>((ushort)(scrolls.Bg1Scroll.X & 15), (ushort)(scrolls.Bg1Scroll.Y & 15)),
                BackgroundMapSize = new Extent16<TilemapCells>(64, 32),
            };
        }
        view = view with
        {
            MainScreenLayers = XrayRoomDisplayRules.RemovesBackground(xrayRoom) && mode == XrayRoomBlendMode.PreserveBackgrounds
                ? ScreenLayers.Foreground | ScreenLayers.Sprites
                : ScreenLayers.Foreground | ScreenLayers.Background | ScreenLayers.Sprites,
        };
        // Reveal BG2 must not inherit the room's parallax or liquid HDMA. Excluded
        // rooms retain their original map and per-line register values.
        // The reveal is BG2, drawn from block pictures (#1392), so the room's layer-2 plane is not shown under it.
        var gameplay = shownReveal is not null
            ? new GameplaySceneLayer(view, ordinary.Rooms with { Bg2 = null, Sky = null, XrayReveal = shownReveal })
            : new GameplaySceneLayer(view, ordinary.Rooms, ordinary.BackgroundLineScrolls);
        var sub = mode != XrayRoomBlendMode.Fireflea && latched.RoomLayer3Fx is { } fx
            ? GameplayEffectCapture.CaptureRoomLayer3Fx(fx, ordinary.Rooms.ShownBg3) : null;
        if (mode != XrayRoomBlendMode.Fireflea && sub is null)
        {
            // Clear_FX_Tilemap leaves an opaque BG3 plane even without a visible FX
            // actor. Room initialization supplies zero scroll and the 32x64 BG3 map;
            // capture its real pixels rather than substituting transparent backdrop.
            sub = new OverlayBlendLayer(
                ordinary.Rooms.ShownBg3, 0,
                Layer3Plane.Rows,
                ScreenGeometry.GameplayHudHeightPixels,
                BlendOperation.Add,
                new Position16<TilePlanePixels>[ScreenGeometry.ScreenHeightPixels]);
        }
        // The arithmetic sign belongs to the room's blend configuration, even when
        // its liquid plane is currently offscreen/disabled and supplies no pixels.
        bool subtract = scene.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        ColorBlend control = XrayRoomDisplayRules.Blend(mode, subtract);
        RgbColor fixedColor = latched.FixedColors.Shared;
        int red = fixedColor.Red, green = fixedColor.Green, blue = fixedColor.Blue;
        if (reveal || mode == XrayRoomBlendMode.Fireflea && red < XrayWindowRenderDefinitions.FixedColorComponent)
            red = green = blue = XrayWindowRenderDefinitions.FixedColorComponent;
        // Phase five deletes the HDMA object but its blend bits have already been
        // published for this pass. Only its fixed color is cleared immediately.
        if (finishedThisFrame && mode != XrayRoomBlendMode.Fireflea) red = green = blue = 0;
        BackgroundColours colours = basis.Paint.Backgrounds;
        bool restoring = finishedThisFrame || xray.BeamPhase is XrayBeamPhase.RestoreSecondHalf or XrayBeamPhase.Finish;
        if (!settingUp) colours = colours.WithBackdrop(restoring ? RgbColor.Black : XrayRoomDisplayRules.ActiveBackdrop);
        var paint = new FramePaint(colours, basis.Paint.SpriteDraws);
        // Releasing Run merely advances the native dispatcher to phase three. That
        // phase closes the HDMA window on the NEXT call and clears the backdrop;
        // restoration keeps ownership until phase five finally unfreezes gameplay.
        bool closed = settingUp || restoring || xray.BeamPhase == XrayBeamPhase.NoBeam;
        ScreenWindow window = closed ? ScreenWindow.Closed : GameplayEffectCapture.CaptureXrayWindow(xray, scrolls.Layer1Position);
        var layer = new GameplayBlendLayer(gameplay, window, reveal, new SceneBlend { Layers = control, BlendsWithLayersBehind = mode != XrayRoomBlendMode.Fireflea, FixedColor = new RgbColor(red, green, blue) }, sub);
        return new(paint, new DisplayLayer[] { layer, Hud(latched) }, basis.Brightness);
    }
}
