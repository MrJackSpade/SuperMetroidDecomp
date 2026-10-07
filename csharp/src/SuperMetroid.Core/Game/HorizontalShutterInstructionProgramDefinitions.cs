namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for the horizontal shutter's stationary program.
/// Its interleaved spritemap operand selects separately installed presentation data.
/// </summary>
internal abstract class HorizontalShutterInstructionProgramDefinitions : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary><c>InstList_ShutterHorizontal</c> at $A2:E9D4.</summary>
    internal const ushort Stationary = 0xe9d4;

    private const ushort PresentationWord = 0xe9d6;

    public static int MechanicsWordCount => 2;
    public static int PresentationWordCount => 1;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(Stationary + 4 * index);
        return new(address, ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index) => index == 0
        ? PresentationWord
        : throw new ArgumentOutOfRangeException(nameof(index));

    /// <summary>True only for the stationary horizontal shutter's visual operand.</summary>
    internal static bool IsPresentationWord(ushort address) => address == PresentationWord;

    internal static ushort ReadMechanicsWord(ushort address)
    {
        if (address == Stationary) return 1;
        if (address == Stationary + 4) return CommonEnemyInstructionCodes.Sleep;
        throw new InvalidDataException(
            $"Horizontal-shutter instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }

    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - Stationary;
        return (uint)offset < 6 && (offset < 2 || offset >= 4);
    }
}
