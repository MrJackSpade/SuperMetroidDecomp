using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Shaktool.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>The Shaktool species owner: its per-room state, AI routines and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class ShaktoolBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Shaktool];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ShaktoolEntity(this, slot, common);

    /// <summary>Checks terrain collisions along each segment's orbit and attack path.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Allocates the linked Shaktool attack circles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>The room's enemy places, where each segment finds its chain when it is placed.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>The chain and neighbours each segment found when it was placed.</summary>
    private readonly EnemyPartLinks<ShaktoolChainPlaces> _chains = new();
    /// <summary>The segment a hit last killed, whose remains the rest of its chain reads for the rest of that update.</summary>
    private readonly KilledShaktoolSegment<RoomEnemySlot, ShaktoolSegmentState> _killedSegment = new();
    /// <summary>Runs common contact behavior for the segment hitboxes.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Binds collision, projectile, and slot owners for a room's Shaktool chain.</summary>
    internal ShaktoolBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision)
    {
        _terrain = terrain;
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Head, five middle segments, and tail occupy consecutive enemy slots.</summary>
    private const int ShaktoolSegmentCount = 7;

    /// <summary>The whole step half a turn from <paramref name="angle"/>, its fraction cleared.</summary>
    private static RotationAngle OppositeOrbitStep(RotationAngle angle) =>
        (angle + RotationAngle.HalfTurn).WholeStep;

    /// <summary>Initializes the Shaktool, establishing the segment's orbit and owner-slot offset.</summary>
    internal void InitializeShaktool(RoomEnemySlot slot)
    {
        ShaktoolSpawnSettings settings = slot.SpawnSettings<ShaktoolSpawnSettings>();
        ShaktoolSegmentDefinition definition = settings.Segment;
        var state = new ShaktoolSegmentState();
        slot.SetSpeciesState(state);

        // The population entry names the chain's initial motion; the chain then writes its flags to every segment
        // together.
        state.MotionFlags = settings.Motion;
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.TargetAngle = RotationAngle.Zero;
        state.ResetOrientation();
        slot.Processing = slot.Processing.With(definition.Processing);
        slot.Contact = slot.Contact.With(definition.Contact);
        // The segment's definition names how far after the chain's leading saw the room lists it. A kill empties the
        // struck segment's owner to the first enemy, and the chain's death then deletes the seven enemies from the
        // first. Both retail Shaktool room states list the chain first, so those are its own segments; a chain
        // anywhere else would delete unrelated enemies, which no retail room reaches.
        RoomEnemySlot? owner = definition.OwnerSlotDistance == 0
            ? slot
            : _slotTable.ListedBeforeOrNone(slot, definition.OwnerSlotDistance);
        if (owner is null || !owner.IsListedFirst())
        {
            throw new InvalidDataException(
                $"Shaktool's chain starts in enemy slot {slot.SlotIndex - definition.OwnerSlotDistance}; its death deletes the first seven slots, " +
                "where both retail Shaktool room states place it.");
        }
        _chains.Place(slot, new ShaktoolChainPlaces(
            Chain: [owner, .. _slotTable.ListedRunAfter(owner, ShaktoolSegmentCount - 1)],
            Previous: _slotTable.ListedBeforeOrNone(slot, 1),
            Next: _slotTable.ListedAfterOrNone(slot, 1)));
        state.FrameBehavior = definition.FrameBehavior;
        // Initialization subtracts the parallel all-zero table; synchronization
        // uses the same authored velocity directly.
        state.AngularVelocity = definition.AngularVelocity;
        state.OrbitAngle = definition.InitialOrbitAngle;

        // The segment's definition names the animation it starts.
        state.Program = AnimationProgramCursor<ShaktoolProgram>.Start(definition.InitialProgram);
        slot.Layer = definition.Layer;

        // The leading saw is the chain's fixed anchor. Every later segment's curious drawing-queue
        // read aliases the preceding enemy record's X/sub-X/Y/sub-Y words. Express
        // that alias structurally instead of treating queue scratch as an unrelated center.
        if (definition != ShaktoolSegmentDefinitions.LeadingSaw)
            PositionShaktoolAroundPreviousSegment(slot, state);
    }

    /// <summary>Runs Shaktool's hurt and main behaviours.</summary>
    private void RunShaktoolMain(
        RoomEnemySlot slot,
        ShaktoolSegmentState state,
        RoomLevelData? level)
    {
        switch (state.FrameBehavior)
        {
            case ShaktoolFrameBehavior.Idle:
                return;

            case ShaktoolFrameBehavior.OrbitPreviousSegment:
                AdvanceShaktoolOrbit(slot, state);
                return;

            case ShaktoolFrameBehavior.OrbitAndOrientCenter:
                AdvanceAndOrientShaktoolCenter(slot, state);
                return;

            case ShaktoolFrameBehavior.DriveTailAndReverseAtWalls:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Shaktool tail collision requires the active room level.");
                }
                DriveShaktoolTail(slot, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Shaktool frame behaviour {state.FrameBehavior?.ToString() ?? "none"} " +
                    "is not translated.");
        }
    }

    /// <summary>Runs the linked-segment placement helper.</summary>
    internal void PositionShaktoolAroundPreviousSegment(
        RoomEnemySlot slot,
        ShaktoolSegmentState state)
    {
        RoomEnemySlot previous = _chains.Of(slot).Previous
            ?? throw new InvalidOperationException("Shaktool's anchor has no preceding segment.");
        // The segment placement reads the preceding segment's place without testing it. A lethal shot clears the
        // struck segment and deletes the chain, but the later segments still run this update; their final orbit uses
        // the cleared coordinates.
        slot.Position = previous.Position.Advance(ShaktoolOrbitTables.Displacement(state.OrbitAngle.ToTrigonometryAngle()));
    }


    /// <summary>Runs the common orbit step.</summary>
    private void AdvanceShaktoolOrbit(RoomEnemySlot slot, ShaktoolSegmentState state)
    {
        PositionShaktoolAroundPreviousSegment(slot, state);
        ShaktoolMotionFlags flags = state.MotionFlags;
        int increment;
        if ((flags & ShaktoolMotionFlags.Straightening) != ShaktoolMotionFlags.None)
        {
            state.TargetAngle = state.TargetAngle.AddRaw(ShaktoolOrbitDefinitions.TableStep);
            increment = ShaktoolOrbitDefinitions.TableStep;
        }
        else
        {
            increment = state.AngularVelocity;
        }

        state.OrbitAngle = (flags & ShaktoolMotionFlags.Clockwise) != ShaktoolMotionFlags.None
            ? state.OrbitAngle.AddRaw(-increment)
            : state.OrbitAngle.AddRaw(increment);
    }

    /// <summary>Runs middle-segment orientation selection.</summary>
    internal void AdvanceAndOrientShaktoolCenter(
        RoomEnemySlot slot,
        ShaktoolSegmentState state)
    {
        AdvanceShaktoolOrbit(slot, state);
        RoomEnemySlot nextSlot = GetNextShaktoolSegment(slot);
        ShaktoolSegmentState next = RequireShaktoolState(nextSlot);

        // The heading lies halfway along the turn from the angle opposite this segment's on to the next segment's.
        RotationAngle oppositeAngle = state.OrbitAngle.AddRaw(RotationAngle.HalfTurn.RawValue);
        RotationAngle midpoint = oppositeAngle.AddRaw(oppositeAngle.UnitsTo(next.OrbitAngle) / 2);
        if (state.OrientationReversed)
            midpoint = midpoint.AddRaw(RotationAngle.HalfTurn.RawValue);

        RotationAngle heading = midpoint.TurnedBySteps(ShaktoolOrbitDefinitions.OrientationBucketLead);
        int directionBucket = heading.Sector(ShaktoolOrbitDefinitions.OrientationBuckets) *
            (RotationAngle.StepsPerTurn / ShaktoolOrbitDefinitions.OrientationBuckets);
        state.OrientationBucket = directionBucket;
        state.Program = AnimationProgramCursor<ShaktoolProgram>.Start(
            ShaktoolPrograms.ForOrientationBucket(directionBucket));
        slot.FrameHold = WrappingCountdown.Start(1);
    }

    /// <summary>Runs the last segment's collision/reversal controller.</summary>
    private void DriveShaktoolTail(
        RoomEnemySlot slot,
        ShaktoolSegmentState state,
        RoomLevelData level)
    {
        Position16<RoomPixels> previous = slot.Position.Whole;
        AdvanceShaktoolOrbit(slot, state);
        Position16<RoomPixels> candidate = slot.Position.Whole;
        Offset16<RoomPixels> step = candidate - previous;

        // The helper restores only whole pixels before collision. Subpositions
        // remain those the orbit step produced, a subtle asymmetry that affects wall alignment.
        slot.Position = slot.Position.WithWhole(previous);
        SignedSixteenSixteen xDisplacement = SignedSixteenSixteen.FromWholePixels(step.X);
        bool collided = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            xDisplacement);
        if (!collided)
        {
            slot.Position = slot.Position.WithWholeY(previous.Y);
            SignedSixteenSixteen yDisplacement = SignedSixteenSixteen.FromWholePixels(step.Y);
            collided = _terrain.MoveEnemyVertically(level, slot, yDisplacement);
        }

        if (collided)
        {
            ReverseShaktoolAfterCollision(slot, state, previous);
            return;
        }

        slot.Position = slot.Position.WithWhole(candidate);
        ShaktoolMotionFlags flags = state.MotionFlags;
        if ((flags & ShaktoolMotionFlags.Straightening) != ShaktoolMotionFlags.None)
        {
            // The straightening step deliberately adds a second step after the first increment.
            state.TargetAngle = state.TargetAngle.AddRaw(ShaktoolOrbitDefinitions.TableStep);
            return;
        }

        if (state.TargetAngle.WholeStep == state.OrbitAngle.WholeStep)
        {
            SynchronizeShaktoolOrbitTargets(slot, state.TargetAngle);
            // The game keeps only the high byte of the restarted progress, which is already whole.
            state.CurlProgress = ShaktoolOrbitDefinitions.StraighteningStart;
            SetShaktoolGroupMotionFlags(
                slot,
                state.MotionFlags & ~ShaktoolMotionFlags.ReversedOnce);
        }

        // Accumulation stops once the chain straightens, at most one increment past the threshold.
        state.CurlProgress += state.AngularVelocity;
        if (state.CurlProgress >= ShaktoolOrbitDefinitions.StraighteningThreshold)
        {
            SetShaktoolGroupMotionFlags(
                slot,
                state.MotionFlags | ShaktoolMotionFlags.Straightening);
        }
    }

    /// <summary>Reverses chain direction at terrain contact, including the first-hit endpoint swap and synchronized flags.</summary>
    internal void ReverseShaktoolAfterCollision(
        RoomEnemySlot slot,
        ShaktoolSegmentState state,
        Position16<RoomPixels> previous)
    {
        ShaktoolMotionFlags flags = state.MotionFlags;
        if ((flags & ShaktoolMotionFlags.ReversedOnce) != ShaktoolMotionFlags.None)
        {
            SetShaktoolGroupMotionFlags(
                slot,
                (flags ^ ShaktoolMotionFlags.Clockwise) &
                ~(ShaktoolMotionFlags.ReversedOnce | ShaktoolMotionFlags.Straightening));
        }
        else
        {
            slot.Position = slot.Position.WithWhole(previous);
            ReverseShaktoolChainEndpoints(slot);

            // The group's motion flags are set twice. The second call reads the parameter just written
            // by the first, leaving the reversed-once bit set while clearing the straightening bit.
            SetShaktoolGroupMotionFlags(slot, flags | ShaktoolMotionFlags.ReversedOnce);
            SetShaktoolGroupMotionFlags(
                slot,
                state.MotionFlags & ~ShaktoolMotionFlags.Straightening);
        }

        state.ResetOrientation();
        RoomEnemySlot owner = OwnerSlot(slot);
        RotationAngle target = EnemyMath.CalculateBearing(
            new(CoordinateAxis.Difference(slot.Position.X.Whole, owner.Position.X.Whole), CoordinateAxis.Difference(slot.Position.Y.Whole, owner.Position.Y.Whole))).ToMirroredTableAngle().ToAngle();
        target = (state.MotionFlags & ShaktoolMotionFlags.Clockwise) != ShaktoolMotionFlags.None
            ? target.AddRaw(-RotationAngle.QuarterTurn.RawValue)
            : target.AddRaw(RotationAngle.QuarterTurn.RawValue);
        SetShaktoolGroupTargetAngle(slot, target);

        RoomEnemySlot[] group = GetShaktoolGroup(slot);
        for (int index = ShaktoolSegmentCount - 1; index >= 0; index--)
        {
            RoomEnemySlot segment = group[index];
            ShaktoolSegmentState segmentState = RequireShaktoolState(segment);
            CalculateShaktoolAngularVelocity(segmentState);
            segmentState.FrameBehavior = ShaktoolFrameBehavior.Idle;
            segmentState.Program = AnimationProgramCursor<ShaktoolProgram>.Start(
                ShaktoolPrograms.CollisionForSegment(index));
            segment.FrameHold = WrappingCountdown.Start(1);
        }
    }

    /// <summary>Runs the endpoint/angle reversal helper.</summary>
    private void ReverseShaktoolChainEndpoints(RoomEnemySlot anySegment)
    {
        RoomEnemySlot[] group = GetShaktoolGroup(anySegment);
        ShaktoolSegmentState center = RequireShaktoolState(group[3]);
        center.OrientationReversed = !center.OrientationReversed;

        SwapMirroredShaktoolAngles(group[6], group[1]);
        SwapMirroredShaktoolAngles(group[5], group[2]);
        SwapMirroredShaktoolAngles(group[4], group[3]);

        RoomEnemySlot head = group[0];
        RoomEnemySlot tail = group[6];
        {
            var held0 = head.Position;
            var held1 = tail.Position;
            tail.Position = held0;
            head.Position = held1;
        }

        for (int index = 1; index < ShaktoolSegmentCount - 1; index++)
        {
            group[index].Position = group[index].Position.WithFractionX(ShaktoolOrbitDefinitions.SwapFraction).WithFractionY(ShaktoolOrbitDefinitions.SwapFraction);
        }
    }

    /// <summary>Swaps two segment orbit angles after rotating each by half a turn.</summary>
    private void SwapMirroredShaktoolAngles(RoomEnemySlot left, RoomEnemySlot right)
    {
        ShaktoolSegmentState leftState = RequireShaktoolState(left);
        ShaktoolSegmentState rightState = RequireShaktoolState(right);
        RotationAngle saved = leftState.OrbitAngle;
        leftState.OrbitAngle = OppositeOrbitStep(rightState.OrbitAngle);
        rightState.OrbitAngle = OppositeOrbitStep(saved);
    }

    /// <summary>Runs the per-segment convergence speed calculation: four raw angle units per whole step left to the target in the orbit direction.</summary>
    private static void CalculateShaktoolAngularVelocity(ShaktoolSegmentState state)
    {
        RotationAngle turnLeft = (state.MotionFlags & ShaktoolMotionFlags.Clockwise) != ShaktoolMotionFlags.None
            ? state.OrbitAngle - state.TargetAngle
            : state.TargetAngle - state.OrbitAngle;
        state.AngularVelocity = 4 * turnLeft.ToTrigonometryAngle().RawValue;
    }

    /// <summary>Runs group synchronization.</summary>
    private void SynchronizeShaktoolOrbitTargets(RoomEnemySlot anySegment, RotationAngle target)
    {
        RoomEnemySlot[] group = GetShaktoolGroup(anySegment);
        for (int index = 0; index < ShaktoolSegmentCount; index++)
        {
            ShaktoolSegmentState state = RequireShaktoolState(group[index]);
            state.OrbitAngle = target;
            state.AngularVelocity = ShaktoolSegments.ForIndex(index).AngularVelocity;
        }
    }

    /// <summary>Publishes a shared convergence target to all seven initialized segments.</summary>
    private void SetShaktoolGroupTargetAngle(RoomEnemySlot anySegment, RotationAngle target)
    {
        foreach (RoomEnemySlot segment in GetShaktoolGroup(anySegment))
            RequireShaktoolState(segment).TargetAngle = target;
    }

    /// <summary>Writes the motion flags to the entire chain.</summary>
    private void SetShaktoolGroupMotionFlags(
        RoomEnemySlot anySegment,
        ShaktoolMotionFlags flags)
    {
        foreach (RoomEnemySlot segment in GetShaktoolGroup(anySegment))
            RequireShaktoolState(segment).MotionFlags = flags;
    }

    /// <summary>The chain's leading saw, which the segment found when it was placed.</summary>
    private RoomEnemySlot OwnerSlot(RoomEnemySlot segment) => _chains.Of(segment).Chain[0];

    /// <summary>The seven segments of the chain a segment found when it was placed, each validated as initialized.</summary>
    private RoomEnemySlot[] GetShaktoolGroup(RoomEnemySlot anySegment)
    {
        _ = RequireShaktoolState(anySegment);
        RoomEnemySlot[] group = _chains.Of(anySegment).Chain;
        for (int index = 0; index < group.Length; index++)
        {
            // Group helpers also run during the remainder of the lethal update, reaching the killed segment's remains.
            if (group[index].SpeciesState<ShaktoolSegmentState>() is null && _killedSegment.RemainsAt(group[index]) is null)
            {
                throw new InvalidDataException(
                    $"Shaktool group rooted at slot {group[0].SlotIndex} is missing segment {index}.");
            }
        }
        return group;
    }

    /// <summary>The enemy listed after a linked segment center, which it found when it was placed.</summary>
    private RoomEnemySlot GetNextShaktoolSegment(RoomEnemySlot slot) =>
        _chains.Of(slot).Next ?? throw new InvalidDataException(
            $"Shaktool center slot {slot.SlotIndex} has no following segment.");

    /// <summary>
    /// The state of the segment in <paramref name="slot"/>: its own, or, for the segment a hit killed this update, the
    /// emptied remains the rest of its chain reads in its place (<see cref="KilledShaktoolSegment{TPlace, TSegment}"/>).
    /// </summary>
    private ShaktoolSegmentState RequireShaktoolState(RoomEnemySlot slot) =>
        slot.SpeciesState<ShaktoolSegmentState>() ?? _killedSegment.RemainsAt(slot) ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Shaktool state.");

    /// <summary>Carries out one cue of a Shaktool segment's animation.</summary>
    internal ShaktoolProgram? ApplyShaktoolCue(RoomEnemySlot slot, ShaktoolSegmentState state, ShaktoolCue cue)
    {
        switch (cue.Kind)
        {
            case ShaktoolCueKind.LowerAwayFromCenter:
            case ShaktoolCueKind.RaiseTowardCenter:
            {
                RoomEnemySlot[] group = GetShaktoolGroup(slot);
                // Flipping the direction's top bit turns it half a turn.
                TrigonometryAngle centerDirection = new(RequireShaktoolState(group[3]).OrientationBucket);
                MoveShaktoolSegmentForAnimation(
                    slot,
                    cue.Kind == ShaktoolCueKind.LowerAwayFromCenter
                        ? centerDirection.TurnedBySteps(EnemyTrigonometryAngleDefinitions.HalfTurn)
                        : centerDirection);
                break;
            }

            case ShaktoolCueKind.Lower:
            case ShaktoolCueKind.Raise:
            {
                TrigonometryAngle targetDirection = state.TargetAngle.ToTrigonometryAngle();
                MoveShaktoolSegmentForAnimation(
                    slot,
                    cue.Kind == ShaktoolCueKind.Lower
                        ? targetDirection.TurnedBySteps(EnemyTrigonometryAngleDefinitions.HalfTurn)
                        : targetDirection);
                break;
            }

            case ShaktoolCueKind.Nothing:
                // Explicit empty step used between the two long attack pauses.
                break;

            case ShaktoolCueKind.ResetFunctions:
            {
                RoomEnemySlot[] group = GetShaktoolGroup(slot);
                for (int index = 0; index < ShaktoolSegmentCount; index++)
                {
                    RequireShaktoolState(group[index]).FrameBehavior =
                        ShaktoolSegments.ForIndex(index).FrameBehavior;
                }
                break;
            }

            default:
                throw new InvalidOperationException($"Undefined {nameof(ShaktoolCueKind)} {cue.Kind}.");
        }

        return null;
    }

    /// <summary>Runs the one-frame sine movement helper.</summary>
    private static void MoveShaktoolSegmentForAnimation(RoomEnemySlot slot, TrigonometryAngle angle)
    {
        // The decompilation names a 320-word view whose indices 64..319 are the game's
        // 256-word table. Convert those full-view indices explicitly: X reads
        // full[angle+64], while Y reads full[angle]. A naïve +64 on the authored table rotates
        // both components by a quarter turn.
        SignedEightEight xVelocity = ReadShaktoolCommonSineSample(angle.RawValue + 64);
        SignedEightEight yVelocity = ReadShaktoolCommonSineSample(angle.RawValue);
        slot.Position = slot.Position.Advance(new(xVelocity.ToSixteenSixteenDelta(), yVelocity.ToSixteenSixteenDelta()));
    }

    /// <summary>Maps the Shaktool orbit index into the shared signed enemy sine table.</summary>
    internal static SignedEightEight ReadShaktoolCommonSineSample(int fullTableIndex) =>
        SignedEightEight.FromSignedRaw(Trigonometry.SignedSine(TrigonometryAngle.Wrapped(fullTableIndex - 64)));

    /// <summary>
    /// Implements Shaktool's fatal-shot tail: a hit that kills one segment deletes the seven enemies from the room's
    /// first, which initialization requires to be the chain's own segments.
    /// </summary>
    private void ResolveShaktoolShotAfterCommon(RoomEnemySlot struckSegment)
    {
        if (struckSegment.Health != 0)
            return;
        // The generic death clears the entire common slot, which empties the segment's owner to the first enemy.
        _killedSegment.RecordKill(struckSegment);
        foreach (RoomEnemySlot segment in _chains.Of(struckSegment).Chain)
        {
            segment.Contact = EnemyContact.None;
            segment.Processing = EnemyProcessing.None;
            segment.Presence = EnemyPresence.Deleted;
        }
    }

    /// <summary>Runs the post-common lethal-shot tail for bomb hits.</summary>
    internal void AddShaktoolBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Shaktool &&
                                                       c.SelectedShotAi == EnemyReaction.ShaktoolShot)
        {
            AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Registers front and linked attack-circle movement callbacks.</summary>
    internal void AddShaktoolProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            RunShaktoolFrontCircleFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.ShaktoolFrontCircleFlight);

        table.AddFrameBehavior(context =>
        {
            RunShaktoolLinkedCircleFrameBehavior(context.Projectile, context.Level);
        }, EnemyProjectileFrameBehavior.ShaktoolTrailingCircleFlight);
    }

    /// <summary>Ports Shaktool's main AI.</summary>
    internal void RunShaktoolMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunShaktoolMain(slot, RequireShaktoolState(slot), frame.Level);
    }

    /// <summary>Advances Shaktool's typed animation.</summary>
    internal void AdvanceShaktoolTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireShaktoolState(slot);
        AnimationProgramCursor<ShaktoolProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, ShaktoolAnimations.Programs, ref cursor, ShaktoolFrameDefinitions.ShownFrames.Of,
                (cue, _) => ApplyShaktoolCue(slot, state, cue));
        state.Program = cursor;
    }

    /// <summary>Shaktool contact is the common normal-enemy touch, death animation included.</summary>
    internal void AddShaktoolTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Shaktool &&
                               EnemyCollision.TouchAi(slot) == EnemyReaction.ShaktoolTouch,
            context => _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus));
    }

    /// <summary>Runs the post-common lethal-shot tail for regular projectile hits.</summary>
    internal void AddShaktoolShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Shaktool &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.ShaktoolShot)
        {
            AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
        });
    }
    /// <summary>Runs the front circle's per-frame behaviour.</summary>
    internal static void RunShaktoolFrontCircleFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Delete();
        }
    }

    /// <summary>
    /// The projectile slot a middle or back attack circle follows. Nothing that spawns the circles names the circle they
    /// follow, so they follow the first slot and vanish once it is free.
    /// </summary>
    private const int LinkedCircleOwnerSlot = 0;

    /// <summary>Runs the middle and back linked circles' per-frame behaviour.</summary>
    internal void RunShaktoolLinkedCircleFrameBehavior(
        EnemyProjectile projectile,
        RoomLevelData level)
    {
        if (!_projectiles.EnemyProjectiles[LinkedCircleOwnerSlot].IsActive)
        {
            projectile.Delete();
            return;
        }

        _ = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
        _ = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
    }
}
