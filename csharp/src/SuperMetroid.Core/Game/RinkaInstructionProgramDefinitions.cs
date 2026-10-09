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
    /// <summary>Duration of the initial intangible and invisible setup stage before the Rinka becomes active.</summary>
    private const ushort HiddenHold = 64;

    /// <summary>Duration assigned to the first pose before the pulse's varying per-pose dwell begins.</summary>
    private const ushort SeedHold = 16;

    /// <summary>Base dwell added to the mirrored pose-distance timing for the pulse cycle.</summary>
    private const int MinimumPulseHold = 5;

    /// <summary>Number of poses in each compiled Rinka pulse cycle.</summary>
    internal const int PoseCount = 8;

    /// <summary>Byte length of the non-frame setup instructions preceding each pose list.</summary>
    internal const int SetupBytes = 8;

    /// <summary>Total byte length of one Rinka instruction list, including setup, pose entries, and the loop-control pair.</summary>
    internal const int ListBytes = SetupBytes + PoseCount * 4 + 4;

    /// <summary>Number of spritemap selector words across the ordinary and off-screen Mother Brain lists.</summary>
    public static int PresentationWordCount => 2 * (PoseCount + 1);

    /// <summary>Maps a flattened selector index to its spritemap operand in the ordinary or special Rinka list.</summary>
    /// <param name="index">Zero-based index across both lists' setup and pose presentation operands.</param>
    /// <returns>The bank-relative address of the selected installed-artwork selector word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int part = index % (PoseCount + 1);
        return (ushort)(OrdinaryInitial + index / (PoseCount + 1) * ListBytes +
            (part == 0 ? 4 : SetupBytes + (part - 1) * 4 + 2));
    }

    /// <summary>Returns the compiled duration, callback, or loop-control word while excluding installed spritemap selectors.</summary>
    /// <param name="address">Bank-relative address within one of the two Rinka instruction lists.</param>
    /// <returns>The native word value used by instruction execution.</returns>
    /// <exception cref="InvalidDataException">The address is not an owned mechanics word or identifies a presentation selector.</exception>
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

    /// <summary>Identifies the spritemap selector offsets within one compiled Rinka list.</summary>
    /// <param name="offset">Byte offset relative to the beginning of a list.</param>
    /// <returns>True for the initial selector and each pose selector, which belong to installed artwork rather than mechanics.</returns>
    internal static bool IsPresentationOffset(int offset) => offset == 4 ||
        (offset >= SetupBytes && offset < ListBytes - 4 && (offset - SetupBytes) % 4 == 2);
}
