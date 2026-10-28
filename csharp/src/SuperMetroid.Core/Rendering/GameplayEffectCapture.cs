using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Captures each gameplay effect as the display model describes it, at the frame boundary (#1409): per-line scrolls for
/// the liquids' waves, colour windows for beams, the Power Bomb and the Ceres haze, the X-ray's window lines, and the
/// room effects' layer blends. The backend draws these descriptions; nothing here draws.
/// </summary>
public static class GameplayEffectCapture
{
    /// <summary>The screen's width in pixels.</summary>
    private const int Width = DisplayScreen.Width;
    /// <summary>The screen's height in lines.</summary>
    private const int Height = DisplayScreen.Height;
    /// <summary>The lines the HUD fills at the top of the screen.</summary>
    private const int HudHeight = DisplayScreen.HudHeight;

    /// <summary>
    /// Resolves water's optional per-scanline BG2 distortion from <c>$88:C5E4</c>. The
    /// ordinary compositor already accepts the resulting 192-line HDMA projection.
    /// </summary>
    public static ushort[]? BuildWaterBg2HorizontalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        Position16<Bg2Pixels> bg2Scroll)
    {
        if (fx.Type != RoomFxType.Water || (fx.LiquidOptions & RoomLiquidOptions.Bg2Wave) == RoomLiquidOptions.None)
            return null;

        var result = Enumerable.Repeat(bg2Scroll.X, Height - HudHeight).ToArray();
        int verticalPhase = bg2Scroll.Y & 15;
        for (int screenY = Math.Max(HudHeight, fx.WaterSurfaceScreenY + 1);
             screenY < Height;
             screenY++)
        {
            int index = (fx.WaterBg2WavePhase + verticalPhase +
                screenY - fx.WaterSurfaceScreenY - 1) &
                (WaterWaveDefinitions.WaveDisplacementCount - 1);
            result[screenY - HudHeight] = unchecked((ushort)(
                bg2Scroll.X + LiquidEffects.WaterWaveDisplacement(index)));
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
            (fx.LiquidOptions & RoomLiquidOptions.Bg2Wave) != RoomLiquidOptions.None;
        bool usesHorizontal =
            (fx.LiquidOptions & RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave) != RoomLiquidOptions.None;
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
        int bg2VerticalScroll)
    {
        if (fx.Type is not (RoomFxType.Lava or RoomFxType.Acid) ||
            (fx.LiquidOptions & RoomLiquidOptions.Bg2Wave) == RoomLiquidOptions.None)
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
        int baseScroll,
        int bg2VerticalScroll,
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
                (LavaAcidWaveDefinitions.WaveDisplacementCount - 1);
            int displacement = axis == Axis.Vertical
                ? LiquidEffects.HeatVerticalWaveDisplacement(waveIndex)
                : LiquidEffects.HeatHorizontalWaveDisplacement(waveIndex);
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
            int halfWidth = PowerBombShapeDefinitions.ReadPreScaledHalfWidth(
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
            EightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.TopOffset(0));
        if (scanlineDistance > currentOuterScanline)
            return -1;

        int selectedHalfWidth = -1;
        int finalHalfWidth = 0;
        for (int curveIndex = 0; curveIndex < 32; curveIndex++)
        {
            // The 65816 routine uses the high byte of an unsigned 8x8 product. An
            // ordinary integer multiply followed by `>> 8` is exactly that operation.
            int innerScanline = EightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.TopOffset(curveIndex));
            int halfWidth = EightEight.Multiply(horizontalRadius, PowerBombShapeDefinitions.Width(curveIndex));
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
        RgbColor.ExpandChannel(value);

    /// <summary>One signed 8.8 screen-space direction vector reconstructed from `$91:C9D4`.</summary>
    internal static Offset16<ScreenPixels> ReadXrayDirection(int angle)
    {
        int wrappedAngle = RotationAngle.NormalizeTableIndex(angle).TableIndex;

        // The four cardinals are mathematically exact. At horizontal entries the ROM table
        // contains `$3C00` as a screen-sized infinity substitute, but using (±1,0) here is
        // the identical limiting ray and also preserves the dedicated zero-width line.
        if (wrappedAngle == RotationAngle.Zero.TableIndex)
            return Direction(0, -XrayScopeDefinitions.Window.UnitVector);
        if (wrappedAngle == RotationAngle.QuarterTurn.TableIndex)
            return Direction(XrayScopeDefinitions.Window.UnitVector, 0);
        if (wrappedAngle == RotationAngle.HalfTurn.TableIndex)
            return Direction(0, XrayScopeDefinitions.Window.UnitVector);
        if (wrappedAngle == RotationAngle.ThreeQuarterTurn.TableIndex)
            return Direction(-XrayScopeDefinitions.Window.UnitVector, 0);

        int tangentIndex = wrappedAngle & (RotationAngle.HalfTurn.TableIndex - 1);
        int tangent = AbsoluteTangentDefinitions.Sample(tangentIndex);
        if (wrappedAngle < RotationAngle.QuarterTurn.TableIndex)
            return Direction(tangent, -XrayScopeDefinitions.Window.UnitVector);
        if (wrappedAngle < RotationAngle.HalfTurn.TableIndex)
            return Direction(tangent, XrayScopeDefinitions.Window.UnitVector);
        if (wrappedAngle < RotationAngle.ThreeQuarterTurn.TableIndex)
            return Direction(-tangent, XrayScopeDefinitions.Window.UnitVector);
        return Direction(-tangent, -XrayScopeDefinitions.Window.UnitVector);

        // One screen-space ray direction; the table's largest tangent ($3C00) fits a signed word.
        static Offset16<ScreenPixels> Direction(int x, int y) => new(checked((short)x), checked((short)y));
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
        ushort[] windowEdges = EyeBeamWindowBuilder.Build(origin, angle, angularWidth);
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        int red = ExpandFiveBit(beam.Color.Red), green = ExpandFiveBit(beam.Color.Green), blue = ExpandFiveBit(beam.Color.Blue);
        for (int y = HudHeight; y < Height; y++)
        {
            int left = ScanlineSpanWord.Left(windowEdges[y]), right = ScanlineSpanWord.Right(windowEdges[y]);
            if (left <= right) windows[y] = new(left, right, red, green, blue);
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
        MotherBrainBeamLine[] lines = RainbowBeamWindowBuilder.Build(beam.Geometry);
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        int red = ExpandFiveBit((byte)beam.Color.Red);
        int green = ExpandFiveBit((byte)beam.Color.Green);
        int blue = ExpandFiveBit((byte)beam.Color.Blue);
        for (int y = HudHeight; y < Height; y++)
        {
            MotherBrainBeamLine line = lines[y];
            if (line.Crosses) windows[y] = new(line.Left, line.Right, red, green, blue);
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
        return new(gameplay, windows, false, new SceneBlend { Layers = WaterfallRoomDisplayRules.Blend, BlendsWithSubscreen = true, FixedColor = RgbColor.Black }, CaptureRoomLayer3Fx(fx, gameplay.Rooms.ShownBg3), subscreenUsesBg2: true);
    }

    /// <summary>Resolves FX type, liquid visibility and wave phase into a backend-neutral BG equation.</summary>
    public static FxPlaneBlendRenderLayer? CaptureRoomLayer3Fx(RoomLayer3FxRenderSnapshot fx, Layer3Cells plane)
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
        int firstSurfaceLine = fx.WaterSurfaceScreenY - (ScreenGeometry.BackgroundTileSizePixels - 1);
        for (int y = HudHeight; y < Height; y++)
        {
            int wave = RoomFxTypes.UsesWater(fx.Type) && y > fx.WaterSurfaceScreenY
                ? LiquidEffects.WaterWaveDisplacement((y - fx.WaterSurfaceScreenY - 1 - fx.WaterBg3WavePhase +
                    WaterWaveDefinitions.WaveDisplacementCount) % WaterWaveDefinitions.WaveDisplacementCount) : 0;
            scrolls[y] = new(unchecked((ushort)(fx.Scroll.X + wave)), liquid && y < firstSurfaceLine ? (ushort)0 : fx.Scroll.Y);
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
        return new(gameplay, emptyWindows, false, new SceneBlend { Layers = AtmosphereDriftDefinitions.SporesBlendedLayers, BlendsWithSubscreen = true, FixedColor = RgbColor.Black }, CaptureRoomLayer3Fx(fx, gameplay.Rooms.ShownBg3));
    }

    /// <summary>Resolves the selected Ceres gradient into RGB window rows, with no boss-state reference.</summary>
    public static ScanlineColorAddRenderLayer CaptureCeresHaze(bool ridleyIsDead,
        int intensity = CeresHazeRenderDefinitions.MaximumComponent,
        RoomFxBlendInks? colors = null)
    {
        var windows = new ColorAddWindow[Height];
        Array.Fill(windows, ColorAddWindow.Empty);
        for (int y = HudHeight; y < Height; y++)
        {
            RgbColor haze = CeresHazeRenderDefinitions.ResolveComponents(
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
        int red = ExpandFiveBit(explosion.FixedColor.Red);
        int green = ExpandFiveBit(explosion.FixedColor.Green);
        int blue = ExpandFiveBit(explosion.FixedColor.Blue);
        // Gameplay protects its HUD; full-screen cinematics use the same native
        // window profile starting at scanline zero instead.
        for (int y = firstVisibleScanline; y < Height; y++)
        {
            int halfWidth = ReadPowerBombHalfWidth(explosion, y - centerY);
            if (halfWidth < 0) continue;
            int left = Math.Max(0, centerX - halfWidth);
            int right = Math.Min(Width - 1, centerX + halfWidth);
            if (left <= right) windows[y] = new(left, right, red, green, blue);
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
        bool horizontal = width == 0 && (angle == RotationAngle.QuarterTurn.TableIndex || angle == RotationAngle.ThreeQuarterTurn.TableIndex);
        var lines = new XrayWindowLine[Height];
        Array.Fill(lines, new XrayWindowLine(1, 0));
        for (int y = HudHeight; y < Height; y++)
        {
            long left = 0, right = Width - 1;
            if (horizontal)
            {
                if (y != originY) continue;
                if (angle == RotationAngle.QuarterTurn.TableIndex) left = Math.Max(left, originX);
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

    /// <summary>The suit pickup's light beam as a colour added through its window, captured once; nothing later reads the pickup.</summary>
    public static ScanlineColorAddRenderLayer? CaptureSuitPickup(SamusSuitPickupFlash? flash)
    {
        if (flash is not { } active) return null;
        ushort[] endpoints = SuitPickupBeamWindowBuilder.Build(active.Beam);
        var windows = new ColorAddWindow[Height];
        int red = ExpandFiveBit(active.Color.Red);
        int green = ExpandFiveBit(active.Color.Green);
        int blue = ExpandFiveBit(active.Color.Blue);
        for (int y = 0; y < Height; y++)
            windows[y] = new(ScanlineSpanWord.Left(endpoints[y]), ScanlineSpanWord.Right(endpoints[y]), red, green, blue);
        return new(windows);
    }
}
