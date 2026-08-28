using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Named view of Waver's seven private words at <c>$0FA8-$0FB4</c>. Keeping the words in
/// their cartridge roles makes instruction-list changes and fixed-point motion inspectable
/// without leaking generic <c>VariableA</c> names into the translated AI.
/// </summary>
public sealed class WaverEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal WaverEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Low 16 bits of the signed horizontal 16.16 displacement.</summary>
    public ushort XSubvelocity
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>High 16 bits of the signed horizontal 16.16 displacement.</summary>
    public short XVelocity
    {
        get => unchecked((short)_slot.VariableB);
        internal set => _slot.VariableB = unchecked((ushort)value);
    }

    /// <summary>The instruction-list index currently installed in the common enemy slot.</summary>
    public ushort CurrentInstructionListIndex
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Byte angle advanced by two per unobstructed frame.</summary>
    public ushort Angle
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Set by instruction <c>$A3:86E3</c> after the four-frame spin completes.</summary>
    public bool SpinFinished
    {
        get => _slot.VariableE != 0;
        internal set => _slot.VariableE = value ? (ushort)1 : (ushort)0;
    }

    /// <summary>
    /// Requested list: bit zero selects facing, bit one selects the temporary spin family.
    /// </summary>
    public ushort RequestedInstructionListIndex
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }
}

/// <summary>Literal translation of Waver enemy AI <c>$A3:8687-$881D</c>.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort WaverDefinition = 0xd63f;

    private const int WaverInstructionListPointers = 0xa386db;
    private const int WaverEightBitSineTable = 0xa0b143;
    private const int WaverHorizontalSpeedFixed = 0x00018000;
    private const int WaverVerticalRadius = 4;

    private readonly WaverEnemyState?[] _waverStates =
        new WaverEnemyState?[MaximumEnemyCount];

    /// <summary>Typed Waver state for all 32 physical enemy slots.</summary>
    public IReadOnlyList<WaverEnemyState?> WaverStates => _waverStates;

    /// <summary>Ports <c>InitAI_Waver</c> at <c>$A3:86ED</c>.</summary>
    private void InitializeWaver(RoomEnemySlot slot)
    {
        bool facingRight = (slot.Parameter1 & 1) != 0;
        int signedSpeed = facingRight
            ? WaverHorizontalSpeedFixed
            : -WaverHorizontalSpeedFixed;
        var state = new WaverEnemyState(slot)
        {
            XSubvelocity = unchecked((ushort)signedSpeed),
            XVelocity = unchecked((short)(signedSpeed >> 16)),
            CurrentInstructionListIndex = 0,
            Angle = 0,
            SpinFinished = false,
            RequestedInstructionListIndex = unchecked((ushort)(slot.Parameter1 & 1)),
        };
        _waverStates[slot.SlotIndex] = state;

        // The native initializer first installs the left steady list and then invokes the
        // change detector. Parameter bit zero therefore changes only right-facing spawns;
        // left-facing spawns intentionally observe current==requested and keep this list.
        slot.CurrentInstruction = 0x86a7;
        SetWaverInstructionList(slot, state);
    }

    /// <summary>Ports <c>MainAI_Waver</c> at <c>$A3:874C</c>.</summary>
    private void RunWaverMain(
        RoomEnemySlot slot,
        WaverEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Waver AI requires active room collision data.");

        int horizontalDisplacement =
            (state.XVelocity << 16) | state.XSubvelocity;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            // `$A3:875F-$8781` negates the overlapping velocity bytes and writes them back
            // as two words. For every initialized Waver this is exactly a signed 16.16
            // negation; expressing that value directly preserves the native ±1.5 px/frame.
            horizontalDisplacement = unchecked(-horizontalDisplacement);
            state.XSubvelocity = unchecked((ushort)horizontalDisplacement);
            state.XVelocity = unchecked((short)(horizontalDisplacement >> 16));
            state.RequestedInstructionListIndex = unchecked((ushort)(
                (state.RequestedInstructionListIndex ^ 1) & 1));
            SetWaverInstructionList(slot, state);
        }
        else
        {
            int verticalPixels = ReadWaverNegativeSinePixels(state.Angle);
            if (MoveEnemyVertically(level, slot, verticalPixels << 16))
            {
                // Vertical collision reflects the sine phase by half a turn. The explicit
                // byte mask matters when a debugger has written high garbage into var3.
                state.Angle = unchecked((ushort)((state.Angle + 0x80) & 0xff));
            }
            else
            {
                state.Angle = unchecked((ushort)(state.Angle + 2));
            }
        }

        // Once per half-wave, angle low seven bits equal $38. Waver temporarily swaps to
        // the four-map spin list without changing the facing bit.
        if ((state.Angle & 0x7f) == 0x38)
        {
            state.RequestedInstructionListIndex = unchecked((ushort)(
                state.RequestedInstructionListIndex | 2));
            SetWaverInstructionList(slot, state);
        }

        // The list's $86:E3 command sets this flag after 32 animation frames. Main AI owns
        // the return to steady art, matching the command/AI seam rather than timing it here.
        if (state.SpinFinished)
        {
            state.SpinFinished = false;
            state.RequestedInstructionListIndex = unchecked((ushort)(
                state.RequestedInstructionListIndex & 1));
            SetWaverInstructionList(slot, state);
        }
    }

    /// <summary>
    /// Replays the integer word returned by <c>EightBitNegativeSineMultiplication</c> with
    /// radius four. Waver deliberately discards that routine's fractional result before
    /// calling vertical movement, so the flight path advances in whole-pixel steps.
    /// </summary>
    private int ReadWaverNegativeSinePixels(ushort angle)
    {
        int byteAngle = angle & 0xff;
        int magnitude = _bus!.ReadByte(WaverEightBitSineTable + (byteAngle & 0x7f));
        int pixels = magnitude * WaverVerticalRadius >> 8;
        return byteAngle < 0x80 ? -pixels : pixels;
    }

    private void SetWaverInstructionList(RoomEnemySlot slot, WaverEnemyState state)
    {
        ushort requested = state.RequestedInstructionListIndex;
        if (requested == state.CurrentInstructionListIndex)
            return;
        if (requested >= 4)
        {
            throw new InvalidDataException(
                $"Waver instruction-list index {requested} exceeds its four-entry table.");
        }

        state.CurrentInstructionListIndex = requested;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            WaverInstructionListPointers + requested * 2);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private WaverEnemyState RequireWaverState(RoomEnemySlot slot) =>
        _waverStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Waver state.");
}
