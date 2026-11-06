using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Game-owned room effect selected by one sixteen-byte FX record.
/// </summary>
/// <remarks>
/// Every room effect shares the base/target position, packed 8.8 Y velocity, and
/// timer the room-FX loader sets. This owner retains those words even when that effect's
/// renderer is not translated yet, allowing PLMs to make their shared-state
/// writes without inventing another representation for the corresponding room-effect state.
/// The translated effect overlay effects are lava, acid, water, spores, Landing Site rain, and Climb fog.
/// </remarks>
[RoomLifetime]
public sealed class RoomFxOverlayState : IGameplayShadingSource
{
    /// <summary>Tracks frame selection for the liquid, rain, fog, and spore tile animations.</summary>
    private readonly RoomFxAnimatedTilesState animatedTiles = new();

    /// <summary>The Fireflea state, fixed colour and installed FX artwork, which outlive each room.</summary>
    private readonly RoomFxSession _session;

    /// <summary>The Fireflea FX state. It lives for the session, so a room load does not reset it.</summary>
    public FirefleaFxState Fireflea => _session.Fireflea;

    /// <summary>The room's fixed colour. It is session-lived, so it keeps the last writer's colour across rooms.</summary>
    public RoomFixedColor FixedColors => _session.FixedColors;

    /// <summary>The effect whose authored page the room's effect overlay plane shows, chosen when the room loaded; null when it shows none.</summary>
    public RoomFxType? EffectPageType { get; private set; }

    /// <summary>The effect overlay plane the room shows (#1348, #1466): the cleared picture, with its effect's page picture when it has one.</summary>
    public FxOverlayPlane EffectPlane => FxOverlayPlane.Of(EffectPageType);

    /// <summary>
    /// The palette blend the last FX record loaded selected for the effect overlay's blend colours (#1512), or null while no record
    /// has loaded one and the colours stay the tileset's.
    /// </summary>
    public RoomFxPaletteBlend? PaletteBlend { get; private set; }
    /// <summary>
    /// Rain, fog, and spores texture drift: one 8.8 accumulator per axis. Its signed high
    /// byte offsets effect overlay scroll before the next velocity step.
    /// </summary>
    private Offset16<OverlaySubpixels> drift;
    /// <summary>
    /// Rain's horizontal texture drift per frame in 8.8 pixels, chosen from the random number when the room loads;
    /// only X is set. The fall is <see cref="RainFallVelocity"/>.
    /// </summary>
    private Offset16<OverlaySubpixels> rainWindVelocity;
    /// <summary>Previous camera point, used to make rain follow camera movement.</summary>
    private Position16<RoomPixels> previousCamera;
    /// <summary>How many tide-phase units make one full turn of the negative-cosine lookup.</summary>
    private const int TidePhasePeriod = 65536;

    /// <summary>
    /// The surface height a record that names none holds: below every room, so a base there shows no liquid and a target
    /// there disables the motion.
    /// </summary>
    private const int UnnamedSurfaceY = ushort.MaxValue;
    /// <summary>
    /// Negative-cosine lookup phase for the liquid tide, 0 through 65535: its whole 1/256 turns select the sample, and
    /// it returns to zero after a full turn through <see cref="AdvanceTidePhase"/>.
    /// </summary>
    internal int TidePhase { get; set; }
    /// <summary>Signed fixed-point tide displacement added to the base surface.</summary>
    internal int TideFixedOffset { get; set; }
    /// <summary>
    /// Water's effect overlay surface drift: an 8.8 horizontal accumulator whose signed whole pixels offset the surface
    /// texture. Only X moves.
    /// </summary>
    private Offset16<OverlaySubpixels> waterDrift;
    /// <summary>Countdown before advancing the water effect overlay waveform.</summary>
    private WrappingCountdown waterOverlayWaveTimer;
    /// <summary>Countdown for the optional water background horizontal wave.</summary>
    private WrappingCountdown waterBackgroundWaveTimer;
    /// <summary>Selected effect overlay water displacement frame.</summary>
    private int waterOverlayWavePhase;
    /// <summary>Selected background water displacement frame.</summary>
    private int waterBackgroundWavePhase;
    /// <summary>
    /// Liquid surface relative to the camera, used to clip the effect plane: the signed word of the wrapped difference, or
    /// the largest signed word when no surface is shown.
    /// </summary>
    private int waterSurfaceScreenY;
    /// <summary>Countdown for lava or acid's optional background wave.</summary>
    private WrappingCountdown lavaAcidBackgroundWaveTimer;
    /// <summary>Selected lava or acid background wave displacement frame.</summary>
    private int lavaAcidBackgroundWavePhase;
    /// <summary>
    /// Set after the first liquid effect overlay per-line effect pass installs its pre-instruction. Later passes
    /// run it before main-loop RNG; water uses this latch despite its older lava/acid name.
    /// </summary>
    internal bool LavaAcidOverlayFrameBehaviorInstalled { get; set; }
    /// <summary>
    /// Records direct deletion of liquid scroll per-line effect objects by an enemy, which stops
    /// their per-frame updates until the next room load.
    /// </summary>
    internal bool LiquidMotionStopped { get; set; }

    /// <summary>per-line effect object variable two of the lava/acid effect overlay object: the ambient sound timer.</summary>
    private WrappingCountdown lavaSoundTimer;
    /// <summary>Current liquid rise or fall state selected by the FX record.</summary>
    private LiquidRisePhase liquidRisePhase;
    /// <summary>Audio requests emitted in the game's order during the current effect pass.</summary>
    private readonly List<SoundRequest> soundRequests = [];
    /// <summary>The room's power-bomb explosion, which suppresses effect sounds while it is active.</summary>
    private IActivePowerBomb? audioPowerBomb;
    /// <summary>Countdown between quake sound requests in earthquake FX rooms.</summary>
    internal WrappingCountdown EarthquakeSoundTimer { get; set; }
    /// <summary>Position in the room earthquake sound sequence.</summary>
    internal int EarthquakeSoundSequenceIndex { get; set; }

    /// <summary>The literal FX type byte selected from the active room/door record.</summary>
    public RoomFxType Type { get; internal set; }

    /// <summary>
    /// The liquid surface before tide displacement, a room Y with subpixels: the FX record's base whole pixels (a room
    /// load starts the fraction at half a pixel), moved toward <see cref="TargetSurface"/> by <see cref="YVelocity"/>.
    /// </summary>
    public FixedCoordinate BaseY { get; internal set; }

    /// <summary>
    /// Where a rising or falling liquid stops, at X zero. A Y from 32768 up disables the motion, as the game's
    /// sign test does.
    /// </summary>
    public Position16<RoomPixels> TargetSurface { get; private set; }

    /// <summary>The liquid's signed 8.8 vertical velocity in room pixels per update while it moves toward its target.</summary>
    public SignedEightEight YVelocity { get; private set; }

    /// <summary>Zero-extended FX timer byte loaded from record offset eight.</summary>
    public WrappingCountdown Timer { get; private set; }

    /// <summary>FX B layer-blending selector installed by the effect pre-instruction.</summary>
    public LayerBlendingConfiguration LayerBlendConfiguration { get; private set; } = LayerBlendingConfiguration.Unconfigured;

    /// <summary>Live effect overlay scroll-register shadows (the effect overlay's scroll).</summary>
    public Position16<OverlayPixels> Scroll { get; private set; }

    /// <summary>Whether the translated effect supplies a gameplay-region effect overlay plane.</summary>
    public bool IsRenderable => ShowsPage(Type);

    /// <summary>Whether a room of effect <paramref name="type"/> shows the effect's authored page on the effect overlay.</summary>
    public static bool ShowsPage(RoomFxType type) => type is
        RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water or RoomFxType.TourianEntranceStatue or
        RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;

    /// <summary>
    /// The live liquid surface after rising and tide processing, at X zero. A Y from 32768 up (65535 with no liquid)
    /// lies above the room, as the game's sign test reads it.
    /// </summary>
    public Position16<RoomPixels> Surface { get; internal set; } = NoSurface;

    /// <summary>The surface published while no liquid is loaded.</summary>
    internal static Position16<RoomPixels> NoSurface { get; } = new(0, ushort.MaxValue);

    /// <summary>The liquid options the FX record selected, as PLMs have since changed them.</summary>
    public RoomLiquidOptions LiquidOptions { get; internal set; }

    /// <summary>
    /// Sound requests emitted by the current effect pass in queue order.
    /// The list is cleared at the beginning of every <see cref="Step"/> call.
    /// </summary>
    public IReadOnlyList<SoundRequest> SoundRequests => soundRequests;

    /// <summary>
    /// Global earthquake words requested by the current effect pass. The timer value is a
    /// bit mask because the game sets the bits, not assigns them.
    /// </summary>
    public RoomFxEarthquakeRequest? EarthquakeRequest { get; private set; }

    /// <summary>Builds one room's effect-overlay FX from the current door's FX record.</summary>
    /// <param name="session">The Fireflea state, fixed colour and installed FX artwork, which outlive each room.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the FX record.</param>
    /// <param name="randomNumber">The session random number rain reads.</param>
    /// <param name="room">The room, for rooms that suppress the earthquake sound.</param>
    public static RoomFxOverlayState ForRoom(
        RoomFxSession session,
        RoomFxHeader? fx,
        DoorDefinition? door,
        int randomNumber,
        RoomHeaderDefinition? room = null) =>
        ForRecord(session, new RoomFxLoadSource { Fx = fx, Door = door, RandomNumber = randomNumber, Room = room, SuppliedDefinition = null });

    /// <summary>Builds one room's effect-overlay FX from a load source, which may supply its record directly.</summary>
    /// <param name="session">The Fireflea state, fixed colour and installed FX artwork, which outlive each room.</param>
    /// <param name="source">The FX record's source.</param>
    internal static RoomFxOverlayState ForRecord(RoomFxSession session, RoomFxLoadSource source)
    {
        var fx = new RoomFxOverlayState(session);
        fx.SelectRecord(source);
        return fx;
    }

    /// <summary>
    /// Selects the door's FX record and initializes the liquid and palette state shared by room effects and PLMs.
    /// The effect overlay plane the room shows is cleared under the HUD with the effect's page from row 31 (<see cref="FxOverlayPlane"/>).
    /// </summary>
    private void SelectRecord(RoomFxLoadSource source)
    {
        if (source.Room is { } room && RoomEarthquakeDefinitions.SoundSuppressedRooms.Contains(room))
            EarthquakeSoundTimer = WrappingCountdown.Start(ushort.MaxValue);
        if ((source.SuppliedDefinition ?? source.Fx?.Select(source.Door)) is not { } definition)
            return;

        BaseY = BaseY.WithWhole(definition.BaseYPosition ?? UnnamedSurfaceY);
        TargetSurface = new(0, definition.TargetYPosition ?? UnnamedSurfaceY);
        YVelocity = definition.YVelocity;
        Timer = WrappingCountdown.FromRaw(definition.Timer);
        LiquidOptions = definition.LiquidOptions;
        Surface = new(0, BaseY.Whole);

        Type = definition.Type;
        LayerBlendConfiguration = definition.EffectLayerBlend;
        animatedTiles.Load(Type);
        PaletteBlend = definition.PaletteBlend;

        if (Type == RoomFxType.Fireflea)
            Fireflea.Initialize();
        if (!IsRenderable)
            return;

        // The room shows this effect's page under the HUD until the next room.
        EffectPageType = Type;

        if (RoomFxTypes.UsesWater(Type))
        {
            // Both spawned per-line effect objects execute their phase initializer on the first
            // handler pass. A one-frame timer reproduces that first-call rotation.
            waterOverlayWaveTimer = WrappingCountdown.Start(1);
            waterBackgroundWaveTimer = WrappingCountdown.Start(1);
        }
        else if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            // Lava and acid start with timer B at one, so the first per-line effect object pass rotates A
            // from zero to $1E and then reloads either four or six. Expressing the already-
            // resolved waveform phase as zero produces the same first visible table.
            lavaAcidBackgroundWaveTimer = WrappingCountdown.FromRaw(UsesLavaAcidVerticalWave
                ? LavaAcidWaveDefinitions.VerticalWavePhaseDuration
                : LavaAcidWaveDefinitions.HorizontalWavePhaseDuration);
            lavaAcidBackgroundWavePhase = 0;
        }
        else if (Type == RoomFxType.Rain)
        {
            // Rain masks the random word with six *after* shifting it once,
            // producing byte offsets 0/2/4/6 into a word table. Expressed as a C#
            // element index that is bits two and three of the original random word.
            rainWindVelocity = new Offset16<OverlaySubpixels>(
                WordWrap.Signed(AtmosphereDrift.RainHorizontalVelocity((source.RandomNumber >> 2) & 3)), 0);
        }
    }

    /// <summary>
    /// True for the liquids whose effect overlay per-line effect step moves the surface: lava and
    /// acid and water. That motion belongs to the per-line effect pass.
    /// </summary>
    public bool MovesLiquidBeforeMainLoop =>
        Type is RoomFxType.Lava or RoomFxType.Acid || RoomFxTypes.UsesWater(Type);

    /// <summary>
    /// Runs liquid motion and shared-state writes for lava, acid and water before
    /// main-loop random-number generation. Their setup installs the callback on the
    /// first per-line effect pass; lava/acid's callback then swaps the shared random bytes on
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
        EarthquakeRequest = null;
        soundRequests.Clear();
        if (!LavaAcidOverlayFrameBehaviorInstalled)
        {
            LavaAcidOverlayFrameBehaviorInstalled = true;
            // Water's setup sets the sound timer in the same pass that installs the frame behaviour.
            if (lavaOrAcid)
                lavaSoundTimer = WrappingCountdown.Start(LavaAcidWaveDefinitions.AmbientSoundPeriod);
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
        system.SwapRandomBytes();
    }

    /// <summary>
    /// The lava sound clock: while a lava surface's Y is not negative, its timer
    /// counts down and, on reaching zero, reloads and queues a random ambient sound.
    /// Acid shares the per-frame behaviour step but not this sound.
    /// </summary>
    private void QueueLavaAmbientSound(int random)
    {
        if (Type != RoomFxType.Lava || CoordinateAxis.AsSigned(Surface.Y) < 0)
            return;
        lavaSoundTimer = lavaSoundTimer.Decrement().Remaining;
        if (!lavaSoundTimer.IsZero)
            return;
        lavaSoundTimer = WrappingCountdown.Start(LavaAcidWaveDefinitions.AmbientSoundPeriod);
        soundRequests.Add(new SoundRequest(
            LiquidEffects.LavaAmbientSound(random),
            LavaAcidWaveDefinitions.AmbientSoundPriority,
            SoundSuppressed: audioPowerBomb?.IsActive == true));
    }

    /// <summary>Runs the active effect's per-frame behaviour step and rain animtile handler.</summary>
    public void Step(ArtworkFrameSelections frames, RoomFxOverlayFrame frame)
    {
        Ensure.NotNull(frames);
        audioPowerBomb = frame.PowerBomb;
        if (!frame.LiquidMotionAlreadyAdvanced)
        {
            soundRequests.Clear();
            EarthquakeRequest = null;
        }
        if (Type == RoomFxType.Fireflea)
        {
            LayerBlendConfiguration = LayerBlendingConfiguration.Fireflea;
            Fireflea.Step(FixedColors, frame.TimeIsFrozen, frame.FirefleaDarknessLevel);
            return;
        }
        if (!IsRenderable || frame.TimeIsFrozen)
            return;

        // The shared animated-tiles handler runs independently of the per-line effect
        // pre-instruction. In particular, lava/acid need this transfer before their effect overlay
        // tilemap can name anything other than stale standard-HUD characters.
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
            // Spores sample each signed accumulator before adding its velocity.
            // Unlike rain, spores are anchored to the current camera, not its delta.
            Scroll = frame.Camera.AsOverlayScroll() + drift.WholePixels();
            drift = drift.WithY(WordWrap.Signed(drift.Y - AtmosphereDriftDefinitions.SporesVerticalVelocity));
            return;
        }

        // Fog anchors to the foreground and moves its texture by -$40/+50 in 8.8.
        Scroll = frame.Camera.AsOverlayScroll() + drift.WholePixels();
        drift = new(WordWrap.Signed(drift.X + AtmosphereDriftDefinitions.FogHorizontalVelocity),
            WordWrap.Signed(drift.Y - AtmosphereDriftDefinitions.FogVerticalVelocity));
    }

    /// <summary>
    /// Seeds camera-dependent liquid registers during a room transition without advancing
    /// a wave clock. Destination rendering can therefore show the correct surface on its
    /// first frame boundary instead of waiting for the first gameplay handler call.
    /// </summary>
    public void PrimeViewport(Position16<RoomPixels> camera)
    {
        if (Type is not (RoomFxType.Water or RoomFxType.TourianEntranceStatue or RoomFxType.Lava or RoomFxType.Acid))
            return;
        Surface = new(0, BaseY.Whole);
        waterSurfaceScreenY = CoordinateAxis.Difference(Surface.Y, camera.Y);
        Scroll = camera.AsOverlayScroll().WithY(ComputeLiquidVerticalScroll(Surface.Y, camera.Y));
    }

    /// <summary>Captures values visible alongside the current frame boundary's sprite frame.</summary>
    public RoomFxOverlayRenderSnapshot? CaptureForDisplay() => IsRenderable
        ? new RoomFxOverlayRenderSnapshot(
            Type,
            LayerBlendConfiguration,
            Scroll,
            Surface,
            LiquidOptions,
            waterOverlayWavePhase,
            waterBackgroundWavePhase,
            waterSurfaceScreenY)
        {
            LavaAcidBackgroundWavePhase = lavaAcidBackgroundWavePhase,
        }
        : null;


    /// <summary>
    /// Applies shared liquid writes made by room PLMs and enemy instructions. These are
    /// intentionally not a rendering shortcut: PLMs write the same fields the
    /// room-FX loader sets. The liquid handler consumes those shared words
    /// on its next ordinary effect pass, just as the game does.
    /// </summary>
    internal void SetLiquidMotion(
        int? baseYPosition = null,
        int? targetYPosition = null,
        int? packedYVelocity = null,
        int? timer = null)
    {
        // Each value is a word the writer copies; the checked conversions reject anything wider.
        if (baseYPosition is int baseY)
            BaseY = BaseY.WithWhole(baseY);
        if (targetYPosition is int targetY)
            TargetSurface = new(0, targetY);
        if (packedYVelocity is int velocity)
            YVelocity = new(velocity);
        if (timer is int newTimer)
            Timer = WrappingCountdown.FromRaw(newTimer);
    }

    /// <summary>
    /// LoadFxEntry reloads motion, blending and three colors, but does not restart
    /// per-line effects, animated tiles, or the liquid's current motion phase as a room load would.
    /// </summary>
    internal LayerBlendingConfiguration ApplyEntry(RoomFxRecordDefinition definition)
    {
        Ensure.NotNull(definition);
        BaseY = BaseY.WithWhole(definition.BaseYPosition ?? UnnamedSurfaceY);
        TargetSurface = new(0, definition.TargetYPosition ?? UnnamedSurfaceY);
        YVelocity = definition.YVelocity;
        Timer = WrappingCountdown.FromRaw(definition.Timer);
        LiquidOptions = definition.LiquidOptions;
        LayerBlendConfiguration = definition.EffectLayerBlend;
        PaletteBlend = definition.PaletteBlend;
        return definition.DefaultLayerBlend;
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
    /// Applies the enable-water-physics PLM step to the
    /// liquid-options word. The PLM clears only its own bit, so
    /// this mutation must live on the room-FX owner that republishes the word to Samus;
    /// changing only Samus's projected copy would be undone by the next FX frame.
    /// </summary>
    /// <summary>
    /// Clears the water-physics-disabled bit as <c>Instruction_PLM_EnableWaterPhysics</c> does; see
    /// <see cref="SetLiquidMotion"/> for why the room-FX owner holds the word.
    /// </summary>
    internal void EnableWaterPhysics() =>
        LiquidOptions &= ~RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave;

    /// <summary>
    /// Runs the observable static-water path of the water effect and both circular wave
    /// clocks. Rising/tidal rooms retain their loaded surface words; the presently reported
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
        waterSurfaceScreenY = CoordinateAxis.Difference(Surface.Y, camera.Y);
        Scroll = (camera.AsOverlayScroll() + waterDrift.WholePixels()).WithY(ComputeLiquidVerticalScroll(Surface.Y, camera.Y));

        waterOverlayWaveTimer = waterOverlayWaveTimer.Decrement().Remaining;
        if (waterOverlayWaveTimer.IsZero)
        {
            waterOverlayWaveTimer = WrappingCountdown.Start(WaterWaveDefinitions.OverlayWavePhaseDuration);
            waterOverlayWavePhase =
                (waterOverlayWavePhase + 1) % WaterWaveDefinitions.WaveDisplacementCount;
        }

        if ((LiquidOptions & RoomLiquidOptions.SurfaceDrift) != RoomLiquidOptions.None)
            waterDrift += WaterDriftVelocity;

        // FX $26 spawns only the effect overlay water object. Its second per-line effect object moves
        // the statue vertically instead of installing the ordinary background X wave.
        if (Type != RoomFxType.Water || (LiquidOptions & RoomLiquidOptions.BackgroundWave) == RoomLiquidOptions.None)
            return;
        waterBackgroundWaveTimer = waterBackgroundWaveTimer.Decrement().Remaining;
        if (waterBackgroundWaveTimer.IsZero)
        {
            waterBackgroundWaveTimer = WrappingCountdown.Start(WaterWaveDefinitions.BackgroundWavePhaseDuration);
            waterBackgroundWavePhase =
                (waterBackgroundWavePhase + 1) % WaterWaveDefinitions.WaveDisplacementCount;
        }
    }

    /// <summary>Advances rise and tide state before publishing the current liquid surface.</summary>
    private void AdvanceLiquidMotion(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        StepLiquidRise(randomNumber, earthquakeSoundDelay);
        StepLiquidTide();
        Surface = new(0, ComputeTidalYPosition());
    }

    /// <summary>
    /// Runs the visible outputs of the lava/acid effect and its heat haze. The liquid
    /// surface and the effect overlay plane follow the room camera, while one of two sixteen-scanline background
    /// waveforms rotates at the cadence selected by the record's liquid-options byte.
    /// </summary>
    private void StepLavaAcid(
        Position16<RoomPixels> camera,
        int randomNumber,
        PowerOnEarthquakeSoundDelay earthquakeSoundDelay,
        bool liquidMotionAlreadyAdvanced)
    {
        if (!liquidMotionAlreadyAdvanced)
            AdvanceLiquidMotion(randomNumber, earthquakeSoundDelay);
        waterSurfaceScreenY = CoordinateAxis.Difference(Surface.Y, camera.Y);
        // Lava/acid uses the same surface-relative vertical scroll equation as water.
        // Zeroing it anchored the animated characters to the screen instead of
        // beginning their first texture row immediately below the liquid boundary.
        Scroll = camera.AsOverlayScroll().WithY(ComputeLiquidVerticalScroll(Surface.Y, camera.Y));

        if (!UsesLavaAcidVerticalWave && !UsesLavaAcidHorizontalWave)
            return;
        lavaAcidBackgroundWaveTimer = lavaAcidBackgroundWaveTimer.Decrement().Remaining;
        if (!lavaAcidBackgroundWaveTimer.IsZero)
            return;

        lavaAcidBackgroundWavePhase =
            (lavaAcidBackgroundWavePhase - 1 + LavaAcidWaveDefinitions.WaveDisplacementCount) %
            LavaAcidWaveDefinitions.WaveDisplacementCount;
        lavaAcidBackgroundWaveTimer = WrappingCountdown.FromRaw(UsesLavaAcidVerticalWave
            ? LavaAcidWaveDefinitions.VerticalWavePhaseDuration
            : LavaAcidWaveDefinitions.HorizontalWavePhaseDuration);
    }

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
                int velocity = YVelocity.SignedRawValue;
                bool movesTowardTarget = velocity > 0
                    ? TargetSurface.Y > BaseY.Whole
                    : velocity < 0 && TargetSurface.Y < BaseY.Whole;
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
        EarthquakeRequest = new RoomFxEarthquakeRequest(
            RoomEarthquakeDefinitions.RisingLiquidType,
            RoomEarthquakeDefinitions.RisingLiquidTimerBits);
    }

    /// <summary>
    /// Runs the signed timer and eight-entry earthquake-sound loop.
    /// All eight sound words are library-two Earthquake; the negative ninth
    /// word resets selection to zero. Constant sound selection and the index bound
    /// replace those words; the independently retained rhythm supplies only delays.
    /// </summary>
    /// <param name="randomNumber">Current RNG word, sampled without advancing it; its low two bits add zero through three updates to the next earthquake-sound delay.</param>
    /// <param name="earthquakeSoundDelay">
    /// The frame the save file found at power-on adds to the next delay. The earthquake sound request restores the carry
    /// with the status restore, so the delay addition adds it. Both callers' per-line effect objects are the first their FX
    /// spawns after the door deletes the per-line effect objects, so they run in slot zero, which inherits the main loop's
    /// power-on carry (<see cref="PowerOnEarthquakeSoundDelay"/>); a later slot would see the clear carry from
    /// the room-effect slot loop.
    /// </param>
    private void HandleEarthquakeSoundEffect(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        if (EarthquakeSoundTimer.IsNegative)
            return;

        EarthquakeSoundTimer = EarthquakeSoundTimer.Decrement().Remaining;
        if (!EarthquakeSoundTimer.IsNegative)
            return;

        ReadOnlySpan<ushort> baseTimers =
            RoomEarthquakeDefinitions.RisingLiquidSoundBaseTimers;
        if ((uint)EarthquakeSoundSequenceIndex >= (uint)baseTimers.Length)
            EarthquakeSoundSequenceIndex = 0;

        soundRequests.Add(new SoundRequest(
            Library2Sound.Lavaquake,
            Priority: SoundRequestPriority.Normal,
            SoundSuppressed: audioPowerBomb?.IsActive == true));
        EarthquakeSoundTimer = WrappingCountdown.FromRaw(baseTimers[EarthquakeSoundSequenceIndex])
            .AddFrames((randomNumber & 3) + earthquakeSoundDelay.ExtraFrames);
        EarthquakeSoundSequenceIndex++;
    }

    /// <summary>The statue per-line effect steps call the same earthquake sound owner.</summary>
    internal void PublishStatueEarthquakeSound(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay) =>
        HandleEarthquakeSoundEffect(randomNumber, earthquakeSoundDelay);

    /// <summary>Runs <c>RaiseOrLowerFx</c> for the shared liquid words.</summary>
    private bool AdvanceBaseYToTarget()
    {
        if (CoordinateAxis.AsSigned(TargetSurface.Y) < 0)
            return true;

        int velocity = YVelocity.SignedRawValue;
        int delta = SixteenSixteen.FromEightEight(velocity);
        uint fixedPosition = unchecked(BaseY.RawValue + (uint)delta);
        int candidate = FixedCoordinate.WholeOf(unchecked((int)fixedPosition));
        BaseY = FixedCoordinate.FromRaw(fixedPosition);

        if (velocity >= 0)
        {
            if (WordWrap.Signed(candidate) < 0)
                candidate = ushort.MaxValue;
            BaseY = BaseY.WithWhole(candidate);
            if (candidate <= TargetSurface.Y)
                return false;
        }
        else
        {
            if (WordWrap.Signed(candidate) < 0)
                candidate = 0;
            BaseY = BaseY.WithWhole(candidate);
            if (candidate > TargetSurface.Y)
                return false;
        }

        BaseY = new FixedCoordinate(TargetSurface.Y, 0);
        return true;
    }

    /// <summary>
    /// Runs <c>FxHandleTide</c>. The game writes the scaled sine word starting
    /// one byte into the 16.16 offset pair, equivalent to the eight-bit shift below. The
    /// asymmetric phase deltas deliberately spend different durations in each half-wave.
    /// </summary>
    internal void StepLiquidTide()
    {
        int scale;
        int positiveDelta;
        int negativeDelta;
        if ((LiquidOptions & RoomLiquidOptions.SmallTide) != RoomLiquidOptions.None)
        {
            scale = LiquidTideDefinitions.SmallTideScale;
            positiveDelta = LiquidTideDefinitions.SmallTidePositivePhaseDelta;
            negativeDelta = LiquidTideDefinitions.SmallTideNegativePhaseDelta;
        }
        else if ((LiquidOptions & RoomLiquidOptions.LargeTide) != RoomLiquidOptions.None)
        {
            scale = LiquidTideDefinitions.LargeTideScale;
            positiveDelta = LiquidTideDefinitions.LargeTidePositivePhaseDelta;
            negativeDelta = LiquidTideDefinitions.LargeTideNegativePhaseDelta;
        }
        else
        {
            TideFixedOffset = 0;
            return;
        }

        // The tide indexes the negative-cosine prefix, not the sine origin.
        int sample = Trigonometry.SignedNegativeCosineWord(Ensure.BetweenInclusive(0, byte.MaxValue, EightEight.WholeOf(TidePhase)));
        TideFixedOffset = SixteenSixteen.FromEightEight(sample * scale);
        AdvanceTidePhase(sample >= 0 ? positiveDelta : negativeDelta);
    }

    /// <summary>Moves the tide phase on by <paramref name="delta"/> units, returning to zero after a full turn.</summary>
    private void AdvanceTidePhase(int delta) => TidePhase = (TidePhase + delta) % TidePhasePeriod;

    /// <summary>Adds signed fixed-point tide displacement to the base liquid surface.</summary>
    private int ComputeTidalYPosition()
    {
        uint baseFixed = BaseY.RawValue;
        return FixedCoordinate.WholeOf(unchecked((int)(baseFixed + (uint)TideFixedOffset)));
    }

    /// <summary>Whether the FX options select vertical background displacement for lava or acid.</summary>
    private bool UsesLavaAcidVerticalWave =>
        (LiquidOptions & RoomLiquidOptions.BackgroundWave) != RoomLiquidOptions.None;

    /// <summary>Whether the FX options select horizontal background displacement for lava or acid.</summary>
    private bool UsesLavaAcidHorizontalWave =>
        (LiquidOptions & RoomLiquidOptions.WaterPhysicsDisabledOrHorizontalHeatWave) != RoomLiquidOptions.None;

    /// <summary>Computes effect overlay vertical scroll from the liquid surface relative to camera Y.</summary>
    private static int ComputeLiquidVerticalScroll(int surfaceY, int cameraY)
    {
        if (CoordinateAxis.AsSigned(surfaceY) < 0)
            return 0;
        int relative = CoordinateAxis.Difference(surfaceY, cameraY);
        if (relative <= 0)
            return WordWrap.Unsigned(((relative ^ 31) & 31) | 256);
        return relative < 256
            ? WordWrap.LowByte(~relative)
            : 0;
    }

    /// <summary>Creates one room's effect state, with no FX record selected, over the session's carried state.</summary>
    /// <param name="session">The Fireflea state, fixed colour and installed FX artwork, which outlive each room.</param>
    public RoomFxOverlayState(RoomFxSession session)
    {
        _session = Ensure.NotNull(session);
        Type = RoomFxType.None;
        TargetSurface = Position16<RoomPixels>.Zero;
        YVelocity = default;
        Timer = WrappingCountdown.Start(0);
        LayerBlendConfiguration = default;
        Scroll = Position16<OverlayPixels>.Zero;
        drift = Offset16<OverlaySubpixels>.Zero;
        rainWindVelocity = Offset16<OverlaySubpixels>.Zero;
        previousCamera = Position16<RoomPixels>.Zero;
        // Half a pixel: the liquid starts centred on its whole position.
        BaseY = new FixedCoordinate(0, 1 << 15);
        TidePhase = 0;
        TideFixedOffset = 0;
        Surface = NoSurface;
        LiquidOptions = RoomLiquidOptions.None;
        waterDrift = Offset16<OverlaySubpixels>.Zero;
        waterBackgroundWaveTimer = WrappingCountdown.Start(0);
        waterOverlayWaveTimer = WrappingCountdown.Start(0);
        waterOverlayWavePhase = waterBackgroundWavePhase = 0;
        waterSurfaceScreenY = short.MaxValue;
        lavaAcidBackgroundWaveTimer = WrappingCountdown.Start(0);
        lavaAcidBackgroundWavePhase = 0;
        liquidRisePhase = LiquidRisePhase.Dormant;
        LavaAcidOverlayFrameBehaviorInstalled = false;
        LiquidMotionStopped = false;
        lavaSoundTimer = WrappingCountdown.Start(0);
        EarthquakeRequest = null;
        EarthquakeSoundTimer = WrappingCountdown.Start(0);
        EarthquakeSoundSequenceIndex = 0;
    }

    /// <summary>
    /// Deletes the liquid's scroll per-line effect objects, as zeroing their channel words does: no
    /// later pass moves the liquid, queues its ambient sound or (for lava/acid) swaps the
    /// RNG bytes. Effects without per-line liquid motion are unaffected.
    /// </summary>
    public void StopLiquidMotion() => LiquidMotionStopped = true;

    /// <summary>Rain's texture fall each frame, in 8.8 pixels.</summary>
    private static readonly Offset16<OverlaySubpixels> RainFallVelocity =
        new(0, unchecked(-AtmosphereDriftDefinitions.RainVerticalVelocity));

    /// <summary>Water's surface drift each frame its <see cref="RoomLiquidOptions.SurfaceDrift"/> option is set.</summary>
    private static readonly Offset16<OverlaySubpixels> WaterDriftVelocity =
        new(WordWrap.Signed(WaterWaveDefinitions.HorizontalSubscrollVelocity), 0);

    /// <summary>The mutually exclusive callbacks installed in the rise-function word.</summary>
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
