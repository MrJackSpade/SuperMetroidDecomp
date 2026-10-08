namespace SuperMetroid.Core.Game;

/// <summary>Same-bank dispatch words used by Zoa AI $A3:B482-$B536.</summary>
public enum ZoaEnemyFunction : ushort
{
    /// <summary>$A3:B482, Function_Zoa_WaitForSamusToGetNear: remain hidden until Samus is fewer than 128 horizontal pixels away, then select the facing rise list.</summary>
    WaitForSamus = 0xb482,
    /// <summary>$A3:B4A8, Function_Zoa_Rising: become visible and move upward 0.5 pixel per NTSC AI update while Samus is above; reaching her height selects horizontal launch.</summary>
    Rising = 0xb4a8,
    /// <summary>$A3:B4D6, Function_Zoa_Shooting: launch the enemy itself horizontally using instruction-controlled speeds; once its center leaves the camera square, hide and restore its spawn whole coordinates.</summary>
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

    /// <summary>Whole-pixel room X captured at initialization and restored by offscreen launch reset; the current X subposition is intentionally not captured or reset with it.</summary>
    public ushort SpawnXPosition
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Whole-pixel room Y captured at initialization and restored when launch ends offscreen; fractional Y accumulated during rising survives the reset.</summary>
    public ushort SpawnYPosition
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Element index zero through three in the instruction-list pointer table.</summary>
    public ZoaAnimationSelector InstructionListTableIndex
    {
        get => (ZoaAnimationSelector)_slot.VariableC;
        internal set => _slot.VariableC = (ushort)value;
    }

    /// <summary>Last installed animation selector, initially zero; $A3:B537 compares it with the requested selector so unchanged lists keep their timer and loop state.</summary>
    public ZoaAnimationSelector PreviousInstructionListTableIndex
    {
        get => (ZoaAnimationSelector)_slot.VariableD;
        internal set => _slot.VariableD = (ushort)value;
    }

    /// <summary>Same-bank AI dispatch word in slot variable F, initialized to wait and transitioned through rise, horizontal launch, and offscreen return to wait.</summary>
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

    private const int ZoaActivationColumnDistance = 0x0080;
    private const int ZoaRisingSubpixelSpeed = 0x00008000;

    private readonly ZoaEnemyState?[] _zoaStates = new ZoaEnemyState?[MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Zoa</c> at $A3:B44A.</summary>
    private void InitializeZoa(RoomEnemySlot slot)
    {
        var state = new ZoaEnemyState(slot)
        {
            Function = ZoaEnemyFunction.WaitForSamus,
            InstructionListTableIndex = ZoaAnimationSelector.None,
            PreviousInstructionListTableIndex = ZoaAnimationSelector.None,
            XSpeedTableIndex = 0,
            SpawnXPosition = slot.XPosition,
            SpawnYPosition = slot.YPosition,
        };
        _zoaStates[slot.SlotIndex] = state;

        // The initializer installs list zero directly while also setting “previous” to
        // zero. Consequently the later change detector is deliberately a no-op at rest.
        slot.CurrentInstruction = ZoaAnimationDefinitions.InstructionList(
            ZoaAnimationSelector.None);
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    /// <summary>Ports <c>MainAI_Zoa</c> and its complete three-entry indirect dispatch.</summary>
    private static void RunZoaMain(
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
                throw new InvalidDataException(
                    $"Zoa function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void RunZoaWait(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState samus)
    {
        ushort signedDistance = unchecked((ushort)(samus.XPosition - slot.XPosition));
        ushort absoluteDistance = unchecked((short)signedDistance) < 0
            ? unchecked((ushort)-signedDistance)
            : signedDistance;
        if (absoluteDistance >= ZoaActivationColumnDistance)
            return;

        // Odd indexes are rising lists: one faces left and three faces right. The sign test
        // uses the same wrapped Samus-minus-enemy word as the common bank-$A0 helper.
        state.InstructionListTableIndex = ZoaAnimationSelector.Rising |
            (unchecked((short)signedDistance) < 0
                ? ZoaAnimationSelector.None
                : ZoaAnimationSelector.FacingRight);
        SetZoaInstructionList(slot, state);
        state.Function = ZoaEnemyFunction.Rising;
    }

    private static void RunZoaRising(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState samus)
    {
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        if (unchecked((short)(samus.YPosition - slot.YPosition)) < 0)
        {
            // NTSC subtracts exactly 0.5 px/frame. The PAL regional constant is 0.625;
            // this project targets the user's Japan/USA retail ROM and reads NTSC data.
            AddZoaDisplacement(slot, xDisplacement: 0, yDisplacement: -ZoaRisingSubpixelSpeed);
            return;
        }

        state.InstructionListTableIndex &= ZoaAnimationSelector.FacingRight;
        SetZoaInstructionList(slot, state);
        slot.VariableE = 0; // Native Enemy.var5; shared instruction-loop scratch word.
        state.Function = ZoaEnemyFunction.Shooting;
    }

    private static void RunZoaShooting(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        ushort cameraX,
        ushort cameraY)
    {
        int unsignedDisplacement = ZoaSpeedDefinitions.Displacement(state.XSpeedTableIndex);

        // Shooting list zero travels left by subtraction; list two travels right by
        // addition. The instruction bytecode changes speed at 64-, 8-, and 48-frame marks.
        int xDisplacement = state.InstructionListTableIndex == ZoaAnimationSelector.None
            ? -unsignedDisplacement
            : unsignedDisplacement;
        AddZoaDisplacement(slot, xDisplacement, yDisplacement: 0);
        if (EnemyCenterIsOnScreen(slot, cameraX, cameraY))
        {
            SetZoaInstructionList(slot, state);
            return;
        }

        // Once its center leaves the inclusive 256x256 camera square, Zoa teleports to the
        // saved population coordinate and becomes invisible until the next proximity wake.
        // `$A3:B51E-$B527` stores only the whole words: the subpixel fractions left by the
        // rise and the shot carry into the next flight.
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        slot.XPosition = state.SpawnXPosition;
        slot.YPosition = state.SpawnYPosition;
        state.InstructionListTableIndex = ZoaAnimationSelector.None;
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

    private static void SetZoaInstructionList(RoomEnemySlot slot, ZoaEnemyState state)
    {
        if (state.InstructionListTableIndex == state.PreviousInstructionListTableIndex)
            return;
        state.PreviousInstructionListTableIndex = state.InstructionListTableIndex;
        slot.CurrentInstruction = ZoaAnimationDefinitions.InstructionList(
            state.InstructionListTableIndex);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private ZoaEnemyState RequireZoaState(RoomEnemySlot slot) =>
        _zoaStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Zoa state.");
}
