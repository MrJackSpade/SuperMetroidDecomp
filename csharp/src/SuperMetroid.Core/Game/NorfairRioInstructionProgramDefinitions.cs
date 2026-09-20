namespace SuperMetroid.Core.Game;

/// <summary>One compiled Norfair Rio mechanics word at its bank-$A2 address.</summary>
internal readonly record struct NorfairRioInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, private callbacks, and loop control for Norfair Rio parent and flame
/// programs. Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class NorfairRioInstructionProgramDefinitions
{
    /// <summary><c>InstList_Geruta_Main_Idle</c> at $A2:C0F1.</summary>
    internal const ushort Idle = 0xc0f1;
    /// <summary><c>InstList_Geruta_Main_Swoop_StartDescending</c> at $A2:C107.</summary>
    internal const ushort StartDescending = 0xc107;
    /// <summary><c>InstList_Geruta_Main_Swoop_Descending</c> at $A2:C12F.</summary>
    internal const ushort Descending = 0xc12f;
    /// <summary><c>InstList_Geruta_Main_Swoop_StartAscending</c> at $A2:C145.</summary>
    internal const ushort StartAscending = 0xc145;
    /// <summary><c>InstList_Geruta_Main_Swoop_Ascending</c> at $A2:C179.</summary>
    internal const ushort Ascending = 0xc179;
    /// <summary><c>InstList_Geruta_Flames_Ascending</c> at $A2:C18F.</summary>
    internal const ushort FlamesAscending = 0xc18f;
    /// <summary><c>InstList_Geruta_Flames_Descending</c> at $A2:C1A3.</summary>
    internal const ushort FlamesDescending = 0xc1a3;
    /// <summary><c>GerutaConstants</c>, adjacent non-instruction data at $A2:C1B7.</summary>
    internal const ushort AdjacentMovementDefinitions = 0xc1b7;

    private static readonly NorfairRioInstructionMechanicsWord[] Words =
        BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static NorfairRioInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed Norfair Rio control or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            NorfairRioInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Norfair Rio instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

    private static NorfairRioInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<NorfairRioInstructionMechanicsWord>(capacity: 65);
        AddCallback(words, Idle,
            NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8_duplicate);
        AddLoop(words, unchecked((ushort)(Idle + 2)), [13, 18, 13, 18], Idle);

        AddCallbackFrameProgram(
            words,
            StartDescending,
            [
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_4,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_0,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative4,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC_duplicate,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative10,
            ],
            NorfairRioInstructionCodes.Instruction_Geruta_SetFinishedSwoopStartAnimationFlag);

        AddCallback(words, Descending,
            NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC);
        AddLoop(words, unchecked((ushort)(Descending + 2)), [6, 5, 8, 6], Descending);

        AddCallbackFrameProgram(
            words,
            StartAscending,
            [
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative10,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC_duplicate,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative4,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_0,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_4,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8_duplicate,
                NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_C,
            ],
            NorfairRioInstructionCodes.Instruction_Geruta_SetFinishedSwoopStartAnimationFlag);

        AddCallback(words, Ascending,
            NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_C_duplicate);
        AddLoop(words, unchecked((ushort)(Ascending + 2)), [6, 5, 8, 6], Ascending);
        AddLoop(words, FlamesAscending, [6, 5, 8, 6], FlamesAscending);
        AddLoop(words, FlamesDescending, [6, 5, 8, 6], FlamesDescending);
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 34);
        AddPresentationWords(words, unchecked((ushort)(Idle + 2)), frameCount: 4, stride: 4);
        AddPresentationWords(words, unchecked((ushort)(StartDescending + 2)),
            frameCount: 6, stride: 6);
        AddPresentationWords(words, unchecked((ushort)(Descending + 2)),
            frameCount: 4, stride: 4);
        AddPresentationWords(words, unchecked((ushort)(StartAscending + 2)),
            frameCount: 8, stride: 6);
        AddPresentationWords(words, unchecked((ushort)(Ascending + 2)),
            frameCount: 4, stride: 4);
        AddPresentationWords(words, FlamesAscending, frameCount: 4, stride: 4);
        AddPresentationWords(words, FlamesDescending, frameCount: 4, stride: 4);
        return words.ToArray();
    }

    private static void AddLoop(
        List<NorfairRioInstructionMechanicsWord> words,
        ushort frameEntry,
        ReadOnlySpan<ushort> durations,
        ushort target)
    {
        for (int frame = 0; frame < durations.Length; frame++)
            words.Add(new(unchecked((ushort)(frameEntry + frame * 4)), durations[frame]));
        ushort gotoAddress = unchecked((ushort)(frameEntry + durations.Length * 4));
        words.Add(new(gotoAddress, CommonEnemyInstructionCodes.Goto));
        words.Add(new(unchecked((ushort)(gotoAddress + 2)), target));
    }

    private static void AddCallbackFrameProgram(
        List<NorfairRioInstructionMechanicsWord> words,
        ushort entry,
        ReadOnlySpan<ushort> callbacks,
        ushort completionCallback)
    {
        for (int frame = 0; frame < callbacks.Length; frame++)
        {
            ushort callbackAddress = unchecked((ushort)(entry + frame * 6));
            AddCallback(words, callbackAddress, callbacks[frame]);
            words.Add(new(unchecked((ushort)(callbackAddress + 2)), (ushort)1));
        }
        ushort completionAddress = unchecked((ushort)(entry + callbacks.Length * 6));
        AddCallback(words, completionAddress, completionCallback);
        words.Add(new(unchecked((ushort)(completionAddress + 2)),
            CommonEnemyInstructionCodes.Sleep));
    }

    private static void AddCallback(
        List<NorfairRioInstructionMechanicsWord> words,
        ushort address,
        ushort callback) => words.Add(new(address, callback));

    private static void AddPresentationWords(
        List<ushort> words,
        ushort frameEntry,
        int frameCount,
        int stride)
    {
        for (int frame = 0; frame < frameCount; frame++)
            words.Add(unchecked((ushort)(frameEntry + frame * stride + 2)));
    }
}
