namespace SuperMetroid.Core.Game;

/// <summary>One compiled ordinary-Metroid mechanics word at its bank-$A3 address.</summary>
internal readonly record struct MetroidInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled animation timing, sound callbacks, and loop control for ordinary Metroids.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MetroidInstructionProgramDefinitions
{
    /// <summary><c>InstList_Metroid_ChasingSamus</c> at $A3:E9CF.</summary>
    internal const ushort ChasingSamus = 0xe9cf;
    /// <summary><c>InstList_Metroid_DrainingSamus</c> at $A3:EA25.</summary>
    internal const ushort DrainingSamus = 0xea25;
    /// <summary><c>Instruction_Metroid_PlayRandomMetroidSFX</c> entry at $A3:EA1F.</summary>
    internal const ushort ChasingSoundCallback = 0xea1f;
    /// <summary><c>Instruction_Metroid_PlayDrainingSamusSFX</c> entry at $A3:EA39.</summary>
    internal const ushort DrainingSoundCallback = 0xea39;
    /// <summary><c>BombedOffVelocities</c>, adjacent non-instruction data at $A3:EA3F.</summary>
    internal const ushort AdjacentBombedOffVelocities = 0xea3f;

    private static readonly ushort[] FrameDurations = [16, 16, 6, 10, 16];
    private static readonly MetroidInstructionMechanicsWord[] Words = BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MetroidInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Metroid control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MetroidInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Metroid instruction mechanics pointer $A3:{address:X4} is not compiled.");
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

    private static MetroidInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<MetroidInstructionMechanicsWord>(capacity: 31);
        AddLoop(
            words,
            ChasingSamus,
            repetitions: 4,
            EnemyInstructionCodePointers.Instruction_Metroid_PlayRandomMetroidSFX);
        AddLoop(
            words,
            DrainingSamus,
            repetitions: 1,
            EnemyInstructionCodePointers.Instruction_Metroid_PlayDrainingSamusSFX);
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 25);
        AddPresentationWords(words, ChasingSamus, frameCount: 20);
        AddPresentationWords(words, DrainingSamus, frameCount: 5);
        return words.ToArray();
    }

    private static void AddLoop(
        List<MetroidInstructionMechanicsWord> words,
        ushort entry,
        int repetitions,
        ushort callback)
    {
        int frameCount = FrameDurations.Length * repetitions;
        for (int frame = 0; frame < frameCount; frame++)
        {
            words.Add(new(
                unchecked((ushort)(entry + frame * 4)),
                FrameDurations[frame % FrameDurations.Length]));
        }

        ushort callbackAddress = unchecked((ushort)(entry + frameCount * 4));
        words.Add(new(callbackAddress, callback));
        words.Add(new(unchecked((ushort)(callbackAddress + 2)),
            CommonEnemyInstructionCodes.Goto));
        words.Add(new(unchecked((ushort)(callbackAddress + 4)), entry));
    }

    private static void AddPresentationWords(
        List<ushort> words,
        ushort entry,
        int frameCount)
    {
        for (int frame = 0; frame < frameCount; frame++)
            words.Add(unchecked((ushort)(entry + frame * 4 + 2)));
    }
}
