using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Shaktool species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ShaktoolBehavior : IEnemySpecies
{
    /// <summary>Stores terrain.</summary>
    private readonly EnemyTerrain _terrain;
    /// <summary>Stores projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;
    /// <summary>Stores slot table.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Stores collision.</summary>
    private readonly EnemyCollision _collision;

    /// <summary>Initializes a new shaktool behavior instance.</summary>
    internal ShaktoolBehavior(EnemyTerrain terrain, EnemyProjectileEngine projectiles, EnemySlotTable slotTable, EnemyCollision collision)
    {
        _terrain = terrain;
        _projectiles = projectiles;
        _slotTable = slotTable;
        _collision = collision;
    }

    /// <summary>Defines shaktool segment count.</summary>
    private const int ShaktoolSegmentCount = 7;

    /// <summary>Stores shaktool segments.</summary>
    internal readonly ShaktoolSegmentState?[] _shaktoolSegments =
        new ShaktoolSegmentState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Resets shaktool room state.</summary>
    internal void ResetShaktoolRoomState() => Array.Clear(_shaktoolSegments);

    /// <summary>Ports <c>Shaktool_Init</c> at <c>$AA:DE43</c>.</summary>
    /// <summary>The whole table step half a turn from <paramref name="angle"/>, its fraction cleared.</summary>
    private static ushort OppositeOrbitStep(ushort angle) =>
        SnesAngle.FromTableIndex(SnesAngle.FromRaw(angle).AddRaw(SnesAngle.HalfTurn.RawValue).TableIndex).RawValue;

    /// <summary>Initializes shaktool.</summary>
    internal void InitializeShaktool(RoomEnemySlot slot)
    {
        if ((slot.Parameter2 & 1) != 0 || slot.Parameter2 > 12)
        {
            throw new InvalidDataException(
                $"Shaktool parameter two ${slot.Parameter2:X4} is not an even index 0..12.");
        }

        int segmentIndex = slot.Parameter2 >> 1;
        ShaktoolSegmentDefinition definition = ShaktoolSegmentDefinitions.ForIndex(segmentIndex);
        var state = new ShaktoolSegmentState(slot);
        _shaktoolSegments[slot.SlotIndex] = state;

        slot.InstructionTimer = 1;
        slot.Timer = 0;
        state.TargetAngle = 0;
        state.OrientationAndAcceleration = 0;
        slot.Properties = unchecked((ushort)(slot.Properties | definition.PropertyMask));
        state.OwnerNativeIndex = unchecked((ushort)(slot.NativeIndex -
            definition.OwnerNativeOffset));
        state.PreInstruction = definition.PreInstruction;
        // Native initialization subtracts the parallel all-zero table; synchronization
        // uses the same authored velocity directly.
        state.AngularVelocity = definition.AngularVelocity;
        state.OrbitAngle = definition.InitialOrbitAngle;

        // The second half of $DEB1 is a parallel list-pointer table. Parameter two is
        // already a byte offset, so +14+p2 selects exactly entries seven through thirteen.
        slot.CurrentInstruction = definition.InitialInstruction;
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
                    $"Shaktool pre-instruction $AA:{(ushort)state.PreInstruction:X4} " +
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

        RoomEnemySlot previous = _slotTable._slots[slot.SlotIndex - 1];
        // $AA:DC42/$DC4A/$DC60/$DC68 read the preceding physical record, without
        // testing its header. A lethal shot clears the head and marks the group
        // deleted, but EnemyMain still visits later entries in this frame's
        // prebuilt active list. Their final orbit uses the cleared coordinates.

        (int xDisplacement, int yDisplacement) = ShaktoolOrbitTables.Displacement(
            SnesAngle.FromRaw(state.OrbitAngle).TableIndex);
        slot.Position = previous.Position.Advance(new(
            SnesSignedSixteenSixteen.FromRaw(xDisplacement),
            SnesSignedSixteenSixteen.FromRaw(yDisplacement)));
    }

    /// <summary>Ports the common orbit step at <c>$AA:DCAC</c>.</summary>
    private void AdvanceShaktoolOrbit(RoomEnemySlot slot, ShaktoolSegmentState state)
    {
        PositionShaktoolAroundPreviousSegment(slot, state);
        ShaktoolMotionFlags flags = (ShaktoolMotionFlags)slot.Parameter1;
        ushort increment;
        if ((flags & ShaktoolMotionFlags.Straightening) != 0)
        {
            state.TargetAngle = unchecked((ushort)(state.TargetAngle + 0x0100));
            increment = 0x0100;
        }
        else
        {
            increment = state.AngularVelocity;
        }

        state.OrbitAngle = (flags & ShaktoolMotionFlags.Clockwise) != 0
            ? unchecked((ushort)(state.OrbitAngle - increment))
            : unchecked((ushort)(state.OrbitAngle + increment));
    }

    /// <summary>Ports middle-segment orientation selection at <c>$AA:DCD7</c>.</summary>
    internal void AdvanceAndOrientShaktoolCenter(
        RoomEnemySlot slot,
        ShaktoolSegmentState state)
    {
        AdvanceShaktoolOrbit(slot, state);
        RoomEnemySlot nextSlot = GetNextShaktoolSegment(slot);
        ShaktoolSegmentState next = RequireShaktoolState(nextSlot);

        ushort oppositeAngle = unchecked((ushort)(state.OrbitAngle ^ 0x8000));
        ushort midpoint = unchecked((ushort)(oppositeAngle +
            (unchecked((ushort)(next.OrbitAngle - oppositeAngle)) >> 1)));
        if ((state.OrientationAndAcceleration & 0x8000) != 0)
            midpoint ^= 0x8000;

        ushort directionBucket = unchecked((ushort)(((SnesLittleEndian.HighByte(midpoint)) + 8) & 0x00e0));
        state.OrientationAndAcceleration = unchecked((ushort)(
            (SnesLittleEndian.HighByteInPlace(state.OrientationAndAcceleration)) | directionBucket));
        slot.CurrentInstruction =
            ShaktoolInstructionDefinitions.ForOrientationBucket(directionBucket);
        slot.InstructionTimer = 1;
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
        int xDisplacement = SnesSixteenSixteen.FromWhole(step.X);
        bool collided = _terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            xDisplacement);
        if (!collided)
        {
            slot.Position = slot.Position.WithWholeY(previous.Y);
            int yDisplacement = SnesSixteenSixteen.FromWhole(step.Y);
            collided = _terrain.MoveEnemyVertically(level, slot, yDisplacement);
        }

        if (collided)
        {
            ReverseShaktoolAfterCollision(slot, state, previous);
            return;
        }

        slot.Position = slot.Position.WithWhole(candidate);
        ShaktoolMotionFlags flags = (ShaktoolMotionFlags)slot.Parameter1;
        if ((flags & ShaktoolMotionFlags.Straightening) != 0)
        {
            // DD25 deliberately adds a second $0100 after DCAC's first increment.
            state.TargetAngle = unchecked((ushort)(state.TargetAngle + 0x0100));
            return;
        }

        if (SnesAngle.FromRaw(state.TargetAngle).TableIndex == SnesAngle.FromRaw(state.OrbitAngle).TableIndex)
        {
            SynchronizeShaktoolOrbitTargets(slot, state.TargetAngle);
            state.OrientationAndAcceleration = 0x7800;
            SetShaktoolGroupMotionFlags(
                slot,
                (ShaktoolMotionFlags)slot.Parameter1 & ~ShaktoolMotionFlags.ReversedOnce);
            state.OrientationAndAcceleration = unchecked((ushort)(
                SnesLittleEndian.HighByteInPlace(state.OrientationAndAcceleration)));
        }

        state.OrientationAndAcceleration = unchecked((ushort)(
            state.OrientationAndAcceleration + state.AngularVelocity));
        if (state.OrientationAndAcceleration >= 0xf000)
        {
            SetShaktoolGroupMotionFlags(
                slot,
                (ShaktoolMotionFlags)slot.Parameter1 | ShaktoolMotionFlags.Straightening);
        }
    }

    /// <summary>Reverse shaktool after collision.</summary>
    internal void ReverseShaktoolAfterCollision(
        RoomEnemySlot slot,
        ShaktoolSegmentState state,
        Position16<RoomPixels> previous)
    {
        ShaktoolMotionFlags flags = (ShaktoolMotionFlags)slot.Parameter1;
        if ((flags & ShaktoolMotionFlags.ReversedOnce) != 0)
        {
            SetShaktoolGroupMotionFlags(
                slot,
                (flags ^ ShaktoolMotionFlags.Clockwise) &
                (ShaktoolMotionFlags)0x8fff);
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
                (ShaktoolMotionFlags)slot.Parameter1 & ~ShaktoolMotionFlags.Straightening);
        }

        state.OrientationAndAcceleration = 0;
        RoomEnemySlot owner = _slotTable.SlotFromNativeIndex(state.OwnerNativeIndex);
        byte angle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(slot.Position.X.Whole - owner.Position.X.Whole)), unchecked((short)(slot.Position.Y.Whole - owner.Position.Y.Whole))));
        ushort target = SnesAngle.FromTableIndex(angle).RawValue;
        target = ((ShaktoolMotionFlags)slot.Parameter1 & ShaktoolMotionFlags.Clockwise) != 0
            ? unchecked((ushort)(target - 0x4000))
            : unchecked((ushort)(target + 0x4000));
        SetShaktoolGroupTargetAngle(slot, target);

        RoomEnemySlot[] group = GetShaktoolGroup(slot);
        for (int index = ShaktoolSegmentCount - 1; index >= 0; index--)
        {
            RoomEnemySlot segment = group[index];
            ShaktoolSegmentState segmentState = RequireShaktoolState(segment);
            CalculateShaktoolAngularVelocity(segment, segmentState);
            segmentState.PreInstruction = ShaktoolPreInstruction.IdleAfterAttack;
            segment.CurrentInstruction =
                ShaktoolInstructionDefinitions.CollisionForSegment(index);
            segment.InstructionTimer = 1;
        }
    }

    /// <summary>Ports the endpoint/angle reversal helper at <c>$AA:DB59</c>.</summary>
    private void ReverseShaktoolChainEndpoints(RoomEnemySlot anySegment)
    {
        RoomEnemySlot[] group = GetShaktoolGroup(anySegment);
        RequireShaktoolState(group[3]).OrientationAndAcceleration ^= 0x8000;

        SwapMirroredShaktoolAngles(group[6], group[1]);
        SwapMirroredShaktoolAngles(group[5], group[2]);
        SwapMirroredShaktoolAngles(group[4], group[3]);

        RoomEnemySlot head = group[0];
        RoomEnemySlot tail = group[6];
        (tail.Position, head.Position) = (head.Position, tail.Position);

        for (int index = 1; index < ShaktoolSegmentCount - 1; index++)
        {
            group[index].Position = group[index].Position.WithFractionX(0x8000).WithFractionY(0x8000);
        }
    }

    /// <summary>Swap mirrored shaktool angles.</summary>
    private void SwapMirroredShaktoolAngles(RoomEnemySlot left, RoomEnemySlot right)
    {
        ShaktoolSegmentState leftState = RequireShaktoolState(left);
        ShaktoolSegmentState rightState = RequireShaktoolState(right);
        ushort saved = leftState.OrbitAngle;
        leftState.OrbitAngle = OppositeOrbitStep(rightState.OrbitAngle);
        rightState.OrbitAngle = OppositeOrbitStep(saved);
    }

    /// <summary>Ports the per-segment convergence speed calculation at <c>$AA:DC07</c>.</summary>
    private static void CalculateShaktoolAngularVelocity(
        RoomEnemySlot slot,
        ShaktoolSegmentState state)
    {
        ushort difference = ((ShaktoolMotionFlags)slot.Parameter1 &
            ShaktoolMotionFlags.Clockwise) != 0
            ? unchecked((ushort)(state.OrbitAngle - state.TargetAngle))
            : unchecked((ushort)(state.TargetAngle - state.OrbitAngle));
        state.AngularVelocity = unchecked((ushort)(4 * (SnesLittleEndian.HighByte(difference))));
    }

    /// <summary>Ports group synchronization at <c>$AA:DC6F</c>.</summary>
    private void SynchronizeShaktoolOrbitTargets(RoomEnemySlot anySegment, ushort target)
    {
        RoomEnemySlot[] group = GetShaktoolGroup(anySegment);
        for (int index = 0; index < ShaktoolSegmentCount; index++)
        {
            ShaktoolSegmentState state = RequireShaktoolState(group[index]);
            state.OrbitAngle = target;
            state.AngularVelocity = ShaktoolSegmentDefinitions.ForIndex(index).AngularVelocity;
        }
    }

    /// <summary>Sets shaktool group target angle.</summary>
    private void SetShaktoolGroupTargetAngle(RoomEnemySlot anySegment, ushort target)
    {
        foreach (RoomEnemySlot segment in GetShaktoolGroup(anySegment))
            RequireShaktoolState(segment).TargetAngle = target;
    }

    /// <summary>Sets shaktool group motion flags.</summary>
    private void SetShaktoolGroupMotionFlags(
        RoomEnemySlot anySegment,
        ShaktoolMotionFlags flags)
    {
        foreach (RoomEnemySlot segment in GetShaktoolGroup(anySegment))
            segment.Parameter1 = (ushort)flags;
    }

    /// <summary>Gets shaktool group.</summary>
    private RoomEnemySlot[] GetShaktoolGroup(RoomEnemySlot anySegment)
    {
        ShaktoolSegmentState state = RequireShaktoolState(anySegment);
        int ownerSlotIndex = state.OwnerNativeIndex / RoomEnemySystem.NativeSlotSize;
        if (state.OwnerNativeIndex % RoomEnemySystem.NativeSlotSize != 0 ||
            ownerSlotIndex + ShaktoolSegmentCount > _slotTable._slots.Length)
        {
            throw new InvalidDataException(
                $"Shaktool owner native index ${state.OwnerNativeIndex:X4} is invalid.");
        }

        var group = new RoomEnemySlot[ShaktoolSegmentCount];
        for (int index = 0; index < group.Length; index++)
        {
            RoomEnemySlot segment = _slotTable._slots[ownerSlotIndex + index];
            // Group helpers also run during the remainder of the lethal frame.
            // The initialized view survives common-slot clearing and continues to
            // expose its native words; a cleared header is not a missing record.
            if (_shaktoolSegments[segment.SlotIndex] is null)
            {
                throw new InvalidDataException(
                    $"Shaktool group rooted at slot {ownerSlotIndex} is missing segment {index}.");
            }
            group[index] = segment;
        }
        return group;
    }

    /// <summary>Gets next shaktool segment.</summary>
    private RoomEnemySlot GetNextShaktoolSegment(RoomEnemySlot slot)
    {
        if (slot.SlotIndex + 1 >= _slotTable._slots.Length)
        {
            throw new InvalidDataException(
                $"Shaktool center slot {slot.SlotIndex} has no following segment.");
        }
        return _slotTable._slots[slot.SlotIndex + 1];
    }

    /// <summary>Requires shaktool state.</summary>
    private ShaktoolSegmentState RequireShaktoolState(RoomEnemySlot slot) =>
        _shaktoolSegments[slot.SlotIndex] is { } state
            ? state
            : throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} has no initialized Shaktool state.");


    /// <summary>Animation callback dispatcher for <c>$AA:D931-$D9BA</c>.</summary>
    internal bool TryProcessShaktoolInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Shaktool)
            return false;

        ShaktoolSegmentState state = RequireShaktoolState(slot);
        if (!Enum.IsDefined((ShaktoolInstruction)opcode))
            return false;
        switch ((ShaktoolInstruction)opcode)
        {
            case ShaktoolInstruction.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931:
            case ShaktoolInstruction.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F:
            {
                RoomEnemySlot[] group = GetShaktoolGroup(slot);
                byte centerDirection = unchecked((byte)RequireShaktoolState(group[3])
                    .OrientationAndAcceleration);
                MoveShaktoolSegmentForAnimation(
                    slot,
                    opcode == (ushort)ShaktoolInstruction.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931
                        ? unchecked((byte)(centerDirection ^ 0x80))
                        : centerDirection);
                break;
            }

            case ShaktoolInstruction.Instruction_Shaktool_Lower1Pixel:
            case ShaktoolInstruction.Instruction_Shaktool_Raise1Pixel:
            {
                byte targetDirection = SnesAngle.FromRaw(state.TargetAngle).TableIndex;
                MoveShaktoolSegmentForAnimation(
                    slot,
                    opcode == (ushort)ShaktoolInstruction.Instruction_Shaktool_Lower1Pixel
                        ? unchecked((byte)(targetDirection ^ 0x80))
                        : targetDirection);
                break;
            }

            case ShaktoolInstruction.RTL_AAD99F:
                // Explicit RTL stub used between the two long attack pauses.
                break;

            case ShaktoolInstruction.Instruction_Shaktool_ResetShaktoolFunctions:
            {
                RoomEnemySlot[] group = GetShaktoolGroup(slot);
                for (int index = 0; index < ShaktoolSegmentCount; index++)
                {
                    RequireShaktoolState(group[index]).PreInstruction =
                        ShaktoolSegmentDefinitions.ForIndex(index).PreInstruction;
                }
                break;
            }

            default:
                throw new InvalidOperationException($"Undefined {nameof(ShaktoolInstruction)} {opcode:X4}.");
        }

        cursor = unchecked((ushort)(cursor + 2));
        return true;
    }

    /// <summary>Ports the one-frame sine movement helper at <c>$AA:D956</c>.</summary>
    private static void MoveShaktoolSegmentForAnimation(RoomEnemySlot slot, byte angle)
    {
        // The decompilation names a 320-word view whose indices 64..319 are the cartridge's
        // 256-word table at $A0:B443. Convert those full-view indices explicitly: X reads
        // full[angle+64], while Y reads full[angle]. A naïve +64 on the ROM table rotates
        // both components by a quarter turn.
        ushort xVelocity = ReadShaktoolCommonSineSample(angle + 64);
        ushort yVelocity = ReadShaktoolCommonSineSample(angle);
        slot.Position = slot.Position.Advance(new(new SnesSignedEightEight(xVelocity).ToSixteenSixteenDelta(), new SnesSignedEightEight(yVelocity).ToSixteenSixteenDelta()));
    }

    /// <summary>Reads shaktool common sine sample.</summary>
    internal static ushort ReadShaktoolCommonSineSample(int fullTableIndex) =>
        unchecked((ushort)EnemyTrigonometryTables.SignedSine((byte)(fullTableIndex - 64)));

    /// <summary>
    /// Implements Shaktool's fatal-shot tail at <c>$AA:DF34</c>, including its read
    /// of the owner index after common death has cleared the struck enemy's RAM.
    /// </summary>
    private void ResolveShaktoolShotAfterCommon(RoomEnemySlot struckSegment)
    {
        if (struckSegment.Health != 0)
            return;
        // $A0:A3AF clears the entire common slot, including VariableE. $AA:DF40
        // reads that now-zero owner and writes properties without checking headers.
        // Do not recover the old group from cached state: native also targets slots
        // 0..6 when Shaktool was placed later in a constructed population.
        ushort ownerNativeIndex = struckSegment.VariableE;
        int ownerSlotIndex = ownerNativeIndex / RoomEnemySystem.NativeSlotSize;
        if (ownerNativeIndex % RoomEnemySystem.NativeSlotSize != 0 ||
            ownerSlotIndex + ShaktoolSegmentCount > _slotTable._slots.Length)
            throw new InvalidDataException($"Shaktool post-shot owner ${ownerNativeIndex:X4} is invalid.");
        for (int index = 0; index < ShaktoolSegmentCount; index++)
            _slotTable._slots[ownerSlotIndex + index].Properties = (ushort)EnemyProperties.Deleted;
    }

    /// <summary>Register shaktool bomb reactions.</summary>
    internal void RegisterShaktoolBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.ShaktoolShot)
        {
            AfterCommon = c => ResolveShaktoolShotAfterCommon(c.Enemy),
        });
    }

    /// <summary>Register shaktool projectiles.</summary>
    internal void RegisterShaktoolProjectiles(EnemyProjectileHandlerTable table)
    {
        table.RegisterPreInstruction(context =>
        {
            RunShaktoolFrontCirclePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.ShaktoolsAttack_Front);

        table.RegisterPreInstruction(context =>
        {
            RunShaktoolLinkedCirclePreInstruction(context.Projectile, context.Level);
        }, EnemyProjectilePreInstruction.ShaktoolsAttack_MiddleBack_Moving);
    }

    /// <summary>Register shaktool initialization ai.</summary>
    internal void RegisterShaktoolInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Shaktool, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeShaktool(slot);
        });
    }

    /// <summary>Register shaktool main ai.</summary>
    internal void RegisterShaktoolMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_HurtAI_Shaktool, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool, (slot, frame, nmiFrameCounter8) =>
        {
            RunShaktoolMain(slot, RequireShaktoolState(slot), frame.Level);
        });
    }

    /// <summary>Register shaktool owned instructions.</summary>
    internal void RegisterShaktoolOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessShaktoolInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>`$AA:DF2F`: Shaktool contact is the common normal-enemy touch, death animation included.</summary>
    internal void RegisterShaktoolTouch(EnemyAiTable table)
    {
        table.AddTouch(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool &&
                               EnemyCollision.TouchAi(slot) == EnemyInteractionCallback.ShaktoolTouch,
            context => _collision.ResolveNormalEnemyTouch(context.Slot, context.Samus));
    }

    /// <summary>Register shaktool shot reactions.</summary>
    internal void RegisterShaktoolShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.ShaktoolShot)
        {
            AfterCommon = (c, _) => ResolveShaktoolShotAfterCommon(c.Enemy),
        });
    }
    /// <summary>Ports front-circle pre-instruction <c>$86:BE03</c>.</summary>
    internal static void RunShaktoolFrontCirclePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        if (EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true) ||
            EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false))
        {
            projectile.Clear();
        }
    }

    /// <summary>Ports middle/back linked-circle pre-instruction <c>$86:BE12</c>.</summary>
    internal void RunShaktoolLinkedCirclePreInstruction(
        RoomEnemyProjectileSlot projectile,
        RoomLevelData level)
    {
        ushort ownerNativeIndex = projectile.Variable0;
        if ((ownerNativeIndex & 1) != 0 ||
            ownerNativeIndex >= _projectiles.EnemyProjectiles.Count * 2 ||
            !_projectiles.EnemyProjectiles[ownerNativeIndex >> 1].IsActive)
        {
            projectile.Clear();
            return;
        }

        _ = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: true);
        _ = EnemyProjectileEngine.MoveProjectileAxis(projectile, level, horizontal: false);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetShaktoolRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
