namespace SuperMetroid.Core.Game;

/// <summary>Compiled mechanics words from Skultera's swimming and turning programs.</summary>
/// <remarks>
/// Layer callbacks, durations, turn completion, sleeps, and loop control are immutable
/// simulation data. The twenty-two interleaved spritemap pointers are compiled
/// separately from their editable composition assets.
/// </remarks>
internal abstract class SkulteraInstructionProgramDefinitions
{
    /// <summary><c>$A3:902A</c>, the layer callback and three-frame left-swimming loop.</summary>
    internal const ushort SwimmingLeft = 0x902a;

    /// <summary><c>$A3:903C</c>, the eight-frame turn from left to right.</summary>
    internal const ushort TurningRight = 0x903c;

    /// <summary><c>$A3:9060</c>, the layer callback and three-frame right-swimming loop.</summary>
    internal const ushort SwimmingRight = 0x9060;

    /// <summary><c>$A3:9072</c>, the eight-frame turn from right to left.</summary>
    internal const ushort TurningLeft = 0x9072;

    /// <summary>
    /// $A3:903C/9040/9044/9048: first half of the turn's mirrored holds.
    /// Mirroring removes duplicate samples, but does not derive these four timings.
    /// Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it. A short-series fit such as 13*0.8^n is incidental, not a derivation.
    /// </summary>
    private static readonly ushort[] TurnHalfDurations = [13, 10, 8, 6];
    /// <summary>$A3:902C/9062: swimming cadence. Reviewed under #1165 as authored animation cadence: the interpreter loads each value into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort SwimmingDuration = 14;

    /// <summary>Number of compiled mechanics words across Skultera's two swim and two turn programs.</summary>
    public static int MechanicsWordCount => 32;

    /// <summary>Number of compiled spritemap pointers, split evenly between left-starting and right-starting animations.</summary>
    public static int PresentationWordCount => 22;

    /// <summary>
    /// Resolves a mechanics-word position to its instruction address and operand value.
    /// The ordering covers left swimming and its turn before the mirrored right-side programs.
    /// </summary>
    /// <param name="index">Zero-based position in the compiled mechanics-word sequence.</param>
    /// <returns>The instruction address and value stored at that position.</returns>
    /// <exception cref="IndexOutOfRangeException">The position is outside the compiled sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool startsLeft = index < 16;
        int local = index % 16;
        ushort swim = startsLeft ? SwimmingLeft : SwimmingRight;
        ushort turn = startsLeft ? TurningRight : TurningLeft;
        if (local == 0)
            return new(swim, startsLeft ? EnemyInstructionCodePointers.Instruction_Skultera_SetLayerTo2
                : EnemyInstructionCodePointers.Instruction_Skultera_SetLayerTo6);
        if (local < 4) return new((ushort)(swim + 2 + 4 * (local - 1)), SwimmingDuration);
        if (local < 6)
            return new((ushort)(swim + 14 + 2 * (local - 4)), local == 4 ? CommonEnemyInstructionCodes.Goto : (ushort)(swim + 2));
        int frame = local - 6;
        if (frame < 8)
        {
            int distanceFromEnd = Math.Min(frame, 7 - frame);
            ushort duration = TurnHalfDurations[distanceFromEnd];
            return new((ushort)(turn + 4 * frame), duration);
        }
        return new((ushort)(turn + 32 + 2 * (frame - 8)), frame == 8
            ? EnemyInstructionCodePointers.Instruction_Skultera_SetTurnFinishedFlag : CommonEnemyInstructionCodes.Sleep);
    }

    /// <summary>Returns the instruction address of one compiled spritemap pointer.</summary>
    /// <param name="index">Zero-based position in the 22-pointer presentation sequence.</param>
    /// <returns>The address operand that points to the corresponding spritemap.</returns>
    /// <exception cref="IndexOutOfRangeException">The position is outside the presentation sequence.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        bool startsLeft = index < 11;
        int local = index % 11;
        return local < 3 ? (ushort)((startsLeft ? SwimmingLeft : SwimmingRight) + 4 + 4 * local)
            : (ushort)((startsLeft ? TurningRight : TurningLeft) + 2 + 4 * (local - 3));
    }

    /// <summary>Checks whether an instruction address is one of Skultera's compiled spritemap-pointer operands.</summary>
    /// <param name="address">Instruction address to classify.</param>
    /// <returns><see langword="true"/> when the address occurs in the presentation sequence; otherwise, <see langword="false"/>.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        for (int index = 0; index < PresentationWordCount; index++)
            if (PresentationWordAddress(index) == address) return true;
        return false;
    }
    /// <summary>Returns fixed Skultera control or rejects pointers outside all four programs.</summary>
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
            $"Skultera instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
