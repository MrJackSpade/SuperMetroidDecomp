using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Rendering;

/// <summary>Simulation-side resolution of live room state into owned PPU composition inputs.</summary>
public static class GameplayDisplayCapture
{
    /// <summary>
    /// Captures the full currently modeled gameplay composition, including mixed video-mode bands.
    /// </summary>
    public static LayeredRenderSnapshot? TryCaptureFrame(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        LayeredRenderSnapshot basis = runtime.Room.ActiveDoor?.UsesCeresElevatorMode7 == true || runtime.Enemies.RidleyEncounter.CeresRidley is { Mode7Active: true }
            ? CaptureMode7Base(runtime) : CaptureOrdinaryBase(runtime);
        var layers = new List<RenderLayer>(basis.Layers.ToArray());
        GameplayPpuRenderSnapshot ppu = runtime.Display.DisplayedGameplayPpu;
        bool doorOwnsDisplay = runtime.Room.DoorTransitionMainScreenLayers is not null;
        if (!doorOwnsDisplay && CaptureXray(runtime, basis) is { } xray) return xray;
        if (!doorOwnsDisplay && runtime.Enemies.PhantoonBehavior.Phantoon is not null)
            layers[0] = CapturePhantoonBlending(runtime, basis);
        if (!doorOwnsDisplay && runtime.RoomLayer3Fx.Type == RoomFxType.Fireflea)
            layers[0] = CaptureFirefleaDarkness(runtime, basis);
        if (!doorOwnsDisplay)
        {
            // Rainbow configuration $24 removes BG3 from the main screen. Its fixed
            // color window replaces room liquid/fog blending while the beam owns HDMA.
            if (runtime.Enemies.MotherBrainEncounter.MotherBrain?.RainbowBeamHdma.Active != true && runtime.Display.DisplayedRoomLayer3Fx is { } fx)
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
                else AddLayer(SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx));
            }
            if (runtime.CeresHaze.Enabled)
                AddLayer(SnesGameplayFrameRenderer.CaptureCeresHaze(runtime.CeresHaze.IsRed,
                    runtime.CeresHaze.Intensity, runtime.Artwork.MapPresentation?.RoomFxPaletteBlends));
            if (runtime.Display.DisplayedMorphBallEyeBeam is { } eye)
                AddLayer(SnesGameplayFrameRenderer.CaptureMorphBallEyeBeam(runtime.AddressSpace, eye, ppu.Layer1Position));
            if (runtime.Enemies.MotherBrainEncounter.MotherBrain is { } motherBrain)
                AddLayer(SnesGameplayFrameRenderer.CaptureMotherBrainRainbowBeam(motherBrain.RainbowBeamHdma));
        }
        // Preserve the reference ordering, including the effects which remain enabled
        // during the door IRQ. Message glyphs precede the suit's final color window.
        AddLayer(SnesGameplayFrameRenderer.CapturePowerBombColorMath(runtime.AddressSpace,
            runtime.BombProjectiles.PowerBombExplosion, ppu.Layer1Position));
        AddLayer(GameplayMessageBoxRenderer.Capture(runtime.MessageBox));
        AddLayer(SamusSuitPickupRenderer.Capture(runtime.SuitPickup));
        return new(basis.Memory, layers.ToArray(), basis.ObjectSelection, basis.Brightness);

        void AddLayer(RenderLayer? layer) { if (layer is not null) layers.Add(layer); }
    }

    /// <summary>
    /// Captures the ordinary Mode-1 base only. Room color math, windows, messages and
    /// suit effects are deliberately excluded; this is not a substitute for a complete frame.
    /// Mode-7 rooms are rejected rather than silently interpreting their memory as tiles.
    /// </summary>
    [PpuLayoutBoundary("codec: ordinary gameplay render packet (BG base registers)")]
    public static LayeredRenderSnapshot CaptureOrdinaryBase(SuperMetroidRuntime runtime)
    {
        Ensure.NotNull(runtime);
        if (runtime.Room.ActiveDoor is null)
            throw new InvalidOperationException("A cartridge room and door must be loaded before capture.");
        if (runtime.Room.ActiveDoor.UsesCeresElevatorMode7 || runtime.Enemies.RidleyEncounter.CeresRidley is { Mode7Active: true })
            throw new InvalidOperationException("Ordinary base capture cannot represent a Mode-7 scene.");

        GameplayPpuRenderSnapshot ppu = runtime.Display.DisplayedGameplayPpu;
        RoomShakeFrameResult shake = ppu.RoomShake;
        Position16<RoomPixels> bg1Scroll = ppu.Bg1Scroll + shake.Bg1;
        Position16<Bg2Pixels> bg2Scroll = ppu.Bg2Scroll + shake.Bg2;
        if (runtime.TourianStatues.Enabled)
            // $88:DBCB supplies BG2VOFS through HDMA after the ordinary scroll
            // register upload. It replaces, rather than adds to, the quake-adjusted
            // register. Type 13 also gives the statue OBJ zero quake displacement.
            bg2Scroll = bg2Scroll.WithY(unchecked((ushort)(ppu.Layer1Position.Y + runtime.TourianStatues.DisplayedVerticalOffset)));
        RoomLayer3FxRenderSnapshot? fx = runtime.Display.DisplayedRoomLayer3Fx;
        ScrollingSkyState? sky = runtime.Room.ScrollingSky;
        ushort[]? skyX = sky is { HorizontalHdmaConfigured: true } ? sky.BuildGameplayHorizontalScrolls(runtime.Room.Camera?.PixelPosition.Y
            ?? throw new InvalidOperationException("Scrolling sky has no gameplay camera.")) : null;
        if (skyX is not null)
            for (int line = 0; line < skyX.Length; line++) skyX[line] = Add(skyX[line], shake.Bg2.X);

        ushort[]? waterX = fx is { } water
            ? SnesGameplayFrameRenderer.BuildWaterBg2HorizontalScrolls(water, bg2Scroll) : null;
        ushort[]? lavaX = fx is { } lava
            ? SnesGameplayFrameRenderer.BuildLavaAcidBg2HorizontalScrolls(lava, bg2Scroll) : null;
        ushort[]? lavaY = fx is { } vertical
            ? SnesGameplayFrameRenderer.BuildLavaAcidBg2VerticalScrolls(vertical, bg2Scroll.Y) : null;
        KraidEnemyState? kraid = runtime.Enemies.KraidBehavior.Kraid;
        bool kraidBg = kraid is { OwnsBg2Tilemap: true };
        bool crocomireBg = runtime.Enemies.CrocomireBehavior.Crocomire is not null;
        bool verticalStatueMap = runtime.RoomLayer3Fx.Type == RoomFxType.TourianEntranceStatue;
        ushort character = runtime.Room.ActiveRoom?.State.SetupCallback ==
            RoomSetupCallback.SetCeresRidleyBgCharacterBaseAndSpawnHaze
            ? GameplayRenderDefinitions.CeresCharacterWord : (ushort)0;
        GameplayWindowRegisterSnapshot displayedWindows =
            runtime.GameplayWindows.DisplayedGameplayWindowRegisters;
        SnesMainScreenLayers displayedMainScreen =
            (SnesMainScreenLayers)(displayedWindows.MainScreen & 0x1f);
        var registers = new OrdinaryGameplayRegisters(bg1Scroll,
            new Position16<Bg2Pixels>(kraidBg ? Add(kraid!.Bg2Scroll.X, shake.Bg2.X)
                : crocomireBg ? Add(runtime.Enemies.CrocomireBehavior.CrocomireBg2Scroll.X, shake.Bg2.X) : bg2Scroll.X, kraidBg ? Add(kraid!.Bg2Scroll.Y, shake.Bg2.Y)
                : crocomireBg ? Add(runtime.Enemies.CrocomireBehavior.CrocomireBg2Scroll.Y, shake.Bg2.Y)
                : sky is not null ? Add(sky.VerticalScroll, shake.Bg2.Y) : bg2Scroll.Y),
            kraidBg ? new Extent16<TilemapCells>(KraidBackgroundRomData.TilemapWidthInTiles, KraidBackgroundRomData.TilemapHeightInTiles)
                : runtime.Enemies.MotherBrainEncounter.MotherBrain is { HasBg2ScrollOverride: true } ? new Extent16<TilemapCells>(32, 32)
                : sky?.HorizontalHdmaConfigured != true && !verticalStatueMap ? new Extent16<TilemapCells>(64, 32)
                : new Extent16<TilemapCells>(32, 64),
            kraidBg ? KraidBackgroundRomData.LiveBg2TilemapWord : GameplayVramLayout.Bg2TilemapWord,
            character, character, runtime.Display.GameplayHudCharacterBaseWord,
            runtime.Room.DoorTransitionMainScreenLayers ??
                (runtime.Enemies.MotherBrainEncounter.MotherBrain?.DeathBg2Hidden == true
                    ? SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj
                    : displayedMainScreen),
            ppu.Bg2FirstScanline, ppu.Bg2EndScanline,
            Windows: displayedWindows.Windows,
            MainScreenWindowMask:
                (SnesMainScreenLayers)(displayedWindows.MainScreenWindow & 0x1f),
            Bg2Mosaic: BackgroundMosaicSampling.ForBg2(runtime.Enemies.PhantoonBehavior.Phantoon?.Blending.DisplayedMosaic ?? 0));
        // Producers can return longer HDMA storage; the renderer consumes only the
        // gameplay region. Own exactly those visible register values in the packet.
        var layer = new OrdinaryGameplayRenderLayer(registers, new Bg2HdmaScrollTables(
            VisibleLines(runtime.Enemies.PhantoonBehavior.Phantoon?.Wave.DisplayedScrolls ?? lavaX ?? waterX ?? skyX),
            VisibleLines(crocomireBg
                ? runtime.Enemies.CrocomireBehavior.CrocomireDeath is { MeltingHdmaActive: true } melting
                    ? melting.Bg2ScrollByScanline : null
                : lavaY)),
            runtime.Room.DoorTransitionMainScreenLayers is null && runtime.Enemies.MotherBrainEncounter.MotherBrain?.RisingHdmaActive == true
                ? MotherBrainAscentDisplayDefinitions.BuildGameplayLayers() : default);
        return new(PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedOam),
            new RenderLayer[] { layer }, GameplayRenderDefinitions.ObjectSelection,
            SnesPpuLayout.MaximumMasterBrightness);
    }

    /// <summary>Visible lines.</summary>
    private static ushort[] VisibleLines(IReadOnlyList<ushort>? values)
    {
        if (values is null) return [];
        int count = SnesPpuLayout.ScreenHeightPixels - SnesPpuLayout.GameplayHudHeightPixels;
        if (values.Count < count) throw new InvalidDataException("Published HDMA does not cover the gameplay viewport.");
        var result = new ushort[count];
        for (int i = 0; i < count; i++) result[i] = values[i];
        return result;
    }

    /// <summary>Adds.</summary>
    private static ushort Add(ushort scroll, short shake) => unchecked((ushort)(scroll + shake));

    /// <summary>Normal Fireflea blending ($88:80B0), before and after X-ray owns the window.</summary>
    private static GameplayColorMathRenderLayer CaptureFirefleaDarkness(SuperMetroidRuntime runtime, LayeredRenderSnapshot basis)
    {
        if (basis.Layers[0] is not OrdinaryGameplayRenderLayer ordinary)
            throw new InvalidOperationException("Fireflea darkness requires ordinary Mode-1 gameplay.");
        var fixedColor = new FixedColorMemory(runtime.MemoryBus);
        // The source-aware operation is shared with X-ray, but an empty window means
        // every pixel is eligible. Native CGADSUB enables only BG2 and the backdrop:
        // foreground and Samus must not be tinted by a whole-frame darkening overlay.
        var closed = Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray();
        return new(ordinary, closed, revealBlocks: false, math: new ColorMathRegisters { ColorMath = SnesColorMathControl.Bg2 | SnesColorMathControl.Backdrop | SnesColorMathControl.Subtract, AddSubscreen = false, FixedRed = fixedColor.RedIntensity, FixedGreen = fixedColor.GreenIntensity, FixedBlue = fixedColor.BlueIntensity });
    }

    /// <summary>Captures mode7 base.</summary>
    [PpuLayoutBoundary("codec: Mode 7 gameplay render packet (BG3 and floor base registers)")]
    private static LayeredRenderSnapshot CaptureMode7Base(SuperMetroidRuntime runtime)
    {
        GameplayPpuRenderSnapshot ppu = runtime.Display.DisplayedGameplayPpu;
        RoomShakeFrameResult shake = ppu.RoomShake;
        Mode7RenderRegisters registers;
        Mode1FloorBand? floor = null;
        if (runtime.Enemies.RidleyEncounter.CeresRidley is { Mode7Active: true } getaway)
        {
            registers = new(unchecked((short)getaway.Mode7MatrixA), unchecked((short)getaway.Mode7MatrixB),
                unchecked((short)getaway.Mode7MatrixC), unchecked((short)getaway.Mode7MatrixD),
                getaway.Mode7Center.AsSignedRegister(),
                new Offset16<Mode7Pixels>(
                    unchecked((short)(getaway.Mode7Offset.X + shake.Bg1.X)),
                    unchecked((short)(getaway.Mode7Offset.Y + shake.Bg1.Y))));
            floor = new(GameplayRenderDefinitions.CeresFloorFirstScanline, GameplayVramLayout.Bg2TilemapWord,
                GameplayRenderDefinitions.CeresCharacterWord,
                ppu.Bg2Scroll + shake.Bg2,
                new Extent16<TilemapCells>(64, 32));
        }
        else
        {
            SamusMode7Transform m = runtime.Display.DisplayedSamusMode7Transform
                ?? throw new InvalidOperationException("The Ceres elevator shaft has no NMI-published Mode 7 matrix.");
            registers = new(unchecked((short)m.MatrixA), unchecked((short)m.MatrixB),
                unchecked((short)m.MatrixC), unchecked((short)m.MatrixA),
                m.Center.AsMode7Register(),
                new Offset16<Mode7Pixels>(
                    unchecked((short)Add(ppu.Bg1Scroll.X, shake.Bg1.X)),
                    unchecked((short)Add(ppu.Bg1Scroll.Y, shake.Bg1.Y))));
        }
        var layer = new Mode7GameplayRenderLayer(registers, GameplayVramLayout.HudTilemapWord,
            GameplayVramLayout.HudCharacterWord, SnesPpuLayout.GameplayHudHeightPixels, floor);
        return new(PpuMemorySnapshot.Capture(runtime.Vram, runtime.Cgram, runtime.DisplayedOam),
            new RenderLayer[] { layer }, GameplayRenderDefinitions.ObjectSelection, SnesPpuLayout.MaximumMasterBrightness);
    }

    /// <summary>Captures phantoon blending.</summary>
    private static RenderLayer CapturePhantoonBlending(SuperMetroidRuntime runtime, LayeredRenderSnapshot basis)
    {
        var ordinary = (OrdinaryGameplayRenderLayer)basis.Layers[0];
        var mode = runtime.Enemies.PhantoonBehavior.Phantoon!.Blending.DisplayedConfiguration;
        if (mode is not (LayerBlendingConfiguration.PhantoonHidden or LayerBlendingConfiguration.PhantoonSemiTransparent))
            return ordinary;
        var gameplay = new OrdinaryGameplayRenderLayer(ordinary.Registers with
        {
            MainScreenLayers = ordinary.Registers.MainScreenLayers & ~SnesMainScreenLayers.Bg2,
        }, ordinary.Bg2LineScrolls);
        if (mode == LayerBlendingConfiguration.PhantoonHidden) return gameplay;
        // $88:80D9: TM=$15, TS=$02, CGADSUB=$35. Gameplay HUD is the separate
        // IRQ band; below it BG2 adds to eligible main sources, never occluding BG1.
        return new GameplayColorMathRenderLayer(gameplay, Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray(), false, new ColorMathRegisters { ColorMath = SnesColorMathControl.Bg1 | SnesColorMathControl.Bg3 | SnesColorMathControl.Obj | SnesColorMathControl.Backdrop, AddSubscreen = true, FixedRed = 0, FixedGreen = 0, FixedBlue = 0 }, subscreenUsesBg2: true);
    }

    /// <summary>Captures X-ray's layer ownership before priority resolution and color math.</summary>
    [PpuLayoutBoundary("codec: X-ray render packet, reveal tilemap written into the snapshot BG2 image")]
    private static LayeredRenderSnapshot? CaptureXray(SuperMetroidRuntime runtime, LayeredRenderSnapshot basis)
    {
        bool finishedThisFrame = runtime.Player.LastXrayBeamStep is { Completed: true, PhaseAtStart: XrayBeamPhase.Finish };
        if (runtime.Player.Samus is not { } samus || !samus.Xray.IsActive && !finishedThisFrame) return null;
        if (!samus.Xray.SetupStage.ShowsBlendedRoom()) return null;
        bool settingUp = samus.Xray.SetupStage != XraySetupStage.Complete;
        if (basis.Layers[0] is not OrdinaryGameplayRenderLayer ordinary)
            throw new NotSupportedException("X-ray display requires ordinary gameplay layers.");
        var room = runtime.Room.ActiveRoom ?? throw new InvalidOperationException("X-ray has no room.");
        ISnesMemoryBus memoryBus = runtime.MemoryBus;
        var ppu = runtime.Display.DisplayedGameplayPpu;
        var mode = XrayRoomDisplayRules.Select(room.Pointer, runtime.RoomLayer3Fx.Type, runtime.Enemies.RoomStatus.BossId);
        bool reveal = mode == XrayRoomBlendMode.RevealBlocks;
        byte[] bytes = basis.Memory.Vram.ToArray();
        var registers = ordinary.Registers;
        if (reveal)
        {
            ushort[] map = new XrayMemory(memoryBus).RevealTilemap.ToArray();
            for (int i = 0; i < map.Length; i++)
            {
                int offset = (GameplayVramLayout.Bg2TilemapWord + i) * 2;
                bytes[offset] = (byte)map[i]; bytes[offset + 1] = SnesLittleEndian.HighByte(map[i]);
            }
            registers = registers with
            {
                // The reveal map is drawn on BG2 at BG1's fine scroll within one 16-pixel block.
                Bg2Scroll = new Position16<Bg2Pixels>((ushort)(ppu.Bg1Scroll.X & 15), (ushort)(ppu.Bg1Scroll.Y & 15)),
                Bg2MapSize = new Extent16<TilemapCells>(64, 32), Bg2TilemapWord = GameplayVramLayout.Bg2TilemapWord,
            };
        }
        registers = registers with
        {
            MainScreenLayers = XrayRoomDisplayRules.RemovesBg2(room.Pointer) && mode == XrayRoomBlendMode.PreserveBackgrounds
                ? SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj
                : SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Bg2 | SnesMainScreenLayers.Obj,
        };
        // Reveal BG2 must not inherit the room's parallax or liquid HDMA. Excluded
        // rooms retain their original map and per-line register values.
        var gameplay = reveal ? new OrdinaryGameplayRenderLayer(registers)
            : new OrdinaryGameplayRenderLayer(registers, ordinary.Bg2LineScrolls);
        var sub = mode != XrayRoomBlendMode.Fireflea && runtime.Display.DisplayedRoomLayer3Fx is { } fx
            ? SnesGameplayFrameRenderer.CaptureRoomLayer3Fx(fx) : null;
        if (mode != XrayRoomBlendMode.Fireflea && sub is null)
        {
            // Clear_FX_Tilemap leaves an opaque BG3 plane even without a visible FX
            // actor. Room initialization supplies zero scroll and the 32x64 BG3 map;
            // capture its real pixels rather than substituting transparent backdrop.
            sub = new Bg2BppColorMathRenderLayer(
                RoomFxRomData.Layer3.LiquidTilemapBaseWord,
                runtime.Display.GameplayHudCharacterBaseWord,
                RoomFxRomData.Layer3.LiquidTilemapSize.Y,
                SnesPpuLayout.GameplayHudHeightPixels,
                ExpandedColorMathOperation.Add,
                new Position16<BgPlanePixels>[SnesPpuLayout.ScreenHeightPixels]);
        }
        // The arithmetic sign belongs to the room's blend configuration, even when
        // its liquid plane is currently offscreen/disabled and supplies no pixels.
        bool subtract = runtime.RoomLayer3Fx.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        var control = XrayRoomDisplayRules.ColorMath(mode, subtract);
        var fixedColor = new FixedColorMemory(memoryBus);
        byte red = fixedColor.RedIntensity, green = fixedColor.GreenIntensity, blue = fixedColor.BlueIntensity;
        if (reveal || mode == XrayRoomBlendMode.Fireflea && red < XrayWindowRenderDefinitions.FixedColorComponent)
            red = green = blue = XrayWindowRenderDefinitions.FixedColorComponent;
        // Phase five deletes the HDMA object but its blend bits have already been
        // published for this pass. Only its fixed color is cleared immediately.
        if (finishedThisFrame && mode != XrayRoomBlendMode.Fireflea) red = green = blue = 0;
        var colors = basis.Memory.Cgram.ToArray();
        bool restoring = finishedThisFrame || samus.Xray.BeamPhase is XrayBeamPhase.RestoreSecondHalf or XrayBeamPhase.Finish;
        if (!settingUp) colors[0] = restoring ? Bgr555.Black : XrayRoomDisplayRules.ActiveBackdrop;
        var memory = new PpuMemorySnapshot(bytes, colors, basis.Memory.Oam, basis.Memory.ModeledSpriteCount);
        // Releasing Run merely advances the native dispatcher to phase three. That
        // phase closes the HDMA window on the NEXT call and clears the backdrop;
        // restoration keeps ownership until phase five finally unfreezes gameplay.
        bool closed = settingUp || restoring || samus.Xray.BeamPhase == XrayBeamPhase.NoBeam;
        var lines = closed ? Enumerable.Repeat(new XrayWindowLine(255, 0), SnesPpuLayout.ScreenHeightPixels).ToArray()
            : SnesGameplayFrameRenderer.CaptureXrayWindowLines(runtime.AddressSpace, samus, ppu.Layer1Position);
        var layer = new GameplayColorMathRenderLayer(gameplay, lines, reveal, new ColorMathRegisters { ColorMath = control, AddSubscreen = mode != XrayRoomBlendMode.Fireflea, FixedRed = red, FixedGreen = green, FixedBlue = blue }, sub);
        return new(memory, new RenderLayer[] { layer }, basis.ObjectSelection, basis.Brightness);
    }
}
