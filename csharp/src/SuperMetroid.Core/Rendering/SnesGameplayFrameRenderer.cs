using SuperMetroid.Core.Assets;
using System.Buffers;
using System.Runtime.CompilerServices;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Rendering;

/// <summary>Composes the currently modeled gameplay PPU layers into one desktop frame.</summary>
[PpuLayoutBoundary("backend: software PPU gameplay frame compositor")]
public static class SnesGameplayFrameRenderer
{

    /// <summary>Visible SNES viewport width in pixels.</summary>
    public const int Width = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>Visible SNES viewport height in physical scanlines.</summary>
    public const int Height = SnesPpuLayout.ScreenHeightPixels;
    /// <summary>Height in pixels of the fixed gameplay HUD above the room viewport.</summary>
    public const int HudHeight = SnesPpuLayout.GameplayHudHeightPixels;

    /// <summary>
    /// Composes the ordinary gameplay PPU layout used by Ceres and most interior rooms,
    /// including the priority bit of every BG tile and all four OBJ priority groups.
    /// </summary>
    public static Rgba32[] RenderHudOrdinaryBackgroundsAndObjs(
        IPaletteColors cgram,
        SpriteFrameBuffer sprites,
        OrdinaryGameplayRenderLayer layer,
        Rgba32[]? outputBuffer = null)
    {
        Ensure.NotNull(layer);
        Rgba32[] output = CreateBackdrop(cgram, outputBuffer);
        OrdinaryGameplayView registers = layer.View;
        Span<SnesMainScreenLayers> windowMasks = stackalloc SnesMainScreenLayers[Width];
        if (registers.MainScreenWindowMask == SnesMainScreenLayers.None)
            windowMasks.Clear();
        else
            for (int x = 0; x < Width; x++)
                windowMasks[x] = registers.Windows.MaskedLayers(x, registers.MainScreenWindowMask);
        // BGMODE=$09 is Mode 1 with the BG3-priority flag. Below the HUD, BG3 is disabled
        // by TM and the relevant back-to-front ladder is OBJ0, OBJ1, BG2-low, BG1-low,
        // OBJ2, BG2-high, BG1-high, OBJ3. Samus's body entries use OBJ2, so cartridge-
        // authored high-priority door frames, railings, and laboratory consoles cover her.
        // Resolve OAM ownership once. Priority planes are a BG-compositor concern only;
        // decoding all 128 sprite records independently for each of the four planes was
        // both slower and easier to get subtly wrong at cross-priority overlaps.
        int pixelCount = Width * Height;
        Rgba32[] rentedObjectPixels = ArrayPool<Rgba32>.Shared.Rent(pixelCount);
        byte[] rentedObjectPriorities = ArrayPool<byte>.Shared.Rent(pixelCount);
        try
        {
            Span<Rgba32> objectPixels = rentedObjectPixels.AsSpan(0, pixelCount);
            Span<byte> objectPriorities = rentedObjectPriorities.AsSpan(0, pixelCount);
            SnesObjRenderer.RenderResolved(sprites.Draws, cgram, new ObjRasterTargets(objectPixels, objectPriorities));
            CompositeOrdinaryGameplayViewport(output, cgram, objectPixels, objectPriorities, layer, windowMasks);
        }
        finally
        {
            // Every opaque object pixel is overwritten before use and transparency is
            // governed by the parallel priority sentinel, so clearing these private render
            // buffers during return would spend frame time without changing observability.
            ArrayPool<Rgba32>.Shared.Return(rentedObjectPixels, clearArray: false);
            ArrayPool<byte>.Shared.Return(rentedObjectPriorities, clearArray: false);
        }
        DrawHud(output, cgram, layer.Rooms.ShownBg3);
        if ((registers.MainScreenWindowMask & SnesMainScreenLayers.Bg3) != 0)
        {
            // The modeled HUD IRQ admits only BG3. A masked HUD pixel therefore
            // exposes backdrop, not the gameplay planes disabled on these scanlines.
            Rgba32 backdrop = cgram.GetRgba(0);
            for (int x = 0; x < Width; x++)
                if ((windowMasks[x] & SnesMainScreenLayers.Bg3) != 0)
                    for (int y = 0; y < HudHeight; y++)
                        output[y * Width + x] = backdrop;
        }
        return output;
    }

    /// <summary>Resolves the Mode-1 BG and OBJ priority ladder in one viewport scan, with tilemap and palette values cached for this frame.</summary>
    /// <remarks>This pure PPU projection runs roughly 50,000 times per frame; optimized Debug JIT code keeps it inside the video-frame budget.</remarks>
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private static void CompositeOrdinaryGameplayViewport(
        Span<Rgba32> output,
        IPaletteColors cgram,
        ReadOnlySpan<Rgba32> objectPixels,
        ReadOnlySpan<byte> objectPriorities,
        OrdinaryGameplayRenderLayer layer,
        ReadOnlySpan<SnesMainScreenLayers> windowMasks)
    {
        if (objectPixels.Length != output.Length ||
            objectPriorities.Length != output.Length)
        {
            throw new ArgumentException(
                "A resolved gameplay OBJ raster must contain exactly 256x224 pixels.",
                nameof(objectPixels));
        }

        // The layer validated its BG2 map size and every per-line table at construction.
        OrdinaryGameplayView registers = layer.View;
        Position16<RoomPixels> bg1Scroll = registers.Bg1Scroll;
        Extent16<TilemapCells> bg2TilemapSize = registers.Bg2MapSize;
        SnesMainScreenLayers mainScreenLayers = registers.MainScreenLayers;
        int bg2FirstScanline = registers.Bg2FirstScanline;
        int bg2EndScanline = registers.Bg2EndScanline;
        BackgroundMosaicSampling bg2Mosaic = registers.Bg2Mosaic;
        ReadOnlySpan<SnesMainScreenLayers> mainScreenLayersByLine = layer.MainScreenLayersByLine;

        // Resolve the complete Mode-1 ladder in a single destination scan. The previous
        // implementation walked the 256x192 viewport eight times (four BG insertions and
        // four OBJ insertions), repeating tilemap address arithmetic and moving temporary
        // RGBA planes through memory. Each candidate below receives its literal back-to-
        // front rank from BGMODE=$09; the largest opaque rank owns the final pixel.
        // Snapshot the hardware arrays once and expand the tiny 256-color CGRAM table once
        // per frame. Going through bounds-checking object methods and decoding BGR555 for
        // both background candidates at every pixel dominated Debug-host frame time while
        // producing the same immutable values throughout this render pass.
        Span<Rgba32> palette = stackalloc Rgba32[SnesCgram.ColorCount];
        ExpandCgram(cgram, palette);

        // BG1 comes from the room backgrounds; BG2 from the room's layer-2 plane or the sky's rows, or the
        // tilemap its owner loaded when it has neither, under any words written over it (#1346, #1347, #1348).
        IBgSource bg1 = layer.Rooms.Bg1Source;
        IBgSource bg2 = layer.Rooms.Bg2Shown(bg2TilemapSize);
        // Room blocks draw from their pre-coloured pictures (#1392), recoloured where a palette effect changed a row.
        var pictures = new BgPicturePixels(cgram);
        for (int screenY = HudHeight; screenY < Height; screenY++)
        {
            SnesMainScreenLayers activeLayers = mainScreenLayersByLine.IsEmpty ? mainScreenLayers
                : mainScreenLayersByLine[screenY - HudHeight];
            bool bg1Enabled = (activeLayers & SnesMainScreenLayers.Bg1) != 0;
            bool bg2LayerEnabled = (activeLayers & SnesMainScreenLayers.Bg2) != 0;
            bool objEnabled = (activeLayers & SnesMainScreenLayers.Obj) != 0;
            bool bg2Enabled = bg2LayerEnabled && screenY >= bg2FirstScanline && screenY < bg2EndScanline;
            // Output rows are zero-based, while Mode-1 BG sampling starts on
            // physical scanline one. OBJ's preceding-line evaluation is separate.
            int physicalBackgroundY = screenY + SnesPpuLayout.FirstVisibleBackgroundScanline;
            ushort bg1ScrolledY = unchecked((ushort)(bg1Scroll.Y + physicalBackgroundY));
            Position16<Bg2Pixels> lineBg2Scroll = layer.Bg2ScrollAt(screenY - HudHeight);
            ushort activeBg2HorizontalScroll = lineBg2Scroll.X;
            ushort bg2ScrolledY = unchecked((ushort)bg2Mosaic.SourceY(screenY, lineBg2Scroll.Y));

            for (int screenX = 0; screenX < Width; screenX++)
            {
                int destination = screenY * Width + screenX;
                Rgba32 winner = output[destination];
                int winnerRank = 0;

                if (bg2Enabled && (windowMasks[screenX] & SnesMainScreenLayers.Bg2) == 0 &&
                    bg2.TrySample(new Position16<BgPlanePixels>(
                        unchecked((ushort)bg2Mosaic.SourceX(screenX, activeBg2HorizontalScroll)), bg2ScrolledY), out BgSample bg2Sample) &&
                    pictures.TryDecode(bg2Sample, palette, ColorZero.Transparent, out Rgba32 bg2Color, out bool bg2High))
                {
                    winner = bg2Color;
                    winnerRank = bg2High ? 6 : 3;
                }

                if (bg1Enabled && (windowMasks[screenX] & SnesMainScreenLayers.Bg1) == 0 &&
                    bg1.TrySample(new Position16<BgPlanePixels>(
                        unchecked((ushort)(bg1Scroll.X + screenX)), bg1ScrolledY), out BgSample bg1Sample) &&
                    pictures.TryDecode(bg1Sample, palette, ColorZero.Transparent, out Rgba32 bg1Color, out bool bg1High))
                {
                    int bg1Rank = bg1High ? 7 : 4;
                    if (bg1Rank > winnerRank)
                    {
                        winner = bg1Color;
                        winnerRank = bg1Rank;
                    }
                }

                byte objPriority = objectPriorities[destination];
                if (objEnabled && (windowMasks[screenX] & SnesMainScreenLayers.Obj) == 0 &&
                    objPriority != SnesObjRenderer.TransparentPriority)
                {
                    int objRank = objPriority switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 5,
                        3 => 8,
                        _ => throw new InvalidDataException(
                            $"Resolved OBJ priority {objPriority} is outside zero through three."),
                    };
                    if (objRank > winnerRank)
                    {
                        winner = objectPixels[destination];
                        winnerRank = objRank;
                    }
                }

                output[destination] = winner;
            }
        }
    }

    /// <summary>
    /// Expands one frame-stable CGRAM snapshot into directly indexable host colors. The
    /// 256-entry conversion is substantially cheaper than decoding the same palette word
    /// independently for tens of thousands of background pixels.
    /// </summary>
    internal static void ExpandCgram(IPaletteColors cgram, Span<Rgba32> destination)
    {
        Ensure.LengthEqual<Rgba32>(destination, SnesCgram.ColorCount);

        ReadOnlySpan<Bgr555> source = cgram.Colors;
        for (int color = 0; color < destination.Length; color++)
            destination[color] = source[color].ToRgba32();
    }

    /// <summary>Renders the four BG3 HUD rows from the BG3 plane's pictures (#1403).</summary>
    internal static void DrawHud(Span<Rgba32> output, IPaletteColors cgram, Layer3Cells plane) =>
        SnesBgTilemapRenderer.Render2Bpp(output[..(HudHeight * Width)], cgram, plane, Layer3Plane.HudRows);

    /// <summary>
    /// Resolves water's optional per-scanline BG2 distortion from <c>$88:C5E4</c>. The
    /// ordinary compositor already accepts the resulting 192-line HDMA projection.
    /// </summary>
    public static ushort[]? BuildWaterBg2HorizontalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        Position16<Bg2Pixels> bg2Scroll)
    {
        if (fx.Type != RoomFxType.Water || (fx.LiquidOptions & RoomLiquidOptions.Bg2Wave) == 0)
            return null;

        var result = Enumerable.Repeat(bg2Scroll.X, Height - HudHeight).ToArray();
        int verticalPhase = bg2Scroll.Y & 15;
        for (int screenY = Math.Max(HudHeight, fx.WaterSurfaceScreenY + 1);
             screenY < Height;
             screenY++)
        {
            int index = (fx.WaterBg2WavePhase + verticalPhase +
                screenY - fx.WaterSurfaceScreenY - 1) &
                (RoomFxRomData.Water.WaveDisplacementCount - 1);
            result[screenY - HudHeight] = unchecked((ushort)(
                bg2Scroll.X + RoomFxRomData.Water.WaveDisplacement(index)));
        }
        return result;
    }

    /// <summary>
    /// Resolves the optional lava/acid BG2HOFS waveform from <c>$88:B53B</c>. Vertical
    /// distortion has cartridge priority when both option bits are set, matching the
    /// branch order in <c>$88:B4D5</c>.
    /// </summary>
    public static ushort[]? BuildLavaAcidBg2HorizontalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        Position16<Bg2Pixels> bg2Scroll)
    {
        bool isLavaAcid = fx.Type is RoomFxType.Lava or RoomFxType.Acid;
        bool usesVertical =
            (fx.LiquidOptions & RoomLiquidOptions.Bg2Wave) != 0;
        bool usesHorizontal =
            (fx.LiquidOptions & RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave) != 0;
        if (!isLavaAcid || usesVertical || !usesHorizontal)
            return null;

        return BuildLavaAcidBg2Wave(
            bg2Scroll.X,
            bg2Scroll.Y,
            fx.LavaAcidBg2WavePhase,
            axis: Axis.Horizontal);
    }

    /// <summary>Resolves the Norfair heat-haze BG2VOFS waveform from <c>$88:B5A9</c>.</summary>
    public static ushort[]? BuildLavaAcidBg2VerticalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        ushort bg2VerticalScroll)
    {
        if (fx.Type is not (RoomFxType.Lava or RoomFxType.Acid) ||
            (fx.LiquidOptions & RoomLiquidOptions.Bg2Wave) == 0)
        {
            return null;
        }

        return BuildLavaAcidBg2Wave(
            bg2VerticalScroll,
            bg2VerticalScroll,
            fx.LavaAcidBg2WavePhase,
            axis: Axis.Vertical);
    }

    /// <summary>Expands the rotating sixteen-entry heat-haze wave into one BG2 scroll word per gameplay scanline.</summary>
    private static ushort[] BuildLavaAcidBg2Wave(
        ushort baseScroll,
        ushort bg2VerticalScroll,
        int wavePhase,
        Axis axis)
    {
        var result = new ushort[Height - HudHeight];
        int verticalPhase = bg2VerticalScroll & 15;
        for (int line = 0; line < result.Length; line++)
        {
            // `$88:C0B1` is 16 one-scanline indirect entries repeated through the frame.
            // Its pointer starts at BG2VOFS's low nibble, while `$88:B53B/$B5A9` rotates
            // the sixteen resident words backward through A. HUD height is 32, exactly two
            // waveform periods, so gameplay-local and physical scanline indices coincide.
            int waveIndex = (verticalPhase + line + wavePhase) &
                (RoomFxRomData.LavaAcid.WaveDisplacementCount - 1);
            short displacement = axis == Axis.Vertical
                ? RoomFxRomData.LavaAcid.VerticalWaveDisplacement(waveIndex)
                : RoomFxRomData.LavaAcid.HorizontalWaveDisplacement(waveIndex);
            result[line] = unchecked((ushort)(baseScroll + displacement));
        }
        return result;
    }

    /// <summary>Samples the active power-bomb window's half-width at a signed distance from its center, or minus one outside the flash.</summary>
    internal static int ReadPowerBombHalfWidth(
        PowerBombRenderSnapshot explosion,
        int yFromCenter)
    {
        PowerBombExplosionPhase renderedPhase = explosion.RenderedPhase;
        if (renderedPhase == PowerBombExplosionPhase.Inactive)
        {
            // Spawn occurs during projectile processing, after the frame's HDMA-object
            // pass. Status is already active at that point, but no window table exists
            // until the next frame; the cartridge therefore displays no flash yet.
            return -1;
        }

        if ((renderedPhase is PowerBombExplosionPhase.PreExplosionYellow or
             PowerBombExplosionPhase.ExplosionWhite or PowerBombExplosionPhase.Afterglow) &&
            explosion.RenderedShape is { } shape)
        {
            // $88:8B98 only fades COLDATA: the final white window remains installed.
            // It can still leave visible corners outside the oval near screen edges.
            // Each 192-byte record is a half-profile, not a 192-line top-to-bottom
            // bitmap. Byte zero is the widest center line; increasing indices travel
            // away from the center until the first zero terminates the native copier.
            int shapeLine = Math.Abs(yFromCenter);
            if ((uint)shapeLine >= 192)
                return -1;
            byte halfWidth = PowerBombShapeDefinitions.ReadPreScaledHalfWidth(
                shape, shapeLine);
            return halfWidth == 0 ? -1 : halfWidth;
        }

        int horizontalRadius = renderedPhase == PowerBombExplosionPhase.PreExplosionWhite
            ? explosion.RenderedPreExplosionRadius.WholePixels
            : explosion.RenderedExplosionRadius.WholePixels;
        int scanlineDistance = Math.Abs(yFromCenter);
        if (horizontalRadius == 0 || scanlineDistance >= 192)
            return -1;

        // `$88:8CC6/8D04/8D46` all build the same width profile and differ only in how
        // they clip left/right endpoints for an off-screen origin. The desktop renderer
        // performs that clipping after this method, so only the common profile builder
        // is needed here. Widths increase while vertical boundaries decrease.
        int currentOuterScanline =
            SnesEightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.TopOffset(0));
        if (scanlineDistance > currentOuterScanline)
            return -1;

        int selectedHalfWidth = -1;
        int finalHalfWidth = 0;
        for (int curveIndex = 0; curveIndex < 32; curveIndex++)
        {
            // The 65816 routine uses the high byte of an unsigned 8x8 product. An
            // ordinary integer multiply followed by `>> 8` is exactly that operation.
            int innerScanline = SnesEightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.TopOffset(curveIndex));
            int halfWidth = SnesEightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.Width(curveIndex));
            finalHalfWidth = halfWidth;

            // Native code fills both endpoints inclusively, then begins the next band
            // on the same endpoint. Assigning again on a shared boundary intentionally
            // lets the later, wider band win just as the original loop does.
            if (scanlineDistance >= innerScanline && scanlineDistance <= currentOuterScanline)
                selectedHalfWidth = halfWidth;

            currentOuterScanline = innerScanline;
        }

        // After the 32 curve bands, `$88:8DE9/90DF` fills every remaining line through
        // the center with the final (nearly full-radius) width.
        if (scanlineDistance <= currentOuterScanline)
            selectedHalfWidth = finalHalfWidth;

        return selectedHalfWidth;
    }

    /// <summary>Replicates low color bits to expand one SNES five-bit channel to host eight-bit color.</summary>
    internal static byte ExpandFiveBit(int value) =>
        Bgr555.ExpandChannel(value);

    /// <summary>One signed 8.8 screen-space direction vector reconstructed from `$91:C9D4`.</summary>
    internal static Offset16<ScreenPixels> ReadXrayDirection(int angle)
    {
        int wrappedAngle = SnesAngle.NormalizeTableIndex(angle).TableIndex;

        // The four cardinals are mathematically exact. At horizontal entries the ROM table
        // contains `$3C00` as a screen-sized infinity substitute, but using (±1,0) here is
        // the identical limiting ray and also preserves the dedicated zero-width line.
        if (wrappedAngle == SnesAngle.Zero.TableIndex)
            return Direction(0, -SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle == SnesAngle.QuarterTurn.TableIndex)
            return Direction(SamusXrayRomData.Window.UnitVector, 0);
        if (wrappedAngle == SnesAngle.HalfTurn.TableIndex)
            return Direction(0, SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle == SnesAngle.ThreeQuarterTurn.TableIndex)
            return Direction(-SamusXrayRomData.Window.UnitVector, 0);

        int tangentIndex = wrappedAngle & (SnesAngle.HalfTurn.TableIndex - 1);
        int tangent = AbsoluteTangentDefinitions.Sample(tangentIndex);
        if (wrappedAngle < SnesAngle.QuarterTurn.TableIndex)
            return Direction(tangent, -SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle < SnesAngle.HalfTurn.TableIndex)
            return Direction(tangent, SamusXrayRomData.Window.UnitVector);
        if (wrappedAngle < SnesAngle.ThreeQuarterTurn.TableIndex)
            return Direction(-tangent, SamusXrayRomData.Window.UnitVector);
        return Direction(-tangent, -SamusXrayRomData.Window.UnitVector);

        // One screen-space ray direction; the table's largest tangent ($3C00) fits a signed word.
        static Offset16<ScreenPixels> Direction(int x, int y) => new(checked((short)x), checked((short)y));
    }

    /// <summary>Fills a correctly sized gameplay frame with CGRAM color zero before any visible PPU layer is composed.</summary>
    internal static Rgba32[] CreateBackdrop(IPaletteColors cgram, Rgba32[]? outputBuffer = null)
    {
        Ensure.NotNull(cgram);

        // CGRAM color zero is the PPU backdrop. Starting with it (rather than transparent
        // host pixels) matches every screen location no translated layer has covered yet.
        var output = outputBuffer ?? new Rgba32[Width * Height];
        if (output.Length != Width * Height)
            throw new ArgumentException("Gameplay output must have native frame dimensions.", nameof(outputBuffer));
        Array.Fill(output, cgram.GetRgba(0));
        return output;
    }

    /// <summary>Resolves the published eye cone and ROM tangent table into inclusive color windows.</summary>
    public static ScanlineColorAddRenderLayer? CaptureMorphBallEyeBeam(
        MorphBallEyeBeamRenderSnapshot beam, Position16<RoomPixels> layer1)
    {
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or MorphBallEyeBeamPhase.PendingInitialization)
            return null;
        Offset16<ScreenPixels> origin = beam.WorldPosition.RelativeToCamera(layer1);
        int angle = beam.Angle.TableIndex;
        int angularWidth = unchecked((byte)beam.AngularWidth);
        ushort[] nativeWindows = EyeBeamWindowBuilder.Build(origin, angle, angularWidth);
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        byte red = ExpandFiveBit(beam.Red), green = ExpandFiveBit(beam.Green), blue = ExpandFiveBit(beam.Blue);
        for (int y = HudHeight; y < Height; y++)
        {
            int left = SnesLittleEndian.LowByte(nativeWindows[y]), right = SnesLittleEndian.HighByte(nativeWindows[y]);
            if (left <= right) windows[y] = new((byte)left, (byte)right, red, green, blue);
        }
        return new(windows);
    }

    /// <summary>
    /// Captures bank-$88 layer configuration $24's fixed-color addition through window 1,
    /// rasterizing the published beam geometry. This shared layer is consumed by both the
    /// software and Windows Direct3D backends.
    /// </summary>
    public static ScanlineColorAddRenderLayer? CaptureMotherBrainRainbowBeam(MotherBrainRainbowBeamRenderSnapshot beam)
    {
        if (!beam.Active) return null;
        ushort[] nativeWindows = RainbowBeamWindowBuilder.Build(beam.Geometry);
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        byte red = ExpandFiveBit((byte)beam.Color.Red);
        byte green = ExpandFiveBit((byte)beam.Color.Green);
        byte blue = ExpandFiveBit((byte)beam.Color.Blue);
        for (int y = HudHeight; y < Height; y++)
        {
            ushort word = nativeWindows[y];
            byte left = (byte)word, right = SnesLittleEndian.HighByte(word);
            if (left <= right) windows[y] = new(left, right, red, green, blue);
        }
        return new(windows);
    }

    /// <summary>Composes native configuration $16 with both BG2 and BG3 on the subscreen.</summary>
    public static GameplayColorMathRenderLayer CaptureWaterfall(OrdinaryGameplayRenderLayer gameplay,
        RoomLayer3FxRenderSnapshot fx)
    {
        if (fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
            throw new ArgumentException("Waterfall capture requires configuration $16.", nameof(fx));
        var windows = new XrayWindowLine[Height];
        Array.Fill(windows, new XrayWindowLine(255, 0));
        return new(gameplay, windows, false, new ColorMathRegisters { ColorMath = WaterfallRoomDisplayRules.ColorMath, AddSubscreen = true, FixedRed = 0, FixedGreen = 0, FixedBlue = 0 }, CaptureRoomLayer3Fx(fx, gameplay.Rooms.ShownBg3), subscreenUsesBg2: true);
    }

    /// <summary>Resolves FX type, liquid visibility and wave phase into a backend-neutral BG equation.</summary>
    public static Bg2BppColorMathRenderLayer? CaptureRoomLayer3Fx(RoomLayer3FxRenderSnapshot fx, Layer3Cells plane)
    {
        LayerBlendingConfiguration required = fx.Type switch
        {
            RoomFxType.Lava or RoomFxType.Acid => LayerBlendingConfiguration.LavaAcidAdditive,
            RoomFxType.Water or RoomFxType.TourianEntranceStatue => fx.LayerBlendConfiguration,
            RoomFxType.Rain => LayerBlendingConfiguration.Rain,
            RoomFxType.Fog => LayerBlendingConfiguration.FogAdditive,
            RoomFxType.Spores => LayerBlendingConfiguration.Spores,
            RoomFxType.None or
                RoomFxType.ScrollingSky or
                RoomFxType.UnusedScrollingSky or
                RoomFxType.Fireflea or
                RoomFxType.CeresRidley or
                RoomFxType.CeresElevator or
                RoomFxType.CeresHaze => throw new NotSupportedException($"Room FX type {fx.Type} has no captured BG3 compositor."),
            _ => throw new NotSupportedException($"Room FX type {fx.Type} has no captured BG3 compositor."),
        };
        if (RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is not
            (LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive or
            LayerBlendingConfiguration.LiquidOrFogAdditive))
            throw new InvalidDataException("Water has an incompatible layer-blending configuration.");
        if (fx.LayerBlendConfiguration != required) throw new InvalidDataException("Room FX has an incompatible layer-blending configuration.");
        bool liquid = fx.Type is RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid;
        if (liquid && unchecked((short)fx.Surface.Y) < 0) return null;
        bool atmosphere = fx.Type is RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;
        var scrolls = new Position16<BgPlanePixels>[Height];
        int firstSurfaceLine = fx.WaterSurfaceScreenY - (SnesPpuLayout.BackgroundTileSizePixels - 1);
        for (int y = HudHeight; y < Height; y++)
        {
            ushort vertical = liquid && y < firstSurfaceLine ? (ushort)0 : fx.Scroll.Y;
            int wave = RoomFxTypes.UsesWater(fx.Type) && y > fx.WaterSurfaceScreenY
                ? RoomFxRomData.Water.WaveDisplacement((y - fx.WaterSurfaceScreenY - 1 - fx.WaterBg3WavePhase +
                    RoomFxRomData.Water.WaveDisplacementCount) % RoomFxRomData.Water.WaveDisplacementCount) : 0;
            scrolls[y] = new(unchecked((ushort)(fx.Scroll.X + wave)), vertical);
        }
        bool subtract = RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        // Liquids sample the whole plane; atmospheres repeat its lower 32 rows.
        return new(plane, atmosphere ? Layer3Plane.Rows / 2 : 0,
            atmosphere ? Layer3Plane.Rows / 2 : Layer3Plane.Rows,
            HudHeight, subtract ? ExpandedColorMathOperation.Subtract : ExpandedColorMathOperation.Add, scrolls);
    }

    /// <summary>
    /// FX $08 retains the winning main-screen source until color math. A post-frame
    /// overlay would incorrectly brighten foreground terrain (and OBJ palettes 0-3).
    /// Use the same literal Mode-1 composition as the other source-aware effects.
    /// </summary>
    public static GameplayColorMathRenderLayer CaptureSpores(OrdinaryGameplayRenderLayer gameplay,
        RoomLayer3FxRenderSnapshot fx)
    {
        if (fx.Type != RoomFxType.Spores)
            throw new ArgumentException("Spore blending requires a spores FX snapshot.", nameof(fx));
        var emptyWindows = new XrayWindowLine[Height];
        Array.Fill(emptyWindows, new XrayWindowLine(255, 0));
        return new(gameplay, emptyWindows, false, new ColorMathRegisters { ColorMath = (SnesColorMathControl)RoomFxRomData.Spores.ColorMathSources, AddSubscreen = true, FixedRed = 0, FixedGreen = 0, FixedBlue = 0 }, CaptureRoomLayer3Fx(fx, gameplay.Rooms.ShownBg3));
    }

    /// <summary>Resolves the selected Ceres gradient into RGB window rows, with no boss-state reference.</summary>
    public static ScanlineColorAddRenderLayer CaptureCeresHaze(bool ridleyIsDead,
        int intensity = CeresHazeRenderDefinitions.MaximumComponent,
        RoomFxPaletteBlendCatalog? colors = null)
    {
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        for (int y = HudHeight; y < Height; y++)
        {
            Bgr555 haze = CeresHazeRenderDefinitions.ResolveComponents(
                y, intensity, ridleyIsDead, colors);
            windows[y] = new(0, Width - 1, ExpandFiveBit(haze.Red),
                ExpandFiveBit(haze.Green), ExpandFiveBit(haze.Blue));
        }
        return new(windows);
    }

    /// <summary>
    /// Resolves ROM curve data and the rendered explosion phase on the producer side.
    /// The renderer receives only clipped endpoints/colors, not a bus or projectile owner.
    /// </summary>
    public static ScanlineColorAddRenderLayer? CapturePowerBombColorMath(
        PowerBombRenderSnapshot explosion, Position16<RoomPixels> layer1,
        int firstVisibleScanline = HudHeight)
    {
        if ((uint)firstVisibleScanline > Height)
            throw new ArgumentOutOfRangeException(nameof(firstVisibleScanline));
        if (!explosion.IsActive) return null;
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        Offset16<ScreenPixels> center = explosion.Position
            .RelativeToCamera(layer1);
        int centerX = center.X;
        int centerY = center.Y;
        byte red = ExpandFiveBit(explosion.FixedColorRed);
        byte green = ExpandFiveBit(explosion.FixedColorGreen);
        byte blue = ExpandFiveBit(explosion.FixedColorBlue);
        // Gameplay protects its HUD; full-screen cinematics use the same native
        // window profile starting at scanline zero instead.
        for (int y = firstVisibleScanline; y < Height; y++)
        {
            int halfWidth = ReadPowerBombHalfWidth(explosion, y - centerY);
            if (halfWidth < 0) continue;
            int left = Math.Max(0, centerX - halfWidth);
            int right = Math.Min(Width - 1, centerX + halfWidth);
            if (left <= right) windows[y] = new((byte)left, (byte)right, red, green, blue);
        }
        return new(windows);
    }

    /// <summary>Resolves the same tangent inequalities as the reference X-ray raster into immutable line intervals.</summary>
    public static XrayWindowLine[] CaptureXrayWindowLines(SamusXrayDisplaySnapshot xray,
        Position16<RoomPixels> layer1)
    {
        bool leftFacing = xray.FacingLeft;
        Offset16<ScreenPixels> samusOnScreen = xray.SamusPosition.RelativeToCamera(layer1);
        int originX = samusOnScreen.X +
            (leftFacing ? -XrayWindowRenderDefinitions.EyeHorizontalOffset : XrayWindowRenderDefinitions.EyeHorizontalOffset);
        int originY = samusOnScreen.Y -
            (xray.Crouching
                ? XrayWindowRenderDefinitions.CrouchingEyeHeight : XrayWindowRenderDefinitions.StandingEyeHeight);
        int angle = xray.Angle.TableIndex;
        int width = xray.AngularWidth;
        Offset16<ScreenPixels> a = ReadXrayDirection(angle - width), b = ReadXrayDirection(angle + width);
        bool horizontal = width == 0 && (angle == SnesAngle.QuarterTurn.TableIndex || angle == SnesAngle.ThreeQuarterTurn.TableIndex);
        var lines = new XrayWindowLine[Height];
        Array.Fill(lines, new XrayWindowLine(1, 0));
        for (int y = HudHeight; y < Height; y++)
        {
            long left = 0, right = Width - 1;
            if (horizontal)
            {
                if (y != originY) continue;
                if (angle == SnesAngle.QuarterTurn.TableIndex) left = Math.Max(left, originX);
                else right = Math.Min(right, originX);
            }
            else
            {
                long dy = y - originY;
                if (!IntegerWindowIntersection.IntersectGreaterEqual(-a.Y, a.X * dy + (long)a.Y * originX,
                        -XrayWindowRenderDefinitions.SubpixelTolerance, ref left, ref right)) continue;
                if (!IntegerWindowIntersection.IntersectGreaterEqual(b.Y, -(long)b.X * dy - (long)b.Y * originX,
                        -XrayWindowRenderDefinitions.SubpixelTolerance, ref left, ref right)) continue;
            }
            if (left <= right) lines[y] = new((byte)left, (byte)right);
        }
        return lines;
    }
}
