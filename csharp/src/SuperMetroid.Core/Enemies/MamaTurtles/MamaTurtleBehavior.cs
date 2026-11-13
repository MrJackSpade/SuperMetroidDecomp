using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Programs;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Enemies.Platforms;
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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. MamaTurtlePartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MamaTurtleEntity(this, slot, common);

    /// <summary>Moves parent and children against room blocks during hover, fall, and crawling.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Records the wall-impact shake from Mama Turtle's hover collision.</summary>
    private readonly RoomEarthquake _earthquake;
    /// <summary>The room's enemy places, where Mama finds her four babies when she is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>
    /// The room's Mama Turtle, the parent of every Baby Turtle: the first enemy her room lists, which her placement
    /// requires; null until she is placed.
    /// </summary>
    private RoomEnemySlot? _mama;
    /// <summary>The four babies Mama found when she was placed, which her room lists right after her; null until then.</summary>
    private RoomEnemySlot[]? _babies;
    /// <summary>Applies touch damage for Mama's expanded body or exposed shell.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Connects the tatori family to room terrain, collision, slots, and earthquake output.</summary>
    internal MamaTurtleBehavior(EnemyTerrain terrain, RoomEarthquake earthquake, EnemySlotTable slotTable, EnemyCollision collision, EnemyAudioQueue audio)
    {
        _audio = audio;
        _terrain = terrain;
        _earthquake = earthquake;
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Carries out one cue of the Mama Turtle's animations; rising to hover continues at its spin.</summary>
    private MamaTurtleSegment? ApplyMamaTurtleCue(RoomEnemySlot slot, MamaTurtleCue cue)
    {
        switch (cue.Kind)
        {
            case MamaTurtleCueKind.EnterShell:
                StartMamaTurtleEnteringShell(slot.RequireSpeciesState<MamaTurtleEnemyState>());
                return null;
            case MamaTurtleCueKind.RiseToHoverRightwards:
                StartMamaTurtleRisingToHover(slot.RequireSpeciesState<MamaTurtleEnemyState>(), direction: HorizontalDirection.Right);
                return MamaTurtleSegment.MamaSpinning;
            case MamaTurtleCueKind.RiseToHoverLeftwards:
                StartMamaTurtleRisingToHover(slot.RequireSpeciesState<MamaTurtleEnemyState>(), direction: HorizontalDirection.Left);
                return MamaTurtleSegment.MamaSpinning;
            case MamaTurtleCueKind.PlaySpinningSound:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            case MamaTurtleCueKind.Crawl:
            case MamaTurtleCueKind.LeaveShell:
            case MamaTurtleCueKind.LeftShell:
            case MamaTurtleCueKind.LoopOrTurnAround:
            case MamaTurtleCueKind.SetSpinningStoppable:
            default:
                throw new InvalidOperationException($"The Mama Turtle's animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>
    /// Carries out one cue of a baby turtle's animations. The crawl loop, leaving the shell while ridden and the end of
    /// leaving it continue at the animation their callbacks return; every other cue runs on.
    /// </summary>
    private MamaTurtleSegment? ApplyBabyTurtleCue(RoomEnemySlot slot, MamaTurtleCue cue, SamusState? samus, RoomLevelData? level)
    {
        switch (cue.Kind)
        {
            case MamaTurtleCueKind.Crawl:
                ProcessBabyTurtleCrawlAnimation(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>(), samus, level);
                return null;
            case MamaTurtleCueKind.LoopOrTurnAround:
                return SelectBabyTurtleCrawlLoop(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>());
            case MamaTurtleCueKind.LeaveShell:
                return SelectBabyTurtleLeaveShell(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>(), samus);
            case MamaTurtleCueKind.LeftShell:
                return FinishBabyTurtleLeavingShell(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>(), samus);
            case MamaTurtleCueKind.SetSpinningStoppable:
                slot.RequireSpeciesState<BabyTurtleEnemyState>().Action = BabyTurtleAction.SpinningStoppable;
                return null;
            case MamaTurtleCueKind.PlaySpinningSound:
                _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleSpinSound, priority: SoundRequestPriority.Normal);
                return null;
            case MamaTurtleCueKind.EnterShell:
            case MamaTurtleCueKind.RiseToHoverLeftwards:
            case MamaTurtleCueKind.RiseToHoverRightwards:
            default:
                throw new InvalidOperationException($"A baby turtle's animations have no {cue.Kind} cue.");
        }
    }

    /// <summary>Maximum crawl distance from a baby's spawn before its animation reverses direction.</summary>
    private const int BabyTurtleTravelDistance = 48;
    /// <summary>Upper room Y where Mama pauses after a rider-powered ascent.</summary>
    private const int MamaTurtlePeakYPosition = 488;
    /// <summary>Whole pixels climbed per tick while Samus rides Mama toward the peak.</summary>
    private const int MamaTurtleRisingSpeed = 7;
    /// <summary>Ticks held at the ascent peak before Mama begins falling.</summary>
    private const int MamaTurtlePeakPauseFrames = 30;
    /// <summary>Whole-speed cap the fall compares Mama's own vertical speed with.</summary>
    private const int MamaTurtleMaximumFallingSpeed = 4;
    /// <summary>Whole-speed cap for horizontal hover acceleration.</summary>
    private const int MamaTurtleMaximumHoveringSpeed = 3;
    /// <summary>How many Baby Turtles crawl on Mama's shell.</summary>
    private const int BabyTurtleCount = 4;

    /// <summary>Initializes the Mama Turtle.</summary>
    /// <exception cref="InvalidDataException">Mama is not in the first enemy slot.</exception>
    internal void InitializeMamaTurtle(RoomEnemySlot slot)
    {
        // The fall's speed cap and the expanded contact box read the first enemy slot's values, not the
        // current enemy's, and every baby's parent is the first enemy. The one retail Mama Turtle population places
        // Mama in that slot, so they are her own; anywhere else she would take another enemy's speed and bounds, which
        // no retail room reaches.
        if (!slot.IsListedFirst())
        {
            throw new InvalidDataException(
                $"Mama Turtle occupies enemy slot {slot.SlotIndex}; her fall and contact box read the first slot, " +
                "where the one retail Mama Turtle population places her.");
        }
        _mama = slot;
        _babies = _slotTable.ListedRunAfter(slot, BabyTurtleCount);

        var state = new MamaTurtleEnemyState
        {
            Action = MamaTurtleAction.Initial,
            AsleepFlag = 1,
        };
        slot.SetSpeciesState(state);

        // The population already supplies the animating property, but the initializer explicitly sets it again.
        // Spell that write out because animation processing is essential to leaving shell.
        slot.Processing = slot.Processing.With(EnemyProcessing.Animates);
        slot.Frame = EnemyFrameId.Empty;
        slot.AnimationClock.Restart();
        slot.Radius = slot.Radius.WithY(0);
        state.Playback = new(MamaTurtleAnimations.StartingWith(MamaTurtleSegment.MamaAsleep), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Initializes the Baby Turtle.</summary>
    internal static void InitializeBabyTurtle(RoomEnemySlot slot)
    {
        var state = new BabyTurtleEnemyState
        {
            SpawnTopCenter = new Position16<RoomPixels>(
                slot.Position.X.Whole, CoordinateAxis.Wrap(slot.Position.Y.Whole - slot.Radius.Y)),
            Action = BabyTurtleAction.CrawlingNotCarryingSamus,
            Velocity = new Offset16<RoomPixels>(slot.SpawnSettings<TurtleSpawnSettings>().Speed, 0),
        };
        slot.SetSpeciesState(state);

        state.Playback = EnemyAnimationPlayback.Start(slot, MamaTurtleAnimations.StartingWith(slot.SpawnSettings<TurtleSpawnSettings>().Speed < 0
            ? MamaTurtleSegment.BabyCrawlingLeft
            : MamaTurtleSegment.BabyCrawlingRight));
    }

    /// <summary>Runs the Mama Turtle's main behaviour and its indirect functions.</summary>
    private void RunMamaTurtleMain(
        RoomEnemySlot slot,
        MamaTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level,
        int frameCycle)
    {
        if (samus is null)
            throw new InvalidOperationException("Mama Turtle AI requires the active Samus actor.");

        switch (state.Action)
        {
            case MamaTurtleAction.Initial:
                LinkMamaTurtleChildren(slot, state);
                return;
            case MamaTurtleAction.Idle:
                return;
            case MamaTurtleAction.Asleep:
                RunMamaTurtleAsleep(slot, state, samus);
                return;
            case MamaTurtleAction.LeavingShell:
                RequireTurtleLevel(level);
                RunMamaTurtleLeavingShell(
                    slot, state, samus, level, frameCycle);
                return;
            case MamaTurtleAction.EnteringShell:
                RunMamaTurtleEnteringShell(slot, state, samus);
                return;
            case MamaTurtleAction.RisingToHover:
                RequireTurtleLevel(level);
                RunMamaTurtleRisingToHover(slot, state, samus, level);
                return;
            case MamaTurtleAction.Hovering:
                RequireTurtleLevel(level);
                RunMamaTurtleHovering(slot, state, samus, level);
                return;
            case MamaTurtleAction.RisingToPeak:
                RunMamaTurtleRisingToPeak(slot, state, samus);
                return;
            case MamaTurtleAction.HoveringAtPeak:
                RunMamaTurtleHoveringAtPeak(slot, state, samus);
                return;
            case MamaTurtleAction.Falling:
                RequireTurtleLevel(level);
                RunMamaTurtleFalling(slot, state, samus, level);
                return;
            default:
                throw new InvalidDataException(
                    $"Mama Turtle main-AI function {state.Action?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Runs the first-frame parent/child setup on the four babies Mama found when she was placed.</summary>
    private void LinkMamaTurtleChildren(RoomEnemySlot mama, MamaTurtleEnemyState state)
    {
        RoomEnemySlot[] babies = _babies ?? throw new InvalidOperationException("Mama Turtle links her babies before she was placed.");
        foreach (RoomEnemySlot child in babies)
        {
            if (child.DefinitionId != EnemyDefinitionId.BabyTurtle)
            {
                throw new InvalidDataException(
                    $"Mama Turtle slot {mama.SlotIndex} expected Baby Turtle in following " +
                    $"slot {child.SlotIndex}, found {child.DefinitionId}.");
            }

            _ = child.RequireSpeciesState<BabyTurtleEnemyState>();
        }

        state.Action = MamaTurtleAction.Asleep;
    }

    /// <summary>Runs the sleeping shell's height and carry step.</summary>
    internal static void RunMamaTurtleAsleep(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (state.AsleepFlag == 0)
        {
            state.Action = MamaTurtleAction.LeavingShell;
            mama.Contact = mama.Contact.Without(EnemyContact.IgnoresSamus);
            return;
        }

        mama.Radius = mama.Radius.WithY(0);
        int signedDifference = CoordinateAxis.Difference(mama.Position.X.Whole, samus.Position.X.Whole);
        int distance = Math.Abs(signedDifference);
        if (distance >= 24)
            return;

        // The first 24 contour entries describe Samus to the left; the second 24 describe
        // Samus to the right. This asymmetry is visible around the sleeping shell's lip.
        int contourOffset = MamaTurtleShell.GetOffset(signedDifference);
        mama.Radius = mama.Radius.WithY(CoordinateAxis.Wrap(-contourOffset));
        mama.Contact = mama.Contact.With(EnemyContact.SolidToSamus);

        if (!PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
            return;

        int shellTop = CoordinateAxis.Wrap(mama.Position.Y.Whole - mama.Radius.Y);
        int overlap = CoordinateAxis.Wrap(samus.Kinematics.BottomBoundary - shellTop);
        if (CoordinateAxis.AsSigned(overlap) < 0)
            return;

        samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, CoordinateAxis.AsSigned(overlap));
    }

    /// <summary>Runs the deliberately jittery shell-opening step.</summary>
    private void RunMamaTurtleLeavingShell(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level,
        int frameCycle)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (!MamaTurtleCadenceDefinitions.LeavingShellStepEverySecondUpdate.IsDue(frameCycle))
            return;

        if (PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
        {
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(1, 0);
        }

        mama.Position = mama.Position.WithWrappedWholeY(mama.Position.Y.Whole - 1);
        mama.Radius = mama.Radius.WithY(16);
        mama.Position = mama.Position.WithWholeX(MamaTurtleCadenceDefinitions.LeavingShellSidestepsLeft.IsSetIn(mama.Position.Y.Whole)
            ? CoordinateAxis.Wrap(mama.Position.X.Whole - 1)
            : CoordinateAxis.Wrap(mama.Position.X.Whole + 1));

        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, SignedSixteenSixteen.FromWholePixels(1)))
            return;

        StartTurtleAnimation(
            mama,
            state,
            MamaTurtleSegment.MamaLeaveShellLeft);

        // The game's store at this point does not reach the enemy, so nothing is stored here; no enemy field is
        // invented for it.
        state.Action = MamaTurtleAction.Idle;
    }

    /// <summary>Runs the facing-dependent return-to-shell selector.</summary>
    private static void RunMamaTurtleEnteringShell(
        RoomEnemySlot mama,
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
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (_terrain.MoveEnemyVertically(level, mama, SignedSixteenSixteen.FromWholePixels(-1)))
            return;

        if (PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
        {
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, 1);
        }

        if (--state.FunctionTimer != 0)
            return;

        // The two adjacent acceleration records accelerate toward Samus's side by a sixteenth of a pixel per update.
        state.XAcceleration = CoordinateAxis.Difference(mama.Position.X.Whole, samus.Position.X.Whole) < 0
            ? SignedSixteenSixteen.FromSixteenthsOfPixel(1)
            : SignedSixteenSixteen.FromSixteenthsOfPixel(-1);
        state.Velocity = state.Velocity.WithX(SignedSixteenSixteen.Zero);
        state.Action = MamaTurtleAction.Hovering;
    }

    /// <summary>Runs accelerating horizontal hover and its original wall bug.</summary>
    private void RunMamaTurtleHovering(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, mama, state.Velocity.X))
        {
            ReverseMamaTurtleAtWall(state);
            _earthquake.Start(new EarthquakeShake(EarthquakeStrength.Light, EarthquakeDirection.Horizontal, EarthquakeReach.Foreground), 16);
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.MamaTurtleWallSound, priority: SoundRequestPriority.Normal);
            return;
        }

        HandleSamusLandingOnMamaTurtle(mama, state, samus);
        AddAndClampMamaTurtleHorizontalAcceleration(state);
    }

    /// <summary>Handles Samus landing on the hovering Mama Turtle.</summary>
    private static void HandleSamusLandingOnMamaTurtle(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (!PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
            return;

        state.Action = MamaTurtleAction.RisingToPeak;
        SignedSixteenSixteen result = samus.Kinematics.ExtraDisplacement.X - state.Velocity.X;
        samus.Kinematics.ExtraDisplacement = samus.Kinematics.ExtraDisplacement.WithX(result.WholePixels < -16 ? result.WithWholePixels(-16) : result);
    }

    /// <summary>Adds signed 16.16 hover acceleration as one 16.16 sum and caps whole speed using the old velocity's sign.</summary>
    internal static void AddAndClampMamaTurtleHorizontalAcceleration(
        MamaTurtleEnemyState state)
    {
        SignedSixteenSixteen velocity = state.Velocity.X;
        // One 16.16 add: the fraction's overflow reaches the whole pixels.
        SignedSixteenSixteen sum = velocity + state.XAcceleration;
        int oldWhole = velocity.WholePixels;
        int newWhole = sum.WholePixels;

        int cappedWhole = Math.Abs(newWhole) < MamaTurtleMaximumHoveringSpeed
            ? newWhole
            // Chooses the cap's sign from the old velocity, before storing the
            // just-computed result. Preserve that one-frame zero-crossing behavior.
            : oldWhole < 0
                ? -MamaTurtleMaximumHoveringSpeed
                : MamaTurtleMaximumHoveringSpeed;
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
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (CoordinateAxis.Difference(mama.Position.Y.Whole, MamaTurtlePeakYPosition) < 0)
        {
            state.FunctionTimer = MamaTurtlePeakPauseFrames;
            state.Action = MamaTurtleAction.HoveringAtPeak;
            state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.Zero);
            return;
        }

        if (PlatformsBehavior.IsSamusRidingPlatform(mama, samus))
        {
            mama.Position = mama.Position.WithWrappedWholeY(mama.Position.Y.Whole - MamaTurtleRisingSpeed);
            samus.Kinematics.ExtraDisplacement -= new Offset16<RoomPixels>(0, MamaTurtleRisingSpeed);
            return;
        }

        state.Action = MamaTurtleAction.Falling;
        state.Velocity = state.Velocity.WithY(SignedSixteenSixteen.Zero);
    }

    /// <summary>Runs the 30-frame peak delay.</summary>
    private void RunMamaTurtleHoveringAtPeak(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);
        if (--state.FunctionTimer == 0)
            state.Action = MamaTurtleAction.Falling;
    }

    /// <summary>Runs gravity, collision, and landing-animation selection.</summary>
    private void RunMamaTurtleFalling(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        ResolveMamaTurtleExpandedContact(mama, samus);

        // The cap compares the first enemy slot's speed, which initialization requires to be Mama's own.
        if (state.Velocity.Y.WholePixels < MamaTurtleMaximumFallingSpeed)
        {
            // The one-eighth-pixel fraction step carries into the whole pixels; both are one 16.16 add.
            state.Velocity = state.Velocity.WithY(
                state.Velocity.Y + SignedSixteenSixteen.FromParts(0, 8192));
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

    /// <summary>Runs the Baby Turtle's main behaviour and its six live functions.</summary>
    private void RunBabyTurtleMain(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle AI requires the active Samus actor.");

        // The prologue requires the child's parent to be a Mama Turtle. The game also clears and republishes a
        // carried-height value on the parent here, which nothing reads.
        _ = RequireMamaTurtleParent();
        switch (state.Action)
        {
            case BabyTurtleAction.CrawlingNotCarryingSamus:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    state.Action = BabyTurtleAction.HidingCarryingSamus;
                    state.NotCarryingSamusReactionTimer = 4;
                    StartTurtleAnimation(
                        baby,
                        state,
                        state.Velocity.X < 0
                            ? MamaTurtleSegment.BabyHidingLeft
                            : MamaTurtleSegment.BabyHidingRight);
                }
                return;

            case BabyTurtleAction.HidingCarryingSamus:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    state.NotCarryingSamusReactionTimer = 4;
                    return;
                }
                if (--state.NotCarryingSamusReactionTimer == 0)
                {
                    state.Action = BabyTurtleAction.HidingNotCarryingSamus;
                    state.FunctionTimer = 60;
                }
                return;

            case BabyTurtleAction.HidingNotCarryingSamus:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    state.Action = BabyTurtleAction.SpinningUnstoppable;
                    StartTurtleAnimation(
                        baby,
                        state,
                        MamaTurtleSegment.BabySpinning);
                    bool facingRight = SamusPoseMetadata.ReadFacingDirection(samus) == SamusFacingDirection.Right;
                    state.Velocity = new Offset16<RoomPixels>(facingRight ? 3 : -3, 1);
                    return;
                }
                if (--state.FunctionTimer == 0)
                {
                    StartBabyTurtleCrawlingAnimation(baby, state);
                    state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
                }
                return;

            case BabyTurtleAction.SpinningUnstoppable:
                RequireTurtleLevel(level);
                MoveSpinningBabyTurtle(baby, state, level);
                return;

            case BabyTurtleAction.SpinningStoppable:
                if (PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                {
                    StartBabyTurtleCrawlingAnimation(baby, state);
                    state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
                    return;
                }
                RequireTurtleLevel(level);
                MoveSpinningBabyTurtle(baby, state, level);
                return;

            case BabyTurtleAction.CrawlingCarryingSamus:
                if (!PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
                    state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Baby Turtle main-AI function {state.Action?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Moves a spinning child against terrain, reversing its X velocity on wall collision.</summary>
    private void MoveSpinningBabyTurtle(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        RoomLevelData level)
    {
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.X)))
        {
            state.Velocity = state.Velocity.WithX(-state.Velocity.X);
            return;
        }
        _terrain.MoveEnemyVertically(level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.Y));
    }

    /// <summary>Animation cue: crawl across the sleeping parent contour.</summary>
    private void ProcessBabyTurtleCrawlAnimation(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null || level is null)
        {
            throw new InvalidOperationException(
                "Baby Turtle crawl bytecode requires Samus and active room collision data.");
        }

        bool carryingSamus = PlatformsBehavior.IsSamusRidingPlatform(baby, samus);
        if (carryingSamus)
        {
            samus.Kinematics.ExtraDisplacement += new Offset16<RoomPixels>(state.Velocity.X, 0);
        }

        int previousY = baby.Position.Y.Whole;
        baby.Position = baby.Position.WithWholeY(state.SpawnTopCenter.Y);
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.X));

        RoomEnemySlot parentSlot = RoomMama();
        MamaTurtleEnemyState parent = parentSlot.RequireSpeciesState<MamaTurtleEnemyState>();
        if (parent.Action != MamaTurtleAction.Asleep)
            return;

        int signedDifference = CoordinateAxis.Difference(parentSlot.Position.X.Whole, baby.Position.X.Whole);
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
        RoomEnemySlot baby,
        BabyTurtleEnemyState state)
    {
        int signedDifference = CoordinateAxis.Difference(state.SpawnTopCenter.X, baby.Position.X.Whole);
        int distance = Math.Abs(signedDifference);
        if (distance >= BabyTurtleTravelDistance)
        {
            state.Velocity = state.Velocity.WithX(signedDifference < 0 ? -1 : 1);
        }
        return state.Velocity.X < 0
            ? MamaTurtleSegment.BabyCrawlingLeft
            : MamaTurtleSegment.BabyCrawlingRight;
    }

    /// <summary>Animation cue: begin Mama's shell-entry behavior.</summary>
    private static void StartMamaTurtleEnteringShell(MamaTurtleEnemyState state) =>
        state.Action = MamaTurtleAction.EnteringShell;

    /// <summary>Animation cues: begin rising with selected X direction.</summary>
    private static void StartMamaTurtleRisingToHover(
        MamaTurtleEnemyState state,
        HorizontalDirection direction)
    {
        // The authored cue names describe the visual facing, while the stored velocity is the
        // opposite sign: the facing-left cue writes -1 and the facing-right cue writes +1.
        state.Action = MamaTurtleAction.RisingToHover;
        state.Velocity = state.Velocity.WithWholeX(direction == HorizontalDirection.Right ? -1 : 1);
        state.FunctionTimer = 16;
    }

    /// <summary>Animation cue: branch out of hiding only while ridden; otherwise the hiding animation runs on.</summary>
    private static MamaTurtleSegment? SelectBabyTurtleLeaveShell(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle shell bytecode requires Samus.");
        if (!PlatformsBehavior.IsSamusRidingPlatform(baby, samus))
            return null;
        return state.Velocity.X < 0
            ? MamaTurtleSegment.BabyLeaveShellLeft
            : MamaTurtleSegment.BabyLeaveShellRight;
    }

    /// <summary>Animation cue: restore crawling after leaving the shell.</summary>
    private static MamaTurtleSegment FinishBabyTurtleLeavingShell(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Baby Turtle shell bytecode requires Samus.");
        state.Action = PlatformsBehavior.IsSamusRidingPlatform(baby, samus)
            ? BabyTurtleAction.CrawlingCarryingSamus
            : BabyTurtleAction.CrawlingNotCarryingSamus;
        return state.Velocity.X < 0
            ? MamaTurtleSegment.BabyCrawlingLeft
            : MamaTurtleSegment.BabyCrawlingRight;
    }

    /// <summary>Runs Mama's custom ordinary-touch callback.</summary>
    private void ResolveMamaTurtleTouch(
        RoomEnemySlot mama,
        MamaTurtleEnemyState state,
        SamusState samus)
    {
        if (mama.Contact.HasAny(EnemyContact.SolidToSamus))
            return;
        _collision.ResolveNormalEnemyTouch(mama, samus);
        state.Action = MamaTurtleAction.Falling;
        state.Velocity = state.Velocity.WithWholeY(2);
    }

    /// <summary>Runs Baby's direction flip, separation, and wake callback.</summary>
    private void ResolveBabyTurtleTouch(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state,
        SamusState samus,
        RoomLevelData? level)
    {
        if (state.Action == BabyTurtleAction.CrawlingCarryingSamus)
            return;
        RequireTurtleLevel(level);

        if (state.Velocity.X < 0)
        {
            StartTurtleAnimation(
                baby,
                state,
                MamaTurtleSegment.BabyCrawlingRight);
            state.Velocity = state.Velocity.WithX(1);
        }
        else
        {
            StartTurtleAnimation(
                baby,
                state,
                MamaTurtleSegment.BabyCrawlingLeft);
            state.Velocity = state.Velocity.WithX(-1);
        }

        baby.Position = baby.Position.WithWholeX(CoordinateAxis.Difference(baby.Position.X.Whole, samus.Position.X.Whole) >= 0
            ? CoordinateAxis.Wrap(samus.Position.X.Whole + samus.Kinematics.Radius.X + baby.Radius.X)
            : CoordinateAxis.Wrap(samus.Position.X.Whole - samus.Kinematics.Radius.X - baby.Radius.X));
        state.Action = BabyTurtleAction.CrawlingNotCarryingSamus;
        _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level, baby, SignedSixteenSixteen.FromWholePixels(state.Velocity.X));
        WakeMamaTurtle();
    }

    /// <summary>Private tail of the Baby Turtle's shot AI.</summary>
    private void ResolveBabyTurtleShotAfterCommon() =>
        WakeMamaTurtle();

    /// <summary>Decrements the parent's asleep flag after a baby touch or shot reaction.</summary>
    private void WakeMamaTurtle()
    {
        MamaTurtleEnemyState parent = RequireMamaTurtleParent();
        parent.AsleepFlag = Math.Max(0, parent.AsleepFlag - 1);
    }

    /// <summary>Runs Mama's expanded private damage box.</summary>
    private void ResolveMamaTurtleExpandedContact(
        RoomEnemySlot mama,
        SamusState samus)
    {
        // The four bounds come from the first enemy slot, which initialization requires to be Mama's own.
        int left = CoordinateAxis.Wrap(
            mama.Position.X.Whole - mama.Radius.X - 8);
        int right = CoordinateAxis.Wrap(
            mama.Position.X.Whole + mama.Radius.X + 8);
        int top = CoordinateAxis.Wrap(
            mama.Position.Y.Whole - mama.Radius.Y + 4);
        int bottom = CoordinateAxis.Wrap(
            mama.Position.Y.Whole + mama.Radius.Y - 4);

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

        _collision.ResolveNormalEnemyTouch(mama, samus);
    }

    /// <summary>The babies' parent, the room's Mama Turtle's place, rejecting a non-Mama occupant.</summary>
    private MamaTurtleEnemyState RequireMamaTurtleParent()
    {
        RoomEnemySlot parent = RoomMama();
        if (parent.DefinitionId != EnemyDefinitionId.MamaTurtle)
        {
            throw new InvalidDataException(
                $"Baby Turtle parent slot {parent.SlotIndex} names enemy " +
                $"{parent.DefinitionId}, not Mama Turtle.");
        }
        return parent.RequireSpeciesState<MamaTurtleEnemyState>();
    }

    /// <summary>The room's Mama Turtle's place, every baby's parent.</summary>
    /// <exception cref="InvalidOperationException">No Mama Turtle has been placed in the room.</exception>
    private RoomEnemySlot RoomMama() =>
        _mama ?? throw new InvalidOperationException("A Baby Turtle reached its parent in a room where no Mama Turtle was placed.");

    /// <summary>Selects the left or right crawl animation from the child's signed X velocity.</summary>
    private static void StartBabyTurtleCrawlingAnimation(
        RoomEnemySlot baby,
        BabyTurtleEnemyState state) =>
        StartTurtleAnimation(
            baby,
            state,
            state.Velocity.X < 0
                ? MamaTurtleSegment.BabyCrawlingLeft
                : MamaTurtleSegment.BabyCrawlingRight);

    /// <summary>Starts a Mama Turtle animation with its frame timer set to one.</summary>
    private static void StartTurtleAnimation(RoomEnemySlot slot, MamaTurtleEnemyState state, MamaTurtleSegment segment)
    {
        state.Playback = new(MamaTurtleAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Starts a baby turtle animation with its frame timer set to one.</summary>
    private static void StartTurtleAnimation(RoomEnemySlot slot, BabyTurtleEnemyState state, MamaTurtleSegment segment)
    {
        state.Playback = new(MamaTurtleAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        slot.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Rejects turtle movement without active room collision data.</summary>
    private static void RequireTurtleLevel([NotNull] RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Turtle movement requires active room collision data.");
    }

    /// <summary>Runs the baby wake callback after common bomb damage.</summary>
    internal void AddMamaTurtleBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.BabyTurtle &&
                                                       c.SelectedShotAi == EnemyReaction.BabyTurtleShot)
        {
            AfterCommon = _ => ResolveBabyTurtleShotAfterCommon(),
        });
    }

    /// <summary>Runs MamaTurtle's main AI.</summary>
    internal void RunMamaTurtleMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMamaTurtleMain(
            slot,
            slot.RequireSpeciesState<MamaTurtleEnemyState>(),
            frame.Samus,
            frame.Level,
            frame.Clock.ShortCounter);
    }

    /// <summary>Runs BabyTurtle's main AI.</summary>
    internal void RunBabyTurtleMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunBabyTurtleMain(slot, slot.RequireSpeciesState<BabyTurtleEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances MamaTurtle's typed animation.</summary>
    internal void AdvanceMamaTurtleTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _)
    {
        if (!slot.AnimationClock.Expire())
            return;
        MamaTurtleEnemyState state = slot.RequireSpeciesState<MamaTurtleEnemyState>();
        AnimationPlayback<MamaTurtleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, MamaTurtleAnimations.Segments, ref playback,
            cue => ApplyMamaTurtleCue(slot, cue) is { } target
                ? new AnimationContinuation<MamaTurtleSegment>(MamaTurtleAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Advances BabyTurtle's typed animation.</summary>
    internal void AdvanceBabyTurtleTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext context)
    {
        if (!slot.AnimationClock.Expire())
            return;
        BabyTurtleEnemyState state = slot.RequireSpeciesState<BabyTurtleEnemyState>();
        AnimationPlayback<MamaTurtleSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(slot, MamaTurtleAnimations.Segments, ref playback,
            cue => ApplyBabyTurtleCue(slot, cue, context.Samus, context.Level) is { } target
                ? new AnimationContinuation<MamaTurtleSegment>(MamaTurtleAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);
        state.Playback = playback;
    }

    /// <summary>Runs the baby wake callback after common projectile damage.</summary>
    internal void AddMamaTurtleShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.BabyTurtle &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.BabyTurtleShot)
        {
            AfterCommon = _ => ResolveBabyTurtleShotAfterCommon(),
        });
    }

    /// <summary>Routes Mama contact to shell/fall handling and baby contact to separation and wake behavior.</summary>
    internal void AddMamaTurtleTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.MamaTurtle &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.MamaTurtleTouch,
            context => ResolveMamaTurtleTouch(context.Slot, context.Slot.RequireSpeciesState<MamaTurtleEnemyState>(), context.Samus));

        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.BabyTurtle &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.BabyTurtleTouch,
            context => ResolveBabyTurtleTouch(context.Slot, context.Slot.RequireSpeciesState<BabyTurtleEnemyState>(), context.Samus, context.Level));
    }
}
