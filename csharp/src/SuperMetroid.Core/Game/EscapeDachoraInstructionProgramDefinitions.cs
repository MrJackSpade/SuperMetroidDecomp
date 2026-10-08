namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled control words for the escape-sequence Dachora's low/high-tide pacing and
/// accelerating departure programs. Spritemap selections resolve installed artwork.
/// </summary>
internal abstract class EscapeDachoraInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_0</c> at $B3:E964.</summary>
    internal const ushort RunningAroundLowTide = 0xe964;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_1</c> at $B3:E968.</summary>
    internal const ushort RunningAroundLowTideLeft = 0xe968;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_LowTide_2</c> at $B3:E99C.</summary>
    internal const ushort RunningAroundLowTideRight = 0xe99c;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_0</c> at $B3:E9D0.</summary>
    internal const ushort RunningAroundHighTide = 0xe9d0;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_1</c> at $B3:E9D4.</summary>
    internal const ushort RunningAroundHighTideLeft = 0xe9d4;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_2</c> at $B3:E9FC.</summary>
    internal const ushort RunningAroundHighTideLeftLoop = 0xe9fc;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_3</c> at $B3:EA04.</summary>
    internal const ushort RunningAroundHighTideRight = 0xea04;
    /// <summary><c>InstList_DachoraEscape_RunningAroundAimlessly_HighTide_4</c> at $B3:EA2C.</summary>
    internal const ushort RunningAroundHighTideRightLoop = 0xea2c;
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_0</c> at $B3:EA34.</summary>
    internal const ushort RunningForEscape = 0xea34;
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_1</c> at $B3:EA38.</summary>
    internal const ushort RunningForEscapeAccelerating = 0xea38;
    /// <summary><c>InstList_DachoraEscape_RunningForEscape_2</c> at $B3:EA80.</summary>
    internal const ushort RunningForEscapeMaximumSpeed = 0xea80;

    /// <summary>$B3:E968..E98B and corresponding directional runs have six duration/visual/move triples.</summary>
    private const int RunFrames = 6;
    /// <summary>$B3:E966/E99A/E9D2/EA02: chosen five-lap pacing count is retained as selected NPC movement choreography.</summary>
    private const ushort PacingRepeats = 5;
    /// <summary>$B3:E968..E9BE: chosen low-tide hold3 is retained as selected NPC movement choreography.</summary>
    private const ushort LowTideHold = 3;
    /// <summary>$B3:E9D4..EA26: chosen high-tide hold2 is retained as selected NPC movement choreography.</summary>
    private const ushort HighTideHold = 2;
    /// <summary>$B3:EA34: chosen turn/departure hold30 is retained as selected NPC movement choreography.</summary>
    private const ushort DepartureTurnHold = 30;
    /// <summary>$B3:EA38: chosen pre-acceleration hold90 is retained as selected NPC movement choreography.</summary>
    private const ushort DeparturePauseHold = 90;
    /// <summary>$B3:EA3E/EA44: chosen initial moving hold5 is retained as selected NPC movement choreography.</summary>
    private const int AccelerationInitialHold = 5;
    /// <summary>$B3:EA4A/EA5C/EA6E: chosen one-tick hold reduction is retained as selected NPC movement choreography.</summary>
    private const int AccelerationHoldReduction = 1;
    /// <summary>$B3:EA38..EA7E: selected half-gait acceleration grouping; subdivision is calculated from the six-pose run.</summary>
    private const int AccelerationFramesPerStep = RunFrames / 2;
    /// <summary>$B3:EA80..EAA2: selected every-update terminal cadence; one is the instruction scheduler minimum positive hold.</summary>
    private const ushort MaximumSpeedHold = 1;

    // These reviewed script choices determine movement callback and event/acid branch timing.
    // The half-gait and every-update policies are selected choreography, not consequences of physics.
    // No independent movement-step, artwork or room-geometry exemption is implied.
    private const int LowDirectionControls = RunFrames * 2 + 8;
    private const int HighDirectionControls = RunFrames * 2 + 6;
    private const int LowTideControls = 2 + 2 * LowDirectionControls;
    private const int HighTideControls = 2 + 2 * HighDirectionControls;
    private const int DepartureFrames = 3 * RunFrames;
    public static int MechanicsWordCount => LowTideControls + HighTideControls + 1 + 2 * DepartureFrames + 2;
    public static int PresentationWordCount => 4 * RunFrames + 1 + DepartureFrames;

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        if (index < LowTideControls + HighTideControls)
        {
            bool high = index >= LowTideControls;
            int within = high ? index - LowTideControls : index;
            ushort root = high ? RunningAroundHighTide : RunningAroundLowTide;
            if (within < 2) return new((ushort)(root + within * 2), within == 0 ? CommonEnemyInstructionCodes.SetTimer : PacingRepeats);
            int directionControls = high ? HighDirectionControls : LowDirectionControls;
            bool right = (within - 2) / directionControls != 0;
            int local = (within - 2) % directionControls;
            ushort start = high ? right ? RunningAroundHighTideRight : RunningAroundHighTideLeft
                : right ? RunningAroundLowTideRight : RunningAroundLowTideLeft;
            if (local < RunFrames * 2)
                return new((ushort)(start + local / 2 * 6 + local % 2 * 4), local % 2 == 0
                    ? high ? HighTideHold : LowTideHold
                    : right ? EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6
                        : EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionMinus6);
            int branchWord = local - RunFrames * 2;
            int address = start + RunFrames * 6 + branchWord * 2;
            if (!high && branchWord < 2)
                return new((ushort)address, branchWord == 0 ? EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfAcidLessThanCE
                    : right ? RunningAroundHighTideRightLoop : RunningAroundHighTideLeftLoop);
            int common = branchWord - (high ? 0 : 2);
            ushort value = common switch
            {
                0 => EscapeAnimalInstructionCodes.InstList_DachoraEscape_GotoY_IfCrittersEscaped,
                1 => right ? RunningForEscapeAccelerating : RunningForEscape,
                2 => CommonEnemyInstructionCodes.DecrementTimerAndGotoDuplicate,
                3 => start,
                4 => right ? CommonEnemyInstructionCodes.Goto : CommonEnemyInstructionCodes.SetTimer,
                5 => right ? root : PacingRepeats,
                _ => throw new InvalidOperationException("Dachora pacing control lies outside its native branch suffix."),
            };
            return new((ushort)address, value);
        }
        int departure = index - LowTideControls - HighTideControls;
        if (departure == 0) return new(RunningForEscape, DepartureTurnHold);
        if (departure <= 2 * DepartureFrames)
        {
            int frame = (departure - 1) / 2, slot = (departure - 1) % 2;
            ushort hold = frame == 0 ? DeparturePauseHold : (ushort)Math.Max(MaximumSpeedHold,
                AccelerationInitialHold - frame / AccelerationFramesPerStep * AccelerationHoldReduction);
            return new((ushort)(RunningForEscapeAccelerating + frame * 6 + slot * 4), slot == 0 ? hold
                : EscapeAnimalInstructionCodes.Instruction_DachoraEscape_XPositionPlus6);
        }
        int tail = departure - 1 - 2 * DepartureFrames;
        return new((ushort)(RunningForEscapeAccelerating + DepartureFrames * 6 + tail * 2),
            tail == 0 ? CommonEnemyInstructionCodes.Goto : RunningForEscapeMaximumSpeed);
    }

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 4 * RunFrames)
        {
            ushort start = (index / RunFrames) switch
            {
                0 => RunningAroundLowTideLeft, 1 => RunningAroundLowTideRight,
                2 => RunningAroundHighTideLeft, _ => RunningAroundHighTideRight,
            };
            return (ushort)(start + index % RunFrames * 6 + sizeof(ushort));
        }
        return index == 4 * RunFrames ? (ushort)(RunningForEscape + sizeof(ushort))
            : (ushort)(RunningForEscapeAccelerating + (index - 4 * RunFrames - 1) * 6 + sizeof(ushort));
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            InstructionMechanicsWord candidate = MechanicsWord(middle);
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }
        throw new InvalidDataException(
            $"Escape Dachora instruction mechanics pointer $B3:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xb30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
