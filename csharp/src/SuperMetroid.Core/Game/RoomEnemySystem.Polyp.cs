namespace SuperMetroid.Core.Game;

/// <summary>
/// Native function pointers stored in Polyp's <c>$0FA8,x</c> word. These are sequential AI
/// states, not combinable flags, so retaining their cartridge addresses is the most useful
/// debugger representation.
/// </summary>
public enum PolypEnemyFunction : ushort
{
    WaitingForSamus = 0xb596,
    ShootingRock = 0xb5b2,
    Cooldown = 0xb5ea,
}

/// <summary>Typed view of Polyp's function and cooldown words.</summary>
public sealed class PolypEnemyState
{
    private readonly RoomEnemySlot _slot;

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
    internal const ushort PolypDefinition = 0xd1ff;

    private const ushort PolypInstructionList = 0xb51a;
    private const int PolypCooldownTable = 0xa2b520;
    private const int PolypInitialYSpeedTable = 0xa2b530;
    private const int PolypXVelocityTable = 0xa2b550;
    private const ushort PolypProximity = 0x0040;
    private const ushort PolypRandomSeed = 0x0011;

    private readonly PolypEnemyState?[] _polypStates =
        new PolypEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical Polyp-capable enemy slot.</summary>
    public IReadOnlyList<PolypEnemyState?> PolypStates => _polypStates;

    /// <summary>Ports <c>InitAI_Polyp</c> at $A2:B570.</summary>
    private void InitializePolyp(RoomEnemySlot slot)
    {
        Action<ushort> setRandomNumber = _setRandomNumber ?? throw new InvalidOperationException(
            "Polyp initialization requires the writable cartridge RNG seam.");

        slot.CurrentInstruction = PolypInstructionList;
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
                state.CooldownTimer = unchecked((ushort)(state.CooldownTimer - 1));
                if (unchecked((short)state.CooldownTimer) < 0)
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
        int xVelocityOffset = _nextRandom!() & 0x001e;
        ushort xVelocity = ReadWord(_bus!, PolypXVelocityTable + xVelocityOffset);

        int ySpeedOffset = _nextRandom!() & 0x001e;
        ushort initialYSpeed = ReadWord(_bus!, PolypInitialYSpeedTable + ySpeedOffset);
        SpawnPolypRock(slot, initialYSpeed, xVelocity);

        state.Function = PolypEnemyFunction.Cooldown;
        int cooldownOffset = _nextRandom!() & 0x000e;
        state.CooldownTimer = ReadWord(_bus!, PolypCooldownTable + cooldownOffset);
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

    private PolypEnemyState RequirePolypState(RoomEnemySlot slot) =>
        _polypStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Polyp state.");
}
