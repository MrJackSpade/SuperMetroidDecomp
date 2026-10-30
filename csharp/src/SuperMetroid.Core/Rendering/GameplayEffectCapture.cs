using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Captures each gameplay effect as the display model describes it, at the frame boundary (#1409): per-line scrolls for
/// the liquids' waves, colours added through window shapes for beams, the Power Bomb and the Ceres haze, the X-ray's cone, and the
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
    public static ushort[]? BuildWaterBackgroundHorizontalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        Position16<BackgroundPixels> bg2Scroll)
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
    public static ushort[]? BuildLavaAcidBackgroundHorizontalScrolls(
        RoomLayer3FxRenderSnapshot fx,
        Position16<BackgroundPixels> bg2Scroll)
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
    public static ushort[]? BuildLavaAcidBackgroundVerticalScrolls(
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

    /// <summary>Converts the eye's aim and half-width into the cone its beam lights.</summary>
    /// <remarks>The cone's apex is one line above the eye's position.</remarks>
    public static ColourAddWindowLayer? CaptureMorphBallEyeBeam(
        MorphBallEyeBeamCone beam, Position16<RoomPixels> layer1)
    {
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or MorphBallEyeBeamPhase.PendingInitialization)
            return null;
        Offset16<ScreenPixels> eye = beam.Origin.RelativeToCamera(layer1);
        int angle = beam.Angle.TableIndex;
        var cone = new WindowCone(eye.WithY(eye.Y - EyeWindowRenderDefinitions.ApexYOffset),
            angle - beam.AngularWidth, angle + beam.AngularWidth);
        return new(ScreenWindow.Of(cone, HudHeight), beam.Color);
    }

    /// <summary>
    /// The rainbow beam's cone from Mother Brain's mouth, below the HUD, in the colour the beam adds. The beam's edge
    /// angles are measured clockwise from straight down, so each is reflected into the screen's angles, which swaps
    /// which edge the cone sweeps from.
    /// </summary>
    public static ColourAddWindowLayer? CaptureMotherBrainRainbowBeam(MotherBrainRainbowBeamRenderSnapshot beam)
    {
        if (!beam.Active) return null;
        if (beam.Geometry is not { } geometry) return new(ScreenWindow.Closed, beam.Color);
        int down = WindowCone.AngleUnitsPerTurn / 2;
        var cone = new WindowCone(Offset16<ScreenPixels>.Of(geometry.MouthScreenX, geometry.MouthY),
            down - geometry.EdgeB, down - geometry.EdgeA);
        return new(ScreenWindow.Of(cone, MotherBrainBeamDefinitions.FirstLine), beam.Color);
    }

    /// <summary>Composes native configuration $16 with both BG2 and BG3 on the subscreen.</summary>
    public static GameplayBlendLayer CaptureWaterfall(GameplaySceneLayer gameplay,
        RoomLayer3FxRenderSnapshot fx)
    {
        if (fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
            throw new ArgumentException("Waterfall capture requires configuration $16.", nameof(fx));
        return new(gameplay, ScreenWindow.Closed, false, new SceneBlend { Layers = WaterfallRoomDisplayRules.Blend, BlendsWithLayersBehind = true, FixedColor = RgbColor.Black }, CaptureRoomLayer3Fx(fx, gameplay.Rooms.ShownBg3), subscreenUsesBg2: true);
    }

    /// <summary>Resolves FX type, liquid visibility and wave phase into a backend-neutral BG equation.</summary>
    public static OverlayBlendLayer? CaptureRoomLayer3Fx(RoomLayer3FxRenderSnapshot fx, Layer3Cells plane)
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
        var scrolls = new Position16<TilePlanePixels>[Height];
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
            HudHeight, subtract ? BlendOperation.Subtract : BlendOperation.Add, scrolls);
    }

    /// <summary>
    /// FX $08 retains the winning main-screen source until color math. A post-frame
    /// overlay would incorrectly brighten foreground terrain (and OBJ palettes 0-3).
    /// Use the same literal Mode-1 composition as the other source-aware effects.
    /// </summary>
    public static GameplayBlendLayer CaptureSpores(GameplaySceneLayer gameplay,
        RoomLayer3FxRenderSnapshot fx)
    {
        if (fx.Type != RoomFxType.Spores)
            throw new ArgumentException("Spore blending requires a spores FX snapshot.", nameof(fx));
        return new(gameplay, ScreenWindow.Closed, false, new SceneBlend { Layers = AtmosphereDriftDefinitions.SporesBlendedLayers, BlendsWithLayersBehind = true, FixedColor = RgbColor.Black }, CaptureRoomLayer3Fx(fx, gameplay.Rooms.ShownBg3));
    }

    /// <summary>
    /// The Ceres haze: its tint added across the gameplay area, strengthening in bands down the screen and dimmed by
    /// how far the haze has faded, with no boss-state reference.
    /// </summary>
    public static ColourAddWindowLayer CaptureCeresHaze(bool ridleyIsDead,
        int intensity = CeresHazeRenderDefinitions.MaximumComponent,
        RoomFxBlendInks? colors = null)
    {
        RgbColor tint = colors is null
            ? RoomFxPaletteBlendRules.StockCeresHazeTint(ridleyIsDead)
            : ridleyIsDead ? colors.CeresHazeRed : colors.CeresHazeBlue;
        // The fade's strength is relative to the whole haze: below it the bands dim, and the fade-out's first frame
        // shows one step more than whole.
        var steps = new LineColourSteps(CeresHazeRenderDefinitions.RampFirstLine, CeresHazeRenderDefinitions.BandHeight,
            CeresHazeRenderDefinitions.MaximumComponent, intensity - CeresHazeRenderDefinitions.MaximumComponent);
        return new(ScreenWindow.Of(WindowRectangle.Spanning(0, HudHeight, Width - 1, Height - 1), HudHeight), tint, steps);
    }

    /// <summary>
    /// The Power Bomb's blast as an oval of added colour around its centre (#1468). While the blast walks its authored
    /// yellow or white ovals, and through the afterglow that keeps the last of them, the oval is the walked one;
    /// otherwise it is the blast's current radius. The oval is three quarters as tall as it is wide.
    /// </summary>
    /// <param name="explosion">The blast as the frame shows it.</param>
    /// <param name="layer1">The camera, which places the blast on the screen.</param>
    /// <param name="firstVisibleScanline">The first line the blast shows on: below the HUD in gameplay, the top of the screen in cinematics.</param>
    public static ColourAddWindowLayer? CapturePowerBombGlow(
        PowerBombRenderSnapshot explosion, Position16<RoomPixels> layer1,
        int firstVisibleScanline = HudHeight)
    {
        if ((uint)firstVisibleScanline > Height)
            throw new ArgumentOutOfRangeException(nameof(firstVisibleScanline));
        if (!explosion.IsActive) return null;
        int radius = PowerBombHorizontalRadius(explosion);
        int verticalRadius = radius * PowerBombExplosionDefinitions.OvalHeightParts / PowerBombExplosionDefinitions.OvalWidthParts;
        ScreenWindow window = verticalRadius == 0 ? ScreenWindow.Closed
            : ScreenWindow.Of(WindowOval.Ellipse(explosion.Position.RelativeToCamera(layer1),
                new Extent16<ScreenPixels>(checked((ushort)radius), checked((ushort)verticalRadius))),
                firstVisibleScanline);
        return new(window, explosion.FixedColor);
    }

    /// <summary>The horizontal radius, in whole pixels, of the blast's oval this frame; zero shows nothing.</summary>
    private static int PowerBombHorizontalRadius(PowerBombRenderSnapshot explosion)
    {
        PowerBombExplosionPhase phase = explosion.RenderedPhase;
        // A blast spawns after this frame's oval was chosen, so it shows nothing until the next frame.
        if (phase == PowerBombExplosionPhase.Inactive) return 0;
        // The afterglow only fades the colour: the last white oval stays.
        if (phase is PowerBombExplosionPhase.PreExplosionYellow or PowerBombExplosionPhase.ExplosionWhite
                or PowerBombExplosionPhase.Afterglow && explosion.RenderedShape is { } shape)
            return shape.HorizontalRadius();
        return phase == PowerBombExplosionPhase.PreExplosionWhite
            ? explosion.RenderedPreExplosionRadius.WholePixels
            : explosion.RenderedExplosionRadius.WholePixels;
    }

    /// <summary>
    /// The X-ray scope's cone from Samus's eye (#1468): three pixels ahead of her, at her standing or crouching eye
    /// height, sweeping the scope's half-width either side of its aim, below the HUD.
    /// </summary>
    public static ScreenWindow CaptureXrayWindow(SamusXrayDisplaySnapshot xray, Position16<RoomPixels> layer1)
    {
        Offset16<ScreenPixels> samusOnScreen = xray.SamusPosition.RelativeToCamera(layer1);
        int eyeX = samusOnScreen.X +
            (xray.FacingLeft ? -XrayWindowRenderDefinitions.EyeHorizontalOffset : XrayWindowRenderDefinitions.EyeHorizontalOffset);
        int eyeY = samusOnScreen.Y -
            (xray.Crouching ? XrayWindowRenderDefinitions.CrouchingEyeHeight : XrayWindowRenderDefinitions.StandingEyeHeight);
        int angle = xray.Angle.TableIndex;
        var cone = new WindowCone(Offset16<ScreenPixels>.Of(eyeX, eyeY), angle - xray.AngularWidth, angle + xray.AngularWidth);
        return ScreenWindow.Of(cone, HudHeight);
    }

    /// <summary>
    /// The suit pickup's light beam as a colour added through its window (#1468), captured once; nothing later reads
    /// the pickup. The beam's lines run from the top of the screen.
    /// </summary>
    public static ColourAddWindowLayer? CaptureSuitPickup(SamusSuitPickupFlash? flash)
    {
        if (flash is not { } active) return null;
        return new(SuitPickupBeamWindow(active.Beam), active.Color);
    }

    /// <summary>The window of each light-beam outline: columns, a rounded column, its wiped remainder or the centre line.</summary>
    private static ScreenWindow SuitPickupBeamWindow(SamusSuitPickupBeam beam)
    {
        int left = beam.Left.X, right = beam.Right.X;
        int lines = SuitPickupBeamDefinitions.BeamLines;
        int centre = lines / 2;
        if (left > right) return ScreenWindow.Closed;
        switch (beam.Shape)
        {
            case SamusSuitPickupBeamShape.None:
                return ScreenWindow.Closed;
            case SamusSuitPickupBeamShape.GrowingFromEdges:
            {
                int grown = Math.Min(beam.Progress, lines);
                if (grown <= 0) return ScreenWindow.Closed;
                return new([WindowRectangle.Spanning(left, 0, right, grown - 1),
                    WindowRectangle.Spanning(left, lines - grown, right, lines - 1)]);
            }
            case SamusSuitPickupBeamShape.Column:
                return ScreenWindow.Of(WindowRectangle.Spanning(left, 0, right, lines - 1));
            case SamusSuitPickupBeamShape.CurvedColumn:
                return ScreenWindow.Of(CurvedColumn(left, right, centre));
            case SamusSuitPickupBeamShape.ClosingCurvedColumn:
            {
                // The upper wipe always clears at least the first line; the lower wipe is one line shorter.
                int topWiped = Math.Clamp(beam.Progress, 1, lines);
                int bottomWiped = Math.Clamp(beam.Progress - 1, 1, lines);
                int first = Math.Min(topWiped, Height);
                int end = Math.Clamp(lines - bottomWiped, first, Height);
                return ScreenWindow.Of(CurvedColumn(left, right, centre), first, end);
            }
            case SamusSuitPickupBeamShape.CenterLine:
                return ScreenWindow.Of(WindowRectangle.Spanning(left, centre, right, centre));
            default:
                throw new ArgumentOutOfRangeException(nameof(beam), beam.Shape, "Unknown suit-pickup beam shape.");
        }
    }

    /// <summary>The beam's column widened on each line by its rounded sides, widest at the middle line.</summary>
    private static WindowOval CurvedColumn(int left, int right, int centre) =>
        new(Offset16<ScreenPixels>.Of(left, centre), right - left,
            new Extent16<ScreenPixels>(SuitPickupBeamDefinitions.CurveWidth, SuitPickupBeamDefinitions.BeamLines / 2));
}
