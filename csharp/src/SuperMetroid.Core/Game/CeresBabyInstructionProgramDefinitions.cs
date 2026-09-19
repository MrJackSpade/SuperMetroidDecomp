namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics word in the Ceres Baby's bank-$A6 draw program.</summary>
internal readonly record struct CeresBabyInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Engine-owned control words from the Ceres Baby's mixed draw instruction program.
/// </summary>
/// <remarks>
/// Callback identities, branch targets, and frame durations determine animation cadence
/// and control flow. Palette and spritemap operands remain live cartridge presentation
/// reads so replacement artwork does not become fixed engine data.
/// </remarks>
internal static class CeresBabyInstructionProgramDefinitions
{
    /// <summary><c>InstList_BabyMetroidCutscene_0</c> at $A6:BF31.</summary>
    internal const ushort Initial = 0xbf31;

    /// <summary><c>InstList_BabyMetroidCutscene_1</c> at $A6:BF59.</summary>
    internal const ushort ExpressiveLoop = 0xbf59;

    private static readonly CeresBabyInstructionMechanicsWord[] Words = CreateWords();

    private static readonly ushort[] PresentationWords =
    [
        0xbf37, 0xbf3b, 0xbf3f, 0xbf43, 0xbf4b, 0xbf4f, 0xbf53, 0xbf57,
        0xbf5f, 0xbf63, 0xbf67, 0xbf6b, 0xbf6f, 0xbf73, 0xbf77, 0xbf7b,
        0xbf7f, 0xbf83, 0xbf87, 0xbf8b, 0xbf8f, 0xbf93, 0xbf97, 0xbf9b,
        0xbf9f, 0xbfa3, 0xbfa7, 0xbfab, 0xbfaf, 0xbfb3, 0xbfb7, 0xbfbb,
        0xbfbf,
    ];

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;

    internal static CeresBabyInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];

    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>
    /// Reads one compiled control word and rejects presentation or adjacent code addresses.
    /// </summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            CeresBabyInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Ceres Baby instruction mechanics pointer $A6:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < Words.Length; index++)
        {
            ushort wordAddress = Words[index].Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }

    private static CeresBabyInstructionMechanicsWord[] CreateWords()
    {
        var words = new List<CeresBabyInstructionMechanicsWord>();

        Add(words, 0xbf31, CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoXIfNotFalling);
        Add(words, 0xbf33, ExpressiveLoop);
        AddFrame(words, 0xbf35, 10);
        AddFrame(words, 0xbf39, 10);
        AddFrame(words, 0xbf3d, 10);
        AddFrame(words, 0xbf41, 10);
        Add(words, 0xbf45, CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoXIfNotFalling);
        Add(words, 0xbf47, ExpressiveLoop);
        AddFrame(words, 0xbf49, 10);
        AddFrame(words, 0xbf4d, 10);
        AddFrame(words, 0xbf51, 10);
        AddFrame(words, 0xbf55, 10);

        Add(words, 0xbf59, CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_PlayCrySFXOrGotoX);
        Add(words, 0xbf5b, Initial);
        AddPaletteFrame(words, 0xbf5d, 0xbf61, 6);
        AddPaletteFrame(words, 0xbf65, 0xbf69, 5);
        AddPaletteFrame(words, 0xbf6d, 0xbf71, 4);
        AddPaletteFrame(words, 0xbf75, 0xbf79, 3);
        AddPaletteFrame(words, 0xbf7d, 0xbf81, 2);
        AddPaletteFrame(words, 0xbf85, 0xbf89, 3);
        AddPaletteFrame(words, 0xbf8d, 0xbf91, 4);
        AddPaletteFrame(words, 0xbf95, 0xbf99, 5);
        AddPaletteFrame(words, 0xbf9d, 0xbfa1, 6);
        AddPaletteFrame(words, 0xbfa5, 0xbfa9, 7);
        AddPaletteFrame(words, 0xbfad, 0xbfb1, 8);
        AddPaletteFrame(words, 0xbfb5, 0xbfb9, 9);
        Add(words, 0xbfbd, CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_UpdateColors);
        Add(words, 0xbfc1, CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoXIfNotFalling);
        Add(words, 0xbfc3, ExpressiveLoop);
        Add(words, 0xbfc5, CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_GotoX);
        Add(words, 0xbfc7, Initial);

        return [.. words];
    }

    private static void AddPaletteFrame(
        List<CeresBabyInstructionMechanicsWord> words,
        ushort callbackAddress,
        ushort durationAddress,
        ushort duration)
    {
        Add(words, callbackAddress,
            CeresEnemyCodePointers.Instruction_BabyMetroidCutscene_UpdateColors);
        AddFrame(words, durationAddress, duration);
    }

    private static void AddFrame(
        List<CeresBabyInstructionMechanicsWord> words,
        ushort address,
        ushort duration) => Add(words, address, duration);

    private static void Add(
        List<CeresBabyInstructionMechanicsWord> words,
        ushort address,
        ushort value) => words.Add(new(address, value));
}
