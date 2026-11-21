using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The mama turtle species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MamaTurtleBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= new Dictionary<EnemyDefinitionId, EnemyEntityCreation>
    {
        [EnemyDefinitionId.BabyTurtle] = (occupant, place, common) => EnemyOccupant.Of(new BabyTurtleEntity(this, occupant, place, common)),
        [EnemyDefinitionId.MamaTurtle] = (occupant, place, common) => EnemyOccupant.Of(new MamaTurtleEntity(this, occupant, place, common)),
    };

    /// <summary>Moves parent and children against room blocks during hover, fall, and crawling.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Records the wall-impact shake from Mama Turtle's hover collision.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>The room's enemy places, where Mama finds her four babies when she is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The four babies each Mama found when she was placed, which her room lists right after her.</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace[]> _babiesOfMama = new();
    /// <summary>Each baby's Mama, linked when she found it as she was placed (#1816).</summary>
    private readonly EnemyPartLinks<RoomEnemyPlace> _mamaOfBaby = new();
    /// <summary>Applies touch damage for Mama's expanded body or exposed shell.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Mama Turtle and her babies to room terrain, collision, places, and earthquake output.</summary>
    internal MamaTurtleBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, EnemyPlaces places, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _earthquake = earthquake;
        _places = places;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Mama Turtle's animations; rising to hover continues at its spin.</summary>
    private MamaTurtleSegment? ApplyMamaTurtleCue(RoomEnemyPlace place, MamaTurtleCue cue) =>
        cue.Match(ApplyMamaTurtleCueCases, new ApplyMamaTurtleCueContext(place, this));

    /// <summary>What <c>ApplyMamaTurtleCue</c> does on each <see cref="MamaTurtleCue"/>, built once (#2553).</summary>
    private static MamaTurtleCueCases<ApplyMamaTurtleCueContext, MamaTurtleSegment?> ApplyMamaTurtleCueCases { get; } = new(
            RiseToHover: static (rise, context) =>
            {
                StartMamaTurtleRisingToHover(context.Place.RequireSpeciesState<MamaTurtleEnemyState>(), direction: rise.Direction);
                return MamaTurtleSegment.Spinning;
            },
            Signal: static (signal, context) =>
            {
                context.Owner.MamaTurtleSignals[signal.Kind](context.Place);
                return null;
            });

    /// <summary>What <c>ApplyMamaTurtleCue</c>'s cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Place">The <c>place</c> <c>ApplyMamaTurtleCue</c> was handed.</param>
    /// <param name="Owner">The <see cref="MamaTurtleBehavior"/> reacting.</param>
    private readonly record struct ApplyMamaTurtleCueContext(RoomEnemyPlace Place, MamaTurtleBehavior Owner);

    /// <summary>What each of the Mama Turtle's signals does; each runs on.</summary>
    private IdentityTable<MamaTurtleCueKind, Action<RoomEnemyPlace>> MamaTurtleSignals => field ??= new(new Dictionary<MamaTurtleCueKind, Action<RoomEnemyPlace>>
    {
        [MamaTurtleCueKind.EnterShell] = place => StartMamaTurtleEnteringShell(place.RequireSpeciesState<MamaTurtleEnemyState>()),
        [MamaTurtleCueKind.PlaySpinningSound] = _ => _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal),
    });

    /// <summary>
    /// Carries out one cue of a baby turtle's animations. The crawl loop, leaving the shell while ridden and the end of
    /// leaving it continue at the animation their cues return; every other cue runs on.
    /// </summary>
    private BabyTurtleSegment? ApplyBabyTurtleCue(RoomEnemyPlace place, BabyTurtleCue cue, SamusState samus, RoomLevelData level) =>
        BabyTurtleCues[cue.Kind](place, samus, level);

    /// <summary>What each of a baby turtle's cues does, and the animation it continues at, or null to run on.</summary>
    private IdentityTable<BabyTurtleCueKind, Func<RoomEnemyPlace, SamusState, RoomLevelData, BabyTurtleSegment?>> BabyTurtleCues => field ??=
        new(new Dictionary<BabyTurtleCueKind, Func<RoomEnemyPlace, SamusState, RoomLevelData, BabyTurtleSegment?>>
        {
            [BabyTurtleCueKind.Crawl] = (place, samus, level) =>
            {
                ProcessBabyTurtleCrawlAnimation(place, place.RequireSpeciesState<BabyTurtleEnemyState>(), samus, level);
                return null;
            },
            [BabyTurtleCueKind.LoopOrTurnAround] = (place, _, _) => SelectBabyTurtleCrawlLoop(place, place.RequireSpeciesState<BabyTurtleEnemyState>()),
            [BabyTurtleCueKind.LeaveShell] = (place, samus, _) => SelectBabyTurtleLeaveShell(place, samus),
            [BabyTurtleCueKind.LeftShell] = (place, samus, _) => FinishBabyTurtleLeavingShell(place, place.RequireSpeciesState<BabyTurtleEnemyState>(), samus),
            [BabyTurtleCueKind.SetSpinningStoppable] = (place, _, _) =>
            {
                place.RequireSpeciesState<BabyTurtleEnemyState>().Action = BabyTurtleAction.SpinningStoppable;
                return null;
            },
            [BabyTurtleCueKind.PlaySpinningSound] = (_, _, _) =>
            {
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            },
        });

    /// <summary>Initializes the Mama Turtle and links her to the four babies her room lists with her.</summary>
    internal void InitializeMamaTurtle(RoomEnemyPlace place)
    {
        RoomEnemyPlace[] babies = _places.Members(place, MamaTurtlePartListingDefinitions.Listing, EnemyDefinitionId.MamaTurtle, EnemyDefinitionId.BabyTurtle);
        _babiesOfMama.Place(place, babies);
        foreach (RoomEnemyPlace baby in babies)
            _mamaOfBaby.Place(baby, place);

        var state = new MamaTurtleEnemyState(MamaTurtleAction.Placed);
        place.SetSpeciesState(state);

        // Mama animates: leaving the shell needs her animation to run.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.PointAtOrigin;
        place.AnimationClock.Restart();
        place.Radius = place.Radius.WithY(0);
        state.Playback = new(MamaTurtleAnimations.StartingWith(MamaTurtleSegment.Asleep), AnimationPlace.StartOfAnimation);
    }

    /// <summary>
    /// Initializes the Baby Turtle, one of the four its Mama found when she was placed; the last of them completes the
    /// family, which is checked once then.
    /// </summary>
    internal void InitializeBabyTurtle(RoomEnemyOccupant occupant, RoomEnemyPlace place)
    {
        var state = new BabyTurtleEnemyState(BabyTurtleAction.CrawlingNotCarryingSamus)
        {
            SpawnTopCenter = new Position16<RoomPixels>(place.Position.X.Whole, place.Position.Y.Whole - place.Radius.Y),
            Heading = occupant.SpawnSettings<BabyTurtleSpawnSettings>().Heading,
            HorizontalSpeed = MamaTurtleMotionDefinitions.BabyCrawlSpeed,
        };
        place.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(place, BabyTurtleAnimations.StartingWith(BabyCrawl(state.Heading)));
        CheckMamaTurtleFamilyCompletedBy(place);
    }

    /// <summary>
    /// Checks, as <paramref name="baby"/> is placed, that a Mama found it when she was placed and, when it is the last of
    /// her four babies, that all four are baby turtles. A killed baby's place, empty until it respawns, stays hers.
    /// </summary>
    /// <exception cref="InvalidOperationException">No Mama Turtle placed before it found <paramref name="baby"/>.</exception>
    /// <exception cref="InvalidDataException">The four places listed after the Mama do not all hold baby turtles.</exception>
    private void CheckMamaTurtleFamilyCompletedBy(RoomEnemyPlace baby)
    {
        RoomEnemyPlace mama = MamaOf(baby);
        RoomEnemyPlace[] babies = _babiesOfMama.Of(mama);
        if (babies[^1] != baby)
            return;
        foreach (RoomEnemyPlace child in babies)
        {
            if (child.Entity is not BabyTurtleEntity && child.Occupant is { } found)
            {
                throw new InvalidDataException(
                    $"Mama Turtle place {mama.Ordinal} expected Baby Turtle in following " +
                    $"place {child.Ordinal}, found {found.DefinitionId}.");
            }
        }
    }

    /// <summary>What <c>RunMamaTurtleMain</c> does for each <see cref="MamaTurtleAction"/> member, each member's own arm.</summary>
    private IdentityTable<MamaTurtleAction, Action<RoomEnemyOccupant, RoomEnemyPlace, MamaTurtleEnemyState, SamusState, PseudoScrewCharge, RoomLevelData, int>> RunMamaTurtleMainForMamaTurtleAction => field ??= new(new Dictionary<MamaTurtleAction, Action<RoomEnemyOccupant, RoomEnemyPlace, MamaTurtleEnemyState, SamusState, PseudoScrewCharge, RoomLevelData, int>>
    {
        [MamaTurtleAction.Placed] = (_, place, state, samus, pseudoScrew, level, frameCycle) => state.Action = MamaTurtleAction.Asleep,
        [MamaTurtleAction.PlacedAndWoken] = (_, place, state, samus, pseudoScrew, level, frameCycle) => state.Action = MamaTurtleAction.Woken,
        [MamaTurtleAction.Idle] = (_, place, state, samus, pseudoScrew, level, frameCycle) => { },
        [MamaTurtleAction.Asleep] = (_, place, state, samus, pseudoScrew, level, frameCycle) => RunMamaTurtleAsleep(place, samus),
        [MamaTurtleAction.Woken] = (_, place, state, samus, pseudoScrew, level, frameCycle) =>
        {
            state.Action = MamaTurtleAction.LeavingShell;
            place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        },
        [MamaTurtleAction.LeavingShell] = (turtle, place, state, samus, pseudoScrew, level, frameCycle) =>
        {
            RunMamaTurtleLeavingShell(
                turtle, state, samus, pseudoScrew, level, frameCycle);
        },
        [MamaTurtleAction.EnteringShell] = (_, place, state, samus, pseudoScrew, level, frameCycle) => RunMamaTurtleEnteringShell(place, state, samus),
        [MamaTurtleAction.RisingToHover] = (turtle, place, state, samus, pseudoScrew, level, frameCycle) => RunMamaTurtleRisingToHover(turtle, state, samus, pseudoScrew, level),
        [MamaTurtleAction.Hovering] = (turtle, place, state, samus, pseudoScrew, level, frameCycle) => RunMamaTurtleHovering(turtle, state, samus, pseudoScrew, level),
        [MamaTurtleAction.RisingToPeak] = (turtle, place, state, samus, pseudoScrew, level, frameCycle) => RunMamaTurtleRisingToPeak(turtle, state, samus, pseudoScrew),
        [MamaTurtleAction.HoveringAtPeak] = (turtle, place, state, samus, pseudoScrew, level, frameCycle) => RunMamaTurtleHoveringAtPeak(turtle, state, samus, pseudoScrew),
        [MamaTurtleAction.Falling] = (turtle, place, state, samus, pseudoScrew, level, frameCycle) => RunMamaTurtleFalling(turtle, state, samus, pseudoScrew, level),
    });

    /// <summary>Runs the Mama Turtle's main behaviour and its actions.</summary>
    private void RunMamaTurtleMain(
        RoomEnemyOccupant turtle,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level,
        int frameCycle)
    {
        RoomEnemyPlace place = turtle.Place;
        RunMamaTurtleMainForMamaTurtleAction[state.Action](turtle, place, state, samus, pseudoScrew, level, frameCycle);
    }

    /// <summary>Runs the dormant shell's height and carry step.</summary>
    private static void RunMamaTurtleAsleep(
        RoomEnemyPlace mama,
        SamusState samus)
    {
        mama.Radius = mama.Radius.WithY(0);
        // The shell's contour on Samus's left differs from its contour on her right; the asymmetry is visible
        // around the dormant shell's lip.
        if (!MamaTurtleShell.TryGetSurface(mama.Position.X.Whole - samus.Position.X.Whole, out int contourOffset))
            return;

        mama.Radius = mama.Radius.WithY(-contourOffset);
        mama.Contact = mama.Contact.With(EnemyContact.SolidToSamus);

        if (!EnemyCollision.IsSamusStandingOn(mama, samus))
            return;

        int shellTop = mama.Position.Y.Whole - mama.Radius.Y;
        int overlap = samus.Kinematics.BottomBoundary - shellTop;
        if (overlap < 0)
            return;

        samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, overlap);
    }

    /// <summary>Runs the deliberately jittery shell-opening step.</summary>
    private void RunMamaTurtleLeavingShell(
        RoomEnemyOccupant turtle,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level,
        int frameCycle)
    {
        RoomEnemyPlace mama = turtle.Place;
        ResolveMamaTurtleExpandedContact(turtle, samus, pseudoScrew);
        if (!MamaTurtleCadenceDefinitions.LeavingShellStep.IsDue(frameCycle))
            return;

        if (EnemyCollision.IsSamusStandingOn(mama, samus))
        {
            samus.Kinematics.ExtraDisplacement -= MamaTurtleMotionDefinitions.LeavingShellRiderShift;
        }

        mama.Position = mama.Position.WithWholeY(mama.Position.Y.Whole - MamaTurtleMotionDefinitions.LeavingShellClimb);
        mama.Radius = mama.Radius.WithY(MamaTurtleMotionDefinitions.AwakeHalfHeight);
        mama.Position = mama.Position.WithWholeX(MamaTurtleCadenceDefinitions.LeavingShellSidestepsLeftAt(mama.Position.Y.Whole)
            ? mama.Position.X.Whole - MamaTurtleMotionDefinitions.LeavingShellShake
            : mama.Position.X.Whole + MamaTurtleMotionDefinitions.LeavingShellShake);

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, SignedSixteenSixteen.FromWholePixels(MamaTurtleMotionDefinitions.LeavingShellPush)))
            return;

        StartTurtleAnimation(
            mama,
            state,
            MamaTurtleSegment.LeaveShellLeft);

        // Leaving the shell changes nothing else about Mama.
        state.Action = MamaTurtleAction.Idle;
    }

    /// <summary>Runs the facing-dependent return-to-shell selector.</summary>
    private static void RunMamaTurtleEnteringShell(
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        StartTurtleAnimation(
            mama,
            state,
            mama.Position.X.Whole >= samus.Position.X.Whole
                ? MamaTurtleSegment.EnterShellLeft
                : MamaTurtleSegment.EnterShellRight);
        state.Action = MamaTurtleAction.Idle;
    }

    /// <summary>Runs the upward transition (<see cref="MamaTurtleMotionDefinitions.RiseToHoverDuration"/>).</summary>
    private void RunMamaTurtleRisingToHover(
        RoomEnemyOccupant turtle,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level)
    {
        RoomEnemyPlace mama = turtle.Place;
        ResolveMamaTurtleExpandedContact(turtle, samus, pseudoScrew);
        if (_terrain.MoveEnemyVertically(level, mama, SignedSixteenSixteen.FromWholePixels(-MamaTurtleMotionDefinitions.RiseToHoverSpeed)))
            return;

        if (EnemyCollision.IsSamusStandingOn(mama, samus))
        {
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, MamaTurtleMotionDefinitions.RiseToHoverSpeed);
        }

        if (!state.ActionTimer.StepEnds())
            return;

        // She hovers off from rest toward Samus's side, speeding up by her hover acceleration each update.
        state.XAcceleration = MamaTurtleMotionDefinitions.HoverAcceleration.Toward(
            mama.Position.X.Whole < samus.Position.X.Whole ? HorizontalDirection.Right : HorizontalDirection.Left);
        state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.Zero);
        state.Action = MamaTurtleAction.Hovering;
    }

    /// <summary>Runs the accelerating horizontal hover, which a wall reverses with a quake and a sound.</summary>
    private void RunMamaTurtleHovering(
        RoomEnemyOccupant turtle,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level)
    {
        RoomEnemyPlace mama = turtle.Place;
        ResolveMamaTurtleExpandedContact(turtle, samus, pseudoScrew);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, state.Velocity.X))
        {
            ReverseMamaTurtleAtWall(state);
            _earthquake.Start(MamaTurtleMotionDefinitions.WallQuake, MamaTurtleMotionDefinitions.WallQuakeDuration);
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleWallSound, priority: SoundRequestPriority.Normal);
            return;
        }

        HandleSamusLandingOnMamaTurtle(mama, state, samus);
        AddAndClampMamaTurtleHorizontalAcceleration(state);
    }

    /// <summary>Handles Samus landing on the hovering Mama Turtle.</summary>
    private static void HandleSamusLandingOnMamaTurtle(
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (!EnemyCollision.IsSamusStandingOn(mama, samus))
            return;

        state.Action = MamaTurtleAction.RisingToPeak;
        SignedSixteenSixteen result = samus.Kinematics.ExtraDisplacement.X - state.Velocity.X;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(result.WithWholePixelsAtLeast(-MamaTurtleMotionDefinitions.LargestLeftwardCarry));
    }

    /// <summary>
    /// Adds the hover acceleration to the horizontal speed, a fraction past a whole pixel carrying into the whole pixels, and
    /// caps the whole speed using the old velocity's sign.
    /// </summary>
    private static void AddAndClampMamaTurtleHorizontalAcceleration(
        MamaTurtleEnemyState state)
    {
        SignedSixteenSixteen velocity = state.Velocity.X;
        // A fraction past a whole pixel carries into the whole pixels.
        SignedSixteenSixteen sum = velocity + state.XAcceleration;

        // The cap holds only the whole pixels, the sum's fraction kept, and takes its sign from the velocity before this
        // update's acceleration, so a velocity crossing zero this update is capped toward its old side for one update.
        state.Velocity = state.Velocity.WithX(sum.WholePixelsReachEitherWay(MamaTurtleMotionDefinitions.FastestHover)
            ? sum.WithWholePixelsOf(velocity.Along(MamaTurtleMotionDefinitions.FastestHover))
            : sum);
    }

    /// <summary>
    /// How far the falling Mama Turtle moves this update for a fall speed of <paramref name="fall"/>: its whole pixels alone
    /// (#2555). The fraction stays in her speed and only decides on which update a later whole pixel is reached.
    /// </summary>
    private static SignedSixteenSixteen WholePixelFall(SignedSixteenSixteen fall) => fall.WithoutFraction();

    /// <summary>Reverses the hovering turtle's horizontal velocity and acceleration at a wall (<see cref="MamaTurtleWallReversal"/>).</summary>
    private static void ReverseMamaTurtleAtWall(MamaTurtleEnemyState state)
    {
        SignedSixteenSixteen velocity = state.Velocity.X;
        state.Velocity = state.Velocity.WithX(MamaTurtleWallReversal.ReversedVelocity(velocity));
        state.XAcceleration = MamaTurtleWallReversal.ReversedAcceleration(state.XAcceleration, velocity);
    }

    /// <summary>Runs the rider-powered ascent (<see cref="MamaTurtleMotionDefinitions.RiddenAscentSpeed"/>).</summary>
    private void RunMamaTurtleRisingToPeak(
        RoomEnemyOccupant turtle,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace mama = turtle.Place;
        ResolveMamaTurtleExpandedContact(turtle, samus, pseudoScrew);
        if (mama.Position.Y.Whole < MamaTurtleMotionDefinitions.RiddenAscentPeak)
        {
            state.ActionTimer = UpdateCountdown.For(MamaTurtleMotionDefinitions.PeakPause);
            state.Action = MamaTurtleAction.HoveringAtPeak;
            state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.Zero);
            return;
        }

        if (EnemyCollision.IsSamusStandingOn(mama, samus))
        {
            mama.Position = mama.Position.WithWholeY(mama.Position.Y.Whole - MamaTurtleMotionDefinitions.RiddenAscentSpeed);
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, MamaTurtleMotionDefinitions.RiddenAscentSpeed);
            return;
        }

        state.Action = MamaTurtleAction.Falling;
        state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.Zero);
    }

    /// <summary>Runs the peak pause (<see cref="MamaTurtleMotionDefinitions.PeakPause"/>).</summary>
    private void RunMamaTurtleHoveringAtPeak(
        RoomEnemyOccupant turtle,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        _ = turtle.Place;
        ResolveMamaTurtleExpandedContact(turtle, samus, pseudoScrew);
        if (state.ActionTimer.StepEnds())
            state.Action = MamaTurtleAction.Falling;
    }

    /// <summary>Runs gravity, collision, and landing-animation selection.</summary>
    private void RunMamaTurtleFalling(
        RoomEnemyOccupant turtle,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level)
    {
        RoomEnemyPlace mama = turtle.Place;
        ResolveMamaTurtleExpandedContact(turtle, samus, pseudoScrew);

        // The cap compares Mama's own fall speed.
        if (state.Velocity.Y < MamaTurtleMotionDefinitions.FastestFall)
        {
            // The fall acceleration's fraction step carries into the whole pixels.
            state.Velocity = state.Velocity.WithY(
                state.Velocity.Y + MamaTurtleMotionDefinitions.FallAcceleration);
        }

        if (!_terrain.MoveEnemyVertically(level, mama, WholePixelFall(state.Velocity.Y)))
            return;

        // She lands facing the way her horizontal velocity carries her, which its sign says (#2473): her hover's
        // acceleration and the wall reversal can each turn that velocity round, so no separate heading is kept.
        StartTurtleAnimation(
            mama,
            state,
            state.Velocity.X.IsNegative
                ? MamaTurtleSegment.LeaveShellLeft
                : MamaTurtleSegment.LeaveShellRight);
        state.Action = MamaTurtleAction.Idle;
    }

    /// <summary>What <c>RunBabyTurtleMain</c> does for each <see cref="BabyTurtleAction"/> member, each member's own arm.</summary>
    private IdentityTable<BabyTurtleAction, Action<RoomEnemyPlace, BabyTurtleEnemyState, SamusState, RoomLevelData>> RunBabyTurtleMainForBabyTurtleAction => field ??= new(new Dictionary<BabyTurtleAction, Action<RoomEnemyPlace, BabyTurtleEnemyState, SamusState, RoomLevelData>>
    {
        [BabyTurtleAction.CrawlingNotCarryingSamus] = (baby, state, samus, level) =>
        {
            if (EnemyCollision.IsSamusStandingOn(baby, samus))
            {
                state.Action = BabyTurtleAction.HidingCarryingSamus;
                state.NotCarryingSamusReactionTimer = UpdateCountdown.For(MamaTurtleMotionDefinitions.BabyRiderLeftDelay);
                StartTurtleAnimation(
                    baby,
                    state,
                    state.Heading == HorizontalDirection.Left
                        ? BabyTurtleSegment.HidingLeft
                        : BabyTurtleSegment.HidingRight);
            }
        },
        [BabyTurtleAction.HidingCarryingSamus] = (baby, state, samus, level) =>
        {
            if (EnemyCollision.IsSamusStandingOn(baby, samus))
            {
                state.NotCarryingSamusReactionTimer = UpdateCountdown.For(MamaTurtleMotionDefinitions.BabyRiderLeftDelay);
                return;
            }
            if (state.NotCarryingSamusReactionTimer.StepEnds())
            {
                state.Action = BabyTurtleAction.HidingNotCarryingSamus;
                state.ActionTimer = UpdateCountdown.For(MamaTurtleMotionDefinitions.BabyHideDuration);
            }
        },
        [BabyTurtleAction.HidingNotCarryingSamus] = (baby, state, samus, level) =>
        {
            if (EnemyCollision.IsSamusStandingOn(baby, samus))
            {
                state.Action = BabyTurtleAction.SpinningUnstoppable;
                StartTurtleAnimation(
                    baby,
                    state,
                    BabyTurtleSegment.Spinning);
                // The baby spins off the way Samus faces (#2338).
                state.Heading = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Right ? HorizontalDirection.Right : HorizontalDirection.Left;
                state.HorizontalSpeed = MamaTurtleMotionDefinitions.BabySpinVelocity.X;
                state.SpinFallSpeed = MamaTurtleMotionDefinitions.BabySpinVelocity.Y;
                return;
            }
            if (state.ActionTimer.StepEnds())
            {
                StartBabyTurtleCrawlingAnimation(baby, state);
                state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
            }
        },
        [BabyTurtleAction.SpinningUnstoppable] = (baby, state, samus, level) => MoveSpinningBabyTurtle(baby, state, level),
        [BabyTurtleAction.SpinningStoppable] = (baby, state, samus, level) =>
        {
            if (EnemyCollision.IsSamusStandingOn(baby, samus))
            {
                StartBabyTurtleCrawlingAnimation(baby, state);
                state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
                return;
            }
            MoveSpinningBabyTurtle(baby, state, level);
        },
        [BabyTurtleAction.CrawlingCarryingSamus] = (baby, state, samus, level) =>
        {
            if (!EnemyCollision.IsSamusStandingOn(baby, samus))
                state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
        },
    });

    /// <summary>Runs the Baby Turtle's main behaviour and its six live actions.</summary>
    private void RunBabyTurtleMain(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        RunBabyTurtleMainForBabyTurtleAction[state.Action](baby, state, samus, level);
    }

    /// <summary>Moves a spinning child against terrain, reversing its X velocity on wall collision.</summary>
    private void MoveSpinningBabyTurtle(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.X)))
        {
            state.Heading = state.Heading.Reversed();
            return;
        }
        _terrain.MoveEnemyVertically(level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.Y));
    }

    /// <summary>Animation cue: crawl across the dormant parent contour.</summary>
    private void ProcessBabyTurtleCrawlAnimation(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        bool carryingSamus = EnemyCollision.IsSamusStandingOn(baby, samus);
        if (carryingSamus)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(state.Velocity.X, 0);
        }

        int previousY = baby.Position.Y.Whole;
        baby.Position = baby.Position.WithWholeY(state.SpawnTopCenter.Y);
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.X));

        RoomEnemyPlace parentPlace = MamaOf(baby);
        MamaTurtleEnemyState parent = parentPlace.RequireSpeciesState<MamaTurtleEnemyState>();
        if (!parent.LiesInShell)
            return;

        int contourOffset = MamaTurtleShell.TryGetSurface(parentPlace.Position.X.Whole - baby.Position.X.Whole, out int surface)
            ? surface
            : MamaTurtleMotionDefinitions.BabySinkClearOfShell;
        _terrain.MoveEnemyVertically(level, baby, SignedSixteenSixteen.FromWholePixels(contourOffset));

        if (carryingSamus)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, baby.Position.Y.Whole - previousY);
        }
    }

    /// <summary>Animation cue: turn around at the baby's travel distance from spawn.</summary>
    private static BabyTurtleSegment SelectBabyTurtleCrawlLoop(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state)
    {
        int signedDifference = state.SpawnTopCenter.X - baby.Position.X.Whole;
        int distance = Math.Abs(signedDifference);
        if (distance >= MamaTurtleMotionDefinitions.BabyTravelDistance)
        {
            // At the end of its crawl the baby crawls back toward where it spawned, at its crawling speed.
            state.Heading = signedDifference < 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            state.HorizontalSpeed = MamaTurtleMotionDefinitions.BabyCrawlSpeed;
        }
        return BabyCrawl(state.Heading);
    }

    /// <summary>Animation cue: begin Mama's shell-entry behavior.</summary>
    private static void StartMamaTurtleEnteringShell(MamaTurtleEnemyState state) =>
        state.Action = MamaTurtleAction.EnteringShell;

    /// <summary>Animation cues: begin rising toward <paramref name="direction"/>.</summary>
    private static void StartMamaTurtleRisingToHover(
        MamaTurtleEnemyState state,
        HorizontalDirection direction)
    {
        state.Action = MamaTurtleAction.RisingToHover;
        state.Velocity = state.Velocity.WithX(state.Velocity.X.WithWholePixelsOf(MamaTurtleMotionDefinitions.RiseToHoverDrift.Toward(direction)));
        state.ActionTimer = UpdateCountdown.For(MamaTurtleMotionDefinitions.RiseToHoverDuration);
    }

    /// <summary>
    /// Animation cue: branch out of hiding only while ridden, whichever way the baby crawls (#2337); otherwise the hiding
    /// animation runs on.
    /// </summary>
    private static BabyTurtleSegment? SelectBabyTurtleLeaveShell(RoomEnemyPlace baby, SamusState samus) =>
        EnemyCollision.IsSamusStandingOn(baby, samus) ? BabyTurtleSegment.LeaveShell : null;

    /// <summary>Animation cue: restore crawling after leaving the shell.</summary>
    private static BabyTurtleSegment FinishBabyTurtleLeavingShell(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state,
        SamusState samus)
    {
        state.Action = EnemyCollision.IsSamusStandingOn(baby, samus)
            ? BabyTurtleAction.CrawlingCarryingSamus
            : BabyTurtleAction.CrawlingNotCarryingSamus;
        return BabyCrawl(state.Heading);
    }

    /// <summary>Runs Mama's custom ordinary-touch reaction.</summary>
    private void ResolveMamaTurtleTouch(
        RoomEnemyOccupant turtle,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace mama = turtle.Place;
        if (mama.Contact.HasAny(EnemyContact.SolidToSamus))
            return;
        _collision.ResolveOrdinaryTouch(turtle, samus, pseudoScrew);
        state.Action = MamaTurtleAction.Falling;
        state.Velocity = state.Velocity.WithY(state.Velocity.Y.WithWholePixelsOf(MamaTurtleMotionDefinitions.KnockedDownFallSpeed));
    }

    /// <summary>Runs Baby's direction flip, separation, and wake reaction.</summary>
    private void ResolveBabyTurtleTouch(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (state.Action == BabyTurtleAction.CrawlingCarryingSamus)
            return;

        // Samus's touch turns the baby around, crawling (#2338).
        state.Heading = state.Heading.Reversed();
        state.HorizontalSpeed = MamaTurtleMotionDefinitions.BabyCrawlSpeed;
        StartTurtleAnimation(baby, state, BabyCrawl(state.Heading));

        baby.Position = baby.Position.WithWholeX(baby.Position.X.Whole >= samus.Position.X.Whole
            ? samus.Position.X.Whole + samus.Kinematics.Radius.X + baby.Radius.X
            : samus.Position.X.Whole - samus.Kinematics.Radius.X - baby.Radius.X);
        state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.X));
        WakeMamaTurtle(baby);
    }

    /// <summary>Finishes the Baby Turtle's reaction to a shot on <paramref name="baby"/>: its mother wakes.</summary>
    private void ResolveBabyTurtleShotAfterCommon(RoomEnemyPlace baby) =>
        WakeMamaTurtle(baby);

    /// <summary>Wakes <paramref name="baby"/>'s Mama after a baby touch or shot reaction.</summary>
    private void WakeMamaTurtle(RoomEnemyPlace baby)
    {
        MamaStateOf(baby).Wake();
    }

    /// <summary>Runs Mama's expanded private damage box: Samus, while not invincible, touching it is hurt as by any enemy.</summary>
    private void ResolveMamaTurtleExpandedContact(
        RoomEnemyOccupant turtle,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        RoomEnemyPlace mama = turtle.Place;
        if (!MamaTurtleDamageBox.Touches(mama, samus) || !samus.InvincibilityTimer.IsZero)
            return;

        _collision.ResolveOrdinaryTouch(turtle, samus, pseudoScrew);
    }

    /// <summary>The state of <paramref name="baby"/>'s Mama, who found it when she was placed.</summary>
    private MamaTurtleEnemyState MamaStateOf(RoomEnemyPlace baby) => MamaOf(baby).RequireSpeciesState<MamaTurtleEnemyState>();

    /// <summary>The place of <paramref name="baby"/>'s Mama, who linked it when she was placed.</summary>
    /// <exception cref="InvalidOperationException">No Mama Turtle found <paramref name="baby"/> as she was placed.</exception>
    private RoomEnemyPlace MamaOf(RoomEnemyPlace baby) => _mamaOfBaby.Of(baby);

    /// <summary>Starts the crawl animation of the way the child heads.</summary>
    private static void StartBabyTurtleCrawlingAnimation(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state) =>
        StartTurtleAnimation(baby, state, BabyCrawl(state.Heading));

    /// <summary>The baby's crawl heading <paramref name="heading"/>.</summary>
    private static BabyTurtleSegment BabyCrawl(HorizontalDirection heading) =>
        heading == HorizontalDirection.Left ? BabyTurtleSegment.CrawlingLeft : BabyTurtleSegment.CrawlingRight;

    /// <summary>Starts a Mama Turtle animation, which shows its first frame on the next update.</summary>
    private static void StartTurtleAnimation(RoomEnemyPlace place, MamaTurtleEnemyState state, MamaTurtleSegment segment)
    {
        state.Playback = new(MamaTurtleAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Starts a baby turtle animation, which shows its first frame on the next update.</summary>
    private static void StartTurtleAnimation(RoomEnemyPlace place, BabyTurtleEnemyState state, BabyTurtleSegment segment)
    {
        state.Playback = new(BabyTurtleAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs the baby wake reaction after common bomb damage.</summary>
    internal EnemyBombReaction BabyTurtleBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveBabyTurtleShotAfterCommon(c.Enemy),
    };

    /// <summary>Runs MamaTurtle's main AI.</summary>
    internal void RunMamaTurtleMainAi(RoomEnemyOccupant turtle, IEnemyAiFrame frame)
    {
        RoomEnemyPlace place = turtle.Place;
        RunMamaTurtleMain(
            turtle,
            place.RequireSpeciesState<MamaTurtleEnemyState>(),
            frame.Samus,
            frame.PseudoScrew,
            frame.Level,
            frame.Clock.ShortCounter);
    }

    /// <summary>Runs BabyTurtle's main AI.</summary>
    internal void RunBabyTurtleMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunBabyTurtleMain(place, place.RequireSpeciesState<BabyTurtleEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances MamaTurtle's typed animation.</summary>
    internal void AdvanceMamaTurtleTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        MamaTurtleEnemyState state = place.RequireSpeciesState<MamaTurtleEnemyState>();
        AnimationPlayback<MamaTurtleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, MamaTurtleAnimations.Segments, ref playback,
            cue => ApplyMamaTurtleCue(place, cue) is { } target
                ? new AnimationContinuation<MamaTurtleSegment>(MamaTurtleAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Advances BabyTurtle's typed animation.</summary>
    internal void AdvanceBabyTurtleTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext context)
    {
        if (!place.AnimationClock.Expire())
            return;
        BabyTurtleEnemyState state = place.RequireSpeciesState<BabyTurtleEnemyState>();
        AnimationPlayback<BabyTurtleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, BabyTurtleAnimations.Segments, ref playback,
            cue => ApplyBabyTurtleCue(place, cue, context.Samus, context.Level) is { } target
                ? new AnimationContinuation<BabyTurtleSegment>(BabyTurtleAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Runs the baby wake reaction after common projectile damage.</summary>
    internal EnemyShotReaction BabyTurtleShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveBabyTurtleShotAfterCommon(c.Enemy),
    };

    /// <summary>Routes Mama contact to shell and fall handling.</summary>
    internal void ReactToMamaTurtleTouch(EnemyTouchContext context) =>
        ResolveMamaTurtleTouch(context.Occupant, context.Place.RequireSpeciesState<MamaTurtleEnemyState>(), context.Samus, context.PseudoScrew);

    /// <summary>Routes baby contact to separation and wake behavior.</summary>
    internal void ReactToBabyTurtleTouch(EnemyTouchContext context) =>
        ResolveBabyTurtleTouch(context.Place, context.Place.RequireSpeciesState<BabyTurtleEnemyState>(), context.Samus, context.Level);
}
