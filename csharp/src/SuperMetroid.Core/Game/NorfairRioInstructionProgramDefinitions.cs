namespace SuperMetroid.Core.Game;

/// <summary>One compiled Norfair Rio mechanics word at its bank-$A2 address.</summary>
internal readonly record struct NorfairRioInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing, private callbacks, and loop control for Norfair Rio parent and flame
/// programs. Interleaved spritemap operands select extracted presentation frames.
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

    // Pose holds are authored animation cadence (reviewed under #1165); program geometry and the
    // callback dispatch below are calculated.
    private static readonly ushort[] IdleHolds = [13, 18];
    private static readonly ushort[] FlightHolds = [6, 5, 8, 6];

    internal static int MechanicsWordCount => 65;
    internal static int PresentationWordCount => 34;

    /// <summary>
    /// $A2:C0F1-C1B6: three parent loops, two callback/pose transition sequences,
    /// and two flame loops. Each timed pose occupies four bytes; interleaved
    /// follower-offset callbacks make the transition poses six bytes wide.
    /// </summary>
    internal static NorfairRioInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 7) return LoopWord(index, Idle, NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8_duplicate, true);
        if (index < 21) return TransitionWord(index - 7, StartDescending, false);
        if (index < 28) return LoopWord(index - 21, Descending, NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC, false);
        if (index < 46) return TransitionWord(index - 28, StartAscending, true);
        if (index < 53) return LoopWord(index - 46, Ascending, NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_C_duplicate, false);
        if (index < 59) return LoopWord(index - 53, FlamesAscending, 0, false);
        return LoopWord(index - 59, FlamesDescending, 0, false);
    }

    private static NorfairRioInstructionMechanicsWord LoopWord(int index, ushort start, ushort setup, bool idle)
    {
        int frameStart = start;
        if (setup != 0)
        {
            if (index == 0) return new(start, setup);
            index--;
            frameStart += 2;
        }
        if (index < 4) return new((ushort)(frameStart + index * 4), idle ? IdleHolds[index % 2] : FlightHolds[index]);
        return new((ushort)(frameStart + 16 + (index - 4) * 2), index == 4 ? CommonEnemyInstructionCodes.Goto : start);
    }

    private static NorfairRioInstructionMechanicsWord TransitionWord(int index, ushort start, bool ascending)
    {
        int poses = ascending ? 8 : 6;
        if (index < poses * 2)
            return new((ushort)(start + index / 2 * 6 + index % 2 * 2),
                index % 2 == 0 ? TransitionCallback(index / 2, ascending) : (ushort)1);
        return new((ushort)(start + poses * 6 + (index - poses * 2) * 2),
            index == poses * 2 ? NorfairRioInstructionCodes.Instruction_Geruta_SetFinishedSwoopStartAnimationFlag : CommonEnemyInstructionCodes.Sleep);
    }

    /// <summary>Named per-pose dispatch: each pose selects the handler that sets the flame Y offset
    /// (-16,-12,-4,0,4,8,8,12 ascending) attaching the flame to that drawn pose. A semantic case mapping.</summary>
    private static ushort TransitionCallback(int pose, bool ascending) => ascending ? pose switch
    {
        0 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative10,
        1 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC_duplicate,
        2 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative4,
        3 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_0,
        4 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_4,
        5 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8,
        6 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8_duplicate,
        _ => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_C,
    } : pose switch
    {
        0 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_8,
        1 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_4,
        2 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_0,
        3 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative4,
        4 => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negativeC_duplicate,
        _ => NorfairRioInstructionCodes.Instruction_Geruta_SetFlamesYOffset_negative10,
    };

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        if (index < 4) return (ushort)(Idle + 4 + index * 4);
        if (index < 10) return (ushort)(StartDescending + 4 + (index - 4) * 6);
        if (index < 14) return (ushort)(Descending + 4 + (index - 10) * 4);
        if (index < 22) return (ushort)(StartAscending + 4 + (index - 14) * 6);
        if (index < 26) return (ushort)(Ascending + 4 + (index - 22) * 4);
        if (index < 30) return (ushort)(FlamesAscending + 2 + (index - 26) * 4);
        return (ushort)(FlamesDescending + 2 + (index - 30) * 4);
    }

    internal static bool IsPresentationWord(ushort address)
    {
        for (int index = 0; index < PresentationWordCount; index++)
            if (PresentationWordAddress(index) == address) return true;
        return false;
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0;
        int high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            var candidate = MechanicsWord(middle);
            if (candidate.Address == address) return candidate.Value;
            if (candidate.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException($"Norfair Rio instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
