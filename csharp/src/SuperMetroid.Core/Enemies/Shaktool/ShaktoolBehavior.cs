using SuperMetroid.Core.Animation;
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
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new ShaktoolEntity(this, occupant, place, common));

    /// <summary>Checks terrain collisions along each segment's orbit and attack path.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>The room's enemy places, where each segment finds its chain when it is placed.</summary>
    private readonly EnemyPlaces _places;
    /// <summary>The chain and neighbours each segment found when it was placed.</summary>
    private readonly EnemyPartLinks<ShaktoolChainPlaces> _chains = new();
    /// <summary>Runs common contact behavior for the segment hitboxes.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds collision and place owners for a room's Shaktool chain.</summary>
    internal ShaktoolBehavior(EnemyTerrain terrain, EnemyPlaces places, EnemyCollision collision)
    {
        _terrain = terrain;
        _places = places;
        _collision = collision;
    }

    /// <summary>The whole step half a turn from <paramref name="angle"/>, its fraction cleared.</summary>
    private static RotationAngle OppositeOrbitStep(RotationAngle angle) =>
        (angle + RotationAngle.HalfTurn).WholeStep;

    /// <summary>Initializes the Shaktool, establishing the segment's orbit and its place in its chain.</summary>
    internal void InitializeShaktool(RoomEnemyOccupant occupant, ShaktoolSpawnSettings settings, RoomEnemyPlace place)
    {
        ShaktoolSegmentDefinition definition = settings.Segment;
        var state = new ShaktoolSegmentState();
        place.SetSpeciesState(state);

        place.AnimationClock.Restart();
        state.ResetOrientation();
        place.Processing = place.Processing.With(definition.Processing);
        place.Contact = place.Contact.With(definition.Contact);
        // The segment's definition is its place in the chain, which the room lists together. The chain's death deletes
        // the enemies the room lists first (ShaktoolDeathDeletesFirstListed), so the chain must be listed first.
        RoomEnemyPlace owner = definition == ShaktoolSegmentDefinitions.LeadingSaw
            ? place
            : _places.Member(place, ShaktoolSegmentDefinitions.Chain, definition, ShaktoolSegmentDefinitions.LeadingSaw);
        if (!ShaktoolDeathDeletesFirstListed.DeletesOwnChain(_places.InUpdateOrder, owner))
        {
            throw new InvalidDataException(
                $"Shaktool's chain starts at {owner}; its death deletes the first seven places, where both Shaktool room states place it.");
        }
        _chains.Place(place, new ShaktoolChainPlaces(
            Chain: owner == place ? StartShaktoolChain(owner, settings.Motion) : JoinShaktoolChain(owner, settings.Motion),
            Previous: _places.MemberBefore(place, ShaktoolSegmentDefinitions.Chain, definition),
            Next: _places.MemberAfter(place, ShaktoolSegmentDefinitions.Chain, definition)));
        state.Motion = definition.Motion;
        // Initialization and synchronization both take the segment's authored angular velocity unchanged.
        state.AngularVelocity = definition.AngularVelocity;
        state.OrbitAngle = definition.InitialOrbitAngle;

        // The segment's definition names the animation it starts.
        state.Playback = new(ShaktoolAnimations.StartingWith(definition.InitialAnimation), AnimationPlace.StartOfAnimation);

        // The leading saw is the chain's fixed anchor. Every later segment centers on the preceding segment's
        // position, fractions of a pixel included. Express that link structurally instead of treating a value the game left behind as an
        // unrelated center.
        if (definition != ShaktoolSegmentDefinitions.LeadingSaw)
            PositionShaktoolAroundPreviousSegment(place, state);
    }

    /// <summary>
    /// Starts the chain <paramref name="leadingSaw"/> leads (#2452): its places hold its segments in chain order, leading
    /// saw first, and the leading saw's placement names how the chain starts moving, with no target yet.
    /// </summary>
    private ShaktoolChain StartShaktoolChain(RoomEnemyPlace leadingSaw, ShaktoolChainMotion motion)
    {
        return new ShaktoolChain(
            [.. ShaktoolSegmentDefinitions.Segments.Select(part => new ShaktoolChainSegment(
                part == ShaktoolSegmentDefinitions.LeadingSaw
                    ? leadingSaw
                    : _places.Member(leadingSaw, ShaktoolSegmentDefinitions.Chain, ShaktoolSegmentDefinitions.LeadingSaw, part),
                part))],
            motion);
    }

    /// <summary>
    /// The chain led by <paramref name="leadingSaw"/>, which a later segment joins as it is placed (#2452). Every segment of
    /// one chain is placed naming the same starting motion, since the chain moves as one.
    /// </summary>
    /// <exception cref="InvalidDataException">The segment's placement names another starting motion than its leading saw's.</exception>
    private ShaktoolChain JoinShaktoolChain(RoomEnemyPlace leadingSaw, ShaktoolChainMotion motion)
    {
        ShaktoolChain chain = _chains.Of(leadingSaw).Chain;
        return chain.Motion == motion
            ? chain
            : throw new InvalidDataException(
                $"A Shaktool segment is placed moving {motion}, but its chain's leading saw in enemy place {leadingSaw.Ordinal} moving {chain.Motion}.");
    }

    /// <summary>The chain a segment found when it was placed: its segments and their shared motion and target (#2452).</summary>
    private ShaktoolChain ChainOf(RoomEnemyPlace segment) => _chains.Of(segment).Chain;

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

    /// <summary>Places the segment on its orbit around the segment before it in the chain.</summary>
    private void PositionShaktoolAroundPreviousSegment(
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
    private static FixedVector<RoomPixels> OrbitDisplacement(TrigonometryAngle angle) => new(
        ShaktoolOrbitDefinitions.OrbitSine.At(angle), ShaktoolOrbitDefinitions.OrbitSine.At(angle - TrigonometryAngle.QuarterTurn));

    /// <summary>
    /// Places the segment on its orbit and turns the orbit on its chain's way round, by the segment's own angular speed, or
    /// by the straightening turn while the chain straightens.
    /// </summary>
    private void AdvanceShaktoolOrbit(RoomEnemyPlace place, ShaktoolSegmentState state)
    {
        PositionShaktoolAroundPreviousSegment(place, state);
        ShaktoolChainMotion motion = ChainOf(place).Motion;
        RotationAngle increment = motion.Phase == ShaktoolChainPhase.Straightening ? ShaktoolOrbitDefinitions.StraighteningTurn : state.AngularVelocity;
        state.OrbitAngle = motion.OrbitDirection == RotationDirection.Clockwise
            ? state.OrbitAngle + increment
            : state.OrbitAngle - increment;
    }

    /// <summary>
    /// Orbits the chain's centre segment and turns it to face halfway between the segments either side of it, showing the
    /// head's picture for that facing (#2284, #2452).
    /// </summary>
    private void AdvanceAndOrientShaktoolCenter(
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
        // The head shows the picture its facing chooses in place of its animation, which takes no further step until a
        // wall's head bob starts it again.
        place.Frame = ShaktoolHeadPictures.Aiming(state.Orientation);
        place.AnimationClock.Stop();
    }

    /// <summary>
    /// Moves the chain's last segment along its orbit through the room's terrain: striking terrain reverses the chain;
    /// otherwise the segment curls the chain toward its target until it straightens.
    /// </summary>
    private void DriveShaktoolTail(
        RoomEnemyPlace place,
        ShaktoolSegmentState state,
        RoomLevelData level)
    {
        Position16<RoomPixels> previous = place.Position.Whole;
        AdvanceShaktoolOrbit(place, state);
        Position16<RoomPixels> candidate = place.Position.Whole;
        Offset16<RoomPixels> step = candidate - previous;

        // Only the whole pixels go back before the terrain move; the fractions stay where the
        // orbit left them, a subtle asymmetry that affects how the segment meets a wall.
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
        ShaktoolChain chain = ChainOf(place);
        if (chain.Motion.Phase == ShaktoolChainPhase.Straightening)
            return;

        if (chain.TargetAngle.WholeStep == state.OrbitAngle.WholeStep)
        {
            SynchronizeShaktoolOrbitTargets(place, chain.TargetAngle);
            state.CurlProgress = ShaktoolOrbitDefinitions.StraighteningStart;
            chain.Motion = chain.Motion with { NextWall = ShaktoolWallResponse.TurnEndForEnd };
        }

        // Accumulation stops once the chain straightens, at most one increment past the threshold.
        state.CurlProgress += state.AngularVelocity;
        if (state.CurlProgress >= ShaktoolOrbitDefinitions.StraighteningThreshold)
            chain.Motion = chain.Motion with { Phase = ShaktoolChainPhase.Straightening };
    }

    /// <summary>
    /// Reverses the chain at terrain contact as its next wall response says: it turns end for end, or its orbit turns the
    /// other way round; either way it curls again and turns toward a new target.
    /// </summary>
    private void ReverseShaktoolAfterCollision(
        RoomEnemyPlace place,
        ShaktoolSegmentState state,
        Position16<RoomPixels> previous)
    {
        ShaktoolChain chain = ChainOf(place);
        ShaktoolChainMotion motion = chain.Motion;
        switch (motion.NextWall)
        {
            case ShaktoolWallResponse.ReverseOrbit:
                chain.Motion = new(ShaktoolChainPhase.Curling, ShaktoolWallResponse.TurnEndForEnd, motion.OrbitDirection.Reversed());
                break;

            case ShaktoolWallResponse.TurnEndForEnd:
                place.Position = place.Position.WithWhole(previous);
                ReverseShaktoolChainEndpoints(place);
                // The chain curls again, and the next wall reverses its orbit (#2284).
                chain.Motion = motion with { Phase = ShaktoolChainPhase.Curling, NextWall = ShaktoolWallResponse.ReverseOrbit };
                break;

            default:
                throw new InvalidOperationException($"Undefined {nameof(ShaktoolWallResponse)} {motion.NextWall}.");
        }

        state.ResetOrientation();
        RoomEnemyPlace owner = chain.LeadingSaw;
        // The chain measures its orbit clockwise from straight up, as its orbit's sine across reads it.
        RotationAngle bearing = (TrigonometryAngle.QuarterTurn - EnemyMath.CalculateAimAngle(
            new(place.Position.X.Whole - owner.Position.X.Whole, place.Position.Y.Whole - owner.Position.Y.Whole))).ToAngle();
        chain.TargetAngle = chain.Motion.OrbitDirection == RotationDirection.Clockwise
            ? bearing + RotationAngle.QuarterTurn
            : bearing - RotationAngle.QuarterTurn;

        // From the trailing saw back to the leading one, each segment stops and plays its head bob.
        foreach (ShaktoolChainSegment segment in GetShaktoolGroup(place).Reverse())
        {
            ShaktoolSegmentState segmentState = RequireShaktoolState(segment.Place);
            CalculateShaktoolAngularVelocity(chain, segmentState);
            segmentState.Motion = ShaktoolSegmentMotion.Idle;
            segmentState.Playback = new(ShaktoolAnimations.StartingWith(
                segment.Definition.CollisionAnimation), AnimationPlace.StartOfAnimation);
            segment.Place.AnimationClock.StepOnNextUpdate();
        }
    }

    /// <summary>
    /// Turns the chain end for end: the head's facing reverses, the segments mirrored across the chain trade orbit angles
    /// and the two saws trade places.
    /// </summary>
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

        // Every segment between the two saws takes the swap's fractions of a pixel.
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
    /// Sets the speed the segment turns toward its chain's target at: a share of the whole steps left to the target its
    /// chain's way round (<see cref="ShaktoolOrbitDefinitions.ConvergenceShares"/>).
    /// </summary>
    private static void CalculateShaktoolAngularVelocity(ShaktoolChain chain, ShaktoolSegmentState state)
    {
        RotationAngle turnLeft = chain.Motion.OrbitDirection == RotationDirection.Clockwise
            ? chain.TargetAngle - state.OrbitAngle
            : state.OrbitAngle - chain.TargetAngle;
        state.AngularVelocity = turnLeft.WholeStep.DividedBy(ShaktoolOrbitDefinitions.ConvergenceShares);
    }

    /// <summary>Puts every segment of the chain at the target angle, each turning at its own angular speed.</summary>
    private void SynchronizeShaktoolOrbitTargets(RoomEnemyPlace anySegment, RotationAngle target)
    {
        foreach (ShaktoolChainSegment segment in GetShaktoolGroup(anySegment))
        {
            ShaktoolSegmentState state = RequireShaktoolState(segment.Place);
            state.OrbitAngle = target;
            state.AngularVelocity = segment.Definition.AngularVelocity;
        }
    }

    /// <summary>
    /// The seven segments of the chain a segment found when it was placed, <paramref name="anySegment"/> among them, each
    /// validated as initialized.
    /// </summary>
    private IReadOnlyList<ShaktoolChainSegment> GetShaktoolGroup(RoomEnemyPlace anySegment)
    {
        IReadOnlyList<ShaktoolChainSegment> group = ChainOf(anySegment).Segments;
        foreach (ShaktoolChainSegment segment in group)
        {
            // The chain keeps moving through the rest of the update that kills a segment, reaching the killed segment's cleared state.
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
    private void ApplyShaktoolCue(RoomEnemyPlace place, ShaktoolCue cue)
    {
        switch (cue.Kind)
        {
            case ShaktoolCueKind.Lower:
            case ShaktoolCueKind.Raise:
            {
                TrigonometryAngle targetDirection = ChainOf(place).TargetAngle.ToTrigonometryAngle();
                MoveShaktoolSegmentForAnimation(
                    place,
                    cue.Kind == ShaktoolCueKind.Lower
                        ? targetDirection + TrigonometryAngle.HalfTurn
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

    /// <summary>Moves the segment one update along <paramref name="angle"/>, a pixel at most, as its cue raises or lowers it.</summary>
    private static void MoveShaktoolSegmentForAnimation(RoomEnemyPlace place, TrigonometryAngle angle)
    {
        // X reads the shared signed sine at the angle and Y the sine a quarter turn behind it, the negative cosine.
        SignedEightEight xVelocity = SineVelocity(angle);
        SignedEightEight yVelocity = SineVelocity(angle.TurnedBy(TrigonometryAngle.HalfTurn).TurnedBy(TrigonometryAngle.QuarterTurn));
        place.Position = place.Position.Advance(new(xVelocity.ToSixteenSixteenDelta(), yVelocity.ToSixteenSixteenDelta()));
    }

    /// <summary>The shared enemy sine velocity at <paramref name="angle"/>: one pixel per update at its peak.</summary>
    private static SignedEightEight SineVelocity(TrigonometryAngle angle) =>
        EnemyMath.SineVelocity(angle, 1);

    /// <summary>
    /// Ends Shaktool after a fatal shot: a hit that kills one segment deletes the enemies the room lists first
    /// (<see cref="ShaktoolDeathDeletesFirstListed"/>), which initialization requires to be the chain's own segments.
    /// </summary>
    private void ResolveShaktoolShotAfterCommon(RoomEnemyPlace struckSegment)
    {
        if (struckSegment.Health != 0)
            return;
        foreach (RoomEnemyPlace segment in ShaktoolDeathDeletesFirstListed.Deleted(_places.InUpdateOrder, ShaktoolSegmentDefinitions.Segments.Length))
        {
            segment.Contact = EnemyContact.None;
            segment.Processing = EnemyProcessing.None;
            segment.Presence = EnemyPresence.Deleted;
        }
    }

    /// <summary>Shaktool's reaction to a bomb: after the common damage, a lethal hit ends the whole Shaktool.</summary>
    internal EnemyBombReaction ShaktoolBombReaction => field ??= new()
    {
        AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
    };

    /// <summary>Runs Shaktool's main AI.</summary>
    internal void RunShaktoolMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunShaktoolMain(place, RequireShaktoolState(place), frame.Level);
    }

    /// <summary>
    /// Advances Shaktool's typed animation. The head's clock stays stopped while its chain orients it, so its animation
    /// takes no step and the picture its facing chose stays shown.
    /// </summary>
    internal void AdvanceShaktoolTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        var state = RequireShaktoolState(place);
        AnimationPlayback<ShaktoolSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, ShaktoolAnimations.Segments, ref playback,
            cue =>
            {
                ApplyShaktoolCue(place, cue);
                return null;
            });
        state.Playback = playback;
    }

    /// <summary>Shaktool contact is the common normal-enemy touch, death animation included.</summary>
    internal void ReactToShaktoolTouch(EnemyTouchContext context) =>
        _collision.ResolveOrdinaryTouch(context);

    /// <summary>Shaktool's reaction to a shot: after the common damage, a lethal hit ends the whole Shaktool.</summary>
    internal EnemyShotReaction ShaktoolShotReaction => field ??= new()
    {
        AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
    };
}
