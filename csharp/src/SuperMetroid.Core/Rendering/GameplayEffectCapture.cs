using System.Collections.Immutable;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Captures each gameplay effect as the display model describes it, at the frame boundary (#1409): sampled waves for
/// the liquids' waves and the heat haze (#1468), colours added through window shapes for beams, the Power Bomb and the
/// Ceres haze, the X-ray's cone, and the
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
    /// <summary>The liquids' and the heat haze's amplitude: a sample of one moves a line one whole pixel, in 1/256 pixel.</summary>
    private static readonly int OnePixelPerSample = EightEight.FromWhole(1);

    /// <summary>
    /// Describes water's optional background wave (#1468): below the surface, background's horizontal scroll follows
    /// the water waveform one sample a line, counted from the line below the surface and advanced by the wave's phase and
    /// The background's fine vertical scroll; the lines above keep background's own scroll. Null without the wave.
    /// </summary>
    public static SampledScrollWave? BuildWaterBackgroundHorizontalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        Position16<BackgroundPixels> backgroundScroll)
    {
        if (fx.Type != RoomFxType.Water || (fx.LiquidOptions & RoomLiquidOptions.BackgroundWave) == RoomLiquidOptions.None)
            return null;
        int belowSurface = fx.WaterSurfaceScreenY + 1;
        return new SampledScrollWave(backgroundScroll.X, LiquidEffects.WaterWaveform, OnePixelPerSample,
            SampledScrollWave.PhaseWhere(fx.WaterBackgroundWavePhase + (backgroundScroll.Y & 15), belowSurface, LiquidEffects.WaterWaveform.Length),
            Math.Clamp(belowSurface, HudHeight, Height), Height);
    }

    /// <summary>
    /// Describes the optional lava/acid the background's horizontal scroll heat haze (#1468), or null without it. Vertical
    /// distortion takes priority when both option bits are set, as the game checks it first.
    /// </summary>
    public static SampledScrollWave? BuildLavaAcidBackgroundHorizontalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        Position16<BackgroundPixels> backgroundScroll)
    {
        bool isLavaAcid = fx.Type is RoomFxType.Lava or RoomFxType.Acid;
        bool usesVertical =
            (fx.LiquidOptions & RoomLiquidOptions.BackgroundWave) != RoomLiquidOptions.None;
        bool usesHorizontal =
            (fx.LiquidOptions & RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave) != RoomLiquidOptions.None;
        if (!isLavaAcid || usesVertical || !usesHorizontal)
            return null;

        return HeatHaze(backgroundScroll.X, backgroundScroll.Y, fx.LavaAcidBackgroundWavePhase, LiquidEffects.HeatHorizontalWaveform);
    }

    /// <summary>Describes the Norfair heat-haze the background's vertical scroll wave (#1468), or null without it.</summary>
    public static SampledScrollWave? BuildLavaAcidBackgroundVerticalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        int backgroundVerticalScroll)
    {
        if (fx.Type is not (RoomFxType.Lava or RoomFxType.Acid) ||
            (fx.LiquidOptions & RoomLiquidOptions.BackgroundWave) == RoomLiquidOptions.None)
        {
            return null;
        }

        // The vertical haze repeats the water wave's samples.
        return HeatHaze(backgroundVerticalScroll, backgroundVerticalScroll, fx.LavaAcidBackgroundWavePhase, LiquidEffects.WaterWaveform);
    }

    /// <summary>
    /// The rotating sixteen-sample heat haze over every gameplay line (#1468). The haze is sixteen one-line
    /// entries repeated through the frame: it starts at the background's vertical scroll's low nibble, while each frame
    /// the sixteen resident words backward through A, so the first gameplay line reaches the sample at that nibble plus the
    /// wave's phase.
    /// </summary>
    private static SampledScrollWave HeatHaze(int baseScroll, int backgroundVerticalScroll, int wavePhase, ImmutableArray<int> waveform) =>
        new(baseScroll, waveform, OnePixelPerSample,
            SampledScrollWave.PhaseWhere((backgroundVerticalScroll & 15) + wavePhase, HudHeight, waveform.Length),
            HudHeight, Height);

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

    /// <summary>
    /// The waterfall rooms' blend (#1514): the waterfall, the background and the room's liquid lying behind the foreground,
    /// darken the foreground, the actors taking part in blends and the backdrop, unhalved, everywhere on the screen.
    /// </summary>
    public static GameplayBlendLayer CaptureWaterfall(GameplaySceneLayer gameplay,
        RoomFxOverlayRenderSnapshot fx)
    {
        if (fx.LayerBlendConfiguration != LayerBlendingConfiguration.WaterfallSubtractive)
            throw new ArgumentException("Waterfall capture requires the waterfall rooms' subtractive blend.", nameof(fx));
        return new(gameplay, ScreenWindow.Closed, false, new GameplayBlend
        {
            Blended = BlendedLayers.Foreground | BlendedLayers.Actors | BlendedLayers.Backdrop,
            Operation = BlendOperation.Subtract,
            Halved = false,
            Colour = RgbColor.Black,
        }, CaptureRoomFxOverlay(fx, gameplay.Rooms.ShownFxOverlay), backgroundBehind: true);
    }

    /// <summary>Resolves FX type, liquid visibility and wave phase into a backend-neutral BG equation.</summary>
    public static OverlayBlendLayer? CaptureRoomFxOverlay(RoomFxOverlayRenderSnapshot fx, FxOverlayCells plane)
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
                RoomFxType.CeresHaze => throw new NotSupportedException($"Room FX type {fx.Type} has no captured effect overlay compositor."),
            _ => throw new NotSupportedException($"Room FX type {fx.Type} has no captured effect overlay compositor."),
        };
        if (RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is not
            (LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive or
            LayerBlendingConfiguration.LiquidOrFogAdditive))
            throw new InvalidDataException("Water has an incompatible layer-blending configuration.");
        if (fx.LayerBlendConfiguration != required) throw new InvalidDataException("Room FX has an incompatible layer-blending configuration.");
        bool liquid = fx.Type is RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid;
        if (liquid && CoordinateAxis.AsSigned(fx.Surface.Y) < 0) return null;
        bool atmosphere = fx.Type is RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;
        // Below a water surface the plane moves sideways with the water waveform, one sample a line counted from the line
        // below the surface and drawn back by the wave's phase (#1468).
        int belowSurface = fx.WaterSurfaceScreenY + 1;
        LineScroll horizontal = RoomFxTypes.UsesWater(fx.Type)
            ? new SampledScrollWave(fx.Scroll.X, LiquidEffects.WaterWaveform, OnePixelPerSample,
                SampledScrollWave.PhaseWhere(-fx.WaterOverlayWavePhase, belowSurface, LiquidEffects.WaterWaveform.Length),
                Math.Clamp(belowSurface, HudHeight, Height), Height)
            : ScrollBands.Uniform(fx.Scroll.X);
        // A liquid's lines above the tile row holding its surface show the plane's top row.
        int firstSurfaceLine = fx.WaterSurfaceScreenY - (ScreenGeometry.BackgroundTileSizePixels - 1);
        LineScroll vertical = liquid ? ScrollBands.Split(0, firstSurfaceLine, fx.Scroll.Y) : ScrollBands.Uniform(fx.Scroll.Y);
        bool subtract = RoomFxTypes.UsesWater(fx.Type) && fx.LayerBlendConfiguration is
            LayerBlendingConfiguration.WaterSubtractive or LayerBlendingConfiguration.WaterfallSubtractive;
        // Liquids sample the whole plane; atmospheres repeat its lower 32 rows.
        return new(plane, atmosphere ? FxOverlayPlane.Rows / 2 : 0,
            atmosphere ? FxOverlayPlane.Rows / 2 : FxOverlayPlane.Rows,
            HudHeight, subtract ? BlendOperation.Subtract : BlendOperation.Add, horizontal, vertical);
    }

    /// <summary>
    /// The spores' blend (#1514): the spores, lying behind, brighten the background, the actors taking part in blends and
    /// the backdrop where each is frontmost, everywhere on the screen; the foreground keeps its colour, so the spores never
    /// brighten the room's terrain, as an overlay added over the finished frame would.
    /// </summary>
    public static GameplayBlendLayer CaptureSpores(GameplaySceneLayer gameplay,
        RoomFxOverlayRenderSnapshot fx)
    {
        if (fx.Type != RoomFxType.Spores)
            throw new ArgumentException("Spore blending requires a spores FX snapshot.", nameof(fx));
        return new(gameplay, ScreenWindow.Closed, false, new GameplayBlend
        {
            Blended = BlendedLayers.Background | BlendedLayers.Actors | BlendedLayers.Backdrop,
            Operation = BlendOperation.Add,
            Halved = false,
            Colour = RgbColor.Black,
        }, CaptureRoomFxOverlay(fx, gameplay.Rooms.ShownFxOverlay));
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
    /// <param name="firstLine">The first screen line the blast covers: the playfield's first line below the HUD in gameplay, the top of the screen in cinematics.</param>
    public static ColourAddWindowLayer? CapturePowerBombGlow(
        PowerBombRenderSnapshot explosion, Position16<RoomPixels> layer1,
        int firstLine = HudHeight)
    {
        if ((uint)firstLine > Height)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        if (!explosion.IsActive) return null;
        int radius = PowerBombHorizontalRadius(explosion);
        int verticalRadius = radius * PowerBombExplosionDefinitions.OvalHeightParts / PowerBombExplosionDefinitions.OvalWidthParts;
        ScreenWindow window = verticalRadius == 0 ? ScreenWindow.Closed
            : ScreenWindow.Of(WindowOval.Ellipse(explosion.Position.RelativeToCamera(layer1),
                new Extent16<ScreenPixels>(radius, verticalRadius)),
                firstLine);
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
