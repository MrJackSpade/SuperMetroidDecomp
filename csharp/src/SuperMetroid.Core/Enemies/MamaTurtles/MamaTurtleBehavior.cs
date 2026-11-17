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
internal sealed class MamaTurtleBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.BabyTurtle, EnemyDefinitionId.MamaTurtle];

    /// <summary>The mama turtle, or one of her babies (#2146).</summary>
    /// <exception cref="InvalidOperationException">The place holds no mama turtle definition.</exception>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common)
    {
        EnemyDefinitionId definition = Ensure.NotNull(place).DefinitionId;
        if (definition == EnemyDefinitionId.MamaTurtle)
            return new MamaTurtleEntity(this, place, common);
        if (definition == EnemyDefinitionId.BabyTurtle)
            return new BabyTurtleEntity(this, place, common);
        throw new InvalidOperationException($"The mama turtle species creates no {place.DefinitionId}.");
    }

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

    /// <summary>Connects the tatori family to room terrain, collision, places, and earthquake output.</summary>
    internal MamaTurtleBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, EnemyPlaces places, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _earthquake = earthquake;
        _places = places;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Mama Turtle's animations; rising to hover continues at its spin.</summary>
    private MamaTurtleSegment? ApplyMamaTurtleCue(RoomEnemyPlace place, MamaTurtleCue cue)
    {
        if (cue is MamaTurtleCue.RiseToHover rise)
        {
            StartMamaTurtleRisingToHover(place.RequireSpeciesState<MamaTurtleEnemyState>(), direction: rise.Direction);
            return MamaTurtleSegment.MamaSpinning;
        }
        MamaTurtleCueKind kind = ((MamaTurtleCue.Signal)cue).Kind;
        switch (kind)
        {
            case MamaTurtleCueKind.EnterShell:
                StartMamaTurtleEnteringShell(place.RequireSpeciesState<MamaTurtleEnemyState>());
                return null;
            case MamaTurtleCueKind.PlaySpinningSound:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            case MamaTurtleCueKind.Crawl:
            case MamaTurtleCueKind.LeaveShell:
            case MamaTurtleCueKind.LeftShell:
            case MamaTurtleCueKind.LoopOrTurnAround:
            case MamaTurtleCueKind.SetSpinningStoppable:
            default:
                throw new InvalidOperationException($"The Mama Turtle's animations have no {kind} cue.");
        }
    }

    /// <summary>
    /// Carries out one cue of a baby turtle's animations. The crawl loop, leaving the shell while ridden and the end of
    /// leaving it continue at the animation their cues return; every other cue runs on.
    /// </summary>
    private MamaTurtleSegment? ApplyBabyTurtleCue(RoomEnemyPlace place, MamaTurtleCue cue, SamusState samus, RoomLevelData level)
    {
        if (cue is not MamaTurtleCue.Signal { Kind: var kind })
            throw new InvalidOperationException($"A baby turtle's animations have no {cue} cue.");
        switch (kind)
        {
            case MamaTurtleCueKind.Crawl:
                ProcessBabyTurtleCrawlAnimation(place, place.RequireSpeciesState<BabyTurtleEnemyState>(), samus, level);
                return null;
            case MamaTurtleCueKind.LoopOrTurnAround:
                return SelectBabyTurtleCrawlLoop(place, place.RequireSpeciesState<BabyTurtleEnemyState>());
            case MamaTurtleCueKind.LeaveShell:
                return SelectBabyTurtleLeaveShell(place, samus);
            case MamaTurtleCueKind.LeftShell:
                return FinishBabyTurtleLeavingShell(place, place.RequireSpeciesState<BabyTurtleEnemyState>(), samus);
            case MamaTurtleCueKind.SetSpinningStoppable:
                place.RequireSpeciesState<BabyTurtleEnemyState>().Action = BabyTurtleAction.SpinningStoppable;
                return null;
            case MamaTurtleCueKind.PlaySpinningSound:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            case MamaTurtleCueKind.EnterShell:
            default:
                throw new InvalidOperationException($"A baby turtle's animations have no {kind} cue.");
        }
    }

    /// <summary>Initializes the Mama Turtle and links her to the four babies her room lists right after her.</summary>
    internal void InitializeMamaTurtle(RoomEnemyPlace place)
    {
        RoomEnemyPlace[] babies = _places.ListedRunAfter(place, MamaTurtleMotionDefinitions.BabyCount);
        _babiesOfMama.Place(place, babies);
        foreach (RoomEnemyPlace baby in babies)
            _mamaOfBaby.Place(baby, place);

        var state = new MamaTurtleEnemyState(MamaTurtleAction.Initial)
        {
            Asleep = true,
        };
        place.SetSpeciesState(state);

        // The population already supplies the animating property, and the setup sets it again; leaving the
        // shell needs the animation to run.
        place.Processing = place.Processing.With(EnemyProcessing.Animates);
        place.Frame = EnemyFrameId.Empty;
        place.AnimationClock.Restart();
        place.Radius = place.Radius.WithY(0);
        state.Playback = new(MamaTurtleAnimations.StartingWith(MamaTurtleSegment.MamaAsleep), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Initializes the Baby Turtle.</summary>
    internal static void InitializeBabyTurtle(RoomEnemyPlace place)
    {
        var state = new BabyTurtleEnemyState(BabyTurtleAction.CrawlingNotCarryingSamus)
        {
            SpawnTopCenter = new Position16<RoomPixels>(
                place.Position.X.Whole, CoordinateAxis.Wrap(place.Position.Y.Whole - place.Radius.Y)),
            Heading = place.SpawnSettings<BabyTurtleSpawnSettings>().Heading,
            HorizontalSpeed = MamaTurtleMotionDefinitions.BabyCrawlSpeed,
        };
        place.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(place, MamaTurtleAnimations.StartingWith(BabyCrawl(state.Heading)));
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
            case MamaTurtleAction.Initial:
                LinkMamaTurtleChildren(place, state);
                return;
            case MamaTurtleAction.Idle:
                return;
            case MamaTurtleAction.Asleep:
                RunMamaTurtleAsleep(place, state, samus);
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

    /// <summary>Runs the first-frame parent/child setup on the four babies Mama found when she was placed.</summary>
    private void LinkMamaTurtleChildren(RoomEnemyPlace mama, MamaTurtleEnemyState state)
    {
        RoomEnemyPlace[] babies = _babiesOfMama.Of(mama);
        foreach (RoomEnemyPlace child in babies)
        {
            if (child.DefinitionId != EnemyDefinitionId.BabyTurtle)
            {
                throw new InvalidDataException(
                    $"Mama Turtle place {mama.Ordinal} expected Baby Turtle in following " +
                    $"place {child.Ordinal}, found {child.DefinitionId}.");
            }

            _ = child.RequireSpeciesState<BabyTurtleEnemyState>();
        }

        state.Action = MamaTurtleAction.Asleep;
    }

    /// <summary>Runs the dormant shell's height and carry step.</summary>
    internal static void RunMamaTurtleAsleep(
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (!state.Asleep)
        {
            state.Action = MamaTurtleAction.LeavingShell;
            mama.Contact = mama.Contact.Without(EnemyContact.IgnoresSamus);
            return;
        }

        mama.Radius = mama.Radius.WithY(0);
        int signedDifference = CoordinateAxis.Difference(mama.Position.X.Whole, samus.Position.X.Whole);
        int distance = Math.Abs(signedDifference);
        if (distance >= MamaTurtleShellContourDefinitions.HalfWidth)
            return;

        // The shell's contour on Samus's left differs from its contour on her right; the asymmetry is visible
        // around the dormant shell's lip.
        int contourOffset = MamaTurtleShell.GetOffset(signedDifference);
        mama.Radius = mama.Radius.WithY(CoordinateAxis.Wrap(-contourOffset));
        mama.Contact = mama.Contact.With(EnemyContact.SolidToSamus);

        if (!EnemyCollision.IsSamusStandingOn(mama, samus))
            return;

        int shellTop = CoordinateAxis.Wrap(mama.Position.Y.Whole - mama.Radius.Y);
        int overlap = CoordinateAxis.Wrap(samus.Kinematics.BottomBoundary - shellTop);
        if (CoordinateAxis.AsSigned(overlap) < 0)
            return;

        samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, CoordinateAxis.AsSigned(overlap));
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

        mama.Position = mama.Position.WithWrappedWholeY(mama.Position.Y.Whole - MamaTurtleMotionDefinitions.LeavingShellClimb);
        mama.Radius = mama.Radius.WithY(MamaTurtleMotionDefinitions.AwakeHalfHeight);
        mama.Position = mama.Position.WithWholeX(MamaTurtleCadenceDefinitions.LeavingShellSidestepsLeft.IsSetIn(mama.Position.Y.Whole)
            ? CoordinateAxis.Wrap(mama.Position.X.Whole - 1)
            : CoordinateAxis.Wrap(mama.Position.X.Whole + 1));

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, SignedSixteenSixteen.FromWholePixels(1)))
            return;

        StartTurtleAnimation(
            mama,
            state,
            MamaTurtleSegment.MamaLeaveShellLeft);

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
            CoordinateAxis.Difference(mama.Position.X.Whole, samus.Position.X.Whole) >= 0
                ? MamaTurtleSegment.MamaEnterShellLeft
                : MamaTurtleSegment.MamaEnterShellRight);
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

        if (--state.ActionTimer != 0)
            return;

        // The two adjacent accelerations accelerate toward Samus's side by a sixteenth of a pixel per update.
        state.XAcceleration = CoordinateAxis.Difference(mama.Position.X.Whole, samus.Position.X.Whole) < 0
            ? MamaTurtleMotionDefinitions.HoverAcceleration
            : -MamaTurtleMotionDefinitions.HoverAcceleration;
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
            _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Horizontal, EarthquakeReach.Foreground),
                MamaTurtleMotionDefinitions.WallQuakeDuration);
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
        int oldWhole = velocity.WholePixels;
        int newWhole = sum.WholePixels;

        int cappedWhole = Math.Abs(newWhole) < MamaTurtleMotionDefinitions.FastestHover
            ? newWhole
            // Chooses the cap's sign from the old velocity, before storing the
            // just-computed result. Preserve that one-frame zero-crossing behavior.
            : oldWhole < 0
                ? -MamaTurtleMotionDefinitions.FastestHover
                : MamaTurtleMotionDefinitions.FastestHover;
        state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.FromParts(cappedWhole, sum.Fraction));
    }

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
        if (CoordinateAxis.Difference(mama.Position.Y.Whole, MamaTurtleMotionDefinitions.RiddenAscentPeak) < 0)
        {
            state.ActionTimer = MamaTurtleMotionDefinitions.PeakPause;
            state.Action = MamaTurtleAction.HoveringAtPeak;
            state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.Zero);
            return;
        }

        if (EnemyCollision.IsSamusStandingOn(mama, samus))
        {
            mama.Position = mama.Position.WithWrappedWholeY(mama.Position.Y.Whole - MamaTurtleMotionDefinitions.RiddenAscentSpeed);
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, MamaTurtleMotionDefinitions.RiddenAscentSpeed);
            return;
        }

        state.Action = MamaTurtleAction.Falling;
        state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.Zero);
    }

    /// <summary>Runs the 30-frame peak delay.</summary>
    private void RunMamaTurtleHoveringAtPeak(
        RoomEnemyPlace mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        ResolveMamaTurtleExpandedContact(mama, samus, pseudoScrew);
        if (--state.ActionTimer == 0)
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
        if (state.Velocity.Y.WholePixels < MamaTurtleMotionDefinitions.FastestFall)
        {
            // The one-eighth-pixel fraction step carries into the whole pixels.
            state.Velocity = state.Velocity.WithY(
                state.Velocity.Y + MamaTurtleMotionDefinitions.FallAcceleration);
        }

        // Only the whole part is passed to the downward move; the fraction influences later
        // whole-speed carries but is not itself part of this frame's displacement.
        if (!_terrain.MoveEnemyVertically(level, mama, state.Velocity.Y.WithoutFraction()))
            return;

        StartTurtleAnimation(
            mama,
            state,
            state.Velocity.X.WholePixels < 0
                ? MamaTurtleSegment.MamaLeaveShellLeft
                : MamaTurtleSegment.MamaLeaveShellRight);
        state.Action = MamaTurtleAction.Idle;
    }

    /// <summary>Runs the Baby Turtle's main behaviour and its six live actions.</summary>
    private void RunBabyTurtleMain(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The prologue requires the child's parent to be a Mama Turtle. The game also clears and republishes a
        // carried-height value on the parent here, which nothing reads.
        _ = RequireMamaTurtleParent(baby);
        switch (state.Action)
        {
            case BabyTurtleAction.CrawlingNotCarryingSamus:
                if (EnemyCollision.IsSamusStandingOn(baby, samus))
                {
                    state.Action = BabyTurtleAction.HidingCarryingSamus;
                    state.NotCarryingSamusReactionTimer = MamaTurtleMotionDefinitions.BabyRiderLeftDelay;
                    StartTurtleAnimation(
                        baby,
                        state,
                        state.Heading == HorizontalDirection.Left
                            ? MamaTurtleSegment.BabyHidingLeft
                            : MamaTurtleSegment.BabyHidingRight);
                }
                return;

            case BabyTurtleAction.HidingCarryingSamus:
                if (EnemyCollision.IsSamusStandingOn(baby, samus))
                {
                    state.NotCarryingSamusReactionTimer = MamaTurtleMotionDefinitions.BabyRiderLeftDelay;
                    return;
                }
                if (--state.NotCarryingSamusReactionTimer == 0)
                {
                    state.Action = BabyTurtleAction.HidingNotCarryingSamus;
                    state.ActionTimer = MamaTurtleMotionDefinitions.BabyHideDuration;
                }
                return;

            case BabyTurtleAction.HidingNotCarryingSamus:
                if (EnemyCollision.IsSamusStandingOn(baby, samus))
                {
                    state.Action = BabyTurtleAction.SpinningUnstoppable;
                    StartTurtleAnimation(
                        baby,
                        state,
                        MamaTurtleSegment.BabySpinning);
                    // The baby spins off the way Samus faces (#2338).
                    state.Heading = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Right ? HorizontalDirection.Right : HorizontalDirection.Left;
                    state.HorizontalSpeed = MamaTurtleMotionDefinitions.BabySpinVelocity.X;
                    state.SpinFallSpeed = MamaTurtleMotionDefinitions.BabySpinVelocity.Y;
                    return;
                }
                if (--state.ActionTimer == 0)
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

        RoomEnemyPlace parentPlace = MamaOf(baby);
        MamaTurtleEnemyState parent = parentPlace.RequireSpeciesState<MamaTurtleEnemyState>();
        if (parent.Action != MamaTurtleAction.Asleep)
            return;

        int signedDifference = CoordinateAxis.Difference(parentPlace.Position.X.Whole, baby.Position.X.Whole);
        int distance = Math.Abs(signedDifference);
        int contourOffset = distance >= MamaTurtleShellContourDefinitions.HalfWidth
            ? 1
            : MamaTurtleShell.GetOffset(signedDifference);
        _terrain.MoveEnemyVertically(level, baby, SignedSixteenSixteen.FromWholePixels(contourOffset));

        if (carryingSamus)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(0, CoordinateAxis.Difference(baby.Position.Y.Whole, previousY));
        }
    }

    /// <summary>Animation cue: turn around at 48 pixels from spawn.</summary>
    private static MamaTurtleSegment SelectBabyTurtleCrawlLoop(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state)
    {
        int signedDifference = CoordinateAxis.Difference(state.SpawnTopCenter.X, baby.Position.X.Whole);
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
        state.Velocity = state.Velocity.WithWholeX(direction == HorizontalDirection.Right
            ? MamaTurtleMotionDefinitions.RiseToHoverDrift
            : -MamaTurtleMotionDefinitions.RiseToHoverDrift);
        state.ActionTimer = MamaTurtleMotionDefinitions.RiseToHoverDuration;
    }

    /// <summary>
    /// Animation cue: branch out of hiding only while ridden, whichever way the baby crawls (#2337); otherwise the hiding
    /// animation runs on.
    /// </summary>
    private static MamaTurtleSegment? SelectBabyTurtleLeaveShell(RoomEnemyPlace baby, SamusState samus) =>
        EnemyCollision.IsSamusStandingOn(baby, samus) ? MamaTurtleSegment.BabyLeaveShell : null;

    /// <summary>Animation cue: restore crawling after leaving the shell.</summary>
    private static MamaTurtleSegment FinishBabyTurtleLeavingShell(
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
        _collision.ResolveNormalEnemyTouch(mama, samus, pseudoScrew);
        state.Action = MamaTurtleAction.Falling;
        state.Velocity = state.Velocity.WithWholeY(MamaTurtleMotionDefinitions.KnockedDownFallSpeed);
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

        baby.Position = baby.Position.WithWholeX(CoordinateAxis.Difference(baby.Position.X.Whole, samus.Position.X.Whole) >= 0
            ? CoordinateAxis.Wrap(samus.Position.X.Whole + samus.Kinematics.Radius.X + baby.Radius.X)
            : CoordinateAxis.Wrap(samus.Position.X.Whole - samus.Kinematics.Radius.X - baby.Radius.X));
        state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.X));
        WakeMamaTurtle(baby);
    }

    /// <summary>Private tail of the Baby Turtle's shot AI on <paramref name="baby"/>.</summary>
    private void ResolveBabyTurtleShotAfterCommon(RoomEnemyPlace baby) =>
        WakeMamaTurtle(baby);

    /// <summary>Wakes <paramref name="baby"/>'s Mama after a baby touch or shot reaction.</summary>
    private void WakeMamaTurtle(RoomEnemyPlace baby)
    {
        MamaTurtleEnemyState parent = RequireMamaTurtleParent(baby);
        parent.Asleep = false;
    }

    /// <summary>Runs Mama's expanded private damage box.</summary>
    private void ResolveMamaTurtleExpandedContact(
        RoomEnemyPlace mama,
        SamusState samus,
        PseudoScrewCharge pseudoScrew)
    {
        // The box is Mama's body grown on each side and shrunk at the top and bottom.
        int left = CoordinateAxis.Wrap(
            mama.Position.X.Whole - mama.Radius.X - MamaTurtleMotionDefinitions.DamageBoxReach);
        int right = CoordinateAxis.Wrap(
            mama.Position.X.Whole + mama.Radius.X + MamaTurtleMotionDefinitions.DamageBoxReach);
        int top = CoordinateAxis.Wrap(
            mama.Position.Y.Whole - mama.Radius.Y + MamaTurtleMotionDefinitions.DamageBoxInset);
        int bottom = CoordinateAxis.Wrap(
            mama.Position.Y.Whole + mama.Radius.Y - MamaTurtleMotionDefinitions.DamageBoxInset);

        int samusLeftMinusOne = CoordinateAxis.Wrap(
            samus.Position.X.Whole - samus.Kinematics.Radius.X - 1);
        if (CoordinateAxis.Difference(samusLeftMinusOne, right) >= 0)
            return;
        int samusRight = CoordinateAxis.Wrap(
            samus.Position.X.Whole + samus.Kinematics.Radius.X);
        if (CoordinateAxis.Difference(samusRight, left) < 0)
            return;
        int samusTopPlusOne = CoordinateAxis.Wrap(
            samus.Position.Y.Whole - samus.Kinematics.Radius.Y + 1);
        if (CoordinateAxis.Difference(samusTopPlusOne, bottom) >= 0)
            return;
        int samusBottom = CoordinateAxis.Wrap(
            samus.Position.Y.Whole + samus.Kinematics.Radius.Y);
        if (CoordinateAxis.Difference(samusBottom, top) < 0 || !samus.InvincibilityTimer.IsZero)
            return;

        _collision.ResolveNormalEnemyTouch(mama, samus, pseudoScrew);
    }

    /// <summary>The state of <paramref name="baby"/>'s Mama, rejecting a place no longer holding a Mama Turtle.</summary>
    private MamaTurtleEnemyState RequireMamaTurtleParent(RoomEnemyPlace baby)
    {
        RoomEnemyPlace parent = MamaOf(baby);
        if (parent.DefinitionId != EnemyDefinitionId.MamaTurtle)
        {
            throw new InvalidDataException(
                $"Baby Turtle parent place {parent.Ordinal} names enemy " +
                $"{parent.DefinitionId}, not Mama Turtle.");
        }
        return parent.RequireSpeciesState<MamaTurtleEnemyState>();
    }

    /// <summary>The place of <paramref name="baby"/>'s Mama, who linked it when she was placed.</summary>
    /// <exception cref="InvalidOperationException">No Mama Turtle found <paramref name="baby"/> as she was placed.</exception>
    private RoomEnemyPlace MamaOf(RoomEnemyPlace baby) => _mamaOfBaby.Of(baby);

    /// <summary>Starts the crawl animation of the way the child heads.</summary>
    private static void StartBabyTurtleCrawlingAnimation(
        RoomEnemyPlace baby,
        BabyTurtleEnemyState state) =>
        StartTurtleAnimation(baby, state, BabyCrawl(state.Heading));

    /// <summary>The baby's crawl heading <paramref name="heading"/>.</summary>
    private static MamaTurtleSegment BabyCrawl(HorizontalDirection heading) =>
        heading == HorizontalDirection.Left ? MamaTurtleSegment.BabyCrawlingLeft : MamaTurtleSegment.BabyCrawlingRight;

    /// <summary>Starts a Mama Turtle animation with its frame timer set to one.</summary>
    private static void StartTurtleAnimation(RoomEnemyPlace place, MamaTurtleEnemyState state, MamaTurtleSegment segment)
    {
        state.Playback = new(MamaTurtleAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Starts a baby turtle animation with its frame timer set to one.</summary>
    private static void StartTurtleAnimation(RoomEnemyPlace place, BabyTurtleEnemyState state, MamaTurtleSegment segment)
    {
        state.Playback = new(MamaTurtleAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs the baby wake reaction after common bomb damage.</summary>
    internal EnemyBombReaction BabyTurtleBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveBabyTurtleShotAfterCommon(c.Enemy),
    };

    /// <summary>Runs MamaTurtle's main AI.</summary>
    internal void RunMamaTurtleMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMamaTurtleMain(
            place,
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
    internal void AdvanceMamaTurtleTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
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
        AnimationPlayback<MamaTurtleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, MamaTurtleAnimations.Segments, ref playback,
            cue => ApplyBabyTurtleCue(place, cue, context.Samus, context.Level) is { } target
                ? new AnimationContinuation<MamaTurtleSegment>(MamaTurtleAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
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
        ResolveMamaTurtleTouch(context.Place, context.Place.RequireSpeciesState<MamaTurtleEnemyState>(), context.Samus, context.PseudoScrew);

    /// <summary>Routes baby contact to separation and wake behavior.</summary>
    internal void ReactToBabyTurtleTouch(EnemyTouchContext context) =>
        ResolveBabyTurtleTouch(context.Place, context.Place.RequireSpeciesState<BabyTurtleEnemyState>(), context.Samus, context.Level);
}
