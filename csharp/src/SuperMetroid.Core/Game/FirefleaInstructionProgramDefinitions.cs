namespace SuperMetroid.Core.Game;

/// <summary>One compiled Fireflea mechanics word at its native bank-$A3 address.</summary>
internal readonly record struct FirefleaInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing and loop control for Fireflea's single 52-frame program. The interleaved
/// spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class FirefleaInstructionProgramDefinitions
{
    /// <summary><c>InstList_Fireflea</c> at $A3:8C2F.</summary>
    internal const ushort Loop = 0x8c2f;
    /// <summary>The adjacent unused Fireflea data block at $A3:8D03.</summary>
    internal const ushort AdjacentUnusedData = 0x8d03;
    internal const int FrameCount = 52;

    private static readonly FirefleaInstructionMechanicsWord[] Words = BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static FirefleaInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            FirefleaInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Fireflea instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }

    private static FirefleaInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new FirefleaInstructionMechanicsWord[FrameCount + 2];
        for (int frame = 0; frame < FrameCount; frame++)
        {
            words[frame] = new(
                unchecked((ushort)(Loop + frame * 4)),
                unchecked((ushort)((frame & 1) == 0 ? 2 : 1)));
        }
        ushort gotoAddress = unchecked((ushort)(Loop + FrameCount * 4));
        words[FrameCount] = new(gotoAddress, CommonEnemyInstructionCodes.Goto);
        words[FrameCount + 1] = new(unchecked((ushort)(gotoAddress + 2)), Loop);
        return words;
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new ushort[FrameCount];
        for (int frame = 0; frame < FrameCount; frame++)
            words[frame] = unchecked((ushort)(Loop + frame * 4 + 2));
        return words;
    }
}
