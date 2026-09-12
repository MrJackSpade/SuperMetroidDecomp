using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA function pointers stored in Shaktool's native variable F. Keeping the raw
/// addresses visible makes debugger watches line up with the ROM while giving each of the
/// five actually-used routines a domain name.
/// </summary>
public enum ShaktoolPreInstruction : ushort
{
    IdleAfterAttack = 0xdcaa,
    IdleHead = 0xdcab,
    OrbitPreviousSegment = 0xdcac,
    OrbitAndOrientCenter = 0xdcd7,
    DriveTailAndReverseAtWalls = 0xdd25,
}

/// <summary>
/// Typed view over one of Shaktool's seven consecutive enemy records. The properties are
/// deliberately backed by native variables A-F instead of duplicated host state, so a C#
/// debugger shows exactly the words that the 65816 implementation would mutate.
/// </summary>
public sealed class ShaktoolSegmentState
{
    private readonly RoomEnemySlot _slot;

    internal ShaktoolSegmentState(RoomEnemySlot slot) => _slot = slot;

    public RoomEnemySlot Slot => _slot;
    public ushort TargetAngle
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }
    public ushort OrbitAngle
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }
    public ushort AngularVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }
    public ushort OrientationAndAcceleration
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }
    public ushort OwnerNativeIndex
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }
    public ShaktoolPreInstruction PreInstruction
    {
        get => (ShaktoolPreInstruction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

[Flags]
internal enum ShaktoolMotionFlags : ushort
{
    None = 0,
    ReversedOnce = 0x2000,
    Straightening = 0x4000,
    Clockwise = 0x8000,
}

/// <summary>
/// Cartridge-faithful translation of Shaktool definition <c>$F07F</c>. The creature is not
/// one large sprite: seven consecutive enemy records form a kinematic chain. Slots one
/// through six orbit the immediately preceding record, the middle record selects a rotated
/// body map, and the last record collision-tests the complete chain and reverses its ends.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort ShaktoolDefinition = 0xf07f;
    internal const ushort ShaktoolTouchAi = EnemyAiCodePointers.BankAA.ShaktoolTouch;
    internal const ushort ShaktoolShotAi = EnemyAiCodePointers.BankAA.ShaktoolShot;

    private const int ShaktoolSegmentCount = 7;
    private const int ShaktoolPropertyTable = 0xaade95;
    private const int ShaktoolOwnerOffsetTable = 0xaadea3;
    private const int ShaktoolInitialAngleAndListTable = 0xaadeb1;
    private const int ShaktoolLayerTable = 0xaadecd;
    private const int ShaktoolPreInstructionTable = 0xaadedb;
    private const int ShaktoolAngularVelocityTable = 0xaadee9;
    private const int ShaktoolAngularVelocitySubtractTable = 0xaadef7;
    private const int ShaktoolCollisionListTable = 0xaadf13;
    private const int ShaktoolAttackListTable = 0xaadf21;
    private const int ShaktoolOrientationListTable = 0xaadd15;

    private readonly ShaktoolSegmentState?[] _shaktoolSegments =
        new ShaktoolSegmentState?[MaximumEnemyCount];

    /// <summary>
    /// Per-physical-slot Shaktool state. A live retail group occupies seven consecutive
    /// non-null entries; every other room slot remains null.
    /// </summary>
    public IReadOnlyList<ShaktoolSegmentState?> ShaktoolSegments => _shaktoolSegments;

    private void ResetShaktoolRoomState() => Array.Clear(_shaktoolSegments);

    /// <summary>Ports <c>Shaktool_Init</c> at <c>$AA:DE43</c>.</summary>
    private void InitializeShaktool(RoomEnemySlot slot)
    {
        if ((slot.Parameter2 & 1) != 0 || slot.Parameter2 > 12)
        {
            throw new InvalidDataException(
                $"Shaktool parameter two ${slot.Parameter2:X4} is not an even index 0..12.");
        }

        int segmentIndex = slot.Parameter2 >> 1;
        var state = new ShaktoolSegmentState(slot);
        _shaktoolSegments[slot.SlotIndex] = state;

        slot.InstructionTimer = 1;
        slot.Timer = 0;
        state.TargetAngle = 0;
        state.OrientationAndAcceleration = 0;
        slot.Properties = unchecked((ushort)(slot.Properties |
            ReadWord(_bus!, ShaktoolPropertyTable + segmentIndex * 2)));
        state.OwnerNativeIndex = unchecked((ushort)(slot.NativeIndex -
            ReadWord(_bus!, ShaktoolOwnerOffsetTable + segmentIndex * 2)));
        state.PreInstruction = (ShaktoolPreInstruction)ReadWord(
            _bus!,
            ShaktoolPreInstructionTable + segmentIndex * 2);
        state.AngularVelocity = unchecked((ushort)(
            ReadWord(_bus!, ShaktoolAngularVelocityTable + segmentIndex * 2) -
            ReadWord(_bus!, ShaktoolAngularVelocitySubtractTable + segmentIndex * 2)));
        state.OrbitAngle = ReadWord(
            _bus!,
            ShaktoolInitialAngleAndListTable + segmentIndex * 2);

        // The second half of $DEB1 is a parallel list-pointer table. Parameter two is
        // already a byte offset, so +14+p2 selects exactly entries seven through thirteen.
        slot.CurrentInstruction = ReadWord(
            _bus!,
            ShaktoolInitialAngleAndListTable + 14 + slot.Parameter2);
        slot.Layer = ReadWord(_bus!, ShaktoolLayerTable + segmentIndex * 2);

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
    private void PositionShaktoolAroundPreviousSegment(
        RoomEnemySlot slot,
        ShaktoolSegmentState state)
    {
        if (slot.SlotIndex == 0)
            throw new InvalidOperationException("Shaktool's anchor has no preceding segment.");

        RoomEnemySlot previous = _slots[slot.SlotIndex - 1];
        if (previous.EnemyDefinitionPointer != ShaktoolDefinition)
        {
            throw new InvalidDataException(
                $"Shaktool slot {slot.SlotIndex} is not preceded by another Shaktool segment.");
        }

        (int xDisplacement, int yDisplacement) = ShaktoolOrbitTables.Displacement(
            unchecked((byte)(state.OrbitAngle >> 8)));
        (slot.XPosition, slot.XSubposition) = AddShaktoolFixed(
            previous.XPosition,
            previous.XSubposition,
            xDisplacement);
        (slot.YPosition, slot.YSubposition) = AddShaktoolFixed(
            previous.YPosition,
            previous.YSubposition,
            yDisplacement);
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
    private void AdvanceAndOrientShaktoolCenter(
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

        ushort directionBucket = unchecked((ushort)(((midpoint >> 8) + 8) & 0x00e0));
        state.OrientationAndAcceleration = unchecked((ushort)(
            (state.OrientationAndAcceleration & 0xff00) | directionBucket));
        slot.CurrentInstruction = ReadWord(
            _bus!,
            ShaktoolOrientationListTable + (directionBucket >> 5) * 2);
        slot.InstructionTimer = 1;
    }

    /// <summary>Ports the last segment's collision/reversal controller at <c>$AA:DD25</c>.</summary>
    private void DriveShaktoolTail(
        RoomEnemySlot slot,
        ShaktoolSegmentState state,
        RoomLevelData level)
    {
        ushort previousX = slot.XPosition;
        ushort previousY = slot.YPosition;
        AdvanceShaktoolOrbit(slot, state);
        ushort candidateX = slot.XPosition;
        ushort candidateY = slot.YPosition;

        // The native helper restores only whole-pixel words before collision. Subpositions
        // remain those produced by DC2A, a subtle asymmetry that affects wall alignment.
        slot.XPosition = previousX;
        slot.YPosition = previousY;
        int xDisplacement = unchecked((short)(candidateX - previousX)) << 16;
        bool collided = MoveEnemyHorizontallyIgnoringNonSquareSlopes(
            level,
            slot,
            xDisplacement);
        if (!collided)
        {
            slot.YPosition = previousY;
            int yDisplacement = unchecked((short)(candidateY - previousY)) << 16;
            collided = MoveEnemyVertically(level, slot, yDisplacement);
        }

        if (collided)
        {
            ReverseShaktoolAfterCollision(slot, state, previousX, previousY);
            return;
        }

        slot.XPosition = candidateX;
        slot.YPosition = candidateY;
        ShaktoolMotionFlags flags = (ShaktoolMotionFlags)slot.Parameter1;
        if ((flags & ShaktoolMotionFlags.Straightening) != 0)
        {
            // DD25 deliberately adds a second $0100 after DCAC's first increment.
            state.TargetAngle = unchecked((ushort)(state.TargetAngle + 0x0100));
            return;
        }

        if (((state.TargetAngle ^ state.OrbitAngle) & 0xff00) == 0)
        {
            SynchronizeShaktoolOrbitTargets(slot, state.TargetAngle);
            state.OrientationAndAcceleration = 0x7800;
            SetShaktoolGroupMotionFlags(
                slot,
                (ShaktoolMotionFlags)slot.Parameter1 & ~ShaktoolMotionFlags.ReversedOnce);
            state.OrientationAndAcceleration = unchecked((ushort)(
                (state.OrientationAndAcceleration >> 8) << 8));
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

    private void ReverseShaktoolAfterCollision(
        RoomEnemySlot slot,
        ShaktoolSegmentState state,
        ushort previousX,
        ushort previousY)
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
            slot.XPosition = previousX;
            slot.YPosition = previousY;
            ReverseShaktoolChainEndpoints(slot);

            // DB0E is called twice. The second call reads the parameter word just written
            // by the first, leaving $2000 set while clearing the $4000 straightening bit.
            SetShaktoolGroupMotionFlags(slot, flags | ShaktoolMotionFlags.ReversedOnce);
            SetShaktoolGroupMotionFlags(
                slot,
                (ShaktoolMotionFlags)slot.Parameter1 & ~ShaktoolMotionFlags.Straightening);
        }

        state.OrientationAndAcceleration = 0;
        RoomEnemySlot owner = SlotFromNativeIndex(state.OwnerNativeIndex);
        byte angle = CalculateCartridgeAngle(
            unchecked((short)(slot.XPosition - owner.XPosition)),
            unchecked((short)(slot.YPosition - owner.YPosition)));
        ushort target = unchecked((ushort)(angle << 8));
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
            segment.CurrentInstruction = ReadWord(
                _bus!,
                ShaktoolCollisionListTable + index * 2);
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
        (tail.XSubposition, head.XSubposition) = (head.XSubposition, tail.XSubposition);
        (tail.YSubposition, head.YSubposition) = (head.YSubposition, tail.YSubposition);
        (tail.XPosition, head.XPosition) = (head.XPosition, tail.XPosition);
        (tail.YPosition, head.YPosition) = (head.YPosition, tail.YPosition);

        for (int index = 1; index < ShaktoolSegmentCount - 1; index++)
        {
            group[index].XSubposition = 0x8000;
            group[index].YSubposition = 0x8000;
        }
    }

    private void SwapMirroredShaktoolAngles(RoomEnemySlot left, RoomEnemySlot right)
    {
        ShaktoolSegmentState leftState = RequireShaktoolState(left);
        ShaktoolSegmentState rightState = RequireShaktoolState(right);
        ushort saved = leftState.OrbitAngle;
        leftState.OrbitAngle = unchecked((ushort)((rightState.OrbitAngle ^ 0x8000) & 0xff00));
        rightState.OrbitAngle = unchecked((ushort)((saved ^ 0x8000) & 0xff00));
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
        state.AngularVelocity = unchecked((ushort)(4 * (difference >> 8)));
    }

    /// <summary>Ports group synchronization at <c>$AA:DC6F</c>.</summary>
    private void SynchronizeShaktoolOrbitTargets(RoomEnemySlot anySegment, ushort target)
    {
        RoomEnemySlot[] group = GetShaktoolGroup(anySegment);
        for (int index = 0; index < ShaktoolSegmentCount; index++)
        {
            ShaktoolSegmentState state = RequireShaktoolState(group[index]);
            state.OrbitAngle = target;
            state.AngularVelocity = ReadWord(
                _bus!,
                ShaktoolAngularVelocityTable + index * 2);
        }
    }

    private void SetShaktoolGroupTargetAngle(RoomEnemySlot anySegment, ushort target)
    {
        foreach (RoomEnemySlot segment in GetShaktoolGroup(anySegment))
            RequireShaktoolState(segment).TargetAngle = target;
    }

    private void SetShaktoolGroupMotionFlags(
        RoomEnemySlot anySegment,
        ShaktoolMotionFlags flags)
    {
        foreach (RoomEnemySlot segment in GetShaktoolGroup(anySegment))
            segment.Parameter1 = (ushort)flags;
    }

    private RoomEnemySlot[] GetShaktoolGroup(RoomEnemySlot anySegment)
    {
        ShaktoolSegmentState state = RequireShaktoolState(anySegment);
        int ownerSlotIndex = state.OwnerNativeIndex / NativeSlotSize;
        if (state.OwnerNativeIndex % NativeSlotSize != 0 ||
            ownerSlotIndex + ShaktoolSegmentCount > _slots.Length)
        {
            throw new InvalidDataException(
                $"Shaktool owner native index ${state.OwnerNativeIndex:X4} is invalid.");
        }

        var group = new RoomEnemySlot[ShaktoolSegmentCount];
        for (int index = 0; index < group.Length; index++)
        {
            RoomEnemySlot segment = _slots[ownerSlotIndex + index];
            if (segment.EnemyDefinitionPointer != ShaktoolDefinition ||
                _shaktoolSegments[segment.SlotIndex] is null)
            {
                throw new InvalidDataException(
                    $"Shaktool group rooted at slot {ownerSlotIndex} is missing segment {index}.");
            }
            group[index] = segment;
        }
        return group;
    }

    private RoomEnemySlot GetNextShaktoolSegment(RoomEnemySlot slot)
    {
        if (slot.SlotIndex + 1 >= _slots.Length ||
            _slots[slot.SlotIndex + 1].EnemyDefinitionPointer != ShaktoolDefinition)
        {
            throw new InvalidDataException(
                $"Shaktool center slot {slot.SlotIndex} has no following segment.");
        }
        return _slots[slot.SlotIndex + 1];
    }

    private ShaktoolSegmentState RequireShaktoolState(RoomEnemySlot slot) =>
        slot.EnemyDefinitionPointer == ShaktoolDefinition &&
        _shaktoolSegments[slot.SlotIndex] is { } state
            ? state
            : throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} has no initialized Shaktool state.");

    private static (ushort Position, ushort Subposition) AddShaktoolFixed(
        ushort position,
        ushort subposition,
        int displacement)
    {
        int fixedPosition = unchecked((position << 16) | subposition);
        fixedPosition = unchecked(fixedPosition + displacement);
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    /// <summary>Animation callback dispatcher for <c>$AA:D931-$D9BA</c>.</summary>
    private bool TryProcessShaktoolInstruction(
        RoomEnemySlot slot,
        ushort opcode,
        ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != ShaktoolDefinition)
            return false;

        ShaktoolSegmentState state = RequireShaktoolState(slot);
        switch (opcode)
        {
            case ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931:
            case ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Raise1PixelTowardsProj_AAD93F:
            {
                RoomEnemySlot[] group = GetShaktoolGroup(slot);
                byte centerDirection = unchecked((byte)RequireShaktoolState(group[3])
                    .OrientationAndAcceleration);
                MoveShaktoolSegmentForAnimation(
                    slot,
                    opcode == ShaktoolInstructionCodes.UNUSED_Instruction_Shaktool_Lower1PixelAwayFromProj_AAD931
                        ? unchecked((byte)(centerDirection ^ 0x80))
                        : centerDirection);
                break;
            }

            case ShaktoolInstructionCodes.Instruction_Shaktool_Lower1Pixel:
            case ShaktoolInstructionCodes.Instruction_Shaktool_Raise1Pixel:
            {
                byte targetDirection = unchecked((byte)(state.TargetAngle >> 8));
                MoveShaktoolSegmentForAnimation(
                    slot,
                    opcode == ShaktoolInstructionCodes.Instruction_Shaktool_Lower1Pixel
                        ? unchecked((byte)(targetDirection ^ 0x80))
                        : targetDirection);
                break;
            }

            case ShaktoolInstructionCodes.RTL_AAD99F:
                // Explicit RTL stub used between the two long attack pauses.
                break;

            case ShaktoolInstructionCodes.Instruction_Shaktool_ResetShaktoolFunctions:
            {
                RoomEnemySlot[] group = GetShaktoolGroup(slot);
                for (int index = 0; index < ShaktoolSegmentCount; index++)
                {
                    RequireShaktoolState(group[index]).PreInstruction =
                        (ShaktoolPreInstruction)ReadWord(
                            _bus!,
                            ShaktoolPreInstructionTable + index * 2);
                }
                break;
            }

            default:
                return false;
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
        (slot.XPosition, slot.XSubposition) = AddEightBitVelocity(
            slot.XPosition,
            slot.XSubposition,
            xVelocity);
        (slot.YPosition, slot.YSubposition) = AddEightBitVelocity(
            slot.YPosition,
            slot.YSubposition,
            yVelocity);
    }

    private static ushort ReadShaktoolCommonSineSample(int fullTableIndex) =>
        unchecked((ushort)EnemyTrigonometryTables.SignedSine((byte)(fullTableIndex - 64)));

    /// <summary>
    /// Ports unused retail routine <c>$AA:DAE5</c>. No cartridge caller reaches it, so the
    /// normal main AI deliberately does not invoke this helper; debugger audits can still
    /// prove the seven authored attack lists independently.
    /// </summary>
    internal bool TryStartUnusedShaktoolAttack(RoomEnemySlot anySegment)
    {
        if ((_nextRandom!() & 0x8431) != 0)
            return false;
        StartUnusedShaktoolAttack(anySegment);
        return true;
    }

    internal void StartUnusedShaktoolAttack(RoomEnemySlot anySegment)
    {
        RoomEnemySlot[] group = GetShaktoolGroup(anySegment);
        for (int index = ShaktoolSegmentCount - 1; index >= 0; index--)
        {
            RoomEnemySlot segment = group[index];
            RequireShaktoolState(segment).PreInstruction =
                ShaktoolPreInstruction.IdleAfterAttack;
            segment.CurrentInstruction = ReadWord(
                _bus!,
                ShaktoolAttackListTable + index * 2);
            segment.InstructionTimer = 1;
        }
    }

    /// <summary>
    /// Implements Shaktool's fatal-shot tail at <c>$AA:DF34</c>: either vulnerable end dying
    /// writes literal property $0200 to all seven records, deleting the complete chain.
    /// </summary>
    private void ResolveShaktoolShotAfterCommon(RoomEnemySlot struckSegment)
    {
        if (struckSegment.Health != 0)
            return;
        foreach (RoomEnemySlot segment in GetShaktoolGroup(struckSegment))
            segment.Properties = (ushort)EnemyProperties.Deleted;
    }
}
