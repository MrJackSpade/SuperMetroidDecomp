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
    /// <summary>Physical enemy slots containing the seven-segment chain.</summary>
    private readonly EnemySlotTable _slotTable;
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

    /// <summary>The whole table step half a turn from <paramref name="angle"/>, its fraction cleared.</summary>
    private static RotationAngle OppositeOrbitStep(RotationAngle angle) =>
        RotationAngle.FromTableIndex(angle.AddRaw(RotationAngle.HalfTurn.RawValue).TableIndex);

    /// <summary>Ports <c>Shaktool_Init</c> at <c>$AA:DE43</c>, establishing the segment's orbit and owner-slot offset.</summary>
    internal void InitializeShaktool(RoomEnemySlot slot)
    {
        if ((slot.Spawn.Population.Parameter2 & 1) != 0 || slot.Spawn.Population.Parameter2 > 12)
        {
            throw new InvalidDataException(
                $"Shaktool parameter two ${slot.Spawn.Population.Parameter2:X4} is not an even index 0..12.");
        }

        int segmentIndex = slot.Spawn.Population.Parameter2 >> 1;
        ShaktoolSegmentDefinition definition = ShaktoolSegments.ForIndex(segmentIndex);
        var state = new ShaktoolSegmentState();
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // The population's first parameter holds the chain's initial motion flags; the chain then writes its flags
        // to every segment together.
        state.MotionFlags = InitialMotionFlags(slot.Spawn.Population.Parameter1);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        state.TargetAngle = RotationAngle.Zero;
        state.ResetOrientation();
        slot.Properties = slot.Properties.With(definition.PropertyMask);
        state.OwnerSlotIndex = slot.SlotIndex - definition.OwnerSlotDistance;
        state.PreInstruction = definition.PreInstruction;
        // Native initialization subtracts the parallel all-zero table; synchronization
        // uses the same authored velocity directly.
        state.AngularVelocity = definition.AngularVelocity;
        state.OrbitAngle = definition.InitialOrbitAngle;

        // The second half of $DEB1 is a parallel list-pointer table. Parameter two is
        // already a byte offset, so +14+p2 selects exactly entries seven through thirteen;
        // the definition names the animation each selects.
        state.Program = AnimationProgramCursor<ShaktoolProgram>.Start(definition.InitialProgram);
        slot.Layer = definition.Layer;

        // Slot zero is the fixed head/anchor. Every later segment's curious drawing-queue
        // read aliases the preceding enemy record's X/sub-X/Y/sub-Y words in WRAM. Express
        // that alias structurally instead of treating queue scratch as an unrelated center.
        if (segmentIndex != 0)
            PositionShaktoolAroundPreviousSegment(slot, state);
    }

    /// <summary>Ports <c>Shaktool_Hurt</c>/<c>Shaktool_Main</c> at <c>$AA:DCA3</c>.</summary>
    private void RunShaktoolMain(
        RoomEnemySlot slot,
        ShaktoolSegmentState state,
        RoomLevelData? level)
    {
        switch (state.PreInstruction)
        {
            case ShaktoolPreInstruction.IdleAfterAttack:
            case ShaktoolPreInstruction.IdleHead:
                return;

            case ShaktoolPreInstruction.OrbitPreviousSegment:
                AdvanceShaktoolOrbit(slot, state);
                return;

            case ShaktoolPreInstruction.OrbitAndOrientCenter:
                AdvanceAndOrientShaktoolCenter(slot, state);
                return;

            case ShaktoolPreInstruction.DriveTailAndReverseAtWalls:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Shaktool tail collision requires the active room level.");
                }
                DriveShaktoolTail(slot, state, level);
                return;

            default:
                throw new InvalidDataException(
                    $"Shaktool pre-instruction {state.PreInstruction?.ToString() ?? "none"} " +
                    "is not translated.");
        }
    }

    /// <summary>Ports the linked-segment placement helper at <c>$AA:DC2A</c>.</summary>
    internal void PositionShaktoolAroundPreviousSegment(
        RoomEnemySlot slot,
        ShaktoolSegmentState state)
    {
        if (slot.SlotIndex == 0)
            throw new InvalidOperationException("Shaktool's anchor has no preceding segment.");

        RoomEnemySlot previous = _slotTable.Slots[slot.SlotIndex - 1];
        // $AA:DC42/$DC4A/$DC60/$DC68 read the preceding physical record, without
        // testing its header. A lethal shot clears the head and marks the group
        // deleted, but EnemyMain still visits later entries in this frame's
        // prebuilt active list. Their final orbit uses the cleared coordinates.

        slot.Position = previous.Position.Advance(ShaktoolOrbitTables.Displacement(state.OrbitAngle.ToByteAngle()));
    }

    /// <summary>
    /// The chain's initial motion state from the population's first parameter: bit 13 reversed once, bit 14
    /// straightening, bit 15 clockwise. No other bit of the word is read.
    /// </summary>
    private static ShaktoolMotionFlags InitialMotionFlags(int parameter)
    {
        static bool Has(int parameter, int bit) => (parameter >> bit & 1) != 0;
        return (Has(parameter, 13) ? ShaktoolMotionFlags.ReversedOnce : 0) |
            (Has(parameter, 14) ? ShaktoolMotionFlags.Straightening : 0) |
            (Has(parameter, 15) ? ShaktoolMotionFlags.Clockwise : 0);
    }

    /// <summary>Ports the common orbit step at <c>$AA:DCAC</c>.</summary>
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

    /// <summary>Ports middle-segment orientation selection at <c>$AA:DCD7</c>.</summary>
    internal void AdvanceAndOrientShaktoolCenter(
        RoomEnemySlot slot,
        ShaktoolSegmentState state)
    {
        AdvanceShaktoolOrbit(slot, state);
        RoomEnemySlot nextSlot = GetNextShaktoolSegment(slot);
        ShaktoolSegmentState next = RequireShaktoolState(nextSlot);

        int oppositeAngle = unchecked((ushort)(state.OrbitAngle.RawValue ^ RotationAngle.HalfTurn.RawValue));
        int midpoint = unchecked((ushort)(oppositeAngle +
            (unchecked((ushort)(next.OrbitAngle.RawValue - oppositeAngle)) >> 1)));
        if (state.OrientationReversed)
            midpoint ^= RotationAngle.HalfTurn.RawValue;

        int directionBucket = unchecked((ushort)((RotationAngle.FromRaw(checked((ushort)midpoint)).TableIndex + 8) & ShaktoolOrbitDefinitions.DirectionBucketMask));
        state.OrientationBucket = directionBucket;
        state.Program = AnimationProgramCursor<ShaktoolProgram>.Start(
            ShaktoolPrograms.ForOrientationBucket(directionBucket));
        slot.InstructionTimer = WrappingCountdown.Start(1);
    }

    /// <summary>Ports the last segment's collision/reversal controller at <c>$AA:DD25</c>.</summary>
    private void DriveShaktoolTail(
        RoomEnemySlot slot,
        ShaktoolSegmentState state,
        RoomLevelData level)
    {
        Position16<RoomPixels> previous = slot.Position.Whole;
        AdvanceShaktoolOrbit(slot, state);
        Position16<RoomPixels> candidate = slot.Position.Whole;
        Offset16<RoomPixels> step = candidate - previous;

        // The native helper restores only whole-pixel words before collision. Subpositions
        // remain those produced by DC2A, a subtle asymmetry that affects wall alignment.
        slot.Position = slot.Position.WithWhole(previous);
        int xDisplacement = SixteenSixteen.FromWhole(step.X);
        bool collided = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            xDisplacement);
        if (!collided)
        {
            slot.Position = slot.Position.WithWholeY(previous.Y);
            int yDisplacement = SixteenSixteen.FromWhole(step.Y);
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
            // DD25 deliberately adds a second $0100 after DCAC's first increment.
            state.TargetAngle = state.TargetAngle.AddRaw(ShaktoolOrbitDefinitions.TableStep);
            return;
        }

        if (state.TargetAngle.TableIndex == state.OrbitAngle.TableIndex)
        {
            SynchronizeShaktoolOrbitTargets(slot, state.TargetAngle);
            // The cartridge keeps only the high byte of the restarted progress, which is already whole.
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

            // DB0E is called twice. The second call reads the parameter word just written
            // by the first, leaving $2000 set while clearing the $4000 straightening bit.
            SetShaktoolGroupMotionFlags(slot, flags | ShaktoolMotionFlags.ReversedOnce);
            SetShaktoolGroupMotionFlags(
                slot,
                state.MotionFlags & ~ShaktoolMotionFlags.Straightening);
        }

        state.ResetOrientation();
        RoomEnemySlot owner = OwnerSlot(state);
        RotationAngle target = EnemyMath.CalculateBearing(
            new(unchecked((short)(slot.Position.X.Whole - owner.Position.X.Whole)), unchecked((short)(slot.Position.Y.Whole - owner.Position.Y.Whole)))).ReinterpretAsTableAngle().ToAngle();
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
            segmentState.PreInstruction = ShaktoolPreInstruction.IdleAfterAttack;
            segmentState.Program = AnimationProgramCursor<ShaktoolProgram>.Start(
                ShaktoolPrograms.CollisionForSegment(index));
            segment.InstructionTimer = WrappingCountdown.Start(1);
        }
    }

    /// <summary>Ports the endpoint/angle reversal helper at <c>$AA:DB59</c>.</summary>
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
    private static void SwapMirroredShaktoolAngles(RoomEnemySlot left, RoomEnemySlot right)
    {
        ShaktoolSegmentState leftState = RequireShaktoolState(left);
        ShaktoolSegmentState rightState = RequireShaktoolState(right);
        RotationAngle saved = leftState.OrbitAngle;
        leftState.OrbitAngle = OppositeOrbitStep(rightState.OrbitAngle);
        rightState.OrbitAngle = OppositeOrbitStep(saved);
    }

    /// <summary>Ports the per-segment convergence speed calculation at <c>$AA:DC07</c>: four raw angle units per whole table step left to the target in the orbit direction.</summary>
    private static void CalculateShaktoolAngularVelocity(ShaktoolSegmentState state)
    {
        int difference = (state.MotionFlags & ShaktoolMotionFlags.Clockwise) != ShaktoolMotionFlags.None
            ? unchecked((ushort)(state.OrbitAngle.RawValue - state.TargetAngle.RawValue))
            : unchecked((ushort)(state.TargetAngle.RawValue - state.OrbitAngle.RawValue));
        state.AngularVelocity = 4 * RotationAngle.FromRaw(checked((ushort)difference)).TableIndex;
    }

    /// <summary>Ports group synchronization at <c>$AA:DC6F</c>.</summary>
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

    /// <summary>The slot of the chain's first segment, which must be one of the room's enemy slots.</summary>
    private RoomEnemySlot OwnerSlot(ShaktoolSegmentState state)
    {
        if (state.OwnerSlotIndex is < 0 or >= EnemySlotTable.MaximumEnemyCount)
            throw new ArgumentOutOfRangeException(nameof(state), $"Shaktool owner slot {state.OwnerSlotIndex} is not an enemy slot.");
        return _slotTable.Slots[state.OwnerSlotIndex];
    }

    /// <summary>Resolves and validates seven consecutive slots from a segment's owner slot.</summary>
    private RoomEnemySlot[] GetShaktoolGroup(RoomEnemySlot anySegment)
    {
        ShaktoolSegmentState state = RequireShaktoolState(anySegment);
        int ownerSlotIndex = state.OwnerSlotIndex;
        if (ownerSlotIndex < 0 ||
            ownerSlotIndex + ShaktoolSegmentCount > _slotTable.Slots.Count)
        {
            throw new InvalidDataException(
                $"Shaktool owner slot {ownerSlotIndex} is invalid.");
        }

        var group = new RoomEnemySlot[ShaktoolSegmentCount];
        for (int index = 0; index < group.Length; index++)
        {
            RoomEnemySlot segment = _slotTable.Slots[ownerSlotIndex + index];
            // Group helpers also run during the remainder of the lethal frame.
            // The segment's state survives common-slot clearing with its record
            // values emptied; a cleared header is not a missing record.
            if (segment.SpeciesState<ShaktoolSegmentState>() is null)
            {
                throw new InvalidDataException(
                    $"Shaktool group rooted at slot {ownerSlotIndex} is missing segment {index}.");
            }
            group[index] = segment;
        }
        return group;
    }

    /// <summary>Requires a following physical slot for a linked segment center.</summary>
    private RoomEnemySlot GetNextShaktoolSegment(RoomEnemySlot slot)
    {
        if (slot.SlotIndex + 1 >= _slotTable.Slots.Count)
        {
            throw new InvalidDataException(
                $"Shaktool center slot {slot.SlotIndex} has no following segment.");
        }
        return _slotTable.Slots[slot.SlotIndex + 1];
    }

    /// <summary>Requires an initialized extension for the given physical segment slot.</summary>
    private static ShaktoolSegmentState RequireShaktoolState(RoomEnemySlot slot) =>
        slot.SpeciesState<ShaktoolSegmentState>() is { } state
            ? state
            : throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} has no initialized Shaktool state.");

    /// <summary>Carries out one cue of a Shaktool segment's animation (native callbacks <c>$AA:D931-$D9BA</c>).</summary>
    internal ShaktoolProgram? ApplyShaktoolCue(RoomEnemySlot slot, ShaktoolSegmentState state, ShaktoolCue cue)
    {
        switch (cue.Kind)
        {
            case ShaktoolCueKind.LowerAwayFromCenter:
            case ShaktoolCueKind.RaiseTowardCenter:
            {
                RoomEnemySlot[] group = GetShaktoolGroup(slot);
                // Flipping the direction's top bit turns it half a turn.
                ByteAngle centerDirection = new(checked((byte)RequireShaktoolState(group[3])
                    .OrientationBucket));
                MoveShaktoolSegmentForAnimation(
                    slot,
                    cue.Kind == ShaktoolCueKind.LowerAwayFromCenter
                        ? centerDirection.AddTableUnits(EnemyByteAngleDefinitions.HalfTurn)
                        : centerDirection);
                break;
            }

            case ShaktoolCueKind.Lower:
            case ShaktoolCueKind.Raise:
            {
                ByteAngle targetDirection = state.TargetAngle.ToByteAngle();
                MoveShaktoolSegmentForAnimation(
                    slot,
                    cue.Kind == ShaktoolCueKind.Lower
                        ? targetDirection.AddTableUnits(EnemyByteAngleDefinitions.HalfTurn)
                        : targetDirection);
                break;
            }

            case ShaktoolCueKind.Nothing:
                // Explicit RTL stub used between the two long attack pauses.
                break;

            case ShaktoolCueKind.ResetFunctions:
            {
                RoomEnemySlot[] group = GetShaktoolGroup(slot);
                for (int index = 0; index < ShaktoolSegmentCount; index++)
                {
                    RequireShaktoolState(group[index]).PreInstruction =
                        ShaktoolSegments.ForIndex(index).PreInstruction;
                }
                break;
            }

            default:
                throw new InvalidOperationException($"Undefined {nameof(ShaktoolCueKind)} {cue.Kind}.");
        }

        return null;
    }

    /// <summary>Ports the one-frame sine movement helper at <c>$AA:D956</c>.</summary>
    private static void MoveShaktoolSegmentForAnimation(RoomEnemySlot slot, ByteAngle angle)
    {
        // The decompilation names a 320-word view whose indices 64..319 are the cartridge's
        // 256-word table at $A0:B443. Convert those full-view indices explicitly: X reads
        // full[angle+64], while Y reads full[angle]. A naïve +64 on the ROM table rotates
        // both components by a quarter turn.
        int xVelocity = ReadShaktoolCommonSineSample(angle.RawValue + 64);
        int yVelocity = ReadShaktoolCommonSineSample(angle.RawValue);
        slot.Position = slot.Position.Advance(new(new SignedEightEight(checked((ushort)xVelocity)).ToSixteenSixteenDelta(), new SignedEightEight(checked((ushort)yVelocity)).ToSixteenSixteenDelta()));
    }

    /// <summary>Maps the Shaktool orbit index into the shared signed enemy sine table.</summary>
    internal static ushort ReadShaktoolCommonSineSample(int fullTableIndex) =>
        unchecked((ushort)Trigonometry.SignedSine(new ByteAngle((byte)(fullTableIndex - 64))));

    /// <summary>
    /// Implements Shaktool's fatal-shot tail at <c>$AA:DF34</c>, including its read
    /// of the owner index after common death has cleared the struck enemy's RAM.
    /// </summary>
    private void ResolveShaktoolShotAfterCommon(RoomEnemySlot struckSegment)
    {
        if (struckSegment.Health != 0)
            return;
        // $A0:A3AF clears the entire common slot, which empties the segment's owner. $AA:DF40
        // reads that now-zero owner and writes properties without checking headers.
        // Do not recover the old group from before the kill: native also targets slots
        // 0..6 when Shaktool was placed later in a constructed population.
        int ownerSlotIndex = RequireShaktoolState(struckSegment).OwnerSlotIndex;
        if (ownerSlotIndex < 0 ||
            ownerSlotIndex + ShaktoolSegmentCount > _slotTable.Slots.Count)
            throw new InvalidDataException($"Shaktool post-shot owner slot {ownerSlotIndex} is invalid.");
        for (int index = 0; index < ShaktoolSegmentCount; index++)
            _slotTable.Slots[ownerSlotIndex + index].Properties = new EnemyPropertyWord(EnemyProperties.Deleted);
    }

    /// <summary>Runs the native post-common lethal-shot tail for bomb hits.</summary>
    internal void AddShaktoolBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Shaktool &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.ShaktoolShot)
        {
            AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Registers front and linked attack-circle movement callbacks.</summary>
    internal void AddShaktoolProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddPreInstruction(context =>
        {
            RunShaktoolFrontCirclePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.ShaktoolsAttack_Front);

        table.AddPreInstruction(context =>
        {
            RunShaktoolLinkedCirclePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.ShaktoolsAttack_MiddleBack_Moving);
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

    /// <summary>`$AA:DF2F`: Shaktool contact is the common normal-enemy touch, death animation included.</summary>
    internal void AddShaktoolTouch(EnemySharedReactions table)
    {
        table.AddTouch(slot => slot.DefinitionId == EnemyDefinitionId.Shaktool &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.ShaktoolTouch,
            context => _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus));
    }

    /// <summary>Runs the native post-common lethal-shot tail for regular projectile hits.</summary>
    internal void AddShaktoolShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Shaktool &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.ShaktoolShot)
        {
            AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
        });
    }
    /// <summary>Ports front-circle pre-instruction <c>$86:BE03</c>.</summary>
    internal static void RunShaktoolFrontCirclePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical))
        {
            projectile.Clear();
        }
    }

    /// <summary>Ports middle/back linked-circle pre-instruction <c>$86:BE12</c>.</summary>
    internal void RunShaktoolLinkedCirclePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        int ownerProjectileOffset = checked((ushort)projectile.Variable0);
        if ((ownerProjectileOffset & 1) != 0 ||
            ownerProjectileOffset >= _projectiles.EnemyProjectiles.Count * 2 ||
            !_projectiles.EnemyProjectiles[ownerProjectileOffset >> 1].IsActive)
        {
            projectile.Clear();
            return;
        }

        _ = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Horizontal);
        _ = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, axis: Axis.Vertical);
    }
}
