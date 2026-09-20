namespace SuperMetroid.Core.Game;

/// <summary>One compiled Morph Ball eye mechanics word at its bank-$A8 address.</summary>
internal readonly record struct MorphBallEyeInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled frame timing and terminal control for the Morph Ball eye body and mount.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MorphBallEyeInstructionProgramDefinitions
{
    /// <summary><c>InstList_Eye_Active</c> at $A8:8FAC.</summary>
    internal const ushort Active = 0x8fac;
    /// <summary><c>InstList_Eye_FacingRight_Deactivating</c> at $A8:8FF0.</summary>
    internal const ushort FacingRightDeactivating = 0x8ff0;
    /// <summary><c>InstList_Eye_FacingRight_Closed</c> at $A8:8FFC.</summary>
    internal const ushort FacingRightClosed = 0x8ffc;
    /// <summary><c>InstList_Eye_FacingLeft_Deactivating</c> at $A8:9002.</summary>
    internal const ushort FacingLeftDeactivating = 0x9002;
    /// <summary><c>InstList_Eye_FacingLeft_Closed</c> at $A8:900E.</summary>
    internal const ushort FacingLeftClosed = 0x900e;
    /// <summary><c>InstList_Eye_FacingRight_Activating</c> at $A8:9014.</summary>
    internal const ushort FacingRightActivating = 0x9014;
    /// <summary><c>InstList_Eye_FacingLeft_Activating</c> at $A8:9026.</summary>
    internal const ushort FacingLeftActivating = 0x9026;
    /// <summary><c>InstList_Eye_Mount_FacingRight</c> at $A8:9038.</summary>
    internal const ushort MountFacingRight = 0x9038;
    /// <summary><c>InstList_Eye_Mount_FacingDown</c> at $A8:903E.</summary>
    internal const ushort MountFacingDown = 0x903e;
    /// <summary><c>InstList_Eye_Mount_FacingLeft</c> at $A8:9044.</summary>
    internal const ushort MountFacingLeft = 0x9044;
    /// <summary><c>InstList_Eye_Mount_FacingUp</c> at $A8:904A.</summary>
    internal const ushort MountFacingUp = 0x904a;
    /// <summary><c>EyeConstants</c>, adjacent non-instruction data at $A8:9050.</summary>
    internal const ushort AdjacentProximityDefinitions = 0x9050;

    internal const int ActiveFrameCount = 16;

    private static readonly MorphBallEyeInstructionMechanicsWord[] Words =
        BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MorphBallEyeInstructionMechanicsWord MechanicsWord(int index) =>
        Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed eye control or rejects pointers outside its eleven lists.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MorphBallEyeInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Morph Ball eye instruction mechanics pointer $A8:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa80000)
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

    private static MorphBallEyeInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<MorphBallEyeInstructionMechanicsWord>(capacity: 41);
        AddFrames(words, Active, ActiveFrameCount, duration: 10);
        ushort activeGoto = unchecked((ushort)(Active + ActiveFrameCount * 4));
        words.Add(new(activeGoto, CommonEnemyInstructionCodes.Goto));
        words.Add(new(unchecked((ushort)(activeGoto + 2)), Active));

        AddFrames(words, FacingRightDeactivating, [8, 48, 5]);
        AddSleepProgram(words, FacingRightClosed, duration: 48);
        AddFrames(words, FacingLeftDeactivating, [8, 48, 5]);
        AddSleepProgram(words, FacingLeftClosed, duration: 48);
        AddSleepProgram(words, FacingRightActivating, [32, 5, 48, 8]);
        AddSleepProgram(words, FacingLeftActivating, [32, 5, 48, 8]);
        AddSleepProgram(words, MountFacingRight, duration: 1);
        AddSleepProgram(words, MountFacingDown, duration: 1);
        AddSleepProgram(words, MountFacingLeft, duration: 1);
        AddSleepProgram(words, MountFacingUp, duration: 1);
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 36);
        AddPresentationWords(words, Active, ActiveFrameCount);
        AddPresentationWords(words, FacingRightDeactivating, 3);
        AddPresentationWords(words, FacingRightClosed, 1);
        AddPresentationWords(words, FacingLeftDeactivating, 3);
        AddPresentationWords(words, FacingLeftClosed, 1);
        AddPresentationWords(words, FacingRightActivating, 4);
        AddPresentationWords(words, FacingLeftActivating, 4);
        AddPresentationWords(words, MountFacingRight, 1);
        AddPresentationWords(words, MountFacingDown, 1);
        AddPresentationWords(words, MountFacingLeft, 1);
        AddPresentationWords(words, MountFacingUp, 1);
        return words.ToArray();
    }

    private static void AddFrames(
        List<MorphBallEyeInstructionMechanicsWord> words,
        ushort entry,
        int frameCount,
        ushort duration)
    {
        for (int frame = 0; frame < frameCount; frame++)
            words.Add(new(unchecked((ushort)(entry + frame * 4)), duration));
    }

    private static void AddFrames(
        List<MorphBallEyeInstructionMechanicsWord> words,
        ushort entry,
        ReadOnlySpan<ushort> durations)
    {
        for (int frame = 0; frame < durations.Length; frame++)
            words.Add(new(unchecked((ushort)(entry + frame * 4)), durations[frame]));
    }

    private static void AddSleepProgram(
        List<MorphBallEyeInstructionMechanicsWord> words,
        ushort entry,
        ushort duration)
    {
        words.Add(new(entry, duration));
        words.Add(new(unchecked((ushort)(entry + 4)), CommonEnemyInstructionCodes.Sleep));
    }

    private static void AddSleepProgram(
        List<MorphBallEyeInstructionMechanicsWord> words,
        ushort entry,
        ReadOnlySpan<ushort> durations)
    {
        AddFrames(words, entry, durations);
        words.Add(new(unchecked((ushort)(entry + durations.Length * 4)),
            CommonEnemyInstructionCodes.Sleep));
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
