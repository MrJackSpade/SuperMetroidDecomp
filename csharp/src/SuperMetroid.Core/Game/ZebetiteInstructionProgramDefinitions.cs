namespace SuperMetroid.Core.Game;

/// <summary>
/// One of the ten health-tier programs used by the large and paired-small Zebetites.
/// </summary>
internal readonly record struct ZebetiteInstructionProgram(
    ushort Entry,
    ushort Presentation);

/// <summary>
/// Compiled timing and terminal control for every Zebetite health-tier program. The
/// spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal abstract class ZebetiteInstructionProgramDefinitions
{
    /// <summary><c>InstList_Big_HealthGreaterThanEqualTo800</c> at $A6:FDCC.</summary>
    internal const ushort BigHealthAtLeast800 = 0xfdcc;

    internal static int ProgramCount => 10;
    public static int MechanicsWordCount => ProgramCount * 2;
    public static int PresentationWordCount => ProgramCount;

    /// <summary>
    /// $A6:FDCC-FE07 contains five large and five split-barrier health stages.
    /// Each installs one pose (duration and visual selector) then sleeps:
    /// three words per stage, with no authored animation cadence to index.
    /// </summary>
    internal static ZebetiteInstructionProgram ProgramAt(int index)
    {
        if ((uint)index >= ProgramCount) throw new IndexOutOfRangeException();
        return Program((ushort)(BigHealthAtLeast800 + index * 3 * sizeof(ushort)));
    }

    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Word(ProgramAt(index / 2).Entry, (ushort)((index & 1) * 4));
    }

    public static ushort PresentationWordAddress(int index) => ProgramAt(index).Presentation;
    /// <summary>Returns fixed Zebetite control or rejects pointers outside its ten lists.</summary>
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
            $"Zebetite instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    private static ZebetiteInstructionProgram Program(ushort entry) =>
        new(entry, unchecked((ushort)(entry + 2)));

    private static InstructionMechanicsWord Word(ushort entry, ushort offset) =>
        new(
            unchecked((ushort)(entry + offset)),
            offset == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
}
