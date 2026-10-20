using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.PipeBugs.Artwork;
using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.PipeBugs;

/// <summary>The pipe bugs species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class PipeBugsBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. PipeBugsPartDefinitions.Parts.Keys];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new PipeBugsEntity(this, slot, common);

    /// <summary>Provides the consecutive physical slots used by Norfair's five-bug formation.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds the shared enemy pool needed to validate Norfair formation membership.</summary>
    internal PipeBugsBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Pixels a Brinstar pipe bug rises above its stored spawn Y before flying.</summary>
    private const int BrinstarPipeBugEmergenceHeight = 16;
    /// <summary>Strict horizontal range that wakes a hidden Brinstar pipe bug.</summary>
    private const ushort BrinstarPipeBugTriggerWidth = 64;
    /// <summary>Vertical band above a Brinstar bug in which Samus can trigger emergence.</summary>
    private const ushort BrinstarPipeBugTriggerTop = 96;
    /// <summary>Native wait before an offscreen Brinstar bug may emerge again.</summary>
    private const ushort BrinstarPipeBugRespawnFrames = 48;

    /// <summary>Leader plus four consecutive followers addressed by Norfair's native routine.</summary>
    private const int NorfairFormationSize = 5;

    /// <summary>Forward horizontal range that wakes a yellow pipe bug.</summary>
    private const ushort YellowPipeBugTriggerDistance = 192;
    /// <summary>Strict vertical distance allowed by the yellow bug's Samus trigger.</summary>
    private const ushort YellowPipeBugTriggerHeight = 48;
    /// <summary>Frames between a yellow bug becoming visible and starting its straight flight.</summary>
    private const ushort YellowPipeBugEmergenceDelayFrames = 24;
    /// <summary>Distance past Samus at which a yellow bug begins its one-time arc.</summary>
    private const ushort YellowPipeBugArcTriggerDistance = 48;
    /// <summary>First quadratic-speed index for the yellow bug's vertical arc.</summary>
    private const ushort YellowPipeBugArcCounterStart = 40;

    /// <summary>Ports <c>BrinstarPipeBug_Init</c> at $B3:883B.</summary>
    internal static void InitializeBrinstarPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        state.EmergenceTop = state.SpawnPosition + new Offset16<RoomPixels>(0, -BrinstarPipeBugEmergenceHeight);
        state.Function = PipeBugEnemyFunction.BrinstarWaitUntilOnScreen;
        state.RespawnDelay = NativeCountdown.Start(BrinstarPipeBugRespawnFrames);
        state.AnimationState = PipeBugAnimationSelector.None;
        state.InstalledAnimationState = PipeBugAnimationSelector.None;
        state.Program = AnimationProgramCursor<PipeBugProgram>.Start(PipeBugDefinitions.BrinstarProgram(
            strong: slot.Spawn.Population.Parameter1 != 0,
            PipeBugAnimationSelector.None));
    }

    /// <summary>Ports <c>NorfairPipeBug_Init</c> at $B3:8B61.</summary>
    internal static void InitializeNorfairPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.LinearSpeedTableOffset = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter2) * EnemyLinearSpeedDefinitions.RecordSize;
        state.FormationCounter = 0;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<PipeBugProgram>.Start(PipeBugProgram.GametRisingLeft);
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
    }

    /// <summary>Ports <c>BrinstarYellowPipeBug_Init</c> at $B3:8F4C.</summary>
    internal static void InitializeYellowPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnPosition = slot.Position.Whole;
        slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<PipeBugProgram>.Start(slot.Spawn.Population.Parameter1 != 0
            ? PipeBugProgram.GeegaFlyingLeft
            : PipeBugProgram.GeegaFlyingRight);

        // Parameter two is a logical entry index. Each common linear-speed record is eight
        // bytes: positive fraction/whole followed by its separately authored negative pair.
        ushort speedOffset = unchecked((ushort)(slot.Spawn.Population.Parameter2 * 8));
        state.PositiveVelocity = ReadPipeBugLinearVelocity(speedOffset);
        state.NegativeVelocity = ReadPipeBugLinearVelocity(unchecked((ushort)(speedOffset + 4)));
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
                RunBrinstarPipeBugWaiting(slot, state, samus);
                return;
            case PipeBugEnemyFunction.BrinstarEmerge:
                RequirePipeBugSamus(samus);
                RunBrinstarPipeBugEmergence(slot, state, samus);
                return;
            case PipeBugEnemyFunction.BrinstarFlyHorizontally:
                RunBrinstarPipeBugFlight(slot, state, camera.X);
                return;
            case PipeBugEnemyFunction.BrinstarRespawnDelay:
                state.RespawnDelay = state.RespawnDelay.Decrement().Remaining;
                if (state.RespawnDelay.IsZero)
                    state.Function = PipeBugEnemyFunction.BrinstarWaitForSamus;
                return;

            case PipeBugEnemyFunction.NorfairWaitForFormation:
                RunNorfairPipeBugFormationWait(slot, state);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairWaitForSamus:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugSamusWait(slot, state, samus);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairRise:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugRise(slot, state, samus);
                ResetNorfairPipeBugIfOffScreen(slot, state, camera);
                return;
            case PipeBugEnemyFunction.NorfairLeaderStagger:
                state.CountFormationTick();
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
                RunNorfairPipeBugStaggerWait(slot, state, samus);
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
                RunYellowPipeBugWaiting(slot, state, samus);
                return;
            case PipeBugEnemyFunction.YellowEmergenceDelay:
                RunYellowPipeBugDelay(slot, state);
                return;
            case PipeBugEnemyFunction.YellowFlyLeft:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugStraightFlight(slot, state, samus, camera, movingLeft: true);
                return;
            case PipeBugEnemyFunction.YellowFlyRight:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugStraightFlight(slot, state, samus, camera, movingLeft: false);
                return;
            case PipeBugEnemyFunction.YellowArcLeft:
                RunYellowPipeBugArc(slot, state, camera, movingLeft: true);
                return;
            case PipeBugEnemyFunction.YellowArcRight:
                RunYellowPipeBugArc(slot, state, camera, movingLeft: false);
                return;
            default:
                throw new InvalidDataException(
                    $"Pipe Bug function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Wakes a hidden Brinstar bug only when Samus enters its narrow horizontal and upper vertical band.</summary>
    private static void RunBrinstarPipeBugWaiting(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        short deltaY = unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole));
        if (deltaY >= 0 || unchecked((short)(deltaY + BrinstarPipeBugTriggerTop)) < 0)
            return;

        state.BrinstarFliesLeft = (samus.Position.Whole - slot.Position.Whole).X < 0;
        if (EnemyMath.WrappedMagnitude(unchecked((ushort)(slot.Position.X.Whole - samus.Position.X.Whole))) >=
            BrinstarPipeBugTriggerWidth)
        {
            return;
        }

        state.Function = PipeBugEnemyFunction.BrinstarEmerge;
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        slot.Timer = NativeCountdown.Start(0);
        state.AnimationState = state.BrinstarFliesLeft
            ? PipeBugAnimationSelector.None
            : PipeBugAnimationSelector.FacingRight;
        SelectBrinstarPipeBugAnimation(slot, state);
    }

    /// <summary>Applies the native fractional Y decrement until the bug rises above Samus and switches to horizontal flight.</summary>
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
        if ((state.EmergenceTop - slot.Position.Whole).Y < 0 ||
            slot.Position.Y.Whole >= samus.Position.Y.Whole)
        {
            return;
        }

        state.AnimationState |= PipeBugAnimationSelector.Shooting;
        SelectBrinstarPipeBugAnimation(slot, state);
        state.Function = PipeBugEnemyFunction.BrinstarFlyHorizontally;
    }

    /// <summary>Moves two pixels per frame, then restores the stored spawn position and starts an invisible respawn wait offscreen.</summary>
    private static void RunBrinstarPipeBugFlight(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        ushort cameraX)
    {
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
            (state.BrinstarFliesLeft ? -2 : 2))));
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
        state.RespawnDelay = NativeCountdown.Start(BrinstarPipeBugRespawnFrames);
        state.Function = PipeBugEnemyFunction.BrinstarRespawnDelay;
    }

    /// <summary>Installs a strong or ordinary Brinstar instruction list only when the selected animation changes.</summary>
    internal static void SelectBrinstarPipeBugAnimation(
        RoomEnemySlot slot,
        PipeBugEnemyState state)
    {
        if (state.AnimationState == state.InstalledAnimationState)
            return;
        state.InstalledAnimationState = state.AnimationState;
        state.Program = AnimationProgramCursor<PipeBugProgram>.Start(PipeBugDefinitions.BrinstarProgram(
            strong: slot.Spawn.Population.Parameter1 != 0,
            state.AnimationState));
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Arms the Norfair leader only after all five consecutive formation members remain dormant.</summary>
    private void RunNorfairPipeBugFormationWait(RoomEnemySlot slot, PipeBugEnemyState state)
    {
        // Only the first record has a nonzero low parameter byte. It is the formation
        // controller and may arm itself only while all four following physical records are
        // still at the dormant function. The native code addresses them at +$40 increments.
        if ((SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter2)) == 0)
            return;
        RoomEnemySlot[] formation = RequireNorfairPipeBugFormation(slot);
        if (formation.All(member =>
                member.RequireSpeciesState<PipeBugEnemyState>().Function == PipeBugEnemyFunction.NorfairWaitForFormation))
        {
            state.Function = PipeBugEnemyFunction.NorfairWaitForSamus;
        }
    }

    /// <summary>Launches the five-bug rise when Samus enters the leader's trigger, retaining follower instruction timers.</summary>
    internal void RunNorfairPipeBugSamusWait(
        RoomEnemySlot leader,
        PipeBugEnemyState leaderState,
        SamusState samus)
    {
        ushort triggerWidth = unchecked((byte)leader.Spawn.Population.Parameter2);
        if (!EnemyMath.IsWithinStrictModularDistance(leader.Position.X.Whole, samus.Position.X.Whole, triggerWidth) ||
            unchecked((short)(leader.Position.Y.Whole - samus.Position.Y.Whole)) < 0)
        {
            return;
        }

        leaderState.CountFormationTick();
        // $B3:8C19-$8C1F resets only the current (leader) slot. The following
        // formation stores change every instruction pointer, but leave follower
        // timers intact, including zero timers in cleared respawn placeholders.
        leader.InstructionTimer = NativeCountdown.Start(1);
        leader.Timer = NativeCountdown.Start(0);
        bool samusIsRight = unchecked((short)(samus.Position.X.Whole - leader.Position.X.Whole)) >= 0;
        RoomEnemySlot[] formation = RequireNorfairPipeBugFormation(leader);
        foreach (RoomEnemySlot member in formation)
        {
            member.RequireSpeciesState<PipeBugEnemyState>().Program = AnimationProgramCursor<PipeBugProgram>.Start(samusIsRight
                ? PipeBugProgram.GametRisingRight
                : PipeBugProgram.GametRisingLeft);
            member.RequireSpeciesState<PipeBugEnemyState>().Function = PipeBugEnemyFunction.NorfairRise;
        }

        for (int index = 0; index < formation.Length; index++)
        {
            PipeBugEnemyState memberState = formation[index].RequireSpeciesState<PipeBugEnemyState>();
            memberState.StaggerTarget = PipeBugDefinitions.NorfairStaggerTarget(index);
            memberState.NorfairPostRiseFunction = PipeBugDefinitions.NorfairPostRiseFunction(index);
        }
    }

    /// <summary>Raises one Norfair bug until it passes Samus's Y and selects its authored post-rise function.</summary>
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
        state.RiseEnd = slot.Position.Whole;
        InstallPipeBugProgram(slot, state,
            unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0
                ? PipeBugProgram.GametRisingRight
                : PipeBugProgram.GametRisingLeft);
    }

    /// <summary>Offsets a rising Norfair member by its signed stagger target before its timed flight wait.</summary>
    private static void RunNorfairPipeBugVerticalStagger(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        short signedOffset)
    {
        state.CountFormationTick();
        // Negative offsets move upward with table entry 66; positive offsets move downward
        // with entry 64. The native routine clamps exactly at emergenceY +/- 16/32.
        AddPipeBugLinearVelocity(
            slot,
            horizontal: false,
            byteOffset: signedOffset < 0 ? 132 : 128);
        Position16<RoomPixels> target = state.RiseEnd + new Offset16<RoomPixels>(0, signedOffset);
        short distancePastTarget = (slot.Position.Whole - target).Y;
        bool reached = signedOffset < 0
            ? distancePastTarget < 0
            : distancePastTarget >= 0;
        if (!reached)
            return;
        slot.Position = slot.Position.WithY(new(target.Y, 0));
        state.Function = PipeBugEnemyFunction.NorfairWaitForStagger;
    }

    /// <summary>Waits for a member's stagger counter and then chooses left or right flight from Samus's live X.</summary>
    private static void RunNorfairPipeBugStaggerWait(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        state.CountFormationTick();
        if (!state.HasReachedStaggerTarget)
            return;

        state.FormationCounter = 0;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        if (unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)) >= 0)
        {
            state.Function = PipeBugEnemyFunction.NorfairFlyRight;
            state.Program = AnimationProgramCursor<PipeBugProgram>.Start(PipeBugProgram.GametFlyingRight);
        }
        else
        {
            state.Function = PipeBugEnemyFunction.NorfairFlyLeft;
            state.Program = AnimationProgramCursor<PipeBugProgram>.Start(PipeBugProgram.GametFlyingLeft);
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

    /// <summary>Finds the leader's four following physical slots and rejects an incomplete or unrelated formation.</summary>
    private RoomEnemySlot[] RequireNorfairPipeBugFormation(RoomEnemySlot leader)
    {
        if (leader.SlotIndex + NorfairFormationSize > RoomEnemySystem.MaximumEnemyCount)
            throw new InvalidDataException("Norfair Pipe Bug formation overruns the physical enemy pool.");
        var formation = new RoomEnemySlot[NorfairFormationSize];
        for (int index = 0; index < formation.Length; index++)
        {
            RoomEnemySlot member = _slotTable.Slots[leader.SlotIndex + index];
            PipeBugEnemyState? state = member.SpeciesState<PipeBugEnemyState>();
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

    /// <summary>Wakes a yellow bug when Samus is ahead on its authored side and within the narrow vertical band.</summary>
    private static void RunYellowPipeBugWaiting(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        short deltaX = unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole));
        bool inFront = slot.Spawn.Population.Parameter1 != 0
            ? deltaX < 0 && unchecked((short)(deltaX + YellowPipeBugTriggerDistance)) >= 0
            : deltaX >= 0 && unchecked((short)(deltaX - YellowPipeBugTriggerDistance)) < 0;
        if (!inFront ||
            !EnemyMath.IsWithinStrictModularDistance(slot.Position.Y.Whole, samus.Position.Y.Whole, YellowPipeBugTriggerHeight))
        {
            return;
        }

        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        state.EmergenceDelay = NativeCountdown.Start(YellowPipeBugEmergenceDelayFrames);
        state.Function = PipeBugEnemyFunction.YellowEmergenceDelay;
    }

    /// <summary>Counts down the visible emergence delay before installing directional straight-flight art.</summary>
    private static void RunYellowPipeBugDelay(RoomEnemySlot slot, PipeBugEnemyState state)
    {
        state.EmergenceDelay = state.EmergenceDelay.Decrement().Remaining;
        if (!state.EmergenceDelay.IsZero)
            return;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        if (slot.Spawn.Population.Parameter1 != 0)
        {
            state.Program = AnimationProgramCursor<PipeBugProgram>.Start(PipeBugProgram.GeegaFlyingLeft);
            state.Function = PipeBugEnemyFunction.YellowFlyLeft;
        }
        else
        {
            state.Program = AnimationProgramCursor<PipeBugProgram>.Start(PipeBugProgram.GeegaFlyingRight);
            state.Function = PipeBugEnemyFunction.YellowFlyRight;
        }
    }

    /// <summary>Advances yellow flight and begins its single quadratic arc after passing Samus by the native threshold.</summary>
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
        state.ArcCounter = YellowPipeBugArcCounterStart;
        state.ArcRising = true;
        InstallPipeBugProgram(slot, state,
            movingLeft
                ? PipeBugProgram.GeegaArcingLeft
                : PipeBugProgram.GeegaArcingRight);
    }

    /// <summary>Applies the two quadratic vertical phases until the bug returns to its spawn height.</summary>
    private static void RunYellowPipeBugArc(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        Position16<RoomPixels> camera,
        bool movingLeft)
    {
        if (ResetYellowPipeBugIfOffScreen(slot, state, camera))
            return;
        AddYellowPipeBugHorizontalVelocity(slot, state, movingLeft);
        if (state.ArcRising)
        {
            // The cartridge decrements and ends the rise when the record would go below zero, holding it at zero; the
            // count never goes below zero, so the end is tested before the decrement.
            if (state.ArcCounter == 0)
            {
                state.ArcRising = false;
            }
            else
            {
                state.ArcCounter--;
                AddPipeBugDisplacement(slot, horizontal: false,
                    EnemyMath.ReadQuadraticEnemySpeed(checked((ushort)state.ArcCounter), negative: false));
            }
            return;
        }

        state.ArcCounter++;
        AddPipeBugDisplacement(slot, horizontal: false,
            EnemyMath.ReadQuadraticEnemySpeed(checked((ushort)state.ArcCounter), negative: true));
        bool returnedToSpawnY = movingLeft
            ? unchecked((short)(slot.Position.Y.Whole - state.SpawnPosition.Y)) < 0
            : unchecked((short)(state.SpawnPosition.Y - slot.Position.Y.Whole)) >= 0;
        if (!returnedToSpawnY)
            return;

        state.ArcCompleted = true;
        state.ArcRising = true;
        state.Function = movingLeft
            ? PipeBugEnemyFunction.YellowFlyLeft
            : PipeBugEnemyFunction.YellowFlyRight;
        InstallPipeBugProgram(slot, state,
            movingLeft
                ? PipeBugProgram.GeegaFlyingLeft
                : PipeBugProgram.GeegaFlyingRight);
    }

    /// <summary>Hides an offscreen yellow bug, restores its spawn point, and makes its arc available again.</summary>
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
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<PipeBugProgram>.Start(slot.Spawn.Population.Parameter1 != 0
            ? PipeBugProgram.GeegaFlyingLeft
            : PipeBugProgram.GeegaFlyingRight);
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        return true;
    }

    /// <summary>Applies the separately authored positive or negative 16.16 flight speed.</summary>
    private static void AddYellowPipeBugHorizontalVelocity(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        bool movingLeft)
    {
        slot.Position = slot.Position.AdvanceX(movingLeft ? state.NegativeVelocity : state.PositiveVelocity);
    }

    /// <summary>Reads a native linear-speed entry and adds it to one axis, carrying the fraction into the whole.</summary>
    private static void AddPipeBugLinearVelocity(
        RoomEnemySlot slot,
        bool horizontal,
        int byteOffset)
    {
        SnesSignedSixteenSixteen velocity = ReadPipeBugLinearVelocity(unchecked((ushort)byteOffset));
        slot.Position = horizontal ? slot.Position.AdvanceX(velocity) : slot.Position.AdvanceY(velocity);
    }

    /// <summary>The signed 16.16 entry of the common linear speed table at <paramref name="byteOffset"/>.</summary>
    private static SnesSignedSixteenSixteen ReadPipeBugLinearVelocity(ushort byteOffset)
    {
        SnesSignedSixteenSixteen speed = EnemyMath.ReadLinearEnemySpeed(byteOffset);
        return speed;
    }

    /// <summary>Adds a wrapped signed 16.16 displacement to one position axis.</summary>
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
            slot.Position = slot.Position.WithX(SnesFixedPosition.FromRaw(unchecked(position)));
        }
        else
        {
            slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked(position)));
        }
    }

    /// <summary>Creates species state at the enemy's physical slot before its first AI pass.</summary>
    private static PipeBugEnemyState CreatePipeBugState(RoomEnemySlot slot)
    {
        var state = new PipeBugEnemyState(slot);
        slot.SetSpeciesState(state, state.ClearWithRecord);
        return state;
    }

    /// <summary>Restarts an animation at timer one and clears its animation frame timer.</summary>
    internal static void InstallPipeBugProgram(RoomEnemySlot slot, PipeBugEnemyState state, PipeBugProgram program)
    {
        state.Program = AnimationProgramCursor<PipeBugProgram>.Start(program);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Rejects a behavior callback that needs Samus but received no live player actor.</summary>
    private static void RequirePipeBugSamus([NotNull] SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Pipe Bug behavior requires the active Samus actor.");
    }

    /// <summary>Advances PipeBugs's typed animation.</summary>
    internal static void AdvancePipeBugsTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, PipeBugAnimations.Programs, PipeBugFrameDefinitions.ShownFrames.Of,
            static (PipeBugEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor);

    /// <summary>Ports Zeb's initialization AI.</summary>
    internal static void RunZebInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeBrinstarPipeBug(slot);
    }

    /// <summary>Ports Gamet's initialization AI.</summary>
    internal static void RunGametInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeNorfairPipeBug(slot);
    }

    /// <summary>Ports Geega's initialization AI.</summary>
    internal static void RunGeegaInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeYellowPipeBug(slot);
    }

    /// <summary>Ports PipeBugs's main AI.</summary>
    internal void RunPipeBugsMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunPipeBugMain(
            slot,
            slot.RequireSpeciesState<PipeBugEnemyState>(),
            frame.Samus,
            frame.Camera);
    }
}
