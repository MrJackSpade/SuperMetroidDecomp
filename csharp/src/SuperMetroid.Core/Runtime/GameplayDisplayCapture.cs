using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Runtime;

/// <summary>Resolution of one published gameplay display frame into owned PPU composition inputs.</summary>
public static class GameplayDisplayCapture
{
    /// <summary>The TM and TMW bits that admit a layer: the four backgrounds and OBJ.</summary>
    private const SnesMainScreenLayers TmLayerBits = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Bg3 |
        SnesMainScreenLayers.Bg4 | SnesMainScreenLayers.Obj;

    /// <summary>
    /// Captures the full currently modeled gameplay composition, including mixed video-mode bands,
    /// room FX, X-ray ownership, message glyphs, and the final suit/power-bomb color effects.
    /// </summary>
    public static LayeredRenderSnapshot Capture(GameplayDisplayFrame frame)
    {
        Ensure.NotNull(frame);
        GameplayNmiDisplayLatch latched = frame.Latched;
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        LayeredRenderSnapshot basis = UsesMode7(latched) ? CaptureMode7Base(frame) : CaptureOrdinaryBase(frame);
        var layers = new List<RenderLayer>(basis.Layers.ToArray());
        GameplayPpuRenderSnapshot ppu = latched.Ppu;
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
                    if (layers[0] is not OrdinaryGameplayRenderLayer ordinary)
                        throw new NotSupportedException("Spore FX requires an ordinary Mode-1 gameplay base.");
                    layers[0] = SnesGameplayFrameRenderer.CaptureSpores(ordinary, fx);
                }
                else if (fx.LayerBlendConfiguration == LayerBlendingConfiguration.WaterfallSubtractive)
                {
                    if (layers[0] is not OrdinaryGameplayRenderLayer ordinary)
                        throw new NotSupportedException("Waterfall FX requires an ordinary Mode-1 gameplay base.");
                    layers[0] = SnesGameplayFrameRenderer.CaptureWaterfall(ordinary, fx);
                }
                else AddLayer(SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx, Bg3(latched)));
            }
            if (live.CeresHaze is { } haze)
                AddLayer(SnesGameplayFrameRenderer.CaptureCeresHaze(haze.IsRed, haze.Intensity, live.RoomFxPaletteBlends));
            if (latched.MorphBallEyeBeam is { } eye)
                AddLayer(SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(eye, ppu.Layer1Position));
            if (rainbow is { } beam)
                AddLayer(SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(beam));
        }
        // Preserve the reference ordering, including the effects which remain enabled
        // during the door IRQ. Message glyphs precede the suit's final color window.
        AddLayer(SnesGameplayFrameRenderer.CapturePowerBombColorMath(PowerBomb(frame), ppu.Layer1Position));
        AddLayer(live.MessageBox);
        AddLayer(SamusSuitPickupRenderer.Capture(SuitPickup(frame)));
        return new(basis.Memory, layers.ToArray(), basis.ObjectSelection, basis.Brightness);

        // Append active optional effects without introducing empty render layers.
        void AddLayer(RenderLayer? layer) { if (layer is not null) layers.Add(layer); }
    }

    /// <summary>
    /// Captures the ordinary Mode-1 base only. Room color math, windows, messages and
    /// suit effects are deliberately excluded; this is not a substitute for a complete frame.
    /// Mode-7 rooms are rejected rather than silently interpreting their memory as tiles.
    /// </summary>
    [PpuLayoutBoundary("codec: ordinary gameplay render packet (BG base registers)")]
    public static LayeredRenderSnapshot CaptureOrdinaryBase(GameplayDisplayFrame frame)
    {
        Ensure.NotNull(frame);
        GameplayNmiDisplayLatch latched = frame.Latched;
        GameplayFrameEndDisplayInputs live = frame.FrameEnd;
        if (UsesMode7(latched))
            throw new InvalidOperationException("Ordinary base capture cannot represent a Mode-7 scene.");

        GameplayPpuRenderSnapshot ppu = latched.Ppu;
        GameplayLatchedSceneRegisters scene = latched.Scene;
        GameplayLatchedBossBackgrounds bosses = latched.Bosses;
        RoomShakeFrameResult shake = ppu.RoomShake;
        Position16<RoomPixels> bg1Scroll = ppu.Bg1Scroll + shake.Bg1;
        // Draygon (#1354) and Mother Brain (#1356) place their body images with their own BG2 scrolls, which the room's
        // water waves then displace.
        Position16<Bg2Pixels> bg2Scroll = (bosses.DraygonBody?.Scroll ?? bosses.MotherBrainBody?.Scroll ?? ppu.Bg2Scroll) + shake.Bg2;
        if (live.TourianStatuesEnabled)
            // $88:DBCB supplies BG2VOFS through HDMA after the ordinary scroll
            // register upload. It replaces, rather than adds to, the quake-adjusted
            // register. Type 13 also gives the statue OBJ zero quake displacement.
            bg2Scroll = bg2Scroll.WithY(unchecked((ushort)(ppu.Layer1Position.Y + latched.TourianStatueVerticalOffset)));
        RoomLayer3FxRenderSnapshot? fx = latched.RoomLayer3Fx;
        ScrollingSkyRegisterSnapshot? sky = scene.ScrollingSky;
        ushort[]? skyX = SkyHorizontalScrolls(live, shake);

        ushort[]? waterX = fx is { } water
            ? SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(water, bg2Scroll) : null;
        ushort[]? lavaX = fx is { } lava
            ? SnesGameplayFrameRenderer.BuildLavaAcidBg2HorizontalScrolls(lava, bg2Scroll) : null;
        ushort[]? lavaY = fx is { } vertical
            ? SnesGameplayFrameRenderer.BuildLavaAcidBg2VerticalScrolls(vertical, bg2Scroll.Y) : null;
        BossBg2Layer? kraidBody = bosses.KraidBody;
        // Draygon's body shows only on the scanlines around it ($88:DF94), placed against the frame's camera (#1354).
        DraygonBodyScanlines bg2Lines = bosses.DraygonBody?.VisibleLines(ppu.Layer1Position) ?? new DraygonBodyScanlines(32, 224);
        Position16<Bg2Pixels>? crocomireBg2Scroll = bosses.CrocomireBg2Scroll;
        bool verticalStatueMap = scene.FxType == RoomFxType.TourianEntranceStatue;
        ushort character = latched.Rooms?.Characters == RoomBackgroundCharacters.ObjectCharacterPage
            ? GameplayRenderDefinitions.CeresCharacterWord : (ushort)0;
        GameplayWindowRegisterSnapshot displayedWindows = latched.Windows;
        SnesMainScreenLayers displayedMainScreen =
            displayedWindows.MainScreen & TmLayerBits;
        var registers = new OrdinaryGameplayRegisters(bg1Scroll,
            new Position16<Bg2Pixels>(kraidBody is { } kraidX ? Add(kraidX.Scroll.X, shake.Bg2.X)
                : crocomireBg2Scroll is { } crocomireX ? Add(crocomireX.X, shake.Bg2.X) : bg2Scroll.X,
                kraidBody is { } kraidY ? Add(kraidY.Scroll.Y, shake.Bg2.Y)
                : crocomireBg2Scroll is { } crocomireY ? Add(crocomireY.Y, shake.Bg2.Y)
                : sky is { } skyRegisters ? Add(skyRegisters.Bg2Scroll.Y, shake.Bg2.Y) : bg2Scroll.Y),
            kraidBody is { } kraidMap ? kraidMap.Tilemap.Size
                : bosses.MotherBrainBody is { Scroll: not null } ? new Extent16<TilemapCells>(32, 32)
                : sky?.HorizontalHdmaConfigured != true && !verticalStatueMap ? new Extent16<TilemapCells>(64, 32)
                : new Extent16<TilemapCells>(32, 64),
            GameplayVramLayout.Bg2TilemapWord,
            character, character, scene.HudCharacterWord,
            live.DoorTransitionMainScreenLayers ??
                (bosses.MotherBrainBody is { Hidden: true }
                    ? SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj
                    : displayedMainScreen),
            bg2Lines.First, bg2Lines.End,
            Windows: displayedWindows.Windows,
            MainScreenWindowMask:
                displayedWindows.MainScreenWindow & TmLayerBits,
            Bg2Mosaic: latched.Phantoon is { } phantoonDisplay ? new BackgroundMosaicSampling(phantoonDisplay.MosaicBlockSize) : default);
        // Producers can return longer HDMA storage; the renderer consumes only the
        // gameplay region. Own exactly those visible register values in the packet.
        // Phantoon's wave owns the BG2 horizontal scroll while it shows; otherwise lava, water or the sky does.
        IReadOnlyList<ushort>? bg2HorizontalScrolls = lavaX ?? waterX ?? skyX;
        if (latched.Phantoon?.Wave is { } phantoonWave)
            bg2HorizontalScrolls = phantoonWave.GameplayScrolls();
        // A sky room shows the sky's rows on BG2 (#1347); Kraid shows his body there while he owns it (#1352), and a
        // boss that owns the enemy BG2 image shows it through BG2's map size (#1353).
        RoomBackgrounds rooms = (latched.Rooms
            ?? throw new InvalidOperationException("Ordinary gameplay capture has no room backgrounds.")) with
            {
                Sky = live.ScrollingSky?.Tilemap,
                BossBg2 = (kraidBody?.Tilemap ?? bosses.EnemyBg2?.Tilemap(registers.Bg2MapSize))?.WithPictures(bosses.BodyPictures),
            };
        var layer = new OrdinaryGameplayRenderLayer(registers, rooms, new Bg2HdmaScrollTables(
            VisibleLines(bg2HorizontalScrolls),
            VisibleLines(crocomireBg2Scroll is not null ? live.CrocomireMeltingScrolls : lavaY)),
            live.DoorTransitionMainScreenLayers is null && live.MotherBrainRisingHdmaActive
                ? MotherBrainAscentDisplayDefinitions.BuildGameplayLayers() : default);
        return new(frame.Memory, new RenderLayer[] { layer }, GameplayRenderDefinitions.ObjectSelection,
            SnesPpuLayout.MaximumMasterBrightness);
    }

    /// <summary>The BG3 plane the accepted NMI displays (#1348).</summary>
    private static Layer3Plane Bg3(GameplayNmiDisplayLatch latched) => (latched.Rooms
        ?? throw new InvalidOperationException("A gameplay frame shows BG3 only once a room is loaded.")).Bg3;

    /// <summary>Whether the latched BGMODE selects the Ceres elevator shaft's or Ridley getaway's Mode 7.</summary>
    internal static bool UsesMode7(GameplayNmiDisplayLatch latched) =>
        latched.Scene.CeresElevatorMode7 || latched.Bosses.CeresRidleyMode7 is not null;

    /// <summary>The Mode 7 registers Ceres Ridley's latched shadows program, before room shake.</summary>
    internal static Mode7RenderRegisters CeresRidleyRegisters(CeresRidleyMode7Snapshot ridley) => new(
        unchecked(ridley.MatrixA), unchecked(ridley.MatrixB),
        unchecked(ridley.MatrixC), unchecked(ridley.MatrixD),
        ridley.Center.AsSignedRegister(), ridley.Offset);

    /// <summary>The sky's HDMA BG2HOFS per gameplay line with the latched room shake added, or null without one.</summary>
    internal static ushort[]? SkyHorizontalScrolls(GameplayFrameEndDisplayInputs live, RoomShakeFrameResult shake)
    {
        if (live.ScrollingSky?.GameplayLineScrolls() is not { } table) return null;
        // Room shake is added to the live BG2HOFS register after HDMA supplies each band value.
        var lines = new ushort[table.Length];
        for (int line = 0; line < lines.Length; line++) lines[line] = Add(table[line], shake.Bg2.X);
        return lines;
    }

    /// <summary>The rainbow beam's live window description drawn in the NMI-latched COLDATA color.</summary>
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

    /// <summary>The power bomb's live window inputs drawn in the NMI-latched COLDATA color.</summary>
    internal static PowerBombRenderSnapshot PowerBomb(GameplayDisplayFrame frame)
    {
        FixedColorRegisterSnapshot color = frame.Latched.FixedColors.PowerBomb;
        return frame.FrameEnd.PowerBomb with
        {
            FixedColorRed = color.Red, FixedColorGreen = color.Green, FixedColorBlue = color.Blue,
        };
    }

    /// <summary>The suit pickup's live light beam drawn in the NMI-latched COLDATA color, or null while inactive.</summary>
    internal static SamusSuitPickupFlash? SuitPickup(GameplayDisplayFrame frame)
    {
        if (frame.FrameEnd.SuitPickupBeam is not { } beam) return null;
        FixedColorRegisterSnapshot color = frame.Latched.FixedColors.SuitPickup;
        return new(beam, color.Red, color.Green, color.Blue);
    }

    /// <summary>Copies only gameplay scanlines from a published HDMA table and rejects coverage shorter than the viewport.</summary>
    private static ushort[] VisibleLines(IReadOnlyList<ushort>? values)
    {
        if (values is null) return [];
        int count = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;
        if (values.Count < count) throw new InvalidDataException("Published HDMA does not cover the gameplay viewport.");
        var result = new ushort[count];
        for (int i = 0; i < count; i++) result[i] = values[i];
        return result;
    }

    /// <summary>Adds signed quake displacement to a 16-bit PPU scroll register with native wrapping.</summary>
    private static ushort Add(ushort scroll, short shake) => unchecked((ushort)(scroll + shake));

    /// <summary>Normal Fireflea blending ($88:80B0), before and after X-ray owns the window.</summary>
    private static GameplayColorMathRenderLayer CaptureFirefleaDarkness(FixedColorRegisterSnapshot fixedColor, LayeredRenderSnapshot basis)
    {
        if (basis.Layers[0] is not OrdinaryGameplayRenderLayer ordinary)
            throw new InvalidOperationException("Fireflea darkness requires ordinary Mode-1 gameplay.");
        // The source-aware operation is shared with X-ray, but an empty window means
        // every pixel is eligible. Native CGADSUB enables only BG2 and the backdrop:
        // foreground and Samus must not be tinted by a whole-frame darkening overlay.
        var closed = Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray();
        return new(ordinary, closed, revealBlocks: false, math: new ColorMathRegisters { ColorMath = SnesColorMathControl.Bg2 | SnesColorMathControl.Backdrop | SnesColorMathControl.Subtract, AddSubscreen = false, FixedRed = fixedColor.RedIntensity, FixedGreen = fixedColor.GreenIntensity, FixedBlue = fixedColor.BlueIntensity });
    }

    /// <summary>Captures the Ceres shaft or Ridley Mode-7 matrix and HUD, adding the Mode-1 floor band during Ridley's getaway.</summary>
    [PpuLayoutBoundary("codec: Mode 7 gameplay render packet (BG3 and floor base registers)")]
    private static LayeredRenderSnapshot CaptureMode7Base(GameplayDisplayFrame frame)
    {
        GameplayNmiDisplayLatch latched = frame.Latched;
        GameplayPpuRenderSnapshot ppu = latched.Ppu;
        RoomShakeFrameResult shake = ppu.RoomShake;
        Mode7RenderRegisters registers;
        Mode1FloorBand? floor = null;
        if (latched.Bosses.CeresRidleyMode7 is { } ridley)
        {
            Mode7RenderRegisters getaway = CeresRidleyRegisters(ridley);
            registers = getaway with
            {
                Scroll = new Offset16<Mode7Pixels>(
                    unchecked((short)(getaway.Scroll.X + shake.Bg1.X)),
                    unchecked((short)(getaway.Scroll.Y + shake.Bg1.Y))),
            };
            floor = new(GameplayRenderDefinitions.CeresFloorFirstScanline, GameplayVramLayout.Bg2TilemapWord,
                GameplayRenderDefinitions.CeresCharacterWord,
                ppu.Bg2Scroll + shake.Bg2,
                new Extent16<TilemapCells>(64, 32),
                latched.Rooms?.Bg2);
        }
        else
        {
            SamusMode7Transform m = latched.SamusMode7Transform
                ?? throw new InvalidOperationException("The Ceres elevator shaft has no NMI-published Mode 7 matrix.");
            registers = new(unchecked(m.MatrixA), unchecked(m.MatrixB),
                unchecked(m.MatrixC), unchecked(m.MatrixA),
                m.Center.AsMode7Register(),
                new Offset16<Mode7Pixels>(
                    unchecked((short)Add(ppu.Bg1Scroll.X, shake.Bg1.X)),
                    unchecked((short)Add(ppu.Bg1Scroll.Y, shake.Bg1.Y))));
        }
        var layer = new Mode7GameplayRenderLayer(registers, Bg3(latched), (latched.Rooms?.Mode7
                ?? throw new InvalidOperationException("A Mode 7 gameplay frame shows a Ceres room's Mode 7 map.")),
            GameplayVramLayout.HudCharacterWord, SnesPpuLayout.GameplayHudHeightPixels, floor);
        return new(frame.Memory, new RenderLayer[] { layer }, GameplayRenderDefinitions.ObjectSelection, SnesPpuLayout.MaximumMasterBrightness);
    }

    /// <summary>Removes BG2 when Phantoon is hidden, or uses it as an additive subscreen when the body is semitransparent.</summary>
    private static RenderLayer CapturePhantoonBlending(PhantoonDisplaySnapshot phantoon, LayeredRenderSnapshot basis)
    {
        var ordinary = (OrdinaryGameplayRenderLayer)basis.Layers[0];
        var mode = phantoon.Configuration;
        if (mode is not (LayerBlendingConfiguration.PhantoonHidden or LayerBlendingConfiguration.PhantoonSemiTransparent))
            return ordinary;
        var gameplay = new OrdinaryGameplayRenderLayer(ordinary.Registers with
        {
            MainScreenLayers = ordinary.Registers.MainScreenLayers & ~SnesMainScreenLayers.Bg2,
        }, ordinary.Rooms, ordinary.Bg2LineScrolls);
        if (mode == LayerBlendingConfiguration.PhantoonHidden) return gameplay;
        // $88:80D9: TM=$15, TS=$02, CGADSUB=$35. Gameplay HUD is the separate
        // IRQ band; below it BG2 adds to eligible main sources, never occluding BG1.
        return new GameplayColorMathRenderLayer(gameplay, Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray(), false, new ColorMathRegisters { ColorMath = SnesColorMathControl.Bg1 | SnesColorMathControl.Bg3 | SnesColorMathControl.Obj | SnesColorMathControl.Backdrop, AddSubscreen = true, FixedRed = 0, FixedGreen = 0, FixedBlue = 0 }, subscreenUsesBg2: true);
    }

    /// <summary>Captures X-ray's layer ownership before priority resolution and color math.</summary>
    [PpuLayoutBoundary("codec: X-ray render packet, reveal tilemap written into the snapshot BG2 image")]
    private static LayeredRenderSnapshot? CaptureXray(GameplayDisplayFrame frame, LayeredRenderSnapshot basis)
    {
        if (frame.FrameEnd.Xray is not { } xray) return null;
        bool finishedThisFrame = xray.FinishedThisFrame;
        if (!xray.SetupStage.ShowsBlendedRoom()) return null;
        bool settingUp = xray.SetupStage != XraySetupStage.Complete;
        if (basis.Layers[0] is not OrdinaryGameplayRenderLayer ordinary)
            throw new NotSupportedException("X-ray display requires ordinary gameplay layers.");
        GameplayNmiDisplayLatch latched = frame.Latched;
        GameplayLatchedSceneRegisters scene = latched.Scene;
        RoomHeaderDefinition xrayRoom = scene.Room ?? throw new InvalidOperationException("X-ray has no room.");
        var ppu = latched.Ppu;
        var mode = XrayRoomDisplayRules.Select(xrayRoom, scene.FxType, scene.BossId);
        bool reveal = mode == XrayRoomBlendMode.RevealBlocks;
        byte[] bytes = basis.Memory.Vram.ToArray();
        var registers = ordinary.Registers;
        if (reveal)
        {
            registers = registers with
            {
                // The reveal map is drawn on BG2 at BG1's fine scroll within one 16-pixel block.
                Bg2Scroll = new Position16<Bg2Pixels>((ushort)(ppu.Bg1Scroll.X & 15), (ushort)(ppu.Bg1Scroll.Y & 15)),
                Bg2MapSize = new Extent16<TilemapCells>(64, 32), Bg2TilemapWord = GameplayVramLayout.Bg2TilemapWord,
            };
        }
        registers = registers with
        {
            MainScreenLayers = XrayRoomDisplayRules.RemovesBg2(xrayRoom) && mode == XrayRoomBlendMode.PreserveBackgrounds
                ? SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj
                : SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj,
        };
        // Reveal BG2 must not inherit the room's parallax or liquid HDMA. Excluded
        // rooms retain their original map and per-line register values.
        // The reveal is BG2, drawn from block pictures (#1392), so the room's layer-2 plane is not shown under it. Before
        // the first accepted NMI of an X-ray use there is no reveal and BG2 still shows the room's BG2 map in VRAM.
        var gameplay = reveal ? new OrdinaryGameplayRenderLayer(registers, ordinary.Rooms with { Bg2 = null, Sky = null, XrayReveal = latched.XrayReveal })
            : new OrdinaryGameplayRenderLayer(registers, ordinary.Rooms, ordinary.Bg2LineScrolls);
        var sub = mode != XrayRoomBlendMode.Fireflea && latched.RoomLayer3Fx is { } fx
            ? SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx, ordinary.Rooms.Bg3) : null;
        if (mode != XrayRoomBlendMode.Fireflea && sub is null)
        {
            // Clear_FX_Tilemap leaves an opaque BG3 plane even without a visible FX
            // actor. Room initialization supplies zero scroll and the 32x64 BG3 map;
            // capture its real pixels rather than substituting transparent backdrop.
            sub = new Bg2BppColorMathRenderLayer(
                ordinary.Rooms.Bg3, 0,
                scene.HudCharacterWord,
                Layer3Plane.Rows,
                SnesPpuLayout.GameplayHudHeightPixels,
                ExpandedColorMathOperation.Add,
                new Position16<BgPlanePixels>[SnesPpuLayout.ScreenHeightPixels]);
        }
        // The arithmetic sign belongs to the room's blend configuration, even when
        // its liquid plane is currently offscreen/disabled and supplies no pixels.
        bool subtract = scene.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        var control = XrayRoomDisplayRules.ColorMath(mode, subtract);
        FixedColorRegisterSnapshot fixedColor = latched.FixedColors.Shared;
        int red = fixedColor.RedIntensity, green = fixedColor.GreenIntensity, blue = fixedColor.BlueIntensity;
        if (reveal || mode == XrayRoomBlendMode.Fireflea && red < XrayWindowRenderDefinitions.FixedColorComponent)
            red = green = blue = XrayWindowRenderDefinitions.FixedColorComponent;
        // Phase five deletes the HDMA object but its blend bits have already been
        // published for this pass. Only its fixed color is cleared immediately.
        if (finishedThisFrame && mode != XrayRoomBlendMode.Fireflea) red = green = blue = 0;
        var colors = basis.Memory.Cgram.ToArray();
        bool restoring = finishedThisFrame || xray.BeamPhase is XrayBeamPhase.RestoreSecondHalf or XrayBeamPhase.Finish;
        if (!settingUp) colors[0] = restoring ? Bgr555.Black : XrayRoomDisplayRules.ActiveBackdrop;
        var memory = new PpuMemorySnapshot(bytes, colors, basis.Memory.SpriteDraws);
        // Releasing Run merely advances the native dispatcher to phase three. That
        // phase closes the HDMA window on the NEXT call and clears the backdrop;
        // restoration keeps ownership until phase five finally unfreezes gameplay.
        bool closed = settingUp || restoring || xray.BeamPhase == XrayBeamPhase.NoBeam;
        var lines = closed ? Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray()
            : SnesGameplayFrameRenderer.CaptureXrayWindowLines(xray, ppu.Layer1Position);
        var layer = new GameplayColorMathRenderLayer(gameplay, lines, reveal, new ColorMathRegisters { ColorMath = control, AddSubscreen = mode != XrayRoomBlendMode.Fireflea, FixedRed = red, FixedGreen = green, FixedBlue = blue }, sub);
        return new(memory, new RenderLayer[] { layer }, basis.ObjectSelection, basis.Brightness);
    }
}
