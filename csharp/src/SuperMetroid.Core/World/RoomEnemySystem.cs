using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Rooms.Scrolling;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SaveStation;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Escape;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Enemies.EffectActors;
using SuperMetroid.Core.Enemies.Grapple;

namespace SuperMetroid.Core.World;

/// <summary>
/// Room-enemy loader, scheduler, shared animation, and draw queues. The game's
/// gameplay definitions are its own; installed artwork supplies pixels and colors.
/// </summary>
/// <remarks>
/// The room keeps a fixed set of enemy places in <see cref="EnemyPlaceOrder"/>, each fixed for the room. Multi-part enemies
/// hold their parts as references they find once, by role, where the population lists them (<see cref="EnemyCompositeListing"/>);
/// the rules that reproduce a reach by placement itself (a struck part's follow-up, the gunship pad's height taken from its neighbour's place, the
/// Baby Metroid's victim) are compatibility services that read the places in order. A compacted list would lose those.
/// </remarks>
public sealed partial class RoomEnemySystem : IEnemyDisplaySources
{
    /// <summary>The enemies' draw phase: the live enemies the enemy pass queues to draw, and the frames each one shows.</summary>
    internal EnemyDrawList Drawing { get; }

    /// <summary>Advances each animating enemy's animation and shows the frames the bosses show on their bodies.</summary>
    internal EnemyAnimationPlayer AnimationPlayer { get; }

    /// <summary>Places the room's enemy population in the room's enemy places when the room loads, with its kill quota.</summary>
    internal EnemyPopulationLoader PopulationLoader { get; }

    /// <summary>Chooses the enemies the enemy pass runs this update: those near enough the screen, in their places' order.</summary>
    internal EnemyProcessingWindow ProcessingWindow { get; }

    /// <summary>Puts populations into places and runs their AI.</summary>
    internal EnemyLifecycle Lifecycle { get; }

    /// <summary>Caps how fast the camera follows Samus while an enemy carries her.</summary>
    internal SamusScrollCap ScrollCap { get; }

    /// <summary>The room's liquid, which the room's enemies raise or lower.</summary>
    internal EnemyRoomFx RoomFx { get; }

    /// <summary>What the room's enemies tell the rest of the game: the kills, the boss present, the room's darkness and whether a door transition holds them.</summary>
    internal EnemyRoomStatus RoomStatus { get; }

    /// <summary>
    /// The encounters of the room's bosses the room itself reads (#2194), which each attaches as its body sets up: the
    /// room's main, display and colours read them here, never through a boss's species.
    /// </summary>
    internal RoomBossEncounters BossEncounters { get; } = new();

    /// <summary>Samus's touches, shots, bombs and power bombs against the room's enemies, and the enemies' reactions to them.</summary>
    internal EnemyCollision Collision { get; }

    /// <summary>The room's enemy collision, which every species' shared participation reads once the system is built.</summary>
    private EnemyCollision CollisionOfRoom() => Collision;

    /// <summary>Enemy deaths and their drops: the death explosions, the scattered pickups and Samus collecting them.</summary>
    internal EnemyDeathsAndDrops DeathsAndDrops { get; }

    /// <summary>What the current enemy pass reads: the random number, the frame counter and whether time is frozen.</summary>
    internal EnemyFrameContext FrameContext { get; }

    /// <summary>How the room's enemies look: the room's appearance, the boss body layer and the shading each draw shows.</summary>
    internal EnemyPresentation Presentation { get; }

    /// <summary>The room's enemy places, in their order, and how far the population and later spawns have filled them.</summary>
    internal EnemyPlaces Places { get; }

    /// <summary>The room camera's distance mode, which the bosses request (#1874).</summary>
    internal RoomCameraDistance CameraDistance { get; } = new();

    /// <summary>The room camera's floor, which a boss's live fight holds (#2243).</summary>
    internal RoomCameraFloor CameraFloor { get; } = new();

    /// <summary>The grapple beam's grip on enemies: which enemy its tip latches on to, and what a grappled enemy does.</summary>
    internal GrappleBehavior GrappleBehavior { get; }

    /// <summary>The room's earthquake, which every enemy, room object and effect that shakes the room starts or sustains.</summary>
    internal RoomEarthquake Earthquake { get; }

    /// <summary>
    /// The room's species as their composition built them (#2571): the contracts the phases iterate and the capabilities
    /// through which the rest of the game reaches the enemies.
    /// </summary>
    internal RoomEnemySpecies SpeciesComposition { get; }

    /// <summary>The species registry that binds each installed occupant's entity.</summary>
    private EnemySpeciesRegistry Species => SpeciesComposition.Registry;

    /// <summary>The loaded room's enemy definitions, services and artwork, shared with the composed owners.</summary>
    internal EnemyRoomBinding Room { get; }

    /// <summary>
    /// Rebinds the room's active escape typewriters to the host's current escape-warning text, after the host binds new
    /// presentation content.
    /// </summary>
    /// <param name="value">The installed escape-warning text.</param>
    internal void RebindEscapeTypewriters(EscapeTypewriterPresentation value)
    {
        Ensure.NotNull(value);
        EscapeTypewriterState? active = BossEncounters.MotherBrain?.EscapeTypewriter;
        if (active is not null)
            active.BindMessage(value.Get(active.MessageId));
        EscapeTypewriterState? foreground = SpeciesComposition.Displays.ForegroundTypewriter;
        if (foreground is not null)
            foreground.BindMessage(value.Get(foreground.MessageId));
    }

    /// <summary>The room's effect actors.</summary>
    internal RoomEffectActors EffectActors { get; }

    /// <summary>
    /// Whether a Puyo striking a ceiling turns back: told of the room's draws, the charge flare's and the HUD's redraws,
    /// and read by the Puyos.
    /// </summary>
    internal PuyoCeilingTurnFromLastDraw PuyoCeilingTurn { get; }

    /// <summary>The room's enemy projectiles: their places, their spawns, their motion and their touches on Samus.</summary>
    internal EnemyProjectileEngine Projectiles { get; }

    /// <summary>The sound and music requests the room's enemies queue, and the music their room starts when it loads.</summary>
    internal EnemyAudioQueue Audio { get; }

    /// <summary>How enemies move against the room's terrain: steps over flat ground and slopes, the solid ahead, and the blocks they break.</summary>
    internal EnemyTerrain Terrain { get; }

    /// <summary>Post-AI collision snapshots for the room's active enemies.</summary>
    private readonly List<SolidEnemyCollisionBody> _interactiveCollisionBodies = new();

    /// <summary>The elevator journey the runtime reads and resets.</summary>
    internal IElevatorJourney Elevator => SpeciesComposition.Elevator;

    /// <summary>The Ceres encounter's status and runtime operations.</summary>
    internal ICeresEncounter Ceres => SpeciesComposition.Ceres;

    /// <summary>The state and hooks the room main behaviours and scrolling use.</summary>
    internal IEnemyRoomMainHooks RoomMainHooks => field ??= new EnemyRoomMainHooks(BossEncounters, CameraDistance, CameraFloor);

    /// <summary>The operations the room loader runs.</summary>
    internal IEnemyLoaderHooks LoaderHooks => SpeciesComposition.LoaderHooks;

    /// <summary>The enemy setup the attract demo's room setups perform.</summary>
    internal IEnemyAttractDemoHooks AttractDemoHooks => SpeciesComposition.AttractDemoHooks;

    /// <summary>The enemy-side effects room objects and room effects trigger.</summary>
    internal IEnemyObjectSpawns RoomObjectSpawns => SpeciesComposition.RoomObjectSpawns;

    /// <summary>The room-object work the Chozo statues and downward gates exchange with their room objects each frame.</summary>
    internal IEnemyObjectRequests RoomObjectRequests => SpeciesComposition.RoomObjectRequests;

    /// <summary>
    /// The statue room's entrance statue and unlock effects as its statue sequence drives them; null in every other room
    /// (#2486).
    /// </summary>
    internal RoomTourianStatues? TourianStatue => SpeciesComposition.TourianStatue;

    /// <summary>The ceiling tiles that fall in Ceres station during its escape.</summary>
    internal CeresFallingTiles CeresFallingTiles => SpeciesComposition.CeresFallingTiles;

    /// <summary>The electricity a save station shows while it saves.</summary>
    internal SaveStationElectricity SaveStationElectricity => SpeciesComposition.SaveStationElectricity;

    /// <summary>The encounters' work that steps after the enemy pass's room effects on an unfrozen update.</summary>
    internal IReadOnlyList<IStepAfterEnemyPass> AfterPass => SpeciesComposition.AfterPass;

    /// <summary>The encounters' scenes that step beside the enemy pass and show among the actors, in their order.</summary>
    internal IReadOnlyList<IEnemyPassScene> PassScenes => SpeciesComposition.PassScenes;

    /// <summary>
    /// Shows <paramref name="frame"/> of the escape's shutter flash on the destroyable shutters the room holds, and on
    /// nothing once both are destroyed (#1512, #2288). Mother Brain's exploded door's fragments are pre-coloured in their
    /// own colours and do not show it (#1544).
    /// </summary>
    internal void ShowEscapeShutterFlash(ColourEffectFrame frame) => SpeciesComposition.EscapeShutterFlash.ShowEscapeShutterFlash(frame);

    /// <summary>The live enemy state the display latch and drawing read.</summary>
    internal IEnemyDisplaySources DisplaySources => this;

    /// <inheritdoc/>
    IEnumerable<IDisplayLatch> IEnemyDisplaySources.DisplayLatches => SpeciesComposition.DisplayLatches;

    /// <inheritdoc/>
    MorphBallEyeBeamCone? IEnemyDisplaySources.LatchedBeamCone => SpeciesComposition.Displays.LatchedBeamCone;

    /// <inheritdoc/>
    IEnumerable<IStepWithRoomEffects> IEnemyDisplaySources.RoomEffectSteps => SpeciesComposition.RoomEffectSteps;

    /// <inheritdoc/>
    PhantoonDisplaySnapshot? IEnemyDisplaySources.WavingBody =>
        BossEncounters.Phantoon is { } phantoon ? PhantoonDisplaySnapshot.Capture(phantoon) : null;

    /// <inheritdoc/>
    bool IEnemyDisplaySources.ClipsRisingFloor => BossEncounters.MotherBrain is { RisingFloorClipActive: true };

    /// <inheritdoc/>
    MotherBrainRainbowBeamRenderSnapshot? IEnemyDisplaySources.RainbowBeam => BossEncounters.MotherBrain?.RainbowBeam.RenderSnapshot;

    /// <inheritdoc/>
    CrocomireMeltDistortion? IEnemyDisplaySources.BackgroundDistortion =>
        BossEncounters.Crocomire?.Death is { MeltingScrollActive: true } melting ? melting.MeltDistortion : null;

    /// <inheritdoc/>
    FloorView? IEnemyDisplaySources.FlownFloor => SpeciesComposition.Displays.FlownFloor;

    /// <inheritdoc/>
    ImmutableArray<PerspectiveFloorScenery> IEnemyDisplaySources.FlownFloorWalls => SpeciesComposition.Displays.FlownFloorWalls;

    /// <inheritdoc/>
    IBossBodyDisplay? IEnemyDisplaySources.BossBody => Presentation.BossBodyDisplay;

    /// <inheritdoc />
    IEnumerable<IGameplayShadingSource> IEnemyDisplaySources.ShadingSources
    {
        get
        {
            foreach (IGameplayShadingSource species in SpeciesComposition.Shadings)
                yield return species;
            if (BossEncounters.MotherBrain is { } motherBrain)
                yield return motherBrain.Colours;
            foreach (IGameplayShadingSource boss in Presentation.BossShadings.InOrder)
                yield return boss;
        }
    }

    /// <summary>
    /// Steps the species' work that runs before selection (the Morph Ball eye's beam), then decides which enemies to process.
    /// </summary>
    /// <remarks>
    /// Main gameplay builds the window first, before Samus's input and movement, so the grapple's endpoint scan sees this
    /// update's interactive enemies, including one a death explosion respawned at the end of the previous update. The
    /// Morph Ball eye's beam steps earlier still, at the start of the update, which shows as it starts, a beam set up on
    /// one update beginning on the next, and as it ends, the full beam seeing the eye shut down on the update after.
    /// </remarks>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    internal void PrepareEnemyProcessingList(Position16<RoomPixels> camera)
    {
        foreach (IStepBeforeEnemySelection step in SpeciesComposition.BeforeSelection)
            step.StepBeforeEnemySelection();
        ProcessingWindow.DetermineWhichEnemiesToProcess(camera);
    }

    /// <summary>Grapple endpoint resolution against the room's enemies.</summary>
    internal IGrappleTargets GrappleTargets => GrappleBehavior;

    /// <summary>The events the enemies have published since the current pass or the room's load began, in publication order.</summary>
    internal EnemyPassEvents PassEvents { get; } = new();

    /// <inheritdoc/>
    public EnemyFrameResult LastEnemyFrame => new(PassEvents.Published);

    /// <summary>
    /// Creates a room's enemy system: every enemy place, each with its stable ordinal, without loading a population or binding artwork, random-number, or gameplay services.
    /// </summary>
    /// <param name="dependencies">
    /// The enemy session, the room's compatibility services, enemy definitions, game-state services, area, boss artwork
    /// and breakable terrain, and Samus's passage through a door.
    /// </param>
    internal RoomEnemySystem(RoomEnemySystemDependencies dependencies)
    {
        Ensure.NotNull(dependencies);
        EnemySession session = dependencies.Session;
        RoomEnemyCompatibility compatibility = Ensure.NotNull(dependencies.Compatibility);
        BossArtworkBinding = Ensure.NotNull(dependencies.BossArtwork);
        Room = new EnemyRoomBinding(session, dependencies.Definitions, dependencies.Services, dependencies.Area);
        // Shared collaborators first: every species owner receives the ones it uses.
        ScrollCap = new SamusScrollCap();
        RoomFx = new EnemyRoomFx(dependencies.RoomFx);
        RoomStatus = new EnemyRoomStatus(session);
        FrameContext = new EnemyFrameContext(session);
        Presentation = new EnemyPresentation(session, Ensure.NotNull(dependencies.Looks), compatibility.FadeStepCount);
        BossPresentation = new BossPresentation(Presentation, BossArtworkBinding);
        var common = new EnemyCommonParticipation(CollisionOfRoom);
        Places = new EnemyPlaces();
        Audio = new EnemyAudioQueue(session.AudioRequests);
        ProcessingWindow = new EnemyProcessingWindow(FrameContext, Places);
        Lifecycle = new EnemyLifecycle(Room, Places, ProcessingWindow, common);
        PopulationLoader = new EnemyPopulationLoader(Room, RoomStatus, Places, Lifecycle);
        Earthquake = Ensure.NotNull(dependencies.Earthquake);
        PuyoCeilingTurn = compatibility.PuyoCeilingTurn;
        EffectActors = new RoomEffectActors(Presentation, compatibility.EffectActorPlaces, PuyoCeilingTurn);
        Projectiles = new EnemyProjectileEngine(Room, Presentation, EffectActors, Earthquake, Audio, new EnemyProjectilePlaces());
        Terrain = new EnemyTerrain(dependencies.BreakableTerrain);
        DeathsAndDrops = new EnemyDeathsAndDrops(RoomStatus, Projectiles, Room, Lifecycle, compatibility.PowerBombRadius, Audio);
        Collision = new EnemyCollision(new EnemyCollisionDependencies { Lifecycle = Lifecycle, Audio = Audio, DeathsAndDrops = DeathsAndDrops, Earthquake = Earthquake, Projectiles = Projectiles, EffectActors = EffectActors, Places = Places, PowerBombRadius = compatibility.PowerBombRadius });
        GrappleBehavior = new GrappleBehavior(Lifecycle, DeathsAndDrops);
        SpeciesComposition = new RoomEnemySpecies(new RoomEnemySpeciesDependencies
        {
            Session = session, Room = Room, Places = Places, Common = common, Lifecycle = Lifecycle, Audio = Audio, Projectiles = Projectiles,
            EffectActors = EffectActors, Collision = Collision, DeathsAndDrops = DeathsAndDrops, Terrain = Terrain,
            Earthquake = Earthquake, PassEvents = PassEvents, FrameContext = FrameContext, Presentation = Presentation,
            BossPresentation = BossPresentation, BossArtworkBinding = BossArtworkBinding, BossEncounters = BossEncounters,
            RoomStatus = RoomStatus, RoomFx = RoomFx, ScrollCap = ScrollCap, CameraDistance = CameraDistance,
            CameraFloor = CameraFloor, PuyoCeilingTurn = PuyoCeilingTurn, DoorPassage = dependencies.DoorPassage,
            Scenery = dependencies.Scenery, BullInheritance = compatibility.BullInheritance,
            BeetomInheritance = compatibility.BeetomInheritance, FadeStepCount = compatibility.FadeStepCount,
            UnauthoredBoostCadence = compatibility.UnauthoredBoostCadence, YardKickDistance = compatibility.YardKickDistance,
            FallingSparkFloorProbe = compatibility.FallingSparkFloorProbe,
        });
        AnimationPlayer = new EnemyAnimationPlayer(Presentation);
        Drawing = new EnemyDrawList(new EnemyDrawListDependencies { LateDraws = SpeciesComposition.LateDraws, FrameContext = FrameContext, AnimationPlayer = AnimationPlayer, Presentation = Presentation, Room = Room, PuyoCeilingTurn = PuyoCeilingTurn });
    }

    /// <summary>
    /// Starts each enemy projectile place from the whole position and velocity the room this one replaces left
    /// (<see cref="RoomHandOver"/>), before the population loads.
    /// </summary>
    internal void TakeOverFrom(RoomHandOver handOver) => Projectiles.TakeOverFrom(handOver);

    /// <summary>Leaves each enemy projectile place's whole position and velocity for the room that replaces this one (<see cref="RoomHandOver"/>).</summary>
    internal void HandOverTo(RoomHandOver handOver) => Projectiles.HandOverTo(handOver);

    /// <summary>Collision values read from the current places after their AI has run.</summary>
    public IReadOnlyList<SolidEnemyCollisionBody> InteractiveCollisionBodies =>
        _interactiveCollisionBodies;

    /// <summary>What the room's bosses draw with: its presentation surfaces and the installed boss artwork.</summary>
    internal BossPresentation BossPresentation { get; }

    /// <summary>The bosses' installed artwork, read through the session's binding.</summary>
    internal BossArtworkBinding BossArtworkBinding { get; }
    /// <summary>Whether <see cref="Load"/> has loaded this room's population; a room loads it once.</summary>
    private bool _populationLoaded;

    /// <summary>
    /// Loads the room's enemies: the artwork their population draws from, their population and each one's initialization.
    /// </summary>
    public void Load(IRoomEnemyLoad request)
    {
        Ensure.NotNull(request);
        if (_populationLoaded)
            throw new InvalidOperationException("A room's enemy system loads its population once; each room builds its own.");
        _populationLoaded = true;
        IRoomEnemyDefinitionSource definitions = Room.Definitions;
        // The species take the room's scenario and the already-positioned destination camera, which initialization AI
        // reads before the first enemy frame captures its own.
        SpeciesComposition.LoadPreparation.PrepareRoomLoad(request);
        FrameContext.BeginRoomLoad(request.DeferLoaderTimeCameraReads);
        Presentation.BindBackgrounds(request.Appearance);
        PassEvents.Begin();

        // A room that places no enemies keeps drawing its spawned objects from the standard artwork (#1599): an empty
        // population loads no enemy artwork, so the objects such a room shows appear in their standard variants.
        RoomEnemyPopulationDefinition population = request.Population;
        if (!population.Placements.IsEmpty)
            Presentation.DrawPopulation(population);
        PopulationLoader.LoadPopulation(
            Species,
            definitions,
            population,
            new EnemyPopulationSurroundings(request.Level, request.Samus, request.ControllerInput, request.Camera));
    }

    /// <summary>
    /// Runs one enemy pass over the processing list the frame already built: executes selected AI and animations,
    /// publishes live collision bodies and frame requests, and builds queues for the later draw phase.
    /// </summary>
    /// <param name="frame">What one enemy pass reads from the rest of the frame: the camera, Samus and her projectiles, controller input, clocks and the room systems enemies touch (#633).</param>
    internal EnemyFrameResult StepFrame(RoomEnemyFrame frame)
    {
        ScrollCap.BeginFrame(frame.SamusPreviousPositionCheckpoint);
        FrameContext.BeginFrame(frame.TimeIsFrozen, frame.FrameCounter);
        var aiFrame = new ClockedEnemyAiFrame(frame, new FrameCounters(frame.ShortCounter));
        PassEvents.Begin();
        foreach (IEnemySpecies species in SpeciesComposition.FrameParticipants)
            species.BeginEnemyFrame(frame);
        Drawing.Clear();
        ShakeWallJumpLaunchEnemy(frame.Samus);

        foreach (RoomEnemyOccupant occupant in ProcessingWindow.ActiveEnemies)
        {
            // The pass reaches the enemy that held the place as its list was fixed, even after another enemy's turn
            // emptied the place.
            RoomEnemyPlace place = occupant.Place;
            // The enemy pass counts tangible actors' invincibility
            // before checking time freeze. X-ray therefore stops their movement but
            // not this wait. Remember whether it ran at entry: the update that ends it
            // still skips collision, and a new AI-owned wait must not lose an update.
            bool invincibleAtEntry = place.Invincibility.IsRunning;
            if (!place.Contact.HasAny(EnemyContact.IgnoresSamus) && invincibleAtEntry)
                place.Invincibility.Count();
            // The enemy pass checks contact against the pre-movement actor, not the position
            // just moved into by AI. A rising support temporarily overlaps its rider until
            // Samus's movement consumes the carry; a late touch pass would retrigger that support.
            if (!frame.TimeIsFrozen && !invincibleAtEntry)
            {
                // Extended collision runs projectiles before bombs/touch and
                // before selecting hurt/main AI. In particular, a retained Plasma
                // hit must publish Phantoon's hurt clock before this frame ticks it.
                if (place.Entity is IEnemyContactHooks hooks)
                    hooks.ResolveBeforeOrdinaryHits(frame);
                Collision.ResolveOrdinaryProjectileHit(frame.SamusProjectiles, frame.SamusBombs, Room.Area,
                    frame.Samus, occupant, frame.Camera);
                // Samus's input phase has already moved her bombs. The enemy pass checks
                // them before this enemy's touch and AI, so an escape a bomb starts begins this update.
                Collision.ResolveOrdinaryBombHit(frame.SamusBombs, frame.SamusProjectiles, frame.Samus, occupant, frame.Camera);
                if (place.IsFree)
                    continue;
                Collision.ResolveOrdinarySamusContact(frame.Samus, frame.PseudoScrew, frame.ControllerInput, frame.Level, occupant, frame.Camera);
                if (place.Entity is IEnemyAfterSamusContact contact)
                    contact.ResolveAfterSamusContact(frame.Samus, frame.PseudoScrew);
                if (place.IsFree)
                    continue;
            }
            if (!frame.TimeIsFrozen)
            {
                bool ranActorAi = GrappleBehavior.RunCommonGrappleAi(occupant, aiFrame);
                // The hurt behaviour replaces main AI while the enemy is hurt: the species' own, or the inert one its
                // definition names. The grappled, hurt and frozen states are tried in that order, and the first one the
                // enemy is in selects its behaviour even when that behaviour does nothing, so the hurt state delays
                // frozen behaviour until the flash ends.
                if (!ranActorAi &&
                    place.IsHurt &&
                    place.Entity is IEnemyHurtReaction hurt &&
                    hurt.RunHurt(frame))
                {
                    ranActorAi = true;
                }
                // The enemy pass selects the AI from the first state the enemy is in alone. A frozen
                // clock without the frozen state (a Geruta flame sharing its parent's freeze)
                // still runs main AI.
                if (!ranActorAi && place.IsFrozen)
                {
                    // A Rinka's termination clears or replaces the enemy in this place.
                    // The pass's list of active enemies was fixed before AI, so skip the rest
                    // of this departed enemy's turn exactly as its own death does.
                    var frozen = place.Entity as IEnemyFrozenAi;
                    if (frozen?.PreemptFrozenTurn() == true)
                        continue;

                    // Test the clock before decrementing: reaching zero retains frozen
                    // AI for this update. Removing the ice instead thaws the enemy
                    // immediately, leaving the freeze clock at the remainder
                    // ThawedFreezeLeftover names rather than zero, which matters when an enemy
                    // is hurt and frozen at once.
                    place.HurtFlash.End();
                    bool thaw = place.Freeze.IsOver ||
                        (frame.Samus.EquippedBeams & SamusBeamFlags.Ice) == SamusBeamFlags.None;
                    if (!thaw)
                        place.Freeze.Count();
                    else
                    {
                        place.ClearFrozen();
                        bool stillGrappled = place.IsGrappled;
                        bool stillHurt = place.IsHurt;
                        ThawedFreezeLeftover.Hold hold = stillGrappled
                            ? stillHurt ? ThawedFreezeLeftover.Hold.GrappledAndHurt : ThawedFreezeLeftover.Hold.Grappled
                            : stillHurt ? ThawedFreezeLeftover.Hold.Hurt : ThawedFreezeLeftover.Hold.None;
                        place.Freeze.Restart(ThawedFreezeLeftover.Of(hold));
                    }
                    frozen?.RunFrozen();
                    // The frame clock advances after every enemy whose AI runs,
                    // a frozen enemy's included. Animation gating below remains separate.
                    ranActorAi = true;
                }
                else if (!ranActorAi)
                {
                    EnemyLifecycle.RunMainAi(place, aiFrame);
                    ranActorAi = true;
                }

                if (ranActorAi)
                {
                    place.FrameCounter++;
                    // The enemy pass skips the animation whenever the enemy is
                    // frozen, even if the hurt state, tried first, selected an
                    // enemy-specific hurt behaviour above: a frozen Metroid's body holds its frame while its frozen
                    // behaviour runs its two outer effect actors.
                    if (!place.IsFrozen &&
                        place.Processing.HasAny(EnemyProcessing.Animates))
                        EnemyAnimationPlayer.AdvanceAnimation(
                            place,
                            frame.Samus,
                            frame.Level,
                            frame.Camera,
                            frame.ControllerInput);
                }
            }

            // The enemy pass queues each live enemy to show only after its AI and animation work; a hidden
            // or removed enemy is not shown. The draw pass reaches an enemy its radius puts on
            // screen, and every enemy that collides by its frames' hitboxes wherever it is. Only a reached enemy's draw
            // clears the value a Puyo's later ceiling strike reads, consumes its shake tick and shows its boss body
            // frame, so which enemies are reached stays the game's; what is off screen is culled by the display.
            // The gunship uses the radius test.
            bool reached = place.CollidesByFrameHitboxes ||
                EnemyProcessingWindow.IsWithinProcessingWindow(place, frame.Camera);
            if (!place.Presence.HasAny(EnemyPresence.Hidden | EnemyPresence.Deleted))
                Drawing.Enqueue(place, reached);

            // The enemy pass counts the flash down after the enemy joins its draw queue, so a boss's draw colours show
            // the flash of the update just processed.
            if (!frame.TimeIsFrozen && place.HurtFlash.IsRunning)
            {
                place.HurtFlash.Count();
                // The enemy pass ends the hurt state once too few flash updates remain, even while another AI state stays
                // set, so a Metroid's own hurt behaviour ends.
                if (place.HurtFlash.UpdatesLeft < EnemyCollisionDefinitions.HurtStateMinimumFlashUpdates)
                    place.ClearHurt();
            }
        }

        // The processing window fixes only which enemies interact this frame. Samus's
        // collision reads their live position and size, so publish bodies after AI has
        // had its chance to move multi-part enemies.
        _interactiveCollisionBodies.Clear();
        foreach (RoomEnemyOccupant occupant in ProcessingWindow.InteractiveEnemies)
        {
            RoomEnemyPlace place = occupant.Place;
            _interactiveCollisionBodies.Add(new SolidEnemyCollisionBody(
                place,
                place.Position.Whole,
                new(place.Radius.X, place.Radius.Y),
                // A frozen enemy blocks Samus whatever its properties say.
                place.Freeze.IsRunning || place.Contact.HasAny(EnemyContact.SolidToSamus)));
        }
        FrameContext.CountEnemyPass();
        frame.Samus.Kinematics.ClearSolidEnemyCollisions();
        // The room-wide glows step after every enemy has updated, whether or not time is frozen.
        foreach (IRoomGlow glow in SpeciesComposition.Glows)
            glow.StepGlow();
        if (!frame.TimeIsFrozen)
            EffectActors.StepRoomEffectActors();
        return LastEnemyFrame;
    }

    /// <summary>
    /// Shakes the solid or frozen enemy Samus's latest wall jump launched from, as the enemy pass does before any enemy
    /// updates. While no enemy place is filled the request waits.
    /// </summary>
    internal void ShakeWallJumpLaunchEnemy(SamusState samus)
    {
        if (Places.EnemyCount == 0 || samus.Kinematics.WallJumpLaunch.Take() is not { } launchedFrom)
            return;
        launchedFrom.ShakeFromWallJumpLaunch();
    }

    /// <summary>Runs the room shaking: the earthquake's frame step, then the actor shake it selects.</summary>
    public RoomShakeFrameResult HandleRoomShaking(bool timeIsFrozen)
    {
        // This runs after drawing in the game, so the actors' shake (RoomEarthquakeDefinitions.EnemyShakeDuration) becomes visible
        // on the following frame rather than retroactively changing already-emitted sprites.
        if (Earthquake.Advance(timeIsFrozen))
        {
            foreach (RoomEnemyOccupant occupant in ProcessingWindow.ActiveEnemies)
                occupant.Place.Shake.Restart(RoomEarthquakeDefinitions.EnemyShakeDuration);
        }
        return Earthquake.LastRoomShake;
    }

    /// <summary>Advances enemy-projectile actors after Samus movement and before room objects.</summary>
    public void AdvanceEnemyProjectileAnimations(
        RoomLevelData level,
        SamusState samus,
        Position16<RoomPixels> camera,
        Compatibility.ShortFrameCount shortFrameCounter,
        SamusBombProjectileSystem samusBombs,
        int frameCounter)
    {
        Projectiles.RunPass(level, samus, camera, shortFrameCounter, samusBombs, frameCounter);
    }
}
