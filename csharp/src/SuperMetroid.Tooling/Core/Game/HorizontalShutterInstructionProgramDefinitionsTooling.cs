using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="HorizontalShutterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(HorizontalShutterInstructionProgramDefinitions))]
internal abstract class HorizontalShutterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => 2;
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => 1;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(HorizontalShutterInstructionProgramDefinitions.Stationary + 4 * index);
        return new(address, HorizontalShutterInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => index == 0
        ? HorizontalShutterInstructionProgramDefinitions.PresentationWord
        : throw new ArgumentOutOfRangeException(nameof(index));
    /// <summary>Indicates whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000) return false;
        int offset = unchecked((ushort)address) - HorizontalShutterInstructionProgramDefinitions.Stationary;
        return (uint)offset < 6 && (offset < 2 || offset >= 4);
    }
}
