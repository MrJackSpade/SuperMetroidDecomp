namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled mechanics words from Mother Brain's bank-$A9 body animation programs.
/// </summary>
/// <remarks>
/// The native streams interleave mechanics with presentation. Command words and frame
/// durations control body movement, pose, footsteps, and AI-visible timing, so they belong
/// to the translated mechanics model. The word following each duration is an extended
/// spritemap pointer and deliberately remains a cartridge/presentation lookup.
/// </remarks>
internal static class MotherBrainBodyInstructionProgramDefinitions
{
    /// <summary>$A9:9730-$A9:9851, five forward-walk programs.</summary>
    private const ushort ForwardWalkReallyFast = 0x9730;

    /// <summary>$A9:9852-$A9:9973, five backward-walk programs.</summary>
    private const ushort BackwardWalkSlow = 0x9852;

    /// <summary>$A9:9974, InstList_MotherBrainBody_CrouchAndThenStandUp.</summary>
    private const ushort CrouchAndThenStandUp = 0x9974;

    /// <summary>$A9:99AA, InstList_MotherBrainBody_StandingUpAfterCrouching_Slow.</summary>
    private const ushort StandAfterCrouchSlow = 0x99aa;

    /// <summary>$A9:99C6, InstList_MotherBrainBody_StandingUpAfterCrouching_Fast.</summary>
    private const ushort StandAfterCrouchFast = 0x99c6;

    /// <summary>$A9:99E2, InstList_MotherBrainBody_StandingUpAfterLeaningDown.</summary>
    private const ushort StandAfterLeaning = 0x99e2;

    /// <summary>$A9:99F2, InstList_MotherBrainBody_LeaningDown.</summary>
    private const ushort LeanDown = 0x99f2;

    /// <summary>$A9:9A02, InstList_MotherBrainBody_Crouched.</summary>
    private const ushort Crouched = 0x9a02;

    /// <summary>$A9:9A0A, InstList_MotherBrainBody_Crouch_Slow.</summary>
    private const ushort CrouchSlow = 0x9a0a;

    /// <summary>$A9:9A26, InstList_MotherBrainBody_Crouch_Fast.</summary>
    private const ushort CrouchFast = 0x9a26;

    private static readonly MotherBrainBodyInstructionMechanicsWord[] Words = CreateWords();

    /// <summary>All compiled addresses, exposed internally for cartridge-equivalence tests.</summary>
    internal static IReadOnlyList<MotherBrainBodyInstructionMechanicsWord> AllWords => Words;

    /// <summary>
    /// Looks up a mechanics word while returning false for interleaved spritemap words and
    /// for instruction streams outside the translated body-program family.
    /// </summary>
    internal static bool TryGetWord(ushort address, out ushort word)
    {
        int low = 0;
        int high = Words.Length - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            MotherBrainBodyInstructionMechanicsWord candidate = Words[middle];
            if (candidate.Address == address)
            {
                word = candidate.Word;
                return true;
            }

            if (candidate.Address < address)
                low = middle + 1;
            else
                high = middle - 1;
        }

        word = 0;
        return false;
    }

    private static MotherBrainBodyInstructionMechanicsWord[] CreateWords()
    {
        var words = new List<MotherBrainBodyInstructionMechanicsWord>();

        AddForwardWalk(words, ForwardWalkReallyFast + 0x00, 2);
        AddForwardWalk(words, ForwardWalkReallyFast + 0x3a, 4);
        AddForwardWalk(words, ForwardWalkReallyFast + 0x74, 6);
        AddForwardWalk(words, ForwardWalkReallyFast + 0xae, 8);
        AddForwardWalk(words, ForwardWalkReallyFast + 0xe8, 10);

        AddBackwardWalk(words, BackwardWalkSlow + 0x00, 8);
        AddBackwardWalk(words, BackwardWalkSlow + 0x3a, 2);
        AddBackwardWalk(words, BackwardWalkSlow + 0x74, 4);
        AddBackwardWalk(words, BackwardWalkSlow + 0xae, 6);
        AddBackwardWalk(words, BackwardWalkSlow + 0xe8, 10);

        AddCrouchAndStand(words);
        AddStandAfterCrouch(words, StandAfterCrouchSlow, 16);
        AddStandAfterCrouch(words, StandAfterCrouchFast, 8);
        AddStandAfterLeaning(words);
        AddLeanDown(words);
        AddCrouched(words);
        AddCrouch(words, CrouchSlow, 8, 8, 8, 8);
        AddCrouch(words, CrouchFast, 8, 2, 2, 8);

        words.Sort(static (left, right) => left.Address.CompareTo(right.Address));
        for (int index = 1; index < words.Count; index++)
        {
            if (words[index - 1].Address == words[index].Address)
                throw new InvalidOperationException(
                    $"Duplicate Mother Brain body mechanics word at $A9:{words[index].Address:X4}.");
        }
        return [.. words];
    }

    private static void AddForwardWalk(
        List<MotherBrainBodyInstructionMechanicsWord> words,
        int start,
        ushort duration)
    {
        Add(words, start + 0x00, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking);
        AddFrame(words, start + 0x02, duration);
        Add(words, start + 0x06, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_ScrollRightBy1);
        AddFrame(words, start + 0x08, duration);
        Add(words, start + 0x0c, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyRightBy2);
        AddFrame(words, start + 0x0e, duration);
        Add(words, start + 0x12, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1);
        AddFrame(words, start + 0x14, duration);
        Add(words, start + 0x18, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy1_RightBy3_Footstep);
        AddFrame(words, start + 0x1a, duration);
        Add(words, start + 0x1e, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy15);
        AddFrame(words, start + 0x20, duration);
        Add(words, start + 0x24, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy6);
        AddFrame(words, start + 0x26, duration);
        Add(words, start + 0x2a, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy2);
        AddFrame(words, start + 0x2c, duration);
        Add(words, start + 0x30, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep);
        Add(words, start + 0x32, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding);
        AddFrame(words, start + 0x34, duration);
        Add(words, start + 0x38, MotherBrainInstructionCodes.Instruction_CommonA9_Sleep);
    }

    private static void AddBackwardWalk(
        List<MotherBrainBodyInstructionMechanicsWord> words,
        int start,
        ushort duration)
    {
        Add(words, start + 0x00, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToWalking);
        AddFrame(words, start + 0x02, duration);
        Add(words, start + 0x06, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy2_RightBy1);
        AddFrame(words, start + 0x08, duration);
        Add(words, start + 0x0c, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy4_RightBy2);
        AddFrame(words, start + 0x0e, duration);
        Add(words, start + 0x12, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy4_LeftBy6);
        AddFrame(words, start + 0x14, duration);
        Add(words, start + 0x18, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy15_Footstep);
        AddFrame(words, start + 0x1a, duration);
        Add(words, start + 0x1e, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1_LeftBy3);
        AddFrame(words, start + 0x20, duration);
        Add(words, start + 0x24, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy1);
        AddFrame(words, start + 0x26, duration);
        Add(words, start + 0x2a, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyLeftBy2);
        AddFrame(words, start + 0x2c, duration);
        Add(words, start + 0x30, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy2_LeftBy1_Footstep_d);
        Add(words, start + 0x32, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding);
        AddFrame(words, start + 0x34, duration);
        Add(words, start + 0x38, MotherBrainInstructionCodes.Instruction_CommonA9_Sleep);
    }

    private static void AddCrouchAndStand(List<MotherBrainBodyInstructionMechanicsWord> words)
    {
        Add(words, CrouchAndThenStandUp + 0x00, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition);
        AddFrame(words, CrouchAndThenStandUp + 0x02, 8);
        Add(words, CrouchAndThenStandUp + 0x06, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4);
        AddFrame(words, CrouchAndThenStandUp + 0x08, 8);
        Add(words, CrouchAndThenStandUp + 0x0c, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2);
        AddFrame(words, CrouchAndThenStandUp + 0x0e, 8);
        Add(words, CrouchAndThenStandUp + 0x12, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2);
        Add(words, CrouchAndThenStandUp + 0x14, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouching);
        AddFrame(words, CrouchAndThenStandUp + 0x16, 8);
        AddFrame(words, CrouchAndThenStandUp + 0x1a, 8);
        Add(words, CrouchAndThenStandUp + 0x1e, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4);
        Add(words, CrouchAndThenStandUp + 0x20, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition);
        AddFrame(words, CrouchAndThenStandUp + 0x22, 8);
        Add(words, CrouchAndThenStandUp + 0x26, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4);
        AddFrame(words, CrouchAndThenStandUp + 0x28, 8);
        Add(words, CrouchAndThenStandUp + 0x2c, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2);
        AddFrame(words, CrouchAndThenStandUp + 0x2e, 8);
        Add(words, CrouchAndThenStandUp + 0x32, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding);
        Add(words, CrouchAndThenStandUp + 0x34, MotherBrainInstructionCodes.Instruction_CommonA9_Sleep);
    }

    private static void AddStandAfterCrouch(
        List<MotherBrainBodyInstructionMechanicsWord> words,
        ushort start,
        ushort duration)
    {
        Add(words, start + 0x00, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition);
        AddFrame(words, start + 0x02, duration);
        Add(words, start + 0x06, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy10_ScrollLeftBy4);
        AddFrame(words, start + 0x08, duration);
        Add(words, start + 0x0c, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy16_ScrollLeftBy4);
        AddFrame(words, start + 0x0e, duration);
        Add(words, start + 0x12, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2);
        AddFrame(words, start + 0x14, duration);
        Add(words, start + 0x18, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding);
        Add(words, start + 0x1a, MotherBrainInstructionCodes.Instruction_CommonA9_Sleep);
    }

    private static void AddStandAfterLeaning(List<MotherBrainBodyInstructionMechanicsWord> words)
    {
        Add(words, StandAfterLeaning + 0x00, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition);
        AddFrame(words, StandAfterLeaning + 0x02, 8);
        Add(words, StandAfterLeaning + 0x06, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyUpBy12_ScrollRightBy2);
        AddFrame(words, StandAfterLeaning + 0x08, 8);
        Add(words, StandAfterLeaning + 0x0c, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToStanding);
        Add(words, StandAfterLeaning + 0x0e, MotherBrainInstructionCodes.Instruction_CommonA9_Sleep);
    }

    private static void AddLeanDown(List<MotherBrainBodyInstructionMechanicsWord> words)
    {
        Add(words, LeanDown + 0x00, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition);
        AddFrame(words, LeanDown + 0x02, 8);
        Add(words, LeanDown + 0x06, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4);
        Add(words, LeanDown + 0x08, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToLeaningDown);
        AddFrame(words, LeanDown + 0x0a, 8);
        Add(words, LeanDown + 0x0e, MotherBrainInstructionCodes.Instruction_CommonA9_Sleep);
    }

    private static void AddCrouched(List<MotherBrainBodyInstructionMechanicsWord> words)
    {
        Add(words, Crouched + 0x00, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouching);
        AddFrame(words, Crouched + 0x02, 8);
        Add(words, Crouched + 0x06, MotherBrainInstructionCodes.Instruction_CommonA9_Sleep);
    }

    private static void AddCrouch(
        List<MotherBrainBodyInstructionMechanicsWord> words,
        ushort start,
        ushort standingDuration,
        ushort leaningDuration,
        ushort uncrouchingDuration,
        ushort crouchedDuration)
    {
        Add(words, start + 0x00, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouchingTransition);
        AddFrame(words, start + 0x02, standingDuration);
        Add(words, start + 0x06, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy12_ScrollLeftBy4);
        AddFrame(words, start + 0x08, leaningDuration);
        Add(words, start + 0x0c, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy16_ScrollRightBy2);
        AddFrame(words, start + 0x0e, uncrouchingDuration);
        Add(words, start + 0x12, MotherBrainInstructionCodes.Instruction_MotherBrainBody_MoveBodyDownBy10_ScrollRightBy2);
        Add(words, start + 0x14, MotherBrainInstructionCodes.Instruction_MotherBrainBody_SetPoseToCrouching);
        AddFrame(words, start + 0x16, crouchedDuration);
        Add(words, start + 0x1a, MotherBrainInstructionCodes.Instruction_CommonA9_Sleep);
    }

    private static void AddFrame(
        List<MotherBrainBodyInstructionMechanicsWord> words,
        int address,
        ushort duration) => Add(words, address, duration);

    private static void Add(
        List<MotherBrainBodyInstructionMechanicsWord> words,
        int address,
        ushort word) => words.Add(new(unchecked((ushort)address), word));
}

/// <summary>One compiled command or duration word at its native bank-$A9 address.</summary>
internal readonly record struct MotherBrainBodyInstructionMechanicsWord(
    ushort Address,
    ushort Word);
