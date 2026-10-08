using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HorizontalShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HorizontalShutterInstructionProgramDefinitions))]
internal abstract class HorizontalShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int MechanicsWordCount => 2;
    public static int PresentationWordCount => 1;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(HorizontalShutterInstructionProgramDefinitions.Stationary + 4 * index);
        return new(address, HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static ushort PresentationWordAddress(int index) => index == 0
        ? HorizontalShutterInstructionProgramDefinitions.PresentationWord
        : throw new ArgumentOutOfRangeException(nameof(index));
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - HorizontalShutterInstructionProgramDefinitions.Stationary;
        return (uint)offset < 6 && (offset < 2 || offset >= 4);
    }
}
