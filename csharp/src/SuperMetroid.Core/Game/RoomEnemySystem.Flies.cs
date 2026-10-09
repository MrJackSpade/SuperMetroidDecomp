namespace SuperMetroid.Core.Game;

/// <summary>The bank-$A2 function pointer stored in a Mellow-family enemy's variable F.</summary>
public enum FlyEnemyFunction : ushort
{
    /// <summary>Native <c>Function_Flies_IdleMovement_ClockwiseCircle</c> at <c>$A2:B14E</c>; circles with increasing table offsets, switching direction on wrap or attacking when cooldown is zero and Samus is within 112 horizontal pixels.</summary>
    ClockwiseCircle = 0xb14e,
    /// <summary>Native <c>Function_Flies_IdleMovement_AntiClockwiseCircle</c> at <c>$A2:B17C</c>; circles with decreasing table offsets and the same proximity/cooldown admission as the clockwise state.</summary>
    AntiClockwiseCircle = 0xb17c,
    /// <summary>Native <c>Function_Flies_AttackSamus</c> at <c>$A2:B1AA</c>; advances the sampled attack velocity until crossing the saved Samus Y, then reverses vertical velocity and retreats.</summary>
    AttackSamus = 0xb1aa,
    /// <summary>Native <c>Function_Flies_Retreat</c> at <c>$A2:B1D2</c>; continues with reversed Y velocity until the accumulated attack duration counts down through signed underflow, then starts a 24-update idle cooldown.</summary>
    Retreat = 0xb1d2,
}

/// <summary>
/// Typed debugger view of the six private words shared by Mellow, Mella, and Memu. Every
/// property remains backed by the common enemy slot so debugger state matches WRAM A-F.
/// </summary>
public sealed class FlyEnemyState
{
    /// <summary>Provides the live WRAM-backed enemy variables exposed by this debugger view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a debugger view over the supplied enemy's shared variable slots.</summary>
    /// <param name="slot">Enemy slot whose variables hold the fly AI state.</param>
    internal FlyEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native <c>Flies.retreatTimer</c> in variable A at <c>$0FA8,x</c>; counts up per attack update, down through signed underflow during retreat, and then down from 24 to gate another idle attack.</summary>
    public ushort RetreatTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Raw signed 8.8 horizontal attack/retreat velocity in variable B at <c>$0FAA,x</c>, in room pixels per actor update; sampled from Samus's attack-start angle and retained during retreat.</summary>
    public ushort XVelocity
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Raw signed 8.8 vertical attack/retreat velocity in variable C at <c>$0FAC,x</c>, in room pixels per actor update; its sign reverses when the saved target Y is crossed.</summary>
    public ushort YVelocity
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Samus's whole room-pixel Y coordinate captured when the attack begins, in variable D at <c>$0FAE,x</c>; determines the turnaround rather than tracking Samus's later movement.</summary>
    public ushort TargetYPosition
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>
    /// Even byte offset into the 256-entry sine/cosine table. The idle routines retain
    /// the native nine-bit range because they add/subtract $20 before masking with $1FF.
    /// </summary>
    public ushort Angle
    {
        get => _slot.VariableE;
        internal set => _slot.VariableE = value;
    }

    /// <summary>Current indirect bank-$A2 AI address, native <c>Flies.function</c> in variable F at <c>$0FB2,x</c>; reads and writes the owning enemy slot directly.</summary>
    public FlyEnemyFunction Function
    {
        get => (FlyEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }
}

/// <summary>Shared Mellow/Mella/Memu AI translated literally from $A2:B06B-$B1E7.</summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Enemy-definition pointer for the Mellow fly variant.</summary>
    internal const ushort MellowDefinition = 0xd0ff;
    /// <summary>Enemy-definition pointer for the Mella fly variant.</summary>
    internal const ushort MellaDefinition = 0xd13f;
    /// <summary>Enemy-definition pointer for the Memu fly variant.</summary>
    internal const ushort MemuDefinition = 0xd17f;

    /// <summary>Horizontal proximity threshold, in room pixels, that admits a fly attack.</summary>
    private const int FlyAttackHorizontalRange = 0x70;
    /// <summary>Per-slot views of initialized fly AI variables, absent until a fly is initialized.</summary>
    private readonly FlyEnemyState?[] _flyStates = new FlyEnemyState?[MaximumEnemyCount];

    /// <summary>Initializes a fly's idle circle state and selects its flight instruction program.</summary>
    /// <param name="slot">Enemy slot to initialize.</param>
    private void InitializeFly(RoomEnemySlot slot)
    {
        var state = new FlyEnemyState(slot)
        {
            Angle = 0,
            Function = FlyEnemyFunction.ClockwiseCircle,
        };
        _flyStates[slot.SlotIndex] = state;
        slot.CurrentInstruction = FlyInstructionProgramDefinitions.Flight;
        slot.SpritemapPointer = 0x804d;
        slot.InstructionTimer = 1;
    }

    /// <summary>Advances one fly AI update, including its RNG side effect and current movement state.</summary>
    /// <param name="slot">Enemy whose state and position are advanced.</param>
    /// <param name="samus">Active actor used for idle attack admission; required for a fly update.</param>
    private void RunFlyMain(RoomEnemySlot slot, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Fly AI requires the active Samus actor.");

        // $A2:B11F advances the global RNG even though this family does not consume the
        // returned word. The side effect matters when several random enemies share a room.
        _nextRandom!();
        FlyEnemyState state = RequireFlyState(slot);
        switch (state.Function)
        {
            case FlyEnemyFunction.ClockwiseCircle:
                RunFlyCircle(slot, state, samus, clockwise: true);
                return;
            case FlyEnemyFunction.AntiClockwiseCircle:
                RunFlyCircle(slot, state, samus, clockwise: false);
                return;
            case FlyEnemyFunction.AttackSamus:
                RunFlyAttack(slot, state);
                return;
            case FlyEnemyFunction.Retreat:
                MoveFlyAccordingToVelocities(slot, state);
                state.RetreatTimer = unchecked((ushort)(state.RetreatTimer - 1));
                if ((short)state.RetreatTimer < 0)
                {
                    state.RetreatTimer = 24;
                    state.Function = FlyEnemyFunction.ClockwiseCircle;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Fly function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Moves a fly around its idle circle and switches direction when the angle wraps.</summary>
    /// <param name="slot">Enemy slot whose position is updated.</param>
    /// <param name="state">Persistent angle and cooldown variables for the fly.</param>
    /// <param name="samus">Actor whose horizontal proximity can trigger an attack.</param>
    /// <param name="clockwise">Whether this update advances the circle angle clockwise.</param>
    private static void RunFlyCircle(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus,
        bool clockwise)
    {
        if (state.RetreatTimer != 0)
        {
            state.RetreatTimer = unchecked((ushort)(state.RetreatTimer - 1));
        }
        else if (Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition))) <
            FlyAttackHorizontalRange)
        {
            SetFlyToAttackSamus(slot, state, samus);
            return;
        }

        MoveFlyAccordingToAngle(slot, state);
        state.Angle = unchecked((ushort)((state.Angle + (clockwise ? 0x20 : -0x20)) & 0x01ff));
        if (state.Angle == 0)
        {
            state.Function = clockwise
                ? FlyEnemyFunction.AntiClockwiseCircle
                : FlyEnemyFunction.ClockwiseCircle;
        }
    }

    /// <summary>Captures Samus's current target position and derives the fly's attack velocity.</summary>
    /// <param name="slot">Attacking fly's current position and variable storage.</param>
    /// <param name="state">State that receives velocity, target Y, and attack mode.</param>
    /// <param name="samus">Actor position sampled to aim the attack.</param>
    private static void SetFlyToAttackSamus(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus)
    {
        byte angle = CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - slot.XPosition)),
            unchecked((short)(samus.YPosition - slot.YPosition)));
        state.XVelocity = unchecked((ushort)(ReadSignedSineCosine(angle + 64) * 2));
        state.YVelocity = unchecked((ushort)(ReadSignedSineCosine(angle) * 4));
        state.TargetYPosition = samus.YPosition;
        state.Function = FlyEnemyFunction.AttackSamus;
    }

    /// <summary>Moves the fly along its sampled attack vector and reverses it after crossing target Y.</summary>
    /// <param name="slot">Enemy slot whose position is advanced.</param>
    /// <param name="state">Attack vector, target Y, and elapsed attack timer.</param>
    private static void RunFlyAttack(RoomEnemySlot slot, FlyEnemyState state)
    {
        MoveFlyAccordingToVelocities(slot, state);
        state.RetreatTimer = unchecked((ushort)(state.RetreatTimer + 1));

        bool crossedTarget = (short)state.YVelocity < 0
            ? slot.YPosition < state.TargetYPosition
            : slot.YPosition >= state.TargetYPosition;
        if (!crossedTarget)
            return;

        state.YVelocity = unchecked((ushort)-state.YVelocity);
        state.Function = FlyEnemyFunction.Retreat;
    }

    /// <summary>Applies one sine/cosine table step to the fly's room position and subpixel offsets.</summary>
    /// <param name="slot">Enemy slot whose position is advanced.</param>
    /// <param name="state">Current angle used to select the table entries.</param>
    private static void MoveFlyAccordingToAngle(RoomEnemySlot slot, FlyEnemyState state)
    {
        int tableIndex = state.Angle >> 1;
        (slot.XPosition, slot.XSubposition) = AddEightBitVelocity(
            slot.XPosition,
            slot.XSubposition,
            unchecked((ushort)ReadSignedSineCosine(tableIndex + 64)));
        (slot.YPosition, slot.YSubposition) = AddEightBitVelocity(
            slot.YPosition,
            slot.YSubposition,
            unchecked((ushort)ReadSignedSineCosine(tableIndex)));
    }

    /// <summary>Applies the stored signed 8.8 attack or retreat velocities to the fly's position.</summary>
    /// <param name="slot">Enemy slot whose position and subpixel offsets are advanced.</param>
    /// <param name="state">Horizontal and vertical velocities to apply.</param>
    private static void MoveFlyAccordingToVelocities(RoomEnemySlot slot, FlyEnemyState state)
    {
        (slot.XPosition, slot.XSubposition) = AddEightBitVelocity(
            slot.XPosition,
            slot.XSubposition,
            state.XVelocity);
        (slot.YPosition, slot.YSubposition) = AddEightBitVelocity(
            slot.YPosition,
            slot.YSubposition,
            state.YVelocity);
    }

    /// <summary>Reads one signed word from the shared negative-cosine/sine lookup table.</summary>
    /// <param name="index">Word index, including any phase offset required by the caller.</param>
    /// <returns>The signed table value used as a movement component.</returns>
    private static short ReadSignedSineCosine(int index)
    {
        // The backing table begins with negative cosine at index zero, sine at index 64,
        // and continues far enough for every byte angle plus the cosine phase offset.
        return EnemyTrigonometryTables.SignedNegativeCosineWord(index);
    }

    /// <summary>Returns initialized per-slot fly state or fails when initialization was skipped.</summary>
    /// <param name="slot">Enemy slot whose fly state is required.</param>
    /// <returns>The state view backed by that slot's variables.</returns>
    private FlyEnemyState RequireFlyState(RoomEnemySlot slot) =>
        _flyStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized fly state.");
}
