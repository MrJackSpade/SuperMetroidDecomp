namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A2 address.</summary>
internal readonly record struct RinkaInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled mechanics words from the ordinary and Mother Brain Rinka instruction programs.
/// </summary>
/// <remarks>
/// Both native lists interleave engine state with presentation data. Callback identities,
/// frame durations, the common goto opcode, and its loop targets affect simulation and live
/// here. The word following every duration is a spritemap pointer; those eighteen words
/// are presentation selectors installed as editable artwork. Constructed
/// diagnostic buses without installed artwork may still provide mutable words.
/// </remarks>
internal static class RinkaInstructionProgramDefinitions
{
    /// <summary><c>$A2:B9E0</c>, ordinary room-Rinka animation program.</summary>
    internal const ushort OrdinaryInitial = 0xb9e0;

    /// <summary><c>$A2:BA0C</c>, off-screen Mother Brain Rinka animation program.</summary>
    internal const ushort SpecialInitial = 0xba0c;

    // REQUIRED timing/content choices: initial hidden hold, seed hold, minimum
    // pulse hold and eight-pose cycle. Mirrored dwell follows the same returning
    // sprite stages in A2:B9EC-BA04 and BA18-BA30; no chosen magnitude is exempt.
    private const ushort HiddenHold = 64;
    private const ushort SeedHold = 16;
    private const int MinimumPulseHold = 5;
    private const int PoseCount = 8;
    private const int SetupBytes = 8;
    private const int ListBytes = SetupBytes + PoseCount * 4 + 4;
    private const int WordsPerList = 3 + PoseCount + 2;

    internal static int MechanicsWordCount => 2 * WordsPerList;
    internal static int PresentationWordCount => 2 * (PoseCount + 1);

    internal static RinkaInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int part = index % WordsPerList;
        int offset = part switch
        {
            0 => 0,
            1 => 2,
            2 => 6,
            _ when part < 3 + PoseCount => SetupBytes + (part - 3) * 4,
            _ => SetupBytes + PoseCount * 4 + (part - 3 - PoseCount) * 2,
        };
        ushort address = (ushort)(OrdinaryInitial + index / WordsPerList * ListBytes + offset);
        return new(address, ReadMechanicsWord(address));
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int part = index % (PoseCount + 1);
        return (ushort)(OrdinaryInitial + index / (PoseCount + 1) * ListBytes +
            (part == 0 ? 4 : SetupBytes + (part - 1) * 4 + 2));
    }

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int relative = address - OrdinaryInitial;
        if ((uint)relative < 2 * ListBytes && (relative & 1) == 0 && !IsPresentationOffset(relative % ListBytes))
        {
            int offset = relative % ListBytes;
            if (offset == 0) return relative < ListBytes
                ? RinkaInstructionCodes.Instruction_Rinka_SetAsIntangibleAndInvisible
                : RinkaInstructionCodes.Instruction_Rinka_SetAsIntangibleInvisibleAndActiveOffScreen;
            if (offset == 2) return HiddenHold;
            if (offset == 6) return RinkaInstructionCodes.Instruction_Rinka_FireRinka;
            if (offset < SetupBytes + PoseCount * 4)
            {
                int pose = (offset - SetupBytes) / 4;
                return pose == 0 ? SeedHold : (ushort)(MinimumPulseHold + Math.Abs(pose - PoseCount / 2));
            }
            if (offset == ListBytes - 4) return CommonEnemyInstructionCodes.Goto;
            return (ushort)(OrdinaryInitial + relative / ListBytes * ListBytes + SetupBytes);
        }
        throw new InvalidDataException($"Rinka instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    private static bool IsPresentationOffset(int offset) => offset == 4 ||
        (offset >= SetupBytes && offset < ListBytes - 4 && (offset - SetupBytes) % 4 == 2);

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int relative = (address & 0xfffe) - OrdinaryInitial;
        return (uint)relative < 2 * ListBytes && !IsPresentationOffset(relative % ListBytes);
    }
}
