namespace SuperMetroid.Core.Game;

/// <summary>Same-bank dispatch words used by Zoa AI $A3:B482-$B536.</summary>
public enum ZoaEnemyFunction : ushort
{
    WaitForSamus = 0xb482,
    Rising = 0xb4a8,
    Shooting = 0xb4d6,
}

/// <summary>
/// Named view of Zoa's common slot words and its one parallel $7E:7800 speed-table word.
/// “Shooting” is the ROM's name for the creature launching itself horizontally; it does not
/// create a separate enemy projectile.
/// </summary>
public sealed class ZoaEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal ZoaEnemyState(RoomEnemySlot slot) => _slot = slot;

    public ushort SpawnXPosition
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    public ushort SpawnYPosition
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Element index zero through three in the instruction-list pointer table.</summary>
    public ushort InstructionListTableIndex
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    public ushort PreviousInstructionListTableIndex
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ZoaEnemyFunction Function
    {
        get => (ZoaEnemyFunction)_slot.VariableF;
        internal set => _slot.VariableF = (ushort)value;
    }

    /// <summary>Byte offset zero, four, eight, or twelve into the five-row X-speed table.</summary>
    public ushort XSpeedTableIndex { get; internal set; }
}

/// <summary>Literal translation of Zoa enemy AI $A3:B3C1-$B556.</summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort ZoaDefinition = 0xda7f;

    private const ushort ZoaFacingLeftShootingInstructionList = 0xb3c1;
    private const int ZoaInstructionListPointerTable = 0xa3b40d;
    private const int ZoaXSpeedTable = 0xa3b415;
    private const int ZoaActivationColumnDistance = 0x0080;
    private const int ZoaRisingSubpixelSpeed = 0x00008000;

    private readonly ZoaEnemyState?[] _zoaStates = new ZoaEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical slot currently owned by a Zoa.</summary>
    public IReadOnlyList<ZoaEnemyState?> ZoaStates => _zoaStates;

    /// <summary>Ports <c>InitAI_Zoa</c> at $A3:B44A.</summary>
    private void InitializeZoa(RoomEnemySlot slot)
    {
        var state = new ZoaEnemyState(slot)
        {
            Function = ZoaEnemyFunction.WaitForSamus,
            InstructionListTableIndex = 0,
            PreviousInstructionListTableIndex = 0,
            XSpeedTableIndex = 0,
            SpawnXPosition = slot.XPosition,
            SpawnYPosition = slot.YPosition,
        };
        _zoaStates[slot.SlotIndex] = state;

        // The initializer installs list zero directly while also setting “previous” to
        // zero. Consequently the later change detector is deliberately a no-op at rest.
        slot.CurrentInstruction = ZoaFacingLeftShootingInstructionList;
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    /// <summary>Ports <c>MainAI_Zoa</c> and its complete three-entry indirect dispatch.</summary>
    private void RunZoaMain(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState? samus,
        ushort cameraX,
        ushort cameraY)
    {
        if (samus is null)
            throw new InvalidOperationException("Zoa AI requires the active Samus actor.");

        switch (state.Function)
        {
            case ZoaEnemyFunction.WaitForSamus:
                RunZoaWait(slot, state, samus);
                return;
            case ZoaEnemyFunction.Rising:
                RunZoaRising(slot, state, samus);
                return;
            case ZoaEnemyFunction.Shooting:
                RunZoaShooting(slot, state, cameraX, cameraY);
                return;
            default:
                throw new NotSupportedException(
                    $"Zoa function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private void RunZoaWait(RoomEnemySlot slot, ZoaEnemyState state, SamusState samus)
    {
        ushort signedDistance = unchecked((ushort)(samus.XPosition - slot.XPosition));
        ushort absoluteDistance = unchecked((short)signedDistance) < 0
            ? unchecked((ushort)-signedDistance)
            : signedDistance;
        if (absoluteDistance >= ZoaActivationColumnDistance)
            return;

        // Odd indexes are rising lists: one faces left and three faces right. The sign test
        // uses the same wrapped Samus-minus-enemy word as the common bank-$A0 helper.
        state.InstructionListTableIndex = unchecked((short)signedDistance) < 0
            ? (ushort)1
            : (ushort)3;
        SetZoaInstructionList(slot, state);
        state.Function = ZoaEnemyFunction.Rising;
    }

    private void RunZoaRising(RoomEnemySlot slot, ZoaEnemyState state, SamusState samus)
    {
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        if (unchecked((short)(samus.YPosition - slot.YPosition)) < 0)
        {
            // NTSC subtracts exactly 0.5 px/frame. The PAL regional constant is 0.625;
            // this project targets the user's Japan/USA retail ROM and reads NTSC data.
            AddZoaDisplacement(slot, xDisplacement: 0, yDisplacement: -ZoaRisingSubpixelSpeed);
            return;
        }

        state.InstructionListTableIndex = unchecked((ushort)(
            state.InstructionListTableIndex - 1));
        SetZoaInstructionList(slot, state);
        slot.VariableE = 0; // Native Enemy.var5; shared instruction-loop scratch word.
        state.Function = ZoaEnemyFunction.Shooting;
    }

    private void RunZoaShooting(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        int tableAddress = ZoaXSpeedTable + state.XSpeedTableIndex;
        ushort whole = ReadWord(_bus!, tableAddress);
        ushort fraction = ReadWord(_bus!, tableAddress + 2);
        int unsignedDisplacement = unchecked((whole << 16) | fraction);

        // Shooting list zero travels left by subtraction; list two travels right by
        // addition. The instruction bytecode changes speed at 64-, 8-, and 48-frame marks.
        int xDisplacement = state.InstructionListTableIndex == 0
            ? -unsignedDisplacement
            : unsignedDisplacement;
        AddZoaDisplacement(slot, xDisplacement, yDisplacement: 0);
        if (ZoaCenterIsOnScreen(slot, cameraX, cameraY))
        {
            SetZoaInstructionList(slot, state);
            return;
        }

        // Once its center leaves the inclusive 256x256 camera square, Zoa teleports to the
        // saved population coordinate and becomes invisible until the next proximity wake.
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        slot.XPosition = state.SpawnXPosition;
        slot.YPosition = state.SpawnYPosition;
        slot.XSubposition = 0;
        slot.YSubposition = 0;
        state.InstructionListTableIndex = 0;
        SetZoaInstructionList(slot, state);
        state.Function = ZoaEnemyFunction.WaitForSamus;
    }

    private static void AddZoaDisplacement(
        RoomEnemySlot slot,
        int xDisplacement,
        int yDisplacement)
    {
        if (xDisplacement != 0)
        {
            uint x = ((uint)slot.XPosition << 16) | slot.XSubposition;
            x = unchecked(x + (uint)xDisplacement);
            slot.XPosition = unchecked((ushort)(x >> 16));
            slot.XSubposition = unchecked((ushort)x);
        }
        if (yDisplacement != 0)
        {
            uint y = ((uint)slot.YPosition << 16) | slot.YSubposition;
            y = unchecked(y + (uint)yDisplacement);
            slot.YPosition = unchecked((ushort)(y >> 16));
            slot.YSubposition = unchecked((ushort)y);
        }
    }

    private static bool ZoaCenterIsOnScreen(
        RoomEnemySlot slot,
        ushort cameraX,
        ushort cameraY) =>
        !IsNegative16(slot.XPosition - cameraX) &&
        !IsNegative16(cameraX + 0x0100 - slot.XPosition) &&
        !IsNegative16(slot.YPosition - cameraY) &&
        !IsNegative16(cameraY + 0x0100 - slot.YPosition);

    private void SetZoaInstructionList(RoomEnemySlot slot, ZoaEnemyState state)
    {
        if (state.InstructionListTableIndex == state.PreviousInstructionListTableIndex)
            return;
        if (state.InstructionListTableIndex > 3)
        {
            throw new InvalidDataException(
                $"Zoa instruction-list index {state.InstructionListTableIndex} exceeds its four-entry table.");
        }

        state.PreviousInstructionListTableIndex = state.InstructionListTableIndex;
        slot.CurrentInstruction = ReadWord(
            _bus!,
            ZoaInstructionListPointerTable + state.InstructionListTableIndex * 2);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private ZoaEnemyState RequireZoaState(RoomEnemySlot slot) =>
        _zoaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Zoa state.");
}
