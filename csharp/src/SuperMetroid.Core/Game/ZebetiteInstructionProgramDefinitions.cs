namespace SuperMetroid.Core.Game;

/// <summary>One compiled Zebetite mechanics word at its native bank-$A6 address.</summary>
internal readonly record struct ZebetiteInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// One of the ten health-tier programs used by the large and paired-small Zebetites.
/// </summary>
internal readonly record struct ZebetiteInstructionProgram(
    ushort Entry,
    ushort Presentation,
    ushort Sleep);

/// <summary>
/// Compiled timing and terminal control for every Zebetite health-tier program. The
/// spritemap selections resolve compiled identities to installed artwork.
/// </summary>
internal static class ZebetiteInstructionProgramDefinitions
{
    /// <summary><c>InstList_Big_HealthGreaterThanEqualTo800</c> at $A6:FDCC.</summary>
    internal const ushort BigHealthAtLeast800 = 0xfdcc;
    /// <summary><c>InstList_Big_HealthLessThan800</c> at $A6:FDD2.</summary>
    internal const ushort BigHealthBelow800 = 0xfdd2;
    /// <summary><c>InstList_Big_HealthLessThan600</c> at $A6:FDD8.</summary>
    internal const ushort BigHealthBelow600 = 0xfdd8;
    /// <summary><c>InstList_Big_HealthLessThan400</c> at $A6:FDDE.</summary>
    internal const ushort BigHealthBelow400 = 0xfdde;
    /// <summary><c>InstList_Big_HealthLessThan200</c> at $A6:FDE4.</summary>
    internal const ushort BigHealthBelow200 = 0xfde4;
    /// <summary><c>InstList_Small_HealthGreaterThanEqualTo800</c> at $A6:FDEA.</summary>
    internal const ushort SmallHealthAtLeast800 = 0xfdea;
    /// <summary><c>InstList_Small_HealthLessThan800</c> at $A6:FDF0.</summary>
    internal const ushort SmallHealthBelow800 = 0xfdf0;
    /// <summary><c>InstList_Small_HealthLessThan600</c> at $A6:FDF6.</summary>
    internal const ushort SmallHealthBelow600 = 0xfdf6;
    /// <summary><c>InstList_Small_HealthLessThan400</c> at $A6:FDFC.</summary>
    internal const ushort SmallHealthBelow400 = 0xfdfc;
    /// <summary><c>InstList_Small_HealthLessThan200</c> at $A6:FE02.</summary>
    internal const ushort SmallHealthBelow200 = 0xfe02;
    /// <summary><c>Spritemap_Zebetite_Big_HealthGreaterThanEqualTo800</c> at $A6:FE08.</summary>
    internal const ushort FirstSpritemap = 0xfe08;

    internal static int ProgramCount => 10;
    internal static int MechanicsWordCount => ProgramCount * 2;
    internal static int PresentationWordCount => ProgramCount;

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

    internal static ZebetiteInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Word(ProgramAt(index / 2).Entry, (ushort)((index & 1) * 4));
    }

    internal static ushort PresentationWordAddress(int index) => ProgramAt(index).Presentation;
    /// <summary>Returns fixed Zebetite control or rejects pointers outside its ten lists.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            ZebetiteInstructionMechanicsWord candidate = MechanicsWord(middle);
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

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
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

    private static ZebetiteInstructionProgram Program(ushort entry) =>
        new(entry, unchecked((ushort)(entry + 2)), unchecked((ushort)(entry + 4)));

    private static ZebetiteInstructionMechanicsWord Word(ushort entry, ushort offset) =>
        new(
            unchecked((ushort)(entry + offset)),
            offset == 0 ? (ushort)1 : CommonEnemyInstructionCodes.Sleep);
}
