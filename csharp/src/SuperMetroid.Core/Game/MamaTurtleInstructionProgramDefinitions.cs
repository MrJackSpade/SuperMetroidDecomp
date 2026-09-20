namespace SuperMetroid.Core.Game;

/// <summary>One compiled tatori-family mechanics word at its bank-$A2 address.</summary>
internal readonly record struct MamaTurtleInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, callbacks, and control flow for Mama Turtle and Baby Turtle programs.
/// Interleaved spritemap operands remain live cartridge presentation data.
/// </summary>
internal static class MamaTurtleInstructionProgramDefinitions
{
    /// <summary><c>InstList_BabyTurtle_CrawlingLeft</c> at $A2:8B80.</summary>
    internal const ushort BabyCrawlingLeft = 0x8b80;
    /// <summary><c>InstList_BabyTurtle_Spinning</c> at $A2:8BD2.</summary>
    internal const ushort BabySpinning = 0x8bd2;
    /// <summary><c>InstList_MamaTurtle_Spinning</c> at $A2:8C02.</summary>
    internal const ushort MamaSpinning = 0x8c02;
    /// <summary><c>InstList_MamaTurtle_FacingLeft_EnterShell</c> at $A2:8C1C.</summary>
    internal const ushort MamaEnterShellLeft = 0x8c1c;
    /// <summary><c>InstList_BabyTurtle_FacingLeft_Hiding</c> at $A2:8C30.</summary>
    internal const ushort BabyHidingLeft = 0x8c30;
    /// <summary><c>InstList_MamaTurtle_Asleep</c> at $A2:8C44.</summary>
    internal const ushort MamaAsleep = 0x8c44;
    /// <summary><c>InstList_MamaTurtle_FacingLeft_LeaveShell</c> at $A2:8C4A.</summary>
    internal const ushort MamaLeaveShellLeft = 0x8c4a;
    /// <summary><c>InstList_BabyTurtle_FacingLeft_LeaveShell</c> at $A2:8C62.</summary>
    internal const ushort BabyLeaveShellLeft = 0x8c62;
    /// <summary><c>InstList_BabyTurtle_CrawlingRight</c> at $A2:8C72.</summary>
    internal const ushort BabyCrawlingRight = 0x8c72;
    /// <summary><c>InstList_MamaTurtle_FacingRight_EnterShell</c> at $A2:8D00.</summary>
    internal const ushort MamaEnterShellRight = 0x8d00;
    /// <summary><c>InstList_BabyTurtle_FacingRight_Hiding</c> at $A2:8D14.</summary>
    internal const ushort BabyHidingRight = 0x8d14;
    /// <summary><c>InstList_MamaTurtle_FacingRight_LeaveShell</c> at $A2:8D28.</summary>
    internal const ushort MamaLeaveShellRight = 0x8d28;
    /// <summary><c>InstList_BabyTurtle_FacingRight_LeaveShell</c> at $A2:8D40.</summary>
    internal const ushort BabyLeaveShellRight = 0x8d40;
    /// <summary><c>BabyTurtleConstants_travelDistance</c> at $A2:8D50.</summary>
    internal const ushort AdjacentMovementDefinitions = 0x8d50;

    private static readonly MamaTurtleInstructionMechanicsWord[] Words =
        BuildMechanicsWords();
    private static readonly ushort[] PresentationWords = BuildPresentationWords();

    internal static int MechanicsWordCount => Words.Length;
    internal static int PresentationWordCount => PresentationWords.Length;
    internal static MamaTurtleInstructionMechanicsWord MechanicsWord(int index) => Words[index];
    internal static ushort PresentationWordAddress(int index) => PresentationWords[index];

    /// <summary>Returns fixed tatori control data or rejects non-mechanics pointers.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MamaTurtleInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
                return candidate.Value;
            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        throw new InvalidDataException(
            $"Tatori instruction mechanics pointer $A2:{address:X4} is not compiled.");
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

    private static MamaTurtleInstructionMechanicsWord[] BuildMechanicsWords()
    {
        var words = new List<MamaTurtleInstructionMechanicsWord>(capacity: 117);
        AddBabyCrawl(words, BabyCrawlingLeft);

        AddFrame(words, BabySpinning, 1);
        AddCallback(words, unchecked((ushort)(BabySpinning + 4)),
            MamaTurtleInstructionCodes.PlaySpinningSound);
        AddFrames(words, unchecked((ushort)(BabySpinning + 6)), [4, 5, 5, 5]);
        AddCallback(words, unchecked((ushort)(BabySpinning + 22)),
            MamaTurtleInstructionCodes.SetSpinningStoppable);
        AddGoto(words, unchecked((ushort)(BabySpinning + 24)), BabySpinning);

        AddFrame(words, MamaSpinning, 1);
        AddCallback(words, unchecked((ushort)(MamaSpinning + 4)),
            MamaTurtleInstructionCodes.PlaySpinningSound);
        AddFrames(words, unchecked((ushort)(MamaSpinning + 6)), [4, 5, 5, 5]);
        AddGoto(words, unchecked((ushort)(MamaSpinning + 22)), MamaSpinning);

        AddFrames(words, MamaEnterShellLeft, [32, 5, 5]);
        AddCallback(words, unchecked((ushort)(MamaEnterShellLeft + 12)),
            MamaTurtleInstructionCodes.RiseToHoverRightwards);
        AddFrame(words, unchecked((ushort)(MamaEnterShellLeft + 14)), 0x7fff);
        AddSleep(words, unchecked((ushort)(MamaEnterShellLeft + 18)));

        AddFrames(words, BabyHidingLeft, [5, 5, 64]);
        AddCallback(words, unchecked((ushort)(BabyHidingLeft + 12)),
            MamaTurtleInstructionCodes.LeaveShell);
        AddFrame(words, unchecked((ushort)(BabyHidingLeft + 14)), 0x7fff);
        AddSleep(words, unchecked((ushort)(BabyHidingLeft + 18)));

        AddFrame(words, MamaAsleep, 0x7fff);
        AddSleep(words, unchecked((ushort)(MamaAsleep + 4)));

        AddFrames(words, MamaLeaveShellLeft, [16, 5, 5, 96]);
        AddCallback(words, unchecked((ushort)(MamaLeaveShellLeft + 16)),
            MamaTurtleInstructionCodes.EnterShell);
        AddFrame(words, unchecked((ushort)(MamaLeaveShellLeft + 18)), 0x7fff);
        AddSleep(words, unchecked((ushort)(MamaLeaveShellLeft + 22)));

        AddFrames(words, BabyLeaveShellLeft, [5, 47]);
        AddCallback(words, unchecked((ushort)(BabyLeaveShellLeft + 8)),
            MamaTurtleInstructionCodes.LeftShell);
        AddFrame(words, unchecked((ushort)(BabyLeaveShellLeft + 10)), 47);
        AddSleep(words, unchecked((ushort)(BabyLeaveShellLeft + 14)));

        AddBabyCrawl(words, BabyCrawlingRight);

        AddFrames(words, MamaEnterShellRight, [1, 5, 5]);
        AddCallback(words, unchecked((ushort)(MamaEnterShellRight + 12)),
            MamaTurtleInstructionCodes.RiseToHoverLeftwards);
        AddFrame(words, unchecked((ushort)(MamaEnterShellRight + 14)), 0x7fff);
        AddSleep(words, unchecked((ushort)(MamaEnterShellRight + 18)));

        AddFrames(words, BabyHidingRight, [5, 5, 64]);
        AddCallback(words, unchecked((ushort)(BabyHidingRight + 12)),
            MamaTurtleInstructionCodes.LeaveShell);
        AddFrame(words, unchecked((ushort)(BabyHidingRight + 14)), 0x7fff);
        AddSleep(words, unchecked((ushort)(BabyHidingRight + 18)));

        AddFrames(words, MamaLeaveShellRight, [16, 5, 5, 96]);
        AddCallback(words, unchecked((ushort)(MamaLeaveShellRight + 16)),
            MamaTurtleInstructionCodes.EnterShell);
        AddFrame(words, unchecked((ushort)(MamaLeaveShellRight + 18)), 0x7fff);
        AddSleep(words, unchecked((ushort)(MamaLeaveShellRight + 22)));

        AddFrames(words, BabyLeaveShellRight, [5, 47]);
        AddCallback(words, unchecked((ushort)(BabyLeaveShellRight + 8)),
            MamaTurtleInstructionCodes.LeftShell);
        AddFrame(words, unchecked((ushort)(BabyLeaveShellRight + 10)), 47);
        AddSleep(words, unchecked((ushort)(BabyLeaveShellRight + 14)));
        return words.ToArray();
    }

    private static ushort[] BuildPresentationWords()
    {
        var words = new List<ushort>(capacity: 75);
        AddBabyCrawlPresentation(words, BabyCrawlingLeft);
        AddPresentation(words, BabySpinning, 1);
        AddPresentation(words, unchecked((ushort)(BabySpinning + 6)), 4);
        AddPresentation(words, MamaSpinning, 1);
        AddPresentation(words, unchecked((ushort)(MamaSpinning + 6)), 4);
        AddPresentation(words, MamaEnterShellLeft, 3);
        AddPresentation(words, unchecked((ushort)(MamaEnterShellLeft + 14)), 1);
        AddPresentation(words, BabyHidingLeft, 3);
        AddPresentation(words, unchecked((ushort)(BabyHidingLeft + 14)), 1);
        AddPresentation(words, MamaAsleep, 1);
        AddPresentation(words, MamaLeaveShellLeft, 4);
        AddPresentation(words, unchecked((ushort)(MamaLeaveShellLeft + 18)), 1);
        AddPresentation(words, BabyLeaveShellLeft, 2);
        AddPresentation(words, unchecked((ushort)(BabyLeaveShellLeft + 10)), 1);
        AddBabyCrawlPresentation(words, BabyCrawlingRight);
        AddPresentation(words, MamaEnterShellRight, 3);
        AddPresentation(words, unchecked((ushort)(MamaEnterShellRight + 14)), 1);
        AddPresentation(words, BabyHidingRight, 3);
        AddPresentation(words, unchecked((ushort)(BabyHidingRight + 14)), 1);
        AddPresentation(words, MamaLeaveShellRight, 4);
        AddPresentation(words, unchecked((ushort)(MamaLeaveShellRight + 18)), 1);
        AddPresentation(words, BabyLeaveShellRight, 2);
        AddPresentation(words, unchecked((ushort)(BabyLeaveShellRight + 10)), 1);
        return words.ToArray();
    }

    private static void AddBabyCrawl(
        List<MamaTurtleInstructionMechanicsWord> words,
        ushort entry)
    {
        for (int step = 0; step < 8; step++)
        {
            ushort callback = unchecked((ushort)(entry + step * 10));
            AddCallback(words, callback, MamaTurtleInstructionCodes.Crawl);
            AddFrames(words, unchecked((ushort)(callback + 2)), [10, 10]);
        }
        AddCallback(words, unchecked((ushort)(entry + 80)),
            MamaTurtleInstructionCodes.LoopOrTurnAroundIfMovedTooFar);
    }

    private static void AddBabyCrawlPresentation(List<ushort> words, ushort entry)
    {
        for (int step = 0; step < 8; step++)
            AddPresentation(words, unchecked((ushort)(entry + step * 10 + 2)), 2);
    }

    private static void AddFrames(
        List<MamaTurtleInstructionMechanicsWord> words,
        ushort firstDuration,
        ReadOnlySpan<ushort> durations)
    {
        for (int frame = 0; frame < durations.Length; frame++)
            AddFrame(words, unchecked((ushort)(firstDuration + frame * 4)), durations[frame]);
    }

    private static void AddFrame(
        List<MamaTurtleInstructionMechanicsWord> words,
        ushort address,
        ushort duration) => words.Add(new(address, duration));

    private static void AddCallback(
        List<MamaTurtleInstructionMechanicsWord> words,
        ushort address,
        ushort callback) => words.Add(new(address, callback));

    private static void AddGoto(
        List<MamaTurtleInstructionMechanicsWord> words,
        ushort address,
        ushort target)
    {
        words.Add(new(address, CommonEnemyInstructionCodes.Goto));
        words.Add(new(unchecked((ushort)(address + 2)), target));
    }

    private static void AddSleep(
        List<MamaTurtleInstructionMechanicsWord> words,
        ushort address) => words.Add(new(address, CommonEnemyInstructionCodes.Sleep));

    private static void AddPresentation(List<ushort> words, ushort firstDuration, int count)
    {
        for (int frame = 0; frame < count; frame++)
            words.Add(unchecked((ushort)(firstDuration + frame * 4 + 2)));
    }
}
