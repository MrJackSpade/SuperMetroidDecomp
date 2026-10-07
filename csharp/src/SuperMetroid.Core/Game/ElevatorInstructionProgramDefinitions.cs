namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the ordinary elevator's two-frame animation loop.
/// Its two spritemap operands resolve through compiled presentation selectors.
/// </summary>
internal abstract class ElevatorInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_Elevator</c> at $A3:94D6.</summary>
    internal const ushort Loop = 0x94d6;

    /// <summary>The controller-input table immediately after the program, at $A3:94E2.</summary>
    internal const ushort FirstAdjacentMechanicsData = 0x94e2;

    public static int MechanicsWordCount => 4;
    public static int PresentationWordCount => 2;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(Loop + (index < 2 ? 4 * index : 8 + 2 * (index - 2)));
        return new(address, ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(Loop + 2 + 4 * index);
    }
    internal static bool IsPresentationWord(ushort address)
    {
        int offset = address - (Loop + 2);
        return (uint)offset < 8 && offset % 4 == 0;
    }
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int offset = address - Loop;
        if (offset is 0 or 4) return 2;
        if (offset == 8) return CommonEnemyInstructionCodes.Goto;
        if (offset == 10) return Loop;
        throw new InvalidDataException(
            $"Elevator instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        int offset = unchecked((ushort)address) - Loop;
        return (uint)offset < 12 && (offset >= 8 || offset % 4 < 2);
    }
}
