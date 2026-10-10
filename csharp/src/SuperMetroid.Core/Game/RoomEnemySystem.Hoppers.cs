using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Same-bank function pointers used by the Sidehopper/Dessgeega state machine. The apparent
/// duplicates at $AD20 and $AD44 are real ROM entry points: the direction-selection setup
/// chooses a different pointer even though both entries currently dispatch identical motion.
/// </summary>
public enum HopperEnemyFunction : ushort
{
    /// <summary>$A3:ABD6, Function_Hopper_Hop: consumes a shared RNG value and chooses small or big hop from its low bit without starting movement that update.</summary>
    ChooseHopSize = 0xabd6,
    /// <summary>$A3:ABE6, Function_Hopper_SmallHop: installs the precomputed small-hop speed index, index delta 3, and positive three-pixel X velocity before choosing floor or ceiling direction setup.</summary>
    PrepareSmallHop = 0xabe6,
    /// <summary>$A3:AC13, Function_Hopper_BigHop: installs the precomputed big-hop speed index, index delta 4, and positive three-pixel X velocity before choosing floor or ceiling direction setup.</summary>
    PrepareBigHop = 0xac13,
    /// <summary>$A3:AC40, Function_Hopper_Hop_UpsideUp: compares Samus X against a floor Hopper and selects the leftward/backward or rightward/forward start pointer.</summary>
    ChooseDirectionUpsideUp = 0xac40,
    /// <summary>$A3:AC56, Function_Hopper_Hop_UpsideDown: compares Samus X against a ceiling Hopper and selects the leftward/backward or rightward/forward start pointer.</summary>
    ChooseDirectionUpsideDown = 0xac56,
    /// <summary>$A3:AC6C, Function_Hopper_HopBackwards_UpsideUp: negates X velocity, installs the floor jumping list, and selects the floor backward movement entry.</summary>
    StartBackwardHopUpsideUp = 0xac6c,
    /// <summary>$A3:AC8F, Function_Hopper_HopForwards_UpsideUp: retains positive X velocity, installs the floor jumping list, and selects the distinct forward movement entry.</summary>
    StartForwardHopUpsideUp = 0xac8f,
    /// <summary>$A3:ACA8, Function_Hopper_HopBackwards_UpsideDown: negates X velocity, installs the ceiling jumping list, and selects the ceiling backward movement entry.</summary>
    StartBackwardHopUpsideDown = 0xaca8,
    /// <summary>$A3:ACCB, Function_Hopper_HopForwards_UpsideDown: retains positive X velocity, installs the ceiling jumping list, and selects the distinct forward movement entry.</summary>
    StartForwardHopUpsideDown = 0xaccb,
    /// <summary>$A3:ACE4, Function_Hopper_Landed: restores the variant's floor or ceiling idle list and waits for its instruction-driven ready flag.</summary>
    Landed = 0xace4,
    /// <summary>$A3:AD0E, Function_Hopper_Jumping_UpsideUp: floor hop motion selected by the initial leftward branch, resolving vertical movement before horizontal movement and landing.</summary>
    JumpingUpsideUpBackward = 0xad0e,
    /// <summary>$A3:AD20, Function_Hopper_Jumping_UpsideUp_duplicate: distinct floor hop entry selected by the initial rightward branch, with the same native motion as $AD0E.</summary>
    JumpingUpsideUpForward = 0xad20,
    /// <summary>$A3:AD32, Function_Hopper_Jumping_UpsideDown: ceiling hop motion selected by the initial leftward branch, reversing the quadratic table's world-Y halves relative to floor hopping.</summary>
    JumpingUpsideDownBackward = 0xad32,
    /// <summary>$A3:AD44, Function_Hopper_Jumping_UpsideDown_duplicate: distinct ceiling hop entry selected by the initial rightward branch, with the same native motion as $AD32.</summary>
    JumpingUpsideDownForward = 0xad44,
    /// <summary>$A3:AD56, Function_Hopper_WaitToHop: consumes and clears the ready flag published by instruction $A3:AAFE, then returns to random hop-size selection.</summary>
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

    /// <summary>The current bank-$A3 hopper dispatcher pointer, backed by the owning slot's VariableA and advanced one setup stage per AI call.</summary>
    public HopperEnemyFunction Function
    {
        get => (HopperEnemyFunction)_slot.VariableA;
        internal set => _slot.VariableA = (ushort)value;
    }

    /// <summary>Quadratic velocity record index backed by VariableB, multiplied by eight to address its positive/negative 16.16 pair; retreats to zero at the apex and grows up to $40 during return.</summary>
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

    /// <summary>Record-index step backed by VariableD: 3 for small hops or 4 for big hops, subtracted while moving away from the attachment surface and added while returning.</summary>
    public ushort YSpeedTableIndexDelta
    {
        get => _slot.VariableD;
        internal set => _slot.VariableD = value;
    }

    /// <summary>Native extension offset $00: last installed bank-$A3 idle/jumping instruction-list identity, retained alongside the live instruction cursor.</summary>
    public ushort InstalledInstructionList { get; internal set; }
    /// <summary>Native extension offset $02: initial quadratic record index calculated with step 3 and accumulated-height threshold $1000 using the cartridge's odd-byte speed-word reads.</summary>
    public ushort SmallHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>Native extension offset $04: initial quadratic record index calculated with step 4 and accumulated-height threshold $3000 using the cartridge's odd-byte speed-word reads.</summary>
    public ushort BigHopInitialYSpeedTableIndex { get; internal set; }
    /// <summary>Native extension offset $06: true selects motion back toward the attachment surface, downward for floor Hoppers or upward for ceiling Hoppers; collision or the apex ends the outward phase.</summary>
    public bool Falling { get; internal set; }
    /// <summary>Native extension offset $08: latch set by idle-animation instruction $A3:AAFE and consumed by WaitToHop; authored animation cadence, not a host jump timer, controls the next hop.</summary>
    public bool ReadyToHop { get; internal set; }

    /// <summary>Byte offset zero for Sidehopper physics, two for every other definition.</summary>
    public ushort HopTableIndex { get; internal set; }

    /// <summary>Twice the header variant, used as a byte offset into four-word list tables.</summary>
    public ushort VariantTableOffset { get; internal set; }

    /// <summary>Population parameter one / native <c>$0FB4</c>: zero means floor, nonzero means ceiling.</summary>
    public bool UpsideDown { get; internal set; }
}

/// <summary>
/// Literal translation of shared hopper AI $A3:AA68-$AEDD. It covers small/large
/// Sidehopper, the Tourian Sidehopper palette/vulnerability variant, and small/large
/// Dessgeega without inventing per-room behavior.
/// </summary>
public sealed partial class RoomEnemySystem
{
    /// <summary>Executes the hopper family's private animation instructions.</summary>
    private bool TryProcessHopperInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (!IsHopperDefinition(slot.EnemyDefinitionPointer) ||
            !Enum.IsDefined((HopperInstruction)word))
            return false;

        switch ((HopperInstruction)word)
        {
            case HopperInstruction.SidehopperQueueSoundInY:
                // Sidehopper's list passes a library-two sound operand, then the native
                // instruction returns the cursor after that operand. Audio playback is
                // an outer concern; publishing the exact word keeps the event observable.
                LastHopperSoundEffect = ReadEnemyInstructionMechanicsWord(slot, unchecked((ushort)(cursor + 2)));
                cursor = unchecked((ushort)(cursor + 4));
                return true;
            case HopperInstruction.ReadyToHop:
                RequireHopperState(slot).ReadyToHop = true;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Hopper does not own instruction ${word:X4}.");
        }
    }


    private const ushort HopperRandomSeed = 0x0025;
    private const ushort MaximumHopperYSpeedTableIndex = 0x0040;

    private readonly HopperEnemyState?[] _hopperStates =
        new HopperEnemyState?[MaximumEnemyCount];

    private static bool IsHopperDefinition(EnemyDefinitionId definitionPointer) => definitionPointer is
        EnemyDefinitionId.Sidehopper or
        EnemyDefinitionId.Dessgeega or
        EnemyDefinitionId.SidehopperLarge or
        EnemyDefinitionId.SidehopperTourian or
        EnemyDefinitionId.DessgeegaLarge;

    /// <summary>Ports <c>InitAI_Hopper</c> at $A3:AB09.</summary>
    private void InitializeHopper(RoomEnemySlot slot)
    {
        if (slot.Definition.VariantIndex > 3)
        {
            throw new InvalidDataException(
                $"Hopper definition ${(int)slot.EnemyDefinitionPointer:X4} variant " +
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
            UpsideDown = slot.Parameter1 != 0,
            Function = HopperEnemyFunction.ChooseHopSize,
        };
        _hopperStates[slot.SlotIndex] = state;

        SetHopperInstructionList(slot, state, ReadHopperInstructionList(
            state, state.UpsideDown, jumping: false));

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
                throw new InvalidDataException(
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

    private static void StartHopperJump(
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
            state, upsideDown, jumping: true));
        state.Function = (upsideDown, backward) switch
        {
            (false, true) => HopperEnemyFunction.JumpingUpsideUpBackward,
            (false, false) => HopperEnemyFunction.JumpingUpsideUpForward,
            (true, true) => HopperEnemyFunction.JumpingUpsideDownBackward,
            (true, false) => HopperEnemyFunction.JumpingUpsideDownForward,
        };
    }

    private static void LandHopper(RoomEnemySlot slot, HopperEnemyState state)
    {
        SetHopperInstructionList(slot, state, ReadHopperInstructionList(
            state, state.UpsideDown, jumping: false));
        state.Function = HopperEnemyFunction.WaitToHop;
    }

    private void RunHopperMovement(
        RoomEnemySlot slot,
        HopperEnemyState state,
        RoomLevelData level,
        bool upsideDown)
    {
        bool movingAwayFromSurface = !state.Falling;
        // Native has four movement routines: floor actors use the negative half while
        // jumping and the positive half while falling; ceiling actors do the inverse.
        // Inequality expresses that table exactly. Equality reverses both phases, making
        // each actor collide with its starting surface and then migrate to the opposite one.
        bool useNegativeSpeed = upsideDown != movingAwayFromSurface;
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

    private static ushort CalculateInitialHopperYSpeedTableIndex(
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
            ushort heightIncrement = EnemyQuadraticSpeedDefinitions.ReadWord(tableIndex * 8 + 1);
            accumulatedHeight = unchecked((ushort)(accumulatedHeight + heightIncrement));
            if (unchecked((short)(accumulatedHeight - jumpHeight)) >= 0)
                return tableIndex;
        }

        throw new InvalidDataException(
            "Hopper initial-speed calculation did not reach its ROM jump height.");
    }

    private static ushort ReadHopperInstructionList(
        HopperEnemyState state,
        bool upsideDown,
        bool jumping) =>
        HopperAnimationDefinitions.InstructionList(
            (ushort)(state.VariantTableOffset >> 1), upsideDown, jumping);

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
