using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Bank-$AA function pointers stored in Shaktool's native variable F. Keeping the raw
/// addresses visible makes debugger watches line up with the ROM while giving each of the
/// five actually-used routines a domain name.
/// </summary>
public enum ShaktoolPreInstruction : ushort
{
    /// <summary><c>$AA:DCAA</c>, native <c>RTS_AADCAA</c>: suspends segment motion during the collision-response instruction sequence until its reset callback restores the segment functions.</summary>
    IdleAfterAttack = 0xdcaa,
    /// <summary><c>$AA:DCAB</c>, native <c>RTS_AADCAB</c>: no-op function for the first, anchoring saw piece; this is not the central head's orientation routine.</summary>
    IdleHead = 0xdcab,
    /// <summary><c>$AA:DCAC</c>, native <c>Function_Shaktool_ArmPiece_SetPosition_HandleCurling</c>: positions an arm relative to the preceding record, then advances its neighbor angle according to the curling flags.</summary>
    OrbitPreviousSegment = 0xdcac,
    /// <summary><c>$AA:DCD7</c>, native <c>Function_Shaktool_Head</c>: performs the arm motion for the middle record and selects one of eight head animations from the adjacent segment angles and flip flag.</summary>
    OrbitAndOrientCenter = 0xdcd7,
    /// <summary><c>$AA:DD25</c>, native <c>Function_Shaktool_FinalPiece</c>: collision-tests the final saw's orbital movement, updates the chain's curl state, and reverses its ends when obstructed.</summary>
    DriveTailAndReverseAtWalls = 0xdd25,
}

/// <summary>
/// Typed view over one of Shaktool's seven consecutive enemy records. The properties are
/// deliberately backed by native variables A-F instead of duplicated host state, so a C#
/// debugger shows exactly the words that the 65816 implementation would mutate.
/// </summary>
public sealed class ShaktoolSegmentState
{
    /// <summary>Enemy record supplying the native variable words exposed by this state view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a typed view over one initialized Shaktool segment record.</summary>
    /// <param name="slot">Enemy slot whose variables A through F store this segment's state.</param>
    internal ShaktoolSegmentState(RoomEnemySlot slot) => _slot = slot;
    /// <summary>Native <c>facingAngle</c> at <c>$0FA8 + enemy index</c>, stored in variable A: the chain's target heading, also advanced during straightening and used by attack movement instructions; one wrapped 16-bit turn is <c>$10000</c>.</summary>
    public ushort TargetAngle
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }
    /// <summary>Native <c>neighborAngle</c> at <c>$0FAA + enemy index</c>, stored in variable B: the angle around the immediately preceding record, in wrapped 16-bit turns; its high byte indexes the 256-angle displacement table.</summary>
    public ushort OrbitAngle
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }
    /// <summary>Native <c>neighborAngleDelta</c> at <c>$0FAC + enemy index</c>, stored in variable C: the unsigned orbital increment per gameplay update in 1/65536-turn units; movement flags determine its sign, and straightening substitutes <c>$0100</c>.</summary>
    public ushort AngularVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }
    /// <summary>Native role-dependent word at <c>$0FAE + enemy index</c>, stored in variable D: the central head uses its low byte for eight orientation buckets and bit 15 for flipping; the final saw instead accumulates angular increments to detect full curling at <c>$F000</c>.</summary>
    public ushort OrientationAndAcceleration
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }
    /// <summary>Native <c>primaryPieceEnemyIndex</c> at <c>$0FB0 + enemy index</c>, stored in variable E: the byte-offset enemy index of the first record in the seven-piece chain, whose consecutive records are <c>$40</c> bytes apart.</summary>
    public ushort OwnerNativeIndex
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }
    /// <summary>Native <c>function</c> at <c>$0FB2 + enemy index</c>, stored in variable F: the bank-$AA segment motion callback run by Shaktool's main AI; collision-response instructions temporarily replace it with the no-op attack function.</summary>
    public ShaktoolPreInstruction PreInstruction
    {
        get => (ShaktoolPreInstruction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>Composable parameter-one bits shared across the seven Shaktool records to track chain reversal and motion mode.</summary>
[Flags]
internal enum ShaktoolMotionFlags : ushort
{
    /// <summary>Records that the chain has completed its first collision-driven endpoint reversal.</summary>
    ReversedOnce = 0x2000,
    /// <summary>Marks the convergence phase in which each segment straightens toward the shared target angle.</summary>
    Straightening = 0x4000,
    /// <summary>Selects clockwise rather than counter-clockwise angular convergence and endpoint orientation.</summary>
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
    /// <summary>Bank-$AA enemy definition pointer for the seven-record Shaktool chain.</summary>
    internal const ushort ShaktoolDefinition = 0xf07f;
    /// <summary>Native bank-$AA touch callback used by Shaktool's segment hitboxes.</summary>
    internal const ushort ShaktoolTouchAi = EnemyAiCodePointers.BankAA.ShaktoolTouch;
    /// <summary>Native bank-$AA shot callback that handles ordinary hits and fatal chain deletion.</summary>
    internal const ushort ShaktoolShotAi = EnemyAiCodePointers.BankAA.ShaktoolShot;

    /// <summary>Number of consecutive enemy records that make up one Shaktool body and saw chain.</summary>
    private const int ShaktoolSegmentCount = 7;

    /// <summary>Initialized typed state views indexed by enemy-slot index for the current room.</summary>
    private readonly ShaktoolSegmentState?[] _shaktoolSegments =
        new ShaktoolSegmentState?[MaximumEnemyCount];

    /// <summary>Clears cached segment views when a room reset invalidates the previous enemy population.</summary>
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
    private void PositionShaktoolAroundPreviousSegment(
        RoomEnemySlot slot,
        ShaktoolSegmentState state)
    {
        if (slot.SlotIndex == 0)
            throw new InvalidOperationException("Shaktool's anchor has no preceding segment.");

        RoomEnemySlot previous = _slots[slot.SlotIndex - 1];
        // $AA:DC42/$DC4A/$DC60/$DC68 read the preceding physical record, without
        // testing its header. A lethal shot clears the head and marks the group
        // deleted, but EnemyMain still visits later entries in this frame's
        // prebuilt active list. Their final orbit uses the cleared coordinates.

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

    /// <summary>Handles the tail's collision response: first collision swaps chain endpoints, later collision reverses rotation.</summary>
    /// <param name="slot">Tail segment whose movement encountered the collision.</param>
    /// <param name="state">Tail state used to find the chain owner and update its orientation fields.</param>
    /// <param name="previousX">Whole-pixel X position to restore before the initial endpoint reversal.</param>
    /// <param name="previousY">Whole-pixel Y position to restore before the initial endpoint reversal.</param>
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

    /// <summary>Swaps a mirrored pair of orbital angles, applying the half-turn and discarding fractional angle bytes.</summary>
    /// <param name="left">Segment receiving the mirrored angle from the right segment.</param>
    /// <param name="right">Segment receiving the mirrored angle from the left segment.</param>
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
            state.AngularVelocity = ShaktoolSegmentDefinitions.ForIndex(index).AngularVelocity;
        }
    }

    /// <summary>Writes one target heading to every initialized member of the chain.</summary>
    /// <param name="anySegment">Any segment whose stored owner index identifies the chain.</param>
    /// <param name="target">Wrapped 16-bit turn angle adopted by all segments.</param>
    private void SetShaktoolGroupTargetAngle(RoomEnemySlot anySegment, ushort target)
    {
        foreach (RoomEnemySlot segment in GetShaktoolGroup(anySegment))
            RequireShaktoolState(segment).TargetAngle = target;
    }

    /// <summary>Copies the shared chain motion flags into parameter one of all seven segments.</summary>
    /// <param name="anySegment">Any segment used to resolve the chain's first record.</param>
    /// <param name="flags">Combined reversal, straightening, and rotation-direction bits.</param>
    private void SetShaktoolGroupMotionFlags(
        RoomEnemySlot anySegment,
        ShaktoolMotionFlags flags)
    {
        foreach (RoomEnemySlot segment in GetShaktoolGroup(anySegment))
            segment.Parameter1 = (ushort)flags;
    }

    /// <summary>Resolves and validates the seven consecutive enemy records belonging to a Shaktool chain.</summary>
    /// <param name="anySegment">Initialized segment whose native owner index points to the chain's first record.</param>
    /// <returns>All seven initialized slots in head-to-tail order.</returns>
    /// <exception cref="InvalidDataException">The owner index is misaligned, exceeds the slot array, or a segment lacks state.</exception>
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

    /// <summary>Returns the next physical record, which is the orbit center for a non-tail segment.</summary>
    /// <param name="slot">Current segment in slot order.</param>
    /// <returns>The immediately following enemy slot.</returns>
    /// <exception cref="InvalidDataException">The slot array has no record after the supplied segment.</exception>
    private RoomEnemySlot GetNextShaktoolSegment(RoomEnemySlot slot)
    {
        if (slot.SlotIndex + 1 >= _slots.Length)
        {
            throw new InvalidDataException(
                $"Shaktool center slot {slot.SlotIndex} has no following segment.");
        }
        return _slots[slot.SlotIndex + 1];
    }

    /// <summary>Returns the state view created by Shaktool initialization for a slot.</summary>
    /// <param name="slot">Enemy slot expected to belong to the active Shaktool population.</param>
    /// <returns>The view backed by the slot's native variables.</returns>
    /// <exception cref="InvalidOperationException">No Shaktool state was initialized for the slot.</exception>
    private ShaktoolSegmentState RequireShaktoolState(RoomEnemySlot slot) =>
        _shaktoolSegments[slot.SlotIndex] is { } state
            ? state
            : throw new InvalidOperationException(
                $"Enemy slot {slot.SlotIndex} has no initialized Shaktool state.");

    /// <summary>Adds signed fixed-point displacement to a 16.16 position with native unchecked wrapping.</summary>
    /// <param name="position">Whole-pixel part of the starting coordinate.</param>
    /// <param name="subposition">Fractional 16-bit part of the starting coordinate.</param>
    /// <param name="displacement">Signed fixed-point movement amount.</param>
    /// <returns>The wrapped whole and fractional coordinate after the movement.</returns>
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
                        ShaktoolSegmentDefinitions.ForIndex(index).PreInstruction;
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

    /// <summary>Reads the signed sine displacement corresponding to an index in the decompiler's 320-word table view.</summary>
    /// <param name="fullTableIndex">Full-view index, where entries 64 through 319 alias the cartridge's 256-sample table.</param>
    /// <returns>The signed table sample represented as its unchecked 16-bit word.</returns>
    private static ushort ReadShaktoolCommonSineSample(int fullTableIndex) =>
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
        int ownerSlotIndex = ownerNativeIndex / NativeSlotSize;
        if (ownerNativeIndex % NativeSlotSize != 0 ||
            ownerSlotIndex + ShaktoolSegmentCount > _slots.Length)
            throw new InvalidDataException($"Shaktool post-shot owner ${ownerNativeIndex:X4} is invalid.");
        for (int index = 0; index < ShaktoolSegmentCount; index++)
            _slots[ownerSlotIndex + index].Properties = (ushort)EnemyProperties.Deleted;
    }
}
