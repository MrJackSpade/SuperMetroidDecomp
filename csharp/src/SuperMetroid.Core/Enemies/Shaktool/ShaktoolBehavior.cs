using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The Shaktool species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ShaktoolBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Shaktool];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new ShaktoolEntity(this, place, common);

    /// <summary>Checks terrain collisions along each segment's orbit and attack path.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the linked Shaktool attack circles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each segment finds its chain when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The chain and neighbours each segment found when it was placed.</summary>
    private readonly EnemyPartLinks<ShaktoolChainPlaces> _chains = new();
    /// <summary>Runs common contact behavior for the segment hitboxes.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds collision, projectile, and place owners for a room's Shaktool chain.</summary>
    internal ShaktoolBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemyPlaces places, EnemyCollision collision)
    {
        _terrain = terrain;
        _projectiles = projectiles;
        _places = places;
        _collision = collision;
    }

    /// <summary>The whole step half a turn from <paramref name="angle"/>, its fraction cleared.</summary>
    private static RotationAngle OppositeOrbitStep(RotationAngle angle) =>
        (angle + RotationAngle.HalfTurn).WholeStep;

    /// <summary>Initializes the Shaktool, establishing the segment's orbit and owner-place offset.</summary>
    internal void InitializeShaktool(RoomEnemyPlace place)
    {
        ShaktoolSpawnSettings settings = place.SpawnSettings<ShaktoolSpawnSettings>();
        ShaktoolSegmentDefinition definition = settings.Segment;
        var state = new ShaktoolSegmentState();
        place.SetSpeciesState(state);

        // The population entry names the chain's initial motion; the chain then writes its flags to every segment
        // together.
        state.MotionFlags = settings.Motion;
        place.AnimationClock.Restart();
        state.TargetAngle = RotationAngle.Zero;
        state.ResetOrientation();
        place.Processing = place.Processing.With(definition.Processing);
        place.Contact = place.Contact.With(definition.Contact);
        // The segment's definition names how far after the chain's leading saw the room lists it. The chain's death
        // deletes the enemies the room lists first (ShaktoolDeathDeletesFirstListed), so the chain must be listed first.
        RoomEnemyPlace? owner = definition.OwnerPlaceDistance == 0
            ? place
            : _places.ListedBeforeOrNone(place, definition.OwnerPlaceDistance);
        if (owner is null || !ShaktoolDeathDeletesFirstListed.DeletesOwnChain(_places.InUpdateOrder, owner))
        {
            throw new InvalidDataException(
                $"Shaktool's chain starts in enemy place {place.Ordinal - definition.OwnerPlaceDistance}; its death deletes the first seven places, " +
                "where both retail Shaktool room states place it.");
        }
        // The chain's places hold its segments in chain order, leading saw first.
        RoomEnemyPlace[] chainPlaces = [owner, .. _places.ListedRunAfter(owner, ShaktoolSegmentDefinitions.SegmentCount - 1)];
        _chains.Place(place, new ShaktoolChainPlaces(
            Chain: [.. chainPlaces.Zip(ShaktoolSegmentDefinitions.Segments, (segment, definition) => new ShaktoolChainSegment(segment, definition))],
            Previous: _places.ListedBeforeOrNone(place, 1),
            Next: _places.ListedAfterOrNone(place, 1)));
        state.Motion = definition.Motion;
        // Initialization and synchronization both take the segment's authored angular velocity unchanged.
        state.AngularVelocity = definition.AngularVelocity;
        state.OrbitAngle = definition.InitialOrbitAngle;

        // The segment's definition names the animation it starts.
        state.Playback = new(ShaktoolAnimations.StartingWith(definition.InitialAnimation), AnimationPlace.StartOfAnimation);

        // The leading saw is the chain's fixed anchor. Every later segment centers on the preceding segment's
        // position, subpixels included. Express that link structurally instead of treating a value the game left behind as an
        // unrelated center.
        if (definition != ShaktoolSegmentDefinitions.LeadingSaw)
            PositionShaktoolAroundPreviousSegment(place, state);
    }

    /// <summary>Runs Shaktool's hurt and main behaviours.</summary>
    private void RunShaktoolMain(
        RoomEnemyPlace place,
        ShaktoolSegmentState state,
        RoomLevelData level)
    {
        switch (state.Motion)
        {
            case ShaktoolSegmentMotion.Idle:
                return;

            case ShaktoolSegmentMotion.OrbitPreviousSegment:
                AdvanceShaktoolOrbit(place, state);
                return;

            case ShaktoolSegmentMotion.OrbitAndOrientCenter:
                AdvanceAndOrientShaktoolCenter(place, state);
                return;

            case ShaktoolSegmentMotion.DriveTailAndReverseAtWalls:
                DriveShaktoolTail(place, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Shaktool has no motion {state.Motion}.");
        }
    }

    /// <summary>Runs the linked-segment placement helper.</summary>
    internal void PositionShaktoolAroundPreviousSegment(
        RoomEnemyPlace place,
        ShaktoolSegmentState state)
    {
        RoomEnemyPlace previous = _chains.Of(place).Previous
            ?? throw new InvalidOperationException("Shaktool's anchor has no preceding segment.");
        // The segment placement reads the preceding segment's place without testing it. A lethal shot clears the
        // struck segment and deletes the chain, but the later segments still run this update; their final orbit uses
        // the cleared coordinates.
        place.Position = previous.Position.Advance(OrbitDisplacement(state.OrbitAngle.ToTrigonometryAngle()));
    }

    /// <summary>
    /// A segment's place from the one before it at <paramref name="angle"/>: the orbit's sine
    /// (<see cref="ShaktoolOrbitDefinitions.OrbitSine"/>) at the angle across, and at the angle a quarter turn behind down.
    /// </summary>
    internal static FixedVector<RoomPixels> OrbitDisplacement(TrigonometryAngle angle) => new(
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(ShaktoolOrbitDefinitions.OrbitSine.At(angle)),
        SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(ShaktoolOrbitDefinitions.OrbitSine.At(angle - TrigonometryAngle.QuarterTurn)));

    /// <summary>Runs the common orbit step.</summary>
    private void AdvanceShaktoolOrbit(RoomEnemyPlace place, ShaktoolSegmentState state)
    {
        PositionShaktoolAroundPreviousSegment(place, state);
        ShaktoolMotionFlags flags = state.MotionFlags;
        RotationAngle increment;
        if ((flags & ShaktoolMotionFlags.Straightening) != ShaktoolMotionFlags.None)
        {
            state.TargetAngle += ShaktoolOrbitDefinitions.StraighteningTurn;
            increment = ShaktoolOrbitDefinitions.StraighteningTurn;
        }
        else
        {
            increment = state.AngularVelocity;
        }

        state.OrbitAngle = (flags & ShaktoolMotionFlags.Clockwise) != ShaktoolMotionFlags.None
            ? state.OrbitAngle - increment
            : state.OrbitAngle + increment;
    }

    /// <summary>Runs middle-segment orientation selection.</summary>
    internal void AdvanceAndOrientShaktoolCenter(
        RoomEnemyPlace place,
        ShaktoolSegmentState state)
    {
        AdvanceShaktoolOrbit(place, state);
        RoomEnemyPlace nextPlace = GetNextShaktoolSegment(place);
        ShaktoolSegmentState next = RequireShaktoolState(nextPlace);

        // The heading lies halfway along the turn from the angle opposite this segment's on to the next segment's.
        RotationAngle oppositeAngle = state.OrbitAngle + RotationAngle.HalfTurn;
        RotationAngle midpoint = oppositeAngle + (next.OrbitAngle - oppositeAngle).DividedBy(2);
        if (state.OrientationReversed)
            midpoint += RotationAngle.HalfTurn;

        state.Orientation = ShaktoolOrbitDefinitions.Orientations.At(midpoint);
        state.Playback = new(ShaktoolAnimations.StartingWith(
            ShaktoolAnimationChoices.ForOrientation(state.Orientation)), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs the last segment's collision/reversal controller.</summary>
    private void DriveShaktoolTail(
        RoomEnemyPlace place,
        ShaktoolSegmentState state,
        RoomLevelData level)
    {
        Position16<RoomPixels> previous = place.Position.Whole;
        AdvanceShaktoolOrbit(place, state);
        Position16<RoomPixels> candidate = place.Position.Whole;
        Offset16<RoomPixels> step = candidate - previous;

        // The helper restores only whole pixels before collision. Subpositions
        // remain those the orbit step produced, a subtle asymmetry that affects wall alignment.
        place.Position = place.Position.WithWhole(previous);
        SignedSixteenSixteen xDisplacement = SignedSixteenSixteen.FromWholePixels(step.X);
        bool collided = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            place,
            xDisplacement);
        if (!collided)
        {
            place.Position = place.Position.WithWholeY(previous.Y);
            SignedSixteenSixteen yDisplacement = SignedSixteenSixteen.FromWholePixels(step.Y);
            collided = _terrain.MoveEnemyVertically(level, place, yDisplacement);
        }

        if (collided)
        {
            ReverseShaktoolAfterCollision(place, state, previous);
            return;
        }

        place.Position = place.Position.WithWhole(candidate);
        ShaktoolMotionFlags flags = state.MotionFlags;
        if ((flags & ShaktoolMotionFlags.Straightening) != ShaktoolMotionFlags.None)
        {
            // The straightening step deliberately adds a second step after the first increment.
            state.TargetAngle += ShaktoolOrbitDefinitions.StraighteningTurn;
            return;
        }

        if (state.TargetAngle.WholeStep == state.OrbitAngle.WholeStep)
        {
            SynchronizeShaktoolOrbitTargets(place, state.TargetAngle);
            // The game keeps only the whole part of the restarted progress, which is already whole.
            state.CurlProgress = ShaktoolOrbitDefinitions.StraighteningStart;
            SetShaktoolGroupMotionFlags(
                place,
                state.MotionFlags & ~ShaktoolMotionFlags.ReversedOnce);
        }

        // Accumulation stops once the chain straightens, at most one increment past the threshold.
        state.CurlProgress += state.AngularVelocity;
        if (state.CurlProgress >= ShaktoolOrbitDefinitions.StraighteningThreshold)
        {
            SetShaktoolGroupMotionFlags(
                place,
                state.MotionFlags | ShaktoolMotionFlags.Straightening);
        }
    }

    /// <summary>Reverses chain direction at terrain contact, including the first-hit endpoint swap and synchronized flags.</summary>
    internal void ReverseShaktoolAfterCollision(
        RoomEnemyPlace place,
        ShaktoolSegmentState state,
        Position16<RoomPixels> previous)
    {
        ShaktoolMotionFlags flags = state.MotionFlags;
        if ((flags & ShaktoolMotionFlags.ReversedOnce) != ShaktoolMotionFlags.None)
        {
            SetShaktoolGroupMotionFlags(
                place,
                (flags ^ ShaktoolMotionFlags.Clockwise) &
                ~(ShaktoolMotionFlags.ReversedOnce | ShaktoolMotionFlags.Straightening));
        }
        else
        {
            place.Position = place.Position.WithWhole(previous);
            ReverseShaktoolChainEndpoints(place);

            // The group's motion flags are set twice. The second call reads the parameter just written
            // by the first, leaving the reversed-once bit set while clearing the straightening bit.
            SetShaktoolGroupMotionFlags(place, flags | ShaktoolMotionFlags.ReversedOnce);
            SetShaktoolGroupMotionFlags(
                place,
                state.MotionFlags & ~ShaktoolMotionFlags.Straightening);
        }

        state.ResetOrientation();
        RoomEnemyPlace owner = OwnerPlace(place);
        RotationAngle target = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(place.Position.X.Whole, owner.Position.X.Whole), CoordinateAxis.Difference(place.Position.Y.Whole, owner.Position.Y.Whole))).ToMirroredTrigonometryAngle().ToAngle();
        target = (state.MotionFlags & ShaktoolMotionFlags.Clockwise) != ShaktoolMotionFlags.None
            ? target - RotationAngle.QuarterTurn
            : target + RotationAngle.QuarterTurn;
        SetShaktoolGroupTargetAngle(place, target);

        // From the trailing saw back to the leading one, each segment stops and plays its head bob.
        foreach (ShaktoolChainSegment segment in GetShaktoolGroup(place).Reverse())
        {
            ShaktoolSegmentState segmentState = RequireShaktoolState(segment.Place);
            CalculateShaktoolAngularVelocity(segmentState);
            segmentState.Motion = ShaktoolSegmentMotion.Idle;
            segmentState.Playback = new(ShaktoolAnimations.StartingWith(
                segment.Definition.CollisionAnimation), AnimationPlace.StartOfAnimation);
            segment.Place.AnimationClock.StepOnNextUpdate();
        }
    }

    /// <summary>Runs the endpoint/angle reversal helper.</summary>
    private void ReverseShaktoolChainEndpoints(RoomEnemyPlace anySegment)
    {
        IReadOnlyList<ShaktoolChainSegment> group = GetShaktoolGroup(anySegment);
        RoomEnemyPlace At(ShaktoolSegmentDefinition segment) => group.Single(link => link.Definition == segment).Place;
        ShaktoolSegmentState center = RequireShaktoolState(At(ShaktoolSegmentDefinitions.Head));
        center.OrientationReversed = !center.OrientationReversed;

        // Each segment's orbit angle trades places with the one mirrored across the chain, the head's with the inner
        // arm that trails it.
        SwapMirroredShaktoolAngles(At(ShaktoolSegmentDefinitions.TrailingSaw), At(ShaktoolSegmentDefinitions.LeadingOuterArm));
        SwapMirroredShaktoolAngles(At(ShaktoolSegmentDefinitions.TrailingOuterArm), At(ShaktoolSegmentDefinitions.LeadingInnerArm));
        SwapMirroredShaktoolAngles(At(ShaktoolSegmentDefinitions.TrailingInnerArm), At(ShaktoolSegmentDefinitions.Head));

        RoomEnemyPlace leadingSaw = At(ShaktoolSegmentDefinitions.LeadingSaw);
        RoomEnemyPlace trailingSaw = At(ShaktoolSegmentDefinitions.TrailingSaw);
        FixedPosition<RoomPixels> leadingSawPosition = leadingSaw.Position;
        leadingSaw.Position = trailingSaw.Position;
        trailingSaw.Position = leadingSawPosition;

        // Every segment between the two saws takes the swap's subpixels.
        foreach (ShaktoolChainSegment segment in group)
        {
            if (segment.Definition == ShaktoolSegmentDefinitions.LeadingSaw || segment.Definition == ShaktoolSegmentDefinitions.TrailingSaw)
                continue;
            segment.Place.Position = segment.Place.Position.WithFractionX(ShaktoolOrbitDefinitions.SwapFraction).WithFractionY(ShaktoolOrbitDefinitions.SwapFraction);
        }
    }

    /// <summary>Swaps two segment orbit angles after rotating each by half a turn.</summary>
    private static void SwapMirroredShaktoolAngles(RoomEnemyPlace left, RoomEnemyPlace right)
    {
        ShaktoolSegmentState leftState = RequireShaktoolState(left);
        ShaktoolSegmentState rightState = RequireShaktoolState(right);
        RotationAngle saved = leftState.OrbitAngle;
        leftState.OrbitAngle = OppositeOrbitStep(rightState.OrbitAngle);
        rightState.OrbitAngle = OppositeOrbitStep(saved);
    }

    /// <summary>
    /// Runs the per-segment convergence speed calculation: a sixty-fourth of the whole steps left to the target in the
    /// orbit direction, four 256ths of a step for each.
    /// </summary>
    private static void CalculateShaktoolAngularVelocity(ShaktoolSegmentState state)
    {
        RotationAngle turnLeft = (state.MotionFlags & ShaktoolMotionFlags.Clockwise) != ShaktoolMotionFlags.None
            ? state.OrbitAngle - state.TargetAngle
            : state.TargetAngle - state.OrbitAngle;
        state.AngularVelocity = turnLeft.WholeStep.DividedBy(ShaktoolOrbitDefinitions.ConvergenceShares);
    }

    /// <summary>Runs group synchronization.</summary>
    private void SynchronizeShaktoolOrbitTargets(RoomEnemyPlace anySegment, RotationAngle target)
    {
        foreach (ShaktoolChainSegment segment in GetShaktoolGroup(anySegment))
        {
            ShaktoolSegmentState state = RequireShaktoolState(segment.Place);
            state.OrbitAngle = target;
            state.AngularVelocity = segment.Definition.AngularVelocity;
        }
    }

    /// <summary>Publishes a shared convergence target to all seven initialized segments.</summary>
    private void SetShaktoolGroupTargetAngle(RoomEnemyPlace anySegment, RotationAngle target)
    {
        foreach (ShaktoolChainSegment segment in GetShaktoolGroup(anySegment))
            RequireShaktoolState(segment.Place).TargetAngle = target;
    }

    /// <summary>Writes the motion flags to the entire chain.</summary>
    private void SetShaktoolGroupMotionFlags(
        RoomEnemyPlace anySegment,
        ShaktoolMotionFlags flags)
    {
        foreach (ShaktoolChainSegment segment in GetShaktoolGroup(anySegment))
            RequireShaktoolState(segment.Place).MotionFlags = flags;
    }

    /// <summary>The chain's leading saw, which the segment found when it was placed.</summary>
    private RoomEnemyPlace OwnerPlace(RoomEnemyPlace segment) => _chains.Of(segment).Chain[0].Place;

    /// <summary>The seven segments of the chain a segment found when it was placed, each validated as initialized.</summary>
    private IReadOnlyList<ShaktoolChainSegment> GetShaktoolGroup(RoomEnemyPlace anySegment)
    {
        _ = RequireShaktoolState(anySegment);
        IReadOnlyList<ShaktoolChainSegment> group = _chains.Of(anySegment).Chain;
        foreach (ShaktoolChainSegment segment in group)
        {
            // Group helpers also run during the remainder of the lethal update, reaching the killed segment's cleared state.
            if (segment.Place.FoundOrClearedState<ShaktoolSegmentState>() is null)
            {
                throw new InvalidDataException(
                    $"Shaktool group rooted at place {group[0].Place.Ordinal} is missing its segment at place {segment.Place.Ordinal}.");
            }
        }
        return group;
    }

    /// <summary>The enemy listed after a linked segment center, which it found when it was placed.</summary>
    private RoomEnemyPlace GetNextShaktoolSegment(RoomEnemyPlace place) =>
        _chains.Of(place).Next ?? throw new InvalidDataException(
            $"Shaktool center place {place.Ordinal} has no following segment.");

    /// <summary>
    /// The state of the segment in <paramref name="place"/>: its own, or, for the segment a hit killed this update, the
    /// emptied state the rest of its chain reads and writes among the states the kill cleared
    /// (<see cref="RoomEnemyPlace.FoundOrClearedState{T}"/>, <see cref="RemovedEnemyLeftovers{TStates}"/>).
    /// </summary>
    private static ShaktoolSegmentState RequireShaktoolState(RoomEnemyPlace place) =>
        place.FoundOrClearedState<ShaktoolSegmentState>() ?? throw new InvalidOperationException(
            $"Enemy place {place.Ordinal} has no initialized Shaktool state.");

    /// <summary>Carries out one cue of a Shaktool segment's animation.</summary>
    internal void ApplyShaktoolCue(RoomEnemyPlace place, ShaktoolSegmentState state, ShaktoolCue cue)
    {
        switch (cue.Kind)
        {
            case ShaktoolCueKind.Lower:
            case ShaktoolCueKind.Raise:
            {
                TrigonometryAngle targetDirection = state.TargetAngle.ToTrigonometryAngle();
                MoveShaktoolSegmentForAnimation(
                    place,
                    cue.Kind == ShaktoolCueKind.Lower
                        ? targetDirection.TurnedBySteps(EnemyTrigonometryAngleDefinitions.HalfTurn)
                        : targetDirection);
                break;
            }

            case ShaktoolCueKind.ResumeMotion:
            {
                foreach (ShaktoolChainSegment segment in GetShaktoolGroup(place))
                    RequireShaktoolState(segment.Place).Motion = segment.Definition.Motion;
                break;
            }

            default:
                throw new InvalidOperationException($"Undefined {nameof(ShaktoolCueKind)} {cue.Kind}.");
        }
    }

    /// <summary>Runs the one-frame sine movement helper.</summary>
    private static void MoveShaktoolSegmentForAnimation(RoomEnemyPlace place, TrigonometryAngle angle)
    {
        // X reads the shared signed sine at the angle and Y the sine a quarter turn behind it, the negative cosine.
        SignedEightEight xVelocity = SineVelocity(angle);
        SignedEightEight yVelocity = SineVelocity(angle.TurnedBy(TrigonometryAngle.HalfTurn).TurnedBy(TrigonometryAngle.QuarterTurn));
        place.Position = place.Position.Advance(new(xVelocity.ToSixteenSixteenDelta(), yVelocity.ToSixteenSixteenDelta()));
    }

    /// <summary>The shared enemy sine velocity at <paramref name="angle"/>: one pixel per update at its peak.</summary>
    internal static SignedEightEight SineVelocity(TrigonometryAngle angle) =>
        EnemyMath.SineVelocity(angle, 1);

    /// <summary>
    /// Implements Shaktool's fatal-shot tail: a hit that kills one segment deletes the enemies the room lists first
    /// (<see cref="ShaktoolDeathDeletesFirstListed"/>), which initialization requires to be the chain's own segments.
    /// </summary>
    private void ResolveShaktoolShotAfterCommon(RoomEnemyPlace struckSegment)
    {
        if (struckSegment.Health != 0)
            return;
        foreach (RoomEnemyPlace segment in ShaktoolDeathDeletesFirstListed.Deleted(_places.InUpdateOrder, ShaktoolSegmentDefinitions.SegmentCount))
        {
            segment.Contact = EnemyContact.None;
            segment.Processing = EnemyProcessing.None;
            segment.Presence = EnemyPresence.Deleted;
        }
    }

    /// <summary>Runs the post-common lethal-shot tail for bomb hits.</summary>
    internal EnemyBombReaction ShaktoolBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
    };

    /// <summary>Shaktool's attack circles, front, middle and back (#1760): their spawner starts their animations.</summary>
    internal EnemyProjectileBehavior AttackCircles => field ??= new AttackCircleBehavior(_projectiles);

    /// <summary>
    /// An attack circle: the front one's flight with room collision, and the trailing ones' while the first
    /// enemy-projectile place, the front circle's, stays live.
    /// </summary>
    /// <param name="projectiles">The enemy projectiles, whose first place the trailing circles follow.</param>
    private sealed class AttackCircleBehavior(EnemyProjectileEngine projectiles)
        : EnemyProjectileMotionBehavior<ShaktoolCircleMotion>(StartingMotions), IEnemyProjectileCues<ShaktoolCircleCue>
    {
        /// <summary>The front circle leads from its spawn; the trailing ones hold still until their animations start them.</summary>
        private static readonly Dictionary<RoomEnemyProjectileKind, ShaktoolCircleMotion?> StartingMotions = new()
        {
            [RoomEnemyProjectileKind.ShaktoolAttackFrontCircle] = ShaktoolCircleMotion.Leading,
            [RoomEnemyProjectileKind.ShaktoolAttackMiddleCircle] = null,
            [RoomEnemyProjectileKind.ShaktoolAttackBackCircle] = null,
        };

        /// <summary>Carries out a trailing circle's animation cue: it starts following the front circle.</summary>
        public void RunCue(EnemyProjectileCueContext context, ShaktoolCircleCue cue)
        {
            if (cue != ShaktoolCircleCue.StartTrailing)
                throw new InvalidOperationException($"Undefined {nameof(ShaktoolCircleCue)} {cue}.");
            context.Projectile.StartMotion(ShaktoolCircleMotion.Trailing);
        }

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, ShaktoolCircleMotion motion)
        {
            switch (motion)
            {
                case ShaktoolCircleMotion.Leading:
                    FlyShaktoolFrontCircle(context.Projectile, context.Level);
                    break;
                case ShaktoolCircleMotion.Trailing:
                    FlyShaktoolTrailingCircle(context.Projectile, context.Level);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(ShaktoolCircleMotion)} {motion}.");
            }
        }

        /// <summary>
        /// Flies the middle or back circle one update behind the front one: a circle vanishes once the first enemy-projectile
        /// place is free (<see cref="ShaktoolCirclesFollowFirstPlace"/>).
        /// </summary>
        private void FlyShaktoolTrailingCircle(
            EnemyProjectile projectile,
            RoomLevelData level)
        {
            if (!ShaktoolCirclesFollowFirstPlace.Lasts(projectiles.EnemyProjectiles, place => place.IsActive))
            {
                projectile.Delete();
                return;
            }

            _ = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
            _ = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
        }
    }

    /// <summary>Runs Shaktool's main AI.</summary>
    internal void RunShaktoolMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunShaktoolMain(place, RequireShaktoolState(place), frame.Level);
    }

    /// <summary>Advances Shaktool's typed animation.</summary>
    internal void AdvanceShaktoolTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        var state = RequireShaktoolState(place);
        AnimationPlayback<ShaktoolSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, ShaktoolAnimations.Segments, ref playback,
            cue =>
            {
                ApplyShaktoolCue(place, state, cue);
                return null;
            });
        state.Playback = playback;
    }

    /// <summary>Shaktool contact is the common normal-enemy touch, death animation included.</summary>
    internal void ReactToShaktoolTouch(EnemyTouchContext context) =>
        _collision.ResolveNormalEnemyTouch(context.Place, context.Samus);

    /// <summary>Runs the post-common lethal-shot tail for regular projectile hits.</summary>
    internal EnemyShotReaction ShaktoolShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
    };
    /// <summary>Flies the front circle one update.</summary>
    internal static void FlyShaktoolFrontCircle(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Delete();
        }
    }
}
