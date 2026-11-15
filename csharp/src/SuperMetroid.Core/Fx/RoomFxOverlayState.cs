using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Fx;

/// <summary>
/// Game-owned room effect the room's effect selection shows for the door Samus entered by.
/// </summary>
/// <remarks>
/// The room's liquid has a level, a target level it moves toward, a speed and a wait before it moves. The room effect's
/// load gives them, and a room with no effect has neither level nor target. Room objects and enemies then move the liquid
/// with its own operations (<see cref="SetLiquidLevel"/>, <see cref="SetLiquidSpeed"/>, <see cref="MoveLiquidAfter"/>,
/// <see cref="MoveLiquidTo"/>, <see cref="EndLiquidWaitNextUpdate"/>, <see cref="HaltLiquid"/>), whatever the effect
/// shows on its overlay; the liquid's motion acts on them on its next pass.
/// The modelled effect overlay effects are lava, acid, water, spores, Landing Site rain, and Climb fog.
/// </remarks>
[RoomLifetime]
public sealed class RoomFxOverlayState : IGameplayShadingSource
{
    /// <summary>Tracks frame selection for the liquid, rain, fog, and spore block animations.</summary>
    private readonly BlockAnimationPlayer blockAnimations = new();

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
    /// Whether the room's last applied effect animates Samus's visor (<see cref="RoomEffectDefinition.VisorAnimation"/>):
    /// selected when the room loads, steady for a room with no effect, and replaced when a fight applies another effect.
    /// </summary>
    public RoomVisorAnimation VisorAnimation { get; private set; }

    /// <summary>The effect whose pictures the room's effect overlay shows, chosen when the room loaded; null when it shows none.</summary>
    public RoomFxType? OverlayEffect { get; private set; }

    /// <summary>The effect overlay the room shows (#1348, #1466, #1601): its effect's liquid or atmosphere when it has one, otherwise nothing.</summary>
    public FxOverlayPlane EffectPlane => FxOverlayPlane.Of(OverlayEffect);

    /// <summary>
    /// The tint the last effect loaded selected for the effect overlay's blend colours (#1512), or null while no effect
    /// has loaded one and the colours stay the tileset's.
    /// </summary>
    public RoomEffectTint? Tint { get; private set; }
    /// <summary>
    /// Rain, fog, and spores texture drift: one signed 1/256-pixel running sum per axis, its fractions kept between
    /// updates. Its whole pixels shift the effect overlay's scroll before the next velocity step; a sum carried past
    /// either end of its range continues from the other, which keeps the drifting textures' pictures as they have always
    /// been drawn.
    /// </summary>
    private EightEightVector<OverlayPixels> drift;
    /// <summary>
    /// Rain's sideways texture drift each frame, the wind chosen from the random number when the room loads; it only
    /// blows across. The fall is <see cref="RainFall"/>.
    /// </summary>
    private EightEightVector<OverlayPixels> rainWind;
    /// <summary>Previous camera point, used to make rain follow camera movement.</summary>
    private Position16<RoomPixels> previousCamera;
    /// <summary>The fraction a room load starts the liquid's surface at: half a pixel, centring the liquid on its whole line.</summary>
    private const int LoadedSurfaceFraction = (FixedCoordinate.LastFraction + 1) / 2;
    /// <summary>
    /// How far round its rise and fall the liquid's tide is: the surface sits at the negative cosine of this angle's whole
    /// step, and <see cref="AdvanceTidePhase"/> turns it on, continuing round past a full turn.
    /// </summary>
    internal RotationAngle TidePhase { get; set; }
    /// <summary>How far the tide displaces the liquid surface from its base line, in pixels and subpixels.</summary>
    internal SignedSixteenSixteen TideOffset { get; set; }
    /// <summary>
    /// Water's effect overlay surface drift: a signed 1/256-pixel horizontal running sum, its fraction kept between updates,
    /// whose whole pixels offset the surface texture. Only X moves.
    /// </summary>
    private EightEightVector<OverlayPixels> waterDrift;
    /// <summary>Where water's surface wave on the effect overlay stands in its cycle.</summary>
    private WaveCycle waterOverlayWave;
    /// <summary>Where water's optional sideways background wave stands in its cycle.</summary>
    private WaveCycle waterBackgroundWave;
    /// <summary>
    /// The screen line of the liquid's surface below the camera's top, negative for a surface above the screen, used to
    /// clip the effect plane; null while the liquid has no surface or none has been placed on the screen yet.
    /// </summary>
    private int? waterSurfaceScreenY;
    /// <summary>
    /// The screen line the liquid was last placed on the screen at (<see cref="RoomFxOverlayRenderSnapshot.LiquidSurfaceLine"/>),
    /// or null while it is not placed: none has been, or the liquid had no surface within the room then.
    /// </summary>
    private int? liquidSurfaceLine;
    /// <summary>Countdown for lava or acid's optional background wave.</summary>
    private int lavaAcidBackgroundWaveTimer;
    /// <summary>Selected lava or acid background wave displacement frame.</summary>
    private int lavaAcidBackgroundWavePhase;
    /// <summary>
    /// Whether the liquid's motion has begun (<see cref="LiquidMotionStart"/>): a room's liquid begins moving on its
    /// second motion pass after the room loads, and each pass from then moves the lava, acid or water before the main
    /// loop's random number, until the next room load.
    /// </summary>
    internal LiquidMotionStart MotionStart { get; set; }
    /// <summary>
    /// Records an enemy ending the liquid's scrolling motion directly, which stops
    /// its per-frame updates until the next room load.
    /// </summary>
    internal bool LiquidMotionStopped { get; set; }

    /// <summary>The lava/acid effect overlay's ambient sound timer.</summary>
    private int lavaSoundTimer;
    /// <summary>Current liquid rise or fall state the room effect's liquid motion selects.</summary>
    private LiquidRisePhase liquidRisePhase;
    /// <summary>
    /// The update's audio requests the effect's sounds join as it makes them: the runtime's, once it points the room's
    /// systems at it; the effect's own requests before that.
    /// </summary>
    internal GameplayAudioRequests AudioRequests { get; private set; } = new();
    /// <summary>
    /// Countdown to the next quake sound while rising lava or acid or the Tourian statues rumble: the sound plays on the
    /// update the countdown runs past zero.
    /// </summary>
    internal WrappingCountdown EarthquakeSoundTimer { get; set; }

    /// <summary>
    /// Where the quake sounds stand in their rhythm (<see cref="RoomEarthquakeDefinitions.RisingLiquidSoundBaseDelays"/>):
    /// the delay the next sound waits after it plays, returning to the first after the last.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The position lies outside the rhythm.</exception>
    internal int EarthquakeSoundRhythmPosition
    {
        get;
        set => field = Ensure.BetweenInclusive(0, RoomEarthquakeDefinitions.RisingLiquidSoundBaseDelays.Count - 1, value);
    }

    /// <summary>
    /// Whether the room's quakes are silent (<see cref="RoomEarthquakeDefinitions.SoundSuppressedRooms"/>): rising lava or
    /// acid and the Tourian statues shake such a room without their quake sound.
    /// </summary>
    internal bool QuakeSoundsSilenced { get; private set; }

    /// <summary>The kind of effect the room shows.</summary>
    public RoomFxType Type { get; internal set; }

    /// <summary>
    /// The liquid surface before tide displacement, a room line at or below the room's top with its fraction: the room
    /// effect's starting level (a room load starts the fraction at half a pixel), moved toward <see cref="TargetSurface"/> by
    /// <see cref="LiquidLevelSpeed"/>; null when the effect names no surface or the room has no effect (#1546).
    /// </summary>
    public FixedCoordinate? BaseY { get; internal set; }

    /// <summary>
    /// The room line, at or below the room's top, a rising or falling liquid stops at, or null when nothing names one and
    /// the liquid never moves (#1546).
    /// </summary>
    public int? TargetSurface { get; private set; }

    /// <summary>The liquid's signed 1/256-pixel level speed in room pixels per update while it moves toward its target: negative rises.</summary>
    public SignedEightEight LiquidLevelSpeed { get; private set; }

    /// <summary>
    /// The updates the liquid waits, once its level has somewhere to go, before it moves toward its target, as the room
    /// effect or a room object gives it. The wait counts down one update at a time and ends on reaching zero, so a wait of
    /// zero runs past zero and lasts a whole round of the countdown (<see cref="WrappingCountdown"/>).
    /// </summary>
    public WrappingCountdown LiquidMoveDelay { get; private set; }

    /// <summary>How the effect's overlay blends with the room (#1545), as the room effect describes it, or null when it shows no overlay.</summary>
    public RoomEffectBlend? Blend { get; private set; }

    /// <summary>
    /// The effect overlay's live scroll: an atmosphere's across and down; a liquid's across only, as its surface line
    /// places it down (#1706).
    /// </summary>
    public Position16<OverlayPixels> Scroll { get; private set; }

    /// <summary>Whether the room's effect shows its pictures on the effect overlay over the gameplay area.</summary>
    public bool IsRenderable => ShowsOnOverlay(Type);

    /// <summary>Whether a room of effect <paramref name="type"/> shows the effect's liquid or atmosphere on the effect overlay.</summary>
    public static bool ShowsOnOverlay(RoomFxType type) => type is
        RoomFxType.Lava or RoomFxType.Acid or RoomFxType.Water or RoomFxType.TourianEntranceStatue or
        RoomFxType.Rain or RoomFxType.Fog or RoomFxType.Spores;

    /// <summary>
    /// The live liquid surface's room line after rising and tide processing, or null while the liquid has no surface
    /// (#1546). A tide can carry the surface above the room's top, to a negative line, which every reader treats as no
    /// surface, as the game's below-zero check does.
    /// </summary>
    public int? Surface { get; internal set; }

    /// <summary>
    /// The liquid the room effect fills the room with, as room objects have since changed it, or null for an effect with no
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

    /// <summary>Builds one room's effect-overlay FX from the effect the current door selects.</summary>
    /// <param name="session">The Fireflea state, which outlives each room.</param>
    /// <param name="fx">The room state's FX definition.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    /// <param name="randomNumber">The session random number rain reads.</param>
    /// <param name="room">The room, for rooms that suppress the earthquake sound.</param>
    public static RoomFxOverlayState ForRoom(
        RoomFxSession session,
        RoomFxSelection? fx,
        DoorDefinition? door,
        int randomNumber,
        RoomDefinition? room = null) =>
        ForSource(session, new RoomFxLoadSource { Fx = fx, Door = door, RandomNumber = randomNumber, Room = room, SuppliedDefinition = null });

    /// <summary>Builds one room's effect-overlay FX from a load source, which may supply its effect directly.</summary>
    /// <param name="session">The Fireflea state, which outlives each room.</param>
    /// <param name="source">The room effect's source.</param>
    internal static RoomFxOverlayState ForSource(RoomFxSession session, RoomFxLoadSource source)
    {
        var fx = new RoomFxOverlayState(session);
        fx.SelectEffect(source);
        return fx;
    }

    /// <summary>
    /// Selects the door's room effect and initializes the liquid and tint state shared by room effects and room objects.
    /// From then until Samus leaves the room, the effect overlay shows the effect's liquid or atmosphere, or nothing for an
    /// effect without one (<see cref="FxOverlayPlane"/>).
    /// </summary>
    private void SelectEffect(RoomFxLoadSource source)
    {
        QuakeSoundsSilenced = source.Room is { } room && RoomEarthquakeDefinitions.SoundSuppressedRooms.Contains(room);
        if ((source.SuppliedDefinition ?? source.Fx?.Select(source.Door)) is not { } definition)
            return;

        LoadSurfaceAndTarget(definition);
        LiquidLevelSpeed = definition.LiquidLevelSpeed;
        LiquidMoveDelay = WrappingCountdown.Start(definition.LiquidMoveDelay);
        Liquid = definition.Liquid;
        Surface = BaseLine;

        Type = definition.Type;
        Blend = definition.EffectBlend;
        VisorAnimation = definition.VisorAnimation;
        blockAnimations.Play(BlockAnimationLoops.OnEffectOverlay(Type));
        Tint = definition.Tint;

        if (Type == RoomFxType.Fireflea)
            Fireflea.Initialize();
        if (!IsRenderable)
            return;

        // The room shows this effect on its overlay until Samus leaves it.
        OverlayEffect = Type;

        if (RoomFxTypes.UsesWater(Type))
        {
            // Both water waves, the surface's and the background's, start at their first phase.
            waterOverlayWave = WaveCycle.StartingAt(WaterWaveDefinitions.FirstPhase);
            waterBackgroundWave = WaveCycle.StartingAt(WaterWaveDefinitions.FirstPhase);
        }
        else if (Type is RoomFxType.Lava or RoomFxType.Acid)
        {
            // Lava and acid start their heat haze at its first displacement, held for a whole phase: the haze's
            // first step in the game moves it to that displacement and starts that wait, so the first haze shown is
            // the same.
            lavaAcidBackgroundWaveTimer = HazePhaseDuration;
            lavaAcidBackgroundWavePhase = 0;
        }
        else if (Type == RoomFxType.Rain)
        {
            rainWind = new(AtmosphereDrift.RainHorizontalVelocity(AtmosphereDrift.RainWindChosenBy(source.RandomNumber)),
                SignedEightEight.Zero);
        }
    }

    /// <summary>
    /// True for the liquids whose surface motion step moves the surface: lava and
    /// acid and water. That motion belongs to the liquid's motion pass.
    /// </summary>
    public bool MovesLiquidBeforeMainLoop =>
        Type is RoomFxType.Lava or RoomFxType.Acid || RoomFxTypes.UsesWater(Type);

    /// <summary>
    /// Loads the effect's liquid level and target: a named surface keeps the fraction the surface had, or a room load's half
    /// pixel when it had none.
    /// </summary>
    private void LoadSurfaceAndTarget(RoomEffectDefinition definition)
    {
        BaseY = definition.LiquidStartLevel is int baseY ? SurfaceAt(baseY) : null;
        TargetSurface = definition.LiquidTargetLevel is int targetY ? SurfaceLine(targetY) : null;
    }

    /// <summary>The base surface at whole line <paramref name="line"/>, keeping the current fraction or a room load's.</summary>
    private FixedCoordinate SurfaceAt(int line) =>
        (BaseY ?? new FixedCoordinate(0, LoadedSurfaceFraction)).WithWhole(SurfaceLine(line));

    /// <summary>
    /// Accepts a named surface or target line: a room line at or below the room's top, no deeper than the deepest room line
    /// (<see cref="CoordinateAxis.LastNonNegative"/>). The tide's surface line, the rise's test for passing the room's top
    /// and the overlay's test for no surface read a surface's room line, on which a liquid above the room's top lies at a
    /// negative line; a named line deeper than the deepest would read there (#1627).
    /// </summary>
    private static int SurfaceLine(int line) => Ensure.BetweenInclusive(0, CoordinateAxis.LastNonNegative, line);

    /// <summary>The base surface's whole line, or null without a surface.</summary>
    private int? BaseLine => BaseY is { } baseY ? RoomLineOf(baseY) : null;

    /// <summary>
    /// The room line a liquid level lies on: its whole pixels from the room's top, negative for a level above the room's
    /// top.
    /// </summary>
    private static int RoomLineOf(FixedCoordinate level) => (level - RoomTop).WholePixels;

    /// <summary>The room's top line, from which a liquid level's room line is measured.</summary>
    private static readonly FixedCoordinate RoomTop = default;

    /// <summary>
    /// Runs liquid motion and shared-state writes for lava, acid and water before
    /// main-loop random-number generation. The liquid's first motion pass after the room loads only begins its
    /// motion (<see cref="LiquidMotionStart"/>); lava/acid's motion then swaps the shared random state on
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
        if (!MotionStart.HasBegun)
        {
            MotionStart = LiquidMotionStart.Begun;
            // The pass that begins lava's or acid's motion also starts its ambient sound clock.
            if (lavaOrAcid)
                lavaSoundTimer = LavaAcidWaveDefinitions.AmbientSoundPeriod;
            return;
        }
        if (timeIsFrozen)
            return;
        if (!lavaOrAcid)
        {
            // Water's rise and tide; water leaves the random number alone.
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

    /// <summary>Runs the active effect's per-frame behaviour step and rain block animation.</summary>
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

        // The shared block animations run independently of the liquid motion's
        // per-frame step. In particular, lava and acid select the animation frame their pictures
        // show here, before the surface step that follows.
        blockAnimations.Step(frames);

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
            Scroll = Position16<OverlayPixels>.Zero + cameraDelta + drift.Whole;
            drift += rainWind + RainFall;
            previousCamera = frame.Camera;
            return;
        }

        if (Type == RoomFxType.Spores)
        {
            // Spores sample each signed running sum before adding its velocity.
            // Unlike rain, spores are anchored to the current camera, not its delta.
            Scroll = frame.Camera.AsOverlayScroll() + drift.Whole;
            drift += SporesRise;
            return;
        }

        // Fog anchors to the foreground; its texture drifts right as it rises.
        Scroll = frame.Camera.AsOverlayScroll() + drift.Whole;
        drift += FogDrift;
    }

    /// <summary>
    /// Seeds the liquid surface's camera-dependent screen line during a room transition without advancing
    /// a wave clock. Destination rendering can therefore show the correct surface on its
    /// first frame boundary instead of waiting for the first gameplay update.
    /// </summary>
    public void PrimeViewport(Position16<RoomPixels> camera)
    {
        if (!RoomFxTypes.ShowsLiquid(Type))
            return;
        Surface = BaseLine;
        PlaceLiquid(camera, Offset16<OverlayPixels>.Zero);
    }

    /// <summary>
    /// The surface's screen line below <paramref name="camera"/>'s top, negative for a surface above the screen, or null
    /// without a surface.
    /// </summary>
    private int? SurfaceScreenLine(Position16<RoomPixels> camera) =>
        Surface is int line ? (Position16<RoomPixels>.Zero + new Offset16<RoomPixels>(0, line)).RelativeToCamera(camera).Y : null;

    /// <summary>
    /// Places the liquid on the screen below <paramref name="camera"/> (#1706): its surface's screen line, which the effect
    /// overlay shows the liquid from while the surface lies within the room, and the overlay's horizontal scroll, the
    /// camera's moved by <paramref name="surfaceDrift"/>. A liquid scrolls only across: its surface line places it down.
    /// </summary>
    private void PlaceLiquid(Position16<RoomPixels> camera, Offset16<OverlayPixels> surfaceDrift)
    {
        waterSurfaceScreenY = SurfaceScreenLine(camera);
        liquidSurfaceLine = Surface is >= 0 ? waterSurfaceScreenY : null;
        Scroll = (camera.AsOverlayScroll() + surfaceDrift).WithY(0);
    }

    /// <summary>Captures values visible alongside the current frame boundary's sprite frame.</summary>
    public RoomFxOverlayRenderSnapshot? CaptureForDisplay() => IsRenderable
        ? new RoomFxOverlayRenderSnapshot(
            Type,
            Blend,
            Scroll,
            Surface,
            Liquid,
            waterOverlayWave.Phase,
            waterBackgroundWave.Phase,
            waterSurfaceScreenY)
        {
            LavaAcidBackgroundWavePhase = lavaAcidBackgroundWavePhase,
            LiquidSurfaceLine = liquidSurfaceLine,
        }
        : null;


    /// <summary>
    /// Moves the liquid's surface to room line <paramref name="line"/>, keeping its fraction, as the Chozo statue's lowered
    /// acid and the Speed Booster escape's lava stages do. The liquid's motion carries on from there on its next pass.
    /// </summary>
    /// <param name="line">The room line, at or below the room's top and no deeper than the deepest room line.</param>
    internal void SetLiquidLevel(int line) => BaseY = SurfaceAt(line);

    /// <summary>Changes the speed the liquid moves toward its target at, leaving its target and its wait.</summary>
    /// <param name="speed">The level speed in room pixels an update: negative rises.</param>
    internal void SetLiquidSpeed(SignedEightEight speed) => LiquidLevelSpeed = speed;

    /// <summary>
    /// Sets the liquid moving toward its target at <paramref name="speed"/> once it has waited <paramref name="delay"/>
    /// updates, as the Chozo statue lowers the acid and the escape's trigger raises it.
    /// </summary>
    /// <param name="delay">The updates the liquid waits once its level has somewhere to go (<see cref="LiquidMoveDelay"/>).</param>
    /// <param name="speed">The level speed in room pixels an update: negative rises.</param>
    internal void MoveLiquidAfter(int delay, SignedEightEight speed)
    {
        LiquidLevelSpeed = speed;
        LiquidMoveDelay = WrappingCountdown.Start(delay);
    }

    /// <summary>
    /// Gives the liquid a new target, <paramref name="target"/>, and sets it moving there at <paramref name="speed"/> once it
    /// has waited <paramref name="delay"/> updates, as Ridley's fight raises and drains its room's lava.
    /// </summary>
    /// <param name="target">The room line, at or below the room's top and no deeper than the deepest room line.</param>
    /// <param name="speed">The level speed in room pixels an update: negative rises.</param>
    /// <param name="delay">The updates the liquid waits once its level has somewhere to go (<see cref="LiquidMoveDelay"/>).</param>
    internal void MoveLiquidTo(int target, SignedEightEight speed, int delay)
    {
        TargetSurface = SurfaceLine(target);
        MoveLiquidAfter(delay, speed);
    }

    /// <summary>
    /// Cuts the liquid's wait to its next update, as the Speed Booster escape does once Samus has run far enough: a liquid
    /// already waiting starts moving on its next pass.
    /// </summary>
    internal void EndLiquidWaitNextUpdate() => LiquidMoveDelay = WrappingCountdown.Start(1);

    /// <summary>
    /// Halts the liquid where it stands, as the Speed Booster escape does when Samus has no Speed Booster: it loses its
    /// target and its speed, and its wait is cleared.
    /// </summary>
    internal void HaltLiquid()
    {
        TargetSurface = null;
        MoveLiquidAfter(0, SignedEightEight.Zero);
    }

    /// <summary>
    /// Reapplying a room effect restores motion, blending and three colors, but does not restart
    /// the room effect's screen waves, block animations, or the liquid's current motion phase as a room load would.
    /// </summary>
    internal void ApplyEntry(RoomEffectDefinition definition)
    {
        Ensure.NotNull(definition);
        LoadSurfaceAndTarget(definition);
        LiquidLevelSpeed = definition.LiquidLevelSpeed;
        LiquidMoveDelay = WrappingCountdown.Start(definition.LiquidMoveDelay);
        Liquid = definition.Liquid;
        Blend = definition.EffectBlend;
        VisorAnimation = definition.VisorAnimation;
        Tint = definition.Tint;
    }

    /// <summary>
    /// Lays the shade of the tint the last effect selected over the room effect's pictures and over the HUD's
    /// (#1512), each group pre-coloured in it on its own sheet (#1731).
    /// </summary>
    public void ShadeGameplay(GameplayShadingBuilder frame)
    {
        Ensure.NotNull(frame);
        if (Tint is not { } tint)
            return;
        frame.Shade(GameplayPictures.FxOverlay, RoomFxBlendShading.Of(tint));
        frame.Shade(GameplayPictures.Hud, RoomFxBlendShading.Of(tint));
    }

    /// <summary>
    /// Lets the room's water surface affect Samus, as the enable-water-physics room object does. It changes only that
    /// fact of the water, so this change lives on the room-FX owner that republishes the water to Samus; changing only
    /// Samus's projected copy would be undone by the next FX frame.
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
        PlaceLiquid(camera, waterDrift.Whole);

        waterOverlayWave = waterOverlayWave.Advanced(WaterWaveDefinitions.OverlayWavePhaseDuration,
            WaterWaveDefinitions.WaveDisplacementCount);

        if (Liquid is RoomWater { SurfaceDrift: true })
            waterDrift += WaterSurfaceDrift;

        // The Tourian entrance statue's FX starts only the water surface's motion. Its second motion moves
        // the statue vertically instead of installing the ordinary background X wave.
        if (Type != RoomFxType.Water || Liquid is not RoomWater { BackgroundWave: true })
            return;
        waterBackgroundWave = waterBackgroundWave.Advanced(WaterWaveDefinitions.BackgroundWavePhaseDuration,
            WaterWaveDefinitions.WaveDisplacementCount);
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
        // Lava and acid lie on the screen from their surface line as water does, the strip's surface row on that line.
        PlaceLiquid(camera, Offset16<OverlayPixels>.Zero);

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
    /// Runs the parallel three-phase motions for lava/acid
    /// and for water. A nonzero velocity first arms the wait phase,
    /// the liquid's move delay then expires, and only then does the signed 1/256-pixel velocity move the
    /// 1/65536-pixel base position toward the target. Door-specific Norfair entries rely on this
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
                bool movesTowardTarget = LiquidLevelSpeed > SignedEightEight.Zero
                    ? target > surface
                    : LiquidLevelSpeed.IsNegative && target < surface;
                if (movesTowardTarget)
                    liquidRisePhase = LiquidRisePhase.Waiting;
                return;
            }

            case LiquidRisePhase.Waiting:
                PublishRisingLiquidFeedback(randomNumber, earthquakeSoundDelay);
                LiquidMoveDelay = LiquidMoveDelay.Decrement().Remaining;
                if (LiquidMoveDelay.IsZero)
                    liquidRisePhase = LiquidRisePhase.Moving;
                return;

            case LiquidRisePhase.Moving:
                PublishRisingLiquidFeedback(randomNumber, earthquakeSoundDelay);
                if (AdvanceBaseYToTarget())
                {
                    LiquidLevelSpeed = default;
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
        // Water's phases only wait/move. Lava and acid's
        // parallel phases additionally request sound and a global earthquake.
        if (Type is not (RoomFxType.Lava or RoomFxType.Acid))
            return;
        HandleEarthquakeSoundEffect(randomNumber, earthquakeSoundDelay);
        SustainedEarthquake = RoomEarthquakeDefinitions.RisingLiquidShake;
    }

    /// <summary>
    /// Counts down to the next quake sound and plays it on the update the countdown runs past zero, then waits the next
    /// delay of the sound's rhythm; after the rhythm's last delay it starts again from the first. A room whose quakes are
    /// silent (<see cref="QuakeSoundsSilenced"/>) plays none and leaves the countdown and the rhythm where they stand.
    /// </summary>
    /// <param name="randomNumber">The current random number, sampled without advancing it; the number modulo 4 adds zero through three updates to the next earthquake-sound delay.</param>
    /// <param name="earthquakeSoundDelay">
    /// The frame the save file found at power-on adds to every next delay (<see cref="PowerOnEarthquakeSoundDelay"/>).
    /// Both callers' effects are the first their room starts after the door ends the previous room's effects, so they
    /// receive that frame; an effect started after another would add nothing.
    /// </param>
    private void HandleEarthquakeSoundEffect(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        if (QuakeSoundsSilenced)
            return;

        CountdownStep step = EarthquakeSoundTimer.Decrement();
        EarthquakeSoundTimer = step.Remaining;
        if (!step.IsNegative)
            return;

        IReadOnlyList<int> baseDelays = RoomEarthquakeDefinitions.RisingLiquidSoundBaseDelays;
        AudioRequests.QueueSound(EnemySound.Lavaquake, SoundRequestPriority.Normal);
        EarthquakeSoundTimer = WrappingCountdown.Start(baseDelays[EarthquakeSoundRhythmPosition])
            .AddFrames(RoomEarthquakeDefinitions.RisingLiquidSoundJitter.Of(randomNumber) + earthquakeSoundDelay.ExtraFrames);
        EarthquakeSoundRhythmPosition = (EarthquakeSoundRhythmPosition + 1) % baseDelays.Count;
    }

    /// <summary>The statue descent's steps call the same earthquake sound owner.</summary>
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
            .MovedBy(LiquidLevelSpeed.ToSixteenSixteenDelta());
        int line = RoomLineOf(moved);

        // Moving down, the surface reaches the target once it passes it, or once it passes the deepest room line and so
        // lies above the room's top, where a surface the room effect does not name also starts. Moving up, it reaches the
        // target once it is no deeper, which a surface risen past the room's top is not, as every target lies within the
        // room.
        bool reached = !LiquidLevelSpeed.IsNegative
            ? line > target || line < 0
            : line <= target;
        if (!reached)
        {
            BaseY = moved;
            return false;
        }

        BaseY = new FixedCoordinate(target, 0);
        return true;
    }

    /// <summary>
    /// Moves the tide. The scaled sine is a 1/256-pixel displacement, which becomes the 1/65536-pixel offset. The asymmetric phase
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
    /// Adds the tide's displacement to the base liquid surface, giving the surface's room line, negative once the tide
    /// carries it above the room's top; a liquid with no surface has none, and no retail room effect gives one a tide.
    /// </summary>
    /// <exception cref="InvalidOperationException">A liquid with no surface has a tide.</exception>
    private int? ComputeTidalYPosition()
    {
        if (BaseY is { } baseY)
            return RoomLineOf(baseY.MovedBy(TideOffset));
        return TideOffset.IsZero
            ? null
            : throw new InvalidOperationException("A room liquid with no surface has a tide.");
    }

    /// <summary>Creates one room's effect state, with no room effect selected, over the session's carried state.</summary>
    /// <param name="session">The Fireflea state, which outlives each room.</param>
    public RoomFxOverlayState(RoomFxSession session)
    {
        _session = Ensure.NotNull(session);
        Type = RoomFxType.None;
        TargetSurface = null;
        LiquidLevelSpeed = default;
        LiquidMoveDelay = WrappingCountdown.Start(0);
        Blend = null;
        Scroll = Position16<OverlayPixels>.Zero;
        drift = EightEightVector<OverlayPixels>.Zero;
        rainWind = EightEightVector<OverlayPixels>.Zero;
        previousCamera = Position16<RoomPixels>.Zero;
        BaseY = null;
        TidePhase = RotationAngle.Zero;
        TideOffset = SignedSixteenSixteen.Zero;
        Surface = null;
        Liquid = null;
        waterDrift = EightEightVector<OverlayPixels>.Zero;
        waterOverlayWave = default;
        waterBackgroundWave = default;
        waterSurfaceScreenY = null;
        liquidSurfaceLine = null;
        lavaAcidBackgroundWaveTimer = 0;
        lavaAcidBackgroundWavePhase = 0;
        liquidRisePhase = LiquidRisePhase.Dormant;
        MotionStart = LiquidMotionStart.AfterRoomLoad;
        LiquidMotionStopped = false;
        lavaSoundTimer = 0;
        SustainedEarthquake = null;
        EarthquakeSoundTimer = WrappingCountdown.Start(0);
        EarthquakeSoundRhythmPosition = 0;
        QuakeSoundsSilenced = false;
    }

    /// <summary>
    /// Ends the liquid's wave: no later update's liquid wave moves the liquid, queues its ambient sound or
    /// (for lava/acid) swaps the halves of the shared random number. Effects without a liquid wave are
    /// unaffected.
    /// </summary>
    public void StopLiquidMotion() => LiquidMotionStopped = true;

    /// <summary>Rain's texture fall each frame (<see cref="AtmosphereDriftDefinitions.RainFallSpeed"/>).</summary>
    private static readonly EightEightVector<OverlayPixels> RainFall =
        new(SignedEightEight.Zero, -AtmosphereDriftDefinitions.RainFallSpeed);

    /// <summary>The spores' texture rise each frame (<see cref="AtmosphereDriftDefinitions.SporesRiseSpeed"/>).</summary>
    private static readonly EightEightVector<OverlayPixels> SporesRise =
        new(SignedEightEight.Zero, -AtmosphereDriftDefinitions.SporesRiseSpeed);

    /// <summary>
    /// The fog's texture drift each frame: right by <see cref="AtmosphereDriftDefinitions.FogDriftSpeed"/> as it rises by
    /// <see cref="AtmosphereDriftDefinitions.FogRiseSpeed"/>.
    /// </summary>
    private static readonly EightEightVector<OverlayPixels> FogDrift =
        new(AtmosphereDriftDefinitions.FogDriftSpeed, -AtmosphereDriftDefinitions.FogRiseSpeed);

    /// <summary>Water's surface drift each frame while its water's <see cref="RoomWater.SurfaceDrift"/> is set.</summary>
    private static readonly EightEightVector<OverlayPixels> WaterSurfaceDrift =
        new(WaterWaveDefinitions.SurfaceDriftSpeed, SignedEightEight.Zero);

    /// <summary>The mutually exclusive phases of the liquid's rise.</summary>
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
