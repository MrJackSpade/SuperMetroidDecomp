namespace SuperMetroid.Core.Game;

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
internal abstract class RinkaInstructionProgramDefinitions
{
    /// <summary><c>$A2:B9E0</c>, ordinary room-Rinka animation program.</summary>
    internal const ushort OrdinaryInitial = 0xb9e0;

    /// <summary><c>$A2:BA0C</c>, off-screen Mother Brain Rinka animation program.</summary>
    internal const ushort SpecialInitial = 0xba0c;

    // Authored timing/content choices (reviewed under #1165): initial hidden hold, seed hold, minimum
    // pulse hold and eight-pose cycle. Mirrored dwell follows the same returning
    // sprite stages in A2:B9EC-BA04 and BA18-BA30; no chosen magnitude is exempt.
    private const ushort HiddenHold = 64;
    private const ushort SeedHold = 16;
    private const int MinimumPulseHold = 5;
    internal const int PoseCount = 8;
    internal const int SetupBytes = 8;
    internal const int ListBytes = SetupBytes + PoseCount * 4 + 4;
    public static int PresentationWordCount => 2 * (PoseCount + 1);

    public static ushort PresentationWordAddress(int index)
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
                ? (ushort)RinkaInstruction.Instruction_Rinka_SetAsIntangibleAndInvisible
                : (ushort)RinkaInstruction.Instruction_Rinka_SetAsIntangibleInvisibleAndActiveOffScreen;
            if (offset == 2) return HiddenHold;
            if (offset == 6) return (ushort)RinkaInstruction.Instruction_Rinka_FireRinka;
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

    internal static bool IsPresentationOffset(int offset) => offset == 4 ||
        (offset >= SetupBytes && offset < ListBytes - 4 && (offset - SetupBytes) % 4 == 2);
}
