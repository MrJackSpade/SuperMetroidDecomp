namespace SuperMetroid.Core.Game;

/// <summary>One compiled Lower Norfair Rio mechanics word at its bank-$A2 address.</summary>
internal readonly record struct LowerNorfairRioInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, private callbacks, and loop control for Lower Norfair Rio parent and
/// flame programs. Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class LowerNorfairRioInstructionProgramDefinitions
{
    /// <summary><c>InstList_Holtz_Idle_0</c> at $A2:C61A.</summary>
    internal const ushort Idle = 0xc61a;
    /// <summary><c>InstList_Holtz_PrepareToSwoop</c> at $A2:C630.</summary>
    internal const ushort PrepareToSwoop = 0xc630;
    /// <summary><c>InstList_Holtz_Swoop_Descending</c> at $A2:C65A.</summary>
    internal const ushort Descending = 0xc65a;
    /// <summary><c>InstList_Holtz_Swoop_Ascending_Part1</c> at $A2:C662.</summary>
    internal const ushort AscendingPart1 = 0xc662;
    /// <summary><c>InstList_Holtz_Swoop_Part2_0</c> at $A2:C674.</summary>
    internal const ushort AscendingPart2 = 0xc674;
    /// <summary><c>InstList_Holtz_SwoopCooldown</c> at $A2:C686.</summary>
    internal const ushort Cooldown = 0xc686;
    /// <summary><c>InstList_Holtz_Flames</c> at $A2:C6B0.</summary>
    internal const ushort Flames = 0xc6b0;
    /// <summary><c>UNUSED_HoltzConstants_A2C6C0</c>, adjacent data at $A2:C6C0.</summary>
    internal const ushort AdjacentMovementDefinitions = 0xc6c0;

    private static readonly LowerNorfairRioInstructionMechanicsWord[] Words =
        BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static LowerNorfairRioInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Lower Norfair Rio control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            LowerNorfairRioInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Lower Norfair Rio instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000)
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

    private static LowerNorfairRioInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<LowerNorfairRioInstructionMechanicsWord>(capacity: 51);
        AddCallback(words, Idle, LowerNorfairRioInstructionCodes.HideFlames);
        AddLoop(words, unchecked((ushort)(Idle + 2)), [11, 11, 11, 11],
            unchecked((ushort)(Idle + 2)));

        AddCallback(words, PrepareToSwoop, LowerNorfairRioInstructionCodes.HideFlames);
        AddFrames(words, unchecked((ushort)(PrepareToSwoop + 2)),
            [3, 3, 3, 3, 2, 1, 2, 3, 3]);
        AddCallback(words, unchecked((ushort)(PrepareToSwoop + 38)),
            LowerNorfairRioInstructionCodes.SetAnimationFinishedFlag);
        AddSleep(words, unchecked((ushort)(PrepareToSwoop + 40)));

        AddCallback(words, Descending, LowerNorfairRioInstructionCodes.HideFlames);
        AddFrames(words, unchecked((ushort)(Descending + 2)), [1]);
        AddSleep(words, unchecked((ushort)(Descending + 6)));

        AddCallback(words, AscendingPart1, LowerNorfairRioInstructionCodes.HideFlames);
        AddFrames(words, unchecked((ushort)(AscendingPart1 + 2)), [3, 3, 3]);
        AddCallback(words, unchecked((ushort)(AscendingPart1 + 14)),
            LowerNorfairRioInstructionCodes.SetAnimationFinishedFlag);
        AddSleep(words, unchecked((ushort)(AscendingPart1 + 16)));

        AddCallback(words, AscendingPart2, LowerNorfairRioInstructionCodes.ShowFlames);
        AddLoop(words, unchecked((ushort)(AscendingPart2 + 2)), [2, 2, 2],
            unchecked((ushort)(AscendingPart2 + 2)));

        AddCallback(words, Cooldown, LowerNorfairRioInstructionCodes.ShowFlames);
        AddFrames(words, unchecked((ushort)(Cooldown + 2)), [3, 3, 2, 1, 2, 3, 1, 1, 1]);
        AddCallback(words, unchecked((ushort)(Cooldown + 38)),
            LowerNorfairRioInstructionCodes.SetAnimationFinishedFlag);
        AddSleep(words, unchecked((ushort)(Cooldown + 40)));

        AddLoop(words, Flames, [6, 4, 3], Flames);
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 32);
        AddPresentationWords(words, unchecked((ushort)(Idle + 2)), 4);
        AddPresentationWords(words, unchecked((ushort)(PrepareToSwoop + 2)), 9);
        AddPresentationWords(words, unchecked((ushort)(Descending + 2)), 1);
        AddPresentationWords(words, unchecked((ushort)(AscendingPart1 + 2)), 3);
        AddPresentationWords(words, unchecked((ushort)(AscendingPart2 + 2)), 3);
        AddPresentationWords(words, unchecked((ushort)(Cooldown + 2)), 9);
        AddPresentationWords(words, Flames, 3);
        return words.ToArray();
    }

    private static void AddFrames(
        List<LowerNorfairRioInstructionMechanicsWord> words,
        ushort firstDuration,
        ReadOnlySpan<ushort> durations)
    {
        for (int frame = 0; frame < durations.Length; frame++)
        {
            words.Add(new(
                unchecked((ushort)(firstDuration + frame * 4)),
                durations[frame]));
        }
    }

    private static void AddLoop(
        List<LowerNorfairRioInstructionMechanicsWord> words,
        ushort firstDuration,
        ReadOnlySpan<ushort> durations,
        ushort target)
    {
        AddFrames(words, firstDuration, durations);
        ushort gotoAddress = unchecked((ushort)(firstDuration + durations.Length * 4));
        words.Add(new(gotoAddress, CommonEnemyInstructionCodes.Goto));
        words.Add(new(unchecked((ushort)(gotoAddress + 2)), target));
    }

    private static void AddCallback(
        List<LowerNorfairRioInstructionMechanicsWord> words,
        ushort address,
        ushort callback) => words.Add(new(address, callback));

    private static void AddSleep(
        List<LowerNorfairRioInstructionMechanicsWord> words,
        ushort address) => words.Add(new(address, CommonEnemyInstructionCodes.Sleep));

    private static void AddPresentationWords(
        List<ushort> words,
        ushort firstDuration,
        int frameCount)
    {
        for (int frame = 0; frame < frameCount; frame++)
            words.Add(unchecked((ushort)(firstDuration + frame * 4 + 2)));
    }
}
