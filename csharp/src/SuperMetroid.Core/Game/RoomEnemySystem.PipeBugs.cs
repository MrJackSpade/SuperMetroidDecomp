namespace SuperMetroid.Core.Game;

/// <summary>
/// Literal bank-$B3 function words used by all three Pipe Bug state machines. These are
/// addresses, not a host-designed progression enum: retaining the ROM values makes the
/// debugger state directly comparable with enemy WRAM and makes an unknown branch fail
/// explicitly instead of silently selecting a plausible-looking behavior.
/// </summary>
public enum PipeBugEnemyFunction : ushort
{
    BrinstarWaitUntilOnScreen = 0x8880,
    BrinstarWaitForSamus = 0x8890,
    BrinstarEmerge = 0x88e3,
    BrinstarFlyHorizontally = 0x891c,
    BrinstarRespawnDelay = 0x897e,

    NorfairWaitForFormation = 0x8bcd,
    NorfairWaitForSamus = 0x8bff,
    NorfairRise = 0x8ca6,
    NorfairLeaderStagger = 0x8cff,
    NorfairUpperNearStagger = 0x8d0c,
    NorfairUpperFarStagger = 0x8d4e,
    NorfairLowerNearStagger = 0x8d90,
    NorfairLowerFarStagger = 0x8dd2,
    NorfairFlyLeft = 0x8e14,
    NorfairFlyRight = 0x8e35,
    NorfairWaitForStagger = 0x8e5a,

    YellowWaitForSamus = 0x8fb5,
    YellowEmergenceDelay = 0x8ff5,
    YellowFlyLeft = 0x9028,
    YellowFlyRight = 0x90bd,
    YellowArcLeft = 0x915a,
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
    public ushort DefinitionAtInitialization { get; }

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

    public bool IsBrinstar =>
        DefinitionAtInitialization is
            RoomEnemySystem.BrinstarPipeBugDefinition or
            RoomEnemySystem.StrongBrinstarPipeBugDefinition;

    public bool IsNorfair =>
        DefinitionAtInitialization == RoomEnemySystem.NorfairPipeBugDefinition;

    public bool IsYellow =>
        DefinitionAtInitialization == RoomEnemySystem.YellowPipeBugDefinition;

    // The names below follow their role in the currently selected species. They are kept
    // together because the cartridge overlays all of them in one Enemy_PipeBug structure.
    public ushort SpawnX { get; internal set; }
    public ushort SpawnY { get; internal set; }
    public ushort EmergenceTopY { get; internal set; }
    public ushort AnimationState { get; internal set; }
    public ushort InstalledAnimationState { get; internal set; }
    public ushort DelayOrCounter { get; internal set; }
    public ushort LinearSpeedTableOffset { get; internal set; }
    public ushort EmergenceY { get; internal set; }
    public ushort StaggerTarget { get; internal set; }
    public PipeBugEnemyFunction NorfairPostRiseFunction { get; internal set; }
    public ushort PositiveVelocityWhole { get; internal set; }
    public ushort PositiveVelocityFraction { get; internal set; }
    public ushort NegativeVelocityWhole { get; internal set; }
    public ushort NegativeVelocityFraction { get; internal set; }
    public ushort EmergenceDelay { get; internal set; }
    public ushort ArcCounter { get; internal set; }
    public ushort ArcPhase { get; internal set; }
    public bool ArcCompleted { get; internal set; }
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
    internal const ushort BrinstarPipeBugDefinition = 0xf193;
    internal const ushort StrongBrinstarPipeBugDefinition = 0xf1d3;
    internal const ushort NorfairPipeBugDefinition = 0xf213;
    internal const ushort YellowPipeBugDefinition = 0xf253;

    private const int BrinstarPipeBugNormalInstructionTable = 0xb3882b;
    private const int BrinstarPipeBugStrongInstructionTable = 0xb38833;
    private const ushort BrinstarPipeBugNormalInitialInstruction = 0x87ab;
    private const ushort BrinstarPipeBugStrongInitialInstruction = 0x8a1d;
    private const ushort BrinstarPipeBugEmergenceHeight = 16;
    private const ushort BrinstarPipeBugTriggerWidth = 64;
    private const ushort BrinstarPipeBugTriggerTop = 96;
    private const ushort BrinstarPipeBugRespawnFrames = 48;

    private const ushort NorfairPipeBugLeftRiseInstruction = 0x8ae1;
    private const ushort NorfairPipeBugRightRiseInstruction = 0x8b21;
    private const ushort NorfairPipeBugFlyLeftInstruction = 0x8b05;
    private const ushort NorfairPipeBugFlyRightInstruction = 0x8b45;
    private const int NorfairFormationSize = 5;

    private const ushort YellowPipeBugLeftInstruction = 0x8efc;
    private const ushort YellowPipeBugLeftArcInstruction = 0x8f10;
    private const ushort YellowPipeBugRightInstruction = 0x8f24;
    private const ushort YellowPipeBugRightArcInstruction = 0x8f38;
    private const ushort YellowPipeBugTriggerDistance = 192;
    private const ushort YellowPipeBugTriggerHeight = 48;
    private const ushort YellowPipeBugEmergenceDelayFrames = 24;
    private const ushort YellowPipeBugArcTriggerDistance = 48;
    private const ushort YellowPipeBugArcCounterStart = 40;

    private readonly PipeBugEnemyState?[] _pipeBugStates =
        new PipeBugEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical Pipe Bug enemy slot.</summary>
    public IReadOnlyList<PipeBugEnemyState?> PipeBugStates => _pipeBugStates;

    internal static bool IsPipeBugDefinition(ushort definition) =>
        definition is
            BrinstarPipeBugDefinition or
            StrongBrinstarPipeBugDefinition or
            NorfairPipeBugDefinition or
            YellowPipeBugDefinition;

    private static bool IsBrinstarPipeBugDefinition(ushort definition) =>
        definition is BrinstarPipeBugDefinition or StrongBrinstarPipeBugDefinition;

    private void ResetPipeBugRoomState() => Array.Clear(_pipeBugStates);

    /// <summary>Ports <c>BrinstarPipeBug_Init</c> at $B3:883B.</summary>
    private void InitializeBrinstarPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnX = slot.XPosition;
        state.SpawnY = slot.YPosition;
        state.EmergenceTopY = unchecked((ushort)(slot.YPosition - BrinstarPipeBugEmergenceHeight));
        state.Function = PipeBugEnemyFunction.BrinstarWaitUntilOnScreen;
        state.DelayOrCounter = BrinstarPipeBugRespawnFrames;
        state.AnimationState = 0;
        state.InstalledAnimationState = 0;
        slot.CurrentInstruction = slot.Parameter1 != 0
            ? BrinstarPipeBugStrongInitialInstruction
            : BrinstarPipeBugNormalInitialInstruction;
    }

    /// <summary>Ports <c>NorfairPipeBug_Init</c> at $B3:8B61.</summary>
    private void InitializeNorfairPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnX = slot.XPosition;
        state.SpawnY = slot.YPosition;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.LinearSpeedTableOffset = unchecked((ushort)((slot.Parameter2 >> 8) * 8));
        state.DelayOrCounter = 0;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = NorfairPipeBugLeftRiseInstruction;
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
    }

    /// <summary>Ports <c>BrinstarYellowPipeBug_Init</c> at $B3:8F4C.</summary>
    private void InitializeYellowPipeBug(RoomEnemySlot slot)
    {
        PipeBugEnemyState state = CreatePipeBugState(slot);
        state.SpawnX = slot.XPosition;
        state.SpawnY = slot.YPosition;
        slot.XSubposition = 0;
        slot.YSubposition = 0;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = slot.Parameter1 != 0
            ? YellowPipeBugLeftInstruction
            : YellowPipeBugRightInstruction;

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
        ushort cameraX,
        ushort cameraY)
    {
        switch (state.Function)
        {
            case PipeBugEnemyFunction.BrinstarWaitUntilOnScreen:
                // The poorly named native CheckIfEnemyIsOnScreen returns zero when the
                // origin is visible. $B3:8880 branches on that zero result, so this initial
                // state arms the proximity check on-screen, not after a despawn boundary.
                if (!EnemyWithNormalSpritesIsOffScreen(slot, cameraX, cameraY))
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
                RunBrinstarPipeBugFlight(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.BrinstarRespawnDelay:
                if (state.DelayOrCounter-- == 1)
                    state.Function = PipeBugEnemyFunction.BrinstarWaitForSamus;
                return;

            case PipeBugEnemyFunction.NorfairWaitForFormation:
                RunNorfairPipeBugFormationWait(slot, state);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairWaitForSamus:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugSamusWait(slot, state, samus!);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairRise:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugRise(slot, state, samus!);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairLeaderStagger:
                state.DelayOrCounter = unchecked((ushort)(state.DelayOrCounter + 1));
                state.Function = PipeBugEnemyFunction.NorfairWaitForStagger;
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairUpperNearStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, -16);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairUpperFarStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, -32);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairLowerNearStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, 16);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairLowerFarStagger:
                RunNorfairPipeBugVerticalStagger(slot, state, 32);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairWaitForStagger:
                RequirePipeBugSamus(samus);
                RunNorfairPipeBugStaggerWait(slot, state, samus!);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairFlyLeft:
                AddPipeBugLinearVelocity(slot, horizontal: true, state.LinearSpeedTableOffset + 4);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
                return;
            case PipeBugEnemyFunction.NorfairFlyRight:
                AddPipeBugLinearVelocity(slot, horizontal: true, state.LinearSpeedTableOffset);
                ResetNorfairPipeBugIfOffScreen(slot, state, cameraX, cameraY);
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
                RunYellowPipeBugStraightFlight(slot, state, samus!, cameraX, cameraY, movingLeft: true);
                return;
            case PipeBugEnemyFunction.YellowFlyRight:
                RequirePipeBugSamus(samus);
                RunYellowPipeBugStraightFlight(slot, state, samus!, cameraX, cameraY, movingLeft: false);
                return;
            case PipeBugEnemyFunction.YellowArcLeft:
                RunYellowPipeBugArc(slot, state, cameraX, cameraY, movingLeft: true);
                return;
            case PipeBugEnemyFunction.YellowArcRight:
                RunYellowPipeBugArc(slot, state, cameraX, cameraY, movingLeft: false);
                return;
            default:
                throw new InvalidDataException(
                    $"Pipe Bug function $B3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunBrinstarPipeBugWaiting(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        short deltaY = unchecked((short)(samus.YPosition - slot.YPosition));
        if (deltaY >= 0 || unchecked((short)(deltaY + BrinstarPipeBugTriggerTop)) < 0)
            return;

        ushort deltaX = unchecked((ushort)(samus.XPosition - slot.XPosition));
        state.VariableA = (ushort)(state.VariableA & 0x7fff | (deltaX & 0x8000));
        if (WrappedMagnitude(unchecked((ushort)(slot.XPosition - samus.XPosition))) >=
            BrinstarPipeBugTriggerWidth)
        {
            return;
        }

        state.Function = PipeBugEnemyFunction.BrinstarEmerge;
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        slot.Timer = 0;
        state.AnimationState = (state.VariableA & 0x8000) != 0 ? (ushort)0 : (ushort)2;
        SelectBrinstarPipeBugAnimation(slot, state);
    }

    private void RunBrinstarPipeBugEmergence(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        // $B3:88E3 decrements the subposition as a separate word, then subtracts either one
        // or two pixels according to the old word. This is deliberately not normal 16.16
        // subtraction; preserving the oddity prevents a one-pixel/frame timing drift.
        bool oldSubpositionWasNonzero = slot.YSubposition-- != 0;
        slot.YPosition = unchecked((ushort)(
            slot.YPosition + (oldSubpositionWasNonzero ? -1 : -2)));
        if (unchecked((short)(state.EmergenceTopY - slot.YPosition)) < 0 ||
            slot.YPosition >= samus.YPosition)
        {
            return;
        }

        state.AnimationState |= 1;
        SelectBrinstarPipeBugAnimation(slot, state);
        state.Function = PipeBugEnemyFunction.BrinstarFlyHorizontally;
    }

    private void RunBrinstarPipeBugFlight(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        slot.XPosition = unchecked((ushort)(slot.XPosition +
            ((state.VariableA & 0x8000) != 0 ? -2 : 2)));
        if (!EnemyWithNormalSpritesIsOffScreen(slot, cameraX, cameraY))
            return;

        slot.XPosition = state.SpawnX;
        slot.XSubposition = 0;
        slot.YPosition = state.SpawnY;
        // This apparently strange assignment is literal $B3:8958 behavior: the original
        // stores the integer spawn Y into both halves rather than clearing the fraction.
        slot.YSubposition = state.SpawnY;
        state.AnimationState = 0;
        SelectBrinstarPipeBugAnimation(slot, state);
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.DelayOrCounter = BrinstarPipeBugRespawnFrames;
        state.Function = PipeBugEnemyFunction.BrinstarRespawnDelay;
    }

    private void SelectBrinstarPipeBugAnimation(RoomEnemySlot slot, PipeBugEnemyState state)
    {
        if (state.AnimationState == state.InstalledAnimationState)
            return;
        state.InstalledAnimationState = state.AnimationState;
        int table = slot.Parameter1 != 0
            ? BrinstarPipeBugStrongInstructionTable
            : BrinstarPipeBugNormalInstructionTable;
        slot.CurrentInstruction = ReadWord(_bus!, table + state.AnimationState * 2);
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
        if (!IsWithinStrictModularDistance(leader.XPosition, samus.XPosition, triggerWidth) ||
            unchecked((short)(leader.YPosition - samus.YPosition)) < 0)
        {
            return;
        }

        leaderState.DelayOrCounter = unchecked((ushort)(leaderState.DelayOrCounter + 1));
        bool samusIsRight = unchecked((short)(samus.XPosition - leader.XPosition)) >= 0;
        RoomEnemySlot[] formation = RequireNorfairPipeBugFormation(leader);
        foreach (RoomEnemySlot member in formation)
        {
            InstallPipeBugInstruction(
                member,
                samusIsRight ? NorfairPipeBugRightRiseInstruction : NorfairPipeBugLeftRiseInstruction);
            RequirePipeBugState(member).Function = PipeBugEnemyFunction.NorfairRise;
        }

        ushort[] staggerTargets = [104, 96, 88, 112, 120];
        PipeBugEnemyFunction[] staggerFunctions =
        [
            PipeBugEnemyFunction.NorfairLeaderStagger,
            PipeBugEnemyFunction.NorfairUpperNearStagger,
            PipeBugEnemyFunction.NorfairUpperFarStagger,
            PipeBugEnemyFunction.NorfairLowerNearStagger,
            PipeBugEnemyFunction.NorfairLowerFarStagger,
        ];
        for (int index = 0; index < formation.Length; index++)
        {
            PipeBugEnemyState memberState = RequirePipeBugState(formation[index]);
            memberState.StaggerTarget = staggerTargets[index];
            memberState.NorfairPostRiseFunction = staggerFunctions[index];
        }
    }

    private void RunNorfairPipeBugRise(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        AddPipeBugLinearVelocity(slot, horizontal: false, byteOffset: 132);
        if (unchecked((short)(slot.YPosition - samus.YPosition)) >= 0)
            return;

        state.Function = state.NorfairPostRiseFunction;
        state.EmergenceY = slot.YPosition;
        InstallPipeBugInstruction(
            slot,
            unchecked((short)(samus.XPosition - slot.XPosition)) >= 0
                ? NorfairPipeBugRightRiseInstruction
                : NorfairPipeBugLeftRiseInstruction);
    }

    private void RunNorfairPipeBugVerticalStagger(
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
            ? unchecked((short)(slot.YPosition - target)) < 0
            : unchecked((short)(slot.YPosition - target)) >= 0;
        if (!reached)
            return;
        slot.YPosition = target;
        slot.YSubposition = 0;
        state.Function = PipeBugEnemyFunction.NorfairWaitForStagger;
    }

    private void RunNorfairPipeBugStaggerWait(
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
        if (unchecked((short)(samus.XPosition - slot.XPosition)) >= 0)
        {
            state.Function = PipeBugEnemyFunction.NorfairFlyRight;
            slot.CurrentInstruction = NorfairPipeBugFlyRightInstruction;
        }
        else
        {
            state.Function = PipeBugEnemyFunction.NorfairFlyLeft;
            slot.CurrentInstruction = NorfairPipeBugFlyLeftInstruction;
        }
    }

    private void ResetNorfairPipeBugIfOffScreen(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        if (!EnemyWithNormalSpritesIsOffScreen(slot, cameraX, cameraY))
            return;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        state.Function = PipeBugEnemyFunction.NorfairWaitForFormation;
        slot.XPosition = state.SpawnX;
        slot.YPosition = state.SpawnY;
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
            if (state?.DefinitionAtInitialization != NorfairPipeBugDefinition)
            {
                throw new InvalidDataException(
                    $"Norfair Pipe Bug formation slot {leader.SlotIndex + index} is " +
                    $"owned by initialized enemy ${state?.DefinitionAtInitialization ?? 0:X4}, " +
                    $"expected ${NorfairPipeBugDefinition:X4}.");
            }
            // `$B3:8BCD/$8BFF/$8C52` never re-check the live definition. They read and
            // write five consecutive 64-byte records even after generic death has changed
            // one member's definition to zero. Retaining the initialized typed owner here
            // preserves that raw alias without accepting an unrelated never-Pipe-Bug slot.
            formation[index] = member;
        }
        return formation;
    }

    private void RunYellowPipeBugWaiting(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus)
    {
        short deltaX = unchecked((short)(samus.XPosition - slot.XPosition));
        bool inFront = slot.Parameter1 != 0
            ? deltaX < 0 && unchecked((short)(deltaX + YellowPipeBugTriggerDistance)) >= 0
            : deltaX >= 0 && unchecked((short)(deltaX - YellowPipeBugTriggerDistance)) < 0;
        if (!inFront ||
            !IsWithinStrictModularDistance(slot.YPosition, samus.YPosition, YellowPipeBugTriggerHeight))
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
            slot.CurrentInstruction = YellowPipeBugLeftInstruction;
            state.Function = PipeBugEnemyFunction.YellowFlyLeft;
        }
        else
        {
            slot.CurrentInstruction = YellowPipeBugRightInstruction;
            state.Function = PipeBugEnemyFunction.YellowFlyRight;
        }
    }

    private void RunYellowPipeBugStraightFlight(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        SamusState samus,
        ushort cameraX,
        ushort cameraY,
        bool movingLeft)
    {
        AddYellowPipeBugHorizontalVelocity(slot, state, movingLeft);
        if (ResetYellowPipeBugIfOffScreen(slot, state, cameraX, cameraY))
            return;
        if (state.ArcCompleted)
            return;

        bool passedSamus = movingLeft
            ? unchecked((short)(slot.XPosition - samus.XPosition - YellowPipeBugArcTriggerDistance)) < 0
            : unchecked((short)(samus.XPosition - slot.XPosition - YellowPipeBugArcTriggerDistance)) < 0;
        if (!passedSamus)
            return;

        state.Function = movingLeft
            ? PipeBugEnemyFunction.YellowArcLeft
            : PipeBugEnemyFunction.YellowArcRight;
        state.DelayOrCounter = 0;
        state.ArcCounter = YellowPipeBugArcCounterStart;
        state.ArcPhase = 1;
        state.ArcStartX = slot.XPosition;
        InstallPipeBugInstruction(
            slot,
            movingLeft ? YellowPipeBugLeftArcInstruction : YellowPipeBugRightArcInstruction);
    }

    private void RunYellowPipeBugArc(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        ushort cameraX,
        ushort cameraY,
        bool movingLeft)
    {
        if (ResetYellowPipeBugIfOffScreen(slot, state, cameraX, cameraY))
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
            ? unchecked((short)(slot.YPosition - state.SpawnY)) < 0
            : unchecked((short)(state.SpawnY - slot.YPosition)) >= 0;
        if (!returnedToSpawnY)
            return;

        state.ArcCompleted = true;
        state.ArcPhase = 1;
        state.Function = movingLeft
            ? PipeBugEnemyFunction.YellowFlyLeft
            : PipeBugEnemyFunction.YellowFlyRight;
        InstallPipeBugInstruction(
            slot,
            movingLeft ? YellowPipeBugLeftInstruction : YellowPipeBugRightInstruction);
    }

    private bool ResetYellowPipeBugIfOffScreen(
        RoomEnemySlot slot,
        PipeBugEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        if (!EnemyWithNormalSpritesIsOffScreen(slot, cameraX, cameraY))
            return false;
        slot.XPosition = state.SpawnX;
        slot.YPosition = state.SpawnY;
        slot.XSubposition = 0;
        slot.YSubposition = 0;
        state.Function = PipeBugEnemyFunction.YellowWaitForSamus;
        state.ArcCompleted = false;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        slot.CurrentInstruction = slot.Parameter1 != 0
            ? YellowPipeBugLeftInstruction
            : YellowPipeBugRightInstruction;
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

    private void AddPipeBugLinearVelocity(
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
        ushort position = horizontal ? slot.XPosition : slot.YPosition;
        ushort subposition = horizontal ? slot.XSubposition : slot.YSubposition;
        uint sum = (uint)subposition + fraction;
        position = unchecked((ushort)(position + whole + (sum > ushort.MaxValue ? 1 : 0)));
        subposition = unchecked((ushort)sum);
        if (horizontal)
        {
            slot.XPosition = position;
            slot.XSubposition = subposition;
        }
        else
        {
            slot.YPosition = position;
            slot.YSubposition = subposition;
        }
    }

    private static void AddPipeBugDisplacement(
        RoomEnemySlot slot,
        bool horizontal,
        int displacement)
    {
        uint position = horizontal
            ? ((uint)slot.XPosition << 16) | slot.XSubposition
            : ((uint)slot.YPosition << 16) | slot.YSubposition;
        position = unchecked(position + (uint)displacement);
        if (horizontal)
        {
            slot.XPosition = unchecked((ushort)(position >> 16));
            slot.XSubposition = unchecked((ushort)position);
        }
        else
        {
            slot.YPosition = unchecked((ushort)(position >> 16));
            slot.YSubposition = unchecked((ushort)position);
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
