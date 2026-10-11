namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$B3 function words used by all three Pipe Bug state machines. These are
/// addresses, not a host-designed progression enum: retaining the ROM values makes the
/// debugger state directly comparable with enemy WRAM and makes an unknown branch fail
/// explicitly instead of silently selecting a plausible-looking behavior.
/// </summary>
public enum PipeBugEnemyFunction : ushort
{
    /// <summary>$B3:8880, waits until a Brinstar Pipe Bug's origin is on screen.</summary>
    BrinstarWaitUntilOnScreen = 0x8880,
    /// <summary>$B3:8890, waits for Samus to enter the Brinstar emergence region.</summary>
    BrinstarWaitForSamus = 0x8890,
    /// <summary>$B3:88E3, raises a Brinstar Pipe Bug out of its pipe.</summary>
    BrinstarEmerge = 0x88e3,
    /// <summary>$B3:891C, flies a Brinstar Pipe Bug horizontally until it leaves the screen.</summary>
    BrinstarFlyHorizontally = 0x891c,
    /// <summary>$B3:897E, holds a hidden Brinstar Pipe Bug before rearming it.</summary>
    BrinstarRespawnDelay = 0x897e,

    /// <summary>$B3:8BCD, waits for the five-member Norfair formation to become ready.</summary>
    NorfairWaitForFormation = 0x8bcd,
    /// <summary>$B3:8BFF, waits for Samus to trigger the Norfair formation.</summary>
    NorfairWaitForSamus = 0x8bff,
    /// <summary>$B3:8CA6, raises one Norfair formation member toward Samus.</summary>
    NorfairRise = 0x8ca6,
    /// <summary>$B3:8CFF, advances the formation leader's stagger counter.</summary>
    NorfairLeaderStagger = 0x8cff,
    /// <summary>$B3:8D0C, moves the near upper formation member to its stagger height.</summary>
    NorfairUpperNearStagger = 0x8d0c,
    /// <summary>$B3:8D4E, moves the far upper formation member to its stagger height.</summary>
    NorfairUpperFarStagger = 0x8d4e,
    /// <summary>$B3:8D90, moves the near lower formation member to its stagger height.</summary>
    NorfairLowerNearStagger = 0x8d90,
    /// <summary>$B3:8DD2, moves the far lower formation member to its stagger height.</summary>
    NorfairLowerFarStagger = 0x8dd2,
    /// <summary>$B3:8E14, flies a released Norfair formation member left.</summary>
    NorfairFlyLeft = 0x8e14,
    /// <summary>$B3:8E35, flies a released Norfair formation member right.</summary>
    NorfairFlyRight = 0x8e35,
    /// <summary>$B3:8E5A, waits for one Norfair member's stagger target.</summary>
    NorfairWaitForStagger = 0x8e5a,

    /// <summary>$B3:8FB5, waits for Samus to enter a yellow Pipe Bug's trigger region.</summary>
    YellowWaitForSamus = 0x8fb5,
    /// <summary>$B3:8FF5, delays before a yellow Pipe Bug begins straight flight.</summary>
    YellowEmergenceDelay = 0x8ff5,
    /// <summary>$B3:9028, flies a yellow Pipe Bug left.</summary>
    YellowFlyLeft = 0x9028,
    /// <summary>$B3:90BD, flies a yellow Pipe Bug right.</summary>
    YellowFlyRight = 0x90bd,
    /// <summary>$B3:915A, performs the leftward yellow Pipe Bug arc.</summary>
    YellowArcLeft = 0x915a,
    /// <summary>$B3:91D8, performs the rightward yellow Pipe Bug arc.</summary>
    YellowArcRight = 0x91d8,
}

/// <summary>
/// Debugger-visible projection of Pipe Bug's native variables. Words A-F remain backed by
/// the physical 64-byte enemy record. The remaining named properties model the extended
/// $7E:7800 enemy words used by this family; several meanings intentionally overlap because
/// the three ROM initializers reuse the same storage for unrelated state machines.
/// </summary>
public sealed class PipeBugEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal PipeBugEnemyState(RoomEnemySlot slot)
    {
        _slot = slot;
        DefinitionAtInitialization = slot.EnemyDefinitionPointer;
    }

    /// <summary>
    /// Header which created this physical state projection. Generic death clears the live
    /// definition word on the following scheduler scan, but native formation code continues
    /// to address the record's variables through fixed <c>+$40</c> aliases. Species-aware
    /// typed views must therefore use this immutable owner, not the disposable live word.
    /// </summary>
    public EnemyDefinitionId DefinitionAtInitialization { get; }

    /// <summary>Native variable A for Norfair/yellow bugs, or direction for Brinstar bugs.</summary>
    public ushort VariableA
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Native variable F for Brinstar bugs; variable A for the other two species.</summary>
    public PipeBugEnemyFunction Function
    {
        get => (PipeBugEnemyFunction)(IsBrinstar ? _slot.VariableF : _slot.VariableA);
        internal set
        {
            if (IsBrinstar)
                _slot.VariableF = (ushort)value;
            else
                _slot.VariableA = (ushort)value;
        }
    }

    /// <summary>Gets whether the initialized owner is either Brinstar Pipe Bug variant.</summary>
    public bool IsBrinstar =>
        DefinitionAtInitialization is
            EnemyDefinitionId.Zeb or
            EnemyDefinitionId.Zebbo;

    // The names below follow their role in the currently selected species. They are kept
    // together because the cartridge overlays all of them in one Enemy_PipeBug structure.
    /// <summary>Gets the horizontal spawn position restored after leaving the screen.</summary>
    public ushort SpawnX { get; internal set; }
    /// <summary>Gets the vertical spawn position restored after leaving the screen.</summary>
    public ushort SpawnY { get; internal set; }
    /// <summary>Gets the upper Y boundary of a Brinstar Pipe Bug's emergence.</summary>
    public ushort EmergenceTopY { get; internal set; }
    /// <summary>Gets the requested Brinstar animation selector bits.</summary>
    public PipeBugAnimationSelector AnimationState { get; internal set; }
    /// <summary>Gets the Brinstar animation selector currently installed in the enemy slot.</summary>
    public PipeBugAnimationSelector InstalledAnimationState { get; internal set; }
    /// <summary>Gets the species-specific respawn delay or formation stagger counter.</summary>
    public ushort DelayOrCounter { get; internal set; }
    /// <summary>Gets the byte offset of this Norfair bug's positive linear-speed record.</summary>
    public ushort LinearSpeedTableOffset { get; internal set; }
    /// <summary>Gets the Norfair member's Y position at the end of its initial rise.</summary>
    public ushort EmergenceY { get; internal set; }
    /// <summary>Gets the formation counter value at which this Norfair member departs.</summary>
    public ushort StaggerTarget { get; internal set; }
    /// <summary>Gets the function selected for this Norfair member after rising.</summary>
    public PipeBugEnemyFunction NorfairPostRiseFunction { get; internal set; }
    /// <summary>Gets the integer word of a yellow Pipe Bug's positive 16.16 velocity.</summary>
    public ushort PositiveVelocityWhole { get; internal set; }
    /// <summary>Gets the fractional word of a yellow Pipe Bug's positive 16.16 velocity.</summary>
    public ushort PositiveVelocityFraction { get; internal set; }
    /// <summary>Gets the integer word of a yellow Pipe Bug's negative 16.16 velocity.</summary>
    public ushort NegativeVelocityWhole { get; internal set; }
    /// <summary>Gets the fractional word of a yellow Pipe Bug's negative 16.16 velocity.</summary>
    public ushort NegativeVelocityFraction { get; internal set; }
    /// <summary>Gets the yellow Pipe Bug's countdown before straight flight begins.</summary>
    public ushort EmergenceDelay { get; internal set; }
    /// <summary>Gets the quadratic-speed table counter used by a yellow Pipe Bug's arc.</summary>
    public ushort ArcCounter { get; internal set; }
    /// <summary>Gets whether the yellow Pipe Bug arc is in its rising or returning phase.</summary>
    public ushort ArcPhase { get; internal set; }
    /// <summary>Gets whether a yellow Pipe Bug has completed its one authored arc.</summary>
    public bool ArcCompleted { get; internal set; }
    /// <summary>Gets the horizontal position at which the current yellow arc began.</summary>
    public ushort ArcStartX { get; internal set; }
}

/// <summary>
/// Complete translation of the four retail Pipe Bug headers: normal and strong Brinstar
/// bugs, the synchronized five-bug Norfair formation, and yellow Brinstar bugs. The family
/// has no enemy projectile routine. Its attack is the ROM-authored emergence/flight path;
/// contact damage, beam damage, freeze, power bombs, grapple cancel, and death therefore
/// remain owned by the common enemy handlers selected by each untouched header.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort BrinstarPipeBugEmergenceHeight = 16;
    private const ushort BrinstarPipeBugTriggerWidth = 64;
    private const ushort BrinstarPipeBugTriggerTop = 96;
    private const ushort BrinstarPipeBugRespawnFrames = 48;

    private const int NorfairFormationSize = 5;

    private const ushort YellowPipeBugTriggerDistance = 192;
    private const ushort YellowPipeBugTriggerHeight = 48;
    private const ushort YellowPipeBugEmergenceDelayFrames = 24;
    private const ushort YellowPipeBugArcTriggerDistance = 48;
    private const ushort YellowPipeBugArcCounterStart = 40;

    private readonly PipeBugEnemyState?[] _pipeBugStates =
        new PipeBugEnemyState?[MaximumEnemyCount];

    private static bool IsBrinstarPipeBugDefinition(EnemyDefinitionId definition) =>
        definition is EnemyDefinitionId.Zeb or
            EnemyDefinitionId.Zebbo;

    private void ResetPipeBugRoomState() => Array.Clear(_pipeBugStates);

    /// <summary>Ports <c>BrinstarPipeBug_Init</c> at $B3:883B.</summary>
    private void InitializeBrinstarPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnX = slot.Position.X.Whole;
        state.SpawnY = slot.Position.Y.Whole;
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
    private void InitializeNorfairPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnX = slot.Position.X.Whole;
        state.SpawnY = slot.Position.Y.Whole;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.LinearSpeedTableOffset = unchecked((ushort)((slot.Parameter2 >> 8) * 8));
        state.DelayOrCounter = 0;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = NorfairPipeBugInstructionProgramDefinitions.RisingLeft;
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
    }

    /// <summary>Ports <c>BrinstarYellowPipeBug_Init</c> at $B3:8F4C.</summary>
    private void InitializeYellowPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnX = slot.Position.X.Whole;
        state.SpawnY = slot.Position.Y.Whole;
        slot.Position = slot.Position.WithFractionX(0).WithFractionY(0);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = slot.Parameter1 != 0
            ? YellowPipeBugInstructionProgramDefinitions.FlyingLeft
            : YellowPipeBugInstructionProgramDefinitions.FlyingRight;

        // Parameter two is a logical entry index. Each common linear-speed record is eight
        // bytes: positive fraction/whole followed by its separately authored negative pair.
        ushort speedOffset = unchecked((ushort)(slot.Parameter2 * 8));
        (short positiveWhole, ushort positiveFraction) = ReadLinearEnemySpeed(speedOffset);
        (short negativeWhole, ushort negativeFraction) =
            ReadLinearEnemySpeed(unchecked((ushort)(speedOffset + 4)));
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
                if (!EnemyWithNormalSpritesIsOffScreen(slot, camera))
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
        if (WrappedMagnitude(unchecked((ushort)(slot.Position.X.Whole - samus.Position.X.Whole))) >=
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

    private static void RunBrinstarPipeBugFlight(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        ushort cameraX)
    {
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
            ((state.VariableA & 0x8000) != 0 ? -2 : 2))));
        // $B3:8949 tests only the horizontal screen extent.
        if (!EnemyIsHorizontallyOffScreen(slot, cameraX))
            return;

        slot.Position = slot.Position.WithWholeX(state.SpawnX).WithFractionX(0).WithWholeY(state.SpawnY);
        // This apparently strange assignment is literal $B3:8958 behavior: the original
        // stores the integer spawn Y into both halves rather than clearing the fraction.
        slot.Position = slot.Position.WithFractionY(state.SpawnY);
        state.AnimationState = PipeBugAnimationSelector.None;
        SelectBrinstarPipeBugAnimation(slot, state);
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.DelayOrCounter = BrinstarPipeBugRespawnFrames;
        state.Function = PipeBugEnemyFunction.BrinstarRespawnDelay;
    }

    private static void SelectBrinstarPipeBugAnimation(
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

    private void RunNorfairPipeBugFormationWait(RoomEnemySlot slot, PipeBugEnemyState state)
    {
        // Only the first record has a nonzero low parameter byte. It is the formation
        // controller and may arm itself only while all four following physical records are
        // still at the dormant function. The native code addresses them at +$40 increments.
        if ((slot.Parameter2 & 0x00ff) == 0)
            return;
        RoomEnemySlot[] formation = RequireNorfairPipeBugFormation(slot);
        if (formation.All(member =>
                RequirePipeBugState(member).Function == PipeBugEnemyFunction.NorfairWaitForFormation))
        {
            state.Function = PipeBugEnemyFunction.NorfairWaitForSamus;
        }
    }

    private void RunNorfairPipeBugSamusWait(
        RoomEnemySlot leader,
        PipeBugEnemyState leaderState,
        SamusState samus)
    {
        ushort triggerWidth = unchecked((byte)leader.Parameter2);
        if (!IsWithinStrictModularDistance(leader.Position.X.Whole, samus.Position.X.Whole, triggerWidth) ||
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
        if (EnemyCenterIsOnScreen(slot, camera))
            return;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
        slot.Position = slot.Position.WithWhole(new(state.SpawnX, state.SpawnY));
    }

    private RoomEnemySlot[] RequireNorfairPipeBugFormation(RoomEnemySlot leader)
    {
        if (leader.SlotIndex + NorfairFormationSize > MaximumEnemyCount)
            throw new InvalidDataException("Norfair Pipe Bug formation overruns the physical enemy pool.");
        var formation = new RoomEnemySlot[NorfairFormationSize];
        for (int index = 0; index < formation.Length; index++)
        {
            RoomEnemySlot member = _slots[leader.SlotIndex + index];
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
            !IsWithinStrictModularDistance(slot.Position.Y.Whole, samus.Position.Y.Whole, YellowPipeBugTriggerHeight))
        {
            return;
        }

        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        state.EmergenceDelay = YellowPipeBugEmergenceDelayFrames;
        state.Function = PipeBugEnemyFunction.YellowEmergenceDelay;
    }

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
                    ReadQuadraticEnemySpeed(state.ArcCounter, negative: false));
            }
            return;
        }

        state.ArcCounter = unchecked((ushort)(state.ArcCounter + 1));
        AddPipeBugDisplacement(slot, horizontal: false,
            ReadQuadraticEnemySpeed(state.ArcCounter, negative: true));
        bool returnedToSpawnY = movingLeft
            ? unchecked((short)(slot.Position.Y.Whole - state.SpawnY)) < 0
            : unchecked((short)(state.SpawnY - slot.Position.Y.Whole)) >= 0;
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

    private static bool ResetYellowPipeBugIfOffScreen(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        Position16<RoomPixels> camera)
    {
        // Every Geega flight function resets through CheckIfEnemyCenterIsOnScreen.
        if (EnemyCenterIsOnScreen(slot, camera))
            return false;
        slot.Position = FixedPosition<RoomPixels>.FromWhole(new(state.SpawnX, state.SpawnY));
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

    private static void AddPipeBugLinearVelocity(
        RoomEnemySlot slot,
        bool horizontal,
        int byteOffset)
    {
        (short whole, ushort fraction) = ReadLinearEnemySpeed(unchecked((ushort)byteOffset));
        AddPipeBugFixedVelocity(slot, horizontal, unchecked((ushort)whole), fraction);
    }

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

    private PipeBugEnemyState CreatePipeBugState(RoomEnemySlot slot)
    {
        var state = new PipeBugEnemyState(slot);
        _pipeBugStates[slot.SlotIndex] = state;
        return state;
    }

    private PipeBugEnemyState RequirePipeBugState(RoomEnemySlot slot) =>
        _pipeBugStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Pipe Bug state.");

    private static void InstallPipeBugInstruction(RoomEnemySlot slot, ushort instruction)
    {
        slot.CurrentInstruction = instruction;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private static void RequirePipeBugSamus(SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Pipe Bug behavior requires the active Samus actor.");
    }
}
