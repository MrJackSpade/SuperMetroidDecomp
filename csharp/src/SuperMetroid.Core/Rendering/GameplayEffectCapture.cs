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
    private const int Width = ScreenGeometry.ScreenWidthPixels;
    /// <summary>The screen's height in lines.</summary>
    private const int Height = ScreenGeometry.ScreenHeightPixels;
    /// <summary>The lines the HUD fills at the top of the screen.</summary>
    private const int HudHeight = ScreenGeometry.GameplayHudHeightPixels;

    /// <summary>
    /// Describes water's optional background wave (#1468): below the surface, background's horizontal scroll follows
    /// the background waveform one sample a line, counted from the line below the surface and advanced by the wave's phase
    /// and the background's vertical scroll within its block; the lines above keep background's own scroll. Null
    /// without the wave, or while the frame places no water surface.
    /// </summary>
    public static SampledScrollWave? BuildWaterBackgroundHorizontalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        Offset16<BackgroundPixels> backgroundScroll)
    {
        if (fx.Type != RoomFxType.Water || fx.Liquid is not RoomWater { BackgroundWave: true } ||
            fx.WaterSurfaceScreenY is not int surfaceLine)
            return null;
        int belowSurface = surfaceLine + 1;
        return new SampledScrollWave(backgroundScroll.X, WaterWaveDefinitions.BackgroundWaveform,
            SampledScrollWave.PhaseWhere(fx.WaterBackgroundWavePhase + PhaseInBackgroundBlock(backgroundScroll.Y), belowSurface, WaterWaveDefinitions.BackgroundWaveform.Length),
            Math.Clamp(belowSurface, HudHeight, Height), Height);
    }

    /// <summary>
    /// Describes lava's or acid's horizontal heat haze over the background's horizontal scroll (#1468), or null for a liquid
    /// without it.
    /// </summary>
    public static SampledScrollWave? BuildLavaAcidBackgroundHorizontalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        Offset16<BackgroundPixels> backgroundScroll)
    {
        if (fx.Liquid is not RoomLavaOrAcid { Haze: HeatHazeDirection.Horizontal })
            return null;

        return HeatHaze(backgroundScroll.X, backgroundScroll.Y, fx.LavaAcidBackgroundWavePhase, LavaAcidWaveDefinitions.HazeWaveforms[HeatHazeDirection.Horizontal]);
    }

    /// <summary>
    /// Describes lava's or acid's vertical heat haze as a wave over the background's vertical scroll (#1468), or null for a
    /// liquid without it: its lines move about the background row the screen's top shows (#1514), and its samples are
    /// counted from the camera's row.
    /// </summary>
    /// <param name="fx">The room effect the frame shows.</param>
    /// <param name="backgroundVerticalScroll">The background camera's top row, signed (#2308): negative above the background's origin.</param>
    public static SampledScrollWave? BuildLavaAcidBackgroundVerticalScrolls(
        RoomFxOverlayRenderSnapshot fx,
        int backgroundVerticalScroll)
    {
        if (fx.Liquid is not RoomLavaOrAcid { Haze: HeatHazeDirection.Vertical })
            return null;

        // The vertical haze's lines move about the row the screen's top shows (RoomView), while its phase stays counted from
        // the camera's row.
        return HeatHaze(RoomView.ShownRow(backgroundVerticalScroll), backgroundVerticalScroll,
            fx.LavaAcidBackgroundWavePhase, LavaAcidWaveDefinitions.HazeWaveforms[HeatHazeDirection.Vertical]);
    }

    /// <summary>
    /// The heat haze over every gameplay line (#1468, #2367). The haze repeats its samples, one a
    /// line, down the frame: it starts at the background's vertical scroll within its block, drawn back by the
    /// wave's phase, so each later phase draws the waveform one line further down.
    /// </summary>
    private static SampledScrollWave HeatHaze(int baseScroll, int backgroundVerticalScroll, int wavePhase, ImmutableArray<int> waveform) =>
        new(baseScroll, waveform,
            SampledScrollWave.PhaseWhere(PhaseInBackgroundBlock(backgroundVerticalScroll) - wavePhase, HudHeight, waveform.Length),
            HudHeight, Height);

    /// <summary>
    /// Where the background's vertical scroll lies within the background's block: the row of its block the camera's top
    /// row is on, counted downward for a scroll above the room's top as well.
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
    /// <param name="gameplay">The room's scene the effect blends.</param>
    /// <param name="overlay">The effect's liquid or atmosphere.</param>
    /// <param name="behind">How <paramref name="overlay"/>, lying behind the room, blends: its blend.</param>
    public static GameplayBlendLayer CaptureEffectBlendingBehind(GameplaySceneLayer gameplay, ShownRoomEffectOverlay overlay,
        OverlayBehindRoomBlend behind)
    {
        Ensure.NotNull(behind);
        return new(gameplay, ScreenWindow.Closed, false, new GameplayBlend
        {
            Blended = behind.Blended,
            Operation = behind.Operation,
            Halved = false,
            Colour = RgbColor.Black,
        }, CaptureRoomFxOverlay(overlay), behind.BackgroundBehind);
    }

    /// <summary>
    /// The effect overlay's blend of what the room effect shows on it (#1706, #2505): an atmosphere's artwork repeating at its
    /// scroll, or a liquid's strip and body from its surface line, with the overlay's horizontal scroll and how it combines;
    /// none for an effect showing nothing. A liquid shows nothing while it has no surface, its surface lies above the room,
    /// or the frame has not placed it on the screen.
    /// </summary>
    /// <param name="overlay">What the room effect shows on the effect overlay.</param>
    public static OverlayBlendLayer? CaptureRoomFxOverlay(RoomEffectOverlay overlay) =>
        Ensure.NotNull(overlay).Map(OverlayLayers.Instance);

    /// <summary>Each kind of effect overlay as the effect overlay's blend: none for an overlay showing nothing.</summary>
    private sealed class OverlayLayers : IRoomEffectOverlayMapping<OverlayBlendLayer?>
    {
        /// <summary>The one mapping.</summary>
        internal static OverlayLayers Instance { get; } = new();

        /// <inheritdoc/>
        public OverlayBlendLayer? Nothing() => null;

        /// <inheritdoc/>
        public OverlayBlendLayer? Atmosphere(AtmosphereEffectOverlay atmosphere) => CaptureAtmosphere(atmosphere);

        /// <inheritdoc/>
        public OverlayBlendLayer? Liquid(LiquidEffectOverlay liquid) => CaptureLiquid(liquid);
    }

    /// <summary>The atmosphere's picture repeating at its scroll across and down, blended as the atmosphere blends.</summary>
    private static OverlayBlendLayer CaptureAtmosphere(AtmosphereEffectOverlay atmosphere) =>
        new(new AtmosphereOverlay(atmosphere.Artwork, ScrollBands.Uniform(atmosphere.Scroll.Y)),
            HudHeight, atmosphere.Blend.Operation, ScrollBands.Uniform(atmosphere.Scroll.X));

    /// <summary>
    /// The liquid's strip and body from its surface line, scrolled across and, below a water surface, rippling with the
    /// surface's wave; none while the frame places no surface.
    /// </summary>
    private static OverlayBlendLayer? CaptureLiquid(LiquidEffectOverlay liquid)
    {
        if (liquid.SurfaceScreenLine is not int placedLine) return null;
        // Below a water surface the overlay moves sideways with the surface waveform, one sample a line counted from the line
        // below the surface and drawn back by the wave's phase (#1468).
        int belowSurface = placedLine + 1;
        LineScroll horizontal = liquid.SurfaceWavePhase is int phase
            ? new SampledScrollWave(liquid.HorizontalScroll, WaterWaveDefinitions.SurfaceWaveform,
                SampledScrollWave.PhaseWhere(-phase, belowSurface, WaterWaveDefinitions.SurfaceWaveform.Length),
                Math.Clamp(belowSurface, HudHeight, Height), Height)
            : ScrollBands.Uniform(liquid.HorizontalScroll);
        // A liquid last placed while its surface lay above the room shows nothing until the effect places it again.
        return new(liquid.ShownSurfaceLine is int surfaceLine ? new LiquidOverlay(liquid.Artwork, surfaceLine) : null,
            HudHeight, liquid.Blend.Operation, horizontal);
    }

    /// <summary>
    /// The Ceres haze: its tint added across the gameplay area, strengthening in bands down the screen and dimmed by
    /// how far the haze has faded, with no boss-state reference.
    /// </summary>
    /// <param name="tints">The installed tints of the haze.</param>
    /// <param name="hazeTint">The tint the haze adds.</param>
    /// <param name="strength">How strongly the haze shows.</param>
    public static ColourAddWindowLayer CaptureCeresHaze(CeresHazeTints tints, CeresHazeTint hazeTint, CeresHazeStrength strength)
    {
        RgbColor tint = Ensure.NotNull(tints).For(hazeTint);
        // The fade's strength is relative to the whole haze: below it the bands dim.
        var steps = new LineColourSteps(CeresHazeRenderDefinitions.RampFirstLine, CeresHazeRenderDefinitions.BandHeight,
            CeresHazeRenderDefinitions.MaximumComponent, strength.FadeSteps - CeresHazeRenderDefinitions.MaximumComponent);
        return new(ScreenWindow.Of(WindowRectangle.Spanning(0, HudHeight, Width - 1, Height - 1), HudHeight), tint, steps);
    }

    /// <summary>
    /// The Power Bomb's blast as the oval of added colour it shows around its centre (#1468, #1927), three quarters as
    /// tall as it is wide.
    /// </summary>
    /// <param name="explosion">The blast as the frame shows it.</param>
    /// <param name="cameraPosition">The camera, which places the blast on the screen.</param>
    /// <remarks>The blast covers the playfield from its first line below the HUD.</remarks>
    public static ColourAddWindowLayer? CapturePowerBombGlow(PowerBombRenderSnapshot explosion, Position16<RoomPixels> cameraPosition)
    {
        if (!explosion.IsActive) return null;
        PowerBombBlastOval oval = explosion.Oval;
        ScreenWindow window = oval.Radii.Y == 0 ? ScreenWindow.Closed
            : ScreenWindow.Of(WindowOval.Ellipse(oval.Centre.RelativeToCamera(cameraPosition),
                new Extent16<ScreenPixels>(oval.Radii.X, oval.Radii.Y)),
                HudHeight);
        return new(window, oval.Colour);
    }

    /// <summary>
    /// The X-ray scope's cone from Samus's eye (#1468): three pixels ahead of her, at her standing or crouching eye
    /// height, sweeping the scope's half-width either side of its aim, below the HUD.
    /// </summary>
    public static ScreenWindow CaptureXrayWindow(SamusXrayDisplaySnapshot xray, Position16<RoomPixels> cameraPosition)
    {
        Offset16<ScreenPixels> samusOnScreen = xray.SamusPosition.RelativeToCamera(cameraPosition);
        int eyeX = samusOnScreen.X +
            XrayWindowRenderDefinitions.EyeHorizontalOffset.Toward(xray.FacingLeft ? HorizontalDirection.Left : HorizontalDirection.Right);
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

    /// <summary>What <c>SuitPickupBeamWindow</c> comes to for each <see cref="SamusSuitPickupBeamShape"/> member, each member's own arm.</summary>
    private static IdentityTable<SamusSuitPickupBeamShape, Func<SamusSuitPickupBeam, int, int, int, int, ScreenWindow>> SuitPickupBeamWindowForSamusSuitPickupBeamShape => field ??= new(new Dictionary<SamusSuitPickupBeamShape, Func<SamusSuitPickupBeam, int, int, int, int, ScreenWindow>>
    {
        [SamusSuitPickupBeamShape.None] = static (beam, left, right, lines, centre) => ScreenWindow.Closed,
        [SamusSuitPickupBeamShape.GrowingFromEdges] = static (beam, left, right, lines, centre) =>
        {
            int grown = Math.Min(beam.Progress, lines);
            if (grown <= 0) return ScreenWindow.Closed;
            return new([WindowRectangle.Spanning(left, 0, right, grown - 1),
                WindowRectangle.Spanning(left, lines - grown, right, lines - 1)]);
        },
        [SamusSuitPickupBeamShape.Column] = static (beam, left, right, lines, centre) => ScreenWindow.Of(WindowRectangle.Spanning(left, 0, right, lines - 1)),
        [SamusSuitPickupBeamShape.CurvedColumn] = static (beam, left, right, lines, centre) => ScreenWindow.Of(CurvedColumn(left, right, centre)),
        [SamusSuitPickupBeamShape.ClosingCurvedColumn] = static (beam, left, right, lines, centre) =>
        {
            // The upper wipe always clears at least the first line; the lower wipe is one line shorter.
            int topWiped = Math.Clamp(beam.Progress, 1, lines);
            int bottomWiped = Math.Clamp(beam.Progress - 1, 1, lines);
            int first = Math.Min(topWiped, Height);
            int end = Math.Clamp(lines - bottomWiped, first, Height);
            return ScreenWindow.Of(CurvedColumn(left, right, centre), first, end);
        },
        [SamusSuitPickupBeamShape.CenterLine] = static (beam, left, right, lines, centre) => ScreenWindow.Of(WindowRectangle.Spanning(left, centre, right, centre)),
    });

    /// <summary>The window of each light-beam outline: columns, a rounded column, its wiped remainder or the centre line.</summary>
    private static ScreenWindow SuitPickupBeamWindow(SamusSuitPickupBeam beam)
    {
        int left = beam.Left, right = beam.Right;
        int lines = SuitPickupBeamDefinitions.BeamLines;
        int centre = lines / 2;
        if (left > right) return ScreenWindow.Closed;
        return SuitPickupBeamWindowForSamusSuitPickupBeamShape[beam.Shape](beam, left, right, lines, centre);
    }

    /// <summary>The beam's column widened on each line by its rounded sides, widest at the middle line.</summary>
    private static WindowOval CurvedColumn(int left, int right, int centre) =>
        new(Offset16<ScreenPixels>.Of(left, centre), right - left,
            new Extent16<ScreenPixels>(SuitPickupBeamDefinitions.CurveWidth, SuitPickupBeamDefinitions.BeamLines / 2));
}
