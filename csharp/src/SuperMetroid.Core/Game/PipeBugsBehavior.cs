using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>The pipe bugs species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PipeBugsBehavior : IEnemySpecies
{
    /// <summary>Stores slot table.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Initializes a new pipe bugs behavior instance.</summary>
    internal PipeBugsBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Stores brinstar pipe bug emergence height.</summary>
    private const ushort BrinstarPipeBugEmergenceHeight = 16;
    /// <summary>Stores brinstar pipe bug trigger width.</summary>
    private const ushort BrinstarPipeBugTriggerWidth = 64;
    /// <summary>Stores brinstar pipe bug trigger top.</summary>
    private const ushort BrinstarPipeBugTriggerTop = 96;
    /// <summary>Stores brinstar pipe bug respawn frames.</summary>
    private const ushort BrinstarPipeBugRespawnFrames = 48;

    /// <summary>Stores norfair formation size.</summary>
    private const int NorfairFormationSize = 5;

    /// <summary>Stores yellow pipe bug trigger distance.</summary>
    private const ushort YellowPipeBugTriggerDistance = 192;
    /// <summary>Stores yellow pipe bug trigger height.</summary>
    private const ushort YellowPipeBugTriggerHeight = 48;
    /// <summary>Stores yellow pipe bug emergence delay frames.</summary>
    private const ushort YellowPipeBugEmergenceDelayFrames = 24;
    /// <summary>Stores yellow pipe bug arc trigger distance.</summary>
    private const ushort YellowPipeBugArcTriggerDistance = 48;
    /// <summary>Stores yellow pipe bug arc counter start.</summary>
    private const ushort YellowPipeBugArcCounterStart = 40;

    /// <summary>Stores pipe bug states.</summary>
    internal readonly PipeBugEnemyState?[] _pipeBugStates =
        new PipeBugEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Checks whether the definition identifies a Brinstar pipe bug.</summary>
    internal static bool IsBrinstarPipeBugDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.Zeb or
            EnemyDefinitionId.Zebbo;

    /// <summary>Resets pipe bug room state.</summary>
    internal void ResetPipeBugRoomState() => Array.Clear(_pipeBugStates);

    /// <summary>Ports <c>BrinstarPipeBug_Init</c> at $B3:883B.</summary>
    internal void InitializeBrinstarPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        state.EmergenceTopY = unchecked((ushort)(slot.Position.Y.Whole - BrinstarPipeBugEmergenceHeight));
        state.Function = PipeBugEnemyFunction.BrinstarWaitUntilOnScreen;
        state.DelayOrCounter = BrinstarPipeBugRespawnFrames;
        state.AnimationState = PipeBugAnimationSelector.None;
        state.InstalledAnimationState = PipeBugAnimationSelector.None;
        slot.CurrentInstruction = PipeBugDefinitions.BrinstarInstructionList(
            strong: slot.Parameter1 != 0,
            PipeBugAnimationSelector.None);
    }

    /// <summary>Ports <c>NorfairPipeBug_Init</c> at $B3:8B61.</summary>
    internal void InitializeNorfairPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.LinearSpeedTableOffset = unchecked((ushort)((SnesLittleEndian.HighByte(slot.Parameter2)) * 8));
        state.DelayOrCounter = 0;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = NorfairPipeBugInstructionProgramDefinitions.RisingLeft;
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
    }

    /// <summary>Ports <c>BrinstarYellowPipeBug_Init</c> at $B3:8F4C.</summary>
    internal void InitializeYellowPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = slot.Parameter1 != 0
            ? YellowPipeBugInstructionProgramDefinitions.FlyingLeft
            : YellowPipeBugInstructionProgramDefinitions.FlyingRight;

        // Parameter two is a logical entry index. Each common linear-speed record is eight
        // bytes: positive fraction/whole followed by its separately authored negative pair.
        ushort speedOffset = unchecked((ushort)(slot.Parameter2 * 8));
        (short positiveWhole, ushort positiveFraction) = EnemyMath.ReadLinearEnemySpeed(speedOffset);
        (short negativeWhole, ushort negativeFraction) =
            EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)(speedOffset + 4)));
        state.PositiveVelocityWhole = unchecked((ushort)positiveWhole);
        state.PositiveVelocityFraction = positiveFraction;
        state.NegativeVelocityWhole = unchecked((ushort)negativeWhole);
        state.NegativeVelocityFraction = negativeFraction;
        state.Function = PipeBugEnemyFunction.YellowWaitForSamus;
        state.ArcCompleted = false;
    }

    /// <summary>Dispatches the three bank-$B3 main routines and every indirect target.</summary>
    private void RunPipeBugMain(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        switch (state.Function)
        {
            case PipeBugEnemyFunction.BrinstarWaitUntilOnScreen:
                // The poorly named native CheckIfEnemyIsOnScreen returns zero when the
                // origin is visible. $B3:8880 branches on that zero result, so this initial
                // state arms the proximity check on-screen, not after a despawn boundary.
                if (!EnemyProcessingWindow.EnemyWithNormalSpritesIsOffScreen(slot, camera))
                    state.Function = PipeBugEnemyFunction.BrinstarWaitForSamus;
                return;
            case PipeBugEnemyFunction.BrinstarWaitForSamus:
                RequirePipeBugSamus(samus);
                RunBrinstarPipeBugWaiting(slot, state, samus!);
                return;
            case PipeBugEnemyFunction.BrinstarEmerge:
                RequirePipeBugSamus(samus);
                RunBrinstarPipeBugEmergence(slot, state, samus!);
                return;
            case PipeBugEnemyFunction.BrinstarFlyHorizontally:
                RunBrinstarPipeBugFlight(slot, state, camera.X);
                return;
            case PipeBugEnemyFunction.BrinstarRespawnDelay:
                if (state.DelayOrCounter-- == 1)
                    state.Function = PipeBugEnemyFunction.BrinstarWaitForSamus;
                return;

            case PipeBugEnemyFunction.NorfairWaitForFormation:
                RunNorfairPipeBugFormationWait(slot, state);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairWaitForSamus:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugSamusWait(slot, state, samus!);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairRise:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugRise(slot, state, samus!);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairLeaderStagger:
                state.DelayOrCounter = unchecked((ushort)(state.DelayOrCounter + 1));
                state.Function = PipeBugEnemyFunction.NorfairWaitForStagger;
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairUpperNearStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, -16);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairUpperFarStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, -32);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairLowerNearStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, 16);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairLowerFarStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, 32);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairWaitForStagger:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugStaggerWait(slot, state, samus!);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairFlyLeft:
                AddPipeBugLinearVelocity(slot, horizontal: true, state.LinearSpeedTableOffset + 4);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairFlyRight:
                AddPipeBugLinearVelocity(slot, horizontal: true, state.LinearSpeedTableOffset);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;

            case PipeBugEnemyFunction.YellowWaitForSamus:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugWaiting(slot, state, samus!);
                return;
            case PipeBugEnemyFunction.YellowEmergenceDelay:
                RunYellowPipeBugDelay(slot, state);
                return;
            case PipeBugEnemyFunction.YellowFlyLeft:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugStraightFlight(slot, state, samus!, camera, movingLeft: true);
                return;
            case PipeBugEnemyFunction.YellowFlyRight:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugStraightFlight(slot, state, samus!, camera, movingLeft: false);
                return;
            case PipeBugEnemyFunction.YellowArcLeft:
                RunYellowPipeBugArc(slot, state, camera, movingLeft: true);
                return;
            case PipeBugEnemyFunction.YellowArcRight:
                RunYellowPipeBugArc(slot, state, camera, movingLeft: false);
                return;
            default:
                throw new InvalidDataException(
                    $"Pipe Bug function $B3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Runs brinstar pipe bug waiting.</summary>
    private static void RunBrinstarPipeBugWaiting(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        short deltaY = unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole));
        if (deltaY >= 0 || unchecked((short)(deltaY + BrinstarPipeBugTriggerTop)) < 0)
            return;

        ushort deltaX = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        state.VariableA = (ushort)(state.VariableA & 0x7fff | (deltaX & 0x8000));
        if (EnemyMath.WrappedMagnitude(unchecked((ushort)(slot.Position.X.Whole - samus.Position.X.Whole))) >=
            BrinstarPipeBugTriggerWidth)
        {
            return;
        }

        state.Function = PipeBugEnemyFunction.BrinstarEmerge;
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        slot.Timer = 0;
        state.AnimationState = (state.VariableA & 0x8000) != 0
            ? PipeBugAnimationSelector.None
            : PipeBugAnimationSelector.FacingRight;
        SelectBrinstarPipeBugAnimation(slot, state);
    }

    /// <summary>Runs brinstar pipe bug emergence.</summary>
    private static void RunBrinstarPipeBugEmergence(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        // $B3:88E3 decrements the subposition as a separate word, then subtracts either one
        // or two pixels according to the old word. This is deliberately not normal 16.16
        // subtraction; preserving the oddity prevents a one-pixel/frame timing drift.
        SnesFixedPosition y = slot.Position.Y;
        bool oldSubpositionWasNonzero = y.Fraction != 0;
        slot.Position = slot.Position.WithY(new(
            unchecked((ushort)(y.Whole + (oldSubpositionWasNonzero ? -1 : -2))),
            unchecked((ushort)(y.Fraction - 1))));
        if (unchecked((short)(state.EmergenceTopY - slot.Position.Y.Whole)) < 0 ||
            slot.Position.Y.Whole >= samus.Position.Y.Whole)
        {
            return;
        }

        state.AnimationState |= PipeBugAnimationSelector.Shooting;
        SelectBrinstarPipeBugAnimation(slot, state);
        state.Function = PipeBugEnemyFunction.BrinstarFlyHorizontally;
    }

    /// <summary>Runs brinstar pipe bug flight.</summary>
    private static void RunBrinstarPipeBugFlight(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        ushort cameraX)
    {
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
            ((state.VariableA & 0x8000) != 0 ? -2 : 2))));
        // $B3:8949 tests only the horizontal screen extent.
        if (!EnemyProcessingWindow.EnemyIsHorizontallyOffScreen(slot, cameraX))
            return;

        slot.Position = slot.Position.WithWhole(state.SpawnPosition).WithFractionX(0);
        // This apparently strange assignment is literal $B3:8958 behavior: the original
        // stores the integer spawn Y into both halves rather than clearing the fraction.
        slot.Position = slot.Position.WithFractionY(state.SpawnPosition.Y);
        state.AnimationState = PipeBugAnimationSelector.None;
        SelectBrinstarPipeBugAnimation(slot, state);
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.DelayOrCounter = BrinstarPipeBugRespawnFrames;
        state.Function = PipeBugEnemyFunction.BrinstarRespawnDelay;
    }

    /// <summary>Selects brinstar pipe bug animation.</summary>
    internal static void SelectBrinstarPipeBugAnimation(
        RoomEnemySlot slot,
        PipeBugEnemyState state)
    {
        if (state.AnimationState == state.InstalledAnimationState)
            return;
        state.InstalledAnimationState = state.AnimationState;
        slot.CurrentInstruction = PipeBugDefinitions.BrinstarInstructionList(
            strong: slot.Parameter1 != 0,
            state.AnimationState);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Runs norfair pipe bug formation wait.</summary>
    private void RunNorfairPipeBugFormationWait(RoomEnemySlot slot, PipeBugEnemyState state)
    {
        // Only the first record has a nonzero low parameter byte. It is the formation
        // controller and may arm itself only while all four following physical records are
        // still at the dormant function. The native code addresses them at +$40 increments.
        if ((SnesLittleEndian.LowByte(slot.Parameter2)) == 0)
            return;
        RoomEnemySlot[] formation = RequireNorfairPipeBugFormation(slot);
        if (formation.All(member =>
                RequirePipeBugState(member).Function == PipeBugEnemyFunction.NorfairWaitForFormation))
        {
            state.Function = PipeBugEnemyFunction.NorfairWaitForSamus;
        }
    }

    /// <summary>Runs norfair pipe bug samus wait.</summary>
    internal void RunNorfairPipeBugSamusWait(
        RoomEnemySlot leader,
        PipeBugEnemyState leaderState,
        SamusState samus)
    {
        ushort triggerWidth = unchecked((byte)leader.Parameter2);
        if (!EnemyMath.IsWithinStrictModularDistance(leader.Position.X.Whole, samus.Position.X.Whole, triggerWidth) ||
            unchecked((short)(leader.Position.Y.Whole - samus.Position.Y.Whole)) < 0)
        {
            return;
        }

        leaderState.DelayOrCounter = unchecked((ushort)(leaderState.DelayOrCounter + 1));
        // $B3:8C19-$8C1F resets only the current (leader) slot. The following
        // formation stores change every instruction pointer, but leave follower
        // timers intact, including zero timers in cleared respawn placeholders.
        leader.InstructionTimer = 1;
        leader.Timer = 0;
        bool samusIsRight = unchecked((short)(samus.Position.X.Whole - leader.Position.X.Whole)) >= 0;
        RoomEnemySlot[] formation = RequireNorfairPipeBugFormation(leader);
        foreach (RoomEnemySlot member in formation)
        {
            member.CurrentInstruction = samusIsRight
                ? NorfairPipeBugInstructionProgramDefinitions.RisingRight
                : NorfairPipeBugInstructionProgramDefinitions.RisingLeft;
            RequirePipeBugState(member).Function = PipeBugEnemyFunction.NorfairRise;
        }

        for (int index = 0; index < formation.Length; index++)
        {
            PipeBugEnemyState memberState = RequirePipeBugState(formation[index]);
            memberState.StaggerTarget = PipeBugDefinitions.NorfairStaggerTarget(index);
            memberState.NorfairPostRiseFunction = PipeBugDefinitions.NorfairPostRiseFunction(index);
        }
    }

    /// <summary>Runs norfair pipe bug rise.</summary>
    private static void RunNorfairPipeBugRise(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        AddPipeBugLinearVelocity(slot, horizontal: false, byteOffset: 132);
        if (unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)) >= 0)
            return;

        state.Function = state.NorfairPostRiseFunction;
        state.EmergenceY = slot.Position.Y.Whole;
        InstallPipeBugInstruction(
            slot,
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0
                ? NorfairPipeBugInstructionProgramDefinitions.RisingRight
                : NorfairPipeBugInstructionProgramDefinitions.RisingLeft);
    }

    /// <summary>Runs norfair pipe bug vertical stagger.</summary>
    private static void RunNorfairPipeBugVerticalStagger(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        short signedOffset)
    {
        state.DelayOrCounter = unchecked((ushort)(state.DelayOrCounter + 1));
        // Negative offsets move upward with table entry 66; positive offsets move downward
        // with entry 64. The native routine clamps exactly at emergenceY +/- 16/32.
        AddPipeBugLinearVelocity(
            slot,
            horizontal: false,
            byteOffset: signedOffset < 0 ? 132 : 128);
        ushort target = unchecked((ushort)(state.EmergenceY + signedOffset));
        bool reached = signedOffset < 0
            ? unchecked((short)(slot.Position.Y.Whole - target)) < 0
            : unchecked((short)(slot.Position.Y.Whole - target)) >= 0;
        if (!reached)
            return;
        slot.Position = slot.Position.WithY(new(target, 0));
        state.Function = PipeBugEnemyFunction.NorfairWaitForStagger;
    }

    /// <summary>Runs norfair pipe bug stagger wait.</summary>
    private static void RunNorfairPipeBugStaggerWait(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        state.DelayOrCounter = unchecked((ushort)(state.DelayOrCounter + 1));
        if (unchecked((short)(state.DelayOrCounter - state.StaggerTarget)) < 0)
            return;

        state.DelayOrCounter = 0;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0)
        {
            state.Function = PipeBugEnemyFunction.NorfairFlyRight;
            slot.CurrentInstruction = NorfairPipeBugInstructionProgramDefinitions.FlyingRight;
        }
        else
        {
            state.Function = PipeBugEnemyFunction.NorfairFlyLeft;
            slot.CurrentInstruction = NorfairPipeBugInstructionProgramDefinitions.FlyingLeft;
        }
    }

    /// <summary>
    /// <c>ResetEnemyIfOffScreen</c> ($B3:8BA8) tests the bare enemy center with
    /// <c>CheckIfEnemyCenterIsOnScreen</c>, not the sprite-padded visibility check.
    /// </summary>
    private static void ResetNorfairPipeBugIfOffScreen(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        Position16<RoomPixels> camera)
    {
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
            return;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
    }

    /// <summary>Requires norfair pipe bug formation.</summary>
    private RoomEnemySlot[] RequireNorfairPipeBugFormation(RoomEnemySlot leader)
    {
        if (leader.SlotIndex + NorfairFormationSize > RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidDataException("Norfair Pipe Bug formation overruns the physical enemy pool.");
        var formation = new RoomEnemySlot[NorfairFormationSize];
        for (int index = 0; index < formation.Length; index++)
        {
            RoomEnemySlot member = _slotTable._slots[leader.SlotIndex + index];
            PipeBugEnemyState? state = _pipeBugStates[member.SlotIndex];
            if (state?.DefinitionAtInitialization != EnemyDefinitionId.Gamet)
            {
                throw new InvalidDataException(
                    $"Norfair Pipe Bug formation slot {leader.SlotIndex + index} is " +
                    $"owned by initialized enemy ${(int)(state?.DefinitionAtInitialization ?? 0):X4}, " +
                    $"expected ${(int)EnemyDefinitionId.Gamet:X4}.");
            }
            // `$B3:8BCD/$8BFF/$8C52` never re-check the live definition. They read and
            // write five consecutive 64-byte records even after generic death has changed
            // one member's definition to zero. Retaining the initialized typed owner here
            // preserves that raw alias without accepting an unrelated never-Pipe-Bug slot.
            formation[index] = member;
        }
        return formation;
    }

    /// <summary>Runs yellow pipe bug waiting.</summary>
    private static void RunYellowPipeBugWaiting(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        short deltaX = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole));
        bool inFront = slot.Parameter1 != 0
            ? deltaX < 0 && unchecked((short)(deltaX + YellowPipeBugTriggerDistance)) >= 0
            : deltaX >= 0 && unchecked((short)(deltaX - YellowPipeBugTriggerDistance)) < 0;
        if (!inFront ||
            !EnemyMath.IsWithinStrictModularDistance(slot.Position.Y.Whole, samus.Position.Y.Whole, YellowPipeBugTriggerHeight))
        {
            return;
        }

        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        state.EmergenceDelay = YellowPipeBugEmergenceDelayFrames;
        state.Function = PipeBugEnemyFunction.YellowEmergenceDelay;
    }

    /// <summary>Runs yellow pipe bug delay.</summary>
    private static void RunYellowPipeBugDelay(RoomEnemySlot slot, PipeBugEnemyState state)
    {
        state.EmergenceDelay = unchecked((ushort)(state.EmergenceDelay - 1));
        if (state.EmergenceDelay != 0)
            return;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        if (slot.Parameter1 != 0)
        {
            slot.CurrentInstruction = YellowPipeBugInstructionProgramDefinitions.FlyingLeft;
            state.Function = PipeBugEnemyFunction.YellowFlyLeft;
        }
        else
        {
            slot.CurrentInstruction = YellowPipeBugInstructionProgramDefinitions.FlyingRight;
            state.Function = PipeBugEnemyFunction.YellowFlyRight;
        }
    }

    /// <summary>Runs yellow pipe bug straight flight.</summary>
    private static void RunYellowPipeBugStraightFlight(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus,
        Position16<RoomPixels> camera,
        bool movingLeft)
    {
        AddYellowPipeBugHorizontalVelocity(slot, state, movingLeft);
        if (ResetYellowPipeBugIfOffScreen(slot, state, camera))
            return;
        if (state.ArcCompleted)
            return;

        bool passedSamus = movingLeft
            ? unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole - YellowPipeBugArcTriggerDistance)) < 0
            : unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole - YellowPipeBugArcTriggerDistance)) < 0;
        if (!passedSamus)
            return;

        state.Function = movingLeft
            ? PipeBugEnemyFunction.YellowArcLeft
            : PipeBugEnemyFunction.YellowArcRight;
        state.DelayOrCounter = 0;
        state.ArcCounter = YellowPipeBugArcCounterStart;
        state.ArcPhase = 1;
        state.ArcStartX = slot.Position.X.Whole;
        InstallPipeBugInstruction(
            slot,
            movingLeft
                ? YellowPipeBugInstructionProgramDefinitions.ArcingLeft
                : YellowPipeBugInstructionProgramDefinitions.ArcingRight);
    }

    /// <summary>Runs yellow pipe bug arc.</summary>
    private static void RunYellowPipeBugArc(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        Position16<RoomPixels> camera,
        bool movingLeft)
    {
        if (ResetYellowPipeBugIfOffScreen(slot, state, camera))
            return;
        AddYellowPipeBugHorizontalVelocity(slot, state, movingLeft);
        if (state.ArcPhase != 0)
        {
            state.ArcCounter = unchecked((ushort)(state.ArcCounter - 1));
            if ((state.ArcCounter & 0x8000) != 0)
            {
                state.ArcCounter = 0;
                state.ArcPhase = 0;
            }
            else
            {
                AddPipeBugDisplacement(slot, horizontal: false,
                    EnemyMath.ReadQuadraticEnemySpeed(state.ArcCounter, negative: false));
            }
            return;
        }

        state.ArcCounter = unchecked((ushort)(state.ArcCounter + 1));
        AddPipeBugDisplacement(slot, horizontal: false,
            EnemyMath.ReadQuadraticEnemySpeed(state.ArcCounter, negative: true));
        bool returnedToSpawnY = movingLeft
            ? unchecked((short)(slot.Position.Y.Whole - state.SpawnPosition.Y)) < 0
            : unchecked((short)(state.SpawnPosition.Y - slot.Position.Y.Whole)) >= 0;
        if (!returnedToSpawnY)
            return;

        state.ArcCompleted = true;
        state.ArcPhase = 1;
        state.Function = movingLeft
            ? PipeBugEnemyFunction.YellowFlyLeft
            : PipeBugEnemyFunction.YellowFlyRight;
        InstallPipeBugInstruction(
            slot,
            movingLeft
                ? YellowPipeBugInstructionProgramDefinitions.FlyingLeft
                : YellowPipeBugInstructionProgramDefinitions.FlyingRight);
    }

    /// <summary>Resets yellow pipe bug if off screen.</summary>
    private static bool ResetYellowPipeBugIfOffScreen(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        Position16<RoomPixels> camera)
    {
        // Every Geega flight function resets through CheckIfEnemyCenterIsOnScreen.
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
            return false;
        slot.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
        state.Function = PipeBugEnemyFunction.YellowWaitForSamus;
        state.ArcCompleted = false;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = slot.Parameter1 != 0
            ? YellowPipeBugInstructionProgramDefinitions.FlyingLeft
            : YellowPipeBugInstructionProgramDefinitions.FlyingRight;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        return true;
    }

    /// <summary>Adds yellow pipe bug horizontal velocity.</summary>
    private static void AddYellowPipeBugHorizontalVelocity(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        bool movingLeft)
    {
        ushort whole = movingLeft ? state.NegativeVelocityWhole : state.PositiveVelocityWhole;
        ushort fraction = movingLeft
            ? state.NegativeVelocityFraction
            : state.PositiveVelocityFraction;
        AddPipeBugFixedVelocity(slot, horizontal: true, whole, fraction);
    }

    /// <summary>Adds pipe bug linear velocity.</summary>
    private static void AddPipeBugLinearVelocity(
        RoomEnemySlot slot,
        bool horizontal,
        int byteOffset)
    {
        (short whole, ushort fraction) = EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)byteOffset));
        AddPipeBugFixedVelocity(slot, horizontal, unchecked((ushort)whole), fraction);
    }

    /// <summary>Adds pipe bug fixed velocity.</summary>
    private static void AddPipeBugFixedVelocity(
        RoomEnemySlot slot,
        bool horizontal,
        ushort whole,
        ushort fraction)
    {
        ushort position = horizontal ? slot.Position.X.Whole : slot.Position.Y.Whole;
        ushort subposition = horizontal ? slot.Position.X.Fraction : slot.Position.Y.Fraction;
        uint sum = (uint)subposition + fraction;
        position = unchecked((ushort)(position + whole + (sum > ushort.MaxValue ? 1 : 0)));
        subposition = unchecked((ushort)sum);
        if (horizontal)
        {
            slot.Position = slot.Position.WithX(new(position, subposition));
        }
        else
        {
            slot.Position = slot.Position.WithY(new(position, subposition));
        }
    }

    /// <summary>Adds pipe bug displacement.</summary>
    private static void AddPipeBugDisplacement(
        RoomEnemySlot slot,
        bool horizontal,
        int displacement)
    {
        uint position = horizontal
            ? slot.Position.X.RawValue
            : slot.Position.Y.RawValue;
        position = unchecked(position + (uint)displacement);
        if (horizontal)
        {
            slot.Position = slot.Position.WithX(SnesFixedPosition.FromRaw(unchecked((uint)position)));
        }
        else
        {
            slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked((uint)position)));
        }
    }

    /// <summary>Creates pipe bug state.</summary>
    private PipeBugEnemyState CreatePipeBugState(RoomEnemySlot slot)
    {
        var state = new PipeBugEnemyState(slot);
        _pipeBugStates[slot.SlotIndex] = state;
        return state;
    }

    /// <summary>Requires pipe bug state.</summary>
    private PipeBugEnemyState RequirePipeBugState(RoomEnemySlot slot) =>
        _pipeBugStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Pipe Bug state.");

    /// <summary>Installs pipe bug instruction.</summary>
    internal static void InstallPipeBugInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>Requires pipe bug samus.</summary>
    private static void RequirePipeBugSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Pipe Bug behavior requires the active Samus actor.");
    }

    /// <summary>Registers pipe bugs initialization ai.</summary>
    internal void RegisterPipeBugsInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Zeb_Zebbo, slot => IsBrinstarPipeBugDefinition(slot.EnemyDefinitionPointer), (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeBrinstarPipeBug(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Gamet, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Gamet, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeNorfairPipeBug(slot);
        });

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Geega, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Geega, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeYellowPipeBug(slot);
        });
    }

    /// <summary>Registers pipe bugs main ai.</summary>
    internal void RegisterPipeBugsMainAi(EnemyAiTable table)
    {

        EnemyMainAiHandler shared88 = (slot, frame, nmiFrameCounter8) =>
        {
            RunPipeBugMain(
                slot,
                RequirePipeBugState(slot),
                frame.Samus,
                frame.Camera);
        };

        table.RegisterMain(EnemyAiRoutine.MainAI_Zeb_Zebbo, slot => IsBrinstarPipeBugDefinition(slot.EnemyDefinitionPointer), shared88);

        table.RegisterMain(EnemyAiRoutine.MainAI_Gamet, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Gamet, shared88);

        table.RegisterMain(EnemyAiRoutine.MainAI_Geega, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Geega, shared88);
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        ResetPipeBugRoomState();
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
