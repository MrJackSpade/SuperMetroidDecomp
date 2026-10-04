namespace SuperMetroid.Core.Game;

/// <summary>One compiled Fireflea mechanics word at its native bank-$A3 address.</summary>
internal readonly record struct FirefleaInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled timing and loop control for Fireflea's single 52-frame program. The interleaved
/// spritemap selectors are compiled separately from their editable OAM compositions.
/// </summary>
internal static class FirefleaInstructionProgramDefinitions
{
    /// <summary><c>InstList_Fireflea</c> at $A3:8C2F.</summary>
    internal const ushort Loop = 0x8c2f;
    /// <summary>The adjacent unused Fireflea data block at $A3:8D03.</summary>
    internal const ushort AdjacentUnusedData = 0x8d03;
    internal const int FrameCount = 52;

    internal static int MechanicsWordCount => FrameCount + 2;
    internal static int PresentationWordCount => FrameCount;

    /// <summary>Calculates alternating timed records followed by the loop command and its target.</summary>
    internal static FirefleaInstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < FrameCount)
            return new((ushort)(Loop + index * 4), (ushort)(2 - (index & 1)));
        return new((ushort)(Loop + FrameCount * 4 + (index - FrameCount) * 2),
            index == FrameCount ? CommonEnemyInstructionCodes.Goto : Loop);
    }

    internal static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= FrameCount)
            throw new IndexOutOfRangeException();
        return (ushort)(Loop + index * 4 + 2);
    }

    /// <summary>Whether an address is one of the 52 visual operands.</summary>
    internal static bool IsPresentationWord(ushort address) =>
        address >= Loop + 2 &&
        address <= Loop + (FrameCount - 1) * 4 + 2 &&
        (address - Loop - 2) % 4 == 0;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Loop;
        if (offset >= 0 && offset < FrameCount * 4 && (offset & 3) == 0)
            return (ushort)(2 - ((offset / 4) & 1));
        if (offset == FrameCount * 4)
            return CommonEnemyInstructionCodes.Goto;
        if (offset == FrameCount * 4 + 2)
            return Loop;
        throw new InvalidDataException(
            $"Fireflea instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000)
            return false;
        int offset = unchecked((ushort)address) - Loop;
        return offset >= 0 &&
            (offset < FrameCount * 4 ? (offset & 3) < 2 : offset < FrameCount * 4 + 4);
    }
}