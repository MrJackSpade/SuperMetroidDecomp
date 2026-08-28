using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Same-bank function pointers used by the Sidehopper/Dessgeega state machine. The apparent
/// duplicates at $AD20 and $AD44 are real ROM entry points: the direction-selection setup
/// chooses a different pointer even though both entries currently dispatch identical motion.
/// </summary>
public enum HopperEnemyFunction : ushort
{
    ChooseHopSize = 0xabd6,
    PrepareSmallHop = 0xabe6,
    PrepareBigHop = 0xac13,
    ChooseDirectionUpsideUp = 0xac40,
    ChooseDirectionUpsideDown = 0xac56,
    StartBackwardHopUpsideUp = 0xac6c,
    StartForwardHopUpsideUp = 0xac8f,
    StartBackwardHopUpsideDown = 0xaca8,
    StartForwardHopUpsideDown = 0xaccb,
    Landed = 0xace4,
    JumpingUpsideUpBackward = 0xad0e,
    JumpingUpsideUpForward = 0xad20,
    JumpingUpsideDownBackward = 0xad32,
    JumpingUpsideDownForward = 0xad44,
    WaitToHop = 0xad56,
}

/// <summary>
/// Typed projection of the four common enemy words at $0FAA-$0FB0 and the seven parallel
/// extension words at $7E:7800-$780C used by hopper AI. These names intentionally retain
/// “speed table index” where the cartridge does: replacing them with host gravity fields
/// would hide the buggy quadratic table that defines the retail jump arc.
/// </summary>
public sealed class HopperEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal HopperEnemyState(RoomEnemySlot slot) => _slot = slot;

    public HopperEnemyFunction Function
    {
        get => (HopperEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    public ushort YSpeedTableIndex
    {
        get => _slot.VariableB;
        internal set => _slot.VariableB = value;
    }

    /// <summary>Signed whole-pixel horizontal velocity; the fractional word is always zero.</summary>
    public short XVelocity
    {
        get => unchecked((short)_slot.VariableC);
        internal set => _slot.VariableC = unchecked((ushort)value);
    }

    public ushort YSpeedTableIndexDelta
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    public ushort InstalledInstructionList { get; internal set; }
    public ushort SmallHopInitialYSpeedTableIndex { get; internal set; }
    public ushort BigHopInitialYSpeedTableIndex { get; internal set; }
    public bool Falling { get; internal set; }
    public bool ReadyToHop { get; internal set; }

    /// <summary>Byte offset zero for Sidehopper physics, two for every other definition.</summary>
    public ushort HopTableIndex { get; internal set; }

    /// <summary>Twice the header variant, used as a byte offset into four-word list tables.</summary>
    public ushort VariantTableOffset { get; internal set; }

    /// <summary>Population initialization word: zero means floor, nonzero means ceiling.</summary>
    public bool UpsideDown { get; internal set; }
}

/// <summary>
/// Literal translation of shared hopper AI $A3:AA68-$AEDD. It covers small/large
/// Sidehopper, the Tourian Sidehopper palette/vulnerability variant, and small/large
/// Dessgeega without inventing per-room behavior.
/// </summary>
public sealed partial class RoomEnemySystem
{
    internal const ushort SidehopperDefinition = 0xd93f;
    internal const ushort DessgeegaDefinition = 0xd97f;
    internal const ushort LargeSidehopperDefinition = 0xd9bf;
    internal const ushort TourianSidehopperDefinition = 0xd9ff;
    internal const ushort LargeDessgeegaDefinition = 0xda3f;

    private const int HopperLandedUpsideUpPointerTable = 0xa3aac2;
    private const int HopperLandedUpsideDownPointerTable = 0xa3aaca;
    private const int HopperJumpingUpsideUpPointerTable = 0xa3aad2;
    private const int HopperJumpingUpsideDownPointerTable = 0xa3aada;
    private const ushort HopperRandomSeed = 0x0025;
    private const ushort MaximumHopperYSpeedTableIndex = 0x0040;

    private readonly HopperEnemyState?[] _hopperStates =
        new HopperEnemyState?[MaximumEnemyCount];

    /// <summary>Typed state for every physical slot currently owned by the hopper family.</summary>
    public IReadOnlyList<HopperEnemyState?> HopperStates => _hopperStates;

    private static bool IsHopperDefinition(ushort definitionPointer) => definitionPointer is
        SidehopperDefinition or
        DessgeegaDefinition or
        LargeSidehopperDefinition or
        TourianSidehopperDefinition or
        LargeDessgeegaDefinition;

    /// <summary>Ports <c>InitAI_Hopper</c> at $A3:AB09.</summary>
    private void InitializeHopper(RoomEnemySlot slot)
    {
        if (slot.Definition.VariantIndex > 3)
        {
            throw new InvalidDataException(
                $"Hopper definition ${slot.EnemyDefinitionPointer:X4} variant " +
                $"{slot.Definition.VariantIndex} exceeds its four-entry animation tables.");
        }
        if (_setRandomNumber is null)
        {
            throw new InvalidOperationException(
                "Hopper initialization requires the shared RNG seed-write callback.");
        }

        // The initializer overwrites the global RNG seed and consumes one generated value
        // even though it never reads that value. This changes later enemy randomness and is
        // therefore observable whenever a hopper appears earlier in population order.
        _setRandomNumber(HopperRandomSeed);
        _nextRandom!();

        var state = new HopperEnemyState(slot)
        {
            Falling = false,
            ReadyToHop = false,
            HopTableIndex = slot.Definition.VariantIndex == 0 ? (ushort)0 : (ushort)2,
            VariantTableOffset = unchecked((ushort)(slot.Definition.VariantIndex * 2)),
            UpsideDown = slot.Spawn.Population.InitializationParameter != 0,
            Function = HopperEnemyFunction.ChooseHopSize,
        };
        _hopperStates[slot.SlotIndex] = state;

        SetHopperInstructionList(slot, state, ReadHopperInstructionList(
            state,
            state.UpsideDown
                ? HopperLandedUpsideDownPointerTable
                : HopperLandedUpsideUpPointerTable));

        // Both physics rows currently contain the same constants, but the header-dependent
        // byte offset and two independent calculations are part of the ROM contract.
        state.SmallHopInitialYSpeedTableIndex = CalculateInitialHopperYSpeedTableIndex(
            jumpHeight: 0x1000,
            tableIndexDelta: 3);
        state.BigHopInitialYSpeedTableIndex = CalculateInitialHopperYSpeedTableIndex(
            jumpHeight: 0x3000,
            tableIndexDelta: 4);
    }

    /// <summary>Ports <c>MainAI_Hopper</c> and every indirect target at $A3:ABCF-$AD56.</summary>
    private void RunHopperMain(
        RoomEnemySlot slot,
        SamusState? samus,
        RoomLevelData? level)
    {
        HopperEnemyState state = RequireHopperState(slot);
        switch (state.Function)
        {
            case HopperEnemyFunction.ChooseHopSize:
                // The ROM consumes an entire frame choosing one of two setup functions.
                state.Function = (_nextRandom!() & 1) == 0
                    ? HopperEnemyFunction.PrepareSmallHop
                    : HopperEnemyFunction.PrepareBigHop;
                return;
            case HopperEnemyFunction.PrepareSmallHop:
                PrepareHopperHop(state, bigHop: false);
                return;
            case HopperEnemyFunction.PrepareBigHop:
                PrepareHopperHop(state, bigHop: true);
                return;
            case HopperEnemyFunction.ChooseDirectionUpsideUp:
                ChooseHopperDirection(slot, state, samus, upsideDown: false);
                return;
            case HopperEnemyFunction.ChooseDirectionUpsideDown:
                ChooseHopperDirection(slot, state, samus, upsideDown: true);
                return;
            case HopperEnemyFunction.StartBackwardHopUpsideUp:
                StartHopperJump(slot, state, upsideDown: false, backward: true);
                return;
            case HopperEnemyFunction.StartForwardHopUpsideUp:
                StartHopperJump(slot, state, upsideDown: false, backward: false);
                return;
            case HopperEnemyFunction.StartBackwardHopUpsideDown:
                StartHopperJump(slot, state, upsideDown: true, backward: true);
                return;
            case HopperEnemyFunction.StartForwardHopUpsideDown:
                StartHopperJump(slot, state, upsideDown: true, backward: false);
                return;
            case HopperEnemyFunction.Landed:
                LandHopper(slot, state);
                return;
            case HopperEnemyFunction.JumpingUpsideUpBackward:
            case HopperEnemyFunction.JumpingUpsideUpForward:
                RequireHopperLevel(level);
                RunHopperMovement(slot, state, level!, upsideDown: false);
                return;
            case HopperEnemyFunction.JumpingUpsideDownBackward:
            case HopperEnemyFunction.JumpingUpsideDownForward:
                RequireHopperLevel(level);
                RunHopperMovement(slot, state, level!, upsideDown: true);
                return;
            case HopperEnemyFunction.WaitToHop:
                if (state.ReadyToHop)
                {
                    state.ReadyToHop = false;
                    state.Function = HopperEnemyFunction.ChooseHopSize;
                }
                return;
            default:
                throw new NotSupportedException(
                    $"Hopper function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void RequireHopperLevel(RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Hopper movement requires room level data.");
    }

    private static void PrepareHopperHop(HopperEnemyState state, bool bigHop)
    {
        state.YSpeedTableIndexDelta = bigHop ? (ushort)4 : (ushort)3;
        state.XVelocity = 3;
        state.YSpeedTableIndex = bigHop
            ? state.BigHopInitialYSpeedTableIndex
            : state.SmallHopInitialYSpeedTableIndex;
        state.Function = state.UpsideDown
            ? HopperEnemyFunction.ChooseDirectionUpsideDown
            : HopperEnemyFunction.ChooseDirectionUpsideUp;
    }

    private static void ChooseHopperDirection(
        RoomEnemySlot slot,
        HopperEnemyState state,
        SamusState? samus,
        bool upsideDown)
    {
        if (samus is null)
            throw new InvalidOperationException("Hopper direction selection requires Samus.");

        bool samusIsLeft = unchecked((short)(samus.XPosition - slot.XPosition)) < 0;
        state.Function = (upsideDown, samusIsLeft) switch
        {
            (false, true) => HopperEnemyFunction.StartBackwardHopUpsideUp,
            (false, false) => HopperEnemyFunction.StartForwardHopUpsideUp,
            (true, true) => HopperEnemyFunction.StartBackwardHopUpsideDown,
            (true, false) => HopperEnemyFunction.StartForwardHopUpsideDown,
        };
    }

    private void StartHopperJump(
        RoomEnemySlot slot,
        HopperEnemyState state,
        bool upsideDown,
        bool backward)
    {
        // “Backward” is the leftward branch selected when Samus is left of the actor. The
        // initial setup always writes +3, so only this branch negates the whole-pixel word.
        if (backward)
            state.XVelocity = unchecked((short)-state.XVelocity);

        SetHopperInstructionList(slot, state, ReadHopperInstructionList(
            state,
            upsideDown
                ? HopperJumpingUpsideDownPointerTable
                : HopperJumpingUpsideUpPointerTable));
        state.Function = (upsideDown, backward) switch
        {
            (false, true) => HopperEnemyFunction.JumpingUpsideUpBackward,
            (false, false) => HopperEnemyFunction.JumpingUpsideUpForward,
            (true, true) => HopperEnemyFunction.JumpingUpsideDownBackward,
            (true, false) => HopperEnemyFunction.JumpingUpsideDownForward,
        };
    }

    private void LandHopper(RoomEnemySlot slot, HopperEnemyState state)
    {
        SetHopperInstructionList(slot, state, ReadHopperInstructionList(
            state,
            state.UpsideDown
                ? HopperLandedUpsideDownPointerTable
                : HopperLandedUpsideUpPointerTable));
        state.Function = HopperEnemyFunction.WaitToHop;
    }

    private void RunHopperMovement(
        RoomEnemySlot slot,
        HopperEnemyState state,
        RoomLevelData level,
        bool upsideDown)
    {
        bool movingAwayFromSurface = !state.Falling;
        bool useNegativeSpeed = upsideDown == movingAwayFromSurface;
        int verticalDisplacement = ReadQuadraticEnemySpeed(
            state.YSpeedTableIndex,
            negative: useNegativeSpeed);

        // Every native movement entry resolves Y first. A collision while rising reverses
        // X and changes to the falling half without performing horizontal motion that frame.
        // A collision while falling installs Landed and likewise ends the frame immediately.
        if (MoveEnemyVertically(level, slot, verticalDisplacement))
        {
            if (movingAwayFromSurface)
            {
                state.XVelocity = unchecked((short)-state.XVelocity);
                state.Falling = true;
            }
            else
            {
                state.Falling = false;
                state.Function = HopperEnemyFunction.Landed;
            }
            return;
        }

        int horizontalDisplacement = state.XVelocity << 16;
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(level, slot, horizontalDisplacement))
        {
            state.XVelocity = unchecked((short)-state.XVelocity);
            if (movingAwayFromSurface)
            {
                // Rising code treats a wall exactly like a ceiling/floor interruption and
                // immediately enters the falling half. Falling code only reverses X.
                state.Falling = true;
                return;
            }
        }

        if (movingAwayFromSurface)
        {
            ushort next = unchecked((ushort)(
                state.YSpeedTableIndex - state.YSpeedTableIndexDelta));
            state.YSpeedTableIndex = next;
            if ((short)next < 0)
            {
                state.YSpeedTableIndex = 0;
                state.Falling = true;
            }
            return;
        }

        ushort fallingIndex = unchecked((ushort)(
            state.YSpeedTableIndex + state.YSpeedTableIndexDelta));
        state.YSpeedTableIndex = unchecked((short)(fallingIndex - MaximumHopperYSpeedTableIndex)) < 0
            ? fallingIndex
            : MaximumHopperYSpeedTableIndex;
    }

    private ushort CalculateInitialHopperYSpeedTableIndex(
        ushort jumpHeight,
        ushort tableIndexDelta)
    {
        ushort tableIndex = 0;
        ushort accumulatedHeight = 0;
        for (int iteration = 0; iteration < ushort.MaxValue; iteration++)
        {
            tableIndex = unchecked((ushort)(tableIndex + tableIndexDelta));

            // $A3:ABAF deliberately reads at table+1: the word consists of the subspeed's
            // high byte and speed's low byte. A naturally aligned 16-bit read changes every
            // initial jump index, so retain this odd unaligned cartridge access literally.
            ushort heightIncrement = ReadWord(
                _bus!,
                QuadraticEnemySpeedTable + tableIndex * 8 + 1);
            accumulatedHeight = unchecked((ushort)(accumulatedHeight + heightIncrement));
            if (unchecked((short)(accumulatedHeight - jumpHeight)) >= 0)
                return tableIndex;
        }

        throw new InvalidDataException(
            "Hopper initial-speed calculation did not reach its ROM jump height.");
    }

    private ushort ReadHopperInstructionList(HopperEnemyState state, int pointerTable) =>
        ReadWord(_bus!, pointerTable + state.VariantTableOffset);

    private static void SetHopperInstructionList(
        RoomEnemySlot slot,
        HopperEnemyState state,
        ushort instructionList)
    {
        state.InstalledInstructionList = instructionList;
        slot.CurrentInstruction = instructionList;
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private HopperEnemyState RequireHopperState(RoomEnemySlot slot) =>
        _hopperStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Hopper state.");
}
