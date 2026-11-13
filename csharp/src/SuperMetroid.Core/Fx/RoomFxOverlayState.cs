using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Game-owned room effect selected by the room's effect record for the door Samus entered by.
/// </summary>
/// <remarks>
/// Every room effect shares the base/target position, packed 8.8 Y velocity, and
/// timer the room-FX loader sets. This owner retains those values even when that effect's
/// renderer is not modelled yet, allowing room objects to make their shared-state
/// writes without inventing another representation for the corresponding room-effect state.
/// The modelled effect overlay effects are lava, acid, water, spores, Landing Site rain, and Climb fog.
/// </remarks>
[RoomLifetime]
public sealed class RoomFxOverlayState : IGameplayShadingSource
{
    /// <summary>Tracks frame selection for the liquid, rain, fog, and spore tile animations.</summary>
    private readonly RoomFxAnimatedTilesState animatedTiles = new();

    /// <summary>The Fireflea state, which outlives each room.</summary>
    private readonly RoomFxSession _session;

    /// <summary>The Fireflea FX state. It lives for the session, so a room load does not reset it.</summary>
    public FirefleaFxState Fireflea => _session.Fireflea;

    /// <summary>
    /// The shade Fireflea darkness subtracts from this room (<see cref="FirefleaFxState.Tint"/>), or null when the room's
    /// effect is not Fireflea darkness. No other room shows it.
    /// </summary>
    public RgbColor? FirefleaTint => Type == RoomFxType.Fireflea ? Fireflea.Tint : null;

    /// <summary>
    /// Whether the room's last applied FX record animates Samus's visor (<see cref="RoomFxRecordDefinition.VisorAnimation"/>):
    /// selected when the room loads, steady for a room with no record, and replaced when a fight applies another record.
    /// </summary>
    public RoomVisorAnimation VisorAnimation { get; private set; }

    /// <summary>The effect whose authored page the room's effect overlay shows, chosen when the room loaded; null when it shows none.</summary>
    public RoomFxType? EffectPageType { get; private set; }

    /// <summary>The effect overlay the room shows (#1348, #1466, #1601): its effect's page when it has one, otherwise nothing.</summary>
    public FxOverlayPlane EffectPlane => FxOverlayPlane.Of(EffectPageType);

    /// <summary>
    /// The palette blend the last FX record loaded selected for the effect overlay's blend colours (#1512), or null while no record
    /// has loaded one and the colours stay the tileset's.
    /// </summary>
    public RoomFxPaletteBlend? PaletteBlend { get; private set; }
    /// <summary>
    /// Rain, fog, and spores texture drift: one 8.8 running sum per axis. Its signed whole
    /// pixels shift the effect overlay's scroll before the next velocity step.
    /// </summary>
    private Offset16<OverlaySubpixels> drift;
    /// <summary>
    /// Rain's horizontal texture drift per frame in 8.8 pixels, chosen from the random number when the room loads;
    /// only X is set. The fall is <see cref="RainFallVelocity"/>.
    /// </summary>
    private Offset16<OverlaySubpixels> rainWindVelocity;
    /// <summary>Previous camera point, used to make rain follow camera movement.</summary>
    private Position16<RoomPixels> previousCamera;
    /// <summary>The fraction a room load starts the liquid's surface at: half a pixel, centring the liquid on its whole line.</summary>
    private const int LoadedSurfaceFraction = 1 << 15;
    /// <summary>
    /// How far round its rise and fall the liquid's tide is: the surface sits at the negative cosine of this angle's whole
    /// step, and <see cref="AdvanceTidePhase"/> turns it on, continuing round past a full turn.
    /// </summary>
    internal RotationAngle TidePhase { get; set; }
    /// <summary>How far the tide displaces the liquid surface from its base line, in pixels and subpixels.</summary>
    internal SignedSixteenSixteen TideOffset { get; set; }
    /// <summary>
    /// Water's effect overlay surface drift: an 8.8 horizontal running sum whose signed whole pixels offset the surface
    /// texture. Only X moves.
    /// </summary>
    private Offset16<OverlaySubpixels> waterDrift;
    /// <summary>Countdown before advancing the water effect overlay waveform.</summary>
    private int waterOverlayWaveTimer;
    /// <summary>Countdown for the optional water background horizontal wave.</summary>
    private int waterBackgroundWaveTimer;
    /// <summary>Selected effect overlay water displacement frame.</summary>
    private int waterOverlayWavePhase;
    /// <summary>Selected background water displacement frame.</summary>
    private int waterBackgroundWavePhase;
    /// <summary>
    /// Liquid surface relative to the camera, used to clip the effect plane: the signed value of the wrapped difference, or
    /// null while the liquid has no surface or none has been placed on the screen yet.
    /// </summary>
    private int? waterSurfaceScreenY;
    /// <summary>Countdown for lava or acid's optional background wave.</summary>
    private int lavaAcidBackgroundWaveTimer;
    /// <summary>Selected lava or acid background wave displacement frame.</summary>
    private int lavaAcidBackgroundWavePhase;
    /// <summary>
    /// Set after the first liquid effect overlay per-line effect pass installs its per-frame step. Later passes
    /// run it before main-loop RNG; water uses this latch despite its older lava/acid name.
    /// </summary>
    internal bool LavaAcidOverlayFrameBehaviorInstalled { get; set; }
    /// <summary>
    /// Records direct deletion of liquid scroll per-line effect objects by an enemy, which stops
    /// their per-frame updates until the next room load.
    /// </summary>
    internal bool LiquidMotionStopped { get; set; }

    /// <summary>The lava/acid effect overlay's ambient sound timer.</summary>
    private int lavaSoundTimer;
    /// <summary>Current liquid rise or fall state selected by the FX record.</summary>
    private LiquidRisePhase liquidRisePhase;
    /// <summary>
    /// The update's audio requests the effect's sounds join as it makes them: the runtime's, once it points the room's
    /// systems at it; the effect's own record before that.
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; private set; } = new();
    /// <summary>Countdown between quake sound requests in earthquake FX rooms.</summary>
    internal WrappingCountdown EarthquakeSoundTimer { get; set; }
    /// <summary>Position in the room earthquake sound sequence.</summary>
    internal int EarthquakeSoundSequenceIndex { get; set; }

    /// <summary>The FX type the active room/door record selects.</summary>
    public RoomFxType Type { get; internal set; }

    /// <summary>
    /// The liquid surface before tide displacement, a room Y with subpixels from 0 through
    /// <see cref="CoordinateAxis.LastNonNegative"/>: the FX record's base
    /// whole pixels (a room load starts the fraction at half a pixel), moved toward <see cref="TargetSurface"/> by
    /// <see cref="YVelocity"/>; null when the record names no surface (#1546).
    /// </summary>
    public FixedCoordinate? BaseY { get; internal set; }

    /// <summary>
    /// The room line from 0 through <see cref="CoordinateAxis.LastNonNegative"/> a rising or falling liquid stops at, or null when nothing names one and the
    /// liquid never moves (#1546).
    /// </summary>
    public int? TargetSurface { get; private set; }

    /// <summary>The liquid's signed 8.8 vertical velocity in room pixels per update while it moves toward its target.</summary>
    public SignedEightEight YVelocity { get; private set; }

    /// <summary>The FX timer, 0 through 255, the active record supplies.</summary>
    public WrappingCountdown Timer { get; private set; }

    /// <summary>How the effect's overlay blends with the room (#1545), as the FX record describes it, or null when it shows no overlay.</summary>
    public RoomEffectBlend? Blend { get; private set; }

    /// <summary>The effect overlay's live scroll.</summary>
    public Position16<OverlayPixels> Scroll { get; private set; }

    /// <summary>Whether the room's effect shows its page on the effect overlay over the gameplay area.</summary>
    public bool IsRenderable => ShowsPage(Type);

    /// <summary>Whether a room of effect <paramref name="type"/> shows the effect's authored page on the effect overlay.</summary>
    public static bool ShowsPage(RoomFxType type) => type is
        RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water or RoomFxType.TourianEntranceStatue or
        RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;

    /// <summary>
    /// The live liquid surface's room line after rising and tide processing, or null while the liquid has no surface
    /// (#1546). A tide can carry the surface above the room's top, to a negative line, which every reader treats as no
    /// surface, as the game's sign test does.
    /// </summary>
    public int? Surface { get; internal set; }

    /// <summary>
    /// The liquid the FX record fills the room with, as room objects have since changed it, or null for an effect with no
    /// liquid (#1695).
    /// </summary>
    public RoomLiquid? Liquid { get; internal set; }

    /// <summary>Points the effect's sounds at <paramref name="requests"/>, the update's audio requests of the runtime running it.</summary>
    internal void BindAudioRequests(GameplayAudioRequests requests) => AudioRequests = Ensure.NotNull(requests);

    /// <summary>
    /// The earthquake the current effect pass sustains (<see cref="RoomEarthquake.Sustain"/>), if any: rising lava's or
    /// acid's, every update it waits or moves.
    /// </summary>
    public EarthquakeShake? SustainedEarthquake { get; private set; }

    /// <summary>Builds one room's effect-overlay FX from the current door's FX record.</summary>
    /// <param name="session">The Fireflea state, which outlives each room.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="randomNumber">The session random number rain reads.</param>
    /// <param name="room">The room, for rooms that suppress the earthquake sound.</param>
    public static RoomFxOverlayState ForRoom(
        RoomFxSession session,
        RoomFxSelection? fx,
        DoorDefinition? door,
        int randomNumber,
        RoomDefinition? room = null) =>
        ForRecord(session, new RoomFxLoadSource { Fx = fx, Door = door, RandomNumber = randomNumber, Room = room, SuppliedDefinition = null });

    /// <summary>Builds one room's effect-overlay FX from a load source, which may supply its record directly.</summary>
    /// <param name="session">The Fireflea state, which outlives each room.</param>
    /// <param name="source">The FX record's source.</param>
    internal static RoomFxOverlayState ForRecord(RoomFxSession session, RoomFxLoadSource source)
    {
        var fx = new RoomFxOverlayState(session);
        fx.SelectRecord(source);
        return fx;
    }

    /// <summary>
    /// Selects the door's FX record and initializes the liquid and palette state shared by room effects and room objects.
    /// From then until Samus leaves the room, the effect overlay shows the effect's page where <see cref="FxOverlayPageDefinitions"/>
    /// places it, or nothing for an effect without one (<see cref="FxOverlayPlane"/>).
    /// </summary>
    private void SelectRecord(RoomFxLoadSource source)
    {
        if (source.Room is { } room && RoomEarthquakeDefinitions.SoundSuppressedRooms.Contains(room))
            EarthquakeSoundTimer = WrappingCountdown.Start(WrappingCountdown.LongestStart);
        if ((source.SuppliedDefinition ?? source.Fx?.Select(source.Door)) is not { } definition)
            return;

        LoadSurfaceAndTarget(definition);
        YVelocity = definition.YVelocity;
        Timer = WrappingCountdown.FromRaw(definition.Timer);
        Liquid = definition.Liquid;
        Surface = BaseLine;

        Type = definition.Type;
        Blend = definition.EffectBlend;
        VisorAnimation = definition.VisorAnimation;
        animatedTiles.Load(Type);
        PaletteBlend = definition.PaletteBlend;

        if (Type == RoomFxType.Fireflea)
            Fireflea.Initialize();
        if (!IsRenderable)
            return;

        // The room shows this effect's page until Samus leaves it.
        EffectPageType = Type;

        if (RoomFxTypes.UsesWater(Type))
        {
            // Both spawned per-line effect objects execute their phase initializer on the first
            // handler pass. A one-frame timer reproduces that first-call rotation.
            waterOverlayWaveTimer = 1;
            waterBackgroundWaveTimer = 1;
        }
        else if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            // Lava and acid start with timer B at one, so the first per-line effect object pass rotates A
            // from zero to 30 and then reloads either four or six. Expressing the already-
            // resolved waveform phase as zero produces the same first visible table.
            lavaAcidBackgroundWaveTimer = HazePhaseDuration;
            lavaAcidBackgroundWavePhase = 0;
        }
        else if (Type == RoomFxType.Rain)
        {
            rainWindVelocity = new Offset16<OverlaySubpixels>(
                AtmosphereDrift.RainHorizontalVelocity(AtmosphereDrift.RainWindChosenBy(source.RandomNumber)), 0);
        }
    }

    /// <summary>
    /// True for the liquids whose effect overlay per-line effect step moves the surface: lava and
    /// acid and water. That motion belongs to the per-line effect pass.
    /// </summary>
    public bool MovesLiquidBeforeMainLoop =>
        Type is RoomFxType.Lava or RoomFxType.Acid || RoomFxTypes.UsesWater(Type);

    /// <summary>
    /// Loads the record's surface and target: a named surface keeps the fraction the surface had, or a room load's half
    /// pixel when it had none.
    /// </summary>
    private void LoadSurfaceAndTarget(RoomFxRecordDefinition definition)
    {
        BaseY = definition.BaseYPosition is int baseY ? SurfaceAt(baseY) : null;
        TargetSurface = definition.TargetYPosition is int targetY ? SurfaceLine(targetY) : null;
    }

    /// <summary>The base surface at whole line <paramref name="line"/>, keeping the current fraction or a room load's.</summary>
    private FixedCoordinate SurfaceAt(int line) =>
        (BaseY ?? new FixedCoordinate(0, LoadedSurfaceFraction)).WithWhole(SurfaceLine(line));

    /// <summary>
    /// Accepts a named surface or target line: a line from the room's top that the liquid's readers find within the room.
    /// The tide's surface line, the rise's test for passing the room's top and the overlay's test for no surface read the
    /// line signed, so it stays within <see cref="CoordinateAxis.LastNonNegative"/>; a line past it would read as above the
    /// room's top (#1627).
    /// </summary>
    private static int SurfaceLine(int line) => Ensure.BetweenInclusive(0, CoordinateAxis.LastNonNegative, line);

    /// <summary>The base surface's whole line, or null without a surface.</summary>
    private int? BaseLine => BaseY is { } baseY ? baseY.Whole : null;

    /// <summary>
    /// Runs liquid motion and shared-state writes for lava, acid and water before
    /// main-loop random-number generation. Their setup installs the callback on the
    /// first per-line effect pass; lava/acid's callback then swaps the shared random state on
    /// subsequent unfrozen passes, even off screen. Rising/tidal motion also runs during
    /// door fades and pause entry. Visual updates remain in <see cref="Step"/>,
    /// which must not advance that motion twice.
    /// </summary>
    public void AdvanceLiquidMotion(SharedGameState system, bool timeIsFrozen)
    {
        Ensure.NotNull(system);
        if (!MovesLiquidBeforeMainLoop || LiquidMotionStopped)
            return;
        bool lavaOrAcid = Type is RoomFxType.Lava or RoomFxType.Acid;
        SustainedEarthquake = null;
        if (!LavaAcidOverlayFrameBehaviorInstalled)
        {
            LavaAcidOverlayFrameBehaviorInstalled = true;
            // Water's setup sets the sound timer in the same pass that installs the frame behaviour.
            if (lavaOrAcid)
                lavaSoundTimer = LavaAcidWaveDefinitions.AmbientSoundPeriod;
            return;
        }
        if (timeIsFrozen)
            return;
        if (!lavaOrAcid)
        {
            // Water's rising function and tide; water leaves the random number alone.
            AdvanceLiquidMotion(system.RandomNumber, system.EarthquakeSoundDelay);
            return;
        }
        int random = system.RandomNumber;
        AdvanceLiquidMotion(random, system.EarthquakeSoundDelay);
        QueueLavaAmbientSound(random);
        system.SwapRandomHalves();
    }

    /// <summary>
    /// The lava sound clock: while a lava surface's Y is not negative, its timer
    /// counts down and, on reaching zero, reloads and queues a random ambient sound.
    /// Acid shares the per-frame behaviour step but not this sound.
    /// </summary>
    private void QueueLavaAmbientSound(int random)
    {
        if (Type != RoomFxType.Lava || Surface is not >= 0)
            return;
        if (--lavaSoundTimer != 0)
            return;
        lavaSoundTimer = LavaAcidWaveDefinitions.AmbientSoundPeriod;
        AudioRequests.QueueSound(LiquidEffects.LavaAmbientSound(random), LavaAcidWaveDefinitions.AmbientSoundPriority);
    }

    /// <summary>Runs the active effect's per-frame behaviour step and rain animtile handler.</summary>
    public void Step(ArtworkFrameSelections frames, RoomFxOverlayFrame frame)
    {
        Ensure.NotNull(frames);
        if (!frame.LiquidMotionAlreadyAdvanced)
            SustainedEarthquake = null;
        if (Type == RoomFxType.Fireflea)
        {
            Fireflea.Step(frame.TimeIsFrozen, frame.FirefleaDarknessStep);
            return;
        }
        if (!IsRenderable || frame.TimeIsFrozen)
            return;

        // The shared animated-tiles handler runs independently of the per-line effect's
        // per-frame step. In particular, lava and acid select the animation frame their page
        // shows here, before the surface step that follows.
        animatedTiles.Step(frames);

        if (LiquidMotionStopped && MovesLiquidBeforeMainLoop)
            return;

        if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            StepLavaAcid(frame.Camera, frame.RandomNumber, frame.EarthquakeSoundDelay, frame.LiquidMotionAlreadyAdvanced);
            return;
        }

        if (RoomFxTypes.UsesWater(Type))
        {
            StepWater(frame.Camera, frame.RandomNumber, frame.EarthquakeSoundDelay, frame.LiquidMotionAlreadyAdvanced);
            return;
        }

        if (Type == RoomFxType.Rain)
        {
            Offset16<OverlayPixels> cameraDelta = previousCamera.AsOverlayScroll() - frame.Camera.AsOverlayScroll();
            Scroll = Position16<OverlayPixels>.Zero + cameraDelta + drift.WholePixels();
            drift += rainWindVelocity + RainFallVelocity;
            previousCamera = frame.Camera;
            return;
        }

        if (Type == RoomFxType.Spores)
        {
            // Spores sample each signed running sum before adding its velocity.
            // Unlike rain, spores are anchored to the current camera, not its delta.
            Scroll = frame.Camera.AsOverlayScroll() + drift.WholePixels();
            drift = Offset16<OverlaySubpixels>.Wrapped(drift.X, drift.Y - AtmosphereDriftDefinitions.SporesVerticalVelocity);
            return;
        }

        // Fog anchors to the foreground and moves its texture by +80 X and -64 Y in 8.8.
        Scroll = frame.Camera.AsOverlayScroll() + drift.WholePixels();
        drift = Offset16<OverlaySubpixels>.Wrapped(drift.X + AtmosphereDriftDefinitions.FogHorizontalVelocity,
            drift.Y - AtmosphereDriftDefinitions.FogVerticalVelocity);
    }

    /// <summary>
    /// Seeds the liquid surface's camera-dependent screen line during a room transition without advancing
    /// a wave clock. Destination rendering can therefore show the correct surface on its
    /// first frame boundary instead of waiting for the first gameplay handler call.
    /// </summary>
    public void PrimeViewport(Position16<RoomPixels> camera)
    {
        if (Type is not (RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid))
            return;
        Surface = BaseLine;
        waterSurfaceScreenY = SurfaceScreenLine(camera);
        Scroll = camera.AsOverlayScroll().WithY(ComputeLiquidVerticalScroll(Surface, camera.Y));
    }

    /// <summary>The surface's screen line below <paramref name="camera"/>'s top, or null without a surface.</summary>
    private int? SurfaceScreenLine(Position16<RoomPixels> camera) =>
        Surface is int line ? CoordinateAxis.Difference(CoordinateAxis.Wrap(line), camera.Y) : null;

    /// <summary>Captures values visible alongside the current frame boundary's sprite frame.</summary>
    public RoomFxOverlayRenderSnapshot? CaptureForDisplay() => IsRenderable
        ? new RoomFxOverlayRenderSnapshot(
            Type,
            Blend,
            Scroll,
            Surface,
            Liquid,
            waterOverlayWavePhase,
            waterBackgroundWavePhase,
            waterSurfaceScreenY)
        {
            LavaAcidBackgroundWavePhase = lavaAcidBackgroundWavePhase,
        }
        : null;


    /// <summary>
    /// Applies shared liquid writes made by room objects and enemy scripts. These are
    /// intentionally not a rendering shortcut: room objects write the same fields the
    /// room-FX loader sets. The liquid handler consumes those shared values
    /// on its next ordinary effect pass, just as the game does.
    /// </summary>
    internal void SetLiquidMotion(
        int? baseYPosition = null,
        int? targetYPosition = null,
        SignedEightEight? yVelocity = null,
        int? timer = null)
    {
        // Each value is a line or cell the writer copies; the checks reject anything outside a room's lines or wider.
        if (baseYPosition is int baseY)
            BaseY = SurfaceAt(baseY);
        if (targetYPosition is int targetY)
            TargetSurface = SurfaceLine(targetY);
        if (yVelocity is { } velocity)
            YVelocity = velocity;
        if (timer is int newTimer)
            Timer = WrappingCountdown.FromRaw(newTimer);
    }

    /// <summary>Removes the liquid's target, as the Speed Booster escape does when Samus has no Speed Booster: the liquid stops.</summary>
    internal void RemoveLiquidTarget() => TargetSurface = null;

    /// <summary>
    /// Reloading a record's entry restores motion, blending and three colors, but does not restart
    /// per-line effects, animated tiles, or the liquid's current motion phase as a room load would.
    /// </summary>
    internal void ApplyEntry(RoomFxRecordDefinition definition)
    {
        Ensure.NotNull(definition);
        LoadSurfaceAndTarget(definition);
        YVelocity = definition.YVelocity;
        Timer = WrappingCountdown.FromRaw(definition.Timer);
        Liquid = definition.Liquid;
        Blend = definition.EffectBlend;
        VisorAnimation = definition.VisorAnimation;
        PaletteBlend = definition.PaletteBlend;
    }

    /// <summary>
    /// Lays the shade of the palette blend the last record selected over the effect-overlay pictures (#1512): the effect's
    /// pages, and the HUD's, which show the same colours.
    /// </summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (PaletteBlend is not { } blend)
            return;
        frame.Shade(GameplayPictures.FxOverlay, RoomFxBlendShading.Of(blend));
        frame.Shade(GameplayPictures.Hud, RoomFxBlendShading.Of(blend));
    }

    /// <summary>
    /// Lets the room's water surface affect Samus, as the enable-water-physics room object does. It changes only that
    /// fact of the water, so this change lives on the room-FX owner that republishes the water to Samus; changing only
    /// Samus's projected copy would be undone by the next FX frame (see <see cref="SetLiquidMotion"/>).
    /// </summary>
    /// <exception cref="InvalidOperationException">The room's effect has no water.</exception>
    internal void EnableWaterPhysics() =>
        Liquid = Liquid is RoomWater water
            ? water.WithSurfaceEnabled()
            : throw new InvalidOperationException($"A {Type} room has no water whose surface a room object can enable.");

    /// <summary>
    /// Runs the observable static-water path of the water effect and both circular wave
    /// clocks. Rising/tidal rooms retain their loaded surfaces; the presently reported
    /// room has zero velocity and therefore exercises the game's normal static branch.
    /// </summary>
    private void StepWater(
        Position16<RoomPixels> camera,
        int randomNumber,
        PowerOnEarthquakeSoundDelay earthquakeSoundDelay,
        bool liquidMotionAlreadyAdvanced)
    {
        if (!liquidMotionAlreadyAdvanced)
            AdvanceLiquidMotion(randomNumber, earthquakeSoundDelay);
        waterSurfaceScreenY = SurfaceScreenLine(camera);
        Scroll = (camera.AsOverlayScroll() + waterDrift.WholePixels()).WithY(ComputeLiquidVerticalScroll(Surface, camera.Y));

        if (--waterOverlayWaveTimer == 0)
        {
            waterOverlayWaveTimer = WaterWaveDefinitions.OverlayWavePhaseDuration;
            waterOverlayWavePhase =
                (waterOverlayWavePhase + 1) % WaterWaveDefinitions.WaveDisplacementCount;
        }

        if (Liquid is RoomWater { SurfaceDrift: true })
            waterDrift += WaterDriftVelocity;

        // The Tourian entrance statue's FX spawns only the effect overlay water object. Its second per-line effect object moves
        // the statue vertically instead of installing the ordinary background X wave.
        if (Type != RoomFxType.Water || Liquid is not RoomWater { BackgroundWave: true })
            return;
        if (--waterBackgroundWaveTimer == 0)
        {
            waterBackgroundWaveTimer = WaterWaveDefinitions.BackgroundWavePhaseDuration;
            waterBackgroundWavePhase =
                (waterBackgroundWavePhase + 1) % WaterWaveDefinitions.WaveDisplacementCount;
        }
    }

    /// <summary>Advances rise and tide state before publishing the current liquid surface.</summary>
    private void AdvanceLiquidMotion(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        StepLiquidRise(randomNumber, earthquakeSoundDelay);
        StepLiquidTide();
        Surface = ComputeTidalYPosition();
    }

    /// <summary>
    /// Runs the visible outputs of the lava/acid effect and its heat haze. The liquid
    /// surface and the effect overlay follow the room camera, while one of two sixteen-row background
    /// waveforms rotates at the cadence of the liquid's heat haze.
    /// </summary>
    private void StepLavaAcid(
        Position16<RoomPixels> camera,
        int randomNumber,
        PowerOnEarthquakeSoundDelay earthquakeSoundDelay,
        bool liquidMotionAlreadyAdvanced)
    {
        if (!liquidMotionAlreadyAdvanced)
            AdvanceLiquidMotion(randomNumber, earthquakeSoundDelay);
        waterSurfaceScreenY = SurfaceScreenLine(camera);
        // Lava/acid uses the same surface-relative vertical scroll equation as water.
        // Zeroing it anchored the animated characters to the screen instead of
        // beginning their first texture row immediately below the liquid boundary.
        Scroll = camera.AsOverlayScroll().WithY(ComputeLiquidVerticalScroll(Surface, camera.Y));

        if (Liquid is not RoomLavaOrAcid { Haze: HeatHazeDirection.Horizontal or HeatHazeDirection.Vertical })
            return;
        if (--lavaAcidBackgroundWaveTimer != 0)
            return;

        lavaAcidBackgroundWavePhase =
            (lavaAcidBackgroundWavePhase - 1 + LavaAcidWaveDefinitions.WaveDisplacementCount) %
            LavaAcidWaveDefinitions.WaveDisplacementCount;
        lavaAcidBackgroundWaveTimer = HazePhaseDuration;
    }

    /// <summary>
    /// How long each phase of lava's or acid's heat haze lasts: the vertical haze's for a vertical haze, else the
    /// horizontal one's, which also starts the clock of a liquid with no haze.
    /// </summary>
    private int HazePhaseDuration => Liquid is RoomLavaOrAcid { Haze: HeatHazeDirection.Vertical }
        ? LavaAcidWaveDefinitions.VerticalWavePhaseDuration
        : LavaAcidWaveDefinitions.HorizontalWavePhaseDuration;

    /// <summary>
    /// Runs the parallel callback trios for lava/acid
    /// and for water. A nonzero velocity first arms the wait phase,
    /// the record timer then expires, and only then does the signed 8.8 velocity move the
    /// 16.16 base position toward the target. Door-specific Norfair entries rely on this
    /// sequence to raise lava into the visible viewport.
    /// </summary>
    private void StepLiquidRise(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        switch (liquidRisePhase)
        {
            case LiquidRisePhase.Dormant:
            {
                // A missing surface or target lies below every line (AbsentLiquidSurfaceDepth).
                int surface = AbsentLiquidSurfaceDepth.Of(BaseLine);
                int target = AbsentLiquidSurfaceDepth.Of(TargetSurface);
                bool movesTowardTarget = YVelocity > SignedEightEight.Zero
                    ? target > surface
                    : YVelocity.IsNegative && target < surface;
                if (movesTowardTarget)
                    liquidRisePhase = LiquidRisePhase.Waiting;
                return;
            }

            case LiquidRisePhase.Waiting:
                PublishRisingLiquidFeedback(randomNumber, earthquakeSoundDelay);
                Timer = Timer.Decrement().Remaining;
                if (Timer.IsZero)
                    liquidRisePhase = LiquidRisePhase.Moving;
                return;

            case LiquidRisePhase.Moving:
                PublishRisingLiquidFeedback(randomNumber, earthquakeSoundDelay);
                if (AdvanceBaseYToTarget())
                {
                    YVelocity = default;
                    liquidRisePhase = LiquidRisePhase.Dormant;
                }
                return;

            default:
                throw new InvalidDataException(
                    $"Unknown room-liquid rise phase {liquidRisePhase}.");
        }
    }

    /// <summary>
    /// Runs the earthquake sound clock and the shared
    /// writes used while lava or acid waits and moves. Sound cadence and screen shake are
    /// parallel outputs of the game's FX actor; neither is inferred by the renderer.
    /// </summary>
    private void PublishRisingLiquidFeedback(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        // Water's callbacks only wait/move. Lava and acid's
        // parallel callbacks additionally request sound and a global earthquake.
        if (Type is not (RoomFxType.Lava or RoomFxType.Acid))
            return;
        HandleEarthquakeSoundEffect(randomNumber, earthquakeSoundDelay);
        SustainedEarthquake = RoomEarthquakeDefinitions.RisingLiquidShake;
    }

    /// <summary>
    /// Runs the signed timer and the earthquake sound's loop of eight delays. Every one of the eight plays the
    /// earthquake sound; after the eighth the loop starts again from the first. The independently retained rhythm
    /// supplies only the delays.
    /// </summary>
    /// <param name="randomNumber">The current random number, sampled without advancing it; the number modulo 4 adds zero through three updates to the next earthquake-sound delay.</param>
    /// <param name="earthquakeSoundDelay">
    /// The frame the save file found at power-on adds to every next delay (<see cref="PowerOnEarthquakeSoundDelay"/>).
    /// Both callers' effects are the first their room starts after the door ends the previous room's effects, so they
    /// receive that frame; an effect started after another would add nothing.
    /// </param>
    private void HandleEarthquakeSoundEffect(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        if (EarthquakeSoundTimer.IsNegative)
            return;

        EarthquakeSoundTimer = EarthquakeSoundTimer.Decrement().Remaining;
        if (!EarthquakeSoundTimer.IsNegative)
            return;

        IReadOnlyList<int> baseDelays = RoomEarthquakeDefinitions.RisingLiquidSoundBaseDelays;
        if ((uint)EarthquakeSoundSequenceIndex >= (uint)baseDelays.Count)
            EarthquakeSoundSequenceIndex = 0;

        AudioRequests.QueueSound(EnemySound.Lavaquake, SoundRequestPriority.Normal);
        EarthquakeSoundTimer = WrappingCountdown.Start(baseDelays[EarthquakeSoundSequenceIndex])
            .AddFrames(RoomEarthquakeDefinitions.RisingLiquidSoundJitter.Of(randomNumber) + earthquakeSoundDelay.ExtraFrames);
        EarthquakeSoundSequenceIndex++;
    }

    /// <summary>The statue per-line effect steps call the same earthquake sound owner.</summary>
    internal void PublishStatueEarthquakeSound(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay) =>
        HandleEarthquakeSoundEffect(randomNumber, earthquakeSoundDelay);

    /// <summary>
    /// Raises or lowers the shared liquid surface: moves it one step toward its target and reports
    /// whether it reached it. A liquid with no target stops at once; one with no surface moves from below every line
    /// (<see cref="AbsentLiquidSurfaceDepth"/>).
    /// </summary>
    private bool AdvanceBaseYToTarget()
    {
        if (TargetSurface is not int target)
            return true;

        FixedCoordinate moved = (BaseY ?? new FixedCoordinate(AbsentLiquidSurfaceDepth.Of(null), LoadedSurfaceFraction))
            .MovedBy(YVelocity.ToSixteenSixteenDelta());
        int candidate = moved.Whole;

        // Moving down, the surface reaches the target once it passes it, or passes the deepest line a surface can hold;
        // moving up, once it reaches it or rises past the room's top.
        bool reached = !YVelocity.IsNegative
            ? candidate > target
            : CoordinateAxis.AsSigned(candidate) < 0 || candidate <= target;
        if (!reached)
        {
            BaseY = moved;
            return false;
        }

        BaseY = new FixedCoordinate(target, 0);
        return true;
    }

    /// <summary>
    /// Moves the tide. The scaled sine is an 8.8 displacement, which becomes the 16.16 offset. The asymmetric phase
    /// deltas deliberately spend different durations in each half-wave.
    /// </summary>
    internal void StepLiquidTide()
    {
        int scale;
        RotationAngle positiveDelta;
        RotationAngle negativeDelta;
        switch (Liquid?.Tide ?? LiquidTide.None)
        {
            case LiquidTide.Small:
                scale = LiquidTideDefinitions.SmallTideScale;
                positiveDelta = LiquidTideDefinitions.SmallTidePositivePhaseDelta;
                negativeDelta = LiquidTideDefinitions.SmallTideNegativePhaseDelta;
                break;
            case LiquidTide.Large:
                scale = LiquidTideDefinitions.LargeTideScale;
                positiveDelta = LiquidTideDefinitions.LargeTidePositivePhaseDelta;
                negativeDelta = LiquidTideDefinitions.LargeTideNegativePhaseDelta;
                break;
            case LiquidTide.None:
                TideOffset = SignedSixteenSixteen.Zero;
                return;
            default:
                throw new InvalidOperationException($"Undefined liquid tide {Liquid?.Tide}.");
        }

        // The tide follows the negative cosine of its phase, not the sine: the sine a quarter turn behind it.
        int sample = Trigonometry.SignedSine((TidePhase - RotationAngle.QuarterTurn).ToTrigonometryAngle());
        TideOffset = SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(sample * scale);
        AdvanceTidePhase(sample >= 0 ? positiveDelta : negativeDelta);
    }

    /// <summary>Turns the tide phase on by <paramref name="delta"/>, continuing round past a full turn.</summary>
    private void AdvanceTidePhase(RotationAngle delta) => TidePhase += delta;

    /// <summary>
    /// Adds signed fixed-point tide displacement to the base liquid surface, giving its signed line; a liquid with no
    /// surface has none, and no retail record gives one a tide.
    /// </summary>
    /// <exception cref="InvalidOperationException">A liquid with no surface has a tide.</exception>
    private int? ComputeTidalYPosition()
    {
        if (BaseY is { } baseY)
            return CoordinateAxis.AsSigned(baseY.MovedBy(TideOffset).Whole);
        return TideOffset.IsZero
            ? null
            : throw new InvalidOperationException("A room liquid with no surface has a tide.");
    }

    /// <summary>The overlay's lines in its surface band, above the liquid's body.</summary>
    private const int SurfaceBandLines = 256;

    /// <summary>The lines in which the liquid's body below its surface repeats.</summary>
    private const int LiquidBodyRepeatLines = 32;

    /// <summary>
    /// Computes the effect overlay's vertical scroll from the liquid surface relative to the camera's top; none shows nothing.
    /// A surface below the camera's top scrolls the overlay up from the last line of its surface band, so the surface lies
    /// where it is; a surface at or above the camera's top shows the liquid's body, the band after the surface band, at the
    /// place in its repeat that the lines the surface lies above the camera reach.
    /// </summary>
    private static int ComputeLiquidVerticalScroll(int? surfaceY, int cameraY)
    {
        if (surfaceY is not int line || line < 0)
            return 0;
        int belowCameraTop = CoordinateAxis.Difference(line, cameraY);
        if (belowCameraTop <= 0)
        {
            int aboveCameraTop = -belowCameraTop;
            return SurfaceBandLines + (aboveCameraTop + LiquidBodyRepeatLines - 1) % LiquidBodyRepeatLines;
        }
        return belowCameraTop < SurfaceBandLines
            ? SurfaceBandLines - 1 - belowCameraTop
            : 0;
    }

    /// <summary>Creates one room's effect state, with no FX record selected, over the session's carried state.</summary>
    /// <param name="session">The Fireflea state, which outlives each room.</param>
    public RoomFxOverlayState(RoomFxSession session)
    {
        _session = Ensure.NotNull(session);
        Type = RoomFxType.None;
        TargetSurface = 0;
        YVelocity = default;
        Timer = WrappingCountdown.Start(0);
        Blend = null;
        Scroll = Position16<OverlayPixels>.Zero;
        drift = Offset16<OverlaySubpixels>.Zero;
        rainWindVelocity = Offset16<OverlaySubpixels>.Zero;
        previousCamera = Position16<RoomPixels>.Zero;
        BaseY = new FixedCoordinate(0, LoadedSurfaceFraction);
        TidePhase = RotationAngle.Zero;
        TideOffset = SignedSixteenSixteen.Zero;
        Surface = null;
        Liquid = null;
        waterDrift = Offset16<OverlaySubpixels>.Zero;
        waterBackgroundWaveTimer = 0;
        waterOverlayWaveTimer = 0;
        waterOverlayWavePhase = waterBackgroundWavePhase = 0;
        waterSurfaceScreenY = null;
        lavaAcidBackgroundWaveTimer = 0;
        lavaAcidBackgroundWavePhase = 0;
        liquidRisePhase = LiquidRisePhase.Dormant;
        LavaAcidOverlayFrameBehaviorInstalled = false;
        LiquidMotionStopped = false;
        lavaSoundTimer = 0;
        SustainedEarthquake = null;
        EarthquakeSoundTimer = WrappingCountdown.Start(0);
        EarthquakeSoundSequenceIndex = 0;
    }

    /// <summary>
    /// Ends the liquid's per-line motion effects: no later pass moves the liquid, queues its ambient sound or
    /// (for lava/acid) swaps the halves of the shared random number. Effects without per-line liquid motion are
    /// unaffected.
    /// </summary>
    public void StopLiquidMotion() => LiquidMotionStopped = true;

    /// <summary>Rain's texture fall each frame, in 8.8 pixels.</summary>
    private static readonly Offset16<OverlaySubpixels> RainFallVelocity =
        new(0, unchecked(-AtmosphereDriftDefinitions.RainVerticalVelocity));

    /// <summary>Water's surface drift each frame while its water's <see cref="RoomWater.SurfaceDrift"/> is set.</summary>
    private static readonly Offset16<OverlaySubpixels> WaterDriftVelocity =
        new(WaterWaveDefinitions.HorizontalSubscrollVelocity, 0);

    /// <summary>The mutually exclusive callbacks installed as the rise function.</summary>
    private enum LiquidRisePhase
    {
        /// <summary>Waits until signed velocity points from the current base toward the target.</summary>
        Dormant,
        /// <summary>Counts down the room FX timer while publishing rising-liquid sound and shake.</summary>
        Waiting,
        /// <summary>Advances the liquid base each update until it reaches the target and clears velocity.</summary>
        Moving,
    }
}
