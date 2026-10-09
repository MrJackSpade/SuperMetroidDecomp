namespace SuperMetroid.Core.Game;

/// <summary>
/// Native function pointers stored in Polyp's <c>$0FA8,x</c> word. These are sequential AI
/// states, not combinable flags, so retaining their cartridge addresses is the most useful
/// debugger representation.
/// </summary>
public enum PolypEnemyFunction : ushort
{
    /// <summary>Function_Polyp_WaitForSamusToGetNear, $A2:B596: tests strict modular distances below 64 whole pixels on both axes, selecting the shoot function without spawning until the next AI update.</summary>
    WaitingForSamus = 0xb596,
    /// <summary>Function_Polyp_ShootRock, $A2:B5B2: consumes separate shared-RNG samples for horizontal velocity, initial quadratic Y-speed selector, and cooldown; requests one bank-$86 lava rock and enters cooldown even if the projectile pool is full.</summary>
    ShootingRock = 0xb5b2,
    /// <summary>Function_Polyp_Cooldown, $A2:B5EA: decrements the native word once per AI update and rearms proximity checking only when the signed result is negative, so an initial nonnegative count N waits N+1 updates.</summary>
    Cooldown = 0xb5ea,
}

/// <summary>Typed view of Polyp's function and cooldown words.</summary>
public sealed class PolypEnemyState
{
    /// <summary>The room-owned enemy slot whose variable words back this typed Polyp state view.</summary>
    private readonly RoomEnemySlot _slot;

    /// <summary>Creates a state view over the enemy slot initialized for Polyp behavior.</summary>
    /// <param name="slot">The room enemy slot that stores the function pointer and cooldown words.</param>
    internal PolypEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Native <c>$0FA8,x</c> indirect main-AI pointer.</summary>
    public PolypEnemyFunction Function
    {
        get => (PolypEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Native <c>$0FAA,x</c>, decremented through $FFFF before rearming.</summary>
    public ushort CooldownTimer
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }
}

/// <summary>
/// Literal actor-half translation of retail Polyp <c>$A0:D1FF</c>. The visible four-pixel
/// vent is an indestructible, non-contact body; when Samus enters its strict 64-by-64 range,
/// three independent RNG samples choose one bank-$86 lava rock and the following cooldown.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>$A0:D1FF, the native instruction-list definition used to initialize a Polyp actor.</summary>
    internal const ushort PolypDefinition = 0xd1ff;

    /// <summary>$0040, the strict per-axis distance threshold for switching from waiting to the rock-throw action.</summary>
    private const ushort PolypProximity = 0x0040;
    /// <summary>$0011, the shared cartridge RNG seed written by each Polyp initializer.</summary>
    private const ushort PolypRandomSeed = 0x0011;

    /// <summary>Typed Polyp state views indexed by room enemy slot; entries are populated when Polyp actors initialize.</summary>
    private readonly PolypEnemyState?[] _polypStates =
        new PolypEnemyState?[MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Polyp</c> at $A2:B570.</summary>
    private void InitializePolyp(RoomEnemySlot slot)
    {
        Action<ushort> setRandomNumber = _setRandomNumber ?? throw new InvalidOperationException(
            "Polyp initialization requires the writable cartridge RNG seam.");

        slot.CurrentInstruction = PolypInstructionProgramDefinitions.Stationary;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
        _polypStates[slot.SlotIndex] = new PolypEnemyState(slot)
        {
            Function = PolypEnemyFunction.WaitingForSamus,
            CooldownTimer = 0,
        };

        // Every Polyp initializer writes $0011 to the shared seed. In Volcano the fourth
        // actor therefore leaves this exact value for the first gameplay frame; treating it
        // as actor-local randomness would produce a different launch sequence.
        setRandomNumber(PolypRandomSeed);
    }

    /// <summary>Ports <c>MainAI_Polyp</c> and its three indirect functions at $A2:B58F.</summary>
    private void RunPolypMain(
        RoomEnemySlot slot,
        PolypEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case PolypEnemyFunction.WaitingForSamus:
                if (samus is null)
                {
                    throw new InvalidOperationException(
                        "Polyp proximity AI requires the active Samus actor.");
                }

                if (IsWithinPolypAxis(samus.XPosition, slot.XPosition) &&
                    IsWithinPolypAxis(samus.YPosition, slot.YPosition))
                {
                    // Native AI only installs the shoot function here. The rock is emitted
                    // on the next enemy frame, a one-frame tell preserved deliberately.
                    state.Function = PolypEnemyFunction.ShootingRock;
                }
                return;

            case PolypEnemyFunction.ShootingRock:
                ShootPolypRock(slot, state);
                return;

            case PolypEnemyFunction.Cooldown:
                NativeWordCounterStep timer = NativeWordCounter.Decrement(state.CooldownTimer);
                state.CooldownTimer = timer.Value;
                if (timer.IsNegative)
                    state.Function = PolypEnemyFunction.WaitingForSamus;
                return;

            default:
                throw new InvalidDataException(
                    $"Polyp function $A2:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the three independent random-table selections at $A2:B5B2.</summary>
    private void ShootPolypRock(RoomEnemySlot slot, PolypEnemyState state)
    {
        ushort xVelocity = PolypLaunchDefinitions.XVelocity(_nextRandom!());

        ushort initialYSpeed = PolypLaunchDefinitions.InitialYIndex(_nextRandom!());
        SpawnPolypRock(slot, initialYSpeed, xVelocity);

        state.Function = PolypEnemyFunction.Cooldown;
        state.CooldownTimer = PolypLaunchDefinitions.Cooldown(_nextRandom!());
    }

    /// <summary>
    /// Recreates <c>GetSignedYMinusX</c> followed by the signed threshold comparison used by
    /// both bank-$A0 proximity helpers. Equality at 64 pixels is outside; the modular $8000
    /// difference also remains outside without passing through host <c>Math.Abs</c> overflow.
    /// </summary>
    private static bool IsWithinPolypAxis(ushort samusCoordinate, ushort enemyCoordinate)
    {
        ushort difference = unchecked((ushort)(samusCoordinate - enemyCoordinate));
        ushort magnitude = (difference & 0x8000) != 0
            ? unchecked((ushort)-difference)
            : difference;
        return unchecked((short)(magnitude - PolypProximity)) < 0;
    }

    /// <summary>Gets the initialized Polyp state associated with a room enemy slot.</summary>
    /// <param name="slot">The enemy slot whose Polyp state is required.</param>
    /// <returns>The state view created during Polyp initialization.</returns>
    /// <exception cref="InvalidOperationException">The slot has not been initialized as a Polyp.</exception>
    private PolypEnemyState RequirePolypState(RoomEnemySlot slot) =>
        _polypStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Polyp state.");
}
