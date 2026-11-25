using System.Diagnostics.CodeAnalysis;
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
/// The overlay effects are lava, acid, water, spores, Landing Site rain, and Climb fog.
/// </remarks>
public sealed partial class RoomFxOverlayState : IGameplayShadingSource
{
    /// <summary>
    /// The loop of the effect's own block animation (lava, acid, rain or spores), selecting the frame its pictures show, or
    /// null for an effect that shows none (<see cref="BlockAnimationLoops.OnEffectOverlay"/>).
    /// </summary>
    private readonly SuperMetroid.Core.Animation.AnimationPlayer<BlockAnimation, ArtworkFrame, SuperMetroid.Core.Animation.NoAnimationCue>? effectOverlayAnimation;

    /// <summary>
    /// The room's Fireflea darkness, which its Fireflea effect starts as the room loads and which lasts as long as the
    /// room; null when the room's effect is not Fireflea darkness (#2053).
    /// </summary>
    public FirefleaFxState? Fireflea { get; internal set; }

    /// <summary>
    /// Whether the room's last applied effect animates Samus's visor (<see cref="RoomEffectDefinition.VisorAnimation"/>):
    /// selected when the room loads, steady for a room with no effect, and replaced when a fight applies another effect.
    /// </summary>
    public RoomVisorAnimation VisorAnimation { get; private set; }


    /// <summary>
    /// The tint the last effect loaded selected for the effect overlay's blend colours (#1512), or null while no effect
    /// has loaded one and the colours stay the tileset's.
    /// </summary>
    internal RoomEffectTint? Tint { get; set; }
    /// <summary>
    /// Rain, fog, and spores texture drift, its fractions kept between updates. Its whole pixels shift the effect overlay's
    /// scroll before the next velocity step.
    /// </summary>
    private TextureDrift drift;
    /// <summary>
    /// Rain's sideways texture drift each frame, the wind chosen from the random number when the room loads; it only
    /// blows across. The fall is <see cref="RainFall"/>.
    /// </summary>
    private readonly PixelVelocity<OverlayPixels> rainWind;
    /// <summary>Previous camera point, used to make rain follow camera movement.</summary>
    private Position16<RoomPixels> previousCamera;
    /// <summary>
    /// The surface a room load starts the liquid at on <paramref name="line"/>: half a pixel down it, centring the liquid on
    /// its whole line.
    /// </summary>
    private static FixedCoordinate LoadedSurfaceAt(int line) => FixedCoordinate.FromWhole(line).WithFraction(PixelFraction.Half);
    /// <summary>
    /// How far round its rise and fall the liquid's tide is: the surface sits at the negative cosine of this angle's whole
    /// step, and <see cref="AdvanceTidePhase"/> turns it on, continuing round past a full turn.
    /// </summary>
    internal RotationAngle TidePhase { get; set; }
    /// <summary>How far the tide displaces the liquid surface from its base line, to a fraction of a pixel.</summary>
    internal SignedSixteenSixteen TideOffset { get; set; }
    /// <summary>
    /// Water's effect overlay surface drift, its fraction kept between updates, whose whole pixels offset the surface
    /// texture. It only moves across.
    /// </summary>
    private TextureDrift waterDrift;
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
    /// <summary>Where lava's or acid's heat haze stands in its cycle.</summary>
    private WaveCycle lavaAcidHaze;
    /// <summary>
    /// Where the liquid's motion stands (<see cref="LiquidMotion"/>, #2560): a room's liquid begins moving on its second
    /// motion pass after the room loads, and each pass from then moves the lava, acid or water before the update draws its
    /// random number, until an enemy ends the motion directly, which stops its per-frame updates until the next room load.
    /// </summary>
    internal LiquidMotion Motion { get; set; } = LiquidMotion.AwaitingFirstPass;

    /// <summary>Lava's bubbling clock, which falls due each time lava bubbles.</summary>
    private IntervalClock lavaBubbling;
    /// <summary>Current liquid rise or fall state the room effect's liquid motion selects.</summary>
    private LiquidRisePhase liquidRisePhase;

    /// <summary>
    /// The update's sound requests the effect's sounds join as it makes them (#2057, #2544): the runtime's, given as the room
    /// loads its effect.
    /// </summary>
    private readonly GameplaySoundRequests sounds;
    /// <summary>
    /// Countdown to the next quake sound while rising lava or acid or the Tourian statues rumble: the sound plays on the
    /// update the countdown runs past zero.
    /// </summary>
    internal WrappingCountdown EarthquakeSoundTimer { get; set; }

    /// <summary>
    /// Where the quake sounds stand in their rhythm (<see cref="RoomEarthquakeDefinitions.RisingLiquidSoundBaseDelays"/>):
    /// the delay the next sound waits after it plays, returning to the first after the last.
    /// </summary>
    internal CyclingSequence<int> EarthquakeSoundRhythm { get; set; } =
        CyclingSequence<int>.StartOf(RoomEarthquakeDefinitions.RisingLiquidSoundBaseDelays);

    /// <summary>
    /// Whether the room's quakes are silent (<see cref="RoomDefinition.QuakesAreSilent"/>): rising lava or
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

    /// <summary>The liquid's level speed in room pixels per update while it moves toward its target: negative rises.</summary>
    internal SignedEightEight LiquidLevelSpeed { get; set; }

    /// <summary>
    /// The updates the liquid waits, once its level has somewhere to go, before it moves toward its target, as the room
    /// effect or a room object gives it. The wait counts down one update at a time and ends on reaching zero, so a wait of
    /// zero runs past zero and lasts a whole round of the countdown (<see cref="WrappingCountdown"/>).
    /// </summary>
    internal WrappingCountdown LiquidMoveDelay { get; set; }

    /// <summary>How the effect's overlay blends with the room (#1545), as the room effect describes it, or null when it shows no overlay.</summary>
    public RoomEffectBlend? Blend => overlayLook?.Blend;

    /// <summary>
    /// What the effect shows on the effect overlay and how it blends (#2544), as the room effect describes it: its liquid or
    /// its atmosphere, or null for an effect that shows nothing there.
    /// </summary>
    private RoomEffectOverlayLook? overlayLook;

    /// <summary>
    /// The effect overlay's live scroll: an atmosphere's across and down; a liquid's across only, as its surface line
    /// places it down (#1706).
    /// </summary>
    internal Position16<OverlayPixels> Scroll { get; set; }

    /// <summary>Whether the room's effect shows its pictures on the effect overlay over the gameplay area.</summary>
    internal bool IsRenderable => ShowsOnOverlay(Type);

    /// <summary>Whether a room of effect <paramref name="type"/> shows the effect's liquid or atmosphere on the effect overlay.</summary>
    public static bool ShowsOnOverlay(RoomFxType type) => RoomFxTypes.Of(type).Overlay != RoomFxOverlayMotion.None;

    /// <summary>
    /// The live liquid surface's room line after rising and tide processing, or null while the liquid has no surface
    /// (#1546). A tide can carry the surface above the room's top, to a negative line, where the room shows no liquid.
    /// </summary>
    public int? Surface => liveSurface?.Line;

    /// <summary>
    /// The live liquid surface while the liquid has one (<see cref="Surface"/>): published by the room's load, each liquid
    /// motion pass and each viewport priming, and moved in place while the liquid keeps a surface, so a reader that took it
    /// reads the surface as it moves.
    /// </summary>
    private RoomLiquidSurfaceLine? liveSurface;

    /// <summary>Finds the live liquid surface (<see cref="liveSurface"/>), which a liquid without a surface does not have.</summary>
    /// <param name="surface">The live surface, found when the liquid has one.</param>
    internal bool TryLiveSurface([NotNullWhen(true)] out RoomLiquidSurfaceLine? surface)
    {
        surface = liveSurface;
        return surface is not null;
    }

    /// <summary>
    /// Publishes the liquid's surface line, <paramref name="line"/>, or its having none: a published surface moves its line
    /// in place, a liquid's first surface is created, and a liquid without one drops it.
    /// </summary>
    private void PublishSurface(int? line)
    {
        if (line is not int surface)
        {
            liveSurface = null;
            return;
        }
        if (liveSurface is { } live)
            live.Line = surface;
        else
            liveSurface = new RoomLiquidSurfaceLine(surface);
    }

    /// <summary>
    /// The liquid the room effect fills the room with, as room objects have since changed it, or null for an effect with no
    /// liquid (#1695).
    /// </summary>
    public RoomLiquid? Liquid { get; internal set; }

    /// <summary>
    /// The earthquake the current effect pass sustains (<see cref="RoomEarthquake.Sustain"/>), if any: rising lava's or
    /// acid's, every update it waits or moves.
    /// </summary>
    public EarthquakeShake? SustainedEarthquake { get; private set; }

    /// <summary>Builds one room's effect-overlay FX from the effect the current door selects.</summary>
    /// <param name="fx">The room state's effect selection, from which <paramref name="door"/> picks the room's effect.</param>
    /// <param name="door">The entry door, which selects the room effect.</param>
    /// <param name="randomNumber">The session random number rain reads.</param>
    /// <param name="sounds">The update's sound requests of the runtime running the effect, which its sounds join.</param>
    /// <param name="room">The room, for rooms that suppress the earthquake sound.</param>
    internal static RoomFxOverlayState ForRoom(
        RoomFxSelection? fx,
        DoorDefinition? door,
        int randomNumber,
        GameplaySoundRequests sounds,
        RoomDefinition? room = null) =>
        ForSource(new RoomFxLoadSource { Fx = fx, Door = door, RandomNumber = randomNumber, Room = room, SuppliedDefinition = null }, sounds);

    /// <summary>Builds one room's effect-overlay FX from a load source, which may supply its effect directly.</summary>
    /// <param name="source">The room effect's source.</param>
    /// <param name="sounds">The update's sound requests of the runtime running the effect, which its sounds join.</param>
    internal static RoomFxOverlayState ForSource(RoomFxLoadSource source, GameplaySoundRequests sounds) => new(source, sounds);

    /// <summary>
    /// Builds a room's effect state with the effect its door selects, initializing the liquid and tint state shared by room
    /// effects and room objects. From then until Samus leaves the room, the effect overlay shows the effect's liquid or
    /// atmosphere, or nothing for an effect without one.
    /// </summary>
    /// <param name="source">The room effect's source.</param>
    /// <param name="sounds">The update's sound requests of the runtime running the effect, which its sounds join.</param>
    private RoomFxOverlayState(RoomFxLoadSource source, GameplaySoundRequests sounds)
    {
        this.sounds = Ensure.NotNull(sounds);
        QuakeSoundsSilenced = source.Room is { QuakesAreSilent: true };
        if ((source.SuppliedDefinition ?? source.Fx?.Select(source.Door)) is not { } definition)
            return;

        LoadSurfaceAndTarget(definition);
        LiquidLevelSpeed = definition.LiquidLevelSpeed;
        LiquidMoveDelay = WrappingCountdown.Start(definition.LiquidMoveDelay);
        Liquid = definition.Liquid;
        PublishSurface(BaseLine);

        Type = definition.Type;
        overlayLook = RoomEffectOverlayLook.Of(definition.Type, definition.EffectBlend);
        VisorAnimation = definition.VisorAnimation;
        effectOverlayAnimation = BlockAnimationLoops.OnEffectOverlay(Type) is { } overlayAnimation ? BlockAnimationLoops.Start(overlayAnimation) : null;
        Tint = definition.Tint;

        if (RoomFxTypes.Of(Type).DarkensWithFirefleas)
            Fireflea = new FirefleaFxState();
        if (!IsRenderable)
            return;

        if (RoomFxTypes.UsesWater(Type))
        {
            // Both water waves, the surface's and the background's, start at their first phase.
            waterOverlayWave = WaveCycle.StartingAt(0);
            waterBackgroundWave = WaveCycle.StartingAt(0);
        }
        else if (RoomFxTypes.IsLavaOrAcid(Type))
        {
            // Lava and acid start their heat haze where the room's load leaves it: the game's first haze step set its
            // first phase and started its wait, so the first phase moves on sooner than the later ones.
            lavaAcidHaze = LavaAcidWaveDefinitions.HazeStart;
        }
        else if (RoomFxTypes.Of(Type).Overlay == RoomFxOverlayMotion.Rain)
        {
            rainWind = new(AtmosphereDrift.RainHorizontalVelocity(AtmosphereDriftDefinitions.RainWindDraw.Choose(source.RandomNumber)),
                SignedEightEight.Zero);
        }
    }

    /// <summary>
    /// True for the liquids whose surface motion step moves the surface: lava and
    /// acid and water. That motion belongs to the liquid's motion pass.
    /// </summary>
    public bool HasMovingLiquid => RoomFxTypes.ShowsLiquid(Type);

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
        BaseY?.WithWhole(SurfaceLine(line)) ?? LoadedSurfaceAt(SurfaceLine(line));

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
    /// the update draws its random number. The liquid's first motion pass after the room loads only begins its
    /// motion (<see cref="LiquidMotion"/>); lava/acid's motion then swaps the shared random state on
    /// subsequent unfrozen passes, even off screen. Rising/tidal motion also runs during
    /// door fades and pause entry. Visual updates remain in <see cref="Step"/>, which places the surface this pass left.
    /// </summary>
    public void AdvanceLiquidMotion(SharedGameState system, bool timeIsFrozen)
    {
        Ensure.NotNull(system);
        if (!HasMovingLiquid || Motion.IsStopped)
            return;
        bool lavaOrAcid = RoomFxTypes.IsLavaOrAcid(Type);
        SustainedEarthquake = null;
        if (!Motion.IsMoving)
        {
            Motion = LiquidMotion.Moving;
            // The pass that begins lava's or acid's motion also starts its bubbling clock.
            if (lavaOrAcid)
                lavaBubbling = IntervalClock.Starting(LavaAcidWaveDefinitions.AmbientSoundPeriod);
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
        system.StirRandomForLiquidWave();
    }

    /// <summary>
    /// Lava's bubbling: while a lava surface lies within the room, its clock runs, and each time it falls due lava queues
    /// the bubbling sound <paramref name="random"/> chooses. Acid shares the update's step but not this sound.
    /// </summary>
    private void QueueLavaAmbientSound(int random)
    {
        if (!RoomFxTypes.Of(Type).BubblesAmbiently || Surface is not >= 0)
            return;
        lavaBubbling = lavaBubbling.Run(out bool bubbles);
        if (bubbles)
            sounds.QueueSound(LavaAcidWaveDefinitions.AmbientSound.Choose(random), LavaAcidWaveDefinitions.AmbientSoundPriority);
    }

    /// <summary>
    /// Passes the effect's own block animation (lava, acid, rain or spores) once, selecting the frame its pictures show: the
    /// cartridge's animated tiles object of the effect, which its animated tiles handler steps with the room's others,
    /// during a door's transition as in play (#3037).
    /// </summary>
    internal void StepAnimation(ArtworkFrameSelections frames)
    {
        Ensure.NotNull(frames);
        if (effectOverlayAnimation is not null)
            BlockAnimationLoops.Pass(effectOverlayAnimation, frames);
    }

    /// <summary>
    /// Runs the active effect's step for one update and its overlay's block animation. A liquid's surface is where
    /// its motion pass (<see cref="AdvanceLiquidMotion(SharedGameState, bool)"/>), which runs earlier in the update, left it.
    /// </summary>
    public void Step(ArtworkFrameSelections frames, RoomFxOverlayFrame frame)
    {
        Ensure.NotNull(frames);
        if (Fireflea is { } fireflea)
        {
            fireflea.Step(frame.TimeIsFrozen, frame.FirefleaDarkness);
            return;
        }
        if (!IsRenderable || frame.TimeIsFrozen)
            return;

        // The shared block animations run independently of the liquid motion's
        // per-frame step. In particular, lava and acid select the animation frame their pictures
        // show here, before the surface step that follows.
        StepAnimation(frames);
        StepOverlayDisplay(frame.Camera);
    }

    /// <summary>
    /// Runs the effect's overlay display for one update of a door's transition (#3047): where the cartridge's HDMA objects
    /// run, through the room left's waits and fade out until the door's header deletes them, and through the room entered's
    /// updates from its animated tiles' step on. The overlay of a Fireflea room, or of an effect that shows none, has
    /// nothing placed there; while time is frozen nothing moves.
    /// </summary>
    /// <param name="camera">The camera the overlay is placed against.</param>
    /// <param name="timeIsFrozen">Whether time is frozen.</param>
    internal void StepDisplayDuringDoor(Position16<RoomPixels> camera, bool timeIsFrozen)
    {
        if (Fireflea is not null || !IsRenderable || timeIsFrozen)
            return;
        StepOverlayDisplay(camera);
    }

    /// <summary>
    /// The cartridge's HDMA objects of the effect, for one update: the liquid placed on the screen from its surface with its
    /// waves or haze, or the rain's, spores' or fog's scroll moved on.
    /// </summary>
    private void StepOverlayDisplay(Position16<RoomPixels> camera)
    {
        if (Motion.IsStopped && HasMovingLiquid)
            return;

        if (RoomFxTypes.IsLavaOrAcid(Type))
        {
            StepLavaAcid(camera);
            return;
        }

        if (RoomFxTypes.UsesWater(Type))
        {
            StepWater(camera);
            return;
        }

        if (RoomFxTypes.Of(Type).Overlay == RoomFxOverlayMotion.Rain)
        {
            Offset16<OverlayPixels> cameraDelta = previousCamera.AsOverlayScroll() - camera.AsOverlayScroll();
            Scroll = Position16<OverlayPixels>.Zero + cameraDelta + drift.Whole;
            drift += rainWind;
            drift += RainFall;
            previousCamera = camera;
            return;
        }

        if (RoomFxTypes.Of(Type).Overlay == RoomFxOverlayMotion.Spores)
        {
            // Spores sample each signed running sum before adding its velocity.
            // Unlike rain, spores are anchored to the current camera, not its delta.
            Scroll = camera.AsOverlayScroll() + drift.Whole;
            drift += SporesRise;
            return;
        }

        // Fog anchors to the foreground; its texture drifts right as it rises.
        Scroll = camera.AsOverlayScroll() + drift.Whole;
        drift += FogDrift;
    }

    /// <summary>
    /// Seeds the liquid surface's line where its base line and tide place it, and its camera-dependent screen line,
    /// during a room transition without advancing a wave clock. Destination rendering can therefore show the correct
    /// surface on its first frame boundary instead of waiting for the first gameplay update. A room's liquid enters with
    /// no tide offset, so its surface enters on its base line.
    /// </summary>
    public void PrimeViewport(Position16<RoomPixels> camera)
    {
        if (!RoomFxTypes.ShowsLiquid(Type))
            return;
        PublishSurface(ComputeTidalYPosition());
        PlaceLiquid(camera, Offset16<OverlayPixels>.Zero);
    }

    /// <summary>
    /// The surface's screen line below <paramref name="camera"/>'s top, negative for a surface above the screen, or null
    /// without a surface: the surface's room line less the camera's, a camera above the room's top lying on a negative line.
    /// </summary>
    private int? SurfaceScreenLine(Position16<RoomPixels> camera) =>
        Surface is int line ? line - (camera - Position16<RoomPixels>.Zero).Y : null;

    /// <summary>
    /// Places the liquid on the screen below <paramref name="camera"/> (#1706): its surface's screen line, which the effect
    /// overlay shows the liquid from while the surface lies within the room, and the overlay's horizontal scroll, the
    /// camera's moved by <paramref name="surfaceDrift"/>. A liquid scrolls only across: its surface line places it down.
    /// </summary>
    private void PlaceLiquid(Position16<RoomPixels> camera, Offset16<OverlayPixels> surfaceDrift)
    {
        waterSurfaceScreenY = SurfaceScreenLine(camera);
        // A surface above the room's top is a negative room line, which shows no surface.
        liquidSurfaceLine = Surface is >= 0 ? waterSurfaceScreenY : null;
        Scroll = (camera.AsOverlayScroll() + surfaceDrift).WithY(0);
    }

    /// <summary>Captures values visible alongside the current frame boundary's sprite frame.</summary>
    public RoomFxOverlayRenderSnapshot? CaptureForDisplay() => overlayLook is { } look
        ? new RoomFxOverlayRenderSnapshot(
            Type,
            look,
            Scroll,
            Surface,
            Liquid,
            waterOverlayWave.Phase,
            RunsWaterBackgroundWave ? waterBackgroundWave.Phase : null,
            waterSurfaceScreenY)
        {
            LavaAcidBackgroundWavePhase = lavaAcidHaze.Phase,
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
    internal void EndLiquidWaitNextUpdate() => LiquidMoveDelay = WrappingCountdown.OneFrame;

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
    /// Reapplying a room effect restores its motion, blending, visor animation and tint, but does not restart
    /// the room effect's screen waves, block animations, or the liquid's current motion phase as a room load would. The
    /// room keeps the kind of effect it loaded, which the reapplied effect's blend now blends.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// The reapplied effect describes no blend for a room whose effect shows on the overlay, or one for a room whose effect
    /// shows nothing there.
    /// </exception>
    internal void ApplyEntry(RoomEffectDefinition definition)
    {
        Ensure.NotNull(definition);
        LoadSurfaceAndTarget(definition);
        LiquidLevelSpeed = definition.LiquidLevelSpeed;
        LiquidMoveDelay = WrappingCountdown.Start(definition.LiquidMoveDelay);
        Liquid = definition.Liquid;
        overlayLook = RoomEffectOverlayLook.Of(Type, definition.EffectBlend);
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
        frame.Shade(GameplayPictures.FxOverlay, RoomEffectTintShading.Of(tint));
        frame.Shade(GameplayPictures.Hud, RoomEffectTintShading.Of(tint));
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
    /// Places the water on the screen where its motion pass (<see cref="AdvanceLiquidMotion(SharedGameState, bool)"/>) left
    /// its surface and runs both circular wave clocks.
    /// </summary>
    private void StepWater(Position16<RoomPixels> camera)
    {
        PlaceLiquid(camera, waterDrift.Whole);

        waterOverlayWave = waterOverlayWave.Advanced(WaterWaveDefinitions.OverlayWavePhaseDuration,
            WaterWaveDefinitions.SurfaceWaveform.Length);

        if (Liquid is RoomWater { SurfaceDrift: true })
            waterDrift += WaterSurfaceDrift;

        if (!RunsWaterBackgroundWave)
            return;
        waterBackgroundWave = waterBackgroundWave.Advanced(WaterWaveDefinitions.BackgroundWavePhaseDuration,
            WaterWaveDefinitions.BackgroundWaveform.Length);
    }

    /// <summary>
    /// Whether the effect runs the water's background wave: a water effect whose water has one. The Tourian entrance
    /// statue's FX starts only the water surface's motion; its second motion moves the statue vertically instead of
    /// installing the ordinary background X wave.
    /// </summary>
    private bool RunsWaterBackgroundWave => RoomFxTypes.Of(Type).WavesBackground && Liquid is RoomWater { BackgroundWave: true };

    /// <summary>Advances rise and tide state before publishing the current liquid surface.</summary>
    private void AdvanceLiquidMotion(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        StepLiquidRise(randomNumber, earthquakeSoundDelay);
        StepLiquidTide();
        PublishSurface(ComputeTidalYPosition());
    }

    /// <summary>
    /// Runs the visible outputs of the lava/acid effect and its heat haze. The liquid surface its motion pass
    /// (<see cref="AdvanceLiquidMotion(SharedGameState, bool)"/>) left and the effect overlay follow the room camera,
    /// while the heat haze moves on through its cycle at its own cadence.
    /// </summary>
    private void StepLavaAcid(Position16<RoomPixels> camera)
    {
        // Lava and acid lie on the screen from their surface line as water does, the strip's surface row on that line.
        PlaceLiquid(camera, Offset16<OverlayPixels>.Zero);

        if (Liquid is not RoomLavaOrAcid { Haze: HeatHazeDirection.Horizontal or HeatHazeDirection.Vertical })
            return;
        lavaAcidHaze = lavaAcidHaze.Advanced(HazePhaseDuration, LavaAcidWaveDefinitions.HazePhaseCount);
    }

    /// <summary>
    /// How long each phase of lava's or acid's heat haze lasts: the vertical haze's for a vertical haze, else the
    /// horizontal one's, which also starts the clock of a liquid with no haze.
    /// </summary>
    private int HazePhaseDuration => Liquid is RoomLavaOrAcid { Haze: HeatHazeDirection.Vertical }
        ? LavaAcidWaveDefinitions.VerticalWavePhaseDuration
        : LavaAcidWaveDefinitions.HorizontalWavePhaseDuration;

    /// <summary>What <see cref="StepLiquidRise"/> does for each <see cref="LiquidRisePhase"/> member, each member's own arm.</summary>
    private IdentityTable<LiquidRisePhase, Action<int, PowerOnEarthquakeSoundDelay>> StepLiquidRiseForLiquidRisePhase => field ??= new(new Dictionary<LiquidRisePhase, Action<int, PowerOnEarthquakeSoundDelay>>
    {
        [LiquidRisePhase.Dormant] = (randomNumber, earthquakeSoundDelay) =>
        {
            // A missing surface or target lies below every line (AbsentLiquidSurfaceDepth).
            int surface = AbsentLiquidSurfaceDepth.Of(BaseLine);
            int target = AbsentLiquidSurfaceDepth.Of(TargetSurface);
            bool movesTowardTarget = LiquidLevelSpeed > SignedEightEight.Zero
                ? target > surface
                : LiquidLevelSpeed.IsNegative && target < surface;
            if (movesTowardTarget)
                liquidRisePhase = LiquidRisePhase.Waiting;
        },
        [LiquidRisePhase.Waiting] = (randomNumber, earthquakeSoundDelay) =>
        {
            PublishRisingLiquidFeedback(randomNumber, earthquakeSoundDelay);
            LiquidMoveDelay = LiquidMoveDelay.Decrement().Remaining;
            if (LiquidMoveDelay.IsZero)
                liquidRisePhase = LiquidRisePhase.Moving;
        },
        [LiquidRisePhase.Moving] = (randomNumber, earthquakeSoundDelay) =>
        {
            PublishRisingLiquidFeedback(randomNumber, earthquakeSoundDelay);
            if (AdvanceBaseYToTarget())
            {
                LiquidLevelSpeed = default;
                liquidRisePhase = LiquidRisePhase.Dormant;
            }
        },
    });

    /// <summary>
    /// Runs the parallel three-phase motions for lava/acid
    /// and for water. A nonzero velocity first arms the wait phase,
    /// the liquid's move delay then expires, and only then does the liquid's speed move its
    /// surface toward the target. Door-specific Norfair entries rely on this
    /// sequence to raise lava into the visible viewport.
    /// </summary>
    private void StepLiquidRise(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        StepLiquidRiseForLiquidRisePhase[liquidRisePhase](randomNumber, earthquakeSoundDelay);
    }

    /// <summary>
    /// Plays the earthquake sound and shakes the screen
    /// while lava or acid waits and moves. Sound cadence and screen shake are
    /// separate results of the room's liquid effect; the display derives neither from the other.
    /// </summary>
    private void PublishRisingLiquidFeedback(int randomNumber, PowerOnEarthquakeSoundDelay earthquakeSoundDelay)
    {
        // Water's phases only wait/move. Lava and acid's
        // parallel phases additionally request sound and a global earthquake.
        if (!RoomFxTypes.IsLavaOrAcid(Type))
            return;
        HandleEarthquakeSoundEffect(randomNumber, earthquakeSoundDelay);
        SustainedEarthquake = RoomEarthquakeDefinitions.RisingLiquidShake;
    }

    /// <summary>
    /// Counts down to the next quake sound and plays it on the update the countdown runs past zero, then waits the next
    /// delay of the sound's rhythm; after the rhythm's last delay it starts again from the first. A room whose quakes are
    /// silent (<see cref="QuakeSoundsSilenced"/>) plays none and leaves the countdown and the rhythm where they stand.
    /// </summary>
    /// <param name="randomNumber">The current random number, sampled without advancing it; its four-way choice adds zero through three updates to the next earthquake-sound delay.</param>
    /// <param name="earthquakeSoundDelay">
    /// The frame the save file found at power-on adds to every next delay (<see cref="PowerOnEarthquakeSoundDelay"/>).
    /// Both effects are the first their room starts after the door ends the previous room's effects, so they
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

        sounds.QueueSound(EnemySound.Lavaquake, SoundRequestPriority.Normal);
        EarthquakeSoundTimer = WrappingCountdown.Start(EarthquakeSoundRhythm.Current)
            .AddFrames(RoomEarthquakeDefinitions.RisingLiquidSoundJitter.Of(randomNumber) + earthquakeSoundDelay.ExtraFrames);
        EarthquakeSoundRhythm = EarthquakeSoundRhythm.Next;
    }

    /// <summary>Plays the statue descent's earthquake sound, as a room's earthquake plays it.</summary>
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

        FixedCoordinate moved = (BaseY ?? LoadedSurfaceAt(AbsentLiquidSurfaceDepth.Of(null)))
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

        BaseY = FixedCoordinate.FromWhole(target);
        return true;
    }

    /// <summary>
    /// Moves the tide: the surface's offset is the tide's wave at its crest's height, and the phase advances by a different
    /// step in each half-wave, so the tide spends longer in one half than in the other.
    /// </summary>
    internal void StepLiquidTide()
    {
        LiquidTide tide = Liquid?.Tide ?? LiquidTide.None;
        if (tide == LiquidTide.None)
        {
            TideOffset = SignedSixteenSixteen.Zero;
            return;
        }
        // Each moving tide's reach and pace are its motion (#2557); a tide the catalog does not list fails loudly.
        LiquidTideMotion motion = Ensure.Listed(LiquidTideDefinitions.Motions, tide);

        // The tide follows the negative cosine of its phase, not the sine: the sine a quarter turn behind it. A crest's
        // height is positive, so the offset lies above the base line exactly while the wave is negative.
        TideOffset = Trigonometry.SineDisplacement((TidePhase - RotationAngle.QuarterTurn).ToTrigonometryAngle(), motion.PeakPixels);
        AdvanceTidePhase(TideOffset.IsNegative ? motion.AbovePhaseDelta : motion.AtOrBelowPhaseDelta);
    }

    /// <summary>Turns the tide phase on by <paramref name="delta"/>, continuing round past a full turn.</summary>
    private void AdvanceTidePhase(RotationAngle delta) => TidePhase += delta;

    /// <summary>
    /// Adds the tide's displacement to the base liquid surface, giving the surface's room line, negative once the tide
    /// carries it above the room's top; a liquid with no surface has none. Only a liquid with a surface has a tide, which
    /// each room effect's definition checks once (<see cref="RoomEffectDefinition.LiquidStartLevel"/>), so a liquid with no
    /// surface has no displacement to add.
    /// </summary>
    private int? ComputeTidalYPosition() => BaseY is { } baseY ? RoomLineOf(baseY.MovedBy(TideOffset)) : null;

    /// <summary>
    /// The line the room's lava or acid lies at for what rises from it: the live surface of a room whose liquid is lava or
    /// acid, and otherwise, as for lava or acid with no surface, the line a missing surface lies at
    /// (<see cref="AbsentLiquidSurfaceDepth"/>).
    /// </summary>
    internal int LavaAcidLine => AbsentLiquidSurfaceDepth.Of(Liquid is RoomLavaOrAcid ? Surface : null);

    /// <summary>
    /// Ends the liquid's wave: no later update's liquid wave moves the liquid, queues its ambient sound or
    /// (for lava/acid) swaps the halves of the shared random number. Effects without a liquid wave are
    /// unaffected.
    /// </summary>
    public void StopLiquidMotion() => Motion = LiquidMotion.Stopped;

    /// <summary>Rain's texture fall each frame (<see cref="AtmosphereDriftDefinitions.RainFallSpeed"/>).</summary>
    private static readonly PixelVelocity<OverlayPixels> RainFall =
        new(SignedEightEight.Zero, -AtmosphereDriftDefinitions.RainFallSpeed);

    /// <summary>The spores' texture rise each frame (<see cref="AtmosphereDriftDefinitions.SporesRiseSpeed"/>).</summary>
    private static readonly PixelVelocity<OverlayPixels> SporesRise =
        new(SignedEightEight.Zero, -AtmosphereDriftDefinitions.SporesRiseSpeed);

    /// <summary>
    /// The fog's texture drift each frame: right by <see cref="AtmosphereDriftDefinitions.FogDriftSpeed"/> as it rises by
    /// <see cref="AtmosphereDriftDefinitions.FogRiseSpeed"/>.
    /// </summary>
    private static readonly PixelVelocity<OverlayPixels> FogDrift =
        new(AtmosphereDriftDefinitions.FogDriftSpeed, -AtmosphereDriftDefinitions.FogRiseSpeed);

    /// <summary>Water's surface drift each frame while its water's <see cref="RoomWater.SurfaceDrift"/> is set.</summary>
    private static readonly PixelVelocity<OverlayPixels> WaterSurfaceDrift =
        new(WaterWaveDefinitions.SurfaceDriftSpeed, SignedEightEight.Zero);

    /// <summary>The mutually exclusive phases of the liquid's rise.</summary>
    private enum LiquidRisePhase
    {
        /// <summary>Waits until signed velocity points from the current base toward the target.</summary>
        Dormant,
        /// <summary>Counts down the liquid's move delay, while rising lava or acid rumbles and shakes the screen.</summary>
        Waiting,
        /// <summary>Advances the liquid base each update until it reaches the target and clears velocity.</summary>
        Moving,
    }
}
