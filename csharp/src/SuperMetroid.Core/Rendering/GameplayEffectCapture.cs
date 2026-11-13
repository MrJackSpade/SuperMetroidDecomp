using System.Collections.Immutable;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Captures each gameplay effect as the display model describes it, at the frame boundary (#1409): sampled waves for
/// the liquids' waves and the heat haze (#1468), colours added through window shapes for beams, the Power Bomb and the
/// Ceres haze, the X-ray's cone, and the
/// room effects' own blends (#1545). The backend draws these descriptions; nothing here draws.
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
    /// the background's vertical scroll within its 16-pixel block; the lines above keep background's own scroll. Null
    /// without the wave, or while the frame places no water surface.
    /// </summary>
    public static SampledScrollWave? BuildWaterBackgroundHorizontalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        Position16<BackgroundPixels> backgroundScroll)
    {
        if (fx.Type != RoomFxType.Water || fx.Liquid is not RoomWater { BackgroundWave: true } ||
            fx.WaterSurfaceScreenY is not int surfaceLine)
            return null;
        int belowSurface = surfaceLine + 1;
        return new SampledScrollWave(backgroundScroll.X, LiquidEffects.WaterWaveform, OnePixelPerSample,
            SampledScrollWave.PhaseWhere(fx.WaterBackgroundWavePhase + PhaseInBackgroundBlock(backgroundScroll.Y), belowSurface, LiquidEffects.WaterWaveform.Length),
            Math.Clamp(belowSurface, HudHeight, Height), Height);
    }

    /// <summary>
    /// Describes lava's or acid's horizontal heat haze over the background's horizontal scroll (#1468), or null for a liquid
    /// without it.
    /// </summary>
    public static SampledScrollWave? BuildLavaAcidBackgroundHorizontalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        Position16<BackgroundPixels> backgroundScroll)
    {
        if (fx.Liquid is not RoomLavaOrAcid { Haze: HeatHazeDirection.Horizontal })
            return null;

        return HeatHaze(backgroundScroll.X, backgroundScroll.Y, fx.LavaAcidBackgroundWavePhase, LiquidEffects.HeatHorizontalWaveform);
    }

    /// <summary>
    /// Describes the Norfair heat-haze the background's vertical scroll wave (#1468), or null without it: around the
    /// background row the screen's top shows, below the camera row it hides (#1514), its samples counted from the
    /// camera's row.
    /// </summary>
    /// <param name="fx">The room effect the frame shows.</param>
    /// <param name="backgroundVerticalScroll">The background camera's top row.</param>
    public static SampledScrollWave? BuildLavaAcidBackgroundVerticalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        int backgroundVerticalScroll)
    {
        if (fx.Liquid is not RoomLavaOrAcid { Haze: HeatHazeDirection.Vertical })
            return null;

        // The vertical haze repeats the water wave's samples. Its lines move about the row the screen's top shows, while
        // its phase stays counted from the camera's row.
        return HeatHaze(GameplaySceneView.LayerRowAtScreenTop(backgroundVerticalScroll), backgroundVerticalScroll,
            fx.LavaAcidBackgroundWavePhase, LiquidEffects.WaterWaveform);
    }

    /// <summary>
    /// The rotating sixteen-sample heat haze over every gameplay line (#1468). The haze repeats its sixteen samples, one a
    /// line, down the frame: it starts at the background's vertical scroll within its 16-pixel block, while each frame
    /// rotates the sixteen samples backward, so the first gameplay line reaches the sample at that place plus the wave's
    /// phase.
    /// </summary>
    private static SampledScrollWave HeatHaze(int baseScroll, int backgroundVerticalScroll, int wavePhase, ImmutableArray<int> waveform) =>
        new(baseScroll, waveform, OnePixelPerSample,
            SampledScrollWave.PhaseWhere(PhaseInBackgroundBlock(backgroundVerticalScroll) + wavePhase, HudHeight, waveform.Length),
            HudHeight, Height);

    /// <summary>
    /// Where the background's vertical scroll lies within the background's 16-pixel block: the row of its block the
    /// camera's top row is on, zero through fifteen, counted downward for a scroll above the room's top as well.
    /// </summary>
    private static int PhaseInBackgroundBlock(int backgroundVerticalScroll) =>
        ((backgroundVerticalScroll % RoomLevelPlane.BlockPixels) + RoomLevelPlane.BlockPixels) % RoomLevelPlane.BlockPixels;

    /// <summary>Converts the eye's aim and half-width into the cone its beam lights.</summary>
    /// <remarks>The cone's apex is one line above the eye's position.</remarks>
    public static ColourAddWindowLayer? CaptureMorphBallEyeBeam(
        MorphBallEyeBeamCone beam, Position16<RoomPixels> cameraPosition)
    {
        if (beam.Phase is MorphBallEyeBeamPhase.Inactive or MorphBallEyeBeamPhase.PendingInitialization)
            return null;
        Offset16<ScreenPixels> eye = beam.Origin.RelativeToCamera(cameraPosition);
        TrigonometryAngle angle = beam.Angle.ToTrigonometryAngle();
        var cone = new WindowCone(eye.WithY(eye.Y - EyeWindowRenderDefinitions.ApexYOffset),
            angle - beam.AngularWidth, angle + beam.AngularWidth);
        return new(ScreenWindow.Of(cone, HudHeight), beam.Color);
    }

    /// <summary>
    /// The rainbow beam's cone from Mother Brain's mouth, placed against the frame's camera, below the HUD, in the colour
    /// the beam adds. The beam's edge angles are measured from straight down turning toward its right, so each is
    /// reflected into the screen's angles, which swaps which edge the cone sweeps from.
    /// </summary>
    /// <param name="beam">The shown beam.</param>
    /// <param name="cameraPosition">The camera, which places the mouth on the screen.</param>
    public static ColourAddWindowLayer CaptureMotherBrainRainbowBeam(MotherBrainRainbowBeamRenderSnapshot beam,
        Position16<RoomPixels> cameraPosition)
    {
        var cone = new WindowCone(beam.Cone.Mouth.RelativeToCamera(cameraPosition),
            TrigonometryAngle.HalfTurn - beam.Cone.UpperEdge, TrigonometryAngle.HalfTurn - beam.Cone.LowerEdge);
        return new(ScreenWindow.Of(cone, HudHeight), beam.Color);
    }

    /// <summary>
    /// The blend of a room effect whose overlay lies behind the room (#1514, #1545): the overlay, and the background where
    /// the effect lays it behind too, change the layers the effect's blend names where each is frontmost, unhalved,
    /// everywhere on the screen. The spores brighten the background, the actors taking part in blends and the backdrop, so
    /// they never brighten the room's terrain; the waterfall darkens the foreground, those actors and the backdrop.
    /// </summary>
    public static GameplayBlendLayer CaptureEffectBlendingBehind(GameplaySceneLayer gameplay,
        RoomFxOverlayRenderSnapshot fx)
    {
        RoomEffectBlend blend = fx.Blend
            ?? throw new ArgumentException($"Room FX type {fx.Type} describes no blend.", nameof(fx));
        BlendedLayers blended = blend.BlendsBehind
            ?? throw new ArgumentException($"Room FX type {fx.Type} lays its overlay over the room, not behind it.", nameof(fx));
        return new(gameplay, ScreenWindow.Closed, false, new GameplayBlend
        {
            Blended = blended,
            Operation = blend.Operation,
            Halved = false,
            Colour = RgbColor.Black,
        }, CaptureRoomFxOverlay(fx, gameplay.Rooms.ShownFxOverlay), blend.BackgroundBehind);
    }

    /// <summary>
    /// Resolves the room effect's blend, its liquid's place and its wave's phase into the effect overlay's blend (#1706): an
    /// atmosphere's <paramref name="artwork"/> repeating at its scroll, or a liquid's strip and body from its surface line,
    /// with the overlay's horizontal scroll and how it combines. A liquid shows nothing while it has no surface, its surface
    /// lies above the room, or the frame has not placed it on the screen.
    /// </summary>
    /// <exception cref="InvalidDataException">The effect shows artwork but describes no blend, or its artwork is another kind of effect's.</exception>
    public static OverlayBlendLayer? CaptureRoomFxOverlay(RoomFxOverlayRenderSnapshot fx, FxOverlayArtwork artwork)
    {
        if (!RoomFxOverlayState.ShowsOnOverlay(fx.Type))
            throw new NotSupportedException($"Room FX type {fx.Type} has no captured effect overlay compositor.");
        RoomEffectBlend blend = fx.Blend
            ?? throw new InvalidDataException($"Room FX type {fx.Type} shows artwork but describes no blend.");
        RoomEffectArtwork? shown = Ensure.NotNull(artwork).Shown;
        if (!RoomFxTypes.ShowsLiquid(fx.Type))
        {
            return new(shown is null ? null : new AtmosphereOverlay(shown as AtmosphereArtwork
                    ?? throw new InvalidDataException($"Room FX type {fx.Type} is an atmosphere but shows a liquid's artwork."), ScrollBands.Uniform(fx.Scroll.Y)),
                HudHeight, blend.Operation, ScrollBands.Uniform(fx.Scroll.X));
        }
        if (fx.Surface is not >= 0 || fx.WaterSurfaceScreenY is not int placedLine) return null;
        // Below a water surface the overlay moves sideways with the water waveform, one sample a line counted from the line
        // below the surface and drawn back by the wave's phase (#1468).
        int belowSurface = placedLine + 1;
        LineScroll horizontal = RoomFxTypes.UsesWater(fx.Type)
            ? new SampledScrollWave(fx.Scroll.X, LiquidEffects.WaterWaveform, OnePixelPerSample,
                SampledScrollWave.PhaseWhere(-fx.WaterOverlayWavePhase, belowSurface, LiquidEffects.WaterWaveform.Length),
                Math.Clamp(belowSurface, HudHeight, Height), Height)
            : ScrollBands.Uniform(fx.Scroll.X);
        // A liquid last placed while its surface lay above the room shows nothing until the effect places it again.
        return new(shown is null || fx.LiquidSurfaceLine is not int surfaceLine ? null : new LiquidOverlay(shown as LiquidArtwork
                ?? throw new InvalidDataException($"Room FX type {fx.Type} is a liquid but shows an atmosphere's artwork."), surfaceLine),
            HudHeight, blend.Operation, horizontal);
    }

    /// <summary>
    /// The Ceres haze: its tint added across the gameplay area, strengthening in bands down the screen and dimmed by
    /// how far the haze has faded, with no boss-state reference.
    /// </summary>
    /// <param name="tints">The installed tints of the haze.</param>
    /// <param name="ridleyIsDead">Whether Ceres Ridley is dead, which selects the red tint.</param>
    /// <param name="intensity">How far the haze has faded in.</param>
    public static ColourAddWindowLayer CaptureCeresHaze(CeresHazeTints tints, bool ridleyIsDead,
        int intensity = CeresHazeRenderDefinitions.MaximumComponent)
    {
        RgbColor tint = Ensure.NotNull(tints).For(ridleyIsDead);
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
    /// <param name="cameraPosition">The camera, which places the blast on the screen.</param>
    /// <param name="firstLine">The first screen line the blast covers: the playfield's first line below the HUD in gameplay, the top of the screen in cinematics.</param>
    public static ColourAddWindowLayer? CapturePowerBombGlow(
        PowerBombRenderSnapshot explosion, Position16<RoomPixels> cameraPosition,
        int firstLine = HudHeight)
    {
        if ((uint)firstLine > Height)
            throw new ArgumentOutOfRangeException(nameof(firstLine));
        if (!explosion.IsActive) return null;
        int radius = PowerBombHorizontalRadius(explosion);
        int verticalRadius = radius * PowerBombExplosionDefinitions.OvalHeightParts / PowerBombExplosionDefinitions.OvalWidthParts;
        ScreenWindow window = verticalRadius == 0 ? ScreenWindow.Closed
            : ScreenWindow.Of(WindowOval.Ellipse(explosion.Position.RelativeToCamera(cameraPosition),
                new Extent16<ScreenPixels>(radius, verticalRadius)),
                firstLine);
        return new(window, explosion.GlowColor);
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
    public static ScreenWindow CaptureXrayWindow(SamusXrayDisplaySnapshot xray, Position16<RoomPixels> cameraPosition)
    {
        Offset16<ScreenPixels> samusOnScreen = xray.SamusPosition.RelativeToCamera(cameraPosition);
        int eyeX = samusOnScreen.X +
            (xray.FacingLeft ? -XrayWindowRenderDefinitions.EyeHorizontalOffset : XrayWindowRenderDefinitions.EyeHorizontalOffset);
        int eyeY = samusOnScreen.Y -
            (xray.Crouching ? XrayWindowRenderDefinitions.CrouchingEyeHeight : XrayWindowRenderDefinitions.StandingEyeHeight);
        TrigonometryAngle angle = xray.Angle.ToTrigonometryAngle();
        var cone = new WindowCone(Offset16<ScreenPixels>.Of(eyeX, eyeY),
            angle - xray.AngularWidth, angle + xray.AngularWidth);
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
