using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Earthquakes;

namespace SuperMetroid.Core.Enemies.MamaTurtles;

/// <summary>The mama turtle species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MamaTurtleBehavior : IEnemySpecies, IEnemySettingsGroupFactory<MamaTurtleSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.BabyTurtle, EnemyDefinitionId.MamaTurtle];

    /// <inheritdoc/>
    public int PartCount => MamaTurtleMotionDefinitions.BabyCount;

    /// <summary>
    /// The mama turtle, who heads the four babies her room lists right after her (#2146, #2554): each baby holds her from its
    /// setup and first crawls the way her placement names for it.
    /// </summary>
    public EnemyPlaced CreateGroup(EnemyGroupPlaces places, EnemyCommonParticipation common, MamaTurtleSpawnSettings settings)
    {
        RoomEnemyPlace head = Ensure.NotNull(places).Head;
        var mama = new MamaTurtleEntity(this, head, common);
        return new EnemyPlaced(mama, [.. Ensure.NotNull(settings).BabyHeadings.Select(heading =>
            (EnemyPartBuild)((baby, babyCommon, _) => new BabyTurtleEntity(this, baby, babyCommon, new BabyTurtleStart(head, mama.State, heading))))]);
    }

    /// <summary>Moves parent and children against room blocks during hover, fall, and crawling.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Records the wall-impact shake from Mama Turtle's hover collision.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>Applies touch damage for Mama's expanded body or exposed shell.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects Mama Turtle and her babies to room terrain, collision, places, and earthquake output.</summary>
    internal MamaTurtleBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _earthquake = earthquake;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Mama Turtle's animations; rising to hover continues at its spin.</summary>
    private MamaTurtleSegment? ApplyMamaTurtleCue(MamaTurtleEnemyState state, MamaTurtleCue cue)
    {
        switch (cue)
        {
            case MamaTurtleCue.RiseToHover rise:
                StartMamaTurtleRisingToHover(state, direction: rise.Direction);
                return MamaTurtleSegment.Spinning;
            case MamaTurtleCue.Signal { Kind: MamaTurtleCueKind.EnterShell }:
                StartMamaTurtleEnteringShell(state);
                return null;
            case MamaTurtleCue.Signal { Kind: MamaTurtleCueKind.PlaySpinningSound }:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(MamaTurtleCue)} {cue}.");
        }
    }

    /// <summary>
    /// Carries out one cue of a baby turtle's animations. The crawl loop, leaving the shell while ridden and the end of
    /// leaving it continue at the animation their cues return; every other cue runs on.
    /// </summary>
    private BabyTurtleSegment? ApplyBabyTurtleCue(RoomEnemyPlace place, BabyTurtleEnemyState state, BabyTurtleCue cue, SamusState samus, RoomLevelData level)
    {
        switch (cue.Kind)
        {
            case BabyTurtleCueKind.Crawl:
                ProcessBabyTurtleCrawlAnimation(place, state, samus, level);
                return null;
            case BabyTurtleCueKind.LoopOrTurnAround:
                return SelectBabyTurtleCrawlLoop(place, state);
            case BabyTurtleCueKind.LeaveShell:
                return SelectBabyTurtleLeaveShell(place, samus);
            case BabyTurtleCueKind.LeftShell:
                return FinishBabyTurtleLeavingShell(place, state, samus);
            case BabyTurtleCueKind.SetSpinningStoppable:
                state.Action = BabyTurtleAction.SpinningStoppable;
                return null;
            case BabyTurtleCueKind.PlaySpinningSound:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            default:
                throw new InvalidOperationException($"Undefined {nameof(BabyTurtleCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Initializes the Mama Turtle, who heads the four babies her room lists right after her.</summary>
    internal static MamaTurtleEnemyState InitializeMamaTurtle(RoomEnemyPlace place)
    {
        var state = new MamaTurtleEnemyState(MamaTurtleAction.Placed);

        // Mama animates: leaving the shell needs her animation to run.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.Restart();
        place.Radius = place.Radius.WithY(0);
        state.Playback = new(MamaTurtleAnimations.StartingWith(MamaTurtleSegment.Asleep), AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>Initializes the Baby Turtle, one of the four its Mama heads.</summary>
    /// <param name="place">The baby's place.</param>
    /// <param name="start">The Mama that heads it, which it holds from here, and the way it first crawls.</param>
    internal static BabyTurtleEnemyState InitializeBabyTurtle(RoomEnemyPlace place, BabyTurtleStart start)
    {
        var state = new BabyTurtleEnemyState(BabyTurtleAction.CrawlingNotCarryingSamus, Ensure.NotNull(start).Mama, start.MamaPlace)
        {
            SpawnTopCenter = new Position16<RoomPixels>(place.Position.X.Whole, place.Position.Y.Whole - place.Radius.Y),
            Heading = start.Heading,
            HorizontalSpeed = MamaTurtleMotionDefinitions.BabyCrawlSpeed,
        };

        state.Playback = EnemyAnimationPlayback.Start(place, BabyTurtleAnimations.StartingWith(BabyCrawl(state.Heading)));
        return state;
    }

    /// <summary>Runs the Mama Turtle's main behaviour and its actions.</summary>
    private void RunMamaTurtleMain(
        RoomEnemyPlace place,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level,
        int frameCycle)
    {
        switch (state.Action)
        {
            case MamaTurtleAction.Placed:
                state.Action = MamaTurtleAction.Asleep;
                return;
            case MamaTurtleAction.PlacedAndWoken:
                state.Action = MamaTurtleAction.Woken;
                return;
            case MamaTurtleAction.Idle:
                return;
            case MamaTurtleAction.Asleep:
                RunMamaTurtleAsleep(place, samus);
                return;
            case MamaTurtleAction.Woken:
                state.Action = MamaTurtleAction.LeavingShell;
                place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
                return;
            case MamaTurtleAction.LeavingShell:
                RunMamaTurtleLeavingShell(
                    place, state, samus, pseudoScrew, level, frameCycle);
                return;
            case MamaTurtleAction.EnteringShell:
                RunMamaTurtleEnteringShell(place, state, samus);
                return;
            case MamaTurtleAction.RisingToHover:
                RunMamaTurtleRisingToHover(place, state, samus, pseudoScrew, level);
                return;
            case MamaTurtleAction.Hovering:
                RunMamaTurtleHovering(place, state, samus, pseudoScrew, level);
                return;
            case MamaTurtleAction.RisingToPeak:
                RunMamaTurtleRisingToPeak(place, state, samus, pseudoScrew);
                return;
            case MamaTurtleAction.HoveringAtPeak:
                RunMamaTurtleHoveringAtPeak(place, state, samus, pseudoScrew);
                return;
            case MamaTurtleAction.Falling:
                RunMamaTurtleFalling(place, state, samus, pseudoScrew, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Mama Turtle has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Runs the dormant shell's height and carry step.</summary>
    internal static void RunMamaTurtleAsleep(
        RoomEnemyPlace mama,
        SamusState samus)
    {
        mama.Radius = mama.Radius.WithY(0);
        int signedDifference = mama.Position.X.Whole - samus.Position.X.Whole;
        int distance = Math.Abs(signedDifference);
        if (distance >= MamaTurtleShellContourDefinitions.HalfWidth)
            return;

        // The shell's contour on Samus's left differs from its contour on her right; the asymmetry is visible
        // around the dormant shell's lip.
        int contourOffset = MamaTurtleShell.GetOffset(signedDifference);
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
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level,
        int frameCycle)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, pseudoScrew);
        if (!MamaTurtleCadenceDefinitions.LeavingShellStepEverySecondUpdate.IsDue(frameCycle))
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

    /// <summary>Runs the 16-frame upward transition.</summary>
    private void RunMamaTurtleRisingToHover(
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, pseudoScrew);
        if (_terrain.MoveEnemyVertically(level, mama, SignedSixteenSixteen.FromWholePixels(-MamaTurtleMotionDefinitions.RiseToHoverSpeed)))
            return;

        if (EnemyCollision.IsSamusStandingOn(mama, samus))
        {
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, MamaTurtleMotionDefinitions.RiseToHoverSpeed);
        }

        if (!state.ActionTimer.StepEnds())
            return;

        // She hovers off from rest toward Samus's side, speeding up a sixteenth of a pixel each update.
        state.XAcceleration = MamaTurtleMotionDefinitions.HoverAcceleration.Toward(
            mama.Position.X.Whole < samus.Position.X.Whole ? HorizontalDirection.Right : HorizontalDirection.Left);
        state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.Zero);
        state.Action = MamaTurtleAction.Hovering;
    }

    /// <summary>Runs the accelerating horizontal hover, which a wall reverses with a quake and a sound.</summary>
    private void RunMamaTurtleHovering(
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, pseudoScrew);
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
    internal static void AddAndClampMamaTurtleHorizontalAcceleration(
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
    internal static void ReverseMamaTurtleAtWall(MamaTurtleEnemyState state)
    {
        SignedSixteenSixteen velocity = state.Velocity.X;
        state.Velocity = state.Velocity.WithX(MamaTurtleWallReversal.ReversedVelocity(velocity));
        state.XAcceleration = MamaTurtleWallReversal.ReversedAcceleration(state.XAcceleration, velocity);
    }

    /// <summary>Runs the rider-powered seven-pixel ascent.</summary>
    private void RunMamaTurtleRisingToPeak(
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, pseudoScrew);
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
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, pseudoScrew);
        if (state.ActionTimer.StepEnds())
            state.Action = MamaTurtleAction.Falling;
    }

    /// <summary>Runs gravity, collision, and landing-animation selection.</summary>
    private void RunMamaTurtleFalling(
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, pseudoScrew);

        // The cap compares Mama's own fall speed.
        if (state.Velocity.Y < MamaTurtleMotionDefinitions.FastestFall)
        {
            // The one-eighth-pixel fraction step carries into the whole pixels.
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

    /// <summary>Runs the Baby Turtle's main behaviour and its six live actions.</summary>
    private void RunBabyTurtleMain(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case BabyTurtleAction.CrawlingNotCarryingSamus:
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
                return;

            case BabyTurtleAction.HidingCarryingSamus:
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
                return;

            case BabyTurtleAction.HidingNotCarryingSamus:
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
                return;

            case BabyTurtleAction.SpinningUnstoppable:
                MoveSpinningBabyTurtle(baby, state, level);
                return;

            case BabyTurtleAction.SpinningStoppable:
                if (EnemyCollision.IsSamusStandingOn(baby, samus))
                {
                    StartBabyTurtleCrawlingAnimation(baby, state);
                    state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
                    return;
                }
                MoveSpinningBabyTurtle(baby, state, level);
                return;

            case BabyTurtleAction.CrawlingCarryingSamus:
                if (!EnemyCollision.IsSamusStandingOn(baby, samus))
                    state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Baby Turtle has no behaviour for action {state.Action}.");
        }
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

        RoomEnemyPlace parentPlace = state.MamaPlace;
        MamaTurtleEnemyState parent = state.Mama;
        if (!parent.LiesInShell)
            return;

        int signedDifference = parentPlace.Position.X.Whole - baby.Position.X.Whole;
        int distance = Math.Abs(signedDifference);
        int contourOffset = distance >= MamaTurtleShellContourDefinitions.HalfWidth
            ? MamaTurtleMotionDefinitions.BabySinkClearOfShell
            : MamaTurtleShell.GetOffset(signedDifference);
        _terrain.MoveEnemyVertically(level, baby, SignedSixteenSixteen.FromWholePixels(contourOffset));

        if (carryingSamus)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, baby.Position.Y.Whole - previousY);
        }
    }

    /// <summary>Animation cue: turn around at 48 pixels from spawn.</summary>
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
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        if (mama.Contact.HasAny(EnemyContact.SolidToSamus))
            return;
        _collision.ResolveOrdinaryTouch(mama, samus, pseudoScrew);
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
        WakeMamaTurtle(state);
    }

    /// <summary>Finishes the Baby Turtle's reaction to a shot on <paramref name="baby"/>: its mother wakes.</summary>
    private static void ResolveBabyTurtleShotAfterCommon(BabyTurtleEnemyState baby) =>
        WakeMamaTurtle(baby);

    /// <summary>Wakes <paramref name="baby"/>'s Mama after a baby touch or shot reaction.</summary>
    private static void WakeMamaTurtle(BabyTurtleEnemyState baby) => baby.Mama.Wake();

    /// <summary>Runs Mama's expanded private damage box: Samus, while not invincible, touching it is hurt as by any enemy.</summary>
    private void ResolveMamaTurtleExpandedContact(
        RoomEnemyPlace mama,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        if (!MamaTurtleDamageBox.Touches(mama, samus) || !samus.InvincibilityTimer.IsZero)
            return;

        _collision.ResolveOrdinaryTouch(mama, samus, pseudoScrew);
    }

    /// <summary>Starts the crawl animation of the way the child heads.</summary>
    private static void StartBabyTurtleCrawlingAnimation(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state) =>
        StartTurtleAnimation(baby, state, BabyCrawl(state.Heading));

    /// <summary>The baby's crawl heading <paramref name="heading"/>.</summary>
    private static BabyTurtleSegment BabyCrawl(HorizontalDirection heading) =>
        heading == HorizontalDirection.Left ? BabyTurtleSegment.CrawlingLeft : BabyTurtleSegment.CrawlingRight;

    /// <summary>Starts a Mama Turtle animation with its frame timer set to one.</summary>
    private static void StartTurtleAnimation(RoomEnemyPlace place, MamaTurtleEnemyState state, MamaTurtleSegment segment)
    {
        state.Playback = new(MamaTurtleAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Starts a baby turtle animation with its frame timer set to one.</summary>
    private static void StartTurtleAnimation(RoomEnemyPlace place, BabyTurtleEnemyState state, BabyTurtleSegment segment)
    {
        state.Playback = new(BabyTurtleAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs the baby wake reaction after common bomb damage.</summary>
    /// <param name="state">The baby's state, which the reaction acts on.</param>
    internal static EnemyBombReaction BabyTurtleBombReactionOf(BabyTurtleEnemyState state) => new()
    {
        AfterCommon = _ => ResolveBabyTurtleShotAfterCommon(state),
    };

    /// <summary>Runs MamaTurtle's main AI.</summary>
    internal void RunMamaTurtleMainAi(RoomEnemyPlace place, MamaTurtleEnemyState state, IEnemyAiFrame frame)
    {
        RunMamaTurtleMain(
            place,
            state,
            frame.Samus,
            frame.PseudoScrew,
            frame.Level,
            frame.Clock.ShortCounter);
    }

    /// <summary>Runs BabyTurtle's main AI.</summary>
    internal void RunBabyTurtleMainAi(RoomEnemyPlace place, BabyTurtleEnemyState state, IEnemyAiFrame frame)
    {
        RunBabyTurtleMain(place, state, frame.Samus, frame.Level);
    }

    /// <summary>Advances MamaTurtle's typed animation.</summary>
    internal void AdvanceMamaTurtleTypedAnimation(RoomEnemyPlace place, MamaTurtleEnemyState state, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<MamaTurtleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, MamaTurtleAnimations.Segments, ref playback,
            cue => ApplyMamaTurtleCue(state, cue) is { } target
                ? new AnimationContinuation<MamaTurtleSegment>(MamaTurtleAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Advances BabyTurtle's typed animation.</summary>
    internal void AdvanceBabyTurtleTypedAnimation(RoomEnemyPlace place, BabyTurtleEnemyState state, EnemyAnimationCueContext context)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<BabyTurtleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, BabyTurtleAnimations.Segments, ref playback,
            cue => ApplyBabyTurtleCue(place, state, cue, context.Samus, context.Level) is { } target
                ? new AnimationContinuation<BabyTurtleSegment>(BabyTurtleAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Runs the baby wake reaction after common projectile damage.</summary>
    /// <param name="state">The baby's state, which the reaction acts on.</param>
    internal static EnemyShotReaction BabyTurtleShotReactionOf(BabyTurtleEnemyState state) => new()
    {
        AfterCommon = _ => ResolveBabyTurtleShotAfterCommon(state),
    };

    /// <summary>Routes Mama contact to shell and fall handling.</summary>
    /// <param name="context">The touch.</param>
    /// <param name="state">The enemy's own state, which the touch acts on.</param>
    internal void ReactToMamaTurtleTouch(EnemyTouchContext context, MamaTurtleEnemyState state) =>
        ResolveMamaTurtleTouch(context.Place, state, context.Samus, context.PseudoScrew);

    /// <summary>Routes baby contact to separation and wake behavior.</summary>
    /// <param name="context">The touch.</param>
    /// <param name="state">The enemy's own state, which the touch acts on.</param>
    internal void ReactToBabyTurtleTouch(EnemyTouchContext context, BabyTurtleEnemyState state) =>
        ResolveBabyTurtleTouch(context.Place, state, context.Samus, context.Level);
}
