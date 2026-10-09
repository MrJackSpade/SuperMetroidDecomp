namespace SuperMetroid.Core.Game;

/// <summary>
/// One of the ten health-tier programs used by the large and paired-small Zebetites.
/// </summary>
/// <param name="Entry">Bank-$A6 address of the program's duration word.</param>
/// <param name="Presentation">Address of the visual selector paired with that duration.</param>
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

    /// <summary>Number of compiled health-tier instruction lists shared by the large and paired-small Zebetites.</summary>
    internal static int ProgramCount => 10;

    /// <summary>Number of duration and terminal-control words across all ten programs.</summary>
    public static int MechanicsWordCount => ProgramCount * 2;

    /// <summary>Number of visual selector words, one for each health-tier program.</summary>
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

    /// <summary>Returns one compiled duration or sleep-control word in program-list order.</summary>
    /// <param name="index">Zero-based index across the twenty mechanics words.</param>
    /// <returns>The bank-relative address and expected value of the selected word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics table.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Word(ProgramAt(index / 2).Entry, (ushort)((index & 1) * 4));
    }

    /// <summary>Returns the address of a health-tier program's paired visual selector word.</summary>
    /// <param name="index">Zero-based health-tier program index, from zero through nine.</param>
    /// <returns>The bank-relative address of that program's presentation selector.</returns>
    /// <exception cref="IndexOutOfRangeException">The index does not identify one of the ten programs.</exception>
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

    /// <summary>Pairs a program's duration entry with the visual selector stored in its next word.</summary>
    /// <param name="entry">The bank-relative address of the program's duration word.</param>
    /// <returns>The addresses used to verify its mechanics and presentation data.</returns>
    private static ZebetiteInstructionProgram Program(ushort entry) =>
        new(entry, unchecked((ushort)(entry + 2)));

    /// <summary>Describes the expected value at a mechanics offset within one program.</summary>
    /// <param name="entry">The bank-relative address of the program's duration word.</param>
    /// <param name="offset">Byte offset selecting the duration word or following terminal command.</param>
    /// <returns>The selected address and its expected duration or shared sleep opcode.</returns>
    private static InstructionMechanicsWord Word(ushort entry, ushort offset) =>
        new(
            unchecked((ushort)(entry + offset)),
            offset == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
}
